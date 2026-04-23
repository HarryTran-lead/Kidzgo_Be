-- Run this script in the `kidzgo` database, schema `public`.
-- If your SQL console is still holding an old transaction, run `ROLLBACK;` first.
--
-- Assumptions:
-- 1. Keep all users with role Admin / ManagementStaff / Teacher, plus the explicit user id below.
-- 2. Keep profiles belonging to kept users to avoid orphaned staff/admin profile references.
-- 3. Keep `Branches`, `EmailTemplates`, `NotificationTemplates`, `TeacherCompensationSettings`,
--    and other system/master tables not listed below.
-- 4. "English Club" tuition is mapped to program "Speaking Club".
-- 5. Cambridge Starters receives the same 3 tuition tiers as Movers/Flyers/KET/PET.
-- 6. Kem LMS package pricing is assumed as:
--      12 buoi @ 100,000
--      24 buoi @ 90,000
--      36 buoi @ 80,000
--      48 buoi @ 70,000
-- 7. Current RRULE parser supports only one BYHOUR/BYMINUTE pair per class.
--    Classes with 2 study days but different times are forced to the first listed time.

DROP TABLE IF EXISTS keep_users;
DROP TABLE IF EXISTS keep_profiles;
DROP TABLE IF EXISTS seed_context;

CREATE TEMP TABLE seed_context AS
SELECT
    'b3bf97ee-0489-4458-ae8a-4f18e77572fe'::uuid AS keep_user_id,
    '5524df75-5a84-e66e-c862-973cbf1c7cc9'::uuid AS keep_program_id,
    'cc509804-5b7d-8005-5ebb-0a21a8300253'::uuid AS keep_class_id,
    '47399a72-e948-4461-a09a-319ffce9f359'::uuid AS keep_room_id,
    DATE '2026-04-21' AS start_date,
    DATE '2027-04-30' AS end_date,
    timezone('UTC', now()) AS now_utc,
    COALESCE(
        (SELECT "BranchId" FROM public."Classes" WHERE "Id" = 'cc509804-5b7d-8005-5ebb-0a21a8300253'::uuid),
        (
            SELECT "BranchId"
            FROM public."BranchPrograms"
            WHERE "ProgramId" = '5524df75-5a84-e66e-c862-973cbf1c7cc9'::uuid
            ORDER BY "CreatedAt" NULLS LAST, "Id"
            LIMIT 1
        ),
        (SELECT "BranchId" FROM public."Classrooms" WHERE "Id" = '47399a72-e948-4461-a09a-319ffce9f359'::uuid),
        (SELECT "BranchId" FROM public."Users" WHERE "Id" = 'b3bf97ee-0489-4458-ae8a-4f18e77572fe'::uuid),
        (SELECT "Id" FROM public."Branches" WHERE "IsActive" = TRUE ORDER BY "CreatedAt" NULLS LAST, "Id" LIMIT 1),
        (SELECT "Id" FROM public."Branches" ORDER BY "CreatedAt" NULLS LAST, "Id" LIMIT 1)
    )::uuid AS branch_id;

CREATE TEMP TABLE keep_users AS
SELECT DISTINCT u."Id"
FROM public."Users" u
JOIN seed_context ctx ON TRUE
WHERE u."Id" = ctx.keep_user_id
   OR u."Role" IN ('Admin', 'ManagementStaff', 'Teacher');

CREATE TEMP TABLE keep_profiles AS
SELECT p."Id"
FROM public."Profiles" p
WHERE EXISTS (
    SELECT 1
    FROM keep_users ku
    WHERE ku."Id" = p."UserId"
);

