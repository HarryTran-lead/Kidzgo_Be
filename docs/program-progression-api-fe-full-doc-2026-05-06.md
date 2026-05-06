# Program Progression API - Full Documentation

## 1. Phạm Vi Dữ Liệu & Quyền Hạn Theo Role

### 1.1 Mỗi Role Được Xem Dữ Liệu Gì

| Role | Program Progression Rules | Schedules | Assessments | My Assessment Schedules |
|------|--------------------------|-----------|-------------|------------------------|
| **Admin** | ✅ View All, Create, Edit | ✅ View All, Create, Edit, Cancel, Mark No-Show | ✅ View All, Create, Edit, Approve | ❌ |
| **ManagementStaff** | ✅ View All | ✅ View All, Create, Edit, Cancel, Mark No-Show | ✅ View All, Create, Edit, Approve | ❌ |
| **Teacher** | ❌ | ✅ View (department/assigned) | ✅ View, Create, Edit | ✅ View Own Schedules |
| **Parent** | ❌ | ❌ | ❌ | ✅ View Children's Schedules |
| **Student** | ❌ | ❌ | ❌ | ✅ View Own Schedules |

### 1.2 Phạm Vi Dữ Liệu

| Role | Scope | Chi Tiết |
|------|-------|----------|
| **Admin** | All | Toàn bộ dữ liệu hệ thống |
| **ManagementStaff** | All | Toàn bộ dữ liệu hệ thống |
| **Teacher** | Department/Assigned | Chỉ các schedules và assessments được assign cho teacher đó |
| **Parent** | Own Children | Chỉ schedules của các con của parent |
| **Student** | Own | Chỉ schedules của chính student đó |

### 1.3 Các Hành Động Được Phép

| Action | Admin | ManagementStaff | Teacher | Parent | Student |
|--------|-------|-----------------|---------|--------|---------|
| **View Rules** | ✅ | ✅ | ❌ | ❌ | ❌ |
| **Create Rules** | ✅ | ❌ | ❌ | ❌ | ❌ |
| **Edit Rules** | ✅ | ❌ | ❌ | ❌ | ❌ |
| **View Schedules** | ✅ All | ✅ All | ✅ Assigned | ❌ | ❌ |
| **Create Schedules** | ✅ | ✅ | ❌ | ❌ | ❌ |
| **Edit Schedules** | ✅ | ✅ | ❌ | ❌ | ❌ |
| **Cancel Schedules** | ✅ | ✅ | ❌ | ❌ | ❌ |
| **Mark Participant No-Show** | ✅ | ✅ | ❌ | ❌ | ❌ |
| **View My Assessment Schedules** | ❌ | ❌ | ✅ | ✅ | ✅ |
| **View Assessments** | ✅ All | ✅ All | ✅ Assigned | ❌ | ❌ |
| **Create Assessments** | ✅ | ✅ | ✅ | ❌ | ❌ |
| **Edit Assessments** | ✅ | ✅ | ✅ (if not approved) | ❌ | ❌ |
| **Approve Assessments** | ✅ | ✅ | ❌ | ❌ | ❌ |
| **Bulk Approve Assessments** | ✅ | ✅ | ❌ | ❌ | ❌ |

---

## 2. Danh Sách API Endpoints

### 2.1 Program Progression Rules APIs

#### 2.1.1 Get All Rules
- **Endpoint:** `GET /api/program-progressions/rules`
- **Method:** GET
- **Authorization:** Admin, ManagementStaff
- **Mô tả:** Lấy danh sách các quy tắc program progression với khả năng filter theo source program và trạng thái active

**Query Parameters:**

| Field | Type | Required | Mô tả |
|-------|------|----------|-------|
| sourceProgramId | Guid | ❌ | Filter theo source program |
| isActive | boolean | ❌ | Filter theo trạng thái active/inactive |

**Response Success (200 OK):**
```json
{
  "data": [
    {
      "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
      "sourceProgramId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
      "sourceProgramName": "Level 1",
      "targetProgramId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
      "targetProgramName": "Level 2",
      "method": "Shields",
      "minimumShieldCount": 10,
      "minimumSkillShieldCount": 3,
      "minimumOverallScore": null,
      "carryOverRemainingSessions": true,
      "stopCurrentEnrollmentOnApproval": true,
      "isActive": true,
      "notes": "Standard progression rule",
      "shieldMappings": [
        {
          "sourceShieldId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
          "targetShieldId": "3fa85f64-5717-4562-b3fc-2c963f66afa6"
        }
      ],
      "classificationBands": [
        {
          "minimumScore": 80,
          "maximumScore": 100,
          "classification": "Excellent",
          "targetProgramId": "3fa85f64-5717-4562-b3fc-2c963f66afa6"
        }
      ]
    }
  ]
}
```

**Response Error:**
- **401 Unauthorized:** Missing or invalid token
- **403 Forbidden:** User không có quyền truy cập

---

#### 2.1.2 Get Rule By ID
- **Endpoint:** `GET /api/program-progressions/rules/{id}`
- **Method:** GET
- **Authorization:** Admin, ManagementStaff
- **Mô tả:** Lấy chi tiết một rule theo ID

**Path Parameters:**

| Field | Type | Required | Mô tả |
|-------|------|----------|-------|
| id | Guid | ✅ | ID của rule |

**Response Success (200 OK):**
```json
{
  "data": {
    "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "sourceProgramId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "sourceProgramName": "Level 1",
    "targetProgramId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "targetProgramName": "Level 2",
    "method": "Shields",
    "minimumShieldCount": 10,
    "minimumSkillShieldCount": 3,
    "minimumOverallScore": null,
    "carryOverRemainingSessions": true,
    "stopCurrentEnrollmentOnApproval": true,
    "isActive": true,
    "notes": "Standard progression rule",
    "shieldMappings": [],
    "classificationBands": []
  }
}
```

