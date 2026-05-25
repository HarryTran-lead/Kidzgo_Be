-- Merge duplicate lesson plan units inside each module into one canonical unit.
-- Canonical grouping priority:
-- 1. moduleId + unit number for UNIT/REVISION patterns
-- 2. moduleId + normalized text fallback when no unit number can be derived
--
-- Effects:
-- - remaps LessonPlanTemplates.LessonPlanUnitId to the canonical unit
-- - normalizes canonical unit Name / NameNormalized / OrderIndex
-- - disables duplicate units and moves their NameNormalized to a tombstone key
--
-- Review on staging before production.

BEGIN;

DROP TABLE IF EXISTS tmp_unit_merge_candidates;
CREATE TEMP TABLE tmp_unit_merge_candidates AS
WITH source_units AS (
    SELECT
        u."Id",
        u."ModuleId",
        u."Name",
        COALESCE(u."NameNormalized", '') AS "NameNormalized",
        u."OrderIndex",
        u."IsActive",
        u."CreatedAt",
        u."UpdatedAt",
        UPPER(REGEXP_REPLACE(TRIM(COALESCE(u."Name", '')), '\s+', ' ', 'g')) AS raw_name,
        UPPER(REGEXP_REPLACE(TRIM(COALESCE(u."NameNormalized", '')), '\s+', ' ', 'g')) AS raw_key
    FROM public."LessonPlanUnits" AS u
),
parsed_units AS (
    SELECT
        su.*,
        CASE
            WHEN su.raw_key ~ '^UNIT\|STARTER$' OR su.raw_name ~ '(^| )UNIT STARTER($|[: ].*)' THEN 'UNIT|STARTER'
            WHEN su.raw_key ~ '^UNIT\|[0-9]+$' THEN 'UNIT|' || SUBSTRING(su.raw_key FROM '^UNIT\|([0-9]+)$')
            WHEN su.raw_name ~ '(^| )UNIT[[:space:]]+0*[0-9]+' THEN 'UNIT|' || SUBSTRING(su.raw_name FROM 'UNIT[[:space:]]+0*([0-9]+)')
            WHEN su.raw_key ~ '^REVISION\|[0-9]+$' THEN 'REVISION|' || SUBSTRING(su.raw_key FROM '^REVISION\|([0-9]+)$')
            WHEN su.raw_name ~ '(^| )REVISION[[:space:]]+0*[0-9]+' THEN 'REVISION|' || SUBSTRING(su.raw_name FROM 'REVISION[[:space:]]+0*([0-9]+)')
            ELSE 'TEXT|' || REGEXP_REPLACE(
                REGEXP_REPLACE(
                    REGEXP_REPLACE(su.raw_name, '[!?.;,]+$', '', 'g'),
                    '[^A-Z0-9]+',
                    '',
                    'g'),
                '^TEXT\|',
                '',
                'g')
        END AS canonical_key,
        CASE
            WHEN su.raw_key ~ '^UNIT\|([0-9]+)$' THEN SUBSTRING(su.raw_key FROM '^UNIT\|([0-9]+)$')
            WHEN su.raw_name ~ '(^| )UNIT[[:space:]]+0*([0-9]+)' THEN SUBSTRING(su.raw_name FROM 'UNIT[[:space:]]+0*([0-9]+)')
            WHEN su.raw_key ~ '^REVISION\|([0-9]+)$' THEN SUBSTRING(su.raw_key FROM '^REVISION\|([0-9]+)$')
            WHEN su.raw_name ~ '(^| )REVISION[[:space:]]+0*([0-9]+)' THEN SUBSTRING(su.raw_name FROM 'REVISION[[:space:]]+0*([0-9]+)')
            ELSE NULL
        END AS canonical_number_text,
        NULLIF(
            BTRIM(
                REGEXP_REPLACE(
                    REGEXP_REPLACE(
                        REGEXP_REPLACE(su.raw_name, '(^| )UNIT[[:space:]]+STARTER', '', 'i'),
                        '(^| )UNIT[[:space:]]+0*[0-9]+',
                        '',
                        'i'),
                    '^[:\-\s]+|[!?.;,\s]+$',
                    '',
                    'g')),
            '') AS parsed_unit_title
    FROM source_units AS su
),
ranked_units AS (
    SELECT
        pu.*,
        COALESCE(NULLIF(pu.parsed_unit_title, ''), NULL) AS canonical_title,
        ROW_NUMBER() OVER (
            PARTITION BY pu."ModuleId", pu.canonical_key
            ORDER BY
                CASE WHEN pu.raw_key = pu.canonical_key THEN 0 ELSE 1 END,
                CASE WHEN COALESCE(NULLIF(pu.parsed_unit_title, ''), NULL) IS NULL THEN 1 ELSE 0 END,
                CASE WHEN pu."IsActive" THEN 0 ELSE 1 END,
                pu."OrderIndex",
                pu."CreatedAt",
                pu."Id"
        ) AS canonical_rank,
        MIN(pu."OrderIndex") OVER (PARTITION BY pu."ModuleId", pu.canonical_key) AS merged_order_index
    FROM parsed_units AS pu
)
SELECT
    ru."Id" AS unit_id,
    ru."ModuleId" AS module_id,
    ru.canonical_key,
    CASE
        WHEN ru.canonical_key = 'UNIT|STARTER' THEN
            CASE
                WHEN ru.canonical_title IS NULL THEN 'UNIT STARTER'
                ELSE 'UNIT STARTER: ' || ru.canonical_title
            END
        WHEN ru.canonical_key LIKE 'UNIT|%' THEN
            CASE
                WHEN ru.canonical_title IS NULL THEN 'UNIT ' || SUBSTRING(ru.canonical_key FROM 'UNIT\|([0-9]+)')
                ELSE 'UNIT ' || SUBSTRING(ru.canonical_key FROM 'UNIT\|([0-9]+)') || ': ' || ru.canonical_title
            END
        WHEN ru.canonical_key LIKE 'REVISION|%' THEN
            'REVISION ' || SUBSTRING(ru.canonical_key FROM 'REVISION\|([0-9]+)')
        ELSE REGEXP_REPLACE(REGEXP_REPLACE(ru.raw_name, '[!?.;,]+$', '', 'g'), '\s+', ' ', 'g')
    END AS canonical_name,
    ru.merged_order_index,
    FIRST_VALUE(ru."Id") OVER (
        PARTITION BY ru."ModuleId", ru.canonical_key
        ORDER BY ru.canonical_rank
    ) AS canonical_unit_id
