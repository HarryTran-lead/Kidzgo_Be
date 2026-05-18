# FE API Spec - Level + Secondary Level Flow

Updated date: 2026-05-18

## 1) Scope
Tai lieu nay mo ta API cho FE o cac luong da chinh sua:
- Placement Test theo huong `Program -> Level`.
- Registration theo huong `Program + Level (+ SecondaryLevel)`.
- Tuition Plan theo huong `Program` bat buoc, `LevelId` optional (null = goi dung chung cho toan Program).

## 2) Response Contract

### 2.1 Success
- `200 OK`:
```json
{
  "isSuccess": true,
  "data": {}
}
```
- `201 Created` (API tao moi):
```json
{
  "isSuccess": true,
  "data": {}
}
```

### 2.2 Error
- Chuan `ProblemDetails`:
```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.1",
  "title": "<error_code>",
  "status": 400,
  "detail": "<message>",
  "errors": [
    {
      "code": "<error_code>",
      "description": "<message>"
    }
  ]
}
```
- Rieng case block doi status (`StatusChangeBlockedError`) tra `409` dang:
```json
{
  "success": false,
  "code": "<error_code>",
  "message": "<message>",
  "details": {
    "entity": "TuitionPlan",
    "entityId": "guid",
    "reasons": ["ACTIVE_ENROLLMENTS_EXIST"],
    "counts": {
      "activeEnrollments": 5
    }
  }
}
```

## 3) Role, Scope, Action

### 3.1 Role xem du lieu gi
| Role | Placement Test | Registration | Tuition Plan |
|---|---|---|---|
| Admin | Full | Full | Full |
| ManagementStaff | Full | Full | Full (tru delete) |
| AccountantStaff | Chi xem danh sach/chi tiet placement test | Khong co quyen trong controller nay | Khong co quyen trong controller nay |
| Anonymous | Khong | Khong | Chi API `/api/tuition-plans/active` |

### 3.2 Pham vi du lieu (own/department/all)
| Role | Scope hien tai |
|---|---|
| Admin | all |
| ManagementStaff | all |
| AccountantStaff | all (trong cac API Placement duoc cap quyen) |
| Anonymous | public data (tuition plan active) |

Ghi chu: hien chua co filter cuong buc theo `own/department` trong cac controller nay; FE co the dung filter query (`branchId`, `programId`, `status`, ...) de thu hep du lieu hien thi.

### 3.3 Hanh dong duoc phep
| Role | view | create | edit | approve | delete |
|---|---|---|---|---|---|
| Admin | Yes | Yes | Yes | N/A | Yes (tuition plan) |
| ManagementStaff | Yes | Yes | Yes | N/A | No |
| AccountantStaff | Placement only | No | No | N/A | No |

## 4) API List

## 4.1 Tuition Plan APIs
Base path: `/api/tuition-plans`

| Method | Endpoint | Mo ta |
|---|---|---|
| POST | `/api/tuition-plans` | Tao tuition plan |
| GET | `/api/tuition-plans` | Danh sach tuition plan |
| GET | `/api/tuition-plans/active` | Danh sach tuition plan active (public) |
| GET | `/api/tuition-plans/{id}` | Chi tiet tuition plan |
| PUT | `/api/tuition-plans/{id}` | Cap nhat tuition plan |
| PATCH | `/api/tuition-plans/{id}/toggle-status` | Bat/tat active |
| DELETE | `/api/tuition-plans/{id}` | Xoa mem tuition plan |

### POST /api/tuition-plans
- Body:
| Field | Type | Required |
|---|---|---|
| programId | Guid | Yes |
| levelId | Guid? | No |
| learningTicketTypeId | Guid? | No |
| name | string | Yes |
| totalSessions | int | Yes |
| tuitionAmount | decimal | Yes |
| currency | string | Yes |

- Rule chinh:
| Rule | Error code |
|---|---|
| Program phai ton tai | `TuitionPlan.ProgramNotFound` |
| Neu co `levelId` thi level phai active va thuoc dung program | `TuitionPlan.LevelNotFoundInProgram` |
| Neu co `learningTicketTypeId` thi ticket type phai active | `TuitionPlan.LearningTicketTypeNotFound` |
| `levelId = null` nghia la goi ap dung toan program | N/A |

- Success data chinh:
`id, programId, levelId, levelName, learningTicketTypeId, name, totalSessions, tuitionAmount, unitPriceSession, currency, isActive`.

