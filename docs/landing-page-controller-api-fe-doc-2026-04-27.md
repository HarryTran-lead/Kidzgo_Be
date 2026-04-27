# Tai Lieu API FE - LandingPageController - 2026-04-27

Tai lieu nay tong hop toan bo API trong `LandingPageController.cs` de FE doc va tich hop landing page dong.

Pham vi tai lieu:

- Lay du lieu landing page public
- Lay cau hinh landing page cho staff/admin
- Cap nhat cau hinh landing page

## Tong quan role va pham vi du lieu

Tat ca API trong controller deu dung chung response wrapper `ApiResult<T>`.

| Role | Du lieu duoc xem | Pham vi du lieu | Hanh dong duoc phep |
| --- | --- | --- | --- |
| Admin | Public landing data + landing settings | `public` + `global` | `view_public`, `view_settings`, `edit_settings` |
| ManagementStaff | Public landing data + landing settings | `public` + `global` | `view_public`, `view_settings`, `edit_settings` |
| Teacher | Public landing data | `public` | `view_public` |
| Parent | Public landing data | `public` | `view_public` |
| Student | Public landing data | `public` | `view_public` |
| Anonymous | Public landing data | `public` | `view_public` |

Ghi chu:

- `GET /api/landing-page` la public, khong can dang nhap.
- `GET /api/landing-page/settings` va `PUT /api/landing-page/settings` chi cho `Admin`, `ManagementStaff`.
- Settings hien tai la 1 cau hinh global duy nhat, khong co scope `own` hay `department`.

## Dinh dang response chung

Success tu `MatchOk()`:

```json
{
  "isSuccess": true,
  "data": {}
}
```

Error domain/validation:

```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.1",
  "title": "LandingPage.FeaturedProgramsInvalid",
  "status": 400,
  "detail": "Some featured programs are invalid or unavailable: ..."
}
```

Validation pipeline co the tra them `errors`:

```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.1",
  "title": "Validation.General",
  "status": 400,
  "detail": "One or more validation errors occurred",
  "errors": [
    {
      "code": "Validation.General",
      "description": "Footer social link label is required."
    }
  ]
}
```

## Danh sach API

### 1. GET `/api/landing-page`

Dung de lay du lieu landing page public ma FE render cho tat ca user.

Roles: `Admin`, `ManagementStaff`, `Teacher`, `Parent`, `Student`, `Anonymous`

Params: khong co.

Response success:

```json
{
  "isSuccess": true,
  "data": {
    "logoUrl": "https://cdn.example.com/rex/logo.png",
    "featuredProgramsSection": {
      "title": "Khoa Hoc Noi Bat",
      "subtitle": "Kham pha cac khoa hoc phu hop cho be"
    },
    "featuredClassesSection": {
      "title": "Lop Hoc Sieu Vui",
      "subtitle": "Nhung lop hoc noi bat dang tuyen sinh"
    },
    "featuredTeachersSection": {
      "title": "Doi Ngu Giao Vien",
      "subtitle": "Nhung guong mat dong hanh cung be"
    },
    "footer": {
      "address": "To 3, Ap Ong Lang, Cua Duong, Phu Quoc, Kien Giang",
      "contactPhone": "0357.800.889 (Sai Gon)",
      "contactPhones": [
        "0357.800.889 (Sai Gon)",
        "0356.616.019 (Phu Quoc)"
      ],
      "contactEmail": "rex@example.com",
      "addresses": [
        "To 3, Ap Ong Lang, Cua Duong, Phu Quoc, Kien Giang",
        "Hem 68 Doan Thi Diem, Khu Pho 11, Duong Dong, Phu Quoc, Kien Giang"
      ],
      "socialLinks": [
        {
          "label": "Facebook",
          "url": "https://www.facebook.com/rexenglish",
          "iconKey": "facebook"
        }
      ]
    },
    "featuredPrograms": [
      {
        "id": "guid",
        "name": "Tieng Anh Tong Quat",
        "code": "GENERAL",
        "description": "Mo ta ngan",
        "isMakeup": false,
        "isSupplementary": false,
        "isActive": true,
        "tags": ["Moi trinh do", "Hoc qua choi"],
        "tuitionPlans": [
          {
            "id": "guid",
            "branchId": "guid",
            "branchName": "HCM",
            "name": "Goi 48 buoi",
            "totalSessions": 48,
            "tuitionAmount": 30000000,
            "unitPriceSession": 625000,
            "currency": "VND",
            "isActive": true
          }
        ]
      }
    ],
    "featuredClasses": [
      {
        "id": "guid",
        "branchId": "guid",
        "branchName": "HCM",
        "programId": "guid",
        "programName": "Tieng Anh Tong Quat",
        "code": "GENERAL-A1",
        "title": "Be Khoi Dau",
        "mainTeacherId": "guid",
        "mainTeacherName": "Thay Tom",
        "assistantTeacherId": null,
        "assistantTeacherName": null,
        "startDate": "2026-05-05",
        "endDate": "2026-08-05",
        "status": "Recruiting",
        "capacity": 12,
        "currentEnrollmentCount": 8,
        "weeklyScheduleSlots": [
          {
            "dayOfWeek": "MO",
            "startTime": "18:00",
            "durationMinutes": 90
          }
        ],
        "scheduleText": "Thu 2 18:00",
        "description": "Lop danh cho be moi bat dau",
        "roomId": "guid",
        "roomName": "Phong 01",
        "tags": ["3-5 tuoi", "8-10 be", "2 buoi"]
      }
    ],
    "featuredTeachers": [
      {
        "id": "guid",
        "name": "Thay David",
        "avatarUrl": "https://cdn.example.com/teacher.png",
        "branchId": "guid",
        "branchName": "HCM",
        "isActive": true,
        "teachingClassCount": 3,
        "programNames": ["Tieng Anh Tong Quat", "IELTS"]
      }
    ],
    "updatedAt": "2026-04-27T09:00:00Z"
  }
}
```

