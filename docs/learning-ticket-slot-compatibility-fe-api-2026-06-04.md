# Doc full ve cac API yeu cau cua FE

Tai lieu nay mo ta nhung thay doi lien quan den:

- `LearningTicketTypeController`
- `SlotTypeController`
- `TicketTypeCompatibilityController`

Muc tieu la de FE build duoc man hinh van hanh cho `learning ticket type`, `slot type`, va `ticket-slot compatibility` theo huong `default rule + manual override`.

## 1. Tong quan thay doi

### 1.1 LearningTicketTypeController

Thay doi chinh:

- `LearningTicketType` khong con chi la master data `Code/Name/Description/IsActive`.
- Them `CompatibilityMode`.
- Them 4 nhom rule mac dinh:
  - `AllowedDayGroups`
  - `AllowedTimeBands`
  - `AllowedTeacherTypes`
  - `AllowedUsageTypes`

Y nghia:

- Day la `default matching rule` cua ticket type.
- Rule nay duoc he thong dung de tinh `compatible / incompatible` voi `SlotType` ma khong bat buoc phai co row trong bang `TicketTypeCompatibilities`.

### 1.2 SlotTypeController

Thay doi chinh:

- `SlotType` duoc bo sung metadata de he thong co the auto-match:
  - `DayGroup`
  - `TimeBand`
  - `TeacherType`
  - `UsageType`

Y nghia:

- Moi slot type la mot bo tag nghiep vu.
- He thong se so tag nay voi rule cua ticket type de ra ket qua compatibility.

### 1.3 TicketTypeCompatibilityController

Thay doi chinh:

- Van giu cac API CRUD cu cho `TicketTypeCompatibility`.
- Them API `matrix` de FE lay ket qua compatibility hieu luc cuoi cung.
- Them API `bulk upsert overrides` de FE luu override theo tung `learning ticket type`.

Y nghia:

- Bang `TicketTypeCompatibilities` bay gio dong vai tro `manual override`, khong con la nguon truth duy nhat.
- Thu tu uu tien:
  1. Neu co override row thi dung override row.
  2. Neu khong co override row thi dung `CompatibilityMode` + default rule.

## 2. Moi role duoc xem du lieu gi

### 2.1 Role co quyen

| Role | Xem Learning Ticket Type | Xem Slot Type | Xem Compatibility | Ghi chu |
|---|---|---|---|---|
| `Admin` | Co | Co | Co | Full access |
| `ManagementStaff` | Co | Co | Co | Khong duoc delete |
| Cac role khac | Khong | Khong | Khong | `401/403` |

### 2.2 Pham vi du lieu

| Module | Pham vi du lieu |
|---|---|
| Learning Ticket Type | `all` |
| Slot Type | `all` |
| Ticket Type Compatibility | `all` |

Ghi chu:

- Day la master data toan he thong.
- Khong co `own`.
- Khong co `department`.

### 2.3 Cac hanh dong duoc phep

| Role | View | Create | Edit | Delete | Xem Matrix | Bulk Override |
|---|---|---|---|---|---|---|
| `Admin` | Co | Co | Co | Co | Co | Co |
| `ManagementStaff` | Co | Co | Co | Khong | Co | Co |
| Cac role khac | Khong | Khong | Khong | Khong | Khong | Khong |

## 3. Response format chung

### 3.1 Success

`200 OK` hoac `201 Created`

```json
{
  "isSuccess": true,
  "data": {}
}
```

### 3.2 Error

`400 / 404 / 409 / 401 / 403 / 500`

```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.1",
  "title": "Some.Error.Code",
  "status": 400,
  "detail": "Human readable message",
  "errors": [
    {
      "code": "Some.Error.Code",
      "description": "Human readable message"
    }
  ]
}
```

Ghi chu:

- Validation error tra `400`.
- Not found tra `404`.
- Conflict tra `409`.
- Khong dang nhap hoac token loi tra `401`.
- Khong du role tra `403`.

## 4. Status definition

## 4.1 Danh sach status / enum can FE biet

### A. `IsActive`

| Gia tri | Y nghia |
|---|---|
| `true` | Hoat dong |
| `false` | Ngung hoat dong |

### B. `TicketCompatibilityMode`

