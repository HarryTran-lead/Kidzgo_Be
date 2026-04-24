-- Run this script in the target PostgreSQL database, schema `public`.
-- If the SQL console is still holding an old transaction, run `ROLLBACK;` first.
--
-- This reset keeps only:
--   Branches, Users, Profiles, Blogs, NotificationTemplates, EmailTemplates
--
-- Assumptions:
-- 1. QuestionBankItems can be deleted.
-- 2. Tuition plans are seeded as global plans (`BranchId = NULL`).
-- 3. BranchPrograms are seeded for every active branch. If no active branch exists,
--    the script uses every branch instead.
-- 4. Seeded classes/classrooms are created in the first active branch, or the first
--    available branch if none are active.
-- 5. Cambridge Starters is missing in the tuition-plan note from the source input,
--    but it is seeded with the same 3 tiers as Movers/Flyers/KET/PET so Starters
--    classes still have fee data.
-- 6. The source note "English Club" is mapped to the supplementary program
--    "Speaking Club".
-- 7. Kem LMS package pricing is assumed as:
--      12 buoi @ 100,000
--      24 buoi @ 90,000
--      36 buoi @ 80,000
--      48 buoi @ 70,000
-- 8. Supplementary class capacities default to 10 because the source did not specify them.
-- 9. Weekly schedules are seeded as structured weekly slot JSON.
--    Some legacy rows may still use RRULE for backward compatibility.

BEGIN;

CREATE TEMP TABLE seed_context AS
SELECT
    timezone('UTC', now()) AS now_utc,
    CURRENT_DATE::date AS start_date,
    (CURRENT_DATE + INTERVAL '12 months' - INTERVAL '1 day')::date AS end_date,
    COALESCE(
        (SELECT "Id" FROM public."Branches" WHERE "IsActive" = TRUE ORDER BY "CreatedAt" NULLS LAST, "Id" LIMIT 1),
        (SELECT "Id" FROM public."Branches" ORDER BY "CreatedAt" NULLS LAST, "Id" LIMIT 1)
    )::uuid AS primary_branch_id;

CREATE TEMP TABLE required_branch
(
    primary_branch_id uuid NOT NULL
);

INSERT INTO required_branch (primary_branch_id)
SELECT primary_branch_id
FROM seed_context;

CREATE TEMP TABLE target_branches AS
SELECT b."Id" AS branch_id
FROM public."Branches" b
WHERE b."IsActive" = TRUE;

INSERT INTO target_branches (branch_id)
SELECT b."Id"
FROM public."Branches" b
WHERE NOT EXISTS (SELECT 1 FROM target_branches);

CREATE TEMP TABLE seed_programs
(
    program_id uuid PRIMARY KEY,
    code text NOT NULL,
    name text NOT NULL,
    description text,
    is_supplementary boolean NOT NULL
);

