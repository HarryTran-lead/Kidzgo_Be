BEGIN;

-- This script keeps source data as-is wherever the schema allows NULL.
-- For schema-required fields that are not present in the source payload:
-- 1. "Code" values are generated consistently from Name/Title.
-- 2. "StartDate" uses CURRENT_DATE because the column is NOT NULL.
-- 3. "Status" uses 'Planned' because the column is NOT NULL.

CREATE OR REPLACE FUNCTION pg_temp.seed_uuid(seed text)
RETURNS uuid
LANGUAGE sql
IMMUTABLE
AS $$
    SELECT (
        substr(md5(seed), 1, 8) || '-' ||
        substr(md5(seed), 9, 4) || '-' ||
        substr(md5(seed), 13, 4) || '-' ||
        substr(md5(seed), 17, 4) || '-' ||
        substr(md5(seed), 21, 12)
    )::uuid;
$$;

CREATE OR REPLACE FUNCTION pg_temp.base_code(seed text)
RETURNS text
LANGUAGE sql
IMMUTABLE
AS $$
    SELECT upper(
        regexp_replace(
            translate(
                lower(coalesce(seed, '')),
                'àáạảãâầấậẩẫăằắặẳẵèéẹẻẽêềếệểễìíịỉĩòóọỏõôồốộổỗơờớợởỡùúụủũưừứựửữỳýỵỷỹđ',
                'aaaaaaaaaaaaaaaaaeeeeeeeeeeeiiiiiooooooooooooooooouuuuuuuuuuuyyyyyd'
            ),
            '[^a-z0-9]+',
            '',
            'g'
        )
    );
$$;

CREATE OR REPLACE FUNCTION pg_temp.required_code(seed text, prefix text, max_len integer)
RETURNS text
LANGUAGE sql
IMMUTABLE
AS $$
    SELECT CASE
        WHEN pg_temp.base_code(seed) = '' THEN LEFT(prefix || substr(md5(seed), 1, GREATEST(max_len - length(prefix), 0)), max_len)
        ELSE LEFT(pg_temp.base_code(seed), max_len)
    END;
$$;

CREATE TEMP TABLE seed_branch
(
    name text NOT NULL,
    address text NULL,
    contact_email text NULL,
    contact_phone text NULL
) ON COMMIT DROP;

INSERT INTO seed_branch (name, address, contact_email, contact_phone)
VALUES
    (
        'Anh Ngữ Rex',
        'S302 Vinhomes Grand Park, phường Long Bình, thành Phố Hồ Chí Minh',
        'tearexenglish@gmail.com',
        NULL
    );

CREATE TEMP TABLE seed_programs
(
    source_order integer PRIMARY KEY,
    name text NOT NULL,
    is_supplementary boolean NOT NULL
) ON COMMIT DROP;

INSERT INTO seed_programs (source_order, name, is_supplementary)
VALUES
    (1, 'Apple 2', false),
    (2, 'Phonics Foundation', false),
    (3, 'Cambridge Starters', false),
    (4, 'Cambridge Movers', false),
    (5, 'Cambridge Flyers', false),
    (6, 'KET (A2 Key)', false),
    (7, 'PET (B1 Preliminary)', false),
    (8, 'Kèm LMS', true),
    (9, 'Speaking Club (cuối tuần)', true),
    (10, 'Writing Booster', true),
    (11, 'Summer Camp', true);

CREATE TEMP TABLE seed_tuition_plans
(
    source_order integer PRIMARY KEY,
    program_name text NOT NULL,
    name text NOT NULL,
    total_sessions integer NOT NULL,
    tuition_amount numeric NOT NULL,
    currency text NOT NULL
) ON COMMIT DROP;

INSERT INTO seed_tuition_plans (source_order, program_name, name, total_sessions, tuition_amount, currency)
VALUES
    (1, 'Apple 2', 'Basic Plan', 24, 4200000, 'VND'),
    (2, 'Apple 2', 'Standard Plan', 48, 7800000, 'VND'),
    (3, 'Apple 2', 'Premium Plan', 96, 15600000, 'VND'),
    (4, 'Phonics Foundation', 'Basic Plan', 24, 4200000, 'VND'),
    (5, 'Phonics Foundation', 'Standard Plan', 48, 7800000, 'VND'),
    (6, 'Phonics Foundation', 'Premium Plan', 96, 15600000, 'VND'),
    (7, 'Cambridge Movers', 'Basic Plan', 24, 4200000, 'VND'),
    (8, 'Cambridge Movers', 'Standard Plan', 48, 7800000, 'VND'),
    (9, 'Cambridge Movers', 'Premium Plan', 96, 15600000, 'VND'),
    (10, 'Cambridge Flyers', 'Basic Plan', 24, 4200000, 'VND'),
    (11, 'Cambridge Flyers', 'Standard Plan', 48, 7800000, 'VND'),
    (12, 'Cambridge Flyers', 'Premium Plan', 96, 15600000, 'VND'),
    (13, 'KET (A2 Key)', 'Basic Plan', 24, 4200000, 'VND'),
    (14, 'KET (A2 Key)', 'Standard Plan', 48, 7800000, 'VND'),
    (15, 'KET (A2 Key)', 'Premium Plan', 96, 15600000, 'VND'),
    (16, 'PET (B1 Preliminary)', 'Basic Plan', 24, 4200000, 'VND'),
    (17, 'PET (B1 Preliminary)', 'Standard Plan', 48, 7800000, 'VND'),
    (18, 'PET (B1 Preliminary)', 'Premium Plan', 96, 15600000, 'VND'),
    (19, 'Speaking Club (cuối tuần)', 'English Club (phụ)', 12, 600000, 'VND');