| Gia tri | Y nghia |
|---|---|
| `AllowAll` | Ticket type mac dinh hoc duoc tat ca slot type neu khong co override |
| `RuleBased` | Ticket type duoc auto-match theo 4 nhom rule |

### C. `SlotDayGroup`

| Gia tri | Y nghia |
|---|---|
| `None` | Chua gan tag / khong xac dinh |
| `Weekday` | Ngay thuong |
| `Weekend` | Cuoi tuan |

### D. `SlotTimeBand`

| Gia tri | Y nghia |
|---|---|
| `None` | Chua gan tag / khong xac dinh |
| `Morning` | Sang |
| `Afternoon` | Chieu |
| `Evening` | Toi |

### E. `SlotTeacherType`

| Gia tri | Y nghia |
|---|---|
| `None` | Chua gan tag / khong xac dinh |
| `Standard` | Lop giao vien thuong |
| `Native` | Lop giao vien nuoc ngoai |

### F. `SlotUsageType`

| Gia tri | Y nghia |
|---|---|
| `None` | Chua gan tag / khong xac dinh |
| `Standard` | Lop thuong |
| `Makeup` | Lop bu |
| `Remedial` | Lop phu dao |
| `Review` | Lop on tap |
| `Custom` | Loai khac do van hanh tu dinh nghia |

### G. `Matrix Cell Source`

| Gia tri | Y nghia |
|---|---|
| `OverrideAllow` | Compatible do manual override = `true` |
| `OverrideDeny` | Incompatible do manual override = `false` |
| `AllowAll` | Compatible do mode `AllowAll` |
| `Rule` | Ket qua tinh tu `RuleBased` |
| `NoTicketType` | Runtime fallback khi khong co ticket type |
| `NoSlotType` | Runtime fallback khi khong co slot type |

## 4.2 Luong chuyen trang thai

Khong co workflow approve.

Chi co 2 nhom thay doi:

- Bat/tat `IsActive`
- Tao/sua/xoa `override`

Khong co state machine nghiep vu phuc tap.

## 5. Danh sach API

## 5.1 LearningTicketType APIs

### 5.1.1 `POST /api/learning-ticket-types`

- Method: `POST`
- Muc dich: Tao learning ticket type va default rule
- Role: `Admin`, `ManagementStaff`

#### Body

| Field | Type | Required | Ghi chu |
|---|---|---|---|
| `code` | `string` | Co | Unique, server se normalize uppercase |
| `name` | `string` | Co | Ten hien thi |
| `description` | `string?` | Khong | Mo ta |
| `compatibilityMode` | `TicketCompatibilityMode` | Co | `AllowAll` hoac `RuleBased` |
| `allowedDayGroups` | `SlotDayGroup[]` | Khong | Multi-select, de rong = khong gioi han theo day group |
| `allowedTimeBands` | `SlotTimeBand[]` | Khong | Multi-select, de rong = khong gioi han theo time band |
| `allowedTeacherTypes` | `SlotTeacherType[]` | Khong | Multi-select, de rong = khong gioi han theo teacher type |
| `allowedUsageTypes` | `SlotUsageType[]` | Khong | Multi-select, de rong = khong gioi han theo usage type |
| `isActive` | `bool` | Co | Trang thai hoat dong |

#### Success response

`201 Created`

```json
{
  "isSuccess": true,
  "data": {
    "id": "guid",
    "code": "WEEKEND",
    "name": "Ve hoc cuoi tuan",
    "description": "Optional",
    "compatibilityMode": "RuleBased",
    "allowedDayGroups": ["Weekend"],
    "allowedTimeBands": [],
    "allowedTeacherTypes": [],
    "allowedUsageTypes": [],
    "isActive": true,
    "createdAt": "2026-06-04T10:00:00Z",
    "updatedAt": "2026-06-04T10:00:00Z"
  }
}
```

#### Error

| HTTP | Code | Message |
|---|---|---|
| `400` | Validation | Code/name rong, qua do dai, enum khong hop le |
| `409` | `LearningTicketType.CodeExists` | Learning ticket type code da ton tai |
| `401` | Unauthorized | Chua dang nhap |
| `403` | Forbidden | Khong du role |

