Dưới đây là Phase 1 chi tiết cho BE. Mục tiêu là sửa ít nhất nhưng đổi đúng lõi từ model cũ sang model mới.
Phase 1 — Ticket + Section Runtime Core
Mục tiêu Phase 1
Chuyển backend từ:
Registration.RemainingSessions / UsedSessions
sang:
TicketItem + TicketLedger
và bổ sung:
SectionType
để xử lý các buổi học thực tế như normal, review, makeup, remedial, assessment.

1. Giữ lại entity cũ nào?
Phase 1 không rewrite toàn bộ.
Vẫn giữ:
Student
Class
Session
Attendance
Registration
TuitionPlan
LessonPlanTemplate
LessonPlan
Nhưng đổi vai trò:
Entity cũ
Vai trò mới trong Phase 1
TuitionPlan
Package template / gói bán
Registration
Quan hệ học viên đã mua/đăng ký gói
Session
Tạm xem là SectionRuntime
RemainingSessions
Chỉ nên là cache/display, không là source of truth
Attendance
Trigger ticket consumption


2. Thêm SectionType cho Session
Cần thêm field vào Session
SectionType
Giá trị đề xuất:
normal
review
makeup
remedial
assessment
Ý nghĩa từng loại
SectionType
Ý nghĩa
Có trừ ticket?
Có advance lesson không?
normal
Buổi học theo lesson plan
Có
Có
review
Ôn thi / ôn tập
Có
Không
makeup
Học bù
Tùy policy
Tùy
remedial
Phụ đạo / củng cố
Tùy policy
Không hoặc tùy
assessment
Kiểm tra năng lực
Tùy policy
Không

Ví dụ
Session 18
Type: review
Content: ôn thi trường
Consume ticket: yes
Advance lesson progression: no
BE cần làm
- Add enum SectionType.
- Add column SectionType vào Session.
- Default existing Session = normal.
- API create/update session cho phép truyền sectionType.
- Response session trả về sectionType.

3. Thêm TicketItem
TicketItem là gì?
TicketItem là từng “vé học” cụ thể của học viên.
Khi phụ huynh mua package 24, hệ thống tạo 24 TicketItem.
Entity đề xuất
TicketItem
- Id
- StudentId
- RegistrationId
- TuitionPlanId
- TicketTypeId / SlotTypeId optional
- Status: available / consumed / expired / voided
- Source: purchase / free_grant / adjustment
- CreatedAt
- ConsumedAt nullable
- ConsumedBySessionId nullable
Phase 1 đơn giản
Nếu chưa có TicketTypeId thì có thể để sau, chỉ cần:
StudentId
RegistrationId
Status
ConsumedBySessionId
Ví dụ
Minh mua gói 24
→ tạo TicketItem #1 đến #24
Status: available

4. Thêm TicketLedger
TicketLedger là gì?
Là sổ lịch sử cộng/trừ vé.
Không chỉ biết còn bao nhiêu, mà biết vì sao cộng/trừ.
Entity đề xuất
TicketLedger
- Id
- StudentId
- RegistrationId
- TicketItemId nullable
- SessionId nullable
- AttendanceId nullable
- TransactionType: grant / consume / refund / void / adjustment
- Quantity: +1 / -1
- Reason
- CreatedAt
- CreatedBy
Ví dụ ledger
+24 tickets
Reason: Purchase Starters 24

-1 ticket
Reason: Present in Session 15/05

+5 tickets
Reason: Free remedial support
Phase 1 tối thiểu
Khi mua gói:
Tạo 24 TicketItem
Tạo 24 dòng ledger +1
Hoặc nếu muốn gọn:
Tạo 24 TicketItem
Tạo 1 dòng ledger +24
Khuyến nghị demo:
1 dòng ledger +24
mỗi lần học tạo ledger -1
Dễ nhìn hơn.

5. Tạo TicketConsumptionPolicyService
Mục đích
Không để AttendanceService tự quyết trừ buổi.
Tách rule ra service riêng.
TicketConsumptionPolicyService
Input
StudentId
RegistrationId
SessionId
AttendanceStatus
SectionType
Output
ShouldConsumeTicket: true/false
Quantity: 0/1
Reason
AdvanceLessonProgression: true/false
Rule Phase 1 đề xuất
AttendanceStatus
SectionType
Consume Ticket
Advance Lesson
present
normal
1
yes
present
review
1
no
present
remedial
1 hoặc 0 theo config
no
present
makeup
0 hoặc 1 theo config
no/tùy
present
assessment
1 hoặc 0 theo config
no
absent_with_notice
any
0
no
absent_without_notice
any
1
no
late
any
1
tùy section type

Phase 1 đơn giản nhất
present => consume 1
late => consume 1
absent_with_notice => consume 0
absent_without_notice => consume 1
SectionType chỉ quyết định:
normal => advance lesson
review/remedial/makeup/assessment => không advance lesson

6. Sửa AttendanceService
Hiện tại có thể đang làm
Attendance present
→ Registration.UsedSessions += 1
→ Registration.RemainingSessions -= 1
Cần đổi thành
Attendance present
→ call TicketConsumptionPolicyService
→ find available TicketItem
→ mark TicketItem consumed
→ create TicketLedger -1
→ update RemainingSessions cache nếu còn dùng
Flow chuẩn
1. Teacher/Admin mark attendance.
2. AttendanceService lưu Attendance.
3. AttendanceService gọi TicketConsumptionPolicyService.
4. Nếu ShouldConsumeTicket = true:
  - tìm 1 TicketItem available của student/registration
  - set TicketItem.Status = consumed
  - set ConsumedBySessionId = sessionId
  - set ConsumedAt = now
  - tạo TicketLedger transaction -1
