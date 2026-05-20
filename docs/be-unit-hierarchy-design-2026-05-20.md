# BE Design: Unit Hierarchy - Program -> Level -> Module -> Unit -> Lesson

> Created: 2026-05-20  
> Goal: Stop grouping lesson plans by parsing `Title` on FE. Add a real Unit entity in BE so FE can use stable IDs for CRUD, reorder, and rendering.

---

## 1. Current Problem

FE currently groups lesson plan templates by parsing strings:

```text
"UNIT 03: FAMILY AND FRIENDS - Lesson 1" -> "UNIT 03: FAMILY AND FRIENDS"
"UNIT 3: FAMILY AND FRIENDS - Lesson 2"  -> "UNIT 3: FAMILY AND FRIENDS"
```

Because `LessonPlanTemplates` has no real unit FK:

- Same unit can be duplicated because names differ by zero-padding, case, or punctuation.
- FE cannot CRUD units safely.
- FE cannot drag-drop reorder units or lessons inside a unit.
- Import logic cannot guarantee a canonical Unit -> Lesson hierarchy.

---

## 2. Design Decision

Create a new domain entity named `LessonPlanUnit`.

Do not reuse `SyllabusUnit`. `SyllabusUnit` is parsed syllabus structure. `LessonPlanUnit` is the stable grouping entity for imported lesson plan templates.

Hierarchy:

```text
Program
  -> Level
    -> Module
      -> LessonPlanUnit
        -> LessonPlanTemplate
```

Existing entities stay valid:

- `Module` remains under `Level`.
- `LessonPlanTemplate.ModuleId` remains required.
- `LessonPlanTemplate.SessionTemplateId` remains optional one-to-one.
- New `LessonPlanTemplate.LessonPlanUnitId` is optional to support old/orphan data.

---

## 3. Database Migration

### 3.1 New Table: `LessonPlanUnits`

Use current project naming style: PascalCase table and column names in schema `public`.

```csharp
migrationBuilder.CreateTable(
    name: "LessonPlanUnits",
    schema: "public",
    columns: table => new
    {
        Id = table.Column<Guid>(type: "uuid", nullable: false),
        ModuleId = table.Column<Guid>(type: "uuid", nullable: false),
        Name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
        NameNormalized = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
        OrderIndex = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
        IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
        CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
        UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
    },
    constraints: table =>
    {
        table.PrimaryKey("PK_LessonPlanUnits", x => x.Id);
        table.ForeignKey(
            name: "FK_LessonPlanUnits_Modules_ModuleId",
            column: x => x.ModuleId,
            principalSchema: "public",
            principalTable: "Modules",
            principalColumn: "Id",
            onDelete: ReferentialAction.Cascade);
    });

migrationBuilder.CreateIndex(
    name: "IX_LessonPlanUnits_ModuleId_NameNormalized",
    schema: "public",
    table: "LessonPlanUnits",
    columns: new[] { "ModuleId", "NameNormalized" },
    unique: true);

migrationBuilder.CreateIndex(
    name: "IX_LessonPlanUnits_ModuleId_OrderIndex",
    schema: "public",
    table: "LessonPlanUnits",
    columns: new[] { "ModuleId", "OrderIndex" });
```

### 3.2 Update `LessonPlanTemplates`

```csharp
migrationBuilder.AddColumn<Guid>(
    name: "LessonPlanUnitId",
    schema: "public",
    table: "LessonPlanTemplates",
    type: "uuid",
    nullable: true);

migrationBuilder.AddColumn<int>(
    name: "OrderIndexInUnit",
    schema: "public",
    table: "LessonPlanTemplates",
    type: "integer",
    nullable: false,
    defaultValue: 0);

migrationBuilder.CreateIndex(
    name: "IX_LessonPlanTemplates_LessonPlanUnitId",
    schema: "public",
    table: "LessonPlanTemplates",
    column: "LessonPlanUnitId");

migrationBuilder.AddForeignKey(
    name: "FK_LessonPlanTemplates_LessonPlanUnits_LessonPlanUnitId",
    schema: "public",
    table: "LessonPlanTemplates",
    column: "LessonPlanUnitId",
    principalSchema: "public",
    principalTable: "LessonPlanUnits",
    principalColumn: "Id",
    onDelete: ReferentialAction.SetNull);
```

