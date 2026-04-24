-- Run in PostgreSQL schema `public`.
-- Purpose:
-- 1. Clear only session data and direct session-linked records.
-- 2. Upsert seeded classrooms/classes/schedule segments.
-- 3. Assign the provided teachers without timetable conflicts.
-- 4. STARTERS-S1 starts on 2026-01-01.

BEGIN;

CREATE TEMP TABLE seed_context AS
SELECT
    timezone('UTC', now()) AS now_utc,
    CURRENT_DATE::date AS default_start_date,
    (CURRENT_DATE + INTERVAL '12 months' - INTERVAL '1 day')::date AS default_end_date,
    COALESCE(
        (SELECT "Id"
         FROM public."Branches"
         WHERE "IsActive" = TRUE
         ORDER BY "CreatedAt" NULLS LAST, "Id"
         LIMIT 1),
        (SELECT "Id"
         FROM public."Branches"
         ORDER BY "CreatedAt" NULLS LAST, "Id"
         LIMIT 1)
    )::uuid AS primary_branch_id;

CREATE TEMP TABLE required_branch
(
    primary_branch_id uuid NOT NULL
);

INSERT INTO required_branch (primary_branch_id)
SELECT primary_branch_id
FROM seed_context;

CREATE TEMP TABLE seed_classes
(
    class_id uuid PRIMARY KEY,
    room_id uuid NOT NULL,
    program_code text NOT NULL,
    code text NOT NULL,
    title text NOT NULL,
    capacity integer NOT NULL,
    main_teacher_id uuid NOT NULL,
    start_date date NOT NULL,
    end_date date NOT NULL,
    weekly_schedule_json text NOT NULL,
    description text
);

INSERT INTO seed_classes
(
    class_id,
    room_id,
    program_code,
    code,
    title,
    capacity,
    main_teacher_id,
    start_date,
    end_date,
    weekly_schedule_json,
    description
)
SELECT
    data.class_id,
    data.room_id,
    data.program_code,
    data.code,
    data.title,
    data.capacity,
    data.main_teacher_id,
    data.start_date,
    data.end_date,
    data.weekly_schedule_json,
    data.description