TRUNCATE TABLE
    public."RefreshTokens",
    public."PasswordResetTokens",
    public."ParentPinResetTokens",
    public."DeviceTokens",
    public."ParentStudentLinks",
    public."LeadActivities",
    public."LeadChildren",
    public."PlacementTests",
    public."Leads",
    public."ExamSubmissionAnswers",
    public."ExamSubmissions",
    public."ExamQuestions",
    public."ExamResults",
    public."Exams",
    public."InvoiceLines",
    public."Payments",
    public."CashbookEntries",
    public."Invoices",
    public."AttendanceStreaks",
    public."MissionProgresses",
    public."RewardRedemptions",
    public."StarTransactions",
    public."StudentLevels",
    public."HomeworkSubmissionAttempts",
    public."HomeworkStudents",
    public."HomeworkQuestions",
    public."HomeworkAssignments",
    public."LessonPlans",
    public."LessonPlanTemplates",
    public."MediaAssets",
    public."Notifications",
    public."Contracts",
    public."MonthlyWorkHours",
    public."PayrollLines",
    public."PayrollPayments",
    public."PayrollRuns",
    public."SessionRoles",
    public."ShiftAttendances",
    public."QuestionBankItems",
    public."TeachingMaterialAnnotations",
    public."TeachingMaterialBookmarks",
    public."TeachingMaterialViewProgresses",
    public."TeachingMaterialSlides",
    public."TeachingMaterials",
    public."BranchPrograms",
    public."ProgramLeavePolicies",
    public."ExtracurricularPrograms",
    public."EnrollmentConfirmationPdfs",
    public."Registrations",
    public."ReportComments",
    public."ReportRequests",
    public."MonthlyReportData",
    public."MonthlyReportJobs",
    public."StudentMonthlyReports",
    public."SessionReports",
    public."Attendances",
    public."MakeupAllocations",
    public."MakeupCredits",
    public."LeaveRequests",
    public."StudentSessionAssignments",
    public."Sessions",
    public."ClassEnrollmentScheduleSegments",
    public."PauseEnrollmentRequestHistories",
    public."PauseEnrollmentRequests",
    public."ClassEnrollments",
    public."ClassScheduleSegments",
    public."TuitionPlans",
    public."TicketComments",
    public."Tickets",
    public."AuditLogs",
    public."MissionRewardRules",
    public."Missions",
    public."RewardStoreItems",
    public."Blogs"
RESTART IDENTITY CASCADE;

DELETE FROM public."Profiles" p
WHERE NOT EXISTS (
    SELECT 1
    FROM keep_profiles kp
    WHERE kp."Id" = p."Id"
);

DELETE FROM public."Users" u
WHERE NOT EXISTS (
    SELECT 1
    FROM keep_users ku
    WHERE ku."Id" = u."Id"
);

DELETE FROM public."Classes" c
USING seed_context ctx
WHERE c."Id" <> ctx.keep_class_id;

DELETE FROM public."Programs" p
USING seed_context ctx
WHERE p."Id" <> ctx.keep_program_id;

DELETE FROM public."Classrooms" r
USING seed_context ctx
WHERE r."Id" <> ctx.keep_room_id;

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
    ctx.keep_room_id,
    ctx.branch_id,
    'Seed Room',
    20,
    'Preserved room used by reset_academic_seed_postgresql.sql',
    NULL,
    NULL,
    NULL,
    TRUE
FROM seed_context ctx
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
    v.id,
    v.name,
    v.code,
    v.description,
    TRUE,
    FALSE,
    FALSE,
    v.is_supplementary,
    ctx.now_utc,
    ctx.now_utc
