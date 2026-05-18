PHASE 2 — ACADEMIC PROGRESSION SYSTEM
(Backend Implementation Spec cho Rex)

1. Mục tiêu Phase 2
Phase 2 tập trung giải quyết:
Academic Progression

Sau Phase 1 + 1.5
system đã hiểu:
- runtime section
- attendance
- ticket consumption
- session type
- slot type
- teaching log

Nhưng chưa hiểu:
- học viên đang ở module nào
- competency thật
- assessment pass/fail
- progression thật
- remedial requirement

Phase 2 sẽ thêm:
Learning Outcome Layer

2. Kiến trúc tổng thể
Program
→ Level
→ Module
→ LessonPlanTemplate

Student
→ StudentProgress
→ Assessment
→ TeacherEvaluation
→ PromotionDecision
→ RemedialPlan

3. Entity mới cần tạo
3.1 Level
Purpose
Tách chương trình thành level thật.

Entity
Level
- Id
- ProgramId
- Code
- Name
- Order
- Description
- IsActive

Ví dụ
Code
Name
STARTERS
Starters
MOVERS
Movers
FLYERS
Flyers


3.2 Module
Purpose
Chia nhỏ Level thành competency chunks.

Entity
Module
- Id
- LevelId
- Code
- Name
- Order
- Description
- PlannedSessionCount
- IsActive

Ví dụ
Module
Meaning
STARTERS_M1
Alphabet
STARTERS_M2
Numbers
STARTERS_M3
Basic Sentences


3.3 LessonPlanTemplate update
Add:
ModuleId
SessionOrder

Ví dụ
Session
Lesson
Module
1
Lesson 1
Module 1
2
Lesson 2
Module 1
3
Lesson 3
Module 1


Mục đích
System biết:
lesson thuộc module nào

3.4 StudentProgress
Đây là lõi của Phase 2

Entity
StudentProgress
- Id
- StudentId
- ModuleId
- Status
- CompletionPercent
- AssessmentStatus
- PromotionStatus
- LastAssessmentId
- CurrentLessonPlanTemplateId
- StartedAt
- CompletedAt

Status enum
Status
NOT_STARTED
IN_PROGRESS
COMPLETED
REMEDIAL_REQUIRED


PromotionStatus
Status
PENDING
PASSED
FAILED
REMEDIAL_REQUIRED


3.5 Assessment
Entity
Assessment
- Id
- StudentId
- ModuleId
- SessionId
- Type
- Score
- Result
- TeacherComment
- AssessedBy
- AssessedAt

Result enum
Result
PASS
FAIL
PENDING


3.6 TeacherEvaluation
Entity
TeacherEvaluation
- Id
- StudentId
- ModuleId
- Speaking
- Listening
- Reading
- Writing
- Participation
- Confidence
- Behavior
- Notes
- EvaluatedBy
- EvaluatedAt

Scale
1–5

3.7 PromotionDecision
Entity
PromotionDecision
- Id
- StudentId
- ModuleId
- Decision
- Reason
- ApprovedBy
- ApprovedAt

Decision enum
Decision
PASS
FAIL
REMEDIAL_REQUIRED


3.8 RemedialPlan
Entity
RemedialPlan
- Id
- StudentId
- ModuleId
- WeakSkills
- RecommendedSessionCount
- Notes
- CreatedBy
- CreatedAt

4. Business Logic
4.1 Lesson Completion Logic
Planned
P38–39

Actual
Only P38 completed

Completion
50%

System:
Carry forward pending content

Update StudentProgress
CompletionPercent

4.2 Module Completion Logic
Rule
Module completed khi:
- all required lesson plans completed
OR
- completion >= threshold

Suggested threshold
80%

4.3 Assessment Logic
PASS
Score >= 70

FAIL
Score < 70

4.4 Promotion Logic
Promotion decision dựa trên:
Assessment
+
TeacherEvaluation
+
Completion

Ví dụ
PASS
Assessment = PASS
Completion >= 80%
Confidence >= 3

FAIL
Assessment FAIL

REMEDIAL
Speaking weak
Confidence low

4.5 Remedial Logic
Khi:
PromotionDecision = REMEDIAL_REQUIRED

System auto create:
RemedialPlan

Ví dụ
WeakSkills:
- speaking
- phonics

Suggested:
2 remedial sessions

5. API cần làm
5.1 Levels
GET /levels
POST /levels
PUT /levels/{id}

5.2 Modules
GET /modules
POST /modules
PUT /modules/{id}

5.3 Student Progress
GET /student-progress/{studentId}
POST /student-progress/update

5.4 Assessments
POST /assessments
GET /assessments/{studentId}

5.5 Teacher Evaluation
POST /teacher-evaluations
GET /teacher-evaluations/{studentId}

5.6 Promotion
POST /promotion-decisions

5.7 Remedial
POST /remedial-plans
GET /remedial-plans/{studentId}

6. Service cần tạo
A. ProgressionService
- calculate completion
- update module status

B. AssessmentService
- create assessment
- calculate pass/fail

C. PromotionService
- evaluate promotion
- create promotion decision

