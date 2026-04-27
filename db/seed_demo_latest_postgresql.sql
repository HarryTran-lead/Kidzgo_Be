-- Demo seed for the current Kidzgo BE schema.
-- Safe characteristics:
-- 1. Idempotent: uses fixed demo IDs and upserts.
-- 2. Non-destructive: does not truncate existing business data.
-- 3. Self-contained: creates a dedicated demo branch, programs, classes, users,
--    profiles, registrations, sessions, attendances, and notifications.
--
-- Suggested usage:
-- 1. Apply the latest EF Core migrations first.
-- 2. Run this script on the target PostgreSQL database in schema `public`.
--
-- Demo credentials:
-- - Password for all demo users: Password123!
-- - PIN for admin/staff/teacher users: 1234
-- - PIN for the parent profile: 1234
--
-- Demo accounts:
-- - admin.demo@kidzgo.local
-- - management.demo@kidzgo.local
-- - accountant.demo@kidzgo.local
-- - teacher.main.demo@kidzgo.local
-- - teacher.assistant.demo@kidzgo.local
-- - parent.demo@kidzgo.local
-- - student.an.demo@kidzgo.local
-- - student.binh.demo@kidzgo.local

BEGIN;

SELECT pg_advisory_xact_lock(62425002);

CREATE TEMP TABLE demo_context AS
SELECT
    timezone('UTC', now()) AS now_utc,
    CURRENT_DATE::date AS today,
    (CURRENT_DATE - INTERVAL '21 days')::date AS class_start_date;

-- Fixed hashes produced with the same PBKDF2-SHA512 scheme used by the BE.
-- Password123! + salt 0123456789ABCDEF0123456789ABCDEF
-- 1234 + salt ABCDEF0123456789ABCDEF0123456789
CREATE TEMP TABLE demo_hashes AS
SELECT
    'DE479F92E6B1E906ECE5CBB756062EDC6F680786DF32A1BE3551E1499DEBABD9-0123456789ABCDEF0123456789ABCDEF'::text AS password_hash,
    'F6DDB6A2352010187D0957FC9B749A01BD9131D7D13BA1962D596FEDBDB993C1-ABCDEF0123456789ABCDEF0123456789'::text AS pin_hash;

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
    '90000000-0000-0000-0000-000000000001'::uuid,
    'DEMO-HCM',
    'Kidzgo Demo Branch HCM',
    '123 Demo Street, District 1, Ho Chi Minh City',
    '0900000001',
    'branch.demo@kidzgo.local',
    TRUE,
    c.now_utc,
    c.now_utc
FROM demo_context c
ON CONFLICT ("Id") DO UPDATE
SET
    "Code" = EXCLUDED."Code",
    "Name" = EXCLUDED."Name",
    "Address" = EXCLUDED."Address",
    "ContactPhone" = EXCLUDED."ContactPhone",
    "ContactEmail" = EXCLUDED."ContactEmail",
    "IsActive" = EXCLUDED."IsActive",
    "UpdatedAt" = EXCLUDED."UpdatedAt";

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
    v.program_id,
    v.name,
    v.code,
    v.description,
    TRUE,
    FALSE,
    FALSE,
    FALSE,
    c.now_utc,
    c.now_utc
FROM demo_context c
CROSS JOIN
(
    VALUES
        (
            '90000000-0000-0000-0000-000000000101'::uuid,
            'DSTAR',
            'Demo Starters',
            'Primary demo program for parent-student flow, registration, sessions, and notifications.'
        ),
        (
            '90000000-0000-0000-0000-000000000102'::uuid,
            'DMOVE',
            'Demo Movers',
            'Secondary demo program used to test a low-remaining-session registration.'
        )
) AS v(program_id, code, name, description)
ON CONFLICT ("Id") DO UPDATE
SET
    "Name" = EXCLUDED."Name",
    "Code" = EXCLUDED."Code",
    "Description" = EXCLUDED."Description",
    "IsActive" = EXCLUDED."IsActive",
    "IsDeleted" = EXCLUDED."IsDeleted",
    "IsMakeup" = EXCLUDED."IsMakeup",
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
    v.id,
    '90000000-0000-0000-0000-000000000001'::uuid,
    v.program_id,
    TRUE,
    NULL,
    c.now_utc,
    c.now_utc
FROM demo_context c
CROSS JOIN
(
    VALUES
        ('90000000-0000-0000-0000-000000000201'::uuid, '90000000-0000-0000-0000-000000000101'::uuid),
        ('90000000-0000-0000-0000-000000000202'::uuid, '90000000-0000-0000-0000-000000000102'::uuid)
) AS v(id, program_id)
ON CONFLICT ("BranchId", "ProgramId") DO UPDATE
SET
    "IsActive" = EXCLUDED."IsActive",
    "DefaultMakeupClassId" = EXCLUDED."DefaultMakeupClassId",
    "UpdatedAt" = EXCLUDED."UpdatedAt";

INSERT INTO public."ProgramLeavePolicies"
(
    "Id",
    "ProgramId",
    "MaxLeavesPerMonth",
    "UpdatedBy",
    "CreatedAt",
    "UpdatedAt"
)
SELECT
    v.id,
    v.program_id,
    v.max_leaves_per_month,
    NULL,
    c.now_utc,
    c.now_utc
FROM demo_context c
CROSS JOIN
(
    VALUES
        ('90000000-0000-0000-0000-000000000301'::uuid, '90000000-0000-0000-0000-000000000101'::uuid, 2),
        ('90000000-0000-0000-0000-000000000302'::uuid, '90000000-0000-0000-0000-000000000102'::uuid, 2)
) AS v(id, program_id, max_leaves_per_month)
ON CONFLICT ("ProgramId") DO UPDATE
SET
    "MaxLeavesPerMonth" = EXCLUDED."MaxLeavesPerMonth",
    "UpdatedBy" = EXCLUDED."UpdatedBy",
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
    '90000000-0000-0000-0000-000000000001'::uuid,
    v.program_id,
    v.name,
    v.total_sessions,
    v.tuition_amount,
    v.unit_price_session,
    'VND',
    TRUE,
    FALSE,
    c.now_utc,
    c.now_utc
