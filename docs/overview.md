Dựa trên phân tích code, tôi sẽ tạo file markdown hoàn chỉnh cho bạn:

```markdown
# 📚 API Documentation - Staff Dashboard Overview

## Mục Lục
- [Tổng quan](#tổng-quan)
- [1. API Staff Overview (Legacy)](#1-api-staff-overview-legacy)
- [2. API Management Staff Overview](#2-api-management-staff-overview)
- [3. API Accountant Staff Overview](#3-api-accountant-staff-overview)
- [So sánh các API](#-so-sánh-các-api)
- [Error Responses](#-error-responses)
- [Lưu ý cho Frontend](#-lưu-ý-cho-frontend)

---

## Tổng quan

Hệ thống có **3 API Dashboard Overview** dành cho Staff với các mục đích khác nhau:

| API | Endpoint | Role | Mô tả |
|-----|----------|------|-------|
| Staff Overview (Legacy) | `GET /api/me/staff/overview` | Admin, ManagementStaff, AccountantStaff | API cũ - nên dùng 2 API mới |
| Management Staff Overview | `GET /api/me/management-staff/overview` | Admin, ManagementStaff | Dashboard cho Quản lý vận hành |
| Accountant Staff Overview | `GET /api/me/accountant-staff/overview` | Admin, ManagementStaff, AccountantStaff | Dashboard cho Kế toán |

---

## 1. API Staff Overview (Legacy)

> ⚠️ **Lưu ý**: Đây là API cũ, khuyến nghị sử dụng `management-staff/overview` hoặc `accountant-staff/overview` thay thế.

### Endpoint
```

GET /api/me/staff/overview
```
### Authorization
- **Roles**: `Admin`, `ManagementStaff`, `AccountantStaff`
- **Header**: `Authorization: Bearer {token}`

### Query Parameters

| Parameter | Type | Required | Default | Mô tả |
|-----------|------|----------|---------|-------|
| `classId` | `Guid` | ❌ | `null` | Lọc theo ID lớp học |
| `studentProfileId` | `Guid` | ❌ | `null` | Lọc theo ID hồ sơ học sinh |
| `leadId` | `Guid` | ❌ | `null` | Lọc theo ID lead |
| `enrollmentId` | `Guid` | ❌ | `null` | Lọc theo ID đăng ký học |
| `fromDate` | `DateTime` | ❌ | `now - 1 tháng` | Ngày bắt đầu lọc |
| `toDate` | `DateTime` | ❌ | `now + 1 tháng` | Ngày kết thúc lọc |

### Response Schema

```typescript
interface StaffOverviewResponse {
  statistics: DashboardStatistics;
  recentLeads: LeadSummaryDto[];              // Max 20 items
  recentEnrollments: EnrollmentSummaryDto[];  // Max 20 items
  classes: ClassSummaryDto[];                 // Max 20 items
  upcomingSessions: SessionSummaryDto[];      // Max 20 items
  pendingMakeupCredits: MakeupCreditSummaryDto[]; // Max 20 items
  pendingLeaveRequests: LeaveRequestSummaryDto[]; // Max 20 items
  pendingInvoices: InvoiceSummaryDto[];       // Max 20 items
  pendingReports: ReportSummaryDto[];         // Max 20 items
  openTickets: TicketSummaryDto[];            // Max 20 items
}

interface DashboardStatistics {
  totalLeads: number;
  totalEnrollments: number;
  totalClasses: number;
  upcomingSessions: number;
  pendingMakeupCredits: number;
  pendingLeaveRequests: number;
  pendingInvoices: number;
  pendingReports: number;
  openTickets: number;
}
```
```


### Ví dụ Request

```shell script
curl -X GET "https://api.kidzgo.vn/api/me/staff/overview?classId=3fa85f64-5717-4562-b3fc-2c963f66afa6&fromDate=2026-04-01&toDate=2026-05-31" \
  -H "Authorization: Bearer eyJhbGciOiJIUzI1NiIs..."
```


### Response Mẫu