### 5.1.2 `GET /api/learning-ticket-types`

- Method: `GET`
- Muc dich: Lay danh sach learning ticket type
- Role: `Admin`, `ManagementStaff`

#### Query params

| Field | Type | Required | Ghi chu |
|---|---|---|---|
| `searchTerm` | `string?` | Khong | Search theo `Code` hoac `Name` |
| `isActive` | `bool?` | Khong | Loc active/inactive |

#### Success response

`200 OK`

```json
{
  "isSuccess": true,
  "data": {
    "items": [
      {
        "id": "guid",
        "code": "DEFAULT",
        "name": "Ve hoc theo goi",
        "description": null,
        "compatibilityMode": "AllowAll",
        "allowedDayGroups": [],
        "allowedTimeBands": [],
        "allowedTeacherTypes": [],
        "allowedUsageTypes": [],
        "isActive": true,
        "createdAt": "2026-06-04T10:00:00Z",
        "updatedAt": "2026-06-04T10:00:00Z"
      }
    ]
  }
}
```

#### Error

| HTTP | Code | Message |
|---|---|---|
| `401` | Unauthorized | Chua dang nhap |
| `403` | Forbidden | Khong du role |

### 5.1.3 `GET /api/learning-ticket-types/{id}`

- Method: `GET`
- Muc dich: Lay chi tiet 1 learning ticket type
- Role: `Admin`, `ManagementStaff`

#### Path params

| Field | Type | Required |
|---|---|---|
| `id` | `guid` | Co |

#### Success response

Format `data` giong object trong `POST`.

#### Error

| HTTP | Code | Message |
|---|---|---|
| `404` | `LearningTicketType.NotFound` | Khong tim thay learning ticket type |
| `401` | Unauthorized | Chua dang nhap |
| `403` | Forbidden | Khong du role |

### 5.1.4 `PUT /api/learning-ticket-types/{id}`

- Method: `PUT`
- Muc dich: Sua learning ticket type va default rule
- Role: `Admin`, `ManagementStaff`

#### Path params

| Field | Type | Required |
|---|---|---|
| `id` | `guid` | Co |

#### Body

Body giong `POST`.

#### Success response

Format `data` giong object trong `POST`.

#### Error

| HTTP | Code | Message |
|---|---|---|
| `400` | Validation | Id rong, code/name rong, enum sai |
| `404` | `LearningTicketType.NotFound` | Khong tim thay learning ticket type |
| `409` | `LearningTicketType.CodeExists` | Code bi trung |
| `401` | Unauthorized | Chua dang nhap |
| `403` | Forbidden | Khong du role |

### 5.1.5 `DELETE /api/learning-ticket-types/{id}`

- Method: `DELETE`
- Muc dich: Xoa learning ticket type
- Role: `Admin` only

#### Path params

| Field | Type | Required |
|---|---|---|
| `id` | `guid` | Co |

#### Success response

`200 OK`

```json
{
  "isSuccess": true,
  "data": null
}
```

#### Error

| HTTP | Code | Message |
|---|---|---|
| `404` | `LearningTicketType.NotFound` | Khong tim thay learning ticket type |
| `409` | `LearningTicketType.InUse` | Dang duoc dung trong tuition plan / learning ticket item / compatibility |
| `401` | Unauthorized | Chua dang nhap |
| `403` | Forbidden | Khong du role |

## 5.2 SlotType APIs

### 5.2.1 `POST /api/slot-types`

- Method: `POST`
- Muc dich: Tao slot type va metadata de auto-match
- Role: `Admin`, `ManagementStaff`

#### Body

| Field | Type | Required | Ghi chu |
|---|---|---|---|
| `code` | `string` | Co | Unique, server normalize uppercase |
| `name` | `string` | Co | Ten hien thi |
| `description` | `string?` | Khong | Mo ta |
| `dayGroup` | `SlotDayGroup` | Co | Single value |
| `timeBand` | `SlotTimeBand` | Co | Single value |
| `teacherType` | `SlotTeacherType` | Co | Single value |
| `usageType` | `SlotUsageType` | Co | Single value |
| `isActive` | `bool` | Co | Trang thai hoat dong |

#### Success response