**Response Error:**
- **401 Unauthorized:** Missing or invalid token
- **403 Forbidden:** User không có quyền truy cập
- **404 Not Found:** Rule không tồn tại

---

#### 2.1.3 Create Rule
- **Endpoint:** `POST /api/program-progressions/rules`
- **Method:** POST
- **Authorization:** Admin only
- **Mô tả:** Tạo mới một program progression rule

**Request Body:**
```json
{
  "sourceProgramId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "targetProgramId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "method": "Shields",
  "minimumShieldCount": 10,
  "minimumSkillShieldCount": 3,
  "minimumOverallScore": null,
  "carryOverRemainingSessions": true,
  "stopCurrentEnrollmentOnApproval": true,
  "isActive": true,
  "notes": "Standard progression rule",
  "shieldMappings": [
    {
      "sourceShieldId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
      "targetShieldId": "3fa85f64-5717-4562-b3fc-2c963f66afa6"
    }
  ],
  "classificationBands": [
    {
      "minimumScore": 80,
      "maximumScore": 100,
      "classification": "Excellent",
      "targetProgramId": "3fa85f64-5717-4562-b3fc-2c963f66afa6"
    }
  ]
}
```

**Body Parameters:**

| Field | Type | Required | Mô tả |
|-------|------|----------|-------|
| sourceProgramId | Guid | ✅ | ID của program nguồn |
| targetProgramId | Guid | ❌ | ID của program đích (null nếu dùng classification bands) |
| method | ProgramProgressionMethod | ✅ | Phương pháp: PassFail, Shields, CambridgeScale |
| minimumShieldCount | int | ❌ | Số shield tối thiểu (cho method Shields) |
| minimumSkillShieldCount | int | ❌ | Số skill shield tối thiểu (cho method Shields) |
| minimumOverallScore | decimal | ❌ | Điểm tổng tối thiểu (cho method CambridgeScale) |
| carryOverRemainingSessions | boolean | ✅ | Chuyển số buổi còn lại sang enrollment mới (default: true) |
| stopCurrentEnrollmentOnApproval | boolean | ✅ | Dừng enrollment hiện tại khi approve (default: true) |
| isActive | boolean | ✅ | Rule có đang active hay không (default: true) |
| notes | string | ❌ | Ghi chú |
| shieldMappings | array | ❌ | Danh sách mapping shields từ source sang target |
| classificationBands | array | ❌ | Danh sách phân loại theo điểm (cho CambridgeScale) |

**Response Success (201 Created):**
```json
{
  "data": {
    "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "sourceProgramId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "targetProgramId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "method": "Shields",
    "isActive": true
  }
}
```

**Response Error:**
- **400 Bad Request:** Validation errors
  ```json
  {
    "errors": {
      "SourceProgramId": ["Source program is required"],
      "Method": ["Invalid progression method"]
    }
  }
  ```
- **401 Unauthorized:** Missing or invalid token
- **403 Forbidden:** User không có quyền Admin

---

#### 2.1.4 Update Rule
- **Endpoint:** `PUT /api/program-progressions/rules/{id}`
- **Method:** PUT
- **Authorization:** Admin only
- **Mô tả:** Cập nhật một program progression rule

**Path Parameters:**

| Field | Type | Required | Mô tả |
|-------|------|----------|-------|
| id | Guid | ✅ | ID của rule cần update |

**Request Body:** Giống Create Rule

**Response Success (200 OK):**
```json
{
  "data": {
    "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "sourceProgramId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "targetProgramId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "method": "Shields",
    "isActive": true
  }
}
```

**Response Error:**
- **400 Bad Request:** Validation errors
- **401 Unauthorized:** Missing or invalid token
- **403 Forbidden:** User không có quyền Admin
- **404 Not Found:** Rule không tồn tại

---

### 2.2 Program Progression Schedules APIs

#### 2.2.1 Get Schedule Availability
- **Endpoint:** `GET /api/program-progressions/schedules/availability`
- **Method:** GET
- **Authorization:** Admin, ManagementStaff
- **Mô tả:** Kiểm tra availability của students và teachers cho một schedule cụ thể (để tránh conflict)

**Query Parameters:**

| Field | Type | Required | Mô tả |
|-------|------|----------|-------|
| sourceClassId | Guid | ✅ | ID của class nguồn |
| scheduledAt | DateTime | ✅ | Thời gian dự kiến schedule |
| durationMinutes | int | ❌ | Thời lượng (phút) |
| excludeScheduleId | Guid | ❌ | Loại trừ schedule ID (khi edit) |
| includeUnavailable | boolean | ❌ | Có bao gồm những người unavailable không (default: false) |

**Response Success (200 OK):**
```json
{
  "data": {
    "availableStudents": [
      {
        "studentProfileId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
        "studentName": "Nguyen Van A",
        "isAvailable": true,
        "conflicts": []
      }
    ],
    "availableTeachers": [
      {
        "userId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
        "teacherName": "Teacher John",
        "isAvailable": true,
        "conflicts": []
      }
    ],
    "unavailableStudents": [],
    "unavailableTeachers": []
  }
}
```

**Response Error:**
- **400 Bad Request:** Invalid parameters
- **401 Unauthorized:** Missing or invalid token
- **403 Forbidden:** User không có quyền truy cập

---

#### 2.2.2 Get All Schedules
- **Endpoint:** `GET /api/program-progressions/schedules`
- **Method:** GET
- **Authorization:** Teacher, ManagementStaff, Admin
- **Mô tả:** Lấy danh sách schedules với pagination và filters

**Query Parameters:**

| Field | Type | Required | Mô tả |
|-------|------|----------|-------|
| sourceClassId | Guid | ❌ | Filter theo class nguồn |
| studentProfileId | Guid | ❌ | Filter theo student |
| assignedTeacherUserId | Guid | ❌ | Filter theo teacher được assign |
| status | ProgramProgressionScheduleStatus | ❌ | Filter theo status: Scheduled, Completed, Cancelled |
| participantStatus | ProgramProgressionScheduleParticipantStatus | ❌ | Filter theo participant status |
| from | DateTime | ❌ | Từ ngày |
| to | DateTime | ❌ | Đến ngày |
| pageNumber | int | ❌ | Số trang (default: 1) |
| pageSize | int | ❌ | Số items mỗi trang (default: 10) |

