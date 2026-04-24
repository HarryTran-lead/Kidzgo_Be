# Backend Business Rules Audit (Code Only)

## Scope
- Audit date: `2026-04-24`
- Source of truth: backend code only
- Included: `CommandHandler`, `DomainEventHandler`, background jobs, core services with side effects
- Excluded as source of truth: FE flow, markdown docs, UI assumptions
- Requested modules:
  - `Auth / Role / Profile`
  - `Lead / Placement / Registration / Enrollment`
  - `Program / Class / Session`
  - `Attendance / Leave / Makeup / Pause`
  - `Homework / Submission / Grading`
  - `Report / AI / Approve / Publish`
  - `Gamification / Mission / Reward`

## Legend
- `Full`: rule is enforced in BE mutation flow
- `Partial`: implemented, but inconsistent / missing a protection / duplicate path / hidden gap
- `CRUD only`: mostly existence + basic field validation, no deep business enforcement
- `UI only / BE not enforced`: UI or controller contract suggests a rule, but handler/service does not enforce it hard

## Important cross-cutting findings
- Role mismatch exists between token emission and API authorization strings.
  - `UserRole` enum only has `Admin`, `ManagementStaff`, `AccountantStaff`, `Teacher`, `Student`, `Parent`.
  - JWT only emits `user.Role.ToString()`.
  - Several controllers use `Staff`, `SalesStaff`, `TeachingAssistant`.
  - Result: those role strings are not issuable from current token provider.
- There are 2 `SelectStudentProfile` implementations.
  - `Authentication/Profiles/...` only validates ownership and returns success.
  - `Profiles/...` issues a new token with `StudentId`.
  - Current auth controller is wired to the first one, so selected student context is effectively FE-managed in that flow.
- Password reset token expiry is generated (`ExpiresAt = now + 1h`) but the reset handler does not check expiry before consuming token.
- Single-session create/update only logs room/teacher conflict and still allows save; batch session update and generated sessions do block conflicts.
- Some enum statuses exist but no active writer was found in current code path:
  - `RegistrationStatus.Paused`
  - `MissionProgressStatus.Expired`
  - `ReportRequestStatus.InProgress`

---

## Auth / Role / Profile

| ID | Rule name | Module | Trigger / API | Valid when | Blocked when | Status / data changes | Code | Impl |
|---|---|---|---|---|---|---|---|---|
| AUTH-01 | JWT claim issuance | Auth / Role / Profile | login, refresh, token create | login flow succeeds | login flow fails earlier | access token contains `NameIdentifier`, `Email`, `Role`, optional `StudentId`; refresh token random 32-byte base64, 1 day expiry | `Kidzgo.Infrastructure/Authentication/TokenProvider.cs`, `Kidzgo.Application/Authentication/Login/LoginCommandHandler.cs`, `.../LoginWithRefreshToken.cs` | Full |
| AUTH-02 | Email login | Auth / Role / Profile | email login | user found by trimmed/lowercased email, password hash valid, user active, not deleted | user missing, wrong password, inactive/deleted | removes all old refresh tokens, creates new refresh token, updates `LastLoginAt`, `LastSeenAt`, `UpdatedAt` | `Kidzgo.Application/Authentication/Login/LoginCommandHandler.cs` | Full |
| AUTH-03 | Phone login | Auth / Role / Profile | phone login | phone matches normalized lookup candidates, user active, not deleted | user missing, inactive/deleted | removes all old refresh tokens, creates new refresh token, updates last seen/login | `Kidzgo.Application/Authentication/Login/LoginByPhoneNumberCommandHandler.cs` | Partial |
| AUTH-04 | Refresh token rotation | Auth / Role / Profile | refresh token login | refresh token exists, not expired, linked user active/not deleted | invalid / expired token, inactive user | rotates token string + expiry in same row, updates user `LastSeenAt` | `Kidzgo.Application/Authentication/Login/LoginWithRefreshToken.cs` | Full |
| AUTH-05 | Logout revokes refresh tokens | Auth / Role / Profile | logout | current user exists in context | n/a | removes all refresh tokens of current user | `Kidzgo.Application/Authentication/Logout/LogoutCommandHandler.cs` | Full |
| AUTH-06 | Forgot password hides account existence | Auth / Role / Profile | forgot password | any email input | n/a outwardly | always returns success outward; if user exists and template `FORGOT_PASSWORD` active, creates `PasswordResetToken` with 1h expiry and sends email | `Kidzgo.Application/Authentication/ForgetPassword/ForgetPasswordCommandHandler.cs`, `.../ForgetPasswordDomainEventHandler.cs` | Full |
| AUTH-07 | Reset password consumes token | Auth / Role / Profile | reset password | token exists and not used | token missing / used | hashes new password, sets `UsedAt` on token | `Kidzgo.Application/Authentication/ResetPassword/ResetPasswordCommandHandler.cs` | Partial |
| AUTH-08 | Change password | Auth / Role / Profile | change password | current user exists, current password valid, new password differs | wrong current password, same new password, user missing | replaces password hash | `Kidzgo.Application/Authentication/ChangePassword/ChangePasswordCommandHandler.cs` | Full |
| AUTH-09 | Auth student-profile select is ownership check only | Auth / Role / Profile | auth `select-student-profile` | profile belongs to current user, type student, active, not deleted | invalid / foreign / inactive profile | no persisted server-side selection; handler only returns success | `Kidzgo.Application/Authentication/Profiles/SelectStudentProfile/SelectStudentProfileCommandHandler.cs` | UI only / BE not enforced |
| AUTH-10 | Alternate student-profile select issues token with `StudentId` | Auth / Role / Profile | alternate profile-select flow | profile belongs to current user, type student, active, not deleted | invalid / foreign / inactive profile | updates profile/user last seen and returns fresh JWT with `StudentId` claim | `Kidzgo.Application/Profiles/SelectStudentProfile/SelectStudentProfileCommandHandler.cs` | Partial |
| AUTH-11 | Role-based authorization strings mismatch tokenable roles | Auth / Role / Profile | controller `[Authorize(Roles = ...)]` | token role string must match controller role string | role string absent from token provider / enum | endpoints using `Staff`, `SalesStaff`, `TeachingAssistant` are effectively unreachable with current JWT emission | `Kidzgo.Domain/Users/UserRole.cs`, `Kidzgo.Infrastructure/Authentication/TokenProvider.cs`, many files in `Kidzgo.API/Controllers/*` | Partial |
| AUTH-12 | Create profile auto-fills and auto-links related profiles | Auth / Role / Profile | create profile | user exists | user missing | new profile starts `IsApproved=false`, `IsActive=false`; student profile may auto-fill from `LeadChild`; parent profile may auto-fill from `Lead`; auto-creates `ParentStudentLink` with existing sibling profiles of same user | `Kidzgo.Application/Profiles/CreateProfile/CreateProfileCommandHandler.cs` | Full |
| AUTH-13 | Approve profile bulk flow | Auth / Role / Profile | approve profile(s) | IDs provided, profile exists, not already approved | not found IDs go to `NotFound`; already approved separated | bulk updates `IsApproved=true`, sends domain event with default password `123456` and default PIN `1234`; does not auto-set `IsActive` | `Kidzgo.Application/Profiles/ApproveProfile/ApproveProfileCommandHandler.cs` | Partial |
| AUTH-14 | Reactivate profile affects all approved profiles of same user | Auth / Role / Profile | reactivate profile | target profile exists | target missing | bulk updates all approved profiles of same user to `IsDeleted=false`, `IsActive=true` | `Kidzgo.Application/Profiles/ReactivateProfile/ReactivateProfileCommandHandler.cs` | Partial |
| AUTH-15 | Parent-student manual linking | Auth / Role / Profile | link parent-student | parent exists, active, type parent; student exists, active, type student; link not duplicated | parent/student missing or wrong type; duplicate link | creates `ParentStudentLink` | `Kidzgo.Application/Profiles/LinkParentStudent/LinkParentStudentCommandHandler.cs` | Full |
| AUTH-16 | Parent PIN verify and first-time setup | Auth / Role / Profile | verify user PIN | current user exists, current active parent profile resolved, PIN numeric and length `< 10` | invalid PIN format, missing parent profile, wrong PIN | if `PinHash` empty, first verify sets PIN; otherwise verifies hash | `Kidzgo.Application/Authentication/VerifyUserPin/VerifyUserPinCommandHandler.cs` | Full |
| AUTH-17 | Parent PIN change | Auth / Role / Profile | change PIN | current user exists, current active parent profile exists, PIN already set, current PIN correct, new PIN numeric and length `< 10` | missing parent profile, current PIN wrong, PIN not set, invalid format | replaces `PinHash` | `Kidzgo.Application/Authentication/ChangePin/ChangeUserPinCommandHandler.cs` | Full |
| AUTH-18 | Parent PIN reset via email | Auth / Role / Profile | request PIN reset by email | active parent profile exists and user has email | invalid parent profile / no email | outward success; if template `PARENT_PIN_RESET` active, deletes old unused tokens and creates new reset token with 1h expiry | `Kidzgo.Application/Authentication/Profiles/RequestParentPinReset/RequestParentPinResetCommandHandler.cs`, `.../ParentPinResetRequestDomainEventHandler.cs` | Full |
| AUTH-19 | Parent PIN reset via Zalo OTP request | Auth / Role / Profile | request PIN reset by Zalo OTP | active parent profile exists and has `ZaloId` | missing parent profile, no Zalo, send failure | deletes old active tokens, creates reset token, 6-digit OTP hash, `OtpExpiresAt=10m`, `ExpiresAt=60m`, `OtpAttemptCount=0`; deletes token back if Zalo send fails | `Kidzgo.Application/Authentication/Profiles/RequestParentPinResetZaloOtp/RequestParentPinResetZaloOtpCommandHandler.cs` | Full |
| AUTH-20 | Parent PIN reset OTP verify | Auth / Role / Profile | verify Zalo OTP | reset challenge exists, unused, active parent profile, OTP not expired | invalid token, inactive profile, expired OTP, max 5 attempts exceeded, wrong OTP | wrong OTP increments `OtpAttemptCount`; success sets `OtpVerifiedAt`; already verified is idempotent and returns same reset token | `Kidzgo.Application/Authentication/Profiles/VerifyParentPinResetZaloOtp/VerifyParentPinResetZaloOtpCommandHandler.cs` | Full |
| AUTH-21 | Parent PIN reset by token | Auth / Role / Profile | reset parent PIN | reset token exists, unused, new PIN numeric and `< 10`; if token requires OTP, OTP must be verified; profile active parent | invalid token, OTP not verified, invalid PIN, invalid profile | hashes new PIN and sets `UsedAt` on reset token | `Kidzgo.Application/Authentication/Profiles/ResetParentPin/ResetParentPinCommandHandler.cs` | Full |

