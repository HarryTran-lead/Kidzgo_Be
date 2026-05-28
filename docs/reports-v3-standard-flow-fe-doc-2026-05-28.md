# Reports V3 - FE API Full Doc (Flow Chuan V3)

Ngay cap nhat: 2026-05-28  
Pham vi: luong `ReportsV3Controller` va cac handler/validator hien tai trong codebase.

## 1. Luong chuan V3 (don gian, co review noi bo truoc khi gui PH)

1. Admin/Management tao hoac cau hinh `ReportPeriod`, `ReportTemplate`, `RiskRuleConfig`.
2. Admin/Management/Teacher goi `POST /api/reports/generate` de tao `ReportRun` + `StudentReport`.
3. He thong tinh snapshot immutable, detect risk, tao recommendation, tao insight.
4. Bao cao duoc tao xong de o trang thai noi bo (PH chua xem duoc): `IsParentPublished = false`.
5. Noi bo xem truoc qua `GET /api/reports/{id}` hoac list report theo hoc sinh.
6. Khi duyet noi bo xong, Admin/Management goi `POST /api/reports/{id}/publish-to-parent`.
7. He thong set `IsParentPublished = true` va tao notification in-app cho PH lien ket.
8. Sau publish, PH moi xem duoc parent report. Neu can kenh ngoai app, dung them `share` + `share-callback`.

## 2. Moi role duoc xem gi

| Role | Du lieu duoc xem | Ghi chu |
|---|---|---|
| Admin | Tat ca report, period, template, risk rule, dashboard | Full access |
| ManagementStaff | Tat ca report/dashboard/period; xem template | Khong quan ly risk rule, khong CRUD template |
| Teacher | Report/risk/recommendation/dashboard trong lop duoc gan | Scope theo lop chinh/phu trach |
| Parent | Chi report cua con lien ket, chi `ReportType=Parent`, chi khi da publish | Bi chan report internal/academic |
| AccountantStaff | Khong co endpoint Reports V3 nao duoc authorize | Khong tham gia flow nay |

## 3. Pham vi du lieu (own / department / all)

| Role | Scope | Dien giai |
|---|---|---|
| Admin | all | Khong gioi han branch/class trong Reports V3 |
| ManagementStaff | all | Khong bi filter theo class trong handler hien tai |
| Teacher | department | Chi class co `MainTeacherId` hoac `AssistantTeacherId` trung user |
| Parent | own | Chi student lien ket qua `ParentStudentLinks` |
| AccountantStaff | none | Khong co quyen vao cac endpoint nay |

## 4. Cac hanh dong duoc phep

| Hanh dong | Admin | ManagementStaff | Teacher | Parent | AccountantStaff |
|---|---|---|---|---|---|
| Generate report | Yes | Yes | Yes (chi lop minh) | No | No |
| Xem chi tiet report | Yes | Yes | Yes (chi lop minh) | Yes (chi parent-report da publish) | No |
| Xem list report hoc sinh | Yes | Yes | Yes (chi lop minh) | Yes (chi con minh + da publish) | No |
| Publish report cho parent | Yes | Yes | No | No | No |
| Share report kenh app/email/zalo/sms | Yes | Yes | Yes (chi lop minh) | No | No |
| Share callback | Yes | Yes | No | No | No |
| Mark viewed (app) | Yes | Yes | No | Yes (chi con minh) | No |
| Quan ly report period (get/create/update) | Yes | Yes | No | No | No |
| Xoa report period | Yes | No | No | No | No |
| Xem report template | Yes | Yes | No | No | No |
| CRUD report template | Yes | No | No | No | No |
| Xem/sua risk rule config | Yes | No | No | No | No |
| Xem class academic dashboard | Yes | Yes | Yes (chi lop minh) | No | No |
| Xem class risk alerts | Yes | Yes | Yes (chi lop minh) | No | No |
| Xem branch dashboard | Yes | Yes | No | No | No |

## 5. Status definition

### 5.1 Student report status

