Phase 1.5 — TicketType / SlotType Compatibility
(Bước chuyển tiếp để Rex vận hành thật ngoài đời)

Mục tiêu của Phase 1.5
Sau khi Phase 1 đã có:
- TicketLedger
- TicketItem
- Attendance consume ticket
- SectionType
thì vấn đề tiếp theo của Rex là:
Không phải mọi class đều cùng giá trị.
Ví dụ thực tế:
- teacher khác giá
- giờ học khác giá
- lớp native khác giá
- lớp VIP khác giá
- weekend khác weekday

Nên Phase 1.5 giải quyết:
Ticket nào được học class nào?

KHÔNG giải quyết ngay:
❌
- conversion phức tạp
- surcharge engine
- pricing engine enterprise

Chỉ giải quyết:
Compatibility
TicketType ↔ SlotType

1. Tư duy lõi của Phase 1.5
Trước đây
Package 24
→ học đâu cũng được

Nhưng thực tế Rex:
❌ không đúng

Vì:
Premium teacher
≠
Standard teacher

Nên model mới:
Package
→ sinh ra TicketType

Class / Section
→ có SlotType

System check:
TicketType compatible với SlotType?

2. Kiến trúc mới
TuitionPlan
↓
TicketType
↓
TicketItem

Class
↓
SlotType

Attendance
↓
Compatibility Check
↓
Consume Ticket

3. Entity cần thêm
A. TicketType
Mục đích
Định nghĩa:
vé này dùng cho loại học nào

Entity
TicketType
- Id
- Code
- Name
- Description
- IsActive

Ví dụ
Code
Meaning
STANDARD
lớp thường
PREMIUM
giáo viên premium
WEEKEND
lớp cuối tuần
IELTS
lớp IELTS
KIDS
lớp Kids


B. SlotType
Mục đích
Định nghĩa:
class/section thuộc loại vận hành nào

Entity
SlotType
- Id
- Code
- Name
- Description
- IsActive

Ví dụ
SlotType
Meaning
STANDARD
lớp thường
PREMIUM
lớp premium
WEEKEND
lớp cuối tuần


C. TicketTypeCompatibility
Mục đích
Cho phép mapping linh hoạt.

Entity
TicketTypeCompatibility
- Id
- TicketTypeId
- SlotTypeId
- IsCompatible

Ví dụ
TicketType
SlotType
Compatible
STANDARD
STANDARD
YES
STANDARD
PREMIUM
NO
PREMIUM
STANDARD
YES
PREMIUM
PREMIUM
YES


Ý nghĩa
Premium ticket:
có thể học standard
Nhưng standard ticket:
không học premium được

4. Sửa TuitionPlan
Hiện tại
TuitionPlan
- Name
- Price
- NumberOfSessions

Cần thêm
TicketTypeId

Ví dụ
TuitionPlan:
Starters 24 Standard
↓
TicketType = STANDARD

Khi mua package
Generate 24 STANDARD TicketItems

5. Sửa TicketItem
Thêm:
TicketTypeId

Ví dụ
TicketItem #001
Type = STANDARD
Status = available

6. Sửa Class
Thêm:
SlotTypeId

Ví dụ
Class:
Starters A1
SlotType = STANDARD

Hoặc
Class:
Native Starters
SlotType = PREMIUM

7. Sửa SessionRuntime
Session inherit SlotType từ Class
hoặc override được.

Entity
Session
- SlotTypeId

Vì sao cần?
Ví dụ:
Class bình thường
nhưng:
1 buổi special native teacher

8. TicketCompatibilityService
Đây là lõi của Phase 1.5

Mục đích
Check:
student ticket
compatible với session slot?

Input
StudentId
SessionId

Logic
1. Lấy available TicketItem.
2. Lấy TicketType.
3. Lấy Session SlotType.
4. Check compatibility.
5. Return:
  - compatible?
  - reason?

Ví dụ
Minh có:
STANDARD ticket

Session:
PREMIUM slot

Result
Not compatible.
Upgrade required.

9. Attendance flow mới
Trước đây
Present
→ consume ticket

Sau Phase 1.5
Present
→ check compatibility
→ consume compatible ticket
→ create ledger

Flow đầy đủ
Attendance
↓
TicketCompatibilityService
↓
Compatible?
↓ yes
Consume Ticket
↓ no
Reject / Require Upgrade

10. Rule phase 1.5
Chỉ cần:
compatible / incompatible

KHÔNG cần:
❌
- auto convert
- dynamic surcharge
- pricing recalculation