### PUT /api/tuition-plans/{id}
- Body giong create.
- Rule them:
| Rule | Error code |
|---|---|
| Tuition plan ton tai va chua bi xoa mem | `TuitionPlan.NotFound` |

### GET /api/tuition-plans, GET /api/tuition-plans/active
- Query:
`branchId?, programId?, levelId?, isActive?, pageNumber=1, pageSize=10`
- Voi `active` endpoint: backend tu ep `isActive = true`.
- Logic loc theo level: khi truyen `levelId`, backend tra ca plan match level do hoac plan chung (`levelId = null`).

### PATCH /api/tuition-plans/{id}/toggle-status
- Rule:
| Rule | Error code |
|---|---|
| Neu tat active ma con active enrollment thi block | `StatusChangeBlockedError` (`ACTIVE_ENROLLMENTS_EXIST`) |

### DELETE /api/tuition-plans/{id}
- Rule:
| Rule | Error code |
|---|---|
| Khong cho xoa neu con enrollment active/paused | `TuitionPlan.HasActiveEnrollments` |

## 4.2 Placement Test APIs
Base path: `/api/placement-tests`

| Method | Endpoint | Mo ta |
|---|---|---|
| POST | `/api/placement-tests` | Tao lich placement test |
| GET | `/api/placement-tests` | Danh sach placement test |
| GET | `/api/placement-tests/{id}` | Chi tiet placement test |
| PUT | `/api/placement-tests/{id}` | Cap nhat lich/thong tin test |
| POST | `/api/placement-tests/{id}/cancel` | Huy placement test |
| POST | `/api/placement-tests/{id}/no-show` | Danh dau no-show |
| PUT | `/api/placement-tests/{id}/results` | Nhap ket qua + de xuat program/level |
| POST | `/api/placement-tests/{id}/notes` | Them note |
| POST | `/api/placement-tests/{id}/convert-to-enrolled` | Convert lead -> enrolled |
| POST | `/api/placement-tests/{id}/retake` | Tao lich retake |
| GET | `/api/placement-tests/availability` | Check lich invigilator/room |
| GET | `/api/placement-tests/available-invigilators` | Alias endpoint availability |
| POST | `/api/placement-tests/{id}/questions/from-bank-matrix` | Sinh cau hoi tu ma tran |

### PUT /api/placement-tests/{id}/results
- Body:
| Field | Type | Required |
|---|---|---|
| listeningScore | decimal? | No |
| speakingScore | decimal? | No |
| readingScore | decimal? | No |
| writingScore | decimal? | No |
| resultScore | decimal? | No |
| programRecommendationId | Guid? | No |
| primaryLevelRecommendationId | Guid? | No |
| secondaryLevelRecommendationId | Guid? | No |
| secondaryLevelSkillFocus | string? | No |
| attachmentUrl | string hoac string[] | No |

- Semantics dac biet:
| Input | Y nghia |
|---|---|
| `programRecommendationId = Guid.Empty` | clear program + clear primary level + clear secondary level + clear secondaryLevelSkillFocus |
| `primaryLevelRecommendationId = Guid.Empty` | clear primary level |
| `secondaryLevelRecommendationId = Guid.Empty` | clear secondary level + clear secondaryLevelSkillFocus |

- Validation chinh:
| Rule | Error code |
|---|---|
| Set primary/secondary level thi bat buoc phai co program recommendation | `PlacementTest.ProgramRecommendationRequired` |
| Primary level phai thuoc program recommendation | `PlacementTest.PrimaryLevelProgramMismatch` |
| Secondary level phai thuoc program recommendation | `PlacementTest.SecondaryLevelProgramMismatch` |
| Primary level va secondary level khong duoc trung | `PlacementTest.SecondaryLevelDuplicated` |
| Co `secondaryLevelSkillFocus` nhung khong co `secondaryLevelRecommendationId` | `PlacementTest.SecondaryLevelMissing` |

- Success data chinh:
`id, scores, programRecommendationId/name, primaryLevelRecommendationId/name, secondaryLevelRecommendationId/name, secondaryLevelSkillFocus, status, updatedAt, newRegistrationId`.

## 4.3 Registration APIs
Base path: `/api/registrations`