FROM seed_context ctx
CROSS JOIN LATERAL (
    VALUES
        ('22222222-2222-2222-2222-222222222301'::uuid, '44444444-4444-4444-4444-444444444301'::uuid, 'APPLE2',   'APPLE-A2',     'Apple A2',                         8, '559ad104-7238-4505-bbcf-7dce076af584'::uuid, CURRENT_DATE, DATE '2026-12-31', '{"type":"weekly-slots","slots":[{"dayOfWeek":"TH","startTime":"18:00","durationMinutes":60},{"dayOfWeek":"SA","startTime":"17:00","durationMinutes":60}]}'::text, 'Age 4-6. Thu 5 18:00-19:00, Thu 7 17:00-18:00.'),
        ('22222222-2222-2222-2222-222222222302'::uuid, '44444444-4444-4444-4444-444444444302'::uuid, 'PHONICS',  'PHONICS-P1',   'Phonics P1',                       6, '559ad104-7238-4505-bbcf-7dce076af584'::uuid, CURRENT_DATE, (CURRENT_DATE + INTERVAL '12 months' - INTERVAL '1 day')::date, '{"type":"weekly-slots","slots":[{"dayOfWeek":"MO","startTime":"19:00","durationMinutes":90},{"dayOfWeek":"WE","startTime":"19:00","durationMinutes":90}]}'::text, 'Age 5-6. Mon 19:00-20:30, Wed 19:00-20:30.'),
        ('22222222-2222-2222-2222-222222222303'::uuid, '44444444-4444-4444-4444-444444444303'::uuid, 'PHONICS',  'PHONICS-P3',   'Phonics P3 + Thuyet trinh',        4, '559ad104-7238-4505-bbcf-7dce076af584'::uuid, CURRENT_DATE, (CURRENT_DATE + INTERVAL '12 months' - INTERVAL '1 day')::date, '{"type":"weekly-slots","slots":[{"dayOfWeek":"TU","startTime":"16:30","durationMinutes":90},{"dayOfWeek":"FR","startTime":"17:30","durationMinutes":90}]}'::text, 'Tue 16:30-18:00, Fri 17:30-19:00.'),
        ('22222222-2222-2222-2222-222222222304'::uuid, '44444444-4444-4444-4444-444444444304'::uuid, 'STARTERS', 'STARTERS-S1',  'Starters S1',                     10, '1cb3f53c-2b97-4937-a3ff-494aad1c190d'::uuid, DATE '2026-01-01',       DATE '2026-12-31', '{"type":"weekly-slots","slots":[{"dayOfWeek":"TH","startTime":"16:00","durationMinutes":90},{"dayOfWeek":"FR","startTime":"19:30","durationMinutes":90}]}'::text, 'Age 6-8. Thu 16:00-17:30, Fri 19:30-21:00.'),
        ('22222222-2222-2222-2222-222222222305'::uuid, '44444444-4444-4444-4444-444444444305'::uuid, 'STARTERS', 'STARTERS-S5',  'Starters S5',                     10, 'b6a91d11-5f38-490e-aff0-23e2887355ea'::uuid, CURRENT_DATE, (CURRENT_DATE + INTERVAL '12 months' - INTERVAL '1 day')::date, '{"type":"weekly-slots","slots":[{"dayOfWeek":"WE","startTime":"19:30","durationMinutes":90},{"dayOfWeek":"FR","startTime":"18:00","durationMinutes":90}]}'::text, 'Wed 19:30-21:00, Fri 18:00-19:30.'),
        ('22222222-2222-2222-2222-222222222306'::uuid, '44444444-4444-4444-4444-444444444306'::uuid, 'MOVERS',   'MOVERS-M4',    'Movers M4',                        7, '1cb3f53c-2b97-4937-a3ff-494aad1c190d'::uuid, CURRENT_DATE, (CURRENT_DATE + INTERVAL '12 months' - INTERVAL '1 day')::date, '{"type":"weekly-slots","slots":[{"dayOfWeek":"TU","startTime":"19:30","durationMinutes":90},{"dayOfWeek":"TH","startTime":"19:30","durationMinutes":90}]}'::text, 'Tue 19:30-21:00, Thu 19:30-21:00.'),
        ('22222222-2222-2222-2222-222222222307'::uuid, '44444444-4444-4444-4444-444444444307'::uuid, 'MOVERS',   'MOVERS-M2',    'Movers M2',                        8, '1cb3f53c-2b97-4937-a3ff-494aad1c190d'::uuid, CURRENT_DATE, (CURRENT_DATE + INTERVAL '12 months' - INTERVAL '1 day')::date, '{"type":"weekly-slots","slots":[{"dayOfWeek":"MO","startTime":"17:30","durationMinutes":90},{"dayOfWeek":"WE","startTime":"17:30","durationMinutes":90}]}'::text, 'Mon 17:30-19:00, Wed 17:30-19:00.'),
        ('22222222-2222-2222-2222-222222222308'::uuid, '44444444-4444-4444-4444-444444444308'::uuid, 'MOVERS',   'MOVERS-M3',    'Movers M3',                        4, 'b6a91d11-5f38-490e-aff0-23e2887355ea'::uuid, CURRENT_DATE, (CURRENT_DATE + INTERVAL '12 months' - INTERVAL '1 day')::date, '{"type":"weekly-slots","slots":[{"dayOfWeek":"WE","startTime":"16:00","durationMinutes":90},{"dayOfWeek":"SA","startTime":"15:30","durationMinutes":90}]}'::text, 'Wed 16:00-17:30, Sat 15:30-17:00.'),
        ('22222222-2222-2222-2222-222222222309'::uuid, '44444444-4444-4444-4444-444444444309'::uuid, 'FLYERS',   'FLYERS-F1',    'Flyers F1',                       10, 'b6a91d11-5f38-490e-aff0-23e2887355ea'::uuid, CURRENT_DATE, (CURRENT_DATE + INTERVAL '12 months' - INTERVAL '1 day')::date, '{"type":"weekly-slots","slots":[{"dayOfWeek":"TU","startTime":"18:00","durationMinutes":90},{"dayOfWeek":"TH","startTime":"18:30","durationMinutes":90}]}'::text, 'Tue 18:00-19:30, Thu 18:30-20:00.'),
        ('22222222-2222-2222-2222-222222222310'::uuid, '44444444-4444-4444-4444-444444444310'::uuid, 'FLYERS',   'FLYERS-F2',    'Flyers F2',                        5, 'a5e1f731-1132-40bd-bc66-2b4521d965dc'::uuid, CURRENT_DATE, (CURRENT_DATE + INTERVAL '12 months' - INTERVAL '1 day')::date, '{"type":"weekly-slots","slots":[{"dayOfWeek":"MO","startTime":"16:00","durationMinutes":90},{"dayOfWeek":"SA","startTime":"17:00","durationMinutes":90}]}'::text, 'Mon 16:00-17:30, Sat 17:00-18:30.'),
        ('22222222-2222-2222-2222-222222222311'::uuid, '44444444-4444-4444-4444-444444444311'::uuid, 'PETB1',    'PET-K1',       'PET K1',                           5, '559ad104-7238-4505-bbcf-7dce076af584'::uuid, CURRENT_DATE, (CURRENT_DATE + INTERVAL '12 months' - INTERVAL '1 day')::date, '{"type":"weekly-slots","slots":[{"dayOfWeek":"FR","startTime":"19:30","durationMinutes":90},{"dayOfWeek":"SA","startTime":"10:00","durationMinutes":90}]}'::text, 'Fri 19:30-21:00, Sat 10:00-11:30.'),
        ('22222222-2222-2222-2222-222222222312'::uuid, '44444444-4444-4444-4444-444444444312'::uuid, 'KEMLMS',   'LMS-ESL-WED',  'Kem LMS - ESL Grade 3 - Cam (Wed)',10, 'a5e1f731-1132-40bd-bc66-2b4521d965dc'::uuid, CURRENT_DATE, (CURRENT_DATE + INTERVAL '12 months' - INTERVAL '1 day')::date, '{"type":"weekly-slots","slots":[{"dayOfWeek":"WE","startTime":"16:00","durationMinutes":90}]}'::text, 'Wed 16:00-17:30.'),
        ('22222222-2222-2222-2222-222222222313'::uuid, '44444444-4444-4444-4444-444444444313'::uuid, 'KEMLMS',   'LMS-ESL-THU',  'Kem LMS - ESL Grade 3 - Cam (Thu)',10, 'b6a91d11-5f38-490e-aff0-23e2887355ea'::uuid, CURRENT_DATE, (CURRENT_DATE + INTERVAL '12 months' - INTERVAL '1 day')::date, '{"type":"weekly-slots","slots":[{"dayOfWeek":"TH","startTime":"19:30","durationMinutes":90}]}'::text, 'Thu 19:30-21:00.'),
        ('22222222-2222-2222-2222-222222222314'::uuid, '44444444-4444-4444-4444-444444444314'::uuid, 'KEMLMS',   'LMS-SCI-WED',  'Kem LMS - Science Grade 3 - Cam', 10, 'a5e1f731-1132-40bd-bc66-2b4521d965dc'::uuid, CURRENT_DATE, (CURRENT_DATE + INTERVAL '12 months' - INTERVAL '1 day')::date, '{"type":"weekly-slots","slots":[{"dayOfWeek":"WE","startTime":"17:30","durationMinutes":90}]}'::text, 'Wed 17:30-19:00.')
) AS data
(
    class_id,
    room_id,
    program_code,
    code,
    title,
    capacity,
    main_teacher_id,
    start_date,
    end_date,
    weekly_schedule_json,
    description
);