Field response quan trong:

| Field | Type | Mo ta |
| --- | --- | --- |
| `logoUrl` | `string?` | Logo hien tren landing page |
| `featuredProgramsSection` | `object` | Title/subtitle section khoa hoc |
| `featuredClassesSection` | `object` | Title/subtitle section lop hoc |
| `featuredTeachersSection` | `object` | Title/subtitle section giao vien |
| `footer.address` | `string?` | Field legacy, fallback bang dia chi dau tien |
| `footer.contactPhone` | `string?` | Field legacy, fallback bang so dien thoai dau tien |
| `footer.contactPhones` | `array<string>` | Danh sach so dien thoai hien footer |
| `footer.addresses` | `array<string>` | Danh sach dia chi footer |
| `footer.socialLinks` | `array<object>` | Danh sach link social/footer |
| `featuredPrograms[].tags` | `array<string>` | Badge/tag marketing cho program |
| `featuredClasses[].tags` | `array<string>` | Badge/tag marketing cho class |
| `featuredPrograms[].tuitionPlans` | `array<object>` | Danh sach tuition plan cua program |
| `featuredClasses[].weeklyScheduleSlots` | `array<object>` | Lich hoc thuc te dang public |

Response loi:

- Khong co loi domain custom.
- Neu chua co settings row, API van tra `200` voi cac field null / array rong.

### 2. GET `/api/landing-page/settings`

Dung de lay cau hinh landing page hien tai cho man hinh admin/staff edit.

Roles: `Admin`, `ManagementStaff`

Params: khong co.

Response success:

```json
{
  "isSuccess": true,
  "data": {
    "logoUrl": "https://cdn.example.com/rex/logo.png",
    "featuredProgramsSection": {
      "title": "Khoa Hoc Noi Bat",
      "subtitle": "Kham pha cac khoa hoc phu hop cho be"
    },
    "featuredClassesSection": {
      "title": "Lop Hoc Sieu Vui",
      "subtitle": "Nhung lop hoc noi bat dang tuyen sinh"
    },
    "featuredTeachersSection": {
      "title": "Doi Ngu Giao Vien",
      "subtitle": "Nhung guong mat dong hanh cung be"
    },
    "footerAddress": "To 3, Ap Ong Lang, Cua Duong, Phu Quoc, Kien Giang",
    "footerContactPhone": "0357.800.889 (Sai Gon)",
    "footerContactPhones": [
      "0357.800.889 (Sai Gon)",
      "0356.616.019 (Phu Quoc)"
    ],
    "footerContactEmail": "rex@example.com",
    "footerAddresses": [
      "To 3, Ap Ong Lang, Cua Duong, Phu Quoc, Kien Giang",
      "Hem 68 Doan Thi Diem, Khu Pho 11, Duong Dong, Phu Quoc, Kien Giang"
    ],
    "footerSocialLinks": [
      {
        "label": "Facebook",
        "url": "https://www.facebook.com/rexenglish",
        "iconKey": "facebook"
      }
    ],
    "featuredProgramIds": ["guid"],
    "featuredClassIds": ["guid"],
    "featuredTeacherIds": ["guid"],
    "featuredProgramConfigs": [
      {
        "id": "guid",
        "tags": ["Moi trinh do", "Hoc qua choi"]
      }
    ],
    "featuredClassConfigs": [
      {
        "id": "guid",
        "tags": ["3-5 tuoi", "8-10 be", "2 buoi"]
      }
    ],
    "featuredPrograms": [],
    "featuredClasses": [],
    "featuredTeachers": [],
    "updatedAt": "2026-04-27T09:00:00Z"
  }
}
```