**Response Success (200 OK):**
```json
{
  "data": {
    "items": [
      {
        "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
        "sourceClassId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
        "sourceClassName": "Level 1 - Class A",
        "scheduledAt": "2026-05-10T10:00:00Z",
        "durationMinutes": 60,
        "roomId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
        "roomName": "Room 101",
        "assignedTeacherUserId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
        "assignedTeacherName": "Teacher John",
        "status": "Scheduled",
        "notes": "Mid-term progression test",
        "participants": [
          {
            "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
            "studentProfileId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
            "studentName": "Nguyen Van A",
            "status": "Scheduled",
            "assessmentId": null,
            "assessmentStatus": null
          }
        ],
        "scheduledParticipantCount": 5,
        "completedParticipantCount": 0
      }
    ],
    "pageNumber": 1,
    "pageSize": 10,
    "totalCount": 25,
    "totalPages": 3
  }
}
```

**Response Error:**
- **401 Unauthorized:** Missing or invalid token
- **403 Forbidden:** User không có quyền truy cập

---

#### 2.2.3 Get Schedule By ID
- **Endpoint:** `GET /api/program-progressions/schedules/{id}`
- **Method:** GET
- **Authorization:** Teacher, ManagementStaff, Admin
- **Mô tả:** Lấy chi tiết một schedule theo ID

**Path Parameters:**

| Field | Type | Required | Mô tả |
|-------|------|----------|-------|
| id | Guid | ✅ | ID của schedule |

**Response Success (200 OK):**
```json
{
  "data": {
    "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "sourceClassId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "sourceClassName": "Level 1 - Class A",
    "scheduledAt": "2026-05-10T10:00:00Z",
    "durationMinutes": 60,
    "roomId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "roomName": "Room 101",
    "assignedTeacherUserId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "assignedTeacherName": "Teacher John",
    "status": "Scheduled",
    "notes": "Mid-term progression test",
    "participants": [
      {
        "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
        "studentProfileId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
        "studentName": "Nguyen Van A",
        "status": "Scheduled",
        "assessmentId": null,
        "assessmentStatus": null
      }
    ]
  }
}
```

**Response Error:**
- **401 Unauthorized:** Missing or invalid token
- **403 Forbidden:** User không có quyền truy cập
- **404 Not Found:** Schedule không tồn tại

---

#### 2.2.4 Create Schedule
- **Endpoint:** `POST /api/program-progressions/schedules`
- **Method:** POST
- **Authorization:** ManagementStaff, Admin
- **Mô tả:** Tạo mới một progression assessment schedule

**Request Body:**
```json
{
  "sourceClassId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "scheduledAt": "2026-05-10T10:00:00Z",
  "durationMinutes": 60,
  "roomId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "assignedTeacherUserId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "notes": "Mid-term progression test",
  "studentProfileIds": [
    "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "3fa85f64-5717-4562-b3fc-2c963f66afa7"
  ]
}
```

**Body Parameters:**

| Field | Type | Required | Mô tả |
|-------|------|----------|-------|
| sourceClassId | Guid | ✅ | ID của class nguồn |
| scheduledAt | DateTime | ✅ | Thời gian schedule |
| durationMinutes | int | ❌ | Thời lượng (phút) |
| roomId | Guid | ❌ | ID của phòng |
| assignedTeacherUserId | Guid | ❌ | ID của teacher được assign |
| notes | string | ❌ | Ghi chú |
| studentProfileIds | array of Guid | ❌ | Danh sách student IDs (nếu null thì lấy all students trong class) |

**Response Success (201 Created):**
```json
{
  "data": {
    "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "sourceClassId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "scheduledAt": "2026-05-10T10:00:00Z",
    "status": "Scheduled"
  }
}
```

**Response Error:**
- **400 Bad Request:** Validation errors
  ```json
  {
    "errors": {
      "SourceClassId": ["Source class is required"],
      "ScheduledAt": ["Scheduled time must be in the future"],
      "StudentProfileIds": ["Some students are not in the source class"]
    }
  }
  ```
- **401 Unauthorized:** Missing or invalid token
- **403 Forbidden:** User không có quyền tạo schedule

---

#### 2.2.5 Update Schedule
- **Endpoint:** `PUT /api/program-progressions/schedules/{id}`
- **Method:** PUT
- **Authorization:** ManagementStaff, Admin
- **Mô tả:** Cập nhật thông tin schedule (chỉ update được nếu status = Scheduled và không có participant nào đã Completed/NoShow)

**Path Parameters:**

| Field | Type | Required | Mô tả |
|-------|------|----------|-------|
| id | Guid | ✅ | ID của schedule cần update |

**Request Body:**
```json
{
  "sourceClassId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "scheduledAt": "2026-05-10T14:00:00Z",
  "durationMinutes": 90,
  "roomId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "assignedTeacherUserId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "notes": "Updated notes"
}
```

**Body Parameters:**

| Field | Type | Required | Mô tả |
|-------|------|----------|-------|
| sourceClassId | Guid | ✅ | ID của class nguồn (không thể đổi) |
| scheduledAt | DateTime | ✅ | Thời gian schedule mới |
| durationMinutes | int | ❌ | Thời lượng (phút) |
| roomId | Guid | ❌ | ID của phòng |
| assignedTeacherUserId | Guid | ❌ | ID của teacher được assign |
| notes | string | ❌ | Ghi chú |

**Response Success (200 OK):**
```json
{
  "data": {
    "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "scheduledAt": "2026-05-10T14:00:00Z",
    "status": "Scheduled"
  }
}
```

