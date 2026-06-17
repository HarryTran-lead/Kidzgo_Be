# FE API Spec - Kidzgo

Tài liệu này mô tả các API FE cần dùng cho các luồng đã chỉnh sửa trong project.

Phạm vi chính:
- Tuition plan đã tách khỏi syllabus/module.
- Attendance, leave request, makeup credit và waitlist đã được chuẩn hóa theo rule vận hành mới.
- Legacy `TicketTypeCompatibility` / `SlotTypes` / `LearningTicketTypes` không còn là contract runtime cho FE.

## 1. Quy ước chung

### 1.1 Success response

Hầu hết API dùng format:

```json
{
  "isSuccess": true,
  "data": { }
}
```

Với API tạo mới, status code là `201 Created`. Với API không trả payload, `data` là `null`.

### 1.2 Page response

Các API phân trang trả về:

```json
{
  "isSuccess": true,
  "data": {
    "page": {
      "items": [],
      "pageNumber": 1,
      "totalPages": 1,
      "totalCount": 0
    }
  }
}
```

### 1.3 Error response

Có 2 kiểu lỗi chính:

- `ProblemDetails` chuẩn cho validation / not found / conflict.
- `StatusChangeBlockedError` cho các case bị chặn bởi nghiệp vụ, trả `409` với body custom:

```json
{
  "success": false,
  "code": "TuitionPlan.StatusChangeBlocked",
  "message": "....",
  "details": {
    "entity": "TuitionPlan",
    "entityId": "guid",
    "reasons": [],
    "counts": {}
  }
}
```

## 2. Role Matrix

| Role | Dữ liệu xem | Phạm vi | Hành động chính |
|---|---|---|---|
| Admin | Toàn bộ dữ liệu vận hành | All | create/edit/approve/delete/toggle/open class/reconcile |
| ManagementStaff | Dữ liệu vận hành | All, hoặc theo branch nếu API có filter | create/edit/approve/open class/assign class/mark attendance |
| Teacher | Dữ liệu lớp mình dạy và session mình phụ trách | Own classes / own sessions | mark/update attendance, xem students, xem session attendance |
| Parent | Dữ liệu của con / makeup / leave liên quan tới con | Own children | request leave, chọn makeup session, xem progress cơ bản |
| Student | Dữ liệu của chính mình | Self | xem attendance history, dùng makeup credit, xem notification |

Ghi chú:
- Một số endpoint hiện chỉ dựa trên `[Authorize]` và filter ở handler, nên FE vẫn phải render theo role để tránh gọi nhầm màn hình.
- Parent ở `GET /api/classes` hiện chỉ thấy các class có `Program.IsMakeup = true`.

## 3. Status Definitions

### 3.1 ClassStatus

| Status | Ý nghĩa |
|---|---|
| `Planned` | Đã lên kế hoạch, chưa mở tuyển sinh |
| `Recruiting` | Đang tuyển sinh |
| `Active` | Đang học |
| `Full` | Đã đầy lớp |
| `Closed` | Đã đóng / kết thúc |
| `Completed` | Hoàn thành |
| `Suspended` | Tạm ngưng |
| `Cancelled` | Hủy |

Luồng chính:
- `Planned -> Recruiting -> Active -> Full -> Closed/Completed`
- `Closed -> Planned` không cho phép.
- Các trạng thái terminal / pause cần không còn enrollment active/pause và không còn future session.

### 3.2 RegistrationStatus

| Status | Ý nghĩa |
|---|---|
| `New` | Mới tạo |
| `WaitingForClass` | Chờ xếp lớp |
| `ClassAssigned` | Đã xếp lớp nhưng chưa vào học |
| `Studying` | Đang học |
| `Paused` | Bảo lưu |
| `Completed` | Hoàn thành |
| `Cancelled` | Hủy |

### 3.3 EntryType

| Status | Ý nghĩa |
|---|---|
| `Immediate` | Vào học ngay |
| `Wait` | Chờ lớp mới |
| `Retake` | Thi lại / xếp lại sau placement test |
| `Makeup` | Legacy-only, giữ để đọc dữ liệu cũ, không dùng cho luồng mới |

### 3.4 LeaveRequestStatus