FROM seed_context ctx
CROSS JOIN (
    VALUES
        ('5524df75-5a84-e66e-c862-973cbf1c7cc9'::uuid, 'Cambridge Starters', 'STARTERS', 'Chuong trinh Cambridge Starters.', FALSE),
        ('11111111-1111-1111-1111-111111111201'::uuid, 'Apple 2', 'APPLE2', 'Chuong trinh chinh Apple 2.', FALSE),
        ('11111111-1111-1111-1111-111111111202'::uuid, 'Phonics Foundation', 'PHONICS', 'Chuong trinh chinh Phonics Foundation.', FALSE),
        ('11111111-1111-1111-1111-111111111204'::uuid, 'Cambridge Movers', 'MOVERS', 'Chuong trinh Cambridge Movers.', FALSE),
        ('11111111-1111-1111-1111-111111111205'::uuid, 'Cambridge Flyers', 'FLYERS', 'Chuong trinh Cambridge Flyers.', FALSE),
        ('11111111-1111-1111-1111-111111111206'::uuid, 'KET (A2 Key)', 'KETA2', 'Chuong trinh Cambridge KET (A2 Key).', FALSE),
        ('11111111-1111-1111-1111-111111111207'::uuid, 'PET (B1 Preliminary)', 'PETB1', 'Chuong trinh Cambridge PET (B1 Preliminary).', FALSE),
        ('11111111-1111-1111-1111-111111111208'::uuid, 'Kem LMS', 'KEMLMS', 'Chuong trinh phu Kem LMS.', TRUE),
        ('11111111-1111-1111-1111-111111111209'::uuid, 'Speaking Club', 'SPKCLUB', 'Chuong trinh phu Speaking Club cuoi tuan.', TRUE),
        ('11111111-1111-1111-1111-111111111210'::uuid, 'Writing Booster', 'WRBOOST', 'Chuong trinh phu Writing Booster.', TRUE),
        ('11111111-1111-1111-1111-111111111211'::uuid, 'Summer Camp', 'SUMCAMP', 'Chuong trinh phu Summer Camp.', TRUE)
) AS v(id, name, code, description, is_supplementary)
ON CONFLICT ("Id") DO UPDATE
SET
    "Name" = EXCLUDED."Name",
    "Code" = EXCLUDED."Code",
    "Description" = EXCLUDED."Description",
    "IsActive" = TRUE,
    "IsDeleted" = FALSE,
    "IsMakeup" = FALSE,
    "IsSupplementary" = EXCLUDED."IsSupplementary",
    "UpdatedAt" = EXCLUDED."UpdatedAt";

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
    ('90000000-0000-0000-0000-' || right(replace(v.id::text, '-', ''), 12))::uuid,
    ctx.branch_id,
    v.id,
    TRUE,
    NULL,
    ctx.now_utc,
    ctx.now_utc
FROM seed_context ctx
CROSS JOIN (
    VALUES
        ('5524df75-5a84-e66e-c862-973cbf1c7cc9'::uuid),
        ('11111111-1111-1111-1111-111111111201'::uuid),
        ('11111111-1111-1111-1111-111111111202'::uuid),
        ('11111111-1111-1111-1111-111111111204'::uuid),
        ('11111111-1111-1111-1111-111111111205'::uuid),
        ('11111111-1111-1111-1111-111111111206'::uuid),
        ('11111111-1111-1111-1111-111111111207'::uuid),
        ('11111111-1111-1111-1111-111111111208'::uuid),
        ('11111111-1111-1111-1111-111111111209'::uuid),
        ('11111111-1111-1111-1111-111111111210'::uuid),
        ('11111111-1111-1111-1111-111111111211'::uuid)
) AS v(id)
ON CONFLICT ("BranchId", "ProgramId") DO UPDATE
SET
    "IsActive" = TRUE,
    "DefaultMakeupClassId" = NULL,
    "UpdatedAt" = EXCLUDED."UpdatedAt";

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
    v.id,
    ctx.branch_id,
    v.program_id,
    v.plan_name,
    v.total_sessions,
    v.tuition_amount,
    v.unit_price_session,
    'VND',
    TRUE,
    FALSE,
    ctx.now_utc,
    ctx.now_utc