CREATE TEMP TABLE resolved_programs
(
    program_code text PRIMARY KEY,
    program_id uuid NOT NULL
);

INSERT INTO resolved_programs (program_code, program_id)
SELECT required_codes.program_code, p."Id"
FROM (SELECT DISTINCT program_code FROM seed_classes) required_codes
LEFT JOIN public."Programs" p
    ON p."Code" = required_codes.program_code;

-- Clear only session data and records directly linked to sessions.
UPDATE public."ReportRequests"
SET "TargetSessionId" = NULL
WHERE "TargetSessionId" IN (SELECT "Id" FROM public."Sessions");

DELETE FROM public."HomeworkAssignments"
WHERE "SessionId" IN (SELECT "Id" FROM public."Sessions");

DELETE FROM public."LeaveRequests"
WHERE "SessionId" IN (SELECT "Id" FROM public."Sessions");

DELETE FROM public."MakeupAllocations"
WHERE "TargetSessionId" IN (SELECT "Id" FROM public."Sessions");

DELETE FROM public."MakeupCredits"
WHERE "SourceSessionId" IN (SELECT "Id" FROM public."Sessions")
   OR "UsedSessionId" IN (SELECT "Id" FROM public."Sessions");

DELETE FROM public."SessionReports"
WHERE "SessionId" IN (SELECT "Id" FROM public."Sessions");