| Status | Y nghia |
|---|---|
| Pending | Gia tri default cua entity, hien tai generate flow thuc te khong giu o buoc nay |
| Processing | Dang build snapshot/risk/recommendation/insight |
| Completed | Generate thanh cong |
| Failed | Co trong enum, nhung generate hien tai chu yeu set `ReportRun=Failed` (khong set ro `StudentReport=Failed`) |
| Superseded | Bao cao cu cung `student + period + reportType` bi thay the boi ban generate moi |

### 5.2 Publish status cho parent

| Field | Y nghia |
|---|---|
| `IsParentPublished=false` | Noi bo xem duoc, Parent chua xem duoc |
| `IsParentPublished=true` | Parent duoc phep xem (neu la `ReportType=Parent`) |
| `ParentPublishedAt` | Thoi diem publish |
| `ParentPublishedBy` | User publish |

### 5.3 Report run status

`ReportRunStatus`: `Pending -> Processing -> Completed | Failed`

### 5.4 Share status

`ReportShareStatus`: `Sent`, `Failed`, `Viewed`

### 5.5 Risk alert status

`RiskAlertStatus`: `Open`, `Resolved`, `Ignored`

### 5.6 Recommendation status

`RecommendationStatus`: `Pending`, `Accepted`, `Done`, `Rejected`

### 5.7 Luong chuyen trang thai chinh

1. Generate report: `StudentReport.Processing -> Completed`.
2. Generate lan moi cung ky/type: report truoc do `Completed` se thanh `Superseded`.
3. Publish parent: khong doi `StudentReport.Status`, chi doi `IsParentPublished`.
4. Share log: `Sent -> Viewed` (mark viewed/callback) hoac `Sent -> Failed` (callback).

## 6. Status/type behavior (phan de test nhanh)

### 6.1 `reportType` trong generate

| `generate.reportType` | `StudentReport.ReportType` | Template duoc map de render |
|---|---|---|
| `parent` | `Parent` | `ReportTemplateType.Parent` |
| `academic` | `Academic` | `ReportTemplateType.Academic` |
| `internal` | `Internal` | `ReportTemplateType.Internal` |

Neu khong tim thay template active theo type map, system tu tao default template.

### 6.2 `period.type` anh huong nhu the nao

`period.type` (`weekly/monthly/module/custom`) la metadata de phan ky bao cao va hien thi trong snapshot (`period.type`).  
Generate logic van chay nhu nhau cho ca 4 loai, chi khac date range cua period.

### 6.3 `template.type` anh huong nhu the nao

| `template.type` | Duoc generate su dung truc tiep? | Ghi chu |
|---|---|---|
| Parent | Yes | Dung khi `reportType=parent` |
| Academic | Yes | Dung khi `reportType=academic` |
| Internal | Yes | Dung khi `reportType=internal` |
| Class | No | Hien tai chua duoc map trong generate |
| Branch | No | Hien tai chua duoc map trong generate |

### 6.4 Tat ca truong hop type ban can test

Ket qua thuc te duoc tinh theo cong thuc:

`KQ = generate.reportType map template + period date range`

Nen bo test toi thieu:
1. 3 gia tri `reportType` (`parent`, `academic`, `internal`).
2. 4 gia tri `period.type` (`weekly`, `monthly`, `module`, `custom`).
3. Scenario co template active va scenario khong co template active (de kiem tra auto-create default).

## 7. Response format chung

### 7.1 Success format

Tat ca endpoint dung `MatchOk`/`MatchCreated` tra theo envelope:

```json
{
  "isSuccess": true,
  "data": {}
}
```

### 7.2 Error format

Da so error tu handler tra `ProblemDetails`:

```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.4",
  "title": "Report.NotFound",
  "status": 404,
  "detail": "Report was not found.",
  "errors": [
    {
      "code": "Report.NotFound",
      "description": "Report was not found."
    }
  ]
}
```

### 7.3 Luu y quan trong ve Unauthorized

Trong code hien tai, `Error.Unauthorized(...)` map ve `ErrorType.Failure`, nen co the ra HTTP `500` thay vi `401/403`.  
FE nen doc them `errors[0].code` de phan biet nghiep vu (`Report.AccessDenied`, `Report.ShareDenied`, ...).

### 7.4 Luu y parse enum sai

