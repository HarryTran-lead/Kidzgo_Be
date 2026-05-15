# Media API Usage (FE)

## 1) Base Info
- Base path: `/api/media`
- Auth: `Bearer access_token` (bắt buộc cho tất cả endpoint)
- Response success wrapper:
```json
{
  "isSuccess": true,
  "data": {}
}
```

## 2) Role Permission Matrix

| Endpoint | Teacher | ManagementStaff | Admin | Parent |
|---|---|---|---|---|
| `POST /api/media` | Yes | Yes | No | No |
| `GET /api/media` | Yes | Yes | Yes | Yes |
| `GET /api/media/{id}` | Yes | Yes | Yes | Yes |
| `PUT /api/media/{id}` | Yes | Yes | No | No |
| `DELETE /api/media/{id}` | Yes | Yes | No | No |
| `POST /api/media/{id}/approve` | No | No | Yes | No |
| `POST /api/media/{id}/reject` | No | No | Yes | No |
| `POST /api/media/{id}/resubmit` | Yes | Yes | No | No |
| `POST /api/media/{id}/publish` | No | No | Yes | No |

Lưu ý business:
- Parent chỉ thấy media đã `Approved` và `IsPublished = true`.
- Media `Pending`/`Rejected` không hiển thị cho parent.

## 3) Enum Values

### `MediaType`
- `Photo`
- `Video`
- `Document`

### `MediaContentType`
- `Homework`
- `Report`
- `Test`
- `Album`
- `ClassPhoto`

### `MediaOwnershipScope`
- `Personal`
- `Class`
- `Branch`

### `Visibility`
- `ClassOnly`
- `Personal`
- `PublicParent`

### `ApprovalStatus`
- `Pending`
- `Approved`
- `Rejected`

## 4) API Details

## 4.1 Create Media
- Method: `POST /api/media`
- Roles: `Teacher, ManagementStaff`

Request body:
```json
{
  "branchId": "11111111-1111-1111-1111-111111111111",
  "classId": "22222222-2222-2222-2222-222222222222",
  "studentProfileId": "33333333-3333-3333-3333-333333333333",
  "monthTag": "2026-05",
  "type": "Photo",
  "contentType": "Album",
  "url": "https://cdn.example.com/media/a.jpg",
  "mimeType": "image/jpeg",
  "fileSize": 120304,
  "originalFileName": "a.jpg",
  "displayOrder": 1,
  "ownershipScope": "Class",
  "caption": "Class activity",
  "visibility": "PublicParent"
}
```

Success (`201 Created`):
```json
{
  "isSuccess": true,
  "data": {
    "id": "aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa",
    "uploaderId": "bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb",
    "uploaderName": "Nguyen Van A",
    "branchId": "11111111-1111-1111-1111-111111111111",
    "branchName": "Main Branch",
    "classId": "22222222-2222-2222-2222-222222222222",
    "className": "Starters A",
    "studentProfileId": "33333333-3333-3333-3333-333333333333",
    "studentName": "Student A",
    "monthTag": "2026-05",
    "ownershipScope": "Class",
    "type": "Photo",
    "contentType": "Album",
    "url": "https://cdn.example.com/media/a.jpg",
    "caption": "Class activity",
    "visibility": "PublicParent",
    "approvalStatus": "Pending",
    "isPublished": false,
    "createdAt": "2026-05-14T10:00:00Z"
  }
}
```

## 4.2 Get Media List
- Method: `GET /api/media`
- Roles: `Teacher, ManagementStaff, Admin, Parent`

Query params:
- `branchId`, `classId`, `studentProfileId`
- `monthTag` (`YYYY-MM`)
- `date` (ISO date)
- `type`, `contentType`, `visibility`, `approvalStatus`
- `isPublished`
- `pageNumber` (default `1`)
- `pageSize` (default `20`)

Success (`200 OK`):
```json
{
  "isSuccess": true,
  "data": {
    "media": {
      "items": [
        {
          "id": "aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa",
          "uploaderId": "bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb",
          "uploaderName": "Nguyen Van A",
          "branchId": "11111111-1111-1111-1111-111111111111",
          "branchName": "Main Branch",
          "classId": "22222222-2222-2222-2222-222222222222",
          "className": "Starters A",
          "studentProfileId": "33333333-3333-3333-3333-333333333333",
          "studentName": "Student A",
          "monthTag": "2026-05",
          "ownershipScope": "Class",
          "type": "Photo",
          "contentType": "Album",
          "url": "https://cdn.example.com/media/a.jpg",
          "caption": "Class activity",
          "visibility": "PublicParent",
          "approvalStatus": "Approved",
          "approvedById": "cccccccc-cccc-cccc-cccc-cccccccccccc",
          "approvedByName": "Admin A",
          "approvedAt": "2026-05-14T11:00:00Z",
          "rejectReason": null,
          "isPublished": true,
          "createdAt": "2026-05-14T10:00:00Z",
          "updatedAt": "2026-05-14T11:30:00Z"
        }
      ],
      "pageNumber": 1,
      "totalPages": 3,
      "totalCount": 55
    }
  }
}
```