FROM demo_context c
CROSS JOIN
(
    VALUES
        (
            '90000000-0000-0000-0000-000000000401'::uuid,
            '90000000-0000-0000-0000-000000000101'::uuid,
            'Demo Starters 6 Sessions',
            6,
            1500000::numeric,
            250000::numeric
        ),
        (
            '90000000-0000-0000-0000-000000000402'::uuid,
            '90000000-0000-0000-0000-000000000102'::uuid,
            'Demo Movers 4 Sessions',
            4,
            1200000::numeric,
            300000::numeric
        )
) AS v(id, program_id, name, total_sessions, tuition_amount, unit_price_session)
ON CONFLICT ("Id") DO UPDATE
SET
    "BranchId" = EXCLUDED."BranchId",
    "ProgramId" = EXCLUDED."ProgramId",
    "Name" = EXCLUDED."Name",
    "TotalSessions" = EXCLUDED."TotalSessions",
    "TuitionAmount" = EXCLUDED."TuitionAmount",
    "UnitPriceSession" = EXCLUDED."UnitPriceSession",
    "Currency" = EXCLUDED."Currency",
    "IsActive" = EXCLUDED."IsActive",
    "IsDeleted" = EXCLUDED."IsDeleted",
    "UpdatedAt" = EXCLUDED."UpdatedAt";

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
VALUES
(
    '90000000-0000-0000-0000-000000000501'::uuid,
    '90000000-0000-0000-0000-000000000001'::uuid,
    'Demo Room 01',
    12,
    'Main demo classroom for Demo Starters.',
    '2',
    32,
    '["TV","Whiteboard","Projector"]',
    TRUE
),
(
    '90000000-0000-0000-0000-000000000502'::uuid,
    '90000000-0000-0000-0000-000000000001'::uuid,
    'Demo Room 02',
    12,
    'Main demo classroom for Demo Movers.',
    '2',
    28,
    '["TV","Whiteboard"]',
    TRUE
)
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

INSERT INTO public."Users"
(
    "Id",
    "Email",
    "PasswordHash",
    "Role",
    "Username",
    "Name",
    "PhoneNumber",
    "PinHash",
    "AvatarUrl",
    "AvatarMimeType",
    "AvatarFileSize",
    "TeacherCompensationType",
    "BranchId",
    "IsActive",
    "IsDeleted",
    "LastLoginAt",
    "LastSeenAt",
    "CreatedAt",
    "UpdatedAt"
)
SELECT
    v.id,
    v.email,
    h.password_hash,
    v.role,
    v.username,
    v.name,
    v.phone_number,
    CASE WHEN v.requires_pin THEN h.pin_hash ELSE NULL END,
    NULL,
    NULL,
    NULL,
    NULL,
    v.branch_id,
    TRUE,
    FALSE,
    NULL,
    NULL,
    c.now_utc,
    c.now_utc
FROM demo_context c
CROSS JOIN demo_hashes h
CROSS JOIN
(
    VALUES
        (
            '90000000-0000-0000-0000-000000000601'::uuid,
            'admin.demo@kidzgo.local',
            'Admin',
            'admin.demo',
            'Demo Admin',
            '0901000001',
            FALSE,
            NULL::uuid
        ),
        (
            '90000000-0000-0000-0000-000000000602'::uuid,
            'management.demo@kidzgo.local',
            'ManagementStaff',
            'management.demo',
            'Demo Management Staff',
            '0901000002',
            TRUE,
            '90000000-0000-0000-0000-000000000001'::uuid
        ),
        (
            '90000000-0000-0000-0000-000000000603'::uuid,
            'accountant.demo@kidzgo.local',
            'AccountantStaff',
            'accountant.demo',
            'Demo Accountant Staff',
            '0901000003',
            TRUE,
            '90000000-0000-0000-0000-000000000001'::uuid
        ),
        (
            '90000000-0000-0000-0000-000000000604'::uuid,
            'teacher.main.demo@kidzgo.local',
            'Teacher',
            'teacher.main.demo',
            'Demo Main Teacher',
            '0901000004',
            TRUE,
            '90000000-0000-0000-0000-000000000001'::uuid
        ),
        (
            '90000000-0000-0000-0000-000000000605'::uuid,
            'teacher.assistant.demo@kidzgo.local',
            'Teacher',
            'teacher.assistant.demo',
            'Demo Assistant Teacher',
            '0901000005',
            TRUE,
            '90000000-0000-0000-0000-000000000001'::uuid
        ),
        (
            '90000000-0000-0000-0000-000000000606'::uuid,
            'parent.demo@kidzgo.local',
            'Parent',
            'parent.demo',
            'Demo Parent',
            '0901000006',
            FALSE,
            NULL::uuid
        ),
        (
            '90000000-0000-0000-0000-000000000607'::uuid,
            'student.an.demo@kidzgo.local',
            'Student',
            'student.an.demo',
            'Nguyen Minh An',
            '0901000007',
            FALSE,
            NULL::uuid
        ),
        (
            '90000000-0000-0000-0000-000000000608'::uuid,
            'student.binh.demo@kidzgo.local',
            'Student',
            'student.binh.demo',
            'Tran Gia Binh',
            '0901000008',
            FALSE,
            NULL::uuid
        )
) AS v(id, email, role, username, name, phone_number, requires_pin, branch_id)
ON CONFLICT ("Email") DO UPDATE
SET
    "PasswordHash" = EXCLUDED."PasswordHash",
    "Role" = EXCLUDED."Role",
    "Username" = EXCLUDED."Username",
    "Name" = EXCLUDED."Name",
    "PhoneNumber" = EXCLUDED."PhoneNumber",
    "PinHash" = EXCLUDED."PinHash",
    "TeacherCompensationType" = EXCLUDED."TeacherCompensationType",
    "BranchId" = EXCLUDED."BranchId",
    "IsActive" = EXCLUDED."IsActive",
    "IsDeleted" = EXCLUDED."IsDeleted",
    "UpdatedAt" = EXCLUDED."UpdatedAt";

INSERT INTO public."Profiles"
(
    "Id",
    "UserId",
    "ProfileType",
    "DisplayName",
    "Name",
    "Gender",
    "DateOfBirth",
    "ZaloId",
    "PinHash",
    "AvatarUrl",
    "AvatarMimeType",
    "AvatarFileSize",
    "IsApproved",
    "IsActive",
    "IsDeleted",
    "LastLoginAt",
    "LastSeenAt",
    "CreatedAt",
    "UpdatedAt"
)
SELECT
    v.id,
    v.user_id,
    v.profile_type,
    v.display_name,
    v.name,
    v.gender,
    v.date_of_birth,
    NULL,
    CASE WHEN v.profile_type = 'Parent' THEN h.pin_hash ELSE NULL END,
    NULL,
    NULL,
    NULL,
    TRUE,
    TRUE,
    FALSE,
    NULL,
    NULL,
    c.now_utc,
    c.now_utc