```json
{
  "isSuccess": true,
  "data": {
    "id": "guid",
    "code": "STANDARD-WEEKEND",
    "name": "Lop thuong cuoi tuan",
    "description": null,
    "dayGroup": "Weekend",
    "timeBand": "Morning",
    "teacherType": "Standard",
    "usageType": "Standard",
    "isActive": true,
    "createdAt": "2026-06-04T10:00:00Z",
    "updatedAt": "2026-06-04T10:00:00Z"
  }
}
```

#### Error

| HTTP | Code | Message |
|---|---|---|
| `400` | Validation | Code/name rong, enum sai |
| `409` | `SlotType.CodeExists` | Slot type code da ton tai |
| `401` | Unauthorized | Chua dang nhap |
| `403` | Forbidden | Khong du role |

### 5.2.2 `GET /api/slot-types`

- Method: `GET`
- Muc dich: Lay danh sach slot type
- Role: `Admin`, `ManagementStaff`

#### Query params

| Field | Type | Required |
|---|---|---|
| `searchTerm` | `string?` | Khong |
| `isActive` | `bool?` | Khong |

#### Success response

```json
{
  "isSuccess": true,
  "data": {
    "items": [
      {
        "id": "guid",
        "code": "NATIVE-WEEKEND",
        "name": "Lop nuoc ngoai cuoi tuan",
        "description": null,
        "dayGroup": "Weekend",
        "timeBand": "Afternoon",
        "teacherType": "Native",
        "usageType": "Standard",
        "isActive": true,
        "createdAt": "2026-06-04T10:00:00Z",
        "updatedAt": "2026-06-04T10:00:00Z"
      }
    ]
  }
}
```

### 5.2.3 `GET /api/slot-types/{id}`

- Method: `GET`
- Muc dich: Lay chi tiet 1 slot type
- Role: `Admin`, `ManagementStaff`

#### Path params

| Field | Type | Required |
|---|---|---|
| `id` | `guid` | Co |

#### Error

| HTTP | Code | Message |
|---|---|---|
| `404` | `SlotType.NotFound` | Khong tim thay slot type |

### 5.2.4 `PUT /api/slot-types/{id}`

- Method: `PUT`
- Muc dich: Sua slot type va metadata
- Role: `Admin`, `ManagementStaff`

#### Path params

| Field | Type | Required |
|---|---|---|
| `id` | `guid` | Co |

#### Body

Body giong `POST`.

#### Error

| HTTP | Code | Message |
|---|---|---|
| `400` | Validation | Id rong, enum sai |
| `404` | `SlotType.NotFound` | Khong tim thay slot type |
| `409` | `SlotType.CodeExists` | Code bi trung |

### 5.2.5 `DELETE /api/slot-types/{id}`

- Method: `DELETE`
- Muc dich: Xoa slot type
- Role: `Admin` only

#### Error

| HTTP | Code | Message |
|---|---|---|
| `404` | `SlotType.NotFound` | Khong tim thay slot type |
| `409` | `SlotType.InUse` | Dang duoc dung trong class / session / compatibility |

## 5.3 TicketTypeCompatibility APIs

### 5.3.1 `POST /api/ticket-type-compatibilities`

- Method: `POST`
- Muc dich: Tao explicit override row thu cong
- Role: `Admin`, `ManagementStaff`
- Luu y: Day la API advanced/manual. FE production khong nen dung day lam luong chinh.

#### Body

| Field | Type | Required |
|---|---|---|
| `learningTicketTypeId` | `guid` | Co |
| `slotTypeId` | `guid` | Co |
| `isCompatible` | `bool` | Co |

#### Success response

```json
{
  "isSuccess": true,
  "data": {
    "id": "guid",
    "learningTicketTypeId": "guid",
    "learningTicketTypeCode": "WEEKEND",
    "slotTypeId": "guid",
    "slotTypeCode": "STANDARD-WEEKEND",
    "isCompatible": true,
    "createdAt": "2026-06-04T10:00:00Z",
    "updatedAt": "2026-06-04T10:00:00Z"
  }
}
```

#### Error

