-- Seed 10 classrooms for PostgreSQL
-- Tất cả room đều thuộc branch:
-- da316382-35e8-4094-a99b-ce45e5f2627a

WITH seed_rooms AS (
    SELECT *
    FROM (
        VALUES
            ('91000000-0000-0000-0000-000000000001'::uuid, 'Room A101', 12, 'Standard classroom for small groups', 'Floor 1', 24.5::numeric, TRUE),
            ('91000000-0000-0000-0000-000000000002'::uuid, 'Room A102', 14, 'Standard classroom', 'Floor 1', 26.0::numeric, TRUE),
            ('91000000-0000-0000-0000-000000000003'::uuid, 'Room A103', 16, 'Classroom for kids classes', 'Floor 1', 28.5::numeric, TRUE),
            ('91000000-0000-0000-0000-000000000004'::uuid, 'Room A104', 18, 'Spacious classroom for larger groups', 'Floor 1', 31.0::numeric, TRUE),
            ('91000000-0000-0000-0000-000000000005'::uuid, 'Room B201', 12, 'Classroom focused on speaking activities', 'Floor 2', 23.0::numeric, TRUE),
            ('91000000-0000-0000-0000-000000000006'::uuid, 'Room B202', 15, 'Multi-purpose classroom', 'Floor 2', 27.5::numeric, TRUE),
            ('91000000-0000-0000-0000-000000000007'::uuid, 'Room B203', 20, 'Large room for small workshops', 'Floor 2', 35.0::numeric, TRUE),
            ('91000000-0000-0000-0000-000000000008'::uuid, 'Room C301', 10, 'Classroom for one-on-one or small groups', 'Floor 3', 18.5::numeric, TRUE),
            ('91000000-0000-0000-0000-000000000009'::uuid, 'Room C302', 14, 'Classroom for writing support', 'Floor 3', 25.0::numeric, TRUE),
            ('91000000-0000-0000-0000-000000000010'::uuid, 'Room C303', 22, 'Large room for clubs or small events', 'Floor 3', 40.0::numeric, TRUE)
    ) AS v(id, name, capacity, note, floor, area, is_active)
)
INSERT INTO public."Classrooms"
(
    "Id",
    "BranchId",
    "Name",
    "Capacity",
    "Note",
    "Floor",
    "Area",
    "IsActive"
)
SELECT
    r.id,
    'da316382-35e8-4094-a99b-ce45e5f2627a'::uuid,
    r.name,
    r.capacity,
    r.note,
    r.floor,
    r.area,
    r.is_active
FROM seed_rooms r
ON CONFLICT ("Id") DO UPDATE
SET
    "BranchId" = EXCLUDED."BranchId",
    "Name" = EXCLUDED."Name",
    "Capacity" = EXCLUDED."Capacity",
    "Note" = EXCLUDED."Note",
    "Floor" = EXCLUDED."Floor",
    "Area" = EXCLUDED."Area",
    "IsActive" = EXCLUDED."IsActive";

SELECT
    "Id",
    "BranchId",
    "Name",
    "Capacity",
    "Floor",
    "Area",
    "IsActive"
FROM public."Classrooms"
WHERE "Id" IN (
    '91000000-0000-0000-0000-000000000001'::uuid,
    '91000000-0000-0000-0000-000000000002'::uuid,
    '91000000-0000-0000-0000-000000000003'::uuid,
    '91000000-0000-0000-0000-000000000004'::uuid,
    '91000000-0000-0000-0000-000000000005'::uuid,
    '91000000-0000-0000-0000-000000000006'::uuid,
    '91000000-0000-0000-0000-000000000007'::uuid,
    '91000000-0000-0000-0000-000000000008'::uuid,
    '91000000-0000-0000-0000-000000000009'::uuid,
    '91000000-0000-0000-0000-000000000010'::uuid
)
ORDER BY "Name";