Vì sao?
Để:
BE không explode complexity

11. API cần thêm
A. Get compatible tickets
GET /students/{id}/compatible-tickets?sessionId=

Response
{
 "compatible": true,
 "ticketType": "STANDARD"
}

B. Validate attendance compatibility
Khi mark attendance:
POST /sessions/{id}/attendance

Nếu incompatible
{
 "success": false,
 "error": "Ticket type incompatible with slot type."
}

12. Migration strategy
Existing TuitionPlans
Mapping:
TuitionPlan
TicketType
Standard 24
STANDARD
Native 24
PREMIUM
Weekend 24
WEEKEND


Existing Classes
Mapping:
Class
SlotType
Starters A1
STANDARD
Native Kids
PREMIUM


13. FE cần sửa gì?
A. Student detail
Hiển thị:
Ticket Type:
STANDARD

B. Session detail
Hiển thị:
Slot Type:
PREMIUM

C. Attendance
Nếu incompatible:
Cannot attend this class.
Upgrade required.

D. Enrollment screen
Warn trước:
Student ticket type incompatible.

14. Test cases Phase 1.5
Test 1
STANDARD ticket → STANDARD class
Expected:
PASS
Consume ticket

Test 2
STANDARD ticket → PREMIUM class
Expected:
REJECT
Incompatible

Test 3
PREMIUM ticket → STANDARD class
Expected:
PASS
Consume premium ticket

Test 4
Session override slot type
Expected:
Use session slot type instead of class slot type

15. Definition of Done
Phase 1.5 xong khi:
1. TuitionPlan sinh đúng TicketType.
2. TicketItem có TicketType.
3. Class/Session có SlotType.
4. Attendance check compatibility trước khi consume.
5. Incompatible ticket bị reject.
6. FE thấy ticket type / slot type.

16. Điều QUAN TRỌNG NHẤT
Phase 1.5 KHÔNG phải pricing engine
Nó chỉ là:
Runtime access control layer

Nghĩa là:
vé nào được học loại lớp nào

Chưa phải:
❌
- tính giá động
- auto conversion
- surcharge

17. Vì sao cực quan trọng với Rex?
Nếu không có layer này:
TuitionPlan sẽ thành spaghetti business logic

Ví dụ sai phổ biến
if premiumTeacher then
  extraPrice = ...
mọi nơi.

Nhưng nếu có TicketType/SlotType
business logic sẽ sạch hơn rất nhiều.
BE vẫn thiết kế FULL architecture
để:
- future-proof
- không phải rewrite sau này
- đúng tư duy enterprise
Bao gồm:
✅ TicketType
✅ SlotType
✅ Compatibility Matrix
✅ TicketCompatibilityService
✅ Runtime Classification

Nhưng trong vận hành THỰC TẾ hiện tại của Rex
Chỉ bật/use những phần cần thiết

Thực tế Phase 1.5 Rex sẽ dùng:
BẮT BUỘC dùng
1. SlotType
Để phân loại runtime:
NORMAL
NATIVE_SUPPORT
WORKSHOP
REVIEW_SUPPORT
ASSESSMENT_SUPPORT

2. SessionType
Để quyết định behavior:
NORMAL
REVIEW
MAKEUP
REMEDIAL
ASSESSMENT

3. Runtime Analytics
Để biết:
- lớp nào review nhiều
- lớp nào native support nhiều
- curriculum delay
- runtime composition

4. Teaching Log Integration
Tracking:
planned
vs
actual

Còn những phần FULL nhưng chưa dùng mạnh
Chỉ implement để sẵn
TicketType
Hiện tại:
DEFAULT

Compatibility Matrix
Hiện tại:
DEFAULT
→ compatible all

TicketCompatibilityService
Hiện tại:
always pass

Nhưng lợi ích rất lớn
Sau này Rex muốn:
- native premium
- VIP teacher
- branch pricing
- special package
↓
BE chỉ:
bật policy/config

KHÔNG cần rewrite kiến trúc

Đây là tư duy cực đúng
“Build enterprise-ready architecture,
run lightweight business policy.”

Ví dụ thực tế
Hiện tại
Package Starters 48
bao gồm tất cả runtime
↓
DEFAULT ticket

Session hôm nay
SessionType = REVIEW
SlotType = NATIVE_SUPPORT
↓
System hiểu:
- review session
- có native teacher
- consume ticket bình thường
- không advance lesson

Nhưng:
KHÔNG có extra charge

Sau này nếu Rex muốn
Native VIP cần package riêng
↓
chỉ cần bật:
PREMIUM ticket required

