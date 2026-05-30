# Hướng Dẫn API Lesson Plan

Tài liệu này mô tả đầy đủ flow hiện tại của `lesson plan` và `lesson plan template` sau khi đã rút gọn API theo đúng nghiệp vụ đang dùng.

## 1. Mục tiêu nghiệp vụ

### 1.1. Lesson plan template
- Là syllabus chuẩn của toàn bộ chương trình.
- Mỗi `Program` có nhiều template, mỗi template tương ứng với `1 SessionIndex`.
- Template được quản lý bởi `Admin` và `ManagementStaff`.
- Dữ liệu template có thể được:
  - tạo thủ công bằng API create,
  - cập nhật thủ công bằng API update,
  - hoặc import hàng loạt từ file `xlsx/xls/csv`.

### 1.2. Lesson plan
- Là giáo án thực tế của một buổi học cụ thể.
- Gắn với `Class + Session`.
- Teacher chỉ được thao tác lesson plan của các session mình dạy.
- Nội dung teacher cập nhật chỉ áp dụng cho buổi đó, lớp đó, lesson plan đó; không ghi đè template gốc.

### 1.3. Flow tổng quát
1. Admin/Staff import syllabus chuẩn vào `lesson_plan_templates`.
2. Mỗi template được map theo `ProgramId + SessionIndex`.
3. Frontend teacher gọi `GET /api/lesson-plans/classes/{classId}/syllabus` để lấy toàn bộ syllabus của lớp.
4. Khi tạo lesson plan:
   - backend tự tìm template theo `Program + SessionIndex` nếu `templateId = null`,
   - backend tự copy `template.SyllabusContent` sang `plannedContent` nếu `plannedContent` rỗng.
5. Teacher cập nhật nội dung dạy thực tế bằng `PUT /api/lesson-plans/{id}`.

## 2. Những API đang còn sử dụng

### 2.1. Nhóm lesson plan template
- `POST /api/lesson-plan-templates`
- `GET /api/lesson-plan-templates`
- `GET /api/lesson-plan-templates/{id}`
- `PUT /api/lesson-plan-templates/{id}`
- `POST /api/lesson-plan-templates/import`

### 2.2. Nhóm lesson plan
- `GET /api/lesson-plans/classes/{classId}/syllabus`
- `POST /api/lesson-plans`
- `GET /api/lesson-plans/{id}`
- `PUT /api/lesson-plans/{id}`

## 3. Những API đã bỏ

Các API sau đã bị bỏ khỏi controller, frontend không được gọi lại:
- `DELETE /api/lesson-plan-templates/{id}`
- `GET /api/lesson-plans`
- `DELETE /api/lesson-plans/{id}`
- `GET /api/lesson-plans/{id}/template`
- `PATCH /api/lesson-plans/{id}/actual`

## 4. Cấu trúc dữ liệu chính

### 4.1. LessonPlanTemplate
- `programId`
- `level`
- `title`
- `sessionIndex`
- `syllabusMetadata`
- `syllabusContent`
- `sourceFileName`
- `attachmentUrl`
- `attachment`
- `isActive`

### 4.2. Ý nghĩa các field mới
- `syllabusMetadata`: metadata chung của cả sheet/file syllabus.
- `syllabusContent`: JSON nội dung chuẩn của một buổi học.
- `sourceFileName`: tên file import gốc.

### 4.3. Ví dụ `syllabusContent`
```json
{
  "sessionIndex": 1,
  "title": "FLYERS 1 - Session 1",
  "dateLabel": "09/05",
  "teacherName": "Ms Sarah",
  "notes": [
    "Assigned by teachers"
  ],
  "activities": [
    {
      "time": "15 mins",
      "book": "Grapeseed",
      "skills": "Listening",
      "classwork": "Unit 5",
      "requiredMaterials": "Pages 88,89",
      "homeworkRequiredMaterials": "HOMEWORK",
      "extra": "Handbook 88,89"
    }
  ]
}
```

Frontend nên parse JSON này để render theo UI; không nên coi đây là plain text thông thường.

## 5. Quy ước response

### 5.1. Response thành công
Các API dùng `MatchOk` hoặc `MatchCreated`, nên response thành công có dạng:

```json
{
  "isSuccess": true,
  "data": {
  }
}
```

### 5.2. Response lỗi nghiệp vụ
Các lỗi domain đang được map qua `Results.Problem(...)`, nên thường có dạng:

```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.4",
  "title": "LessonPlan.ClassNotFound",
  "status": 404,
  "detail": "Class with Id = 'guid' was not found",
  "traceId": "00-..."
}
```

### 5.3. Mapping status code hiện tại
- `Validation` -> `400 Bad Request`
- `NotFound` -> `404 Not Found`
- `Conflict` -> `409 Conflict`

### 5.4. Lỗi xác thực và phân quyền ở tầng ASP.NET
Ngoài lỗi domain ở trên, controller còn dùng `[Authorize]` và `[Authorize(Roles = ...)]`, nên frontend phải xử lý thêm:
- `401 Unauthorized`: chưa đăng nhập hoặc token không hợp lệ
- `403 Forbidden`: có token nhưng không đủ role

### 5.5. Lưu ý quan trọng về lỗi “Unauthorized” trong handler
Một số handler đang trả lỗi domain:
- `LessonPlan.Unauthorized`
- `LessonPlanTemplate.Unauthorized`

Hai lỗi này hiện được map thành `400 Bad Request`, không phải `401/403`, vì chúng đang được khai báo là `Validation`.

Frontend cần phân biệt:
- `401/403`: lỗi auth framework
- `400` với `title = LessonPlan.Unauthorized` hoặc `LessonPlanTemplate.Unauthorized`: lỗi phân quyền nghiệp vụ

## 6. Quy ước cập nhật dữ liệu

### 6.1. Với `PUT /api/lesson-plans/{id}`
- Chỉ field nào khác `null` mới được cập nhật.
- Nếu field gửi lên là `null`, backend sẽ bỏ qua field đó, không xóa dữ liệu cũ.

### 6.2. Với `PUT /api/lesson-plan-templates/{id}`
- Tương tự, chỉ field nào khác `null` mới được cập nhật.
- `SessionIndex` nếu có truyền thì phải > 0.

Điều này rất quan trọng cho frontend:
- Nếu muốn giữ nguyên dữ liệu cũ thì gửi `null`.
- Hiện tại API chưa hỗ trợ clear field về `null` một cách tường minh.

## 7. Chi tiết API LessonPlanTemplate

### 7.1. Tạo template thủ công

`POST /api/lesson-plan-templates`

Role:
- `ManagementStaff`
- `Admin`

Mục đích:
- Tạo một template session thủ công cho một program.

Request body:
```json
{
  "programId": "guid",
  "level": "Flyers",
  "title": "FLYERS 1 - Session 1",
  "sessionIndex": 1,
  "syllabusMetadata": "{\"title\":\"Syllabus Flyers 1\"}",
  "syllabusContent": "{\"sessionIndex\":1,\"activities\":[]}",
  "sourceFileName": "Syllabus - XIN CHAO ENGLISH.xlsx",
  "attachmentUrl": "https://cdn.example.com/syllabus-flyers-1.xlsx",
  "attachment": "https://..."
}
```

Response thành công:
- `201 Created`

Ví dụ:
```json
{
  "isSuccess": true,
  "data": {
    "id": "guid",
    "programId": "guid",
    "level": "Flyers",
    "title": "FLYERS 1 - Session 1",
    "sessionIndex": 1,
    "syllabusMetadata": "{\"title\":\"Syllabus Flyers 1\"}",
    "syllabusContent": "{\"sessionIndex\":1,\"activities\":[]}",
    "sourceFileName": "Syllabus - XIN CHAO ENGLISH.xlsx",
    "attachment": "https://...",
    "isActive": true,
    "createdAt": "2026-04-01T10:00:00Z"
  }
}
```

Lỗi có thể trả:
- `401 Unauthorized`
- `403 Forbidden`
- `404 Not Found`
  - `LessonPlanTemplate.ProgramNotFound`
  - Message: `Program with Id = '...' was not found or inactive`
