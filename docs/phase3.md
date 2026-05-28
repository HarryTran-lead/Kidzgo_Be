PHASE CUỐI — REPORTING, AI INSIGHT & COMMUNICATION
Mục tiêu:
Biến dữ liệu từ ticket/runtime/progression/syllabus thành report, cảnh báo và hành động cụ thể cho:
Phụ huynh
Giáo viên
Academic manager
CS/Admin
Center manager
Phase này KHÔNG sửa:
TicketLedger
Attendance logic
StudentProgress
Assessment
PromotionDecision
Syllabus planning
Chỉ READ DATA → SNAPSHOT → INSIGHT → REPORT → ACTION.
==================================================
ENTITY CẦN THÊM
==================================================
ReportTemplate
Dùng để định nghĩa mẫu báo cáo.
Fields:
Id
Code
Name
Type: parent / academic / class / branch / internal
ContentSchema
IsActive
CreatedAt
Case:
Parent report dùng template khác academic dashboard.

ReportPeriod
Kỳ báo cáo.
Fields:
Id
Code
Name
StartDate
EndDate
Type: weekly / monthly / module / custom
Case:
Báo cáo tháng 6/2026.
Báo cáo sau khi kết thúc Module 1.

ReportRun
Một lần chạy generate report.
Fields:
Id
ReportTemplateId
ReportPeriodId
ClassId nullable
StudentId nullable
BranchId nullable
Status: pending / processing / completed / failed
GeneratedBy
GeneratedAt
ErrorMessage nullable
Case:
Academic manager generate report cho cả lớp Starters A1.

StudentReport
Report chính theo từng học viên.
Fields:
Id
StudentId
ClassId
BranchId
ModuleId nullable
SyllabusId nullable
ReportPeriodId
ReportType
SnapshotJson
SummaryText
Status
CreatedAt
Case:
Report của Minh trong tháng 6.

AIInsight
Insight sinh từ rule hoặc AI.
Fields:
Id
StudentReportId
InsightType: strength / weakness / risk / recommendation / note
Content
ConfidenceScore
SourceDataJson
CreatedAt
Case:
"Minh tiến bộ tốt phần vocabulary nhưng còn yếu speaking confidence."

RiskAlert
Cảnh báo cần xử lý.
Fields:
Id
StudentId
ClassId nullable
BranchId nullable
RiskType
Severity: low / medium / high
Reason
Source
Status: open / resolved / ignored
CreatedAt
ResolvedAt nullable
Case:
Minh nghỉ không phép 2 buổi liên tiếp → tạo risk alert.

Recommendation
Gợi ý hành động.
Fields:
Id
StudentId
ClassId nullable
RecommendationType
Content
Priority
AssignedRole: teacher / academic_manager / cs / admin
Status: pending / accepted / rejected / done
CreatedAt
CompletedAt nullable
Case:
Đề xuất CS gọi phụ huynh vì học viên sắp hết ticket.

ReportShareLog
Log gửi report.
Fields:
Id
StudentReportId
RecipientName
RecipientContact
Channel: app / email / zalo / sms
Status: sent / failed / viewed
SentAt
ViewedAt nullable
Case:
Gửi report cho phụ huynh qua app/Zalo.
==================================================
2. REPORT SNAPSHOT JSON CHUẨN
{
"student": {
"id": "",
"name": "",
"branch": "",
"class": ""
},
"academic_context": {
"program": "",
"level": "",
"module": "",
"syllabus": "",
"syllabus_version": ""
},
"period": {
"from": "",
"to": "",
"type": ""
},
"attendance_summary": {
"total_sections": 0,
"present": 0,
"late": 0,
"absent_with_notice": 0,
"absent_without_notice": 0,
"attendance_rate": 0
},
"ticket_summary": {
"granted": 0,
"consumed": 0,
"remaining": 0,
"package_expiring": false
},
"runtime_summary": {
"normal_sections": 0,
"review_sections": 0,
"makeup_sections": 0,
"remedial_sections": 0,
"assessment_sections": 0
},
"learning_progress": {
"completion_percent": 0,
"current_status": "",
"promotion_status": "",
"current_lesson": ""
},
"assessment_summary": {
"latest_score": null,
"latest_result": "",
"teacher_comment": ""
},
"teacher_evaluation": {
"speaking": null,
"listening": null,
"reading": null,
"writing": null,
"participation": null,
"confidence": null,
"notes": ""
},
"strengths": [],
"weaknesses": [],
"risks": [],
"recommendations": [],
"parent_message": "",
"internal_notes": ""
}
Rule:
Snapshot immutable.
Report đã generate rồi thì không tự đổi theo data mới.
==================================================
3. SERVICE CẦN LÀM
ReportAggregationService
Input:
studentId / classId / branchId
period
Output:
attendance summary
ticket summary
runtime summary
progress summary
assessment summary
teacher evaluation summary

