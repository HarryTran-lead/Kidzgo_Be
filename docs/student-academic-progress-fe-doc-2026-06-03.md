# Student Academic Progress FE Doc

Tai lieu nay tom tat phan "Tien trinh HV" trong he thong de frontend map dung backend.

## 1. Phan nao trong he thong dang xu ly "tien trinh hoc vien"

Neu dang noi den tab `Tien trinh HV` trong man `Academic Progression`, backend chinh la:

- Controller: `Kidzgo.API/Controllers/StudentProgressController.cs`
- Entity luu du lieu: `Kidzgo.Domain/AcademicProgression/StudentProgress.cs`
- Service tinh toan: `Kidzgo.Application/Services/ProgressionService.cs`

Phan nay khac voi:

- `Level & Module`: quan ly cau truc hoc thuat (`levels`, `modules`)
- `Level Progression`: danh gia len level/chuyen chuong trinh (`api/level-progressions/...`)

Noi ngan gon:

- `student-progress` = hoc vien dang hoc toi dau trong module nao, hoan thanh bao nhieu %, da assessment chua, co can remedial khong.
- `level-progressions` = quy trinh danh gia va phe duyet len level/chuyen chuong trinh.

## 2. API FE can dung cho tab `Tien trinh HV`

### 2.1 Dashboard tong quan

`GET /api/student-progress/dashboard`

Role:

- `Admin`
- `ManagementStaff`
- `Teacher`

Dung de hien cac o tong quan o dau man:

- `inProgressStudents`
- `completedStudents`
- `remedialRequiredStudents`
- `failedPromotions`
- `weakModules[]`

Response mau:

```json
{
  "isSuccess": true,
  "data": {
    "inProgressStudents": 32,
    "completedStudents": 18,
    "remedialRequiredStudents": 4,
    "failedPromotions": 2,
    "weakModules": [
      {
        "moduleId": "guid",
        "moduleCode": "STARTERS_M1",
        "moduleName": "Alphabet",
        "remedialCount": 3,
        "averageCompletionPercent": 64.2
      }
    ]
  }
}
```

### 2.2 Chi tiet tien trinh cua 1 hoc vien

`GET /api/student-progress/{studentId}`

Role:

- `Admin`
- `ManagementStaff`
- `Teacher`

API nay tra ve danh sach progress theo tung module cua hoc vien.

Response mau:

```json
{
  "isSuccess": true,
  "data": {
    "items": [
      {
        "id": "guid",
        "studentProfileId": "guid",
        "moduleId": "guid",
        "moduleCode": "STARTERS_M1",
        "moduleName": "Alphabet",
        "levelCode": "STARTERS",
        "status": "InProgress",
        "completionPercent": 85.5,
        "assessmentStatus": "Passed",
        "promotionStatus": "Pending",
        "lastAssessmentId": "guid",
        "currentLessonPlanTemplateId": "guid",
        "startedAt": "2026-05-16T10:00:00Z",
        "completedAt": null
      }
    ]
  }
}
```

Y nghia field:

- `status`: trang thai hoc trong module
- `completionPercent`: % hoan thanh module
- `assessmentStatus`: ket qua assessment gan nhat
- `promotionStatus`: ket qua xet len module/len level
- `currentLessonPlanTemplateId`: bai dang hoc den
- `lastAssessmentId`: assessment noi voi hoc vien/module nay

### 2.3 Update tay progress

`POST /api/student-progress/update`

Role:

- `Admin`
- `ManagementStaff`
- `Teacher`

Request:

```json
{
  "studentProfileId": "guid",
  "moduleId": "guid",
  "currentLessonPlanTemplateId": "guid",
  "completionPercent": 50
}
```

Dung khi can force sync/chinh tay. Trong flow binh thuong FE khong nen goi API nay thuong xuyen neu da co lesson plan va teaching log.

## 3. Progress duoc tinh nhu the nao

Logic nam trong `ProgressionService`.

### 3.1 Nguon du lieu tinh completion

`completionPercent` duoc tinh tu:

- `LessonPlanTemplates` thuoc module
- `Attendances` cua hoc vien
- `Sessions`
- `LessonPlans`

Backend chi tinh tren cac session ma hoc vien co diem danh:

- `Present`
- `Makeup`

Va lay `CompletionPercent` cua `LessonPlan` theo tung template. Neu 1 template xuat hien nhieu lan thi backend lay gia tri cao nhat cua template do.

Cong thuc rut gon:

`completionPercent = average(max completion cua moi lesson template trong module)`

### 3.2 Nguong trang thai

Backend dang dung nguong:

- `Completed` khi `completionPercent >= 80`
- `NotStarted` khi `completionPercent <= 0`
- `RemedialRequired` khi `promotionStatus = RemedialRequired`
- Nguoc lai la `InProgress`

### 3.3 Assessment va promotion anh huong den progress

- Tao assessment se cap nhat `lastAssessmentId` va `assessmentStatus`
- Promotion decision se cap nhat `promotionStatus`
- Neu promotion pass, backend co the tao progress record cho module tiep theo

## 4. Khi nao progress tu dong duoc recalculate

FE can biet phan nay de reload dung luc.

Progress se duoc cap nhat tu dong sau cac thao tac sau:

- Tao lesson plan
- Update lesson plan
- Submit teaching log
- Update teaching log
- Tao assessment
- Tao promotion decision

Frontend nen reload `GET /api/student-progress/{studentId}` sau khi:

- giao vien submit/update teaching log
- giao vien sua lesson plan co lien quan completion
- co assessment moi
- co quyet dinh promotion/remedial moi

## 5. FE lay danh sach hoc vien o dau

`StudentProgressController` khong co API list hoc vien de chon.

Neu UI can dropdown hoac bang hoc vien de click vao xem progress, dung mot trong cac API sau:

- `GET /api/classes/{id}/students`
- `GET /api/teacher/classes/{classId}/students`

Luu y quan trong:

- field `progressPercent` trong `GET /api/classes/{id}/students` la progress theo attendance/session cua lop
- field nay khong phai `completionPercent` cua `student-progress`

FE khong nen dung `progressPercent` cua class student list de thay cho academic progress theo module.

## 6. Goi y mapping cho man hinh trong anh

Tab `Dashboard`:

- goi `GET /api/student-progress/dashboard`

Tab `Tien trinh HV`:

1. Lay danh sach hoc vien tu class/student source
2. Khi user chon 1 hoc vien, goi `GET /api/student-progress/{studentId}`
3. Render timeline/bang theo `data.items[]`, group theo `levelCode` neu can

Tab `Level & Module`:

- khong dung `student-progress`
- dung nhom API level/module rieng

## 7. Phan lien quan nhung khong phai `student-progress`

Neu frontend can hien lich assessment/progression cua hoc vien:

- Student: `GET /api/student/progression-assessments`
- Parent: `GET /api/parent/progression-assessments`
- Staff/teacher quan ly progression: `GET /api/level-progressions/assessments`

Day la du lieu danh gia progression, khong phai danh sach progress theo module.

## 8. Ket luan nhanh

Neu hoi "phan tien trinh hoc vien trong he thong la phan nao" thi cau tra loi ngan gon la:

- Backend chinh: `api/student-progress`
- Du lieu goc: `StudentProgress`
- Tinh toan goc: `ProgressionService`
- UI can list hoc vien tu class/student APIs, sau do mo chi tiet bang `GET /api/student-progress/{studentId}`

