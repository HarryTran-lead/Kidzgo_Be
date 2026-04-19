# Media API Usage Flow

## 1. Mục tiêu

Tài liệu này mô tả flow nghiệp vụ và cách dùng API cho module Media sau các cập nhật mới ở backend:

- Phân biệt đúng media theo `OwnershipScope`: `Personal` và `Class`
- Parent/Student chỉ xem được media đã `Approved` và `Published`
- Media bị `Rejected` có `RejectReason`
- Có flow `Resubmit` để nộp lại media sau khi bị từ chối
- FE nên group album theo `monthTag`, và về mặt nghiệp vụ `1 monthTag = 1 album lớn`

Base URL chính: `/api/media`

Base URL album tổng hợp cho phụ huynh: `/api/parent/media`

## 2. Enum nghiệp vụ

### 2.1 OwnershipScope

- `Personal`: Media cá nhân, gắn với `studentProfileId`
- `Class`: Media của lớp, gắn với `classId`
- `Branch`: Media cấp chi nhánh

### 2.2 Visibility

- `ClassOnly`: Chỉ dành cho ngữ cảnh lớp
- `Personal`: Chỉ dành cho ngữ cảnh cá nhân
- `PublicParent`: Chỉ parent được xem, student không được xem

### 2.3 ApprovalStatus

- `Pending`
- `Approved`
- `Rejected`

## 3. Quy tắc đọc media

### 3.1 Teacher / ManagementStaff / Admin

- Có thể xem media theo filter truyền vào
- Không bị chặn bởi `ApprovalStatus` hay `IsPublished`

### 3.2 Parent

- Chỉ xem được media:
  - đã `Approved`
  - đã `Published`
  - thuộc học sinh đang được link hoặc lớp mà học sinh đó đang học
- Có thể xem `PublicParent`, `ClassOnly`, `Personal`
- Không xem được media `Pending` hoặc `Rejected`

### 3.3 Student

- Chỉ xem được media:
  - đã `Approved`
  - đã `Published`
  - thuộc chính mình hoặc lớp mình đang học
- Không xem được media `Pending` hoặc `Rejected`
- Không xem được media có `Visibility = PublicParent`

## 4. Flow nghiệp vụ chuẩn

### 4.1 Upload và duyệt

1. Teacher upload media bằng `POST /api/media`
2. Hệ thống tạo media với:
   - `ApprovalStatus = Pending`
   - `IsPublished = false`
3. Staff/Admin review:
   - duyệt bằng `POST /api/media/{id}/approve`
   - từ chối bằng `POST /api/media/{id}/reject`
4. Sau khi được duyệt, Staff/Admin publish bằng `POST /api/media/{id}/publish`
5. Parent/Student chỉ thấy media sau bước publish

### 4.2 Reject và nộp lại

1. Staff/Admin reject media và bắt buộc nhập `reason`
2. Media lưu:
   - `ApprovalStatus = Rejected`
   - `RejectReason = ...`
   - `IsPublished = false`
3. Teacher/Staff/Admin có thể:
   - gọi `POST /api/media/{id}/resubmit`, hoặc
   - `PUT /api/media/{id}` để sửa metadata; nếu media đang `Rejected`, backend tự reset về `Pending`
4. Sau khi resubmit, media quay lại hàng chờ duyệt

### 4.3 Album theo tháng

- FE nên xem `monthTag` là khóa album
- Mỗi `monthTag` tương ứng `1 album lớn`
- Trong cùng một `monthTag`, FE có thể tách tab/section theo:
  - `OwnershipScope`
  - `classId`
  - `studentProfileId`

## 5. API chính

### 5.1 Create Media

**Endpoint:** `POST /api/media`

**Roles:** `Teacher,ManagementStaff,Admin`

**Request body mẫu:**

```json
{
  "branchId": "11111111-1111-1111-1111-111111111111",
  "classId": "22222222-2222-2222-2222-222222222222",
  "studentProfileId": null,
  "monthTag": "2026-04",
  "type": "Photo",
  "contentType": "Album",
  "url": "https://cdn.example.com/media/photo-01.jpg",
  "mimeType": "image/jpeg",
  "fileSize": 120034,
  "originalFileName": "photo-01.jpg",
  "displayOrder": 1,
  "ownershipScope": "Class",
  "caption": "Hoạt động lớp tháng 4",
  "visibility": "ClassOnly"
}
```

**Lưu ý validate:**

- `ownershipScope = Personal` thì phải có `studentProfileId`
- `ownershipScope = Class` thì phải có `classId`

### 5.2 Get Media List

**Endpoint:** `GET /api/media`

**Roles:** `All authenticated roles`

**Query phổ biến:**

- `branchId`
- `classId`
- `studentProfileId`
- `monthTag`
- `date`
- `type`
- `contentType`
- `visibility`
- `approvalStatus`
- `isPublished`
- `pageNumber`
- `pageSize`