CREATE TEMP TABLE seed_classes
(
    source_order integer PRIMARY KEY,
    program_name text NOT NULL,
    title text NOT NULL,
    capacity integer NOT NULL
) ON COMMIT DROP;

INSERT INTO seed_classes (source_order, program_name, title, capacity)
VALUES
    (1, 'Apple 2', 'Apple A1', 10),
    (2, 'Phonics Foundation', 'Phonics P1', 8),
    (3, 'Phonics Foundation', 'Phonics P2', 10),
    (4, 'Cambridge Starters', 'Starters S1', 10),
    (5, 'Cambridge Starters', 'Starters S2', 10),
    (6, 'Cambridge Movers', 'Movers M1', 7),
    (7, 'Cambridge Movers', 'Movers M2', 8),
    (8, 'Cambridge Flyers', 'Flyers F1', 10),
    (9, 'KET (A2 Key)', 'KET K1', 8);

DO $$
DECLARE
    branch_id uuid;
    now_utc timestamp with time zone := now();
    class_start_date date := CURRENT_DATE;
BEGIN
    SELECT b."Id"
    INTO branch_id
    FROM public."Branches" AS b
    INNER JOIN seed_branch AS sb
        ON sb.name = b."Name"
    LIMIT 1;

    IF branch_id IS NULL THEN
        branch_id := pg_temp.seed_uuid('branch:' || (SELECT name FROM seed_branch LIMIT 1));

        INSERT INTO public."Branches"
        (
            "Id",
            "Code",
            "Name",
            "Address",
            "ContactPhone",
            "ContactEmail",
            "IsActive",
            "CreatedAt",
            "UpdatedAt"
        )
        SELECT
            branch_id,
            pg_temp.required_code(sb.name, 'B', 32),
            sb.name,
            sb.address,
            sb.contact_phone,
            sb.contact_email,
            true,
            now_utc,
            now_utc
        FROM seed_branch AS sb;
    ELSE
        UPDATE public."Branches" AS b
        SET "Code" = pg_temp.required_code(sb.name, 'B', 32),
            "Name" = sb.name,
            "Address" = sb.address,
            "ContactPhone" = sb.contact_phone,
            "ContactEmail" = sb.contact_email,
            "IsActive" = true,
            "UpdatedAt" = now_utc
        FROM seed_branch AS sb
        WHERE b."Id" = branch_id;
    END IF;

    UPDATE public."Programs" AS p
    SET "Name" = sp.name,
        "Code" = pg_temp.required_code(sp.name, 'P', 10),
        "Description" = NULL,
        "IsActive" = true,
        "IsDeleted" = false,
        "IsMakeup" = false,
        "IsSupplementary" = sp.is_supplementary,
        "UpdatedAt" = now_utc
    FROM seed_programs AS sp
    WHERE p."Id" = pg_temp.seed_uuid('program:' || sp.name);

    INSERT INTO public."Programs"
    (
        "Id",
        "Name",
        "Code",
        "Description",
        "IsActive",
        "IsDeleted",
        "IsMakeup",
        "IsSupplementary",
        "CreatedAt",
        "UpdatedAt"
    )
    SELECT
        pg_temp.seed_uuid('program:' || sp.name),
        sp.name,
        pg_temp.required_code(sp.name, 'P', 10),
        NULL,
        true,
        false,
        false,
        sp.is_supplementary,
        now_utc,
        now_utc
    FROM seed_programs AS sp
    WHERE NOT EXISTS
    (
        SELECT 1
        FROM public."Programs" AS p
        WHERE p."Id" = pg_temp.seed_uuid('program:' || sp.name)
    );

    INSERT INTO public."BranchPrograms"
    (
        "Id",
        "BranchId",
        "ProgramId",
        "IsActive",
        "DefaultMakeupClassId",
        "CreatedAt",
        "UpdatedAt"
    )
    SELECT
        pg_temp.seed_uuid('branch-program:' || branch_id::text || ':' || sp.name),
        branch_id,
        pg_temp.seed_uuid('program:' || sp.name),
        true,
        NULL,
        now_utc,
        now_utc
    FROM seed_programs AS sp
    ON CONFLICT ("BranchId", "ProgramId") DO UPDATE
    SET "IsActive" = true,
        "DefaultMakeupClassId" = NULL,
        "UpdatedAt" = EXCLUDED."UpdatedAt";

    UPDATE public."TuitionPlans" AS tp
    SET "BranchId" = branch_id,
        "TotalSessions" = stp.total_sessions,
        "TuitionAmount" = stp.tuition_amount,
        "UnitPriceSession" = ROUND(stp.tuition_amount / NULLIF(stp.total_sessions, 0), 2),
        "Currency" = stp.currency,
        "IsActive" = true,
        "IsDeleted" = false,
        "UpdatedAt" = now_utc
    FROM seed_tuition_plans AS stp
    INNER JOIN public."Programs" AS p
        ON p."Id" = pg_temp.seed_uuid('program:' || stp.program_name)
    WHERE tp."ProgramId" = p."Id"
      AND tp."BranchId" = branch_id
      AND tp."Name" = stp.name;

    INSERT INTO public."TuitionPlans"
    (
        "Id",
        "BranchId",
        "ProgramId",
        "Name",
        "TotalSessions",
        "TuitionAmount",
        "UnitPriceSession",
        "Currency",
        "IsActive",
        "IsDeleted",
        "CreatedAt",
        "UpdatedAt"
    )
    SELECT
        pg_temp.seed_uuid('tuition-plan:' || stp.program_name || ':' || stp.name),
        branch_id,
        p."Id",
        stp.name,
        stp.total_sessions,
        stp.tuition_amount,
        ROUND(stp.tuition_amount / NULLIF(stp.total_sessions, 0), 2),
        stp.currency,
        true,
        false,
        now_utc,
        now_utc
    FROM seed_tuition_plans AS stp
    INNER JOIN public."Programs" AS p
        ON p."Id" = pg_temp.seed_uuid('program:' || stp.program_name)
    WHERE NOT EXISTS
    (
        SELECT 1
        FROM public."TuitionPlans" AS tp
        WHERE tp."ProgramId" = p."Id"
          AND tp."BranchId" = branch_id
          AND tp."Name" = stp.name
    );

    UPDATE public."Classes" AS c
    SET "BranchId" = branch_id,
        "ProgramId" = p."Id",
        "Code" = pg_temp.required_code(sc.title, 'C', 50),
        "Title" = sc.title,
        "RoomId" = NULL,
        "MainTeacherId" = NULL,
        "AssistantTeacherId" = NULL,
        "StartDate" = class_start_date,
        "EndDate" = NULL,
        "Status" = 'Planned',
        "Capacity" = sc.capacity,
        "SchedulePattern" = NULL,
        "Description" = NULL,
        "UpdatedAt" = now_utc
    FROM seed_classes AS sc
    INNER JOIN public."Programs" AS p
        ON p."Id" = pg_temp.seed_uuid('program:' || sc.program_name)
    WHERE c."BranchId" = branch_id
      AND c."Code" = pg_temp.required_code(sc.title, 'C', 50);

    INSERT INTO public."Classes"
    (
        "Id",
        "BranchId",
        "ProgramId",
        "Code",
        "Title",
        "RoomId",
        "MainTeacherId",
        "AssistantTeacherId",
        "StartDate",
        "EndDate",
        "Status",
        "Capacity",
        "SchedulePattern",
        "Description",
        "CreatedAt",
        "UpdatedAt"
    )
    SELECT
        pg_temp.seed_uuid('class:' || sc.title),
        branch_id,
        p."Id",
        pg_temp.required_code(sc.title, 'C', 50),
        sc.title,
        NULL,
        NULL,
        NULL,
        class_start_date,
        NULL,
        'Planned',
        sc.capacity,
        NULL,
        NULL,
        now_utc,
        now_utc
    FROM seed_classes AS sc
    INNER JOIN public."Programs" AS p
        ON p."Id" = pg_temp.seed_uuid('program:' || sc.program_name)
    WHERE NOT EXISTS
    (
        SELECT 1
        FROM public."Classes" AS c
        WHERE c."BranchId" = branch_id
          AND c."Code" = pg_temp.required_code(sc.title, 'C', 50)
    );

    RAISE NOTICE 'Rex reference data import completed for branch id %.', branch_id;
END
$$;

COMMIT;