| Status | Ý nghĩa |
|---|---|
| `Pending` | Chờ staff duyệt |
| `Approved` | Đã duyệt |
| `Rejected` | Bị từ chối |
| `Cancelled` | Bị hủy |

### 3.5 AttendanceStatus

| Status | Ý nghĩa |
|---|---|
| `Present` | Có mặt |
| `Absent` | Vắng |
| `Makeup` | Đã được tính là buổi bù |
| `NotMarked` | Chưa điểm danh |

### 3.6 MakeupCreditStatus

| Status | Ý nghĩa |
|---|---|
| `Available` | Có thể dùng |
| `Used` | Đã chọn buổi bù |
| `Transferred` | Đã rollover sang package mới / upgrade |
| `Expired` | Hết hiệu lực |

### 3.7 MakeupAllocationStatus

| Status | Ý nghĩa |
|---|---|
| `Pending` | Đã giữ chỗ nhưng chưa chốt |
| `Confirmed` | Đã xác nhận |
| `Cancelled` | Đã hủy |

### 3.8 SessionStatus

| Status | Ý nghĩa |
|---|---|
| `Scheduled` | Đã lên lịch |
| `Completed` | Đã học xong |
| `Cancelled` | Hủy buổi |

### 3.9 AbsenceType

| Status | Ý nghĩa |
|---|---|
| `WithNotice24H` | Báo trước >= 24h |
| `Under24H` | Báo trong vòng 24h |
| `NoNotice` | Không báo trước |
| `LongTerm` | Dài hạn / legacy |

### 3.10 CreatedReason

| Status | Ý nghĩa |
|---|---|
| `ApprovedLeave24H` | Tạo từ leave đã duyệt / báo trước đúng rule |
| `LongTerm` | Legacy / tạo thủ công |

## 4. API List

### 4.1 Tuition Plan

| Endpoint | Auth / Role | Mục đích | Params / Body | Success |
|---|---|---|---|---|
| `POST /api/tuition-plans` | `Admin,ManagementStaff` | Tạo tuition plan | Body: `CreateTuitionPlanRequest` (`programId*`, `levelId*`, `name*`, `totalSessions*`, `tuitionAmount*`, `currency*`) | `201` + `CreateTuitionPlanResponse` |
| `GET /api/tuition-plans` | `Admin,ManagementStaff` | Danh sách plan | Query: `branchId?`, `programId?`, `levelId?`, `isActive?`, `pageNumber`, `pageSize` | `200` + `GetTuitionPlansResponse` |
| `GET /api/tuition-plans/active` | Anonymous | Danh sách plan active để FE public | Query: `branchId?`, `programId?`, `levelId?`, `pageNumber`, `pageSize` | `200` + `GetTuitionPlansResponse` |
| `GET /api/tuition-plans/{id}` | `Admin,ManagementStaff` | Chi tiết plan | Path: `id` | `200` + `GetTuitionPlanByIdResponse` |
| `PUT /api/tuition-plans/{id}` | `Admin,ManagementStaff` | Cập nhật plan | Body: `UpdateTuitionPlanRequest` | `200` + `UpdateTuitionPlanResponse` |
| `DELETE /api/tuition-plans/{id}` | `Admin` | Xóa mềm plan | Path: `id` | `200` + `null` |
| `PATCH /api/tuition-plans/{id}/toggle-status` | `Admin,ManagementStaff` | Bật/tắt active | Path: `id` | `200` + `ToggleTuitionPlanStatusResponse` |

Validation / rule:
- `ProgramId` và `LevelId` bắt buộc.
- `LevelId` phải thuộc đúng `ProgramId`.
- `Name` tối đa 255 ký tự.
- `TotalSessions > 0`.
- `TuitionAmount > 0`.
- `Currency` tối đa 10 ký tự.
- `Delete` và `toggle-status` có thể bị chặn nếu đang có enrollment active/paused.

Error thường gặp:
- `TuitionPlan.ProgramNotFound`
- `TuitionPlan.LevelNotFound`
- `TuitionPlan.LevelProgramMismatch`
- `TuitionPlan.NotFound`
- `TuitionPlan.HasActiveEnrollments`
- `TuitionPlan.UpdateConflict`