```json
{
  "success": true,
  "data": {
    "statistics": {
      "totalLeads": 150,
      "totalEnrollments": 320,
      "totalClasses": 45,
      "upcomingSessions": 28,
      "pendingMakeupCredits": 12,
      "pendingLeaveRequests": 5,
      "pendingInvoices": 18,
      "pendingReports": 8,
      "openTickets": 3
    },
    "recentLeads": [
      {
        "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
        "name": "Nguyễn Văn A",
        "phoneNumber": "0901234567",
        "status": "New",
        "createdAt": "2026-05-05T10:30:00Z"
      }
    ],
    "recentEnrollments": [
      {
        "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
        "classCode": "ENG-A1-001",
        "studentName": "Trần Thị B",
        "enrollDate": "2026-05-01T00:00:00+07:00",
        "status": "Active"
      }
    ],
    "classes": [
      {
        "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
        "code": "ENG-A1-001",
        "title": "Tiếng Anh A1 - Lớp 1",
        "enrollmentCount": 12,
        "capacity": 15,
        "status": "Active"
      }
    ],
    "upcomingSessions": [
      {
        "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
        "classId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
        "classCode": "ENG-A1-001",
        "plannedDatetime": "2026-05-06T09:00:00+07:00",
        "status": "Scheduled"
      }
    ],
    "pendingMakeupCredits": [
      {
        "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
        "studentName": "Lê Văn C",
        "status": "Available",
        "expiresAt": "2026-06-01T00:00:00Z"
      }
    ],
    "pendingLeaveRequests": [
      {
        "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
        "studentName": "Phạm Thị D",
        "requestDate": "2026-05-04T14:30:00Z",
        "status": "Pending"
      }
    ],
    "pendingInvoices": [
      {
        "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
        "invoiceNumber": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
        "studentName": "Hoàng Văn E",
        "amount": 5000000,
        "paymentStatus": "Pending"
      }
    ],
    "pendingReports": [
      {
        "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
        "studentName": "Ngô Thị F",
        "classCode": "ENG-A1-001",
        "status": "Draft",
        "reportMonth": "2026-05-01T00:00:00+07:00"
      }
    ],
    "openTickets": [
      {
        "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
        "title": "Hỗ trợ kỹ thuật",
        "status": "Open",
        "priority": "High",
        "createdAt": "2026-05-04T16:00:00Z"
      }
    ]
  }
}
```


---

## 2. API Management Staff Overview

> 📋 **Mục đích**: Dashboard cho **Nhân viên Quản lý** (Operations) - Tập trung vào quản lý lớp học, học sinh, lead, và vận hành.

### Endpoint
```
GET /api/me/management-staff/overview
```


### Authorization
- **Roles**: `Admin`, `ManagementStaff`
- **Header**: `Authorization: Bearer {token}`

### Query Parameters

| Parameter | Type | Required | Default | Mô tả |
|-----------|------|----------|---------|-------|
| `classId` | `Guid` | ❌ | `null` | Lọc theo ID lớp học |
| `studentProfileId` | `Guid` | ❌ | `null` | Lọc theo ID hồ sơ học sinh |
| `leadId` | `Guid` | ❌ | `null` | Lọc theo ID lead |
| `enrollmentId` | `Guid` | ❌ | `null` | Lọc theo ID đăng ký học |
| `fromDate` | `DateTime` | ❌ | `now - 1 tháng` | Ngày bắt đầu lọc |
| `toDate` | `DateTime` | ❌ | `now + 1 tháng` | Ngày kết thúc lọc |

### Response Schema