INSERT INTO seed_programs (program_id, code, name, description, is_supplementary)
VALUES
    ('11111111-1111-1111-1111-111111111201'::uuid, 'APPLE2', 'Apple 2',
        'Apple 2 là chương trình dành cho học viên nhỏ tuổi, thường từ 4 đến 6 tuổi. Chương trình giúp các bé làm quen với tiếng Anh thông qua hình ảnh, âm thanh, trò chơi, bài hát và các hoạt động tương tác trong lớp. Mục tiêu của chương trình là giúp học viên xây dựng sự tự tin khi nghe và nói tiếng Anh, nhận diện từ vựng quen thuộc, luyện phát âm cơ bản và hình thành thói quen học tiếng Anh tích cực ngay từ giai đoạn đầu.',
        FALSE),
    ('11111111-1111-1111-1111-111111111202'::uuid, 'PHONICS', 'Phonics Foundation',
        'Phonics Foundation là chương trình nền tảng phát âm dành cho học viên từ 5 đến 6 tuổi hoặc học viên mới bắt đầu học tiếng Anh. Học viên được học âm chữ cái, cách ghép âm, nhận diện mặt chữ và luyện đọc các từ đơn giản. Chương trình giúp học viên phát âm rõ hơn, đọc tốt hơn và có nền tảng vững chắc trước khi chuyển sang các cấp độ Cambridge như Starters, Movers và Flyers.',
        FALSE),
    ('11111111-1111-1111-1111-111111111203'::uuid, 'STARTERS', 'Cambridge Starters',
        'Cambridge Starters là chương trình dành cho học viên ở trình độ sơ cấp, thường từ 6 đến 8 tuổi. Nội dung học tập trung phát triển 4 kỹ năng: Listening, Speaking, Reading và Writing theo định hướng Cambridge. Học viên được học từ vựng, mẫu câu giao tiếp cơ bản, luyện nghe hiểu, đọc hiểu đơn giản và làm quen với cấu trúc bài thi Cambridge Starters.',
        FALSE),
    ('11111111-1111-1111-1111-111111111204'::uuid, 'MOVERS', 'Cambridge Movers',
        'Cambridge Movers là chương trình dành cho học viên đã có nền tảng tiếng Anh cơ bản và muốn phát triển kỹ năng ở mức cao hơn Starters. Chương trình giúp học viên mở rộng vốn từ vựng, luyện giao tiếp theo chủ đề, phát triển khả năng đọc hiểu, viết câu hoặc đoạn ngắn và luyện các dạng bài theo chuẩn Cambridge Movers.',
        FALSE),
    ('11111111-1111-1111-1111-111111111205'::uuid, 'FLYERS', 'Cambridge Flyers',
        'Cambridge Flyers là chương trình dành cho học viên có trình độ tiền trung cấp, chuẩn bị hoàn thiện cấp độ Young Learners của Cambridge. Nội dung học tập trung nâng cao khả năng nghe, nói, đọc, viết; sử dụng ngữ pháp linh hoạt hơn; đọc hiểu đoạn văn dài hơn và luyện kỹ năng làm bài thi Cambridge Flyers.',
        FALSE),
    ('11111111-1111-1111-1111-111111111206'::uuid, 'KETA2', 'KET (A2 Key)',
        'KET, còn gọi là A2 Key, là chương trình dành cho học viên ở trình độ A2 theo khung năng lực châu Âu CEFR. Học viên được rèn luyện khả năng giao tiếp trong các tình huống quen thuộc, đọc hiểu văn bản ngắn, viết email hoặc ghi chú đơn giản và luyện đề theo cấu trúc bài thi A2 Key của Cambridge.',
        FALSE),
    ('11111111-1111-1111-1111-111111111207'::uuid, 'PETB1', 'PET (B1 Preliminary)',
        'PET, còn gọi là B1 Preliminary, là chương trình dành cho học viên ở trình độ B1, hướng đến khả năng sử dụng tiếng Anh độc lập trong học tập và giao tiếp hằng ngày. Học viên được luyện kỹ năng đọc hiểu văn bản dài hơn, viết đoạn văn hoặc email, nghe hiểu hội thoại thực tế và trình bày ý kiến bằng tiếng Anh theo chuẩn bài thi B1 Preliminary.',
        FALSE),
    ('11111111-1111-1111-1111-111111111208'::uuid, 'KEMLMS', 'Kèm LMS',
        'Kèm LMS là chương trình phụ hỗ trợ học viên học thêm các môn hoặc nội dung tiếng Anh theo nhu cầu cá nhân, ví dụ ESL, Science hoặc chương trình học quốc tế. Hình thức học linh hoạt theo gói buổi, giúp học viên củng cố kiến thức trên lớp, hoàn thành bài tập LMS, cải thiện kỹ năng học thuật và theo kịp chương trình đang học.',
        TRUE),
    ('11111111-1111-1111-1111-111111111209'::uuid, 'SPKCLUB', 'Speaking Club',
        'Speaking Club là chương trình câu lạc bộ giao tiếp cuối tuần, giúp học viên tăng phản xạ nói tiếng Anh thông qua trò chơi, thảo luận, thuyết trình ngắn, hoạt động nhóm và các tình huống thực tế. Mục tiêu của chương trình là giúp học viên tự tin hơn khi giao tiếp, cải thiện phát âm, mở rộng vốn từ và sử dụng tiếng Anh tự nhiên hơn.',
        TRUE),
    ('11111111-1111-1111-1111-111111111210'::uuid, 'WRBOOST', 'Writing Booster',
        'Writing Booster là chương trình tăng cường kỹ năng viết, phù hợp với học viên cần cải thiện khả năng viết câu, đoạn văn, email hoặc bài viết ngắn. Học viên được hướng dẫn cách xây dựng ý tưởng, sử dụng từ vựng và ngữ pháp phù hợp, sắp xếp nội dung rõ ràng và hạn chế lỗi sai thường gặp khi viết.',
        TRUE),
    ('11111111-1111-1111-1111-111111111211'::uuid, 'SUMCAMP', 'Summer Camp',
        'Summer Camp là chương trình hè kết hợp giữa học tiếng Anh và các hoạt động trải nghiệm. Học viên được tham gia các hoạt động giao tiếp, trò chơi, dự án nhóm, thuyết trình, thủ công, kỹ năng sống hoặc khám phá chủ đề theo tuần. Chương trình giúp học viên vừa học vừa chơi, tăng sự tự tin và duy trì môi trường tiếng Anh trong kỳ nghỉ hè.',
        TRUE);

