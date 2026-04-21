// KidzGo high-level relational schema for dbdiagram.io
// Scope: multi-branch, shared login with profiles, classes/sessions,
// attendance/makeup, lesson plans/homework, exams, AI monthly reports,
// gamification, CRM/lead, media, registrations/enrollment confirmation,
// notifications, tickets.

Table email_templates {
  id uuid [pk]
  code varchar(100) [unique]
  subject varchar(255)
  body text
  placeholders jsonb
  is_active boolean
  is_deleted boolean
  created_at timestamptz
  updated_at timestamptz
}

Table branches {
  id uuid [pk]
  code varchar(32) [unique]
  name varchar(255)
  address text
  contact_phone varchar(32)
  contact_email varchar(255)
  is_active boolean
  created_at timestamptz
  updated_at timestamptz
}

Table users {
  id uuid [pk]
  email varchar(255) [unique]
  password_hash text
  role varchar(20) // PARENT/STUDENT/ADMIN/TEACHER/STAFF
  username varchar(255) // required for ADMIN/TEACHER/STAFF; optional for PARENT/STUDENT
  name varchar(255) // display name for users (optional)
  branch_id uuid [ref: - branches.id] // required for TEACHER/STAFF; null for ADMIN/PARENT/STUDENT
  is_active boolean
  is_deleted boolean
  image_url text
  created_at timestamptz
  updated_at timestamptz
}

Table profiles {
  id uuid [pk]
  user_id uuid [ref: > users.id]
  profile_type varchar(20) // PARENT/STUDENT only
  display_name varchar(255)
  pin_hash varchar(97) // required when selecting PARENT profile, validate PIN < 10 digits before hash. Format: PBKDF2-SHA512 hash (64 hex) + '-' + salt (32 hex) = 97 chars
  is_active boolean
  created_at timestamptz
  updated_at timestamptz
}

Table parent_student_links {
  id uuid [pk]
  parent_profile_id uuid [ref: > profiles.id]
  student_profile_id uuid [ref: > profiles.id]
  created_at timestamptz
}

// NEW: table exists in the current project and was missing from this dbdiagram file
Table refresh_tokens {
  id uuid [pk]
  token varchar(255) [unique]
  user_id uuid [ref: > users.id]
  expires timestamptz
}

// NEW: table exists in the current project and was missing from this dbdiagram file
Table password_reset_tokens {
  id uuid [pk]
  user_id uuid [ref: > users.id]
  token varchar(200)
  expires_at timestamptz
  used_at timestamptz
}

// NEW: table exists in the current project and was missing from this dbdiagram file
Table parent_pin_reset_tokens {
  id uuid [pk]
  profile_id uuid [ref: > profiles.id]
  token varchar(200)
  otp_code_hash varchar(255)
  otp_expires_at timestamptz
  otp_verified_at timestamptz
  otp_attempt_count int
  expires_at timestamptz
  used_at timestamptz
}

// NEW: table exists in the current project and was missing from this dbdiagram file
Table device_tokens {
  id uuid [pk]
  user_id uuid [ref: > users.id]
  token varchar(500)
  device_type varchar(50)
  device_id varchar(200)
  role varchar(20)
  browser varchar(100)
  locale varchar(20)
  branch_id uuid
  is_active boolean
  created_at timestamptz
  updated_at timestamptz
  last_used_at timestamptz

  Indexes {
    user_id
    (user_id, is_active)
    (user_id, device_id, is_active) [unique]
  }
}

// Note: roles and profile_roles are kept for backward compatibility or future use
// For PARENT/STUDENT profiles, can still use profile_roles if needed
// For ADMIN/TEACHER/STAFF, role is directly in users.role enum   

// Table roles {
//   id uuid [pk]
//   code varchar(50) [unique] // ROLE_ADMIN/ROLE_TEACHER/ROLE_STAFF_OPS/ROLE_STAFF_ACC/ROLE_PARENT/ROLE_STUDENT
//   description text
// }
// Table profile_roles {
//   id uuid [pk]
//   profile_id uuid [ref: > profiles.id]
//   role_id uuid [ref: > roles.id]
// }

Table programs {
  id uuid [pk]
  branch_id uuid [ref: > branches.id] // các chi nhánh không dùng chung program
  name varchar(255)
  level varchar(100)
  total_sessions int
  default_tuition_amount numeric
  unit_price_session numeric
  description text
  is_active boolean
  is_deleted boolean
}

// NEW: table exists in the current project and was missing from this dbdiagram file
Table program_leave_policies {
  id uuid [pk]
  program_id uuid [ref: > programs.id]
  max_leaves_per_month int
  updated_by uuid [ref: - users.id]
  created_at timestamptz
  updated_at timestamptz

  Indexes {
    (program_id) [unique]
  }
}