---

## Lead / Placement / Registration / Enrollment

| ID | Rule name | Module | Trigger / API | Valid when | Blocked when | Status / data changes | Code | Impl |
|---|---|---|---|---|---|---|---|---|
| ADM-01 | Create lead requires real contact point and blocks duplicates | Lead / Placement / Registration / Enrollment | create lead | at least one of `ContactName`, normalized phone, email, Zalo present; optional branch exists; optional owner exists and is `ManagementStaff` or `AccountantStaff` | empty contact info, duplicate phone/email/Zalo, invalid branch, invalid owner role | creates `Lead` with `Status=New`, `TouchCount=1`; optional child rows; creates initial `LeadActivity` | `Kidzgo.Application/Leads/CreateLead/CreateLeadCommandHandler.cs` | Full |
| ADM-02 | Create lead child | Lead / Placement / Registration / Enrollment | create lead child | parent lead exists, child name non-empty | missing lead, empty child name | creates `LeadChild` with `Status=New` and a `LeadActivity` | `Kidzgo.Application/Leads/CreateLeadChild/CreateLeadChildCommandHandler.cs` | Full |
| ADM-03 | Assign / self-assign lead increments touch count | Lead / Placement / Registration / Enrollment | assign lead, self-assign lead | lead exists; owner exists and is allowed staff; self-assign only for `ManagementStaff` | lead missing, invalid owner role | sets `OwnerStaffId`, increments `TouchCount`, writes activity log | `Kidzgo.Application/Leads/AssignLead/AssignLeadCommandHandler.cs`, `.../SelfAssignLead/SelfAssignLeadCommandHandler.cs` | Full |
| ADM-04 | Lead status becomes sticky after enrolled | Lead / Placement / Registration / Enrollment | update lead status | lead exists; if old status not `Enrolled` or new status also `Enrolled` | trying to move `Enrolled -> non-Enrolled` | updates lead status; first move to `Contacted` sets `FirstResponseAt`; any status change increments `TouchCount`; adds activity | `Kidzgo.Application/Leads/UpdateLeadStatus/UpdateLeadStatusCommandHandler.cs` | Full |
| ADM-05 | Placement test scheduling needs a lead target and can auto-create default child | Lead / Placement / Registration / Enrollment | schedule placement test | either `LeadChildId` or `LeadId`; duration valid; invigilator + room present; invigilator/room available; if only `LeadId`, handler may auto-create default `LeadChild` | missing target, invalid lead/child, missing invigilator/room, schedule conflict, invalid room/invigilator | creates `PlacementTest` with `Status=Scheduled`; moves `LeadChild` and `Lead` to `BookedTest` | `Kidzgo.Application/PlacementTests/SchedulePlacementTest/SchedulePlacementTestCommandHandler.cs` | Full |
| ADM-06 | Placement invigilator / room availability | Lead / Placement / Registration / Enrollment | schedule/update/retake placement | invigilator exists, active, role in `{Admin, ManagementStaff, AccountantStaff, Teacher}`; room active; optional branch match; no time overlap with other placement tests in same room or same invigilator | invalid invigilator, invalid room, branch mismatch, occupied schedule | blocks schedule creation/update | `Kidzgo.Application/PlacementTests/Shared/PlacementTestScheduleAvailability.cs` | Full |
| ADM-07 | Placement test update cannot modify completed test | Lead / Placement / Registration / Enrollment | update placement test | placement test exists; if schedule fields change, effective room/invigilator/duration/scheduled time valid and available; optional student profile is student; optional class exists | test missing, completed test, invalid student/class, missing room/invigilator when scheduling, schedule conflict | updates student/class/schedule/invigilator/room/duration | `Kidzgo.Application/PlacementTests/UpdatePlacementTest/UpdatePlacementTestCommandHandler.cs` | Full |
| ADM-08 | Placement cancel / no-show cannot target completed test | Lead / Placement / Registration / Enrollment | cancel placement, mark no-show | placement test exists and not completed | missing placement test, completed test | sets `PlacementTestStatus=Cancelled` or `NoShow`; cancel may append reason into `Notes` | `Kidzgo.Application/PlacementTests/CancelPlacementTest/CancelPlacementTestCommandHandler.cs`, `.../MarkPlacementTestNoShow/MarkPlacementTestNoShowCommandHandler.cs` | Full |
| ADM-09 | Placement results complete test and move CRM statuses | Lead / Placement / Registration / Enrollment | update placement results | placement exists; recommended primary program active; secondary recommendation if present must be different and active | invalid placement, invalid recommended program, same primary/secondary | when all 5 skill scores are present and test not completed, sets placement `Completed`, moves `LeadChild` and `Lead` from `BookedTest -> TestDone`, writes `LeadActivity` | `Kidzgo.Application/PlacementTests/UpdatePlacementTestResults/UpdatePlacementTestResultsCommandHandler.cs` | Full |
| ADM-10 | Retake placement can auto-open new registration from recommendation | Lead / Placement / Registration / Enrollment | update placement results for retake | retake placement has `OriginalPlacementTestId`, `StudentProfileId`, valid recommended program, cheapest active matching tuition plan found in same branch/program | missing student, missing recommendation, program not active in branch, tuition plan not found | completes old registration, creates new registration with `Status=WaitingForClass`, `EntryType=Retake`, carries preferred schedule and remaining sessions | `Kidzgo.Application/PlacementTests/UpdatePlacementTestResults/UpdatePlacementTestResultsCommandHandler.cs` | Full |
| ADM-11 | Manual retake placement scheduling | Lead / Placement / Registration / Enrollment | create retake placement | original placement exists; student profile exists and matches original placement; no other scheduled/completed retake for same student; branch exists; target program and tuition plan valid; if scheduled immediately, room/invigilator availability passes | original missing, wrong student, duplicate retake, invalid branch/program/tuition plan, schedule conflict | creates new retake `PlacementTest` with `Status=Scheduled`, copies linkage to original lead/lead child, logs activity | `Kidzgo.Application/PlacementTests/RetakePlacementTest/RetakePlacementTestCommandHandler.cs` | Full |
| ADM-12 | Convert lead to enrolled | Lead / Placement / Registration / Enrollment | convert lead to enrolled | placement test exists; linked lead exists; if `StudentProfileId` supplied it must be a student profile and not bound to another lead child; legacy lead or chosen child not already enrolled | missing placement/lead/student, already enrolled, conflicting student linkage | links lead child to student profile, copies child data into empty profile fields, sets `LeadChild.Status=Enrolled`, sets lead `Enrolled` if any child enrolled, completes placement if needed, adds activity | `Kidzgo.Application/PlacementTests/ConvertLeadToEnrolled/ConvertLeadToEnrolledCommandHandler.cs` | Full |
| ADM-13 | Registration creation validates academic + commercial consistency | Lead / Placement / Registration / Enrollment | create registration | student profile exists and is student; branch active; primary program active and assigned to branch; tuition plan active, same program, branch-compatible; secondary program if present active, different from primary, same branch, not supplementary | invalid student/branch/program/tuition plan; secondary same as primary; supplementary secondary; duplicate active registration touching same primary/secondary program | creates `Registration` with `Status=New`, `TotalSessions` and `RemainingSessions` from tuition plan | `Kidzgo.Application/Registrations/CreateRegistration/CreateRegistrationCommandHandler.cs` | Full |
| ADM-14 | Registration-to-class assignment enforces track, capacity, first-study-date and conflict rules | Lead / Placement / Registration / Enrollment | assign class | registration exists and not completed/cancelled; track valid; if assigning secondary, secondary program exists; if class assignment, class same branch + target program + available status; class not full; session selection pattern valid; `FirstStudyDate` not past, inside class timeline and maps to candidate session; no student schedule conflict | completed/cancelled registration, invalid track, missing secondary program, wait-after-enrollment rollback, invalid class/program/branch/status, full class, invalid selection pattern, invalid first study date, schedule conflict | `EntryType.Wait` keeps waiting list only; otherwise creates `ClassEnrollment Active`, optional schedule segment for supplementary program, syncs `StudentSessionAssignments`, updates class capacity status, sets registration status via `ResolveStatus`, may set `ActualStartDate` | `Kidzgo.Application/Registrations/AssignClass/AssignClassCommandHandler.cs`, `Kidzgo.Application/Registrations/RegistrationTrackHelper.cs` | Full |
| ADM-15 | Suggested classes are filtered by real schedule + capacity | Lead / Placement / Registration / Enrollment | suggest classes | registration exists | registration missing | returns classes in same branch/program with available seat and status in `{Recruiting, Active, Planned, Full-with-seat}`; `PreferredSchedule` is matched against RRULE day/time semantics | `Kidzgo.Application/Registrations/SuggestClasses/SuggestClassesQueryHandler.cs` | Full |
| ADM-16 | Transfer class drops old enrollment and creates new one | Lead / Placement / Registration / Enrollment | transfer class | registration exists and not completed/cancelled; target program/track valid; new class same branch + target program; new class not same as old, active/recruiting, not full; selection pattern valid; no schedule conflict | invalid registration status, wrong track/program/branch, same class, class full/unavailable, conflict | old enrollment becomes `Dropped`, future assignments cancelled from effective date; new active enrollment created; optional schedule segment for supplementary; registration class pointer and status updated; operation becomes `Transfer` | `Kidzgo.Application/Registrations/TransferClass/TransferClassCommandHandler.cs` | Full |
| ADM-17 | Create enrollment directly from class | Lead / Placement / Registration / Enrollment | create enrollment | class exists and status `Active` or `Planned`; student profile exists and is student; no active duplicate in same class; class has seat; selection pattern valid; optional tuition plan active, branch-compatible, same program; no schedule conflict | invalid class/student/plan, duplicate enrollment, class full, selection mismatch, schedule conflict | creates `ClassEnrollment Active`, optional schedule segment, syncs assignments | `Kidzgo.Application/Enrollments/CreateEnrollment/CreateEnrollmentCommandHandler.cs` | Full |
| ADM-18 | Update enrollment re-validates schedule and tuition plan | Lead / Placement / Registration / Enrollment | update enrollment | enrollment exists; optional new selection pattern still subset of class schedule; optional tuition plan active and same class program; no schedule conflict | invalid enrollment, invalid selection pattern, invalid tuition plan, schedule conflict | updates `EnrollDate`, `Track`, `SessionSelectionPattern`, `TuitionPlanId`; re-syncs assignments | `Kidzgo.Application/Enrollments/UpdateEnrollment/UpdateEnrollmentCommandHandler.cs` | Full |
| ADM-19 | Pause / reactivate / drop enrollment | Lead / Placement / Registration / Enrollment | pause enrollment, reactivate enrollment, drop enrollment | pause only when status `Active`; reactivate only when not already active and not dropped, class still available, class not full, no schedule conflict; drop when not already dropped | invalid current status, dropped reactivation, unavailable/full/conflicting class | pause sets `Paused` and cancels future assignments; reactivate sets `Active` and restores assignments; drop sets `Dropped` and cancels future assignments | `Kidzgo.Application/Enrollments/PauseEnrollment/PauseEnrollmentCommandHandler.cs`, `.../ReactivateEnrollment/ReactivateEnrollmentCommandHandler.cs`, `.../DropEnrollment/DropEnrollmentCommandHandler.cs` | Full |
| ADM-20 | Assign tuition plan to enrollment | Lead / Placement / Registration / Enrollment | assign tuition plan | enrollment exists; tuition plan exists, active, branch-compatible, same class program | invalid enrollment, missing/inactive plan, branch/program mismatch | sets `TuitionPlanId` | `Kidzgo.Application/Enrollments/AssignTuitionPlan/AssignTuitionPlanCommandHandler.cs` | Full |
| ADM-21 | Cancel registration cascades to active enrollments | Lead / Placement / Registration / Enrollment | cancel registration | registration exists and not completed/cancelled | missing registration, invalid status | drops active enrollments linked to registration, cancels future assignments, updates class capacity statuses, sets registration `Cancelled`, appends reason to note | `Kidzgo.Application/Registrations/CancelRegistration/CancelRegistrationCommandHandler.cs` | Full |

