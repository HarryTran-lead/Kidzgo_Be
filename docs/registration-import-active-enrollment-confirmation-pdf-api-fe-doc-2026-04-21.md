# Tài Liệu API FE - Registration Import Active + Enrollment Confirmation PDF - 2026-04-21

Tài liệu này mô tả 3 API trong [RegistrationController.cs](D:/KLTN/Kidzgo_Be/Kidzgo.API/Controllers/RegistrationController.cs):

- `POST /api/registrations/import-active`
- `GET /api/registrations/{id}/enrollment-confirmation-pdf`
- `GET /api/registrations/{id}/enrollment-confirmation-pdf/history`

Mục tiêu:

- FE import học sinh đang học giữa chừng từ hệ thống cũ.
- FE preview dữ liệu PDF xác nhận nhập học trước khi generate file.
- FE lấy lịch sử các file PDF đã generate.

## Tổng quan role và phạm vi dữ liệu

Tất cả API trong controller yêu cầu user đã đăng nhập vì controller có `[Authorize]`.

| Role | Dữ liệu được xem | Phạm vi dữ liệu | Hành động được phép |
| --- | --- | --- | --- |
| Admin | Tất cả registration import active, preview PDF, lịch sử PDF | `all` | `view`, `create` |
| ManagementStaff | Tất cả registration import active, preview PDF, lịch sử PDF | `all` | `view`, `create` |
| Teacher | Không được truy cập | `none` | `none` |
| Parent | Không được truy cập | `none` | `none` |
| Student | Không được truy cập | `none` | `none` |
| Anonymous | Không được truy cập | `none` | `none` |

Ghi chú:

- Hiện tại BE chưa có logic scope `own` hoặc `department` cho 3 API này.
- `GET preview` và `GET history` chỉ cho `Admin`, `ManagementStaff`, không mở cho `Teacher`.

## Định dạng response chung

Response success từ `MatchOk()` / `MatchCreated()` được bọc trong `ApiResult<T>`:

```json
{
  "isSuccess": true,
  "data": {}
}
```

Với `POST /api/registrations/import-active`, HTTP status là `201 Created`.

Error từ domain result trả về dạng `ProblemDetails`:

```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.1",
  "title": "Registration.InvalidEntryType",
  "status": 400,
  "detail": "Invalid entry type: makeup. Allowed values are immediate, wait, retake."
}
```

Các lỗi FluentValidation cũng trả `ProblemDetails`; nếu là `ValidationError` sẽ có thêm `extensions.errors`.

## Danh sách API

### 1. POST `/api/registrations/import-active`

Dùng để import một registration đang học giữa chừng từ hệ thống cũ. API này chỉ tạo registration với số buổi đã học/còn lại, chưa tự động xếp lớp.

Sau khi import thành công, FE cần gọi tiếp `POST /api/registrations/{id}/assign-class` để bắt đầu theo dõi từ buổi sắp tới.

Roles: `Admin`, `ManagementStaff`

Phạm vi dữ liệu: `all`

Body JSON:

```json
{
  "studentProfileId": "11111111-1111-1111-1111-111111111111",
  "branchId": "22222222-2222-2222-2222-222222222222",
  "programId": "33333333-3333-3333-3333-333333333333",
  "tuitionPlanId": "44444444-4444-4444-4444-444444444444",
  "expectedStartDate": "2026-04-27T00:00:00Z",
  "actualStartDate": "2026-03-15T00:00:00Z",
  "preferredSchedule": "Thu 2, Thu 4",
  "note": "Import hoc sinh dang hoc giua chung",
  "usedSessions": 12,
  "remainingSessions": 24
}
```

Các field trong body:

| Field | Type | Required | Mô tả |
| --- | --- | --- | --- |
| `studentProfileId` | `Guid` | Yes | Student profile cần import. |
| `branchId` | `Guid` | Yes | Chi nhánh. Phải tồn tại và active. |
| `programId` | `Guid` | Yes | Program chính. Phải tồn tại và active. |
| `tuitionPlanId` | `Guid` | Yes | Tuition plan của program. Phải tồn tại, active và thuộc đúng `programId`. |
| `expectedStartDate` | `DateTime?` | No | Ngày dự kiến bắt đầu học. Chỉ để lưu metadata. |
| `actualStartDate` | `DateTime` | Yes | Ngày thực tế đã bắt đầu học ở hệ thống cũ. Không được ở tương lai. |
| `preferredSchedule` | `string?` | No | Lịch mong muốn. |
| `note` | `string?` | No | Ghi chú import. |
| `usedSessions` | `int` | Yes | Số buổi đã học. Phải `>= 0`. |
| `remainingSessions` | `int` | Yes | Số buổi còn lại. Phải `> 0`. |