```typescript
interface ManagementStaffOverviewResponse {
  statistics: DashboardStatistics;
  recentLeads: LeadSummaryDto[];              // Max 20 items
  recentEnrollments: EnrollmentSummaryDto[];  // Max 20 items
  classes: ClassSummaryDto[];                 // Max 20 items
  upcomingSessions: SessionSummaryDto[];      // Max 20 items
  pendingMakeupCredits: MakeupCreditSummaryDto[]; // Max 20 items
  pendingLeaveRequests: LeaveRequestSummaryDto[]; // Max 20 items
  pendingReports: ReportSummaryDto[];         // Max 20 items
  openTickets: TicketSummaryDto[];            // Max 20 items
}

interface DashboardStatistics {
  totalLeads: number;
  totalEnrollments: number;
  totalClasses: number;
  upcomingSessions: number;
  pendingMakeupCredits: number;
  pendingLeaveRequests: number;
  pendingReports: number;
  openTickets: number;
}

interface LeadSummaryDto {
  id: string;              // UUID
  name: string;            // Tên liên hệ
  phoneNumber: string;     // Số điện thoại
  status: string;          // "New" | "Contacted" | "Qualified" | "Converted" | "Lost"
  createdAt: string;       // ISO 8601 datetime
}

interface EnrollmentSummaryDto {
  id: string;              // UUID
  classCode: string;       // Mã lớp học
  studentName: string;     // Tên học sinh
  enrollDate: string;      // ISO 8601 datetime (Vietnam timezone)
  status: string;          // "Active" | "Completed" | "Withdrawn" | "Suspended"
}

interface ClassSummaryDto {
  id: string;              // UUID
  code: string;            // Mã lớp
  title: string;           // Tên lớp
  enrollmentCount: number; // Số học sinh đang học
  capacity: number;        // Sức chứa tối đa
  status: string;          // "Active" | "Completed" | "Cancelled" | "Pending"
}

interface SessionSummaryDto {
  id: string;              // UUID
  classId: string;         // UUID của lớp
  classCode: string;       // Mã lớp
  plannedDatetime: string; // ISO 8601 datetime (Vietnam timezone)
  status: string;          // "Scheduled" | "InProgress" | "Completed" | "Cancelled"
}

interface MakeupCreditSummaryDto {
  id: string;              // UUID
  studentName: string;     // Tên học sinh
  status: string;          // "Available" | "Used" | "Expired"
  expiresAt: string;       // ISO 8601 datetime
}

interface LeaveRequestSummaryDto {
  id: string;              // UUID
  studentName: string;     // Tên học sinh
  requestDate: string;     // ISO 8601 datetime
  status: string;          // "Pending" | "Approved" | "Rejected"
}

interface ReportSummaryDto {
  id: string;              // UUID
  studentName: string;     // Tên học sinh
  classCode: string;       // Mã lớp
  status: string;          // "Draft" | "Review" | "Published"
  reportMonth: string;     // ISO 8601 datetime (ngày đầu tháng)
}

interface TicketSummaryDto {
  id: string;              // UUID
  title: string;           // Tiêu đề ticket
  status: string;          // "Open" | "InProgress" | "Resolved" | "Closed"
  priority: string;        // Category của ticket
  createdAt: string;       // ISO 8601 datetime
}
```


### Ví dụ Request

```shell script
curl -X GET "https://api.kidzgo.vn/api/me/management-staff/overview?classId=3fa85f64-5717-4562-b3fc-2c963f66afa6&fromDate=2026-04-01&toDate=2026-05-31" \
  -H "Authorization: Bearer eyJhbGciOiJIUzI1NiIs..."
```


### Response Mẫu

```json
{
  "success": true,
  "data": {
    "statistics": {
      "totalLeads": 150,
      "totalEnrollments": 320,
      "totalClasses": 45,
      "upcomingSessions": 28,
      "pendingMakeupCredits": 12,
      "pendingLeaveRequests": 5,
      "pendingReports": 8,
      "openTickets": 3
    },
    "recentLeads": [
      {
        "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
        "name": "Nguyễn Văn A",
        "phoneNumber": "0901234567",
        "status": "New",
        "createdAt": "2026-05-05T10:30:00Z"
      }
    ],
    "recentEnrollments": [
      {
        "id": "4fa85f64-5717-4562-b3fc-2c963f66afa7",
        "classCode": "ENG-A1-001",
        "studentName": "Trần Thị B",
        "enrollDate": "2026-05-01T00:00:00+07:00",
        "status": "Active"
      }
    ],
    "classes": [
      {
        "id": "5fa85f64-5717-4562-b3fc-2c963f66afa8",
        "code": "ENG-A1-001",
        "title": "Tiếng Anh A1 - Lớp 1",
        "enrollmentCount": 12,
        "capacity": 15,
        "status": "Active"
      }
    ],
    "upcomingSessions": [
      {
        "id": "6fa85f64-5717-4562-b3fc-2c963f66afa9",
        "classId": "5fa85f64-5717-4562-b3fc-2c963f66afa8",
        "classCode": "ENG-A1-001",
        "plannedDatetime": "2026-05-06T09:00:00+07:00",
        "status": "Scheduled"
      }
    ],
    "pendingMakeupCredits": [
      {
        "id": "7fa85f64-5717-4562-b3fc-2c963f66afaa",
        "studentName": "Lê Văn C",
        "status": "Available",
        "expiresAt": "2026-06-01T00:00:00Z"
      }
    ],
    "pendingLeaveRequests": [
      {
        "id": "8fa85f64-5717-4562-b3fc-2c963f66afab",
        "studentName": "Phạm Thị D",
        "requestDate": "2026-05-04T14:30:00Z",
        "status": "Pending"
      }
    ],
    "pendingReports": [
      {
        "id": "9fa85f64-5717-4562-b3fc-2c963f66afac",
        "studentName": "Ngô Thị F",
        "classCode": "ENG-A1-001",
        "status": "Draft",
        "reportMonth": "2026-05-01T00:00:00+07:00"
      }
    ],
    "openTickets": [
      {
        "id": "afa85f64-5717-4562-b3fc-2c963f66afad",
        "title": "Hỗ trợ kỹ thuật",
        "status": "Open",
        "priority": "Technical",
        "createdAt": "2026-05-04T16:00:00Z"
      }
    ]
  }
}
```


