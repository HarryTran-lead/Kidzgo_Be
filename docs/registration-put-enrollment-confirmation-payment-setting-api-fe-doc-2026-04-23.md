# Registration PUT Enrollment Confirmation Payment Setting API FE Doc - 2026-04-23

Tai lieu nay mo ta rieng API:

- `PUT /api/registrations/enrollment-confirmation-payment-setting`

API nay nam trong `RegistrationController.cs` va dung de Admin upsert cau hinh payment + policy content hien thi tren phieu xac nhan nhap hoc PDF.

## 1. Role, data scope, allowed actions

| Role | Duoc xem / sua gi | Pham vi du lieu | Hanh dong duoc phep |
| --- | --- | --- | --- |
| Admin | Cau hinh confirmation setting global hoac theo branch | `all` | `create`, `edit`, `activate/deactivate` |
| ManagementStaff | Khong duoc goi PUT nay | `none` | `none` |
| Teacher | Khong duoc truy cap | `none` | `none` |
| Parent | Khong duoc truy cap | `none` | `none` |
| Student | Khong duoc truy cap | `none` | `none` |
| Anonymous | Khong duoc truy cap | `none` | `none` |

Ghi chu:

- `branchId = null` nghia la scope global.
- `branchId != null` nghia la scope theo chi nhanh.
- API nay la upsert: neu chua co setting theo scope thi tao moi, neu da co thi cap nhat.

## 2. Common response format

### Success

```json
{
  "isSuccess": true,
  "data": {}
}
```

### Error business / validation

```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.1",
  "title": "Registration.PaymentBankIdentifierRequired",
  "status": 400,
  "detail": "Bank code or bank BIN is required to generate VietQR."
}
```

## 3. API info

### Endpoint

- Method: `PUT`
- Endpoint: `/api/registrations/enrollment-confirmation-payment-setting`

### API dung de lam gi

- Tao moi hoac cap nhat setting payment dung tren PDF enrollment confirmation
- Cau hinh logo
- Cau hinh policy text cho:
  - `newStudentPolicyLines`
  - `reservationPolicyLines`

### Permission

- Role: `Admin`
- Scope: `all`

## 4. Params / Body

Khong co query param.

Body JSON:

```json
{
  "branchId": "22222222-2222-2222-2222-222222222222",
  "paymentMethod": "Chuyen khoan",
  "accountName": "REX EDUCATION",
  "accountNumber": "089498720",
  "bankName": "MB Bank",
  "bankCode": "MB",
  "bankBin": "970422",
  "vietQrTemplate": "compact2",
  "logoUrl": "https://cdn.example.com/logo.png",
  "newStudentPolicyLines": [
    "Khong ap dung hoan phi.",
    "Nghi co bao truoc 24h se duoc sap xep hoc bu."
  ],
  "reservationPolicyLines": [
    "Chi ap dung bao luu toi da 01 lan cho moi khoa hoc.",
    "Hoc phi da dong khong duoc hoan lai."
  ],
  "isActive": true
}
```

### Field definition

| Field | Type | Required | Rule / y nghia |
| --- | --- | --- | --- |
| `branchId` | `Guid?` | No | `null` = global, co gia tri = branch scope |
| `paymentMethod` | `string?` | No | Neu rong/null sau khi trim thi backend dung gia tri mac dinh |
| `accountName` | `string` | Yes | Khong duoc rong |
| `accountNumber` | `string` | Yes | Khong duoc rong |
| `bankName` | `string` | Yes | Khong duoc rong |
| `bankCode` | `string?` | Conditional | Can co it nhat 1 trong `bankCode` hoac `bankBin` |
| `bankBin` | `string?` | Conditional | Can co it nhat 1 trong `bankCode` hoac `bankBin` |
| `vietQrTemplate` | `string?` | No | Neu rong/null sau trim thi backend dung `compact2` |
| `logoUrl` | `string?` | No | Neu co gia tri phai la absolute `http(s)` hoac `data:image/...` |
| `newStudentPolicyLines` | `List<string>?` | No | Danh sach bullet cho section `Chinh sach ap dung` cua form hoc vien moi |
| `reservationPolicyLines` | `List<string>?` | No | Danh sach bullet cho section `Quy dinh bao luu` cua form hoc vien tiep tuc |
| `isActive` | `bool` | No | Mac dinh `true` |

## 5. Success response

