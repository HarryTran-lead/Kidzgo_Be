# Tai Lieu API FE - Pause Enrollment Request - 2026-04-30

Tai lieu nay mo ta day du cac API trong `PauseEnrollmentRequestController.cs`, bao gom thay doi moi cho phep:

- pause tat ca enrollment hop le cua hoc sinh trong khoang ngay (`AllEligible`)
- pause rieng 1 chuong trinh / 1 track thong qua `classId` (`SingleClass`)

Ghi chu ky thuat:

- Backend khong can migration cho thay doi nay.
- `scope` duoc suy ra tu `PauseEnrollmentRequest.ClassId`:
  - `ClassId = null` -> `AllEligible`
  - `ClassId != null` -> `SingleClass`

## 1. Moi role duoc xem du lieu gi

| Role | Du lieu duoc xem | Ghi chu |
| --- | --- | --- |
| Admin | Tat ca pause enrollment requests | Theo controller auth hien tai |
| ManagementStaff | Tat ca pause enrollment requests | Theo controller auth hien tai |
| Parent | Co the xem list/detail request | List se auto-scope ve hoc sinh cua parent neu khong gui `studentProfileId` |
| Student | Co the xem list/detail request | List se auto-scope ve hoc sinh trong token neu khong gui `studentProfileId` |
| Teacher | Theo controller auth hien tai van goi duoc list/detail/create/cancel | FE nen can nhac an neu khong dung nghiep vu |
| AccountantStaff | Theo controller auth hien tai van goi duoc list/detail/create/cancel | FE nen can nhac an neu khong dung nghiep vu |
| Anonymous | Khong truy cap duoc | `[Authorize]` |

## 2. Pham vi du lieu (own / department / all)

| Role | Pham vi thuc te theo code hien tai |
| --- | --- |
| Admin | `all` |
| ManagementStaff | `all` |
| Parent | `own` mac dinh o list khi khong gui `studentProfileId`, nhung create/detail/cancel hien chua co ownership guard manh |
| Student | `own` mac dinh o list khi khong gui `studentProfileId`, nhung create/detail/cancel hien chua co ownership guard manh |
| Teacher | `all` neu FE cho phep goi |
| AccountantStaff | `all` neu FE cho phep goi |

Ghi chu quan trong:

- `GET /api/pause-enrollment-requests` chi auto-scope Parent/Student khi khong truyen `studentProfileId`.
- Neu FE truyen explicit `studentProfileId`, query handler hien tai khong block theo ownership.
- `GET /api/pause-enrollment-requests/{id}` va `PUT /cancel` hien tai cung chua co ownership guard theo parent/student.

## 3. Cac hanh dong duoc phep

| Role | View | Create | Cancel | Approve | Approve bulk | Reject | Update outcome | Reassign equivalent class |
| --- | --- | --- | --- | --- | --- | --- | --- | --- |
| Admin | Yes | Yes | Yes | Yes | Yes | Yes | Yes | Yes |
| ManagementStaff | Yes | Yes | Yes | Yes | Yes | Yes | Yes | Yes |
| Parent | Yes | Yes | Yes | No | No | No | No | No |
| Student | Yes | Yes | Yes | No | No | No | No | No |
| Teacher | Yes | Yes | Yes | No | No | No | No | No |
| AccountantStaff | Yes | Yes | Yes | No | No | No | No | No |
| Anonymous | No | No | No | No | No | No | No | No |