FROM demo_context c
CROSS JOIN demo_hashes h
CROSS JOIN
(
    VALUES
        (
            '90000000-0000-0000-0000-000000000701'::uuid,
            '90000000-0000-0000-0000-000000000606'::uuid,
            'Parent',
            'Demo Parent',
            'Le Thi Demo',
            'Female',
            '1990-06-15'::date
        ),
        (
            '90000000-0000-0000-0000-000000000702'::uuid,
            '90000000-0000-0000-0000-000000000607'::uuid,
            'Student',
            'Nguyen Minh An',
            'Nguyen Minh An',
            'Male',
            '2016-03-12'::date
        ),
        (
            '90000000-0000-0000-0000-000000000703'::uuid,
            '90000000-0000-0000-0000-000000000608'::uuid,
            'Student',
            'Tran Gia Binh',
            'Tran Gia Binh',
            'Male',
            '2015-08-23'::date
        )
) AS v(id, user_id, profile_type, display_name, name, gender, date_of_birth)
ON CONFLICT ("Id") DO UPDATE
SET
    "UserId" = EXCLUDED."UserId",
    "ProfileType" = EXCLUDED."ProfileType",
    "DisplayName" = EXCLUDED."DisplayName",
    "Name" = EXCLUDED."Name",
    "Gender" = EXCLUDED."Gender",
    "DateOfBirth" = EXCLUDED."DateOfBirth",
    "ZaloId" = EXCLUDED."ZaloId",
    "PinHash" = EXCLUDED."PinHash",
    "IsApproved" = EXCLUDED."IsApproved",
    "IsActive" = EXCLUDED."IsActive",
    "IsDeleted" = EXCLUDED."IsDeleted",
    "UpdatedAt" = EXCLUDED."UpdatedAt";

INSERT INTO public."ParentStudentLinks"
(
    "Id",
    "ParentProfileId",
    "StudentProfileId",
    "CreatedAt"
)
SELECT
    v.id,
    '90000000-0000-0000-0000-000000000701'::uuid,
    v.student_profile_id,
    c.now_utc
FROM demo_context c
CROSS JOIN
(
    VALUES
        ('90000000-0000-0000-0000-000000000801'::uuid, '90000000-0000-0000-0000-000000000702'::uuid),
        ('90000000-0000-0000-0000-000000000802'::uuid, '90000000-0000-0000-0000-000000000703'::uuid)
) AS v(id, student_profile_id)
ON CONFLICT ("Id") DO UPDATE
SET
    "ParentProfileId" = EXCLUDED."ParentProfileId",
    "StudentProfileId" = EXCLUDED."StudentProfileId";

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
    '90000000-0000-0000-0000-000000000001'::uuid,
    v.program_id,
    v.code,
    v.title,
    v.room_id,
    '90000000-0000-0000-0000-000000000604'::uuid,
    '90000000-0000-0000-0000-000000000605'::uuid,
    c.class_start_date,
    NULL,
    'Active',
    12,
    v.schedule_pattern,
    v.description,
    c.now_utc,
    c.now_utc
FROM demo_context c
CROSS JOIN
(
    VALUES
        (
            '90000000-0000-0000-0000-000000000901'::uuid,
            '90000000-0000-0000-0000-000000000101'::uuid,
            '90000000-0000-0000-0000-000000000501'::uuid,
            'DEMO-STA-A',
            'Demo Starters A',
            '{"type":"weekly-pattern","entries":[{"dayOfWeeks":["TU"],"startTime":"18:00","durationMinutes":90}]}',
            'Demo class for a student with 3 remaining sessions.'
        ),
        (
            '90000000-0000-0000-0000-000000000902'::uuid,
            '90000000-0000-0000-0000-000000000102'::uuid,
            '90000000-0000-0000-0000-000000000502'::uuid,
            'DEMO-MOV-A',
            'Demo Movers A',
            '{"type":"weekly-pattern","entries":[{"dayOfWeeks":["TH"],"startTime":"19:00","durationMinutes":90}]}',
            'Demo class for a student with 1 remaining session.'
        )
) AS v(id, program_id, room_id, code, title, schedule_pattern, description)
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
    c.class_start_date,
    NULL,
    v.schedule_pattern,
    c.now_utc,
    c.now_utc
FROM demo_context c
CROSS JOIN
(
    VALUES
        (
            '90000000-0000-0000-0000-000000001001'::uuid,
            '90000000-0000-0000-0000-000000000901'::uuid,
            '{"type":"weekly-pattern","entries":[{"dayOfWeeks":["TU"],"startTime":"18:00","durationMinutes":90}]}'
        ),
        (
            '90000000-0000-0000-0000-000000001002'::uuid,
            '90000000-0000-0000-0000-000000000902'::uuid,
            '{"type":"weekly-pattern","entries":[{"dayOfWeeks":["TH"],"startTime":"19:00","durationMinutes":90}]}'
        )
) AS v(id, class_id, schedule_pattern)
ON CONFLICT ("ClassId", "EffectiveFrom") DO UPDATE
SET
    "EffectiveTo" = EXCLUDED."EffectiveTo",
    "SchedulePattern" = EXCLUDED."SchedulePattern",
    "UpdatedAt" = EXCLUDED."UpdatedAt";

INSERT INTO public."Registrations"
(
    "Id",
    "StudentProfileId",
    "BranchId",
    "ProgramId",
    "TuitionPlanId",
    "SecondaryProgramId",
    "RegistrationDate",
    "ExpectedStartDate",
    "ActualStartDate",
    "PreferredSchedule",
    "Note",
    "Status",
    "ClassId",
    "ClassAssignedDate",
    "EntryType",
    "SecondaryClassId",
    "SecondaryClassAssignedDate",
    "SecondaryEntryType",
    "SecondaryProgramSkillFocus",
    "OriginalRegistrationId",
    "OperationType",
    "TotalSessions",
    "UsedSessions",
    "RemainingSessions",
    "ExpiryDate",
    "CreatedAt",
    "UpdatedAt"
)
SELECT
    v.id,
    v.student_profile_id,
    '90000000-0000-0000-0000-000000000001'::uuid,
    v.program_id,
    v.tuition_plan_id,
    NULL,
    ((c.today - INTERVAL '28 days')::date)::timestamp,
    (c.today - INTERVAL '21 days')::date::timestamp,
    (c.today - INTERVAL '21 days')::date::timestamp,
    v.preferred_schedule,
    v.note,
    'Studying',
    v.class_id,
    ((c.today - INTERVAL '22 days')::date)::timestamp,
    NULL,
    NULL,
    NULL,
    NULL,
    NULL,
    NULL,
    NULL,
    v.total_sessions,
    v.used_sessions,
    v.remaining_sessions,
    ((c.today + INTERVAL '45 days')::date)::timestamp,
    c.now_utc,
    c.now_utc