### 4.2 Class

| Endpoint | Auth / Role | Mục đích | Params / Body | Success |
|---|---|---|---|---|
| `POST /api/classes` | `Admin,ManagementStaff` | Tạo class | Body: `CreateClassRequest` | `201` + `CreateClassResponse` |
| `POST /api/classes/open-from-waiting-list` | `Admin,ManagementStaff` | Mở lớp từ waitlist và auto-assign học viên | Body: `CreateClassRequest` + `track` | `201` + `OpenClassFromWaitingListResponse` |
| `POST /api/classes/preview-sessions` | `Admin,ManagementStaff` | Preview session sinh ra từ lịch | Body: `CreateClassRequest` | `200` + preview DTO |
| `GET /api/classes` | `Admin,ManagementStaff,Parent` | Danh sách class | Query: `branchId?`, `programId?`, `teacherId?`, `studentId?`, `status?`, `searchTerm?`, `pageNumber`, `pageSize` | `200` + `GetClassesResponse` |
| `GET /api/classes/{id}` | `Admin,ManagementStaff` | Chi tiết class | Path: `id` | `200` + `CreateClassResponse`/detail DTO |
| `POST /api/classes/{id}/schedule-segments` | `Admin,ManagementStaff` | Thêm đoạn lịch mới | Body: `AddClassScheduleSegmentRequest` | `200` + `null` |
| `POST /api/classes/{id}/resync-future-lessons` | `Admin,ManagementStaff` | Đồng bộ lesson tương lai | No body | `200` + `null` |
| `GET /api/classes/{id}/students` | `Admin,ManagementStaff,Teacher` | Danh sách học viên trong class | Query: `pageNumber`, `pageSize` | `200` + `GetClassStudentsResponse` |
| `PUT /api/classes/{id}` | `Admin,ManagementStaff` | Cập nhật class | Body: `UpdateClassRequest` | `200` + `UpdateClassResponse` |
| `PATCH /api/classes/{classId}/color` | `Admin,ManagementStaff` | Đổi màu class | Body: `UpdateClassColorRequest` | `200` + `{ isSuccess: true }` |
| `DELETE /api/classes/{id}` | `Admin` | Xóa class | Path: `id` | `200` + `null` |
| `PATCH /api/classes/{id}/status` | `Admin,ManagementStaff` | Đổi trạng thái class | Body: `ChangeClassStatusRequest` | `200` + `ChangeClassStatusResponse` |
| `PATCH /api/classes/{id}/assign-teacher` | `Admin,ManagementStaff` | Gán teacher | Body: `AssignTeacherRequest` | `200` + `null` |
| `GET /api/classes/{id}/capacity` | `Admin,ManagementStaff` | Kiểm tra sức chứa | Path: `id` | `200` + capacity DTO |

Validation / rule:
- Create/update class vẫn cần `SyllabusId`, `StartModuleId`, `StartSessionIndex`, `WeeklyScheduleSlots`.
- Branch, program, level, syllabus phải khớp nhau.
- `Code` phải unique.
- `StartModuleId` phải thuộc level.
- `StartSessionIndex` phải nằm trong số buổi của module.
- `Capacity` không được nhỏ hơn số enrollment active.
- Không đổi branch/program/level nếu class đã có enrollment hoặc session.
- `MainTeacherId` và `AssistantTeacherId` phải là 2 user khác nhau, cùng branch và role `Teacher`.
- `open-from-waiting-list` sẽ tự đổi `Planned -> Recruiting` nếu class vừa tạo chưa recruiting.

Error thường gặp:
- `Class.NotFound`
- `Class.CodeExists`
- `Class.BranchNotFound`
- `Class.ProgramNotFound`
- `Class.LevelNotFound`
- `Class.LevelProgramMismatch`
- `Class.SyllabusNotFound`
- `Class.SyllabusProgramLevelMismatch`
- `Class.StartModuleNotFound`
- `Class.StartModuleLevelMismatch`
- `Class.InvalidStartSessionIndex`
- `Class.HasOperationalDependencies`
- `Class.HasFutureSessions`
- `Class.CapacityBelowActiveEnrollments`
- `Class.MainTeacherNotFound`
- `Class.AssistantTeacherNotFound`
- `Class.TeacherAndAssistantMustDiffer`
- `Class.StatusUnchanged`
- `Class.InvalidStatusTransition`
- `Class.CannotCloseWithActiveEnrollments`
- `Class.CannotCloseWithFutureSessions`