// NEW: table exists in the current project and was missing from this dbdiagram file
Table extracurricular_programs {
  id uuid [pk]
  branch_id uuid [ref: > branches.id]
  name varchar(255)
  type varchar(100)
  date date
  capacity int
  registered_count int
  fee numeric
  location varchar(255)
  is_active boolean
  is_deleted boolean
  created_at timestamptz
  updated_at timestamptz
}

Table classrooms {
  id uuid [pk]
  branch_id uuid [ref: > branches.id]
  name varchar(100)
  capacity int
  note text
  is_active boolean
}

Table classes {
  id uuid [pk]
  branch_id uuid [ref: > branches.id]
  program_id uuid [ref: > programs.id]
  code varchar(50) [unique]
  title varchar(255)
  main_teacher_id uuid [ref: - users.id] // teacher user (role=TEACHER)
  assistant_teacher_id uuid [ref: - users.id] // assistant teacher user (role=TEACHER)
  start_date date
  end_date date
  status varchar(20) // PLANNED/ACTIVE/CLOSED
  capacity int
  schedule_pattern text // RRULE/JSON description
  created_at timestamptz
  updated_at timestamptz
}

Table class_enrollments {
  id uuid [pk]
  class_id uuid [ref: > classes.id]
  student_profile_id uuid [ref: > profiles.id]
  enroll_date date
  status varchar(20) // ACTIVE/PAUSED/DROPPED
  tuition_plan_id uuid [ref: - tuition_plans.id]
  registration_id uuid [ref: - registrations.id] // NEW FIELD
  track varchar(20) // NEW FIELD: Primary/Secondary
  session_selection_pattern varchar(500) // NEW FIELD
  enrollment_confirmation_pdf_url varchar(1000) // NEW FIELD
  enrollment_confirmation_pdf_generated_at timestamptz // NEW FIELD
  enrollment_confirmation_pdf_generated_by uuid // NEW FIELD
  created_at timestamptz // NEW FIELD
  updated_at timestamptz // NEW FIELD
}

// NEW: table exists in the current project and was missing from this dbdiagram file
Table class_schedule_segments {
  id uuid [pk]
  class_id uuid [ref: > classes.id]
  effective_from date
  effective_to date
  schedule_pattern varchar(500)
  created_at timestamptz
  updated_at timestamptz

  Indexes {
    (class_id, effective_from) [unique]
  }
}

// NEW: table exists in the current project and was missing from this dbdiagram file
Table class_enrollment_schedule_segments {
  id uuid [pk]
  class_enrollment_id uuid [ref: > class_enrollments.id]
  effective_from date
  effective_to date
  session_selection_pattern varchar(500)
  created_at timestamptz
  updated_at timestamptz

  Indexes {
    (class_enrollment_id, effective_from) [unique]
  }
}

// NEW: table exists in the current project and was missing from this dbdiagram file
Table registrations {
  id uuid [pk]
  student_profile_id uuid [ref: > profiles.id]
  branch_id uuid [ref: > branches.id]
  program_id uuid [ref: > programs.id]
  tuition_plan_id uuid [ref: > tuition_plans.id]
  secondary_program_id uuid [ref: - programs.id]
  registration_date timestamptz
  expected_start_date timestamptz
  actual_start_date timestamptz
  preferred_schedule text
  note text
  status varchar(20) // New/WaitingForClass/ClassAssigned/Studying/Paused/Completed/Cancelled
  class_id uuid [ref: - classes.id]
  class_assigned_date timestamptz
  entry_type varchar(20) // Immediate/Makeup/Wait/Retake
  secondary_class_id uuid [ref: - classes.id]
  secondary_class_assigned_date timestamptz
  secondary_entry_type varchar(20)
  secondary_program_skill_focus varchar(50)
  original_registration_id uuid [ref: - registrations.id]
  operation_type varchar(20) // Initial/Upgrade/Renewal/Transfer/TransferBranch/Retake
  total_sessions int
  used_sessions int
  remaining_sessions int
  expiry_date timestamptz
  created_at timestamptz
  updated_at timestamptz
}

// NEW: table exists in the current project and was missing from this dbdiagram file
Table enrollment_confirmation_payment_settings {
  id uuid [pk]
  branch_id uuid [ref: - branches.id]
  scope_key varchar(64) [unique]
  payment_method varchar(100)
  account_name varchar(200)
  account_number varchar(50)
  bank_name varchar(200)
  bank_code varchar(50)
  bank_bin varchar(20)
  viet_qr_template varchar(50)
  logo_url varchar(1000)
  is_active boolean
  created_at timestamptz
  updated_at timestamptz
  updated_by uuid

  Indexes {
    branch_id
  }
}

