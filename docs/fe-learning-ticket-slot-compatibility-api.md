# FE API doc - LearningTicketType, SlotType, TicketTypeCompatibility

Cap nhat theo code hien tai ngay 2026-06-04.

Pham vi tai lieu nay mo ta cac thay doi va contract FE can dung cho 3 controller:
- `LearningTicketTypeController`
- `SlotTypeController`
- `TicketTypeCompatibilityController`

Tai lieu nay tap trung vao:
- API contract cho FE
- permission theo role
- status/rule definition
- validation va cac case loi
- huong UI FE nen build

## 1. Tong quan thay doi

### 1.1. Mo hinh moi

He thong da chuyen tu cach cau hinh compatibility theo tung cap thu cong sang mo hinh:
- `LearningTicketType` giu `default rule`
- `SlotType` giu `metadata`
- `TicketTypeCompatibility` giu `manual override`

Noi cach khac:
- `LearningTicketType` khong can liet ke san toan bo `slot type` tuong thich.
- He thong tu tinh ket qua matching runtime dua tren rule mac dinh.
- Bang `TicketTypeCompatibilities` khong auto tao full matrix.
- Bang `TicketTypeCompatibilities` chi dung de luu ngoai le `force allow` hoac `force deny`.

### 1.2. Thu tu danh gia compatibility

Thu tu evaluate hien tai:
1. Neu co row override trong `TicketTypeCompatibilities` cho cap `(learningTicketTypeId, slotTypeId)` thi dung row nay.
2. Neu khong co override va `CompatibilityMode = AllowAll` thi compatible.
3. Neu khong co override va `CompatibilityMode = RuleBased` thi check 4 chieu:
   - `AllowedDayGroups` vs `SlotType.DayGroup`
   - `AllowedTimeBands` vs `SlotType.TimeBand`
   - `AllowedTeacherTypes` vs `SlotType.TeacherType`
   - `AllowedUsageTypes` vs `SlotType.UsageType`

### 1.3. Y nghia cua cac field moi

`LearningTicketType` co them:
- `compatibilityMode`
- `allowedDayGroups`
- `allowedTimeBands`
- `allowedTeacherTypes`
- `allowedUsageTypes`

`SlotType` co them:
- `dayGroup`
- `timeBand`
- `teacherType`
- `usageType`

`TicketTypeCompatibilityController` co them API moi:
- `GET /api/ticket-type-compatibilities/matrix`
- `PUT /api/ticket-type-compatibilities/learning-ticket-types/{learningTicketTypeId}/overrides`

### 1.4. JSON va enum

API dang dung `JsonStringEnumConverter`, vi vay enum di/ve o dang `string`.

Vi du:
```json
{
  "compatibilityMode": "RuleBased",
  "allowedDayGroups": ["Weekend"],
  "dayGroup": "Weekday",
  "teacherType": "Native"
}
```

### 1.5. Format response chung

Response success:
```json
{
  "isSuccess": true,
  "data": {}
}
```

Response error thuong la `ProblemDetails`:
```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.1",
  "title": "LearningTicketType.CodeExists",
  "status": 409,
  "detail": "Learning ticket type code 'DEFAULT' already exists.",
  "errors": [
    {
      "code": "LearningTicketType.CodeExists",
      "description": "Learning ticket type code 'DEFAULT' already exists."
    }
  ]
}
```

Validation error:
```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.1",
  "title": "Validation.General",
  "status": 400,
  "detail": "One or more validation errors occurred",
  "errors": [
    {
      "code": "NotEmptyValidator",
      "description": "'Code' must not be empty."
    }
  ]
}
```

## 2. Mỗi role được xem dữ liệu gì

### 2.1. Role va pham vi du lieu

| Role | Xem du lieu gi | Pham vi du lieu |
| --- | --- | --- |
| `Admin` | Toan bo `LearningTicketType`, `SlotType`, `TicketTypeCompatibility`, `matrix` effective result | `all` |
| `ManagementStaff` | Toan bo `LearningTicketType`, `SlotType`, `TicketTypeCompatibility`, `matrix` effective result | `all` |
| Chua dang nhap | Khong xem duoc | none |

