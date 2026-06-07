# Curriculum Tree API FE Doc

Updated: 2026-06-06
Scope: man `Khung chuong trinh` de FE xem tong quan cau truc dao tao theo tung `Program`

## 1. Muc tieu man hinh

Trang `Khung chuong trinh` dung de xem toan bo cau truc dao tao cua trung tam theo dang cay.

Vi du tree:

```text
Kids English
  -> Starters
    -> Module 1
      -> Unit 1
        -> Get Ready Starters 2nd Edition
          -> Lesson 1
          -> Revision
          -> Assessment
```

Trang nay hien tai chi phuc vu muc dich:

- xem tong quan curriculum
- expand/collapse theo node
- chon 1 program va xem toan bo tree ben trong

Ngoai pham vi hien tai:

- chua can create/update/delete
- chua can drag-drop
- chua can chinh sua thu tu tren FE

## 2. Du lieu FE can hien thi

De render duoc tree nay, FE can backend tra ra cac lop du lieu sau:

- `Program` nao co `Level` nao
- moi `Level` co `Module` nao
- moi `Module` co `Unit` nao
- moi `Unit` dang dung `Syllabus` nao
- moi `Syllabus` co `LessonTemplate` nao

Tree mong muon:

- `Program`
- `Level`
- `Module`
- `Unit`
- `Syllabus`
- `LessonTemplate`

## 3. API toi thieu FE can

Neu chi de demo/man overview, backend khong can lam nhieu API. Toi thieu chi can 2 API GET:

### 3.1 API lay danh sach program

FE dung API nay de do dropdown chon chuong trinh.

Co the tai dung API hien co:

- `GET /api/programs`

FE can toi thieu cac field:

- `id`
- `name`
- `code`
- `isActive`

Neu can man public chi hien program active, co the dung:

- `GET /api/programs/active`

### 3.2 API lay curriculum tree theo program

Day la API moi backend can bo sung cho trang `Khung chuong trinh`.

De xuat endpoint:

- `GET /api/programs/{programId}/curriculum-tree`

Y nghia:

- FE truyen `programId`
- BE tra ve toan bo cau truc cua program do
- scope response di tu `Program -> Level -> Module -> Unit -> Syllabus -> LessonTemplate`

## 4. Response shape de xuat

Success envelope:

```json
{
  "isSuccess": true,
  "data": {
    "programId": "guid",
    "programName": "Kids English",
    "programCode": "KIDS_ENGLISH",
    "levels": [
      {
        "levelId": "guid",
        "levelName": "Starters",
        "levelOrderIndex": 1,
        "modules": [
          {
            "moduleId": "guid",
            "moduleName": "Module 1",
            "moduleOrderIndex": 1,
            "units": [
              {
                "unitId": "guid",
                "unitName": "Unit 1",
                "unitOrderIndex": 1,
                "syllabuses": [
                  {
                    "syllabusId": "guid",
                    "syllabusTitle": "Get Ready Starters 2nd Edition",
                    "syllabusCode": "GET_READY_STARTERS",
                    "version": 1,
                    "isActive": true,
                    "lessonTemplates": [
                      {
                        "lessonTemplateId": "guid",
                        "title": "Lesson 1",
                        "lessonType": "Lesson",
                        "orderIndex": 1,
                        "isActive": true
                      },
                      {
                        "lessonTemplateId": "guid",
                        "title": "Revision",
                        "lessonType": "Revision",
                        "orderIndex": 2,
                        "isActive": true
                      },
                      {
                        "lessonTemplateId": "guid",
                        "title": "Assessment",
                        "lessonType": "Assessment",
                        "orderIndex": 3,
                        "isActive": true
                      }
                    ]
                  }
                ]
              }
            ]
          }
        ]
      }
    ]
  }
}
```

## 5. Quy uoc response de FE de render

De FE render on dinh va khong phai tu parse ten, nen thong nhat:

- cac mang con nen tra `[]`, khong tra `null`
- moi node nen co `id`, `name/title`, `orderIndex` neu co thu tu
- backend nen tra san theo thu tu dung, hoac tra `orderIndex` de FE sort
- FE khong nen tu suy ra thu tu bang cach parse text nhu `Module 1`, `Unit 1`, `Lesson 1`

Field toi thieu tung node:

- `Level`: `levelId`, `levelName`
- `Module`: `moduleId`, `moduleName`, `moduleOrderIndex`
- `Unit`: `unitId`, `unitName`, `unitOrderIndex`
- `Syllabus`: `syllabusId`, `syllabusTitle`, `syllabusCode`, `version`
- `LessonTemplate`: `lessonTemplateId`, `title`, `lessonType`, `orderIndex`

Khong can tra qua nhieu field cho man nay trong scope dau:

- khong can `rawContentJson`
- khong can noi dung syllabus day du
- khong can file dinh kem
- khong can lesson plan runtime

## 6. Luong FE du kien

Luong xu ly don gian:

1. FE load danh sach `Program`
2. user chon 1 `Program`
3. FE goi `GET /api/programs/{programId}/curriculum-tree`
4. FE render tree readonly
5. click vao node thi expand/collapse

State FE can co:

- loading luc doi tree
- empty state khi program chua co du lieu
- error state neu API loi

## 7. Cau noi ngan gon gui BE

Noi gon cho backend:

> Anh/chi lam giup em API lay curriculum tree. FE se chon mot chuong trinh, vi du Kids English, roi goi API de lay toan bo cau truc cua chuong trinh do gom level, module, unit, syllabus va lesson template. Trang nay chu yeu de xem tong quan nen truoc mat chi can API GET, chua can CRUD.

Ban rut gon hon nua:

> FE can 2 API: API lay danh sach chuong trinh va API lay cay chi tiet cua 1 chuong trinh theo `programId`.

## 8. Ket luan

Cho man `Khung chuong trinh`, scope toi thieu hien tai la:

- `GET /api/programs`
- `GET /api/programs/{programId}/curriculum-tree`

Chi can 2 API nay la FE da co the demo duoc man xem tong quan cau truc dao tao theo dang cay.