- `400 Bad Request`
  - `LessonPlanTemplate.SessionIndexRequired`
  - Message: `SessionIndex is required and must be greater than 0`
- `409 Conflict`
  - `LessonPlanTemplate.DuplicateSessionIndex`
  - Message: `Template with SessionIndex X already exists for Program Y`

### 7.2. Lấy danh sách template

`GET /api/lesson-plan-templates?programId=&level=&title=&isActive=&includeDeleted=false&pageNumber=1&pageSize=10`

Role:
- `ManagementStaff`
- `Admin`

Mục đích:
- Lấy danh sách template theo bộ lọc.

Query params:
- `programId`: lọc theo chương trình
- `level`: lọc theo level
- `title`: lọc theo title
- `isActive`: lọc theo trạng thái
- `includeDeleted`: có lấy bản ghi đã xóa mềm hay không
- `pageNumber`
- `pageSize`

Lưu ý:
- `title` hiện đang được so sánh theo dạng bằng tuyệt đối sau khi lowercase, không phải tìm kiếm contains.

Response thành công:
- `200 OK`

Ví dụ:
```json
{
  "isSuccess": true,
  "data": {
    "templates": {
      "items": [
        {
          "id": "guid",
          "programId": "guid",
          "programName": "FLYERS 1",
          "title": "FLYERS 1 - Session 1",
          "level": "Flyers",
          "sessionIndex": 1,
          "syllabusMetadata": "{\"title\":\"Syllabus Flyers 1\"}",
          "syllabusContent": "{\"sessionIndex\":1,\"activities\":[]}",
          "sourceFileName": "Syllabus - XIN CHAO ENGLISH.xlsx",
          "attachment": "https://...",
          "isActive": true,
          "createdBy": "guid",
          "createdByName": "Academic Staff",
          "createdAt": "2026-04-01T10:00:00Z",
          "usedCount": 5
        }
      ],
      "pageNumber": 1,
      "pageSize": 10,
      "totalCount": 1,
      "totalPages": 1
    }
  }
}
```

Lỗi có thể trả:
- `401 Unauthorized`
- `403 Forbidden`

### 7.3. Lấy chi tiết một template

`GET /api/lesson-plan-templates/{id}`

Role:
- `ManagementStaff`
- `Admin`

Response thành công:
- `200 OK`

Ví dụ:
```json
{
  "isSuccess": true,
  "data": {
    "id": "guid",
    "programId": "guid",
    "programName": "FLYERS 1",
    "level": "Flyers",
    "title": "FLYERS 1 - Session 1",
    "sessionIndex": 1,
    "syllabusMetadata": "{\"title\":\"Syllabus Flyers 1\"}",
    "syllabusContent": "{\"sessionIndex\":1,\"activities\":[]}",
    "sourceFileName": "Syllabus - XIN CHAO ENGLISH.xlsx",
    "attachment": "https://...",
    "isActive": true,
    "createdBy": "guid",
    "createdByName": "Academic Staff",
    "createdAt": "2026-04-01T10:00:00Z",
    "usedCount": 5
  }
}
```

Lỗi có thể trả:
- `401 Unauthorized`
- `403 Forbidden`
- `404 Not Found`
  - `LessonPlanTemplate.NotFound`
  - Message: `Lesson plan template with Id = '...' was not found`

### 7.4. Cập nhật template

`PUT /api/lesson-plan-templates/{id}`

Role:
- `ManagementStaff`
- `Admin`

Request body:
```json
{
  "level": "Flyers",
  "title": "FLYERS 1 - Session 1 (updated)",
  "sessionIndex": 1,
  "syllabusMetadata": "{\"title\":\"Syllabus Flyers 1\"}",
  "syllabusContent": "{\"sessionIndex\":1,\"activities\":[{\"time\":\"15 mins\"}]}",
  "sourceFileName": "Syllabus - XIN CHAO ENGLISH.xlsx",
  "attachment": "https://...",
  "isActive": true
}
```

Lưu ý:
- Field nào `null` thì backend bỏ qua, không cập nhật.
- Nếu cập nhật `sessionIndex`, backend sẽ kiểm tra trùng trong cùng program.

Response thành công:
- `200 OK`