---

## Program / Class / Session

| ID | Rule name | Module | Trigger / API | Valid when | Blocked when | Status / data changes | Code | Impl |
|---|---|---|---|---|---|---|---|---|
| PCS-01 | Program creation | Program / Class / Session | create program | request carries fields | no deep BE rule besides command validation | creates active, non-deleted `Program` with `IsMakeup`, `IsSupplementary` flags | `Kidzgo.Application/Programs/CreateProgram/CreateProgramCommandHandler.cs` | CRUD only |
| PCS-02 | Program-branch assignment | Program / Class / Session | assign program to branch | branch active; program active and not deleted; assignment not duplicated | invalid branch/program, duplicate assignment | creates active `BranchProgram` row | `Kidzgo.Application/Programs/AssignProgramToBranch/AssignProgramToBranchCommandHandler.cs` | Full |
| PCS-03 | Program leave policy configuration | Program / Class / Session | upsert program leave policy | program exists; `MaxLeavesPerMonth > 0` | missing program, invalid max leaves | creates or updates `ProgramLeavePolicy`; later consumed by leave-request flow | `Kidzgo.Application/Programs/UpsertProgramLeavePolicy/UpsertProgramLeavePolicyCommandHandler.cs` | Full |
| PCS-04 | Tuition plan creation / update auto-computes unit price | Program / Class / Session | create/update tuition plan | program exists; optional branch active; if branch-scoped, program must be assigned to that branch | invalid program/branch, program not available in branch | calculates `UnitPriceSession = TuitionAmount / TotalSessions`, persists branch/program/package mapping | `Kidzgo.Application/TuitionPlans/CreateTuitionPlan/CreateTuitionPlanCommandHandler.cs`, `.../UpdateTuitionPlan/UpdateTuitionPlanCommandHandler.cs` | Full |
| PCS-05 | Tuition plan delete blocked by active or paused enrollments | Program / Class / Session | delete tuition plan | tuition plan exists and is not used by active/paused enrollments | plan missing, active/paused enrollments exist | soft deletes plan and deactivates it | `Kidzgo.Application/TuitionPlans/DeleteTuitionPlan/DeleteTuitionPlanCommandHandler.cs` | Full |
| PCS-06 | Class creation validates branch-program-teacher consistency | Program / Class / Session | create class | branch active; program active and assigned to branch; class code unique; teacher/assistant users exist, role teacher, same branch; if schedule pattern + end date are given, first up to 10 generated occurrences must not conflict | invalid branch/program, program not assigned, duplicate code, invalid teacher/branch, room/teacher/assistant conflict during sampled occurrences | creates class with `Status=Planned` | `Kidzgo.Application/Classes/CreateClass/CreateClassCommandHandler.cs` | Full |
| PCS-07 | Class update keeps basic consistency but does not re-run sampled conflict block | Program / Class / Session | update class | class exists; branch/program/teacher checks pass; code remains unique | invalid class/branch/program/teacher/code | updates class fields | `Kidzgo.Application/Classes/UpdateClass/UpdateClassCommandHandler.cs` | Partial |
| PCS-08 | Class status transition | Program / Class / Session | change class status | class exists and new status differs | unchanged status; `Closed -> Planned` | updates `ClassStatus`; no other transition matrix enforced here | `Kidzgo.Application/Classes/ChangeClassStatus/ChangeClassStatusCommandHandler.cs` | Partial |
| PCS-09 | Teacher assignment to class | Program / Class / Session | assign teacher | class exists; chosen teacher/assistant exist, role teacher, same branch | invalid class/teacher/branch | updates `MainTeacherId` / `AssistantTeacherId` | `Kidzgo.Application/Classes/AssignTeacher/AssignTeacherCommandHandler.cs` | Full |
| PCS-10 | Supplementary classes can have schedule segments | Program / Class / Session | add class schedule segment | class exists; program is supplementary; effective dates inside class window and ordered; pattern generates at least one occurrence; no duplicate or earlier future segment | non-supplementary program, invalid date window, empty/invalid pattern, duplicate segment, future segment already exists | adjusts previous segment `EffectiveTo`, creates new `ClassScheduleSegment`, updates class schedule pattern; may generate sessions immediately | `Kidzgo.Application/Classes/AddClassScheduleSegment/AddClassScheduleSegmentCommandHandler.cs` | Full |
| PCS-11 | Generate sessions from class pattern | Program / Class / Session | generate sessions from pattern | class exists; class status `Planned` or `Active`; class has `EndDate`; branch/room/teachers still valid; parsed occurrences exist | invalid class status, missing end date, invalid branch/room/teacher/assistant, generated occurrence conflicts, save errors | creates distinct `Session` rows for missing occurrences, optionally future-only, and syncs assignments per new session | `Kidzgo.Application/Sessions/GenerateSessionsFromPattern/GenerateSessionsFromPatternCommandHandler.cs`, `Kidzgo.Application/Services/SessionGenerationService.cs` | Full |
| PCS-12 | Single session create / update allows warning-only conflict checks | Program / Class / Session | create session, update session | class/session exists; class status valid; target session not cancelled/completed for update | invalid class status, missing session, cancelled/completed session update | creates or updates session and syncs assignments; conflict checker is executed but result is warning-only and does not block save | `Kidzgo.Application/Sessions/CreateSession/CreateSessionCommandHandler.cs`, `.../UpdateSession/UpdateSessionCommandHandler.cs` | Partial |
| PCS-13 | Batch session update hard-blocks conflicts | Program / Class / Session | update sessions by class | class exists; targeted sessions are not cancelled/completed; conflict-free after proposed change | class missing, conflict found, session already cancelled/completed | updates chosen session fields, skips conflicting ones, syncs assignments on updated sessions | `Kidzgo.Application/Sessions/UpdateSessionsByClass/UpdateSessionsByClassCommandHandler.cs` | Full |
| PCS-14 | Session completion / cancellation | Program / Class / Session | complete session, cancel session | session exists; completion not on cancelled session; cancellation not on already-cancelled session | missing session, cancelled session complete, already cancelled cancel | `Complete` sets `Status=Completed`, writes `ActualDatetime`; `Cancel` sets `Status=Cancelled` and re-syncs assignments | `Kidzgo.Application/Sessions/CompleteSession/CompleteSessionCommandHandler.cs`, `.../CancelSession/CancelSessionCommandHandler.cs` | Full |
| PCS-15 | Enrollment session-selection pattern must be subset of class schedule | Program / Class / Session | registration assign class, enrollment update, add enrollment schedule segment | parsed selection pattern exists and matches at least one slot; class occurrences exist; every selected slot is subset of class RRULE slots in validation range | invalid RRULE, empty match, not a subset, invalid segment date range | blocks assignment / update / segment creation | `Kidzgo.Application/Services/StudentSessionAssignmentService.cs` | Full |
| PCS-16 | Student session assignment sync | Program / Class / Session | enrollment create/update/reactivate/pause/session create/update/cancel/generation | enrollment active, session not cancelled, session date after enroll date, not paused, selection pattern matches | pause window, cancelled session, out-of-pattern session | creates / reactivates / cancels `StudentSessionAssignment` rows and preserves track + registration linkage | `Kidzgo.Application/Services/StudentSessionAssignmentService.cs` | Full |
| PCS-17 | Student schedule conflict check uses real session slots with minimum gap | Program / Class / Session | assign class, create/update/reactivate enrollment, transfer, reassign equivalent class | no overlap and no slot within 15 minutes of another booked slot; can exclude current enrollment or legacy class from effective date onward | overlap or gap `< 15` minutes | blocks enrollment-affecting mutation with conflict error naming class/time | `Kidzgo.Application/Services/StudentEnrollmentScheduleConflictService.cs` | Full |