| HTTP | Code | Message |
|---|---|---|
| `400` | Validation | Id rong |
| `404` | `TicketTypeCompatibility.LearningTicketTypeNotFound` | Ticket type khong ton tai |
| `404` | `TicketTypeCompatibility.SlotTypeNotFound` | Slot type khong ton tai |
| `409` | `TicketTypeCompatibility.MappingExists` | Da co row cho cap nay |

### 5.3.2 `GET /api/ticket-type-compatibilities`

- Method: `GET`
- Muc dich: Lay danh sach row explicit trong bang `TicketTypeCompatibilities`
- Role: `Admin`, `ManagementStaff`
- Luu y: API nay KHONG tra effective compatibility tu default rule.

#### Query params

| Field | Type | Required |
|---|---|---|
| `learningTicketTypeId` | `guid?` | Khong |
| `slotTypeId` | `guid?` | Khong |

#### Success response

```json
{
  "isSuccess": true,
  "data": {
    "items": [
      {
        "id": "guid",
        "learningTicketTypeId": "guid",
        "learningTicketTypeCode": "WEEKEND",
        "slotTypeId": "guid",
        "slotTypeCode": "STANDARD-WEEKEND",
        "isCompatible": false,
        "createdAt": "2026-06-04T10:00:00Z",
        "updatedAt": "2026-06-04T10:00:00Z"
      }
    ]
  }
}
```

### 5.3.3 `GET /api/ticket-type-compatibilities/matrix`

- Method: `GET`
- Muc dich: Lay effective compatibility matrix cho FE build man van hanh
- Role: `Admin`, `ManagementStaff`
- Day la API FE nen dung lam nguon chinh cho man compatibility.

#### Query params

| Field | Type | Required | Ghi chu |
|---|---|---|---|
| `learningTicketTypeId` | `guid?` | Khong | Neu truyen thi matrix chi cho 1 ticket type |
| `onlyActive` | `bool` | Khong | Default `true` |

#### Success response

```json
{
  "isSuccess": true,
  "data": {
    "learningTicketTypes": [
      {
        "id": "guid",
        "code": "WEEKEND",
        "name": "Ve hoc cuoi tuan",
        "compatibilityMode": "RuleBased",
        "isActive": true
      }
    ],
    "slotTypes": [
      {
        "id": "guid",
        "code": "STANDARD-WEEKEND",
        "name": "Lop thuong cuoi tuan",
        "dayGroup": "Weekend",
        "timeBand": "Morning",
        "teacherType": "Standard",
        "usageType": "Standard",
        "isActive": true
      }
    ],
    "cells": [
      {
        "learningTicketTypeId": "guid",
        "slotTypeId": "guid",
        "isCompatible": true,
        "overrideValue": null,
        "source": "Rule",
        "reason": "Compatible by default rule."
      }
    ]
  }
}
```

#### Giai thich field quan trong

| Field | Y nghia |
|---|---|
| `isCompatible` | Ket qua hieu luc cuoi cung |
| `overrideValue` | `true/false` neu co override, `null` neu dang dung default logic |
| `source` | Nguon sinh ra ket qua |
| `reason` | Human-readable text de FE tooltip / debug |

### 5.3.4 `GET /api/ticket-type-compatibilities/{id}`

- Method: `GET`
- Muc dich: Lay chi tiet 1 override row explicit
- Role: `Admin`, `ManagementStaff`

#### Error

| HTTP | Code | Message |
|---|---|---|
| `404` | `TicketTypeCompatibility.NotFound` | Khong tim thay row |

### 5.3.5 `PUT /api/ticket-type-compatibilities/{id}`

- Method: `PUT`
- Muc dich: Sua 1 override row explicit
- Role: `Admin`, `ManagementStaff`
- Luu y: API advanced/manual.

#### Body

| Field | Type | Required |
|---|---|---|
| `learningTicketTypeId` | `guid` | Co |
| `slotTypeId` | `guid` | Co |
| `isCompatible` | `bool` | Co |

#### Error

| HTTP | Code | Message |
|---|---|---|
| `400` | Validation | Id rong |
| `404` | `TicketTypeCompatibility.NotFound` | Row khong ton tai |
| `404` | `TicketTypeCompatibility.LearningTicketTypeNotFound` | Ticket type khong ton tai |
| `404` | `TicketTypeCompatibility.SlotTypeNotFound` | Slot type khong ton tai |
| `409` | `TicketTypeCompatibility.MappingExists` | Trung cap `learningTicketTypeId + slotTypeId` |