Ghi chu:
- Hien tai khong co rule `own` hoac `department`.
- Khong co branch scoping trong 3 controller nay.
- `Admin` va `ManagementStaff` deu xem duoc ca du lieu active va inactive neu goi API khong filter.

### 2.2. Cac hanh dong duoc phep

| Role | View | Create | Edit | Save override | Delete |
| --- | --- | --- | --- | --- | --- |
| `Admin` | Yes | Yes | Yes | Yes | Yes |
| `ManagementStaff` | Yes | Yes | Yes | Yes | No |
| Chua dang nhap | No | No | No | No | No |

## 3. Danh sach API

## 3.1. LearningTicketType APIs

### 3.1.1. `POST /api/learning-ticket-types`

Mo ta:
- Tao `learning ticket type`.
- Day la noi FE cau hinh `default rule` cho ticket type.

Permission:
- `Admin`
- `ManagementStaff`

Request body:

| Field | Type | Required | Ghi chu |
| --- | --- | --- | --- |
| `code` | `string` | Yes | Se duoc `trim` va `upper-case` o BE |
| `name` | `string` | Yes | Se duoc `trim` |
| `description` | `string \| null` | No | Rong/blank se thanh `null` |
| `compatibilityMode` | `AllowAll \| RuleBased` | Yes | Mac dinh request la `AllowAll` |
| `allowedDayGroups` | `string[]` | No | Gia tri hop le: `Weekday`, `Weekend` |
| `allowedTimeBands` | `string[]` | No | Gia tri hop le: `Morning`, `Afternoon`, `Evening` |
| `allowedTeacherTypes` | `string[]` | No | Gia tri hop le: `Standard`, `Native` |
| `allowedUsageTypes` | `string[]` | No | Gia tri hop le: `Standard`, `Makeup`, `Remedial`, `Review`, `Custom` |
| `isActive` | `boolean` | No | Mac dinh `true` |

Rule quan trong:
- Cac mang `allowed*` rong nghia la khong gioi han o chieu do.
- Khong duoc gui `None` trong cac mang `allowed*`.
- Neu `compatibilityMode = AllowAll` thi `allowed*` co the gui rong; FE nen coi cac field rule la bi ignore.

Response success:
- HTTP `201 Created`
- Body:
```json
{
  "isSuccess": true,
  "data": {
    "id": "guid",
    "code": "DEFAULT",
    "name": "Ve hoc theo goi",
    "description": "Ve hoc theo goi",
    "compatibilityMode": "RuleBased",
    "allowedDayGroups": ["Weekday"],
    "allowedTimeBands": [],
    "allowedTeacherTypes": ["Standard"],
    "allowedUsageTypes": ["Standard"],
    "isActive": true,
    "createdAt": "2026-06-04T10:00:00+07:00",
    "updatedAt": "2026-06-04T10:00:00+07:00"
  }
}
```

Response error:
- `400` validation error
- `401` chua xac thuc
- `403` khong dung role
- `409 LearningTicketType.CodeExists`

### 3.1.2. `GET /api/learning-ticket-types`

Mo ta:
- Lay danh sach `learning ticket type`.
- FE dung de render list master data hoac selector.

Permission:
- `Admin`
- `ManagementStaff`

Query params:

| Param | Type | Required | Ghi chu |
| --- | --- | --- | --- |
| `searchTerm` | `string` | No | Search theo `code` hoac `name` |
| `isActive` | `boolean` | No | Filter active/inactive |

Response success:
- HTTP `200 OK`
```json
{
  "isSuccess": true,
  "data": {
    "items": [
      {
        "id": "guid",
        "code": "DEFAULT",
        "name": "Ve hoc theo goi",
        "description": "Ve hoc theo goi",
        "compatibilityMode": "AllowAll",
        "allowedDayGroups": [],
        "allowedTimeBands": [],
        "allowedTeacherTypes": [],
        "allowedUsageTypes": [],
        "isActive": true,
        "createdAt": "2026-06-04T10:00:00+07:00",
        "updatedAt": "2026-06-04T10:00:00+07:00"
      }
    ]
  }
}
```

Response error:
- `401`
- `403`

### 3.1.3. `GET /api/learning-ticket-types/{id}`

Mo ta:
- Lay chi tiet 1 `learning ticket type`.
- FE nen dung API nay khi mo man edit rule.

