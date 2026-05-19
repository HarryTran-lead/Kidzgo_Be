# API Usage Guide - Kidzgo Backend

Tài liệu này mô tả chi tiết cách sử dụng tất cả các API endpoints trong hệ thống Kidzgo.

## Mục lục

1. [Authentication APIs](#1-authentication-apis)
2. [Email Template APIs](#2-email-template-apis)
3. [Program APIs](#3-program-apis)
4. [Tuition Plan APIs](#4-tuition-plan-apis)
5. [Classroom APIs](#5-classroom-apis)
6. [Class APIs](#6-class-apis)
7. [Enrollment APIs](#7-enrollment-apis)
8. [Exam APIs](#8-exam-apis)
9. [Ticket APIs](#9-ticket-apis)
10. [Notification APIs](#10-notification-apis)
11. [Blog APIs](#11-blog-apis)
12. [File Upload APIs](#12-file-upload-apis)
13. [Master Data APIs](#13-master-data-apis)
14. [Teacher APIs](#14-teacher-apis)
15. [Student APIs](#15-student-apis)
16. [Session APIs](#16-session-apis)
17. [Media APIs](#17-media-apis)
18. [User Management APIs](#18-user-management-apis)
19. [Lead APIs](#19-lead-apis)
20. [Placement Test APIs](#20-placement-test-apis)
21. [Monthly Report APIs](#21-monthly-report-apis)
22. [Session Report APIs](#22-session-report-apis)
23. [Homework APIs](#23-homework-apis)
24. [Exercise APIs](#24-exercise-apis)
25. [Mission APIs](#25-mission-apis)
26. [Lesson Plan APIs](#26-lesson-plan-apis)
27. [Lesson Plan Template APIs](#27-lesson-plan-template-apis)
28. [Attendance APIs](#28-attendance-apis)
29. [Invoice APIs](#29-invoice-apis)
30. [Makeup Credit APIs](#30-makeup-credit-apis)
31. [Leave Request APIs](#31-leave-request-apis)
32. [Parent APIs](#32-parent-apis)
33. [Profile APIs](#33-profile-apis)
34. [Gamification APIs](#34-gamification-apis)
35. [Branch APIs](#35-branch-apis)
36. [User APIs](#36-user-apis)
37. [Student APIs](#37-student-apis)
38. [Teacher APIs](#38-teacher-apis)
39. [Session Management APIs](#39-session-management-apis)
40. [Notification Template APIs](#40-notification-template-apis)
41. [Webhook APIs](#41-webhook-apis)

---

## 1. Authentication APIs

Base URL: `/api/auth/`

### 1.1. Login

**Endpoint:** `POST /api/auth/login`

**Mô tả:** Đăng nhập vào hệ thống và nhận access token cùng refresh token.

**Authorization:** Không cần (Public)

**Request Body:**
```json
{
  "email": "user@example.com",
  "password": "password123"
}
```

**Response (200 OK):**
```json
{
  "isSuccess": true,
  "data": {
    "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
    "refreshToken": "refresh_token_string",
    "expiresIn": 3600
  }
}
```

**Lỗi có thể xảy ra:**
- `400 Bad Request`: Email hoặc password không đúng
- `404 Not Found`: User không tồn tại

---

### 1.2. Refresh Token

**Endpoint:** `POST /api/auth/refresh-token`

**Mô tả:** Lấy access token mới bằng refresh token.

**Authorization:** Không cần (Public)

**Request Body:**
```json
"refresh_token_string"
```

**Response (200 OK):**
```json
{
  "isSuccess": true,
  "data": {
    "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
    "refreshToken": "new_refresh_token_string",
    "expiresIn": 3600
  }
}
```

---

### 1.3. Change Password

**Endpoint:** `PUT /api/auth/change-password`

**Mô tả:** Thay đổi mật khẩu của user đang đăng nhập.

**Authorization:** Required (Bearer Token)

**Request Body:**
```json
{
  "currentPassword": "oldPassword123",
  "newPassword": "newPassword456"
}
```

**Response (200 OK):**
```json
{
  "isSuccess": true,
  "data": null
}
```

**Lỗi có thể xảy ra:**
- `400 Bad Request`: Mật khẩu hiện tại không đúng
- `401 Unauthorized`: Chưa đăng nhập

---

### 1.4. Forget Password

**Endpoint:** `POST /api/auth/forget-password`

**Mô tả:** Gửi email reset password.

**Authorization:** Không cần (Public)

**Request Body:**
```json
{
  "email": "user@example.com"
}
```

**Response (200 OK):**
```json
{
  "isSuccess": true,
  "data": null
}
```

---

### 1.5. Reset Password

**Endpoint:** `POST /api/auth/reset-password`

**Mô tả:** Đặt lại mật khẩu mới bằng token từ email.

**Authorization:** Không cần (Public)

**Request Body:**
```json
{
  "token": "reset_token_from_email",
  "newPassword": "newPassword123"
}
```

**Response (200 OK):**
```json
{
  "isSuccess": true,
  "data": null
}
```

**Lỗi có thể xảy ra:**
- `400 Bad Request`: Token không hợp lệ hoặc đã hết hạn

---

### 1.5.1. Reset Parent PIN

**Endpoint:** `POST /api/auth/reset-pin`

**Mô tả:** Đặt lại PIN mới cho Parent Profile bằng token từ email.

**Authorization:** Không cần (Public)

**Request Body:**
```json
{
  "token": "reset_token_from_email",
  "newPin": "1234"
}
```

**Response (200 OK):**
```json
{
  "isSuccess": true,
  "data": null
}
```

**Lỗi có thể xảy ra:**
- `400 Bad Request`: Token không hợp lệ, đã hết hạn hoặc PIN không hợp lệ

---

### 1.6. Change PIN

**Endpoint:** `PUT /api/auth/change-pin`

**Mô tả:** Đổi PIN cho user đang đăng nhập (Admin/Teacher/Staff).

**Authorization:** Required (Bearer Token)

**Request Body:**
```json
{
  "currentPin": "1234",
  "newPin": "5678"
}
```

**Response (200 OK):**
```json
{
  "isSuccess": true,
  "data": null
}
```

**Lỗi có thể xảy ra:**
- `400 Bad Request`: PIN hiện tại không đúng
- `401 Unauthorized`: Chưa đăng nhập

---

### 1.7. Get Profiles

**Endpoint:** `GET /api/auth/profiles`

**Mô tả:** Lấy danh sách Profiles của user đang đăng nhập (Parent/Student profiles).

**Authorization:** Required (Bearer Token)

**Request Body:** KhÃ´ng cáº§n

**Response (200 OK):**
```json
{
  "isSuccess": true,
  "data": [
    {
      "id": "guid",
      "displayName": "Test Student 01",
      "profileType": "Student"
    },
    {
      "id": "guid",
      "displayName": "Test Parent 01",
      "profileType": "Parent"
    }
  ]
}
```

---

### 1.8. Verify Parent PIN

**Endpoint:** `POST /api/auth/profiles/verify-parent-pin`

**Mô tả:** Xác thực PIN cho Parent Profile.

**Authorization:** Required (Bearer Token)

**Request Body:**
```json
{
  "profileId": "guid",
  "pin": "1234"
}
```

**Response (200 OK):**
```json
{
  "isSuccess": true,
  "data": null
}
```

**Lỗi có thể xảy ra:**
- `400 Bad Request`: PIN không đúng hoặc Profile không hợp lệ
- `404 Not Found`: Profile không tồn tại hoặc không phải Parent

---

### 1.9. Select Student Profile

**Endpoint:** `POST /api/auth/profiles/select-student`

**Mô tả:** Chọn Student Profile để xem dữ liệu (không cần PIN).

**Authorization:** Required (Bearer Token)

**Response (200 OK):**
```json
{
  "isSuccess": true,
  "data": null
}
```

**Lỗi có thể xảy ra:**
- `404 Not Found`: Profile không tồn tại hoặc không phải Student

---

### 1.10. Request Parent PIN Reset

**Endpoint:** `POST /api/auth/profiles/request-pin-reset`

**Mô tả:** Yêu cầu reset PIN cho Parent Profile (gửi email reset link).

**Authorization:** Required (Bearer Token)

**Response (200 OK):**
```json
{
  "isSuccess": true,
  "data": null
}
```

**Lỗi có thể xảy ra:**
- `400 Bad Request`: Profile không hợp lệ hoặc không có email
- `404 Not Found`: Profile không tồn tại hoặc không phải Parent

---

### 1.11. Get Current User

**Endpoint:** `GET /api/me`

**Mô tả:** Lấy thông tin user hiện tại bao gồm role, branchId, và danh sách profiles.

**Authorization:** Required (Bearer Token)

**Response (200 OK):**
```json
{
  "isSuccess": true,
  "data": {
    "id": "guid",
    "userName": "admin@kidzgo.com",
    "fullName": "Admin User",
    "email": "admin@kidzgo.com",
    "role": "Admin",
    "branchId": "guid",
    "branch": {
      "id": "guid",
      "code": "HN001",
      "name": "Chi nhánh Hà Nội",
      "address": "123 Đường ABC",
      "contactPhone": "0123456789",
      "contactEmail": "hanoi@kidzgo.com",
      "isActive": true
    },
    "profiles": [
      {
        "id": "guid",
        "displayName": "Test Student 01",
        "profileType": "Student"
      }
    ],
    "selectedProfileId": null,
    "isActive": true,
    "createdAt": "2024-01-01T00:00:00Z",
    "updatedAt": "2024-01-01T00:00:00Z"
  }
}
```

---

### 1.12. Logout

**Endpoint:** `POST /api/me/logout`

**Mô tả:** Đăng xuất - xóa tất cả refresh tokens của user.

**Authorization:** Required (Bearer Token)

**Response (200 OK):**
```json
{
  "isSuccess": true,
  "data": null
}
```

---

## 2. Email Template APIs

Base URL: `/api/`

### 2.1. Get Email Template

**Endpoint:** `GET /api/email/get`

**Mô tả:** Lấy thông tin email template.

**Authorization:** Required (Bearer Token)

**Response (200 OK):**
```json
{
  "isSuccess": true,
  "data": {
    "id": "guid",
    "header": "Email Header",
    "mainContent": "Main Content",
    "content": "Full Content"
  }
}
```

---

### 2.2. Update Email Template

**Endpoint:** `PUT /api/email/update-email`

**Mô tả:** Cập nhật email template.

**Authorization:** Required (Role: Admin)

**Request Body:**
```json
{
  "id": "guid",
  "header": "New Header",
  "mainContent": "New Main Content",
  "content": "New Full Content"
}
```

**Response (200 OK):**
```json
{
  "isSuccess": true,
  "data": {
    "id": "guid",
    "header": "New Header",
    "mainContent": "New Main Content",
    "content": "New Full Content"
  }
}
```

---

## 3. Program APIs

Base URL: `/api/programs`

### 3.1. Create Program

**Endpoint:** `POST /api/programs`

**Mô tả:** Tạo một Program mới. Program mới tạo sẽ có `IsActive = false` mặc định, cần duyệt qua API toggle-status để kích hoạt.

**Authorization:** Required (Roles: Admin, Staff)

**Request Body:**
```json
{
  "branchId": "11111111-1111-1111-1111-111111111111",
  "name": "Tiếng Anh Trẻ Em",
  "level": "Beginner",
  "totalSessions": 30,
  "defaultTuitionAmount": 5000000,
  "unitPriceSession": 166667,
  "description": "Chương trình học tiếng Anh cho trẻ em"
}
```

**Response (201 Created):**
```json
{
  "isSuccess": true,
  "data": {
    "id": "guid",
    "branchId": "11111111-1111-1111-1111-111111111111",
    "name": "Tiếng Anh Trẻ Em",
    "level": "Beginner",
    "totalSessions": 30,
    "defaultTuitionAmount": 5000000,
    "unitPriceSession": 166667,
    "description": "Chương trình học tiếng Anh cho trẻ em",
    "isActive": false,
    "createdAt": "2024-01-01T00:00:00Z",
    "updatedAt": "2024-01-01T00:00:00Z"
  }
}
```

**Headers:**
- `Location: /api/programs/{id}`

**Lỗi có thể xảy ra:**
- `400 Bad Request`: Dữ liệu không hợp lệ
- `404 Not Found`: Branch không tồn tại hoặc không active
- `409 Conflict`: Tên Program đã tồn tại

---

### 3.2. Get Programs

**Endpoint:** `GET /api/programs`

**Mô tả:** Lấy danh sách Programs với phân trang và filter. Kết quả được sắp xếp theo `CreatedAt` giảm dần (mới nhất trước).

**Authorization:** Required (Roles: Admin, Staff)

**Query Parameters:**
- `branchId` (Guid?, optional): Lọc theo Branch ID
- `searchTerm` (string?, optional): Tìm kiếm theo tên Program
- `isActive` (bool?, optional): Lọc theo trạng thái active
- `pageNumber` (int, default: 1): Số trang
- `pageSize` (int, default: 10): Số lượng items mỗi trang

**Example Request:**
```
GET /api/programs?branchId=11111111-1111-1111-1111-111111111111&isActive=true&pageNumber=1&pageSize=10
```

**Response (200 OK):**
```json
{
  "isSuccess": true,
  "data": {
    "items": [
      {
        "id": "guid",
        "branchId": "11111111-1111-1111-1111-111111111111",
        "name": "Tiếng Anh Trẻ Em",
        "level": "Beginner",
        "totalSessions": 30,
        "defaultTuitionAmount": 5000000,
        "unitPriceSession": 166667,
        "description": "Chương trình học tiếng Anh cho trẻ em",
        "isActive": true,
        "createdAt": "2024-01-01T00:00:00Z",
        "updatedAt": "2024-01-01T00:00:00Z"
      }
    ],
    "pageNumber": 1,
    "pageSize": 10,
    "totalCount": 1,
    "totalPages": 1
  }
}
```

---

### 3.3. Get Active Programs

**Endpoint:** `GET /api/programs/active`

**Mô tả:** Lấy danh sách tất cả Programs đang active (IsActive = true). Không cần authorization.

**Authorization:** Không cần (Public)

**Query Parameters:**
- `branchId` (Guid?, optional): Lọc theo Branch ID
- `searchTerm` (string?, optional): Tìm kiếm theo tên Program
- `pageNumber` (int, default: 1): Số trang
- `pageSize` (int, default: 10): Số lượng items mỗi trang

**Response (200 OK):** Tương tự như Get Programs, nhưng chỉ trả về Programs có `isActive = true`

---

### 3.4. Get Program By ID

**Endpoint:** `GET /api/programs/{id}`

**Mô tả:** Lấy thông tin chi tiết của một Program.

**Authorization:** Không cần (Public)

**Path Parameters:**
- `id` (Guid): ID của Program

**Response (200 OK):**
```json
{
  "isSuccess": true,
  "data": {
    "id": "guid",
    "branchId": "11111111-1111-1111-1111-111111111111",
    "name": "Tiếng Anh Trẻ Em",
    "level": "Beginner",
    "totalSessions": 30,
    "defaultTuitionAmount": 5000000,
    "unitPriceSession": 166667,
    "description": "Chương trình học tiếng Anh cho trẻ em",
    "isActive": true,
    "createdAt": "2024-01-01T00:00:00Z",
    "updatedAt": "2024-01-01T00:00:00Z"
  }
}
```

**Lỗi có thể xảy ra:**
- `404 Not Found`: Program không tồn tại

---

### 3.5. Update Program

**Endpoint:** `PUT /api/programs/{id}`

**Mô tả:** Cập nhật thông tin Program.

**Authorization:** Required (Roles: Admin, Staff)

**Path Parameters:**
- `id` (Guid): ID của Program

**Request Body:**
```json
{
  "branchId": "11111111-1111-1111-1111-111111111111",
  "name": "Tiếng Anh Trẻ Em - Updated",
  "level": "Intermediate",
  "totalSessions": 40,
  "defaultTuitionAmount": 6000000,
  "unitPriceSession": 150000,
  "description": "Chương trình học tiếng Anh cho trẻ em - Updated"
}
```

**Response (200 OK):**
```json
{
  "isSuccess": true,
  "data": {
    "id": "guid",
    "branchId": "11111111-1111-1111-1111-111111111111",
    "name": "Tiếng Anh Trẻ Em - Updated",
    "level": "Intermediate",
    "totalSessions": 40,
    "defaultTuitionAmount": 6000000,
    "unitPriceSession": 150000,
    "description": "Chương trình học tiếng Anh cho trẻ em - Updated",
    "isActive": true,
    "createdAt": "2024-01-01T00:00:00Z",
    "updatedAt": "2024-01-02T00:00:00Z"
  }
}
```

**Lỗi có thể xảy ra:**
- `404 Not Found`: Program không tồn tại
- `409 Conflict`: Tên Program đã tồn tại (nếu đổi tên)

---

### 3.6. Delete Program

**Endpoint:** `DELETE /api/programs/{id}`

**Mô tả:** Xóa mềm Program (set IsDeleted = true, IsActive = false).

**Authorization:** Required (Role: Admin)

**Path Parameters:**
- `id` (Guid): ID của Program

**Response (200 OK):**
```json
{
  "isSuccess": true,
  "data": null
}
```

**Lỗi có thể xảy ra:**
- `404 Not Found`: Program không tồn tại
- `409 Conflict`: Program đang có Tuition Plans liên quan

---

### 3.7. Toggle Program Status

**Endpoint:** `PATCH /api/programs/{id}/toggle-status`

**Mô tả:** Kích hoạt/Vô hiệu hóa Program (toggle IsActive).

**Authorization:** Required (Roles: Admin, Staff)

**Path Parameters:**
- `id` (Guid): ID của Program

**Response (200 OK):**
```json
{
  "isSuccess": true,
  "data": {
    "id": "guid",
    "isActive": true
  }
}
```

**Lỗi có thể xảy ra:**
- `404 Not Found`: Program không tồn tại hoặc đã bị xóa

---

## 4. Tuition Plan APIs

Base URL: `/api/tuition-plans`

### 4.1. Create Tuition Plan

**Endpoint:** `POST /api/tuition-plans`

**Mô tả:** Tạo một Tuition Plan mới. Tuition Plan mới tạo sẽ có `IsActive = false` mặc định. Nếu `branchId` là `null`, Tuition Plan sẽ áp dụng cho tất cả các chi nhánh.

**Authorization:** Required (Roles: Admin, Staff)

**Request Body:**
```json
{
  "branchId": null,
  "programId": "12345678-1234-1234-1234-123456789012",
  "name": "Gói 30 buổi - Khuyến mãi",
  "totalSessions": 30,
  "tuitionAmount": 4500000,
  "unitPriceSession": 150000,
  "currency": "VND"
}
```

**Lưu ý:** Để tạo Tuition Plan cho tất cả chi nhánh, gửi `branchId: null` hoặc không gửi field này.

**Response (201 Created):**
```json
{
  "isSuccess": true,
  "data": {
    "id": "guid",
    "branchId": null,
    "programId": "12345678-1234-1234-1234-123456789012",
    "name": "Gói 30 buổi - Khuyến mãi",
    "totalSessions": 30,
    "tuitionAmount": 4500000,
    "unitPriceSession": 150000,
    "currency": "VND",
    "isActive": false,
    "createdAt": "2024-01-01T00:00:00Z",
    "updatedAt": "2024-01-01T00:00:00Z"
  }
}
```

**Headers:**
- `Location: /api/tuition-plans/{id}`

**Lỗi có thể xảy ra:**
- `400 Bad Request`: Dữ liệu không hợp lệ
- `404 Not Found`: Program không tồn tại hoặc không active

---

**Contract correction for `POST /api/tuition-plans` and `PUT /api/tuition-plans/{id}`:**
- Required body fields: `programId`, `levelId`, `name`, `totalSessions`, `tuitionAmount`, `currency`
- Optional body fields: `moduleId`, `learningTicketTypeId`
- Do not send `branchId` in the body
- Do not send `unitPriceSession` in the body
- FE must send `totalSessions`, not `sessionCount`
- Current backend returns `isActive = true` after create

**Correct request body example:**
```json
{
  "programId": "12345678-1234-1234-1234-123456789012",
  "levelId": "22345678-1234-1234-1234-123456789012",
  "moduleId": null,
  "learningTicketTypeId": null,
  "name": "Starter 24",
  "totalSessions": 24,
  "tuitionAmount": 2400000,
  "currency": "VND"
}
```

### 4.2. Get Tuition Plans

**Endpoint:** `GET /api/tuition-plans`

**Mô tả:** Lấy danh sách Tuition Plans với phân trang và filter. Kết quả được sắp xếp theo `CreatedAt` giảm dần. Nếu filter theo `branchId`, sẽ trả về cả Tuition Plans của branch đó và Tuition Plans global (branchId = null).

**Authorization:** Required (Roles: Admin, Staff)

**Query Parameters:**
- `branchId` (Guid?, optional): Lọc theo Branch ID (sẽ bao gồm cả global plans)
- `programId` (Guid?, optional): Lọc theo Program ID
- `isActive` (bool?, optional): Lọc theo trạng thái active
- `pageNumber` (int, default: 1): Số trang
- `pageSize` (int, default: 10): Số lượng items mỗi trang

**Example Request:**
```
GET /api/tuition-plans?branchId=11111111-1111-1111-1111-111111111111&programId=12345678-1234-1234-1234-123456789012&isActive=true
```

**Response (200 OK):**
```json
{
  "isSuccess": true,
  "data": {
    "items": [
      {
        "id": "guid",
        "branchId": null,
        "programId": "12345678-1234-1234-1234-123456789012",
        "name": "Gói 30 buổi - Khuyến mãi",
        "totalSessions": 30,
        "tuitionAmount": 4500000,
        "unitPriceSession": 150000,
        "currency": "VND",
        "isActive": true,
        "createdAt": "2024-01-01T00:00:00Z",
        "updatedAt": "2024-01-01T00:00:00Z"
      }
    ],
    "pageNumber": 1,
    "pageSize": 10,
    "totalCount": 1,
    "totalPages": 1
  }
}
```

---

### 4.3. Get Active Tuition Plans

**Endpoint:** `GET /api/tuition-plans/active`

**Mô tả:** Lấy danh sách tất cả Tuition Plans đang active (IsActive = true). Không cần authorization.

**Authorization:** Không cần (Public)

**Query Parameters:**
- `branchId` (Guid?, optional): Lọc theo Branch ID
- `programId` (Guid?, optional): Lọc theo Program ID
- `pageNumber` (int, default: 1): Số trang
- `pageSize` (int, default: 10): Số lượng items mỗi trang

**Response (200 OK):** Tương tự như Get Tuition Plans, nhưng chỉ trả về Tuition Plans có `isActive = true`

---

### 4.4. Get Tuition Plan By ID

**Endpoint:** `GET /api/tuition-plans/{id}`

**Mô tả:** Lấy thông tin chi tiết của một Tuition Plan.

**Authorization:** Required (Roles: Admin, Staff)

**Path Parameters:**
- `id` (Guid): ID của Tuition Plan

**Response (200 OK):**
```json
{
  "isSuccess": true,
  "data": {
    "id": "guid",
    "branchId": null,
    "programId": "12345678-1234-1234-1234-123456789012",
    "name": "Gói 30 buổi - Khuyến mãi",
    "totalSessions": 30,
    "tuitionAmount": 4500000,
    "unitPriceSession": 150000,
    "currency": "VND",
    "isActive": true,
    "createdAt": "2024-01-01T00:00:00Z",
    "updatedAt": "2024-01-01T00:00:00Z"
  }
}
```

---

### 4.5. Update Tuition Plan

**Endpoint:** `PUT /api/tuition-plans/{id}`

**Mô tả:** Cập nhật thông tin Tuition Plan.

**Authorization:** Required (Roles: Admin, Staff)

**Path Parameters:**
- `id` (Guid): ID của Tuition Plan

**Request Body:**
```json
{
  "branchId": null,
  "programId": "12345678-1234-1234-1234-123456789012",
  "name": "Gói 30 buổi - Khuyến mãi - Updated",
  "totalSessions": 30,
  "tuitionAmount": 4000000,
  "unitPriceSession": 133333,
  "currency": "VND"
}
```

**Response (200 OK):** Tương tự như Create Tuition Plan

---

### 4.6. Delete Tuition Plan

**Endpoint:** `DELETE /api/tuition-plans/{id}`

**Mô tả:** Xóa mềm Tuition Plan (set IsDeleted = true, IsActive = false).

**Authorization:** Required (Role: Admin)

**Path Parameters:**
- `id` (Guid): ID của Tuition Plan

**Response (200 OK):**
```json
{
  "isSuccess": true,
  "data": null
}
```

**Lỗi có thể xảy ra:**
- `404 Not Found`: Tuition Plan không tồn tại
- `409 Conflict`: Tuition Plan đang có enrollments liên quan

---

### 4.7. Toggle Tuition Plan Status

**Endpoint:** `PATCH /api/tuition-plans/{id}/toggle-status`

**Mô tả:** Kích hoạt/Vô hiệu hóa Tuition Plan (toggle IsActive).

**Authorization:** Required (Roles: Admin, Staff)

**Path Parameters:**
- `id` (Guid): ID của Tuition Plan

**Response (200 OK):**
```json
{
  "isSuccess": true,
  "data": {
    "id": "guid",
    "isActive": true
  }
}
```

---

## 5. Classroom APIs

Base URL: `/api/classrooms`

### 5.1. Create Classroom

**Endpoint:** `POST /api/classrooms`

**Mô tả:** Tạo một Classroom mới. Classroom mới tạo sẽ có `IsActive = false` mặc định.

**Authorization:** Required (Roles: Admin, Staff)

**Request Body:**
```json
{
  "branchId": "11111111-1111-1111-1111-111111111111",
  "name": "Phòng học A1",
  "capacity": 20,
  "note": "Phòng học có máy lạnh và máy chiếu"
}
```

**Response (201 Created):**
```json
{
  "isSuccess": true,
  "data": {
    "id": "guid",
    "branchId": "11111111-1111-1111-1111-111111111111",
    "name": "Phòng học A1",
    "capacity": 20,
    "note": "Phòng học có máy lạnh và máy chiếu",
    "isActive": false,
    "createdAt": "2024-01-01T00:00:00Z",
    "updatedAt": "2024-01-01T00:00:00Z"
  }
}
```

**Headers:**
- `Location: /api/classrooms/{id}`

**Lỗi có thể xảy ra:**
- `400 Bad Request`: Dữ liệu không hợp lệ
- `404 Not Found`: Branch không tồn tại hoặc không active

---

### 5.2. Get Classrooms

**Endpoint:** `GET /api/classrooms`

**Mô tả:** Lấy danh sách Classrooms với phân trang và filter.

**Authorization:** Required (Roles: Admin, Staff)

**Query Parameters:**
- `branchId` (Guid?, optional): Lọc theo Branch ID
- `searchTerm` (string?, optional): Tìm kiếm theo tên Classroom
- `isActive` (bool?, optional): Lọc theo trạng thái active
- `pageNumber` (int, default: 1): Số trang
- `pageSize` (int, default: 10): Số lượng items mỗi trang

**Response (200 OK):**
```json
{
  "isSuccess": true,
  "data": {
    "items": [
      {
        "id": "guid",
        "branchId": "11111111-1111-1111-1111-111111111111",
        "name": "Phòng học A1",
        "capacity": 20,
        "note": "Phòng học có máy lạnh và máy chiếu",
        "isActive": true,
        "createdAt": "2024-01-01T00:00:00Z",
        "updatedAt": "2024-01-01T00:00:00Z"
      }
    ],
    "pageNumber": 1,
    "pageSize": 10,
    "totalCount": 1,
    "totalPages": 1
  }
}
```

---

### 5.3. Get Active Classrooms

**Endpoint:** `GET /api/classrooms/active`

**Mô tả:** Lấy danh sách tất cả Classrooms đang active (IsActive = true). Không cần authorization.

**Authorization:** Không cần (Public)

**Query Parameters:**
- `branchId` (Guid?, optional): Lọc theo Branch ID
- `searchTerm` (string?, optional): Tìm kiếm theo tên Classroom
- `pageNumber` (int, default: 1): Số trang
- `pageSize` (int, default: 10): Số lượng items mỗi trang

**Response (200 OK):** Tương tự như Get Classrooms, nhưng chỉ trả về Classrooms có `isActive = true`

---

### 5.4. Get Classroom By ID

**Endpoint:** `GET /api/classrooms/{id}`

**Mô tả:** Lấy thông tin chi tiết của một Classroom.

**Authorization:** Required (Roles: Admin, Staff)

**Path Parameters:**
- `id` (Guid): ID của Classroom

**Response (200 OK):**
```json
{
  "isSuccess": true,
  "data": {
    "id": "guid",
    "branchId": "11111111-1111-1111-1111-111111111111",
    "name": "Phòng học A1",
    "capacity": 20,
    "note": "Phòng học có máy lạnh và máy chiếu",
    "isActive": true,
    "createdAt": "2024-01-01T00:00:00Z",
    "updatedAt": "2024-01-01T00:00:00Z"
  }
}
```

---

### 5.5. Update Classroom

**Endpoint:** `PUT /api/classrooms/{id}`

**Mô tả:** Cập nhật thông tin Classroom.

**Authorization:** Required (Roles: Admin, Staff)

**Path Parameters:**
- `id` (Guid): ID của Classroom

**Request Body:**
```json
{
  "branchId": "11111111-1111-1111-1111-111111111111",
  "name": "Phòng học A1 - Updated",
  "capacity": 25,
  "note": "Phòng học có máy lạnh, máy chiếu và bảng thông minh"
}
```

**Response (200 OK):** Tương tự như Create Classroom

---

### 5.6. Delete Classroom

**Endpoint:** `DELETE /api/classrooms/{id}`

**Mô tả:** Xóa cứng Classroom (hard delete). Chỉ xóa được nếu Classroom không có active sessions.

**Authorization:** Required (Role: Admin)

**Path Parameters:**
- `id` (Guid): ID của Classroom

**Response (200 OK):**
```json
{
  "isSuccess": true,
  "data": null
}
```

**Lỗi có thể xảy ra:**
- `404 Not Found`: Classroom không tồn tại
- `409 Conflict`: Classroom đang có active sessions

---

### 5.7. Toggle Classroom Status

**Endpoint:** `PATCH /api/classrooms/{id}/toggle-status`

**Mô tả:** Kích hoạt/Vô hiệu hóa Classroom (toggle IsActive).

**Authorization:** Required (Roles: Admin, Staff)

**Path Parameters:**
- `id` (Guid): ID của Classroom

**Response (200 OK):**
```json
{
  "isSuccess": true,
  "data": {
    "id": "guid",
    "isActive": true
  }
}
```

---

## 6. Class APIs

Base URL: `/api/classes`

### 6.1. Create Class

**Endpoint:** `POST /api/classes`

**Mô tả:** Tạo một Class mới. Class mới tạo sẽ có `Status = Planned` mặc định. **Lưu ý:** Main Teacher và Assistant Teacher phải thuộc cùng branch với Class.

**Authorization:** Required (Roles: Admin, Staff)

**Request Body:**
```json
{
  "branchId": "11111111-1111-1111-1111-111111111111",
  "programId": "12345678-1234-1234-1234-123456789012",
  "code": "ENG2-2024",
  "title": "Tiếng Anh Lớp 2 - 2024",
  "mainTeacherId": "aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa",
  "assistantTeacherId": "bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb",
  "startDate": "2024-01-15",
  "endDate": "2024-04-15",
  "capacity": 15,
  "schedulePattern": "RRULE:FREQ=WEEKLY;BYDAY=MO,WE,FR;BYHOUR=18;BYMINUTE=0"
}
```

**Response (201 Created):**
```json
{
  "isSuccess": true,
  "data": {
    "id": "guid",
    "branchId": "11111111-1111-1111-1111-111111111111",
    "programId": "12345678-1234-1234-1234-123456789012",
    "code": "ENG2-2024",
    "title": "Tiếng Anh Lớp 2 - 2024",
    "mainTeacherId": "aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa",
    "assistantTeacherId": "bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb",
    "startDate": "2024-01-15",
    "endDate": "2024-04-15",
    "status": "Planned",
    "capacity": 15,
    "schedulePattern": "RRULE:FREQ=WEEKLY;BYDAY=MO,WE,FR;BYHOUR=18;BYMINUTE=0"
  }
}
```

**Headers:**
- `Location: /api/classes/{id}`

**Lỗi có thể xảy ra:**
- `400 Bad Request`: Dữ liệu không hợp lệ
- `404 Not Found`: Branch, Program, hoặc Teacher không tồn tại
- `409 Conflict`: 
  - Class code đã tồn tại
  - Teacher không thuộc cùng branch với Class

---

### 6.2. Get Classes

**Endpoint:** `GET /api/classes`

**Mô tả:** Lấy danh sách Classes với phân trang và filter.

**Authorization:** Required (Roles: Admin, Staff)

**Query Parameters:**
- `branchId` (Guid?, optional): Lọc theo Branch ID
- `programId` (Guid?, optional): Lọc theo Program ID
- `status` (string?, optional): Lọc theo status (Planned, Active, Closed)
- `searchTerm` (string?, optional): Tìm kiếm theo class code hoặc title
- `pageNumber` (int, default: 1): Số trang
- `pageSize` (int, default: 10): Số lượng items mỗi trang

**Example Request:**
```
GET /api/classes?branchId=11111111-1111-1111-1111-111111111111&status=Active&pageNumber=1&pageSize=10
```

**Response (200 OK):**
```json
{
  "isSuccess": true,
  "data": {
    "items": [
      {
        "id": "guid",
        "branchId": "11111111-1111-1111-1111-111111111111",
        "programId": "12345678-1234-1234-1234-123456789012",
        "code": "ENG2-2024",
        "title": "Tiếng Anh Lớp 2 - 2024",
        "mainTeacherId": "aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa",
        "mainTeacherName": "Nguyễn Văn A",
        "assistantTeacherId": "bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb",
        "assistantTeacherName": "Trần Thị B",
        "startDate": "2024-01-15",
        "endDate": "2024-04-15",
        "status": "Active",
        "capacity": 15,
        "schedulePattern": "RRULE:FREQ=WEEKLY;BYDAY=MO,WE,FR;BYHOUR=18;BYMINUTE=0"
      }
    ],
    "pageNumber": 1,
    "pageSize": 10,
    "totalCount": 1,
    "totalPages": 1
  }
}
```

---

### 6.3. Get Class By ID

**Endpoint:** `GET /api/classes/{id}`

**Mô tả:** Lấy thông tin chi tiết của một Class.

**Authorization:** Required (Roles: Admin, Staff)

**Path Parameters:**
- `id` (Guid): ID của Class

**Response (200 OK):**
```json
{
  "isSuccess": true,
  "data": {
    "id": "guid",
    "branchId": "11111111-1111-1111-1111-111111111111",
    "programId": "12345678-1234-1234-1234-123456789012",
    "code": "ENG2-2024",
    "title": "Tiếng Anh Lớp 2 - 2024",
    "mainTeacherId": "aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa",
    "mainTeacherName": "Nguyễn Văn A",
    "assistantTeacherId": "bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb",
    "assistantTeacherName": "Trần Thị B",
    "startDate": "2024-01-15",
    "endDate": "2024-04-15",
    "status": "Active",
    "capacity": 15,
    "schedulePattern": "RRULE:FREQ=WEEKLY;BYDAY=MO,WE,FR;BYHOUR=18;BYMINUTE=0"
  }
}
```

---

### 6.4. Update Class

**Endpoint:** `PUT /api/classes/{id}`

**Mô tả:** Cập nhật thông tin Class. **Lưu ý:** Main Teacher và Assistant Teacher phải thuộc cùng branch với Class.

**Authorization:** Required (Roles: Admin, Staff)

**Path Parameters:**
- `id` (Guid): ID của Class

**Request Body:**
```json
{
  "branchId": "11111111-1111-1111-1111-111111111111",
  "programId": "12345678-1234-1234-1234-123456789012",
  "code": "ENG2-2024-UPDATED",
  "title": "Tiếng Anh Lớp 2 - 2024 - Updated",
  "mainTeacherId": "aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa",
  "assistantTeacherId": "bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb",
  "startDate": "2024-01-20",
  "endDate": "2024-04-20",
  "capacity": 20,
  "schedulePattern": "RRULE:FREQ=WEEKLY;BYDAY=TU,TH;BYHOUR=19;BYMINUTE=0"
}
```

**Response (200 OK):** Tương tự như Create Class

**Lỗi có thể xảy ra:**
- `404 Not Found`: Class, Branch, Program, hoặc Teacher không tồn tại
- `409 Conflict`: 
  - Class code đã tồn tại (nếu đổi code)
  - Teacher không thuộc cùng branch với Class

---

### 6.5. Delete Class

**Endpoint:** `DELETE /api/classes/{id}`

**Mô tả:** Xóa mềm Class bằng cách set `Status = Closed`. Chỉ xóa được nếu Class không có active enrollments.

**Authorization:** Required (Role: Admin)

**Path Parameters:**
- `id` (Guid): ID của Class

**Response (200 OK):**
```json
{
  "isSuccess": true,
  "data": null
}
```

**Lỗi có thể xảy ra:**
- `404 Not Found`: Class không tồn tại
- `409 Conflict`: Class đang có active enrollments

---

### 6.6. Change Class Status

**Endpoint:** `PATCH /api/classes/{id}/status`

**Mô tả:** Thay đổi trạng thái Class (Planned → Active → Closed). Không thể chuyển từ Closed về Planned.

**Authorization:** Required (Roles: Admin, Staff)

**Path Parameters:**
- `id` (Guid): ID của Class

**Request Body:**
```json
{
  "status": "Active"
}
```

**Status Values:**
- `"Planned"`: Class đã được lên kế hoạch
- `"Active"`: Class đang hoạt động
- `"Closed"`: Class đã đóng

**Response (200 OK):**
```json
{
  "isSuccess": true,
  "data": {
    "id": "guid",
    "status": "Active"
  }
}
```

**Lỗi có thể xảy ra:**
- `400 Bad Request`: Không thể chuyển từ Closed về Planned
- `404 Not Found`: Class không tồn tại

---

### 6.7. Assign Teacher

**Endpoint:** `PATCH /api/classes/{id}/assign-teacher`

**Mô tả:** Gán Main Teacher và/hoặc Assistant Teacher cho Class. **Lưu ý:** Teacher phải thuộc cùng branch với Class.

**Authorization:** Required (Roles: Admin, Staff)

**Path Parameters:**
- `id` (Guid): ID của Class

**Request Body:**
```json
{
  "mainTeacherId": "aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa",
  "assistantTeacherId": "bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"
}
```

**Lưu ý:** Có thể chỉ gán `mainTeacherId` hoặc chỉ `assistantTeacherId`, hoặc cả hai. Để xóa teacher, gửi `null`.

**Response (200 OK):**
```json
{
  "isSuccess": true,
  "data": {
    "classId": "guid",
    "mainTeacherId": "aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa",
    "mainTeacherName": "Nguyễn Văn A",
    "assistantTeacherId": "bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb",
    "assistantTeacherName": "Trần Thị B"
  }
}
```

**Lỗi có thể xảy ra:**
- `404 Not Found`: Class hoặc Teacher không tồn tại
- `409 Conflict`: Teacher không thuộc cùng branch với Class

---

### 6.8. Check Class Capacity

**Endpoint:** `GET /api/classes/{id}/capacity`

**Mô tả:** Kiểm tra capacity của Class (số lượng học sinh hiện tại và capacity tối đa).

**Authorization:** Required (Roles: Admin, Staff)

**Path Parameters:**
- `id` (Guid): ID của Class

**Response (200 OK):**
```json
{
  "isSuccess": true,
  "data": {
    "classId": "guid",
    "capacity": 15,
    "currentEnrollmentCount": 10,
    "availableSlots": 5
  }
}
```

**Lỗi có thể xảy ra:**
- `404 Not Found`: Class không tồn tại

---

## 7. Enrollment APIs

Base URL: `/api/enrollments`

### 7.1. Create Enrollment

**Endpoint:** `POST /api/enrollments`

**Mô tả:** Ghi danh học sinh vào Class. Enrollment mới tạo sẽ có `Status = Active` mặc định.

**Authorization:** Required (Roles: Admin, Staff)

**Request Body:**
```json
{
  "classId": "guid",
  "studentProfileId": "guid",
  "enrollDate": "2024-01-15",
  "tuitionPlanId": "guid"
}
```

**Response (201 Created):**
```json
{
  "isSuccess": true,
  "data": {
    "id": "guid",
    "classId": "guid",
    "classCode": "ENG2-2024",
    "classTitle": "Tiếng Anh Lớp 2 - 2024",
    "studentProfileId": "guid",
    "studentName": "Nguyễn Văn A",
    "enrollDate": "2024-01-15",
    "status": "Active",
    "tuitionPlanId": "guid",
    "tuitionPlanName": "Gói 30 buổi",
    "createdAt": "2024-01-15T10:00:00Z",
    "updatedAt": "2024-01-15T10:00:00Z"
  }
}
```

**Headers:**
- `Location: /api/enrollments/{id}`

**Lỗi có thể xảy ra:**
- `400 Bad Request`: Dữ liệu không hợp lệ
- `404 Not Found`: Class, Student Profile, hoặc Tuition Plan không tồn tại
- `409 Conflict`: 
  - Học sinh đã được ghi danh vào Class này
  - Class đã đầy (không còn slot)
  - Tuition Plan không thuộc Program của Class

---

### 7.2. Get Enrollments

**Endpoint:** `GET /api/enrollments`

**Mô tả:** Xem danh sách học sinh trong Class với phân trang và filter.

**Authorization:** Required (Roles: Admin, Staff)

**Query Parameters:**
- `classId` (Guid?, optional): Lọc theo Class ID
- `studentProfileId` (Guid?, optional): Lọc theo Student Profile ID
- `status` (string?, optional): Lọc theo status (Active, Paused, Dropped)
- `pageNumber` (int, default: 1): Số trang
- `pageSize` (int, default: 10): Số lượng items mỗi trang

**Example Request:**
```
GET /api/enrollments?classId=guid&status=Active&pageNumber=1&pageSize=10
```

**Response (200 OK):**
```json
{
  "isSuccess": true,
  "data": {
    "items": [
      {
        "id": "guid",
        "classId": "guid",
        "classCode": "ENG2-2024",
        "classTitle": "Tiếng Anh Lớp 2 - 2024",
        "studentProfileId": "guid",
        "studentName": "Nguyễn Văn A",
        "enrollDate": "2024-01-15",
        "status": "Active",
        "tuitionPlanId": "guid",
        "tuitionPlanName": "Gói 30 buổi",
        "createdAt": "2024-01-15T10:00:00Z",
        "updatedAt": "2024-01-15T10:00:00Z"
      }
    ],
    "pageNumber": 1,
    "pageSize": 10,
    "totalCount": 1,
    "totalPages": 1
  }
}
```

---

### 7.3. Get Enrollment By ID

**Endpoint:** `GET /api/enrollments/{id}`

**Mô tả:** Xem chi tiết Enrollment.

**Authorization:** Required (Roles: Admin, Staff)

**Path Parameters:**
- `id` (Guid): ID của Enrollment

**Response (200 OK):**
```json
{
  "isSuccess": true,
  "data": {
    "id": "guid",
    "classId": "guid",
    "classCode": "ENG2-2024",
    "classTitle": "Tiếng Anh Lớp 2 - 2024",
    "studentProfileId": "guid",
    "studentName": "Nguyễn Văn A",
    "enrollDate": "2024-01-15",
    "status": "Active",
    "tuitionPlanId": "guid",
    "tuitionPlanName": "Gói 30 buổi",
    "createdAt": "2024-01-15T10:00:00Z",
    "updatedAt": "2024-01-15T10:00:00Z"
  }
}
```

**Lỗi có thể xảy ra:**
- `404 Not Found`: Enrollment không tồn tại

---

### 7.4. Update Enrollment

**Endpoint:** `PUT /api/enrollments/{id}`

**Mô tả:** Cập nhật thông tin Enrollment (chủ yếu là EnrollDate và TuitionPlanId).

**Authorization:** Required (Roles: Admin, Staff)

**Path Parameters:**
- `id` (Guid): ID của Enrollment

**Request Body:**
```json
{
  "enrollDate": "2024-01-20",
  "tuitionPlanId": "guid"
}
```

**Response (200 OK):** Tương tự như Get Enrollment By ID

**Lỗi có thể xảy ra:**
- `404 Not Found`: Enrollment hoặc Tuition Plan không tồn tại
- `409 Conflict`: Tuition Plan không thuộc Program của Class

---

### 7.5. Pause Enrollment

**Endpoint:** `PATCH /api/enrollments/{id}/pause`

**Mô tả:** Tạm dừng Enrollment (chuyển status từ Active → Paused).

**Authorization:** Required (Roles: Admin, Staff)

**Path Parameters:**
- `id` (Guid): ID của Enrollment

**Response (200 OK):**
```json
{
  "isSuccess": true,
  "data": {
    "id": "guid",
    "status": "Paused",
    "updatedAt": "2024-01-20T10:00:00Z"
  }
}
```

**Lỗi có thể xảy ra:**
- `404 Not Found`: Enrollment không tồn tại
- `409 Conflict`: Enrollment không ở trạng thái Active

---

### 7.6. Drop Enrollment

**Endpoint:** `PATCH /api/enrollments/{id}/drop`

**Mô tả:** Hủy Enrollment (chuyển status → Dropped).

**Authorization:** Required (Roles: Admin, Staff)

**Path Parameters:**
- `id` (Guid): ID của Enrollment

**Response (200 OK):**
```json
{
  "isSuccess": true,
  "data": {
    "id": "guid",
    "status": "Dropped",
    "updatedAt": "2024-01-20T10:00:00Z"
  }
}
```

**Lỗi có thể xảy ra:**
- `404 Not Found`: Enrollment không tồn tại

---

### 7.7. Reactivate Enrollment

**Endpoint:** `PATCH /api/enrollments/{id}/reactivate`

**Mô tả:** Kích hoạt lại Enrollment (chuyển status từ Paused → Active). Kiểm tra capacity trước khi reactivate.

**Authorization:** Required (Roles: Admin, Staff)

**Path Parameters:**
- `id` (Guid): ID của Enrollment

**Response (200 OK):**
```json
{
  "isSuccess": true,
  "data": {
    "id": "guid",
    "status": "Active",
    "updatedAt": "2024-01-20T10:00:00Z"
  }
}
```

**Lỗi có thể xảy ra:**
- `404 Not Found`: Enrollment không tồn tại
- `409 Conflict`: 
  - Enrollment không ở trạng thái Paused
  - Class đã đầy (không còn slot)

---

### 7.8. Assign Tuition Plan

**Endpoint:** `PATCH /api/enrollments/{id}/assign-tuition-plan`

**Mô tả:** Gán Tuition Plan cho Enrollment.

**Authorization:** Required (Roles: Admin, Staff)

**Path Parameters:**
- `id` (Guid): ID của Enrollment

**Request Body:**
```json
{
  "tuitionPlanId": "guid"
}
```

**Response (200 OK):** Tương tự như Get Enrollment By ID

**Lỗi có thể xảy ra:**
- `404 Not Found`: Enrollment hoặc Tuition Plan không tồn tại
- `409 Conflict`: 
  - Tuition Plan không active
  - Tuition Plan không thuộc Program của Class

---

### 7.9. Get Student Enrollment History

**Endpoint:** `GET /api/enrollments/student/{studentProfileId}`

**Mô tả:** Xem lịch sử Enrollment của học sinh.

**Authorization:** Required (Roles: Admin, Staff)

**Path Parameters:**
- `studentProfileId` (Guid): ID của Student Profile

**Query Parameters:**
- `pageNumber` (int, default: 1): Số trang
- `pageSize` (int, default: 10): Số lượng items mỗi trang

**Response (200 OK):**
```json
{
  "isSuccess": true,
  "data": {
    "items": [
      {
        "id": "guid",
        "classId": "guid",
        "classCode": "ENG2-2024",
        "classTitle": "Tiếng Anh Lớp 2 - 2024",
        "enrollDate": "2024-01-15",
        "status": "Active",
        "tuitionPlanId": "guid",
        "tuitionPlanName": "Gói 30 buổi",
        "createdAt": "2024-01-15T10:00:00Z",
        "updatedAt": "2024-01-15T10:00:00Z"
      }
    ],
    "pageNumber": 1,
    "pageSize": 10,
    "totalCount": 1,
    "totalPages": 1
  }
}
```

---

## 8. Exam APIs

Base URL: `/api/exams`

### 8.1. Create Exam

**Endpoint:** `POST /api/exams`

**Mô tả:** UC-152: Tạo Exam cho Class. UC-152a: Thiết lập thời gian thi. UC-152b: Thiết lập settings.

**Authorization:** Required (Roles: Teacher, Staff, Admin)

**Request Body:**
```json
{
  "classId": "guid",
  "examType": "Midterm",
  "date": "2024-01-15",
  "maxScore": 100,
  "description": "Bài thi giữa kỳ",
  "scheduledStartTime": "2024-01-15T14:00:00Z",
  "timeLimitMinutes": 90,
  "allowLateStart": true,
  "lateStartToleranceMinutes": 15,
  "autoSubmitOnTimeLimit": true,
  "preventCopyPaste": true,
  "preventNavigation": true,
  "showResultsImmediately": false
}
```

**ExamType Values:**
- `"Placement"`: Bài thi xếp lớp
- `"Progress"`: Bài kiểm tra tiến độ
- `"Midterm"`: Bài thi giữa kỳ
- `"Final"`: Bài thi cuối kỳ
- `"Speaking"`: Bài thi nói

**Response (201 Created):**
```json
{
  "isSuccess": true,
  "data": {
    "id": "guid",
    "classId": "guid",
    "classCode": "ENG2-2024",
    "classTitle": "Tiếng Anh Lớp 2 - 2024",
    "examType": "Midterm",
    "date": "2024-01-15",
    "maxScore": 100,
    "description": "Bài thi giữa kỳ",
    "scheduledStartTime": "2024-01-15T14:00:00Z",
    "timeLimitMinutes": 90,
    "allowLateStart": true,
    "lateStartToleranceMinutes": 15,
    "autoSubmitOnTimeLimit": true,
    "preventCopyPaste": true,
    "preventNavigation": true,
    "showResultsImmediately": false,
    "createdBy": "guid",
    "createdByName": "Teacher Nguyễn Văn A",
    "createdAt": "2024-01-10T10:00:00Z"
  }
}
```

**Headers:**
- `Location: /api/exams/{id}`

**Lỗi có thể xảy ra:**
- `400 Bad Request`: Dữ liệu không hợp lệ
- `404 Not Found`: Class không tồn tại hoặc không active

---

### 8.2. Get Exams

**Endpoint:** `GET /api/exams`

**Mô tả:** UC-153: Xem danh sách Exams của Class (filter theo classId).

**Authorization:** Required (Bearer Token)

**Query Parameters:**
- `classId` (Guid?, optional): Lọc theo Class ID
- `examType` (ExamType?, optional): Lọc theo exam type
- `pageNumber` (int, default: 1): Số trang
- `pageSize` (int, default: 10): Số lượng items mỗi trang

**Response (200 OK):**
```json
{
  "isSuccess": true,
  "data": {
    "items": [
      {
        "id": "guid",
        "classId": "guid",
        "classCode": "ENG2-2024",
        "classTitle": "Tiếng Anh Lớp 2 - 2024",
        "examType": "Midterm",
        "date": "2024-01-15",
        "maxScore": 100,
        "description": "Bài thi giữa kỳ",
        "scheduledStartTime": "2024-01-15T14:00:00Z",
        "timeLimitMinutes": 90,
        "allowLateStart": true,
        "lateStartToleranceMinutes": 15,
        "autoSubmitOnTimeLimit": true,
        "preventCopyPaste": true,
        "preventNavigation": true,
        "showResultsImmediately": false,
        "createdBy": "guid",
        "createdByName": "Teacher Nguyễn Văn A",
        "createdAt": "2024-01-10T10:00:00Z",
        "resultCount": 0
      }
    ],
    "pageNumber": 1,
    "pageSize": 10,
    "totalCount": 1,
    "totalPages": 1
  }
}
```

---

### 8.3. Get Exam By ID

**Endpoint:** `GET /api/exams/{id}`

**Mô tả:** UC-154: Xem chi tiết Exam.

**Authorization:** Required (Bearer Token)

**Path Parameters:**
- `id` (Guid): ID của Exam

**Response (200 OK):** Tương tự như Get Exams, nhưng chỉ trả về 1 exam

**Lỗi có thể xảy ra:**
- `404 Not Found`: Exam không tồn tại

---

### 8.4. Update Exam

**Endpoint:** `PUT /api/exams/{id}`

**Mô tả:** UC-155: Cập nhật Exam.

**Authorization:** Required (Roles: Teacher, Staff, Admin)

**Path Parameters:**
- `id` (Guid): ID của Exam

**Request Body:** Tương tự như Create Exam, nhưng tất cả fields đều optional

**Response (200 OK):** Tương tự như Get Exam By ID

**Lỗi có thể xảy ra:**
- `404 Not Found`: Exam không tồn tại

---

### 8.5. Delete Exam

**Endpoint:** `DELETE /api/exams/{id}`

**Mô tả:** UC-156: Xóa Exam.

**Authorization:** Required (Roles: Teacher, Staff, Admin)

**Path Parameters:**
- `id` (Guid): ID của Exam

**Response (200 OK):**
```json
{
  "isSuccess": true,
  "data": null
}
```

**Lỗi có thể xảy ra:**
- `404 Not Found`: Exam không tồn tại

---

### 8.6. Create Exam Question

**Endpoint:** `POST /api/exams/{examId}/questions`

**Mô tả:** UC-163: Tạo Exam Question.

**Authorization:** Required (Roles: Teacher, Staff, Admin)

**Path Parameters:**
- `examId` (Guid): ID của Exam

**Request Body:**
```json
{
  "questionText": "What is the capital of France?",
  "questionType": "MultipleChoice",
  "options": "[\"Paris\", \"London\", \"Berlin\", \"Madrid\"]",
  "correctAnswer": "Paris",
  "points": 10,
  "orderIndex": 1
}
```

**QuestionType Values:**
- `"MultipleChoice"`: Câu hỏi trắc nghiệm
- `"TextInput"`: Câu hỏi tự luận

**Response (201 Created):**
```json
{
  "isSuccess": true,
  "data": {
    "id": "guid",
    "examId": "guid",
    "questionText": "What is the capital of France?",
    "questionType": "MultipleChoice",
    "options": "[\"Paris\", \"London\", \"Berlin\", \"Madrid\"]",
    "correctAnswer": "Paris",
    "points": 10,
    "orderIndex": 1,
    "createdAt": "2024-01-10T10:00:00Z",
    "updatedAt": "2024-01-10T10:00:00Z"
  }
}
```

---

### 8.7. Get Exam Questions

**Endpoint:** `GET /api/exams/{examId}/questions`

**Mô tả:** UC-164: Xem danh sách Exam Questions của Exam.

**Authorization:** Required (Bearer Token)

**Path Parameters:**
- `examId` (Guid): ID của Exam

**Response (200 OK):**
```json
{
  "isSuccess": true,
  "data": {
    "items": [
      {
        "id": "guid",
        "examId": "guid",
        "questionText": "What is the capital of France?",
        "questionType": "MultipleChoice",
        "options": "[\"Paris\", \"London\", \"Berlin\", \"Madrid\"]",
        "correctAnswer": "Paris",
        "points": 10,
        "orderIndex": 1,
        "createdAt": "2024-01-10T10:00:00Z",
        "updatedAt": "2024-01-10T10:00:00Z"
      }
    ],
    "pageNumber": 1,
    "pageSize": 10,
    "totalCount": 1,
    "totalPages": 1
  }
}
```

---

### 8.8. Get Exam Question By ID

**Endpoint:** `GET /api/exams/{examId}/questions/{questionId}`

**Mô tả:** UC-165: Xem chi tiết Exam Question.

**Authorization:** Required (Bearer Token)

**Path Parameters:**
- `examId` (Guid): ID của Exam
- `questionId` (Guid): ID của Question

**Response (200 OK):** Tương tự như Get Exam Questions, nhưng chỉ trả về 1 question

---

### 8.9. Update Exam Question

**Endpoint:** `PUT /api/exams/{examId}/questions/{questionId}`

**Mô tả:** UC-166: Cập nhật Exam Question.

**Authorization:** Required (Roles: Teacher, Staff, Admin)

**Path Parameters:**
- `examId` (Guid): ID của Exam
- `questionId` (Guid): ID của Question

**Request Body:** Tương tự như Create Exam Question, nhưng tất cả fields đều optional

**Response (200 OK):** Tương tự như Get Exam Question By ID

---

### 8.10. Delete Exam Question

**Endpoint:** `DELETE /api/exams/{examId}/questions/{questionId}`

**Mô tả:** UC-167: Xóa Exam Question.

**Authorization:** Required (Roles: Teacher, Staff, Admin)

**Path Parameters:**
- `examId` (Guid): ID của Exam
- `questionId` (Guid): ID của Question

**Response (200 OK):**
```json
{
  "isSuccess": true,
  "data": null
}
```

---

### 8.11. Start Exam Submission

**Endpoint:** `POST /api/exams/{examId}/submissions/start`

**Mô tả:** UC-169: Học sinh bắt đầu làm bài thi (tạo ExamSubmission, check ScheduledStartTime).

**Authorization:** Required (Bearer Token)

**Path Parameters:**
- `examId` (Guid): ID của Exam

**Request Body:**
```json
{
  "studentProfileId": "guid"
}
```

**Response (201 Created):**
```json
{
  "isSuccess": true,
  "data": {
    "id": "guid",
    "examId": "guid",
    "studentProfileId": "guid",
    "actualStartTime": "2024-01-15T14:00:00Z",
    "status": "InProgress",
    "scheduledStartTime": "2024-01-15T14:00:00Z",
    "timeLimitMinutes": 90
  }
}
```

**Lỗi có thể xảy ra:**
- `400 Bad Request`: Exam chưa bắt đầu hoặc đã quá thời gian cho phép
- `404 Not Found`: Exam hoặc Student Profile không tồn tại
- `409 Conflict`: Đã có submission cho exam này

---

### 8.12. Save Exam Submission Answer

**Endpoint:** `POST /api/exams/{examId}/submissions/{submissionId}/answers`

**Mô tả:** UC-170: Học sinh lưu câu trả lời (ExamSubmissionAnswer).

**Authorization:** Required (Bearer Token)

**Path Parameters:**
- `examId` (Guid): ID của Exam
- `submissionId` (Guid): ID của Submission

**Request Body:**
```json
{
  "questionId": "guid",
  "answer": "Paris"
}
```

**Response (200 OK):**
```json
{
  "isSuccess": true,
  "data": {
    "id": "guid",
    "submissionId": "guid",
    "questionId": "guid",
    "answer": "Paris",
    "answeredAt": "2024-01-15T14:05:00Z"
  }
}
```

**Lưu ý:** Với Multiple Choice, hệ thống sẽ tự động chấm và set `isCorrect` và `pointsAwarded`.

---

### 8.13. Submit Exam Submission

**Endpoint:** `POST /api/exams/{examId}/submissions/{submissionId}/submit`

**Mô tả:** UC-171: Học sinh nộp bài thi. UC-172: Tự động nộp bài khi hết giờ (AutoSubmitOnTimeLimit).

**Authorization:** Required (Bearer Token)

**Path Parameters:**
- `examId` (Guid): ID của Exam
- `submissionId` (Guid): ID của Submission

**Request Body:**
```json
{
  "isAutoSubmit": false
}
```

**Response (200 OK):**
```json
{
  "isSuccess": true,
  "data": {
    "id": "guid",
    "examId": "guid",
    "studentProfileId": "guid",
    "submittedAt": "2024-01-15T15:30:00Z",
    "autoSubmittedAt": null,
    "status": "Submitted",
    "autoScore": 80
  }
}
```

**Lưu ý:** 
- `autoScore` được tính tự động cho Multiple Choice questions
- Nếu `isAutoSubmit = true`, status sẽ là `AutoSubmitted`

---

### 8.14. Get Exam Submissions

**Endpoint:** `GET /api/exams/{examId}/submissions`

**Mô tả:** Xem danh sách Exam Submissions.

**Authorization:** Required (Roles: Teacher, Staff, Admin)

**Path Parameters:**
- `examId` (Guid): ID của Exam

**Query Parameters:**
- `studentProfileId` (Guid?, optional): Lọc theo Student Profile ID
- `status` (ExamSubmissionStatus?, optional): Lọc theo status
- `pageNumber` (int, default: 1): Số trang
- `pageSize` (int, default: 10): Số lượng items mỗi trang

**Response (200 OK):**
```json
{
  "isSuccess": true,
  "data": {
    "items": [
      {
        "id": "guid",
        "examId": "guid",
        "studentProfileId": "guid",
        "studentName": "Nguyễn Văn A",
        "actualStartTime": "2024-01-15T14:00:00Z",
        "submittedAt": "2024-01-15T15:30:00Z",
        "timeSpentMinutes": 90,
        "autoScore": 80,
        "finalScore": null,
        "status": "Submitted"
      }
    ],
    "pageNumber": 1,
    "pageSize": 10,
    "totalCount": 1,
    "totalPages": 1
  }
}
```

---

### 8.15. Get Exam Submission

**Endpoint:** `GET /api/exams/{examId}/submissions/{submissionId}`

**Mô tả:** UC-174: Xem bài làm của học sinh (ExamSubmission với answers).

**Authorization:** Required (Bearer Token)

**Path Parameters:**
- `examId` (Guid): ID của Exam
- `submissionId` (Guid): ID của Submission

**Query Parameters:**
- `includeAnswers` (bool, default: false): Bao gồm answers
- `showCorrectAnswers` (bool, default: false): Hiển thị đáp án đúng (chỉ Teacher/Staff/Admin)

**Response (200 OK):**
```json
{
  "isSuccess": true,
  "data": {
    "id": "guid",
    "examId": "guid",
    "studentProfileId": "guid",
    "studentName": "Nguyễn Văn A",
    "actualStartTime": "2024-01-15T14:00:00Z",
    "submittedAt": "2024-01-15T15:30:00Z",
    "timeSpentMinutes": 90,
    "autoScore": 80,
    "finalScore": null,
    "gradedBy": null,
    "gradedByName": null,
    "gradedAt": null,
    "teacherComment": null,
    "status": "Submitted",
    "answers": [
      {
        "id": "guid",
        "questionId": "guid",
        "questionOrderIndex": 1,
        "questionText": "What is the capital of France?",
        "questionType": "MultipleChoice",
        "questionOptions": "[\"Paris\", \"London\", \"Berlin\", \"Madrid\"]",
        "questionCorrectAnswer": "Paris",
        "questionPoints": 10,
        "answer": "Paris",
        "isCorrect": true,
        "pointsAwarded": 10,
        "teacherFeedback": null,
        "answeredAt": "2024-01-15T14:05:00Z"
      }
    ]
  }
}
```

---

### 8.16. Grade Exam Submission

**Endpoint:** `POST /api/exams/{examId}/submissions/{submissionId}/grade`

**Mô tả:** UC-175: Teacher chấm bài thi (text input, cập nhật FinalScore).

**Authorization:** Required (Roles: Teacher, Staff, Admin)

**Path Parameters:**
- `examId` (Guid): ID của Exam
- `submissionId` (Guid): ID của Submission

**Request Body:**
```json
{
  "finalScore": 85,
  "teacherComment": "Làm bài tốt, cần cải thiện phần writing",
  "answerGrades": [
    {
      "questionId": "guid",
      "pointsAwarded": 8,
      "teacherFeedback": "Good answer, but could be more detailed"
    }
  ]
}
```

**Response (200 OK):**
```json
{
  "isSuccess": true,
  "data": {
    "id": "guid",
    "examId": "guid",
    "studentProfileId": "guid",
    "finalScore": 85,
    "gradedBy": "guid",
    "gradedByName": "Teacher Nguyễn Văn A",
    "gradedAt": "2024-01-16T10:00:00Z",
    "teacherComment": "Làm bài tốt, cần cải thiện phần writing"
  }
}
```

---

### 8.17. Create Exam Result

**Endpoint:** `POST /api/exams/{examId}/results`

**Mô tả:** UC-157: Nhập Exam Result cho 1 học sinh (single: studentId, score, comment, attachments?).

**Authorization:** Required (Roles: Teacher, Staff, Admin)

**Path Parameters:**
- `examId` (Guid): ID của Exam

**Request Body:**
```json
{
  "studentProfileId": "guid",
  "score": 85,
  "comment": "Làm bài tốt",
  "attachmentUrls": [
    "https://res.cloudinary.com/.../exam-result-1.jpg",
    "https://res.cloudinary.com/.../exam-result-2.jpg"
  ]
}
```

**Response (201 Created):**
```json
{
  "isSuccess": true,
  "data": {
    "id": "guid",
    "examId": "guid",
    "studentProfileId": "guid",
    "studentName": "Nguyễn Văn A",
    "score": 85,
    "comment": "Làm bài tốt",
    "attachmentUrls": [
      "https://res.cloudinary.com/.../exam-result-1.jpg",
      "https://res.cloudinary.com/.../exam-result-2.jpg"
    ],
    "gradedBy": "guid",
    "gradedByName": "Teacher Nguyễn Văn A",
    "createdAt": "2024-01-16T10:00:00Z",
    "updatedAt": "2024-01-16T10:00:00Z"
  }
}
```

---

### 8.18. Create Exam Results Bulk

**Endpoint:** `POST /api/exams/{examId}/results/bulk`

**Mô tả:** UC-157a: Nhập Exam Results cho nhiều học sinh cùng lúc (bulk).

**Authorization:** Required (Roles: Teacher, Staff, Admin)

**Path Parameters:**
- `examId` (Guid): ID của Exam

**Request Body:**
```json
{
  "results": [
    {
      "studentProfileId": "guid",
      "score": 85,
      "comment": "Làm bài tốt",
      "attachmentUrls": ["https://res.cloudinary.com/.../exam-result-1.jpg"]
    },
    {
      "studentProfileId": "guid",
      "score": 90,
      "comment": "Xuất sắc",
      "attachmentUrls": []
    }
  ]
}
```

**Response (201 Created):**
```json
{
  "isSuccess": true,
  "data": {
    "createdCount": 2,
    "results": [
      {
        "id": "guid",
        "studentProfileId": "guid",
        "score": 85
      },
      {
        "id": "guid",
        "studentProfileId": "guid",
        "score": 90
      }
    ]
  }
}
```

---

### 8.19. Get Exam Results

**Endpoint:** `GET /api/exams/{examId}/results`

**Mô tả:** UC-158: Xem danh sách Exam Results.

**Authorization:** Required (Bearer Token)

**Path Parameters:**
- `examId` (Guid): ID của Exam

**Query Parameters:**
- `studentProfileId` (Guid?, optional): Lọc theo Student Profile ID
- `pageNumber` (int, default: 1): Số trang
- `pageSize` (int, default: 10): Số lượng items mỗi trang

**Response (200 OK):** Tương tự như Create Exam Result, nhưng trả về danh sách

---

### 8.20. Get Exam Result By ID

**Endpoint:** `GET /api/exams/{examId}/results/{resultId}`

**Mô tả:** UC-159: Xem chi tiết Exam Result.

**Authorization:** Required (Bearer Token)

**Path Parameters:**
- `examId` (Guid): ID của Exam
- `resultId` (Guid): ID của Result

**Response (200 OK):** Tương tự như Create Exam Result

---

### 8.21. Update Exam Result

**Endpoint:** `PUT /api/exams/{examId}/results/{resultId}`

**Mô tả:** UC-160: Cập nhật Exam Result.

**Authorization:** Required (Roles: Teacher, Staff, Admin)

**Path Parameters:**
- `examId` (Guid): ID của Exam
- `resultId` (Guid): ID của Result

**Request Body:** Tương tự như Create Exam Result, nhưng tất cả fields đều optional

**Response (200 OK):** Tương tự như Get Exam Result By ID

---

### 8.22. Get Student Exam Results

**Endpoint:** `GET /api/students/{studentProfileId}/exam-results`

**Mô tả:** UC-162: Parent/Student xem lịch sử Exam Results của học sinh (filter theo type).

**Authorization:** Required (Bearer Token)

**Path Parameters:**
- `studentProfileId` (Guid): ID của Student Profile

**Query Parameters:**
- `examType` (ExamType?, optional): Lọc theo exam type
- `pageNumber` (int, default: 1): Số trang
- `pageSize` (int, default: 10): Số lượng items mỗi trang

**Response (200 OK):**
```json
{
  "isSuccess": true,
  "data": {
    "items": [
      {
        "id": "guid",
        "examId": "guid",
        "examType": "Midterm",
        "examDate": "2024-01-15",
        "classId": "guid",
        "classCode": "ENG2-2024",
        "classTitle": "Tiếng Anh Lớp 2 - 2024",
        "score": 85,
        "comment": "Làm bài tốt",
        "attachmentUrls": ["https://res.cloudinary.com/.../exam-result-1.jpg"],
        "gradedBy": "guid",
        "gradedByName": "Teacher Nguyễn Văn A",
        "createdAt": "2024-01-16T10:00:00Z"
      }
    ],
    "pageNumber": 1,
    "pageSize": 10,
    "totalCount": 1,
    "totalPages": 1
  }
}
```

---

## 9. Ticket APIs

Base URL: `/api/tickets`

### 9.1. Create Ticket

**Endpoint:** `POST /api/tickets`

**Mô tả:** Parent/Student tạo Ticket. Ticket mới tạo sẽ có `Status = Open` mặc định.

**Authorization:** Required (Bearer Token)

**Request Body:**
```json
{
  "openedByProfileId": "guid",
  "branchId": "guid",
  "classId": "guid",
  "category": "Homework",
  "subject": "Vấn đề về bài tập về nhà",
  "message": "Tôi cần hỗ trợ về bài tập về nhà"
}
```

**Category Values:**
- `"Homework"`: Vấn đề về bài tập
- `"Finance"`: Vấn đề về tài chính
- `"Schedule"`: Vấn đề về lịch học
- `"Tech"`: Vấn đề kỹ thuật

**Response (201 Created):**
```json
{
  "isSuccess": true,
  "data": {
    "id": "guid",
    "openedByUserId": "guid",
    "openedByUserName": "Nguyễn Văn A",
    "openedByProfileId": "guid",
    "openedByProfileName": "Phụ huynh Nguyễn Văn A",
    "branchId": "guid",
    "branchName": "Chi nhánh Quận 1",
    "classId": "guid",
    "classCode": "ENG2-2024",
    "classTitle": "Tiếng Anh Lớp 2 - 2024",
    "category": "Homework",
    "message": "Tôi cần hỗ trợ về bài tập về nhà",
    "status": "Open",
    "assignedToUserId": null,
    "createdAt": "2024-01-15T10:00:00Z",
    "updatedAt": "2024-01-15T10:00:00Z"
  }
}
```

**Headers:**
- `Location: /api/tickets/{id}`

**Lỗi có thể xảy ra:**
- `400 Bad Request`: Dữ liệu không hợp lệ
- `404 Not Found`: User, Branch, Class, hoặc Profile không tồn tại

---

### 9.2. Get Tickets

**Endpoint:** `GET /api/tickets`

**Mô tả:** Xem danh sách Tickets với phân trang và filter.

**Authorization:** Required (Bearer Token)

**Query Parameters:**
- `branchId` (Guid?, optional): Lọc theo Branch ID
- `openedByUserId` (Guid?, optional): Lọc theo người tạo
- `assignedToUserId` (Guid?, optional): Lọc theo người được gán
- `status` (string?, optional): Lọc theo status (Open, InProgress, Resolved, Closed)
- `category` (string?, optional): Lọc theo category (Homework, Finance, Schedule, Tech)
- `classId` (Guid?, optional): Lọc theo Class ID
- `mine` (bool?, optional): Lọc tickets của user hiện tại (opened by hoặc assigned to)
- `pageNumber` (int, default: 1): Số trang
- `pageSize` (int, default: 10): Số lượng items mỗi trang

**Example Request:**
```
GET /api/tickets?status=Open&category=Homework&pageNumber=1&pageSize=10
```

**Response (200 OK):**
```json
{
  "isSuccess": true,
  "data": {
    "items": [
      {
        "id": "guid",
        "openedByUserId": "guid",
        "openedByUserName": "Nguyễn Văn A",
        "openedByProfileId": "guid",
        "openedByProfileName": "Phụ huynh Nguyễn Văn A",
        "branchId": "guid",
        "branchName": "Chi nhánh Quận 1",
        "classId": "guid",
        "classCode": "ENG2-2024",
        "classTitle": "Tiếng Anh Lớp 2 - 2024",
        "category": "Homework",
        "subject": "Vấn đề về bài tập về nhà",
        "message": "Tôi cần hỗ trợ về bài tập về nhà",
        "status": "Open",
        "assignedToUserId": null,
        "assignedToUserName": null,
        "createdAt": "2024-01-15T10:00:00Z",
        "updatedAt": "2024-01-15T10:00:00Z",
        "commentCount": 0
      }
    ],
    "pageNumber": 1,
    "pageSize": 10,
    "totalCount": 1,
    "totalPages": 1
  }
}
```

---

### 9.3. Get Ticket By ID

**Endpoint:** `GET /api/tickets/{id}`

**Mô tả:** Xem chi tiết Ticket bao gồm tất cả comments.

**Authorization:** Required (Bearer Token)

**Path Parameters:**
- `id` (Guid): ID của Ticket

**Response (200 OK):**
```json
{
  "isSuccess": true,
  "data": {
    "id": "guid",
    "openedByUserId": "guid",
    "openedByUserName": "Nguyễn Văn A",
    "openedByProfileId": "guid",
    "openedByProfileName": "Phụ huynh Nguyễn Văn A",
    "branchId": "guid",
    "branchName": "Chi nhánh Quận 1",
    "classId": "guid",
    "classCode": "ENG2-2024",
    "classTitle": "Tiếng Anh Lớp 2 - 2024",
    "category": "Homework",
    "subject": "Vấn đề về bài tập về nhà",
    "message": "Tôi cần hỗ trợ về bài tập về nhà",
    "status": "InProgress",
    "assignedToUserId": "guid",
    "assignedToUserName": "Staff Nguyễn Thị B",
    "createdAt": "2024-01-15T10:00:00Z",
    "updatedAt": "2024-01-15T11:00:00Z",
    "comments": [
      {
        "id": "guid",
        "commenterUserId": "guid",
        "commenterUserName": "Staff Nguyễn Thị B",
        "commenterProfileId": null,
        "commenterProfileName": null,
        "message": "Chúng tôi đã kiểm tra và sẽ hỗ trợ bạn ngay",
        "attachmentUrl": null,
        "createdAt": "2024-01-15T11:00:00Z"
      }
    ]
  }
}
```

**Lỗi có thể xảy ra:**
- `404 Not Found`: Ticket không tồn tại

---

### 9.4. Assign Ticket

**Endpoint:** `PATCH /api/tickets/{id}/assign`

**Mô tả:** Gán Ticket cho Staff/Teacher. Tự động chuyển status từ Open → InProgress.

**Authorization:** Required (Roles: Admin, Staff)

**Path Parameters:**
- `id` (Guid): ID của Ticket

**Request Body:**
```json
{
  "assignedToUserId": "guid"
}
```

**Response (200 OK):** Tương tự như Get Ticket By ID

**Lỗi có thể xảy ra:**
- `404 Not Found`: Ticket hoặc User không tồn tại
- `409 Conflict`: 
  - User không phải Staff/Teacher
  - User không thuộc cùng branch với Ticket

---

### 9.5. Update Ticket Status

**Endpoint:** `PATCH /api/tickets/{id}/status`

**Mô tả:** Cập nhật trạng thái Ticket.

**Authorization:** Required (Roles: Admin, Staff, Teacher)

**Path Parameters:**
- `id` (Guid): ID của Ticket

**Request Body:**
```json
{
  "status": "Resolved"
}
```

**Status Values:**
- `"Open"`: Ticket mới tạo
- `"InProgress"`: Đang xử lý
- `"Resolved"`: Đã giải quyết
- `"Closed"`: Đã đóng

**Response (200 OK):** Tương tự như Get Ticket By ID

**Lỗi có thể xảy ra:**
- `404 Not Found`: Ticket không tồn tại

---

### 9.6. Add Ticket Comment

**Endpoint:** `POST /api/tickets/{id}/comments`

**Mô tả:** Thêm comment vào Ticket. Nếu Staff/Teacher comment vào ticket Open, tự động chuyển status → InProgress.

**Authorization:** Required (Bearer Token)

**Path Parameters:**
- `id` (Guid): ID của Ticket

**Request Body:**
```json
{
  "commenterUserId": "guid",
  "commenterProfileId": "guid",
  "message": "Chúng tôi đã kiểm tra và sẽ hỗ trợ bạn ngay",
  "attachmentUrl": "https://res.cloudinary.com/..."
}
```

**Response (201 Created):**
```json
{
  "isSuccess": true,
  "data": {
    "id": "guid",
    "ticketId": "guid",
    "commenterUserId": "guid",
    "commenterUserName": "Staff Nguyễn Thị B",
    "commenterProfileId": null,
    "commenterProfileName": null,
    "message": "Chúng tôi đã kiểm tra và sẽ hỗ trợ bạn ngay",
    "attachmentUrl": null,
    "createdAt": "2024-01-15T11:00:00Z"
  }
}
```

**Headers:**
- `Location: /api/tickets/{id}/comments/{commentId}`

**Lỗi có thể xảy ra:**
- `400 Bad Request`: Dữ liệu không hợp lệ
- `404 Not Found`: Ticket, User, hoặc Profile không tồn tại
- `409 Conflict`: Ticket đã bị Closed

---

### 9.7. Get Ticket History

**Endpoint:** `GET /api/tickets/{id}/history`

**Mô tả:** Xem lịch sử Ticket (timeline: Created, StatusChanged, Assigned, Comments).

**Authorization:** Required (Bearer Token)

**Path Parameters:**
- `id` (Guid): ID của Ticket

**Response (200 OK):**
```json
{
  "isSuccess": true,
  "data": {
    "ticketId": "guid",
    "ticketStatus": "InProgress",
    "history": [
      {
        "type": "Created",
        "commentId": null,
        "commenterUserId": "guid",
        "commenterUserName": "Nguyễn Văn A",
        "commenterProfileId": "guid",
        "commenterProfileName": "Phụ huynh Nguyễn Văn A",
        "message": "Ticket created: Tôi cần hỗ trợ về bài tập về nhà",
        "attachmentUrl": null,
        "timestamp": "2024-01-15T10:00:00Z"
      },
      {
        "type": "Assigned",
        "commentId": null,
        "commenterUserId": "guid",
        "commenterUserName": "Staff Nguyễn Thị B",
        "commenterProfileId": null,
        "commenterProfileName": null,
        "message": "Ticket assigned to Staff Nguyễn Thị B",
        "attachmentUrl": null,
        "timestamp": "2024-01-15T10:30:00Z"
      },
      {
        "type": "Comment",
        "commentId": "guid",
        "commenterUserId": "guid",
        "commenterUserName": "Staff Nguyễn Thị B",
        "commenterProfileId": null,
        "commenterProfileName": null,
        "message": "Chúng tôi đã kiểm tra và sẽ hỗ trợ bạn ngay",
        "attachmentUrl": null,
        "timestamp": "2024-01-15T11:00:00Z"
      }
    ]
  }
}
```

---

### 9.8. Get Ticket SLA

**Endpoint:** `GET /api/tickets/{id}/sla`

**Mô tả:** Theo dõi SLA phản hồi Ticket (first response time, SLA compliance).

**Authorization:** Required (Roles: Admin, Staff)

**Path Parameters:**
- `id` (Guid): ID của Ticket

**Response (200 OK):**
```json
{
  "isSuccess": true,
  "data": {
    "ticketId": "guid",
    "ticketStatus": "InProgress",
    "createdAt": "2024-01-15T10:00:00Z",
    "firstResponseAt": "2024-01-15T11:00:00Z",
    "timeToFirstResponseHours": 1.0,
    "slaTargetHours": 24,
    "isSLACompliant": true,
    "isSLAOverdue": false,
    "totalComments": 3,
    "staffCommentCount": 2
  }
}
```

**Lỗi có thể xảy ra:**
- `404 Not Found`: Ticket không tồn tại

---

## 10. Notification APIs

Base URL: `/api/notifications`

### 10.1. Get Notifications

**Endpoint:** `GET /api/notifications`

**Mô tả:** UC-325-339: Xem danh sách Notifications.

**Authorization:** Required (Bearer Token)

**Query Parameters:**
- `profileId` (Guid?, optional): Lọc theo Profile ID
- `unreadOnly` (bool?, optional): Chỉ lấy notifications chưa đọc
- `pageNumber` (int, default: 1): Số trang
- `pageSize` (int, default: 10): Số lượng items mỗi trang

**Example Request:**
```
GET /api/notifications?profileId=guid&unreadOnly=true&pageNumber=1&pageSize=10
```

**Response (200 OK):**
```json
{
  "isSuccess": true,
  "data": {
    "items": [
      {
        "id": "guid",
        "title": "Thông báo mới",
        "content": "Bạn có bài tập mới cần hoàn thành",
        "deeplink": "/homework/123",
        "channel": "InApp",
        "profileId": "guid",
        "profileName": "Nguyễn Văn A",
        "readAt": null,
        "createdAt": "2024-01-15T10:00:00Z"
      }
    ],
    "pageNumber": 1,
    "pageSize": 10,
    "totalCount": 1,
    "totalPages": 1
  }
}
```

**Channel Values:**
- `"InApp"`: Thông báo trong app
- `"Email"`: Thông báo qua email
- `"ZaloOa"`: Thông báo qua Zalo OA
- `"Push"`: Push notification

---

### 10.2. Broadcast Notification

**Endpoint:** `POST /api/notifications/broadcast`

**Mô tả:** UC-325-339: Admin/ManagementStaff broadcast notification đến nhiều người dùng.

**Authorization:** Required (Roles: Admin, ManagementStaff)

**Request Body:**
```json
{
  "title": "Thông báo mới",
  "content": "Bạn có bài tập mới cần hoàn thành",
  "deeplink": "/homework/123",
  "channel": "InApp",
  "role": "Student",
  "branchId": "guid",
  "classId": "guid",
  "studentProfileId": "guid",
  "userIds": ["guid1", "guid2"],
  "profileIds": ["guid1", "guid2"]
}
```

**Request Parameters:**
| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| `title` | string | Yes | Tiêu đề notification |
| `content` | string | No | Nội dung notification |
| `deeplink` | string | No | Deep link để mở app |
| `channel` | NotificationChannel | No | Kênh gửi (mặc định: InApp) |
| `role` | string | No* | Role người nhận (xem bảng bên dưới) |
| `branchId` | Guid | No* | Lọc theo branch |
| `classId` | Guid | No* | Lọc theo class |
| `studentProfileId` | Guid | No* | Gửi đến một student cụ thể |
| `userIds` | List\<Guid\> | No* | Gửi đến các user IDs cụ thể |
| `profileIds` | List\<Guid\> | No* | Gửi đến các profile IDs cụ thể |

**Chỉ chọn MỘT trong các filter:** `role`, `branchId`, `classId`, `studentProfileId`, `userIds`, `profileIds`. Thứ tự ưu tiên từ trên xuống dưới.

**Giá trị Role được hỗ trợ:**
| Role Value | Description | Nguồn dữ liệu |
|------------|-------------|----------------|
| `Admin` | Admin users | User.Role |
| `ManagementStaff` | Management staff | User.Role |
| `AccountantStaff` | Accountant staff | User.Role |
| `Teacher` | Teachers | User.Role |
| `Parent` | Parents | User.Role + Profile |
| `Student` | Students | Profile.ProfileType |

**Gửi đến nhiều role:**
Có thể kết hợp nhiều role bằng dấu `,` hoặc `+`:
- `"Admin,ManagementStaff"` → gửi cho Admin và ManagementStaff
- `"Parent+Student"` → gửi cho Parents và Students
- `"Admin,ManagementStaff,AccountantStaff,Teacher,Parent,Student"` → gửi cho tất cả

**Response (201 Created):**
```json
{
  "isSuccess": true,
  "data": {
    "createdCount": 10,
    "createdNotificationIds": ["guid1", "guid2", ...]
  }
}
```

**Headers:**
- `Location: /api/notifications/broadcast/{createdCount}`

**Lỗi có thể xảy ra:**
- `400 Bad Request`: InvalidFilters - Không có filter nào được chọn
- `400 Bad Request`: NoRecipients - Không tìm thấy người nhận nào phù hợp với filter
- `404 Not Found`: Branch, Class, hoặc Profile không tồn tại

---

### 10.3. Broadcast Notification History

**Endpoint:** `GET /api/notifications/broadcast-history`

**MÃ´ táº£:** Admin/ManagementStaff xem lá»‹ch sá»­ broadcast notification (group theo láº§n broadcast).

**Authorization:** Required (Roles: Admin, ManagementStaff)

**Query Parameters:**
- `channel` (NotificationChannel?, optional): Lá»c theo kÃªnh gá»­i
- `senderRole` (string?, optional): Lá»c theo role ngÆ°á»i gá»­i (Admin/ManagementStaff)
- `fromDate` (DateTime?, optional): Lá»c tá»« ngÃ y
- `toDate` (DateTime?, optional): Lá»c Ä‘áº¿n ngÃ y
- `pageNumber` (int, default: 1): Sá»‘ trang
- `pageSize` (int, default: 10): Sá»‘ lÆ°á»£ng items má»—i trang

**Response (200 OK):**
```json
{
  "isSuccess": true,
  "data": {
    "broadcasts": {
      "items": [
        {
          "createdAt": "2024-01-15T10:00:00Z",
          "channel": "InApp",
          "title": "ThÃ´ng bÃ¡o má»›i",
          "content": "Báº¡n cÃ³ bÃ i táº­p má»›i cáº§n hoÃ n thÃ nh",
          "deeplink": "/homework/123",
          "senderRole": "ManagementStaff",
          "senderName": "Nguyá»…n VÄƒn A",
          "targetRole": "Parent+Student",
          "recipientCount": 120,
          "pendingCount": 120,
          "sentCount": 0,
          "failedCount": 0
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

---

### 10.4. Mark Notification As Read

**Endpoint:** `PATCH /api/notifications/{id}/read`

**Mô tả:** Đánh dấu notification là đã đọc.

**Authorization:** Required (Bearer Token)

**Path Parameters:**
- `id` (Guid): ID của Notification

**Response (200 OK):**
```json
{
  "isSuccess": true,
  "data": {
    "id": "guid",
    "readAt": "2024-01-15T10:05:00Z"
  }
}
```

**Lỗi có thể xảy ra:**
- `404 Not Found`: Notification không tồn tại

---

## 11. Blog APIs

Base URL: `/api/blogs`

### 11.1. Create Blog

**Endpoint:** `POST /api/blogs`

**Mô tả:** Admin/Staff tạo Blog Post. Blog mới tạo sẽ có `IsPublished = false` mặc định. `CreatedBy` tự động lấy từ JWT token.

**Authorization:** Required (Roles: Admin, Staff)

**Request Body:**
```json
{
  "title": "10 Cách Học Tiếng Anh Hiệu Quả",
  "summary": "Tổng hợp các phương pháp học tiếng Anh hiệu quả cho trẻ em",
  "content": "<p>Nội dung blog post...</p>",
  "featuredImageUrl": "https://res.cloudinary.com/.../blog/featured-image.jpg"
}
```

**Response (201 Created):**
```json
{
  "isSuccess": true,
  "data": {
    "id": "guid",
    "title": "10 Cách Học Tiếng Anh Hiệu Quả",
    "summary": "Tổng hợp các phương pháp học tiếng Anh hiệu quả cho trẻ em",
    "content": "<p>Nội dung blog post...</p>",
    "featuredImageUrl": "https://res.cloudinary.com/.../blog/featured-image.jpg",
    "createdBy": "guid",
    "createdByName": "Admin Nguyễn Văn A",
    "isPublished": false,
    "isDeleted": false,
    "publishedAt": null,
    "createdAt": "2024-01-15T10:00:00Z",
    "updatedAt": "2024-01-15T10:00:00Z"
  }
}
```

**Headers:**
- `Location: /api/blogs/{id}`

**Lỗi có thể xảy ra:**
- `400 Bad Request`: Dữ liệu không hợp lệ
- `404 Not Found`: User không tồn tại hoặc không phải Admin/Staff

---

### 11.2. Get Blogs

**Endpoint:** `GET /api/blogs`

**Mô tả:** Xem danh sách Blog Posts với phân trang và filter.

**Authorization:** Required (Roles: Admin, Staff)

**Query Parameters:**
- `createdBy` (Guid?, optional): Lọc theo creator user ID
- `isPublished` (bool?, optional): Lọc theo published status
- `includeDeleted` (bool?, optional): Include deleted blogs (default: false)
- `pageNumber` (int, default: 1): Số trang
- `pageSize` (int, default: 10): Số lượng items mỗi trang

**Example Request:**
```
GET /api/blogs?isPublished=true&pageNumber=1&pageSize=10
```

**Response (200 OK):**
```json
{
  "isSuccess": true,
  "data": {
    "items": [
      {
        "id": "guid",
        "title": "10 Cách Học Tiếng Anh Hiệu Quả",
        "summary": "Tổng hợp các phương pháp học tiếng Anh hiệu quả cho trẻ em",
        "content": "<p>Nội dung blog post...</p>",
        "featuredImageUrl": "https://res.cloudinary.com/.../blog/featured-image.jpg",
        "createdBy": "guid",
        "createdByName": "Admin Nguyễn Văn A",
        "isPublished": true,
        "isDeleted": false,
        "publishedAt": "2024-01-16T10:00:00Z",
        "createdAt": "2024-01-15T10:00:00Z",
        "updatedAt": "2024-01-16T10:00:00Z"
      }
    ],
    "pageNumber": 1,
    "pageSize": 10,
    "totalCount": 1,
    "totalPages": 1
  }
}
```

---

### 11.3. Get Blog By ID

**Endpoint:** `GET /api/blogs/{id}`

**Mô tả:** Xem chi tiết Blog Post.

**Authorization:** Required (Bearer Token)

**Path Parameters:**
- `id` (Guid): ID của Blog

**Response (200 OK):**
```json
{
  "isSuccess": true,
  "data": {
    "id": "guid",
    "title": "10 Cách Học Tiếng Anh Hiệu Quả",
    "summary": "Tổng hợp các phương pháp học tiếng Anh hiệu quả cho trẻ em",
    "content": "<p>Nội dung blog post...</p>",
    "featuredImageUrl": "https://res.cloudinary.com/.../blog/featured-image.jpg",
    "createdBy": "guid",
    "createdByName": "Admin Nguyễn Văn A",
    "isPublished": true,
    "isDeleted": false,
    "publishedAt": "2024-01-16T10:00:00Z",
    "createdAt": "2024-01-15T10:00:00Z",
    "updatedAt": "2024-01-16T10:00:00Z"
  }
}
```

**Lỗi có thể xảy ra:**
- `404 Not Found`: Blog không tồn tại

---

### 11.4. Update Blog

**Endpoint:** `PUT /api/blogs/{id}`

**Mô tả:** Cập nhật Blog Post.

**Authorization:** Required (Roles: Admin, Staff)

**Path Parameters:**
- `id` (Guid): ID của Blog

**Request Body:**
```json
{
  "title": "10 Cách Học Tiếng Anh Hiệu Quả - Updated",
  "summary": "Tổng hợp các phương pháp học tiếng Anh hiệu quả cho trẻ em - Updated",
  "content": "<p>Nội dung blog post đã được cập nhật...</p>",
  "featuredImageUrl": "https://res.cloudinary.com/.../blog/featured-image-updated.jpg"
}
```

**Response (200 OK):** Tương tự như Get Blog By ID

**Lỗi có thể xảy ra:**
- `404 Not Found`: Blog không tồn tại
- `409 Conflict`: Blog đã bị xóa

---

### 11.5. Delete Blog

**Endpoint:** `DELETE /api/blogs/{id}`

**Mô tả:** Xóa mềm Blog Post (set IsDeleted = true).

**Authorization:** Required (Roles: Admin, Staff)

**Path Parameters:**
- `id` (Guid): ID của Blog

**Response (200 OK):**
```json
{
  "isSuccess": true,
  "data": {
    "id": "guid",
    "title": "10 Cách Học Tiếng Anh Hiệu Quả",
    "isDeleted": true,
    "updatedAt": "2024-01-20T10:00:00Z"
  }
}
```

**Lỗi có thể xảy ra:**
- `404 Not Found`: Blog không tồn tại
- `409 Conflict`: Blog đã bị xóa

---

### 11.6. Publish Blog

**Endpoint:** `PATCH /api/blogs/{id}/publish`

**Mô tả:** Publish Blog Post (set IsPublished = true, PublishedAt = DateTime.UtcNow).

**Authorization:** Required (Roles: Admin, Staff)

**Path Parameters:**
- `id` (Guid): ID của Blog

**Response (200 OK):**
```json
{
  "isSuccess": true,
  "data": {
    "id": "guid",
    "title": "10 Cách Học Tiếng Anh Hiệu Quả",
    "isPublished": true,
    "publishedAt": "2024-01-16T10:00:00Z",
    "updatedAt": "2024-01-16T10:00:00Z"
  }
}
```

**Lỗi có thể xảy ra:**
- `404 Not Found`: Blog không tồn tại
- `409 Conflict`: 
  - Blog đã bị xóa
  - Blog đã được publish

---

### 11.7. Unpublish Blog

**Endpoint:** `PATCH /api/blogs/{id}/unpublish`

**Mô tả:** Unpublish Blog Post (set IsPublished = false).

**Authorization:** Required (Roles: Admin, Staff)

**Path Parameters:**
- `id` (Guid): ID của Blog

**Response (200 OK):**
```json
{
  "isSuccess": true,
  "data": {
    "id": "guid",
    "title": "10 Cách Học Tiếng Anh Hiệu Quả",
    "isPublished": false,
    "updatedAt": "2024-01-20T10:00:00Z"
  }
}
```

**Lỗi có thể xảy ra:**
- `404 Not Found`: Blog không tồn tại
- `409 Conflict`: 
  - Blog đã bị xóa
  - Blog chưa được publish

---

### 11.8. Get Published Blogs

**Endpoint:** `GET /api/blogs/published`

**Mô tả:** Hiển thị Blog Post trên Landing Page. Public endpoint - không cần authentication.

**Authorization:** Không cần (Public)

**Query Parameters:**
- `pageNumber` (int, default: 1): Số trang
- `pageSize` (int, default: 10): Số lượng items mỗi trang

**Response (200 OK):**
```json
{
  "isSuccess": true,
  "data": {
    "items": [
      {
        "id": "guid",
        "title": "10 Cách Học Tiếng Anh Hiệu Quả",
        "summary": "Tổng hợp các phương pháp học tiếng Anh hiệu quả cho trẻ em",
        "featuredImageUrl": "https://res.cloudinary.com/.../blog/featured-image.jpg",
        "createdBy": "guid",
        "createdByName": "Admin Nguyễn Văn A",
        "publishedAt": "2024-01-16T10:00:00Z",
        "createdAt": "2024-01-15T10:00:00Z"
      }
    ],
    "pageNumber": 1,
    "pageSize": 10,
    "totalCount": 1,
    "totalPages": 1
  }
}
```

**Lưu ý:** Response không bao gồm `content` để tiết kiệm bandwidth. Sử dụng `GET /api/blogs/{id}` để lấy full content.

---

## 12. File Upload APIs

Base URL: `/api/files`

### 12.1. Upload File

**Endpoint:** `POST /api/files/upload`

**Mô tả:** Upload file (ảnh hoặc video) lên Cloudinary và trả về public URL. Hỗ trợ cho:
- Ticket attachments
- Blog featured images
- Media assets (Use Case 13)

**Authorization:** Required (Bearer Token)

**Query Parameters:**
- `folder` (string, optional): Folder name trong Cloudinary (default: "uploads")
  - Ví dụ: `tickets`, `media`, `blog`
- `resourceType` (string, optional): `image`, `video`, hoặc `auto` (default: `auto`)
  - `auto`: Tự động detect từ file extension

**Request Body:**
- `file` (FormData): File cần upload (multipart/form-data)

**File Size Limit:** 100MB

**Allowed File Types:**
- Images: `.jpg`, `.jpeg`, `.png`, `.gif`, `.webp`, `.bmp`, `.svg`
- Videos: `.mp4`, `.mov`, `.avi`, `.wmv`, `.flv`, `.webm`

**Example Request (cURL):**
```bash
curl -X 'POST' \
  'https://localhost:7235/api/files/upload?folder=blog&resourceType=auto' \
  -H 'accept: */*' \
  -H 'Authorization: Bearer {token}' \
  -H 'Content-Type: multipart/form-data' \
  -F 'file=@image.jpg;type=image/jpeg'
```

**Response (200 OK):**
```json
{
  "url": "https://res.cloudinary.com/dbvjdx05r/image/upload/v1767628773/blog/388e480a-505a-44d6-a54a-0b9b99a03727.jpg",
  "fileName": "image.jpg",
  "size": 15203,
  "folder": "blog"
}
```

**Lỗi có thể xảy ra:**
- `400 Bad Request`: 
  - File không được cung cấp
  - File size vượt quá 100MB
  - File type không được phép
- `401 Unauthorized`: Chưa đăng nhập
- `500 Internal Server Error`: Lỗi upload lên Cloudinary

---

### 12.2. Delete File

**Endpoint:** `DELETE /api/files?url={publicUrl}`

**Mô tả:** Xóa file từ Cloudinary.

**Authorization:** Required (Roles: Admin, Staff)

**Query Parameters:**
- `url` (string, required): Public URL của file cần xóa

**Example Request:**
```
DELETE /api/files?url=https://res.cloudinary.com/dbvjdx05r/image/upload/v1767628773/blog/388e480a-505a-44d6-a54a-0b9b99a03727.jpg
```

**Response (200 OK):**
```json
{
  "message": "File deleted successfully"
}
```

**Lỗi có thể xảy ra:**
- `400 Bad Request`: URL không được cung cấp
- `404 Not Found`: File không tồn tại hoặc không thể xóa

---

### 12.3. Get Transformed URL

**Endpoint:** `GET /api/files/transform?url={publicUrl}&width=800&height=600&format=webp`

**Mô tả:** Tạo URL đã được transform (resize, format conversion) mà không cần upload lại.

**Authorization:** Không cần (Public)

**Query Parameters:**
- `url` (string, required): Original public URL
- `width` (int?, optional): Desired width
- `height` (int?, optional): Desired height
- `format` (string?, optional): Desired format (`webp`, `jpg`, `png`)

**Example Request:**
```
GET /api/files/transform?url=https://res.cloudinary.com/.../image.jpg&width=800&height=600&format=webp
```

**Response (200 OK):**
```json
{
  "url": "https://res.cloudinary.com/.../c_limit,h_600,w_800,f_webp,q_auto/image.jpg"
}
```

---

## Lưu ý chung

### Authentication

Hầu hết các API yêu cầu authentication. Để sử dụng, bạn cần:

1. Đăng nhập qua `POST /api/auth/login` để lấy `accessToken`
2. Gửi `accessToken` trong header của mỗi request:
   ```
   Authorization: Bearer {accessToken}
   ```

### Phân quyền

- **Admin**: Có quyền truy cập tất cả các API
- **Staff**: Có quyền truy cập hầu hết các API, trừ một số API xóa (chỉ Admin)
- **Public**: Một số API không cần authentication (được đánh dấu `[AllowAnonymous]`)

### Response Format

Tất cả các API đều trả về response theo format:

```json
{
  "isSuccess": true,
  "data": { ... }
}
```

Khi có lỗi:

```json
{
  "isSuccess": false,
  "error": {
    "code": "Error.Code",
    "message": "Error message"
  }
}
```

### Pagination

Các API list đều hỗ trợ phân trang:

- `pageNumber`: Số trang (bắt đầu từ 1)
- `pageSize`: Số lượng items mỗi trang (mặc định: 10)

Response sẽ bao gồm:
- `items`: Danh sách items
- `pageNumber`: Số trang hiện tại
- `pageSize`: Số lượng items mỗi trang
- `totalCount`: Tổng số items
- `totalPages`: Tổng số trang

### Date Format

- Date fields sử dụng format: `YYYY-MM-DD` (ví dụ: `2024-01-15`)
- DateTime fields sử dụng format ISO 8601: `YYYY-MM-DDTHH:mm:ssZ` (ví dụ: `2024-01-01T00:00:00Z`)

### Validation Rules

1. **Teacher và Branch**: Teacher phải thuộc cùng branch với Class khi tạo/cập nhật/assign teacher
2. **Program và Branch**: Program phải thuộc branch được chỉ định
3. **Class Code**: Phải unique trong toàn hệ thống
4. **Status Transitions**: 
   - Class: Planned → Active → Closed (không thể quay lại)
   - Program/Tuition Plan/Classroom: Toggle IsActive

### Error Codes

- `400 Bad Request`: Dữ liệu không hợp lệ
- `401 Unauthorized`: Chưa đăng nhập hoặc token không hợp lệ
- `403 Forbidden`: Không có quyền truy cập
- `404 Not Found`: Resource không tồn tại
- `409 Conflict`: Xung đột dữ liệu (ví dụ: code đã tồn tại, có ràng buộc)

---

## Swagger UI

Bạn có thể truy cập Swagger UI để xem và test các API tại:
```
https://localhost:{port}/swagger
```

Trong Swagger UI, bạn có thể:
- Xem tất cả các endpoints
- Xem request/response schemas
- Test API trực tiếp
- Xem authorization requirements

---

---

## 13. Master Data APIs

Base URL: `/api/`

### 13.1. Get Branches

**Endpoint:** `GET /api/branches`

**Mô tả:** Lấy danh sách Branches. Admin thấy tất cả branches, Staff/Teacher chỉ thấy branch được gán.

**Authorization:** Required (Bearer Token)

**Response (200 OK):**
```json
{
  "isSuccess": true,
  "data": {
    "branches": [
      {
        "id": "11111111-1111-1111-1111-111111111111",
        "code": "HN001",
        "name": "Chi nhánh Hà Nội",
        "address": "123 Đường ABC, Quận 1",
        "contactPhone": "0123456789",
        "contactEmail": "hanoi@kidzgo.com",
        "isActive": true
      }
    ]
  }
}
```

**Lưu ý:**
- Admin: Trả về tất cả branches đang active
- Staff/Teacher: Chỉ trả về branch được gán (BranchId) nếu có, nếu không có thì trả về mảng rỗng

---

### 13.2. Get Levels

**Endpoint:** `GET /api/lookups/levels`

**Mô tả:** Lấy danh sách các level từ Programs (distinct, active programs).

**Authorization:** Required (Bearer Token)

**Response (200 OK):**
```json
{
  "isSuccess": true,
  "data": {
    "levels": [
      "Beginner",
      "Intermediate",
      "Advanced"
    ]
  }
}
```

---

### 13.3. Get Roles

**Endpoint:** `GET /api/roles`

**Mô tả:** Lấy danh sách SessionRoleType enum values.

**Authorization:** Required (Bearer Token)

**Response (200 OK):**
```json
{
  "isSuccess": true,
  "data": {
    "roles": [
      {
        "value": "MainTeacher",
        "displayName": "Main Teacher"
      },
      {
        "value": "AssistantTeacher",
        "displayName": "Assistant Teacher"
      }
    ]
  }
}
```

---

### 13.4. Get Lookups

**Endpoint:** `GET /api/lookups`

**Mô tả:** Lấy tất cả enum values cho các lookups (attendanceStatus, sessionStatus, classStatus, enrollmentStatus, etc.).

**Authorization:** Required (Bearer Token)

**Response (200 OK):**
```json
{
  "isSuccess": true,
  "data": {
    "lookups": {
      "attendanceStatus": [
        {
          "value": "Present",
          "displayName": "Present"
        },
        {
          "value": "Absent",
          "displayName": "Absent"
        },
        {
          "value": "Makeup",
          "displayName": "Makeup"
        }
      ],
      "sessionStatus": [
        {
          "value": "Planned",
          "displayName": "Planned"
        },
        {
          "value": "Completed",
          "displayName": "Completed"
        },
        {
          "value": "Cancelled",
          "displayName": "Cancelled"
        }
      ],
      "classStatus": [
        {
          "value": "Planned",
          "displayName": "Planned"
        },
        {
          "value": "Active",
          "displayName": "Active"
        },
        {
          "value": "Closed",
          "displayName": "Closed"
        }
      ],
      "enrollmentStatus": [
        {
          "value": "Active",
          "displayName": "Active"
        },
        {
          "value": "Paused",
          "displayName": "Paused"
        },
        {
          "value": "Dropped",
          "displayName": "Dropped"
        }
      ]
    }
  }
}
```

---

## 14. Teacher APIs

Base URL: `/api/teacher`

### 14.1. Get Teacher Classes

**Endpoint:** `GET /api/teacher/classes`

**Mô tả:** Lấy danh sách lớp của Teacher (lớp mà teacher là Main Teacher hoặc Assistant Teacher).

**Authorization:** Required (Role: Teacher)

**Query Parameters:**
- `pageNumber` (int, default: 1): Số trang
- `pageSize` (int, default: 10): Số lượng items mỗi trang

**Response (200 OK):**
```json
{
  "isSuccess": true,
  "data": {
    "classes": {
      "items": [
        {
          "id": "guid",
          "branchId": "11111111-1111-1111-1111-111111111111",
          "branchName": "Chi nhánh Hà Nội",
          "programId": "12345678-1234-1234-1234-123456789012",
          "programName": "Tiếng Anh Trẻ Em",
          "code": "ENG2-2024",
          "title": "Tiếng Anh Lớp 2 - 2024",
          "mainTeacherId": "aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa",
          "mainTeacherName": "Nguyễn Văn A",
          "assistantTeacherId": "bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb",
          "assistantTeacherName": "Trần Thị B",
          "startDate": "2024-01-15",
          "endDate": "2024-04-15",
          "status": "Active",
          "capacity": 15,
          "currentEnrollmentCount": 10,
          "schedulePattern": "RRULE:FREQ=WEEKLY;BYDAY=MO,WE,FR;BYHOUR=18;BYMINUTE=0",
          "role": "MainTeacher"
        }
      ],
      "pageNumber": 1,
      "pageSize": 10,
      "totalCount": 1,
      "totalPages": 1,
      "hasPreviousPage": false,
      "hasNextPage": false
    }
  }
}
```

**Lưu ý:** Field `role` sẽ là `"MainTeacher"` hoặc `"AssistantTeacher"` tùy theo vai trò của teacher trong lớp.

---

### 14.2. Get Teacher Timetable

**Endpoint:** `GET /api/teacher/timetable`

**Mô tả:** Lấy thời khóa biểu của Teacher trong khoảng thời gian từ `from` đến `to`.

**Authorization:** Required (Role: Teacher)

**Query Parameters:**
- `from` (DateTime?, optional): Ngày bắt đầu (format: `YYYY-MM-DD` hoặc `YYYY-MM-DDTHH:mm:ssZ`)
- `to` (DateTime?, optional): Ngày kết thúc (format: `YYYY-MM-DD` hoặc `YYYY-MM-DDTHH:mm:ssZ`)

**Example Request:**
```
GET /api/teacher/timetable?from=2025-01-01&to=2025-01-31
```

**Response (200 OK):**
```json
{
  "isSuccess": true,
  "data": {
    "sessions": [
      {
        "id": "guid",
        "classId": "guid",
        "classCode": "ENG2-2024",
        "classTitle": "Tiếng Anh Lớp 2 - 2024",
        "plannedDatetime": "2025-01-15T18:00:00Z",
        "actualDatetime": null,
        "durationMinutes": 90,
        "participationType": "InPerson",
        "status": "Planned",
        "plannedRoomId": "guid",
        "plannedRoomName": "Phòng học A1",
        "actualRoomId": null,
        "actualRoomName": null,
        "plannedTeacherId": "aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa",
        "plannedTeacherName": "Nguyễn Văn A",
        "actualTeacherId": null,
        "actualTeacherName": null,
        "plannedAssistantId": "bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb",
        "plannedAssistantName": "Trần Thị B",
        "lessonPlanId": null,
        "lessonPlanLink": null
      }
    ]
  }
}
```

**Lưu ý:**
- Nếu không có `from` và `to`, sẽ trả về tất cả sessions
- Chỉ trả về sessions mà teacher là PlannedTeacher hoặc ActualTeacher
- Không bao gồm sessions có status = Cancelled
- `lessonPlanLink` sẽ là URL dạng `/api/lesson-plans/{lessonPlanId}` nếu có lesson plan

---

## 15. Student APIs

Base URL: `/api/students`

### 15.1. Get Student Classes

**Endpoint:** `GET /api/students/{studentId}/classes`

**Mô tả:** Lấy danh sách lớp mà Student đang tham gia (enrolled với status = Active).

**Authorization:** Required (Bearer Token)

**Path Parameters:**
- `studentId` (Guid): ID của Student Profile

**Query Parameters:**
- `pageNumber` (int, default: 1): Số trang
- `pageSize` (int, default: 10): Số lượng items mỗi trang

**Response (200 OK):**
```json
{
  "isSuccess": true,
  "data": {
    "classes": {
      "items": [
        {
          "id": "guid",
          "branchId": "11111111-1111-1111-1111-111111111111",
          "branchName": "Chi nhánh Hà Nội",
          "programId": "12345678-1234-1234-1234-123456789012",
          "programName": "Tiếng Anh Trẻ Em",
          "code": "ENG2-2024",
          "title": "Tiếng Anh Lớp 2 - 2024",
          "mainTeacherId": "aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa",
          "mainTeacherName": "Nguyễn Văn A",
          "assistantTeacherId": "bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb",
          "assistantTeacherName": "Trần Thị B",
          "startDate": "2024-01-15",
          "endDate": "2024-04-15",
          "status": "Active",
          "capacity": 15,
          "currentEnrollmentCount": 10,
          "schedulePattern": "RRULE:FREQ=WEEKLY;BYDAY=MO,WE,FR;BYHOUR=18;BYMINUTE=0",
          "enrollDate": "2024-01-15",
          "enrollmentStatus": "Active"
        }
      ],
      "pageNumber": 1,
      "pageSize": 10,
      "totalCount": 1,
      "totalPages": 1,
      "hasPreviousPage": false,
      "hasNextPage": false
    }
  }
}
```

**Lỗi có thể xảy ra:**
- `404 Not Found`: Student profile không tồn tại, không phải student, hoặc đã inactive/deleted

---

### 15.2. Get Student Timetable

**Endpoint:** `GET /api/students/{studentId}/timetable`

**Mô tả:** Lấy thời khóa biểu của Student trong khoảng thời gian từ `from` đến `to`.

**Authorization:** Required (Bearer Token)

**Path Parameters:**
- `studentId` (Guid): ID của Student Profile

**Query Parameters:**
- `from` (DateTime?, optional): Ngày bắt đầu (format: `YYYY-MM-DD` hoặc `YYYY-MM-DDTHH:mm:ssZ`)
- `to` (DateTime?, optional): Ngày kết thúc (format: `YYYY-MM-DD` hoặc `YYYY-MM-DDTHH:mm:ssZ`)

**Example Request:**
```
GET /api/students/11111111-1111-1111-1111-111111111100/timetable?from=2025-01-01&to=2025-01-31
```

**Response (200 OK):**
```json
{
  "isSuccess": true,
  "data": {
    "sessions": [
      {
        "id": "guid",
        "classId": "guid",
        "classCode": "ENG2-2024",
        "classTitle": "Tiếng Anh Lớp 2 - 2024",
        "plannedDatetime": "2025-01-15T18:00:00Z",
        "actualDatetime": null,
        "durationMinutes": 90,
        "participationType": "InPerson",
        "status": "Planned",
        "plannedRoomId": "guid",
        "plannedRoomName": "Phòng học A1",
        "actualRoomId": null,
        "actualRoomName": null,
        "plannedTeacherId": "aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa",
        "plannedTeacherName": "Nguyễn Văn A",
        "actualTeacherId": null,
        "actualTeacherName": null,
        "plannedAssistantId": "bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb",
        "plannedAssistantName": "Trần Thị B",
        "lessonPlanId": null,
        "lessonPlanLink": null
      }
    ]
  }
}
```

**Lưu ý:**
- Chỉ trả về sessions từ các lớp mà student đang enrolled (status = Active)
- Không bao gồm sessions có status = Cancelled
- Nếu không có `from` và `to`, sẽ trả về tất cả sessions
- `lessonPlanLink` sẽ là URL dạng `/api/lesson-plans/{lessonPlanId}` nếu có lesson plan

**Lỗi có thể xảy ra:**
- `404 Not Found`: Student profile không tồn tại, không phải student, hoặc đã inactive/deleted

---

## 16. Session APIs

Base URL: `/api/sessions`

### 16.1. Get Session By ID

**Endpoint:** `GET /api/sessions/{sessionId}`

**Mô tả:** UC-078: Xem chi tiết Session bao gồm attendance summary.

**Authorization:** Required (Bearer Token)

**Path Parameters:**
- `sessionId` (Guid): ID của Session

**Response (200 OK):**
```json
{
  "isSuccess": true,
  "data": {
    "session": {
      "id": "guid",
      "classId": "guid",
      "classCode": "ENG2-2024",
      "classTitle": "Tiếng Anh Lớp 2 - 2024",
      "branchId": "11111111-1111-1111-1111-111111111111",
      "branchName": "Chi nhánh Hà Nội",
      "plannedDatetime": "2025-01-15T18:00:00Z",
      "actualDatetime": "2025-01-15T18:05:00Z",
      "durationMinutes": 90,
      "participationType": "InPerson",
      "status": "Completed",
      "plannedRoomId": "guid",
      "plannedRoomName": "Phòng học A1",
      "actualRoomId": "guid",
      "actualRoomName": "Phòng học A1",
      "plannedTeacherId": "aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa",
      "plannedTeacherName": "Nguyễn Văn A",
      "actualTeacherId": "aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa",
      "actualTeacherName": "Nguyễn Văn A",
      "plannedAssistantId": "bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb",
      "plannedAssistantName": "Trần Thị B",
      "actualAssistantId": "bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb",
      "actualAssistantName": "Trần Thị B",
      "lessonPlanId": "guid",
      "lessonPlanLink": "/api/lesson-plans/guid",
      "attendanceSummary": {
        "totalStudents": 10,
        "presentCount": 8,
        "absentCount": 1,
        "makeupCount": 0,
        "notMarkedCount": 1
      }
    }
  }
}
```

**Lỗi có thể xảy ra:**
- `404 Not Found`: Session không tồn tại

**Lưu ý:**
- `attendanceSummary.totalStudents`: Tổng số học sinh trong lớp (từ ClassEnrollments với status = Active)
- `attendanceSummary.presentCount`: Số học sinh có mặt
- `attendanceSummary.absentCount`: Số học sinh vắng
- `attendanceSummary.makeupCount`: Số học sinh makeup
- `attendanceSummary.notMarkedCount`: Số học sinh chưa được đánh dấu điểm danh
- `lessonPlanLink` sẽ là URL dạng `/api/lesson-plans/{lessonPlanId}` nếu có lesson plan

---

## 17. Media APIs

Base URL: `/api/media`

### 17.1. Create Media

**Endpoint:** `POST /api/media`

**Mô tả:** UC-238: Teacher/Staff upload ảnh/video. UC-239-242: Gắn tag (Class, Student, Month, Type, Visibility). Media mới tạo sẽ có `ApprovalStatus = Pending` và `IsPublished = false` mặc định.

**Authorization:** Required (Roles: Teacher, Staff, Admin)

**Request Body:**
```json
{
  "branchId": "11111111-1111-1111-1111-111111111111",
  "classId": "guid",
  "studentProfileId": "guid",
  "monthTag": "2025-01",
  "type": "Photo",
  "contentType": "Album",
  "url": "https://res.cloudinary.com/.../media/image.jpg",
  "caption": "Ảnh lớp học",
  "visibility": "ClassOnly"
}
```

**Type Values:**
- `"Photo"`: Ảnh
- `"Video"`: Video

**ContentType Values:**
- `"Homework"`: Bài tập về nhà
- `"Report"`: Báo cáo
- `"Test"`: Bài kiểm tra
- `"Album"`: Album ảnh
- `"ClassPhoto"`: Ảnh lớp học

**Visibility Values:**
- `"ClassOnly"`: Chỉ lớp học
- `"Personal"`: Cá nhân
- `"PublicParent"`: Công khai cho phụ huynh

**Response (201 Created):**
```json
{
  "isSuccess": true,
  "data": {
    "id": "guid",
    "uploaderId": "guid",
    "uploaderName": "Teacher Nguyễn Văn A",
    "branchId": "11111111-1111-1111-1111-111111111111",
    "branchName": "Chi nhánh Hà Nội",
    "classId": "guid",
    "className": "Tiếng Anh Lớp 2 - 2024",
    "studentProfileId": "guid",
    "studentName": "Nguyễn Văn B",
    "monthTag": "2025-01",
    "type": "Photo",
    "contentType": "Album",
    "url": "https://res.cloudinary.com/.../media/image.jpg",
    "caption": "Ảnh lớp học",
    "visibility": "ClassOnly",
    "approvalStatus": "Pending",
    "isPublished": false,
    "createdAt": "2025-01-15T10:00:00Z"
  }
}
```

**Headers:**
- `Location: /api/media/{id}`

**Lỗi có thể xảy ra:**
- `400 Bad Request`: Dữ liệu không hợp lệ
- `404 Not Found`: Branch, Class, hoặc Student Profile không tồn tại

---

### 17.2. Get Media

**Endpoint:** `GET /api/media`

**Mô tả:** UC-243: Xem danh sách Media. UC-249: Parent/Student xem album lớp. UC-250: Parent/Student xem album cá nhân. UC-251: Filter Media theo tháng.

**Authorization:** Required (Bearer Token)

**Query Parameters:**
- `branchId` (Guid?, optional): Lọc theo Branch ID
- `classId` (Guid?, optional): Lọc theo Class ID
- `studentProfileId` (Guid?, optional): Lọc theo Student Profile ID
- `monthTag` (string?, optional): Lọc theo tháng (format: YYYY-MM)
- `type` (MediaType?, optional): Lọc theo type (Photo, Video)
- `contentType` (MediaContentType?, optional): Lọc theo contentType (Homework, Report, Test, Album, ClassPhoto)
- `visibility` (Visibility?, optional): Lọc theo visibility (ClassOnly, Personal, PublicParent)
- `approvalStatus` (ApprovalStatus?, optional): Lọc theo approval status (Pending, Approved, Rejected)
- `isPublished` (bool?, optional): Lọc theo published status
- `pageNumber` (int, default: 1): Số trang
- `pageSize` (int, default: 20): Số lượng items mỗi trang

**Example Request:**
```
GET /api/media?classId=guid&isPublished=true&pageNumber=1&pageSize=20
```

**Response (200 OK):**
```json
{
  "isSuccess": true,
  "data": {
    "items": [
      {
        "id": "guid",
        "uploaderId": "guid",
        "uploaderName": "Teacher Nguyễn Văn A",
        "branchId": "11111111-1111-1111-1111-111111111111",
        "branchName": "Chi nhánh Hà Nội",
        "classId": "guid",
        "className": "Tiếng Anh Lớp 2 - 2024",
        "studentProfileId": "guid",
        "studentName": "Nguyễn Văn B",
        "monthTag": "2025-01",
        "type": "Photo",
        "contentType": "Album",
        "url": "https://res.cloudinary.com/.../media/image.jpg",
        "caption": "Ảnh lớp học",
        "visibility": "ClassOnly",
        "approvalStatus": "Approved",
        "approvedById": "guid",
        "approvedByName": "Staff Nguyễn Thị C",
        "approvedAt": "2025-01-15T11:00:00Z",
        "isPublished": true,
        "createdAt": "2025-01-15T10:00:00Z",
        "updatedAt": "2025-01-15T11:00:00Z"
      }
    ],
    "pageNumber": 1,
    "pageSize": 20,
    "totalCount": 1,
    "totalPages": 1
  }
}
```

---

### 17.3. Get Media By ID

**Endpoint:** `GET /api/media/{id}`

**Mô tả:** UC-244: Xem chi tiết Media. UC-252: Download Media (FE sẽ dùng Url từ response).

**Authorization:** Required (Bearer Token)

**Path Parameters:**
- `id` (Guid): ID của Media

**Response (200 OK):**
```json
{
  "isSuccess": true,
  "data": {
    "id": "guid",
    "uploaderId": "guid",
    "uploaderName": "Teacher Nguyễn Văn A",
    "branchId": "11111111-1111-1111-1111-111111111111",
    "branchName": "Chi nhánh Hà Nội",
    "classId": "guid",
    "className": "Tiếng Anh Lớp 2 - 2024",
    "studentProfileId": "guid",
    "studentName": "Nguyễn Văn B",
    "monthTag": "2025-01",
    "type": "Photo",
    "contentType": "Album",
    "url": "https://res.cloudinary.com/.../media/image.jpg",
    "caption": "Ảnh lớp học",
    "visibility": "ClassOnly",
    "approvalStatus": "Approved",
    "approvedById": "guid",
    "approvedByName": "Staff Nguyễn Thị C",
    "approvedAt": "2025-01-15T11:00:00Z",
    "isPublished": true,
    "createdAt": "2025-01-15T10:00:00Z",
    "updatedAt": "2025-01-15T11:00:00Z"
  }
}
```

**Lỗi có thể xảy ra:**
- `404 Not Found`: Media không tồn tại hoặc đã bị xóa

---

### 17.4. Update Media

**Endpoint:** `PUT /api/media/{id}`

**Mô tả:** UC-245: Cập nhật Media (tags, caption, visibility).

**Authorization:** Required (Roles: Teacher, Staff, Admin)

**Path Parameters:**
- `id` (Guid): ID của Media

**Request Body:**
```json
{
  "classId": "guid",
  "studentProfileId": "guid",
  "monthTag": "2025-02",
  "contentType": "ClassPhoto",
  "caption": "Ảnh lớp học - Updated",
  "visibility": "PublicParent"
}
```

**Lưu ý:** Tất cả các fields đều optional. Chỉ cần gửi các fields muốn cập nhật.

**Response (200 OK):**
```json
{
  "isSuccess": true,
  "data": {
    "id": "guid",
    "classId": "guid",
    "studentProfileId": "guid",
    "monthTag": "2025-02",
    "contentType": "ClassPhoto",
    "caption": "Ảnh lớp học - Updated",
    "visibility": "PublicParent",
    "updatedAt": "2025-01-16T10:00:00Z"
  }
}
```

**Lỗi có thể xảy ra:**
- `404 Not Found`: Media không tồn tại hoặc đã bị xóa

---

### 17.5. Delete Media

**Endpoint:** `DELETE /api/media/{id}`

**Mô tả:** UC-246: Xóa mềm Media (set IsDeleted = true).

**Authorization:** Required (Roles: Teacher, Staff, Admin)

**Path Parameters:**
- `id` (Guid): ID của Media

**Response (200 OK):**
```json
{
  "isSuccess": true,
  "data": null
}
```

**Lỗi có thể xảy ra:**
- `404 Not Found`: Media không tồn tại

---

### 17.6. Approve Media

**Endpoint:** `POST /api/media/{id}/approve`

**Mô tả:** UC-247: Staff/Admin approve Media (chuyển ApprovalStatus từ Pending → Approved).

**Authorization:** Required (Roles: Staff, Admin)

**Path Parameters:**
- `id` (Guid): ID của Media

**Response (200 OK):**
```json
{
  "isSuccess": true,
  "data": {
    "id": "guid",
    "approvalStatus": "Approved",
    "approvedById": "guid",
    "approvedByName": "Staff Nguyễn Thị C",
    "approvedAt": "2025-01-15T11:00:00Z"
  }
}
```

**Lỗi có thể xảy ra:**
- `404 Not Found`: Media không tồn tại hoặc đã bị xóa
- `409 Conflict`: Media đã được approve

---

### 17.7. Reject Media

**Endpoint:** `POST /api/media/{id}/reject`

**Mô tả:** UC-247a: Staff/Admin reject Media (chuyển ApprovalStatus từ Pending → Rejected).

**Authorization:** Required (Roles: Staff, Admin)

**Path Parameters:**
- `id` (Guid): ID của Media

**Response (200 OK):**
```json
{
  "isSuccess": true,
  "data": {
    "id": "guid",
    "approvalStatus": "Rejected",
    "updatedAt": "2025-01-15T11:00:00Z"
  }
}
```

**Lỗi có thể xảy ra:**
- `404 Not Found`: Media không tồn tại hoặc đã bị xóa
- `409 Conflict`: Media đã được reject

---

### 17.8. Publish Media

**Endpoint:** `POST /api/media/{id}/publish`

**Mô tả:** UC-248: Publish Media lên gallery (set IsPublished = true). Media phải được approve trước khi publish.

**Authorization:** Required (Roles: Staff, Admin)

**Path Parameters:**
- `id` (Guid): ID của Media

**Response (200 OK):**
```json
{
  "isSuccess": true,
  "data": {
    "id": "guid",
    "isPublished": true,
    "updatedAt": "2025-01-15T12:00:00Z"
  }
}
```

**Lỗi có thể xảy ra:**
- `404 Not Found`: Media không tồn tại hoặc đã bị xóa
- `409 Conflict`: 
  - Media đã được publish
  - Media chưa được approve (phải approve trước khi publish)

---

## 18. User Management APIs

Base URL: `/api/admin/users`

### 18.1. Get Users

**Endpoint:** `GET /api/admin/users`

**Mô tả:** UC-371: Xem danh sách Users với phân trang và filter.

**Authorization:** Required (Role: Admin)

**Query Parameters:**
- `isActive` (bool?, optional): Lọc theo trạng thái active
- `role` (string?, optional): Lọc theo role (Admin, Staff, Teacher)
- `branchId` (Guid?, optional): Lọc theo Branch ID
- `pageNumber` (int, default: 1): Số trang
- `pageSize` (int, default: 10): Số lượng items mỗi trang

**Example Request:**
```
GET /api/admin/users?role=Teacher&isActive=true&pageNumber=1&pageSize=10
```

**Response (200 OK):**
```json
{
  "isSuccess": true,
  "data": {
    "items": [
      {
        "id": "guid",
        "userName": "teacher@kidzgo.com",
        "fullName": "Teacher Nguyễn Văn A",
        "email": "teacher@kidzgo.com",
        "role": "Teacher",
        "branchId": "guid",
        "branchName": "Chi nhánh Hà Nội",
        "isActive": true,
        "createdAt": "2024-01-01T00:00:00Z",
        "updatedAt": "2024-01-01T00:00:00Z"
      }
    ],
    "pageNumber": 1,
    "pageSize": 10,
    "totalCount": 1,
    "totalPages": 1
  }
}
```

---

### 18.2. Get User By ID

**Endpoint:** `GET /api/admin/users/{id}`

**Mô tả:** UC-372: Xem chi tiết User.

**Authorization:** Required (Role: Admin)

**Path Parameters:**
- `id` (Guid): ID của User

**Response (200 OK):**
```json
{
  "isSuccess": true,
  "data": {
    "id": "guid",
    "userName": "teacher@kidzgo.com",
    "fullName": "Teacher Nguyễn Văn A",
    "email": "teacher@kidzgo.com",
    "role": "Teacher",
    "branchId": "guid",
    "branchName": "Chi nhánh Hà Nội",
    "isActive": true,
    "createdAt": "2024-01-01T00:00:00Z",
    "updatedAt": "2024-01-01T00:00:00Z"
  }
}
```

**Lỗi có thể xảy ra:**
- `404 Not Found`: User không tồn tại

---

### 18.3. Create User

**Endpoint:** `POST /api/admin/users`

**Mô tả:** UC-370: Tạo User mới.

**Authorization:** Required (Role: Admin)

**Request Body:**
```json
{
  "name": "Teacher Nguyễn Văn A",
  "email": "teacher@kidzgo.com",
  "password": "password123",
  "role": "Teacher"
}
```

**Role Values:**
- `"Admin"`: Quản trị viên
- `"Staff"`: Nhân viên
- `"Teacher"`: Giáo viên

**Response (201 Created):**
```json
{
  "isSuccess": true,
  "data": {
    "id": "guid",
    "userName": "teacher@kidzgo.com",
    "fullName": "Teacher Nguyễn Văn A",
    "email": "teacher@kidzgo.com",
    "role": "Teacher",
    "isActive": true,
    "createdAt": "2024-01-01T00:00:00Z",
    "updatedAt": "2024-01-01T00:00:00Z"
  }
}
```

**Headers:**
- `Location: /api/admin/users/{id}`

**Lỗi có thể xảy ra:**
- `400 Bad Request`: Dữ liệu không hợp lệ
- `409 Conflict`: Email đã tồn tại

---

### 18.4. Update User

**Endpoint:** `PUT /api/admin/users/{id}`

**Mô tả:** UC-373: Cập nhật thông tin User.

**Authorization:** Required (Role: Admin)

**Path Parameters:**
- `id` (Guid): ID của User

**Request Body:**
```json
{
  "fullName": "Teacher Nguyễn Văn A - Updated",
  "email": "teacher.updated@kidzgo.com",
  "role": "Teacher",
  "isDeleted": false
}
```

**Response (200 OK):**
```json
{
  "isSuccess": true,
  "data": {
    "id": "guid",
    "userName": "teacher.updated@kidzgo.com",
    "fullName": "Teacher Nguyễn Văn A - Updated",
    "email": "teacher.updated@kidzgo.com",
    "role": "Teacher",
    "isActive": true,
    "updatedAt": "2024-01-02T00:00:00Z"
  }
}
```

**Lỗi có thể xảy ra:**
- `404 Not Found`: User không tồn tại
- `409 Conflict`: Email đã tồn tại (nếu đổi email)

---

### 18.5. Delete User

**Endpoint:** `DELETE /api/admin/users/{id}`

**Mô tả:** UC-374: Xóa mềm User (set IsDeleted = true).

**Authorization:** Required (Role: Admin)

**Path Parameters:**
- `id` (Guid): ID của User

**Response (200 OK):**
```json
{
  "isSuccess": true,
  "data": null
}
```

**Lỗi có thể xảy ra:**
- `404 Not Found`: User không tồn tại

---

## 19. Lead APIs

Base URL: `/api/leads`

### 19.1. Create Lead from Web (Public)

**Endpoint:** `POST /api/leads/public`

**Mô tả:** Tạo Lead từ form web public (Landing page). Không cần authentication. Các field khác sẽ được set `null` hoặc mặc định trong backend, `Source` luôn là `"Landing"`.

**Authorization:** Không cần (Public)

**Request Body:**
```json
{
  "contactName": "Nguyễn Văn A",
  "phone": "0912345678",
  "zaloId": "zalo_user_id",
  "email": "parent@example.com"
}
```

**Lưu ý:**
- Ít nhất **một trong bốn field** `contactName`, `phone`, `email`, `zaloId` phải không rỗng.
- `source` được tự động set = `"Landing"`.
- `ownerStaffId` mặc định là `null` (chưa gán cho staff).

**Response (201 Created):**
```json
{
  "isSuccess": true,
  "data": {
    "id": "guid",
    "source": "Landing",
    "campaign": null,
    "contactName": "Nguyễn Văn A",
    "childName": null,
    "childDateOfBirth": null,
    "phone": "0912345678",
    "zaloId": "zalo_user_id",
    "email": "parent@example.com",
    "company": null,
    "subject": null,
    "branchPreference": null,
    "programInterest": null,
    "notes": null,
    "status": "New",
    "ownerStaffId": null,
    "createdAt": "2026-01-27T10:00:00Z"
  }
}
```

---

### 19.2. Create Lead (Internal)

**Endpoint:** `POST /api/leads`

**Mô tả:** Tạo Lead từ Zalo, Referral, Offline hoặc các nguồn nội bộ khác. Cho phép nhập đầy đủ thông tin, bao gồm child info, campaign, branchPreference, ownerStaffId,...

**Authorization:** Required (Roles: `Admin`, `ManagementStaff`, `AccountantStaff`)

**Request Body:**
```json
{
  "source": "Zalo",
  "campaign": "Zalo_OA_2026",
  "contactName": "Nguyễn Văn A",
  "childName": "Nguyễn Văn B",
  "childDateOfBirth": "2017-03-24T00:00:00Z",
  "phone": "0912345678",
  "zaloId": "zalo_user_id",
  "email": "parent@example.com",
  "company": "Trường Tiểu học ABC",
  "subject": "Đăng ký tư vấn khóa học",
  "branchPreference": "guid",
  "programInterest": "Tiếng Anh Lớp 2",
  "notes": "Khách ưu tiên lịch tối thứ 3, 5",
  "ownerStaffId": "guid"
}
```

**Response (201 Created):** Tương tự như 19.1, nhưng các field có thể đầy đủ hơn tùy dữ liệu gửi lên.

---

### 19.3. Get Leads

**Endpoint:** `GET /api/leads`

**Mô tả:** Xem danh sách Leads với filter theo `status`, `source`, `ownerStaffId`, `branchPreference`, `searchTerm` và phân trang.

**Authorization:** Required (Roles: `Admin`, `ManagementStaff`, `AccountantStaff`)

**Query Parameters:**
- `status` (string?, optional): Lọc theo trạng thái lead (`New`, `Contacted`, `BookedTest`, `TestDone`, `Enrolled`, `Lost`)
- `source` (string?, optional): Lọc theo nguồn (`Landing`, `Zalo`, `Referral`, `Offline`, ...)
- `ownerStaffId` (Guid?, optional): Lọc theo staff phụ trách
- `branchPreference` (Guid?, optional): Lọc theo chi nhánh ưu tiên
- `searchTerm` (string?, optional): Tìm theo tên, điện thoại, email
- `page` (int, default: 1): Số trang
- `pageSize` (int, default: 20): Số lượng items mỗi trang

---

### 19.4. Get Lead By ID

**Endpoint:** `GET /api/leads/{id}`

**Mô tả:** Xem chi tiết Lead, bao gồm thông tin liên hệ, child info, branchPreference, status, ownerStaffId,...

**Authorization:** Required (Roles: `Admin`, `ManagementStaff`, `AccountantStaff`)

**Note:** Response detail lead hiện có thêm `programInterestSummary`, là chuỗi tổng hợp từ các `LeadChild.ProgramInterest` khác rỗng, loại trùng và nối bằng dấu phẩy.

---

### 19.5. Update Lead

**Endpoint:** `PUT /api/leads/{id}`

**Mô tả:** Cập nhật thông tin Lead (contact, child info, branchPreference, notes,...). Không cho phép sửa Lead đã `Enrolled`.

**Authorization:** Required (Roles: `Admin`, `ManagementStaff`, `AccountantStaff`)

**Request Body (tất cả fields đều optional):**
```json
{
  "contactName": "Nguyễn Văn A - Updated",
  "childName": "Nguyễn Văn B",
  "childDateOfBirth": "2017-03-24T00:00:00Z",
  "phone": "0912345678",
  "zaloId": "zalo_user_id",
  "email": "parent@example.com",
  "company": "Trường Tiểu học ABC",
  "subject": "Cập nhật nhu cầu khóa học",
  "branchPreference": "guid",
  "programInterest": "Tiếng Anh Lớp 3",
  "notes": "Khách muốn đổi lịch kiểm tra"
}
```

---

### 19.6. Assign Lead

**Endpoint:** `POST /api/leads/{id}/assign`

**Mô tả:** Gán Lead cho 1 staff phụ trách (`ownerStaffId`) và tạo LeadActivity tương ứng.

**Authorization:** Required (Roles: `Admin`, `ManagementStaff`)

---

### 19.7. Update Lead Status

**Endpoint:** `PATCH /api/leads/{id}/status`

**Mô tả:** Cập nhật trạng thái Lead (`New`, `Contacted`, `BookedTest`, `TestDone`, `Enrolled`, `Lost`). Khi chuyển trạng thái, hệ thống sẽ log LeadActivity để theo dõi lịch sử.

**Authorization:** Required (Roles: `Admin`, `ManagementStaff`, `AccountantStaff`)

---

### 19.8. Add Lead Note

**Endpoint:** `POST /api/leads/{id}/notes`

**Mô tả:** Thêm ghi chú/hoạt động cho Lead (gọi điện, nhắn tin, note chăm sóc...) và cập nhật `touchCount`, `nextActionAt`, `firstResponseAt` (nếu là lần contact đầu tiên). Nếu `activityType` là `Call`, `Zalo`, `Sms` hoặc `Email` và Lead đang ở trạng thái `New`, hệ thống sẽ tự động chuyển sang `Contacted`. Có thể truyền `clearNextAction = true` để xóa lịch follow-up hiện tại của Lead; không được truyền đồng thời `nextActionAt` và `clearNextAction = true`.

**Authorization:** Required (Roles: `Admin`, `ManagementStaff`, `AccountantStaff`)

---

### 19.9. Get Lead Activities

**Endpoint:** `GET /api/leads/{id}/activities`

**Mô tả:** Xem lịch sử toàn bộ hoạt động (LeadActivity) của Lead: created, notes, status changes, assignments,...

**Authorization:** Required (Roles: `Admin`, `ManagementStaff`, `AccountantStaff`)

---

### 19.10. Get Lead SLA

**Endpoint:** `GET /api/leads/{id}/sla`

**Mô tả:** Theo dõi thời gian phản hồi đầu tiên cho Lead (`firstResponseAt`) và SLA compliance.

**Authorization:** Required (Roles: `Admin`, `ManagementStaff`, `AccountantStaff`)

---

*Tài liệu này được cập nhật lần cuối: 2026-01-27*

---

## 20. Placement Test APIs

Base URL: `/api/placement-tests`

### 20.1. Schedule Placement Test

**Endpoint:** `POST /api/placement-tests`

**Mô tả:** Đặt lịch Placement Test cho một Lead (có thể kèm sẵn StudentProfile và Class nếu đã biết). Khi đặt lịch, Lead thường chuyển sang trạng thái `BookedTest`.

**Authorization:** Required (Roles: `Admin`, `ManagementStaff`)

**Request Body:**
```json
{
  "leadId": "guid",
  "studentProfileId": "guid",
  "classId": "guid",
  "scheduledAt": "2026-01-28T09:00:00Z",
  "room": "Phòng Test 1",
  "invigilatorUserId": "guid"
}
```

---

### 20.2. Get Placement Tests

**Endpoint:** `GET /api/placement-tests`

**Mô tả:** Xem danh sách Placement Tests với filter theo Lead, Student, trạng thái và khoảng ngày.

**Authorization:** Required (Roles: `Admin`, `ManagementStaff`, `AccountantStaff`)

**Query Parameters:**
- `leadId` (Guid?, optional): Lọc theo Lead
- `studentProfileId` (Guid?, optional): Lọc theo Student Profile
- `status` (`PlacementTestStatus`?, optional): `Scheduled`, `Completed`, `NoShow`, `Cancelled`
- `fromDate` (DateTime?, optional): Ngày bắt đầu
- `toDate` (DateTime?, optional): Ngày kết thúc
- `page` (int, default: 1): Số trang
- `pageSize` (int, default: 20): Số lượng items mỗi trang

---

### 20.3. Get Placement Test By ID

**Endpoint:** `GET /api/placement-tests/{id}`

**Mô tả:** Xem chi tiết một Placement Test, bao gồm Lead, StudentProfile, Class, lịch hẹn, kết quả, notes,...

**Authorization:** Required (Roles: `Admin`, `ManagementStaff`, `AccountantStaff`)

---

### 20.4. Update Placement Test

**Endpoint:** `PUT /api/placement-tests/{id}`

**Mô tả:** Cập nhật thông tin lịch hẹn Placement Test (giờ, phòng, invigilator, studentProfile, class).

**Authorization:** Required (Roles: `Admin`, `ManagementStaff`)

**Request Body (tất cả fields đều optional):**
```json
{
  "scheduledAt": "2026-01-28T10:00:00Z",
  "room": "Phòng Test 2",
  "invigilatorUserId": "guid",
  "studentProfileId": "guid",
  "classId": "guid"
}
```

---

### 20.5. Cancel Placement Test

**Endpoint:** `POST /api/placement-tests/{id}/cancel`

**Mô tả:** Hủy lịch Placement Test với lý do cụ thể, trạng thái chuyển sang `Cancelled`.

**Authorization:** Required (Roles: `Admin`, `ManagementStaff`)

**Request Body (optional):**
```json
{
  "reason": "Phụ huynh xin dời lịch"
}
```

---

### 20.6. Mark Placement Test No-Show

**Endpoint:** `POST /api/placement-tests/{id}/no-show`

**Mô tả:** Đánh dấu học sinh không đến thi (`NoShow`). Dùng khi đến giờ thi mà học sinh không xuất hiện.

**Authorization:** Required (Roles: `Admin`, `ManagementStaff`)

---

### 20.7. Update Placement Test Results

**Endpoint:** `PUT /api/placement-tests/{id}/results`

**Mô tả:** Nhập kết quả Placement Test (Listening/Speaking/Reading/Writing, tổng điểm, gợi ý level/program). Khi đủ điểm và trạng thái phù hợp, hệ thống tự chuyển lead sang `TestDone`.

**Authorization:** Required (Roles: `Admin`, `ManagementStaff`)

**Request Body:**
```json
{
  "listeningScore": 20,
  "speakingScore": 18,
  "readingScore": 22,
  "writingScore": 19,
  "resultScore": 79,
  "levelRecommendation": "Starter 2",
  "programRecommendation": "Tiếng Anh Lớp 2",
  "attachmentUrl": "https://res.cloudinary.com/.../placement-test-result.jpg"
}
```

---

### 20.8. Add Placement Test Note

**Endpoint:** `POST /api/placement-tests/{id}/notes`

**Mô tả:** Thêm ghi chú cho Placement Test (nhận xét thêm của giáo viên, tình trạng học sinh,...).

**Authorization:** Required (Roles: `Admin`, `ManagementStaff`)

**Request Body:**
```json
{
  "note": "Học sinh hơi rụt rè, cần thêm thời gian làm quen."
}
```

---

### 20.9. Convert Lead To Enrolled

**Endpoint:** `POST /api/placement-tests/{id}/convert-to-enrolled`

**Mô tả:** Sau khi có kết quả test và phụ huynh đồng ý ghi danh, API này sẽ:
- Link Placement Test với `studentProfileId` (nếu gửi vào).
- Cập nhật Lead sang trạng thái `Enrolled` và set `ConvertedStudentProfileId`, `ConvertedAt`.
- Trả về thông tin cần thiết để FE tiếp tục tạo Enrollment cho lớp.

**Authorization:** Required (Roles: `Admin`, `ManagementStaff`)

**Request Body (optional):**
```json
{
  "studentProfileId": "guid"
}
```


---

## 21. Monthly Report APIs

Base URL: `/api/monthly-reports`

### 21.1. Create Monthly Report Job

**Endpoint:** `POST /api/monthly-reports/jobs`

**Mô tả:** UC-174: Tạo Monthly Report Job cho một branch trong tháng/năm cụ thể.

**Authorization:** Required (Roles: Admin, ManagementStaff)

**Request Body:**
```json
{
  "month": 1,
  "year": 2026,
  "branchId": "guid"
}
```

**Response (201 Created):**
```json
{
  "isSuccess": true,
  "data": {
    "id": "guid",
    "branchId": "guid",
    "month": 1,
    "year": 2026,
    "status": "Pending",
    "createdAt": "2026-02-08T10:00:00Z"
  }
}
```

---

### 21.2. Get Monthly Report Jobs

**Endpoint:** `GET /api/monthly-reports/jobs`

**Mô tả:** UC-177: Xem danh sách Monthly Report Jobs với phân trang và filter.

**Authorization:** Required (Roles: Admin, ManagementStaff)

**Query Parameters:**
- `branchId` (Guid?, optional): Lọc theo Branch ID
- `month` (int?, optional): Lọc theo tháng
- `year` (int?, optional): Lọc theo năm
- `status` (string?, optional): Lọc theo status
- `pageNumber` (int, default: 1): Số trang
- `pageSize` (int, default: 10): Số lượng items mỗi trang

**Response (200 OK):** Trả về danh sách jobs với pagination

---

### 21.3. Get Monthly Report Job By ID

**Endpoint:** `GET /api/monthly-reports/jobs/{jobId}`

**Mô tả:** UC-178: Xem trạng thái Monthly Report Job.

**Authorization:** Required (Roles: Admin, ManagementStaff)

**Response (200 OK):** Trả về chi tiết job

---

### 21.4. Aggregate Monthly Report Data

**Endpoint:** `POST /api/monthly-reports/jobs/{jobId}/aggregate`

**Mô tả:** UC-175: Gom dữ liệu cho Monthly Report (trigger cho tất cả students trong job).

**Authorization:** Required (Roles: Admin, ManagementStaff)

**Response (200 OK):** Trả về số lượng reports đã tạo/cập nhật

---

### 21.5. Get Monthly Reports

**Endpoint:** `GET /api/monthly-reports`

**Mô tả:** Xem danh sách Monthly Reports với filter và phân trang.

**Authorization:** Required (Roles: Teacher, Admin, ManagementStaff, Parent)

**Query Parameters:**
- `studentProfileId` (Guid?, optional): Lọc theo Student Profile ID
- `classId` (Guid?, optional): Lọc theo Class ID
- `jobId` (Guid?, optional): Lọc theo Job ID
- `month` (int?, optional): Lọc theo tháng
- `year` (int?, optional): Lọc theo năm
- `status` (string?, optional): Lọc theo status
- `pageNumber` (int, default: 1): Số trang
- `pageSize` (int, default: 10): Số lượng items mỗi trang

**Response (200 OK):** Trả về danh sách reports với pagination

**Lưu ý:** 
- Parent/Student chỉ xem được reports có status = Published
- Teacher chỉ xem được reports của lớp mình

---

### 21.6. Get Monthly Report By ID

**Endpoint:** `GET /api/monthly-reports/{reportId}`

**Mô tả:** UC-179: Teacher xem draft Monthly Report. UC-186: Parent/Student xem Monthly Report.

**Authorization:** Required (Roles: Teacher, Admin, ManagementStaff, Parent)

**Response (200 OK):** Trả về chi tiết report

---

### 21.7. Generate Monthly Report Draft

**Endpoint:** `POST /api/monthly-reports/{reportId}/generate-draft`

**Mô tả:** UC-176: Generate draft Monthly Report bằng AI.

**Authorization:** Required (Role: Teacher)

**Response (200 OK):** Trả về report với DraftContent đã được generate

---

### 21.8. Update Monthly Report Draft

**Endpoint:** `PUT /api/monthly-reports/{reportId}/draft`

**Mô tả:** UC-180: Teacher chỉnh sửa draft Monthly Report.

**Authorization:** Required (Role: Teacher)

**Request Body:**
```json
{
  "draftContent": "{\"overview\": \"...\", \"strengths\": [...], ...}"
}
```

**Response (200 OK):** Trả về report đã được cập nhật

**Lưu ý:** Có thể chỉnh sửa report có status = Rejected (sẽ tự động reset về Draft)

---

### 21.9. Submit Monthly Report

**Endpoint:** `POST /api/monthly-reports/{reportId}/submit`

**Mô tả:** UC-181: Teacher submit Monthly Report (chuyển từ Draft → Submitted).

**Authorization:** Required (Role: Teacher)

**Response (200 OK):** Trả về report với status = Submitted

---

### 21.10. Add Report Comment

**Endpoint:** `POST /api/monthly-reports/{reportId}/comments`

**Mô tả:** UC-182: Staff/Admin comment Monthly Report.

**Authorization:** Required (Roles: Admin, ManagementStaff)

**Request Body:**
```json
{
  "content": "Cần bổ sung thêm thông tin về điểm mạnh"
}
```

**Response (201 Created):** Trả về comment đã tạo

---

### 21.11. Approve Monthly Report

**Endpoint:** `POST /api/monthly-reports/{reportId}/approve`

**Mô tả:** UC-183: Staff/Admin approve Monthly Report (chuyển từ Submitted → Approved).

**Authorization:** Required (Roles: Admin, ManagementStaff)

**Response (200 OK):** Trả về report với status = Approved

---

### 21.12. Reject Monthly Report

**Endpoint:** `POST /api/monthly-reports/{reportId}/reject`

**Mô tả:** UC-184: Staff/Admin reject Monthly Report (chuyển từ Submitted → Rejected).

**Authorization:** Required (Roles: Admin, ManagementStaff)

**Response (200 OK):** Trả về report với status = Rejected

---

### 21.13. Publish Monthly Report

**Endpoint:** `POST /api/monthly-reports/{reportId}/publish`

**Mô tả:** UC-185: Publish Monthly Report (chuyển từ Approved → Published).

**Authorization:** Required (Roles: Admin, ManagementStaff)

**Response (200 OK):** Trả về report với status = Published và PdfUrl (nếu có)

---

### 21.14. Generate Monthly Report PDF

**Endpoint:** `POST /api/monthly-reports/{reportId}/generate-pdf`

**Mô tả:** Generate hoặc regenerate PDF cho Monthly Report (on-demand).

**Authorization:** Required (Roles: Admin, ManagementStaff, Teacher)

**Response (200 OK):**
```json
{
  "isSuccess": true,
  "data": {
    "reportId": "guid",
    "pdfUrl": "https://res.cloudinary.com/.../monthly-report.pdf",
    "pdfGeneratedAt": "2026-02-08T10:00:00Z"
  }
}
```

---

## 22. Session Report APIs

Base URL: `/api/session-reports`

### 22.1. Create Session Report

**Endpoint:** `POST /api/session-reports`

**Mô tả:** UC-163: Teacher tạo Session Report. UC-164: Teacher ghi feedback cho từng học sinh.

**Authorization:** Required (Role: Teacher)

**Request Body:**
```json
{
  "sessionId": "guid",
  "studentProfileId": "guid",
  "reportDate": "2026-02-08",
  "feedback": "Học sinh làm bài tốt, cần cải thiện phát âm"
}
```

**Response (201 Created):** Trả về session report đã tạo

---

### 22.2. Get Session Reports

**Endpoint:** `GET /api/session-reports`

**Mô tả:** UC-165: Xem danh sách Session Reports. UC-168: Filter Session Reports theo date range.

**Authorization:** Required (Roles: Teacher, Admin, ManagementStaff)

**Query Parameters:**
- `sessionId` (Guid?, optional): Lọc theo Session ID
- `studentProfileId` (Guid?, optional): Lọc theo Student Profile ID
- `teacherUserId` (Guid?, optional): Lọc theo Teacher User ID
- `classId` (Guid?, optional): Lọc theo Class ID
- `fromDate` (DateOnly?, optional): Ngày bắt đầu
- `toDate` (DateOnly?, optional): Ngày kết thúc
- `pageNumber` (int, default: 1): Số trang
- `pageSize` (int, default: 10): Số lượng items mỗi trang

**Response (200 OK):** Trả về danh sách session reports với pagination

---

### 22.3. Get Session Report By ID

**Endpoint:** `GET /api/session-reports/{id}`

**Mô tả:** UC-166: Xem chi tiết Session Report.

**Authorization:** Required (Roles: Teacher, Admin, ManagementStaff)

**Response (200 OK):** Trả về chi tiết session report

---

### 22.4. Update Session Report

**Endpoint:** `PUT /api/session-reports/{id}`

**Mô tả:** UC-167: Cập nhật Session Report.

**Authorization:** Required (Roles: Teacher, Admin, ManagementStaff)

**Request Body:**
```json
{
  "feedback": "Học sinh làm bài tốt, cần cải thiện phát âm - Updated"
}
```

**Response (200 OK):** Trả về session report đã được cập nhật

---

### 22.5. Get Teacher Session Reports (Monthly)

**Endpoint:** `GET /api/session-reports/teachers/{teacherUserId}/monthly`

**Mô tả:** UC-169: Xem Session Reports của giáo viên trong tháng.

**Authorization:** Required (Roles: Teacher, Admin, ManagementStaff)

**Query Parameters:**
- `year` (int, required): Năm
- `month` (int, required): Tháng
- `pageNumber` (int, default: 1): Số trang
- `pageSize` (int, default: 10): Số lượng items mỗi trang

**Response (200 OK):** Trả về danh sách session reports của teacher trong tháng

---

### 22.6. AI Enhance Draft Feedback

**Endpoint:** `POST /api/session-reports/ai/enhance-feedback`

**Mô tả:** UC-174: AI enhance draft feedback trước khi teacher submit feedback chính thức. Teacher gửi draft feedback (viết tự nhiên), AI sẽ chỉnh sửa văn phong formal hơn. Đây là preview API, KHÔNG lưu vào DB.

**Authorization:** Required (Role: Teacher)

**Request Body:**
```json
{
  "draft": "Hôm nay em học tốt lắm, làm bài đầy đủ, chăm chỉ lắm"
}
```

**Request Body (optional - nếu muốn AI personalize hơn):**
```json
{
  "draft": "Hôm nay em học tốt lắm, làm bài đầy đủ, chăm chỉ lắm",
  "sessionId": "guid",
  "studentProfileId": "guid",
}
```

**Response (200 OK):**
```json
{
  "isSuccess": true,
  "data": {
    "enhancedFeedback": "Học sinh đã có buổi học hiệu quả, hoàn thành đầy đủ các bài tập được giao và thể hiện sự chăm chỉ trong học tập.",
    "originalFeedback": "Hôm nay em học tốt lắm, làm bài đầy đủ, chăm chỉ lắm",
    "isMock": true
  }
}
```

**Lưu ý:**
- Đây là preview API - KHÔNG lưu vào DB
- Teacher có thể gọi nhiều lần để test different versions
- Sau khi ưng ý, teacher submit chính thức (sẽ lưu vào DB)

---

### 22.7. Submit Session Report for Review

**Endpoint:** `POST /api/session-reports/{id}/submit`

**Mô tả:** UC-174a: Teacher submit Session Report cho duyệt (DRAFT → REVIEW).

**Authorization:** Required (Role: Teacher)

**Response (200 OK):** Trả về session report với status = REVIEW

---

### 22.8. Approve Session Report

**Endpoint:** `POST /api/session-reports/{id}/approve`

**Mô tả:** UC-175: Staff/Admin approve Session Report (REVIEW → APPROVED).

**Authorization:** Required (Roles: Admin, ManagementStaff)

**Response (200 OK):** Trả về session report với status = APPROVED

---

### 22.9. Reject Session Report

**Endpoint:** `POST /api/session-reports/{id}/reject`

**Mô tả:** UC-176: Staff/Admin reject Session Report (REVIEW → REJECTED).

**Authorization:** Required (Roles: Admin, ManagementStaff)

**Request Body:**
```json
{
  "reason": "Feedback chưa đủ chi tiết, cần bổ sung thêm thông tin về bài học"
}
```

**Response (200 OK):** Trả về session report với status = REJECTED

---

### 22.10. Publish Session Report

**Endpoint:** `POST /api/session-reports/{id}/publish`

**Mô tả:** UC-177: Publish Session Report (APPROVED → PUBLISHED). Sau khi publish, Parent/Student có thể xem được.

**Authorization:** Required (Roles: Admin, ManagementStaff)

**Response (200 OK):** Trả về session report với status = PUBLISHED

---

*Tài liệu này được cập nhật lần cuối: 2026-02-08*


