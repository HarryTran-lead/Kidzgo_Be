# Tai Lieu API FE - Registration Discount Campaign - 2026-04-28

Tai lieu nay tong hop cac API trong `RegistrationDiscountCampaignController.cs` de FE cau hinh va xem discount campaign ap dung cho registration.

Pham vi tai lieu:

- Tao discount campaign
- Xem danh sach / chi tiet discount campaign
- Cap nhat discount campaign
- Bat/tat discount campaign

## Tong quan role va pham vi du lieu

Tat ca API trong controller deu co `[Authorize]`.

| Role | Du lieu duoc xem | Pham vi du lieu | Hanh dong duoc phep |
| --- | --- | --- | --- |
| Admin | Toan bo discount campaign | `all` | `view`, `create`, `edit`, `toggle_status` |
| ManagementStaff | Toan bo discount campaign | `all` | `view` |
| Teacher | Khong duoc truy cap | `none` | `none` |
| Parent | Khong duoc truy cap | `none` | `none` |
| Student | Khong duoc truy cap | `none` | `none` |
| Anonymous | Khong duoc truy cap | `none` | `none` |

Ghi chu:

- Hien tai khong co filter `own` hay `department`; `Admin` va `ManagementStaff` dang xem theo scope `all`.
- `POST`, `PUT`, `PATCH toggle-status` chi cho `Admin`.

## Dinh dang response chung

Success tu `MatchOk()` / `MatchCreated()`:

```json
{
  "isSuccess": true,
  "data": {}
}
```

Error tu domain/validation:

```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.1",
  "title": "RegistrationDiscountCampaign.NotFound",
  "status": 404,
  "detail": "Registration discount campaign with Id = '...' was not found"
}
```

Validation pipeline co the tra them danh sach `errors`:

```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.1",
  "title": "Validation.General",
  "status": 400,
  "detail": "One or more validation errors occurred",
  "errors": [
    {
      "code": "GreaterThanValidator",
      "description": "'Discount Value' must be greater than '0'."
    }
  ]
}
```

## Danh sach API

### 1. POST `/api/registration-discount-campaigns`

Dung de tao moi mot discount campaign cho registration.

Roles: `Admin`

Body JSON:

```json
{
  "name": "Holiday 30-4",
  "code": "HOLIDAY30APR",
  "description": "Giam gia dip le 30-4",
  "branchId": "guid",
  "programId": "guid",
  "tuitionPlanId": "guid",
  "discountType": "Percentage",
  "discountValue": 10,
  "priority": 100,
  "startDate": "2026-04-28",
  "endDate": "2026-05-05",
  "applyForInitialRegistration": true,
  "applyForRenewal": true,
  "applyForUpgrade": false
}
```

Fields:

| Field | Type | Required | Mo ta |
| --- | --- | --- | --- |
| `name` | `string` | Yes | Ten campaign |
| `code` | `string?` | No | Ma campaign de FE/search hien thi |
| `description` | `string?` | No | Mo ta noi bo hoac hien thi |
| `branchId` | `Guid?` | No | Scope theo chi nhanh; null = tat ca branch |
| `programId` | `Guid?` | No | Scope theo program; null = tat ca program |
| `tuitionPlanId` | `Guid?` | No | Scope theo tuition plan; null = tat ca tuition plan |
| `discountType` | `string` | Yes | `Percentage` hoac `FixedAmount` |
| `discountValue` | `decimal` | Yes | Gia tri giam; > 0 |
| `priority` | `int` | Yes | Do uu tien; so lon hon duoc uu tien cao hon |
| `startDate` | `DateOnly` | Yes | Ngay bat dau ap dung |
| `endDate` | `DateOnly` | Yes | Ngay ket thuc ap dung |
| `applyForInitialRegistration` | `bool` | Yes | Ap dung cho dang ky lan dau |
| `applyForRenewal` | `bool` | Yes | Ap dung cho renewal |
| `applyForUpgrade` | `bool` | Yes | Ap dung cho upgrade |

Response success:

```json
{
  "isSuccess": true,
  "data": {
    "id": "guid",
    "name": "Holiday 30-4",
    "code": "HOLIDAY30APR",
    "description": "Giam gia dip le 30-4",
    "branchId": "guid",
    "branchName": "HCM",
    "programId": "guid",
    "programName": "Apple",
    "tuitionPlanId": "guid",
    "tuitionPlanName": "Goi 48 buoi",
    "discountType": "Percentage",
    "discountValue": 10,
    "priority": 100,
    "startDate": "2026-04-28",
    "endDate": "2026-05-05",
    "applyForInitialRegistration": true,
    "applyForRenewal": true,
    "applyForUpgrade": false,
    "isActive": true,
    "isCurrentlyApplicable": true,
    "createdAt": "2026-04-28T09:00:00Z",
    "updatedAt": "2026-04-28T09:00:00Z"
  }
}
```