FROM demo_context c
CROSS JOIN
(
    VALUES
        (
            '90000000-0000-0000-0000-000000001101'::uuid,
            '90000000-0000-0000-0000-000000000702'::uuid,
            '90000000-0000-0000-0000-000000000101'::uuid,
            '90000000-0000-0000-0000-000000000401'::uuid,
            '90000000-0000-0000-0000-000000000901'::uuid,
            6,
            3,
            3,
            'Tue 18:00',
            'Demo registration at low-session threshold 3.'
        ),
        (
            '90000000-0000-0000-0000-000000001102'::uuid,
            '90000000-0000-0000-0000-000000000703'::uuid,
            '90000000-0000-0000-0000-000000000102'::uuid,
            '90000000-0000-0000-0000-000000000402'::uuid,
            '90000000-0000-0000-0000-000000000902'::uuid,
            4,
            3,
            1,
            'Thu 19:00',
            'Demo registration at low-session threshold 1.'
        )
) AS v(id, student_profile_id, program_id, tuition_plan_id, class_id, total_sessions, used_sessions, remaining_sessions, preferred_schedule, note)
ON CONFLICT ("Id") DO UPDATE
SET
    "StudentProfileId" = EXCLUDED."StudentProfileId",
    "BranchId" = EXCLUDED."BranchId",
    "ProgramId" = EXCLUDED."ProgramId",
    "TuitionPlanId" = EXCLUDED."TuitionPlanId",
    "SecondaryProgramId" = EXCLUDED."SecondaryProgramId",
    "RegistrationDate" = EXCLUDED."RegistrationDate",
    "ExpectedStartDate" = EXCLUDED."ExpectedStartDate",
    "ActualStartDate" = EXCLUDED."ActualStartDate",
    "PreferredSchedule" = EXCLUDED."PreferredSchedule",
    "Note" = EXCLUDED."Note",
    "Status" = EXCLUDED."Status",
    "ClassId" = EXCLUDED."ClassId",
    "ClassAssignedDate" = EXCLUDED."ClassAssignedDate",
    "EntryType" = EXCLUDED."EntryType",
    "SecondaryClassId" = EXCLUDED."SecondaryClassId",
    "SecondaryClassAssignedDate" = EXCLUDED."SecondaryClassAssignedDate",
    "SecondaryEntryType" = EXCLUDED."SecondaryEntryType",
    "SecondaryProgramSkillFocus" = EXCLUDED."SecondaryProgramSkillFocus",
    "OriginalRegistrationId" = EXCLUDED."OriginalRegistrationId",
    "OperationType" = EXCLUDED."OperationType",
    "TotalSessions" = EXCLUDED."TotalSessions",
    "UsedSessions" = EXCLUDED."UsedSessions",
    "RemainingSessions" = EXCLUDED."RemainingSessions",
    "ExpiryDate" = EXCLUDED."ExpiryDate",
    "UpdatedAt" = EXCLUDED."UpdatedAt";

INSERT INTO public."ClassEnrollments"
(
    "Id",
    "ClassId",
    "StudentProfileId",
    "EnrollDate",
    "Status",
    "TuitionPlanId",
    "RegistrationId",
    "Track",
    "SessionSelectionPattern",
    "EnrollmentConfirmationPdfUrl",
    "EnrollmentConfirmationPdfGeneratedAt",
    "EnrollmentConfirmationPdfGeneratedBy",
    "CreatedAt",
    "UpdatedAt"
)
SELECT
    v.id,
    v.class_id,
    v.student_profile_id,
    c.class_start_date,
    'Active',
    v.tuition_plan_id,
    v.registration_id,
    'Primary',
    v.session_selection_pattern,
    NULL,
    NULL,
    NULL,
    c.now_utc,
    c.now_utc
FROM demo_context c
CROSS JOIN
(
    VALUES
        (
            '90000000-0000-0000-0000-000000001201'::uuid,
            '90000000-0000-0000-0000-000000000901'::uuid,
            '90000000-0000-0000-0000-000000000702'::uuid,
            '90000000-0000-0000-0000-000000000401'::uuid,
            '90000000-0000-0000-0000-000000001101'::uuid,
            '{"type":"weekly-pattern","entries":[{"dayOfWeeks":["TU"],"startTime":"18:00","durationMinutes":90}]}'
        ),
        (
            '90000000-0000-0000-0000-000000001202'::uuid,
            '90000000-0000-0000-0000-000000000902'::uuid,
            '90000000-0000-0000-0000-000000000703'::uuid,
            '90000000-0000-0000-0000-000000000402'::uuid,
            '90000000-0000-0000-0000-000000001102'::uuid,
            '{"type":"weekly-pattern","entries":[{"dayOfWeeks":["TH"],"startTime":"19:00","durationMinutes":90}]}'
        )
) AS v(id, class_id, student_profile_id, tuition_plan_id, registration_id, session_selection_pattern)
ON CONFLICT ("Id") DO UPDATE
SET
    "ClassId" = EXCLUDED."ClassId",
    "StudentProfileId" = EXCLUDED."StudentProfileId",
    "EnrollDate" = EXCLUDED."EnrollDate",
    "Status" = EXCLUDED."Status",
    "TuitionPlanId" = EXCLUDED."TuitionPlanId",
    "RegistrationId" = EXCLUDED."RegistrationId",
    "Track" = EXCLUDED."Track",
    "SessionSelectionPattern" = EXCLUDED."SessionSelectionPattern",
    "EnrollmentConfirmationPdfUrl" = EXCLUDED."EnrollmentConfirmationPdfUrl",
    "EnrollmentConfirmationPdfGeneratedAt" = EXCLUDED."EnrollmentConfirmationPdfGeneratedAt",
    "EnrollmentConfirmationPdfGeneratedBy" = EXCLUDED."EnrollmentConfirmationPdfGeneratedBy",
    "UpdatedAt" = EXCLUDED."UpdatedAt";