### 4.3 Registration

| Endpoint | Auth / Role | Mục đích | Params / Body | Success |
|---|---|---|---|---|
| `POST /api/registrations` | `Admin,ManagementStaff` | Tạo đăng ký mới | Body: `CreateRegistrationRequest` | `201` + `CreateRegistrationResponse` |
| `POST /api/registrations/import-active` | `Admin,ManagementStaff` | Import học viên đang học dở từ hệ thống cũ | Body: `ImportActiveRegistrationRequest` | `201` + `ImportActiveRegistrationResponse` |
| `GET /api/registrations` | `Admin,ManagementStaff` | Danh sách registration | Query: `studentProfileId?`, `branchId?`, `programId?`, `status?`, `classId?`, `pageNumber`, `pageSize` | `200` + `GetRegistrationsResponse` |
| `GET /api/registrations/{id}` | `Admin,ManagementStaff` | Chi tiết registration | Path: `id` | `200` + `GetRegistrationByIdResponse` |
| `GET /api/registrations/{id}/history` | `Admin,ManagementStaff` | Lịch sử thao tác registration | Query: `pageNumber`, `pageSize` | `200` + history page |
| `PUT /api/registrations/{id}` | `Admin,ManagementStaff` | Cập nhật registration | Body: `UpdateRegistrationRequest` | `200` + `UpdateRegistrationResponse` |
| `PATCH /api/registrations/{id}/cancel` | `Admin,ManagementStaff` | Hủy registration | Query: `reason?` | `200` + `null` |
| `GET /api/registrations/waiting-list` | `Admin,ManagementStaff` | Danh sách chờ xếp lớp | Query: `branchId?`, `programId?`, `levelId?`, `track?`, `pageNumber`, `pageSize` | `200` + `GetWaitingListResponse` |
| `GET /api/registrations/{id}/suggest-classes` | `Admin,ManagementStaff` | Gợi ý lớp phù hợp | Path: `id` | `200` + suggestion DTO |
| `POST /api/registrations/{id}/assign-class` | `Admin,ManagementStaff` | Xếp lớp / cho vào waitlist | Body: `AssignClassRequest` | `200` + `AssignClassResponse` |
| `POST /api/registrations/{id}/transfer-class` | `Admin,ManagementStaff` | Chuyển lớp | Body: `TransferClassRequest` | `200` + `TransferClassResponse` |
| `POST /api/registrations/{id}/transfer-branch` | `Admin,ManagementStaff` | Chuyển chi nhánh | Body: `TransferRegistrationBranchRequest` | `200` + `TransferRegistrationBranchResponse` |
| `POST /api/registrations/{id}/upgrade?newTuitionPlanId=...` | `Admin,ManagementStaff` | Upgrade tuition plan | Query: `newTuitionPlanId` | `200` + `UpgradeTuitionPlanResponse` |
| `GET /api/registrations/{id}/enrollment-confirmation-pdf` | `Admin,ManagementStaff` | Preview phiếu xác nhận nhập học | Query: `track?`, `formType?` | `200` + preview DTO |
| `GET /api/registrations/{id}/enrollment-confirmation-pdf/history` | `Admin,ManagementStaff` | Lịch sử PDF đã generate | Query: `track?`, `formType?`, `pageNumber`, `pageSize` | `200` + `GetEnrollmentConfirmationPdfHistoryResponse` |
| `POST /api/registrations/{id}/enrollment-confirmation-pdf` | `Admin,ManagementStaff` | Generate PDF | Query: `track?`, `regenerate?`, `formType?` | `200` + `GenerateEnrollmentConfirmationPdfResponse` |
| `GET /api/registrations/enrollment-confirmation-payment-setting` | `Admin,ManagementStaff` | Lấy cấu hình thanh toán trên PDF | Query: `branchId?` | `200` + `GetEnrollmentConfirmationPaymentSettingResponse` |
| `PUT /api/registrations/enrollment-confirmation-payment-setting` | `Admin` | Cập nhật cấu hình thanh toán trên PDF | Body: `EnrollmentConfirmationPaymentSettingRequest` | `200` + `null` |