CREATE TEMP TABLE seed_tuition_plans
(
    tuition_plan_id uuid PRIMARY KEY,
    program_id uuid NOT NULL,
    name text NOT NULL,
    total_sessions integer NOT NULL,
    tuition_amount numeric NOT NULL,
    unit_price_session numeric NOT NULL
);

INSERT INTO seed_tuition_plans (tuition_plan_id, program_id, name, total_sessions, tuition_amount, unit_price_session)
VALUES
    ('aaaaaaaa-0000-0000-0000-000000000001'::uuid, '11111111-1111-1111-1111-111111111201'::uuid, 'Basic Plan', 24, 4200000, 175000),
    ('aaaaaaaa-0000-0000-0000-000000000002'::uuid, '11111111-1111-1111-1111-111111111201'::uuid, 'Standard Plan', 48, 7800000, 162500),
    ('aaaaaaaa-0000-0000-0000-000000000003'::uuid, '11111111-1111-1111-1111-111111111201'::uuid, 'Premium Plan', 96, 15600000, 162500),

    ('aaaaaaaa-0000-0000-0000-000000000004'::uuid, '11111111-1111-1111-1111-111111111202'::uuid, 'Basic Plan', 24, 4200000, 175000),
    ('aaaaaaaa-0000-0000-0000-000000000005'::uuid, '11111111-1111-1111-1111-111111111202'::uuid, 'Standard Plan', 48, 7800000, 162500),
    ('aaaaaaaa-0000-0000-0000-000000000006'::uuid, '11111111-1111-1111-1111-111111111202'::uuid, 'Premium Plan', 96, 15600000, 162500),

    ('aaaaaaaa-0000-0000-0000-000000000007'::uuid, '11111111-1111-1111-1111-111111111203'::uuid, 'Basic Plan', 24, 4200000, 175000),
    ('aaaaaaaa-0000-0000-0000-000000000008'::uuid, '11111111-1111-1111-1111-111111111203'::uuid, 'Standard Plan', 48, 7800000, 162500),
    ('aaaaaaaa-0000-0000-0000-000000000009'::uuid, '11111111-1111-1111-1111-111111111203'::uuid, 'Premium Plan', 96, 15600000, 162500),

    ('aaaaaaaa-0000-0000-0000-000000000010'::uuid, '11111111-1111-1111-1111-111111111204'::uuid, 'Basic Plan', 24, 4200000, 175000),
    ('aaaaaaaa-0000-0000-0000-000000000011'::uuid, '11111111-1111-1111-1111-111111111204'::uuid, 'Standard Plan', 48, 7800000, 162500),
    ('aaaaaaaa-0000-0000-0000-000000000012'::uuid, '11111111-1111-1111-1111-111111111204'::uuid, 'Premium Plan', 96, 15600000, 162500),

    ('aaaaaaaa-0000-0000-0000-000000000013'::uuid, '11111111-1111-1111-1111-111111111205'::uuid, 'Basic Plan', 24, 4200000, 175000),
    ('aaaaaaaa-0000-0000-0000-000000000014'::uuid, '11111111-1111-1111-1111-111111111205'::uuid, 'Standard Plan', 48, 7800000, 162500),
    ('aaaaaaaa-0000-0000-0000-000000000015'::uuid, '11111111-1111-1111-1111-111111111205'::uuid, 'Premium Plan', 96, 15600000, 162500),

    ('aaaaaaaa-0000-0000-0000-000000000016'::uuid, '11111111-1111-1111-1111-111111111206'::uuid, 'Basic Plan', 24, 4200000, 175000),
    ('aaaaaaaa-0000-0000-0000-000000000017'::uuid, '11111111-1111-1111-1111-111111111206'::uuid, 'Standard Plan', 48, 7800000, 162500),
    ('aaaaaaaa-0000-0000-0000-000000000018'::uuid, '11111111-1111-1111-1111-111111111206'::uuid, 'Premium Plan', 96, 15600000, 162500),

    ('aaaaaaaa-0000-0000-0000-000000000019'::uuid, '11111111-1111-1111-1111-111111111207'::uuid, 'Basic Plan', 24, 4200000, 175000),
    ('aaaaaaaa-0000-0000-0000-000000000020'::uuid, '11111111-1111-1111-1111-111111111207'::uuid, 'Standard Plan', 48, 7800000, 162500),
    ('aaaaaaaa-0000-0000-0000-000000000021'::uuid, '11111111-1111-1111-1111-111111111207'::uuid, 'Premium Plan', 96, 15600000, 162500),

    ('aaaaaaaa-0000-0000-0000-000000000022'::uuid, '11111111-1111-1111-1111-111111111209'::uuid, 'Speaking Club Weekend Pack', 12, 600000, 50000),

    ('aaaaaaaa-0000-0000-0000-000000000023'::uuid, '11111111-1111-1111-1111-111111111208'::uuid, 'LMS 12 Sessions', 12, 1200000, 100000),
    ('aaaaaaaa-0000-0000-0000-000000000024'::uuid, '11111111-1111-1111-1111-111111111208'::uuid, 'LMS 24 Sessions', 24, 2160000, 90000),
    ('aaaaaaaa-0000-0000-0000-000000000025'::uuid, '11111111-1111-1111-1111-111111111208'::uuid, 'LMS 36 Sessions', 36, 2880000, 80000),
    ('aaaaaaaa-0000-0000-0000-000000000026'::uuid, '11111111-1111-1111-1111-111111111208'::uuid, 'LMS 48 Sessions', 48, 3360000, 70000);