StudentReportService
Nhiệm vụ:
build StudentReport
save SnapshotJson
save SummaryText

ParentReportService
Nhiệm vụ:
convert report thành ngôn ngữ phụ huynh dễ hiểu
ẩn internal note
ẩn dữ liệu nhạy cảm không cần thiết

AcademicReportService
Nhiệm vụ:
report nội bộ cho academic manager
show risk, weakness, completion, assessment

AIInsightService
Nhiệm vụ:
nhận snapshot
sinh strengths, weaknesses, recommendations
có thể bắt đầu bằng rule-based, sau này gắn AI thật

RiskDetectionService
Nhiệm vụ:
detect risk từ attendance/progress/ticket/assessment

RecommendationService
Nhiệm vụ:
tạo action cụ thể cho teacher/CS/academic manager

ReportShareService
Nhiệm vụ:
share report
ghi ReportShareLog
==================================================
4. API CẦN THÊM
POST /api/reports/generate
Request:
{
"reportType": "parent",
"studentId": "...",
"classId": "...",
"periodId": "..."
}
Response:
{
"reportRunId": "...",
"studentReportId": "...",
"status": "completed"
}

GET /api/reports/{id}

GET /api/students/{id}/reports

GET /api/students/{id}/reports/latest

GET /api/students/{id}/parent-report

GET /api/classes/{id}/academic-dashboard

GET /api/classes/{id}/risk-alerts

GET /api/students/{id}/recommendations

POST /api/reports/{id}/share
Request:
{
"channel": "zalo",
"recipientName": "Parent of Minh",
"recipientContact": "..."
}
==================================================
5. CASE CỤ THỂ CHO BE
CASE 1 — Học viên đi học tốt, tiến bộ tốt
Data:
Attendance rate: 95%
Completion: 85%
Assessment: PASS, score 82
TeacherEvaluation:
Speaking 4
Listening 4
Confidence 4
Expected:
Không tạo RiskAlert
StudentReport strengths gồm:
attendance tốt
nắm bài tốt
tự tin speaking
Recommendation:
continue next module
Parent message:
"Bé duy trì tốt tiến độ học và có thể tiếp tục module tiếp theo."

CASE 2 — Học viên nghỉ nhiều
Data:
Attendance rate: 55%
Absent without notice: 3
Completion: 40%
Expected:
Tạo RiskAlert:
RiskType = low_attendance
Severity = high
Tạo Recommendation:
AssignedRole = cs
Content = "CS liên hệ phụ huynh để xác nhận lịch học và lý do nghỉ."
ParentReport:
báo nhẹ nhàng, không dùng chữ quá nặng.
AcademicReport:
show rõ attendance risk.

CASE 3 — Học viên học đủ buổi nhưng assessment fail
Data:
Attendance rate: 90%
Completion: 85%
Assessment: FAIL, score 45
Speaking: 2
Confidence: 2
Expected:
Tạo RiskAlert:
RiskType = academic_fail
Severity = high
Recommendation:
AssignedRole = academic_manager
Content = "Đề xuất remedial 2 buổi trước khi reassessment."
ParentMessage:
"Bé cần thêm thời gian củng cố speaking và confidence trước khi học phần tiếp theo."
Important:
Phase cuối KHÔNG được tự update PromotionDecision.
Chỉ tạo recommendation.

CASE 4 — Học viên sắp hết ticket
Data:
Remaining ticket: 2
Current module chưa xong
Attendance bình thường
Expected:
RiskAlert:
RiskType = package_expiring
Severity = medium
Recommendation:
AssignedRole = cs
Content = "CS tư vấn phụ huynh gia hạn package."
ParentReport:
có thể show "Số buổi còn lại: 2"
Không ảnh hưởng StudentProgress.