## 4. Dinh dang response chung

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
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.8",
  "title": "PauseEnrollmentRequest.DuplicateActiveRequest",
  "status": 409,
  "detail": "A pending or approved pause request already exists for this student in the selected date range"
}
```

Validation error co the co them `errors` trong `extensions`.

## 5. Danh sach API

### 5.1 POST `/api/pause-enrollment-requests`

Mo ta:

- Tao request bao luu.
- Ho tro 2 mode:
  - `AllEligible`: pause tat ca enrollment active hop le trong khoang bao luu
  - `SingleClass`: pause duy nhat enrollment active cua `classId`

Body:

| Field | Type | Required | Mo ta |
| --- | --- | --- | --- |
| `studentProfileId` | `Guid` | Yes | Hoc sinh can bao luu |
| `pauseFrom` | `DateOnly` | Yes | Ngay bat dau bao luu |
| `pauseTo` | `DateOnly` | Yes | Ngay ket thuc bao luu |
| `reason` | `string?` | No | Ly do bao luu |
| `scope` | `string?` | No | `AllEligible` hoac `SingleClass`. Neu bo trong, backend tu suy ra tu `classId` |
| `classId` | `Guid?` | Co dieu kien | Bat buoc khi `scope = SingleClass`, phai bo trong khi `scope = AllEligible` |

Success `201`:

- `CreatePauseEnrollmentRequestResponse`

| Field | Type |
| --- | --- |
| `id` | `Guid` |
| `studentProfileId` | `Guid` |
| `classId` | `Guid?` |
| `pauseFrom` | `DateOnly` |
| `pauseTo` | `DateOnly` |
| `reason` | `string?` |
| `scope` | `string` |
| `status` | `string` |
| `requestedAt` | `DateTime` |
| `classes` | `PauseEnrollmentClassDto[]` |

`classes[]`:

| Field | Type |
| --- | --- |
| `id` | `Guid` |
| `code` | `string` |
| `title` | `string` |
| `programId` | `Guid` |
| `programName` | `string` |
| `branchId` | `Guid` |
| `branchName` | `string` |
| `startDate` | `DateOnly` |
| `endDate` | `DateOnly?` |
| `status` | `string` |

Response error:

| HTTP | Code | Message/meaning |
| --- | --- | --- |
| `400` | `PauseEnrollmentRequest.InvalidScope` | `scope` khong hop le |
| `400` | `PauseEnrollmentRequest.ClassIdRequiredForSingleClass` | `SingleClass` nhung thieu `classId` |
| `400` | `PauseEnrollmentRequest.ClassIdNotAllowedForAllEligible` | `AllEligible` nhung van gui `classId` |
| `400` | `PauseEnrollmentRequest.ClassNotInPauseRange` | Lop duoc chon khong co assigned study session trong khoang bao luu |
| `404` | `PauseEnrollmentRequest.StudentNotFound` | Hoc sinh khong ton tai / khong active |
| `409` | `PauseEnrollmentRequest.NotEnrolled` | Hoc sinh khong active trong lop duoc chon |
| `409` | `PauseEnrollmentRequest.NoEnrollmentsInRange` | Khong co enrollment active hop le trong khoang bao luu |
| `409` | `PauseEnrollmentRequest.DuplicateActiveRequest` | Da co request pending/approved overlap |
| `401` | Unauthorized | Chua dang nhap |

### 5.2 GET `/api/pause-enrollment-requests`

Mo ta:

- Lay danh sach request bao luu co paging.

Query params:

| Field | Type | Required | Default |
| --- | --- | --- | --- |
| `studentProfileId` | `Guid?` | No | `null` |
| `classId` | `Guid?` | No | `null` |
| `status` | `PauseEnrollmentRequestStatus?` | No | `null` |
| `branchId` | `Guid?` | No | `null` |
| `pageNumber` | `int` | No | `1` |
| `pageSize` | `int` | No | `10` |

Success `200`:

- `Page<PauseEnrollmentRequestResponse>`

`PauseEnrollmentRequestResponse` fields:

| Field | Type |
| --- | --- |
| `id` | `Guid` |
| `studentProfileId` | `Guid` |
| `classId` | `Guid?` |
| `scope` | `string` |
| `pauseFrom` | `DateOnly` |
| `pauseTo` | `DateOnly` |
| `reason` | `string?` |
| `status` | `string` |
| `requestedAt` | `DateTime` |
| `approvedBy` | `Guid?` |
| `approvedAt` | `DateTime?` |
| `cancelledBy` | `Guid?` |
| `cancelledAt` | `DateTime?` |
| `outcome` | `string?` |
| `outcomeNote` | `string?` |
| `outcomeBy` | `Guid?` |
| `outcomeAt` | `DateTime?` |
| `reassignedClassId` | `Guid?` |
| `reassignedEnrollmentId` | `Guid?` |
| `outcomeCompletedBy` | `Guid?` |
| `outcomeCompletedAt` | `DateTime?` |
| `reservedSessionCount` | `int` |
| `reservationExpiresOn` | `DateOnly?` |
| `reservationSnapshotAt` | `DateTime?` |
| `classes` | `PauseEnrollmentClassDto[]` |

Response error:

| HTTP | Code | Message/meaning |
| --- | --- | --- |
| `401` | Unauthorized | Chua dang nhap |
| `400` | Enum parse error | `status` query khong hop le |

Ghi chu:

- Voi request `SingleClass`, `classes` se chi gom lop muc tieu.
- Voi request `AllEligible`, `classes` la danh sach lop hop le trong khoang ngay.

### 5.3 GET `/api/pause-enrollment-requests/{id}`

Mo ta:

- Lay chi tiet 1 request bao luu.

Path params:

| Field | Type | Required |
| --- | --- | --- |
| `id` | `Guid` | Yes |

Success `200`:

- `PauseEnrollmentRequestResponse`

Response error:

| HTTP | Code | Message/meaning |
| --- | --- | --- |
| `404` | `PauseEnrollmentRequest.NotFound` | Khong tim thay request |
| `401` | Unauthorized | Chua dang nhap |

### 5.4 PUT `/api/pause-enrollment-requests/{id}/approve`

Mo ta:

- Duyet request bao luu.
- Neu request la `SingleClass`, chi pause 1 enrollment cua `classId`.
- Neu request la `AllEligible`, pause tat ca enrollment active hop le trong khoang ngay.

Body: khong co.

Success `200`:

```json
{
  "isSuccess": true,
  "data": null
}
```

Response error:

| HTTP | Code | Message/meaning |
| --- | --- | --- |
| `404` | `PauseEnrollmentRequest.NotFound` | Khong tim thay request |
| `409` | `PauseEnrollmentRequest.AlreadyApproved` | Request da duoc approve |
| `409` | `PauseEnrollmentRequest.AlreadyRejected` | Request da bi reject |
| `409` | `PauseEnrollmentRequest.AlreadyCancelled` | Request da bi cancel |
| `409` | `PauseEnrollmentRequest.NoEnrollmentsInRange` | Khong con enrollment hop le de pause |
| `409` | `PauseEnrollmentRequest.NotEnrolled` | Voi `SingleClass`, hoc sinh khong con active trong lop |
| `401` | Unauthorized | Chua dang nhap |
| `403` | Forbidden | Khong phai `Admin`/`ManagementStaff` |

Side effects:

- Set request status -> `Approved`
- Set enrollment target `Active -> Paused`
- Cancel assigned sessions trong khoang bao luu
- Ghi `PauseEnrollmentRequestHistory`
- Tinh `reservedSessionCount`, `reservationExpiresOn`, `reservationSnapshotAt`
- Recalculate class lifecycle

### 5.5 PUT `/api/pause-enrollment-requests/approve-bulk`

Mo ta:

- Approve nhieu request.
- Xu ly tung id, request loi nao thi gom vao `errors`, khong rollback toan bo batch.

Body:

| Field | Type | Required | Mo ta |
| --- | --- | --- | --- |
| `ids` | `Guid[]` | Yes | Danh sach request id can approve |

Success `200`:

- `BulkApprovePauseEnrollmentRequestsResponse`

| Field | Type | Mo ta |
| --- | --- | --- |
| `approvedIds` | `Guid[]` | Cac request approve thanh cong |
| `errors` | `BulkApprovePauseEnrollmentRequestError[]` | Danh sach loi theo tung id |

`BulkApprovePauseEnrollmentRequestError`:

| Field | Type |
| --- | --- |
| `id` | `Guid` |
| `code` | `string` |
| `message` | `string` |

Response error:

| HTTP | Code | Message/meaning |
| --- | --- | --- |
| `401` | Unauthorized | Chua dang nhap |
| `403` | Forbidden | Khong phai `Admin`/`ManagementStaff` |

### 5.6 PUT `/api/pause-enrollment-requests/{id}/reject`

Mo ta:

- Tu choi request.

Body: khong co.

Success `200`:

```json
{
  "isSuccess": true,
  "data": null
}
```

Response error:

| HTTP | Code | Message/meaning |
| --- | --- | --- |
| `404` | `PauseEnrollmentRequest.NotFound` | Khong tim thay request |
| `409` | `PauseEnrollmentRequest.AlreadyApproved` | Request da approve |
| `409` | `PauseEnrollmentRequest.AlreadyRejected` | Request da reject |
| `409` | `PauseEnrollmentRequest.AlreadyCancelled` | Request da cancel |
| `401` | Unauthorized | Chua dang nhap |
| `403` | Forbidden | Khong phai `Admin`/`ManagementStaff` |

### 5.7 PUT `/api/pause-enrollment-requests/{id}/cancel`

Mo ta:

- Huy request bao luu khi request van con `Pending`.

Body: khong co.

Success `200`:

```json
{
  "isSuccess": true,
  "data": null
}
```

Response error:

| HTTP | Code | Message/meaning |
| --- | --- | --- |
| `404` | `PauseEnrollmentRequest.NotFound` | Khong tim thay request |
| `409` | `PauseEnrollmentRequest.AlreadyCancelled` | Request da cancel |
| `409` | `PauseEnrollmentRequest.AlreadyApproved` | Request da approve, khong cho cancel |
| `409` | `PauseEnrollmentRequest.AlreadyRejected` | Request da reject |
| `409` | `PauseEnrollmentRequest.CancelWindowExpired` | Hom nay da >= `pauseFrom` |
| `401` | Unauthorized | Chua dang nhap |

### 5.8 PUT `/api/pause-enrollment-requests/{id}/outcome`

Mo ta:

- Cap nhat huong xu ly sau khi request da duoc approve.

Body:

| Field | Type | Required | Mo ta |
| --- | --- | --- | --- |
| `outcome` | `PauseEnrollmentOutcome` | Yes | `ContinueSameClass`, `ReassignEquivalentClass`, `ContinueWithTutoring` |
| `outcomeNote` | `string?` | No | Ghi chu staff |

Success `200`:

```json
{
  "isSuccess": true,
  "data": null
}
```

Response error:

| HTTP | Code | Message/meaning |
| --- | --- | --- |
| `404` | `PauseEnrollmentRequest.NotFound` | Khong tim thay request |
| `409` | `PauseEnrollmentRequest.OutcomeNotAllowed` | Request chua duoc approve |
| `409` | `PauseEnrollmentRequest.OutcomeAlreadyCompleted` | Outcome da duoc hoan tat |
| `401` | Unauthorized | Chua dang nhap |
| `403` | Forbidden | Khong phai `Admin`/`ManagementStaff` |

Side effects theo `outcome`:

- `ContinueSameClass`: chi luu outcome, khong doi status request.
- `ReassignEquivalentClass`: drop cac paused enrollment lien quan de chuan bi cho step reassign.
- `ContinueWithTutoring`: cancel assignment sau giai doan pause theo logic hien tai.

### 5.9 POST `/api/pause-enrollment-requests/{id}/reassign-equivalent-class`

Mo ta:

- Sau khi outcome da la `ReassignEquivalentClass`, staff dung API nay de chuyen hoc sinh sang lop tuong duong.

Body:

| Field | Type | Required | Mo ta |
| --- | --- | --- | --- |
| `registrationId` | `Guid` | Yes | Registration can xu ly |
| `newClassId` | `Guid` | Yes | Lop moi |
| `track` | `string` | No | `primary`/`secondary`, default `primary` |
| `weeklyPattern` | `WeeklyPatternEntry[]?` | No | Pattern chon buoi hoc neu can |
| `effectiveDate` | `DateTime?` | No | Ngay co hieu luc reassign |

Success `200`:

- `ReassignEquivalentClassResponse`

| Field | Type |
| --- | --- |
| `pauseEnrollmentRequestId` | `Guid` |
| `registrationId` | `Guid` |
| `oldClassId` | `Guid` |
| `oldClassName` | `string` |
| `newClassId` | `Guid` |
| `newClassName` | `string` |
| `droppedEnrollmentId` | `Guid` |
| `newEnrollmentId` | `Guid` |
| `track` | `string` |
| `effectiveDate` | `DateTime` |
| `registrationStatus` | `string` |
| `outcomeCompletedAt` | `DateTime` |

Response error:

| HTTP | Code | Message/meaning |
| --- | --- | --- |
| `404` | `PauseEnrollmentRequest.NotFound` | Khong tim thay request |
| `404` | `Registration.NotFound` | Registration khong ton tai |
| `409` | `PauseEnrollmentRequest.OutcomeNotAllowed` | Request chua approve |
| `409` | `PauseEnrollmentRequest.OutcomeMustBeReassignEquivalentClass` | Chua set dung outcome |
| `409` | `PauseEnrollmentRequest.OutcomeAlreadyCompleted` | Outcome da chot |
| `409` | `PauseEnrollmentRequest.NoPausedEnrollmentToReassign` | Khong tim thay enrollment paused hop le tu request nay |
| `409` | `AlreadyEnrolled` | Hoc sinh da active/paused o lop moi |
| `409` | `Registration.ClassFull` | Lop moi da full |
| `400` | `PauseEnrollmentRequest.RegistrationStudentMismatch` | Registration khong thuoc hoc sinh cua request |
| `400` | `PauseEnrollmentRequest.EffectiveDateBeforePauseEnd` | `effectiveDate <= pauseTo` |
| `400` | `Registration.SecondaryProgramMissing` | Track secondary nhung registration khong co secondary program |
| `400` | `NoClassAssigned` | Track hien tai chua co lop |
| `400` | `ClassNotAvailable` | Lop moi khong o status cho phep |
| `400` | Weekly pattern / schedule conflict error | Pattern khong hop le hoac trung lich |
| `401` | Unauthorized | Chua dang nhap |
| `403` | Forbidden | Khong phai `Admin`/`ManagementStaff` |

## 6. Status definition

### 6.1 `PauseEnrollmentRequestStatus`

| Status | Y nghia |
| --- | --- |
| `Pending` | Request moi tao, chua duoc staff xu ly |
| `Approved` | Request da duoc duyet va enrollment da duoc pause |
| `Rejected` | Request bi tu choi |
| `Cancelled` | Request bi huy truoc khi co hieu luc |

### 6.2 `PauseEnrollmentOutcome`

| Outcome | Y nghia |
| --- | --- |
| `ContinueSameClass` | Het bao luu se quay lai hoc tiep cung lop |
| `ReassignEquivalentClass` | Het bao luu se duoc chuyen sang lop tuong duong |
| `ContinueWithTutoring` | Khong quay lai lop cu, tiep tuc theo huong tutoring |

### 6.3 `scope`

| Scope | Y nghia |
| --- | --- |
| `AllEligible` | Pause tat ca enrollment active hop le cua hoc sinh trong khoang ngay |
| `SingleClass` | Chi pause enrollment active cua 1 `classId` cu the |

## 7. Luong chuyen trang thai

### 7.1 Request status

```text
Pending -> Approved
Pending -> Rejected
Pending -> Cancelled
Approved -> giu nguyen Approved khi cap nhat outcome
```

Ghi chu:

- `Rejected`, `Cancelled`, `Approved` deu la trang thai terminal o level request.
- `Outcome` la state phu, khong thay doi `PauseEnrollmentRequestStatus`.

### 7.2 Enrollment side effects

`Approve`:

- `AllEligible`: `Enrollment.Active -> Enrollment.Paused` cho tat ca enrollment hop le
- `SingleClass`: `Enrollment.Active -> Enrollment.Paused` chi cho 1 enrollment muc tieu

`Outcome = ReassignEquivalentClass`:

- `Enrollment.Paused -> Enrollment.Dropped` cho enrollment cu lien quan
- goi `reassign-equivalent-class` se tao enrollment moi `Active`

## 8. Permission matrix theo role

| Endpoint | Admin | ManagementStaff | Parent | Student | Teacher | AccountantStaff |
| --- | --- | --- | --- | --- | --- | --- |
| `POST /api/pause-enrollment-requests` | Yes | Yes | Yes | Yes | Yes | Yes |
| `GET /api/pause-enrollment-requests` | Yes | Yes | Yes | Yes | Yes | Yes |
| `GET /api/pause-enrollment-requests/{id}` | Yes | Yes | Yes | Yes | Yes | Yes |
| `PUT /{id}/approve` | Yes | Yes | No | No | No | No |
| `PUT /approve-bulk` | Yes | Yes | No | No | No | No |
| `PUT /{id}/reject` | Yes | Yes | No | No | No | No |
| `PUT /{id}/cancel` | Yes | Yes | Yes | Yes | Yes | Yes |
| `PUT /{id}/outcome` | Yes | Yes | No | No | No | No |
| `POST /{id}/reassign-equivalent-class` | Yes | Yes | No | No | No | No |

## 9. Validation rule

### 9.1 Rule kiem tra du lieu

Create request:

- Hoc sinh phai ton tai, active, khong bi delete.
- Hoc sinh phai co it nhat 1 enrollment `Active`.
- `scope` chi cho phep `AllEligible` hoac `SingleClass`.
- `SingleClass` bat buoc co `classId`.
- `AllEligible` khong duoc gui `classId`.
- `SingleClass` yeu cau hoc sinh dang active trong lop do.
- `SingleClass` yeu cau lop do co assigned study sessions nam trong khoang bao luu.
- Khong duoc co request `Pending`/`Approved` bi overlap:
  - `AllEligible` conflict voi moi request overlap cua hoc sinh
  - `SingleClass` conflict voi request overlap cua cung class, va cung conflict voi request `AllEligible`

Approve request:

- Request phai o `Pending`.
- `SingleClass`: hoc sinh van phai con enrollment `Active` trong class muc tieu.
- `AllEligible`: van phai con enrollment hop le trong khoang ngay.

Cancel request:

- Chi huy khi request van `Pending`.
- Chi huy truoc ngay `pauseFrom`.

Outcome:

- Chi set outcome khi request da `Approved`.
- Khong duoc set lai neu `OutcomeCompletedAt` da co.

Reassign equivalent class:

- Request phai `Approved`.
- `Outcome` phai la `ReassignEquivalentClass`.
- Registration phai thuoc cung hoc sinh.
- Registration khong duoc `Completed`/`Cancelled`.
- Track phai hop le.
- Track do phai dang co class assigned.
- `effectiveDate` phai sau `pauseTo`.
- Phai ton tai enrollment `Paused` phat sinh tu request nay.
- Lop moi phai:
  - ton tai
  - cung program
  - khac lop cu
  - chua full
  - status `Active` hoac `Recruiting`
  - khong trung lich

### 9.2 Cac truong hop tra loi

- `400` cho validation/business rule khong hop le
- `404` cho request/student/registration khong ton tai
- `409` cho xung dot state:
  - request da approve/reject/cancel
  - duplicate active request
  - khong con enrollment hop le
  - khong co paused enrollment de reassign
- `401` khi chua dang nhap
- `403` khi role khong duoc phep approve/reject/outcome/reassign