Ghi chu:

- API nay tra ca `raw config` (`featuredProgramIds`, `featuredProgramConfigs`, `footerAddresses`, ...) va `resolved data` (`featuredPrograms`, `featuredClasses`, `featuredTeachers`).
- FE nen dung `featuredPrograms` / `featuredClasses` trong request update neu can luu tag marketing.
- `featuredProgramIds` / `featuredClassIds` duoc giu de backward compatibility.

Response loi:

- `401` Unauthorized
- `403` Forbidden

### 3. PUT `/api/landing-page/settings`

Dung de cap nhat cau hinh global cua landing page.

Roles: `Admin`, `ManagementStaff`

Body JSON:

```json
{
  "logoUrl": "https://cdn.example.com/rex/logo.png",
  "featuredProgramsSectionTitle": "Khoa Hoc Noi Bat",
  "featuredProgramsSectionSubtitle": "Kham pha cac khoa hoc phu hop cho be",
  "featuredClassesSectionTitle": "Lop Hoc Sieu Vui",
  "featuredClassesSectionSubtitle": "Nhung lop hoc noi bat dang tuyen sinh",
  "featuredTeachersSectionTitle": "Doi Ngu Giao Vien",
  "featuredTeachersSectionSubtitle": "Nhung guong mat dong hanh cung be",
  "footerAddress": "To 3, Ap Ong Lang, Cua Duong, Phu Quoc, Kien Giang",
  "footerContactPhone": "0357.800.889 (Sai Gon)",
  "footerContactPhones": [
    "0357.800.889 (Sai Gon)",
    "0356.616.019 (Phu Quoc)"
  ],
  "footerContactEmail": "rex@example.com",
  "footerAddresses": [
    "To 3, Ap Ong Lang, Cua Duong, Phu Quoc, Kien Giang",
    "Hem 68 Doan Thi Diem, Khu Pho 11, Duong Dong, Phu Quoc, Kien Giang"
  ],
  "footerSocialLinks": [
    {
      "label": "Facebook",
      "url": "https://www.facebook.com/rexenglish",
      "iconKey": "facebook"
    },
    {
      "label": "Website",
      "url": "https://rex.edu.vn",
      "iconKey": "website"
    }
  ],
  "featuredPrograms": [
    {
      "id": "guid",
      "tags": ["Moi trinh do", "Hoc qua choi"]
    }
  ],
  "featuredClasses": [
    {
      "id": "guid",
      "tags": ["3-5 tuoi", "8-10 be", "2 buoi"]
    }
  ],
  "featuredTeacherIds": [
    "guid"
  ]
}
```

Fields:

| Field | Type | Required | Mo ta |
| --- | --- | --- | --- |
| `logoUrl` | `string?` | No | Logo landing page |
| `featuredProgramsSectionTitle` | `string?` | No | Tieu de section program |
| `featuredProgramsSectionSubtitle` | `string?` | No | Subtitle section program |
| `featuredClassesSectionTitle` | `string?` | No | Tieu de section class |
| `featuredClassesSectionSubtitle` | `string?` | No | Subtitle section class |
| `featuredTeachersSectionTitle` | `string?` | No | Tieu de section teacher |
| `featuredTeachersSectionSubtitle` | `string?` | No | Subtitle section teacher |
| `footerAddress` | `string?` | No | Field legacy, fallback dia chi dau tien |
| `footerContactPhone` | `string?` | No | Field legacy, fallback so dien thoai dau tien |
| `footerContactPhones` | `array<string>` | No | Danh sach so dien thoai footer |
| `footerContactEmail` | `string?` | No | Email footer |
| `footerAddresses` | `array<string>` | No | Danh sach dia chi footer |
| `footerSocialLinks` | `array<object>` | No | Link social/footer |
| `footerSocialLinks[].label` | `string` | Yes | Ten hien thi |
| `footerSocialLinks[].url` | `string` | Yes | Link dich den |
| `footerSocialLinks[].iconKey` | `string?` | No | Key icon FE tu map |
| `featuredProgramIds` | `array<Guid>?` | No | Format cu, khong kem tags |
| `featuredClassIds` | `array<Guid>?` | No | Format cu, khong kem tags |
| `featuredPrograms` | `array<object>?` | No | Format moi, co thu tu + tags program |
| `featuredPrograms[].id` | `Guid` | Yes | Program duoc chon |
| `featuredPrograms[].tags` | `array<string>` | No | Tag marketing program |
| `featuredClasses` | `array<object>?` | No | Format moi, co thu tu + tags class |
| `featuredClasses[].id` | `Guid` | Yes | Class duoc chon |
| `featuredClasses[].tags` | `array<string>` | No | Tag marketing class |
| `featuredTeacherIds` | `array<Guid>` | No | Danh sach teacher duoc chon |