Validation / rule:
- `StudentProfileId` phải là student profile.
- `BranchId` phải active và student phải được phép học ở branch đó.
- `ProgramId` phải active và được gán cho branch.
- `LevelId` phải thuộc program.
- `TuitionPlanId` phải thuộc đúng `ProgramId` + `LevelId` và đang active.
- Không cho tạo registration trùng program đang active của cùng student.
- `SecondaryLevelId` phải khác `LevelId`.
- `SecondaryLevelSkillFocus` chỉ hợp lệ khi có `SecondaryLevelId`.
- `Update` không được đổi tuition plan nếu đã có class assigned.
- `AssignClass`:
  - `EntryType` chỉ nhận `immediate | wait | retake`.
  - `Track` chỉ nhận `primary | secondary`.
  - `wait` không dùng `FirstStudyDate`.
  - `FirstStudyDate` phải nằm trong schedule thực tế của class.
  - class phải cùng branch/program/level và còn capacity.
- `TransferClass` và `TransferBranch` check conflict lịch học, branch/program/level, capacity.
- `UpgradeTuitionPlan` chỉ cho registration `Studying/ClassAssigned/WaitingForClass`.

Error thường gặp:
- `Registration.StudentNotFound`
- `Registration.BranchNotFound`
- `Registration.ProgramNotFound`
- `Registration.ProgramNotAvailableInBranch`
- `Registration.LevelNotFoundInProgram`
- `Registration.SecondaryLevelDuplicated`
- `Registration.SecondaryLevelMissing`
- `Registration.SecondaryLevelNotFoundInProgram`
- `Registration.TuitionPlanNotFound`
- `Registration.AlreadyExists`
- `Registration.InvalidStatus`
- `Registration.ClassNotFound`
- `Registration.ClassFull`
- `Registration.ClassNotMatchingBranch`
- `Registration.ClassNotMatchingProgram`
- `Registration.ClassNotMatchingLevel`
- `Registration.TuitionPlanLevelMismatch`
- `Registration.InvalidEntryType`
- `Registration.FirstStudyDateInPast`
- `Registration.FirstStudyDateBeforeClassStart`
- `Registration.FirstStudyDateAfterClassEnd`
- `Registration.FirstStudyDateNoSession`
- `Registration.CannotTransferToSameClass`
- `Registration.CannotTransferToSameBranch`
- `Registration.CannotTransferBranchWithSecondaryClass`

### 4.4 Attendance

| Endpoint | Auth / Role | Mục đích | Params / Body | Success |
|---|---|---|---|---|
| `POST /api/attendance/{sessionId}` | `Admin,Teacher` | Điểm danh hàng loạt | Body: `MarkAttendanceRequest` (`attendances[]`) | `200` + `MarkAttendanceResponse` |
| `GET /api/attendance/{sessionId}` | `Admin,Teacher` | Lấy danh sách điểm danh của session | Path: `sessionId` | `200` + `GetSessionAttendanceResponse` (wrapper session + summary) |
| `GET /api/attendance/students` | `Admin,Teacher,Parent,Student` | Lịch sử điểm danh của học sinh hiện tại | Query: `pageNumber`, `pageSize` | `200` + `Page<GetStudentAttendanceHistoryResponse>` |
| `PUT /api/attendance/{sessionId}/students/{studentProfileId}` | `Admin,Teacher` | Sửa một bản ghi attendance | Body: `UpdateAttendanceRequest` | `200` + `UpdateAttendanceResponse` |

Validation / rule:
- Teacher không được điểm danh session tương lai.
- Teacher không được sửa/điểm danh session đã qua ngoài ngày học.
- Attendance chỉ áp dụng cho student đã được assign vào session.
- Khi student có approved leave:
  - attendance sẽ được ép sang `Makeup`.
  - trạng thái này khóa cập nhật với non-admin.
- `Absent` sẽ xác định `AbsenceType` theo rule leave:
  - >=24h => `WithNotice24H`
  - <24h => `Under24H`
  - không có leave => `NoNotice`