---

## Attendance / Leave / Makeup / Pause

| ID | Rule name | Module | Trigger / API | Valid when | Blocked when | Status / data changes | Code | Impl |
|---|---|---|---|---|---|---|---|---|
| ATT-01 | Attendance mark only for real participants and not future sessions for non-admin | Attendance / Leave / Makeup / Pause | mark attendance | session exists; non-admin marks today/past only; each student is an actual session participant | missing session, future session by non-admin, student not in participant list | creates or updates `Attendance`; sets `MarkedBy`, `MarkedAt`; updates registration session consumption for regular participants | `Kidzgo.Application/Attendance/MarkAttendance/MarkAttendanceCommandHandler.cs` | Full |
| ATT-02 | Approved leave locks attendance for non-admin | Attendance / Leave / Makeup / Pause | mark/update attendance | approved leave does not exist for student-session, unless admin | approved leave exists and caller not admin | mark flow returns `Makeup` placeholder result and skips write; update flow blocks with `ApprovedLeaveLocked` | `Kidzgo.Application/Attendance/MarkAttendance/MarkAttendanceCommandHandler.cs`, `.../UpdateAttendance/UpdateAttendanceCommandHandler.cs`, `Kidzgo.Application/Services/ApprovedLeaveAttendanceService.cs` | Full |
| ATT-03 | Absence type resolution uses leave notice hours | Attendance / Leave / Makeup / Pause | mark/update attendance absent | approved leave exists for session/date range | no approved leave | absent attendance gets `WithNotice24H`, `Under24H`, or `NoNotice` from `NoticeHours` | `Kidzgo.Application/Attendance/MarkAttendance/MarkAttendanceCommandHandler.cs`, `.../UpdateAttendance/UpdateAttendanceCommandHandler.cs` | Full |
| ATT-04 | 24h leave creates makeup credit | Attendance / Leave / Makeup / Pause | mark attendance absent, create/approve leave | approved leave with `NoticeHours >= 24` and no existing credit for same source session | credit already exists | creates `MakeupCredit` with `Status=Available`, `CreatedReason=ApprovedLeave24H`; some flows auto-allocate weekend makeup | `Kidzgo.Application/Attendance/MarkAttendance/MarkAttendanceCommandHandler.cs`, `Kidzgo.Application/LeaveRequests/CreateLeaveRequest/CreateLeaveRequestCommandHandler.cs`, `.../ApproveLeaveRequest/ApproveLeaveRequestCommandHandler.cs` | Full |
| ATT-05 | Attendance update window | Attendance / Leave / Makeup / Pause | update attendance | attendance exists; non-admin edits same-day or within 24h after session end; no approved leave lock | missing attendance, future session by non-admin, update after 24h, approved leave exists | updates attendance and writes `AuditLog` with before/after, actor, IP | `Kidzgo.Application/Attendance/UpdateAttendance/UpdateAttendanceCommandHandler.cs` | Full |
| ATT-06 | Approved leave can retroactively change consumption after attendance already exists | Attendance / Leave / Makeup / Pause | approve/reject leave after attendance was marked | attendance exists and student is regular participant | no attendance / no regular participant | service applies attendance transition from actual attendance status to `Makeup`, and back on deactivation; it does not overwrite attendance row itself | `Kidzgo.Application/Services/ApprovedLeaveAttendanceService.cs` | Full |
| ATT-07 | Leave request requires real enrollment or assignment | Attendance / Leave / Makeup / Pause | create leave request | student profile active; class exists; if specific session then session belongs to class and student assigned to that session; if date range then active enrollment exists and assigned sessions in range are found | missing student/class/session, student not assigned, not enrolled, no sessions in range | creates one `LeaveRequest` per matching session date | `Kidzgo.Application/LeaveRequests/CreateLeaveRequest/CreateLeaveRequestCommandHandler.cs` | Full |
| ATT-08 | Leave duplicate and monthly limit | Attendance / Leave / Makeup / Pause | create leave request | no existing pending/approved leave for any selected session; total leave dates in month within configured limit | duplicate leave exists; total selected + existing dates exceed configured max | blocks request creation | `Kidzgo.Application/LeaveRequests/CreateLeaveRequest/CreateLeaveRequestCommandHandler.cs` | Full |
| ATT-09 | 24h rule auto-approval | Attendance / Leave / Makeup / Pause | create leave request | `noticeHours >= 24` | `< 24h` notice | leave row is created with `Status=Approved` if enough notice, otherwise `Pending`; approved leaves also set `ApprovedAt` | `Kidzgo.Application/LeaveRequests/CreateLeaveRequest/CreateLeaveRequestCommandHandler.cs` | Full |
| ATT-10 | Approved leave can auto-schedule weekend makeup | Attendance / Leave / Makeup / Pause | create leave request, approve leave | makeup credit exists; there is active makeup class/session in same branch, eligible future weekend date, not over capacity, no duplicate allocation for same student/session | no eligible session or session full | creates `MakeupAllocation Pending`, marks credit `Used`, writes `UsedSessionId` | `Kidzgo.Application/LeaveRequests/CreateLeaveRequest/CreateLeaveRequestCommandHandler.cs`, `.../ApproveLeaveRequest/ApproveLeaveRequestCommandHandler.cs` | Full |
| ATT-11 | Leave approval / rejection | Attendance / Leave / Makeup / Pause | approve leave, reject leave, bulk approve | leave exists; approve rejects already approved; reject rejects already rejected | missing leave, repeated same decision | approve sets `Approved`, `ApprovedAt`, `ApprovedBy`, creates credits/allocations and applies leave activation; reject sets `Rejected`, removes leave-created credits/allocations and reverses leave activation if it was approved | `Kidzgo.Application/LeaveRequests/ApproveLeaveRequest/ApproveLeaveRequestCommandHandler.cs`, `.../RejectLeaveRequest/RejectLeaveRequestCommandHandler.cs`, `.../BulkApproveLeaveRequests/BulkApproveLeaveRequestsCommandHandler.cs` | Full |
| ATT-12 | Makeup credit usage | Attendance / Leave / Makeup / Pause | use makeup credit | student resolved from current student profile or parent-student link; credit belongs to student, not expired, status `Available` or future-reschedulable `Used`; target session exists, future, weekend, eligible by makeup-date rule, belongs to requested class, student not already in target, target not full, no conflict within +/-1 day and no slot within 2 hours | wrong owner, expired credit, past target date, non-weekend, wrong class, full session, target conflict, changing past allocation, wrong makeup program when rescheduling | cancels old active allocations, marks credit `Used`, sets `UsedSessionId`, creates new `MakeupAllocation Pending` | `Kidzgo.Application/MakeupCredits/UseMakeupCredit/UseMakeupCreditCommandHandler.cs` | Full |
| ATT-13 | Pause request creation | Attendance / Leave / Makeup / Pause | create pause request | student profile active; at least one active enrollment exists; eligible class resolver finds classes with assigned sessions in pause window; no overlapping pending/approved pause request | missing student, no enrollments in range, duplicate overlapping request | creates `PauseEnrollmentRequest` with `Status=Pending` and returns impacted classes | `Kidzgo.Application/PauseEnrollmentRequests/CreatePauseEnrollmentRequest/CreatePauseEnrollmentRequestCommandHandler.cs`, `Kidzgo.Application/Services/PauseEnrollmentEligibleClassResolver.cs` | Full |
| ATT-14 | Pause approval reserves sessions and reconciles future leave/makeup | Attendance / Leave / Makeup / Pause | approve pause request | request exists and still pending; student has active enrollments in affected classes | already approved/rejected/cancelled, no affected enrollments | sets request `Approved`, pauses enrollments, cancels assignments in pause range, cancels overlapping pending/approved leaves, removes affected makeup credits/allocations and related attendance, writes pause history, sets `ReservedSessionCount`, `ReservationExpiresOn = PauseFrom + 3 months` | `Kidzgo.Application/PauseEnrollmentRequests/ApprovePauseEnrollmentRequest/ApprovePauseEnrollmentRequestCommandHandler.cs` | Full |
| ATT-15 | Pause rejection / cancellation | Attendance / Leave / Makeup / Pause | reject pause request, cancel pause request | reject only when pending; cancel only when pending and `today < PauseFrom` | already approved/rejected/cancelled, cancellation window expired | sets request `Rejected` or `Cancelled`; reject also sends notification | `Kidzgo.Application/PauseEnrollmentRequests/RejectPauseEnrollmentRequest/RejectPauseEnrollmentRequestCommandHandler.cs`, `.../CancelPauseEnrollmentRequest/CancelPauseEnrollmentRequestCommandHandler.cs` | Full |
| ATT-16 | Pause outcome management | Attendance / Leave / Makeup / Pause | update pause outcome | pause request is `Approved`, outcome not already completed | request not approved, outcome already completed | `ReassignEquivalentClass` drops paused enrollments from effective date and asks staff follow-up; `ContinueWithTutoring` cancels assignments after pause; stores `Outcome`, `OutcomeNote`, `OutcomeBy`, `OutcomeAt`; sends notifications | `Kidzgo.Application/PauseEnrollmentRequests/UpdatePauseEnrollmentOutcome/UpdatePauseEnrollmentOutcomeCommandHandler.cs` | Full |
| ATT-17 | Reassign equivalent class after pause | Attendance / Leave / Makeup / Pause | reassign equivalent class | pause request approved with outcome `ReassignEquivalentClass`; registration belongs to same student and not completed/cancelled; effective date after pause end; old paused enrollment exists; new class same target program, not same class, active/recruiting, not full, no duplicate enrollment, valid pattern, no conflict | invalid pause outcome, wrong registration, no paused enrollment, invalid target class, class full/unavailable, conflict | old paused enrollment becomes `Dropped`, future assignments cancelled; new active enrollment created; registration class pointer updated; pause outcome marked completed | `Kidzgo.Application/PauseEnrollmentRequests/ReassignEquivalentClass/ReassignEquivalentClassCommandHandler.cs` | Full |
| ATT-18 | Continue-same-class auto-reactivation job | Attendance / Leave / Makeup / Pause | Quartz resume paused enrollments job | pause request approved, outcome `ContinueSameClass`, pause period ended, not already reactivated, class still available and capacity/conflict checks pass | pause not due yet, class unavailable/full, conflict, dropped enrollment | paused enrollments return to `Active`, assignments restored from effective date, reactivation history appended | `Kidzgo.Infrastructure/BackgroundJobs/ResumePausedEnrollmentsJob.cs`, `Kidzgo.Application/Services/PauseEnrollmentReactivationService.cs` | Full |