Lỗi có thể trả:
- `401 Unauthorized`
- `403 Forbidden`
- `400 Bad Request`
  - `LessonPlanTemplate.Unauthorized`
  - Message: `You do not have permission to modify this lesson plan template`
- `400 Bad Request`
  - `LessonPlanTemplate.SessionIndexRequired`
  - Message: `SessionIndex is required and must be greater than 0`
- `404 Not Found`
  - `LessonPlanTemplate.NotFound`
  - Message: `Lesson plan template with Id = '...' was not found`
- `409 Conflict`
  - `LessonPlanTemplate.DuplicateSessionIndex`
  - Message: `Template with SessionIndex X already exists for Program Y`

### 7.5. Import template từ file syllabus

`POST /api/lesson-plan-templates/import?programId=&level=&overwriteExisting=true`

Update:
- `programId` is required for all import file types.
- The backend no longer maps worksheet names to existing programs.
- All parsed syllabus data is imported into the requested `programId`.

Role:
- `ManagementStaff`
- `Admin`

Content type:
- `multipart/form-data`

Form-data:
- `file`: file `xlsx`, `xls`, hoặc `csv`

Query params:
- `programId`: dùng khi import `csv`, hoặc khi muốn ép map cho file chỉ có 1 sheet
- `level`
- `overwriteExisting`

Rule import:
- Với `xlsx/xls`
  - Có thể chứa nhiều sheet.
  - Mỗi sheet được map sang một program.
  - Hệ thống sẽ normalize tên sheet để so khớp với tên/chuẩn code program.
- Với `csv`
  - Chỉ dùng cho một program.
  - `programId` là bắt buộc.

Parser backend đang làm gì:
- Tìm dòng header chứa `Period / Date / Teacher`.
- Bỏ qua metadata phía trên header.
- Bỏ qua sub-header nếu có.
- Gom nhiều dòng con cùng `Period` thành một session.
- Tự xử lý một số trường hợp lệch cột do merge cell trong Excel.
- Upsert theo `ProgramId + SessionIndex`.

Ý nghĩa `overwriteExisting`:
- `true`: template cũ cùng `Program + SessionIndex` sẽ bị cập nhật lại.
- `false`: template đã tồn tại sẽ bị bỏ qua.

Lưu ý về `importedCount`:
- `importedCount` là tổng số bản ghi được tạo mới hoặc cập nhật lại.
- Các bản ghi bị skip vì `overwriteExisting = false` sẽ không được tính vào `importedCount`.

Response thành công:
- `200 OK`

Ví dụ:
```json
{
  "isSuccess": true,
  "data": {
    "importedCount": 17,
    "programs": [
      {
        "programId": "guid",
        "programName": "FLYERS 1",
        "importedSessions": 24
      }
    ]
  }
}
```

Lỗi có thể trả:
- `401 Unauthorized`
- `403 Forbidden`
- `400 Bad Request`
  - Response đặc biệt từ controller khi không gửi file:
  ```json
  {
    "error": "No file provided"
  }
  ```
- `400 Bad Request`
  - `LessonPlanTemplate.UnsupportedImportFileType`
  - Message: `Unsupported syllabus import file type '.pdf'. Only .csv, .xlsx, and .xls are supported`
- `400 Bad Request`
  - `LessonPlanTemplate.ImportFileRequiresProgramId`
  - Message: `ProgramId is required when importing a CSV syllabus file`
- `400 Bad Request`
  - `LessonPlanTemplate.InvalidImportFile`
  - Ví dụ message:
    - `Worksheet 'FLYERS 1' does not contain the required Period/Date/Teacher header`
    - `Worksheet 'FLYERS 1' does not contain any syllabus session rows`
    - `No worksheet with syllabus data was found`
- `404 Not Found`
  - `LessonPlanTemplate.ProgramNotFound`
  - Message: `Program with Id = '...' was not found or inactive`
- `404 Not Found`
  - `LessonPlanTemplate.ProgramMappingNotFound`
  - Message: `Could not map syllabus sheet '...' to an active program`

Update:
- `LessonPlanTemplate.ProgramMappingNotFound` is no longer expected from the import endpoint.
- Import now uses the request `programId` instead of resolving program names from worksheet names.

