1. Tư duy lõi của mô hình mới 
Mô hình mới không lấy khóa học cố định làm trung tâm nữa.
Nó lấy 3 thứ làm trung tâm:
Curriculum = khung học thuật
Ticket = quyền học
Section = buổi học thực tế
Câu chốt:
Curriculum định nghĩa học gì. Ticket cho biết học viên có quyền học bao nhiêu buổi. Section là nơi việc dạy học thật sự diễn ra.

2. Kiến trúc tổng thể
[1] Academic Layer
Program → Level → Module → Lesson Template

       ↓

[2] Commercial Layer
Package → Order Package → Ticket Item → Ticket Ledger

       ↓

[3] Operation Layer
Branch → Class → Schedule / Slot → Enrollment

       ↓

[4] Runtime Teaching Layer
Section Runtime → Attendance → Teaching Log → Homework

       ↓

[5] Progression Layer
Assessment → Promotion / Remedial / Repeat

       ↓

[6] Reporting Layer
Parent Report / Academic Report / AI Report

3. Giải thích từng tầng
1. Academic Layer — Tầng học thuật
Tầng này trả lời:
Trung tâm dạy chương trình gì?
Level nào?
Module nào?
Bài học nào?
Ví dụ:
Program: Kids English
Level: Starters
Module: Starters Module 1
Lesson: Lesson 5 - Colors
Cấu trúc:
Program
→ Level
→ Module
→ Lesson Template
Điểm quan trọng:
Lesson Template chỉ là giáo án mẫu, không phải buổi học thật.

2. Commercial Layer — Tầng thương mại
Tầng này trả lời:
Phụ huynh mua gói nào?
Có bao nhiêu vé học?
Đã dùng bao nhiêu?
Còn bao nhiêu?
Ví dụ:
Minh mua gói Starters 24
→ hệ thống tạo 24 tickets
Cấu trúc:
Package
→ Order Package
→ Ticket Item
→ Ticket Ledger
Điểm quan trọng:
Package không quyết định học viên lên level. Package chỉ tạo quyền tham gia buổi học.

3. Operation Layer — Tầng vận hành lớp học
Tầng này trả lời:
Lớp nào?
Ai dạy?
Học ở đâu?
Lịch nào?
Học viên nào đang thuộc lớp?
Ví dụ:
Class: Starters A1
Teacher: Ms. Hoa
Schedule: T3-T5, 18:00–19:30
Student Minh enroll vào lớp từ 10/05
Cấu trúc:
Branch
→ Class
→ Schedule / Slot
→ Enrollment
Điểm quan trọng:
Class là lớp vận hành, không phải khóa học cố định.

4. Runtime Teaching Layer — Tầng dạy học thực tế
Đây là tầng quan trọng nhất.
Tầng này trả lời:
Hôm nay lớp học buổi nào?
Dạy gì thật?
Ai đi học?
Có trừ ticket không?
Teacher ghi chú gì?
Ví dụ:
Section ngày 15/05
Class: Starters A1
Based on Lesson 5

Teacher dạy thật:
- pages 20–21
- ôn grammar
- homework page 15

Minh present → trừ 1 ticket
Cấu trúc:
Section Runtime
→ Attendance
→ Teaching Log
→ Homework
→ Ticket Consumption
Điểm quan trọng:
Section Runtime có thể không làm tăng Lesson Progression.
Ví dụ review trước kỳ thi:
Section Type: Review
Consume ticket: Yes
Advance lesson: No

5. Progression Layer — Tầng đánh giá và lên cấp
Tầng này trả lời:
Học viên có đủ năng lực lên module/level mới không?
Nếu chưa đạt thì xử lý sao?
Ví dụ:
Minh hoàn thành Module 1
Assessment:
- Listening 85
- Speaking 70
Result: Pass
→ lên Starters Module 2
Nếu fail:
→ chuyển sang Remedial Class
→ add free tickets nếu trung tâm hỗ trợ
Cấu trúc:
Assessment
→ Teacher Evaluation
→ Promotion
→ Remedial Class
Điểm quan trọng:
Promotion dựa trên năng lực, không dựa trên số buổi đã mua.

6. Reporting Layer — Tầng báo cáo
Tầng này tổng hợp dữ liệu từ runtime.
Report lấy từ:
Attendance
Teaching Log
Homework
Ticket Usage
Assessment
Teacher Notes
Progress
Ví dụ report:
Minh đã học 18/24 tickets.
Hiện đang ở Starters Module 1.
Vocabulary tốt, speaking còn chậm.
Teacher đề xuất học thêm 3 remedial sections trước assessment lại.

4. Sơ đồ luồng hoạt động
Admin setup curriculum
       ↓
Create Program / Level / Module / Lesson Template
       ↓
