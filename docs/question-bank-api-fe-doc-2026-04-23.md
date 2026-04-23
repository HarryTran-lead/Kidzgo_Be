# Question Bank API FE Doc - 2026-04-23

Tai lieu nay mo ta cac API trong `QuestionBankController.cs` de FE tich hop.

## Base path

- Prefix: `/api/question-bank`
- Auth: `Bearer token`
- Tat ca endpoint hien tai deu yeu cau role:
  - `Teacher`
  - `ManagementStaff`
  - `Admin`

## Role va pham vi

| Role | Duoc truy cap | Ghi chu |
| --- | --- | --- |
| Admin | Full question bank APIs | create, list, update, soft delete, import, AI generate |
| ManagementStaff | Full question bank APIs | create, list, update, soft delete, import, AI generate |
| Teacher | Full question bank APIs | create, list, update, soft delete, import, AI generate |
| Parent | Khong duoc truy cap | `403` |
| Student | Khong duoc truy cap | `403` |
| Anonymous | Khong duoc truy cap | `401` |

## Common success format

Backend dang wrap thanh cong theo format:

```json
{
  "isSuccess": true,
  "data": {}
}
```

Luu y:

- JSON thuc te khi ra HTTP se o `camelCase`.
- `DELETE` thanh cong se tra:

```json
{
  "isSuccess": true,
  "data": null
}
```

## Common error formats

### 1. ProblemDetails

Phan lon loi business tra theo `ProblemDetails`:

```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.4",
  "title": "Homework.ProgramNotFound",
  "status": 404,
  "detail": "Program with Id = '3fa85f64-5717-4562-b3fc-2c963f66afa6' was not found"
}
```

Validation-style errors business thuong la `400`:

```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.1",
  "title": "Homework.InvalidPoints",
  "status": 400,
  "detail": "Question 1 points must be greater than 0"
}
```

### 2. Plain string bad request

Mot so loi enum parse dang duoc tra truc tiep tu controller:

```json
"Invalid question type: Essay"
```

hoac:

```json
"Invalid level: SuperHard"
```

FE nen handle ca `string` response body cho `400`.

### 3. Plain object bad request

Mot so loi upload file dang tra object don gian:

```json
{
  "error": "No file provided"
}
```

hoac:

```json
{
  "error": "File is empty"
}
```

### 4. AI service unavailable

Hai endpoint AI co the tra `503`:

```json
{
  "type": "about:blank",
  "title": "AI dang ban",
  "status": 503,
  "detail": "...",
  "code": "Homework.AiCreatorBusy"
}
```

hoac:

```json
{
  "type": "about:blank",
  "title": "AI tam gian doan",
  "status": 503,
  "detail": "...",
  "code": "Homework.AiCreatorUnavailable"
}
```

## Enum values

### `questionType`

| Value | Mo ta |
| --- | --- |
| `MultipleChoice` | Cau hoi trac nghiem |
| `TextInput` | Cau hoi nhap text |

### `level`

| Value |
| --- |
| `Easy` |
| `Medium` |
| `Hard` |

### `taskStyle` cho AI

| Value | Mo ta |
| --- | --- |
| `standard` | Tao cau hoi thong thuong |
| `translation` | Tap trung bai dang dich |

## QuestionBankItemDto cho FE

Dung cho list, create response, update response.

```json
{
  "id": "guid",
  "programId": "guid",
  "questionText": "string",
  "questionType": "MultipleChoice",
  "options": ["A", "B", "C", "D"],
  "correctAnswer": "B",
  "points": 1,
  "explanation": "string | null",
  "topic": "string | null",
  "skill": "string | null",
  "grammarTags": ["present simple"],
  "vocabularyTags": ["animals"],
  "level": "Medium",
  "createdAt": "2026-04-23T07:00:00Z",
  "updatedAt": "2026-04-23T08:00:00Z"
}
```

Ghi chu:

- Neu `questionType = TextInput` thi `options` se la `[]`.
- `correctAnswer` cua `MultipleChoice` duoc backend luu theo option text da normalize.
- Khi gui create/update/import cho `MultipleChoice`, `correctAnswer` co the gui theo:
  - option text
  - index `0-based`
  - index `1-based`
  - chu cai `A`, `B`, `C`, `D`

