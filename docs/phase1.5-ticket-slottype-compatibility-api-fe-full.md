# Phase 1.5 - FE API Full Doc (TicketType / SlotType / Compatibility)

## 1) Scope

Tai lieu nay tong hop cac thay doi Phase 1.5 lien quan:
- LearningTicketType
- SlotType (sectionType trong code business hien tai duoc goi la sessionType, con SlotType la loai runtime)
- TicketTypeCompatibility
- Cac API da mo rong them field `LearningTicketTypeId`, `SlotTypeId`
- Attendance + Learning Ticket compatibility behavior

## 2) Moi role duoc xem du lieu gi

- Admin: xem va thao tac toan bo du lieu trong scope API Phase 1.5.
- ManagementStaff: xem va thao tac phan lon API Phase 1.5 (tru endpoint delete chi Admin).
- Teacher: xem ticket balance/ledger/compatible ticket, mark/update attendance.
- Parent, Student: khong co quyen truy cap API master data Phase 1.5.

## 3) Pham vi du lieu (own / department / all)

- Admin: `all`
- ManagementStaff: `all`
- Teacher: thuc te hien tai la `all` tren cac endpoint duoc cap role (chua co loc own/department rieng trong cac API Phase 1.5)
- Parent/Student: khong ap dung cho API moi Phase 1.5

## 4) Cac hanh dong duoc phep (view/create/edit/approve/delete)

- Admin: `view`, `create`, `edit`, `delete`
- ManagementStaff: `view`, `create`, `edit`
- Teacher: `view`, `create/edit` attendance
- Parent/Student: khong co hanh dong trong scope API nay

## 5) Danh sach API

Ghi chu chung:
- Success format:
```json
{
  "isSuccess": true,
  "data": {}
}
```
- Error format (ProblemDetails):
```json
{
  "title": "Error.Code",
  "status": 400,
  "detail": "Error message",
  "errors": [
    {
      "code": "Error.Code",
      "description": "Error message"
    }
  ]
}
```

### 5.1 Learning Ticket Type APIs

#### API: Create LearningTicketType
- Endpoint + Method: `POST /api/learning-ticket-types`
- Mo ta: Tao loai ve hoc.
- Roles: `Admin`, `ManagementStaff`
- Body:
  - `code` (string, required, max 100)
  - `name` (string, required, max 255)
  - `description` (string, optional, max 500)
  - `isActive` (bool, optional, default true)
- Response success: `201 Created`, tra `LearningTicketTypeDto`.
- Response error:
  - `400` validation
  - `409 LearningTicketType.CodeExists`

#### API: Get LearningTicketType list
- Endpoint + Method: `GET /api/learning-ticket-types`
- Mo ta: Lay danh sach loai ve hoc.
- Roles: `Admin`, `ManagementStaff`
- Query:
  - `searchTerm` (string, optional)
  - `isActive` (bool, optional)
- Response success: `200 OK`, `data.items[]`.

#### API: Get LearningTicketType by id
- Endpoint + Method: `GET /api/learning-ticket-types/{id}`
- Roles: `Admin`, `ManagementStaff`
- Response error: `404 LearningTicketType.NotFound`.

#### API: Update LearningTicketType
- Endpoint + Method: `PUT /api/learning-ticket-types/{id}`
- Roles: `Admin`, `ManagementStaff`
- Body: giong create.
- Response error:
  - `404 LearningTicketType.NotFound`
  - `409 LearningTicketType.CodeExists`

#### API: Delete LearningTicketType
- Endpoint + Method: `DELETE /api/learning-ticket-types/{id}`
- Roles: `Admin`
- Response error:
  - `404 LearningTicketType.NotFound`
  - `409 LearningTicketType.InUse`

### 5.2 SlotType APIs

#### API: Create SlotType
- Endpoint + Method: `POST /api/slot-types`
- Roles: `Admin`, `ManagementStaff`
- Body:
  - `code` (string, required, max 100)
  - `name` (string, required, max 255)
  - `description` (string, optional, max 500)
  - `isActive` (bool, optional, default true)
- Response error: `409 SlotType.CodeExists`.