FROM seed_context ctx
CROSS JOIN (
    VALUES
        ('aaaaaaaa-0000-0000-0000-000000000001'::uuid, '11111111-1111-1111-1111-111111111201'::uuid, 'Basic Plan', 24, 4200000::numeric, 175000::numeric),
        ('aaaaaaaa-0000-0000-0000-000000000002'::uuid, '11111111-1111-1111-1111-111111111201'::uuid, 'Standard Plan', 48, 7800000::numeric, 162500::numeric),
        ('aaaaaaaa-0000-0000-0000-000000000003'::uuid, '11111111-1111-1111-1111-111111111201'::uuid, 'Premium Plan', 96, 15600000::numeric, 162500::numeric),

        ('aaaaaaaa-0000-0000-0000-000000000004'::uuid, '11111111-1111-1111-1111-111111111202'::uuid, 'Basic Plan', 24, 4200000::numeric, 175000::numeric),
        ('aaaaaaaa-0000-0000-0000-000000000005'::uuid, '11111111-1111-1111-1111-111111111202'::uuid, 'Standard Plan', 48, 7800000::numeric, 162500::numeric),
        ('aaaaaaaa-0000-0000-0000-000000000006'::uuid, '11111111-1111-1111-1111-111111111202'::uuid, 'Premium Plan', 96, 15600000::numeric, 162500::numeric),

        ('aaaaaaaa-0000-0000-0000-000000000007'::uuid, '5524df75-5a84-e66e-c862-973cbf1c7cc9'::uuid, 'Basic Plan', 24, 4200000::numeric, 175000::numeric),
        ('aaaaaaaa-0000-0000-0000-000000000008'::uuid, '5524df75-5a84-e66e-c862-973cbf1c7cc9'::uuid, 'Standard Plan', 48, 7800000::numeric, 162500::numeric),
        ('aaaaaaaa-0000-0000-0000-000000000009'::uuid, '5524df75-5a84-e66e-c862-973cbf1c7cc9'::uuid, 'Premium Plan', 96, 15600000::numeric, 162500::numeric),

        ('aaaaaaaa-0000-0000-0000-000000000010'::uuid, '11111111-1111-1111-1111-111111111204'::uuid, 'Basic Plan', 24, 4200000::numeric, 175000::numeric),
        ('aaaaaaaa-0000-0000-0000-000000000011'::uuid, '11111111-1111-1111-1111-111111111204'::uuid, 'Standard Plan', 48, 7800000::numeric, 162500::numeric),
        ('aaaaaaaa-0000-0000-0000-000000000012'::uuid, '11111111-1111-1111-1111-111111111204'::uuid, 'Premium Plan', 96, 15600000::numeric, 162500::numeric),

        ('aaaaaaaa-0000-0000-0000-000000000013'::uuid, '11111111-1111-1111-1111-111111111205'::uuid, 'Basic Plan', 24, 4200000::numeric, 175000::numeric),
        ('aaaaaaaa-0000-0000-0000-000000000014'::uuid, '11111111-1111-1111-1111-111111111205'::uuid, 'Standard Plan', 48, 7800000::numeric, 162500::numeric),
        ('aaaaaaaa-0000-0000-0000-000000000015'::uuid, '11111111-1111-1111-1111-111111111205'::uuid, 'Premium Plan', 96, 15600000::numeric, 162500::numeric),

        ('aaaaaaaa-0000-0000-0000-000000000016'::uuid, '11111111-1111-1111-1111-111111111206'::uuid, 'Basic Plan', 24, 4200000::numeric, 175000::numeric),
        ('aaaaaaaa-0000-0000-0000-000000000017'::uuid, '11111111-1111-1111-1111-111111111206'::uuid, 'Standard Plan', 48, 7800000::numeric, 162500::numeric),
        ('aaaaaaaa-0000-0000-0000-000000000018'::uuid, '11111111-1111-1111-1111-111111111206'::uuid, 'Premium Plan', 96, 15600000::numeric, 162500::numeric),

        ('aaaaaaaa-0000-0000-0000-000000000019'::uuid, '11111111-1111-1111-1111-111111111207'::uuid, 'Basic Plan', 24, 4200000::numeric, 175000::numeric),
        ('aaaaaaaa-0000-0000-0000-000000000020'::uuid, '11111111-1111-1111-1111-111111111207'::uuid, 'Standard Plan', 48, 7800000::numeric, 162500::numeric),
        ('aaaaaaaa-0000-0000-0000-000000000021'::uuid, '11111111-1111-1111-1111-111111111207'::uuid, 'Premium Plan', 96, 15600000::numeric, 162500::numeric),

        ('aaaaaaaa-0000-0000-0000-000000000022'::uuid, '11111111-1111-1111-1111-111111111209'::uuid, 'Weekend Pack', 12, 600000::numeric, 50000::numeric),

        ('aaaaaaaa-0000-0000-0000-000000000023'::uuid, '11111111-1111-1111-1111-111111111208'::uuid, 'LMS 12 Sessions', 12, 1200000::numeric, 100000::numeric),
        ('aaaaaaaa-0000-0000-0000-000000000024'::uuid, '11111111-1111-1111-1111-111111111208'::uuid, 'LMS 24 Sessions', 24, 2160000::numeric, 90000::numeric),
        ('aaaaaaaa-0000-0000-0000-000000000025'::uuid, '11111111-1111-1111-1111-111111111208'::uuid, 'LMS 36 Sessions', 36, 2880000::numeric, 80000::numeric),
        ('aaaaaaaa-0000-0000-0000-000000000026'::uuid, '11111111-1111-1111-1111-111111111208'::uuid, 'LMS 48 Sessions', 48, 3360000::numeric, 70000::numeric)
) AS v(id, program_id, plan_name, total_sessions, tuition_amount, unit_price_session);

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
    v.id,
    ctx.branch_id,
    v.program_id,
    v.code,
    v.title,
    ctx.keep_room_id,
    NULL,
    NULL,
    ctx.start_date,
    ctx.end_date,
    'Recruiting',
    v.capacity,
    v.schedule_pattern,
    v.description,
    ctx.now_utc,
    ctx.now_utc