Mot so endpoint tra thang `400 BadRequest` dang:

```json
{
  "message": "Invalid ... "
}
```

Khong theo ProblemDetails envelope.

## 8. DTO format chinh

### 8.1 `GenerateReportResponse`

| Field | Type |
|---|---|
| reportRunId | guid |
| studentReportId | guid |
| status | string |

### 8.2 `StudentReportListItemDto`

| Field | Type |
|---|---|
| id | guid |
| studentId | guid |
| studentName | string |
| classId | guid |
| className | string |
| branchId | guid |
| reportPeriodId | guid |
| reportType | string |
| status | string |
| isParentPublished | bool |
| parentPublishedAt | datetime? |
| createdAt | datetime |

### 8.3 `StudentReportDetailDto`

| Field | Type |
|---|---|
| id | guid |
| studentId | guid |
| studentName | string |
| classId | guid |
| className | string |
| branchId | guid |
| reportPeriodId | guid |
| reportPeriodName | string |
| reportPeriodFrom | date |
| reportPeriodTo | date |
| reportType | string |
| status | string |
| isParentPublished | bool |
| parentPublishedAt | datetime? |
| snapshot | json |
| summaryText | string? |
| createdAt | datetime |
| insights | `ReportInsightDto[]` |
| risks | `RiskAlertDto[]` |
| recommendations | `RecommendationDto[]` |
| shareLogs | `ReportShareLogDto[]` |

### 8.4 Paged format

`PagedResult<T>`:

| Field | Type |
|---|---|
| items | `T[]` |
| total | int |
| page | int |
| pageSize | int |
| hasNext | bool |

### 8.5 `PublishReportToParentResponse`

| Field | Type |
|---|---|
| reportId | guid |
| isParentPublished | bool |
| parentPublishedAt | datetime? |
| notificationsCreated | int |

## 9. Danh sach API + params/body + success/error

### 9.1 Report generation va report reading

| Endpoint + Method | Muc dich | Params/Body (field, type, required) | Response success | Response error (code + message) |
|---|---|---|---|---|
| `POST /api/reports/generate` | Tao report V3 cho 1 hoc sinh | Body: `reportType:string, req`; `studentId:guid, req`; `classId:guid, opt`; `branchId:guid, opt`; `periodId:guid, req`; `idempotencyKey:string<=100, opt` | `ApiResult<GenerateReportResponse>` | `400 message=Invalid reportType`; `Report.PeriodNotFound`; `Report.StudentNotFound`; `Report.ClassNotFound`; `Report.TeacherScopeDenied`; `Report.GenerateFailed` |
| `GET /api/reports/{id}` | Lay chi tiet report | Path: `id:guid, req` | `ApiResult<StudentReportDetailDto>` | `Report.NotFound`; `Report.AccessDenied`; `Report.ParentViewOnly`; `Report.NotPublished` |
| `GET /api/students/{id}/reports` | Lay list report cua hoc sinh | Path: `id:guid, req`; Query: `classId, branchId, periodId:guid?`; `reportType,status:string?`; `q:string?`; `from,to:datetime?`; `sortBy,sortDir:string?`; `page:int=1`; `pageSize:int=20` | `ApiResult<PagedResult<StudentReportListItemDto>>` | `400 message=Invalid reportType/status filter`; `Report.InvalidPaging`; `Report.AccessDenied` |
| `GET /api/students/{id}/reports/latest` | Lay report moi nhat cua hoc sinh | Path: `id:guid, req`; Query: `reportType:string?` | `ApiResult<StudentReportDetailDto>` | `400 message=Invalid reportType filter`; `Report.NotFound`; `Report.AccessDenied` |
| `GET /api/students/{id}/parent-report` | View parent-report toi uu cho PH | Path: `id:guid, req` | `ApiResult<ParentReportViewResponse>` | `Report.NotFound`; `Report.AccessDenied`; `Report.SnapshotInvalid` |

### 9.2 Dashboard va recommendation