---

## Homework / Submission / Grading

| ID | Rule name | Module | Trigger / API | Valid when | Blocked when | Status / data changes | Code | Impl |
|---|---|---|---|---|---|---|---|---|
| HW-01 | Homework assignment creation | Homework / Submission / Grading | create homework assignment | non-empty title; valid `MaxScore`, `RewardStars`, `TimeLimitMinutes`, `MaxAttempts`; class exists; optional session exists and belongs to class; optional mission exists; due date is future | invalid title/score/reward/time/max attempts, missing class/session/mission, past due date | creates `HomeworkAssignment`; auto-creates `HomeworkStudent` for all active students in class; if `SessionId` exists it still assigns all active class students, not only attendees | `Kidzgo.Application/Homework/CreateHomeworkAssignment/CreateHomeworkAssignmentCommandHandler.cs` | Full |
| HW-02 | Homework assignment update is locked after submissions start | Homework / Submission / Grading | update homework assignment | homework exists; no submission is already `Submitted` or `Graded`; changed due date still future; optional mission exists | homework missing, any submitted/graded work exists, invalid due date/field values | updates assignment fields; command attempts to update late status on due-date change, but logic uses updated due date and is weak | `Kidzgo.Application/Homework/UpdateHomeworkAssignment/UpdateHomeworkAssignmentCommandHandler.cs` | Partial |
| HW-03 | Generic submission ownership and attempt limit | Homework / Submission / Grading | submit homework | current context has `StudentId`; `HomeworkStudent` exists and belongs to same student; attempt count `< MaxAttempts` | no student in context, submission missing, not owner, attempt limit reached | creates `HomeworkSubmissionAttempt`; updates submission fields/time/status | `Kidzgo.Application/Homework/SubmitHomework/SubmitHomeworkCommandHandler.cs` | Full |
| HW-04 | Generic submission payload validation by submission type | Homework / Submission / Grading | submit homework | file/image/video have attachment; text has `TextAnswer`; link has URL; quiz can use attachment/text/link | payload does not match submission type | status becomes `Submitted` if on time, `Late` if due date passed or prior status was `Missing`; grading fields are reset on new submit | `Kidzgo.Application/Homework/SubmitHomework/SubmitHomeworkCommandHandler.cs` | Full |
| HW-05 | Missing auto-graded homework cannot be resubmitted via generic flow | Homework / Submission / Grading | submit homework | status is not auto-graded-missing state | status `Missing` with `SubmittedAt == null` and `GradedAt != null` | blocks resubmission | `Kidzgo.Application/Homework/SubmitHomework/SubmitHomeworkCommandHandler.cs` | Full |
| HW-06 | On-time first submission rewards stars and mission tracking | Homework / Submission / Grading | submit homework, submit MCQ homework | submission is first attempt and still on time; assignment has positive `RewardStars` | repeated attempt, late submission, no reward configured | adds stars through gamification service; tracks homework mission progress after save | `Kidzgo.Application/Homework/SubmitHomework/SubmitHomeworkCommandHandler.cs`, `.../SubmitMultipleChoiceHomework/SubmitMultipleChoiceHomeworkCommandHandler.cs`, `Kidzgo.Application/Missions/Shared/HomeworkMissionProgressTracker.cs` | Full |
| HW-07 | Quiz submission is auto-graded immediately | Homework / Submission / Grading | submit multiple-choice homework | caller is owning student; assignment `SubmissionType=Quiz`; answers provided; every question belongs to assignment; time limit not expired; attempts remain | missing student, wrong owner, non-quiz assignment, no answers, unknown question IDs, time expired, status `Missing`, attempt limit reached | computes correct/wrong/skipped, points and scaled score; sets `Status=Graded`, `SubmittedAt`, `GradedAt`, stores answers JSON, creates attempt | `Kidzgo.Application/Homework/SubmitMultipleChoiceHomework/SubmitMultipleChoiceHomeworkCommandHandler.cs` | Full |
| HW-08 | Manual grading | Homework / Submission / Grading | grade homework | submission exists; score >= 0 and <= assignment max; current status `Submitted` or `Graded` | submission missing, invalid score, status not submitted/graded | sets `Status=Graded`, saves score + teacher feedback + `GradedAt`, updates latest attempt mirror | `Kidzgo.Application/Homework/GradeHomework/GradeHomeworkCommandHandler.cs` | Full |
| HW-09 | Manual late/missing transition | Homework / Submission / Grading | mark homework late or missing | command target status is `Late` or `Missing`; current status is `Assigned`, or `Late -> Missing` | invalid target status, invalid source status | updates submission status and retracks homework missions | `Kidzgo.Application/Homework/MarkHomeworkLateOrMissing/MarkHomeworkLateOrMissingCommandHandler.cs` | Full |
| HW-10 | Overdue homework auto-mark job | Homework / Submission / Grading | Quartz overdue homework job | homework still `Assigned`, no submission, due date passed | n/a | bulk marks to `Missing`, sets `Score=0`, `GradedAt=now`, default feedback if empty, then re-tracks homework missions | `Kidzgo.Infrastructure/BackgroundJobs/MarkOverdueHomeworkSubmissionsJob.cs` | Full |
| HW-11 | AI grading can persist as final grade | Homework / Submission / Grading | AI grade homework | submission exists, status `Submitted` or `Graded`, and has text or attachment | missing submission, invalid status, no answer content | if AI actually used, sets `Status=Graded`, computes normalized score, stores serialized AI feedback, updates latest attempt mirror | `Kidzgo.Application/Homework/AiGradeHomework/AiGradeHomeworkCommandHandler.cs` | Full |
| HW-12 | AI hint / recommendation / speaking-practice access control | Homework / Submission / Grading | AI hint, AI recommendation, speaking practice analysis | current user is student owner; assignment has feature flag enabled for hint/recommend; speaking practice file not empty | no student in context, wrong owner, feature not enabled, empty file | no business status transition; returns AI assistance only | `Kidzgo.Application/Homework/GetHomeworkHint/GetHomeworkHintQueryHandler.cs`, `.../GetHomeworkRecommendations/GetHomeworkRecommendationsQueryHandler.cs`, `.../AnalyzeSpeakingPractice/AnalyzeSpeakingPracticeQueryHandler.cs` | Full |

