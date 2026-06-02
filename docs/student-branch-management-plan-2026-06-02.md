# Student Branch Management

Updated: 2026-06-02
Status: implemented trong backend va da co migration.

## Scope

Feature nhom nay da duoc model hoa bang entity rieng:

- `StudentHomeBranch`
- `StudentActiveBranch`
- `BranchTransfer`
- `CrossBranchEnrollment`

Entity/backend additions:

- `StudentBranchState`
- `StudentBranchTransfer`
- validation helper cho cross-branch runtime

## APIs

### Student branch state

- `GET /api/students/{id}/branches`
- `PUT /api/students/{id}/home-branch`
- `PUT /api/students/{id}/active-branch`

Response:

- `studentId`
- `homeBranchId`
- `homeBranchName`
- `activeBranchId`
- `activeBranchName`
- `lastTransferredAt`
- `crossBranchEnrollmentAllowed`
- `transfers[]`

### Branch transfer workflow

- `POST /api/students/{id}/branch-transfer`

Request:

```json
{
  "fromBranchId": "uuid",
  "toBranchId": "uuid",
  "effectiveDate": "2026-06-15",
  "reason": "Parent moved area",
  "keepCurrentClass": false,
  "allowCrossBranchEnrollment": false
}
```

Validation/runtime rules:

- Student phai ton tai va la `ProfileType.Student`
- `fromBranchId` phai match active branch hien tai
- Neu hoc sinh dang co enrollment operational o branch khac branch dich:
  - `keepCurrentClass = false` -> backend block
  - `keepCurrentClass = true` va `allowCrossBranchEnrollment = false` -> backend block
- Neu transfer hop le, backend update ca `homeBranchId` va `activeBranchId`, dong thoi luu audit history

### Cross-branch enrollment

Da bo sung validation va field runtime:

- `allowCrossBranchEnrollment`
- `studentActiveBranchId`
- `studentHomeBranchId`
- `classBranchId`
- `isCrossBranchEnrollment`

Applied on:

- `POST /api/enrollments`
- `POST /api/registrations`
- `POST /api/registrations/import-active`
- `POST /api/registrations/assign-class`
- `POST /api/registrations/transfer-class`

Behavior:

- Neu hoc sinh chua co `StudentBranchState`, backend tu seed state ban dau theo branch dang thao tac
- Neu class/registration o branch khac `activeBranchId` ma khong cho phep cross-branch, backend tra loi validation
- Response enrollment/registration detail da co them thong tin `studentHomeBranchId`, `studentActiveBranchId`, `isCrossBranch...`

## Migration

- `20260602100946_AddStudentBranchManagement`