Permission:
- `Admin`
- `ManagementStaff`

Path params:

| Param | Type | Required |
| --- | --- | --- |
| `id` | `guid` | Yes |

Response success:
- HTTP `200 OK`
- `data` co cung shape voi item cua list.

Response error:
- `401`
- `403`
- `404 LearningTicketType.NotFound`

### 3.1.4. `PUT /api/learning-ticket-types/{id}`

Mo ta:
- Update `learning ticket type`.
- Day la API FE dung de sua `default rule`.

Permission:
- `Admin`
- `ManagementStaff`

Path params:

| Param | Type | Required |
| --- | --- | --- |
| `id` | `guid` | Yes |

Request body:
- Cung field voi `POST /api/learning-ticket-types`

Response success:
- HTTP `200 OK`
- `data` co cung shape voi item cua list.

Response error:
- `400` validation error
- `401`
- `403`
- `404 LearningTicketType.NotFound`
- `409 LearningTicketType.CodeExists`

### 3.1.5. `DELETE /api/learning-ticket-types/{id}`

Mo ta:
- Xoa `learning ticket type`.

Permission:
- `Admin` only

Path params:

| Param | Type | Required |
| --- | --- | --- |
| `id` | `guid` | Yes |

Response success:
- HTTP `200 OK`
```json
{
  "isSuccess": true,
  "data": null
}
```

Response error:
- `401`
- `403`
- `404 LearningTicketType.NotFound`
- `409 LearningTicketType.InUse`

## 3.2. SlotType APIs

### 3.2.1. `POST /api/slot-types`

Mo ta:
- Tao `slot type`.
- Day la noi FE cau hinh metadata cho slot.

Permission:
- `Admin`
- `ManagementStaff`

Request body:

| Field | Type | Required | Ghi chu |
| --- | --- | --- | --- |
| `code` | `string` | Yes | Se duoc `trim` va `upper-case` o BE |
| `name` | `string` | Yes | Se duoc `trim` |
| `description` | `string \| null` | No | Rong/blank se thanh `null` |
| `dayGroup` | `None \| Weekday \| Weekend` | Yes | 1 gia tri duy nhat |
| `timeBand` | `None \| Morning \| Afternoon \| Evening` | Yes | 1 gia tri duy nhat |
| `teacherType` | `None \| Standard \| Native` | Yes | 1 gia tri duy nhat |
| `usageType` | `None \| Standard \| Makeup \| Remedial \| Review \| Custom` | Yes | 1 gia tri duy nhat |
| `isActive` | `boolean` | No | Mac dinh `true` |

Rule quan trong:
- `SlotType` khong dung mang.
- Moi chieu chi nhan 1 gia tri.
- `None` o `SlotType` la hop le, nghia la slot chua duoc tag o chieu do.

Response success:
- HTTP `201 Created`
```json
{
  "isSuccess": true,
  "data": {
    "id": "guid",
    "code": "STANDARD-WEEKEND",
    "name": "Lop thuong cuoi tuan",
    "description": "Lop thuong cuoi tuan",
    "dayGroup": "Weekend",
    "timeBand": "Morning",
    "teacherType": "Standard",
    "usageType": "Standard",
    "isActive": true,
    "createdAt": "2026-06-04T10:00:00+07:00",
    "updatedAt": "2026-06-04T10:00:00+07:00"
  }
}
```

Response error:
- `400` validation error
- `401`
- `403`
- `409 SlotType.CodeExists`

### 3.2.2. `GET /api/slot-types`

Mo ta:
- Lay danh sach `slot type`.

Permission:
- `Admin`
- `ManagementStaff`

Query params:

| Param | Type | Required | Ghi chu |
| --- | --- | --- | --- |
| `searchTerm` | `string` | No | Search theo `code` hoac `name` |
| `isActive` | `boolean` | No | Filter active/inactive |

Response success:
- HTTP `200 OK`
- `data.items[]` co shape nhu item trong vi du `POST`.

Response error:
- `401`
- `403`

### 3.2.3. `GET /api/slot-types/{id}`

Mo ta:
- Lay chi tiet 1 `slot type`.

Permission:
- `Admin`
- `ManagementStaff`

Path params:

| Param | Type | Required |
| --- | --- | --- |
| `id` | `guid` | Yes |

Response success:
- HTTP `200 OK`
- `data` co cung shape voi item cua list.

Response error:
- `401`
- `403`
- `404 SlotType.NotFound`

### 3.2.4. `PUT /api/slot-types/{id}`

Mo ta:
- Update `slot type`.

Permission:
- `Admin`
- `ManagementStaff`

Path params:

| Param | Type | Required |
| --- | --- | --- |
| `id` | `guid` | Yes |

Request body:
- Cung field voi `POST /api/slot-types`

Response success:
- HTTP `200 OK`
- `data` co cung shape voi item cua list.

Response error:
- `400` validation error
- `401`
- `403`
- `404 SlotType.NotFound`
- `409 SlotType.CodeExists`

### 3.2.5. `DELETE /api/slot-types/{id}`

Mo ta:
- Xoa `slot type`.

Permission:
- `Admin` only

Path params:

| Param | Type | Required |
| --- | --- | --- |
| `id` | `guid` | Yes |

Response success:
- HTTP `200 OK`
```json
{
  "isSuccess": true,
  "data": null
}
```

Response error:
- `401`
- `403`
- `404 SlotType.NotFound`
- `409 SlotType.InUse`

## 3.3. TicketTypeCompatibility APIs

### 3.3.1. `GET /api/ticket-type-compatibilities`

Mo ta:
- Lay danh sach row explicit trong bang `TicketTypeCompatibilities`.
- Day khong phai full matrix effective result.
- API nay chi tra ve cac row override/manual row da ton tai trong DB.

Permission:
- `Admin`
- `ManagementStaff`

Query params:

| Param | Type | Required | Ghi chu |
| --- | --- | --- | --- |
| `learningTicketTypeId` | `guid` | No | Filter theo ticket type |
| `slotTypeId` | `guid` | No | Filter theo slot type |

Response success:
- HTTP `200 OK`
```json
{
  "isSuccess": true,
  "data": {
    "items": [
      {
        "id": "guid",
        "learningTicketTypeId": "guid",
        "learningTicketTypeCode": "DEFAULT",
        "slotTypeId": "guid",
        "slotTypeCode": "STANDARD-WEEKEND",
        "isCompatible": false,
        "createdAt": "2026-06-04T10:00:00+07:00",
        "updatedAt": "2026-06-04T10:00:00+07:00"
      }
    ]
  }
}
```

Response error:
- `401`
- `403`

### 3.3.2. `GET /api/ticket-type-compatibilities/matrix`

Mo ta:
- Lay `effective result` cua compatibility.
- Ket qua nay da tinh theo rule mac dinh + override.
- Day la API chinh FE nen dung de render man hinh config compatibility.

Permission:
- `Admin`
- `ManagementStaff`

Query params:

| Param | Type | Required | Ghi chu |
| --- | --- | --- | --- |
| `learningTicketTypeId` | `guid` | No | Nen truyen khi mo man edit cho 1 ticket type |
| `onlyActive` | `boolean` | No | Mac dinh `true` |