FROM seed_context ctx
CROSS JOIN (
    VALUES
        ('22222222-2222-2222-2222-222222222301'::uuid, '11111111-1111-1111-1111-111111111201'::uuid, 'APPLE-A2', 'Apple A2', 8, 'RRULE:FREQ=WEEKLY;BYDAY=TH,SA;BYHOUR=18;BYMINUTE=0;DURATION=60', '4-6 tuoi. Lich goc: Thu 18:00-19:00, Sat 17:00-18:00. Seed force ca 2 buoi thanh 18:00-19:00.'),
        ('22222222-2222-2222-2222-222222222302'::uuid, '11111111-1111-1111-1111-111111111202'::uuid, 'PHONICS-P1', 'Phonics P1', 6, 'RRULE:FREQ=WEEKLY;BYDAY=MO,WE;BYHOUR=19;BYMINUTE=0;DURATION=90', '5-6 tuoi. Mon 19:00-20:30, Wed 19:00-20:30.'),
        ('22222222-2222-2222-2222-222222222303'::uuid, '11111111-1111-1111-1111-111111111202'::uuid, 'PHONICS-P3', 'Phonics P3 + Thuyet trinh', 4, 'RRULE:FREQ=WEEKLY;BYDAY=TU,FR;BYHOUR=16;BYMINUTE=30;DURATION=90', 'Lich goc: Tue 16:30-18:00, Fri 17:30-19:00. Seed force ca 2 buoi thanh 16:30-18:00.'),
        ('cc509804-5b7d-8005-5ebb-0a21a8300253'::uuid, '5524df75-5a84-e66e-c862-973cbf1c7cc9'::uuid, 'STARTERS-S1', 'Starters S1', 10, 'RRULE:FREQ=WEEKLY;BYDAY=TH,FR;BYHOUR=16;BYMINUTE=0;DURATION=90', '6-8 tuoi. Lich goc: Thu 16:00-17:30, Fri 19:30-21:00. Seed force ca 2 buoi thanh 16:00-17:30.'),
        ('22222222-2222-2222-2222-222222222305'::uuid, '5524df75-5a84-e66e-c862-973cbf1c7cc9'::uuid, 'STARTERS-S5', 'Starters S5', 10, 'RRULE:FREQ=WEEKLY;BYDAY=WE,FR;BYHOUR=19;BYMINUTE=30;DURATION=90', 'Lich goc: Wed 19:30-21:00, Fri 18:00-19:30. Seed force ca 2 buoi thanh 19:30-21:00.'),
        ('22222222-2222-2222-2222-222222222306'::uuid, '11111111-1111-1111-1111-111111111204'::uuid, 'MOVERS-M4', 'Movers M4', 7, 'RRULE:FREQ=WEEKLY;BYDAY=TU,TH;BYHOUR=19;BYMINUTE=30;DURATION=90', 'Tue 19:30-21:00, Thu 19:30-21:00.'),
        ('22222222-2222-2222-2222-222222222307'::uuid, '11111111-1111-1111-1111-111111111204'::uuid, 'MOVERS-M2', 'Movers M2', 8, 'RRULE:FREQ=WEEKLY;BYDAY=MO,WE;BYHOUR=17;BYMINUTE=30;DURATION=90', 'Mon 17:30-19:00, Wed 17:30-19:00.'),
        ('22222222-2222-2222-2222-222222222308'::uuid, '11111111-1111-1111-1111-111111111204'::uuid, 'MOVERS-M3', 'Movers M3', 4, 'RRULE:FREQ=WEEKLY;BYDAY=WE,SA;BYHOUR=16;BYMINUTE=0;DURATION=90', 'Lich goc: Wed 16:00-17:30, Sat 15:30-17:00. Seed force ca 2 buoi thanh 16:00-17:30.'),
        ('22222222-2222-2222-2222-222222222309'::uuid, '11111111-1111-1111-1111-111111111205'::uuid, 'FLYERS-F1', 'Flyers F1', 10, 'RRULE:FREQ=WEEKLY;BYDAY=TU,TH;BYHOUR=18;BYMINUTE=0;DURATION=90', 'Lich goc: Tue 18:00-19:30, Thu 18:30-20:00. Seed force ca 2 buoi thanh 18:00-19:30.'),
        ('22222222-2222-2222-2222-222222222310'::uuid, '11111111-1111-1111-1111-111111111205'::uuid, 'FLYERS-F2', 'Flyers F2', 5, 'RRULE:FREQ=WEEKLY;BYDAY=MO,SA;BYHOUR=16;BYMINUTE=0;DURATION=90', 'Lich goc: Mon 16:00-17:30, Sat 17:00-18:30. Seed force ca 2 buoi thanh 16:00-17:30.'),
        ('22222222-2222-2222-2222-222222222311'::uuid, '11111111-1111-1111-1111-111111111207'::uuid, 'PET-K1', 'PET K1', 5, 'RRULE:FREQ=WEEKLY;BYDAY=FR,SA;BYHOUR=19;BYMINUTE=30;DURATION=90', 'Lich goc: Fri 19:30-21:00, Sat 10:00-11:30. Seed force ca 2 buoi thanh 19:30-21:00.'),
        ('22222222-2222-2222-2222-222222222312'::uuid, '11111111-1111-1111-1111-111111111208'::uuid, 'LMS-ESL-WED', 'Kem LMS - ESL Grade 3 - Cam (Wed)', 10, 'RRULE:FREQ=WEEKLY;BYDAY=WE;BYHOUR=16;BYMINUTE=0;DURATION=90', 'Wed 16:00-17:30.'),
        ('22222222-2222-2222-2222-222222222313'::uuid, '11111111-1111-1111-1111-111111111208'::uuid, 'LMS-ESL-THU', 'Kem LMS - ESL Grade 3 - Cam (Thu)', 10, 'RRULE:FREQ=WEEKLY;BYDAY=TH;BYHOUR=19;BYMINUTE=30;DURATION=90', 'Thu 19:30-21:00.'),
        ('22222222-2222-2222-2222-222222222314'::uuid, '11111111-1111-1111-1111-111111111208'::uuid, 'LMS-SCI-WED', 'Kem LMS - Science Grade 3 - Cam', 10, 'RRULE:FREQ=WEEKLY;BYDAY=WE;BYHOUR=17;BYMINUTE=30;DURATION=90', 'Wed 17:30-19:00.')
) AS v(id, program_id, code, title, capacity, schedule_pattern, description)
ON CONFLICT ("Id") DO UPDATE
SET
    "BranchId" = EXCLUDED."BranchId",
    "ProgramId" = EXCLUDED."ProgramId",
    "Code" = EXCLUDED."Code",
    "Title" = EXCLUDED."Title",
    "RoomId" = EXCLUDED."RoomId",
    "MainTeacherId" = NULL,
    "AssistantTeacherId" = NULL,
    "StartDate" = EXCLUDED."StartDate",
    "EndDate" = EXCLUDED."EndDate",
    "Status" = EXCLUDED."Status",
    "Capacity" = EXCLUDED."Capacity",
    "SchedulePattern" = EXCLUDED."SchedulePattern",
    "Description" = EXCLUDED."Description",
    "UpdatedAt" = EXCLUDED."UpdatedAt";

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
    v.id,
    v.class_id,
    ctx.start_date,
    NULL,
    v.schedule_pattern,
    ctx.now_utc,
    ctx.now_utc