### 3.3 Delete Rules

| Deleted Entity | Behavior |
|---|---|
| Module | Cascade deletes `LessonPlanUnits`. Existing module deletion is already constrained by other relations. |
| LessonPlanUnit | Set `LessonPlanTemplates.LessonPlanUnitId = null`. Do not delete lesson plan templates. |
| LessonPlanTemplate | No effect on unit. |

---

## 4. Domain Model

### 4.1 New Entity

```csharp
namespace Kidzgo.Domain.LessonPlans;

public sealed class LessonPlanUnit
{
    public Guid Id { get; set; }
    public Guid ModuleId { get; set; }
    public string Name { get; set; } = null!;
    public string NameNormalized { get; set; } = null!;
    public int OrderIndex { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public Module Module { get; set; } = null!;
    public ICollection<LessonPlanTemplate> LessonPlanTemplates { get; set; } = [];
}
```

### 4.2 Update `Module`

```csharp
public ICollection<LessonPlanUnit> LessonPlanUnits { get; set; } = [];
```

### 4.3 Update `LessonPlanTemplate`

```csharp
public Guid? LessonPlanUnitId { get; set; }
public int OrderIndexInUnit { get; set; }
public LessonPlanUnit? LessonPlanUnit { get; set; }
```

### 4.4 DbContext

```csharp
public DbSet<LessonPlanUnit> LessonPlanUnits => Set<LessonPlanUnit>();
```

---

## 5. EF Configuration

```csharp
public sealed class LessonPlanUnitConfiguration : IEntityTypeConfiguration<LessonPlanUnit>
{
    public void Configure(EntityTypeBuilder<LessonPlanUnit> builder)
    {
        builder.ToTable("LessonPlanUnits", "public");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(x => x.NameNormalized)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(x => x.OrderIndex)
            .HasDefaultValue(0);

        builder.Property(x => x.IsActive)
            .HasDefaultValue(true);

        builder.HasIndex(x => new { x.ModuleId, x.NameNormalized })
            .IsUnique();

        builder.HasIndex(x => new { x.ModuleId, x.OrderIndex });

        builder.HasOne(x => x.Module)
            .WithMany(x => x.LessonPlanUnits)
            .HasForeignKey(x => x.ModuleId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
```

Update `LessonPlanTemplateConfiguration`:

```csharp
builder.Property(x => x.OrderIndexInUnit)
    .HasDefaultValue(0);

builder.HasOne(x => x.LessonPlanUnit)
    .WithMany(x => x.LessonPlanTemplates)
    .HasForeignKey(x => x.LessonPlanUnitId)
    .OnDelete(DeleteBehavior.SetNull);

builder.HasIndex(x => x.LessonPlanUnitId);
builder.HasIndex(x => new { x.LessonPlanUnitId, x.OrderIndexInUnit });
```

Do not remove current unique index:

```csharp
builder.HasIndex(x => new { x.ModuleId, x.SessionIndex }).IsUnique();
```

It is still the overwrite key for imports.

---

## 6. Normalize Utility

Use one C# utility everywhere: import, CRUD, backfill, tests.