```json
{
  "isSuccess": true,
  "data": {
    "id": "11111111-1111-1111-1111-111111111111",
    "branchId": "22222222-2222-2222-2222-222222222222",
    "paymentMethod": "Chuyen khoan",
    "accountName": "REX EDUCATION",
    "accountNumber": "089498720",
    "bankName": "MB Bank",
    "bankCode": "MB",
    "bankBin": "970422",
    "vietQrTemplate": "compact2",
    "logoUrl": "https://cdn.example.com/logo.png",
    "newStudentPolicyLines": [
      "Khong ap dung hoan phi.",
      "Nghi co bao truoc 24h se duoc sap xep hoc bu."
    ],
    "reservationPolicyLines": [
      "Chi ap dung bao luu toi da 01 lan cho moi khoa hoc.",
      "Hoc phi da dong khong duoc hoan lai."
    ],
    "qrPreviewUrl": "https://img.vietqr.io/image/970422-089498720-compact2.png?accountName=REX%20EDUCATION",
    "isActive": true,
    "createdAt": "2026-04-23T09:00:00Z",
    "updatedAt": "2026-04-23T09:15:00Z",
    "updatedBy": "33333333-3333-3333-3333-333333333333"
  }
}
```

Ghi chu:

- `newStudentPolicyLines` va `reservationPolicyLines` la noi dung hien thi tren preview/PDF, khong dieu khien business rule thuc te.
- Neu FE gui `null`, `[]`, hoac danh sach chi toan chuoi rong, backend se luu `null` va khi doc/popup preview se fallback ve policy mac dinh trong he thong.

## 6. Error response

| HTTP | Code | Message | Khi nao |
| --- | --- | --- | --- |
| 400 | `Registration.PaymentAccountNameRequired` | `Payment account name is required.` | `accountName` rong/null |
| 400 | `Registration.PaymentAccountNumberRequired` | `Payment account number is required.` | `accountNumber` rong/null |
| 400 | `Registration.PaymentBankNameRequired` | `Payment bank name is required.` | `bankName` rong/null |
| 400 | `Registration.PaymentBankIdentifierRequired` | `Bank code or bank BIN is required to generate VietQR.` | Ca `bankCode` va `bankBin` deu rong/null |
| 400 | `Registration.PaymentLogoUrlInvalid` | `Logo URL must be an absolute http(s) URL or a data:image URL.` | `logoUrl` sai format |
| 404 | `Registration.BranchNotFound` | `Branch with Id = '...' was not found` | Gui `branchId` nhung branch khong ton tai |
| 401 | Unauthorized | - | Chua dang nhap |
| 403 | Forbidden | - | Role khong phai `Admin` |
| 500 | Server failure | - | Loi he thong ngoai du kien |

Validation error sample:

```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.1",
  "title": "Registration.PaymentLogoUrlInvalid",
  "status": 400,
  "detail": "Logo URL must be an absolute http(s) URL or a data:image URL."
}
```

## 7. Status definition

API nay khong co status enum rieng. Cac trang thai/flag can hieu nhu sau:

| Field | Y nghia |
| --- | --- |
| `isActive = true` | Setting dang bat va co the duoc su dung khi build preview/PDF |
| `isActive = false` | Setting dang tat |
| `branchId = null` | Global scope |
| `branchId != null` | Branch scope |

### Luong trang thai

1. Admin goi PUT lan dau theo 1 scope -> tao moi setting
2. Admin goi PUT tiep theo cung scope -> cap nhat setting cu
3. Admin dat `isActive = false` -> setting bi tat
4. Preview/PDF se uu tien setting branch, neu khong co thi fallback global

## 8. Permission matrix theo role

| Endpoint | Admin | ManagementStaff | Teacher | Parent | Student | Anonymous |
| --- | --- | --- | --- | --- | --- | --- |
| `PUT /api/registrations/enrollment-confirmation-payment-setting` | Yes | No | No | No | No | No |

## 9. Validation rule

| Rule kiem tra | Ket qua khi sai |
| --- | --- |
| `accountName` khong duoc rong | 400 |
| `accountNumber` khong duoc rong | 400 |
| `bankName` khong duoc rong | 400 |
| Phai co it nhat mot trong `bankCode` hoac `bankBin` | 400 |
| `logoUrl` neu co phai la absolute `http(s)` hoac `data:image/...` | 400 |
| `branchId` neu gui len phai ton tai | 404 |
| User phai dang nhap | 401 |
| Role phai la `Admin` | 403 |

## 10. Cac truong hop tra loi can luu y

- Gui `branchId` dung format GUID nhung branch khong ton tai -> `404 Registration.BranchNotFound`
- Gui `newStudentPolicyLines` hoac `reservationPolicyLines` rong -> khong bao loi, backend fallback policy mac dinh khi read
- API nay khong validate do dai tung dong policy trong code hien tai
- API nay khong validate branch phai active, chi can branch ton tai