## 4.3 Get Media Detail
- Method: `GET /api/media/{id}`
- Roles: `Teacher, ManagementStaff, Admin, Parent`
- Response shape: giống item trong list.

## 4.4 Update Media
- Method: `PUT /api/media/{id}`
- Roles: `Teacher, ManagementStaff`

Request body:
```json
{
  "classId": "22222222-2222-2222-2222-222222222222",
  "studentProfileId": "33333333-3333-3333-3333-333333333333",
  "monthTag": "2026-05",
  "contentType": "Album",
  "caption": "Updated caption",
  "visibility": "ClassOnly"
}
```

Lưu ý:
- Nếu media đang `Rejected`, sau update backend tự chuyển về `Pending` để gửi duyệt lại.

## 4.5 Delete Media
- Method: `DELETE /api/media/{id}`
- Roles: `Teacher, ManagementStaff`
- Success: `200 OK` với `{ "isSuccess": true, "data": null }`

## 4.6 Approve Media
- Method: `POST /api/media/{id}/approve`
- Roles: `Admin`

Success:
```json
{
  "isSuccess": true,
  "data": {
    "id": "aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa",
    "approvalStatus": "Approved",
    "approvedById": "cccccccc-cccc-cccc-cccc-cccccccccccc",
    "approvedByName": "Admin A",
    "approvedAt": "2026-05-14T11:00:00Z"
  }
}
```

## 4.7 Reject Media
- Method: `POST /api/media/{id}/reject`
- Roles: `Admin`

Request body:
```json
{
  "reason": "Ảnh mờ, vui lòng upload lại."
}
```

Success:
```json
{
  "isSuccess": true,
  "data": {
    "id": "aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa",
    "approvalStatus": "Rejected",
    "rejectReason": "Ảnh mờ, vui lòng upload lại.",
    "updatedAt": "2026-05-14T11:10:00Z"
  }
}
```

## 4.8 Resubmit Media
- Method: `POST /api/media/{id}/resubmit`
- Roles: `Teacher, ManagementStaff`

Success:
```json
{
  "isSuccess": true,
  "data": {
    "id": "aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa",
    "approvalStatus": "Pending",
    "isPublished": false,
    "updatedAt": "2026-05-14T11:20:00Z"
  }
}
```

## 4.9 Publish Media
- Method: `POST /api/media/{id}/publish`
- Roles: `Admin`
- Yêu cầu business: media phải `Approved` trước khi publish.

## 5) Error Response Format (FE parse)

Backend trả lỗi theo `ProblemDetails` (đa số case):
```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.4",
  "title": "Media.NotFound",
  "status": 404,
  "detail": "Media with Id = '...' was not found",
  "errors": [
    {
      "code": "Media.NotFound",
      "description": "Media with Id = '...' was not found"
    }
  ]
}
```

## 6) Media Error Codes thường gặp

| HTTP | Code | Description |
|---|---|---|
| 400 | `Media.RejectReasonRequired` | Reject reason is required |
| 400 | Validation errors | Ví dụ: `BranchId is required`, `Url is required`, `MonthTag must be in YYYY-MM format` |
| 404 | `Media.NotFound` | Media not found |
| 404 | `Media.BranchNotFound` | Branch not found or inactive |
| 404 | `Media.ClassNotFound` | Class not found |
| 404 | `Media.StudentNotFound` | Student profile not found or is not a student |
| 409 | `Media.AlreadyDeleted` | Media is already deleted |
| 409 | `Media.AlreadyApproved` | Media is already approved |
| 409 | `Media.AlreadyRejected` | Media is already rejected |
| 409 | `Media.AlreadyPublished` | Media is already published |
| 409 | `Media.NotApproved` | Media must be approved before publishing |
| 409 | `Media.NotRejected` | Media must be rejected before resubmitting |

## 7) FE Integration Notes
- FE nên hide nút action theo role:
  - Teacher/ManagementStaff: Create, Update, Delete, Resubmit
  - Admin: Approve, Reject, Publish
  - Parent: chỉ view
- Với Parent UI, nếu gọi list/detail mà không thấy item, coi như item không đủ điều kiện hiển thị (chưa approved/published hoặc không thuộc quyền xem).