---

## 3. API Accountant Staff Overview

> 💰 **Mục đích**: Dashboard cho **Kế toán** - Tập trung vào tài chính, hóa đơn, thanh toán, công nợ và sổ quỹ.

### Endpoint
```
GET /api/me/accountant-staff/overview
```


### Authorization
- **Roles**: `Admin`, `ManagementStaff`, `AccountantStaff`
- **Header**: `Authorization: Bearer {token}`

### Query Parameters

| Parameter | Type | Required | Default | Mô tả |
|-----------|------|----------|---------|-------|
| `studentProfileId` | `Guid` | ❌ | `null` | Lọc theo ID hồ sơ học sinh |
| `invoiceId` | `Guid` | ❌ | `null` | Lọc theo ID hóa đơn |
| `paymentId` | `Guid` | ❌ | `null` | Lọc theo ID thanh toán |
| `fromDate` | `DateTime` | ❌ | `now - 1 tháng` | Ngày bắt đầu lọc |
| `toDate` | `DateTime` | ❌ | `now + 1 tháng` | Ngày kết thúc lọc |

### Response Schema

```typescript
interface AccountantStaffOverviewResponse {
  statistics: FinanceStatistics;
  pendingInvoices: InvoiceSummaryDto[];        // Max 20 items - Hóa đơn chờ thanh toán
  overdueInvoices: InvoiceSummaryDto[];        // Max 20 items - Hóa đơn quá hạn
  recentPayments: PaymentSummaryDto[];         // Max 20 items - Thanh toán gần đây
  debtSummary: DebtSummaryDto[];               // Max 20 items - Tổng hợp công nợ
  pendingPayrolls: PayrollSummaryDto[];        // Max 10 items - Bảng lương chờ duyệt
  recentCashbookEntries: CashbookSummaryDto[]; // Max 20 items - Sổ quỹ
}

interface FinanceStatistics {
  totalRevenue: number;        // Tổng doanh thu trong khoảng thời gian (VND)
  pendingPayments: number;     // Tổng tiền chờ thanh toán (VND)
  overdueAmount: number;       // Tổng tiền quá hạn (VND)
  pendingInvoices: number;     // Số lượng hóa đơn chờ thanh toán
  overdueInvoices: number;     // Số lượng hóa đơn quá hạn
  pendingPayrolls: number;     // Số lượng bảng lương chờ duyệt
  cashBalance: number;         // Số dư tiền mặt hiện tại (VND)
}

interface InvoiceSummaryDto {
  id: string;                  // UUID
  invoiceNumber: string;       // Số hóa đơn (UUID format)
  studentName: string;         // Tên học sinh
  amount: number;              // Số tiền (VND)
  paymentStatus: string;       // "Pending" | "Paid" | "Overdue" | "Cancelled"
  dueDate?: string;            // ISO 8601 datetime (nullable)
  issuedAt: string;            // ISO 8601 datetime
}

interface PaymentSummaryDto {
  id: string;                  // UUID
  invoiceNumber: string;       // Số hóa đơn
  studentName: string;         // Tên học sinh
  amount: number;              // Số tiền (VND)
  paymentMethod: string;       // "Cash" | "Transfer" | "Card" | ...
  paidAt: string;              // ISO 8601 datetime
}

interface DebtSummaryDto {
  studentProfileId: string;    // UUID
  studentName: string;         // Tên học sinh
  totalDebt: number;           // Tổng công nợ (VND)
  overdueDays: number;         // Số ngày quá hạn lâu nhất
  invoiceCount: number;        // Số hóa đơn nợ
}

interface PayrollSummaryDto {
  id: string;                  // UUID
  payrollPeriod: string;       // "2026-04-01 to 2026-04-30"
  totalAmount: number;         // Tổng tiền lương (VND)
  status: string;              // "Draft" | "Approved" | "Paid"
  createdAt: string;           // ISO 8601 datetime
}

interface CashbookSummaryDto {
  id: string;                  // UUID
  type: string;                // "CashIn" | "CashOut"
  amount: number;              // Số tiền (VND)
  description: string;         // Mô tả giao dịch
  transactionDate: string;     // ISO 8601 datetime (Vietnam timezone)
}
```


