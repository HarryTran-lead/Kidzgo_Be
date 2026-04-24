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
-- 9. SchedulePattern is seeded only in structured `weekly-pattern` JSON.
--    Entries sharing the same startTime are grouped in one `dayOfWeeks` list.

BEGIN;

-- Prevent concurrent reset scripts from running at the same time.
SELECT pg_advisory_xact_lock(62425001);

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
        'Apple 2 lÃ  chÆ°Æ¡ng trÃ¬nh dÃ nh cho há»c viÃªn nhá» tuá»•i, thÆ°á»ng tá»« 4 Ä‘áº¿n 6 tuá»•i. ChÆ°Æ¡ng trÃ¬nh giÃºp cÃ¡c bÃ© lÃ m quen vá»›i tiáº¿ng Anh thÃ´ng qua hÃ¬nh áº£nh, Ã¢m thanh, trÃ² chÆ¡i, bÃ i hÃ¡t vÃ  cÃ¡c hoáº¡t Ä‘á»™ng tÆ°Æ¡ng tÃ¡c trong lá»›p. Má»¥c tiÃªu cá»§a chÆ°Æ¡ng trÃ¬nh lÃ  giÃºp há»c viÃªn xÃ¢y dá»±ng sá»± tá»± tin khi nghe vÃ  nÃ³i tiáº¿ng Anh, nháº­n diá»‡n tá»« vá»±ng quen thuá»™c, luyá»‡n phÃ¡t Ã¢m cÆ¡ báº£n vÃ  hÃ¬nh thÃ nh thÃ³i quen há»c tiáº¿ng Anh tÃ­ch cá»±c ngay tá»« giai Ä‘oáº¡n Ä‘áº§u.',
        FALSE),
    ('11111111-1111-1111-1111-111111111202'::uuid, 'PHONICS', 'Phonics Foundation',
        'Phonics Foundation lÃ  chÆ°Æ¡ng trÃ¬nh ná»n táº£ng phÃ¡t Ã¢m dÃ nh cho há»c viÃªn tá»« 5 Ä‘áº¿n 6 tuá»•i hoáº·c há»c viÃªn má»›i báº¯t Ä‘áº§u há»c tiáº¿ng Anh. Há»c viÃªn Ä‘Æ°á»£c há»c Ã¢m chá»¯ cÃ¡i, cÃ¡ch ghÃ©p Ã¢m, nháº­n diá»‡n máº·t chá»¯ vÃ  luyá»‡n Ä‘á»c cÃ¡c tá»« Ä‘Æ¡n giáº£n. ChÆ°Æ¡ng trÃ¬nh giÃºp há»c viÃªn phÃ¡t Ã¢m rÃµ hÆ¡n, Ä‘á»c tá»‘t hÆ¡n vÃ  cÃ³ ná»n táº£ng vá»¯ng cháº¯c trÆ°á»›c khi chuyá»ƒn sang cÃ¡c cáº¥p Ä‘á»™ Cambridge nhÆ° Starters, Movers vÃ  Flyers.',
        FALSE),
    ('11111111-1111-1111-1111-111111111203'::uuid, 'STARTERS', 'Cambridge Starters',
        'Cambridge Starters lÃ  chÆ°Æ¡ng trÃ¬nh dÃ nh cho há»c viÃªn á»Ÿ trÃ¬nh Ä‘á»™ sÆ¡ cáº¥p, thÆ°á»ng tá»« 6 Ä‘áº¿n 8 tuá»•i. Ná»™i dung há»c táº­p trung phÃ¡t triá»ƒn 4 ká»¹ nÄƒng: Listening, Speaking, Reading vÃ  Writing theo Ä‘á»‹nh hÆ°á»›ng Cambridge. Há»c viÃªn Ä‘Æ°á»£c há»c tá»« vá»±ng, máº«u cÃ¢u giao tiáº¿p cÆ¡ báº£n, luyá»‡n nghe hiá»ƒu, Ä‘á»c hiá»ƒu Ä‘Æ¡n giáº£n vÃ  lÃ m quen vá»›i cáº¥u trÃºc bÃ i thi Cambridge Starters.',
        FALSE),
    ('11111111-1111-1111-1111-111111111204'::uuid, 'MOVERS', 'Cambridge Movers',
        'Cambridge Movers lÃ  chÆ°Æ¡ng trÃ¬nh dÃ nh cho há»c viÃªn Ä‘Ã£ cÃ³ ná»n táº£ng tiáº¿ng Anh cÆ¡ báº£n vÃ  muá»‘n phÃ¡t triá»ƒn ká»¹ nÄƒng á»Ÿ má»©c cao hÆ¡n Starters. ChÆ°Æ¡ng trÃ¬nh giÃºp há»c viÃªn má»Ÿ rá»™ng vá»‘n tá»« vá»±ng, luyá»‡n giao tiáº¿p theo chá»§ Ä‘á», phÃ¡t triá»ƒn kháº£ nÄƒng Ä‘á»c hiá»ƒu, viáº¿t cÃ¢u hoáº·c Ä‘oáº¡n ngáº¯n vÃ  luyá»‡n cÃ¡c dáº¡ng bÃ i theo chuáº©n Cambridge Movers.',
        FALSE),
    ('11111111-1111-1111-1111-111111111205'::uuid, 'FLYERS', 'Cambridge Flyers',
        'Cambridge Flyers lÃ  chÆ°Æ¡ng trÃ¬nh dÃ nh cho há»c viÃªn cÃ³ trÃ¬nh Ä‘á»™ tiá»n trung cáº¥p, chuáº©n bá»‹ hoÃ n thiá»‡n cáº¥p Ä‘á»™ Young Learners cá»§a Cambridge. Ná»™i dung há»c táº­p trung nÃ¢ng cao kháº£ nÄƒng nghe, nÃ³i, Ä‘á»c, viáº¿t; sá»­ dá»¥ng ngá»¯ phÃ¡p linh hoáº¡t hÆ¡n; Ä‘á»c hiá»ƒu Ä‘oáº¡n vÄƒn dÃ i hÆ¡n vÃ  luyá»‡n ká»¹ nÄƒng lÃ m bÃ i thi Cambridge Flyers.',
        FALSE),
    ('11111111-1111-1111-1111-111111111206'::uuid, 'KETA2', 'KET (A2 Key)',
        'KET, cÃ²n gá»i lÃ  A2 Key, lÃ  chÆ°Æ¡ng trÃ¬nh dÃ nh cho há»c viÃªn á»Ÿ trÃ¬nh Ä‘á»™ A2 theo khung nÄƒng lá»±c chÃ¢u Ã‚u CEFR. Há»c viÃªn Ä‘Æ°á»£c rÃ¨n luyá»‡n kháº£ nÄƒng giao tiáº¿p trong cÃ¡c tÃ¬nh huá»‘ng quen thuá»™c, Ä‘á»c hiá»ƒu vÄƒn báº£n ngáº¯n, viáº¿t email hoáº·c ghi chÃº Ä‘Æ¡n giáº£n vÃ  luyá»‡n Ä‘á» theo cáº¥u trÃºc bÃ i thi A2 Key cá»§a Cambridge.',
        FALSE),
    ('11111111-1111-1111-1111-111111111207'::uuid, 'PETB1', 'PET (B1 Preliminary)',
        'PET, cÃ²n gá»i lÃ  B1 Preliminary, lÃ  chÆ°Æ¡ng trÃ¬nh dÃ nh cho há»c viÃªn á»Ÿ trÃ¬nh Ä‘á»™ B1, hÆ°á»›ng Ä‘áº¿n kháº£ nÄƒng sá»­ dá»¥ng tiáº¿ng Anh Ä‘á»™c láº­p trong há»c táº­p vÃ  giao tiáº¿p háº±ng ngÃ y. Há»c viÃªn Ä‘Æ°á»£c luyá»‡n ká»¹ nÄƒng Ä‘á»c hiá»ƒu vÄƒn báº£n dÃ i hÆ¡n, viáº¿t Ä‘oáº¡n vÄƒn hoáº·c email, nghe hiá»ƒu há»™i thoáº¡i thá»±c táº¿ vÃ  trÃ¬nh bÃ y Ã½ kiáº¿n báº±ng tiáº¿ng Anh theo chuáº©n bÃ i thi B1 Preliminary.',
        FALSE),
    ('11111111-1111-1111-1111-111111111208'::uuid, 'KEMLMS', 'KÃ¨m LMS',
        'KÃ¨m LMS lÃ  chÆ°Æ¡ng trÃ¬nh phá»¥ há»— trá»£ há»c viÃªn há»c thÃªm cÃ¡c mÃ´n hoáº·c ná»™i dung tiáº¿ng Anh theo nhu cáº§u cÃ¡ nhÃ¢n, vÃ­ dá»¥ ESL, Science hoáº·c chÆ°Æ¡ng trÃ¬nh há»c quá»‘c táº¿. HÃ¬nh thá»©c há»c linh hoáº¡t theo gÃ³i buá»•i, giÃºp há»c viÃªn cá»§ng cá»‘ kiáº¿n thá»©c trÃªn lá»›p, hoÃ n thÃ nh bÃ i táº­p LMS, cáº£i thiá»‡n ká»¹ nÄƒng há»c thuáº­t vÃ  theo ká»‹p chÆ°Æ¡ng trÃ¬nh Ä‘ang há»c.',
        TRUE),
    ('11111111-1111-1111-1111-111111111209'::uuid, 'SPKCLUB', 'Speaking Club',
        'Speaking Club lÃ  chÆ°Æ¡ng trÃ¬nh cÃ¢u láº¡c bá»™ giao tiáº¿p cuá»‘i tuáº§n, giÃºp há»c viÃªn tÄƒng pháº£n xáº¡ nÃ³i tiáº¿ng Anh thÃ´ng qua trÃ² chÆ¡i, tháº£o luáº­n, thuyáº¿t trÃ¬nh ngáº¯n, hoáº¡t Ä‘á»™ng nhÃ³m vÃ  cÃ¡c tÃ¬nh huá»‘ng thá»±c táº¿. Má»¥c tiÃªu cá»§a chÆ°Æ¡ng trÃ¬nh lÃ  giÃºp há»c viÃªn tá»± tin hÆ¡n khi giao tiáº¿p, cáº£i thiá»‡n phÃ¡t Ã¢m, má»Ÿ rá»™ng vá»‘n tá»« vÃ  sá»­ dá»¥ng tiáº¿ng Anh tá»± nhiÃªn hÆ¡n.',
        TRUE),
    ('11111111-1111-1111-1111-111111111210'::uuid, 'WRBOOST', 'Writing Booster',
        'Writing Booster lÃ  chÆ°Æ¡ng trÃ¬nh tÄƒng cÆ°á»ng ká»¹ nÄƒng viáº¿t, phÃ¹ há»£p vá»›i há»c viÃªn cáº§n cáº£i thiá»‡n kháº£ nÄƒng viáº¿t cÃ¢u, Ä‘oáº¡n vÄƒn, email hoáº·c bÃ i viáº¿t ngáº¯n. Há»c viÃªn Ä‘Æ°á»£c hÆ°á»›ng dáº«n cÃ¡ch xÃ¢y dá»±ng Ã½ tÆ°á»Ÿng, sá»­ dá»¥ng tá»« vá»±ng vÃ  ngá»¯ phÃ¡p phÃ¹ há»£p, sáº¯p xáº¿p ná»™i dung rÃµ rÃ ng vÃ  háº¡n cháº¿ lá»—i sai thÆ°á»ng gáº·p khi viáº¿t.',
        TRUE),
    ('11111111-1111-1111-1111-111111111211'::uuid, 'SUMCAMP', 'Summer Camp',
        'Summer Camp lÃ  chÆ°Æ¡ng trÃ¬nh hÃ¨ káº¿t há»£p giá»¯a há»c tiáº¿ng Anh vÃ  cÃ¡c hoáº¡t Ä‘á»™ng tráº£i nghiá»‡m. Há»c viÃªn Ä‘Æ°á»£c tham gia cÃ¡c hoáº¡t Ä‘á»™ng giao tiáº¿p, trÃ² chÆ¡i, dá»± Ã¡n nhÃ³m, thuyáº¿t trÃ¬nh, thá»§ cÃ´ng, ká»¹ nÄƒng sá»‘ng hoáº·c khÃ¡m phÃ¡ chá»§ Ä‘á» theo tuáº§n. ChÆ°Æ¡ng trÃ¬nh giÃºp há»c viÃªn vá»«a há»c vá»«a chÆ¡i, tÄƒng sá»± tá»± tin vÃ  duy trÃ¬ mÃ´i trÆ°á»ng tiáº¿ng Anh trong ká»³ nghá»‰ hÃ¨.',
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
    ('22222222-2222-2222-2222-222222222301'::uuid, '44444444-4444-4444-4444-444444444301'::uuid, '11111111-1111-1111-1111-111111111201'::uuid, 'APPLE-A2', 'Apple A2', 8, '{"type":"weekly-pattern","entries":[{"dayOfWeeks":["TH"],"startTime":"18:00","durationMinutes":60},{"dayOfWeeks":["SA"],"startTime":"17:00","durationMinutes":60}]}', 'Age 4-6. Thu 18:00-19:00, Sat 17:00-18:00.'),
    ('22222222-2222-2222-2222-222222222302'::uuid, '44444444-4444-4444-4444-444444444302'::uuid, '11111111-1111-1111-1111-111111111202'::uuid, 'PHONICS-P1', 'Phonics P1', 6, '{"type":"weekly-pattern","entries":[{"dayOfWeeks":["MO","WE"],"startTime":"19:00","durationMinutes":90}]}', 'Age 5-6. Mon and Wed 19:00-20:30.'),
    ('22222222-2222-2222-2222-222222222303'::uuid, '44444444-4444-4444-4444-444444444303'::uuid, '11111111-1111-1111-1111-111111111202'::uuid, 'PHONICS-P3', 'Phonics P3 + Thuyet trinh', 4, '{"type":"weekly-pattern","entries":[{"dayOfWeeks":["TU"],"startTime":"16:30","durationMinutes":90},{"dayOfWeeks":["FR"],"startTime":"17:30","durationMinutes":90}]}', 'Tue 16:30-18:00, Fri 17:30-19:00.'),
    ('22222222-2222-2222-2222-222222222304'::uuid, '44444444-4444-4444-4444-444444444304'::uuid, '11111111-1111-1111-1111-111111111203'::uuid, 'STARTERS-S1', 'Starters S1', 10, '{"type":"weekly-pattern","entries":[{"dayOfWeeks":["TH"],"startTime":"16:00","durationMinutes":90},{"dayOfWeeks":["FR"],"startTime":"19:30","durationMinutes":90}]}', 'Age 6-8. Thu 16:00-17:30, Fri 19:30-21:00.'),
    ('22222222-2222-2222-2222-222222222305'::uuid, '44444444-4444-4444-4444-444444444305'::uuid, '11111111-1111-1111-1111-111111111203'::uuid, 'STARTERS-S5', 'Starters S5', 10, '{"type":"weekly-pattern","entries":[{"dayOfWeeks":["WE"],"startTime":"19:30","durationMinutes":90},{"dayOfWeeks":["FR"],"startTime":"18:00","durationMinutes":90}]}', 'Age 6-8. Wed 19:30-21:00, Fri 18:00-19:30.'),
    ('22222222-2222-2222-2222-222222222306'::uuid, '44444444-4444-4444-4444-444444444306'::uuid, '11111111-1111-1111-1111-111111111204'::uuid, 'MOVERS-M4', 'Movers M4', 7, '{"type":"weekly-pattern","entries":[{"dayOfWeeks":["TU","TH"],"startTime":"19:30","durationMinutes":90}]}', 'Tue and Thu 19:30-21:00.'),
    ('22222222-2222-2222-2222-222222222307'::uuid, '44444444-4444-4444-4444-444444444307'::uuid, '11111111-1111-1111-1111-111111111204'::uuid, 'MOVERS-M2', 'Movers M2', 8, '{"type":"weekly-pattern","entries":[{"dayOfWeeks":["MO","WE"],"startTime":"17:30","durationMinutes":90}]}', 'Mon and Wed 17:30-19:00.'),
    ('22222222-2222-2222-2222-222222222308'::uuid, '44444444-4444-4444-4444-444444444308'::uuid, '11111111-1111-1111-1111-111111111204'::uuid, 'MOVERS-M3', 'Movers M3', 4, '{"type":"weekly-pattern","entries":[{"dayOfWeeks":["WE"],"startTime":"16:00","durationMinutes":90},{"dayOfWeeks":["SA"],"startTime":"15:30","durationMinutes":90}]}', 'Wed 16:00-17:30, Sat 15:30-17:00.'),
    ('22222222-2222-2222-2222-222222222309'::uuid, '44444444-4444-4444-4444-444444444309'::uuid, '11111111-1111-1111-1111-111111111205'::uuid, 'FLYERS-F1', 'Flyers F1', 10, '{"type":"weekly-pattern","entries":[{"dayOfWeeks":["TU"],"startTime":"18:00","durationMinutes":90},{"dayOfWeeks":["TH"],"startTime":"18:30","durationMinutes":90}]}', 'Tue 18:00-19:30, Thu 18:30-20:00.'),
    ('22222222-2222-2222-2222-222222222310'::uuid, '44444444-4444-4444-4444-444444444310'::uuid, '11111111-1111-1111-1111-111111111205'::uuid, 'FLYERS-F2', 'Flyers F2', 5, '{"type":"weekly-pattern","entries":[{"dayOfWeeks":["MO"],"startTime":"16:00","durationMinutes":90},{"dayOfWeeks":["SA"],"startTime":"17:00","durationMinutes":90}]}', 'Mon 16:00-17:30, Sat 17:00-18:30.'),
    ('22222222-2222-2222-2222-222222222311'::uuid, '44444444-4444-4444-4444-444444444311'::uuid, '11111111-1111-1111-1111-111111111207'::uuid, 'PET-K1', 'PET K1', 5, '{"type":"weekly-pattern","entries":[{"dayOfWeeks":["FR"],"startTime":"19:30","durationMinutes":90},{"dayOfWeeks":["SA"],"startTime":"10:00","durationMinutes":90}]}', 'Fri 19:30-21:00, Sat 10:00-11:30.'),
    ('22222222-2222-2222-2222-222222222312'::uuid, '44444444-4444-4444-4444-444444444312'::uuid, '11111111-1111-1111-1111-111111111208'::uuid, 'LMS-ESL-WED', 'Kem LMS - ESL Grade 3 - Cam (Wed)', 10, '{"type":"weekly-pattern","entries":[{"dayOfWeeks":["WE"],"startTime":"16:00","durationMinutes":90}]}', 'Wed 16:00-17:30.'),
    ('22222222-2222-2222-2222-222222222313'::uuid, '44444444-4444-4444-4444-444444444313'::uuid, '11111111-1111-1111-1111-111111111208'::uuid, 'LMS-ESL-THU', 'Kem LMS - ESL Grade 3 - Cam (Thu)', 10, '{"type":"weekly-pattern","entries":[{"dayOfWeeks":["TH"],"startTime":"19:30","durationMinutes":90}]}', 'Thu 19:30-21:00.'),
    ('22222222-2222-2222-2222-222222222314'::uuid, '44444444-4444-4444-4444-444444444314'::uuid, '11111111-1111-1111-1111-111111111208'::uuid, 'LMS-SCI-WED', 'Kem LMS - Science Grade 3 - Cam', 10, '{"type":"weekly-pattern","entries":[{"dayOfWeeks":["WE"],"startTime":"17:30","durationMinutes":90}]}', 'Wed 17:30-19:00.');

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
    'Active',
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
CROSS JOIN seed_context ctx
-- Chá»‰ táº¡o segments cho supplementary classes (KÃ¨m LMS, Speaking Club, etc.)
INNER JOIN public."Programs" p ON sc.program_id = p."Id" AND p."IsSupplementary" = TRUE;

SELECT 'Program count' AS check_name, COUNT(*) AS total
FROM public."Programs";

SELECT 'Tuition plan count' AS check_name, COUNT(*) AS total
FROM public."TuitionPlans";

SELECT 'Class count' AS check_name, COUNT(*) AS total
FROM public."Classes";

SELECT 'Schedule segment count' AS check_name, COUNT(*) AS total
FROM public."ClassScheduleSegments";

COMMIT;