CREATE TEMP TABLE seed_classes
(
    class_id uuid PRIMARY KEY,
    room_id uuid NOT NULL,
    program_id uuid NOT NULL,
    code text NOT NULL,
    title text NOT NULL,
    capacity integer NOT NULL,
    weekly_schedule_json text NOT NULL,
    description text
);

INSERT INTO seed_classes (class_id, room_id, program_id, code, title, capacity, weekly_schedule_json, description)
VALUES
    ('22222222-2222-2222-2222-222222222301'::uuid, '44444444-4444-4444-4444-444444444301'::uuid, '11111111-1111-1111-1111-111111111201'::uuid, 'APPLE-A2', 'Apple A2', 8, '{"type":"weekly-slots","slots":[{"dayOfWeek":"TH","startTime":"18:00","durationMinutes":60},{"dayOfWeek":"SA","startTime":"17:00","durationMinutes":60}]}', 'Độ tuổi 4-6. Lịch: Thứ 5 18:00-19:00, Thứ 7 17:00-18:00.'),
    ('22222222-2222-2222-2222-222222222302'::uuid, '44444444-4444-4444-4444-444444444302'::uuid, '11111111-1111-1111-1111-111111111202'::uuid, 'PHONICS-P1', 'Phonics P1', 6, 'RRULE:FREQ=WEEKLY;BYDAY=MO,WE;BYHOUR=19;BYMINUTE=0;DURATION=90', 'Độ tuổi 5-6. 1 buổi GVNN, 1 buổi GVVN. Lịch: Thứ 2 19:00-20:30, Thứ 4 19:00-20:30.'),
    ('22222222-2222-2222-2222-222222222303'::uuid, '44444444-4444-4444-4444-444444444303'::uuid, '11111111-1111-1111-1111-111111111202'::uuid, 'PHONICS-P3', 'Phonics P3 + Thuyết trình', 4, '{"type":"weekly-slots","slots":[{"dayOfWeek":"TU","startTime":"16:30","durationMinutes":90},{"dayOfWeek":"FR","startTime":"17:30","durationMinutes":90}]}', 'Lịch: Thứ 3 16:30-18:00, Thứ 6 17:30-19:00.'),
    ('22222222-2222-2222-2222-222222222304'::uuid, '44444444-4444-4444-4444-444444444304'::uuid, '11111111-1111-1111-1111-111111111203'::uuid, 'STARTERS-S1', 'Starters S1', 10, '{"type":"weekly-slots","slots":[{"dayOfWeek":"TH","startTime":"16:00","durationMinutes":90},{"dayOfWeek":"FR","startTime":"19:30","durationMinutes":90}]}', 'Độ tuổi 6-8. 1 buổi GVNN, 1 buổi GVVN. Lịch: Thứ 5 16:00-17:30, Thứ 6 19:30-21:00.'),
    ('22222222-2222-2222-2222-222222222305'::uuid, '44444444-4444-4444-4444-444444444305'::uuid, '11111111-1111-1111-1111-111111111203'::uuid, 'STARTERS-S5', 'Starters S5', 10, '{"type":"weekly-slots","slots":[{"dayOfWeek":"WE","startTime":"19:30","durationMinutes":90},{"dayOfWeek":"FR","startTime":"18:00","durationMinutes":90}]}', 'Độ tuổi 6-8. 1 buổi GVNN, 1 buổi GVVN. Lịch: Thứ 4 19:30-21:00, Thứ 6 18:00-19:30.'),
    ('22222222-2222-2222-2222-222222222306'::uuid, '44444444-4444-4444-4444-444444444306'::uuid, '11111111-1111-1111-1111-111111111204'::uuid, 'MOVERS-M4', 'Movers M4', 7, 'RRULE:FREQ=WEEKLY;BYDAY=TU,TH;BYHOUR=19;BYMINUTE=30;DURATION=90', '1 buổi GVNN, 1 buổi GVVN. Lịch: Thứ 3 19:30-21:00, Thứ 5 19:30-21:00.'),
    ('22222222-2222-2222-2222-222222222307'::uuid, '44444444-4444-4444-4444-444444444307'::uuid, '11111111-1111-1111-1111-111111111204'::uuid, 'MOVERS-M2', 'Movers M2', 8, 'RRULE:FREQ=WEEKLY;BYDAY=MO,WE;BYHOUR=17;BYMINUTE=30;DURATION=90', '1 buổi GVNN, 1 buổi GVVN. Lịch: Thứ 2 17:30-19:00, Thứ 4 17:30-19:00.'),
    ('22222222-2222-2222-2222-222222222308'::uuid, '44444444-4444-4444-4444-444444444308'::uuid, '11111111-1111-1111-1111-111111111204'::uuid, 'MOVERS-M3', 'Movers M3', 4, '{"type":"weekly-slots","slots":[{"dayOfWeek":"WE","startTime":"16:00","durationMinutes":90},{"dayOfWeek":"SA","startTime":"15:30","durationMinutes":90}]}', '2 buổi GVVN. Lịch: Thứ 4 16:00-17:30, Thứ 7 15:30-17:00.'),
    ('22222222-2222-2222-2222-222222222309'::uuid, '44444444-4444-4444-4444-444444444309'::uuid, '11111111-1111-1111-1111-111111111205'::uuid, 'FLYERS-F1', 'Flyers F1', 10, '{"type":"weekly-slots","slots":[{"dayOfWeek":"TU","startTime":"18:00","durationMinutes":90},{"dayOfWeek":"TH","startTime":"18:30","durationMinutes":90}]}', '1 buổi GVNN, 1 buổi GVVN. Lịch: Thứ 3 18:00-19:30, Thứ 5 18:30-20:00.'),
    ('22222222-2222-2222-2222-222222222310'::uuid, '44444444-4444-4444-4444-444444444310'::uuid, '11111111-1111-1111-1111-111111111205'::uuid, 'FLYERS-F2', 'Flyers F2', 5, '{"type":"weekly-slots","slots":[{"dayOfWeek":"MO","startTime":"16:00","durationMinutes":90},{"dayOfWeek":"SA","startTime":"17:00","durationMinutes":90}]}', '1 buổi GVNN, 1 buổi GVVN. Lịch: Thứ 2 16:00-17:30, Thứ 7 17:00-18:30.'),
    ('22222222-2222-2222-2222-222222222311'::uuid, '44444444-4444-4444-4444-444444444311'::uuid, '11111111-1111-1111-1111-111111111207'::uuid, 'PET-K1', 'PET K1', 5, '{"type":"weekly-slots","slots":[{"dayOfWeek":"FR","startTime":"19:30","durationMinutes":90},{"dayOfWeek":"SA","startTime":"10:00","durationMinutes":90}]}', '1 buổi GVNN, 1 buổi GVVN. Lịch: Thứ 6 19:30-21:00, Thứ 7 10:00-11:30.'),
    ('22222222-2222-2222-2222-222222222312'::uuid, '44444444-4444-4444-4444-444444444312'::uuid, '11111111-1111-1111-1111-111111111208'::uuid, 'LMS-ESL-WED', 'Kèm LMS - ESL Grade 3 - Cam (Wed)', 10, 'RRULE:FREQ=WEEKLY;BYDAY=WE;BYHOUR=16;BYMINUTE=0;DURATION=90', 'Lịch: Thứ 4 16:00-17:30.'),
    ('22222222-2222-2222-2222-222222222313'::uuid, '44444444-4444-4444-4444-444444444313'::uuid, '11111111-1111-1111-1111-111111111208'::uuid, 'LMS-ESL-THU', 'Kèm LMS - ESL Grade 3 - Cam (Thu)', 10, 'RRULE:FREQ=WEEKLY;BYDAY=TH;BYHOUR=19;BYMINUTE=30;DURATION=90', 'Lịch: Thứ 5 19:30-21:00.'),
    ('22222222-2222-2222-2222-222222222314'::uuid, '44444444-4444-4444-4444-444444444314'::uuid, '11111111-1111-1111-1111-111111111208'::uuid, 'LMS-SCI-WED', 'Kèm LMS - Science Grade 3 - Cam', 10, 'RRULE:FREQ=WEEKLY;BYDAY=WE;BYHOUR=17;BYMINUTE=30;DURATION=90', 'Lịch: Thứ 4 17:30-19:00.');