---

## Report / AI / Approve / Publish

| ID | Rule name | Module | Trigger / API | Valid when | Blocked when | Status / data changes | Code | Impl |
|---|---|---|---|---|---|---|---|---|
| REP-01 | Session report creation only after session end and by assigned teacher | Report / AI / Approve / Publish | create session report | session exists and already ended; student profile exists and is student; current user exists and is teacher; teacher is planned/actual teacher of session; student is assigned to session; no existing report for same session-student | session missing/not ended, invalid student, wrong teacher, student not assigned, duplicate report | creates `SessionReport` with `Status=Draft`, `IsMonthlyCompiled=false` | `Kidzgo.Application/SessionReports/CreateSessionReport/CreateSessionReportCommandHandler.cs`, `Kidzgo.Application/SessionReports/Shared/SessionReportScheduleGuard.cs` | Full |
| REP-02 | Session report editing window | Report / AI / Approve / Publish | update session report | report exists, status in `{Draft, Review, Rejected}`, session already ended | missing report, invalid status, session not ended | updates feedback; if previously `Rejected`, resets status to `Draft` and clears review metadata | `Kidzgo.Application/SessionReports/UpdateSessionReport/UpdateSessionReportCommandHandler.cs` | Full |
| REP-03 | Session report submit / review / approve / reject / publish | Report / AI / Approve / Publish | submit, approve, reject, publish session report | submit only from `Draft`; approve/reject only from `Review`; publish only from `Approved`; publish and submit also require session ended | invalid current status, session not ended, missing report | transitions `Draft -> Review -> Approved/Rejected -> Published`; stores submit/review/publish metadata; publish creates in-app notifications for linked parents/student | `Kidzgo.Application/SessionReports/SubmitSessionReport/SubmitSessionReportCommandHandler.cs`, `.../ApproveSessionReport/...`, `.../RejectSessionReport/...`, `.../PublishSessionReport/...` | Full |
| REP-04 | Report request creation validates teacher-target relationship | Report / AI / Approve / Publish | create report request | either target student or target class exists; assigned teacher exists and is teacher; session request has session and teacher really assigned to that session; monthly request has valid month/year and target class resolution; target student actually belongs to session/class if student-scoped | missing target, invalid teacher, missing session, class-session mismatch, teacher not assigned, student not in session/class, class ambiguous for monthly student request | creates `ReportRequest` with `Status=Requested`; may auto-create missing monthly report rows; creates teacher notification | `Kidzgo.Application/ReportRequests/CreateReportRequest/CreateReportRequestCommandHandler.cs` | Full |
| REP-05 | Report request completion / cancellation | Report / AI / Approve / Publish | complete report request, cancel report request | complete only from `Requested` or `InProgress`; teacher can only complete own assigned requests; optional linked report must match request target; cancel allowed from any non-`Approved`, non-`Cancelled` state | request missing, unauthorized teacher, invalid status, linked report mismatch | complete moves request to `Submitted`; cancel moves request to `Cancelled` | `Kidzgo.Application/ReportRequests/CompleteReportRequest/CompleteReportRequestCommandHandler.cs`, `.../CancelReportRequest/CancelReportRequestCommandHandler.cs` | Full |
| REP-06 | Report-request workflow auto-links review status | Report / AI / Approve / Publish | session/monthly report submit + review flows | matching request exists in `Requested` or `InProgress` (submit) or in `{Requested, InProgress, Submitted}` (review) | n/a | auto-updates matching request to `Submitted`, `Approved`, or `Rejected` and links report ID | `Kidzgo.Application/ReportRequests/Shared/ReportRequestWorkflow.cs` | Full |
| REP-07 | Monthly report AI draft generation is teacher/class-scoped | Report / AI / Approve / Publish | generate monthly report draft | report exists; caller is teacher; teacher teaches that class; aggregated data exists; AI service returns non-empty result | report missing, non-teacher caller, teacher not assigned to class, no data, AI failure | refreshes/stores `MonthlyReportData` slices, writes `DraftContent`, forces `Status=Draft` | `Kidzgo.Application/MonthlyReports/GenerateMonthlyReportDraft/GenerateMonthlyReportDraftCommandHandler.cs` | Full |
| REP-08 | Monthly report data aggregation input set | Report / AI / Approve / Publish | aggregate monthly report data, generate AI draft | student/month/class inputs valid at caller level | n/a in aggregator itself | aggregates attendance, homework, test, mission, session report notes, lesson-plan topics/contents into JSON slices | `Kidzgo.Infrastructure/Services/MonthlyReportDataAggregator.cs` | Full |
| REP-09 | AI monthly report prompt uses recent published history | Report / AI / Approve / Publish | AI draft generation | aggregated JSON valid; student/profile/class resolvable | invalid JSON, missing student profile, upstream HTTP failure | sends A6 payload with student info, range, attendance/homework/test/mission/topics/session feedbacks and last 3 published reports of same program | `Kidzgo.Infrastructure/AI/HttpAiReportGenerator.cs` | Full |
| REP-10 | AI failure does not destroy report row | Report / AI / Approve / Publish | generate monthly report draft | AI call succeeds | AI unavailable / empty / invalid | handler returns failure; `DraftContent` is not updated; new `MonthlyReportData` changes prepared before AI call are not committed because final save only happens on success | `Kidzgo.Application/MonthlyReports/GenerateMonthlyReportDraft/GenerateMonthlyReportDraftCommandHandler.cs` | Partial |
| REP-11 | Monthly report draft editing | Report / AI / Approve / Publish | update monthly report draft | report exists; status in `{Draft, Review, Rejected}`; caller is teacher of that class | missing report, wrong role, teacher not assigned, invalid status | writes `DraftContent`; if rejected, resets status to `Draft` and clears review metadata; raw text is wrapped to valid JSON if needed | `Kidzgo.Application/MonthlyReports/UpdateMonthlyReportDraft/UpdateMonthlyReportDraftCommandHandler.cs` | Full |
| REP-12 | Monthly report review lifecycle | Report / AI / Approve / Publish | submit, approve, reject monthly report | submit only from `Draft`; approve/reject only from `Review` | report missing, invalid current status | transitions `Draft -> Review -> Approved/Rejected`; records submit/review metadata; syncs linked report requests | `Kidzgo.Application/MonthlyReports/SubmitMonthlyReport/SubmitMonthlyReportCommandHandler.cs`, `.../ApproveMonthlyReport/...`, `.../RejectMonthlyReport/...` | Full |
| REP-13 | Monthly report publish | Report / AI / Approve / Publish | publish monthly report | report exists and status `Approved` | missing report, invalid status | `Status=Published`, `PublishedAt=now`; if `FinalContent` empty and `DraftContent` exists, copies draft into final; raises `MonthlyReportPublishedEvent` | `Kidzgo.Application/MonthlyReports/PublishMonthlyReport/PublishMonthlyReportCommandHandler.cs` | Full |
| REP-14 | Monthly report comments have no workflow gate | Report / AI / Approve / Publish | add report comment | report exists | report missing | simply inserts `ReportComment`; no status restriction in handler | `Kidzgo.Application/MonthlyReports/AddReportComment/AddReportCommentCommandHandler.cs` | CRUD only |

---

## Gamification / Mission / Reward