INSERT INTO public."ClassEnrollmentScheduleSegments"
(
    "Id",
    "ClassEnrollmentId",
    "EffectiveFrom",
    "EffectiveTo",
    "SessionSelectionPattern",
    "CreatedAt",
    "UpdatedAt"
)
SELECT
    v.id,
    v.class_enrollment_id,
    c.class_start_date,
    NULL,
    v.session_selection_pattern,
    c.now_utc,
    c.now_utc
FROM demo_context c
CROSS JOIN
(
    VALUES
        (
            '90000000-0000-0000-0000-000000001301'::uuid,
            '90000000-0000-0000-0000-000000001201'::uuid,
            '{"type":"weekly-pattern","entries":[{"dayOfWeeks":["TU"],"startTime":"18:00","durationMinutes":90}]}'
        ),
        (
            '90000000-0000-0000-0000-000000001302'::uuid,
            '90000000-0000-0000-0000-000000001202'::uuid,
            '{"type":"weekly-pattern","entries":[{"dayOfWeeks":["TH"],"startTime":"19:00","durationMinutes":90}]}'
        )
) AS v(id, class_enrollment_id, session_selection_pattern)
ON CONFLICT ("ClassEnrollmentId", "EffectiveFrom") DO UPDATE
SET
    "EffectiveTo" = EXCLUDED."EffectiveTo",
    "SessionSelectionPattern" = EXCLUDED."SessionSelectionPattern",
    "UpdatedAt" = EXCLUDED."UpdatedAt";

INSERT INTO public."Sessions"
(
    "Id",
    "ClassId",
    "BranchId",
    "PlannedDatetime",
    "PlannedRoomId",
    "PlannedTeacherId",
    "PlannedAssistantId",
    "DurationMinutes",
    "ParticipationType",
    "Status",
    "ActualDatetime",
    "ActualRoomId",
    "ActualTeacherId",
    "ActualAssistantId",
    "Color",
    "CreatedAt",
    "UpdatedAt"
)
SELECT
    v.id,
    v.class_id,
    '90000000-0000-0000-0000-000000000001'::uuid,
    v.planned_datetime,
    v.room_id,
    '90000000-0000-0000-0000-000000000604'::uuid,
    '90000000-0000-0000-0000-000000000605'::uuid,
    90,
    'Main',
    v.status,
    v.actual_datetime,
    CASE WHEN v.actual_datetime IS NULL THEN NULL ELSE v.room_id END,
    CASE WHEN v.actual_datetime IS NULL THEN NULL ELSE '90000000-0000-0000-0000-000000000604'::uuid END,
    CASE WHEN v.actual_datetime IS NULL THEN NULL ELSE '90000000-0000-0000-0000-000000000605'::uuid END,
    v.color,
    c.now_utc,
    c.now_utc
FROM demo_context c
CROSS JOIN
(
    VALUES
        (
            '90000000-0000-0000-0000-000000001401'::uuid,
            '90000000-0000-0000-0000-000000000901'::uuid,
            '90000000-0000-0000-0000-000000000501'::uuid,
            ((CURRENT_DATE - INTERVAL '21 days')::date::timestamp + TIME '18:00'),
            'Completed',
            ((CURRENT_DATE - INTERVAL '21 days')::date::timestamp + TIME '18:00'),
            '#2E86DE'
        ),
        (
            '90000000-0000-0000-0000-000000001402'::uuid,
            '90000000-0000-0000-0000-000000000901'::uuid,
            '90000000-0000-0000-0000-000000000501'::uuid,
            ((CURRENT_DATE - INTERVAL '14 days')::date::timestamp + TIME '18:00'),
            'Completed',
            ((CURRENT_DATE - INTERVAL '14 days')::date::timestamp + TIME '18:00'),
            '#2E86DE'
        ),
        (
            '90000000-0000-0000-0000-000000001403'::uuid,
            '90000000-0000-0000-0000-000000000901'::uuid,
            '90000000-0000-0000-0000-000000000501'::uuid,
            ((CURRENT_DATE - INTERVAL '7 days')::date::timestamp + TIME '18:00'),
            'Completed',
            ((CURRENT_DATE - INTERVAL '7 days')::date::timestamp + TIME '18:00'),
            '#2E86DE'
        ),
        (
            '90000000-0000-0000-0000-000000001404'::uuid,
            '90000000-0000-0000-0000-000000000901'::uuid,
            '90000000-0000-0000-0000-000000000501'::uuid,
            ((CURRENT_DATE + INTERVAL '7 days')::date::timestamp + TIME '18:00'),
            'Scheduled',
            NULL::timestamp,
            '#54A0FF'
        ),
        (
            '90000000-0000-0000-0000-000000001405'::uuid,
            '90000000-0000-0000-0000-000000000901'::uuid,
            '90000000-0000-0000-0000-000000000501'::uuid,
            ((CURRENT_DATE + INTERVAL '14 days')::date::timestamp + TIME '18:00'),
            'Scheduled',
            NULL::timestamp,
            '#54A0FF'
        ),
        (
            '90000000-0000-0000-0000-000000001406'::uuid,
            '90000000-0000-0000-0000-000000000901'::uuid,
            '90000000-0000-0000-0000-000000000501'::uuid,
            ((CURRENT_DATE + INTERVAL '21 days')::date::timestamp + TIME '18:00'),
            'Scheduled',
            NULL::timestamp,
            '#54A0FF'
        ),
        (
            '90000000-0000-0000-0000-000000001407'::uuid,
            '90000000-0000-0000-0000-000000000902'::uuid,
            '90000000-0000-0000-0000-000000000502'::uuid,
            ((CURRENT_DATE - INTERVAL '21 days')::date::timestamp + TIME '19:00'),
            'Completed',
            ((CURRENT_DATE - INTERVAL '21 days')::date::timestamp + TIME '19:00'),
            '#10AC84'
        ),
        (
            '90000000-0000-0000-0000-000000001408'::uuid,
            '90000000-0000-0000-0000-000000000902'::uuid,
            '90000000-0000-0000-0000-000000000502'::uuid,
            ((CURRENT_DATE - INTERVAL '14 days')::date::timestamp + TIME '19:00'),
            'Completed',
            ((CURRENT_DATE - INTERVAL '14 days')::date::timestamp + TIME '19:00'),
            '#10AC84'
        ),
        (
            '90000000-0000-0000-0000-000000001409'::uuid,
            '90000000-0000-0000-0000-000000000902'::uuid,
            '90000000-0000-0000-0000-000000000502'::uuid,
            ((CURRENT_DATE - INTERVAL '7 days')::date::timestamp + TIME '19:00'),
            'Completed',
            ((CURRENT_DATE - INTERVAL '7 days')::date::timestamp + TIME '19:00'),
            '#10AC84'
        ),
        (
            '90000000-0000-0000-0000-000000001410'::uuid,
            '90000000-0000-0000-0000-000000000902'::uuid,
            '90000000-0000-0000-0000-000000000502'::uuid,
            ((CURRENT_DATE + INTERVAL '7 days')::date::timestamp + TIME '19:00'),
            'Scheduled',
            NULL::timestamp,
            '#1DD1A1'
        )
) AS v(id, class_id, room_id, planned_datetime, status, actual_datetime, color)
ON CONFLICT ("Id") DO UPDATE
SET
    "ClassId" = EXCLUDED."ClassId",
    "BranchId" = EXCLUDED."BranchId",
    "PlannedDatetime" = EXCLUDED."PlannedDatetime",
    "PlannedRoomId" = EXCLUDED."PlannedRoomId",
    "PlannedTeacherId" = EXCLUDED."PlannedTeacherId",
    "PlannedAssistantId" = EXCLUDED."PlannedAssistantId",
    "DurationMinutes" = EXCLUDED."DurationMinutes",
    "ParticipationType" = EXCLUDED."ParticipationType",
    "Status" = EXCLUDED."Status",
    "ActualDatetime" = EXCLUDED."ActualDatetime",
    "ActualRoomId" = EXCLUDED."ActualRoomId",
    "ActualTeacherId" = EXCLUDED."ActualTeacherId",
    "ActualAssistantId" = EXCLUDED."ActualAssistantId",
    "Color" = EXCLUDED."Color",
    "UpdatedAt" = EXCLUDED."UpdatedAt";