## APIs

| Method | Endpoint | Roles | Mo ta |
| --- | --- | --- | --- |
| POST | `/api/question-bank` | Teacher, ManagementStaff, Admin | Tao question bank item bang tay |
| GET | `/api/question-bank` | Teacher, ManagementStaff, Admin | Lay danh sach question bank item |
| PUT | `/api/question-bank/{id}` | Teacher, ManagementStaff, Admin | Cap nhat 1 question bank item |
| DELETE | `/api/question-bank/{id}` | Teacher, ManagementStaff, Admin | Soft delete 1 question bank item |
| POST | `/api/question-bank/import?programId={guid}` | Teacher, ManagementStaff, Admin | Import file CSV/Excel/Word/PDF |
| POST | `/api/question-bank/ai-generate` | Teacher, ManagementStaff, Admin | Tao draft question bang AI tu JSON |
| POST | `/api/question-bank/ai-generate/from-file` | Teacher, ManagementStaff, Admin | Tao draft question bang AI tu file multipart |

---

## 1. POST `/api/question-bank`

### Request body

```json
{
  "programId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "items": [
    {
      "questionText": "What is the capital of France?",
      "questionType": "MultipleChoice",
      "options": ["London", "Paris", "Berlin", "Madrid"],
      "correctAnswer": "Paris",
      "points": 1,
      "explanation": "Paris is the capital of France.",
      "topic": "Geography",
      "skill": "Reading",
      "grammarTags": ["wh-question"],
      "vocabularyTags": ["country", "capital"],
      "level": "Easy"
    }
  ]
}
```

### Request fields

| Field | Type | Required | Note |
| --- | --- | --- | --- |
| `programId` | `Guid` | Yes | Program phai ton tai |
| `items` | `CreateQuestionBankItemRequest[]` | Yes | Phai co it nhat 1 item |
| `items[].questionText` | `string` | Yes | Khong duoc rong |
| `items[].questionType` | `string` | Yes | `MultipleChoice` hoac `TextInput` |
| `items[].options` | `string[]` | Conditional | Bat buoc voi `MultipleChoice`, toi thieu 2 phan tu |
| `items[].correctAnswer` | `string` | Yes | Bat buoc, se duoc normalize |
| `items[].points` | `int` | Yes | > 0 |
| `items[].explanation` | `string?` | No | Nullable |
| `items[].topic` | `string?` | No | Nullable |
| `items[].skill` | `string?` | No | Nullable |
| `items[].grammarTags` | `string[]?` | No | Nullable |
| `items[].vocabularyTags` | `string[]?` | No | Nullable |
| `items[].level` | `string` | Yes | `Easy`, `Medium`, `Hard` |

### Success response

```json
{
  "isSuccess": true,
  "data": {
    "items": [
      {
        "id": "guid",
        "programId": "guid",
        "questionText": "What is the capital of France?",
        "questionType": "MultipleChoice",
        "options": ["London", "Paris", "Berlin", "Madrid"],
        "correctAnswer": "Paris",
        "points": 1,
        "explanation": "Paris is the capital of France.",
        "topic": "Geography",
        "skill": "Reading",
        "grammarTags": ["wh-question"],
        "vocabularyTags": ["country", "capital"],
        "level": "Easy",
        "createdAt": "2026-04-23T07:00:00Z",
        "updatedAt": null
      }
    ]
  }
}
```

### Errors

| Status | Format | Example |
| --- | --- | --- |
| 400 | string | `Invalid question type: Essay` |
| 400 | string | `Invalid level: SuperHard` |
| 400 | ProblemDetails | `Homework.NoQuestionsProvided` |
| 400 | ProblemDetails | `Homework.InvalidQuestionText` |
| 400 | ProblemDetails | `Homework.InsufficientOptions` |
| 400 | ProblemDetails | `Homework.InvalidCorrectAnswer` |
| 400 | ProblemDetails | `Homework.InvalidPoints` |
| 404 | ProblemDetails | `Homework.ProgramNotFound` |
| 401 | ProblemDetails | Unauthorized |
| 403 | ProblemDetails | Forbidden |

---

## 2. GET `/api/question-bank`