## 8. Chi tiết API LessonPlan

### 8.1. Lấy toàn bộ syllabus của một lớp

`GET /api/lesson-plans/classes/{classId}/syllabus`

Role:
- `Teacher`
- `ManagementStaff`
- `Admin`

Mục đích:
- Đây là read model chính cho frontend teacher.
- Trả về toàn bộ danh sách buổi học của lớp, template tương ứng, lesson plan tương ứng và quyền sửa của user hiện tại.

Response thành công:
- `200 OK`

Ví dụ:
```json
{
  "isSuccess": true,
  "data": {
    "classId": "guid",
    "classCode": "FLYERS1-A",
    "classTitle": "Flyers 1 - Lớp A",
    "programId": "guid",
    "programName": "FLYERS 1",
    "syllabusMetadata": "{\"title\":\"Syllabus Flyers 1\"}",
    "sessions": [
      {
        "sessionId": "guid",
        "sessionIndex": 1,
        "sessionDate": "2026-05-09T08:00:00Z",
        "plannedTeacherId": "guid",
        "plannedTeacherName": "Ms Sarah",
        "actualTeacherId": null,
        "actualTeacherName": null,
        "lessonPlanId": "guid",
        "templateId": "guid",
        "templateTitle": "FLYERS 1 - Session 1",
        "templateSyllabusContent": "{\"sessionIndex\":1,\"activities\":[]}",
        "plannedContent": "{\"sessionIndex\":1,\"activities\":[]}",
        "actualContent": null,
        "actualHomework": null,
        "teacherNotes": null,
        "canEdit": true
      }
    ]
  }
}
```

Rule quan trọng:
- `sessionIndex` được tính theo thứ tự `PlannedDatetime` của các session trong lớp.
- Nếu lesson plan chưa có:
  - `lessonPlanId = null`
  - `plannedContent` có thể lấy fallback từ template
- `canEdit = true` khi:
  - user không phải teacher, hoặc
  - user là teacher đang là `PlannedTeacher` hoặc `ActualTeacher` của session đó

Lỗi có thể trả:
- `401 Unauthorized`
- `403 Forbidden`
- `400 Bad Request`
  - `LessonPlan.Unauthorized`
  - Message: `You do not have permission to access this lesson plan`
- `404 Not Found`
  - `LessonPlan.ClassNotFound`
  - Message: `Class with Id = '...' was not found`

### 8.2. Tạo lesson plan

`POST /api/lesson-plans`

Role:
- `Teacher`
- `ManagementStaff`
- `Admin`

Request body:
```json
{
  "classId": "guid",
  "sessionId": "guid",
  "templateId": null,
  "plannedContent": null,
  "actualContent": null,
  "actualHomework": null,
  "teacherNotes": null
}
```

Rule nghiệp vụ:
- `classId` phải tồn tại.
- `sessionId` phải tồn tại.
- `session` phải thuộc đúng `classId`.
- Mỗi `session` chỉ có tối đa 1 lesson plan chưa bị xóa mềm.
- Nếu user là teacher:
  - chỉ được tạo lesson plan cho session mình dạy.
- Nếu `templateId = null`:
  - backend tự resolve template theo `Class.ProgramId + SessionIndex`.
- Nếu `plannedContent` rỗng hoặc `null`:
  - backend tự lấy `template.SyllabusContent`.

Response thành công:
- `201 Created`

Ví dụ:
```json
{
  "isSuccess": true,
  "data": {
    "id": "guid",
    "classId": "guid",
    "sessionId": "guid",
    "templateId": "guid",
    "plannedContent": "{\"sessionIndex\":1,\"activities\":[]}",
    "actualContent": null,
    "actualHomework": null,
    "teacherNotes": null,
    "submittedBy": null,
    "submittedAt": null,
    "createdAt": "2026-04-01T10:00:00Z"
  }
}
```

Lỗi có thể trả:
- `401 Unauthorized`
- `403 Forbidden`
- `400 Bad Request`
  - `LessonPlan.Unauthorized`
  - Message: `You do not have permission to access this lesson plan`