// NEW: table exists in the current project and was missing from this dbdiagram file
Table enrollment_confirmation_pdfs {
  id uuid [pk]
  registration_id uuid [ref: > registrations.id]
  enrollment_id uuid [ref: > class_enrollments.id]
  track varchar(20) // Primary/Secondary
  form_type varchar(30) // NewStudent/ContinuingStudent
  pdf_url varchar(1000)
  generated_at timestamptz
  generated_by uuid
  is_active boolean
  snapshot_json jsonb

  Indexes {
    registration_id
    enrollment_id
    generated_at
    (enrollment_id, track, form_type, is_active)
  }
}

Table sessions {
  id uuid [pk]
  class_id uuid [ref: > classes.id]
  branch_id uuid [ref: > branches.id]
  planned_datetime timestamptz
  planned_room_id uuid [ref: - classrooms.id]
  planned_teacher_id uuid [ref: - users.id] // teacher user (role=TEACHER)
  planned_assistant_id uuid [ref: - users.id] // assistant teacher user (role=TEACHER)
  duration_minutes int
  participation_type varchar(20) // MAIN/MAKEUP/EXTRA_PAID/FREE/TRIAL
  status varchar(20) // SCHEDULED/COMPLETED/CANCELLED
  actual_datetime timestamptz
  actual_room_id uuid [ref: - classrooms.id]
  actual_teacher_id uuid [ref: - users.id] // teacher user (role=TEACHER)
  actual_assistant_id uuid [ref: - users.id] // assistant teacher user (role=TEACHER)
  created_at timestamptz
  updated_at timestamptz
}

// NEW: table exists in the current project and was missing from this dbdiagram file
Table student_session_assignments {
  id uuid [pk]
  session_id uuid [ref: > sessions.id]
  student_profile_id uuid [ref: > profiles.id]
  class_enrollment_id uuid [ref: > class_enrollments.id]
  registration_id uuid [ref: - registrations.id]
  track varchar(20) // Primary/Secondary
  status varchar(20) // Assigned/Cancelled
  created_at timestamptz
  updated_at timestamptz

  Indexes {
    session_id
    student_profile_id
    class_enrollment_id
    registration_id
    (session_id, status)
    (student_profile_id, status)
    (session_id, class_enrollment_id) [unique]
  }
}

Table leave_requests {
  id uuid [pk]
  student_profile_id uuid [ref: > profiles.id]
  class_id uuid [ref: > classes.id]
  session_date date
  end_date date // nullable for single day
  reason text
  notice_hours int // hours before planned session
  status varchar(20) // PENDING/APPROVED/REJECTED
  requested_at timestamptz
  approved_by uuid [ref: - users.id] // staff user (role=STAFF)
  approved_at timestamptz
}

// NEW: table exists in the current project and was missing from this dbdiagram file
Table pause_enrollment_requests {
  id uuid [pk]
  student_profile_id uuid [ref: > profiles.id]
  class_id uuid [ref: - classes.id]
  pause_from date
  pause_to date
  reason text
  status varchar(20) // Pending/Approved/Rejected/Cancelled
  requested_at timestamptz
  approved_by uuid [ref: - users.id]
  approved_at timestamptz
  cancelled_by uuid [ref: - users.id]
  cancelled_at timestamptz
  outcome varchar(40) // ContinueSameClass/ReassignEquivalentClass/ContinueWithTutoring
  outcome_note text
  outcome_by uuid [ref: - users.id]
  outcome_at timestamptz
}

// NEW: table exists in the current project and was missing from this dbdiagram file
Table pause_enrollment_request_histories {
  id uuid [pk]
  pause_enrollment_request_id uuid [ref: > pause_enrollment_requests.id]
  student_profile_id uuid [ref: > profiles.id]
  class_id uuid [ref: > classes.id]
  enrollment_id uuid [ref: - class_enrollments.id]
  previous_status varchar(20) // Active/Paused/Dropped
  new_status varchar(20) // Active/Paused/Dropped
  pause_from date
  pause_to date
  changed_at timestamptz
  changed_by uuid [ref: - users.id]
}

Table attendances {
  id uuid [pk]
  session_id uuid [ref: > sessions.id]
  student_profile_id uuid [ref: > profiles.id]
  attendance_status varchar(20) // PRESENT/ABSENT/MAKEUP
  absence_type varchar(30) // WITH_NOTICE_24H/UNDER_24H/NO_NOTICE/LONG_TERM
  marked_by uuid [ref: - users.id] // teacher
  marked_at timestamptz

  
}

