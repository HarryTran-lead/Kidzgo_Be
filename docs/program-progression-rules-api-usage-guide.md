# Program Progression Rules API - Hướng Dẫn Sử Dụng Chi Tiết

## Tổng Quan

API Program Progression Rules được sử dụng để tạo quy tắc thăng tiến từ program này sang program khác. Có 3 phương pháp (method):
- **PassFail**: Đánh giá theo Pass/Fail
- **Shields**: Đánh giá theo số shields đạt được
- **CambridgeScale**: Đánh giá theo thang điểm Cambridge

---

## Method 1: PassFail

### Đặc Điểm
- Phương pháp đơn giản nhất
- Chỉ cần đánh giá học sinh Pass hay Fail trong class
- Không cần điểm số chi tiết

### Required Fields
- ✅ `sourceProgramId` (Guid) - **Required**
- ✅ `targetProgramId` (Guid) - **Required** (program đích khi Pass)
- ✅ `method` = `"PassFail"`
- ❌ `minimumShieldCount` = **null** hoặc không gửi
- ❌ `minimumSkillShieldCount` = **null** hoặc không gửi
- ❌ `minimumOverallScore` = **null** hoặc không gửi
- ❌ `shieldMappings` = **[]** hoặc không gửi
- ❌ `classificationBands` = **[]** hoặc không gửi

### Optional Fields
- `carryOverRemainingSessions` (boolean, default: true)
- `stopCurrentEnrollmentOnApproval` (boolean, default: true)
- `isActive` (boolean, default: true)
- `notes` (string)

### Example Request

```json
POST /api/program-progressions/rules
Content-Type: application/json

{
  "sourceProgramId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "targetProgramId": "7fb96f74-6828-5673-c4gd-3d074g77bgb7",
  "method": "PassFail",
  "carryOverRemainingSessions": true,
  "stopCurrentEnrollmentOnApproval": true,
  "isActive": true,
  "notes": "Progression from Level 1 to Level 2 based on teacher assessment"
}
```

### Validation Rules
- ✅ `sourceProgramId` phải tồn tại
- ✅ `targetProgramId` phải tồn tại
- ✅ Không cần các fields liên quan đến shields hay scores

---

## Method 2: Shields

### Đặc Điểm
- Đánh giá dựa trên số shields học sinh đạt được
- Mỗi skill (Listening, ReadingWriting) có mapping từ điểm số sang số shields
- Shield count: 0-5 shields per skill
- Total shields = tổng shields của tất cả skills

### Required Fields
- ✅ `sourceProgramId` (Guid) - **Required**
- ✅ `targetProgramId` (Guid) - **Required**
- ✅ `method` = `"Shields"`
- ✅ `minimumShieldCount` (int, 0-15) - **Required** - Tổng số shields tối thiểu
- ❌ `minimumSkillShieldCount` (int, 0-5) - **Optional** - Số shields tối thiểu mỗi skill
- ✅ `shieldMappings` (array) - **Required** - Mapping từ điểm số sang shields
- ❌ `minimumOverallScore` = **null** hoặc không gửi - **KHÔNG SỬ DỤNG**
- ❌ `classificationBands` = **[]** hoặc không gửi

### Shield Mappings Structure

`shieldMappings` là array chứa các object với cấu trúc:
```json
{
  "skill": "Listening" | "Speaking" | "ReadingWriting" | "Reading" | "Writing",
  "minScore": decimal (0-100),
  "maxScore": decimal (0-100) hoặc null,
  "shieldCount": int (0-5)
}
```

**Lưu ý quan trọng:**
1. **BẮT BUỘC** phải có mappings cho 2 skills: `Listening` và `ReadingWriting`
2. Các ranges không được overlap (chồng lấn)
3. Chỉ range cuối cùng mới được để `maxScore = null` (open-ended)
4. `shieldCount` phải từ 0-5

### Example Request