Response success:
- HTTP `200 OK`
```json
{
  "isSuccess": true,
  "data": {
    "learningTicketTypes": [
      {
        "id": "guid",
        "code": "DEFAULT",
        "name": "Ve hoc theo goi",
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

Y nghia field trong `cells[]`:

| Field | Type | Y nghia |
| --- | --- | --- |
| `isCompatible` | `boolean` | Ket qua cuoi cung sau khi resolve |
| `overrideValue` | `boolean \| null` | `true` force allow, `false` force deny, `null` follow default rule |
| `source` | `string` | Nguon cua ket qua |
| `reason` | `string` | Giai thich de FE hien thi tooltip/message |

Response error:
- `401`
- `403`

### 3.3.3. `GET /api/ticket-type-compatibilities/{id}`

Mo ta:
- Lay chi tiet 1 row explicit trong bang `TicketTypeCompatibilities`.

Permission:
- `Admin`
- `ManagementStaff`

Path params:

| Param | Type | Required |
| --- | --- | --- |
| `id` | `guid` | Yes |

Response success:
- HTTP `200 OK`
- `data` co cung shape voi item trong `GET /api/ticket-type-compatibilities`.

Response error:
- `401`
- `403`
- `404 TicketTypeCompatibility.NotFound`

### 3.3.4. `POST /api/ticket-type-compatibilities`

Mo ta:
- Tao 1 row explicit trong bang `TicketTypeCompatibilities`.
- Co the dung cho admin tool, debug tool, hoac case dac biet.
- Khong khuyen dung day la UI chinh cho FE.

Permission:
- `Admin`
- `ManagementStaff`

Request body:

| Field | Type | Required |
| --- | --- | --- |
| `learningTicketTypeId` | `guid` | Yes |
| `slotTypeId` | `guid` | Yes |
| `isCompatible` | `boolean` | Yes |

Response success:
- HTTP `201 Created`
- `data` co shape `TicketTypeCompatibilityDto`.

Response error:
- `400` validation error
- `401`
- `403`
- `404 TicketTypeCompatibility.LearningTicketTypeNotFound`
- `404 TicketTypeCompatibility.SlotTypeNotFound`
- `409 TicketTypeCompatibility.MappingExists`

### 3.3.5. `PUT /api/ticket-type-compatibilities/{id}`

Mo ta:
- Update 1 row explicit trong bang `TicketTypeCompatibilities`.
- Khong khuyen dung day la UI chinh cho FE.

Permission:
- `Admin`
- `ManagementStaff`

Path params:

| Param | Type | Required |
| --- | --- | --- |
| `id` | `guid` | Yes |

Request body:

| Field | Type | Required |
| --- | --- | --- |
| `learningTicketTypeId` | `guid` | Yes |
| `slotTypeId` | `guid` | Yes |
| `isCompatible` | `boolean` | Yes |

Response success:
- HTTP `200 OK`
- `data` co shape `TicketTypeCompatibilityDto`.

Response error:
- `400` validation error
- `401`
- `403`
- `404 TicketTypeCompatibility.NotFound`
- `404 TicketTypeCompatibility.LearningTicketTypeNotFound`
- `404 TicketTypeCompatibility.SlotTypeNotFound`
- `409 TicketTypeCompatibility.MappingExists`

### 3.3.6. `PUT /api/ticket-type-compatibilities/learning-ticket-types/{learningTicketTypeId}/overrides`

Mo ta:
- Bulk upsert override cho 1 `learning ticket type`.
- Day la API FE nen dung khi user chinh tung cell tren man hinh config.

Permission:
- `Admin`
- `ManagementStaff`

Path params:

| Param | Type | Required |
| --- | --- | --- |
| `learningTicketTypeId` | `guid` | Yes |

Request body:

| Field | Type | Required | Ghi chu |
| --- | --- | --- | --- |
| `items` | `array` | Yes | Danh sach override can luu |
| `items[].slotTypeId` | `guid` | Yes | Slot type cua cell |
| `items[].isCompatible` | `boolean \| null` | Yes | `true` = force allow, `false` = force deny, `null` = xoa override |

Vi du request:
```json
{
  "items": [
    {
      "slotTypeId": "slot-guid-1",
      "isCompatible": false
    },
    {
      "slotTypeId": "slot-guid-2",
      "isCompatible": true
    },
    {
      "slotTypeId": "slot-guid-3",
      "isCompatible": null
    }
  ]
}
```

Response success:
- HTTP `200 OK`
```json
{
  "isSuccess": true,
  "data": {
    "learningTicketTypeId": "guid",
    "upsertedCount": 2,
    "removedCount": 1,
    "items": [
      {
        "id": "guid",
        "learningTicketTypeId": "guid",
        "learningTicketTypeCode": "DEFAULT",
        "slotTypeId": "guid",
        "slotTypeCode": "STANDARD-WEEKEND",
        "isCompatible": false,
        "createdAt": "2026-06-04T10:00:00+07:00",
        "updatedAt": "2026-06-04T10:05:00+07:00"
      }
    ]
  }
}
```

Response error:
- `400` validation error
- `401`
- `403`
- `404 TicketTypeCompatibility.LearningTicketTypeNotFound`
- `404 TicketTypeCompatibility.SlotTypeNotFound`

### 3.3.7. `DELETE /api/ticket-type-compatibilities/{id}`

Mo ta:
- Xoa 1 row explicit trong bang `TicketTypeCompatibilities`.

Permission:
- `Admin` only

Path params:

| Param | Type | Required |
| --- | --- | --- |
| `id` | `guid` | Yes |

Response success:
- HTTP `200 OK`
```json
{
  "isSuccess": true,
  "data": null
}
```

Response error:
- `401`
- `403`
- `404 TicketTypeCompatibility.NotFound`

## 4. Status definition

## 4.1. Danh sach status

### 4.1.1. `LearningTicketType.isActive`

| Gia tri | Y nghia |
| --- | --- |
| `true` | Active |
| `false` | Inactive |

### 4.1.2. `SlotType.isActive`

| Gia tri | Y nghia |
| --- | --- |
| `true` | Active |
| `false` | Inactive |

### 4.1.3. `LearningTicketType.compatibilityMode`

| Gia tri | Y nghia |
| --- | --- |
| `AllowAll` | Mac dinh hoc duoc tat ca slot type, tru khi bi override chan |
| `RuleBased` | Matching theo 4 chieu rule, tru khi bi override de len ket qua cuoi cung |

### 4.1.4. `TicketTypeCompatibility.isCompatible`

| Gia tri | Y nghia |
| --- | --- |
| `true` | Allow explicit |
| `false` | Deny explicit |

### 4.1.5. `matrix.cells[].source`

| Gia tri | Y nghia |
| --- | --- |
| `OverrideAllow` | Ket qua tu `manual override = true` |
| `OverrideDeny` | Ket qua tu `manual override = false` |
| `AllowAll` | Ket qua tu mode `AllowAll` |
| `Rule` | Ket qua tu `RuleBased` |

### 4.1.6. `matrix.cells[].overrideValue`

| Gia tri | Y nghia |
| --- | --- |
| `true` | Force allow |
| `false` | Force deny |
| `null` | Khong co override, follow default rule |

## 4.2. Luong chuyen trang thai

### 4.2.1. Active / Inactive

Khong co state machine rieng. FE doi trang thai qua `PUT`:
- `LearningTicketType`: doi `isActive`
- `SlotType`: doi `isActive`

Luong:
- `true -> false`
- `false -> true`

### 4.2.2. Compatibility mode

FE doi qua `PUT /api/learning-ticket-types/{id}`:
- `AllowAll -> RuleBased`
- `RuleBased -> AllowAll`

Ghi chu:
- Khi chuyen sang `AllowAll`, cac mang `allowed*` van co the ton tai nhung bi ignore o runtime.
- Khi chuyen sang `RuleBased`, neu tat ca mang `allowed*` deu rong thi hieu ung thuc te gan nhu `AllowAll`.

### 4.2.3. Override cell

FE doi qua `PUT /api/ticket-type-compatibilities/learning-ticket-types/{learningTicketTypeId}/overrides`

Luong:
- `null -> true`
- `null -> false`
- `true -> false`
- `false -> true`
- `true -> null`
- `false -> null`

`null` la trang thai rat quan trong, vi no nghia la bo override va quay lai default rule.

## 5. Permission matrix theo role

| API | Admin | ManagementStaff |
| --- | --- | --- |
| `GET /api/learning-ticket-types` | Yes | Yes |
| `GET /api/learning-ticket-types/{id}` | Yes | Yes |
| `POST /api/learning-ticket-types` | Yes | Yes |
| `PUT /api/learning-ticket-types/{id}` | Yes | Yes |
| `DELETE /api/learning-ticket-types/{id}` | Yes | No |
| `GET /api/slot-types` | Yes | Yes |
| `GET /api/slot-types/{id}` | Yes | Yes |
| `POST /api/slot-types` | Yes | Yes |
| `PUT /api/slot-types/{id}` | Yes | Yes |
| `DELETE /api/slot-types/{id}` | Yes | No |
| `GET /api/ticket-type-compatibilities` | Yes | Yes |
| `GET /api/ticket-type-compatibilities/{id}` | Yes | Yes |
| `GET /api/ticket-type-compatibilities/matrix` | Yes | Yes |
| `POST /api/ticket-type-compatibilities` | Yes | Yes |
| `PUT /api/ticket-type-compatibilities/{id}` | Yes | Yes |
| `PUT /api/ticket-type-compatibilities/learning-ticket-types/{learningTicketTypeId}/overrides` | Yes | Yes |
| `DELETE /api/ticket-type-compatibilities/{id}` | Yes | No |

## 6. Validation rule

## 6.1. Rule kiem tra du lieu

### 6.1.1. LearningTicketType

- `code`:
  - required
  - max length `100`
  - BE se `trim` va `upper-case`
- `name`:
  - required
  - max length `255`
  - BE se `trim`
- `description`:
  - max length `500`
  - blank se thanh `null`
- `allowedDayGroups[]`:
  - chi cho `Weekday`, `Weekend`
  - khong cho `None`
- `allowedTimeBands[]`:
  - chi cho `Morning`, `Afternoon`, `Evening`
  - khong cho `None`
- `allowedTeacherTypes[]`:
  - chi cho `Standard`, `Native`
  - khong cho `None`
- `allowedUsageTypes[]`:
  - chi cho `Standard`, `Makeup`, `Remedial`, `Review`, `Custom`
  - khong cho `None`

### 6.1.2. SlotType

- `code`:
  - required
  - max length `100`
  - BE se `trim` va `upper-case`
- `name`:
  - required
  - max length `255`
  - BE se `trim`
- `description`:
  - max length `500`
  - blank se thanh `null`
- `dayGroup`:
  - hop le: `None`, `Weekday`, `Weekend`
- `timeBand`:
  - hop le: `None`, `Morning`, `Afternoon`, `Evening`
- `teacherType`:
  - hop le: `None`, `Standard`, `Native`
- `usageType`:
  - hop le: `None`, `Standard`, `Makeup`, `Remedial`, `Review`, `Custom`

### 6.1.3. TicketTypeCompatibility explicit row

- `learningTicketTypeId`: required
- `slotTypeId`: required
- cap `(learningTicketTypeId, slotTypeId)` phai unique

### 6.1.4. Bulk overrides

- `learningTicketTypeId`: required
- `items`: khong duoc `null`
- moi `items[].slotTypeId`: required
- khong duoc co duplicate `slotTypeId` trong cung 1 request
- `items[].isCompatible`:
  - `true`: force allow
  - `false`: force deny
  - `null`: xoa override

## 6.2. Cac truong hop tra loi

### 6.2.1. LearningTicketType

- `400`:
  - code rong
  - name rong
  - description qua 500 ky tu
  - `allowed*` co gia tri khong hop le
- `404`:
  - `LearningTicketType.NotFound`
- `409`:
  - `LearningTicketType.CodeExists`
  - `LearningTicketType.InUse`

### 6.2.2. SlotType

- `400`:
  - code rong
  - name rong
  - description qua 500 ky tu
  - enum metadata khong hop le
- `404`:
  - `SlotType.NotFound`
- `409`:
  - `SlotType.CodeExists`
  - `SlotType.InUse`

### 6.2.3. TicketTypeCompatibility

- `400`:
  - id rong
  - duplicate `slotTypeId` trong request overrides
- `404`:
  - `TicketTypeCompatibility.NotFound`
  - `TicketTypeCompatibility.LearningTicketTypeNotFound`
  - `TicketTypeCompatibility.SlotTypeNotFound`
- `409`:
  - `TicketTypeCompatibility.MappingExists`

### 6.2.4. Auth

- `401 Unauthorized`:
  - khong co token
  - token khong hop le
- `403 Forbidden`:
  - co token nhung sai role

## 7. Huong UI FE nen build

## 7.1. Huong chinh nen chon

Khong nen build UI chinh theo kieu CRUD tung row trong `TicketTypeCompatibilities`.

Nen build UI theo huong:
- `default rule + effective preview + bulk override`

Nghia la:
- `LearningTicketType` la noi sua rule mac dinh
- `SlotType` la noi sua metadata
- `matrix` la noi xem ket qua thuc te
- `overrides` la noi staff chinh ngoai le

## 7.2. Man hinh FE de xuat

### 7.2.1. Man 1 - Learning Ticket Type list

Cot de xuat:
- `code`
- `name`
- `compatibilityMode`
- summary `allowed*`
- `isActive`
- action `edit`

Action:
- tao moi
- edit
- filter active/inactive
- search theo code/name

### 7.2.2. Man 2 - Slot Type list

Cot de xuat:
- `code`
- `name`
- `dayGroup`
- `timeBand`
- `teacherType`
- `usageType`
- `isActive`
- action `edit`

Action:
- tao moi
- edit
- filter active/inactive
- search theo code/name

### 7.2.3. Man 3 - Compatibility Config cho 1 Learning Ticket Type

Day la man hinh quan trong nhat.

Layout de xuat:
- Panel A: thong tin `LearningTicketType`
- Panel B: form sua `default rule`
- Panel C: bang `effective compatibility`

Panel B:
- `compatibilityMode`
- `allowedDayGroups`
- `allowedTimeBands`
- `allowedTeacherTypes`
- `allowedUsageTypes`
- `isActive`

Panel C:
- moi dong la 1 `slot type`
- hien:
  - `slotType.code`
  - `slotType.name`
  - `dayGroup`
  - `timeBand`
  - `teacherType`
  - `usageType`
  - `isCompatible`
  - `source`
  - `reason`
  - `override` editor

Override editor nen la tri-state:
- `Follow default`
- `Force allow`
- `Force deny`

## 7.3. API flow FE nen dung

### 7.3.1. Khi vao man hinh config cho 1 ticket type

FE nen goi:
1. `GET /api/learning-ticket-types/{id}`
2. `GET /api/ticket-type-compatibilities/matrix?learningTicketTypeId={id}&onlyActive=true`

Khong can goi `GET /api/ticket-type-compatibilities` cho man hinh chinh.

### 7.3.2. Khi save rule mac dinh

FE goi:
1. `PUT /api/learning-ticket-types/{id}`
2. reload `GET /api/ticket-type-compatibilities/matrix?learningTicketTypeId={id}&onlyActive=true`

### 7.3.3. Khi save override

FE gom cac cell thay doi va goi:
1. `PUT /api/ticket-type-compatibilities/learning-ticket-types/{id}/overrides`
2. reload `GET /api/ticket-type-compatibilities/matrix?learningTicketTypeId={id}&onlyActive=true`

## 7.4. Hanh vi UI nen co

- Neu `compatibilityMode = AllowAll`:
  - disable hoac dim cac field `allowed*`
  - show message: `Rule fields are ignored in AllowAll mode`

- Neu `compatibilityMode = RuleBased` va tat ca `allowed*` deu rong:
  - show warning: `RuleBased but no restriction configured. Effective behavior is close to AllowAll.`

- Neu `source = OverrideAllow`:
  - badge xanh
- Neu `source = OverrideDeny`:
  - badge do
- Neu `source = Rule`:
  - badge neutral
- Neu `source = AllowAll`:
  - badge neutral

- Hien `reason` o tooltip/popover de staff hieu vi sao 1 cell duoc pass/fail.

## 7.5. Khong nen build gi

Khong nen:
- bat user tao full matrix bang tay
- dung `POST/PUT /api/ticket-type-compatibilities` lam flow chinh
- coi `GET /api/ticket-type-compatibilities` la nguon truth cua toan bo compatibility

Vi:
- full matrix se rat lon khi `slot type` tang them nhu `morning/evening/...`
- bang `TicketTypeCompatibilities` hien tai chi la `override storage`
- nguon truth thuc te la `default rule + override`

## 8. Ket luan cho FE

Tam nhin FE nen theo:
- Master data `LearningTicketType`
- Master data `SlotType`
- Man hinh chinh `Compatibility Config` theo tung `LearningTicketType`

Cap API chinh FE se dung thuong xuyen:
- `GET /api/learning-ticket-types`
- `GET /api/learning-ticket-types/{id}`
- `PUT /api/learning-ticket-types/{id}`
- `GET /api/slot-types`
- `GET /api/ticket-type-compatibilities/matrix`
- `PUT /api/ticket-type-compatibilities/learning-ticket-types/{learningTicketTypeId}/overrides`

Cap API explicit row:
- `GET /api/ticket-type-compatibilities`
- `POST /api/ticket-type-compatibilities`
- `PUT /api/ticket-type-compatibilities/{id}`
- `DELETE /api/ticket-type-compatibilities/{id}`

chi nen xem la advanced/admin tool, khong phai main UX.