### Ví dụ Request

```shell script
curl -X GET "https://api.kidzgo.vn/api/me/accountant-staff/overview?fromDate=2026-04-01&toDate=2026-05-31" \
  -H "Authorization: Bearer eyJhbGciOiJIUzI1NiIs..."
```


### Response Mẫu

```json
{
  "success": true,
  "data": {
    "statistics": {
      "totalRevenue": 125000000,
      "pendingPayments": 45000000,
      "overdueAmount": 12500000,
      "pendingInvoices": 18,
      "overdueInvoices": 5,
      "pendingPayrolls": 2,
      "cashBalance": 35000000
    },
    "pendingInvoices": [
      {
        "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
        "invoiceNumber": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
        "studentName": "Nguyễn Văn A",
        "amount": 5000000,
        "paymentStatus": "Pending",
        "dueDate": "2026-05-15T00:00:00+07:00",
        "issuedAt": "2026-05-01T10:00:00Z"
      }
    ],
    "overdueInvoices": [
      {
        "id": "4fa85f64-5717-4562-b3fc-2c963f66afa7",
        "invoiceNumber": "4fa85f64-5717-4562-b3fc-2c963f66afa7",
        "studentName": "Trần Thị B",
        "amount": 2500000,
        "paymentStatus": "Overdue",
        "dueDate": "2026-04-30T00:00:00+07:00",
        "issuedAt": "2026-04-01T10:00:00Z"
      }
    ],
    "recentPayments": [
      {
        "id": "5fa85f64-5717-4562-b3fc-2c963f66afa8",
        "invoiceNumber": "5fa85f64-5717-4562-b3fc-2c963f66afa8",
        "studentName": "Lê Văn C",
        "amount": 5000000,
        "paymentMethod": "Transfer",
        "paidAt": "2026-05-05T14:30:00Z"
      }
    ],
    "debtSummary": [
      {
        "studentProfileId": "6fa85f64-5717-4562-b3fc-2c963f66afa9",
        "studentName": "Trần Thị B",
        "totalDebt": 7500000,
        "overdueDays": 5,
        "invoiceCount": 3
      }
    ],
    "pendingPayrolls": [
      {
        "id": "7fa85f64-5717-4562-b3fc-2c963f66afaa",
        "payrollPeriod": "2026-04-01 to 2026-04-30",
        "totalAmount": 85000000,
        "status": "Approved",
        "createdAt": "2026-05-01T08:00:00Z"
      }
    ],
    "recentCashbookEntries": [
      {
        "id": "8fa85f64-5717-4562-b3fc-2c963f66afab",
        "type": "CashIn",
        "amount": 5000000,
        "description": "Thu học phí - Nguyễn Văn A",
        "transactionDate": "2026-05-05T00:00:00+07:00"
      },
      {
        "id": "9fa85f64-5717-4562-b3fc-2c963f66afac",
        "type": "CashOut",
        "amount": 2000000,
        "description": "Chi văn phòng phẩm",
        "transactionDate": "2026-05-04T00:00:00+07:00"
      }
    ]
  }
}
```


---