Table makeup_credits {
  id uuid [pk]
  student_profile_id uuid [ref: > profiles.id]
  source_session_id uuid [ref: > sessions.id]
  status varchar(20) // AVAILABLE/USED/EXPIRED
  created_reason varchar(30) // APPROVED_LEAVE_24H/LONG_TERM
  expires_at timestamptz
  used_session_id uuid [ref: - sessions.id]
  created_at timestamptz
}

Table makeup_allocations {
  id uuid [pk]
  makeup_credit_id uuid [ref: > makeup_credits.id]
  target_session_id uuid [ref: > sessions.id]
  assigned_by uuid [ref: - users.id]
  assigned_at timestamptz
}

Table lesson_plan_templates {
  id uuid [pk]
  program_id uuid [ref: > programs.id]
  level varchar(100)
  session_index int
  structure_json jsonb
  is_active boolean
  is_deleted boolean
  created_by uuid [ref: - users.id]
  created_at timestamptz
}

Table lesson_plans {
  id uuid [pk]
  session_id uuid [ref: > sessions.id] //1:1 nhé
  template_id uuid [ref: - lesson_plan_templates.id]
  planned_content jsonb
  actual_content jsonb
  actual_homework text
  teacher_notes text
  submitted_by uuid [ref: - users.id]
  submitted_at timestamptz
}

// NEW: table exists in the current project and was missing from this dbdiagram file
Table teaching_materials {
  id uuid [pk]
  program_id uuid [ref: > programs.id]
  unit_number int
  lesson_number int
  lesson_title varchar(255)
  relative_path varchar(500)
  display_name varchar(255)
  original_file_name varchar(255)
  storage_path varchar(500)
  mime_type varchar(255)
  file_extension varchar(20)
  file_size bigint
  file_type varchar(50) // Pdf/Presentation/Audio/Document/Image/Video/Spreadsheet/Archive/Other
  category varchar(50) // ProgramDocument/LessonSlide/LessonAsset/Supplementary/Other
  is_encrypted boolean
  encryption_algorithm varchar(50)
  encryption_key_version varchar(20)
  pdf_preview_path varchar(500)
  pdf_preview_generated_at timestamptz
  pdf_preview_file_size bigint
  uploaded_by_user_id uuid [ref: > users.id]
  created_at timestamptz
  updated_at timestamptz

  Indexes {
    program_id
    (program_id, unit_number, lesson_number)
  }
}

// NEW: table exists in the current project and was missing from this dbdiagram file
Table teaching_material_slides {
  id uuid [pk]
  teaching_material_id uuid [ref: > teaching_materials.id]
  slide_number int
  preview_image_path varchar(500)
  thumbnail_image_path varchar(500)
  width int
  height int
  notes text
  generated_at timestamptz

  Indexes {
    teaching_material_id
    (teaching_material_id, slide_number) [unique]
  }
}

// NEW: table exists in the current project and was missing from this dbdiagram file
Table teaching_material_view_progresses {
  id uuid [pk]
  teaching_material_id uuid [ref: > teaching_materials.id]
  user_id uuid [ref: > users.id]
  progress_percent int
  last_slide_viewed int
  total_time_seconds int
  view_count int
  completed boolean
  first_viewed_at timestamptz
  last_viewed_at timestamptz

  Indexes {
    teaching_material_id
    user_id
    completed
    (teaching_material_id, user_id) [unique]
  }
}

// NEW: table exists in the current project and was missing from this dbdiagram file
Table teaching_material_bookmarks {
  id uuid [pk]
  teaching_material_id uuid [ref: > teaching_materials.id]
  user_id uuid [ref: > users.id]
  note varchar(500)
  created_at timestamptz

  Indexes {
    (teaching_material_id, user_id) [unique]
  }
}

// NEW: table exists in the current project and was missing from this dbdiagram file
Table teaching_material_annotations {
  id uuid [pk]
  teaching_material_id uuid [ref: > teaching_materials.id]
  slide_number int
  user_id uuid [ref: > users.id]
  content varchar(2000)
  color varchar(20)
  position_x float
  position_y float
  type varchar(20) // Note/Highlight/Pin
  visibility varchar(20) // Private/Class/Public
  created_at timestamptz
  updated_at timestamptz

  Indexes {
    teaching_material_id
    user_id
    (teaching_material_id, slide_number)
  }
}

Table homework_assignments {
  id uuid [pk]
  class_id uuid [ref: > classes.id]
  session_id uuid [ref: - sessions.id]
  title varchar(255)
  description text
  due_at timestamptz
  book varchar(255)
  pages varchar(50)
  skills varchar(100)
  submission_type varchar(20) // FILE/IMAGE/TEXT/LINK/QUIZ
  max_score numeric
  reward_stars int
  mission_id uuid [ref: - missions.id]
  created_by uuid [ref: - users.id]
  created_at timestamptz
}