### Query params

| Field | Type | Required | Default |
| --- | --- | --- | --- |
| `programId` | `Guid?` | No | `null` |
| `level` | `string?` | No | `null` |
| `pageNumber` | `int` | No | `1` |
| `pageSize` | `int` | No | `10` |

### Success response

```json
{
  "isSuccess": true,
  "data": {
    "items": {
      "items": [
        {
          "id": "guid",
          "programId": "guid",
          "questionText": "What is the capital of France?",
          "questionType": "MultipleChoice",
          "options": ["London", "Paris", "Berlin", "Madrid"],
          "correctAnswer": "Paris",
          "points": 1,
          "explanation": "Paris is the capital of France.",
          "topic": "Geography",
          "skill": "Reading",
          "grammarTags": ["wh-question"],
          "vocabularyTags": ["country", "capital"],
          "level": "Easy",
          "createdAt": "2026-04-23T07:00:00Z",
          "updatedAt": null
        }
      ],
      "pageNumber": 1,
      "totalPages": 3,
      "totalCount": 23,
      "hasPreviousPage": false,
      "hasNextPage": true
    }
  }
}
```

### FE notes

- Endpoint nay chi tra item chua bi soft delete.
- Hien tai khong co endpoint get detail theo `id`.

### Errors

| Status | Format | Example |
| --- | --- | --- |
| 400 | string | `Invalid level: SuperHard` |
| 401 | ProblemDetails | Unauthorized |
| 403 | ProblemDetails | Forbidden |

---

## 3. PUT `/api/question-bank/{id}`

### Path params

| Field | Type | Required |
| --- | --- | --- |
| `id` | `Guid` | Yes |

### Request body

Shape giong create 1 item:

```json
{
  "programId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "questionText": "What is the capital city of France?",
  "questionType": "MultipleChoice",
  "options": ["London", "Paris", "Berlin", "Madrid"],
  "correctAnswer": "2",
  "points": 2,
  "explanation": "Paris is the capital city of France.",
  "topic": "Geography",
  "skill": "Reading",
  "grammarTags": ["wh-question"],
  "vocabularyTags": ["country", "capital"],
  "level": "Easy"
}
```

### Success response

```json
{
  "isSuccess": true,
  "data": {
    "id": "guid",
    "programId": "guid",
    "questionText": "What is the capital city of France?",
    "questionType": "MultipleChoice",
    "options": ["London", "Paris", "Berlin", "Madrid"],
    "correctAnswer": "Paris",
    "points": 2,
    "explanation": "Paris is the capital city of France.",
    "topic": "Geography",
    "skill": "Reading",
    "grammarTags": ["wh-question"],
    "vocabularyTags": ["country", "capital"],
    "level": "Easy",
    "createdAt": "2026-04-23T07:00:00Z",
    "updatedAt": "2026-04-23T08:30:00Z"
  }
}
```

### Errors

| Status | Format | Example |
| --- | --- | --- |
| 400 | string | `Invalid question type: Essay` |
| 400 | string | `Invalid level: SuperHard` |
| 400 | ProblemDetails | `Homework.InvalidQuestionText` |
| 400 | ProblemDetails | `Homework.InsufficientOptions` |
| 400 | ProblemDetails | `Homework.InvalidCorrectAnswer` |
| 400 | ProblemDetails | `Homework.InvalidPoints` |
| 404 | ProblemDetails | `Homework.QuestionBankItemNotFound` |
| 404 | ProblemDetails | `Homework.ProgramNotFound` |
| 401 | ProblemDetails | Unauthorized |
| 403 | ProblemDetails | Forbidden |

### FE notes

- Neu item da bi soft delete truoc do, update se tra `404 Homework.QuestionBankItemNotFound`.
- `updatedAt` se co gia tri sau khi update thanh cong.

---

## 4. DELETE `/api/question-bank/{id}`

### Path params

| Field | Type | Required |
| --- | --- | --- |
| `id` | `Guid` | Yes |

### Success response

```json
{
  "isSuccess": true,
  "data": null
}
```

### Errors

| Status | Format | Example |
| --- | --- | --- |
| 404 | ProblemDetails | `Homework.QuestionBankItemNotFound` |
| 401 | ProblemDetails | Unauthorized |
| 403 | ProblemDetails | Forbidden |

