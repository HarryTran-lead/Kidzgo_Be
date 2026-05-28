# Phase 3 Decision (Implementation Baseline)

Scope: chốt quyết định implement cho `phase3.md` theo codebase hiện tại.

## 0) Role Mapping Rule (bắt buộc)

- Dùng role kỹ thuật hiện có trong hệ thống: `Admin`, `ManagementStaff`, `AccountantStaff`, `Teacher`, `Parent`.
- Mọi role nghiệp vụ ghi trong docs nhưng không tồn tại (`cs`, `academic_manager`, ...) được hiểu là `ManagementStaff`.
- Không bổ sung `AssignedRole` mới ở DB/API trong phase này.

## 1) `expected_completion_percent` cho rule `learning_delay`

- Nguồn `actual`: `StudentProgress.CompletionPercent`.
- Nguồn `expected`: lấy theo tiến độ chuẩn lớp/module tại `period.endDate`:
  - `expected = min(100, expectedCompletedSections / totalSectionsOfModule * 100)`.
- Trigger cảnh báo:
  - `learning_delay` khi `actual < expected - 10` (buffer 10% để giảm false positive).

## 2) Idempotency cho `generate`

- Mặc định theo immutable snapshot: gọi generate lại cùng period thì tạo `ReportRun` + `StudentReport` mới.
- Hỗ trợ `idempotencyKey` để chống double-click/retry mạng:
  - cùng key + cùng payload scope thì trả về run đã tạo trước đó.
- Không upsert report `completed` cũ.

## 3) Authorization chuẩn API

- `Teacher`: xem report/risk/recommendation trong lớp mình.
- `Parent`: chỉ xem parent view của con mình.
- `ManagementStaff`: xử lý toàn bộ nghiệp vụ tương ứng các role docs bị thiếu (`cs`, `academic_manager`...).
- `Admin`: toàn quyền (cross-branch + re-run + share quản trị).
- `AccountantStaff`: chỉ tham gia các API có liên quan tài chính nếu cần.

## 4) Deduplicate `RiskAlert`

- Dedup key: `(scopeKey, riskType, periodId)`.
- `scopeKey`:
  - student-level: `studentId`
  - class-level: `classId`
  - branch-level: `branchId`
- Nếu trùng key:
  - tăng severity thì update severity + reason/source mới nhất,
  - severity giảm thì giữ mức cao hơn.
- Chỉ tạo alert mới khi khác `periodId` hoặc alert cũ đã `resolved/ignored`.

## 5) Priority khi nhiều rule cùng trúng

- Dùng `riskScore` để xếp hạng:
  - `academic_fail=100`
  - `low_attendance=90`
  - `weak_communication=70`
  - `package_expiring=60`
  - `class_curriculum_delay=55`
  - `high_review_ratio=50`
- Severity theo score:
  - `>=90`: high
  - `60-89`: medium
  - `<60`: low
- Override:
  - assessment `FAIL` => tối thiểu `high`
  - attendance `<50%` => tối thiểu `high`
- Rule score không hard-code: Admin có thể cấu hình `Score` theo từng `riskType` trong `RiskRuleConfig`.

## 6) Status + SLA

- `StudentReport.Status`: `pending -> processing -> completed | failed | superseded`
- `Recommendation.Status`: `pending -> accepted -> done | rejected`
- SLA:
  - `high`: 24h
  - `medium`: 72h
  - `low`: 7 ngày

## 7) Share channel contract + viewed callback

- `POST /api/reports/{id}/share` lưu `ReportShareLog` với:
  - `channel`, `recipientName`, `recipientContact`, `status`, `providerMessageId`, `sentAt`, `viewedAt`, `errorMessage`.
- `app`: set `viewedAt` khi phụ huynh mở report trong app.
- `email/zalo/sms`: update trạng thái qua callback endpoint theo channel, idempotent theo `providerMessageId`.

## 8) Pagination/filter/sort chuẩn

- Query chung:
  - `page`, `pageSize`, `sortBy`, `sortDir`, `q`, `from`, `to`.
- Report filters:
  - `studentId`, `classId`, `branchId`, `periodId`, `reportType`, `status`.
- Risk filters:
  - `riskType`, `severity`, `status`.
- Recommendation filters:
  - `status`, `priority`, `dueFrom`, `dueTo`, `overdue`.
- Response chung:
  - `items`, `total`, `page`, `pageSize`, `hasNext`.

## 9) Non-Regression Guard

- Phase 3 chỉ READ + SNAPSHOT + INSIGHT + REPORT + ACTION.
- Không update trực tiếp:
  - `TicketLedger`
  - `Attendance`
  - `StudentProgress`
  - `Assessment`
  - `PromotionDecision`