```csharp
using System.Text.RegularExpressions;

public static partial class LessonPlanUnitNameNormalizer
{
    public static string Normalize(string raw)
    {
        if (string.IsNullOrWhiteSpace(raw))
        {
            return string.Empty;
        }

        var value = raw.Trim().ToUpperInvariant();
        value = StripLeadingZeroRegex().Replace(value, match => int.Parse(match.Value).ToString());
        value = WhitespaceRegex().Replace(value, " ");
        return value.Trim();
    }

    public static string? ExtractUnitName(string? title)
    {
        if (string.IsNullOrWhiteSpace(title))
        {
            return null;
        }

        var match = LessonSuffixRegex().Match(title);
        if (match.Success)
        {
            return Normalize(match.Groups["unit"].Value);
        }

        var unitMatch = UnitNameRegex().Match(title);
        if (unitMatch.Success)
        {
            return Normalize(unitMatch.Value);
        }

        var revisionMatch = RevisionRegex().Match(title);
        return revisionMatch.Success ? Normalize(revisionMatch.Value) : null;
    }

    [GeneratedRegex(@"\b0+\d+\b")]
    private static partial Regex StripLeadingZeroRegex();

    [GeneratedRegex(@"\s+")]
    private static partial Regex WhitespaceRegex();

    [GeneratedRegex(@"^(?<unit>.+?)\s*[-–]\s*Lesson\s*\d+", RegexOptions.IgnoreCase)]
    private static partial Regex LessonSuffixRegex();

    [GeneratedRegex(@"\bUNIT\s+(STARTER|0*\d+)\b[^-–]*", RegexOptions.IgnoreCase)]
    private static partial Regex UnitNameRegex();

    [GeneratedRegex(@"\bREVISION\s+0*\d+\b", RegexOptions.IgnoreCase)]
    private static partial Regex RevisionRegex();
}
```

Examples:

| Raw | Normalized |
|---|---|
| `UNIT 03: FAMILY AND FRIENDS - Lesson 1` | `UNIT 3: FAMILY AND FRIENDS` |
| `Unit starter Hello lesson 2 done.docx` | `UNIT STARTER HELLO LESSON 2 DONE.DOCX` if used as raw name, so prefer parsed title when available |
| `REVISION 01 - Lesson 2` | `REVISION 1` |

Important: for imports, prefer the parsed lesson title from Word content. Use filename only as fallback.

---

## 7. Import Logic

### 7.1 Integration Points

Update these handlers:

- `Kidzgo.Application/LessonPlanTemplates/ImportLessonPlanTemplateFromWord/ImportLessonPlanTemplateFromWordCommandHandler.cs`
- `Kidzgo.Application/Syllabuses/ImportCurriculumArchive/ImportCurriculumArchiveCommandHandler.cs`

Flow per imported lesson plan:

1. Resolve `ModuleId` using existing import configuration.
2. Resolve `SessionIndex` using existing import configuration.
3. Extract unit name from parsed Word title/content.
4. Find or create `LessonPlanUnit` in the resolved module.
5. Upsert `LessonPlanTemplate` using existing `(ModuleId, SessionIndex)` unique key.
6. Set `LessonPlanTemplate.LessonPlanUnitId`.
7. Set `OrderIndexInUnit` from lesson number if available, otherwise append to end.

### 7.2 Find Or Create Unit

```csharp
private async Task<LessonPlanUnit> FindOrCreateUnitAsync(
    Guid moduleId,
    string rawName,
    CancellationToken cancellationToken)
{
    var normalized = LessonPlanUnitNameNormalizer.Normalize(rawName);

    var existing = await dbContext.LessonPlanUnits
        .FirstOrDefaultAsync(x => x.ModuleId == moduleId && x.NameNormalized == normalized, cancellationToken);

    if (existing is not null)
    {
        return existing;
    }

    var nextOrder = await dbContext.LessonPlanUnits
        .Where(x => x.ModuleId == moduleId)
        .Select(x => (int?)x.OrderIndex)
        .MaxAsync(cancellationToken) ?? -1;

    var unit = new LessonPlanUnit
    {
        Id = Guid.NewGuid(),
        ModuleId = moduleId,
        Name = normalized,
        NameNormalized = normalized,
        OrderIndex = nextOrder + 1,
        IsActive = true,
        CreatedAt = DateTime.UtcNow,
        UpdatedAt = DateTime.UtcNow
    };

    dbContext.LessonPlanUnits.Add(unit);
    return unit;
}
```

Concurrency note: unique index `(ModuleId, NameNormalized)` is the final guard. If two imports create the same unit concurrently, catch duplicate-key error and reload existing unit.