Table homework_students {
  id uuid [pk]
  assignment_id uuid [ref: > homework_assignments.id]
  student_profile_id uuid [ref: > profiles.id]
  status varchar(20) // ASSIGNED/SUBMITTED/GRADED/LATE/MISSING
  started_at timestamptz
  submitted_at timestamptz
  graded_at timestamptz
  score numeric
  teacher_feedback text
  ai_feedback jsonb
  text_answer text
  attachment_url text

  Indexes {
    (assignment_id, student_profile_id) [unique]
  }
}

// NEW: table exists in the current project and was missing from this dbdiagram file
Table homework_questions {
  id uuid [pk]
  homework_assignment_id uuid [ref: > homework_assignments.id]
  order_index int
  question_text text
  question_type varchar(20) // MultipleChoice/TextInput
  options jsonb
  correct_answer text
  points int
  explanation text
}

// NEW: table exists in the current project and was missing from this dbdiagram file
Table homework_submission_attempts {
  id uuid [pk]
  homework_student_id uuid [ref: > homework_students.id]
  attempt_number int
  status varchar(20) // Assigned/Submitted/Graded/Late/Missing
  started_at timestamptz
  submitted_at timestamptz
  graded_at timestamptz
  score numeric
  teacher_feedback text
  ai_feedback jsonb
  text_answer text
  attachment_url text
  created_at timestamptz

  Indexes {
    (homework_student_id, attempt_number) [unique]
  }
}

// NEW: table exists in the current project and was missing from this dbdiagram file
Table question_bank_items {
  id uuid [pk]
  program_id uuid [ref: > programs.id]
  question_text text
  question_type varchar(20) // MultipleChoice/TextInput
  options jsonb
  correct_answer text
  points int
  explanation text
  topic varchar(100)
  skill varchar(100)
  grammar_tags jsonb
  vocabulary_tags jsonb
  level varchar(10) // Easy/Medium/Hard
  created_by uuid
  created_at timestamptz
  updated_at timestamptz

  Indexes {
    (program_id, level)
  }
}

Table exams {
  id uuid [pk]
  class_id uuid [ref: > classes.id]
  exam_type varchar(30) // PLACEMENT/PROGRESS/MIDTERM/FINAL/SPEAKING
  date date
  max_score numeric
  description text
  // Thời gian thi (cho thi tại trung tâm)
  scheduled_start_time timestamptz // Thời gian bắt đầu thi (theo lịch)
  time_limit_minutes int // Thời gian làm bài (phút)
  allow_late_start boolean // Cho phép bắt đầu muộn
  late_start_tolerance_minutes int // Cho phép muộn tối đa bao nhiêu phút
  // Settings
  auto_submit_on_time_limit boolean // Tự động nộp khi hết giờ
  prevent_copy_paste boolean // Prevent copy/paste (frontend)
  prevent_navigation boolean // Prevent mở tab khác (frontend)
  show_results_immediately boolean // Hiển thị kết quả ngay sau khi nộp (cho multiple choice)
  created_by uuid [ref: - users.id]
  created_at timestamptz
  updated_at timestamptz
}

Table exam_questions {
  id uuid [pk]
  exam_id uuid [ref: > exams.id]
  order_index int // Order of question in the exam
  question_text text
  question_type varchar(20) // MULTIPLE_CHOICE/TEXT_INPUT
  options jsonb // JSON array for multiple choice options
  correct_answer text // Correct answer (for multiple choice: option index, for text: answer text)
  points int // Points awarded for correct answer
  explanation text // Explanation of the answer
  created_at timestamptz
  updated_at timestamptz
}

Table exam_submissions {
  id uuid [pk]
  exam_id uuid [ref: > exams.id]
  student_profile_id uuid [ref: > profiles.id]
  // Thời gian thi
  actual_start_time timestamptz // Thời gian học sinh bắt đầu làm (tại trung tâm)
  submitted_at timestamptz // Thời gian nộp bài
  auto_submitted_at timestamptz // Thời gian auto-submit (nếu hết giờ)
  time_spent_minutes int // Thời gian làm bài thực tế
  // Điểm số
  auto_score numeric // Điểm tự động (multiple choice)
  final_score numeric // Điểm cuối cùng (sau khi teacher chấm)
  // Chấm bài
  graded_by uuid [ref: - users.id]
  graded_at timestamptz
  teacher_comment text
  // Trạng thái
  status varchar(20) // NOT_STARTED/IN_PROGRESS/SUBMITTED/AUTO_SUBMITTED/GRADED
}