TRUNCATE TABLE
    public."RefreshTokens",
    public."PasswordResetTokens",
    public."ParentPinResetTokens",
    public."ParentStudentLinks",
    public."DeviceTokens",
    public."Classrooms",
    public."Classes",
    public."ClassEnrollments",
    public."ClassScheduleSegments",
    public."ClassEnrollmentScheduleSegments",
    public."PauseEnrollmentRequests",
    public."PauseEnrollmentRequestHistories",
    public."Leads",
    public."LeadActivities",
    public."LeadChildren",
    public."PlacementTests",
    public."Exams",
    public."ExamResults",
    public."ExamQuestions",
    public."ExamSubmissions",
    public."ExamSubmissionAnswers",
    public."Invoices",
    public."InvoiceLines",
    public."Payments",
    public."CashbookEntries",
    public."Missions",
    public."MissionProgresses",
    public."RewardRedemptions",
    public."RewardStoreItems",
    public."StarTransactions",
    public."StudentLevels",
    public."AttendanceStreaks",
    public."GamificationSettings",
    public."MissionRewardRules",
    public."HomeworkAssignments",
    public."HomeworkStudents",
    public."HomeworkSubmissionAttempts",
    public."HomeworkQuestions",
    public."QuestionBankItems",
    public."LessonPlans",
    public."LessonPlanTemplates",
    public."MediaAssets",
    public."FaqCategories",
    public."FaqItems",
    public."Notifications",
    public."Contracts",
    public."PayrollLines",
    public."PayrollPayments",
    public."PayrollRuns",
    public."SessionRoles",
    public."ShiftAttendances",
    public."MonthlyWorkHours",
    public."TeacherCompensationSettings",
    public."Programs",
    public."BranchPrograms",
    public."ExtracurricularPrograms",
    public."ProgramLeavePolicies",
    public."TuitionPlans",
    public."TeachingMaterials",
    public."TeachingMaterialSlides",
    public."TeachingMaterialViewProgresses",
    public."TeachingMaterialBookmarks",
    public."TeachingMaterialAnnotations",
    public."Registrations",
    public."EnrollmentConfirmationPdfs",
    public."EnrollmentConfirmationPaymentSettings",
    public."MonthlyReportJobs",
    public."MonthlyReportData",
    public."ReportComments",
    public."ReportRequests",
    public."StudentMonthlyReports",
    public."SessionReports",
    public."Attendances",
    public."LeaveRequests",
    public."MakeupAllocations",
    public."MakeupCredits",
    public."Sessions",
    public."StudentSessionAssignments",
    public."Tickets",
    public."TicketComments",
    public."AuditLogs"