| ID | Rule name | Module | Trigger / API | Valid when | Blocked when | Status / data changes | Code | Impl |
|---|---|---|---|---|---|---|---|---|
| GAM-01 | Mission reward rule configuration | Gamification / Mission / Reward | create/update mission reward rule | `TotalRequired > 0`; reward stars/xp not both zero; no duplicate `(MissionType, ProgressMode, TotalRequired)` | invalid total/reward, duplicate rule | creates/updates active reward rule rows used later at mission creation | `Kidzgo.Application/Gamification/CreateMissionRewardRule/CreateMissionRewardRuleCommandHandler.cs`, `.../UpdateMissionRewardRule/UpdateMissionRewardRuleCommandHandler.cs` | Full |
| GAM-02 | Mission creation validates scope and target ownership | Gamification / Mission / Reward | create mission | required target fields provided for scope; end date after start date; target class/student/group exist; if caller is teacher, all targets must belong to classes they teach; active reward rule exists and divides requested `TotalRequired` | invalid scope/date, missing class/student, unauthorized teacher targeting, missing reward rule config | creates mission with resolved rewards; auto-creates `MissionProgress` for target students; sends in-app mission notifications | `Kidzgo.Application/Missions/CreateMission/CreateMissionCommandHandler.cs`, `Kidzgo.Application/Missions/Shared/TeacherMissionTargetGuard.cs`, `.../MissionRewardRuleResolver.cs` | Full |
| GAM-03 | Mission update keeps same scope + reward resolution constraints | Gamification / Mission / Reward | update mission | mission exists; scope/date/target validity still passes; teacher can still manage those targets; reward rule resolves | missing mission, invalid target/date, unauthorized teacher, no reward config | updates mission metadata and recalculates rewards from rule | `Kidzgo.Application/Missions/UpdateMission/UpdateMissionCommandHandler.cs` | Full |
| GAM-04 | Mission delete blocked if already in use | Gamification / Mission / Reward | delete mission | mission exists and has no `MissionProgress` rows | missing mission, progress rows exist | removes mission row | `Kidzgo.Application/Missions/DeleteMission/DeleteMissionCommandHandler.cs` | Full |
| GAM-05 | Mission reward rule resolution is divisor-based | Gamification / Mission / Reward | mission create/update | there is an active rule with same mission type + progress mode and `requestedTotalRequired % rule.TotalRequired == 0` | no matching active rule, invalid `TotalRequired` | reward stars/xp are multiplied by divisor | `Kidzgo.Application/Missions/Shared/MissionRewardRuleResolver.cs` | Full |
| GAM-06 | Mission progress auto-reward | Gamification / Mission / Reward | mission trackers | progress becomes `>= TotalRequired` | `TotalRequired` absent/invalid or threshold not met | `MissionProgress` moves `Assigned -> InProgress -> Completed`; grants stars and XP once completed | `Kidzgo.Application/Missions/Shared/MissionProgressRewardHelper.cs` | Full |
| GAM-07 | Attendance mission progress is driven by real attendance rows | Gamification / Mission / Reward | attendance mark/update | mission active for student and time window; if class-scoped, target class matches | no active mission | count mode counts distinct `Present` session IDs; streak mode resets on `Absent` only | `Kidzgo.Application/Missions/Shared/ClassAttendanceMissionProgressTracker.cs` | Full |
| GAM-08 | Homework mission progress is driven by real homework submissions | Gamification / Mission / Reward | homework submit / overdue mark / status change | mission active for student and time window | no active mission | count mode counts distinct on-time submissions; streak mode groups by due date and resets when any homework due that day is not on-time | `Kidzgo.Application/Missions/Shared/HomeworkMissionProgressTracker.cs` | Full |
| GAM-09 | Daily check-in streak | Gamification / Mission / Reward | student daily check-in | current user has student profile; not already checked in today | non-student caller | creates `AttendanceStreak` for today; if checked in yesterday streak increments, else resets to 1; rewards stars + XP based on settings | `Kidzgo.Application/Gamification/CheckInAttendanceStreak/CheckInAttendanceStreakCommandHandler.cs` | Full |
| GAM-10 | Missed daily check-in streak reset job | Gamification / Mission / Reward | Quartz streak reset job | mission type `NoUnexcusedAbsence`, streak mode, progress > 0, latest check-in before yesterday | n/a | resets mission progress back to `Assigned` with value `0` when streak was missed | `Kidzgo.Infrastructure/BackgroundJobs/ResetMissedDailyCheckInMissionProgressJob.cs`, `Kidzgo.Application/Missions/Shared/DailyCheckInMissionProgressTracker.cs` | Full |
| GAM-11 | Manual stars and XP adjustments | Gamification / Mission / Reward | add/deduct stars, add/deduct XP | student profile exists; deduct stars requires enough balance | missing profile, insufficient stars | stars create `StarTransaction` with new balance; XP updates or creates `StudentLevel`, clamps deducted XP at 0 and recalculates level | `Kidzgo.Application/Gamification/AddStars/AddStarsCommandHandler.cs`, `.../DeductStars/...`, `.../AddXp/...`, `.../DeductXp/...` | Full |
| GAM-12 | System gamification hooks use academic data, not separate fake events | Gamification / Mission / Reward | homework submit, attendance mark, mission completion, daily check-in | underlying academic mutation succeeds | underlying academic mutation blocked | homework and attendance handlers call trackers/service after real data save; mission completion rewards write real star transactions and XP updates | `Kidzgo.Application/Services/GamificationService.cs`, academic handlers above | Full |
| GAM-13 | Reward redemption request deducts stars immediately | Gamification / Mission / Reward | request reward redemption | current user has student profile; item exists, active, not deleted; quantity > 0; student has enough stars | non-student caller, invalid item, inactive item, invalid quantity, insufficient stars | deducts stars first, then creates `RewardRedemption` with `Status=Requested`, stores `ItemName` snapshot and `StarsDeducted` | `Kidzgo.Application/Gamification/RequestRewardRedemption/RequestRewardRedemptionCommandHandler.cs` | Full |
| GAM-14 | Reward redemption status transitions | Gamification / Mission / Reward | approve, cancel, mark delivered, confirm received | `Requested -> Approved`; `Requested/Approved -> Cancelled`; `Approved -> Delivered`; `Delivered -> Received` and only owning student can confirm | invalid current status, wrong student owner | updates redemption timestamps/handler fields; cancel refunds stars through `AddStars` | `Kidzgo.Application/Gamification/ApproveRewardRedemption/ApproveRewardRedemptionCommandHandler.cs`, `.../CancelRewardRedemption/...`, `.../MarkDeliveredRewardRedemption/...`, `.../ConfirmReceivedRewardRedemption/...` | Full |
| GAM-15 | Batch delivery and auto-confirm | Gamification / Mission / Reward | batch deliver redemptions, auto-confirm job | batch deliver filters approved redemptions by optional month/year; auto-confirm finds delivered redemptions older than configured N days and not yet received | invalid month/year input | batch updates to `Delivered`; job auto-updates `Delivered -> Received` after configured wait days (default 3) | `Kidzgo.Application/Gamification/BatchDeliverRewardRedemptions/BatchDeliverRewardRedemptionsCommandHandler.cs`, `Kidzgo.Infrastructure/BackgroundJobs/AutoConfirmRewardRedemptionJob.cs` | Full |

---

## Master Business Rule List

- `AUTH-01` JWT claim issuance
- `AUTH-02` Email login
- `AUTH-03` Phone login
- `AUTH-04` Refresh token rotation
- `AUTH-05` Logout revokes refresh tokens
- `AUTH-06` Forgot password opaque response
- `AUTH-07` Reset password consumes token
- `AUTH-08` Change password
- `AUTH-09` Auth student-profile select is ownership check only
- `AUTH-10` Alternate student-profile select issues token with `StudentId`
- `AUTH-11` Role-based authorization strings mismatch tokenable roles
- `AUTH-12` Create profile auto-fills and auto-links
- `AUTH-13` Approve profile bulk flow
- `AUTH-14` Reactivate profile affects all approved profiles of same user
- `AUTH-15` Parent-student manual linking
- `AUTH-16` Parent PIN verify and first-time setup
- `AUTH-17` Parent PIN change
- `AUTH-18` Parent PIN reset via email
- `AUTH-19` Parent PIN reset via Zalo OTP request
- `AUTH-20` Parent PIN reset OTP verify
- `AUTH-21` Parent PIN reset by token
- `ADM-01` Create lead requires real contact point and blocks duplicates
- `ADM-02` Create lead child
- `ADM-03` Assign / self-assign lead increments touch count
- `ADM-04` Lead status becomes sticky after enrolled
- `ADM-05` Placement test scheduling needs a lead target
- `ADM-06` Placement invigilator / room availability
- `ADM-07` Placement test update cannot modify completed test
- `ADM-08` Placement cancel / no-show cannot target completed test
- `ADM-09` Placement results complete test and move CRM statuses
- `ADM-10` Retake placement can auto-open new registration from recommendation
- `ADM-11` Manual retake placement scheduling
- `ADM-12` Convert lead to enrolled
- `ADM-13` Registration creation validates academic + commercial consistency
- `ADM-14` Registration-to-class assignment enforces real class rules
- `ADM-15` Suggested classes are filtered by real schedule + capacity
- `ADM-16` Transfer class drops old enrollment and creates new one
- `ADM-17` Create enrollment directly from class
- `ADM-18` Update enrollment re-validates schedule and tuition plan
- `ADM-19` Pause / reactivate / drop enrollment
- `ADM-20` Assign tuition plan to enrollment
- `ADM-21` Cancel registration cascades to active enrollments
- `PCS-01` Program creation
- `PCS-02` Program-branch assignment
- `PCS-03` Program leave policy configuration
- `PCS-04` Tuition plan creation / update auto-computes unit price
- `PCS-05` Tuition plan delete blocked by active or paused enrollments
- `PCS-06` Class creation validates branch-program-teacher consistency
- `PCS-07` Class update keeps basic consistency but weak conflict protection
- `PCS-08` Class status transition
- `PCS-09` Teacher assignment to class
- `PCS-10` Supplementary classes can have schedule segments
- `PCS-11` Generate sessions from class pattern
- `PCS-12` Single session create / update allows warning-only conflict checks
- `PCS-13` Batch session update hard-blocks conflicts
- `PCS-14` Session completion / cancellation
- `PCS-15` Enrollment session-selection pattern must be subset of class schedule
- `PCS-16` Student session assignment sync
- `PCS-17` Student schedule conflict check uses real session slots with minimum gap
- `ATT-01` Attendance mark only for real participants
- `ATT-02` Approved leave locks attendance for non-admin
- `ATT-03` Absence type resolution uses leave notice hours
- `ATT-04` 24h leave creates makeup credit
- `ATT-05` Attendance update window
- `ATT-06` Approved leave can retroactively change consumption
- `ATT-07` Leave request requires real enrollment or assignment
- `ATT-08` Leave duplicate and monthly limit
- `ATT-09` 24h rule auto-approval
- `ATT-10` Approved leave can auto-schedule weekend makeup
- `ATT-11` Leave approval / rejection
- `ATT-12` Makeup credit usage
- `ATT-13` Pause request creation
- `ATT-14` Pause approval reserves sessions and reconciles future leave/makeup
- `ATT-15` Pause rejection / cancellation
- `ATT-16` Pause outcome management
- `ATT-17` Reassign equivalent class after pause
- `ATT-18` Continue-same-class auto-reactivation job
- `HW-01` Homework assignment creation
- `HW-02` Homework assignment update is locked after submissions start
- `HW-03` Generic submission ownership and attempt limit
- `HW-04` Generic submission payload validation by submission type
- `HW-05` Missing auto-graded homework cannot be resubmitted
- `HW-06` On-time first submission rewards stars and mission tracking
- `HW-07` Quiz submission is auto-graded immediately
- `HW-08` Manual grading
- `HW-09` Manual late/missing transition
- `HW-10` Overdue homework auto-mark job
- `HW-11` AI grading can persist as final grade
- `HW-12` AI hint / recommendation / speaking-practice access control
- `REP-01` Session report creation only after session end and by assigned teacher
- `REP-02` Session report editing window
- `REP-03` Session report submit / review / approve / reject / publish
- `REP-04` Report request creation validates teacher-target relationship
- `REP-05` Report request completion / cancellation
- `REP-06` Report-request workflow auto-links review status
- `REP-07` Monthly report AI draft generation is teacher/class-scoped
- `REP-08` Monthly report data aggregation input set
- `REP-09` AI monthly report prompt uses recent published history
- `REP-10` AI failure does not destroy report row
- `REP-11` Monthly report draft editing
- `REP-12` Monthly report review lifecycle
- `REP-13` Monthly report publish
- `REP-14` Monthly report comments have no workflow gate
- `GAM-01` Mission reward rule configuration
- `GAM-02` Mission creation validates scope and target ownership
- `GAM-03` Mission update keeps same scope + reward resolution constraints
- `GAM-04` Mission delete blocked if already in use
- `GAM-05` Mission reward rule resolution is divisor-based
- `GAM-06` Mission progress auto-reward
- `GAM-07` Attendance mission progress is driven by real attendance rows
- `GAM-08` Homework mission progress is driven by real homework submissions
- `GAM-09` Daily check-in streak
- `GAM-10` Missed daily check-in streak reset job
- `GAM-11` Manual stars and XP adjustments
- `GAM-12` System gamification hooks use academic data
- `GAM-13` Reward redemption request deducts stars immediately
- `GAM-14` Reward redemption status transitions
- `GAM-15` Batch delivery and auto-confirm

