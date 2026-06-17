                                   PLAN VẬN HÀNH THỰC TẾ
   5 Luồng chính để vận hành ở trung tâm
1. Đăng kí tư vấn -> làm test -> đăng kí lớp -> tham gia vào lớp (quản lí vận hành)
2. Giao bài tập ->làm bài tập ->chấm bài tập (quản lí học vụ) 
3. Báo cáo *Huy
4. Nhiệm vụ và phần thưởng ( nhận sao, đổi sao)
5. Xin nghỉ - bù - bảo lưu 


trung tâm đang vận hành thực tế
Chương trình
Có 3 nhóm chính (Program):
Kids English
IELTS
Communication
Bên trong Kids English (tức là các program chi nhỏ thành các level):
Starters
Movers
Flyers
KET
PET
...
Chưa chia:
Starters Basic
Starters Intermediate
Starters Advanced
Học viên có thể:
Placement Test
Nhảy thẳng vào level phù hợp
=> Không bắt buộc học tuần tự.

2. Package là thứ trung tâm bán
Thực tế trung tâm đang bán:
Gói 24 buổi
Gói 48 buổi
Không phải bán level.
Ví dụ:
Học viên mua:
Starters + 24 buổi => tức là program rồi tới level và chọn số buổi để quy ra vévé
chứ không phải chi tiết chọn Module rồi syllabus như vận hành hiện tại
Điểm rất quan trọng:
Học hết 24 buổi KHÔNG đồng nghĩa lên level.
Nếu chưa đạt:
học tiếp
mua thêm package
hoặc trung tâm hỗ trợ tăng cường

3. Lớp học đang theo mô hình Rolling Class
Đây là business rule quan trọng nhất.
Ví dụ:
Class Starters A
Ban đầu:
An
Bình
Cường
Dũng
Em
Sau 3 tháng:
An lên Movers
Bình lên Movers
Cường lên Movers
Còn:
Dũng
Em
Trung tâm không đóng lớp.
Mà:
tuyển thêm học sinh mới cùng trình độ
cho học chung
Lớp tiếp tục tồn tại.
Có thể kéo dài:
6 tháng
1 năm
2 năm
=> Class không gắn với khóa học cố định.

4. Progress thuộc về lớp
Mỗi lớp có:
Current Lesson
Ví dụ:
Starters A
đang ở:
Lesson 12
Học viên mới vào:
Placement Test đạt
Join thẳng Lesson 12
Không học lại từ đầu.
Đây là đặc điểm rất khác với mô hình academy truyền thống.

5. Giáo viên rất linh hoạt
Hiện tại giáo viên:
Không sửa lesson plan chuẩn.
Nhưng được:
thêm hoạt động
giao homework khác
thêm yêu cầu
kéo dài lesson
bỏ qua phần ít quan trọng
Ví dụ:
Lesson 5
dự kiến:
1 buổi
Thực tế:
2-3 buổi
vẫn được.
=> Teacher flexibility rất cao.

6. Lesson Plan hiện là Semi-Standard
Trung tâm có:
Objectives
Vocabulary
Grammar
Procedure
Homework
Nhưng mới hoàn thiện khoảng:
50%
Chưa phải curriculum chuẩn hóa hoàn toàn.

7. Student Progress thực tế
Có 2 loại progress.
Progress lớp
Ví dụ:
Class Starters A
đang:
Lesson 15

Progress học viên
Ví dụ:
Học viên:
đã học 18 buổi
còn 6 buổi
và
đạt 70%
=> Đây là progress cá nhân.
Hai loại progress này tách biệt.

8. Assessment
Trung tâm đánh giá khi:
cuối package
giáo viên đề xuất
nghi ngờ học viên vượt trình độ
Ví dụ:
Bé học quá nhanh.
Giáo viên thấy:
vượt xa các bạn
→ Placement Test
Nếu đạt:
lên lớp
Nếu không:
ở lại lớp.

9. Waitlist
Nếu chưa đủ người mở lớp:
Học viên vào hàng chờ.
Khi đủ:
5–7 học sinh
Admin mở lớp mới.

10. Branch
Mỗi chi nhánh:
vận hành độc lập
Teacher:
không dạy nhiều branch
Student:
được chuyển branch

Kiến trúc thực tế của trung tâm hiện nay
Nếu mô hình hóa hệ thống thì thực tế đang là:
Program
│
├── Starters
├── Movers
├── Flyers
│
├── Package 24
└── Package 48

Student
│
├── Placement Test
├── Join Class
└── Consume Sessions

Class
│
├── Current Lesson
├── Teacher
├── Students
└── Rolling Lifecycle

Lesson Plan
│
├── Planned Lesson
└── Teaching Log