INSERT INTO public."StudentSessionAssignments"
(
    "Id",
    "SessionId",
    "StudentProfileId",
    "ClassEnrollmentId",
    "RegistrationId",
    "Track",
    "Status",
    "CreatedAt",
    "UpdatedAt"
)
SELECT
    v.id,
    v.session_id,
    v.student_profile_id,
    v.class_enrollment_id,
    v.registration_id,
    'Primary',
    'Assigned',
    c.now_utc,
    c.now_utc
FROM demo_context c
CROSS JOIN
(
    VALUES
        ('90000000-0000-0000-0000-000000001501'::uuid, '90000000-0000-0000-0000-000000001401'::uuid, '90000000-0000-0000-0000-000000000702'::uuid, '90000000-0000-0000-0000-000000001201'::uuid, '90000000-0000-0000-0000-000000001101'::uuid),
        ('90000000-0000-0000-0000-000000001502'::uuid, '90000000-0000-0000-0000-000000001402'::uuid, '90000000-0000-0000-0000-000000000702'::uuid, '90000000-0000-0000-0000-000000001201'::uuid, '90000000-0000-0000-0000-000000001101'::uuid),
        ('90000000-0000-0000-0000-000000001503'::uuid, '90000000-0000-0000-0000-000000001403'::uuid, '90000000-0000-0000-0000-000000000702'::uuid, '90000000-0000-0000-0000-000000001201'::uuid, '90000000-0000-0000-0000-000000001101'::uuid),
        ('90000000-0000-0000-0000-000000001504'::uuid, '90000000-0000-0000-0000-000000001404'::uuid, '90000000-0000-0000-0000-000000000702'::uuid, '90000000-0000-0000-0000-000000001201'::uuid, '90000000-0000-0000-0000-000000001101'::uuid),
        ('90000000-0000-0000-0000-000000001505'::uuid, '90000000-0000-0000-0000-000000001405'::uuid, '90000000-0000-0000-0000-000000000702'::uuid, '90000000-0000-0000-0000-000000001201'::uuid, '90000000-0000-0000-0000-000000001101'::uuid),
        ('90000000-0000-0000-0000-000000001506'::uuid, '90000000-0000-0000-0000-000000001406'::uuid, '90000000-0000-0000-0000-000000000702'::uuid, '90000000-0000-0000-0000-000000001201'::uuid, '90000000-0000-0000-0000-000000001101'::uuid),
        ('90000000-0000-0000-0000-000000001507'::uuid, '90000000-0000-0000-0000-000000001407'::uuid, '90000000-0000-0000-0000-000000000703'::uuid, '90000000-0000-0000-0000-000000001202'::uuid, '90000000-0000-0000-0000-000000001102'::uuid),
        ('90000000-0000-0000-0000-000000001508'::uuid, '90000000-0000-0000-0000-000000001408'::uuid, '90000000-0000-0000-0000-000000000703'::uuid, '90000000-0000-0000-0000-000000001202'::uuid, '90000000-0000-0000-0000-000000001102'::uuid),
        ('90000000-0000-0000-0000-000000001509'::uuid, '90000000-0000-0000-0000-000000001409'::uuid, '90000000-0000-0000-0000-000000000703'::uuid, '90000000-0000-0000-0000-000000001202'::uuid, '90000000-0000-0000-0000-000000001102'::uuid),
        ('90000000-0000-0000-0000-000000001510'::uuid, '90000000-0000-0000-0000-000000001410'::uuid, '90000000-0000-0000-0000-000000000703'::uuid, '90000000-0000-0000-0000-000000001202'::uuid, '90000000-0000-0000-0000-000000001102'::uuid)
) AS v(id, session_id, student_profile_id, class_enrollment_id, registration_id)
ON CONFLICT ("SessionId", "ClassEnrollmentId") DO UPDATE
SET
    "StudentProfileId" = EXCLUDED."StudentProfileId",
    "RegistrationId" = EXCLUDED."RegistrationId",
    "Track" = EXCLUDED."Track",
    "Status" = EXCLUDED."Status",
    "UpdatedAt" = EXCLUDED."UpdatedAt";

INSERT INTO public."Attendances"
(
    "Id",
    "SessionId",
    "StudentProfileId",
    "AttendanceStatus",
    "AbsenceType",
    "MarkedBy",
    "MarkedAt",
    "Note"
)
SELECT
    v.id,
    v.session_id,
    v.student_profile_id,
    'Present',
    NULL,
    '90000000-0000-0000-0000-000000000604'::uuid,
    c.now_utc,
    v.note