- `400 Bad Request`
  - `LessonPlan.SessionClassMismatch`
  - Message: `Session with Id = '...' does not belong to class '...'`
- `404 Not Found`
  - `LessonPlan.ClassNotFound`
  - Message: `Class with Id = '...' was not found`
- `404 Not Found`
  - `Session.NotFound`
  - Message: `Session with Id = '...' was not found`
- `404 Not Found`
  - `LessonPlan.TemplateNotFound`
  - Message: `Lesson plan template with Id = '...' was not found`
- `409 Conflict`
  - `LessonPlan.SessionAlreadyHasLessonPlan`
  - Message: `Session with Id = '...' already has a lesson plan`

### 8.3. Lấy chi tiết một lesson plan

`GET /api/lesson-plans/{id}`

Role:
- `Teacher`
- `ManagementStaff`
- `Admin`

Mục đích:
- Lấy đầy đủ chi tiết một lesson plan để mở màn hình edit/detail.

Response thành công:
- `200 OK`

Ví dụ:
```json
{
  "isSuccess": true,
  "data": {
    "id": "guid",
    "classId": "guid",
    "classCode": "FLYERS1-A",
    "sessionId": "guid",
    "sessionTitle": "Session 09/05/2026 08:00",
    "sessionDate": "2026-05-09T08:00:00Z",
    "templateId": "guid",
    "templateLevel": "Flyers",
    "templateSessionIndex": 1,
    "plannedContent": "{\"sessionIndex\":1,\"activities\":[]}",
    "actualContent": "Đã dạy xong phần Reading",
    "actualHomework": "Workbook pages 88-89",
    "teacherNotes": "Cần tăng speaking",
    "submittedBy": null,
    "submittedByName": null,
    "submittedAt": null,
    "createdAt": "2026-04-01T10:00:00Z"
  }
}
```

Lỗi có thể trả:
- `401 Unauthorized`
- `403 Forbidden`
- `400 Bad Request`
  - `LessonPlan.Unauthorized`
  - Message: `You do not have permission to access this lesson plan`
- `404 Not Found`
  - `LessonPlan.NotFound`
  - Message: `Lesson plan with Id = '...' was not found`

### 8.4. Cập nhật lesson plan

`PUT /api/lesson-plans/{id}`

Role:
- `Teacher`
- `ManagementStaff`
- `Admin`

Request body:
```json
{
  "templateId": "guid",
  "plannedContent": "{\"sessionIndex\":1,\"activities\":[]}",
  "actualContent": "Đã dạy xong phần reading",
  "actualHomework": "Workbook pages 88-89",
  "teacherNotes": "Cần tăng speaking cho 2 học viên"
}
```

Rule nghiệp vụ:
- Lesson plan phải tồn tại.
- Nếu user là teacher thì chỉ được sửa buổi mình dạy.
- Nếu truyền `templateId` thì template đó phải tồn tại.
- Field nào `null` sẽ bị bỏ qua.

Response thành công:
- `200 OK`

Ví dụ:
```json
{
  "isSuccess": true,
  "data": {
    "id": "guid",
    "sessionId": "guid",
    "templateId": "guid",
    "plannedContent": "{\"sessionIndex\":1,\"activities\":[]}",
    "actualContent": "Đã dạy xong phần reading",
    "actualHomework": "Workbook pages 88-89",
    "teacherNotes": "Cần tăng speaking cho 2 học viên"
  }
}
```

Lỗi có thể trả:
- `401 Unauthorized`
- `403 Forbidden`
- `400 Bad Request`
  - `LessonPlan.Unauthorized`
  - Message: `You do not have permission to access this lesson plan`
- `404 Not Found`
  - `LessonPlan.NotFound`
  - Message: `Lesson plan with Id = '...' was not found`
- `404 Not Found`
  - `LessonPlan.TemplateNotFound`
  - Message: `Lesson plan template with Id = '...' was not found`

## 9. Frontend cần làm gì