#### API: Get SlotType list
- Endpoint + Method: `GET /api/slot-types`
- Roles: `Admin`, `ManagementStaff`
- Query:
  - `searchTerm` (string, optional)
  - `isActive` (bool, optional)

#### API: Get SlotType by id
- Endpoint + Method: `GET /api/slot-types/{id}`
- Roles: `Admin`, `ManagementStaff`
- Response error: `404 SlotType.NotFound`.

#### API: Update SlotType
- Endpoint + Method: `PUT /api/slot-types/{id}`
- Roles: `Admin`, `ManagementStaff`
- Body: giong create.
- Response error:
  - `404 SlotType.NotFound`
  - `409 SlotType.CodeExists`

#### API: Delete SlotType
- Endpoint + Method: `DELETE /api/slot-types/{id}`
- Roles: `Admin`
- Response error:
  - `404 SlotType.NotFound`
  - `409 SlotType.InUse`

### 5.3 TicketTypeCompatibility APIs

#### API: Create compatibility mapping
- Endpoint + Method: `POST /api/ticket-type-compatibilities`
- Roles: `Admin`, `ManagementStaff`
- Body:
  - `learningTicketTypeId` (guid, required)
  - `slotTypeId` (guid, required)
  - `isCompatible` (bool, required)
- Response error:
  - `404 TicketTypeCompatibility.LearningTicketTypeNotFound`
  - `404 TicketTypeCompatibility.SlotTypeNotFound`
  - `409 TicketTypeCompatibility.MappingExists`

#### API: Get compatibility list
- Endpoint + Method: `GET /api/ticket-type-compatibilities`
- Roles: `Admin`, `ManagementStaff`
- Query:
  - `learningTicketTypeId` (guid, optional)
  - `slotTypeId` (guid, optional)

#### API: Get compatibility by id
- Endpoint + Method: `GET /api/ticket-type-compatibilities/{id}`
- Roles: `Admin`, `ManagementStaff`
- Response error: `404 TicketTypeCompatibility.NotFound`

#### API: Update compatibility mapping
- Endpoint + Method: `PUT /api/ticket-type-compatibilities/{id}`
- Roles: `Admin`, `ManagementStaff`
- Body: giong create.
- Response error:
  - `404 TicketTypeCompatibility.NotFound`
  - `404 TicketTypeCompatibility.LearningTicketTypeNotFound`
  - `404 TicketTypeCompatibility.SlotTypeNotFound`
  - `409 TicketTypeCompatibility.MappingExists`

#### API: Delete compatibility mapping
- Endpoint + Method: `DELETE /api/ticket-type-compatibilities/{id}`
- Roles: `Admin`
- Response error: `404 TicketTypeCompatibility.NotFound`

### 5.4 APIs duoc mo rong field trong Phase 1.5

#### API: Create TuitionPlan
- Endpoint + Method: `POST /api/tuition-plans`
- Roles: `Admin`, `ManagementStaff`
- Body field moi:
  - `learningTicketTypeId` (guid, optional)
- Behavior:
  - Neu co truyen `learningTicketTypeId`: phai ton tai va active.
  - Neu khong truyen: he thong van tao, ticket sinh ra se theo default policy (khong ep type).
- Response data co them:
  - `learningTicketTypeId`
  - `learningTicketTypeCode`

#### API: Update TuitionPlan
- Endpoint + Method: `PUT /api/tuition-plans/{id}`
- Roles: `Admin`, `ManagementStaff`
- Body field moi: `learningTicketTypeId` (guid, optional)
- Error:
  - `400 TuitionPlan.LearningTicketTypeNotFound`
  - `404 TuitionPlan.NotFound`

#### API: Create Class
- Endpoint + Method: `POST /api/classes`
- Roles: `Admin`, `ManagementStaff`
- Body field moi:
  - `slotTypeId` (guid, optional)
- Error:
  - `400 Class.SlotTypeNotFound` neu truyen id khong ton tai/inactive

#### API: Update Class
- Endpoint + Method: `PUT /api/classes/{id}`
- Roles: `Admin`, `ManagementStaff`
- Body field moi:
  - `slotTypeId` (guid, optional)