RESTART IDENTITY CASCADE;

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
    sp.program_id,
    sp.name,
    sp.code,
    sp.description,
    TRUE,
    FALSE,
    FALSE,
    sp.is_supplementary,
    ctx.now_utc,
    ctx.now_utc
FROM seed_programs sp
CROSS JOIN seed_context ctx;

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
    (
        substr(md5(tb.branch_id::text || sp.program_id::text), 1, 8) || '-' ||
        substr(md5(tb.branch_id::text || sp.program_id::text), 9, 4) || '-' ||
        substr(md5(tb.branch_id::text || sp.program_id::text), 13, 4) || '-' ||
        substr(md5(tb.branch_id::text || sp.program_id::text), 17, 4) || '-' ||
        substr(md5(tb.branch_id::text || sp.program_id::text), 21, 12)
    )::uuid,
    tb.branch_id,
    sp.program_id,
    TRUE,
    NULL,
    ctx.now_utc,
    ctx.now_utc
FROM target_branches tb
CROSS JOIN seed_programs sp
CROSS JOIN seed_context ctx;

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
    stp.tuition_plan_id,
    NULL,
    stp.program_id,
    stp.name,
    stp.total_sessions,
    stp.tuition_amount,
    stp.unit_price_session,
    'VND',
    TRUE,
    FALSE,
    ctx.now_utc,
    ctx.now_utc