### 5.3.6 `PUT /api/ticket-type-compatibilities/learning-ticket-types/{learningTicketTypeId}/overrides`

- Method: `PUT`
- Muc dich: Bulk upsert override theo 1 learning ticket type
- Role: `Admin`, `ManagementStaff`
- Day la API FE nen dung de save override.

#### Path params

| Field | Type | Required |
|---|---|---|
| `learningTicketTypeId` | `guid` | Co |

#### Body

| Field | Type | Required | Ghi chu |
|---|---|---|---|
| `items` | `array` | Co | Danh sach override muon luu |
| `items[].slotTypeId` | `guid` | Co | Slot can override |
| `items[].isCompatible` | `bool?` | Khong | `true/false` = upsert, `null` = xoa override va quay ve default logic |

#### Success response

```json
{
  "isSuccess": true,
  "data": {
    "learningTicketTypeId": "guid",
    "upsertedCount": 3,
    "removedCount": 1,
    "items": [
      {
        "id": "guid",
        "learningTicketTypeId": "guid",
        "learningTicketTypeCode": "WEEKEND",
        "slotTypeId": "guid",
        "slotTypeCode": "STANDARD-WEEKEND",
        "isCompatible": false,
        "createdAt": "2026-06-04T10:00:00Z",
        "updatedAt": "2026-06-04T10:00:00Z"
      }
    ]
  }
}
```

#### Error

| HTTP | Code | Message |
|---|---|---|
| `400` | Validation | `learningTicketTypeId` rong, duplicate `slotTypeId` trong cung request |
| `404` | `TicketTypeCompatibility.LearningTicketTypeNotFound` | Ticket type khong ton tai |
| `404` | `TicketTypeCompatibility.SlotTypeNotFound` | Mot trong cac slot type khong ton tai |

### 5.3.7 `DELETE /api/ticket-type-compatibilities/{id}`

- Method: `DELETE`
- Muc dich: Xoa 1 override row explicit
- Role: `Admin` only

#### Error

| HTTP | Code | Message |
|---|---|---|
| `404` | `TicketTypeCompatibility.NotFound` | Khong tim thay row |

## 6. Permission matrix theo role

| API | Admin | ManagementStaff | Role khac |
|---|---|---|---|
| `POST /api/learning-ticket-types` | Co | Co | Khong |
| `GET /api/learning-ticket-types` | Co | Co | Khong |
| `GET /api/learning-ticket-types/{id}` | Co | Co | Khong |
| `PUT /api/learning-ticket-types/{id}` | Co | Co | Khong |
| `DELETE /api/learning-ticket-types/{id}` | Co | Khong | Khong |
| `POST /api/slot-types` | Co | Co | Khong |
| `GET /api/slot-types` | Co | Co | Khong |
| `GET /api/slot-types/{id}` | Co | Co | Khong |
| `PUT /api/slot-types/{id}` | Co | Co | Khong |
| `DELETE /api/slot-types/{id}` | Co | Khong | Khong |
| `POST /api/ticket-type-compatibilities` | Co | Co | Khong |
| `GET /api/ticket-type-compatibilities` | Co | Co | Khong |
| `GET /api/ticket-type-compatibilities/matrix` | Co | Co | Khong |
| `GET /api/ticket-type-compatibilities/{id}` | Co | Co | Khong |
| `PUT /api/ticket-type-compatibilities/{id}` | Co | Co | Khong |
| `PUT /api/ticket-type-compatibilities/learning-ticket-types/{id}/overrides` | Co | Co | Khong |
| `DELETE /api/ticket-type-compatibilities/{id}` | Co | Khong | Khong |

## 7. Validation rule

## 7.1 LearningTicketType

### Rule kiem tra du lieu

- `code`: required, max `100`, unique, server normalize uppercase
- `name`: required, max `255`
- `description`: max `500`
- `allowedDayGroups[]`: chi nhan `Weekday`, `Weekend`; khong duoc `None`
- `allowedTimeBands[]`: chi nhan `Morning`, `Afternoon`, `Evening`; khong duoc `None`
- `allowedTeacherTypes[]`: chi nhan `Standard`, `Native`; khong duoc `None`
- `allowedUsageTypes[]`: chi nhan `Standard`, `Makeup`, `Remedial`, `Review`, `Custom`; khong duoc `None`