```json
POST /api/program-progressions/rules
Content-Type: application/json

{
  "sourceProgramId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "targetProgramId": "7fb96f74-6828-5673-c4gd-3d074g77bgb7",
  "method": "Shields",
  "minimumShieldCount": 8,
  "minimumSkillShieldCount": 3,
  "minimumOverallScore": null,
  "carryOverRemainingSessions": true,
  "stopCurrentEnrollmentOnApproval": true,
  "isActive": true,
  "notes": "Need 8+ shields total and 3+ shields per skill",
  "shieldMappings": [
    {
      "skill": "Listening",
      "minScore": 0,
      "maxScore": 59.99,
      "shieldCount": 0
    },
    {
      "skill": "Listening",
      "minScore": 60,
      "maxScore": 69.99,
      "shieldCount": 1
    },
    {
      "skill": "Listening",
      "minScore": 70,
      "maxScore": 79.99,
      "shieldCount": 2
    },
    {
      "skill": "Listening",
      "minScore": 80,
      "maxScore": 89.99,
      "shieldCount": 3
    },
    {
      "skill": "Listening",
      "minScore": 90,
      "maxScore": 95.99,
      "shieldCount": 4
    },
    {
      "skill": "Listening",
      "minScore": 96,
      "maxScore": null,
      "shieldCount": 5
    },
    {
      "skill": "ReadingWriting",
      "minScore": 0,
      "maxScore": 59.99,
      "shieldCount": 0
    },
    {
      "skill": "ReadingWriting",
      "minScore": 60,
      "maxScore": 69.99,
      "shieldCount": 1
    },
    {
      "skill": "ReadingWriting",
      "minScore": 70,
      "maxScore": 79.99,
      "shieldCount": 2
    },
    {
      "skill": "ReadingWriting",
      "minScore": 80,
      "maxScore": 89.99,
      "shieldCount": 3
    },
    {
      "skill": "ReadingWriting",
      "minScore": 90,
      "maxScore": 95.99,
      "shieldCount": 4
    },
    {
      "skill": "ReadingWriting",
      "minScore": 96,
      "maxScore": null,
      "shieldCount": 5
    }
  ],
  "classificationBands": []
}
```

### Validation Rules
- ✅ `minimumShieldCount` là **REQUIRED**, phải từ 0-15
- ✅ `minimumSkillShieldCount` là optional, nếu có thì phải từ 0-5
- ✅ `shieldMappings` **PHẢI CÓ** cho cả `Listening` và `ReadingWriting`
- ✅ Các ranges không được overlap
- ✅ Chỉ range cuối mới được `maxScore = null`
- ❌ `minimumOverallScore` phải **null** hoặc không gửi

### Ví Dụ Đơn Giản Hơn (3 shields per skill)

```json
{
  "sourceProgramId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "targetProgramId": "7fb96f74-6828-5673-c4gd-3d074g77bgb7",
  "method": "Shields",
  "minimumShieldCount": 5,
  "minimumSkillShieldCount": 2,
  "minimumOverallScore": null,
  "isActive": true,
  "shieldMappings": [
    {
      "skill": "Listening",
      "minScore": 0,
      "maxScore": 69.99,
      "shieldCount": 1
    },
    {
      "skill": "Listening",
      "minScore": 70,
      "maxScore": 84.99,
      "shieldCount": 2
    },
    {
      "skill": "Listening",
      "minScore": 85,
      "maxScore": null,
      "shieldCount": 3
    },
    {
      "skill": "ReadingWriting",
      "minScore": 0,
      "maxScore": 69.99,
      "shieldCount": 1
    },
    {
      "skill": "ReadingWriting",
      "minScore": 70,
      "maxScore": 84.99,
      "shieldCount": 2
    },
    {
      "skill": "ReadingWriting",
      "minScore": 85,
      "maxScore": null,
      "shieldCount": 3
    }
  ]
}
```

---

## Method 3: CambridgeScale