### 9.1. Với Admin/Staff
- Tạo màn danh sách template theo `program`.
- Gọi `GET /api/lesson-plan-templates` để render danh sách và phân trang.
- Tạo màn chi tiết template qua `GET /api/lesson-plan-templates/{id}`.
- Tạo màn chỉnh sửa template qua `PUT /api/lesson-plan-templates/{id}`.
- Tạo màn import file syllabus qua `POST /api/lesson-plan-templates/import`.
- Nếu cần nhập tay một buổi riêng lẻ thì dùng `POST /api/lesson-plan-templates`.

### 9.2. Với Teacher
- Màn chính phải xoay quanh `GET /api/lesson-plans/classes/{classId}/syllabus`.
- Render toàn bộ session của lớp theo `sessionIndex`.
- Nếu `lessonPlanId = null` và `canEdit = true` thì hiển thị nút tạo lesson plan.
- Nếu `lessonPlanId != null` và `canEdit = true` thì hiển thị nút sửa lesson plan.
- Khi mở chi tiết một lesson plan thì gọi `GET /api/lesson-plans/{id}`.
- Khi lưu chỉnh sửa thì gọi `PUT /api/lesson-plans/{id}`.

### 9.3. Frontend không nên làm
- Không hardcode 17 syllabus trong frontend.
- Không tự map `program -> template` bằng constant local.
- Không tự tính `sessionIndex` thay backend.
- Không dùng file Excel/CSV phía frontend làm nguồn dữ liệu chuẩn.
- Không gọi lại các API đã bỏ.

## 10. UI/UX tối thiểu nên có

### 10.1. Màn template cho admin/staff
- Bộ lọc theo program, level, title, active.
- Upload file `xlsx/xls/csv`.
- Tùy chọn `overwriteExisting`.
- Hiển thị kết quả import theo từng program.
- Màn xem/sửa một session template.

### 10.2. Màn syllabus theo lớp cho teacher
- Header lớp:
  - `classCode`
  - `classTitle`
  - `programName`
- Metadata chung của syllabus.
- Danh sách session theo thứ tự.
- Với mỗi session cần hiển thị:
  - template content
  - planned content
  - actual content
  - homework
  - teacher notes
  - trạng thái có được sửa hay không

### 10.3. Empty/loading/error state
- Empty state khi program chưa có template.
- Empty state khi session chưa có lesson plan.
- Disabled state khi `canEdit = false`.
- Loading state cho import file lớn.
- Error state rõ ràng khi sheet không map được sang program.
- Fallback hiển thị raw text nếu parse JSON lỗi.

## 11. Gợi ý xử lý lỗi ở frontend

### 11.1. Với lỗi problem details
Frontend nên đọc:
- `status`
- `title`
- `detail`

Updated import note:
- Missing `programId` should be handled as a validation error before upload or from the API response.
- `LessonPlanTemplate.ProgramNotFound` should be shown as an invalid or inactive selected program.

Ví dụ:
- `title = LessonPlanTemplate.ProgramMappingNotFound`
- `detail = Could not map syllabus sheet 'Pre – Starters 2' to an active program`

=> UI nên hiện thông báo dễ hiểu kiểu:
- `Không thể map sheet "Pre – Starters 2" vào chương trình đang active trong hệ thống.`

### 11.2. Với lỗi file import thiếu
Riêng trường hợp không gửi file, response không phải problem details mà là:

```json
{
  "error": "No file provided"
}
```

Frontend nên có xử lý riêng cho trường hợp này.

### 11.3. Với lỗi quyền
- `401`: điều hướng login hoặc refresh token
- `403`: báo user không có quyền truy cập chức năng
- `400` với `LessonPlan.Unauthorized` hoặc `LessonPlanTemplate.Unauthorized`: báo user không có quyền trên bản ghi/nghiệp vụ cụ thể

## 12. Tóm tắt

- `lesson_plan_templates` là syllabus chuẩn của chương trình.
- `lesson_plans` là dữ liệu giáo án thực tế của từng buổi học.
- Endpoint quan trọng nhất cho teacher là `GET /api/lesson-plans/classes/{classId}/syllabus`.
- Endpoint quan trọng nhất cho admin/staff là `POST /api/lesson-plan-templates/import`.
- Frontend nên bám đúng bộ API hiện tại và xử lý đầy đủ `400/401/403/404/409` theo tài liệu này.