Table exam_submission_answers {
  id uuid [pk]
  submission_id uuid [ref: > exam_submissions.id]
  question_id uuid [ref: > exam_questions.id]
  answer text // Student's answer
  is_correct boolean // Whether answer is correct (for auto-graded questions)
  points_awarded numeric // Points awarded for this answer
  teacher_feedback text // Teacher feedback (for writing questions)
  answered_at timestamptz // Thời gian trả lời
  
  Indexes {
    (submission_id, question_id) [unique]
  }
}

Table exam_results {
  id uuid [pk]
  exam_id uuid [ref: > exams.id]
  student_profile_id uuid [ref: > profiles.id]
  score numeric
  comment text
  attachment_urls jsonb // JSON array of image URLs (changed from attachment_url string)
  graded_by uuid [ref: - users.id]
  graded_at timestamptz
  created_at timestamptz
  updated_at timestamptz
}

Table monthly_report_jobs {
  id uuid [pk]
  month int
  year int
  branch_id uuid [ref: > branches.id]
  status varchar(20) // PENDING/GENERATING/DONE/FAILED
  started_at timestamptz
  finished_at timestamptz
  ai_payload_ref text
}

Table session_reports {
  id uuid [pk]
  session_id uuid [ref: > sessions.id]
  student_profile_id uuid [ref: > profiles.id]
  teacher_user_id uuid [ref: > users.id]
  report_date date // date of session for filtering monthly reports
  feedback text // teacher's feedback/notes for the student
  ai_generated_summary text // AI-generated summary (for monthly compilation)
  is_monthly_compiled boolean // whether included in monthly report
  created_at timestamptz
  updated_at timestamptz
  
  Indexes {
    (session_id, student_profile_id) [unique] // one report per student per session
    (teacher_user_id, report_date) // for filtering by teacher and date range
  }
}

Table student_monthly_reports {
  id uuid [pk]
  student_profile_id uuid [ref: > profiles.id]
  month int
  year int
  draft_content jsonb
  final_content jsonb
  status varchar(20) // DRAFT/REVIEW/APPROVED/REJECTED
  ai_version varchar(50)
  submitted_by uuid [ref: - users.id]
  reviewed_by uuid [ref: - users.id]
  reviewed_at timestamptz
  published_at timestamptz
}

// NEW: table exists in the current project and was missing from this dbdiagram file
Table monthly_report_data {
  id uuid [pk]
  report_id uuid [ref: > student_monthly_reports.id]
  student_profile_id uuid [ref: > profiles.id]
  month int
  year int
  attendance_data jsonb
  homework_data jsonb
  test_data jsonb
  mission_data jsonb
  notes_data jsonb
  topics_data jsonb
  created_at timestamptz
  updated_at timestamptz

  Indexes {
    (report_id) [unique]
  }
}

Table report_comments {
  id uuid [pk]
  report_id uuid [ref: > student_monthly_reports.id]
  commenter_id uuid [ref: > users.id]
  content text
  created_at timestamptz
}

// NEW: table exists in the current project and was missing from this dbdiagram file
Table report_requests {
  id uuid [pk]
  report_type varchar(20) // Session/Monthly
  status varchar(20) // Requested/InProgress/Submitted/Approved/Rejected/Cancelled
  priority varchar(20) // Low/Normal/High/Urgent
  assigned_teacher_user_id uuid [ref: > users.id]
  requested_by_user_id uuid [ref: > users.id]
  target_student_profile_id uuid [ref: - profiles.id]
  target_class_id uuid [ref: - classes.id]
  target_session_id uuid [ref: - sessions.id]
  month int
  year int
  message varchar(1000)
  due_at timestamptz
  linked_session_report_id uuid [ref: - session_reports.id]
  linked_monthly_report_id uuid [ref: - student_monthly_reports.id]
  submitted_at timestamptz
  created_at timestamptz
  updated_at timestamptz

  Indexes {
    (assigned_teacher_user_id, status, priority, due_at)
    (report_type, target_class_id, month, year)
    (report_type, target_student_profile_id, month, year)
  }
}

Table missions {
  id uuid [pk]
  title varchar(255)
  description text
  scope varchar(20) // CLASS/STUDENT/GROUP
  target_class_id uuid [ref: - classes.id]
  target_student_id uuid // NEW FIELD: no FK configured in EF
  target_group jsonb // optional grouping
  mission_type varchar(50) // HomeworkStreak/ReadingStreak/NoUnexcusedAbsence/ClassAttendance/Custom
  progress_mode varchar(20) // NEW FIELD: Count/Streak
  start_at timestamptz
  end_at timestamptz
  reward_stars int
  reward_exp int // experience points reward
  total_required int // NEW FIELD
  created_by uuid [ref: - users.id]
  created_at timestamptz
}