**Response fields quan trọng:**

- `ownershipScope`
- `approvalStatus`
- `approvedById`
- `approvedByName`
- `approvedAt`
- `rejectReason`
- `isPublished`

**Gợi ý FE:**

- Đối với teacher/staff: có thể filter `approvalStatus=Pending` để review queue
- Đối với parent/student: không cần gửi `approvalStatus`; backend tự chặn item chưa publish

### 5.3 Get Media Detail

**Endpoint:** `GET /api/media/{id}`

**Roles:** `All authenticated roles`

**Lưu ý:**

- Parent/Student nếu không đủ quyền sẽ nhận như not found
- Response có `ownershipScope` và `rejectReason`

### 5.4 Update Media

**Endpoint:** `PUT /api/media/{id}`

**Roles:** `Teacher,ManagementStaff,Admin`

**Request body mẫu:**

```json
{
  "classId": "22222222-2222-2222-2222-222222222222",
  "studentProfileId": null,
  "monthTag": "2026-04",
  "contentType": "ClassPhoto",
  "caption": "Ảnh lớp đã chỉnh",
  "visibility": "ClassOnly"
}
```

**Behavior đặc biệt:**

- Nếu media đang `Rejected`, khi update xong backend tự reset:
  - `ApprovalStatus = Pending`
  - `RejectReason = null`
  - `ApprovedById = null`
  - `ApprovedAt = null`
  - `IsPublished = false`

### 5.5 Approve Media

**Endpoint:** `POST /api/media/{id}/approve`

**Roles:** `ManagementStaff,Admin`

**Kết quả:**

- `ApprovalStatus = Approved`
- xóa `RejectReason`

### 5.6 Reject Media

**Endpoint:** `POST /api/media/{id}/reject`

**Roles:** `ManagementStaff,Admin`

**Request body:**

```json
{
  "reason": "Ảnh sai phạm vi lớp, vui lòng gắn đúng student hoặc class"
}
```

**Kết quả:**

- `ApprovalStatus = Rejected`
- lưu `RejectReason`
- `IsPublished = false`

### 5.7 Resubmit Media

**Endpoint:** `POST /api/media/{id}/resubmit`

**Roles:** `Teacher,ManagementStaff,Admin`

**Kết quả:**

- Chỉ dùng được khi media đang `Rejected`
- Backend reset:
  - `ApprovalStatus = Pending`
  - `RejectReason = null`
  - `ApprovedById = null`
  - `ApprovedAt = null`
  - `IsPublished = false`

### 5.8 Publish Media

**Endpoint:** `POST /api/media/{id}/publish`

**Roles:** `ManagementStaff,Admin`

**Điều kiện:**

- Chỉ publish được khi `ApprovalStatus = Approved`

## 6. API album tổng hợp cho Parent

### 6.1 Get Parent Media Albums

**Endpoint:** `GET /api/parent/media`

**Roles:** `Parent`

**Query hỗ trợ:**

- `studentProfileId`
- `classId`
- `monthTag`
- `date`
- `type`

**Response:**

- `albums`: danh sách album đã group
- `items`: danh sách item thô

**Nguyên tắc group hiện tại:**

- `albumId = monthTag ?? "general"`
- FE nên ưu tiên hiển thị theo `monthTag`
- Trong cùng `monthTag`, có thể group phụ theo `type` hoặc `scope`

## 7. FE flow khuyến nghị

### 7.1 Màn review cho staff/admin

1. Gọi `GET /api/media?approvalStatus=Pending&pageNumber=1&pageSize=20`
2. Review item
3. Nếu đạt:
   - `POST /api/media/{id}/approve`
   - `POST /api/media/{id}/publish`
4. Nếu không đạt:
   - `POST /api/media/{id}/reject`
   - body có `reason`

### 7.2 Màn upload cho teacher

1. Upload file lên storage/CDN
2. Gọi `POST /api/media`
3. Nếu item bị reject:
   - đọc `rejectReason`
   - cho phép sửa metadata hoặc upload lại
4. Sau khi sửa:
   - gọi `PUT /api/media/{id}` hoặc `POST /api/media/{id}/resubmit`

### 7.3 Màn album cho parent/student

1. Gọi `GET /api/media?monthTag=YYYY-MM`
2. Group theo:
   - `monthTag`
   - `ownershipScope`
3. Chỉ render item backend trả về, không cần tự lọc lại theo status publish

## 8. Các case cần nhớ

- Media `Personal` không được lẫn với media `Class`
- Media chưa duyệt hoặc chưa publish thì parent/student không được thấy
- Media `Rejected` không được hiện nội dung cho parent/student
- `PublicParent` chỉ parent thấy, student không thấy
- `monthTag` là trục album chính

