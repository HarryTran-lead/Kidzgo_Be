# FAQ API FE Full Doc - 2026-04-23

Tai lieu nay mo ta day du cac API trong `FaqController.cs` de FE tich hop man hinh public FAQ va man hinh admin/management.

## 1. Tong quan role, scope va action

| Role | Duoc xem du lieu gi | Pham vi du lieu | Hanh dong duoc phep |
| --- | --- | --- | --- |
| Admin | Toan bo FAQ category va FAQ item | `all` | `view`, `create`, `edit`, `delete` |
| ManagementStaff | Toan bo FAQ category va FAQ item | `all` | `view`, `create`, `edit`, `delete` |
| Teacher | Khong duoc truy cap | `none` | `none` |
| Parent | FAQ public da publish | `public` | `view` |
| Student | FAQ public da publish | `public` | `view` |
| Anonymous | FAQ public da publish | `public` | `view` |

Ghi chu:

- Hien tai khong co `own` scope hay `department` scope.
- Public chi thay FAQ item `isPublished = true`, `isDeleted = false`, category `isActive = true`, `isDeleted = false`.
- Admin/ManagementStaff thay duoc du lieu `all`, co filter `includeDeleted`, `includeInactive`, `isPublished`.

## 2. Common response format

### Success

```json
{
  "isSuccess": true,
  "data": {}
}
```

### Error business

```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.8",
  "title": "FaqCategory.HasFaqItems",
  "status": 409,
  "detail": "Cannot delete FAQ category while it still contains FAQ items"
}
```

### Error validation

```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.1",
  "title": "Validation.General",
  "status": 400,
  "detail": "One or more validation errors occurred",
  "errors": [
    {
      "code": "MaximumLengthValidator",
      "description": "Question must not exceed 500 characters"
    }
  ]
}
```

## 3. Danh sach API

| Method | Endpoint | Role | Mo ta |
| --- | --- | --- | --- |
| GET | `/api/faqs/categories` | Anonymous | Lay danh sach category public |
| GET | `/api/faqs` | Anonymous | Lay danh sach FAQ public |
| GET | `/api/faqs/admin/categories` | Admin, ManagementStaff | Lay danh sach category cho admin |
| POST | `/api/faqs/categories` | Admin, ManagementStaff | Tao category |
| PUT | `/api/faqs/categories/{id}` | Admin, ManagementStaff | Cap nhat category |
| DELETE | `/api/faqs/categories/{id}` | Admin, ManagementStaff | Soft delete category |
| GET | `/api/faqs/admin/items` | Admin, ManagementStaff | Lay danh sach FAQ item cho admin |
| POST | `/api/faqs` | Admin, ManagementStaff | Tao FAQ item |
| PUT | `/api/faqs/{id}` | Admin, ManagementStaff | Cap nhat FAQ item |
| DELETE | `/api/faqs/{id}` | Admin, ManagementStaff | Soft delete FAQ item |

## 4. API detail

### 4.1 GET `/api/faqs/categories`

Dung de lay category public cho trang FAQ.

Roles:

- `Anonymous`

Behavior:

- Chi tra category `isActive = true`
- Chi tra category `isDeleted = false`
- Category phai co it nhat 1 FAQ item `isPublished = true` va `isDeleted = false`

Success response:

```json
{
  "isSuccess": true,
  "data": {
    "categories": [
      {
        "id": "guid",
        "name": "Hoc phi va thanh toan",
        "icon": "credit-card",
        "sortOrder": 1,
        "isActive": true,
        "isDeleted": false,
        "totalFaqCount": 4,
        "publishedFaqCount": 4
      }
    ]
  }
}
```

Error responses:

- Hien tai khong co auth error vi endpoint public.

### 4.2 GET `/api/faqs`

Dung de lay FAQ public co phan trang.

Roles:

- `Anonymous`

Query params:

| Field | Type | Required | Default | Mo ta |
| --- | --- | --- | --- | --- |
| `categoryId` | `Guid?` | No | `null` | Loc theo category |
| `searchTerm` | `string?` | No | `null` | Search theo question, answer, category name |
| `pageNumber` | `int` | No | `1` | |
| `pageSize` | `int` | No | `10` | |

Success response:

```json
{
  "isSuccess": true,
  "data": {
    "faqs": {
      "items": [
        {
          "id": "guid",
          "categoryId": "guid",
          "categoryName": "Hoc phi va thanh toan",
          "categoryIcon": "credit-card",
          "categorySortOrder": 1,
          "question": "Hoc phi co duoc hoan lai khong?",
          "answer": "Hien tai trung tam khong ap dung hoan phi.",
          "sortOrder": 1,
          "isPublished": true,
          "isDeleted": false,
          "publishedAt": "2026-04-23T07:00:00Z",
          "createdAt": "2026-04-23T06:00:00Z",
          "updatedAt": "2026-04-23T07:00:00Z"
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

Behavior public:

- Chi tra FAQ `isPublished = true`
- Chi tra FAQ `isDeleted = false`
- Category cua FAQ phai `isActive = true`, `isDeleted = false`

### 4.3 GET `/api/faqs/admin/categories`

Dung cho man hinh admin category list.

Roles:

- `Admin`
- `ManagementStaff`

Query params:

| Field | Type | Required | Default |
| --- | --- | --- | --- |
| `includeInactive` | `bool` | No | `true` |
| `includeDeleted` | `bool` | No | `false` |

Success response:

- Cung shape voi API public category, nhung co the tra category inactive/deleted tuy theo filter.

Error responses:

| HTTP | Code | Khi nao |
| --- | --- | --- |
| 401 | Unauthorized | Chua dang nhap |
| 403 | Forbidden | Role khong phu hop |

### 4.4 POST `/api/faqs/categories`

Dung de tao FAQ category.

Roles:

- `Admin`
- `ManagementStaff`

Body:

| Field | Type | Required | Rule |
| --- | --- | --- | --- |
| `name` | `string` | Yes | Khong rong, max `200` |
| `icon` | `string?` | No | Max `100` |
| `sortOrder` | `int` | Yes | `>= 0` |
| `isActive` | `bool` | No | Mac dinh `true` |

Success response:

```json
{
  "isSuccess": true,
  "data": {
    "id": "guid",
    "name": "Lich hoc va lop",
    "icon": "calendar",
    "sortOrder": 2,
    "isActive": true,
    "isDeleted": false,
    "createdAt": "2026-04-23T07:00:00Z",
    "updatedAt": "2026-04-23T07:00:00Z"
  }
}
```

Error responses:

| HTTP | Code | Khi nao |
| --- | --- | --- |
| 400 | `Validation.General` | Sai validator |
| 409 | `FaqCategory.NameAlreadyExists` | Trung ten category chua bi delete |
| 401 | Unauthorized | Chua dang nhap |
| 403 | Forbidden | Role khong phu hop |

### 4.5 PUT `/api/faqs/categories/{id}`

Dung de cap nhat category.

Roles:

- `Admin`
- `ManagementStaff`

Path params:

| Field | Type | Required |
| --- | --- | --- |
| `id` | `Guid` | Yes |

Body:

- Giong create category.

Success response:

```json
{
  "isSuccess": true,
  "data": {
    "id": "guid",
    "name": "Lich hoc va lop",
    "icon": "calendar",
    "sortOrder": 2,
    "isActive": false,
    "isDeleted": false,
    "updatedAt": "2026-04-23T08:00:00Z"
  }
}
```

Error responses:

| HTTP | Code | Khi nao |
| --- | --- | --- |
| 400 | `Validation.General` | Sai validator |
| 404 | `FaqCategory.NotFound` | Category khong ton tai hoac da delete |
| 409 | `FaqCategory.NameAlreadyExists` | Trung ten category khac chua delete |
| 401 | Unauthorized | Chua dang nhap |
| 403 | Forbidden | Role khong phu hop |

### 4.6 DELETE `/api/faqs/categories/{id}`

Dung de soft delete category.

Roles:

- `Admin`
- `ManagementStaff`

Path params:

| Field | Type | Required |
| --- | --- | --- |
| `id` | `Guid` | Yes |

Success response:

```json
{
  "isSuccess": true,
  "data": {
    "id": "guid",
    "name": "Lich hoc va lop",
    "isDeleted": true,
    "updatedAt": "2026-04-23T08:00:00Z"
  }
}
```

Error responses:

| HTTP | Code | Khi nao |
| --- | --- | --- |
| 404 | `FaqCategory.NotFound` | Category khong ton tai |
| 409 | `FaqCategory.AlreadyDeleted` | Category da delete roi |
| 409 | `FaqCategory.HasFaqItems` | Category van con FAQ item chua delete |
| 401 | Unauthorized | Chua dang nhap |
| 403 | Forbidden | Role khong phu hop |

### 4.7 GET `/api/faqs/admin/items`

Dung cho man hinh admin list FAQ item.

Roles:

- `Admin`
- `ManagementStaff`

Query params:

| Field | Type | Required | Default | Mo ta |
| --- | --- | --- | --- | --- |
| `categoryId` | `Guid?` | No | `null` | Loc theo category |
| `searchTerm` | `string?` | No | `null` | Search question, answer, category name |
| `isPublished` | `bool?` | No | `null` | Loc draft/published |
| `includeDeleted` | `bool` | No | `false` | Co lay FAQ delete hay khong |
| `pageNumber` | `int` | No | `1` | |
| `pageSize` | `int` | No | `10` | |

Success response:

- Cung shape voi API public FAQ, nhung khong ep `publicOnly`.

### 4.8 POST `/api/faqs`

Dung de tao FAQ item.

Roles:

- `Admin`
- `ManagementStaff`

Body:

| Field | Type | Required | Rule |
| --- | --- | --- | --- |
| `categoryId` | `Guid` | Yes | Khong rong, category phai ton tai va chua delete |
| `question` | `string` | Yes | Khong rong, max `500` |
| `answer` | `string` | Yes | Khong rong, max `10000` |
| `sortOrder` | `int` | Yes | `>= 0` |
| `isPublished` | `bool` | Yes | Neu `true` thi set `publishedAt = now` |

Success response:

```json
{
  "isSuccess": true,
  "data": {
    "id": "guid",
    "categoryId": "guid",
    "categoryName": "Hoc phi va thanh toan",
    "question": "Hoc phi co duoc hoan lai khong?",
    "answer": "Hien tai trung tam khong ap dung hoan phi.",
    "sortOrder": 1,
    "isPublished": true,
    "isDeleted": false,
    "publishedAt": "2026-04-23T07:00:00Z",
    "createdAt": "2026-04-23T07:00:00Z",
    "updatedAt": "2026-04-23T07:00:00Z"
  }
}
```

Error responses:

| HTTP | Code | Khi nao |
| --- | --- | --- |
| 400 | `Validation.General` | Sai validator |
| 404 | `FaqCategory.NotFound` | Category khong ton tai hoac da delete |
| 401 | Unauthorized | Chua dang nhap |
| 403 | Forbidden | Role khong phu hop |

### 4.9 PUT `/api/faqs/{id}`

Dung de cap nhat FAQ item.

Roles:

- `Admin`
- `ManagementStaff`

Path params:

| Field | Type | Required |
| --- | --- | --- |
| `id` | `Guid` | Yes |

Body:

- Giong create FAQ.

Success response:

```json
{
  "isSuccess": true,
  "data": {
    "id": "guid",
    "categoryId": "guid",
    "categoryName": "Hoc phi va thanh toan",
    "question": "Hoc phi co duoc hoan lai khong?",
    "answer": "Khong ap dung hoan phi.",
    "sortOrder": 1,
    "isPublished": false,
    "isDeleted": false,
    "publishedAt": null,
    "updatedAt": "2026-04-23T08:00:00Z"
  }
}
```

Error responses:

| HTTP | Code | Khi nao |
| --- | --- | --- |
| 400 | `Validation.General` | Sai validator |
| 404 | `FaqItem.NotFound` | FAQ item khong ton tai hoac da delete |
| 404 | `FaqCategory.NotFound` | Category moi khong ton tai hoac da delete |
| 401 | Unauthorized | Chua dang nhap |
| 403 | Forbidden | Role khong phu hop |

### 4.10 DELETE `/api/faqs/{id}`

Dung de soft delete FAQ item.

Roles:

- `Admin`
- `ManagementStaff`

Path params:

| Field | Type | Required |
| --- | --- | --- |
| `id` | `Guid` | Yes |

Success response:

```json
{
  "isSuccess": true,
  "data": {
    "id": "guid",
    "question": "Hoc phi co duoc hoan lai khong?",
    "isDeleted": true,
    "updatedAt": "2026-04-23T08:00:00Z"
  }
}
```

Error responses:

| HTTP | Code | Khi nao |
| --- | --- | --- |
| 404 | `FaqItem.NotFound` | FAQ item khong ton tai |
| 409 | `FaqItem.AlreadyDeleted` | FAQ item da delete roi |
| 401 | Unauthorized | Chua dang nhap |
| 403 | Forbidden | Role khong phu hop |

## 5. Status definition

FAQ module khong co enum status rieng. Trang thai duoc suy ra tu flag.

### Category status

| Flag | Y nghia |
| --- | --- |
| `isActive = true`, `isDeleted = false` | Category dang hoat dong |
| `isActive = false`, `isDeleted = false` | Category dang inactive |
| `isDeleted = true` | Category da soft delete |

### FAQ item status

| Flag | Y nghia |
| --- | --- |
| `isPublished = false`, `isDeleted = false` | Draft |
| `isPublished = true`, `isDeleted = false` | Published |
| `isDeleted = true` | Deleted |
| `publishedAt != null` | FAQ da publish it nhat 1 lan |

### Luong chuyen trang thai

#### Category

1. Create -> active hoac inactive tuy request
2. Update -> co the doi active/inactive
3. Delete -> `isDeleted = true`, `isActive = false`

#### FAQ item

1. Create voi `isPublished = false` -> draft
2. Create voi `isPublished = true` -> published
3. Update draft -> published -> set `publishedAt`
4. Update published -> draft -> `publishedAt = null`
5. Delete -> `isDeleted = true`, `isPublished = false`, `publishedAt = null`

## 6. Permission matrix theo role

| Endpoint | Admin | ManagementStaff | Teacher | Parent | Student | Anonymous |
| --- | --- | --- | --- | --- | --- | --- |
| `GET /api/faqs/categories` | Yes | Yes | Yes | Yes | Yes | Yes |
| `GET /api/faqs` | Yes | Yes | Yes | Yes | Yes | Yes |
| `GET /api/faqs/admin/categories` | Yes | Yes | No | No | No | No |
| `POST /api/faqs/categories` | Yes | Yes | No | No | No | No |
| `PUT /api/faqs/categories/{id}` | Yes | Yes | No | No | No | No |
| `DELETE /api/faqs/categories/{id}` | Yes | Yes | No | No | No | No |
| `GET /api/faqs/admin/items` | Yes | Yes | No | No | No | No |
| `POST /api/faqs` | Yes | Yes | No | No | No | No |
| `PUT /api/faqs/{id}` | Yes | Yes | No | No | No | No |
| `DELETE /api/faqs/{id}` | Yes | Yes | No | No | No | No |

## 7. Validation rule tong hop

### Category

| Rule | API | Loi |
| --- | --- | --- |
| `name` khong duoc rong | create, update | 400 |
| `name` max 200 | create, update | 400 |
| `icon` max 100 | create, update | 400 |
| `sortOrder >= 0` | create, update | 400 |
| Ten category unique trong tap chua delete, khong phan biet hoa thuong | create, update | 409 `FaqCategory.NameAlreadyExists` |
| Khong duoc delete category neu con FAQ item chua delete | delete category | 409 `FaqCategory.HasFaqItems` |

### FAQ item

| Rule | API | Loi |
| --- | --- | --- |
| `categoryId` bat buoc | create, update | 400 |
| `question` khong duoc rong | create, update | 400 |
| `question` max 500 | create, update | 400 |
| `answer` khong duoc rong | create, update | 400 |
| `answer` max 10000 | create, update | 400 |
| `sortOrder >= 0` | create, update | 400 |
| Category phai ton tai va chua delete | create, update | 404 `FaqCategory.NotFound` |
| FAQ item phai ton tai va chua delete | update | 404 `FaqItem.NotFound` |

## 8. Cac truong hop tra loi can luu y

- Public category chi hien khi category co it nhat 1 FAQ published.
- Public FAQ khong bao gio tra item draft hay deleted.
- `DELETE /api/faqs/categories/{id}` chi la soft delete.
- `DELETE /api/faqs/{id}` chi la soft delete.
- Hien tai chua co API restore category/FAQ da delete.