Response success:

- Cung shape voi `GET /api/landing-page/settings`.

Response loi hay gap:

| HTTP | Code/message | Khi nao |
| --- | --- | --- |
| 400 | Validation pipeline | Vuot max length, footer item rong, tag rong/trung, id rong/trung |
| 400 | `LandingPage.FeaturedProgramsInvalid` | Co `programId` khong ton tai hoac da bi xoa |
| 400 | `LandingPage.FeaturedClassesInvalid` | Co `classId` khong ton tai |
| 400 | `LandingPage.FeaturedTeachersInvalid` | Co `teacherId` khong hop le, khong phai teacher, hoac da bi xoa |
| 401 | Unauthorized | Chua dang nhap |
| 403 | Forbidden | Role khong hop le |

## Status definition

Landing page settings khong co workflow status rieng.

Nhung status/flag FE co the gap trong response:

### Class status trong `featuredClasses[].status`

| Status | Y nghia |
| --- | --- |
| `Planned` | Lop sap khai giang / da len ke hoach |
| `Recruiting` | Lop dang tuyen sinh |
| `Active` | Lop dang hoc |
| `Full` | Lop da day |

Ghi chu:

- Public landing page chi hien class o cac status tren.
- `featuredPrograms[].isActive` va `featuredTeachers[].isActive` la cờ bool, khong phai enum.

## Luong chuyen trang thai

Khong co state machine.

Luong cap nhat:

1. Staff/Admin lay settings hien tai
2. Staff/Admin chinh noi dung, section, tag, footer
3. Goi `PUT /api/landing-page/settings`
4. `GET /api/landing-page` se tu dong reflect du lieu moi

## Permission matrix theo role

| API | Admin | ManagementStaff | Teacher | Parent | Student | Anonymous |
| --- | --- | --- | --- | --- | --- | --- |
| `GET /api/landing-page` | Yes | Yes | Yes | Yes | Yes | Yes |
| `GET /api/landing-page/settings` | Yes | Yes | No | No | No | No |
| `PUT /api/landing-page/settings` | Yes | Yes | No | No | No | No |

## Validation rule tong hop

| Rule | API ap dung | Ket qua khi sai |
| --- | --- | --- |
| User phai dang nhap | Settings GET/PUT | 401 |
| Role phai la `Admin` hoac `ManagementStaff` | Settings GET/PUT | 403 |
| `logoUrl` toi da 1000 ky tu | PUT settings | 400 validation |
| Moi section title toi da 255 ky tu | PUT settings | 400 validation |
| Moi section subtitle toi da 1000 ky tu | PUT settings | 400 validation |
| `footerAddress` toi da 500 ky tu | PUT settings | 400 validation |
| `footerContactPhone` toi da 100 ky tu | PUT settings | 400 validation |
| Moi `footerContactPhones` phai co gia tri va <= 100 ky tu | PUT settings | 400 validation |
| `footerContactEmail` toi da 255 ky tu | PUT settings | 400 validation |
| Moi `footerAddresses` phai co gia tri va <= 500 ky tu | PUT settings | 400 validation |
| `footerSocialLinks[].label` bat buoc, <= 100 ky tu | PUT settings | 400 validation |
| `footerSocialLinks[].url` bat buoc, <= 1000 ky tu | PUT settings | 400 validation |
| `footerSocialLinks[].iconKey` <= 100 ky tu | PUT settings | 400 validation |
| `featuredPrograms` / `featuredClasses` khong duoc co `Guid.Empty` hoac trung id | PUT settings | 400 validation |
| Moi tag trong `featuredPrograms[].tags` / `featuredClasses[].tags` phai non-empty, unique, <= 100 ky tu | PUT settings | 400 validation |
| `featuredTeacherIds` khong duoc rong/trung | PUT settings | 400 validation |
| Featured program phai ton tai va chua deleted | PUT settings | 400 `LandingPage.FeaturedProgramsInvalid` |
| Featured class phai ton tai | PUT settings | 400 `LandingPage.FeaturedClassesInvalid` |
| Featured teacher phai ton tai, role teacher, chua deleted | PUT settings | 400 `LandingPage.FeaturedTeachersInvalid` |

## Luu y FE quan trong

- Thu tu item trong `featuredPrograms`, `featuredClasses`, `featuredTeacherIds` la thu tu hien thi.
- Neu FE can tag marketing, hay dung `featuredPrograms` / `featuredClasses` thay vi chi gui `featuredProgramIds` / `featuredClassIds`.
- `footerAddress` va `footerContactPhone` la field legacy; FE moi nen uu tien `footerAddresses` va `footerContactPhones`.
- `iconKey` trong `footerSocialLinks` chi la key de FE tu map icon, backend khong validate theo whitelist.