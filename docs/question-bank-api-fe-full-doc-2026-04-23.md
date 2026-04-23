# Question Bank API FE Full Doc - 2026-04-23

Tai lieu nay mo ta day du cac API trong `QuestionBankController.cs` de FE tich hop.

## 1. Tong quan role, scope va action

### Role scope

| Role | Duoc xem du lieu gi | Pham vi du lieu | Hanh dong duoc phep |
| --- | --- | --- | --- |
| Admin | Toan bo question bank item | `all` | `view`, `create`, `edit`, `delete`, `import`, `ai-generate` |
| ManagementStaff | Toan bo question bank item | `all` | `view`, `create`, `edit`, `delete`, `import`, `ai-generate` |
| Teacher | Toan bo question bank item | `all` | `view`, `create`, `edit`, `delete`, `import`, `ai-generate` |
| Parent | Khong duoc truy cap | `none` | `none` |
| Student | Khong duoc truy cap | `none` | `none` |
| Anonymous | Khong duoc truy cap | `none` | `none` |

### Ghi chu scope

- Hien tai khong co `own` hay `department` scope.
- Backend khong loc theo creator, branch hay phong ban.
- Tat ca role duoc phep hien tai deu thao tac tren pham vi `all`.

## 2. Common response format

### Success

Tat ca response thanh cong duoc wrap theo:

```json
{
  "isSuccess": true,
  "data": {}
}
```

### Error business / not found / validation

Phan lon loi business tra theo `ProblemDetails`:

```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.1",
  "title": "Homework.InvalidPoints",
  "status": 400,
  "detail": "Question 1 points must be greater than 0"
}
```

### Error plain string

Mot so loi parse enum dang tra truc tiep tu controller:

```json
"Invalid question type: Essay"
```

hoac:

```json
"Invalid level: SuperHard"
```

### Error plain object

Mot so loi upload file dang tra object don gian:

```json
{
  "error": "No file provided"
}
```

### Error AI unavailable

Hai endpoint AI co the tra `503`:

```json
{
  "type": "about:blank",
  "title": "AI dang ban",
  "status": 503,
  "detail": "Hien tai AI dang ban, vui long thu lai sau it phut.",
  "code": "Homework.AiCreatorBusy"
}
```

### HTTP mapping

| Error type | HTTP |
| --- | --- |
| Validation | `400` |
| NotFound | `404` |
| Unauthorized | `401` |
| Forbidden | `403` |
| Failure / unexpected | `500` |
| AI busy / unavailable | `503` |

## 3. Enum va data shape

### `questionType`

| Value | Mo ta |
| --- | --- |
| `MultipleChoice` | Cau hoi trac nghiem |
| `TextInput` | Cau hoi nhap dap an text |

### `level`

| Value | Mo ta |
| --- | --- |
| `Easy` | Do kho de |
| `Medium` | Do kho trung binh |
| `Hard` | Do kho cao |

### `taskStyle`

| Value | Mo ta |
| --- | --- |
| `standard` | Tao cau hoi thong thuong |
| `translation` | Tao cau hoi theo dang dich |

### `QuestionBankItemDto`

```json
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
  "updatedAt": "2026-04-23T08:00:00Z"
}
```

Ghi chu:

- Neu `questionType = TextInput` thi `options` se la `[]`.
- `correctAnswer` cua `MultipleChoice` duoc backend normalize va luu theo option text.
- FE co the gui `correctAnswer` theo option text, index `0-based`, index `1-based`, hoac chu cai `A/B/C/D`.

## 4. Danh sach API

| Method | Endpoint | Role | Mo ta |
| --- | --- | --- | --- |
| POST | `/api/question-bank` | Teacher, ManagementStaff, Admin | Tao question bank item bang tay |
| GET | `/api/question-bank` | Teacher, ManagementStaff, Admin | Lay danh sach question bank item |
| PUT | `/api/question-bank/{id}` | Teacher, ManagementStaff, Admin | Cap nhat 1 question bank item |
| DELETE | `/api/question-bank/{id}` | Teacher, ManagementStaff, Admin | Soft delete 1 question bank item |
| POST | `/api/question-bank/import?programId={guid}` | Teacher, ManagementStaff, Admin | Import question bank tu file |
| POST | `/api/question-bank/ai-generate` | Teacher, ManagementStaff, Admin | Tao draft question bank bang AI tu JSON |
| POST | `/api/question-bank/ai-generate/from-file` | Teacher, ManagementStaff, Admin | Tao draft question bank bang AI tu multipart form va file |