**Response Error:**
- **400 Bad Request:**
  - Schedule không ở trạng thái Scheduled
  - Có participants đã Completed hoặc NoShow
  ```json
  {
    "errors": {
      "Schedule": ["Cannot update schedule that is not in Scheduled status"],
      "Participants": ["Schedule has participants with Completed or NoShow status"]
    }
  }
  ```
- **401 Unauthorized:** Missing or invalid token
- **403 Forbidden:** User không có quyền update
- **404 Not Found:** Schedule không tồn tại

---

#### 2.2.6 Cancel Schedule
- **Endpoint:** `POST /api/program-progressions/schedules/{id}/cancel`
- **Method:** POST
- **Authorization:** ManagementStaff, Admin
- **Mô tả:** Hủy một schedule (chỉ hủy được nếu status = Scheduled và không có participant nào đã Completed/NoShow)

**Path Parameters:**

| Field | Type | Required | Mô tả |
|-------|------|----------|-------|
| id | Guid | ✅ | ID của schedule cần cancel |

**Response Success (200 OK):**
```json
{
  "data": {
    "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "status": "Cancelled"
  }
}
```

**Response Error:**
- **400 Bad Request:**
  - Schedule không ở trạng thái Scheduled
  - Có participants đã Completed hoặc NoShow
  ```json
  {
    "errors": {
      "Schedule": ["Cannot cancel schedule that is not in Scheduled status"],
      "Participants": ["Schedule has participants with assessments or no-show status"]
    }
  }
  ```
- **401 Unauthorized:** Missing or invalid token
- **403 Forbidden:** User không có quyền cancel
- **404 Not Found:** Schedule không tồn tại

---

#### 2.2.7 Mark Participant No-Show
- **Endpoint:** `POST /api/program-progressions/schedules/participants/{participantId}/no-show`
- **Method:** POST
- **Authorization:** ManagementStaff, Admin
- **Mô tả:** Đánh dấu một participant là No-Show (vắng mặt không báo trước)

**Path Parameters:**

| Field | Type | Required | Mô tả |
|-------|------|----------|-------|
| participantId | Guid | ✅ | ID của participant |

**Response Success (200 OK):**
```json
{
  "data": {
    "participantId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "status": "NoShow"
  }
}
```

**Response Error:**
- **400 Bad Request:**
  - Schedule không ở trạng thái Scheduled
  - Participant không ở trạng thái Scheduled
  ```json
  {
    "errors": {
      "Participant": ["Can only mark no-show for scheduled participants"],
      "Schedule": ["Schedule is not in Scheduled status"]
    }
  }
  ```
- **401 Unauthorized:** Missing or invalid token
- **403 Forbidden:** User không có quyền thực hiện
- **404 Not Found:** Participant không tồn tại

---

#### 2.2.8 Get My Assessment Schedules
- **Endpoint:** `GET /api/program-progressions/my-assessment-schedules`
- **Method:** GET
- **Authorization:** Teacher, Student, Parent
- **Mô tả:** Teacher xem schedules được assign, Student xem schedules của mình, Parent xem schedules của con

**Query Parameters:**

| Field | Type | Required | Mô tả |
|-------|------|----------|-------|
| studentProfileId | Guid | ❌ | Filter theo student (chỉ dành cho Parent) |
| status | ProgramProgressionScheduleStatus | ❌ | Filter theo status |
| participantStatus | ProgramProgressionScheduleParticipantStatus | ❌ | Filter theo participant status |
| from | DateTime | ❌ | Từ ngày |
| to | DateTime | ❌ | Đến ngày |
| pageNumber | int | ❌ | Số trang (default: 1) |
| pageSize | int | ❌ | Số items mỗi trang (default: 10) |

**Response Success (200 OK):**
```json
{
  "data": {
    "items": [
      {
        "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
        "sourceClassName": "Level 1 - Class A",
        "scheduledAt": "2026-05-10T10:00:00Z",
        "durationMinutes": 60,
        "roomName": "Room 101",
        "assignedTeacherName": "Teacher John",
        "status": "Scheduled",
        "participantStatus": "Scheduled",
        "assessmentStatus": null
      }
    ],
    "pageNumber": 1,
    "pageSize": 10,
    "totalCount": 5,
    "totalPages": 1
  }
}
```

**Response Error:**
- **401 Unauthorized:** Missing or invalid token
- **403 Forbidden:** User không có quyền truy cập

---

### 2.3 Program Progression Assessments APIs

#### 2.3.1 Get All Assessments
- **Endpoint:** `GET /api/program-progressions/assessments`
- **Method:** GET
- **Authorization:** Teacher, ManagementStaff, Admin
- **Mô tả:** Lấy danh sách assessments với pagination và filters

**Query Parameters:**

| Field | Type | Required | Mô tả |
|-------|------|----------|-------|
| sourceRegistrationId | Guid | ❌ | Filter theo registration |
| studentProfileId | Guid | ❌ | Filter theo student |
| sourceProgramId | Guid | ❌ | Filter theo source program |
| method | ProgramProgressionMethod | ❌ | Filter theo method |
| status | ProgramProgressionAssessmentStatus | ❌ | Filter theo status: Recorded, Approved |
| isEligible | boolean | ❌ | Filter theo eligible for progression |
| pageNumber | int | ❌ | Số trang (default: 1) |
| pageSize | int | ❌ | Số items mỗi trang (default: 10) |

**Response Success (200 OK):**
```json
{
  "data": {
    "items": [
      {
        "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
        "sourceRegistrationId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
        "studentProfileId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
        "studentName": "Nguyen Van A",
        "sourceProgramId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
        "sourceProgramName": "Level 1",
        "method": "CambridgeScale",
        "assessmentDate": "2026-05-10T10:00:00Z",
        "passedInClass": null,
        "listeningScore": 85,
        "speakingScore": 90,
        "readingWritingScore": null,
        "readingScore": 88,
        "writingScore": 82,
        "overallScore": 86.25,
        "status": "Recorded",
        "isEligible": true,
        "targetProgramId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
        "targetProgramName": "Level 2",
        "comment": "Good performance",
        "attachmentUrls": ["https://example.com/file1.pdf"],
        "createdBy": "Teacher John",
        "createdAt": "2026-05-10T11:00:00Z"
      }
    ],
    "pageNumber": 1,
    "pageSize": 10,
    "totalCount": 15,
    "totalPages": 2
  }
}
```

