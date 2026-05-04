# Flow Set Limit Buoi Bu Cho Program - FE Doc - 2026-05-04

Tai lieu nay mo ta API de FE cau hinh gioi han so buoi nghi hop le moi thang cua mot chuong trinh. Trong code BE field ten la `maxLeavesPerMonth`; ve nghiep vu FE co the hien thi la "gioi han buoi bu/thang" vi moi don nghi du dieu kien 24h se tao makeup credit.

## 1. Tom tat flow

1. Admin/Staff chon mot `programId`.
2. FE nhap so buoi toi da duoc nghi hop le trong thang, vi du `2`.
3. FE goi API `PATCH /api/admin/programs/{programId}/monthly-leave-limit`.
4. BE kiem tra program ton tai va `maxLeavesPerMonth > 0`.
5. BE tao moi hoac cap nhat record `ProgramLeavePolicy` theo `programId`.
6. Khi phu huynh tao don nghi, BE dem tong so ngay/buoi nghi `Pending` + `Approved` trong cung thang cua hoc sinh, class va program.
7. Neu tong so buoi nghi vuot limit da cau hinh, BE tra loi loi `LeaveRequest.ExceededMonthlyLeaveLimit` va khong tao don nghi moi, nen cung khong tao them makeup credit.

Ghi chu quan trong:

- Neu program chua cau hinh limit, BE mac dinh `2`.
- Limit nay ap dung khi tao leave request, khong phai API cap nhat truc tiep so makeup credit da co.
- BE hien chua co API GET rieng de lay `ProgramLeavePolicy`; response cua API PATCH tra ve gia tri vua luu.

## 2. API set limit

### PATCH `/api/admin/programs/{programId}/monthly-leave-limit`

Muc dich: tao moi hoac cap nhat gioi han so buoi nghi hop le moi thang cua chuong trinh.

Path params:

| Field | Type | Required | Mo ta |
| --- | --- | --- | --- |
| `programId` | `guid` | Yes | Id cua program can cau hinh limit |

Request headers:

```http
Content-Type: application/json
Authorization: Bearer <accessToken>
```

Request payload:

```json
{
  "maxLeavesPerMonth": 2
}
```

Field rules:

| Field | Type | Required | Rule | FE label goi y |
| --- | --- | --- | --- | --- |
| `maxLeavesPerMonth` | `number` | Yes | So nguyen `> 0` | Gioi han buoi bu/thang |

Success response `200 OK`:

```json
{
  "isSuccess": true,
  "data": {
    "programId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "maxLeavesPerMonth": 2,
    "updatedAt": "2026-05-04T07:30:00Z",
    "updatedBy": "6ca85f64-5717-4562-b3fc-2c963f66afa6"
  }
}
```

Response fields:

| Field | Type | Mo ta |
| --- | --- | --- |
| `programId` | `guid` | Program vua duoc cau hinh |
| `maxLeavesPerMonth` | `number` | Limit BE da luu |
| `updatedAt` | `datetime` | Thoi diem cap nhat |
| `updatedBy` | `guid?` | User cap nhat, co the `null` neu token/context khong co user |

## 3. Error response

### Program khong ton tai

HTTP `404`

```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.4",
  "title": "Program.NotFound",
  "status": 404,
  "detail": "Program with Id = '3fa85f64-5717-4562-b3fc-2c963f66afa6' was not found"
}
```

FE message goi y: `Chuong trinh khong ton tai hoac da bi xoa.`

### Limit nho hon hoac bang 0

HTTP `400`

```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.1",
  "title": "ProgramLeavePolicy.InvalidMaxLeavesPerMonth",
  "status": 400,
  "detail": "Max leaves per month must be greater than 0"
}
```

FE message goi y: `Gioi han buoi bu moi thang phai lon hon 0.`

### Chua dang nhap / khong du quyen

Neu moi truong BE bat authorize cho admin route:

| HTTP | Khi nao | FE message goi y |
| --- | --- | --- |
| `401` | Chua gui token hoac token het han | `Vui long dang nhap lai.` |
| `403` | Token hop le nhung role khong duoc phep | `Ban khong co quyen cau hinh gioi han nay.` |

Luu y: Trong code hien tai `AdminUserController` dang comment `[Authorize(Roles = "Admin")]`, nen viec chan role phu thuoc cau hinh thuc te cua BE khi deploy.

## 4. Flow anh huong khi phu huynh xin nghi

Sau khi set limit, API tao don nghi se dung limit nay:

- BE lay class tu `classId`, sau do lay `classInfo.ProgramId`.
- BE tim `ProgramLeavePolicy` theo `programId`.
- Neu co cau hinh, dung `maxLeavesPerMonth`.
- Neu khong co cau hinh, dung default `2`.
- BE dem cac leave request cua hoc sinh trong cung `classId`, cung thang/nam, status `Pending` hoac `Approved`, va class chua `Closed`.
- BE cong them cac session FE dang xin nghi trong request hien tai.
- Neu tong so ngay/buoi nghi trong thang `>` limit, BE tra loi loi.

Error khi phu huynh xin nghi vuot limit:

HTTP `400`

```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.1",
  "title": "LeaveRequest.ExceededMonthlyLeaveLimit",
  "status": 400,
  "detail": "Student has exceeded the maximum of 2 leaves per month."
}
```

FE message goi y: `Hoc sinh da vuot gioi han 2 buoi nghi/buoi bu trong thang nay.`

## 5. FE handling goi y

UI admin:

- Input chi cho phep so nguyen duong.
- Min value: `1`.
- Khong gui `0`, so am, chuoi rong.
- Sau khi PATCH thanh cong, cap nhat state bang `data.maxLeavesPerMonth`.

Mapping message:

| BE code/title | Message FE |
| --- | --- |
| `Program.NotFound` | `Chuong trinh khong ton tai hoac da bi xoa.` |
| `ProgramLeavePolicy.InvalidMaxLeavesPerMonth` | `Gioi han buoi bu moi thang phai lon hon 0.` |
| `LeaveRequest.ExceededMonthlyLeaveLimit` | `Hoc sinh da vuot gioi han buoi nghi/buoi bu trong thang nay.` |
| `401` | `Vui long dang nhap lai.` |
| `403` | `Ban khong co quyen thuc hien thao tac nay.` |

Example FE call:

```ts
async function updateProgramMakeupLimit(programId: string, maxLeavesPerMonth: number) {
  const res = await fetch(`/api/admin/programs/${programId}/monthly-leave-limit`, {
    method: "PATCH",
    headers: {
      "Content-Type": "application/json",
      Authorization: `Bearer ${token}`
    },
    body: JSON.stringify({ maxLeavesPerMonth })
  });

  const body = await res.json();

  if (!res.ok) {
    throw new Error(body.detail ?? body.title ?? "Update limit failed");
  }

  return body.data;
}
```