Response loi hay gap:

| HTTP | Code/message | Khi nao |
| --- | --- | --- |
| 400 | Validation pipeline | Thieu field bat buoc, `name` rong, `discountValue <= 0`, `priority < 0` |
| 400 | `RegistrationDiscountCampaign.InvalidDateRange` | `endDate < startDate` |
| 400 | `RegistrationDiscountCampaign.InvalidPercentageDiscountValue` | `discountType = Percentage` nhung `discountValue > 100` |
| 400 | `RegistrationDiscountCampaign.MissingApplicability` | Ca 3 co `applyForInitialRegistration`, `applyForRenewal`, `applyForUpgrade` deu `false` |
| 404 | `RegistrationDiscountCampaign.BranchNotFound` | `branchId` khong ton tai |
| 404 | `RegistrationDiscountCampaign.ProgramNotFound` | `programId` khong ton tai hoac program da bi delete |
| 404 | `RegistrationDiscountCampaign.TuitionPlanNotFound` | `tuitionPlanId` khong ton tai hoac tuition plan da bi delete |
| 400 | `RegistrationDiscountCampaign.TuitionPlanProgramMismatch` | `tuitionPlanId` khong thuoc `programId` da chon |
| 400 | `RegistrationDiscountCampaign.TuitionPlanBranchMismatch` | `tuitionPlanId` khong thuoc `branchId` da chon khi tuition plan co branch scope |
| 400 | `RegistrationDiscountCampaign.FixedAmountExceedsTuitionPlanAmount` | `discountType = FixedAmount` va so tien giam lon hon hoc phi cua tuition plan scope |
| 401 | Unauthorized | Chua dang nhap |
| 403 | Forbidden | Khong phai `Admin` |

### 2. GET `/api/registration-discount-campaigns`

Dung de lay danh sach discount campaign.

Roles: `Admin`, `ManagementStaff`

Query params:

| Field | Type | Required | Default | Mo ta |
| --- | --- | --- | --- | --- |
| `branchId` | `Guid?` | No | null | Loc theo branch scope |
| `programId` | `Guid?` | No | null | Loc theo program scope |
| `tuitionPlanId` | `Guid?` | No | null | Loc theo tuition plan scope |
| `isActive` | `bool?` | No | null | Loc campaign dang bat/tat |
| `searchTerm` | `string?` | No | null | Search theo `name` hoac `code` |
| `pageNumber` | `int` | No | 1 | Trang |
| `pageSize` | `int` | No | 10 | So item/trang |

Response success:

```json
{
  "isSuccess": true,
  "data": {
    "campaigns": {
      "items": [
        {
          "id": "guid",
          "name": "Holiday 30-4",
          "code": "HOLIDAY30APR",
          "description": "Giam gia dip le 30-4",
          "branchId": "guid",
          "branchName": "HCM",
          "programId": "guid",
          "programName": "Apple",
          "tuitionPlanId": "guid",
          "tuitionPlanName": "Goi 48 buoi",
          "discountType": "Percentage",
          "discountValue": 10,
          "priority": 100,
          "startDate": "2026-04-28",
          "endDate": "2026-05-05",
          "applyForInitialRegistration": true,
          "applyForRenewal": true,
          "applyForUpgrade": false,
          "isActive": true,
          "isCurrentlyApplicable": true,
          "createdAt": "2026-04-28T09:00:00Z",
          "updatedAt": "2026-04-28T09:00:00Z"
        }
      ],
      "pageNumber": 1,
      "totalPages": 1,
      "totalCount": 1
    }
  }
}
```

Response loi:

- 401 Unauthorized
- 403 Forbidden

Ghi chu:

- Ket qua duoc sort theo `priority` giam dan, sau do `startDate` moi hon, sau do `createdAt` moi hon.
- `isCurrentlyApplicable = true` khi `isActive = true` va ngay hien tai nam trong `[startDate, endDate]`.

### 3. GET `/api/registration-discount-campaigns/{id}`

Dung de lay chi tiet mot discount campaign.

Roles: `Admin`, `ManagementStaff`

Path params:

| Field | Type | Required | Mo ta |
| --- | --- | --- | --- |
| `id` | `Guid` | Yes | Discount campaign id |

Response success:

```json
{
  "isSuccess": true,
  "data": {
    "id": "guid",
    "name": "Holiday 30-4",
    "code": "HOLIDAY30APR",
    "description": "Giam gia dip le 30-4",
    "branchId": "guid",
    "branchName": "HCM",
    "programId": "guid",
    "programName": "Apple",
    "tuitionPlanId": "guid",
    "tuitionPlanName": "Goi 48 buoi",
    "discountType": "Percentage",
    "discountValue": 10,
    "priority": 100,
    "startDate": "2026-04-28",
    "endDate": "2026-05-05",
    "applyForInitialRegistration": true,
    "applyForRenewal": true,
    "applyForUpgrade": false,
    "isActive": true,
    "isCurrentlyApplicable": true,
    "createdAt": "2026-04-28T09:00:00Z",
    "updatedAt": "2026-04-28T09:00:00Z"
  }
}
```

Response loi:

| HTTP | Code/message | Khi nao |
| --- | --- | --- |
| 404 | `RegistrationDiscountCampaign.NotFound` | Khong tim thay campaign |
| 401 | Unauthorized | Chua dang nhap |
| 403 | Forbidden | Role khong hop le |

### 4. PUT `/api/registration-discount-campaigns/{id}`

Dung de cap nhat noi dung discount campaign.

Roles: `Admin`

Body JSON:

```json
{
  "name": "Holiday 30-4 Extended",
  "code": "HOLIDAY30APR",
  "description": "Gia han campaign dip le",
  "branchId": "guid",
  "programId": "guid",
  "tuitionPlanId": "guid",
  "discountType": "FixedAmount",
  "discountValue": 1000000,
  "priority": 120,
  "startDate": "2026-04-28",
  "endDate": "2026-05-10",
  "applyForInitialRegistration": true,
  "applyForRenewal": true,
  "applyForUpgrade": true
}
```

Response success:

```json
{
  "isSuccess": true,
  "data": {
    "id": "guid",
    "name": "Holiday 30-4 Extended",
    "code": "HOLIDAY30APR",
    "description": "Gia han campaign dip le",
    "branchId": "guid",
    "branchName": "HCM",
    "programId": "guid",
    "programName": "Apple",
    "tuitionPlanId": "guid",
    "tuitionPlanName": "Goi 48 buoi",
    "discountType": "FixedAmount",
    "discountValue": 1000000,
    "priority": 120,
    "startDate": "2026-04-28",
    "endDate": "2026-05-10",
    "applyForInitialRegistration": true,
    "applyForRenewal": true,
    "applyForUpgrade": true,
    "isActive": true,
    "isCurrentlyApplicable": true,
    "createdAt": "2026-04-28T09:00:00Z",
    "updatedAt": "2026-04-28T10:30:00Z"
  }
}
```

Response loi hay gap:

| HTTP | Code/message | Khi nao |
| --- | --- | --- |
| 404 | `RegistrationDiscountCampaign.NotFound` | Campaign khong ton tai |
| 400 | Validation pipeline | Request body sai format/rule validator |
| 400 | `RegistrationDiscountCampaign.InvalidDateRange` | `endDate < startDate` |
| 400 | `RegistrationDiscountCampaign.InvalidPercentageDiscountValue` | `% > 100` |
| 400 | `RegistrationDiscountCampaign.MissingApplicability` | Chua bat ky scope ap dung nao |
| 404 | `RegistrationDiscountCampaign.BranchNotFound` / `ProgramNotFound` / `TuitionPlanNotFound` | Scope khong hop le |
| 400 | `RegistrationDiscountCampaign.TuitionPlanProgramMismatch` / `TuitionPlanBranchMismatch` | Scope khong khop |
| 400 | `RegistrationDiscountCampaign.FixedAmountExceedsTuitionPlanAmount` | Fixed amount vuot hoc phi tuition plan scope |
| 401 | Unauthorized | Chua dang nhap |
| 403 | Forbidden | Khong phai `Admin` |

### 5. PATCH `/api/registration-discount-campaigns/{id}/toggle-status`

Dung de bat/tat campaign.

Roles: `Admin`

Path params:

| Field | Type | Required | Mo ta |
| --- | --- | --- | --- |
| `id` | `Guid` | Yes | Discount campaign id |

Response success:

```json
{
  "isSuccess": true,
  "data": {
    "id": "guid",
    "isActive": false,
    "updatedAt": "2026-04-28T11:00:00Z"
  }
}
```

Response loi:

| HTTP | Code/message | Khi nao |
| --- | --- | --- |
| 404 | `RegistrationDiscountCampaign.NotFound` | Khong tim thay campaign |
| 401 | Unauthorized | Chua dang nhap |
| 403 | Forbidden | Khong phai `Admin` |

## Status definition

### DiscountType