### 7.3 Order In Unit

Preferred source:

1. Lesson number from title or filename: `Lesson 1`, `lesson 2`, `lesson 3`.
2. Current count in that unit.

```csharp
var lessonNumber = ExtractLessonNumber(titleOrFileName);
template.OrderIndexInUnit = lessonNumber.HasValue
    ? lessonNumber.Value - 1
    : await GetNextOrderInUnitAsync(unit.Id, cancellationToken);
```

---

## 8. Backfill Existing Data

Use a one-time C# backfill command/service, not a TypeScript script.

Backfill rules:

1. Select `LessonPlanTemplates` where `LessonPlanUnitId IS NULL`.
2. Skip rows without `ModuleId`.
3. Extract unit name from `Title`, then `SyllabusMetadata`, then `SourceFileName`.
4. Find or create `LessonPlanUnit`.
5. Assign `LessonPlanUnitId`.
6. Recalculate `OrderIndexInUnit` per unit.

PostgreSQL reorder SQL after assigning IDs:

```sql
UPDATE public."LessonPlanTemplates" lpt
SET "OrderIndexInUnit" = sub.rn - 1
FROM (
    SELECT "Id",
           ROW_NUMBER() OVER (
               PARTITION BY "LessonPlanUnitId"
               ORDER BY "SessionIndex", "SessionOrder", "CreatedAt"
           ) AS rn
    FROM public."LessonPlanTemplates"
    WHERE "LessonPlanUnitId" IS NOT NULL
) sub
WHERE lpt."Id" = sub."Id";
```

---

## 9. API Endpoints

### 9.1 Unit CRUD

| Method | Path | Purpose |
|---|---|---|
| `GET` | `/api/modules/{moduleId}/lesson-plan-units` | List units in module |
| `POST` | `/api/modules/{moduleId}/lesson-plan-units` | Create unit |
| `PATCH` | `/api/lesson-plan-units/{unitId}` | Rename or activate/deactivate unit |
| `DELETE` | `/api/lesson-plan-units/{unitId}` | Delete empty unit |
| `PATCH` | `/api/modules/{moduleId}/lesson-plan-units/reorder` | Bulk reorder units |

`GET /api/modules/{moduleId}/lesson-plan-units`

```json
{
  "isSuccess": true,
  "data": [
    {
      "id": "uuid",
      "moduleId": "uuid",
      "name": "UNIT 1: I LOVE ANIMALS",
      "orderIndex": 0,
      "lessonCount": 3,
      "isActive": true
    }
  ]
}
```

`POST /api/modules/{moduleId}/lesson-plan-units`

```json
{
  "name": "UNIT 6: NEW UNIT"
}
```

`PATCH /api/lesson-plan-units/{unitId}`

```json
{
  "name": "UNIT 6: UPDATED UNIT",
  "isActive": true
}
```

`PATCH /api/modules/{moduleId}/lesson-plan-units/reorder`

```json
[
  { "id": "uuid-1", "orderIndex": 0 },
  { "id": "uuid-2", "orderIndex": 1 },
  { "id": "uuid-3", "orderIndex": 2 }
]
```

### 9.2 Lessons Inside Unit

| Method | Path | Purpose |
|---|---|---|
| `PATCH` | `/api/lesson-plan-templates/{id}/unit` | Move lesson to another unit |
| `PATCH` | `/api/lesson-plan-units/{unitId}/lessons/reorder` | Reorder lessons in unit |

`PATCH /api/lesson-plan-templates/{id}/unit`

```json
{
  "lessonPlanUnitId": "uuid-target-unit",
  "orderIndexInUnit": 2
}
```

`lessonPlanUnitId = null` means orphan the lesson from unit.

`PATCH /api/lesson-plan-units/{unitId}/lessons/reorder`

```json
[
  { "id": "template-uuid-1", "orderIndexInUnit": 0 },
  { "id": "template-uuid-2", "orderIndexInUnit": 1 },
  { "id": "template-uuid-3", "orderIndexInUnit": 2 }
]
```

