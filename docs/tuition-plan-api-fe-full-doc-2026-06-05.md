# Tuition Plan API FE Full Doc

Last updated: `2026-06-05`

## 1. Muc tieu tai lieu

Tai lieu nay mo ta day du contract FE can dung cho luong `Tuition Plan` sau khi da chuyen sang model:

- `TuitionPlan` co the la goi thuong, khong gan syllabus/module.
- `TuitionPlan` co the la goi ban theo `syllabus`, chon `1..n` module lien tiep.
- FE khong gui `moduleId` nua. Contract moi chi dung `syllabusId + moduleIds[]`.
- Backend van giu `TuitionPlan.ModuleId` trong DB de lam `start module` noi bo, nhung FE khong can quan tam field nay.

## 2. Tom tat nghiep vu

### 2.1 Hai loai tuition plan

1. `Goi thuong`

- Khong co `syllabusId`
- Khong co `moduleIds[]`
- FE nhap `totalSessions`

2. `Goi theo syllabus/module`

- Bat buoc co `syllabusId`
- Bat buoc co `moduleIds[]`
- `moduleIds[]` phai thuoc syllabus da chon
- `moduleIds[]` phai la mot dai lien tiep theo thu tu module
- `totalSessions` phai bang tong `plannedSessionCount` cua cac module da chon

### 2.2 Luu y quan trong cho FE

- Request create/update `TuitionPlan` da bo `moduleId`.
- Response create/update/get/list `TuitionPlan` chi dung `moduleIds` va `modules`.
- `moduleIds[]` la source of truth.
- Neu FE muon hien thi module theo syllabus, nen derive module theo dung logic backend:
  - lay `GET /api/syllabuses/{id}`
  - lay `GET /api/modules?levelId=...`
  - union `moduleId` tu `sessionTemplates`, `units`, `lessons`
  - join voi danh sach module de lay `order`, `plannedSessionCount`, `name`, `code`

## 3. Moi role duoc xem du lieu gi

### 3.1 Tong quan theo role

| Role | Xem tuition plan | Tao/sua tuition plan | Toggle status | Xoa mem | Xem lookup program/level/module/syllabus | Pham vi |
| --- | --- | --- | --- | --- | --- | --- |
| `Admin` | Co | Co | Co | Co | Co | `all` |
| `ManagementStaff` | Co | Co | Co | Khong | Co | `all` |
| `Teacher` | Khong xem CRUD tuition plan | Khong | Khong | Khong | Chi xem `program`, `syllabus` theo cac API cho phep | `all` |
| `Anonymous` | Chi xem `GET /api/tuition-plans/active` | Khong | Khong | Khong | Co the xem `GET /api/programs/active`, `GET /api/programs/{id}` | Chi du lieu active/public |

### 3.2 Pham vi du lieu

- Hien tai backend **khong co co che own / department / branch theo actor** cho luong tuition plan.
- `Admin` va `ManagementStaff` nhin duoc `all`.
- Neu FE muon gioi han theo chi nhanh, dung query param `branchId` o API list.
- `branchId` la filter nghiep vu, **khong phai data permission boundary**.

## 4. Permission Matrix Theo Role

| API | Admin | ManagementStaff | Teacher | Anonymous |
| --- | --- | --- | --- | --- |
| `GET /api/tuition-plans` | Co | Co | Khong | Khong |
| `GET /api/tuition-plans/active` | Co | Co | Co | Co |
| `GET /api/tuition-plans/{id}` | Co | Co | Khong | Khong |
| `POST /api/tuition-plans` | Co | Co | Khong | Khong |
| `PUT /api/tuition-plans/{id}` | Co | Co | Khong | Khong |
| `PATCH /api/tuition-plans/{id}/toggle-status` | Co | Co | Khong | Khong |
| `DELETE /api/tuition-plans/{id}` | Co | Khong | Khong | Khong |
| `GET /api/packages/{id}/syllabuses` | Co | Co | Khong | Khong |
| `POST /api/package-curriculum-mappings` | Co | Co | Khong | Khong |
| `GET /api/programs` | Co | Co | Co | Khong |
| `GET /api/programs/active` | Co | Co | Co | Co |
| `GET /api/programs/{id}` | Co | Co | Co | Co |
| `GET /api/levels` | Co | Co | Khong | Khong |
| `GET /api/modules` | Co | Co | Khong | Khong |
| `GET /api/syllabuses` | Co | Co | Co | Khong |
| `GET /api/syllabuses/{id}` | Co | Co | Co | Khong |