| Endpoint + Method | Muc dich | Params/Body (field, type, required) | Response success | Response error (code + message) |
|---|---|---|---|---|
| `GET /api/classes/{id}/academic-dashboard` | Dashboard hoc vu theo lop | Path: `id:guid, req`; Query: `periodId:guid?` | `ApiResult<ClassAcademicDashboardResponse>` | `Report.ClassNotFound`; `Report.AccessDenied` |
| `GET /api/classes/{id}/risk-alerts` | List risk alert theo lop | Path: `id:guid, req`; Query: `riskType,severity,status:string?`; `sortBy,sortDir:string?`; `page:int=1`; `pageSize:int=20` | `ApiResult<PagedResult<RiskAlertDto>>` | `400 message=Invalid riskType/severity/status`; `Report.InvalidPaging`; `Report.AccessDenied` |
| `GET /api/students/{id}/recommendations` | List recommendation theo hoc sinh | Path: `id:guid, req`; Query: `status,priority:string?`; `dueFrom,dueTo:datetime?`; `overdue:bool?`; `sortBy,sortDir:string?`; `page:int=1`; `pageSize:int=20` | `ApiResult<PagedResult<RecommendationDto>>` | `400 message=Invalid recommendation status/priority`; `Report.InvalidPaging`; `Report.AccessDenied` |
| `GET /api/branches/{id}/dashboard` | Dashboard tong quan branch | Path: `id:guid, req` | `ApiResult<BranchDashboardResponse>` | `Report.BranchNotFound`; `Report.AccessDenied` |

### 9.3 Period APIs

| Endpoint + Method | Muc dich | Params/Body (field, type, required) | Response success | Response error (code + message) |
|---|---|---|---|---|
| `GET /api/reports/periods` | List period | Query: `type:string?`; `from,to:date?`; `q:string?`; `sortBy,sortDir:string?`; `page:int=1`; `pageSize:int=20` | `ApiResult<PagedResult<ReportPeriodDto>>` | `400 message=Invalid period type filter`; `Report.InvalidPaging`; `Report.AccessDenied` |
| `GET /api/reports/periods/{id}` | Chi tiet period | Path: `id:guid, req` | `ApiResult<ReportPeriodDto>` | `Report.PeriodNotFound`; `Report.AccessDenied` |
| `POST /api/reports/periods` | Tao period | Body: `code:string<=50, req`; `name:string<=200, req`; `type:string, req`; `startDate:date, req`; `endDate:date, req` | `201 ApiResult<ReportPeriodDto>` | `400 message=Invalid period type`; validation `EndDate>=StartDate`; `Report.PeriodCodeExists`; `Report.AccessDenied` |
| `PUT /api/reports/periods/{id}` | Sua period | Path: `id:guid, req`; Body nhu create | `ApiResult<ReportPeriodDto>` | `400 message=Invalid period type`; `Report.PeriodNotFound`; `Report.PeriodCodeExists`; validation error; `Report.AccessDenied` |
| `DELETE /api/reports/periods/{id}` | Xoa period | Path: `id:guid, req` | `ApiResult<null>` | `Report.PeriodNotFound`; `Report.PeriodInUse`; `Report.AccessDenied` |

### 9.4 Template APIs

| Endpoint + Method | Muc dich | Params/Body (field, type, required) | Response success | Response error (code + message) |
|---|---|---|---|---|
| `GET /api/reports/templates` | List template | Query: `type:string?`; `isActive:bool?`; `q:string?`; `sortBy,sortDir:string?`; `page:int=1`; `pageSize:int=20` | `ApiResult<PagedResult<ReportTemplateDto>>` | `400 message=Invalid template type filter`; `Report.InvalidPaging`; `Report.AccessDenied` |
| `GET /api/reports/templates/{id}` | Chi tiet template | Path: `id:guid, req` | `ApiResult<ReportTemplateDto>` | `Report.TemplateNotFound`; `Report.AccessDenied` |
| `POST /api/reports/templates` | Tao template | Body: `code:string<=50, req`; `name:string<=200, req`; `type:string, req`; `contentSchema:string(json object), opt`; `isActive:bool, req` | `201 ApiResult<ReportTemplateDto>` | `400 message=Invalid template type`; validation `ContentSchema must be valid JSON object`; `Report.TemplateCodeExists`; `Report.AccessDenied` |
| `PUT /api/reports/templates/{id}` | Sua template | Path: `id:guid, req`; Body nhu create | `ApiResult<ReportTemplateDto>` | `400 message=Invalid template type`; `Report.TemplateNotFound`; `Report.TemplateCodeExists`; validation error; `Report.AccessDenied` |
| `DELETE /api/reports/templates/{id}` | Xoa/deactivate template | Path: `id:guid, req` | `ApiResult<bool>` | `Report.TemplateNotFound`; `Report.AccessDenied` |