### Đặc Điểm
- Đánh giá dựa trên thang điểm Cambridge
- Assessment có điểm cho: Listening, Speaking, Reading, Writing (hoặc ReadingWriting)
- Tính overall score = trung bình các skills
- Có thể dùng classification bands để xác định target program dựa trên điểm số

### Cách 1: Với Target Program Cố Định

#### Required Fields
- ✅ `sourceProgramId` (Guid) - **Required**
- ✅ `targetProgramId` (Guid) - **Required**
- ✅ `method` = `"CambridgeScale"`
- ✅ `minimumOverallScore` (decimal, > 0) - **Required**
- ❌ `minimumShieldCount` = **null** hoặc không gửi
- ❌ `minimumSkillShieldCount` = **null** hoặc không gửi
- ❌ `shieldMappings` = **[]** hoặc không gửi
- ❌ `classificationBands` = **[]** hoặc không gửi (optional)

#### Example Request

```json
POST /api/program-progressions/rules
Content-Type: application/json

{
  "sourceProgramId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "targetProgramId": "7fb96f74-6828-5673-c4gd-3d074g77bgb7",
  "method": "CambridgeScale",
  "minimumOverallScore": 70,
  "carryOverRemainingSessions": true,
  "stopCurrentEnrollmentOnApproval": true,
  "isActive": true,
  "notes": "Need average score >= 70 to progress to Level 2"
}
```

**Cách hoạt động:**
- Học sinh phải đạt overall score >= 70 mới eligible để lên Level 2
- Tất cả học sinh eligible sẽ lên cùng một target program

---

### Cách 2: Với Classification Bands (Multiple Target Programs)

Dùng khi muốn phân loại học sinh vào các target programs khác nhau dựa trên điểm số.

#### Required Fields
- ✅ `sourceProgramId` (Guid) - **Required**
- ❌ `targetProgramId` = **null** - Để null vì dùng bands
- ✅ `method` = `"CambridgeScale"`
- ✅ `minimumOverallScore` (decimal, > 0) - **Required**
- ✅ `classificationBands` (array) - **Required** - Danh sách phân loại
- ❌ `minimumShieldCount` = **null**
- ❌ `minimumSkillShieldCount` = **null**
- ❌ `shieldMappings` = **[]**

#### Classification Bands Structure

```json
{
  "minScore": decimal (0-100),
  "maxScore": decimal (0-100) hoặc null,
  "label": string (required),
  "cefrLevel": string (optional),
  "description": string (optional)
}
```

**Lưu ý:**
- `label` là **required**, không được empty
- Các bands không được overlap
- Chỉ band cuối cùng được `maxScore = null`

#### Example Request

```json
POST /api/program-progressions/rules
Content-Type: application/json

{
  "sourceProgramId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "targetProgramId": null,
  "method": "CambridgeScale",
  "minimumOverallScore": 60,
  "minimumShieldCount": null,
  "minimumSkillShieldCount": null,
  "carryOverRemainingSessions": true,
  "stopCurrentEnrollmentOnApproval": true,
  "isActive": true,
  "notes": "Multi-level progression based on Cambridge scores",
  "shieldMappings": [],
  "classificationBands": [
    {
      "minScore": 60,
      "maxScore": 74.99,
      "label": "Pass",
      "cefrLevel": "A2",
      "description": "Progress to Level 2A"
    },
    {
      "minScore": 75,
      "maxScore": 89.99,
      "label": "Merit",
      "cefrLevel": "A2+",
      "description": "Progress to Level 2B"
    },
    {
      "minScore": 90,
      "maxScore": null,
      "label": "Distinction",
      "cefrLevel": "B1",
      "description": "Progress to Level 3"
    }
  ]
}
```

**Cách hoạt động:**
- Học sinh phải đạt >= 60 (minimumOverallScore) mới eligible
- 60-74.99: vào Level 2A (Pass)
- 75-89.99: vào Level 2B (Merit)
- 90+: vào Level 3 (Distinction)