## 5. Flow FE Khuyen Nghi

1. Goi `GET /api/programs` hoac `GET /api/programs/active`.
2. Khi chon `program`, goi `GET /api/levels?programId=...`.
3. Khi chon `level`, goi `GET /api/syllabuses?programId=...&levelId=...&isActive=true`.
4. Khi chon `syllabus`, goi:
   - `GET /api/syllabuses/{id}` de lay cac nguon module trong syllabus
   - `GET /api/modules?levelId=...&isActive=true` de lay `order` va `plannedSessionCount`
5. FE build danh sach module hop le:
   - union `moduleId` tu `sessionTemplates`, `units`, `lessons`
   - join voi `modules`
   - sort tang dan theo `module.order`
6. FE cho nguoi dung chon mot dai module lien tiep.
7. FE tinh `totalSessions = sum(plannedSessionCount)` cua modules da chon.
8. FE goi `POST /api/tuition-plans` hoac `PUT /api/tuition-plans/{id}`.

## 6. Format Response Chung

### 6.1 Success format

Tat ca API `MatchOk()` va `MatchCreated()` deu tra wrapper:

```json
{
  "isSuccess": true,
  "data": {}
}
```

### 6.2 Error format thong thuong

Backend tra `ProblemDetails`:

```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.1",
  "title": "TuitionPlan.SyllabusRequiredForModuleSelection",
  "status": 400,
  "detail": "Syllabus is required when selecting modules for a tuition plan.",
  "errors": [
    {
      "code": "TuitionPlan.SyllabusRequiredForModuleSelection",
      "description": "Syllabus is required when selecting modules for a tuition plan."
    }
  ]
}
```

### 6.3 Error format validation pipeline

Khi fail FluentValidation, `title` se la `Validation.General`, `detail` se la `One or more validation errors occurred`, va `errors[]` chua danh sach loi field.

### 6.4 Error format dac biet khi block deactivate

Case toggle status bi chan do entity dang duoc su dung:

```json
{
  "success": false,
  "code": "STATUS_CHANGE_BLOCKED",
  "message": "Cannot deactivate because the entity is currently in use.",
  "details": {
    "entity": "TuitionPlan",
    "entityId": "guid",
    "reasons": ["ACTIVE_ENROLLMENTS_EXIST"],
    "counts": {
      "activeEnrollments": 3
    }
  }
}
```

## 7. Status Definition

### 7.1 TuitionPlan status

`TuitionPlan` hien tai khong dung enum status rieng. FE nen map theo 2 field:

| Trang thai FE | Dieu kien |
| --- | --- |
| `Active` | `isActive = true` va `isDeleted = false` |
| `Inactive` | `isActive = false` va `isDeleted = false` |
| `Deleted` | `isDeleted = true` |

Luu y:

- API list/detail binh thuong khong tra `isDeleted`.
- Record `Deleted` duoc coi la soft deleted, khong hien ra o luong binh thuong.

### 7.2 Curriculum mapping status

Quan he `TuitionPlan - Syllabus` dung `PackageCurriculumMapping.IsActive`.

| Trang thai | Y nghia |
| --- | --- |
| `IsActive = true` | Syllabus dang duoc gan active cho tuition plan |
| `IsActive = false` | Mapping cu, da bi deactivate |

## 8. Luong Chuyen Trang Thai

### 8.1 TuitionPlan

1. `Create`
   - Backend tao moi voi `isActive = true`
2. `Toggle status`
   - `Active -> Inactive`
   - `Inactive -> Active`
3. `Delete`
   - `Active/Inactive -> Deleted`
   - Khi delete, backend set ca `isDeleted = true` va `isActive = false`

### 8.2 Rule chan chuyen trang thai

- Khong the `deactivate` neu tuition plan dang co `active` hoac `paused` enrollment.
- Khong the `delete` neu tuition plan dang co `active` hoac `paused` enrollment.

## 9. Danh Sach API