| Value | Y nghia |
| --- | --- |
| `Percentage` | Giam theo phan tram tren hoc phi goc |
| `FixedAmount` | Giam theo so tien co dinh |

### Campaign status fields

| Field | Y nghia |
| --- | --- |
| `isActive = true` | Campaign dang bat, co the duoc xet de ap dung |
| `isActive = false` | Campaign dang tat, khong duoc ap dung |
| `isCurrentlyApplicable = true` | Campaign dang bat va ngay hien tai nam trong khoang `startDate` den `endDate` |
| `isCurrentlyApplicable = false` | Campaign dang tat hoac chua/toi han theo ngay |

## Luong chuyen trang thai

Luong don gian:

1. Admin tao campaign -> `isActive = true`
2. Den truoc `startDate` -> van co the `isActive = true` nhung `isCurrentlyApplicable = false`
3. Trong khoang ngay ap dung -> `isCurrentlyApplicable = true` neu `isActive = true`
4. Admin goi `toggle-status` -> doi `isActive`
5. Het `endDate` -> `isCurrentlyApplicable = false`, nhung `isActive` khong tu dong doi

Ghi chu:

- He thong hien tai khong co enum trang thai rieng cho campaign.
- Tinh "ap dung duoc ngay luc nay hay khong" la field derived `isCurrentlyApplicable`.

## Permission matrix theo role

| API | Admin | ManagementStaff | Teacher | Parent | Student | Anonymous |
| --- | --- | --- | --- | --- | --- | --- |
| `POST /api/registration-discount-campaigns` | Yes | No | No | No | No | No |
| `GET /api/registration-discount-campaigns` | Yes | Yes | No | No | No | No |
| `GET /api/registration-discount-campaigns/{id}` | Yes | Yes | No | No | No | No |
| `PUT /api/registration-discount-campaigns/{id}` | Yes | No | No | No | No | No |
| `PATCH /api/registration-discount-campaigns/{id}/toggle-status` | Yes | No | No | No | No | No |

## Validation rule tong hop

| Rule | API ap dung | Ket qua khi sai |
| --- | --- | --- |
| User phai dang nhap | Tat ca | 401 |
| Role phai dung theo endpoint | Tat ca | 403 |
| `name` bat buoc, khong rong, toi da 200 ky tu | Create, update | 400 validation pipeline |
| `code` toi da 100 ky tu | Create, update | 400 validation pipeline |
| `description` toi da 2000 ky tu | Create, update | 400 validation pipeline |
| `discountType` phai la enum hop le | Create, update | 400 validation pipeline |
| `discountValue > 0` | Create, update | 400 validation pipeline / `RegistrationDiscountCampaign.InvalidDiscountValue` |
| Neu `discountType = Percentage` thi `discountValue <= 100` | Create, update | 400 validation pipeline / `RegistrationDiscountCampaign.InvalidPercentageDiscountValue` |
| `priority >= 0` | Create, update | 400 validation pipeline |
| `endDate >= startDate` | Create, update | 400 `RegistrationDiscountCampaign.InvalidDateRange` |
| It nhat mot trong 3 flag apply phai bat | Create, update | 400 `RegistrationDiscountCampaign.MissingApplicability` |
| `branchId`, `programId`, `tuitionPlanId` neu gui len phai ton tai | Create, update | 404 `BranchNotFound` / `ProgramNotFound` / `TuitionPlanNotFound` |
| Neu co `programId` + `tuitionPlanId` thi tuition plan phai thuoc program do | Create, update | 400 `RegistrationDiscountCampaign.TuitionPlanProgramMismatch` |
| Neu co `branchId` + branch-scoped `tuitionPlanId` thi tuition plan phai thuoc branch do | Create, update | 400 `RegistrationDiscountCampaign.TuitionPlanBranchMismatch` |
| Neu `discountType = FixedAmount` va campaign scope den 1 tuition plan cu the thi so tien giam khong duoc vuot hoc phi cua tuition plan do | Create, update | 400 `RegistrationDiscountCampaign.FixedAmountExceedsTuitionPlanAmount` |

## Luu y FE quan trong

- Scope `branchId`, `programId`, `tuitionPlanId` deu la optional; bo trong nghia la campaign co scope rong hon.
- Khi co nhieu campaign cung match, registration se uu tien campaign co `priority` cao hon.
- Neu campaign `FixedAmount` khong scope den mot tuition plan cu the, backend khong validate voi 1 hoc phi co dinh o luc tao campaign; khi ap dung thuc te, so tien giam van bi cap khong vuot hoc phi goc cua registration.
- Bat/tat campaign khong lam thay doi cac registration da duoc snapshot discount truoc do.