FROM demo_context c
CROSS JOIN
(
    VALUES
        ('90000000-0000-0000-0000-000000001601'::uuid, '90000000-0000-0000-0000-000000001401'::uuid, '90000000-0000-0000-0000-000000000702'::uuid, 'Completed session 1 for Nguyen Minh An.'),
        ('90000000-0000-0000-0000-000000001602'::uuid, '90000000-0000-0000-0000-000000001402'::uuid, '90000000-0000-0000-0000-000000000702'::uuid, 'Completed session 2 for Nguyen Minh An.'),
        ('90000000-0000-0000-0000-000000001603'::uuid, '90000000-0000-0000-0000-000000001403'::uuid, '90000000-0000-0000-0000-000000000702'::uuid, 'Completed session 3 for Nguyen Minh An.'),
        ('90000000-0000-0000-0000-000000001604'::uuid, '90000000-0000-0000-0000-000000001407'::uuid, '90000000-0000-0000-0000-000000000703'::uuid, 'Completed session 1 for Tran Gia Binh.'),
        ('90000000-0000-0000-0000-000000001605'::uuid, '90000000-0000-0000-0000-000000001408'::uuid, '90000000-0000-0000-0000-000000000703'::uuid, 'Completed session 2 for Tran Gia Binh.'),
        ('90000000-0000-0000-0000-000000001606'::uuid, '90000000-0000-0000-0000-000000001409'::uuid, '90000000-0000-0000-0000-000000000703'::uuid, 'Completed session 3 for Tran Gia Binh.')
) AS v(id, session_id, student_profile_id, note)
ON CONFLICT ("SessionId", "StudentProfileId") DO UPDATE
SET
    "AttendanceStatus" = EXCLUDED."AttendanceStatus",
    "AbsenceType" = EXCLUDED."AbsenceType",
    "MarkedBy" = EXCLUDED."MarkedBy",
    "MarkedAt" = EXCLUDED."MarkedAt",
    "Note" = EXCLUDED."Note";

INSERT INTO public."StudentLevels"
(
    "Id",
    "StudentProfileId",
    "CurrentLevel",
    "CurrentXp",
    "UpdatedAt"
)
SELECT
    v.id,
    v.student_profile_id,
    v.current_level,
    v.current_xp,
    c.now_utc
FROM demo_context c
CROSS JOIN
(
    VALUES
        ('90000000-0000-0000-0000-000000001701'::uuid, '90000000-0000-0000-0000-000000000702'::uuid, 'Bronze 2', 120),
        ('90000000-0000-0000-0000-000000001702'::uuid, '90000000-0000-0000-0000-000000000703'::uuid, 'Silver 1', 260)
) AS v(id, student_profile_id, current_level, current_xp)
ON CONFLICT ("StudentProfileId") DO UPDATE
SET
    "CurrentLevel" = EXCLUDED."CurrentLevel",
    "CurrentXp" = EXCLUDED."CurrentXp",
    "UpdatedAt" = EXCLUDED."UpdatedAt";

INSERT INTO public."Notifications"
(
    "Id",
    "RecipientUserId",
    "RecipientProfileId",
    "Channel",
    "Title",
    "Content",
    "Deeplink",
    "Status",
    "SentAt",
    "ReadAt",
    "TemplateId",
    "NotificationTemplateId",
    "CreatedAt",
    "TargetRole",
    "Kind",
    "Priority",
    "SenderRole",
    "SenderName",
    "ScopeBranchId",
    "ScopeClassId",
    "ScopeStudentProfileId"
)
SELECT
    v.id,
    v.recipient_user_id,
    v.recipient_profile_id,
    v.channel,
    v.title,
    v.content,
    v.deeplink,
    'Sent',
    c.now_utc,
    NULL,
    NULL,
    NULL,
    c.now_utc,
    v.target_role,
    'package',
    v.priority,
    'System',
    'Kidzgo',
    '90000000-0000-0000-0000-000000000001'::uuid,
    v.scope_class_id,
    v.scope_student_profile_id