D. RemedialService
- create remedial plan
- assign remedial sessions

7. Runtime Flow thực tế ở Rex
CASE
Starters Module 1

Runtime
24 sessions:
- 18 normal
- 4 review
- 2 native support

Assessment
Score = 45

TeacherEvaluation
Speaking weak
Confidence low

System
PromotionDecision:
REMEDIAL_REQUIRED

Create
RemedialPlan:
2 speaking reinforcement sessions

Đây là điều model cũ không làm được 😄

8. FE cần thêm
Student Progress Screen
- current module
- completion %
- assessment
- progression status

Assessment Form
- score
- teacher comment

Teacher Evaluation Form
- speaking
- confidence
- participation

Academic Dashboard
- delayed students
- weak modules
- remedial required

9. Definition of Done
Phase 2 hoàn thành khi:
1. Có Level + Module structure.
2. LessonPlanTemplate map vào Module.
3. StudentProgress hoạt động.
4. Assessment flow hoạt động.
5. TeacherEvaluation flow hoạt động.
6. PromotionDecision hoạt động.
7. RemedialPlan hoạt động.
8. Academic dashboard tracking competency.

10. Điều QUAN TRỌNG NHẤT
Phase 2 KHÔNG thay đổi vận hành thực tế Rex
Teacher vẫn:
- review
- workshop
- native support
- flexible pacing

Nhưng system bắt đầu hiểu:
học viên thật sự đạt competency chưa

Dưới đây là 1 ví dụ Phase 2 từ đầu đến cuối theo đúng vận hành Rex.
Case: Học viên Minh học Starters Module 1
1. Setup học thuật
Rex có chương trình:
Program: Kids English
Level: Starters
Module: Starters Module 1
Module 1 có lesson plan cố định:
Lesson 1: Alphabet A–D
Lesson 2: Alphabet E–H
Lesson 3: Alphabet I–L
Lesson 4: Review
Lesson 5: Speaking Practice
Lesson 6: Module Assessment

2. Minh đang học trong lớp
Student: Minh
Class: Starters A1
Current Module: Starters Module 1
StudentProgress: IN_PROGRESS
Trong quá trình học, teacher vẫn log thực tế:
Session 2 planned: Alphabet E–H
Actual: chỉ hoàn thành E–F
Completion: 50%
Carry Forward: YES
Hệ thống cập nhật:
StudentProgress của Minh vẫn IN_PROGRESS
Module completion chưa đủ

3. Sau khi học gần hết module
Minh đã hoàn thành khoảng:
CompletionPercent: 85%
Teacher cho Minh làm assessment cuối Module 1.

4. Tạo Assessment
Assessment:
Student: Minh
Module: Starters Module 1
Type: Module Assessment
Score: 78/100
Result: PASS
TeacherComment: Minh nắm tốt alphabet và vocabulary cơ bản.

5. Teacher Evaluation
Teacher đánh giá thêm:
Speaking: 4/5
Listening: 4/5
Reading: 3/5
Writing: 3/5
Participation: 5/5
Confidence: 4/5
Note: Minh tự tin hơn khi speaking, có thể lên module tiếp theo.

6. Promotion Decision
Hệ thống kiểm tra:
Completion >= 80% ✅
Assessment = PASS ✅
TeacherEvaluation đủ tốt ✅
Kết quả:
PromotionDecision: PASS

7. Cập nhật StudentProgress
Module cũ:
Starters Module 1
Status: COMPLETED
CompletedAt: ngày assessment pass
Module mới:
Starters Module 2
Status: IN_PROGRESS
StartedAt: ngày bắt đầu module mới

Nếu Minh fail thì sao?
Giả sử assessment của Minh là:
Score: 45/100
Result: FAIL
TeacherEvaluation:
Speaking: 2/5
Confidence: 2/5
Note: Minh còn yếu phonics và phản xạ nói.
Hệ thống quyết định:
PromotionDecision: REMEDIAL_REQUIRED
Sau đó tạo:
RemedialPlan:
Student: Minh
Module: Starters Module 1
WeakSkills:
- phonics
- speaking confidence
RecommendedSessionCount: 2
Admin/Academic Manager tạo lớp phụ đạo:
Class: Starters M1 Remedial
SessionType: REMEDIAL
SlotType: REMEDIAL_SUPPORT
Minh học 2 buổi phụ đạo, sau đó assessment lại.
Nếu pass:
Starters Module 1 → COMPLETED
Starters Module 2 → IN_PROGRESS

Flow Phase 2 đầy đủ
1. Setup Level + Module
2. Map LessonPlanTemplate vào Module
3. Student học trong class
4. TeachingLog cập nhật actual progress
5. StudentProgress tăng theo completion
6. Student làm Assessment
7. Teacher tạo Evaluation
8. System tạo PromotionDecision
9. Nếu pass → move next module
10. Nếu fail → RemedialPlan
11. Sau remedial → reassessment
12. Pass thì promote
Điểm khác với model cũ
Model cũ:
Minh học đủ buổi → xem như xong

Model mới:
Minh chỉ được lên module khi:
- completion đủ
- assessment pass
- teacher evaluation đạt