## 9.1 API lookup cho FE

### API 1. Get Programs

- Endpoint + Method: `GET /api/programs`
- Muc dich: Lay danh sach program cho man hinh tao/sua tuition plan.
- Auth: `Admin`, `ManagementStaff`, `Teacher`
- Params:

| Field | Type | Required | Note |
| --- | --- | --- | --- |
| `branchId` | `guid?` | Khong | Filter program theo chi nhanh |
| `searchTerm` | `string?` | Khong | Tim theo `name/code/description` |
| `isActive` | `bool?` | Khong | Filter active/inactive |
| `isMakeup` | `bool?` | Khong | Filter loai program |
| `pageNumber` | `int` | Khong | Default `1` |
| `pageSize` | `int` | Khong | Default `10` |

- Response success:

```json
{
  "isSuccess": true,
  "data": {
    "programs": {
      "items": [
        {
          "id": "guid",
          "name": "Kids A",
          "code": "KA",
          "isMakeup": false,
          "isSupplementary": false,
          "description": "string",
          "isActive": true,
          "assignedBranchCount": 2,
          "baseFee": 1000000,
          "fee": 1000000,
          "classCount": 10,
          "studentCount": 120,
          "status": "Active"
        }
      ],
      "pageNumber": 1,
      "totalPages": 1,
      "totalCount": 1,
      "hasPreviousPage": false,
      "hasNextPage": false
    }
  }
}
```

- Response error:
  - `401` neu khong dang nhap
  - `403` neu role khong duoc phep

### API 2. Get Active Programs

- Endpoint + Method: `GET /api/programs/active`
- Muc dich: Lay program active, khong can auth.
- Auth: `Anonymous`
- Params: giong `GET /api/programs`, tru `isActive` duoc hardcode = `true`.
- Response success: giong `GET /api/programs`.
- Response error: thuong chi co `400` neu query invalid.

### API 3. Get Levels

- Endpoint + Method: `GET /api/levels`
- Muc dich: Lay levels theo program da chon.
- Auth: `Admin`, `ManagementStaff`
- Params:

| Field | Type | Required | Note |
| --- | --- | --- | --- |
| `programId` | `guid?` | Khong | Nen gui khi build tuition plan |
| `isActive` | `bool?` | Khong | Filter active/inactive |
| `searchTerm` | `string?` | Khong | Tim theo `code/name` |

- Response success:

```json
{
  "isSuccess": true,
  "data": {
    "items": [
      {
        "id": "guid",
        "programId": "guid",
        "code": "L1",
        "name": "Level 1",
        "order": 1,
        "description": "string",
        "isActive": true
      }
    ]
  }
}
```

- Response error:
  - `401`, `403`

### API 4. Get Modules

- Endpoint + Method: `GET /api/modules`
- Muc dich: Lay module theo level de FE co `order`, `plannedSessionCount`, `code`, `name`.
- Auth: `Admin`, `ManagementStaff`
- Params:

| Field | Type | Required | Note |
| --- | --- | --- | --- |
| `levelId` | `guid?` | Khong | Nen gui khi chon level |
| `isActive` | `bool?` | Khong | Filter active/inactive |
| `searchTerm` | `string?` | Khong | Tim theo `code/name` |

- Response success:

```json
{
  "isSuccess": true,
  "data": {
    "items": [
      {
        "id": "guid",
        "levelId": "guid",
        "levelCode": "L1",
        "code": "M1",
        "name": "Module 1",
        "order": 1,
        "description": "string",
        "plannedSessionCount": 12,
        "lessonPlanCount": 12,
        "isActive": true
      }
    ]
  }
}
```

- Response error:
  - `401`, `403`

### API 5. Get Syllabuses

- Endpoint + Method: `GET /api/syllabuses`
- Muc dich: Lay danh sach syllabus theo program + level.
- Auth: `Admin`, `ManagementStaff`, `Teacher`
- Params:

| Field | Type | Required | Note |
| --- | --- | --- | --- |
| `programId` | `guid?` | Khong | Nen gui |
| `levelId` | `guid?` | Khong | Nen gui |
| `searchTerm` | `string?` | Khong | Tim theo `title/code/version` |
| `isActive` | `bool?` | Khong | Nen gui `true` cho FE |
| `includeDeleted` | `bool` | Khong | Default `false` |
| `pageNumber` | `int` | Khong | Default `1` |
| `pageSize` | `int` | Khong | Default `10` |