### Cac truong hop tra loi

- `LearningTicketType.CodeExists`
- `LearningTicketType.NotFound`
- `LearningTicketType.InUse`
- Validation 400

### Rule nghiep vu quan trong

- Mang rong `[]` tren `Allowed*` co nghia la `khong gioi han` o chieu do.
- Neu `CompatibilityMode = AllowAll` thi he thong bo qua rule mac dinh, chi con bi anh huong boi override.
- Neu `CompatibilityMode = RuleBased` va tat ca `Allowed* = []` thi ket qua thuc te se gan nhu `AllowAll`.

## 7.2 SlotType

### Rule kiem tra du lieu

- `code`: required, max `100`, unique, uppercase
- `name`: required, max `255`
- `description`: max `500`
- `dayGroup`: cho phep `None`, `Weekday`, `Weekend`
- `timeBand`: cho phep `None`, `Morning`, `Afternoon`, `Evening`
- `teacherType`: cho phep `None`, `Standard`, `Native`
- `usageType`: cho phep `None`, `Standard`, `Makeup`, `Remedial`, `Review`, `Custom`

### Cac truong hop tra loi

- `SlotType.CodeExists`
- `SlotType.NotFound`
- `SlotType.InUse`
- Validation 400

### Rule nghiep vu quan trong

- `None` tren `SlotType` co nghia la `chua gan tag / unspecified`.
- O runtime, neu 1 chieu cua slot la `None` thi chieu do se duoc xem la pass.

## 7.3 TicketTypeCompatibility

### Rule kiem tra du lieu

- `learningTicketTypeId`: required
- `slotTypeId`: required
- `PUT overrides`: khong duoc duplicate `slotTypeId` trong cung request

### Cac truong hop tra loi

- `TicketTypeCompatibility.NotFound`
- `TicketTypeCompatibility.LearningTicketTypeNotFound`
- `TicketTypeCompatibility.SlotTypeNotFound`
- `TicketTypeCompatibility.MappingExists`

### Rule nghiep vu quan trong

- `TicketTypeCompatibilities` bay gio la bang `override`.
- He thong KHONG tu dong tao row vao bang nay khi default rule match.
- `isCompatible = null` trong API bulk override = xoa override de quay ve default logic.

## 8. Luong match nghiep vu ma FE can hieu

Thu tu resolve compatibility:

1. Tim row override theo cap `learningTicketTypeId + slotTypeId`
2. Neu co override:
   - `true` => compatible
   - `false` => incompatible
3. Neu khong co override va `compatibilityMode = AllowAll` => compatible
4. Neu khong co override va `compatibilityMode = RuleBased`:
   - So `AllowedDayGroups` voi `SlotType.DayGroup`
   - So `AllowedTimeBands` voi `SlotType.TimeBand`
   - So `AllowedTeacherTypes` voi `SlotType.TeacherType`
   - So `AllowedUsageTypes` voi `SlotType.UsageType`
5. Tat ca chieu pass => compatible
6. Chi can 1 chieu fail => incompatible

## 9. Huong UI ma FE nen build

## 9.1 Nguyen tac chung

Khong khuyen build man `CRUD tung row compatibility` lam luong chinh.

Luong FE nen build:

- `Learning Ticket Type` la noi staff cau hinh `default rule`
- `Slot Type` la noi staff gan metadata cho tung slot
- `Compatibility` la man hinh xem ket qua hieu luc cuoi cung va sua `override`

## 9.2 Man hinh Learning Ticket Type

### List page

Cot goi y:

- `Code`
- `Name`
- `CompatibilityMode`
- `AllowedDayGroups`
- `AllowedTimeBands`
- `AllowedTeacherTypes`
- `AllowedUsageTypes`
- `IsActive`
- `Actions`

### Form create/edit

Field goi y:

- `Code` - input
- `Name` - input
- `Description` - textarea
- `CompatibilityMode` - radio/select
- `AllowedDayGroups` - multi select chips
- `AllowedTimeBands` - multi select chips
- `AllowedTeacherTypes` - multi select chips
- `AllowedUsageTypes` - multi select chips
- `IsActive` - toggle

UX rule:

- Neu chon `AllowAll`, FE nen disable hoac collapse 4 field `Allowed*`.
- Neu chon `RuleBased`, FE hien 4 field `Allowed*`.
- Neu `RuleBased` ma tat ca `Allowed*` dang rong, FE nen canh bao nhe:
  `RuleBased voi tat ca field rong se gan nhu AllowAll.`

## 9.3 Man hinh Slot Type

### List page

Cot goi y:

- `Code`
- `Name`
- `DayGroup`
- `TimeBand`
- `TeacherType`
- `UsageType`
- `IsActive`
- `Actions`

### Form create/edit

Field goi y:

- `Code`
- `Name`
- `Description`
- `DayGroup` - single select
- `TimeBand` - single select
- `TeacherType` - single select
- `UsageType` - single select
- `IsActive`

UX rule:

- Label `None` nen hien thanh `Khong gan tag`.

## 9.4 Man hinh Compatibility

### FE nen dung API nao lam luong chinh

- Doc matrix: `GET /api/ticket-type-compatibilities/matrix`
- Save override: `PUT /api/ticket-type-compatibilities/learning-ticket-types/{learningTicketTypeId}/overrides`

### Khong nen dung lam luong chinh

- `POST /api/ticket-type-compatibilities`
- `PUT /api/ticket-type-compatibilities/{id}`
- `DELETE /api/ticket-type-compatibilities/{id}`

Chi nen de cho:

- advanced admin tool
- debug
- backward compatibility

### Layout UI de xuat

#### Cach 1 - Khuyen nghi

`1 learning ticket type / 1 page hoac drawer`

Ben trai:

- danh sach learning ticket type
- search/filter

Ben phai:

- thong tin ticket type
- default rule
- bang danh sach slot type va ket qua effective compatibility

Cot trong bang:

- `SlotTypeCode`
- `SlotTypeName`
- `DayGroup`
- `TimeBand`
- `TeacherType`
- `UsageType`
- `EffectiveResult`
- `Source`
- `Override`

Hanh dong moi dong:

- `Follow Default`
- `Force Allow`
- `Force Deny`

Mapping save:

- `Follow Default` => `isCompatible = null`
- `Force Allow` => `isCompatible = true`
- `Force Deny` => `isCompatible = false`

#### Cach 2 - Full matrix

Chi nen dung neu so slot type va ticket type it.

- Hang = `LearningTicketType`
- Cot = `SlotType`
- Click vao cell de doi override

Khong khuyen lam cach 2 neu slot type se tiep tuc no ra `morning/evening/...`

### Hien thi result

Goi y color:

- `Compatible` => xanh
- `Incompatible` => do
- `Follow default` => badge neutral
- `OverrideAllow` => xanh dam + icon manual
- `OverrideDeny` => do dam + icon manual
- `AllowAll` => badge `Default mode`
- `Rule` => badge `Rule matched`

### Tooltip / debug info

FE nen show `reason` tu matrix response trong tooltip cua cell:

- `Compatible by default rule.`
- `Blocked by manual override.`
- `Teacher type 'Native' is not allowed.`

Dieu nay rat co ich cho van hanh va QA.

## 10. Khuyen nghi build FE theo thu tu

1. Build CRUD `LearningTicketType`
2. Build CRUD `SlotType`
3. Build man `Compatibility` dua tren `matrix + bulk override`
4. Neu can, de them 1 tab `Advanced Overrides` cho admin goi CRUD explicit row

## 11. Ket luan de FE chot huong

- `LearningTicketType` = noi dinh nghia rule mac dinh
- `SlotType` = noi dinh nghia metadata slot
- `TicketTypeCompatibility` = noi luu override thu cong
- `Matrix` = API quan trong nhat cho FE van hanh
- `Bulk override` = API save chinh cho FE

Neu FE build theo huong nay, man hinh se:

- de van hanh hon so voi CRUD tung row
- van giu du linh hoat cho case dac biet
- tan dung duoc logic auto-match duoi he thong