FROM ranked_units AS ru;

DROP TABLE IF EXISTS tmp_unit_merge_duplicates;
CREATE TEMP TABLE tmp_unit_merge_duplicates AS
SELECT *
FROM tmp_unit_merge_candidates
WHERE unit_id <> canonical_unit_id;

UPDATE public."LessonPlanTemplates" AS lpt
SET
    "LessonPlanUnitId" = dup.canonical_unit_id,
    "UpdatedAt" = NOW()
FROM tmp_unit_merge_duplicates AS dup
WHERE lpt."LessonPlanUnitId" = dup.unit_id;

UPDATE public."LessonPlanUnits" AS u
SET
    "IsActive" = FALSE,
    "UpdatedAt" = NOW(),
    "NameNormalized" = LEFT(COALESCE(u."NameNormalized", 'UNIT') || '|MERGED|' || RIGHT(u."Id"::text, 12), 255)
FROM tmp_unit_merge_duplicates AS dup
WHERE u."Id" = dup.unit_id;

WITH canonical_updates AS (
    SELECT DISTINCT ON (canonical_unit_id)
        canonical_unit_id,
        canonical_name,
        canonical_key,
        merged_order_index
    FROM tmp_unit_merge_candidates
    ORDER BY canonical_unit_id, merged_order_index, canonical_name
)
UPDATE public."LessonPlanUnits" AS u
SET
    "Name" = cu.canonical_name,
    "NameNormalized" = cu.canonical_key,
    "OrderIndex" = cu.merged_order_index,
    "IsActive" = TRUE,
    "UpdatedAt" = NOW()
FROM canonical_updates AS cu
WHERE u."Id" = cu.canonical_unit_id;

-- Optional hard delete after verification:
-- DELETE FROM public."LessonPlanUnits" AS u
-- WHERE u."IsActive" = FALSE
--   AND NOT EXISTS (
--       SELECT 1
--       FROM public."LessonPlanTemplates" AS lpt
--       WHERE lpt."LessonPlanUnitId" = u."Id");

COMMIT;