Error thường gặp:
- `Attendance.NotFound`
- `Attendance.FutureSessionNotAllowed`
- `Attendance.SessionDateClosed`
- `Attendance.ApprovedLeaveLocked`
- `Attendance.StudentNotAssigned`

### 4.5 Leave Request

| Endpoint | Auth / Role | Mục đích | Params / Body | Success |
|---|---|---|---|---|
| `POST /api/leave-requests` | `Authorize` | Tạo leave request | Body: `CreateLeaveRequestRequest` | `201` + `CreateLeaveRequestResponse` |
| `GET /api/leave-requests` | `Authorize` | Danh sách leave request | Query: `studentProfileId?`, `classId?`, `status?`, `branchId?`, `pageNumber`, `pageSize` | `200` + `Page<GetLeaveRequestsResponse>` |
| `GET /api/leave-requests/{id}` | `Authorize` | Chi tiết leave request | Path: `id` | `200` + `GetLeaveRequestByIdResponse` |
| `PUT /api/leave-requests/{id}/approve` | `Admin,ManagementStaff` | Duyệt leave | Path: `id` | `200` + `null` |
| `PUT /api/leave-requests/approve-bulk` | `Admin,ManagementStaff` | Duyệt nhiều leave | Body: `BulkApproveRequest` (`ids[]`) | `200` + `BulkApproveLeaveRequestsResponse` |
| `PUT /api/leave-requests/{id}/reject` | `Admin,ManagementStaff` | Từ chối leave | Path: `id` | `200` + `null` |
| `PUT /api/leave-requests/{id}/cancel` | `Authorize` | Hủy leave | Path: `id` | `200` + `null` |

Validation / rule:
- `SessionDate` không được ở quá khứ.
- `EndDate` nếu có thì phải >= `SessionDate`.
- Nếu leave báo trước >=24h thì request tự ở trạng thái `Approved` và tạo `MakeupCredit`.
- Nếu báo <24h thì request `Pending`, chờ staff duyệt.
- Mỗi student/class/month có giới hạn leave tối đa theo program policy (fallback 2).
- Khi approve leave:
  - tạo `MakeupCredit` với `CreatedReason = ApprovedLeave24H`
  - không auto tạo buổi bù cố định.
- Khi cancel leave đã approved:
  - xóa makeup credit và allocation liên quan.

Error thường gặp:
- `LeaveRequest.NotFound`
- `LeaveRequest.ClassNotFound`
- `LeaveRequest.SessionNotFound`
- `LeaveRequest.NotEnrolled`
- `LeaveRequest.AlreadyApproved`
- `LeaveRequest.AlreadyRejected`
- `LeaveRequest.AlreadyCancelled`
- `LeaveRequest.ExceededMonthlyLeaveLimit`
- `LeaveRequest.CannotCancelPastSession`

### 4.6 Makeup Credit

| Endpoint | Auth / Role | Mục đích | Params / Body | Success |
|---|---|---|---|---|
| `GET /api/makeup-credits/settings` | `Admin,ManagementStaff` | Lấy cấu hình makeup | No body | `200` + `MakeupSettingsResponse` |
| `PUT /api/makeup-credits/settings` | `Admin,ManagementStaff` | Cập nhật expiry days | Body: `UpdateMakeupSettingsRequest` | `200` + `null` |
| `GET /api/makeup-credits` | `Authorize` | Danh sách credit theo học sinh | Query: `studentProfileId` | `200` + `IEnumerable<MakeupCreditResponse>` |
| `GET /api/makeup-credits/all` | `Authorize` | Danh sách credit có filter | Query: `studentProfileId?`, `status?`, `branchId?`, `pageNumber`, `pageSize` | `200` + `Page<MakeupCreditResponse>` |
| `GET /api/makeup-credits/{id}` | `Authorize` | Chi tiết credit | Path: `id` | `200` + `MakeupCreditResponse` |
| `POST /api/makeup-credits/{id}/use` | `Authorize` | Dùng credit để chọn buổi bù | Body: `UseMakeupCreditRequest` (`studentProfileId?`, `classId`, `targetSessionId`) | `200` + `null` |
| `POST /api/makeup-credits/{id}/expire` | `Authorize` | Đánh dấu hết hạn | Body: `ExpireMakeupCreditRequest` | `200` + `null` |
| `GET /api/makeup-credits/{id}/parent/get-available-sessions` | `Authorize` | Gợi ý buổi bù phù hợp | Query: `fromDate?`, `toDate?`, `timeOfDay?` | `200` + list session suggestion |
| `GET /api/makeup-credits/allocations` | `Authorize` | Lịch sử allocation của học sinh | Query: `studentProfileId`, `includeCancelled?` | `200` + list allocation |
| `GET /api/makeup-credits/students` | `Authorize` | Danh sách học viên đang có makeup/leave | Query: `searchTerm?`, `branchId?`, `pageNumber`, `pageSize` | `200` + list student summary |