FROM demo_context c
CROSS JOIN
(
    VALUES
        (
            '90000000-0000-0000-0000-000000001801'::uuid,
            '90000000-0000-0000-0000-000000000606'::uuid,
            '90000000-0000-0000-0000-000000000701'::uuid,
            'InApp',
            'Goi hoc con 3 buoi',
            'Hoc vien Nguyen Minh An con 3 buoi trong goi Demo Starters 6 Sessions.',
            '/parent/registrations/90000000-0000-0000-0000-000000001101',
            'Parent',
            'normal',
            '90000000-0000-0000-0000-000000000901'::uuid,
            '90000000-0000-0000-0000-000000000702'::uuid
        ),
        (
            '90000000-0000-0000-0000-000000001802'::uuid,
            '90000000-0000-0000-0000-000000000607'::uuid,
            '90000000-0000-0000-0000-000000000702'::uuid,
            'InApp',
            'Ban con 3 buoi hoc',
            'Dang ky Demo Starters cua ban con 3 buoi. Vui long theo doi de dang ky tiep.',
            '/student/registrations/90000000-0000-0000-0000-000000001101',
            'Student',
            'normal',
            '90000000-0000-0000-0000-000000000901'::uuid,
            '90000000-0000-0000-0000-000000000702'::uuid
        ),
        (
            '90000000-0000-0000-0000-000000001803'::uuid,
            '90000000-0000-0000-0000-000000000606'::uuid,
            '90000000-0000-0000-0000-000000000701'::uuid,
            'InApp',
            'Goi hoc con 1 buoi',
            'Hoc vien Tran Gia Binh chi con 1 buoi trong goi Demo Movers 4 Sessions.',
            '/parent/registrations/90000000-0000-0000-0000-000000001102',
            'Parent',
            'high',
            '90000000-0000-0000-0000-000000000902'::uuid,
            '90000000-0000-0000-0000-000000000703'::uuid
        ),
        (
            '90000000-0000-0000-0000-000000001804'::uuid,
            '90000000-0000-0000-0000-000000000606'::uuid,
            '90000000-0000-0000-0000-000000000701'::uuid,
            'Email',
            'Goi hoc con 1 buoi',
            'Hoc vien Tran Gia Binh chi con 1 buoi trong goi Demo Movers 4 Sessions. Nen tao dang ky moi som.',
            '/parent/registrations/90000000-0000-0000-0000-000000001102',
            'Parent',
            'high',
            '90000000-0000-0000-0000-000000000902'::uuid,
            '90000000-0000-0000-0000-000000000703'::uuid
        ),
        (
            '90000000-0000-0000-0000-000000001805'::uuid,
            '90000000-0000-0000-0000-000000000606'::uuid,
            '90000000-0000-0000-0000-000000000701'::uuid,
            'Push',
            'Goi hoc con 1 buoi',
            'Hoc vien Tran Gia Binh chi con 1 buoi trong goi Demo Movers 4 Sessions.',
            '/parent/registrations/90000000-0000-0000-0000-000000001102',
            'Parent',
            'high',
            '90000000-0000-0000-0000-000000000902'::uuid,
            '90000000-0000-0000-0000-000000000703'::uuid
        ),
        (
            '90000000-0000-0000-0000-000000001806'::uuid,
            '90000000-0000-0000-0000-000000000608'::uuid,
            '90000000-0000-0000-0000-000000000703'::uuid,
            'InApp',
            'Ban con 1 buoi hoc',
            'Dang ky Demo Movers cua ban chi con 1 buoi. Hay lien he trung tam de dang ky tiep.',
            '/student/registrations/90000000-0000-0000-0000-000000001102',
            'Student',
            'high',
            '90000000-0000-0000-0000-000000000902'::uuid,
            '90000000-0000-0000-0000-000000000703'::uuid
        ),
        (
            '90000000-0000-0000-0000-000000001807'::uuid,
            '90000000-0000-0000-0000-000000000608'::uuid,
            '90000000-0000-0000-0000-000000000703'::uuid,
            'Email',
            'Ban con 1 buoi hoc',
            'Dang ky Demo Movers cua ban chi con 1 buoi. Hay dang ky tiep de khong bi gian doan.',
            '/student/registrations/90000000-0000-0000-0000-000000001102',
            'Student',
            'high',
            '90000000-0000-0000-0000-000000000902'::uuid,
            '90000000-0000-0000-0000-000000000703'::uuid
        ),
        (
            '90000000-0000-0000-0000-000000001808'::uuid,
            '90000000-0000-0000-0000-000000000608'::uuid,
            '90000000-0000-0000-0000-000000000703'::uuid,
            'Push',
            'Ban con 1 buoi hoc',
            'Dang ky Demo Movers cua ban chi con 1 buoi.',
            '/student/registrations/90000000-0000-0000-0000-000000001102',
            'Student',
            'high',
            '90000000-0000-0000-0000-000000000902'::uuid,
            '90000000-0000-0000-0000-000000000703'::uuid
        )
) AS v(id, recipient_user_id, recipient_profile_id, channel, title, content, deeplink, target_role, priority, scope_class_id, scope_student_profile_id)
ON CONFLICT ("Id") DO UPDATE
SET
    "RecipientUserId" = EXCLUDED."RecipientUserId",
    "RecipientProfileId" = EXCLUDED."RecipientProfileId",
    "Channel" = EXCLUDED."Channel",
    "Title" = EXCLUDED."Title",
    "Content" = EXCLUDED."Content",
    "Deeplink" = EXCLUDED."Deeplink",
    "Status" = EXCLUDED."Status",
    "SentAt" = EXCLUDED."SentAt",
    "ReadAt" = EXCLUDED."ReadAt",
    "TemplateId" = EXCLUDED."TemplateId",
    "NotificationTemplateId" = EXCLUDED."NotificationTemplateId",
    "CreatedAt" = EXCLUDED."CreatedAt",
    "TargetRole" = EXCLUDED."TargetRole",
    "Kind" = EXCLUDED."Kind",
    "Priority" = EXCLUDED."Priority",
    "SenderRole" = EXCLUDED."SenderRole",
    "SenderName" = EXCLUDED."SenderName",
    "ScopeBranchId" = EXCLUDED."ScopeBranchId",
    "ScopeClassId" = EXCLUDED."ScopeClassId",
    "ScopeStudentProfileId" = EXCLUDED."ScopeStudentProfileId";

COMMIT;

SELECT 'Demo branch' AS check_name, COUNT(*) AS total
FROM public."Branches"
WHERE "Id" = '90000000-0000-0000-0000-000000000001'::uuid
UNION ALL
SELECT 'Demo users', COUNT(*)
FROM public."Users"
WHERE "Id" IN
(
    '90000000-0000-0000-0000-000000000601'::uuid,
    '90000000-0000-0000-0000-000000000602'::uuid,
    '90000000-0000-0000-0000-000000000603'::uuid,
    '90000000-0000-0000-0000-000000000604'::uuid,
    '90000000-0000-0000-0000-000000000605'::uuid,
    '90000000-0000-0000-0000-000000000606'::uuid,
    '90000000-0000-0000-0000-000000000607'::uuid,
    '90000000-0000-0000-0000-000000000608'::uuid
)
UNION ALL
SELECT 'Demo classes', COUNT(*)
FROM public."Classes"
WHERE "Id" IN
(
    '90000000-0000-0000-0000-000000000901'::uuid,
    '90000000-0000-0000-0000-000000000902'::uuid
)
UNION ALL
SELECT 'Demo registrations', COUNT(*)
FROM public."Registrations"
WHERE "Id" IN
(
    '90000000-0000-0000-0000-000000001101'::uuid,
    '90000000-0000-0000-0000-000000001102'::uuid
)
UNION ALL
SELECT 'Demo sessions', COUNT(*)
FROM public."Sessions"
WHERE "Id" IN
(
    '90000000-0000-0000-0000-000000001401'::uuid,
    '90000000-0000-0000-0000-000000001402'::uuid,
    '90000000-0000-0000-0000-000000001403'::uuid,
    '90000000-0000-0000-0000-000000001404'::uuid,
    '90000000-0000-0000-0000-000000001405'::uuid,
    '90000000-0000-0000-0000-000000001406'::uuid,
    '90000000-0000-0000-0000-000000001407'::uuid,
    '90000000-0000-0000-0000-000000001408'::uuid,
    '90000000-0000-0000-0000-000000001409'::uuid,
    '90000000-0000-0000-0000-000000001410'::uuid
)
UNION ALL
SELECT 'Demo notifications', COUNT(*)
FROM public."Notifications"
WHERE "Id" IN
(
    '90000000-0000-0000-0000-000000001801'::uuid,
    '90000000-0000-0000-0000-000000001802'::uuid,
    '90000000-0000-0000-0000-000000001803'::uuid,
    '90000000-0000-0000-0000-000000001804'::uuid,
    '90000000-0000-0000-0000-000000001805'::uuid,
    '90000000-0000-0000-0000-000000001806'::uuid,
    '90000000-0000-0000-0000-000000001807'::uuid,
    '90000000-0000-0000-0000-000000001808'::uuid
);