## 5. API detail

### 5.1 POST `/api/question-bank`

Dung de tao moi nhieu question bank item bang tay.

Roles:

- `Teacher`
- `ManagementStaff`
- `Admin`

Body:

| Field | Type | Required | Rule |
| --- | --- | --- | --- |
| `programId` | `Guid` | Yes | Program phai ton tai, chua bi delete |
| `items` | `array` | Yes | Phai co it nhat 1 item |
| `items[].questionText` | `string` | Yes | Khong duoc rong |
| `items[].questionType` | `string` | Yes | `MultipleChoice` hoac `TextInput` |
| `items[].options` | `string[]` | Conditional | Bat buoc voi `MultipleChoice`, toi thieu 2 phan tu |
| `items[].correctAnswer` | `string` | Yes | Bat buoc, se duoc normalize |
| `items[].points` | `int` | Yes | Phai > 0 |
| `items[].explanation` | `string?` | No | Nullable |
| `items[].topic` | `string?` | No | Nullable |
| `items[].skill` | `string?` | No | Nullable |
| `items[].grammarTags` | `string[]?` | No | Nullable |
| `items[].vocabularyTags` | `string[]?` | No | Nullable |
| `items[].level` | `string` | Yes | `Easy`, `Medium`, `Hard` |

Success response:

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

Error responses:

| HTTP | Code / message | Khi nao |
| --- | --- | --- |
| 400 | `Invalid question type: ...` | `questionType` parse fail |
| 400 | `Invalid level: ...` | `level` parse fail |
| 400 | `Homework.NoQuestionsProvided` | `items` rong |
| 400 | `Homework.InvalidQuestionText` | `questionText` rong |
| 400 | `Homework.InsufficientOptions` | `MultipleChoice` < 2 options |
| 400 | `Homework.InvalidCorrectAnswer` | `correctAnswer` khong map duoc vao options |
| 400 | `Homework.InvalidPoints` | `points <= 0` |
| 404 | `Homework.ProgramNotFound` | Program khong ton tai hoac bi delete |
| 401 | Unauthorized | Chua dang nhap |
| 403 | Forbidden | Role khong duoc phep |

### 5.2 GET `/api/question-bank`

Dung de lay danh sach question bank item co phan trang.

Roles:

- `Teacher`
- `ManagementStaff`
- `Admin`

Query params:

| Field | Type | Required | Default | Mo ta |
| --- | --- | --- | --- | --- |
| `programId` | `Guid?` | No | `null` | Loc theo program |
| `level` | `string?` | No | `null` | Loc theo do kho |
| `pageNumber` | `int` | No | `1` | So trang |
| `pageSize` | `int` | No | `10` | Kich thuoc trang |

Success response:

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

Error responses:

| HTTP | Code / message | Khi nao |
| --- | --- | --- |
| 400 | `Invalid level: ...` | `level` parse fail |
| 401 | Unauthorized | Chua dang nhap |
| 403 | Forbidden | Role khong duoc phep |

Ghi chu:

- Backend chi tra item `!isDeleted`.
- Hien tai khong co endpoint `GET /api/question-bank/{id}`.
- Ket qua dang sort theo `createdAt desc`.

### 5.3 PUT `/api/question-bank/{id}`

Dung de cap nhat 1 question bank item. API nay cho phep doi ca `programId` cua item sang program khac neu request hop le.

Roles:

- `Teacher`
- `ManagementStaff`
- `Admin`

Path params:

| Field | Type | Required |
| --- | --- | --- |
| `id` | `Guid` | Yes |

Body:

| Field | Type | Required | Rule |
| --- | --- | --- | --- |
| `programId` | `Guid` | Yes | Program phai ton tai, chua bi delete |
| `questionText` | `string` | Yes | Khong duoc rong |
| `questionType` | `string` | Yes | `MultipleChoice` hoac `TextInput` |
| `options` | `string[]` | Conditional | Bat buoc voi `MultipleChoice`, toi thieu 2 phan tu |
| `correctAnswer` | `string` | Yes | Bat buoc |
| `points` | `int` | Yes | Phai > 0 |
| `explanation` | `string?` | No | Rong se duoc save thanh `null` |
| `topic` | `string?` | No | Rong se duoc save thanh `null` |
| `skill` | `string?` | No | Rong se duoc save thanh `null` |
| `grammarTags` | `string[]?` | No | Nullable |
| `vocabularyTags` | `string[]?` | No | Nullable |
| `level` | `string` | Yes | `Easy`, `Medium`, `Hard` |

Success response:

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