| Method | Endpoint | Mo ta |
|---|---|---|
| POST | `/api/registrations` | Tao registration |
| POST | `/api/registrations/import-active` | Import registration dang hoc do |
| GET | `/api/registrations` | Danh sach registration |
| GET | `/api/registrations/{id}` | Chi tiet registration |
| PUT | `/api/registrations/{id}` | Cap nhat registration |
| PATCH | `/api/registrations/{id}/cancel` | Huy registration |
| GET | `/api/registrations/{id}/suggest-classes` | Goi y class primary/secondary |
| POST | `/api/registrations/{id}/assign-class` | Xep lop theo track |
| POST | `/api/registrations/{id}/transfer-class` | Chuyen lop theo track |
| POST | `/api/registrations/{id}/upgrade` | Nang goi |
| GET | `/api/registrations/waiting-list` | Danh sach cho xep lop |
| GET | `/api/registrations/{id}/enrollment-confirmation-pdf` | Preview phieu xac nhan |
| POST | `/api/registrations/{id}/enrollment-confirmation-pdf` | Generate phieu xac nhan |
| GET | `/api/registrations/{id}/enrollment-confirmation-pdf/history` | Lich su file PDF |
| GET | `/api/registrations/enrollment-confirmation-payment-setting` | Lay cau hinh thanh toan |
| PUT | `/api/registrations/enrollment-confirmation-payment-setting` | Cap nhat cau hinh thanh toan |

### POST /api/registrations
- Body:
| Field | Type | Required |
|---|---|---|
| studentProfileId | Guid | Yes |
| branchId | Guid | Yes |
| programId | Guid | Yes |
| levelId | Guid | Yes |
| tuitionPlanId | Guid | Yes |
| secondaryLevelId | Guid? | No |
| secondaryLevelSkillFocus | string? | No |
| expectedStartDate | DateTime? | No |
| preferredSchedule | string? | No |
| note | string? | No |

- Validation chinh:
| Rule | Error code |
|---|---|
| Level phai thuoc program da chon va active | `Registration.LevelNotFoundInProgram` |
| Secondary level neu co phai khac primary level | `Registration.SecondaryLevelDuplicated` |
| Secondary level neu co phai thuoc cung program va active | `Registration.SecondaryLevelNotFoundInProgram` |
| Co `secondaryLevelSkillFocus` ma khong co secondary level | `Registration.SecondaryLevelMissing` |
| Tuition plan phai cung program va la plan level-specific hoac plan chung | `Registration.TuitionPlanNotFound` |

- Success data chinh:
`id, programId/name, levelId/name, secondaryLevelId/name, secondaryLevelSkillFocus, tuitionPlanId/name, status, pricing fields`.

### PUT /api/registrations/{id}
- Body:
| Field | Type | Required |
|---|---|---|
| expectedStartDate | DateTime? | No |
| preferredSchedule | string? | No |
| note | string? | No |
| tuitionPlanId | Guid? | No |
| secondaryLevelId | Guid? | No |
| secondaryLevelSkillFocus | string? | No |
| removeSecondaryLevel | bool | No |

- Validation chinh:
| Rule | Error code |
|---|---|
| Khong update neu status da Completed/Cancelled | `Registration.InvalidStatus` |
| Khong xoa/chuyen secondary level khi secondary class da duoc assign | `Registration.SecondaryClassAssigned` |
| Co `secondaryLevelSkillFocus` ma khong co secondary level hien huu | `Registration.SecondaryLevelMissing` |
| Khong doi tuition plan sau khi da assign bat ky class nao | `Registration.ClassAlreadyAssigned` |
| Tuition plan moi phai cung program va phai la program-wide hoac match level | `DifferentProgram`, `DifferentLevel` |

### POST /api/registrations/{id}/assign-class
- Body:
| Field | Type | Required |
|---|---|---|
| classId | Guid? | Conditional |
| entryType | string (`immediate`, `wait`, `retake`) | No (default `immediate`) |
| track | string (`primary`, `secondary`) | No (default `primary`) |
| firstStudyDate | DateOnly? | No |
| weeklyPattern | List<WeeklyPatternEntry>? | No |

- Validation chinh:
| Rule | Error code |
|---|---|
| `track=secondary` thi registration phai co secondaryLevelId | `Registration.SecondaryLevelMissing` |
| `entryType != wait` thi bat buoc co classId | `Registration.ClassIdRequired` |
| Khong cho assign trung khi track da co class | `Registration.ClassAlreadyAssigned` |
| Class phai cung branch + program va con slot | `Registration.ClassNotMatchingBranch`, `Registration.ClassNotMatchingProgram`, `Registration.ClassFull` |