// NEW: table exists in the current project and was missing from this dbdiagram file
Table mission_reward_rules {
  id uuid [pk]
  mission_type varchar(50)
  progress_mode varchar(20) // Count/Streak
  total_required int
  reward_stars int
  reward_exp int
  is_active boolean
  created_at timestamptz
  updated_at timestamptz

  Indexes {
    (mission_type, progress_mode, total_required) [unique]
  }
}

Table mission_progresses {
  id uuid [pk]
  mission_id uuid [ref: > missions.id]
  student_profile_id uuid [ref: > profiles.id]
  status varchar(20) // ASSIGNED/IN_PROGRESS/COMPLETED/EXPIRED
  progress_value numeric
  completed_at timestamptz
  verified_by uuid [ref: - users.id]

  Indexes {
    (mission_id, student_profile_id) [unique]
  }
}

Table star_transactions {
  id uuid [pk]
  student_profile_id uuid [ref: > profiles.id]
  amount int // positive or negative
  reason varchar(100)
  source_type varchar(30) // MISSION/MANUAL/HOMEWORK/TEST/ADJUSTMENT/ATTENDANCE
  source_id uuid
  balance_after int
  created_by uuid [ref: - users.id]
  created_at timestamptz
}

Table student_levels {
  id uuid [pk]
  student_profile_id uuid [ref: > profiles.id]
  current_level varchar(50)
  current_xp int
  updated_at timestamptz
}

Table attendance_streaks {
  id uuid [pk]
  student_profile_id uuid [ref: > profiles.id]
  attendance_date date
  current_streak int // current consecutive days streak
  reward_stars int // stars awarded (e.g., 1)
  reward_exp int // exp awarded (e.g., 5)
  created_at timestamptz
  
  Indexes {
    (student_profile_id, attendance_date) [unique]
  }
}

// NEW: table exists in the current project and was missing from this dbdiagram file
Table gamification_settings {
  id int [pk]
  check_in_reward_stars int
  check_in_reward_exp int
  created_at timestamptz
  updated_at timestamptz
}

Table reward_store_items {
  id uuid [pk]
  title varchar(255)
  image_url text
  description text
  cost_stars int
  quantity int
  is_active boolean
  is_deleted boolean
  created_at timestamptz
  updated_at timestamptz // NEW FIELD
}

Table reward_redemptions {
  id uuid [pk]
  item_id uuid [ref: > reward_store_items.id]
  item_name varchar(255) // store item name at redemption time
  quantity int // NEW FIELD
  stars_deducted int // NEW FIELD
  student_profile_id uuid [ref: > profiles.id]
  status varchar(20) // REQUESTED/APPROVED/DELIVERED/RECEIVED/CANCELLED
  cancel_reason varchar(1000) // NEW FIELD
  handled_by uuid [ref: - users.id]
  handled_at timestamptz
  delivered_at timestamptz // when staff delivered the reward
  received_at timestamptz // when student confirmed or auto after 3 days
  created_at timestamptz
}

Table leads {
  id uuid [pk]
  source varchar(30) // LANDING/ZALO/REFERRAL/OFFLINE
  contact_name varchar(255)
  phone varchar(50)
  zalo_id varchar(100)
  email varchar(255)
  company varchar(255) // Công ty/Trường học
  subject varchar(255) // Tiêu đề form
  branch_preference uuid [ref: - branches.id]
  program_interest varchar(255)
  notes text
  status varchar(30) // NEW/CONTACTED/BOOKED_TEST/TEST_DONE/ENROLLED/LOST
  owner_staff_id uuid [ref: - users.id]
  first_response_at timestamptz // SLA phản hồi đầu tiên
  touch_count int // số lần chạm
  next_action_at timestamptz // lịch follow-up tiếp theo
  converted_student_profile_id uuid [ref: - profiles.id] // lưu trace khi ghi danh
  converted_at timestamptz
  created_at timestamptz
  updated_at timestamptz
}

// NEW: table exists in the current project and was missing from this dbdiagram file
Table lead_children {
  id uuid [pk]
  lead_id uuid [ref: > leads.id]
  child_name varchar(255)
  dob date
  gender varchar(20)
  program_interest varchar(255)
  notes text
  status varchar(30) // New/BookedTest/TestDone/Enrolled/Lost
  converted_student_profile_id uuid [ref: - profiles.id]
  created_at timestamptz
  updated_at timestamptz
}