### 9.5 Risk rule config APIs

| Endpoint + Method | Muc dich | Params/Body (field, type, required) | Response success | Response error (code + message) |
|---|---|---|---|---|
| `GET /api/reports/risk-rules` | Lay cau hinh rule theo risk type | None | `ApiResult<RiskRuleConfigDto[]>` | `Report.AccessDenied`; `Report.UserNotFound` |
| `PUT /api/reports/risk-rules/{riskType}` | Sua cau hinh 1 risk type | Path: `riskType:string, req`; Body: `isActive:bool, req`; `score:int(0..100), req`; `parametersJson:string(json object), opt` | `ApiResult<RiskRuleConfigDto>` | `400 message=Invalid riskType`; validation error; `Report.AccessDenied` |

### 9.6 Publish/share/viewed APIs

| Endpoint + Method | Muc dich | Params/Body (field, type, required) | Response success | Response error (code + message) |
|---|---|---|---|---|
| `POST /api/reports/{id}/publish-to-parent` | Duyet xong noi bo va mo report cho parent + tao in-app notification | Path: `id:guid, req`; Body: none | `ApiResult<PublishReportToParentResponse>` | `Report.NotFound`; `Report.ParentTypeRequired`; `Report.InvalidStatus`; `Report.PublishParentDenied` |
| `POST /api/reports/{id}/share` | Ghi log gui report qua channel ngoai | Path: `id:guid, req`; Body: `channel:string(app/email/zalo/sms), req`; `recipientName:string<=200, req`; `recipientContact:string<=200, req`; `providerMessageId:string<=200, opt` | `ApiResult<ReportShareLogDto>` | `400 message=Invalid channel`; `Report.NotFound`; `Report.ShareDenied` |
| `POST /api/reports/share-callback` | Cap nhat ket qua provider theo `providerMessageId` | Body: `providerMessageId:string<=200, req`; `status:string(sent/failed/viewed), req`; `errorMessage:string<=1000, opt`; `viewedAt:datetime?, opt` | `ApiResult<ReportShareLogDto>` | `400 message=Invalid callback status`; `Report.ShareLogNotFound`; `Report.ShareCallbackDenied` |
| `POST /api/reports/{id}/mark-viewed` | Danh dau parent da xem (channel app) | Path: `id:guid, req`; Body: none | `ApiResult<ReportShareLogDto>` | `Report.NotFound`; `Report.ShareLogNotFound`; `Report.AccessDenied` |

## 10. Permission matrix theo endpoint

Ky hieu: `Y` = duoc goi; `Y*` = co gioi han scope; `N` = khong duoc goi.

| Endpoint | Admin | ManagementStaff | Teacher | Parent | AccountantStaff |
|---|---|---|---|---|---|
| POST `/api/reports/generate` | Y | Y | Y* | N | N |
| GET `/api/reports/{id}` | Y | Y | Y* | Y* | N |
| GET `/api/students/{id}/reports` | Y | Y | Y* | Y* | N |
| GET `/api/students/{id}/reports/latest` | Y | Y | Y* | Y* | N |
| GET `/api/students/{id}/parent-report` | Y | Y | Y* | Y* | N |
| GET `/api/classes/{id}/academic-dashboard` | Y | Y | Y* | N | N |
| GET `/api/classes/{id}/risk-alerts` | Y | Y | Y* | N | N |
| GET `/api/students/{id}/recommendations` | Y | Y | Y* | N | N |
| GET `/api/branches/{id}/dashboard` | Y | Y | N | N | N |
| GET/POST/PUT `/api/reports/periods...` | Y | Y | N | N | N |
| DELETE `/api/reports/periods/{id}` | Y | N | N | N | N |
| GET `/api/reports/templates...` | Y | Y | N | N | N |
| POST/PUT/DELETE `/api/reports/templates...` | Y | N | N | N | N |
| GET/PUT `/api/reports/risk-rules...` | Y | N | N | N | N |
| POST `/api/reports/{id}/publish-to-parent` | Y | Y | N | N | N |
| POST `/api/reports/{id}/share` | Y | Y | Y* | N | N |
| POST `/api/reports/share-callback` | Y | Y | N | N | N |
| POST `/api/reports/{id}/mark-viewed` | Y | Y | N | Y* | N |