Error responses:

| HTTP | Code / message | Khi nao |
| --- | --- | --- |
| 400 | `Invalid question type: ...` | `questionType` parse fail |
| 400 | `Invalid level: ...` | `level` parse fail |
| 400 | `Homework.InvalidQuestionText` | `questionText` rong |
| 400 | `Homework.InsufficientOptions` | `MultipleChoice` < 2 options |
| 400 | `Homework.InvalidCorrectAnswer` | `correctAnswer` khong hop le |
| 400 | `Homework.InvalidPoints` | `points <= 0` |
| 404 | `Homework.QuestionBankItemNotFound` | Item khong ton tai hoac da bi soft delete |
| 404 | `Homework.ProgramNotFound` | Program moi khong ton tai hoac bi delete |
| 401 | Unauthorized | Chua dang nhap |
| 403 | Forbidden | Role khong duoc phep |

### 5.4 DELETE `/api/question-bank/{id}`

Dung de soft delete 1 question bank item.

Roles:

- `Teacher`
- `ManagementStaff`
- `Admin`

Path params:

| Field | Type | Required |
| --- | --- | --- |
| `id` | `Guid` | Yes |

Success response:

```json
{
  "isSuccess": true,
  "data": null
}
```

Error responses:

| HTTP | Code / message | Khi nao |
| --- | --- | --- |
| 404 | `Homework.QuestionBankItemNotFound` | Item khong ton tai hoac da bi soft delete |
| 401 | Unauthorized | Chua dang nhap |
| 403 | Forbidden | Role khong duoc phep |

Ghi chu:

- Day la soft delete, item se bien mat khoi API list sau khi xoa.
- Goi delete lan 2 cho cung item se tra `404`.

### 5.5 POST `/api/question-bank/import?programId={guid}`

Dung de import nhieu question bank item tu file.

Roles:

- `Teacher`
- `ManagementStaff`
- `Admin`

Content type:

`multipart/form-data`

Query params:

| Field | Type | Required | Rule |
| --- | --- | --- | --- |
| `programId` | `Guid` | Yes | Program phai ton tai, chua bi delete |

Form fields:

| Field | Type | Required | Rule |
| --- | --- | --- | --- |
| `file` | `File` | Yes | Max ~20 MB theo `RequestSizeLimit(20_971_520)` |

Supported file types:

| Extension |
| --- |
| `.csv` |
| `.xls` |
| `.xlsx` |
| `.docx` |
| `.pdf` |

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

Accepted aliases:

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

Import rules:

- Mac dinh `questionType = MultipleChoice` neu cot nay khong co.
- `Options` cua file import dang tach bang dau `|`.
- `GrammarTags` va `VocabularyTags` chap nhan chuoi tach bang `|` hoac `,`.
- `Points` mac dinh = `1` neu khong co.
- File rong, file sai format, file khong co header hop le deu co the bi reject.

Success response:

```json
{
  "isSuccess": true,
  "data": {
    "importedCount": 25
  }
}
```

Error responses:

| HTTP | Code / message | Khi nao |
| --- | --- | --- |
| 400 | `{ "error": "No file provided" }` | Khong gui file |
| 400 | `Homework.UnsupportedQuestionBankFileType` | Sai extension |
| 400 | `Homework.InvalidQuestionBankFile` | File rong, thieu header, khong doc duoc |
| 400 | `Homework.InvalidQuestionBankRow` | 1 row co du lieu sai |
| 404 | `Homework.ProgramNotFound` | Program khong ton tai hoac bi delete |
| 401 | Unauthorized | Chua dang nhap |
| 403 | Forbidden | Role khong duoc phep |

Vi du loi row:

```json
{
  "title": "Homework.InvalidQuestionBankRow",
  "status": 400,
  "detail": "Row 5: Invalid CorrectAnswer"
}
```

### 5.6 POST `/api/question-bank/ai-generate`

Dung de tao draft question bank item bang AI tu JSON request. Ket qua tra ve chua duoc luu vao database.

Roles:

- `Teacher`
- `ManagementStaff`
- `Admin`

Body:

| Field | Type | Required | Rule |
| --- | --- | --- | --- |
| `programId` | `Guid` | Yes | Program phai ton tai |
| `topic` | `string` | Conditional | Bat buoc neu khong co source text/file |
| `questionType` | `string` | Yes | `MultipleChoice` hoac `TextInput` |
| `questionCount` | `int` | Yes | Tu `1` den `50` |
| `level` | `string` | Yes | `Easy`, `Medium`, `Hard` |
| `skill` | `string?` | No | Nullable |
| `taskStyle` | `string` | No | `standard` hoac `translation`, mac dinh `standard` |
| `grammarTags` | `string[]` | No | Mac dinh `[]` |
| `vocabularyTags` | `string[]` | No | Mac dinh `[]` |
| `instructions` | `string?` | No | Nullable |
| `language` | `string` | No | Mac dinh `vi` |
| `pointsPerQuestion` | `int` | No | Phai > 0, mac dinh `1` |

Success response:

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

Error responses:

| HTTP | Code / message | Khi nao |
| --- | --- | --- |
| 400 | `Invalid question type: ...` | `questionType` parse fail |
| 400 | `Invalid level: ...` | `level` parse fail |
| 400 | `Invalid task style: ...` | `taskStyle` khong phai `standard`/`translation` |
| 400 | `Homework.AiCreatorTopicRequired` | Khong co `topic` va cung khong co source text/file |
| 400 | `Homework.AiCreatorQuestionCountInvalid` | `questionCount` ngoai khoang `1..50` |
| 400 | `Homework.AiCreatorInvalidPoints` | `pointsPerQuestion <= 0` |
| 404 | `Homework.ProgramNotFound` | Program khong ton tai |
| 503 | `Homework.AiCreatorBusy` | AI dang ban |
| 503 | `Homework.AiCreatorUnavailable` | AI tam gian doan |
| 401 | Unauthorized | Chua dang nhap |
| 403 | Forbidden | Role khong duoc phep |

Ghi chu:

- Endpoint nay chi tao draft cho FE review.
- Neu FE muon luu thuc su vao question bank thi phai goi tiep `POST /api/question-bank`.

### 5.7 POST `/api/question-bank/ai-generate/from-file`

Dung de tao draft question bank bang AI tu `multipart/form-data`, co the gui kem file nguon. Neu khong gui file, endpoint se fallback ve logic gan giong flow JSON.

Roles:

- `Teacher`
- `ManagementStaff`
- `Admin`

Content type:

`multipart/form-data`

Form fields:

| Field | Type | Required | Rule |
| --- | --- | --- | --- |
| `programId` | `Guid` | Yes | Program phai ton tai |
| `file` | `File` | No | Max ~20 MB; neu gui file rong se tra loi |
| `topic` | `string?` | No | Co the bo trong neu da gui file |
| `questionType` | `string` | Yes | `MultipleChoice` hoac `TextInput` |
| `questionCount` | `int` | No | Mac dinh `10`, valid `1..50` |
| `level` | `string` | Yes | `Easy`, `Medium`, `Hard` |
| `skill` | `string?` | No | Nullable |
| `taskStyle` | `string` | No | `standard` hoac `translation` |
| `grammarTags` | `string[]` | No | Co the gui lap lai field nhieu lan |
| `vocabularyTags` | `string[]` | No | Co the gui lap lai field nhieu lan |
| `instructions` | `string?` | No | Nullable |
| `language` | `string` | No | Mac dinh `vi` |
| `pointsPerQuestion` | `int` | No | Phai > 0 |

Success response:

- Cung shape voi endpoint `POST /api/question-bank/ai-generate`.

Error responses:

| HTTP | Code / message | Khi nao |
| --- | --- | --- |
| 400 | `{ "error": "File is empty" }` | Gui file nhung file rong |
| 400 | `Invalid question type: ...` | `questionType` parse fail |
| 400 | `Invalid level: ...` | `level` parse fail |
| 400 | `Invalid task style: ...` | `taskStyle` khong hop le |
| 400 | `Homework.AiCreatorTopicRequired` | Khong co `topic` va khong extract duoc source tu file |
| 400 | `Homework.AiCreatorQuestionCountInvalid` | `questionCount` ngoai khoang `1..50` |
| 400 | `Homework.AiCreatorInvalidPoints` | `pointsPerQuestion <= 0` |
| 400 | `Homework.UnsupportedQuestionBankFileType` | File nguon sai extension |
| 400 | `Homework.InvalidQuestionBankFile` | File nguon khong doc duoc |
| 404 | `Homework.ProgramNotFound` | Program khong ton tai |
| 503 | `Homework.AiCreatorBusy` | AI dang ban |
| 503 | `Homework.AiCreatorUnavailable` | AI tam gian doan |
| 401 | Unauthorized | Chua dang nhap |
| 403 | Forbidden | Role khong duoc phep |

Ghi chu:

- `grammarTags` va `vocabularyTags` duoc backend normalize theo 2 cach:
- Gui nhieu field cung ten.
- Gui 1 field chua chuoi co dau phay, backend se tu tach.
- Neu co file, backend se extract text tu file roi dua vao AI prompt.

## 6. Status definition

Question bank hien tai khong co enum status rieng tren response. Trang thai duoc suy ra tu du lieu.

### Logical status

| Trang thai | Dieu kien | Y nghia |
| --- | --- | --- |
| Active | `isDeleted = false` | Item dang ton tai va duoc tra ra o API list |
| Deleted | `isDeleted = true` | Item da soft delete, khong con hien tren list |
| AI Draft | Khong luu DB | Ket qua draft cua 2 endpoint AI, chi ton tai trong response |

### Luong chuyen trang thai

1. `POST /api/question-bank` -> tao item moi o trang thai `Active`
2. `PUT /api/question-bank/{id}` -> cap nhat item, van o `Active`
3. `DELETE /api/question-bank/{id}` -> chuyen sang `Deleted`
4. `POST /api/question-bank/ai-generate*` -> chi tao `AI Draft`, chua save
5. FE neu muon luu `AI Draft` thi map ve body create va goi `POST /api/question-bank`

## 7. Permission matrix theo role

| Endpoint | Admin | ManagementStaff | Teacher | Parent | Student | Anonymous |
| --- | --- | --- | --- | --- | --- | --- |
| `POST /api/question-bank` | Yes | Yes | Yes | No | No | No |
| `GET /api/question-bank` | Yes | Yes | Yes | No | No | No |
| `PUT /api/question-bank/{id}` | Yes | Yes | Yes | No | No | No |
| `DELETE /api/question-bank/{id}` | Yes | Yes | Yes | No | No | No |
| `POST /api/question-bank/import` | Yes | Yes | Yes | No | No | No |
| `POST /api/question-bank/ai-generate` | Yes | Yes | Yes | No | No | No |
| `POST /api/question-bank/ai-generate/from-file` | Yes | Yes | Yes | No | No | No |

## 8. Validation rule tong hop

| Rule | API ap dung | Loi |
| --- | --- | --- |
| `programId` phai ton tai | create, update, import, ai-generate, ai-generate/from-file | 404 `Homework.ProgramNotFound` |
| `items` phai co it nhat 1 phan tu | create | 400 `Homework.NoQuestionsProvided` |
| `questionText` khong duoc rong | create, update | 400 `Homework.InvalidQuestionText` |
| `questionType` phai parse duoc | create, update, ai-generate, ai-generate/from-file | 400 plain string |
| `level` phai parse duoc | create, get list, update, ai-generate, ai-generate/from-file, import | 400 plain string hoac `Homework.InvalidQuestionBankRow` |
| `MultipleChoice` phai co it nhat 2 options | create, update, import | 400 `Homework.InsufficientOptions` hoac `Homework.InvalidQuestionBankRow` |
| `correctAnswer` phai hop le | create, update, import | 400 `Homework.InvalidCorrectAnswer` hoac `Homework.InvalidQuestionBankRow` |
| `points` phai > 0 | create, update, import | 400 `Homework.InvalidPoints` hoac `Homework.InvalidQuestionBankRow` |
| `questionCount` trong khoang `1..50` | ai-generate, ai-generate/from-file | 400 `Homework.AiCreatorQuestionCountInvalid` |
| `pointsPerQuestion` phai > 0 | ai-generate, ai-generate/from-file | 400 `Homework.AiCreatorInvalidPoints` |
| `topic` bat buoc neu khong co source file/source text | ai-generate, ai-generate/from-file | 400 `Homework.AiCreatorTopicRequired` |
| File import phai co extension ho tro | import, ai-generate/from-file | 400 `Homework.UnsupportedQuestionBankFileType` |
| File upload khong duoc rong | import, ai-generate/from-file | 400 plain object |

## 9. Cac truong hop tra loi can luu y

- `GET /api/question-bank` hien tai khong validate `programId` co ton tai hay khong; neu `programId` sai thi ket qua don gian la list rong.
- `PUT /api/question-bank/{id}` se tra `404` neu item da bi soft delete.
- `DELETE /api/question-bank/{id}` la soft delete, khong xoa vat ly.
- `POST /api/question-bank/import` co the fail ngay o row dau tien sai; backend khong import partial.
- Hai endpoint AI co the tra `503`, FE nen show UI retry.
- Hai endpoint AI khong tu dong save vao DB.
- Hien tai khong co API get detail theo `id` va khong co API restore sau khi soft delete.