**LƯU Ý QUAN TRỌNG:**
- Trong implementation hiện tại, `classificationBands` chỉ chứa label/cefrLevel/description
- **KHÔNG CÓ** field `targetProgramId` trong band
- Target program được xác định trong code logic, không phải trong API request
- Nếu cần mapping band -> target program, cần update code backend

---

### Validation Rules cho CambridgeScale

- ✅ `minimumOverallScore` là **REQUIRED**, phải > 0
- ✅ Nếu không có `targetProgramId`, phải có `classificationBands`
- ✅ Mỗi band phải có `label` (không được empty)
- ✅ Các bands không được overlap
- ✅ Chỉ band cuối mới được `maxScore = null`
- ❌ `minimumShieldCount` và `minimumSkillShieldCount` phải **null**
- ❌ `shieldMappings` phải **[]** hoặc không gửi

---

## So Sánh 3 Methods

| Feature | PassFail | Shields | CambridgeScale |
|---------|----------|---------|----------------|
| **Complexity** | ⭐ Simple | ⭐⭐⭐ Complex | ⭐⭐ Medium |
| **minimumShieldCount** | ❌ null | ✅ Required (0-15) | ❌ null |
| **minimumSkillShieldCount** | ❌ null | ⭕ Optional (0-5) | ❌ null |
| **minimumOverallScore** | ❌ null | ❌ null | ✅ Required (> 0) |
| **shieldMappings** | ❌ [] | ✅ Required | ❌ [] |
| **classificationBands** | ❌ [] | ❌ [] | ⭕ Optional |
| **targetProgramId** | ✅ Required | ✅ Required | ⭕ Optional (null nếu dùng bands) |
| **Assessment Fields** | passedInClass (bool) | Không cần điểm | Listening, Speaking, Reading/Writing scores |

---

## Câu Hỏi Thường Gặp (FAQ)

### Q1: Method Shields, `minimumOverallScore` để null hay 0?

**Trả lời:** **Phải để `null` hoặc không gửi field này.**

Nếu gửi `minimumOverallScore = 0`, validation vẫn pass nhưng **KHÔNG NÊN** vì:
- Field này không được dùng trong Shields method
- Làm data không clean, gây nhầm lẫn
- Best practice: chỉ gửi các fields cần thiết

```json
// ✅ ĐÚNG
{
  "method": "Shields",
  "minimumShieldCount": 8,
  "minimumOverallScore": null
}

// ✅ ĐÚNG HƠN - không gửi luôn
{
  "method": "Shields",
  "minimumShieldCount": 8
}

// ⚠️ KHÔNG NÊN
{
  "method": "Shields",
  "minimumShieldCount": 8,
  "minimumOverallScore": 0
}
```

---

### Q2: Method PassFail có cần `shieldMappings` và `classificationBands` không?

**Trả lời:** **KHÔNG cần.**

Gửi `[]` hoặc không gửi đều được:
```json
// ✅ ĐÚNG
{
  "method": "PassFail",
  "shieldMappings": [],
  "classificationBands": []
}

// ✅ ĐÚNG HƠN - không gửi luôn
{
  "method": "PassFail"
}
```

---

### Q3: Shields method bắt buộc phải có bao nhiêu skills?

**Trả lời:** **Bắt buộc phải có 2 skills: `Listening` và `ReadingWriting`**

Validation sẽ lỗi nếu thiếu một trong hai:
```json
// ❌ LỖI - thiếu ReadingWriting
{
  "method": "Shields",
  "shieldMappings": [
    { "skill": "Listening", "minScore": 0, "maxScore": null, "shieldCount": 3 }
  ]
}

// ✅ ĐÚNG
{
  "method": "Shields",
  "shieldMappings": [
    { "skill": "Listening", ... },
    { "skill": "ReadingWriting", ... }
  ]
}
```

---

### Q4: CambridgeScale có thể dùng cả `targetProgramId` và `classificationBands` cùng lúc không?

**Trả lời:** **CÓ thể**, nhưng không recommend.