**Response Error:**
- **401 Unauthorized:** Missing or invalid token
- **403 Forbidden:** User không có quyền truy cập

---

#### 2.3.2 Get Assessment By ID
- **Endpoint:** `GET /api/program-progressions/assessments/{id}`
- **Method:** GET
- **Authorization:** Teacher, ManagementStaff, Admin
- **Mô tả:** Lấy chi tiết một assessment theo ID

**Path Parameters:**

| Field | Type | Required | Mô tả |
|-------|------|----------|-------|
| id | Guid | ✅ | ID của assessment |

**Response Success (200 OK):**
```json
{
  "data": {
    "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "sourceRegistrationId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "studentProfileId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "studentName": "Nguyen Van A",
    "sourceProgramId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "sourceProgramName": "Level 1",
    "method": "CambridgeScale",
    "assessmentDate": "2026-05-10T10:00:00Z",
    "passedInClass": null,
    "listeningScore": 85,
    "speakingScore": 90,
    "readingWritingScore": null,
    "readingScore": 88,
    "writingScore": 82,
    "overallScore": 86.25,
    "status": "Recorded",
    "isEligible": true,
    "targetProgramId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "targetProgramName": "Level 2",
    "comment": "Good performance",
    "attachmentUrls": ["https://example.com/file1.pdf"],
    "approvedBy": null,
    "approvedAt": null,
    "approvalNote": null,
    "createdBy": "Teacher John",
    "createdAt": "2026-05-10T11:00:00Z"
  }
}
```

**Response Error:**
- **401 Unauthorized:** Missing or invalid token
- **403 Forbidden:** User không có quyền truy cập
- **404 Not Found:** Assessment không tồn tại

---

#### 2.3.3 Create Assessment
- **Endpoint:** `POST /api/program-progressions/assessments`
- **Method:** POST
- **Authorization:** Teacher, ManagementStaff, Admin
- **Mô tả:** Tạo mới một program progression assessment

**Request Body:**
```json
{
  "sourceRegistrationId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "scheduleParticipantId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "assessmentDate": "2026-05-10T10:00:00Z",
  "passedInClass": null,
  "listeningScore": 85,
  "speakingScore": 90,
  "readingWritingScore": null,
  "readingScore": 88,
  "writingScore": 82,
  "comment": "Good performance",
  "attachmentUrls": ["https://example.com/file1.pdf"]
}
```

**Body Parameters:**

| Field | Type | Required | Mô tả |
|-------|------|----------|-------|
| sourceRegistrationId | Guid | ❌ | ID của registration nguồn (required nếu không có scheduleParticipantId) |
| scheduleParticipantId | Guid | ❌ | ID của schedule participant (required nếu không có sourceRegistrationId) |
| assessmentDate | DateTime | ❌ | Ngày đánh giá |
| passedInClass | boolean | ❌ | Pass/Fail (cho method PassFail) |
| listeningScore | decimal | ❌ | Điểm Listening (cho CambridgeScale) |
| speakingScore | decimal | ❌ | Điểm Speaking (cho CambridgeScale) |
| readingWritingScore | decimal | ❌ | Điểm Reading+Writing (cho CambridgeScale pre-A1 to A2) |
| readingScore | decimal | ❌ | Điểm Reading (cho CambridgeScale B1+) |
| writingScore | decimal | ❌ | Điểm Writing (cho CambridgeScale B1+) |
| comment | string | ❌ | Nhận xét |
| attachmentUrls | array of string | ❌ | Danh sách URLs file đính kèm |

**Response Success (201 Created):**
```json
{
  "data": {
    "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "studentProfileId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "status": "Recorded",
    "isEligible": true
  }
}
```

**Response Error:**
- **400 Bad Request:** Validation errors
  ```json
  {
    "errors": {
      "SourceRegistrationId": ["Must provide either SourceRegistrationId or ScheduleParticipantId"],
      "PassedInClass": ["PassedInClass is required for PassFail method"],
      "ListeningScore": ["Listening score must be between 0 and 100"]
    }
  }
  ```
- **401 Unauthorized:** Missing or invalid token
- **403 Forbidden:** User không có quyền tạo assessment

---

#### 2.3.4 Update Assessment
- **Endpoint:** `PUT /api/program-progressions/assessments/{id}`
- **Method:** PUT
- **Authorization:** Teacher, ManagementStaff, Admin
- **Mô tả:** Cập nhật assessment (chỉ update được nếu status = Recorded, chưa Approved)

**Path Parameters:**

| Field | Type | Required | Mô tả |
|-------|------|----------|-------|
| id | Guid | ✅ | ID của assessment cần update |

**Request Body:**
```json
{
  "sourceRegistrationId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "scheduleParticipantId": null,
  "assessmentDate": "2026-05-10T10:00:00Z",
  "passedInClass": null,
  "listeningScore": 88,
  "speakingScore": 92,
  "readingWritingScore": null,
  "readingScore": 90,
  "writingScore": 85,
  "comment": "Updated comment",
  "attachmentUrls": ["https://example.com/file1.pdf", "https://example.com/file2.pdf"]
}
```

**Body Parameters:** Giống Create Assessment (nhưng không update được sourceRegistrationId và scheduleParticipantId)

**Response Success (200 OK):**
```json
{
  "data": {
    "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "status": "Recorded",
    "isEligible": true
  }
}
```

**Response Error:**
- **400 Bad Request:**
  - Assessment đã được Approved
  ```json
  {
    "errors": {
      "Assessment": ["Cannot update an approved assessment"]
    }
  }
  ```
