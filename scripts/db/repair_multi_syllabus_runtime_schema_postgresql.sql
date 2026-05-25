ALTER TABLE public."Sessions"
ADD COLUMN IF NOT EXISTS "CurriculumSnapshotJson" text NULL;

ALTER TABLE public."LessonPlanTemplates"
ADD COLUMN IF NOT EXISTS "SyllabusId" uuid NULL;

UPDATE public."LessonPlanTemplates" AS lpt
SET "SyllabusId" = st."SyllabusId"
FROM public."SessionTemplates" AS st
WHERE lpt."SessionTemplateId" = st."Id"
  AND lpt."SyllabusId" IS NULL;

UPDATE public."LessonPlanTemplates" AS lpt
SET "SyllabusId" = chosen."Id"
FROM public."Modules" AS m
INNER JOIN public."Levels" AS lvl ON lvl."Id" = m."LevelId"
INNER JOIN LATERAL (
    SELECT s."Id"
    FROM public."Syllabuses" AS s
    WHERE s."ProgramId" = lvl."ProgramId"
      AND s."LevelId" = m."LevelId"
      AND s."IsDeleted" = FALSE
    ORDER BY s."IsActive" DESC, s."UpdatedAt" DESC, s."CreatedAt" DESC
    LIMIT 1
) AS chosen ON TRUE
WHERE lpt."ModuleId" = m."Id"
  AND lpt."SyllabusId" IS NULL;

DO $$
BEGIN
    IF EXISTS (
        SELECT 1
        FROM information_schema.columns
        WHERE table_schema = 'public'
          AND table_name = 'LessonPlanTemplates'
          AND column_name = 'SyllabusId'
          AND is_nullable = 'YES'
    ) AND NOT EXISTS (
        SELECT 1
        FROM public."LessonPlanTemplates"
        WHERE "SyllabusId" IS NULL
    ) THEN
        ALTER TABLE public."LessonPlanTemplates"
        ALTER COLUMN "SyllabusId" SET NOT NULL;
    END IF;
END
$$;

CREATE TABLE IF NOT EXISTS public."CurriculumAssignments"
(
    "Id" uuid NOT NULL,
    "BranchId" uuid NOT NULL,
    "ProgramId" uuid NOT NULL,
    "LevelId" uuid NOT NULL,
    "SyllabusId" uuid NOT NULL,
    "EffectiveFrom" timestamp with time zone NULL,
    "EffectiveTo" timestamp with time zone NULL,
    "IsActive" boolean NOT NULL,
    "CreatedAt" timestamp with time zone NOT NULL,
    "UpdatedAt" timestamp with time zone NOT NULL,
    CONSTRAINT "PK_CurriculumAssignments" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_CurriculumAssignments_Branches_BranchId" FOREIGN KEY ("BranchId") REFERENCES public."Branches" ("Id") ON DELETE RESTRICT,
    CONSTRAINT "FK_CurriculumAssignments_Levels_LevelId" FOREIGN KEY ("LevelId") REFERENCES public."Levels" ("Id") ON DELETE RESTRICT,
    CONSTRAINT "FK_CurriculumAssignments_Programs_ProgramId" FOREIGN KEY ("ProgramId") REFERENCES public."Programs" ("Id") ON DELETE RESTRICT,
    CONSTRAINT "FK_CurriculumAssignments_Syllabuses_SyllabusId" FOREIGN KEY ("SyllabusId") REFERENCES public."Syllabuses" ("Id") ON DELETE CASCADE
);

INSERT INTO public."CurriculumAssignments"
    ("Id", "BranchId", "ProgramId", "LevelId", "SyllabusId", "EffectiveFrom", "EffectiveTo", "IsActive", "CreatedAt", "UpdatedAt")
SELECT (
           substring(md5(bp."BranchId"::text || ':' || s."Id"::text), 1, 8) || '-' ||
           substring(md5(bp."BranchId"::text || ':' || s."Id"::text), 9, 4) || '-' ||
           substring(md5(bp."BranchId"::text || ':' || s."Id"::text), 13, 4) || '-' ||
           substring(md5(bp."BranchId"::text || ':' || s."Id"::text), 17, 4) || '-' ||
           substring(md5(bp."BranchId"::text || ':' || s."Id"::text), 21, 12)
       )::uuid,
       bp."BranchId",
       s."ProgramId",
       s."LevelId",
       s."Id",
       s."EffectiveFrom",
       s."EffectiveTo",
       TRUE,
       NOW(),
       NOW()
FROM public."BranchPrograms" AS bp
INNER JOIN public."Syllabuses" AS s
    ON s."ProgramId" = bp."ProgramId"
WHERE bp."IsActive" = TRUE
  AND s."IsDeleted" = FALSE
  AND s."IsActive" = TRUE
ON CONFLICT ("BranchId", "ProgramId", "LevelId", "SyllabusId", "EffectiveFrom") DO NOTHING;

DROP INDEX IF EXISTS public."IX_LessonPlanTemplates_ModuleId_SessionIndex";

CREATE INDEX IF NOT EXISTS "IX_CurriculumAssignments_BranchId_ProgramId_LevelId_IsActive"
ON public."CurriculumAssignments" ("BranchId", "ProgramId", "LevelId", "IsActive");

CREATE UNIQUE INDEX IF NOT EXISTS "IX_CurriculumAssignments_BranchId_ProgramId_LevelId_SyllabusId_EffectiveFrom"
ON public."CurriculumAssignments" ("BranchId", "ProgramId", "LevelId", "SyllabusId", "EffectiveFrom");

CREATE INDEX IF NOT EXISTS "IX_CurriculumAssignments_LevelId"
ON public."CurriculumAssignments" ("LevelId");

CREATE INDEX IF NOT EXISTS "IX_CurriculumAssignments_ProgramId"
ON public."CurriculumAssignments" ("ProgramId");

CREATE INDEX IF NOT EXISTS "IX_CurriculumAssignments_SyllabusId"
ON public."CurriculumAssignments" ("SyllabusId");

CREATE UNIQUE INDEX IF NOT EXISTS "IX_LessonPlanTemplates_SyllabusId_ModuleId_SessionIndex"
ON public."LessonPlanTemplates" ("SyllabusId", "ModuleId", "SessionIndex");

CREATE UNIQUE INDEX IF NOT EXISTS "IX_LessonPlanTemplates_SyllabusId_ModuleId_SessionOrder"
ON public."LessonPlanTemplates" ("SyllabusId", "ModuleId", "SessionOrder");

DO $$
BEGIN
    IF NOT EXISTS (
        SELECT 1
        FROM pg_constraint
        WHERE conname = 'FK_LessonPlanTemplates_Syllabuses_SyllabusId'
    ) THEN
        ALTER TABLE public."LessonPlanTemplates"
        ADD CONSTRAINT "FK_LessonPlanTemplates_Syllabuses_SyllabusId"
        FOREIGN KEY ("SyllabusId")
        REFERENCES public."Syllabuses" ("Id")
        ON DELETE CASCADE;
    END IF;
END
$$;
