# Tai Lieu API FE - ProgramController (GET APIs) - 2026-04-27

Tai lieu nay tong hop 2 API GET trong `ProgramController.cs` phuc vu FE:

- Lay danh sach program
- Lay chi tiet 1 program

Pham vi tai lieu:

- `GET /api/programs`
- `GET /api/programs/{id}`

## Tong quan role va pham vi du lieu

| Role | Du lieu duoc xem | Pham vi du lieu | Hanh dong duoc phep |
| --- | --- | --- | --- |
| Admin | Danh sach va chi tiet program | `all` | `view` |
| ManagementStaff | Danh sach va chi tiet program | `all` | `view` |
| Teacher | Danh sach program + chi tiet program | `all` | `view` |
| Parent | Chi tiet program public | `public` | `view` |
| Student | Chi tiet program public | `public` | `view` |
| Anonymous | Chi tiet program public | `public` | `view` |

Ghi chu:

- `GET /api/programs` yeu cau role `Admin`, `ManagementStaff`, `Teacher`.
- `GET /api/programs/{id}` la public (`AllowAnonymous`).
- Hien tai khong co scope `own` hay `department`; neu co quyen thi xem duoc toan bo program.

## Dinh dang response chung

Success tu `MatchOk()`:

```json
{
  "isSuccess": true,
  "data": {}
}
```

Error domain:

```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.4",
  "title": "Program.NotFound",
  "status": 404,
  "detail": "Program with Id = 'guid' was not found"
}
```

## Danh sach API

### 1. GET `/api/programs`

Dung de lay danh sach program de FE filter/chon trong cac man staff/teacher.

Roles: `Admin`, `ManagementStaff`, `Teacher`

Query params:

| Field | Type | Required | Default | Mo ta |
| --- | --- | --- | --- | --- |
| `branchId` | `Guid?` | No | null | Loc program theo branch assignment |
| `searchTerm` | `string?` | No | null | Tim theo `name`, `code`, `description` |
| `isActive` | `bool?` | No | null | Loc theo active/inactive |
| `isMakeup` | `bool?` | No | null | Loc theo makeup program |
| `pageNumber` | `int` | No | 1 | Trang |
| `pageSize` | `int` | No | 10 | So item/trang |

Response success:

```json
{
  "isSuccess": true,
  "data": {
    "programs": {
      "items": [
        {
          "id": "guid",
          "name": "Flyers",
          "code": "FLYERS",
          "isMakeup": false,
          "isSupplementary": false,
          "description": "Mo ta program",
          "isActive": true,
          "assignedBranchCount": 1,
          "baseFee": 30000000,
          "fee": 30000000,
          "classCount": 2,
          "studentCount": 20,
          "status": "Active"
        }
      ],
      "pageNumber": 1,
      "totalPages": 1,
      "totalCount": 1
    }
  }
}
```

Field response:

| Field | Type | Mo ta |
| --- | --- | --- |
| `assignedBranchCount` | `int` | So branch dang active assignment cho program |
| `baseFee` | `decimal` | Min `tuitionAmount` cua active tuition plans phu hop filter branch |
| `fee` | `decimal` | Hien tai tinh giong `baseFee` |
| `classCount` | `int` | So lop khong bi `Cancelled` |
| `studentCount` | `int` | So enrollment active cua cac lop thuoc program |
| `status` | `string` | `Active` / `Inactive`, derive tu `isActive` |

Response loi:

- `401` Unauthorized
- `403` Forbidden

Ghi chu:

- Danh sach nay da bo cac field cu `defaultTuitionAmount`, `unitPriceSession`, `totalSessions`.
- Neu co `branchId`, `baseFee`/`fee`, `classCount`, `studentCount` se tinh theo scope branch do khi co the.

### 2. GET `/api/programs/{id}`

Dung de lay chi tiet 1 program.

Roles: `Admin`, `ManagementStaff`, `Teacher`, `Parent`, `Student`, `Anonymous`

Path params:

| Field | Type | Required | Mo ta |
| --- | --- | --- | --- |
| `id` | `Guid` | Yes | Program id |

Response success:

```json
{
  "isSuccess": true,
  "data": {
    "id": "guid",
    "name": "Flyers",
    "code": "FLYERS",
    "isMakeup": false,
    "isSupplementary": false,
    "description": "Mo ta program",
    "isActive": true,
    "branchAssignments": [
      {
        "branchId": "guid",
        "branchName": "HCM",
        "isActive": true,
        "defaultMakeupClassId": null
      }
    ],
    "baseFee": 30000000,
    "fee": 30000000,
    "classCount": 2,
    "studentCount": 20,
    "status": "Active"
  }
}
```

Response loi:

| HTTP | Code/message | Khi nao |
| --- | --- | --- |
| 404 | `Program.NotFound` | Program khong ton tai hoac da bi soft delete |

## Status definition

Program khong co workflow status rieng; field `status` trong response duoc derive tu `isActive`.

| Status | Dieu kien | Y nghia |
| --- | --- | --- |
| `Active` | `isActive = true` | Program dang hoat dong |
| `Inactive` | `isActive = false` | Program da vo hieu hoa |

## Luong chuyen trang thai

2 API GET nay khong thay doi trang thai.

Thong tin `status` duoc cap nhat boi cac API khac nhu toggle-status/update, khong nam trong pham vi tai lieu nay.

## Permission matrix theo role

| API | Admin | ManagementStaff | Teacher | Parent | Student | Anonymous |
| --- | --- | --- | --- | --- | --- | --- |
| `GET /api/programs` | Yes | Yes | Yes | No | No | No |
| `GET /api/programs/{id}` | Yes | Yes | Yes | Yes | Yes | Yes |

## Validation rule tong hop

| Rule | API ap dung | Ket qua khi sai |
| --- | --- | --- |
| User phai dang nhap | `GET /api/programs` | 401 |
| Role phai la `Admin`, `ManagementStaff`, hoac `Teacher` | `GET /api/programs` | 403 |
| Program phai ton tai va chua bi soft delete | `GET /api/programs/{id}` | 404 `Program.NotFound` |
| `searchTerm` neu co se loc theo `name`, `code`, `description` | `GET /api/programs` | Khong tra loi |
| `branchId` neu co se loc program theo branch assignment | `GET /api/programs` | Khong tra loi |
| `isActive` / `isMakeup` neu co se dung lam filter bool | `GET /api/programs` | Khong tra loi |

## Luu y FE quan trong

- `GET /api/programs/{id}` la public, phu hop cho landing/detail page neu FE can xem thong tin 1 program.
- `BranchAssignments` trong detail chi tra cac branch assignment dang `IsActive = true`.
- `baseFee` va `fee` hien tai deu tinh tu active tuition plans; neu FE can chi tiet hoc phi theo goi, hay goi them API tuition plan.