DELETE FROM public."SessionRoles"
WHERE "SessionId" IN (SELECT "Id" FROM public."Sessions");

DELETE FROM public."Attendances"
WHERE "SessionId" IN (SELECT "Id" FROM public."Sessions");

DELETE FROM public."StudentSessionAssignments"
WHERE "SessionId" IN (SELECT "Id" FROM public."Sessions");

DELETE FROM public."LessonPlans"
WHERE "SessionId" IN (SELECT "Id" FROM public."Sessions");

DELETE FROM public."Sessions";

INSERT INTO public."Classrooms"
(
    "Id",
    "BranchId",
    "Name",
    "Capacity",
    "Note",
    "Floor",
    "Area",
    "EquipmentJson",
    "IsActive"
)
SELECT
    sc.room_id,
    ctx.primary_branch_id,
    'Room ' || sc.code,
    sc.capacity,
    'Seed room for ' || sc.title,
    NULL,
    NULL,
    NULL,
    TRUE
FROM seed_classes sc
CROSS JOIN seed_context ctx
ON CONFLICT ("Id") DO UPDATE
SET
    "BranchId" = EXCLUDED."BranchId",
    "Name" = EXCLUDED."Name",
    "Capacity" = EXCLUDED."Capacity",
    "Note" = EXCLUDED."Note",
    "Floor" = EXCLUDED."Floor",
    "Area" = EXCLUDED."Area",
    "EquipmentJson" = EXCLUDED."EquipmentJson",
    "IsActive" = EXCLUDED."IsActive";

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
    sc.class_id,
    ctx.primary_branch_id,
    rp.program_id,
    sc.code,
    sc.title,
    sc.room_id,
    sc.main_teacher_id,
    NULL,
    sc.start_date,
    sc.end_date,
    'Active',
    sc.capacity,
    sc.weekly_schedule_json,
    sc.description,
    ctx.now_utc,
    ctx.now_utc
FROM seed_classes sc
JOIN resolved_programs rp
    ON rp.program_code = sc.program_code
CROSS JOIN seed_context ctx
ON CONFLICT ("Code") DO UPDATE
SET
    "BranchId" = EXCLUDED."BranchId",
    "ProgramId" = EXCLUDED."ProgramId",
    "Title" = EXCLUDED."Title",
    "RoomId" = EXCLUDED."RoomId",
    "MainTeacherId" = EXCLUDED."MainTeacherId",
    "AssistantTeacherId" = EXCLUDED."AssistantTeacherId",
    "StartDate" = EXCLUDED."StartDate",
    "EndDate" = EXCLUDED."EndDate",
    "Status" = EXCLUDED."Status",
    "Capacity" = EXCLUDED."Capacity",
    "SchedulePattern" = EXCLUDED."SchedulePattern",
    "Description" = EXCLUDED."Description",
    "UpdatedAt" = EXCLUDED."UpdatedAt";

DELETE FROM public."ClassScheduleSegments"
WHERE "ClassId" IN (
    SELECT c."Id"
    FROM public."Classes" c
    JOIN seed_classes sc
        ON sc.code = c."Code"
);

INSERT INTO public."ClassScheduleSegments"
(
    "Id",
    "ClassId",
    "EffectiveFrom",
    "EffectiveTo",
    "SchedulePattern",
    "CreatedAt",
    "UpdatedAt"
)
SELECT
    (
        substr(md5(c."Id"::text || '-segment'), 1, 8) || '-' ||
        substr(md5(c."Id"::text || '-segment'), 9, 4) || '-' ||
        substr(md5(c."Id"::text || '-segment'), 13, 4) || '-' ||
        substr(md5(c."Id"::text || '-segment'), 17, 4) || '-' ||
        substr(md5(c."Id"::text || '-segment'), 21, 12)
    )::uuid,
    c."Id",
    sc.start_date,
    NULL,
    sc.weekly_schedule_json,
    ctx.now_utc,
    ctx.now_utc
FROM public."Classes" c
JOIN seed_classes sc
    ON sc.code = c."Code"
CROSS JOIN seed_context ctx;

SELECT 'Sessions cleared' AS check_name, COUNT(*) AS total
FROM public."Sessions";

SELECT 'Classes upserted' AS check_name, COUNT(*) AS total
FROM public."Classes"
WHERE "Code" IN (SELECT code FROM seed_classes);

COMMIT;