Assessment
│
├── Placement Test
├── Progress Evaluation
└── Promotion Decision
3 business rules bất biến nhất của trung tâm
Sau khi đọc toàn bộ nghiệp vụ, mình đánh giá 3 thứ trung tâm chắc chắn không muốn đụng vào là:
1. Rolling Class
Học viên có thể vào giữa chừng sau placement test.
2. Package-based
Trung tâm bán 24 và 48 buổi, không bán level.
3. Teacher Flexibility
Giáo viên được linh hoạt điều chỉnh tiến độ thực tế.

Nếu làm hệ thống, mình sẽ đề xuất mô hình:
Curriculum → Level → Lesson Plan (Planned)
kết hợp với
Package → Attendance → Session Consumption
và
Teaching Log (Actual Progress)
để vừa chuẩn hóa học vụ, vừa không phá cách vận hành hiện tại của trung tâm. Đây là hướng phù hợp nhất với những gì trung tâm đang làm ngoài thực tế.


Điểm cần chỉnh/kiểm tra để sát vận hành thật hơn
TuitionPlan hiện vẫn neo vào levelId, syllabusId/moduleIds. Nếu trung tâm bán “Starters + 24 buổi” thì ổn; nhưng phải tránh logic “hết 24 buổi = xong Starters”.

Bỏ module, unit, template 
Sửa lại gói 

Cần rule rõ khi điểm danh: vắng có trừ buổi không, makeup có trừ gói chính không, remedial/free có trừ không.
Nghỉ có phép thì bù như nào ( có cần tạo 1 lớp riêng không, hiện tại là mình đang có lớp théo type lớp học bù thì nó có giải quyết được vấn đề này không)
Vé , cái slottype , … dạng buổi học thì nó có thật sự quan trọng không. ( ko quan trọng thì bỏ)

Waitlist có endpoint, nhưng rule “đủ 5-7 học viên thì mở lớp” nên có dashboard/cảnh báo/action riêng.
Hiện thông báo cho admin và staff biết được để mở lớp

Teacher không dạy nhiều branch cần backend enforce, FE filter thôi chưa đủ.
Này có

Kết luận: không cần đập lại hệ thống. Hướng đúng nhất là giữ kiến trúc hiện tại, nhưng chuẩn hóa cách dùng: Program/Level/Syllabus là khung học vụ, Class current progress + Teaching Log là tiến độ thật, còn TuitionPlan/LearningTicket + Attendance là gói buổi học mà trung tâm bán.


## Checklist đã implement
- `Kidzgo.API/Controllers/ClassController.cs`: bỏ `SlotTypeId` khỏi luồng tạo/cập nhật class.
- `Kidzgo.API/Requests/CreateClassRequest.cs` và `Kidzgo.API/Requests/UpdateClassRequest.cs`: bỏ field `SlotTypeId`.
- `Kidzgo.API/Controllers/SessionController.cs`: bỏ `SlotTypeId` khỏi create/update/update-by-class session.
- `Kidzgo.API/Requests/CreateSessionRequest.cs`, `Kidzgo.API/Requests/UpdateSessionRequest.cs`, `Kidzgo.API/Requests/UpdateSessionsByClassRequest.cs`: bỏ field `SlotTypeId`.
- `Kidzgo.API/Controllers/TuitionPlanController.cs`: bỏ `LearningTicketTypeId` khỏi create/update tuition plan.
- `Kidzgo.API/Requests/CreateTuitionPlanRequest.cs` và `Kidzgo.API/Requests/UpdateTuitionPlanRequest.cs`: bỏ field `LearningTicketTypeId`.
- `Kidzgo.Application/TuitionPlans/*`: bỏ neo `LearningTicketTypeId` khỏi command, validator, response, handler, query.
- `Kidzgo.Application/Classes/*`, `Kidzgo.Application/Sessions/*`, `Kidzgo.Application/Registrations/*`, `Kidzgo.Application/Enrollments/*`, `Kidzgo.Application/Attendance/*`, `Kidzgo.Application/Services/*`: bỏ logic compatibility slot/ticket trong luồng vận hành chính.
- `Kidzgo.API/Controllers/TicketTypeCompatibilityController.cs`, `Kidzgo.API/Controllers/SlotTypeController.cs`, `Kidzgo.API/Controllers/LearningTicketTypeController.cs` và toàn bộ folder `Kidzgo.Application/TicketTypeCompatibilities/*`, `Kidzgo.Application/SlotTypes/*`, `Kidzgo.Application/LearningTicketTypes/*`: đã xóa khỏi runtime code.
- `Kidzgo.Domain`, `Kidzgo.Infrastructure/Configuration`, `Kidzgo.Application/Abstraction/Data/IDbContext.cs`, `Kidzgo.Infrastructure/Database/ApplicationDbContext.cs`: đã gỡ entity/config/DbSet legacy liên quan.
- Không chạy bất kỳ lệnh migrate nào theo yêu cầu.