- Response success:

```json
{
  "isSuccess": true,
  "data": {
    "syllabuses": {
      "items": [
        {
          "id": "guid",
          "programId": "guid",
          "programName": "Kids A",
          "levelId": "guid",
          "levelName": "Level 1",
          "code": "SYL-KA-L1",
          "version": 1,
          "title": "Syllabus Level 1",
          "isActive": true,
          "unitCount": 10,
          "sessionTemplateCount": 24,
          "createdAt": "2026-06-05T10:00:00Z"
        }
      ],
      "pageNumber": 1,
      "totalPages": 1,
      "totalCount": 1,
      "hasPreviousPage": false,
      "hasNextPage": false
    }
  }
}
```

- Response error:
  - `401`, `403`

### API 6. Get Syllabus Detail

- Endpoint + Method: `GET /api/syllabuses/{id}`
- Muc dich:
  - Lay detail syllabus
  - FE dung API nay de derive module hop le cua syllabus
- Auth: `Admin`, `ManagementStaff`, `Teacher`
- Path params:

| Field | Type | Required |
| --- | --- | --- |
| `id` | `guid` | Co |

- Cac field FE can dung nhat:

| Field | Type | Note |
| --- | --- | --- |
| `id` | `guid` | syllabus id |
| `programId` | `guid` | |
| `levelId` | `guid` | |
| `code` | `string` | |
| `version` | `int` | |
| `title` | `string` | |
| `isActive` | `bool` | |
| `units[]` | array | Co `moduleId`, `moduleName` |
| `lessons[]` | array | Co `moduleId`, `moduleName` |
| `sessionTemplates[]` | array | Co `moduleId`, `moduleName`, `sessionIndexInModule` |
| `lessonPlanTemplateSummaries[]` | array | Co `moduleId`, `moduleCode`, `moduleName`, `moduleOrder`, `plannedSessionCount`, `syllabusSessionTemplateCount` |

- Response success:

```json
{
  "isSuccess": true,
  "data": {
    "id": "guid",
    "programId": "guid",
    "programName": "Kids A",
    "levelId": "guid",
    "levelName": "Level 1",
    "code": "SYL-KA-L1",
    "version": 1,
    "title": "Syllabus Level 1",
    "isActive": true,
    "units": [
      {
        "id": "guid",
        "moduleId": "guid",
        "moduleName": "Module 1",
        "name": "Unit 1",
        "orderIndex": 1
      }
    ],
    "lessons": [],
    "sessionTemplates": [
      {
        "id": "guid",
        "moduleId": "guid",
        "moduleName": "Module 1",
        "sessionIndex": 1,
        "sessionIndexInModule": 1,
        "orderIndex": 1
      }
    ],
    "lessonPlanTemplateSummaries": [
      {
        "moduleId": "guid",
        "moduleCode": "M1",
        "moduleName": "Module 1",
        "moduleOrder": 1,
        "plannedSessionCount": 12,
        "syllabusSessionTemplateCount": 12,
        "importedLessonPlanTemplateCount": 12
      }
    ]
  }
}
```

- Response error:
  - `404` `Syllabus.NotFound`
  - `401`, `403`

## 9.2 Core Tuition Plan APIs

### API 7. Create Tuition Plan

- Endpoint + Method: `POST /api/tuition-plans`
- Muc dich: Tao tuition plan moi.
- Auth: `Admin`, `ManagementStaff`
- Body:

| Field | Type | Required | Note |
| --- | --- | --- | --- |
| `programId` | `guid` | Co | |
| `levelId` | `guid` | Co | |
| `syllabusId` | `guid?` | Khong | Bat buoc neu gui `moduleIds[]` |
| `moduleIds` | `guid[]?` | Khong | Bat buoc neu la goi theo module |
| `learningTicketTypeId` | `guid?` | Khong | Neu gui, backend check active |
| `name` | `string` | Co | Max `255` |
| `totalSessions` | `int` | Co | Goi thuong: > 0. Goi module: phai bang tong buoi |
| `tuitionAmount` | `decimal` | Co | > 0 |
| `currency` | `string` | Co | Max `10` |