5. Nếu ShouldConsumeTicket = false:
  - không consume ticket
  - có thể tạo ledger 0 hoặc skip
6. Return ticket balance mới cho FE.
Nếu không còn ticket?
Trả lỗi rõ:
No available ticket for this student.
Hoặc cho phép:
overdraft / pending payment
nhưng Phase 1 nên chặn.

7. Sửa flow mua package / registration
Hiện tại
Create Registration
→ set TotalSessions
→ set RemainingSessions
Phase 1 mới
Create Registration / OrderPackage
→ generate TicketItems
→ create TicketLedger +N
Nếu chưa muốn thêm OrderPackage
Có thể dùng Registration tạm làm OrderPackage.
Registration = purchased package record
Vậy khi tạo Registration:
TuitionPlan.NumberOfSessions = 24
→ create 24 TicketItem
→ create TicketLedger +24
BE cần làm
- Tìm service tạo Registration hiện tại.
- Sau khi Registration created, gọi TicketGrantService.
- TicketGrantService tạo TicketItem.
- TicketGrantService tạo TicketLedger +N.

8. Thêm TicketGrantService
Mục đích
Cấp vé khi:
- mua package
- tặng thêm buổi
- phụ đạo free
- adjustment
Method đề xuất
GrantTickets(studentId, registrationId, quantity, reason, source)
Ví dụ
GrantTickets(Minh, Reg001, 24, "Purchase Starters 24", "purchase")
Sau này dùng cho free remedial:
GrantTickets(Minh, Reg001, 5, "Free remedial support", "free_grant")

9. API cần thêm/sửa
9.1 Create registration / package purchase
POST /registrations
hoặc:
POST /orders/packages
Response nên trả:
{
 "registrationId": "...",
 "studentId": "...",
 "packageName": "Starters 24",
 "ticketsGranted": 24,
 "ticketBalance": 24
}

9.2 Get ticket balance
GET /students/{studentId}/tickets/balance
Response:
{
 "studentId": "...",
 "available": 23,
 "consumed": 1,
 "totalGranted": 24
}

9.3 Get ticket ledger
GET /students/{studentId}/tickets/ledger
Response:
[
 {
   "transactionType": "grant",
   "quantity": 24,
   "reason": "Purchase Starters 24"
 },
 {
   "transactionType": "consume",
   "quantity": -1,
   "reason": "Present in Section 15/05"
 }
]

9.4 Create / update session
POST /sessions
PUT /sessions/{id}
Thêm:
{
 "sectionType": "normal"
}

9.5 Mark attendance
POST /sessions/{sessionId}/attendance
Request:
{
 "studentId": "...",
 "status": "present"
}
Response nên trả:
{
 "attendanceId": "...",
 "ticketConsumed": true,
 "consumedQuantity": 1,
 "ticketBalance": 23,
 "advanceLessonProgression": true
}

10. RemainingSessions xử lý sao?
Không xóa ngay
Vì FE có thể đang dùng.
Phase 1 nên đổi thành cache/display
RemainingSessions = count(TicketItem where status = available)
Hoặc cập nhật đồng bộ tạm:
sau mỗi ledger transaction, update RemainingSessions cache
Nhưng phải ghi rõ:
Source of truth = TicketLedger / TicketItem
Không còn là Registration.

11. Migration dữ liệu cũ
Nếu hệ thống đã có Registration cũ:
Cần script migrate
Với mỗi Registration:
available = Registration.RemainingSessions
used = Registration.UsedSessions
total = available + used
Tạo:
available TicketItems
consumed TicketItems
TicketLedger +total
TicketLedger -used
Ví dụ
Registration cũ:
Total: 24
Used: 5
Remaining: 19
Migration:
Create 24 TicketItems
Mark 5 consumed
19 available
Ledger +24
Ledger -5

12. Business rule cần chốt trong Phase 1
BE cần config được ít nhất:
Present consume ticket?
Late consume ticket?
Absent with notice consume ticket?
Absent without notice consume ticket?
Review section consume ticket?
Review section advance lesson?
MVP config:
Present = consume 1
Late = consume 1
Absent with notice = consume 0
Absent without notice = consume 1
Review = consume 1, advance lesson 0
Normal = consume 1, advance lesson 1

13. Test case bắt buộc
Test 1 — Mua package 24
Expected:
Registration created
24 TicketItems created
TicketLedger +24
Balance = 24
Test 2 — Đi học normal section
Expected:
Attendance present
1 TicketItem consumed
TicketLedger -1
Balance = 23
AdvanceLessonProgression = true
Test 3 — Review section
Expected:
Attendance present
TicketLedger -1
Balance = 22
AdvanceLessonProgression = false
Test 4 — Absent with notice
Expected:
Attendance absent_with_notice
No ticket consumed
Balance unchanged
Test 5 — Absent without notice
Expected:
Attendance absent_without_notice
TicketLedger -1
Balance decreases
Test 6 — No ticket left
Expected:
Attendance present
Reject consume
Return error: No available ticket

14. FE bị ảnh hưởng gì trong Phase 1?
FE cần đổi các màn:
1. Student package detail:
  hiển thị ticket balance từ ticket API.

2. Attendance screen:
  sau khi điểm danh hiển thị consumed ticket / balance mới.

3. Session create/edit:
  thêm SectionType.

4. Student detail:
  thêm ticket ledger/history.
FE chưa cần làm:
- full module progression
- remedial class full
- AI report

15. Definition of Done cho Phase 1
Phase 1 được xem là xong khi:
1. Mua package tạo ticket.
2. Đi học trừ ticket qua ledger.
3. Attendance không trừ trực tiếp Registration nữa.
4. Session có SectionType.
5. Review section trừ ticket nhưng không advance lesson progression.
6. FE xem được balance và ledger cơ bản.
7. RemainingSessions không còn là source of truth.