### FE notes

- Day la soft delete, item se bien mat khoi list sau khi xoa.
- Goi delete lan 2 cho cung item se ra `404`.

---

## 5. POST `/api/question-bank/import?programId={guid}`

### Content type

`multipart/form-data`

### Query params

| Field | Type | Required |
| --- | --- | --- |
| `programId` | `Guid` | Yes |

### Form fields

| Field | Type | Required | Note |
| --- | --- | --- | --- |
| `file` | `File` | Yes | Max ~20 MB |

### Supported file types

| Extension |
| --- |
| `.csv` |
| `.xls` |
| `.xlsx` |
| `.docx` |
| `.pdf` |

### Import columns

Required columns:

- `QuestionText`
- `Options`
- `CorrectAnswer`
- `Level`

Optional columns:

- `Points`
- `Explanation`
- `QuestionType`
- `Topic`
- `Skill`
- `GrammarTags`
- `VocabularyTags`

Accepted aliases tu code:

- `question`, `question_text`
- `choices`
- `correct_answer`, `answer`, `correct`
- `difficulty`
- `score`
- `type`
- `subject`
- `skills`
- `grammar`, `grammar_tags`
- `vocabulary`, `vocabulary_tags`, `vocabtags`, `vocab_tags`

### Import rules quan trong

- Mac dinh `questionType = MultipleChoice` neu cot nay khong co.
- `Options` cua import dang duoc tach bang dau `|`.
  - Vi du: `Dog|Cat|Bird|Fish`
- `GrammarTags` va `VocabularyTags` chap nhan tach bang `|` hoac `,`.
- `Points` mac dinh = `1` neu khong co.

### Success response

```json
{
  "isSuccess": true,
  "data": {
    "importedCount": 25
  }
}
```

### Errors

| Status | Format | Example |
| --- | --- | --- |
| 400 | object | `{ "error": "No file provided" }` |
| 400 | ProblemDetails | `Homework.UnsupportedQuestionBankFileType` |
| 400 | ProblemDetails | `Homework.InvalidQuestionBankFile` |
| 400 | ProblemDetails | `Homework.InvalidQuestionBankRow` |
| 404 | ProblemDetails | `Homework.ProgramNotFound` |
| 401 | ProblemDetails | Unauthorized |
| 403 | ProblemDetails | Forbidden |

### Vi du loi row

```json
{
  "title": "Homework.InvalidQuestionBankRow",
  "status": 400,
  "detail": "Row 5: Invalid CorrectAnswer"
}
```

---

## 6. POST `/api/question-bank/ai-generate`

### Request body

```json
{
  "programId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "topic": "Animals",
  "questionType": "MultipleChoice",
  "questionCount": 5,
  "level": "Medium",
  "skill": "Reading",
  "taskStyle": "standard",
  "grammarTags": ["present simple"],
  "vocabularyTags": ["animals"],
  "instructions": "Use short kid-friendly questions",
  "language": "vi",
  "pointsPerQuestion": 1
}
```

### Request fields

| Field | Type | Required | Note |
| --- | --- | --- | --- |
| `programId` | `Guid` | Yes | Program phai ton tai |
| `topic` | `string` | Yes | Khong duoc rong neu khong co source file |
| `questionType` | `string` | Yes | `MultipleChoice` hoac `TextInput` |
| `questionCount` | `int` | Yes | 1..50 |
| `level` | `string` | Yes | `Easy`, `Medium`, `Hard` |
| `skill` | `string?` | No | Nullable |
| `taskStyle` | `string` | No | `standard` hoac `translation`, mac dinh `standard` |
| `grammarTags` | `string[]` | No | Mac dinh `[]` |
| `vocabularyTags` | `string[]` | No | Mac dinh `[]` |
| `instructions` | `string?` | No | Nullable |
| `language` | `string` | No | Mac dinh `vi` |
| `pointsPerQuestion` | `int` | No | > 0, mac dinh `1` |

### Success response