- Behavior:
  - Neu khong truyen `slotTypeId` thi giu gia tri cu cua class.

#### API: Create Session
- Endpoint + Method: `POST /api/sessions`
- Roles: `Admin`, `ManagementStaff`
- Body field moi:
  - `slotTypeId` (guid, optional)
- Behavior:
  - Neu khong truyen `slotTypeId`: session se fallback theo `class.slotTypeId`.
- Error:
  - `400 Session.SlotTypeNotFound` neu slot type id khong ton tai/inactive.

#### API: Update Session
- Endpoint + Method: `PUT /api/sessions/{sessionId}`
- Roles: `Admin`, `ManagementStaff`
- Body field moi:
  - `slotTypeId` (guid, optional)
- Behavior:
  - Neu khong truyen: giu `session.slotTypeId`; neu session chua co thi fallback `class.slotTypeId`.

#### API: Bulk Update Sessions By Class
- Endpoint + Method: `PUT /api/sessions/by-class`
- Roles: `Admin`, `ManagementStaff`
- Body field moi:
  - `slotTypeId` (guid, optional)
- Error:
  - `400 Session.SlotTypeNotFound`

### 5.5 Learning ticket compatibility APIs cho FE runtime

#### API: Get student compatible ticket for session
- Endpoint + Method:
  - `GET /api/students/{studentProfileId}/tickets/compatible?sessionId={sessionId}`
  - alias: `GET /api/students/{studentProfileId}/compatible-tickets?sessionId={sessionId}`
- Roles: `Admin`, `ManagementStaff`, `Teacher`
- Response data:
  - `compatible` (bool)
  - `ticketItemId` (guid?)
  - `ticketTypeId` (guid?)
  - `ticketTypeCode` (string?)
  - `reason` (string)

#### API: Get student ticket balance
- Endpoint + Method: `GET /api/students/{studentProfileId}/tickets/balance`
- Roles: `Admin`, `ManagementStaff`, `Teacher`
- Response data:
  - `studentProfileId`, `available`, `consumed`, `totalGranted`

#### API: Get student ticket ledger
- Endpoint + Method: `GET /api/students/{studentProfileId}/tickets/ledger`
- Roles: `Admin`, `ManagementStaff`, `Teacher`
- Response data: danh sach giao dich `Grant/Consume/Refund/Void/Adjustment`.

### 5.6 Attendance APIs lien quan compatibility

#### API: Mark attendance
- Endpoint + Method: `POST /api/attendance/{sessionId}`
- Roles: `Admin`, `Teacher`
- Body:
  - `attendances` (array, required)
  - item:
    - `studentProfileId` (guid, required)
    - `attendanceStatus` (enum, required)
    - `note` (string, optional)
- Response data:
  - `results[]` moi hoc sinh co:
    - `ticketConsumed`
    - `consumedQuantity`
    - `advanceLessonProgression`
    - `ticketBalance`
    - `ticketCompatibilityPassed`
    - `ticketCompatibilityReason`

#### API: Update attendance item
- Endpoint + Method: `PUT /api/attendance/{sessionId}/students/{studentProfileId}`
- Roles: `Admin`, `Teacher`
- Body:
  - `attendanceStatus` (enum, required)
  - `note` (string, optional)
- Response data co cac field compatibility tuong tu mark attendance.

## 6) Status definition

### 6.1 Danh sach status

- AttendanceStatus: `Present`, `Absent`, `Makeup`, `NotMarked`
- AbsenceType: `WithNotice24H`, `Under24H`, `NoNotice`, `LongTerm`
- SectionType (sessionType): `Normal`, `Review`, `Makeup`, `Remedial`, `Assessment`
- SessionStatus: `Scheduled`, `Completed`, `Cancelled`
- LearningTicketItemStatus: `Available`, `Consumed`, `Expired`, `Voided`
- LearningTicketTransactionType: `Grant`, `Consume`, `Refund`, `Void`, `Adjustment`

### 6.2 Y nghia status

