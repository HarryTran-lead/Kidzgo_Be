# FE API Changes - Syllabus Runtime And Unit Canonical

Updated: 2026-05-26  
Audience: Frontend  
Scope: cac API da thay doi lien quan den multi-syllabus runtime, import lesson plan, unit canonical, va syllabus admin actions.

Base paths:

- `/api/classes`
- `/api/branches`
- `/api/syllabuses`

Roles:

- Runtime class APIs: `Admin`, `ManagementStaff`
- Syllabus import/admin APIs: `Admin`, `ManagementStaff`

---

## 1. Summary

BE khong con coi curriculum runtime la implicit theo `Program + Level` nua.

Runtime moi:

- Class phai gan ro `syllabusId`
- Lesson plan import phai gan ro `syllabusId`
- FE khong duoc parse unit tu `title` hay `lessonPlanUnitName` nua
- FE phai dung field unit/module/order ma BE tra san

---

## 2. Create Class

### `POST /api/classes`

Request body bat buoc co them:

- `syllabusId: Guid`

Sample:

```json
{
  "branchId": "uuid",
  "programId": "uuid",
  "levelId": "uuid",
  "syllabusId": "uuid",
  "startModuleId": "uuid",
  "startSessionIndex": 1,
  "code": "STARTERS-Q7-01",
  "title": "Starters Q7 Morning",
  "roomId": "uuid",
  "mainTeacherId": "uuid",
  "assistantTeacherId": "uuid",
  "slotTypeId": "uuid",
  "startDate": "2026-06-01",
  "endDate": "2026-08-31",
  "capacity": 16,
  "sessionsToGenerate": 24,
  "skipHolidays": true,
  "weeklyScheduleSlots": [
    {
      "dayOfWeek": "SA",
      "startTime": "08:00",
      "durationMinutes": 90
    }
  ]
}
```

Class response hien co them:

- `syllabusId`
- `syllabusCode`
- `syllabusVersion`
- `syllabusTitle`

FE impact:

1. User phai chon `Branch -> Program -> Level -> Syllabus`
2. FE khong duoc ngam dinh syllabus theo level

---

## 3. Preview Sessions

### `POST /api/classes/preview-sessions`

API nay dung chung contract tao class, nen da can:

- `syllabusId`

FE phai gui cung syllabus da chon o man create class. Neu khong, preview session va class create thuc te co the lech.

---

## 4. Update Class

### `PUT /api/classes/{id}`

Request body hien co:

- `syllabusId`

FE co the cho phep doi syllabus neu business cho phep, backend se validate lai:

- branch support
- program/level match
- syllabus active

---

## 5. Class Read APIs

### `GET /api/classes`

### `GET /api/classes/{id}`

Response da co them:

- `syllabusId`
- `syllabusCode`
- `syllabusVersion`
- `syllabusTitle`

FE goi y:

- list class nen hien badge `code + version`
- detail class nen hien ro syllabus dang chay

---

## 6. Branch Syllabus Lookup

### `GET /api/branches/{id}/syllabuses`

API moi de lay cac syllabus branch duoc phep dung.

Response item:

- `curriculumAssignmentId`
- `syllabusId`
- `programId`
- `programName`
- `levelId`
- `levelName`
- `code`
- `version`
- `title`
- `effectiveFrom`
- `effectiveTo`
- `isActive`

FE use case:

1. user chon branch
2. goi API nay
3. loc theo `programId` va `levelId`
4. render dropdown syllabus version

---

## 7. Syllabus Version Lookup

### `GET /api/syllabuses/versions`

Query params:

- `branchId: Guid?`
- `programId: Guid?`
- `levelId: Guid?`
- `activeOnly: boolean = true`

Response item:

- `syllabusId`
- `programId`
- `programName`
- `levelId`
- `levelName`
- `code`
- `version`
- `title`
- `edition`
- `isActive`

FE use case:

- preload dropdown
- search/filter theo branch/program/level

---

## 8. Import Lesson Plan Words

### `POST /api/syllabuses/import-lesson-plan-words`

Query params hien tai:

- `programId`
- `levelId`
- `syllabusId`
- `moduleId?`
- `overwriteExisting`

Thay doi quan trong:

- `syllabusId` la bat buoc

Y nghia:

- import lesson plan gio bind vao syllabus cu the
- khong con import runtime chi theo module

---

## 9. Import Curriculum Archive

### `POST /api/syllabuses/import-archive`

Request khong doi query shape:

- `programId`
- `levelId`
- `code`
- `version`
- `overwriteExisting`
- multipart `file`

Response da co them parser/debug metadata:

- `archiveFileName`
- `archiveParserVersion`
- `selectedSyllabusEntryName`
- `selectedSyllabusNormalizedEntryName`
- `selectedSyllabusFileName`
- `selectedSyllabusSourceType`
- `selectedSyllabusParserVersion`
- `importedEntries[].normalizedEntryName`
- `importedEntries[].fileName`
- `importedEntries[].sourceType`
- `importedEntries[].parserVersion`
- `importedEntries[].isPrimarySyllabusSource`
- `skippedItems[]`

Behavior moi:

- backend tu chon entry lesson uu tien neu archive co duplicate
- entry duplicate/khong duoc dung se vao `skippedItems[]`

FE khong can tu phan tich archive structure.

---

## 10. Syllabus Unit Lesson Plans

### `GET /api/syllabuses/{id}/unit-lesson-plans`

Day la endpoint FE nen dung de render danh sach lesson theo module/unit cho syllabus.

Thay doi quan trong:

- FE khong duoc parse unit tu `title` hay `lessonPlanUnitName`
- backend tra san field module/unit/order cho tung lesson

Response level:

- `syllabusId`
- `programId`
- `programName`
- `levelId`
- `levelName`
- `totalModules`
- `totalUnits`
- `totalGroups`
- `totalLessonPlans`
- `groups[]`
- `orphanLessons[]`

Group item:

- `moduleId`
- `moduleCode`
- `moduleName`
- `moduleOrder`
- `moduleOrderIndex`
- `unitCount`
- `lessonPlanCount`
- `units[]`

Unit item:

- `unitId`
- `unitName`
- `orderIndex`
- `unitOrderIndex`
- `unitNumber`
- `unitTitle`
- `lessonPlanCount`
- `lessons[]`

Lesson item:

- `lessonPlanTemplateId`
- `moduleId`
- `moduleOrderIndex`
- `lessonPlanUnitId`
- `unitId`
- `unitOrderIndex`
- `unitNumber`
- `unitTitle`
- `sessionTemplateId`
- `title`
- `lessonNumber`
- `sessionIndex`
- `sessionOrder`
- `sessionIndexInModule`
- `sessionTitle`
- `sessionTopic`
- `sourceFileName`
- `orderIndexInUnit`
- `lessonOrderIndexInUnit`
- `isActive`
- `createdAt`
- `updatedAt`

Ordering rule FE phai theo:

1. module sort theo `moduleOrderIndex`
2. unit sort theo `unitOrderIndex`
3. lesson sort theo `lessonOrderIndexInUnit`
4. chi fallback `sessionIndex` neu can debug

Luu y:

- `unitNumber`, `unitTitle`, `unitOrderIndex` da la field authoritative
- FE khong parse lai tu `title`

---

## 11. Unit Canonical Rule

Backend da chuan hoa write path de tranh tao unit trung trong cung module.

Rule canonical:

- trim khoang trang dau/cuoi
- gom nhieu khoang trang thanh 1
- bo nhieu dau cau cuoi nhu `!`, `.`, `,`, `;`, `?`
- chuan hoa `UNIT n`, `UNIT STARTER`, `REVISION n`

Y nghia FE:

- khong can them logic merge unit o FE
- neu DB da duoc clean script, moi unit logic trong 1 module chi con 1 `unitId`

---

## 12. Hard Delete Syllabus

### `DELETE /api/syllabuses/{id}/hard-delete`

API admin moi.

Behavior:

- xoa thang syllabus
- xoa theo cascade lesson plan templates thuoc syllabus do
- xoa cac lesson plan unit bi orphan sau khi xoa neu unit do khong con template nao dung

Guard:

- neu syllabus dang duoc class dung thi backend tra `409`
- neu con `LessonPlan` dang tham chieu toi template cua syllabus do thi backend tra `409`

Success response:

- `id`
- `deletedLessonPlanTemplateCount`
- `deletedLessonPlanUnitCount`

FE use case:

- man admin maintenance
- show confirm dialog vi day la hard delete

---

## 13. FE Checklist

1. Create class phai co dropdown `Syllabus`
2. Preview sessions phai gui `syllabusId`
3. Update class phai support `syllabusId`
4. Class list/detail phai hien `syllabusCode`, `syllabusVersion`, `syllabusTitle`
5. Import lesson plan words phai gui `syllabusId`
6. Import archive result nen render `skippedItems[]` va parser/debug fields
7. Man lesson-by-unit phai dung `unitNumber`, `unitTitle`, `lessonOrderIndexInUnit` tu API, khong parse title
8. Man admin neu co hard delete syllabus thi goi `DELETE /api/syllabuses/{id}/hard-delete`