## 11. Validation rules

### 11.1 Generate report

| Field | Rule |
|---|---|
| `studentId` | required, non-empty guid |
| `periodId` | required, non-empty guid |
| `idempotencyKey` | optional, max 100 |
| `reportType` | controller parse: `parent|academic|internal` |

### 11.2 Report period

| Field | Rule |
|---|---|
| `code` | required, trim, max 50, unique (case-insensitive) |
| `name` | required, trim, max 200 |
| `type` | required enum `weekly|monthly|module|custom` |
| `endDate` | `>= startDate` |

### 11.3 Report template

| Field | Rule |
|---|---|
| `code` | required, trim, max 50, unique (case-insensitive) |
| `name` | required, trim, max 200 |
| `type` | required enum `parent|academic|class|branch|internal` |
| `contentSchema` | optional, neu co phai la JSON object hop le |
| `isActive` | bool |

### 11.4 Risk rule config

| Field | Rule |
|---|---|
| `riskType` | path enum hop le |
| `score` | 0..100 |
| `parametersJson` | optional, neu co phai la JSON object hop le |

### 11.5 Share/callback

| Field | Rule |
|---|---|
| `channel` | `app|email|zalo|sms` |
| `recipientName` | required, max 200 |
| `recipientContact` | required, max 200 |
| `providerMessageId` | optional, max 200 |
| `callback.errorMessage` | optional, max 1000 |
| `callback.status` | enum `sent|failed|viewed` |

### 11.6 Paging chung

`page > 0`, `pageSize > 0` cho cac API list co paging.

## 12. Cac truong hop tra loi (error cases)

### 12.1 Validation/BadRequest

1. Enum parse sai (`reportType`, `status`, `riskType`, `template type`, `period type`, `channel`) -> `400` voi `message`.
2. FluentValidation fail (`max length`, `required`, `json invalid`, `date rule`, `paging`) -> `400` ProblemDetails.

### 12.2 NotFound

1. `Report.NotFound`
2. `Report.PeriodNotFound`
3. `Report.TemplateNotFound`
4. `Report.ShareLogNotFound`
5. `Report.BranchNotFound`
6. `Report.UserNotFound`

### 12.3 Conflict

1. `Report.PeriodCodeExists`
2. `Report.TemplateCodeExists`
3. `Report.PeriodInUse`

### 12.4 Business validation

1. `Report.ParentTypeRequired` (chi publish parent report)
2. `Report.InvalidStatus` (publish yeu cau completed)
3. `Report.NotPublished` (parent xem report chua publish)

### 12.5 Access denied

1. `Report.AccessDenied`
2. `Report.ShareDenied`
3. `Report.ShareCallbackDenied`
4. `Report.TeacherScopeDenied`
5. `Report.GenerateForbidden`

Luu y: nhom nay hien co the ra HTTP `500` do mapping `ErrorType.Failure`.

## 13. Ghi chu FE khi tich hop

1. Parent chi nen hien thi report co `reportType=Parent` va `isParentPublished=true`.
2. Luong noi bo duyet nhanh co the dua vao nut `Publish to parent`, khong can them submit/approve state moi.
3. Neu dung share callback voi provider ngoai, can quan ly `providerMessageId` de idempotent.
4. De hien thi thong bao cho PH, sau publish se co notification in-app duoc tao (`kind = student_report_progress`, deeplink `/reports/{reportId}`).
5. Nen xu ly ca 2 kieu error payload: `ProblemDetails` va `{ "message": "Invalid ..." }`.