---

## 10. Update Existing Hierarchy API

Keep the route:

```http
GET /api/syllabuses/{syllabusId}/unit-lesson-plans
```

Change the implementation to use `LessonPlanUnitId`, not parsed title grouping.

Expected response:

```json
{
  "isSuccess": true,
  "data": {
    "syllabusId": "uuid",
    "totalTemplates": 50,
    "groups": [
      {
        "moduleId": "uuid",
        "moduleName": "Stater01",
        "moduleCode": "STATERS_STATER01",
        "orderIndex": 1,
        "units": [
          {
            "unitId": "uuid",
            "unitName": "UNIT STARTER: HELLO",
            "orderIndex": 0,
            "lessonCount": 2,
            "lessons": [
              {
                "id": "uuid",
                "title": "UNIT STARTER: HELLO - Lesson 1",
                "sessionIndex": 1,
                "sessionOrder": 1,
                "orderIndexInUnit": 0,
                "sourceFileName": "Unit starter hello lesson 1 done.docx",
                "attachmentUrl": null,
                "isActive": true
              }
            ]
          }
        ]
      }
    ],
    "orphanLessons": []
  }
}
```

Implementation filter should be based on existing syllabus/session relation:

- Join `LessonPlanTemplates` to `SessionTemplates` through `SessionTemplateId`.
- Filter `SessionTemplates.SyllabusId == syllabusId`.
- Include `LessonPlanUnit` and `Module`.
- Return orphan lessons separately if `LessonPlanUnitId` is null.

Do not assume `Modules` has `SyllabusId`. In the current schema, module is level-owned, not syllabus-owned.

---

## 11. Validation Rules

| Rule | Detail |
|---|---|
| Unique unit name in module | Use `NameNormalized` unique index. |
| Reorder units only inside same module | Reject IDs that do not belong to route `moduleId`. |
| Delete unit with lessons | Return `409 Conflict`; FE must move lessons first. |
| Move lesson to unit in another module | Reject with `400 Bad Request`; lesson and unit must share `ModuleId`. |
| `OrderIndex` | Must be `>= 0`. |
| `OrderIndexInUnit` | Must be `>= 0`. |
| Import overwrite | Keep existing `(ModuleId, SessionIndex)` upsert/update behavior. |
| Old data | Allow `LessonPlanUnitId = null` until backfill completes. |

---

## 12. Rollout Order

1. Add `LessonPlanUnit` entity and EF configuration.
2. Add `DbSet<LessonPlanUnit>`.
3. Add migration for `LessonPlanUnits`, `LessonPlanUnitId`, and `OrderIndexInUnit`.
4. Add normalizer/extractor utility.
5. Update Word import and zip import to find/create unit and assign lessons.
6. Add one-time backfill command for old `LessonPlanTemplates`.
7. Add Unit CRUD APIs.
8. Add lesson move/reorder APIs.
9. Update `GET /api/syllabuses/{syllabusId}/unit-lesson-plans`.
10. Update FE to stop parsing title strings.

---

## 13. FE Changes

FE should update types:

```ts
type LessonPlanTemplate = {
  id: string;
  title: string;
  moduleId: string;
  lessonPlanUnitId?: string | null;
  unitName?: string | null;
  sessionIndex: number;
  sessionOrder?: number | null;
  orderIndexInUnit: number;
};
```

Rendering rules:

- Stop using `extractUnitGroup(title)`.
- Use `GET /api/syllabuses/{syllabusId}/unit-lesson-plans`.
- Render by `moduleId -> unitId -> lessons`.
- Use reorder APIs for drag-drop.
- Show `orphanLessons` separately so bad imports are visible.

---

## 14. Non-Goals

- This design does not replace `SyllabusUnits`.
- This design does not change class/session scheduling.
- This design does not require deleting old lesson plans during import.
- This design does not infer module ranges from FE after import; import configuration remains the source for module/session resolution.