## 📊 So sánh các API

| Tính năng | Staff Overview | Management Staff | Accountant Staff |
|-----------|:--------------:|:----------------:|:----------------:|
| **Roles** | Admin, ManagementStaff, AccountantStaff | Admin, ManagementStaff | Admin, ManagementStaff, AccountantStaff |
| **Leads** | ✅ | ✅ | ❌ |
| **Enrollments** | ✅ | ✅ | ❌ |
| **Classes** | ✅ | ✅ | ❌ |
| **Sessions** | ✅ | ✅ | ❌ |
| **Makeup Credits** | ✅ | ✅ | ❌ |
| **Leave Requests** | ✅ | ✅ | ❌ |
| **Reports** | ✅ | ✅ | ❌ |
| **Tickets** | ✅ | ✅ | ❌ |
| **Invoices** | ✅ (pending only) | ❌ | ✅ (pending + overdue) |
| **Payments** | ❌ | ❌ | ✅ |
| **Debt Summary** | ❌ | ❌ | ✅ |
| **Payroll** | ❌ | ❌ | ✅ |
| **Cashbook** | ❌ | ❌ | ✅ |

### Khuyến nghị sử dụng

| Role | API nên dùng |
|------|-------------|
| **Admin** | Cả 2 API mới (Management + Accountant) |
| **ManagementStaff** | `/api/me/management-staff/overview` |
| **AccountantStaff** | `/api/me/accountant-staff/overview` |

---

## 🔐 Error Responses

### 401 Unauthorized
Token không hợp lệ hoặc hết hạn.

```json
{
  "success": false,
  "error": {
    "code": "Unauthorized",
    "message": "Invalid or expired token"
  }
}
```


### 403 Forbidden
User không có quyền truy cập endpoint này.

```json
{
  "success": false,
  "error": {
    "code": "Forbidden",
    "message": "User does not have required role"
  }
}
```


### 404 Not Found
User không tồn tại hoặc không thuộc chi nhánh nào.

```json
{
  "success": false,
  "error": {
    "code": "User.NotFound",
    "message": "User not found or does not belong to any branch"
  }
}
```


---

## 💡 Lưu ý cho Frontend

### 1. Timezone
- Tất cả datetime trong response đều theo múi giờ **Vietnam (UTC+7)**
- Khi hiển thị, không cần convert thêm

### 2. Branch Filter
- Dữ liệu **tự động lọc** theo branch của user đang đăng nhập
- Không có query parameter `branchId` - user chỉ thấy data của branch mình

### 3. Max Items
- Mỗi danh sách trả về tối đa **20 items** (trừ `pendingPayrolls` là **10 items**)
- Nếu cần xem thêm, sử dụng các API list riêng với pagination

### 4. Date Range Default
- Nếu không truyền `fromDate`/`toDate`:
  - `fromDate` = **1 tháng trước**
  - `toDate` = **1 tháng sau**

### 5. Currency
- Tất cả `amount` đều theo đơn vị **VND** (không có decimal)
- Format khi hiển thị: `5.000.000 ₫`

### 6. Status Values

#### Lead Status
| Value | Mô tả |
|-------|-------|
| `New` | Lead mới |
| `Contacted` | Đã liên hệ |
| `Qualified` | Đủ điều kiện |
| `Converted` | Đã chuyển đổi |
| `Lost` | Mất lead |

#### Enrollment Status
| Value | Mô tả |
|-------|-------|
| `Active` | Đang học |
| `Completed` | Hoàn thành |
| `Withdrawn` | Rút khỏi lớp |
| `Suspended` | Tạm dừng |

#### Class Status
| Value | Mô tả |
|-------|-------|
| `Pending` | Chờ mở |
| `Active` | Đang hoạt động |
| `Completed` | Hoàn thành |
| `Cancelled` | Đã hủy |

#### Session Status
| Value | Mô tả |
|-------|-------|
| `Scheduled` | Đã lên lịch |
| `InProgress` | Đang diễn ra |
| `Completed` | Hoàn thành |
| `Cancelled` | Đã hủy |

#### Invoice Status
| Value | Mô tả |
|-------|-------|
| `Pending` | Chờ thanh toán |
| `Paid` | Đã thanh toán |
| `Overdue` | Quá hạn |
| `Cancelled` | Đã hủy |