Response thành công:

```json
{
  "isSuccess": true,
  "data": {
    "id": "55555555-5555-5555-5555-555555555555",
    "studentProfileId": "11111111-1111-1111-1111-111111111111",
    "branchId": "22222222-2222-2222-2222-222222222222",
    "programId": "33333333-3333-3333-3333-333333333333",
    "programName": "LMS",
    "tuitionPlanId": "44444444-4444-4444-4444-444444444444",
    "tuitionPlanName": "Goi 3 thang",
    "registrationDate": "2026-04-21T02:00:00Z",
    "expectedStartDate": "2026-04-27T00:00:00Z",
    "actualStartDate": "2026-03-15T00:00:00Z",
    "preferredSchedule": "Thu 2, Thu 4",
    "note": "Import hoc sinh dang hoc giua chung",
    "status": "New",
    "totalSessions": 36,
    "usedSessions": 12,
    "remainingSessions": 24,
    "createdAt": "2026-04-21T02:00:00Z"
  }
}
```

Response lỗi:

| HTTP | Code/message | Khi nào |
| --- | --- | --- |
| 400 | FluentValidation message | Thiếu `studentProfileId`, `branchId`, `programId`, `tuitionPlanId`, `actualStartDate`; hoặc `usedSessions < 0`; hoặc `remainingSessions <= 0`. |
| 400 | `Registration.ActualStartDateInFuture` | `actualStartDate` lớn hơn thời điểm hiện tại. |
| 400 | `Registration.ImportSessionCountMismatch` | `usedSessions + remainingSessions` không bằng `tuitionPlan.TotalSessions`. |
| 404 | `Registration.StudentNotFound` | `studentProfileId` không tồn tại hoặc không phải student profile. |
| 404 | `Registration.BranchNotFound` | `branchId` không tồn tại hoặc branch không active. |
| 404 | `Registration.ProgramNotFound` | `programId` không tồn tại hoặc program không active. |
| 404 | `Registration.TuitionPlanNotFound` | `tuitionPlanId` không tồn tại, không active, hoặc không thuộc `programId`. |
| 409 | `Registration.AlreadyExists` | Student đã có registration active cho program này. |
| 401 | Unauthorized | Chưa đăng nhập hoặc token không hợp lệ. |
| 403 | Forbidden | Role không thuộc `Admin`, `ManagementStaff`. |

### 2. GET `/api/registrations/{id}/enrollment-confirmation-pdf`

Dùng để preview dữ liệu PDF xác nhận nhập học hiện tại của registration trước khi generate file.

API này:

- không tạo file PDF mới
- resolve `track`
- resolve `formType`
- trả luôn metadata file active gần nhất nếu đã có file generate trước đó

Roles: `Admin`, `ManagementStaff`

Phạm vi dữ liệu: `all`

Query params:

| Field | Type | Required | Default | Mô tả |
| --- | --- | --- | --- | --- |
| `track` | `string` | No | `primary` | `primary` hoặc `secondary`. Nếu giá trị lạ, BE normalize về `primary`. |
| `formType` | `string` | No | `auto` | `auto`, `new`, `newStudent`, `continuing`, `continuingStudent`, `continuing-student`, `renewal`, `re-enroll`, ... |

Response thành công:

```json
{
  "isSuccess": true,
  "data": {
    "registrationId": "55555555-5555-5555-5555-555555555555",
    "enrollmentId": "66666666-6666-6666-6666-666666666666",
    "trackRequested": "primary",
    "trackResolved": "primary",
    "formTypeRequested": "auto",
    "formTypeResolved": "newStudent",
    "canGenerate": true,
    "paymentSettingScope": "branch",
    "warnings": ["MissingParentContact"],
    "activePdf": {
      "pdfRecordId": "77777777-7777-7777-7777-777777777777",
      "pdfUrl": "https://storage.example.com/file.pdf",
      "generatedAt": "2026-04-21T03:00:00Z",
      "generatedBy": "88888888-8888-8888-8888-888888888888",
      "generatedByName": "Staff A",
      "isActive": true,
      "hasSnapshot": true
    },
    "preview": {
      "studentName": "Nguyen Van B",
      "studentDateOfBirth": "2018-03-24",
      "parentName": "Nguyen Van A",
      "parentPhoneNumber": "0909000000",
      "branchName": "HCM",
      "branchAddress": "123 ABC",
      "branchPhoneNumber": "028...",
      "programName": "LMS",
      "programCode": "LMS",
      "classCode": "LMS01",
      "classTitle": "Lop Kem LMS",
      "teacherName": "Teacher A",
      "enrollDate": "2026-04-27",
      "firstStudyDate": "2026-04-27",
      "expectedEndDate": "2026-07-30",
      "studyDaySummary": "Thu 2, Thu 4",
      "tuitionPlanName": "Goi 3 thang",
      "courseDurationText": "36 buoi (27/04/2026 - 30/07/2026)",
      "totalSessions": 36,
      "tuitionAmount": 30000000,
      "unitPriceSession": 833333.33,
      "discountAmount": 0,
      "materialFee": 0,
      "totalPayment": 30000000,
      "currency": "VND",
      "track": "primary",
      "entryType": "Immediate",
      "generatedAt": "2026-04-21T03:10:00Z",
      "issuedByName": "Staff A",
      "paymentMethod": "Chuyen khoan",
      "paymentAccountName": "TRINH DUC ANH",
      "paymentAccountNumber": "0898498720",
      "paymentBankName": "MB",
      "paymentTransferContent": "abcon - LMS01",
      "paymentQrUrl": "https://img.vietqr.io/...",
      "headerLogoUrl": "https://storage.example.com/logo.png",
      "reconciliation": null
    }
  }
}
```

`preview.reconciliation` chỉ có dữ liệu khi `formTypeResolved = continuingStudent`.

Các field chính trong response:

| Field | Type | Mô tả |
| --- | --- | --- |
| `registrationId` | `Guid` | Registration được preview. |
| `enrollmentId` | `Guid` | Active enrollment của `track` được chọn. |
| `trackRequested` | `string` | Giá trị FE gửi lên. |
| `trackResolved` | `string` | Giá trị BE normalize và dùng thực tế. |
| `formTypeRequested` | `string` | Giá trị FE gửi lên. |
| `formTypeResolved` | `string` | `newStudent` hoặc `continuingStudent`. |
| `canGenerate` | `bool` | Nếu preview success thì hiện tại luôn là `true`. |
| `paymentSettingScope` | `string` | `branch`, `global`, `none`. |
| `warnings` | `array<string>` | Danh sách cảnh báo không chặn generate. |
| `activePdf` | `object?` | File PDF active gần nhất đã generate trước đó; có thể `null`. |
| `preview` | `object` | Toàn bộ dữ liệu sống hiện tại dùng để render PDF. |

Các warning hiện tại BE có thể trả:

| Warning | Ý nghĩa |
| --- | --- |
| `MissingParentContact` | Thiếu tên/số điện thoại phụ huynh. |
| `FirstStudyDateMissing` | Chưa resolve được buổi học đầu tiên từ assignment. |
| `PaymentSettingMissing` | Không tìm thấy payment setting ở branch hoặc global. |
| `PaymentSettingFallbackGlobal` | Không có setting theo branch, đang fallback về global setting. |

Response lỗi:

| HTTP | Code/message | Khi nào |
| --- | --- | --- |
| 400 | `Registration.InvalidEnrollmentConfirmationPdfFormType` | `formType` không thuộc nhóm support. |
| 404 | `Registration.NotFound` | Không tìm thấy registration theo `id`. |
| 404 | `Registration.EnrollmentNotFound` | Registration có tồn tại nhưng chưa có active enrollment cho `track` được resolve. |
| 401 | Unauthorized | Chưa đăng nhập hoặc token không hợp lệ. |
| 403 | Forbidden | Role không thuộc `Admin`, `ManagementStaff`. |

### 3. GET `/api/registrations/{id}/enrollment-confirmation-pdf/history`

Dùng để lấy lịch sử các file PDF xác nhận nhập học đã được generate cho một registration.

Roles: `Admin`, `ManagementStaff`

Phạm vi dữ liệu: `all`

Query params:

| Field | Type | Required | Default | Mô tả |
| --- | --- | --- | --- | --- |
| `track` | `string?` | No | null | `primary` hoặc `secondary`. Nếu null thì lấy cả 2 track. Nếu giá trị lạ, BE normalize về `primary`. |
| `formType` | `string?` | No | null | Có thể truyền `auto`, `new`, `newStudent`, `continuing`, `continuingStudent`, ... Nếu `auto` thì BE coi như không filter theo form type. |
| `pageNumber` | `int` | No | 1 | Trang cần lấy. |
| `pageSize` | `int` | No | 10 | Số item mỗi trang. |