CASE 5 — Lớp review quá nhiều, chậm curriculum
Data:
Class Starters A1:
10 sections trong kỳ
5 review sections
normal sections thấp
Class progress chậm hơn expected
Expected:
Class academic dashboard show:
review ratio = 50%
curriculum delay risk
RiskAlert:
RiskType = class_curriculum_delay
Severity = medium
Recommendation:
AssignedRole = academic_manager
Content = "Review lại pacing của lớp và teaching plan."

CASE 6 — Học viên có remedial plan
Data:
Student has RemedialPlan
Remedial sessions attended: 2
Reassessment not yet done
Expected:
ParentReport:
"Bé đã tham gia các buổi củng cố."
AcademicReport:
show remedial status = in progress
Recommendation:
AssignedRole = teacher
Content = "Cần reassessment sau remedial."
Không tự pass/fail học viên.

CASE 7 — Branch manager xem dashboard
Data:
Branch Q7:
10 active classes
3 classes có low attendance
2 classes curriculum delay
5 students package expiring
Expected:
GET /api/branches/{id}/dashboard
Response include:
total active classes
total active students
risk students
risk classes
package expiring count
assessment fail count

CASE 8 — Parent report khác academic report
Same student.
Parent report:
ngôn ngữ nhẹ nhàng
tập trung tiến bộ và đề xuất hỗ trợ
Academic report:
show đầy đủ risk
show score
show weak skill
show recommendation nội bộ
Expected:
2 report type khác nhau nhưng lấy cùng SnapshotJson.

CASE 9 — Data thay đổi sau khi generate report
Data:
Generate report ngày 01/06
Sau đó teacher sửa attendance ngày 02/06
Expected:
Report ngày 01/06 không đổi
Generate report mới thì data mới được tính
Snapshot immutable.

CASE 10 — Share report cho phụ huynh
Action:
POST /api/reports/{id}/share
Expected:
tạo ReportShareLog
status sent hoặc failed
nếu parent mở report thì update viewed
không generate lại report.
==================================================
6. RISK RULE MVP
low_attendance
Condition:
attendance_rate < 70%
attendance_discipline
Condition:
absent_without_notice >= 2
learning_delay
Condition:
completion_percent < expected_completion_percent
academic_fail
Condition:
latest_assessment_result = FAIL
weak_communication
Condition:
speaking <= 2 OR confidence <= 2
package_expiring
Condition:
remaining_ticket <= 3
class_curriculum_delay
Condition:
class actual progress < planned progress
high_review_ratio
Condition:
review_sections / total_sections >= 40%
==================================================
7. RECOMMENDATION RULE MVP
low_attendance:
→ CS contact parent
attendance_discipline:
→ Admin confirm schedule/attendance policy
learning_delay:
→ Teacher add review support
academic_fail:
→ Academic manager create remedial recommendation
weak_communication:
→ Teacher increase speaking activity
package_expiring:
→ CS renewal follow-up
class_curriculum_delay:
→ Academic manager review pacing
high_review_ratio:
→ Academic manager check teaching plan
==================================================
8. DASHBOARD CẦN CÓ
A. Student Report Page
Show:
attendance
progress
assessment
ticket
strengths
weaknesses
recommendations
B. Parent Report View
Show:
progress summary
attendance summary
teacher comment
next recommendation
C. Academic Dashboard
Show:
weak students
delayed students
failed assessments
remedial required
class pacing
D. Branch Dashboard
Show:
active classes
active students
risk students
delayed classes
expiring packages
E. CS Dashboard
Show:
students package expiring
low attendance students
parent follow-up list
==================================================
9. DEFINITION OF DONE
Phase cuối xong khi:
Generate được report theo student/class/branch/period.
Report lưu SnapshotJson immutable.
Có ParentReport và AcademicReport khác nhau.
Có RiskAlert theo rule MVP.
Có Recommendation theo role.
Có report history.
Có share log.
Có class academic dashboard.
Có branch dashboard.
Có CS follow-up list.
Không update trực tiếp TicketLedger, Attendance, StudentProgress, Assessment, PromotionDecision.
Data thay đổi sau report không làm report cũ đổi.