- Nếu có `targetProgramId`: hệ thống dùng targetProgramId cố định
- `classificationBands` lúc này chỉ mang tính descriptive (mô tả phân loại)

Best practice:
- Nếu muốn 1 target program cố định → chỉ dùng `targetProgramId`, không cần bands
- Nếu muốn nhiều target programs → để `targetProgramId = null`, dùng bands

---

### Q5: Shield count có thể > 5 không?

**Trả lời:** **KHÔNG.**

Mỗi skill chỉ có max 5 shields. Validation sẽ lỗi nếu `shieldCount > 5`.

Total shields (minimumShieldCount) có thể lên đến 15 (vì có nhiều skills).

---

## Error Responses

### PassFail Errors

```json
// Missing targetProgramId
{
  "errors": {
    "TargetProgramId": ["Target program is required for PassFail method"]
  }
}
```

---

### Shields Errors

```json
// Missing minimumShieldCount
{
  "type": "ProgramProgression.MinimumShieldCountRequired",
  "title": "MinimumShieldCount is required for shield-based progression and must be between 0 and 15."
}

// Missing shield mappings for required skills
{
  "type": "ProgramProgression.ShieldMappingsMissing",
  "title": "Shield mappings for skill 'Listening' are required."
}

// Overlapping ranges
{
  "type": "ProgramProgression.ShieldMappingsOverlap",
  "title": "Shield mappings for skill 'Listening' contain overlapping ranges."
}

// Invalid shield count
{
  "type": "ProgramProgression.InvalidShieldCount",
  "title": "Shield count for skill 'Listening' must be between 0 and 5."
}
```

---

### CambridgeScale Errors

```json
// Missing minimumOverallScore
{
  "type": "ProgramProgression.MinimumOverallScoreRequired",
  "title": "MinimumOverallScore is required for Cambridge-scale progression."
}

// Missing classification band label
{
  "type": "ProgramProgression.InvalidClassificationBand",
  "title": "Classification band label is required."
}

// Overlapping bands
{
  "type": "ProgramProgression.ClassificationBandsOverlap",
  "title": "Classification bands contain overlapping ranges."
}
```

---

## Testing Tips

### Test Data cho Shields Method

```json
{
  "sourceProgramId": "existing-program-id",
  "targetProgramId": "target-program-id",
  "method": "Shields",
  "minimumShieldCount": 6,
  "minimumSkillShieldCount": 2,
  "shieldMappings": [
    // Listening: 3 levels
    { "skill": "Listening", "minScore": 0, "maxScore": 69, "shieldCount": 1 },
    { "skill": "Listening", "minScore": 70, "maxScore": 84, "shieldCount": 2 },
    { "skill": "Listening", "minScore": 85, "maxScore": null, "shieldCount": 3 },
    // ReadingWriting: 3 levels
    { "skill": "ReadingWriting", "minScore": 0, "maxScore": 69, "shieldCount": 1 },
    { "skill": "ReadingWriting", "minScore": 70, "maxScore": 84, "shieldCount": 2 },
    { "skill": "ReadingWriting", "minScore": 85, "maxScore": null, "shieldCount": 3 }
  ]
}
```

### Test Data cho CambridgeScale với Bands

```json
{
  "sourceProgramId": "existing-program-id",
  "targetProgramId": null,
  "method": "CambridgeScale",
  "minimumOverallScore": 50,
  "classificationBands": [
    { "minScore": 50, "maxScore": 69, "label": "Basic Pass", "cefrLevel": "A2" },
    { "minScore": 70, "maxScore": 84, "label": "Good Pass", "cefrLevel": "A2+" },
    { "minScore": 85, "maxScore": null, "label": "Excellent", "cefrLevel": "B1" }
  ]
}
```

---

## Changelog

| Date | Version | Changes |
|------|---------|---------|
| 2026-05-06 | 1.0 | Initial detailed usage guide |

---

**End of Document**