- Body example: goi thuong

```json
{
  "programId": "guid",
  "levelId": "guid",
  "syllabusId": null,
  "moduleIds": [],
  "learningTicketTypeId": null,
  "name": "Goi 24 buoi",
  "totalSessions": 24,
  "tuitionAmount": 7200000,
  "currency": "VND"
}
```

- Body example: goi theo syllabus/module

```json
{
  "programId": "guid",
  "levelId": "guid",
  "syllabusId": "guid",
  "moduleIds": ["guid-module-1", "guid-module-2"],
  "learningTicketTypeId": "guid",
  "name": "Goi Module 1-2",
  "totalSessions": 24,
  "tuitionAmount": 7200000,
  "currency": "VND"
}
```

- Response success `201 Created`:

```json
{
  "isSuccess": true,
  "data": {
    "id": "guid",
    "programId": "guid",
    "programName": "Kids A",
    "levelId": "guid",
    "levelName": "Level 1",
    "syllabusId": "guid",
    "syllabusCode": "SYL-KA-L1",
    "syllabusVersion": 1,
    "syllabusTitle": "Syllabus Level 1",
    "moduleIds": ["guid-module-1", "guid-module-2"],
    "modules": [
      {
        "moduleId": "guid-module-1",
        "moduleCode": "M1",
        "moduleName": "Module 1",
        "moduleOrder": 1,
        "plannedSessionCount": 12
      }
    ],
    "learningTicketTypeId": "guid",
    "learningTicketTypeCode": "LT01",
    "name": "Goi Module 1-2",
    "totalSessions": 24,
    "tuitionAmount": 7200000,
    "unitPriceSession": 300000,
    "currency": "VND",
    "isActive": true,
    "createdAt": "2026-06-05T10:00:00Z",
    "updatedAt": "2026-06-05T10:00:00Z"
  }
}
```

- Response error:
  - `400` `Validation.General`
  - `400` `TuitionPlan.LevelProgramMismatch`
  - `400` `TuitionPlan.SyllabusRequiredForModuleSelection`
  - `400` `TuitionPlan.ModuleSelectionRequiredForSyllabus`
  - `400` `TuitionPlan.SyllabusProgramMismatch`
  - `400` `TuitionPlan.SyllabusLevelMismatch`
  - `400` `TuitionPlan.SelectedModuleNotInSyllabus`
  - `400` `TuitionPlan.SelectedModulesMustBeConsecutive`
  - `400` `TuitionPlan.ModuleSelectionSessionCountMismatch`
  - `400` `TuitionPlan.LearningTicketTypeNotFound`
  - `404` `TuitionPlan.ProgramNotFound`
  - `404` `TuitionPlan.LevelNotFound`
  - `404` `TuitionPlan.SyllabusNotFound`
  - `404` `TuitionPlan.ModuleNotFound`

### API 8. Get Tuition Plans

- Endpoint + Method: `GET /api/tuition-plans`
- Muc dich: Lay danh sach tuition plan cho man hinh quan ly.
- Auth: `Admin`, `ManagementStaff`
- Params:

| Field | Type | Required | Note |
| --- | --- | --- | --- |
| `branchId` | `guid?` | Khong | Filter qua program-branch assignment |
| `programId` | `guid?` | Khong | |
| `levelId` | `guid?` | Khong | |
| `moduleId` | `guid?` | Khong | Match neu module thuoc danh sach module cua plan |
| `isActive` | `bool?` | Khong | |
| `pageNumber` | `int` | Khong | Default `1` |
| `pageSize` | `int` | Khong | Default `10` |

- Response success:

```json
{
  "isSuccess": true,
  "data": {
    "tuitionPlans": {
      "items": [
        {
          "id": "guid",
          "programId": "guid",
          "programName": "Kids A",
          "levelId": "guid",
          "levelName": "Level 1",
          "syllabusId": "guid",
          "syllabusCode": "SYL-KA-L1",
          "syllabusVersion": 1,
          "syllabusTitle": "Syllabus Level 1",
          "moduleIds": ["guid-module-1", "guid-module-2"],
          "modules": [
            {
              "moduleId": "guid-module-1",
              "moduleCode": "M1",
              "moduleName": "Module 1",
              "moduleOrder": 1,
              "plannedSessionCount": 12
            }
          ],
          "learningTicketTypeId": "guid",
          "learningTicketTypeCode": "LT01",
          "name": "Goi Module 1-2",
          "totalSessions": 24,
          "tuitionAmount": 7200000,
          "unitPriceSession": 300000,
          "currency": "VND",
          "isActive": true,
          "createdAt": "2026-06-05T10:00:00Z",
          "updatedAt": "2026-06-05T10:00:00Z"
        }
      ],
      "pageNumber": 1,
      "totalPages": 1,
      "totalCount": 1,
      "hasPreviousPage": false,
      "hasNextPage": false
    }
  }
}
```

- Response error:
  - `401`, `403`

### API 9. Get Active Tuition Plans

- Endpoint + Method: `GET /api/tuition-plans/active`
- Muc dich: Lay tuition plan active, khong can auth.
- Auth: `Anonymous`
- Params: giong `GET /api/tuition-plans`, nhung backend hardcode `isActive = true`.
- Response success: giong `GET /api/tuition-plans`.
- Response error: thuong chi co `400` neu query invalid.

### API 10. Get Tuition Plan By Id

- Endpoint + Method: `GET /api/tuition-plans/{id}`
- Muc dich: Xem chi tiet mot tuition plan.
- Auth: `Admin`, `ManagementStaff`
- Path params:

| Field | Type | Required |
| --- | --- | --- |
| `id` | `guid` | Co |

- Response success: cung shape item nhu list, nhung tra object don.

```json
{
  "isSuccess": true,
  "data": {
    "id": "guid",
    "programId": "guid",
    "programName": "Kids A",
    "levelId": "guid",
    "levelName": "Level 1",
    "syllabusId": "guid",
    "moduleIds": ["guid-module-1"],
    "modules": [
      {
        "moduleId": "guid-module-1",
        "moduleCode": "M1",
        "moduleName": "Module 1",
        "moduleOrder": 1,
        "plannedSessionCount": 12
      }
    ],
    "name": "Goi M1",
    "totalSessions": 12,
    "tuitionAmount": 3600000,
    "unitPriceSession": 300000,
    "currency": "VND",
    "isActive": true,
    "createdAt": "2026-06-05T10:00:00Z",
    "updatedAt": "2026-06-05T10:00:00Z"
  }
}
```

- Response error:
  - `404` `TuitionPlan.NotFound`
  - `401`, `403`

### API 11. Update Tuition Plan

- Endpoint + Method: `PUT /api/tuition-plans/{id}`
- Muc dich: Sua tuition plan.
- Auth: `Admin`, `ManagementStaff`
- Path params:

| Field | Type | Required |
| --- | --- | --- |
| `id` | `guid` | Co |

- Body: giong `POST /api/tuition-plans`
- Response success `200 OK`: cung shape voi create response.
- Response error:
  - Tat ca loi cua `POST /api/tuition-plans`
  - `404` `TuitionPlan.NotFound`

### API 12. Toggle Tuition Plan Status

- Endpoint + Method: `PATCH /api/tuition-plans/{id}/toggle-status`
- Muc dich: Doi `isActive` cua tuition plan.
- Auth: `Admin`, `ManagementStaff`
- Path params:

| Field | Type | Required |
| --- | --- | --- |
| `id` | `guid` | Co |

- Body: khong co
- Response success:

```json
{
  "isSuccess": true,
  "data": {
    "id": "guid",
    "isActive": false
  }
}
```

- Response error:
  - `404` `TuitionPlan.NotFound`
  - `409` `STATUS_CHANGE_BLOCKED`
    - ly do hien tai: ton tai `active` hoac `paused` enrollment
  - `401`, `403`

### API 13. Delete Tuition Plan

- Endpoint + Method: `DELETE /api/tuition-plans/{id}`
- Muc dich: Xoa mem tuition plan.
- Auth: `Admin`
- Path params:

| Field | Type | Required |
| --- | --- | --- |
| `id` | `guid` | Co |

- Body: khong co
- Response success:

```json
{
  "isSuccess": true,
  "data": null
}
```

- Response error:
  - `404` `TuitionPlan.NotFound`
  - `409` `TuitionPlan.HasActiveEnrollments`
  - `401`, `403`

## 9.3 API mapping syllabus cua tuition plan

Luu y:

- Sau thay doi moi, `POST/PUT /api/tuition-plans` da tu dong sync `syllabusId` vao `PackageCurriculumMapping`.
- Hai API duoi day van ton tai, nhung FE chi nen dung khi can thao tac mapping rieng.

### API 14. Get Package Syllabuses

- Endpoint + Method: `GET /api/packages/{id}/syllabuses`
- Muc dich: Xem mapping syllabus dang active cua tuition plan.
- Auth: `Admin`, `ManagementStaff`
- Path params:

| Field | Type | Required |
| --- | --- | --- |
| `id` | `guid` | Co |

- Response success:

```json
{
  "isSuccess": true,
  "data": {
    "tuitionPlanId": "guid",
    "tuitionPlanName": "Goi M1-M2",
    "syllabuses": [
      {
        "mappingId": "guid",
        "syllabusId": "guid",
        "programId": "guid",
        "programName": "Kids A",
        "levelId": "guid",
        "levelName": "Level 1",
        "code": "SYL-KA-L1",
        "version": 1,
        "title": "Syllabus Level 1",
        "isActive": true
      }
    ]
  }
}
```

- Response error:
  - `404` `TuitionPlan.NotFound`
  - `401`, `403`

### API 15. Create Package Curriculum Mapping

- Endpoint + Method: `POST /api/package-curriculum-mappings`
- Muc dich:
  - Tao/activate mapping syllabus cho tuition plan
  - Deactivate cac mapping active khac cua cung tuition plan
- Auth: `Admin`, `ManagementStaff`
- Body:

| Field | Type | Required | Note |
| --- | --- | --- | --- |
| `packageId` | `guid` | Co | Chinh la `tuitionPlanId` |
| `syllabusId` | `guid` | Co | |

- Response success:

```json
{
  "isSuccess": true,
  "data": {
    "id": "guid",
    "tuitionPlanId": "guid",
    "tuitionPlanName": "Goi M1-M2",
    "syllabusId": "guid",
    "syllabusCode": "SYL-KA-L1",
    "syllabusVersion": 1,
    "syllabusTitle": "Syllabus Level 1",
    "isActive": true
  }
}
```

- Response error:
  - `404` `TuitionPlan.NotFound`
  - `404` `TuitionPlan.SyllabusNotFound`
  - `400` `TuitionPlan.SyllabusInactive`
  - `400` `TuitionPlan.SyllabusProgramMismatch`
  - `400` `TuitionPlan.SyllabusLevelMismatch`
  - `400` `TuitionPlan.SelectedModuleNotInSyllabus`
  - `400` `TuitionPlan.SelectedModulesMustBeConsecutive`
  - `400` `TuitionPlan.ModuleSelectionSessionCountMismatch`
  - `409` `TuitionPlan.CurriculumAlreadyMapped`

## 10. Validation Rule

### 10.1 Rule chung create/update

- `programId` bat buoc
- `levelId` bat buoc
- `name` bat buoc, max `255`
- `totalSessions >= 0`
- `tuitionAmount > 0`
- `currency` bat buoc, max `10`
- `learningTicketTypeId` neu gui thi khong duoc la `Guid.Empty`

### 10.2 Rule cho goi thuong

- `syllabusId` phai `null`
- `moduleIds[]` phai rong hoac khong gui
- `totalSessions > 0`

### 10.3 Rule cho goi theo syllabus/module

- `syllabusId` bat buoc
- `moduleIds[]` bat buoc co it nhat 1 phan tu
- Moi `moduleId`:
  - phai ton tai
  - phai `isActive`
  - phai cung `levelId`
  - phai thuoc syllabus
- Tap module chon phai la mot dai lien tiep theo thu tu module trong syllabus
- `totalSessions` phai bang tong `plannedSessionCount` cua cac module chon

### 10.4 Rule ve learning ticket

- `learningTicketTypeId` la optional
- Neu gui thi backend check phai ton tai va dang active

### 10.5 Rule ve delete/toggle