### POST /api/registrations/{id}/transfer-class
- Body:
| Field | Type | Required |
|---|---|---|
| newClassId | Guid | Yes |
| track | string (`primary`, `secondary`) | No |
| weeklyPattern | List<WeeklyPatternEntry>? | No |
| effectiveDate | DateTime? | No |

- Validation chinh:
| Rule | Error code |
|---|---|
| Track phai co class hien tai moi transfer duoc | `NoClassAssigned` |
| Khong transfer sang chinh class cu | `Registration.CannotTransferToSameClass` |
| `track=secondary` phai co secondary level | `Registration.SecondaryLevelMissing` |

## 5) Status Definition

## 5.1 PlacementTestStatus
| Status | Y nghia |
|---|---|
| Scheduled | Da dat lich test |
| NoShow | Hoc vien khong tham gia buoi test |
| Completed | Da nhap du diem va hoan tat bai test |
| Cancelled | Da huy lich test |

Luong thuc te dang cho phep:
- `Schedule` tao moi -> `Scheduled`.
- Tu moi trang thai khac `Completed` co the `cancel`, `no-show`, `update`.
- Khi nhap du score thi chuyen `Completed`.

## 5.2 RegistrationStatus
| Status | Y nghia |
|---|---|
| New | Moi tao, chua xu ly xep lop |
| WaitingForClass | Cho xep lop (thieu primary class hoac thieu secondary class khi co secondary level) |
| ClassAssigned | Da xep lop nhung chua hoc ngay (retake/makeup) |
| Studying | Dang hoc (co track `immediate`) |
| Paused | Bao luu |
| Completed | Hoan tat |
| Cancelled | Da huy |

Luong chuyen chinh:
- Create -> `New`.
- Assign class/update track -> backend tu resolve `WaitingForClass` / `ClassAssigned` / `Studying` theo `EntryType` cua primary/secondary track.
- Cancel endpoint -> `Cancelled`.
- Flow hoan tat/bao luu do nghiep vu khac cap nhat.

## 6) Permission Matrix theo Role
| API Group | Admin | ManagementStaff | AccountantStaff | Anonymous |
|---|---|---|---|---|
| TuitionPlan CRUD | Yes | Create/Read/Update/Toggle | No | No |
| TuitionPlan Active List | Yes | Yes | Yes | Yes |
| PlacementTest create/update/results/cancel/no-show/retake/notes/convert | Yes | Yes | No | No |
| PlacementTest list/detail | Yes | Yes | Yes | No |
| Registration CRUD + assign/transfer/upgrade + waiting-list + PDF | Yes | Yes | No | No |
| Payment setting update | Yes | No | No | No |

## 7) Validation Rules tong hop (can FE check truoc)
| Rule | Nen check o FE |
|---|---|
| TuitionPlan bat buoc `programId` | Block submit neu thieu |
| TuitionPlan `levelId` optional | Cho phep null de tao goi chung program |
| Registration bat buoc `programId`, `levelId`, `tuitionPlanId` | Block submit |
| Secondary level phai khac primary level | Block submit |
| `secondaryLevelSkillFocus` chi hop le khi da chon secondary level | Block submit |
| Placement results set level thi phai set program truoc | Disable level picker neu chua co program |
| Placement primary/secondary level phai khac nhau | Block submit |

## 8) Error Cases FE can handle
| HTTP | Nhom loi | Vi du code |
|---|---|---|
| 400 | Validation | `PlacementTest.SecondaryLevelMissing`, `Registration.SecondaryLevelDuplicated`, `DifferentLevel` |
| 404 | NotFound | `Registration.NotFound`, `TuitionPlan.NotFound`, `PlacementTest.NotFound` |
| 409 | Conflict | `Registration.AlreadyExists`, `TuitionPlan.HasActiveEnrollments`, `StatusChangeBlockedError` |
| 500 | Server failure | Loi he thong chua phan loai |

## 9) Notes cho FE khi migrate flow moi
- Khong con dung secondary program trong form nhap ket qua/dang ky.
- Dung `secondaryLevelId` va `secondaryLevelSkillFocus`.
- Truong `programId` van bat buoc o registration va tuition plan.
- Khi lay tuition plan theo level, backend da ho tro tra ca plan rieng level va plan chung program (`levelId = null`).
- Route progression: `/api/level-progressions`.
- Progression assessment list da ho tro filter them theo `sourceLevelId` va `targetLevelId`.