FROM seed_tuition_plans stp
CROSS JOIN seed_context ctx;

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
CROSS JOIN seed_context ctx;

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
    sc.program_id,
    sc.code,
    sc.title,
    sc.room_id,
    NULL,
    NULL,
    ctx.start_date,
    ctx.end_date,
    'Recruiting',
    sc.capacity,
    sc.weekly_schedule_json,
    sc.description,
    ctx.now_utc,
    ctx.now_utc
FROM seed_classes sc
CROSS JOIN seed_context ctx;

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
        substr(md5(sc.class_id::text || '-segment'), 1, 8) || '-' ||
        substr(md5(sc.class_id::text || '-segment'), 9, 4) || '-' ||
        substr(md5(sc.class_id::text || '-segment'), 13, 4) || '-' ||
        substr(md5(sc.class_id::text || '-segment'), 17, 4) || '-' ||
        substr(md5(sc.class_id::text || '-segment'), 21, 12)
    )::uuid,
    sc.class_id,
    ctx.start_date,
    NULL,
    sc.weekly_schedule_json,
    ctx.now_utc,
    ctx.now_utc
FROM seed_classes sc
CROSS JOIN seed_context ctx;

SELECT 'Program count' AS check_name, COUNT(*) AS total
FROM public."Programs";

SELECT 'Tuition plan count' AS check_name, COUNT(*) AS total
FROM public."TuitionPlans";

SELECT 'Class count' AS check_name, COUNT(*) AS total
FROM public."Classes";

SELECT 'Schedule segment count' AS check_name, COUNT(*) AS total
FROM public."ClassScheduleSegments";

COMMIT;