Table placement_tests {
  id uuid [pk]
  lead_id uuid [ref: - leads.id]
  lead_child_id uuid [ref: - lead_children.id] // NEW FIELD
  student_profile_id uuid [ref: - profiles.id]
  class_id uuid [ref: - classes.id]
  scheduled_at timestamptz
  status varchar(20) // SCHEDULED/NO_SHOW/COMPLETED/CANCELLED
  room varchar(100)
  invigilator_user_id uuid [ref: - users.id] // teacher/staff user
  original_placement_test_id uuid [ref: - placement_tests.id] // NEW FIELD
  result_score numeric
  listening_score numeric
  speaking_score numeric
  reading_score numeric
  writing_score numeric
  level_recommendation varchar(100)
  program_recommendation_id uuid [ref: - programs.id] // NEW FIELD
  secondary_program_recommendation_id uuid [ref: - programs.id] // NEW FIELD
  secondary_program_skill_focus varchar(50) // NEW FIELD
  notes text
  attachment_url text
  created_at timestamptz // NEW FIELD
  updated_at timestamptz // NEW FIELD
}

Table lead_activities {
  id uuid [pk]
  lead_id uuid [ref: - leads.id]
  activity_type varchar(20) // CALL/ZALO/SMS/EMAIL/NOTE
  content text
  next_action_at timestamptz
  created_by uuid [ref: - users.id]
  created_at timestamptz
}

Table media_assets {
  id uuid [pk]
  uploader_id uuid [ref: > users.id]
  branch_id uuid [ref: > branches.id]
  class_id uuid [ref: - classes.id]
  student_profile_id uuid [ref: - profiles.id]
  month_tag varchar(7) // YYYY-MM
  type varchar(10) // PHOTO/VIDEO
  url text
  caption text
  visibility varchar(20) // CLASS_ONLY/PERSONAL/PUBLIC_PARENT
  created_at timestamptz
}

Table blogs {
  id uuid [pk]
  title varchar(255)
  summary varchar(500) // short summary for preview
  content text // full blog content (can be HTML/Markdown)
  featured_image_url varchar(500)
  created_by uuid [ref: > users.id] // admin/staff
  is_published boolean // whether published on landing page
  is_deleted boolean
  published_at timestamptz
  created_at timestamptz
  updated_at timestamptz
}

Table tuition_plans {
  id uuid [pk]
  branch_id uuid [ref: - branches.id] // nullable: null = applies to all branches
  program_id uuid [ref: > programs.id]
  name varchar(255) // name of the tuition plan (e.g., "Gói 3 tháng - Ưu đãi")
  total_sessions int
  tuition_amount numeric
  unit_price_session numeric
  currency varchar(10)
  is_active boolean
  is_deleted boolean
  created_at timestamptz
  updated_at timestamptz
}

Table notifications {
  id uuid [pk]
  recipient_user_id uuid [ref: > users.id] // user (can be any role)
  recipient_profile_id uuid [ref: - profiles.id] // optional: for PARENT/STUDENT profiles
  channel varchar(20) // INAPP/ZALO_OA/PUSH/EMAIL
  title varchar(255)
  content text
  deeplink text
  status varchar(20) // PENDING/SENT/FAILED
  sent_at timestamptz
  read_at timestamptz // Thời gian user đọc notification
  template_id varchar(100)
  created_at timestamptz
}

Table notification_templates {
  id uuid [pk]
  code varchar(100) [unique]
  channel varchar(20) // ZALO_OA/PUSH/EMAIL
  title varchar(255)
  content text
  placeholders jsonb
  is_active boolean
  is_deleted boolean
  created_at timestamptz
  updated_at timestamptz
}

Table tickets {
  id uuid [pk]
  opened_by_user_id uuid [ref: > users.id] // user (can be any role)
  opened_by_profile_id uuid [ref: - profiles.id] // optional: for PARENT/STUDENT profiles
  branch_id uuid [ref: > branches.id]
  class_id uuid [ref: - classes.id]
  category varchar(30) // HOMEWORK/FINANCE/SCHEDULE/TECH
  subject varchar(200) // Subject/title of the ticket
  message text
  status varchar(20) // OPEN/IN_PROGRESS/RESOLVED/CLOSED
  assigned_to_user_id uuid [ref: - users.id]
  created_at timestamptz
  updated_at timestamptz
}

Table ticket_comments {
  id uuid [pk]
  ticket_id uuid [ref: > tickets.id]
  commenter_user_id uuid [ref: > users.id] // user (can be any role)
  commenter_profile_id uuid [ref: - profiles.id] // optional: for PARENT/STUDENT profiles
  message text
  attachment_url text
  created_at timestamptz
}

Table audit_logs {
  id uuid [pk]
  actor_user_id uuid [ref: - users.id] // user (can be any role)
  actor_profile_id uuid [ref: - profiles.id] // optional: for PARENT/STUDENT profiles
  action varchar(100)
  entity_type varchar(100)
  entity_id uuid
  data_before jsonb
  data_after jsonb
  created_at timestamptz
}