#### Payroll Status
| Value | Mô tả |
|-------|-------|
| `Draft` | Nháp |
| `Approved` | Đã duyệt |
| `Paid` | Đã chi trả |

#### Cashbook Entry Type
| Value | Mô tả |
|-------|-------|
| `CashIn` | Thu tiền mặt |
| `CashOut` | Chi tiền mặt |

---

## 📝 Ví dụ Code Frontend (TypeScript/React)

### Fetch API

```typescript
// types.ts
export interface StaffDashboardStatistics {
  totalLeads: number;
  totalEnrollments: number;
  totalClasses: number;
  upcomingSessions: number;
  pendingMakeupCredits: number;
  pendingLeaveRequests: number;
  pendingReports: number;
  openTickets: number;
}

export interface ManagementStaffOverviewResponse {
  statistics: StaffDashboardStatistics;
  recentLeads: LeadSummaryDto[];
  recentEnrollments: EnrollmentSummaryDto[];
  classes: ClassSummaryDto[];
  upcomingSessions: SessionSummaryDto[];
  pendingMakeupCredits: MakeupCreditSummaryDto[];
  pendingLeaveRequests: LeaveRequestSummaryDto[];
  pendingReports: ReportSummaryDto[];
  openTickets: TicketSummaryDto[];
}

// api.ts
export const fetchManagementStaffOverview = async (
  params?: {
    classId?: string;
    studentProfileId?: string;
    leadId?: string;
    enrollmentId?: string;
    fromDate?: string;
    toDate?: string;
  }
): Promise<ManagementStaffOverviewResponse> => {
  const searchParams = new URLSearchParams();
  
  if (params?.classId) searchParams.append('classId', params.classId);
  if (params?.studentProfileId) searchParams.append('studentProfileId', params.studentProfileId);
  if (params?.leadId) searchParams.append('leadId', params.leadId);
  if (params?.enrollmentId) searchParams.append('enrollmentId', params.enrollmentId);
  if (params?.fromDate) searchParams.append('fromDate', params.fromDate);
  if (params?.toDate) searchParams.append('toDate', params.toDate);

  const response = await fetch(
    `/api/me/management-staff/overview?${searchParams.toString()}`,
    {
      headers: {
        'Authorization': `Bearer ${getAccessToken()}`,
        'Content-Type': 'application/json',
      },
    }
  );

  if (!response.ok) {
    throw new Error('Failed to fetch dashboard data');
  }

  const result = await response.json();
  return result.data;
};
```


### React Hook

```typescript
// useManagementStaffOverview.ts
import { useQuery } from '@tanstack/react-query';
import { fetchManagementStaffOverview } from './api';

export const useManagementStaffOverview = (params?: {
  classId?: string;
  fromDate?: string;
  toDate?: string;
}) => {
  return useQuery({
    queryKey: ['management-staff-overview', params],
    queryFn: () => fetchManagementStaffOverview(params),
    staleTime: 5 * 60 * 1000, // 5 minutes
  });
};

// Component usage
const Dashboard = () => {
  const { data, isLoading, error } = useManagementStaffOverview();

  if (isLoading) return <Loading />;
  if (error) return <Error message={error.message} />;

  return (
    <div>
      <StatisticsCards stats={data.statistics} />
      <RecentLeadsList leads={data.recentLeads} />
      <ClassesList classes={data.classes} />
      {/* ... */}
    </div>
  );
};
```


---

## 📞 Liên hệ

Nếu có thắc mắc về API, vui lòng liên hệ team Backend qua:
- **Slack**: #kidzgo-backend
- **Email**: backend@kidzgo.vn
```
---

Đây là file markdown hoàn chỉnh. Bạn có thể copy và lưu thành file `staff-dashboard-api.md`. File này bao gồm:

1. ✅ Tổng quan 3 API
2. ✅ Chi tiết từng endpoint với query parameters
3. ✅ Response schema đầy đủ (TypeScript interfaces)
4. ✅ Ví dụ request/response
5. ✅ Bảng so sánh các API
6. ✅ Error responses
7. ✅ Lưu ý quan trọng cho Frontend
8. ✅ Giải thích các status values
9. ✅ Code mẫu TypeScript/React
```