- `Available`: ve con dung duoc.
- `Consumed`: ve da bi tru boi attendance.
- `Expired/Voided`: trang thai reserve cho policy mo rong.
- `Normal` sectionType: buoi hoc chinh, co the advance lesson.
- `Review/Remedial/Makeup/Assessment`: session dac thu, mac dinh khong advance lesson khi present.

### 6.3 Luong chuyen trang thai

- LearningTicketItem:
  - `Available -> Consumed` khi consume thanh cong.
  - `Consumed -> Available` khi rollback attendance.
- Session:
  - `Scheduled -> Completed` (complete API)
  - `Scheduled -> Cancelled` (cancel API)
- Attendance:
  - Co the update qua lai giua cac gia tri `Present/Absent/Makeup/NotMarked`.
  - Moi lan doi status se danh gia lai consume/refund theo policy.

## 7) Permission matrix theo role

| API group | Admin | ManagementStaff | Teacher | Parent | Student |
|---|---|---|---|---|---|
| LearningTicketType CRUD | Y (full) | Y (khong delete) | N | N | N |
| SlotType CRUD | Y (full) | Y (khong delete) | N | N | N |
| TicketTypeCompatibility CRUD | Y (full) | Y (khong delete) | N | N | N |
| TuitionPlan create/update + field LearningTicketTypeId | Y | Y | N | N | N |
| Class create/update + field SlotTypeId | Y | Y | N | N | N |
| Session create/update/by-class + SlotTypeId | Y | Y | N | N | N |
| Student ticket balance/ledger/compatible | Y | Y | Y | N | N |
| Attendance mark/update | Y | N | Y | N | N |

## 8) Validation rule

- `LearningTicketType.code`: required, max 100, unique.
- `SlotType.code`: required, max 100, unique.
- `name`: required, max 255.
- `description`: max 500.
- `TicketTypeCompatibility`: unique theo cap `(learningTicketTypeId, slotTypeId)`.
- `learningTicketTypeId`, `slotTypeId` neu duoc truyen vao cac API mo rong phai:
  - khong duoc `Guid.Empty`
  - ton tai va active (voi API co check active).

## 9) Rule kiem tra du lieu (business rules)

- Runtime compatibility policy hien tai la `default-pass`:
  - Neu ticket khong co type hoac chua co mapping explicit thi mac dinh compatible.
- Chon ve khi co nhieu ve compatible:
  - Uu tien `match exact slotType.code == ticketType.code`
  - Neu khong co exact match: fallback FIFO theo ticket `CreatedAt` (cu nhat dung truoc).
- Truong hop incompatible trong mark attendance:
  - Khong reject ca request.
  - Xu ly theo tung student item, item nao khong co ve compatible thi `ticketConsumed=false`, co `ticketCompatibilityReason`.

## 10) Cac truong hop tra loi

- `400 BadRequest`:
  - Validation format field
  - `Class.SlotTypeNotFound`
  - `Session.SlotTypeNotFound`
  - `TuitionPlan.LearningTicketTypeNotFound`
  - `Attendance.FutureSessionNotAllowed`
  - `Attendance.UpdateWindowClosed`
  - `Attendance.ApprovedLeaveLocked`
- `404 NotFound`:
  - `LearningTicketType.NotFound`
  - `SlotType.NotFound`
  - `TicketTypeCompatibility.NotFound`
  - `Attendance.NotFound`
- `409 Conflict`:
  - `LearningTicketType.CodeExists`
  - `SlotType.CodeExists`
  - `TicketTypeCompatibility.MappingExists`
  - `LearningTicketType.InUse`
  - `SlotType.InUse`
- `401/403`:
  - Khong co token hoac role khong du quyen.

## 11) Notes cho FE

- Neu chua can phan loai nang cao, FE co the de `learningTicketTypeId` / `slotTypeId` la null, he thong van chay theo default policy hien tai.
- Khi can bat rule nang cao, FE co the:
  - CRUD master data `LearningTicketType`, `SlotType`
  - Cau hinh `TicketTypeCompatibility`
  - Gan id vao TuitionPlan/Class/Session de kich hoat logic tuong thich theo ma tran.