Response thành công:

```json
{
  "isSuccess": true,
  "data": {
    "pdfs": {
      "items": [
        {
          "pdfRecordId": "77777777-7777-7777-7777-777777777777",
          "registrationId": "55555555-5555-5555-5555-555555555555",
          "enrollmentId": "66666666-6666-6666-6666-666666666666",
          "track": "primary",
          "formType": "newStudent",
          "pdfUrl": "https://storage.example.com/file.pdf",
          "generatedAt": "2026-04-21T03:00:00Z",
          "generatedBy": "88888888-8888-8888-8888-888888888888",
          "generatedByName": "Staff A",
          "isActive": true,
          "hasSnapshot": true,
          "studentName": "Nguyen Van B",
          "classCode": "LMS01",
          "classTitle": "Lop Kem LMS",
          "programName": "LMS"
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

Các field trong item:

| Field | Type | Mô tả |
| --- | --- | --- |
| `pdfRecordId` | `Guid` | ID bản ghi PDF. |
| `registrationId` | `Guid` | Registration gốc. |
| `enrollmentId` | `Guid` | Enrollment gắn với file PDF đó. |
| `track` | `string` | `primary` hoặc `secondary`. |
| `formType` | `string` | `newStudent` hoặc `continuingStudent`. |
| `pdfUrl` | `string` | Download URL sau khi qua file storage service. |
| `generatedAt` | `DateTime` | Thời điểm generate file. |
| `generatedBy` | `Guid?` | User generate file. |
| `generatedByName` | `string?` | Tên user generate file. |
| `isActive` | `bool` | File active hiện tại hay không. |
| `hasSnapshot` | `bool` | Bản ghi có snapshot JSON hay không. |
| `studentName` | `string?` | Tên học sinh. |
| `classCode` | `string?` | Mã lớp. |
| `classTitle` | `string?` | Tên lớp. |
| `programName` | `string?` | Tên chương trình. |

Response lỗi:

| HTTP | Code/message | Khi nào |
| --- | --- | --- |
| 400 | `Registration.InvalidEnrollmentConfirmationPdfFormType` | `formType` không thuộc nhóm support. |
| 404 | `Registration.NotFound` | Không tìm thấy registration theo `id`. |
| 401 | Unauthorized | Chưa đăng nhập hoặc token không hợp lệ. |
| 403 | Forbidden | Role không thuộc `Admin`, `ManagementStaff`. |

Ghi chú:

- Nếu registration tồn tại nhưng chưa từng generate file nào thì API vẫn thành công và trả `items = []`.
- Hiện tại API không có validator riêng cho `pageNumber` / `pageSize`; FE nên gửi giá trị hợp lệ (`pageNumber >= 1`, `pageSize > 0`).

## Định nghĩa status

### 1. Registration status liên quan tới `POST /api/registrations/import-active`

`import-active` hiện tại tạo registration với status cố định là `New`.

| Status | Ý nghĩa |
| --- | --- |
| `New` | Registration đã được import vào hệ thống nhưng chưa xếp lớp, chưa tạo enrollment theo dõi trong hệ thống mới. |

Luồng chuyển trạng thái:

1. FE gọi `POST /api/registrations/import-active`.
2. BE tạo registration với `status = New`.
3. FE gọi tiếp `POST /api/registrations/{id}/assign-class`.
4. Sau API assign class, status có thể chuyển sang `Studying`, `WaitingForClass` hoặc `ClassAssigned` tùy `entryType` và track thực tế.

### 2. Form type liên quan tới 2 API PDF

Đây không phải DB lifecycle status, nhưng là giá trị FE cần hiểu để render đúng form.

| Form type | Ý nghĩa |
| --- | --- |
| `newStudent` | Form dành cho học sinh mới, chưa có lịch sử học trước đó trong ngữ cảnh resolve của BE. |
| `continuingStudent` | Form dành cho học sinh học tiếp / renewal / upgrade / có lịch sử học trước đó. |

Luồng resolve:

1. Nếu FE gửi `formType = auto`, BE tự resolve.
2. Nếu registration có `OriginalRegistrationId` hoặc `OperationType = Renewal/Upgrade`, BE ưu tiên `continuingStudent`.
3. Nếu không, BE tìm registration cũ của cùng student để quyết định `newStudent` hay `continuingStudent`.

### 3. Payment setting scope trong preview

| Value | Ý nghĩa |
| --- | --- |
| `branch` | Đang dùng payment setting riêng của branch. |
| `global` | Không có setting theo branch, đang fallback sang global setting. |
| `none` | Không tìm thấy payment setting active nào. |

### 4. Cờ trạng thái file PDF trong preview/history

| Field | Giá trị | Ý nghĩa |
| --- | --- | --- |
| `isActive` | `true/false` | File PDF active hiện tại hay không. |
| `hasSnapshot` | `true/false` | Có snapshot JSON đã lưu cùng bản ghi PDF hay không. |

## Permission matrix theo role

| API | Admin | ManagementStaff | Teacher | Parent | Student | Anonymous |
| --- | --- | --- | --- | --- | --- | --- |
| `POST /api/registrations/import-active` | Yes | Yes | No | No | No | No |
| `GET /api/registrations/{id}/enrollment-confirmation-pdf` | Yes | Yes | No | No | No | No |
| `GET /api/registrations/{id}/enrollment-confirmation-pdf/history` | Yes | Yes | No | No | No | No |

## Validation rule tổng hợp

### A. Rule cho `POST /api/registrations/import-active`

| Rule | Kết quả khi sai |
| --- | --- |
| User phải đăng nhập | 401 |
| Role phải là `Admin`, `ManagementStaff` | 403 |
| `studentProfileId`, `branchId`, `programId`, `tuitionPlanId` bắt buộc | 400 |
| `actualStartDate` bắt buộc | 400 |
| `usedSessions >= 0` | 400 |
| `remainingSessions > 0` | 400 |
| Student profile phải tồn tại và là student | 404 `Registration.StudentNotFound` |
| Branch phải tồn tại và active | 404 `Registration.BranchNotFound` |
| Program phải tồn tại, active, không deleted | 404 `Registration.ProgramNotFound` |
| Tuition plan phải tồn tại, active và thuộc program | 404 `Registration.TuitionPlanNotFound` |
| Student chưa có registration active cho program này | 409 `Registration.AlreadyExists` |
| `actualStartDate` không được ở tương lai | 400 `Registration.ActualStartDateInFuture` |
| `usedSessions + remainingSessions == tuitionPlan.TotalSessions` | 400 `Registration.ImportSessionCountMismatch` |

### B. Rule cho `GET /api/registrations/{id}/enrollment-confirmation-pdf`

| Rule | Kết quả khi sai |
| --- | --- |
| User phải đăng nhập | 401 |
| Role phải là `Admin`, `ManagementStaff` | 403 |
| Registration phải tồn tại | 404 `Registration.NotFound` |
| `formType` phải thuộc nhóm support hoặc `auto` | 400 `Registration.InvalidEnrollmentConfirmationPdfFormType` |
| Track được resolve phải có active enrollment tương ứng | 404 `Registration.EnrollmentNotFound` |

### C. Rule cho `GET /api/registrations/{id}/enrollment-confirmation-pdf/history`

| Rule | Kết quả khi sai |
| --- | --- |
| User phải đăng nhập | 401 |
| Role phải là `Admin`, `ManagementStaff` | 403 |
| Registration phải tồn tại | 404 `Registration.NotFound` |
| `formType` phải thuộc nhóm support; `auto` được coi như không filter | 400 `Registration.InvalidEnrollmentConfirmationPdfFormType` |
| `track` nếu truyền giá trị lạ sẽ bị normalize về `primary` | Không lỗi |
| Không có file history | Success, `items = []` |

## Lưu ý cho FE

### 1. Luồng import active

`POST /api/registrations/import-active` chỉ tạo registration và snapshot số buổi học hiện tại. API này không:

- tạo enrollment
- xếp lớp
- backfill attendance cũ
- generate PDF

Luồng đúng:

1. FE gọi `POST /api/registrations/import-active`.
2. BE trả registration mới với status `New`.
3. FE gọi tiếp `POST /api/registrations/{id}/assign-class`.
4. Từ buổi sắp tới trở đi hệ thống mới bắt đầu theo dõi assignment/attendance/session consumption.

### 2. Luồng preview PDF

`GET /api/registrations/{id}/enrollment-confirmation-pdf` dùng live data hiện tại, không phải snapshot cũ.

Nếu FE muốn xem file đã generate trước đó thì dùng history API.

### 3. Luồng history PDF

`GET /api/registrations/{id}/enrollment-confirmation-pdf/history` trả lịch sử bản ghi generate, có phân trang.

Nếu một registration có nhiều lần generate lại PDF, history sẽ lưu nhiều record; record active hiện tại được đánh dấu `isActive = true`.