Validation / rule:
- `use`:
  - Parent phải truyền `studentProfileId`.
  - Student chỉ dùng credit của chính mình.
  - Target session phải ở tương lai, cùng branch và cùng level với source session.
  - `targetSession` không được full.
  - Không cho trùng lịch hoặc quá sát lịch khác.
  - Nếu credit đã used thì chỉ được reschedule khi session đã chọn هنوز chưa qua.
- `suggest`:
  - Chỉ gợi ý session sau ngày source session.
  - Có filter `morning/afternoon/evening`.
  - Không gợi ý session cùng class với source session.
- `settings` là cấu hình expiry, nhưng policy nghiệp vụ hiện tại là makeup credit có thể được giữ đến cuối khóa nếu chưa chọn buổi.

Error thường gặp:
- `MakeupCredit.NotFound`
- `MakeupCredit.NotAvailable`
- `MakeupCredit.Expired`
- `MakeupCredit.NotBelongToStudent`
- `MakeupCredit.TargetClassMustBeMakeupProgram`
- `MakeupCredit.MustBeFutureWeek`
- `MakeupCredit.CannotUsePastDate`
- `MakeupCredit.CannotChangeAllocatedPastSession`
- `MakeupCredit.ParentMustProvideStudentProfileId`
- `MakeupCredit.StudentNotBelongToParent`
- `MakeupCredit.SessionNotBelongToClass`
- `MakeupCredit.MustStayInCurrentMakeupProgram`
- `MakeupCredit.TargetSessionFull`

## 5. Business Notes quan trọng cho FE

- Tuition plan giờ là package độc lập, không dùng syllabus/module để suy luận tiến độ gói.
- Class vẫn dùng syllabus/module để chạy template dạy theo buổi.
- Attendance rule:
  - `Absent` trừ buổi.
  - Báo trước >=24h không trừ buổi ngay, tạo makeup credit.
  - Báo trong 24h thì chờ staff duyệt.
  - Approved leave tạo makeup credit, không auto tạo buổi bù cố định.
- Makeup:
  - Phụ huynh/student chọn buổi bù sau.
  - Nếu chưa chọn, credit được giữ lại để dùng tiếp.
  - FE không cần hiển thị slot type / learning ticket type / ticket compatibility nữa.
- Waitlist:
  - Mốc mở lớp là 7 học viên.
  - Admin và ManagementStaff nhận notification.
  - Có flow `open-from-waiting-list` để tạo class rồi auto xếp học viên phù hợp.
- `24+4` chỉ là shorthand nghiệp vụ, không phải field API riêng.

## 6. Rollover update

- `MakeupCreditStatus.Transferred` là trạng thái credit đã được chuyển sang package mới hoặc upgrade.
- `LearningTicketSource.Rollover` là source cho ticket sinh ra từ makeup credit rollover.
- `POST /api/registrations` và `POST /api/registrations/{id}/upgrade` có thêm field response `RolledOverMakeupCredits`.
- `NewTotalSessions` trong upgrade bao gồm:
  - số buổi còn lại của package cũ
  - số buổi của package mới
  - số makeup credit rollover chưa được chọn buổi bù
- `POST /api/makeup-credits/{id}/expire` chỉ cho phép credit `Available`. Credit `Used` hoặc `Transferred` không còn được expire.
- Edge case: nếu cancel registration sau khi rollover, hiện tại hệ thống chưa có reverse flow tự động trả credit về `Available`.