- **401 Unauthorized:** Missing or invalid token
- **403 Forbidden:** User không có quyền update
- **404 Not Found:** Assessment không tồn tại

---

#### 2.3.5 Approve Assessment
- **Endpoint:** `POST /api/program-progressions/assessments/{id}/approve`
- **Method:** POST
- **Authorization:** ManagementStaff, Admin
- **Mô tả:** Approve một assessment và tạo registration mới cho target program

**Path Parameters:**

| Field | Type | Required | Mô tả |
|-------|------|----------|-------|
| id | Guid | ✅ | ID của assessment cần approve |

**Request Body:**
```json
{
  "tuitionPlanId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "approvalNote": "Approved for Level 2"
}
```

**Body Parameters:**

| Field | Type | Required | Mô tả |
|-------|------|----------|-------|
| tuitionPlanId | Guid | ❌ | ID của tuition plan cho registration mới |
| approvalNote | string | ❌ | Ghi chú khi approve |

**Response Success (200 OK):**
```json
{
  "data": {
    "assessmentId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "status": "Approved",
    "newRegistrationId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "targetProgramId": "3fa85f64-5717-4562-b3fc-2c963f66afa6"
  }
}
```

**Response Error:**
- **400 Bad Request:**
  - Assessment đã được Approved
  - Assessment không eligible
  ```json
  {
    "errors": {
      "Assessment": ["Assessment is already approved"],
      "Eligibility": ["Assessment is not eligible for progression"]
    }
  }
  ```
- **401 Unauthorized:** Missing or invalid token
- **403 Forbidden:** User không có quyền approve
- **404 Not Found:** Assessment không tồn tại

---

#### 2.3.6 Bulk Approve Assessments
- **Endpoint:** `POST /api/program-progressions/assessments/bulk-approve`
- **Method:** POST
- **Authorization:** ManagementStaff, Admin
- **Mô tả:** Approve nhiều assessments cùng lúc

**Request Body:**
```json
{
  "items": [
    {
      "assessmentId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
      "tuitionPlanId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
      "approvalNote": "Approved for Level 2"
    },
    {
      "assessmentId": "3fa85f64-5717-4562-b3fc-2c963f66afa7",
      "tuitionPlanId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
      "approvalNote": "Approved for Level 3"
    }
  ]
}
```

**Body Parameters:**

| Field | Type | Required | Mô tả |
|-------|------|----------|-------|
| items | array | ✅ | Danh sách assessments cần approve |
| items[].assessmentId | Guid | ✅ | ID của assessment |
| items[].tuitionPlanId | Guid | ❌ | ID của tuition plan |
| items[].approvalNote | string | ❌ | Ghi chú |

**Response Success (200 OK):**
```json
{
  "data": {
    "successCount": 2,
    "failureCount": 0,
    "results": [
      {
        "assessmentId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
        "success": true,
        "newRegistrationId": "3fa85f64-5717-4562-b3fc-2c963f66afa8"
      },
      {
        "assessmentId": "3fa85f64-5717-4562-b3fc-2c963f66afa7",
        "success": true,
        "newRegistrationId": "3fa85f64-5717-4562-b3fc-2c963f66afa9"
      }
    ]
  }
}
```

**Response Error:**
- **400 Bad Request:** Validation errors
- **401 Unauthorized:** Missing or invalid token
- **403 Forbidden:** User không có quyền bulk approve

---

## 3. Status Definitions

### 3.1 ProgramProgressionScheduleStatus

| Status | Ý Nghĩa | Mô Tả |
|--------|---------|-------|
| **Scheduled** | Đã lên lịch | Schedule đã được tạo và đang chờ thực hiện |
| **Completed** | Hoàn thành | Tất cả participants đã hoàn thành assessment |
| **Cancelled** | Đã hủy | Schedule đã bị hủy |

**Luồng Chuyển Trạng Thái:**
```
Scheduled -> Completed (khi tất cả participants đã có assessment hoặc no-show)
Scheduled -> Cancelled (khi ManagementStaff/Admin cancel)
```

---

### 3.2 ProgramProgressionScheduleParticipantStatus

| Status | Ý Nghĩa | Mô Tả |
|--------|---------|-------|
| **Scheduled** | Đã lên lịch | Participant được schedule để tham gia assessment |
| **Completed** | Hoàn thành | Participant đã hoàn thành assessment |
| **NoShow** | Vắng mặt | Participant không tham gia assessment |
| **Cancelled** | Đã hủy | Participant bị hủy khỏi schedule |

**Luồng Chuyển Trạng Thái:**
```
Scheduled -> Completed (khi tạo assessment cho participant)
Scheduled -> NoShow (khi ManagementStaff/Admin đánh dấu no-show)
Scheduled -> Cancelled (khi schedule bị cancel)
```

---

### 3.3 ProgramProgressionAssessmentStatus

| Status | Ý Nghĩa | Mô Tả |
|--------|---------|-------|
| **Recorded** | Đã ghi nhận | Assessment đã được tạo nhưng chưa approve |
| **Approved** | Đã phê duyệt | Assessment đã được approve và tạo registration mới |

**Luồng Chuyển Trạng Thái:**
```
Recorded -> Approved (khi ManagementStaff/Admin approve)
```

---

### 3.4 ProgramProgressionMethod

| Method | Ý Nghĩa | Mô Tả |
|--------|---------|-------|
| **PassFail** | Đậu/Rớt | Đánh giá dựa trên Pass/Fail trong class |
| **Shields** | Shields | Đánh giá dựa trên số lượng shields đạt được |
| **CambridgeScale** | Cambridge Scale | Đánh giá dựa trên điểm số Cambridge (Listening, Speaking, Reading, Writing) |

---

## 4. Permission Matrix Theo Role

### 4.1 Program Progression Rules

