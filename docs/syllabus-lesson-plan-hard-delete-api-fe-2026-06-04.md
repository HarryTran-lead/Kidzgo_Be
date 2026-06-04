# Syllabus And Lesson Plan Hard Delete API FE Doc

Updated: 2026-06-04
Scope: hard delete `syllabus` va `lesson plan template`
Base paths:

- `/api/syllabuses`
- `/api/lesson-plan-templates`

Roles:

- `Admin`
- `ManagementStaff`

## 1. Muc tieu tai lieu

Tai lieu nay dung cho frontend khi can:

- xoa vinh vien 1 `lesson plan template`
- xoa vinh vien 1 `syllabus`
- xoa vinh vien ca `syllabus` lon kem tat ca `lesson plan template` va `lesson plan` thuc te ben trong

Trong pham vi API hien tai:

- FE xoa `lesson plan` bang route cua `lesson plan template`
- backend tu dong xoa cac `LessonPlan` thuc te reference template do
- khong co route rieng `DELETE /api/lesson-plans/{id}/hard-delete`

Luu y:

- `DELETE /api/lesson-plan-templates/{id}` van la soft delete
- `DELETE /api/lesson-plan-templates/{id}/hard-delete` moi la hard delete
- `DELETE /api/syllabuses/{id}/hard-delete` hien tai da la cascade hard delete that su

## 2. Envelope response

Success:

```json
{
  "isSuccess": true,
  "data": {}
}
```

Error:

```json
{
  "success": false,
  "isSuccess": false,
  "data": null,
  "message": "Cannot hard delete syllabus because it is assigned to 2 class(es).",
  "errors": [
    {
      "code": "Syllabus.HasClasses",
      "description": "Cannot hard delete syllabus because it is assigned to 2 class(es)."
    }
  ]
}
```

## 3. Hard delete lesson plan template

### `DELETE /api/lesson-plan-templates/{id}/hard-delete`

Dung khi FE can xoa vinh vien 1 lesson plan template.

Backend se:

- xoa lesson plan template khoi DB
- xoa cac `LessonPlan` thuc te dang reference template do
- null cac lien ket runtime nhu:
  - `Session.LessonPlanTemplateId`
  - `TeachingLog.PlannedLessonPlanTemplateId`
  - `TeachingLog.ActualLessonPlanTemplateId`
  - `Class.CurrentLessonPlanTemplateId`
  - `StudentProgress.CurrentLessonPlanTemplateId`
- xoa `LessonPlanUnit` neu unit do tro thanh mo coi sau khi xoa

Khong co request body.

Response mau:

```json
{
  "isSuccess": true,
  "data": {
    "id": "guid",
    "deletedLessonPlanCount": 6,
    "deletedLessonPlanUnitCount": 1
  }
}
```

Y nghia field:

- `id`: id cua template vua bi xoa
- `deletedLessonPlanCount`: so lesson plan thuc te bi xoa theo template nay
- `deletedLessonPlanUnitCount`: so unit bi xoa vi khong con template nao reference

Common errors:

- `404 LessonPlanTemplate.NotFound`
- `400 LessonPlanTemplate.Unauthorized`

FE suggestion:

1. user bam `Delete permanently`
2. FE hien confirm modal
3. goi `DELETE /api/lesson-plan-templates/{id}/hard-delete`
4. neu success:
   - remove item khoi list
   - reload `GET /api/lesson-plan-templates?...`
   - neu man dang hien chi tiet template vua xoa thi redirect ve list

## 4. Hard delete syllabus

### `DELETE /api/syllabuses/{id}/hard-delete`

Dung khi FE can xoa vinh vien 1 syllabus va toan bo lesson plan template con ben trong.

Backend se:

- tim tat ca `LessonPlanTemplates` thuoc syllabus
- xoa tat ca `LessonPlans` thuc te dang reference cac template do
- null cac reference runtime dang tro vao cac template do
- xoa cac `LessonPlanUnit` bi mo coi
- xoa `Syllabus`
- xoa cascade cac bang con cua syllabus nhu:
  - `SyllabusUnits`
  - `SyllabusLessons`
  - `SyllabusResources`
  - `SessionTemplates`
  - `CurriculumAssignments`
  - `PackageCurriculumMappings`

Khong co request body.

Response mau:

```json
{
  "isSuccess": true,
  "data": {
    "id": "guid",
    "deletedLessonPlanCount": 24,
    "deletedLessonPlanTemplateCount": 24,
    "deletedLessonPlanUnitCount": 6
  }
}
```

Y nghia field:

- `id`: id cua syllabus vua bi xoa
- `deletedLessonPlanCount`: tong so lesson plan thuc te bi xoa
- `deletedLessonPlanTemplateCount`: tong so template bi xoa trong syllabus nay
- `deletedLessonPlanUnitCount`: tong so lesson plan unit bi xoa vi mo coi

Common errors:

- `404 Syllabus.NotFound`
- `409 Syllabus.HasClasses`

Luu y quan trong:

- neu syllabus dang duoc gan cho class nao do, backend se chan hard delete
- FE nen hien thong bao ro rang la user can remove syllabus khoi class truoc

Response loi mau:

```json
{
  "success": false,
  "isSuccess": false,
  "data": null,
  "message": "Cannot hard delete syllabus because it is assigned to 1 class(es).",
  "errors": [
    {
      "code": "Syllabus.HasClasses",
      "description": "Cannot hard delete syllabus because it is assigned to 1 class(es)."
    }
  ]
}
```

FE suggestion:

1. user bam `Delete permanently`
2. FE hien warning ro:
   - xoa syllabus nay se xoa luon lesson plan template
   - xoa luon lesson plan thuc te dang reference template
   - khong the undo
3. goi `DELETE /api/syllabuses/{id}/hard-delete`
4. neu success:
   - dong modal
   - navigate ve list syllabus
   - reload `GET /api/syllabuses`

## 5. Soft delete va hard delete khac nhau the nao

### Lesson plan template

Soft delete:

- API: `DELETE /api/lesson-plan-templates/{id}`
- khong xoa row khoi DB
- set `IsDeleted = true`, `IsActive = false`
- chi dung khi can an template khoi he thong nhung van giu data

Hard delete:

- API: `DELETE /api/lesson-plan-templates/{id}/hard-delete`
- xoa vinh vien row khoi DB
- xoa luon lesson plan thuc te reference template do

### Syllabus

Hard delete:

- API: `DELETE /api/syllabuses/{id}/hard-delete`
- xoa vinh vien syllabus
- xoa luon lesson plan template va lesson plan ben trong

## 6. Goi y UX cho FE

- Nen dung hard delete chi trong man admin/staff.
- Nen bat user confirm 2 lan neu dang xoa syllabus.
- Nen dung text canh bao:
  - `This action cannot be undone.`
  - `All lesson plan templates and generated lesson plans inside this syllabus will be permanently deleted.`
- Sau khi xoa thanh cong, FE nen reload list thay vi update local state mot cach thu cong neu man co nhieu tab/phu thuoc cheo.

## 7. Ket luan nhanh

Neu FE can xoa 1 lesson plan template:

- goi `DELETE /api/lesson-plan-templates/{id}/hard-delete`

Neu FE can xoa ca syllabus lon kem lesson plan template va lesson plan ben trong:

- goi `DELETE /api/syllabuses/{id}/hard-delete`