```json
{
  "isSuccess": true,
  "data": {
    "aiUsed": true,
    "summary": "Generated 5 medium questions about animals.",
    "items": [
      {
        "questionText": "Which animal says meow?",
        "questionType": "MultipleChoice",
        "options": ["Dog", "Cat", "Cow", "Duck"],
        "correctAnswer": "Cat",
        "points": 1,
        "explanation": "Cats say meow.",
        "topic": "Animals",
        "skill": "Reading",
        "grammarTags": ["present simple"],
        "vocabularyTags": ["animals"],
        "level": "Medium"
      }
    ],
    "warnings": []
  }
}
```

### Errors

| Status | Format | Example |
| --- | --- | --- |
| 400 | string | `Invalid question type: Essay` |
| 400 | string | `Invalid level: SuperHard` |
| 400 | string | `Invalid task style: essay` |
| 400 | ProblemDetails | `Homework.AiCreatorTopicRequired` |
| 400 | ProblemDetails | `Homework.AiCreatorQuestionCountInvalid` |
| 400 | ProblemDetails | `Homework.AiCreatorInvalidPoints` |
| 404 | ProblemDetails | `Homework.ProgramNotFound` |
| 503 | ProblemDetails | `Homework.AiCreatorBusy` |
| 503 | ProblemDetails | `Homework.AiCreatorUnavailable` |
| 401 | ProblemDetails | Unauthorized |
| 403 | ProblemDetails | Forbidden |

---

## 7. POST `/api/question-bank/ai-generate/from-file`

### Content type

`multipart/form-data`

### Form fields

| Field | Type | Required | Note |
| --- | --- | --- | --- |
| `programId` | `Guid` | Yes | Program id |
| `file` | `File` | No | Neu khong gui file thi endpoint van fallback sang flow JSON |
| `topic` | `string?` | No | Neu co file thi co the bo trong |
| `questionType` | `string` | Yes | `MultipleChoice` hoac `TextInput` |
| `questionCount` | `int` | No | Mac dinh `10`, valid 1..50 |
| `level` | `string` | Yes | `Easy`, `Medium`, `Hard` |
| `skill` | `string?` | No | Nullable |
| `taskStyle` | `string` | No | `standard` hoac `translation` |
| `grammarTags` | `string[]` | No | Co the repeat field nhieu lan |
| `vocabularyTags` | `string[]` | No | Co the repeat field nhieu lan |
| `instructions` | `string?` | No | Nullable |
| `language` | `string` | No | Mac dinh `vi` |
| `pointsPerQuestion` | `int` | No | > 0 |

### FE notes

- `grammarTags` va `vocabularyTags` o endpoint nay duoc backend normalize theo 2 cach:
  - gui nhieu field cung ten
  - gui 1 field chua chuoi co dau phay
- Neu gui `file` nhung file rong, controller tra:

```json
{
  "error": "File is empty"
}
```

### Success response

Success shape giong endpoint `/ai-generate`.

### Errors

Ngoai cac loi giong `/ai-generate`, endpoint nay co them:

| Status | Format | Example |
| --- | --- | --- |
| 400 | object | `{ "error": "File is empty" }` |
| 400 | ProblemDetails | `Homework.UnsupportedQuestionBankFileType` |
| 400 | ProblemDetails | `Homework.InvalidQuestionBankFile` |

---

## FE implementation notes

### 1. Sau khi soft delete

- Goi lai `GET /api/question-bank` de refresh list.
- Khong nen optimistic update neu FE can chac chan item da mat khoi page tu backend.

### 2. Khi tao/sua multiple choice

- FE co the gui `correctAnswer` theo text option de an toan nhat.
- Neu gui index, backend van normalize, nhung neu danh sach options bi reorder tren FE thi de sai logic.

### 3. Khi import

- Nen validate extension file o FE truoc khi upload.
- Nen huong dan user format `Options` bang `|`.

### 4. Khi AI generate

- Endpoint AI chi tao draft, khong auto save vao question bank.
- FE can neuon user save bang `POST /api/question-bank` sau khi user confirm.

### 5. Error handling khuyen nghi

- Neu response body la `string`: show thang message do.
- Neu response body co `error`: show `error`.
- Neu response body co `title` va `detail`: uu tien show `detail`, co the log them `title`.
- Neu `status = 503` va `code` la AI busy/unavailable: show UI retry.