| Action | Admin | ManagementStaff | Teacher | Parent | Student |
|--------|-------|-----------------|---------|--------|---------|
| GET /rules | ✅ | ✅ | ❌ | ❌ | ❌ |
| GET /rules/{id} | ✅ | ✅ | ❌ | ❌ | ❌ |
| POST /rules | ✅ | ❌ | ❌ | ❌ | ❌ |
| PUT /rules/{id} | ✅ | ❌ | ❌ | ❌ | ❌ |

### 4.2 Program Progression Schedules

| Action | Admin | ManagementStaff | Teacher | Parent | Student |
|--------|-------|-----------------|---------|--------|---------|
| GET /schedules/availability | ✅ | ✅ | ❌ | ❌ | ❌ |
| GET /schedules | ✅ All | ✅ All | ✅ Assigned | ❌ | ❌ |
| GET /schedules/{id} | ✅ | ✅ | ✅ | ❌ | ❌ |
| POST /schedules | ✅ | ✅ | ❌ | ❌ | ❌ |
| PUT /schedules/{id} | ✅ | ✅ | ❌ | ❌ | ❌ |
| POST /schedules/{id}/cancel | ✅ | ✅ | ❌ | ❌ | ❌ |
| POST /participants/{id}/no-show | ✅ | ✅ | ❌ | ❌ | ❌ |
| GET /my-assessment-schedules | ❌ | ❌ | ✅ | ✅ | ✅ |

### 4.3 Program Progression Assessments

| Action | Admin | ManagementStaff | Teacher | Parent | Student |
|--------|-------|-----------------|---------|--------|---------|
| GET /assessments | ✅ All | ✅ All | ✅ Assigned | ❌ | ❌ |
| GET /assessments/{id} | ✅ | ✅ | ✅ | ❌ | ❌ |
| POST /assessments | ✅ | ✅ | ✅ | ❌ | ❌ |
| PUT /assessments/{id} | ✅ | ✅ | ✅ (if not approved) | ❌ | ❌ |
| POST /assessments/{id}/approve | ✅ | ✅ | ❌ | ❌ | ❌ |
| POST /assessments/bulk-approve | ✅ | ✅ | ❌ | ❌ | ❌ |

---

## 5. Validation Rules

### 5.1 Program Progression Rules

#### Create/Update Rule Validation:
- `sourceProgramId`: Required, phải tồn tại
- `method`: Required, phải là một trong: PassFail, Shields, CambridgeScale
- `targetProgramId`: Optional (có thể null nếu dùng classificationBands)
- **Shields Method:**
  - `minimumShieldCount` hoặc `minimumSkillShieldCount` phải có ít nhất một
  - `shieldMappings` phải valid nếu có
- **CambridgeScale Method:**
  - `minimumOverallScore` phải có
  - `classificationBands` phải có nếu không có targetProgramId
- **PassFail Method:**
  - Không cần thêm validation đặc biệt

**Error Cases:**
```json
{
  "errors": {
    "SourceProgramId": ["Source program not found"],
    "TargetProgramId": ["Target program not found"],
    "Method": ["Invalid progression method"],
    "MinimumShieldCount": ["Minimum shield count or skill shield count is required for Shields method"],
    "MinimumOverallScore": ["Minimum overall score is required for CambridgeScale method"],
    "ClassificationBands": ["Classification bands are required when target program is not specified"]
  }
}
```

---

### 5.2 Program Progression Schedules

#### Create Schedule Validation:
- `sourceClassId`: Required, class phải tồn tại
- `scheduledAt`: Required, phải là thời gian trong tương lai
- `studentProfileIds`: Nếu có, tất cả students phải thuộc sourceClass
- `roomId`: Nếu có, room phải available tại scheduledAt
- `assignedTeacherUserId`: Nếu có, teacher phải available tại scheduledAt

#### Update Schedule Validation:
- Schedule phải ở status = Scheduled
- Không có participant nào ở status Completed hoặc NoShow
- Không thể thay đổi `sourceClassId`

#### Cancel Schedule Validation:
- Schedule phải ở status = Scheduled
- Không có participant nào ở status Completed hoặc NoShow

#### Mark No-Show Validation:
- Schedule phải ở status = Scheduled
- Participant phải ở status = Scheduled

**Error Cases:**
```json
{
  "errors": {
    "SourceClassId": ["Source class not found"],
    "ScheduledAt": ["Scheduled time must be in the future"],
    "StudentProfileIds": ["Some students are not in the source class"],
    "RoomId": ["Room is not available at scheduled time"],
    "AssignedTeacherUserId": ["Teacher is not available at scheduled time"],
    "Schedule": ["Cannot update/cancel schedule that is not in Scheduled status"],
    "Participants": ["Schedule has participants with Completed or NoShow status"]
  }
}
```

---

### 5.3 Program Progression Assessments

#### Create Assessment Validation:
- Phải có `sourceRegistrationId` HOẶC `scheduleParticipantId`
- Nếu có `scheduleParticipantId`:
  - Schedule participant phải ở status Scheduled
  - Chưa có assessment cho participant này
- Nếu có `sourceRegistrationId`:
  - Registration phải tồn tại
  - Registration phải có active progression rule
- **PassFail Method:**
  - `passedInClass` required
- **Shields Method:**
  - Không cần điểm số
- **CambridgeScale Method:**
  - `listeningScore`, `speakingScore` required
  - Pre-A1 to A2: `readingWritingScore` required
  - B1+: `readingScore`, `writingScore` required
  - Tất cả điểm phải trong khoảng 0-100

#### Update Assessment Validation:
- Assessment phải ở status = Recorded (chưa Approved)
- Không thể thay đổi `sourceRegistrationId` và `scheduleParticipantId`
- Validation tương tự Create

#### Approve Assessment Validation:
- Assessment phải ở status = Recorded
- Assessment phải eligible (isEligible = true)
- Target program phải tồn tại
- Nếu có `tuitionPlanId`, phải thuộc target program