FROM seed_context ctx
CROSS JOIN (
    VALUES
        ('33333333-3333-3333-3333-333333333401'::uuid, '22222222-2222-2222-2222-222222222301'::uuid, 'RRULE:FREQ=WEEKLY;BYDAY=TH,SA;BYHOUR=18;BYMINUTE=0;DURATION=60'),
        ('33333333-3333-3333-3333-333333333402'::uuid, '22222222-2222-2222-2222-222222222302'::uuid, 'RRULE:FREQ=WEEKLY;BYDAY=MO,WE;BYHOUR=19;BYMINUTE=0;DURATION=90'),
        ('33333333-3333-3333-3333-333333333403'::uuid, '22222222-2222-2222-2222-222222222303'::uuid, 'RRULE:FREQ=WEEKLY;BYDAY=TU,FR;BYHOUR=16;BYMINUTE=30;DURATION=90'),
        ('33333333-3333-3333-3333-333333333404'::uuid, 'cc509804-5b7d-8005-5ebb-0a21a8300253'::uuid, 'RRULE:FREQ=WEEKLY;BYDAY=TH,FR;BYHOUR=16;BYMINUTE=0;DURATION=90'),
        ('33333333-3333-3333-3333-333333333405'::uuid, '22222222-2222-2222-2222-222222222305'::uuid, 'RRULE:FREQ=WEEKLY;BYDAY=WE,FR;BYHOUR=19;BYMINUTE=30;DURATION=90'),
        ('33333333-3333-3333-3333-333333333406'::uuid, '22222222-2222-2222-2222-222222222306'::uuid, 'RRULE:FREQ=WEEKLY;BYDAY=TU,TH;BYHOUR=19;BYMINUTE=30;DURATION=90'),
        ('33333333-3333-3333-3333-333333333407'::uuid, '22222222-2222-2222-2222-222222222307'::uuid, 'RRULE:FREQ=WEEKLY;BYDAY=MO,WE;BYHOUR=17;BYMINUTE=30;DURATION=90'),
        ('33333333-3333-3333-3333-333333333408'::uuid, '22222222-2222-2222-2222-222222222308'::uuid, 'RRULE:FREQ=WEEKLY;BYDAY=WE,SA;BYHOUR=16;BYMINUTE=0;DURATION=90'),
        ('33333333-3333-3333-3333-333333333409'::uuid, '22222222-2222-2222-2222-222222222309'::uuid, 'RRULE:FREQ=WEEKLY;BYDAY=TU,TH;BYHOUR=18;BYMINUTE=0;DURATION=90'),
        ('33333333-3333-3333-3333-333333333410'::uuid, '22222222-2222-2222-2222-222222222310'::uuid, 'RRULE:FREQ=WEEKLY;BYDAY=MO,SA;BYHOUR=16;BYMINUTE=0;DURATION=90'),
        ('33333333-3333-3333-3333-333333333411'::uuid, '22222222-2222-2222-2222-222222222311'::uuid, 'RRULE:FREQ=WEEKLY;BYDAY=FR,SA;BYHOUR=19;BYMINUTE=30;DURATION=90'),
        ('33333333-3333-3333-3333-333333333412'::uuid, '22222222-2222-2222-2222-222222222312'::uuid, 'RRULE:FREQ=WEEKLY;BYDAY=WE;BYHOUR=16;BYMINUTE=0;DURATION=90'),
        ('33333333-3333-3333-3333-333333333413'::uuid, '22222222-2222-2222-2222-222222222313'::uuid, 'RRULE:FREQ=WEEKLY;BYDAY=TH;BYHOUR=19;BYMINUTE=30;DURATION=90'),
        ('33333333-3333-3333-3333-333333333414'::uuid, '22222222-2222-2222-2222-222222222314'::uuid, 'RRULE:FREQ=WEEKLY;BYDAY=WE;BYHOUR=17;BYMINUTE=30;DURATION=90')
) AS v(id, class_id, schedule_pattern);

SELECT 'Remaining users by role' AS check_name, "Role", COUNT(*) AS total
FROM public."Users"
GROUP BY "Role"
ORDER BY "Role";

SELECT 'Program count' AS check_name, COUNT(*) AS total
FROM public."Programs";

SELECT 'Tuition plan count' AS check_name, COUNT(*) AS total
FROM public."TuitionPlans";

SELECT 'Class count' AS check_name, COUNT(*) AS total
FROM public."Classes";