Create Package 24 / 48
       ↓
Parent buys package
       ↓
System creates tickets
       ↓
Student placement test
       ↓
Admin enrolls student into suitable class
       ↓
Class generates section by schedule
       ↓
Teacher teaches section
       ↓
Attendance + ticket consumption
       ↓
Teaching log + homework
       ↓
Assessment
       ↓
Promote / Remedial / Continue
       ↓
Report to parent

5. Ví dụ đầy đủ một case Rex
Bước 1: Setup học thuật
Kids English
→ Starters
→ Module 1
→ Lesson 1–24
Bước 2: Setup package
Starters 24
→ tạo 24 tickets
Bước 3: Học viên mua gói
Minh mua Starters 24
→ Minh có 24 Starters Tickets
Bước 4: Xếp lớp
Minh placement phù hợp Starters Module 1
→ enroll vào Starters A1
Bước 5: Buổi học thật
Section 15/05
Based on Lesson 5
Teacher dạy pages 20–21
Minh present
→ consume 1 ticket
Bước 6: Gần kỳ thi
Section 20/05
Type: Review
Teacher ôn grammar cho kỳ thi trường
Minh present
→ consume 1 ticket
→ không advance lesson progression
Bước 7: Đánh giá
Minh làm Module Assessment
Pass
→ lên Starters Module 2
Nếu fail:
→ enroll vào Starters M1 Remedial
→ nếu free thì add extra tickets 0đ

6. Kiến trúc database mức entity
Academic:
- Program
- Level
- Module
- LessonTemplate
- LessonVersion

Commercial:
- Package
- PackageCurriculumMapping
- OrderPackage
- TicketType
- TicketItem
- TicketLedger

Operation:
- Branch
- Class
- SlotType
- Schedule
- Teacher
- Student
- Enrollment

Runtime:
- Section
- SectionType
- SectionLessonSnapshot
- Attendance
- TeachingLog
- Homework
- SectionStudentDetail

Progression:
- Assessment
- AssessmentResult
- TeacherEvaluation
- Promotion
- RemedialClass

Reporting:
- ParentReport
- AcademicReport
- AIReport



Kết luận chính
Backend hiện tại đang là:
Class-centric + Registration/TuitionPlan-centric
Còn model mới cần chuyển sang:
Section Runtime + Ticket Ledger-centric
Nghĩa là thay đổi lớn nhất không phải đổi tên table, mà là đổi nguồn sự thật:
Cũ:
Registration.RemainingSessions là nguồn sự thật số buổi còn lại.

Mới:
TicketLedger là nguồn sự thật số buổi còn lại.
Việc bắt buộc làm trước demo
Chỉ cần tập trung 4 việc này:
1. Thêm SectionType cho Session
Giữ Session hiện tại, chưa cần rename ngay.
Thêm:
SectionType:
- normal
- review
- makeup
- remedial
- assessment
Mục tiêu:
Review section vẫn trừ vé nhưng không advance lesson progression.
2. Tạo TicketItem và TicketLedger
Khi mua package:
OrderPackage → generate TicketItem → TicketLedger +24/+48
Khi đi học:
Attendance Present → TicketLedger -1
Không nên trừ trực tiếp Registration.RemainingSessions nữa.
3. Tách consume khỏi Registration
Registration chỉ nên giữ vai trò:
học viên đang có gói / đang được đăng ký học
Không nên là nơi quyết định:
học xong level
4. Attendance gọi TicketConsumptionPolicyService
Flow mới:
Mark attendance
→ check attendance status
→ check section type
→ consume ticket or not
→ create ticket ledger transaction
Ví dụ:
Present + normal section → -1 ticket
Present + review section → -1 ticket
Absent with notice → 0 ticket
Absent without notice → -1 ticket
Backlog code nên chia như này
Phase 1 — Bắt buộc, ít phá nhất
1. Add SectionType enum/lookup.
2. Add OrderPackage nếu chưa có.
3. Add TicketItem.
4. Add TicketLedger.
5. Add TicketConsumptionPolicyService.
6. Update AttendanceService:
  - không update RemainingSessions trực tiếp
  - tạo TicketLedger transaction
7. Giữ RemainingSessions tạm thời là derived/cache nếu cần.
Phase 2 — Đúng học thuật hơn
1. Add Level.
2. Add Module.
3. Map LessonPlanTemplate vào Module.
4. Add StudentProgress.
5. Add Assessment theo Module.
6. Add TeacherEvaluation.
7. Promotion dựa trên Assessment + Evaluation.
Phase 3 — Hoàn chỉnh vận hành Rex
1. RemedialClass / RemedialEnrollment.
2. Free ticket policy.
3. SlotType compatibility.
4. Ticket conversion.
5. Branch curriculum pricing.
6. AI report từ runtime data.