**Error Cases:**
```json
{
  "errors": {
    "SourceRegistrationId": ["Must provide either SourceRegistrationId or ScheduleParticipantId"],
    "ScheduleParticipantId": ["Schedule participant not found or not in Scheduled status"],
    "PassedInClass": ["PassedInClass is required for PassFail method"],
    "ListeningScore": ["Listening score is required for CambridgeScale method"],
    "SpeakingScore": ["Speaking score must be between 0 and 100"],
    "ReadingWritingScore": ["Reading+Writing score is required for Pre-A1 to A2 levels"],
    "Assessment": ["Cannot update an approved assessment", "Assessment is already approved"],
    "Eligibility": ["Assessment is not eligible for progression"],
    "TuitionPlanId": ["Tuition plan does not belong to target program"]
  }
}
```

---

## 6. Business Logic & Flow

### 6.1 Program Progression Flow

```
1. Admin tạo Progression Rules cho các programs
   ↓
2. ManagementStaff tạo Assessment Schedule cho một class
   - Chọn students cần đánh giá
   - Assign teacher
   - Chọn room và thời gian
   ↓
3. Teacher/Student/Parent xem "My Assessment Schedules"
   ↓
4. Teacher tạo Assessments cho từng student
   - Nhập điểm số/kết quả theo method (PassFail/Shields/CambridgeScale)
   - System tự động tính toán eligibility
   ↓
5. ManagementStaff/Admin review và Approve Assessments
   - System tự động tạo Registration mới cho target program
   - Nếu rule.stopCurrentEnrollmentOnApproval = true: dừng enrollment hiện tại
   - Nếu rule.carryOverRemainingSessions = true: chuyển số buổi còn lại
   ↓
6. Registration mới được tạo với status = Pending
   - Parent cần confirm và thanh toán
```

### 6.2 Assessment Eligibility Logic

**PassFail Method:**
- Eligible nếu `passedInClass = true`

**Shields Method:**
- Eligible nếu:
  - `totalShields >= minimumShieldCount` (nếu có)
  - VÀ `skillShields >= minimumSkillShieldCount` (nếu có)

**CambridgeScale Method:**
- Tính `overallScore` = trung bình các điểm skills
- Nếu có `targetProgramId`:
  - Eligible nếu `overallScore >= minimumOverallScore`
- Nếu không có `targetProgramId` (dùng classificationBands):
  - Tìm band phù hợp với overallScore
  - Eligible nếu tìm thấy band và có targetProgramId trong band

### 6.3 Schedule Completion Logic

Schedule tự động chuyển sang Completed khi:
- TẤT CẢ participants có status = Completed hoặc NoShow hoặc Cancelled

### 6.4 Participant Completion Logic

Participant tự động chuyển sang Completed khi:
- Tạo assessment cho participant đó (thông qua scheduleParticipantId)

---

## 7. Common Error Codes & Messages

### 7.1 Authentication & Authorization Errors

| HTTP Code | Error Message | Mô Tả |
|-----------|---------------|-------|
| 401 | Unauthorized | Missing hoặc invalid token |
| 403 | Forbidden | User không có quyền truy cập resource |

### 7.2 Validation Errors

| HTTP Code | Error Type | Example Message |
|-----------|------------|-----------------|
| 400 | Required Field | "SourceProgramId is required" |
| 400 | Invalid Format | "ScheduledAt must be in the future" |
| 400 | Invalid Value | "Listening score must be between 0 and 100" |
| 400 | Business Rule | "Cannot update an approved assessment" |
| 400 | Conflict | "Room is not available at scheduled time" |

### 7.3 Not Found Errors

| HTTP Code | Error Message | Mô Tả |
|-----------|---------------|-------|
| 404 | Rule not found | Rule ID không tồn tại |
| 404 | Schedule not found | Schedule ID không tồn tại |
| 404 | Assessment not found | Assessment ID không tồn tại |
| 404 | Participant not found | Participant ID không tồn tại |

---

## 8. Notes & Best Practices

### 8.1 Frontend Implementation Tips

1. **Permission Checking:**
   - Frontend nên kiểm tra role của user để hiển thị/ẩn các buttons và features phù hợp
   - Không rely hoàn toàn vào backend authorization, nhưng backend luôn là final gate

2. **Schedule Creation:**
   - Nên gọi `/schedules/availability` trước khi submit form để kiểm tra conflicts
   - Hiển thị warnings nếu có teacher/student unavailable

3. **Assessment Form:**
   - Form fields thay đổi theo method (PassFail/Shields/CambridgeScale)
   - Validate điểm số (0-100) trước khi submit
   - Hiển thị eligibility result real-time

4. **Status Display:**
   - Dùng màu sắc khác nhau cho các status
   - Scheduled: blue, Completed: green, Cancelled: gray, NoShow: red

5. **Date/Time:**
   - Tất cả datetime từ API đều là UTC
   - Frontend cần convert sang local timezone để hiển thị

### 8.2 Common Use Cases

**UC1: Tạo Assessment Schedule cho cả class**
```
POST /api/program-progressions/schedules
Body: {
  sourceClassId: "...",
  scheduledAt: "2026-05-15T10:00:00Z",
  studentProfileIds: null  // null = all students in class
}
```

**UC2: Teacher tạo Assessment từ Schedule**
```
POST /api/program-progressions/assessments
Body: {
  scheduleParticipantId: "...",
  listeningScore: 85,
  speakingScore: 90,
  ...
}
```

**UC3: ManagementStaff approve nhiều assessments cùng lúc**
```
POST /api/program-progressions/assessments/bulk-approve
Body: {
  items: [
    { assessmentId: "...", tuitionPlanId: "..." },
    { assessmentId: "...", tuitionPlanId: "..." }
  ]
}
```

**UC4: Parent xem lịch assessment của con**
```
GET /api/program-progressions/my-assessment-schedules?studentProfileId=...
```

---

## 9. Changelog

| Date | Version | Changes |
|------|---------|---------|
| 2026-05-06 | 1.0 | Initial documentation |

---

**End of Document**