- Khong duoc deactivate neu tuition plan dang duoc dung boi enrollment `Active` hoac `Paused`
- Khong duoc delete neu tuition plan dang duoc dung boi enrollment `Active` hoac `Paused`

## 11. Cac Truong Hop Tra Loi

### 11.1 Validation / business errors

| Code | HTTP | Y nghia |
| --- | --- | --- |
| `Validation.General` | `400` | Loi FluentValidation hoac request shape sai |
| `TuitionPlan.LevelProgramMismatch` | `400` | Level khong thuoc program da chon |
| `TuitionPlan.SyllabusRequiredForModuleSelection` | `400` | Co `moduleIds[]` nhung khong co `syllabusId` |
| `TuitionPlan.ModuleSelectionRequiredForSyllabus` | `400` | Co `syllabusId` nhung khong chon module |
| `TuitionPlan.SyllabusProgramMismatch` | `400` | Syllabus khong thuoc program da chon |
| `TuitionPlan.SyllabusLevelMismatch` | `400` | Syllabus khong thuoc level da chon |
| `TuitionPlan.SelectedModuleNotInSyllabus` | `400` | Module khong thuoc syllabus |
| `TuitionPlan.SelectedModulesMustBeConsecutive` | `400` | Cac module chon khong lien tiep |
| `TuitionPlan.ModuleSelectionSessionCountMismatch` | `400` | `totalSessions` khong khop tong buoi |
| `TuitionPlan.SyllabusInactive` | `400` | Syllabus dang inactive |
| `TuitionPlan.LearningTicketTypeNotFound` | `400` | Learning ticket type khong ton tai hoac inactive |

### 11.2 Not found errors

| Code | HTTP | Y nghia |
| --- | --- | --- |
| `TuitionPlan.NotFound` | `404` | Tuition plan khong ton tai hoac da bi xoa mem |
| `TuitionPlan.ProgramNotFound` | `404` | Program khong ton tai |
| `TuitionPlan.LevelNotFound` | `404` | Level khong ton tai |
| `TuitionPlan.SyllabusNotFound` | `404` | Syllabus khong ton tai hoac da xoa |
| `TuitionPlan.ModuleNotFound` | `404` | Module khong ton tai |

### 11.3 Conflict errors

| Code | HTTP | Y nghia |
| --- | --- | --- |
| `TuitionPlan.HasActiveEnrollments` | `409` | Khong duoc xoa vi dang co enrollment su dung |
| `TuitionPlan.CurriculumAlreadyMapped` | `409` | Tuition plan da mapping syllabus nay roi |
| `STATUS_CHANGE_BLOCKED` | `409` | Khong duoc deactivate vi dang co enrollment su dung |

## 12. Recommendation Cho FE

### 12.1 Nen dung endpoint nao

- Quan ly tuition plan:
  - `GET /api/tuition-plans`
  - `POST /api/tuition-plans`
  - `PUT /api/tuition-plans/{id}`
  - `PATCH /api/tuition-plans/{id}/toggle-status`
  - `DELETE /api/tuition-plans/{id}`
- Lookup khi tao/sua:
  - `GET /api/programs`
  - `GET /api/levels`
  - `GET /api/syllabuses`
  - `GET /api/syllabuses/{id}`
  - `GET /api/modules`

### 12.2 Khong nen dung nua

- FE khong nen gui `moduleId` trong body create/update tuition plan.
- FE khong nen tu coi `lessonPlanTemplateSummaries` la danh sach module hop le duy nhat neu muon khop 100% voi backend.
- FE khong can goi `POST /api/package-curriculum-mappings` neu da tao/sua tuition plan bang `syllabusId`.

### 12.3 Cach build UI chon module an toan nhat

1. Chon `program`
2. Chon `level`
3. Chon `syllabus`
4. Lay `syllabus detail`
5. Lay `modules` theo `level`
6. Union module ids tu:
   - `sessionTemplates[].moduleId`
   - `units[].moduleId`
   - `lessons[].moduleId`
7. Join voi `modules` de lay `order`, `name`, `plannedSessionCount`
8. Sort theo `module.order`
9. Chi cho chon cac module lien tiep
10. Tu dong tinh `totalSessions`