---

## Status Transition Map

### Auth / Profile
- `Profile approval`: `IsApproved=false -> true` by approve flow.
- `Profile activation`: create profile starts inactive; activation is not coupled to approval in current handler set.
- `Profile reactivation`: target profile lookup triggers bulk `all approved profiles of same user -> IsDeleted=false, IsActive=true`.
- `Selected student context`: current auth flow does not persist a server-side selected profile; alternate flow can issue token with `StudentId`.

### Lead / Placement / Registration / Enrollment
- `LeadStatus`: `New -> Contacted -> BookedTest -> TestDone -> Enrolled`; `* -> Lost` via generic update; `Enrolled -> non-Enrolled` blocked.
- `LeadChildStatus`: `New -> BookedTest -> TestDone -> Enrolled`; `* -> Lost` via surrounding CRM flows.
- `PlacementTestStatus`: `Scheduled -> Completed`, `Scheduled -> NoShow`, `Scheduled -> Cancelled`.
- `RegistrationStatus`:
  - create: `New`
  - assign wait: `WaitingForClass`
  - assign class with `Retake` or legacy `Makeup`: `ClassAssigned`
  - assign class with `Immediate`: `Studying`
  - retake result can force old registration `-> Completed`
  - cancel: `-> Cancelled`
  - note: `Paused` enum exists but no writer found in current mutation flow
- `EnrollmentStatus`: `Active -> Paused`, `Paused -> Active`, `Active/Paused -> Dropped`; dropped cannot reactivate.

### Program / Class / Session
- `ClassStatus`:
  - create: `Planned`
  - explicit status handler forbids only `Closed -> Planned`
  - capacity helper auto-syncs `* -> Full` when seat limit reached
  - capacity helper auto-syncs `Full -> Recruiting/Active` when seat frees up
- `SessionStatus`: `Scheduled -> Completed` or `Scheduled -> Cancelled`.
- `StudentSessionAssignmentStatus`: active sync creates/keeps `Assigned`; pause/cancel/pattern mismatch makes `Cancelled`; reactivation restores `Assigned`.

### Attendance / Leave / Makeup / Pause
- `AttendanceStatus`: mark/update to `Present`, `Absent`, `NotMarked`; approved leave causes consumption logic to behave like `Makeup` for regular participants.
- `AbsenceType`: `WithNotice24H`, `Under24H`, `NoNotice`.
- `LeaveRequestStatus`: create gives `Approved` when `noticeHours >= 24`, otherwise `Pending`; manual review can move to `Approved` or `Rejected`; pause reconciliation can move pending/approved to `Cancelled`.
- `MakeupCreditStatus`: `Available -> Used`; expiry path exists by status enum/job family; some flows delete credits outright instead of transitioning status.
- `MakeupAllocationStatus`: create `Pending`; enum has `Confirmed`, but main flows observed here mostly create `Pending` and cancel old allocations with `Cancelled`.
- `PauseEnrollmentRequestStatus`: `Pending -> Approved`, `Pending -> Rejected`, `Pending -> Cancelled`.
- `PauseEnrollmentOutcome`: `ContinueSameClass`, `ReassignEquivalentClass`, `ContinueWithTutoring`.

### Homework / Submission / Grading
- `HomeworkStatus`:
  - create assignment auto-students: `Assigned`
  - generic submit on time: `Assigned -> Submitted`
  - generic submit late: `Assigned/Missing -> Late`
  - quiz submit: `Assigned -> Graded`
  - manual grading: `Submitted/Graded -> Graded`
  - manual late/missing: `Assigned -> Late`, `Assigned -> Missing`, `Late -> Missing`
  - overdue job: `Assigned -> Missing`

### Report / AI / Approve / Publish
- `ReportStatus` for session report: `Draft -> Review -> Approved/Rejected -> Published`; rejected edit resets `Rejected -> Draft`.
- `ReportStatus` for monthly report: `Draft -> Review -> Approved/Rejected -> Published`; rejected edit resets `Rejected -> Draft`.
- `ReportRequestStatus`: `Requested -> Submitted -> Approved/Rejected`; current cancel handler allows `Requested`, `Submitted`, and even `Rejected` to move to `Cancelled`; `InProgress` enum exists in readers/workflow checks but no writer was found.

### Gamification / Mission / Reward
- `MissionProgressStatus`: `Assigned -> InProgress -> Completed`; missed streak reset can push back to `Assigned`; `Expired` enum exists but no writer found.
- `RedemptionStatus`: `Requested -> Approved -> Delivered -> Received`; `Requested/Approved -> Cancelled`.
- `AttendanceStreak`: today row is created once per day; streak resets to `1` if yesterday row is absent.

---

## Top 20 Rules To Defend In Viva

1. `ADM-14` Registration-to-class assignment is not CRUD; it validates branch, program, seat, schedule pattern, first study date and conflict before creating real enrollment.
2. `PCS-17` Schedule conflict check is session-based and uses a hard `15-minute` minimum gap, not just same-day same-class checking.
3. `PCS-16` Student session assignments are explicit rows, so attendance/leave/makeup/pause all run on real session membership.
4. `ATT-07` Leave request only works if the student is truly assigned or enrolled in the target class/session.
5. `ATT-09` The `24h` rule is coded from `(session.PlannedDatetime - now).TotalHours`, not left to UI.
6. `ATT-10` Approved 24h leave creates makeup credit and can auto-book a valid weekend makeup session.
7. `ATT-12` Makeup booking is heavily constrained: ownership, expiry, weekend, eligible future week, capacity, duplicate session, and `2-hour` conflict gap.
8. `ATT-14` Pause approval is not just a status flip; it reconciles future leave, makeup allocations, credits, attendance, and reserved sessions.
9. `ATT-18` Continue-same-class pause is resumed by background job with fresh capacity/conflict checks, not blindly reopened.
10. `HW-01` Homework may be class-level or session-linked, but BE still assigns it to all active class students, including absentees.
11. `HW-07` Multiple-choice homework is auto-graded immediately in BE and writes final score/status.
12. `HW-10` Overdue homework is auto-marked `Missing` by Quartz and re-fed into mission progress.
13. `REP-01` Session report can only be created after the session ends and only by the teacher really assigned to that session.
14. `REP-07` Monthly report AI draft generation is class-scoped and teacher-scoped; not any teacher can generate for any class.
15. `REP-09` AI report input is grounded in real attendance, homework, tests, missions, lesson topics and previous published reports.
16. `GAM-12` Gamification is attached to real academic mutations, not a detached toy module.
17. `GAM-02` Mission creation enforces teacher target ownership, so teachers cannot create missions for foreign classes/students.
18. `GAM-13` Reward redemption deducts stars immediately and stores `ItemName` snapshot at redemption time.
19. `AUTH-11` Current BE has a real auth-role inconsistency: some controller roles are not issuable by the JWT provider.
20. `AUTH-07` Password reset token expiry is generated but not enforced in the reset handler, which is an honest limitation to mention if asked about BE hardening.

---

## Short conclusion
- Backend rule density is highest in these areas:
  - registration-to-class assignment
  - session assignment + conflict handling
  - attendance / leave / makeup / pause
  - session report / monthly report workflow
  - mission progress + reward redemption
- Backend is weaker or inconsistent in these areas:
  - auth role-string consistency
  - password reset expiry enforcement
  - duplicate profile-select flows
  - single-session conflict checking vs batch conflict checking
  - some dormant enum statuses not yet fully wired
