# Ho so ban giao Backend Kidzgo

Tai lieu nay tong hop cac noi dung can nop cho phan Backend (BE) cua de tai, duoc kiem tra truc tiep tu source code trong thu muc `Kidzgo_Be`.

## 1. Tong quan pham vi BE

- Nen tang: ASP.NET Core Web API tren `.NET 9`.
- Kien truc ma nguon: `Kidzgo.API` + `Kidzgo.Application` + `Kidzgo.Domain` + `Kidzgo.Infrastructure`.
- Co so du lieu: `PostgreSQL` thong qua `Entity Framework Core` va `Npgsql`.
- So controller hien co: `51`.
- So module cap 1 trong `Kidzgo.Application`: `49`.
- So background jobs trong `Kidzgo.Infrastructure/BackgroundJobs`: `7`.

## 2. Database

### 2.1. Script/artefact tao cac bang trong database

create table public."__EFMigrationsHistory"
(
    "MigrationId"    varchar(150) not null
        constraint "PK___EFMigrationsHistory"
            primary key,
    "ProductVersion" varchar(32)  not null
);

alter table public."__EFMigrationsHistory"
    owner to postgres;

create table public."Branches"
(
    "Id"           uuid                     not null
        constraint "PK_Branches"
            primary key,
    "Code"         varchar(32)              not null,
    "Name"         varchar(255)             not null,
    "Address"      text,
    "ContactPhone" varchar(32),
    "ContactEmail" varchar(255),
    "IsActive"     boolean                  not null,
    "CreatedAt"    timestamp with time zone not null,
    "UpdatedAt"    timestamp with time zone not null
);

alter table public."Branches"
    owner to postgres;

create unique index "IX_Branches_Code"
    on public."Branches" ("Code");

create table public."EmailTemplates"
(
    "Id"           uuid                     not null
        constraint "PK_EmailTemplates"
            primary key,
    "Code"         varchar(100)             not null,
    "Subject"      varchar(255)             not null,
    "Body"         text,
    "Placeholders" jsonb,
    "IsActive"     boolean                  not null,
    "IsDeleted"    boolean                  not null,
    "CreatedAt"    timestamp with time zone not null,
    "UpdatedAt"    timestamp with time zone not null
);

alter table public."EmailTemplates"
    owner to postgres;

create unique index "IX_EmailTemplates_Code"
    on public."EmailTemplates" ("Code");

create table public."NotificationTemplates"
(
    "Id"           uuid                     not null
        constraint "PK_NotificationTemplates"
            primary key,
    "Code"         varchar(100)             not null,
    "Channel"      varchar(20)              not null,
    "Title"        varchar(255)             not null,
    "Content"      text,
    "Placeholders" jsonb,
    "IsActive"     boolean                  not null,
    "IsDeleted"    boolean                  not null,
    "CreatedAt"    timestamp with time zone not null,
    "UpdatedAt"    timestamp with time zone not null,
    "Category"     varchar(100)
);

alter table public."NotificationTemplates"
    owner to postgres;

create unique index "IX_NotificationTemplates_Code"
    on public."NotificationTemplates" ("Code");

create table public."RewardStoreItems"
(
    "Id"          uuid                                                                   not null
        constraint "PK_RewardStoreItems"
            primary key,
    "Title"       varchar(255)                                                           not null,
    "Description" text,
    "CostStars"   integer                                                                not null,
    "Quantity"    integer                                                                not null,
    "IsActive"    boolean                                                                not null,
    "IsDeleted"   boolean                                                                not null,
    "CreatedAt"   timestamp with time zone                                               not null,
    "ImageUrl"    varchar(500),
    "UpdatedAt"   timestamp with time zone default '-infinity'::timestamp with time zone not null
);

alter table public."RewardStoreItems"
    owner to postgres;

create table public."Classrooms"
(
    "Id"            uuid         not null
        constraint "PK_Classrooms"
            primary key,
    "BranchId"      uuid         not null
        constraint "FK_Classrooms_Branches_BranchId"
            references public."Branches"
            on delete cascade,
    "Name"          varchar(100) not null,
    "Capacity"      integer      not null,
    "Note"          text,
    "IsActive"      boolean      not null,
    "Area"          numeric,
    "EquipmentJson" text,
    "Floor"         varchar(50)
);

alter table public."Classrooms"
    owner to postgres;

create index "IX_Classrooms_BranchId"
    on public."Classrooms" ("BranchId");

create table public."Programs"
(
    "Id"              uuid                                      not null
        constraint "PK_Programs"
            primary key,
    "Name"            varchar(255)                              not null,
    "Description"     text,
    "IsActive"        boolean                                   not null,
    "IsDeleted"       boolean                                   not null,
    "CreatedAt"       timestamp with time zone                  not null,
    "UpdatedAt"       timestamp with time zone                  not null,
    "Code"            varchar(10) default ''::character varying not null,
    "IsMakeup"        boolean     default false                 not null,
    "IsSupplementary" boolean     default false                 not null
);

alter table public."Programs"
    owner to postgres;

create table public."Users"
(
    "Id"                      uuid                     not null
        constraint "PK_Users"
            primary key,
    "Email"                   varchar(255)             not null,
    "PasswordHash"            varchar(255)             not null,
    "Role"                    varchar(20)              not null,
    "Username"                varchar(100),
    "Name"                    varchar(255),
    "PinHash"                 varchar(255),
    "BranchId"                uuid
        constraint "FK_Users_Branches_BranchId"
            references public."Branches"
            on delete restrict,
    "IsActive"                boolean                  not null,
    "IsDeleted"               boolean                  not null,
    "CreatedAt"               timestamp with time zone not null,
    "UpdatedAt"               timestamp with time zone not null,
    "PhoneNumber"             varchar(50),
    "AvatarFileSize"          bigint,
    "AvatarMimeType"          text,
    "AvatarUrl"               text,
    "LastLoginAt"             timestamp with time zone,
    "LastSeenAt"              timestamp with time zone,
    "TeacherCompensationType" varchar(30)
);

alter table public."Users"
    owner to postgres;

create table public."MonthlyReportJobs"
(
    "Id"           uuid                                                                   not null
        constraint "PK_MonthlyReportJobs"
            primary key,
    "Month"        integer                                                                not null,
    "Year"         integer                                                                not null,
    "BranchId"     uuid                                                                   not null
        constraint "FK_MonthlyReportJobs_Branches_BranchId"
            references public."Branches"
            on delete restrict,
    "Status"       varchar(20)                                                            not null,
    "StartedAt"    timestamp with time zone,
    "FinishedAt"   timestamp with time zone,
    "AiPayloadRef" text,
    "CreatedAt"    timestamp with time zone default '-infinity'::timestamp with time zone not null,
    "CreatedBy"    uuid
        constraint "FK_MonthlyReportJobs_Users_CreatedBy"
            references public."Users"
            on delete restrict,
    "ErrorMessage" varchar(1000),
    "RetryCount"   integer                  default 0                                     not null,
    "UpdatedAt"    timestamp with time zone default '-infinity'::timestamp with time zone not null
);

alter table public."MonthlyReportJobs"
    owner to postgres;

create index "IX_MonthlyReportJobs_BranchId"
    on public."MonthlyReportJobs" ("BranchId");

create index "IX_MonthlyReportJobs_CreatedBy"
    on public."MonthlyReportJobs" ("CreatedBy");

create index "IX_Users_BranchId"
    on public."Users" ("BranchId");

create unique index "IX_Users_Email"
    on public."Users" ("Email");

create table public."TuitionPlans"
(
    "Id"               uuid                     not null
        constraint "PK_TuitionPlans"
            primary key,
    "BranchId"         uuid
        constraint "FK_TuitionPlans_Branches_BranchId"
            references public."Branches"
            on delete restrict,
    "ProgramId"        uuid                     not null
        constraint "FK_TuitionPlans_Programs_ProgramId"
            references public."Programs"
            on delete restrict,
    "Name"             varchar(255)             not null,
    "TotalSessions"    integer                  not null,
    "TuitionAmount"    numeric                  not null,
    "UnitPriceSession" numeric                  not null,
    "Currency"         varchar(10)              not null,
    "IsActive"         boolean                  not null,
    "IsDeleted"        boolean                  not null,
    "CreatedAt"        timestamp with time zone not null,
    "UpdatedAt"        timestamp with time zone not null
);

alter table public."TuitionPlans"
    owner to postgres;

create index "IX_TuitionPlans_BranchId"
    on public."TuitionPlans" ("BranchId");

create index "IX_TuitionPlans_ProgramId"
    on public."TuitionPlans" ("ProgramId");

create table public."Blogs"
(
    "Id"                 uuid                     not null
        constraint "PK_Blogs"
            primary key,
    "Title"              varchar(255)             not null,
    "Summary"            varchar(500),
    "Content"            text                     not null,
    "FeaturedImageUrl"   varchar(500),
    "CreatedBy"          uuid                     not null
        constraint "FK_Blogs_Users_CreatedBy"
            references public."Users"
            on delete restrict,
    "IsPublished"        boolean                  not null,
    "IsDeleted"          boolean                  not null,
    "PublishedAt"        timestamp with time zone,
    "CreatedAt"          timestamp with time zone not null,
    "UpdatedAt"          timestamp with time zone not null,
    "AttachmentFileUrl"  varchar(500),
    "AttachmentImageUrl" varchar(500)
);

alter table public."Blogs"
    owner to postgres;

create index blog_published_idx
    on public."Blogs" ("IsPublished", "PublishedAt");

create index "IX_Blogs_CreatedBy"
    on public."Blogs" ("CreatedBy");

create table public."CashbookEntries"
(
    "Id"            uuid                     not null
        constraint "PK_CashbookEntries"
            primary key,
    "BranchId"      uuid                     not null
        constraint "FK_CashbookEntries_Branches_BranchId"
            references public."Branches"
            on delete restrict,
    "Type"          varchar(10)              not null,
    "Amount"        numeric                  not null,
    "Currency"      varchar(10)              not null,
    "Description"   text,
    "RelatedType"   varchar(30),
    "RelatedId"     uuid,
    "EntryDate"     date                     not null,
    "CreatedBy"     uuid
        constraint "FK_CashbookEntries_Users_CreatedBy"
            references public."Users"
            on delete restrict,
    "AttachmentUrl" text,
    "CreatedAt"     timestamp with time zone not null
);

alter table public."CashbookEntries"
    owner to postgres;

create index "IX_CashbookEntries_BranchId"
    on public."CashbookEntries" ("BranchId");

create index "IX_CashbookEntries_CreatedBy"
    on public."CashbookEntries" ("CreatedBy");

create table public."Classes"
(
    "Id"                 uuid                     not null
        constraint "PK_Classes"
            primary key,
    "BranchId"           uuid                     not null
        constraint "FK_Classes_Branches_BranchId"
            references public."Branches"
            on delete restrict,
    "ProgramId"          uuid                     not null
        constraint "FK_Classes_Programs_ProgramId"
            references public."Programs"
            on delete restrict,
    "Code"               varchar(50)              not null,
    "Title"              varchar(255)             not null,
    "MainTeacherId"      uuid
        constraint "FK_Classes_Users_MainTeacherId"
            references public."Users"
            on delete restrict,
    "AssistantTeacherId" uuid
        constraint "FK_Classes_Users_AssistantTeacherId"
            references public."Users"
            on delete restrict,
    "StartDate"          date                     not null,
    "EndDate"            date,
    "Status"             varchar(20)              not null,
    "Capacity"           integer                  not null,
    "SchedulePattern"    text,
    "CreatedAt"          timestamp with time zone not null,
    "UpdatedAt"          timestamp with time zone not null,
    "Description"        text,
    "RoomId"             uuid
        constraint "FK_Classes_Classrooms_RoomId"
            references public."Classrooms"
            on delete set null
);

alter table public."Classes"
    owner to postgres;

create index "IX_Classes_AssistantTeacherId"
    on public."Classes" ("AssistantTeacherId");

create index "IX_Classes_BranchId"
    on public."Classes" ("BranchId");

create unique index "IX_Classes_Code"
    on public."Classes" ("Code");

create index "IX_Classes_MainTeacherId"
    on public."Classes" ("MainTeacherId");

create index "IX_Classes_ProgramId"
    on public."Classes" ("ProgramId");

create index "IX_Classes_RoomId"
    on public."Classes" ("RoomId");

create table public."Contracts"
(
    "Id"                     uuid        not null
        constraint "PK_Contracts"
            primary key,
    "StaffUserId"            uuid        not null
        constraint "FK_Contracts_Users_StaffUserId"
            references public."Users"
            on delete restrict,
    "ContractType"           varchar(20) not null,
    "StartDate"              date        not null,
    "EndDate"                date,
    "BaseSalary"             numeric,
    "HourlyRate"             numeric,
    "AllowanceFixed"         numeric,
    "MinimumMonthlyHours"    numeric,
    "OvertimeRateMultiplier" numeric,
    "BranchId"               uuid        not null
        constraint "FK_Contracts_Branches_BranchId"
            references public."Branches"
            on delete restrict,
    "IsActive"               boolean     not null
);

alter table public."Contracts"
    owner to postgres;

create index "IX_Contracts_BranchId"
    on public."Contracts" ("BranchId");

create index "IX_Contracts_StaffUserId"
    on public."Contracts" ("StaffUserId");

create table public."LessonPlanTemplates"
(
    "Id"                         uuid                                       not null
        constraint "PK_LessonPlanTemplates"
            primary key,
    "ProgramId"                  uuid                                       not null
        constraint "FK_LessonPlanTemplates_Programs_ProgramId"
            references public."Programs"
            on delete restrict,
    "Level"                      varchar(100),
    "SessionIndex"               integer                                    not null,
    "IsActive"                   boolean                                    not null,
    "IsDeleted"                  boolean                                    not null,
    "CreatedBy"                  uuid
        constraint "FK_LessonPlanTemplates_Users_CreatedBy"
            references public."Users"
            on delete restrict,
    "CreatedAt"                  timestamp with time zone                   not null,
    "AttachmentUrl"              varchar(500) default ''::text,
    "AttachmentFileSize"         bigint,
    "AttachmentMimeType"         varchar(100),
    "AttachmentOriginalFileName" varchar(255),
    "Title"                      varchar(255) default ''::character varying not null,
    "SourceFileName"             varchar(255),
    "SyllabusContent"            text,
    "SyllabusMetadata"           text
);

alter table public."LessonPlanTemplates"
    owner to postgres;

create index "IX_LessonPlanTemplates_CreatedBy"
    on public."LessonPlanTemplates" ("CreatedBy");

create index "IX_LessonPlanTemplates_ProgramId"
    on public."LessonPlanTemplates" ("ProgramId");

create table public."PasswordResetTokens"
(
    "Id"        uuid                     not null
        constraint "PK_PasswordResetTokens"
            primary key,
    "UserId"    uuid                     not null
        constraint "FK_PasswordResetTokens_Users_UserId"
            references public."Users"
            on delete cascade,
    "Token"     varchar(200)             not null,
    "ExpiresAt" timestamp with time zone not null,
    "UsedAt"    timestamp with time zone
);

alter table public."PasswordResetTokens"
    owner to postgres;

create index "IX_PasswordResetTokens_UserId"
    on public."PasswordResetTokens" ("UserId");

create table public."PayrollRuns"
(
    "Id"          uuid                     not null
        constraint "PK_PayrollRuns"
            primary key,
    "PeriodStart" date                     not null,
    "PeriodEnd"   date                     not null,
    "BranchId"    uuid                     not null
        constraint "FK_PayrollRuns_Branches_BranchId"
            references public."Branches"
            on delete restrict,
    "Status"      varchar(20)              not null,
    "ApprovedBy"  uuid
        constraint "FK_PayrollRuns_Users_ApprovedBy"
            references public."Users"
            on delete restrict,
    "PaidAt"      timestamp with time zone,
    "CreatedAt"   timestamp with time zone not null
);

alter table public."PayrollRuns"
    owner to postgres;

create index "IX_PayrollRuns_ApprovedBy"
    on public."PayrollRuns" ("ApprovedBy");

create index "IX_PayrollRuns_BranchId"
    on public."PayrollRuns" ("BranchId");

create table public."Profiles"
(
    "Id"             uuid                     not null
        constraint "PK_Profiles"
            primary key,
    "UserId"         uuid                     not null
        constraint "FK_Profiles_Users_UserId"
            references public."Users"
            on delete cascade,
    "ProfileType"    varchar(20)              not null,
    "DisplayName"    varchar(255)             not null,
    "PinHash"        varchar(97),
    "IsActive"       boolean                  not null,
    "IsDeleted"      boolean                  not null,
    "CreatedAt"      timestamp with time zone not null,
    "UpdatedAt"      timestamp with time zone not null,
    "AvatarFileSize" bigint,
    "AvatarMimeType" text,
    "AvatarUrl"      text,
    "DateOfBirth"    date,
    "Name"           varchar(255),
    "Gender"         text,
    "IsApproved"     boolean default false    not null,
    "ZaloId"         text,
    "LastLoginAt"    timestamp with time zone,
    "LastSeenAt"     timestamp with time zone
);

alter table public."Profiles"
    owner to postgres;

create index "IX_Profiles_UserId"
    on public."Profiles" ("UserId");

create table public."RefreshTokens"
(
    "Id"      uuid                     not null
        constraint "PK_RefreshTokens"
            primary key,
    "Token"   text                     not null,
    "UserId"  uuid                     not null
        constraint "FK_RefreshTokens_Users_UserId"
            references public."Users"
            on delete cascade,
    "Expires" timestamp with time zone not null
);

alter table public."RefreshTokens"
    owner to postgres;

create unique index "IX_RefreshTokens_Token"
    on public."RefreshTokens" ("Token");

create index "IX_RefreshTokens_UserId"
    on public."RefreshTokens" ("UserId");

create table public."Exams"
(
    "Id"                        uuid                                                                   not null
        constraint "PK_Exams"
            primary key,
    "ClassId"                   uuid                                                                   not null
        constraint "FK_Exams_Classes_ClassId"
            references public."Classes"
            on delete cascade,
    "ExamType"                  varchar(30)                                                            not null,
    "Date"                      date                                                                   not null,
    "MaxScore"                  numeric,
    "Description"               text,
    "CreatedBy"                 uuid
        constraint "FK_Exams_Users_CreatedBy"
            references public."Users"
            on delete restrict,
    "CreatedAt"                 timestamp with time zone default '-infinity'::timestamp with time zone not null,
    "UpdatedAt"                 timestamp with time zone default '-infinity'::timestamp with time zone not null,
    "AllowLateStart"            boolean                  default false                                 not null,
    "AutoSubmitOnTimeLimit"     boolean                  default false                                 not null,
    "LateStartToleranceMinutes" integer,
    "PreventCopyPaste"          boolean                  default false                                 not null,
    "PreventNavigation"         boolean                  default false                                 not null,
    "ScheduledStartTime"        timestamp with time zone,
    "ShowResultsImmediately"    boolean                  default false                                 not null,
    "TimeLimitMinutes"          integer
);

alter table public."Exams"
    owner to postgres;

create index "IX_Exams_ClassId"
    on public."Exams" ("ClassId");

create index "IX_Exams_CreatedBy"
    on public."Exams" ("CreatedBy");

create table public."Missions"
(
    "Id"              uuid                                           not null
        constraint "PK_Missions"
            primary key,
    "Title"           varchar(255)                                   not null,
    "Description"     text,
    "Scope"           varchar(20)                                    not null,
    "TargetClassId"   uuid
        constraint "FK_Missions_Classes_TargetClassId"
            references public."Classes"
            on delete restrict,
    "TargetGroup"     jsonb,
    "MissionType"     varchar(50)                                    not null,
    "StartAt"         timestamp with time zone,
    "EndAt"           timestamp with time zone,
    "RewardStars"     integer,
    "RewardExp"       integer,
    "CreatedBy"       uuid
        constraint "FK_Missions_Users_CreatedBy"
            references public."Users"
            on delete restrict,
    "CreatedAt"       timestamp with time zone                       not null,
    "TotalRequired"   integer,
    "TargetStudentId" uuid,
    "ProgressMode"    varchar(20) default 'Count'::character varying not null
);

alter table public."Missions"
    owner to postgres;

create index "IX_Missions_CreatedBy"
    on public."Missions" ("CreatedBy");

create index "IX_Missions_TargetClassId"
    on public."Missions" ("TargetClassId");

create table public."Sessions"
(
    "Id"                 uuid                     not null
        constraint "PK_Sessions"
            primary key,
    "ClassId"            uuid                     not null
        constraint "FK_Sessions_Classes_ClassId"
            references public."Classes"
            on delete cascade,
    "BranchId"           uuid                     not null
        constraint "FK_Sessions_Branches_BranchId"
            references public."Branches"
            on delete restrict,
    "PlannedDatetime"    timestamp with time zone not null,
    "PlannedRoomId"      uuid
        constraint "FK_Sessions_Classrooms_PlannedRoomId"
            references public."Classrooms"
            on delete restrict,
    "PlannedTeacherId"   uuid
        constraint "FK_Sessions_Users_PlannedTeacherId"
            references public."Users"
            on delete restrict,
    "PlannedAssistantId" uuid
        constraint "FK_Sessions_Users_PlannedAssistantId"
            references public."Users"
            on delete restrict,
    "DurationMinutes"    integer                  not null,
    "ParticipationType"  varchar(20)              not null,
    "Status"             varchar(20)              not null,
    "ActualDatetime"     timestamp with time zone,
    "ActualRoomId"       uuid
        constraint "FK_Sessions_Classrooms_ActualRoomId"
            references public."Classrooms"
            on delete restrict,
    "ActualTeacherId"    uuid
        constraint "FK_Sessions_Users_ActualTeacherId"
            references public."Users"
            on delete restrict,
    "ActualAssistantId"  uuid
        constraint "FK_Sessions_Users_ActualAssistantId"
            references public."Users"
            on delete restrict,
    "CreatedAt"          timestamp with time zone not null,
    "UpdatedAt"          timestamp with time zone not null,
    "Color"              varchar(50)
);

alter table public."Sessions"
    owner to postgres;

create index "IX_Sessions_ActualAssistantId"
    on public."Sessions" ("ActualAssistantId");

create index "IX_Sessions_ActualRoomId"
    on public."Sessions" ("ActualRoomId");

create index "IX_Sessions_ActualTeacherId"
    on public."Sessions" ("ActualTeacherId");

create index "IX_Sessions_BranchId"
    on public."Sessions" ("BranchId");

create index "IX_Sessions_ClassId"
    on public."Sessions" ("ClassId");

create index "IX_Sessions_PlannedAssistantId"
    on public."Sessions" ("PlannedAssistantId");

create index "IX_Sessions_PlannedRoomId"
    on public."Sessions" ("PlannedRoomId");

create index "IX_Sessions_PlannedTeacherId"
    on public."Sessions" ("PlannedTeacherId");

create table public."MonthlyWorkHours"
(
    "Id"               uuid    not null
        constraint "PK_MonthlyWorkHours"
            primary key,
    "StaffUserId"      uuid    not null
        constraint "FK_MonthlyWorkHours_Users_StaffUserId"
            references public."Users"
            on delete restrict,
    "ContractId"       uuid    not null
        constraint "FK_MonthlyWorkHours_Contracts_ContractId"
            references public."Contracts"
            on delete restrict,
    "BranchId"         uuid    not null
        constraint "FK_MonthlyWorkHours_Branches_BranchId"
            references public."Branches"
            on delete restrict,
    "Year"             integer not null,
    "Month"            integer not null,
    "TotalHours"       numeric not null,
    "TeachingHours"    numeric not null,
    "RegularHours"     numeric not null,
    "OvertimeHours"    numeric not null,
    "TeachingSessions" integer not null,
    "IsLocked"         boolean not null
);

alter table public."MonthlyWorkHours"
    owner to postgres;

create index "IX_MonthlyWorkHours_ContractId"
    on public."MonthlyWorkHours" ("ContractId");

create index monthly_work_hours_payroll_idx
    on public."MonthlyWorkHours" ("BranchId", "Year", "Month", "IsLocked");

create unique index monthly_work_hours_unique
    on public."MonthlyWorkHours" ("StaffUserId", "ContractId", "Year", "Month");

create table public."ShiftAttendances"
(
    "Id"          uuid    not null
        constraint "PK_ShiftAttendances"
            primary key,
    "StaffUserId" uuid    not null
        constraint "FK_ShiftAttendances_Users_StaffUserId"
            references public."Users"
            on delete restrict,
    "ContractId"  uuid
        constraint "FK_ShiftAttendances_Contracts_ContractId"
            references public."Contracts"
            on delete restrict,
    "ShiftDate"   date    not null,
    "ShiftHours"  numeric not null,
    "Role"        varchar(50),
    "ApprovedBy"  uuid
        constraint "FK_ShiftAttendances_Users_ApprovedBy"
            references public."Users"
            on delete restrict,
    "ApprovedAt"  timestamp with time zone
);

alter table public."ShiftAttendances"
    owner to postgres;

create index "IX_ShiftAttendances_ApprovedBy"
    on public."ShiftAttendances" ("ApprovedBy");

create index "IX_ShiftAttendances_ContractId"
    on public."ShiftAttendances" ("ContractId");

create index "IX_ShiftAttendances_StaffUserId"
    on public."ShiftAttendances" ("StaffUserId");

create table public."PayrollLines"
(
    "Id"            uuid        not null
        constraint "PK_PayrollLines"
            primary key,
    "PayrollRunId"  uuid        not null
        constraint "FK_PayrollLines_PayrollRuns_PayrollRunId"
            references public."PayrollRuns"
            on delete cascade,
    "StaffUserId"   uuid        not null
        constraint "FK_PayrollLines_Users_StaffUserId"
            references public."Users"
            on delete restrict,
    "ComponentType" varchar(30) not null,
    "SourceId"      uuid,
    "Amount"        numeric     not null,
    "Description"   text,
    "IsPaid"        boolean     not null,
    "PaidAt"        timestamp with time zone
);

alter table public."PayrollLines"
    owner to postgres;

create index "IX_PayrollLines_PayrollRunId"
    on public."PayrollLines" ("PayrollRunId");

create index "IX_PayrollLines_StaffUserId"
    on public."PayrollLines" ("StaffUserId");

create table public."PayrollPayments"
(
    "Id"              uuid        not null
        constraint "PK_PayrollPayments"
            primary key,
    "PayrollRunId"    uuid        not null
        constraint "FK_PayrollPayments_PayrollRuns_PayrollRunId"
            references public."PayrollRuns"
            on delete cascade,
    "StaffUserId"     uuid        not null
        constraint "FK_PayrollPayments_Users_StaffUserId"
            references public."Users"
            on delete restrict,
    "Amount"          numeric     not null,
    "Method"          varchar(20) not null,
    "PaidAt"          timestamp with time zone,
    "CashbookEntryId" uuid
        constraint "FK_PayrollPayments_CashbookEntries_CashbookEntryId"
            references public."CashbookEntries"
            on delete restrict
);

alter table public."PayrollPayments"
    owner to postgres;

create index "IX_PayrollPayments_CashbookEntryId"
    on public."PayrollPayments" ("CashbookEntryId");

create index "IX_PayrollPayments_PayrollRunId"
    on public."PayrollPayments" ("PayrollRunId");

create index "IX_PayrollPayments_StaffUserId"
    on public."PayrollPayments" ("StaffUserId");

create table public."AttendanceStreaks"
(
    "Id"               uuid                     not null
        constraint "PK_AttendanceStreaks"
            primary key,
    "StudentProfileId" uuid                     not null
        constraint "FK_AttendanceStreaks_Profiles_StudentProfileId"
            references public."Profiles"
            on delete cascade,
    "AttendanceDate"   date                     not null,
    "CurrentStreak"    integer                  not null,
    "RewardStars"      integer                  not null,
    "RewardExp"        integer                  not null,
    "CreatedAt"        timestamp with time zone not null
);

alter table public."AttendanceStreaks"
    owner to postgres;

create unique index attendance_streak_unique
    on public."AttendanceStreaks" ("StudentProfileId", "AttendanceDate");

create table public."AuditLogs"
(
    "Id"             uuid                     not null
        constraint "PK_AuditLogs"
            primary key,
    "ActorUserId"    uuid
        constraint "FK_AuditLogs_Users_ActorUserId"
            references public."Users"
            on delete restrict,
    "ActorProfileId" uuid
        constraint "FK_AuditLogs_Profiles_ActorProfileId"
            references public."Profiles"
            on delete restrict,
    "Action"         varchar(100)             not null,
    "EntityType"     varchar(100)             not null,
    "EntityId"       uuid,
    "DataBefore"     jsonb,
    "DataAfter"      jsonb,
    "CreatedAt"      timestamp with time zone not null,
    "IpAddress"      varchar(64)
);

alter table public."AuditLogs"
    owner to postgres;

create index "IX_AuditLogs_ActorProfileId"
    on public."AuditLogs" ("ActorProfileId");

create index "IX_AuditLogs_ActorUserId"
    on public."AuditLogs" ("ActorUserId");

create table public."Invoices"
(
    "Id"               uuid        not null
        constraint "PK_Invoices"
            primary key,
    "BranchId"         uuid        not null
        constraint "FK_Invoices_Branches_BranchId"
            references public."Branches"
            on delete restrict,
    "StudentProfileId" uuid        not null
        constraint "FK_Invoices_Profiles_StudentProfileId"
            references public."Profiles"
            on delete restrict,
    "ClassId"          uuid
        constraint "FK_Invoices_Classes_ClassId"
            references public."Classes"
            on delete restrict,
    "Type"             varchar(30) not null,
    "Amount"           numeric     not null,
    "Currency"         varchar(10) not null,
    "DueDate"          date,
    "Status"           varchar(20) not null,
    "Description"      text,
    "PayosPaymentLink" text,
    "PayosQr"          text,
    "IssuedAt"         timestamp with time zone,
    "IssuedBy"         uuid
        constraint "FK_Invoices_Users_IssuedBy"
            references public."Users"
            on delete restrict,
    "PayosOrderCode"   bigint
);

alter table public."Invoices"
    owner to postgres;

create index "IX_Invoices_BranchId"
    on public."Invoices" ("BranchId");

create index "IX_Invoices_ClassId"
    on public."Invoices" ("ClassId");

create index "IX_Invoices_IssuedBy"
    on public."Invoices" ("IssuedBy");

create index "IX_Invoices_StudentProfileId"
    on public."Invoices" ("StudentProfileId");

create table public."Leads"
(
    "Id"               uuid                     not null
        constraint "PK_Leads"
            primary key,
    "Source"           varchar(30)              not null,
    "Campaign"         varchar(100),
    "ContactName"      varchar(255)             not null,
    "Phone"            varchar(50),
    "ZaloId"           varchar(100),
    "Email"            varchar(255),
    "BranchPreference" uuid
        constraint "FK_Leads_Branches_BranchPreference"
            references public."Branches"
            on delete restrict,
    "Notes"            text,
    "Status"           varchar(30)              not null,
    "OwnerStaffId"     uuid
        constraint "FK_Leads_Users_OwnerStaffId"
            references public."Users"
            on delete restrict,
    "FirstResponseAt"  timestamp with time zone,
    "TouchCount"       integer                  not null,
    "NextActionAt"     timestamp with time zone,
    "CreatedAt"        timestamp with time zone not null,
    "UpdatedAt"        timestamp with time zone not null,
    "Company"          varchar(255),
    "Subject"          varchar(255)
);

alter table public."Leads"
    owner to postgres;

create index "IX_Leads_BranchPreference"
    on public."Leads" ("BranchPreference");

create index "IX_Leads_OwnerStaffId"
    on public."Leads" ("OwnerStaffId");

create table public."LeaveRequests"
(
    "Id"               uuid                     not null
        constraint "PK_LeaveRequests"
            primary key,
    "StudentProfileId" uuid                     not null
        constraint "FK_LeaveRequests_Profiles_StudentProfileId"
            references public."Profiles"
            on delete cascade,
    "ClassId"          uuid                     not null
        constraint "FK_LeaveRequests_Classes_ClassId"
            references public."Classes"
            on delete cascade,
    "SessionDate"      date                     not null,
    "EndDate"          date,
    "Reason"           text,
    "NoticeHours"      integer,
    "Status"           varchar(20)              not null,
    "RequestedAt"      timestamp with time zone not null,
    "ApprovedBy"       uuid
        constraint "FK_LeaveRequests_Users_ApprovedBy"
            references public."Users"
            on delete restrict,
    "ApprovedAt"       timestamp with time zone,
    "CancelledAt"      timestamp with time zone,
    "SessionId"        uuid
        constraint "FK_LeaveRequests_Sessions_SessionId"
            references public."Sessions"
            on delete restrict
);

alter table public."LeaveRequests"
    owner to postgres;

create index "IX_LeaveRequests_ApprovedBy"
    on public."LeaveRequests" ("ApprovedBy");

create index "IX_LeaveRequests_ClassId"
    on public."LeaveRequests" ("ClassId");

create index "IX_LeaveRequests_StudentProfileId"
    on public."LeaveRequests" ("StudentProfileId");

create index "IX_LeaveRequests_SessionId"
    on public."LeaveRequests" ("SessionId");

create table public."MediaAssets"
(
    "Id"               uuid                                                                   not null
        constraint "PK_MediaAssets"
            primary key,
    "UploaderId"       uuid                                                                   not null
        constraint "FK_MediaAssets_Users_UploaderId"
            references public."Users"
            on delete restrict,
    "BranchId"         uuid                                                                   not null
        constraint "FK_MediaAssets_Branches_BranchId"
            references public."Branches"
            on delete restrict,
    "ClassId"          uuid
        constraint "FK_MediaAssets_Classes_ClassId"
            references public."Classes"
            on delete restrict,
    "StudentProfileId" uuid
        constraint "FK_MediaAssets_Profiles_StudentProfileId"
            references public."Profiles"
            on delete restrict,
    "MonthTag"         varchar(7),
    "Type"             varchar(10)                                                            not null,
    "Url"              text                                                                   not null,
    "Caption"          text,
    "Visibility"       varchar(20)                                                            not null,
    "CreatedAt"        timestamp with time zone                                               not null,
    "ApprovalStatus"   varchar(20)              default ''::character varying                 not null,
    "ApprovedAt"       timestamp with time zone,
    "ApprovedById"     uuid
        constraint "FK_MediaAssets_Users_ApprovedById"
            references public."Users"
            on delete restrict,
    "ContentType"      varchar(20)              default ''::character varying                 not null,
    "IsDeleted"        boolean                  default false                                 not null,
    "IsPublished"      boolean                  default false                                 not null,
    "UpdatedAt"        timestamp with time zone default '-infinity'::timestamp with time zone not null,
    "DisplayOrder"     integer,
    "FileSize"         bigint                   default 0                                     not null,
    "MimeType"         varchar(100),
    "OriginalFileName" varchar(255),
    "OwnershipScope"   varchar(20)              default ''::character varying                 not null,
    "RejectReason"     varchar(1000)
);

alter table public."MediaAssets"
    owner to postgres;

create index "IX_MediaAssets_BranchId"
    on public."MediaAssets" ("BranchId");

create index "IX_MediaAssets_ClassId"
    on public."MediaAssets" ("ClassId");

create index "IX_MediaAssets_StudentProfileId"
    on public."MediaAssets" ("StudentProfileId");

create index "IX_MediaAssets_UploaderId"
    on public."MediaAssets" ("UploaderId");

create index "IX_MediaAssets_ApprovedById"
    on public."MediaAssets" ("ApprovedById");

create table public."Notifications"
(
    "Id"                     uuid                     not null
        constraint "PK_Notifications"
            primary key,
    "RecipientUserId"        uuid                     not null
        constraint "FK_Notifications_Users_RecipientUserId"
            references public."Users"
            on delete cascade,
    "RecipientProfileId"     uuid
        constraint "FK_Notifications_Profiles_RecipientProfileId"
            references public."Profiles"
            on delete cascade,
    "Channel"                varchar(20)              not null,
    "Title"                  varchar(255)             not null,
    "Content"                text,
    "Deeplink"               text,
    "Status"                 varchar(20)              not null,
    "SentAt"                 timestamp with time zone,
    "TemplateId"             varchar(100),
    "CreatedAt"              timestamp with time zone not null,
    "ReadAt"                 timestamp with time zone,
    "Kind"                   text,
    "Priority"               text,
    "SenderName"             text,
    "SenderRole"             text,
    "TargetRole"             text,
    "ScopeBranchId"          uuid
        constraint "FK_Notifications_Branches_ScopeBranchId"
            references public."Branches"
            on delete set null,
    "ScopeClassId"           uuid
        constraint "FK_Notifications_Classes_ScopeClassId"
            references public."Classes"
            on delete set null,
    "ScopeStudentProfileId"  uuid,
    "NotificationTemplateId" uuid
        constraint "FK_Notifications_NotificationTemplates_NotificationTemplateId"
            references public."NotificationTemplates"
            on delete set null
);

alter table public."Notifications"
    owner to postgres;

create index "IX_Notifications_RecipientProfileId"
    on public."Notifications" ("RecipientProfileId");

create index "IX_Notifications_RecipientUserId"
    on public."Notifications" ("RecipientUserId");

create index "IX_Notifications_ScopeBranchId"
    on public."Notifications" ("ScopeBranchId");

create index "IX_Notifications_ScopeClassId"
    on public."Notifications" ("ScopeClassId");

create index "IX_Notifications_NotificationTemplateId"
    on public."Notifications" ("NotificationTemplateId");

create table public."ParentStudentLinks"
(
    "Id"               uuid                     not null
        constraint "PK_ParentStudentLinks"
            primary key,
    "ParentProfileId"  uuid                     not null
        constraint "FK_ParentStudentLinks_Profiles_ParentProfileId"
            references public."Profiles"
            on delete cascade,
    "StudentProfileId" uuid                     not null
        constraint "FK_ParentStudentLinks_Profiles_StudentProfileId"
            references public."Profiles"
            on delete cascade,
    "CreatedAt"        timestamp with time zone not null
);

alter table public."ParentStudentLinks"
    owner to postgres;

create index "IX_ParentStudentLinks_ParentProfileId"
    on public."ParentStudentLinks" ("ParentProfileId");

create index "IX_ParentStudentLinks_StudentProfileId"
    on public."ParentStudentLinks" ("StudentProfileId");

create table public."RewardRedemptions"
(
    "Id"               uuid                     not null
        constraint "PK_RewardRedemptions"
            primary key,
    "ItemId"           uuid                     not null
        constraint "FK_RewardRedemptions_RewardStoreItems_ItemId"
            references public."RewardStoreItems"
            on delete restrict,
    "ItemName"         varchar(255)             not null,
    "StudentProfileId" uuid                     not null
        constraint "FK_RewardRedemptions_Profiles_StudentProfileId"
            references public."Profiles"
            on delete restrict,
    "Status"           varchar(20)              not null,
    "HandledBy"        uuid
        constraint "FK_RewardRedemptions_Users_HandledBy"
            references public."Users"
            on delete restrict,
    "HandledAt"        timestamp with time zone,
    "DeliveredAt"      timestamp with time zone,
    "ReceivedAt"       timestamp with time zone,
    "CreatedAt"        timestamp with time zone not null,
    "Quantity"         integer default 1        not null,
    "CancelReason"     varchar(1000),
    "StarsDeducted"    integer
);

alter table public."RewardRedemptions"
    owner to postgres;

create index "IX_RewardRedemptions_HandledBy"
    on public."RewardRedemptions" ("HandledBy");

create index "IX_RewardRedemptions_ItemId"
    on public."RewardRedemptions" ("ItemId");

create index "IX_RewardRedemptions_StudentProfileId"
    on public."RewardRedemptions" ("StudentProfileId");

create table public."StarTransactions"
(
    "Id"               uuid                     not null
        constraint "PK_StarTransactions"
            primary key,
    "StudentProfileId" uuid                     not null
        constraint "FK_StarTransactions_Profiles_StudentProfileId"
            references public."Profiles"
            on delete cascade,
    "Amount"           integer                  not null,
    "Reason"           varchar(100),
    "SourceType"       varchar(30)              not null,
    "SourceId"         uuid,
    "BalanceAfter"     integer                  not null,
    "CreatedBy"        uuid
        constraint "FK_StarTransactions_Users_CreatedBy"
            references public."Users"
            on delete restrict,
    "CreatedAt"        timestamp with time zone not null
);

alter table public."StarTransactions"
    owner to postgres;

create index "IX_StarTransactions_CreatedBy"
    on public."StarTransactions" ("CreatedBy");

create index "IX_StarTransactions_StudentProfileId"
    on public."StarTransactions" ("StudentProfileId");

create table public."StudentLevels"
(
    "Id"               uuid                     not null
        constraint "PK_StudentLevels"
            primary key,
    "StudentProfileId" uuid                     not null
        constraint "FK_StudentLevels_Profiles_StudentProfileId"
            references public."Profiles"
            on delete cascade,
    "CurrentLevel"     varchar(50)              not null,
    "CurrentXp"        integer                  not null,
    "UpdatedAt"        timestamp with time zone not null
);

alter table public."StudentLevels"
    owner to postgres;

create unique index "IX_StudentLevels_StudentProfileId"
    on public."StudentLevels" ("StudentProfileId");

create table public."StudentMonthlyReports"
(
    "Id"               uuid                                                                   not null
        constraint "PK_StudentMonthlyReports"
            primary key,
    "StudentProfileId" uuid                                                                   not null
        constraint "FK_StudentMonthlyReports_Profiles_StudentProfileId"
            references public."Profiles"
            on delete cascade,
    "Month"            integer                                                                not null,
    "Year"             integer                                                                not null,
    "DraftContent"     jsonb,
    "FinalContent"     jsonb,
    "Status"           varchar(20)                                                            not null,
    "AiVersion"        varchar(50),
    "SubmittedBy"      uuid
        constraint "FK_StudentMonthlyReports_Users_SubmittedBy"
            references public."Users"
            on delete restrict,
    "ReviewedBy"       uuid
        constraint "FK_StudentMonthlyReports_Users_ReviewedBy"
            references public."Users"
            on delete restrict,
    "ReviewedAt"       timestamp with time zone,
    "PublishedAt"      timestamp with time zone,
    "ClassId"          uuid
        constraint "FK_StudentMonthlyReports_Classes_ClassId"
            references public."Classes"
            on delete restrict,
    "CreatedAt"        timestamp with time zone default '-infinity'::timestamp with time zone not null,
    "JobId"            uuid
        constraint "FK_StudentMonthlyReports_MonthlyReportJobs_JobId"
            references public."MonthlyReportJobs"
            on delete restrict,
    "PdfGeneratedAt"   timestamp with time zone,
    "PdfUrl"           text,
    "UpdatedAt"        timestamp with time zone default '-infinity'::timestamp with time zone not null
);

alter table public."StudentMonthlyReports"
    owner to postgres;

create index "IX_StudentMonthlyReports_ReviewedBy"
    on public."StudentMonthlyReports" ("ReviewedBy");

create index "IX_StudentMonthlyReports_SubmittedBy"
    on public."StudentMonthlyReports" ("SubmittedBy");

create index "IX_StudentMonthlyReports_ClassId"
    on public."StudentMonthlyReports" ("ClassId");

create index "IX_StudentMonthlyReports_JobId"
    on public."StudentMonthlyReports" ("JobId");

create unique index "IX_StudentMonthlyReports_StudentProfileId_ClassId_Month_Year"
    on public."StudentMonthlyReports" ("StudentProfileId", "ClassId", "Month", "Year");

create table public."Tickets"
(
    "Id"                  uuid                                       not null
        constraint "PK_Tickets"
            primary key,
    "OpenedByUserId"      uuid                                       not null
        constraint "FK_Tickets_Users_OpenedByUserId"
            references public."Users"
            on delete restrict,
    "OpenedByProfileId"   uuid
        constraint "FK_Tickets_Profiles_OpenedByProfileId"
            references public."Profiles"
            on delete restrict,
    "BranchId"            uuid                                       not null
        constraint "FK_Tickets_Branches_BranchId"
            references public."Branches"
            on delete restrict,
    "ClassId"             uuid
        constraint "FK_Tickets_Classes_ClassId"
            references public."Classes"
            on delete restrict,
    "Category"            varchar(30)                                not null,
    "Message"             text                                       not null,
    "Status"              varchar(20)                                not null,
    "AssignedToUserId"    uuid
        constraint "FK_Tickets_Users_AssignedToUserId"
            references public."Users"
            on delete restrict,
    "CreatedAt"           timestamp with time zone                   not null,
    "UpdatedAt"           timestamp with time zone                   not null,
    "Subject"             varchar(200) default ''::character varying not null,
    "Type"                varchar(20)  default ''::character varying not null,
    "IncidentCategory"    varchar(50),
    "IncidentEvidenceUrl" text,
    "IncidentStatus"      varchar(20),
    "IsIncidentReport"    boolean      default false                 not null
);

alter table public."Tickets"
    owner to postgres;

create index "IX_Tickets_AssignedToUserId"
    on public."Tickets" ("AssignedToUserId");

create index "IX_Tickets_BranchId"
    on public."Tickets" ("BranchId");

create index "IX_Tickets_ClassId"
    on public."Tickets" ("ClassId");

create index "IX_Tickets_OpenedByProfileId"
    on public."Tickets" ("OpenedByProfileId");

create index "IX_Tickets_OpenedByUserId"
    on public."Tickets" ("OpenedByUserId");

create table public."ExamResults"
(
    "Id"               uuid                                                                   not null
        constraint "PK_ExamResults"
            primary key,
    "ExamId"           uuid                                                                   not null
        constraint "FK_ExamResults_Exams_ExamId"
            references public."Exams"
            on delete cascade,
    "StudentProfileId" uuid                                                                   not null
        constraint "FK_ExamResults_Profiles_StudentProfileId"
            references public."Profiles"
            on delete cascade,
    "Score"            numeric,
    "Comment"          text,
    "AttachmentUrls"   jsonb,
    "GradedBy"         uuid
        constraint "FK_ExamResults_Users_GradedBy"
            references public."Users"
            on delete restrict,
    "GradedAt"         timestamp with time zone,
    "CreatedAt"        timestamp with time zone default '-infinity'::timestamp with time zone not null,
    "UpdatedAt"        timestamp with time zone default '-infinity'::timestamp with time zone not null
);

alter table public."ExamResults"
    owner to postgres;

create index "IX_ExamResults_ExamId"
    on public."ExamResults" ("ExamId");

create index "IX_ExamResults_GradedBy"
    on public."ExamResults" ("GradedBy");

create index "IX_ExamResults_StudentProfileId"
    on public."ExamResults" ("StudentProfileId");

create table public."MissionProgresses"
(
    "Id"               uuid        not null
        constraint "PK_MissionProgresses"
            primary key,
    "MissionId"        uuid        not null
        constraint "FK_MissionProgresses_Missions_MissionId"
            references public."Missions"
            on delete cascade,
    "StudentProfileId" uuid        not null
        constraint "FK_MissionProgresses_Profiles_StudentProfileId"
            references public."Profiles"
            on delete cascade,
    "Status"           varchar(20) not null,
    "ProgressValue"    numeric,
    "CompletedAt"      timestamp with time zone,
    "VerifiedBy"       uuid
        constraint "FK_MissionProgresses_Users_VerifiedBy"
            references public."Users"
            on delete restrict
);

alter table public."MissionProgresses"
    owner to postgres;

create index "IX_MissionProgresses_StudentProfileId"
    on public."MissionProgresses" ("StudentProfileId");

create index "IX_MissionProgresses_VerifiedBy"
    on public."MissionProgresses" ("VerifiedBy");

create unique index mission_progress_unique
    on public."MissionProgresses" ("MissionId", "StudentProfileId");

create table public."Attendances"
(
    "Id"               uuid        not null
        constraint "PK_Attendances"
            primary key,
    "SessionId"        uuid        not null
        constraint "FK_Attendances_Sessions_SessionId"
            references public."Sessions"
            on delete cascade,
    "StudentProfileId" uuid        not null
        constraint "FK_Attendances_Profiles_StudentProfileId"
            references public."Profiles"
            on delete cascade,
    "AttendanceStatus" varchar(20) not null,
    "AbsenceType"      varchar(30),
    "MarkedBy"         uuid
        constraint "FK_Attendances_Users_MarkedBy"
            references public."Users"
            on delete restrict,
    "MarkedAt"         timestamp with time zone,
    "Note"             text
);

alter table public."Attendances"
    owner to postgres;

create unique index attendance_unique
    on public."Attendances" ("SessionId", "StudentProfileId");

create index "IX_Attendances_MarkedBy"
    on public."Attendances" ("MarkedBy");

create index "IX_Attendances_StudentProfileId"
    on public."Attendances" ("StudentProfileId");

create table public."HomeworkAssignments"
(
    "Id"                   uuid                     not null
        constraint "PK_HomeworkAssignments"
            primary key,
    "ClassId"              uuid                     not null
        constraint "FK_HomeworkAssignments_Classes_ClassId"
            references public."Classes"
            on delete cascade,
    "SessionId"            uuid
        constraint "FK_HomeworkAssignments_Sessions_SessionId"
            references public."Sessions"
            on delete restrict,
    "Title"                varchar(255)             not null,
    "Description"          text,
    "DueAt"                timestamp with time zone,
    "Book"                 varchar(255),
    "Pages"                varchar(50),
    "Skills"               varchar(100),
    "SubmissionType"       varchar(20)              not null,
    "MaxScore"             numeric,
    "RewardStars"          integer,
    "MissionId"            uuid
        constraint "FK_HomeworkAssignments_Missions_MissionId"
            references public."Missions"
            on delete restrict,
    "CreatedBy"            uuid
        constraint "FK_HomeworkAssignments_Users_CreatedBy"
            references public."Users"
            on delete restrict,
    "CreatedAt"            timestamp with time zone not null,
    "ExpectedAnswer"       text,
    "Instructions"         text,
    "Rubric"               text,
    "AttachmentUrl"        text,
    "TimeLimitMinutes"     integer,
    "AiHintEnabled"        boolean default false    not null,
    "AiRecommendEnabled"   boolean default false    not null,
    "GrammarTags"          jsonb,
    "SpeakingExpectedText" text,
    "SpeakingMode"         varchar(20),
    "TargetWords"          jsonb,
    "Topic"                varchar(100),
    "VocabularyTags"       jsonb,
    "MaxAttempts"          integer default 1        not null
);

alter table public."HomeworkAssignments"
    owner to postgres;

create index "IX_HomeworkAssignments_ClassId"
    on public."HomeworkAssignments" ("ClassId");

create index "IX_HomeworkAssignments_CreatedBy"
    on public."HomeworkAssignments" ("CreatedBy");

create index "IX_HomeworkAssignments_MissionId"
    on public."HomeworkAssignments" ("MissionId");

create index "IX_HomeworkAssignments_SessionId"
    on public."HomeworkAssignments" ("SessionId");

create table public."LessonPlans"
(
    "Id"                 uuid                  not null
        constraint "PK_LessonPlans"
            primary key,
    "SessionId"          uuid                  not null
        constraint "FK_LessonPlans_Sessions_SessionId"
            references public."Sessions"
            on delete cascade,
    "TemplateId"         uuid
        constraint "FK_LessonPlans_LessonPlanTemplates_TemplateId"
            references public."LessonPlanTemplates"
            on delete restrict,
    "PlannedContent"     text,
    "ActualContent"      text,
    "ActualHomework"     text,
    "TeacherNotes"       text,
    "SubmittedBy"        uuid
        constraint "FK_LessonPlans_Users_SubmittedBy"
            references public."Users"
            on delete restrict,
    "SubmittedAt"        timestamp with time zone,
    "IsDeleted"          boolean default false not null,
    "CoverImageFileSize" bigint,
    "CoverImageMimeType" varchar(100),
    "CoverImageUrl"      varchar(500),
    "MediaFileSize"      bigint,
    "MediaMimeType"      varchar(100),
    "MediaType"          varchar(20),
    "MediaUrl"           varchar(500),
    "ClassId"            uuid                  not null
        constraint "FK_LessonPlans_Classes_ClassId"
            references public."Classes"
            on delete cascade
);

alter table public."LessonPlans"
    owner to postgres;

create index "IX_LessonPlans_SubmittedBy"
    on public."LessonPlans" ("SubmittedBy");

create index "IX_LessonPlans_TemplateId"
    on public."LessonPlans" ("TemplateId");

create unique index session_unique
    on public."LessonPlans" ("SessionId");

create index "IX_LessonPlans_ClassId"
    on public."LessonPlans" ("ClassId");

create table public."MakeupCredits"
(
    "Id"               uuid                     not null
        constraint "PK_MakeupCredits"
            primary key,
    "StudentProfileId" uuid                     not null
        constraint "FK_MakeupCredits_Profiles_StudentProfileId"
            references public."Profiles"
            on delete cascade,
    "SourceSessionId"  uuid                     not null
        constraint "FK_MakeupCredits_Sessions_SourceSessionId"
            references public."Sessions"
            on delete restrict,
    "Status"           varchar(20)              not null,
    "CreatedReason"    varchar(30)              not null,
    "ExpiresAt"        timestamp with time zone,
    "UsedSessionId"    uuid
        constraint "FK_MakeupCredits_Sessions_UsedSessionId"
            references public."Sessions"
            on delete restrict,
    "CreatedAt"        timestamp with time zone not null
);

alter table public."MakeupCredits"
    owner to postgres;

create index "IX_MakeupCredits_SourceSessionId"
    on public."MakeupCredits" ("SourceSessionId");

create index "IX_MakeupCredits_StudentProfileId"
    on public."MakeupCredits" ("StudentProfileId");

create index "IX_MakeupCredits_UsedSessionId"
    on public."MakeupCredits" ("UsedSessionId");

create table public."SessionReports"
(
    "Id"                 uuid                                      not null
        constraint "PK_SessionReports"
            primary key,
    "SessionId"          uuid                                      not null
        constraint "FK_SessionReports_Sessions_SessionId"
            references public."Sessions"
            on delete restrict,
    "StudentProfileId"   uuid                                      not null
        constraint "FK_SessionReports_Profiles_StudentProfileId"
            references public."Profiles"
            on delete restrict,
    "TeacherUserId"      uuid                                      not null
        constraint "FK_SessionReports_Users_TeacherUserId"
            references public."Users"
            on delete restrict,
    "ReportDate"         date                                      not null,
    "Feedback"           text                                      not null,
    "AiGeneratedSummary" text,
    "IsMonthlyCompiled"  boolean                                   not null,
    "CreatedAt"          timestamp with time zone                  not null,
    "UpdatedAt"          timestamp with time zone                  not null,
    "AiVersion"          text,
    "DraftContent"       text,
    "FinalContent"       text,
    "PublishedAt"        timestamp with time zone,
    "ReviewedAt"         timestamp with time zone,
    "ReviewedByUserId"   uuid
        constraint "FK_SessionReports_Users_ReviewedByUserId"
            references public."Users",
    "Status"             varchar(20) default ''::character varying not null,
    "SubmittedByUserId"  uuid
        constraint "FK_SessionReports_Users_SubmittedByUserId"
            references public."Users"
);

alter table public."SessionReports"
    owner to postgres;

create index session_report_teacher_date_idx
    on public."SessionReports" ("TeacherUserId", "ReportDate");

create unique index session_report_unique
    on public."SessionReports" ("SessionId", "StudentProfileId");

create index "IX_SessionReports_StudentProfileId"
    on public."SessionReports" ("StudentProfileId");

create index "IX_SessionReports_ReviewedByUserId"
    on public."SessionReports" ("ReviewedByUserId");

create index "IX_SessionReports_SubmittedByUserId"
    on public."SessionReports" ("SubmittedByUserId");

create table public."SessionRoles"
(
    "Id"               uuid        not null
        constraint "PK_SessionRoles"
            primary key,
    "SessionId"        uuid        not null
        constraint "FK_SessionRoles_Sessions_SessionId"
            references public."Sessions"
            on delete cascade,
    "StaffUserId"      uuid        not null
        constraint "FK_SessionRoles_Users_StaffUserId"
            references public."Users"
            on delete restrict,
    "RoleType"         varchar(30) not null,
    "PayableUnitPrice" numeric,
    "PayableAllowance" numeric
);

alter table public."SessionRoles"
    owner to postgres;

create index "IX_SessionRoles_SessionId"
    on public."SessionRoles" ("SessionId");

create index "IX_SessionRoles_StaffUserId"
    on public."SessionRoles" ("StaffUserId");

create table public."InvoiceLines"
(
    "Id"          uuid        not null
        constraint "PK_InvoiceLines"
            primary key,
    "InvoiceId"   uuid        not null
        constraint "FK_InvoiceLines_Invoices_InvoiceId"
            references public."Invoices"
            on delete cascade,
    "ItemType"    varchar(30) not null,
    "Quantity"    integer     not null,
    "UnitPrice"   numeric     not null,
    "Description" text,
    "SessionIds"  jsonb
);

alter table public."InvoiceLines"
    owner to postgres;

create index "IX_InvoiceLines_InvoiceId"
    on public."InvoiceLines" ("InvoiceId");

create table public."Payments"
(
    "Id"            uuid        not null
        constraint "PK_Payments"
            primary key,
    "InvoiceId"     uuid        not null
        constraint "FK_Payments_Invoices_InvoiceId"
            references public."Invoices"
            on delete cascade,
    "Method"        varchar(20) not null,
    "Amount"        numeric     not null,
    "PaidAt"        timestamp with time zone,
    "ReferenceCode" varchar(100),
    "ConfirmedBy"   uuid
        constraint "FK_Payments_Users_ConfirmedBy"
            references public."Users"
            on delete restrict,
    "EvidenceUrl"   text
);

alter table public."Payments"
    owner to postgres;

create index "IX_Payments_ConfirmedBy"
    on public."Payments" ("ConfirmedBy");

create index "IX_Payments_InvoiceId"
    on public."Payments" ("InvoiceId");

create table public."LeadActivities"
(
    "Id"           uuid                     not null
        constraint "PK_LeadActivities"
            primary key,
    "LeadId"       uuid                     not null
        constraint "FK_LeadActivities_Leads_LeadId"
            references public."Leads"
            on delete cascade,
    "ActivityType" varchar(20)              not null,
    "Content"      text,
    "NextActionAt" timestamp with time zone,
    "CreatedBy"    uuid
        constraint "FK_LeadActivities_Users_CreatedBy"
            references public."Users"
            on delete restrict,
    "CreatedAt"    timestamp with time zone not null
);

alter table public."LeadActivities"
    owner to postgres;

create index "IX_LeadActivities_CreatedBy"
    on public."LeadActivities" ("CreatedBy");

create index "IX_LeadActivities_LeadId"
    on public."LeadActivities" ("LeadId");

create table public."ReportComments"
(
    "Id"              uuid                     not null
        constraint "PK_ReportComments"
            primary key,
    "ReportId"        uuid
        constraint "FK_ReportComments_StudentMonthlyReports_ReportId"
            references public."StudentMonthlyReports"
            on delete cascade,
    "CommenterId"     uuid                     not null
        constraint "FK_ReportComments_Users_CommenterId"
            references public."Users"
            on delete restrict,
    "Content"         text                     not null,
    "CreatedAt"       timestamp with time zone not null,
    "SessionReportId" uuid
        constraint "FK_ReportComments_SessionReports_SessionReportId"
            references public."SessionReports"
            on delete cascade,
    constraint "CK_ReportComment_AtLeastOneReportId"
        check (("ReportId" IS NOT NULL) OR ("SessionReportId" IS NOT NULL))
);

alter table public."ReportComments"
    owner to postgres;

create index "IX_ReportComments_CommenterId"
    on public."ReportComments" ("CommenterId");

create index "IX_ReportComments_ReportId"
    on public."ReportComments" ("ReportId");

create index "IX_ReportComments_SessionReportId"
    on public."ReportComments" ("SessionReportId");

create table public."TicketComments"
(
    "Id"                  uuid                     not null
        constraint "PK_TicketComments"
            primary key,
    "TicketId"            uuid                     not null
        constraint "FK_TicketComments_Tickets_TicketId"
            references public."Tickets"
            on delete cascade,
    "CommenterUserId"     uuid                     not null
        constraint "FK_TicketComments_Users_CommenterUserId"
            references public."Users"
            on delete restrict,
    "CommenterProfileId"  uuid
        constraint "FK_TicketComments_Profiles_CommenterProfileId"
            references public."Profiles"
            on delete restrict,
    "Message"             text                     not null,
    "AttachmentUrl"       text,
    "CreatedAt"           timestamp with time zone not null,
    "IncidentCommentType" varchar(30)
);

alter table public."TicketComments"
    owner to postgres;

create index "IX_TicketComments_CommenterProfileId"
    on public."TicketComments" ("CommenterProfileId");

create index "IX_TicketComments_CommenterUserId"
    on public."TicketComments" ("CommenterUserId");

create index "IX_TicketComments_TicketId"
    on public."TicketComments" ("TicketId");

create table public."HomeworkStudents"
(
    "Id"               uuid        not null
        constraint "PK_HomeworkStudents"
            primary key,
    "AssignmentId"     uuid        not null
        constraint "FK_HomeworkStudents_HomeworkAssignments_AssignmentId"
            references public."HomeworkAssignments"
            on delete cascade,
    "StudentProfileId" uuid        not null
        constraint "FK_HomeworkStudents_Profiles_StudentProfileId"
            references public."Profiles"
            on delete cascade,
    "Status"           varchar(20) not null,
    "SubmittedAt"      timestamp with time zone,
    "GradedAt"         timestamp with time zone,
    "Score"            numeric,
    "TeacherFeedback"  text,
    "AiFeedback"       jsonb,
    "AttachmentUrl"    text,
    "TextAnswer"       text,
    "StartedAt"        timestamp with time zone
);

alter table public."HomeworkStudents"
    owner to postgres;

create unique index homework_student_unique
    on public."HomeworkStudents" ("AssignmentId", "StudentProfileId");

create index "IX_HomeworkStudents_StudentProfileId"
    on public."HomeworkStudents" ("StudentProfileId");

create table public."MakeupAllocations"
(
    "Id"              uuid                                                                   not null
        constraint "PK_MakeupAllocations"
            primary key,
    "MakeupCreditId"  uuid                                                                   not null
        constraint "FK_MakeupAllocations_MakeupCredits_MakeupCreditId"
            references public."MakeupCredits"
            on delete cascade,
    "TargetSessionId" uuid                                                                   not null
        constraint "FK_MakeupAllocations_Sessions_TargetSessionId"
            references public."Sessions"
            on delete cascade,
    "AssignedBy"      uuid
        constraint "FK_MakeupAllocations_Users_AssignedBy"
            references public."Users"
            on delete restrict,
    "AssignedAt"      timestamp with time zone,
    "CreatedAt"       timestamp with time zone default '-infinity'::timestamp with time zone not null,
    "Status"          text                     default ''::text                              not null
);

alter table public."MakeupAllocations"
    owner to postgres;

create index "IX_MakeupAllocations_AssignedBy"
    on public."MakeupAllocations" ("AssignedBy");

create index "IX_MakeupAllocations_MakeupCreditId"
    on public."MakeupAllocations" ("MakeupCreditId");

create index "IX_MakeupAllocations_TargetSessionId"
    on public."MakeupAllocations" ("TargetSessionId");

create table public."ParentPinResetTokens"
(
    "Id"              uuid                     not null
        constraint "PK_ParentPinResetTokens"
            primary key,
    "ProfileId"       uuid                     not null
        constraint "FK_ParentPinResetTokens_Profiles_ProfileId"
            references public."Profiles"
            on delete cascade,
    "Token"           varchar(200)             not null,
    "ExpiresAt"       timestamp with time zone not null,
    "UsedAt"          timestamp with time zone,
    "OtpAttemptCount" integer default 0        not null,
    "OtpCodeHash"     varchar(255),
    "OtpExpiresAt"    timestamp with time zone,
    "OtpVerifiedAt"   timestamp with time zone
);

alter table public."ParentPinResetTokens"
    owner to postgres;

create index "IX_ParentPinResetTokens_ProfileId"
    on public."ParentPinResetTokens" ("ProfileId");

create table public."MonthlyReportData"
(
    "Id"               uuid                     not null
        constraint "PK_MonthlyReportData"
            primary key,
    "ReportId"         uuid                     not null
        constraint "FK_MonthlyReportData_StudentMonthlyReports_ReportId"
            references public."StudentMonthlyReports"
            on delete cascade,
    "StudentProfileId" uuid                     not null
        constraint "FK_MonthlyReportData_Profiles_StudentProfileId"
            references public."Profiles"
            on delete cascade,
    "Month"            integer                  not null,
    "Year"             integer                  not null,
    "AttendanceData"   jsonb,
    "HomeworkData"     jsonb,
    "TestData"         jsonb,
    "MissionData"      jsonb,
    "NotesData"        jsonb,
    "CreatedAt"        timestamp with time zone not null,
    "UpdatedAt"        timestamp with time zone not null,
    "TopicsData"       jsonb
);

alter table public."MonthlyReportData"
    owner to postgres;

create unique index "IX_MonthlyReportData_ReportId"
    on public."MonthlyReportData" ("ReportId");

create index "IX_MonthlyReportData_StudentProfileId"
    on public."MonthlyReportData" ("StudentProfileId");

create table public."ExamQuestions"
(
    "Id"            uuid                     not null
        constraint "PK_ExamQuestions"
            primary key,
    "ExamId"        uuid                     not null
        constraint "FK_ExamQuestions_Exams_ExamId"
            references public."Exams"
            on delete cascade,
    "OrderIndex"    integer                  not null,
    "QuestionText"  text                     not null,
    "QuestionType"  varchar(20)              not null,
    "Options"       jsonb,
    "CorrectAnswer" text,
    "Points"        integer                  not null,
    "Explanation"   text,
    "CreatedAt"     timestamp with time zone not null,
    "UpdatedAt"     timestamp with time zone not null
);

alter table public."ExamQuestions"
    owner to postgres;

create index "IX_ExamQuestions_ExamId"
    on public."ExamQuestions" ("ExamId");

create table public."ExamSubmissions"
(
    "Id"               uuid        not null
        constraint "PK_ExamSubmissions"
            primary key,
    "ExamId"           uuid        not null
        constraint "FK_ExamSubmissions_Exams_ExamId"
            references public."Exams"
            on delete cascade,
    "StudentProfileId" uuid        not null
        constraint "FK_ExamSubmissions_Profiles_StudentProfileId"
            references public."Profiles"
            on delete restrict,
    "ActualStartTime"  timestamp with time zone,
    "SubmittedAt"      timestamp with time zone,
    "AutoSubmittedAt"  timestamp with time zone,
    "TimeSpentMinutes" integer,
    "AutoScore"        numeric(18, 2),
    "FinalScore"       numeric(18, 2),
    "GradedBy"         uuid
        constraint "FK_ExamSubmissions_Users_GradedBy"
            references public."Users"
            on delete restrict,
    "GradedAt"         timestamp with time zone,
    "TeacherComment"   text,
    "Status"           varchar(20) not null
);

alter table public."ExamSubmissions"
    owner to postgres;

create index "IX_ExamSubmissions_ExamId"
    on public."ExamSubmissions" ("ExamId");

create index "IX_ExamSubmissions_GradedBy"
    on public."ExamSubmissions" ("GradedBy");

create index "IX_ExamSubmissions_StudentProfileId"
    on public."ExamSubmissions" ("StudentProfileId");

create table public."ExamSubmissionAnswers"
(
    "Id"              uuid    not null
        constraint "PK_ExamSubmissionAnswers"
            primary key,
    "SubmissionId"    uuid    not null
        constraint "FK_ExamSubmissionAnswers_ExamSubmissions_SubmissionId"
            references public."ExamSubmissions"
            on delete cascade,
    "QuestionId"      uuid    not null
        constraint "FK_ExamSubmissionAnswers_ExamQuestions_QuestionId"
            references public."ExamQuestions"
            on delete restrict,
    "Answer"          text    not null,
    "IsCorrect"       boolean not null,
    "PointsAwarded"   numeric(18, 2),
    "TeacherFeedback" text,
    "AnsweredAt"      timestamp with time zone
);

alter table public."ExamSubmissionAnswers"
    owner to postgres;

create index "IX_ExamSubmissionAnswers_QuestionId"
    on public."ExamSubmissionAnswers" ("QuestionId");

create index "IX_ExamSubmissionAnswers_SubmissionId"
    on public."ExamSubmissionAnswers" ("SubmissionId");

create table public."LeadChildren"
(
    "Id"                        uuid                                      not null
        constraint "PK_LeadChildren"
            primary key,
    "LeadId"                    uuid                                      not null
        constraint "FK_LeadChildren_Leads_LeadId"
            references public."Leads"
            on delete cascade,
    "ChildName"                 varchar(255)                              not null,
    "Dob"                       date,
    "Gender"                    varchar(20) default ''::character varying not null,
    "ProgramInterest"           varchar(255),
    "Notes"                     text,
    "Status"                    varchar(30)                               not null,
    "ConvertedStudentProfileId" uuid
        constraint "FK_LeadChildren_Profiles_ConvertedStudentProfileId"
            references public."Profiles"
            on delete restrict,
    "CreatedAt"                 timestamp with time zone                  not null,
    "UpdatedAt"                 timestamp with time zone                  not null
);

alter table public."LeadChildren"
    owner to postgres;

create table public."PlacementTests"
(
    "Id"                               uuid                                                                   not null
        constraint "PK_PlacementTests"
            primary key,
    "LeadId"                           uuid
        constraint "FK_PlacementTests_Leads_LeadId"
            references public."Leads"
            on delete restrict,
    "StudentProfileId"                 uuid
        constraint "FK_PlacementTests_Profiles_StudentProfileId"
            references public."Profiles"
            on delete restrict,
    "ClassId"                          uuid
        constraint "FK_PlacementTests_Classes_ClassId"
            references public."Classes"
            on delete restrict,
    "ScheduledAt"                      timestamp with time zone,
    "Status"                           varchar(20)                                                            not null,
    "Room"                             varchar(100),
    "InvigilatorUserId"                uuid
        constraint "FK_PlacementTests_Users_InvigilatorUserId"
            references public."Users"
            on delete restrict,
    "ResultScore"                      numeric,
    "ListeningScore"                   numeric,
    "SpeakingScore"                    numeric,
    "ReadingScore"                     numeric,
    "WritingScore"                     numeric,
    "LevelRecommendation"              varchar(100),
    "Notes"                            text,
    "AttachmentUrl"                    text,
    "CreatedAt"                        timestamp with time zone default '-infinity'::timestamp with time zone not null,
    "UpdatedAt"                        timestamp with time zone default '-infinity'::timestamp with time zone not null,
    "LeadChildId"                      uuid
        constraint "FK_PlacementTests_LeadChildren_LeadChildId"
            references public."LeadChildren"
            on delete restrict,
    "OriginalPlacementTestId"          uuid
        constraint "FK_PlacementTests_PlacementTests_OriginalPlacementTestId"
            references public."PlacementTests",
    "SecondaryProgramSkillFocus"       varchar(50),
    "ProgramRecommendationId"          uuid
        constraint "FK_PlacementTests_Programs_ProgramRecommendationId"
            references public."Programs"
            on delete restrict,
    "SecondaryProgramRecommendationId" uuid
        constraint "FK_PlacementTests_Programs_SecondaryProgramRecommendationId"
            references public."Programs"
            on delete restrict,
    "DurationMinutes"                  integer                  default 60                                    not null,
    "RoomId"                           uuid
        constraint "FK_PlacementTests_Classrooms_RoomId"
            references public."Classrooms"
            on delete restrict
);

alter table public."PlacementTests"
    owner to postgres;

create index "IX_PlacementTests_ClassId"
    on public."PlacementTests" ("ClassId");

create index "IX_PlacementTests_InvigilatorUserId"
    on public."PlacementTests" ("InvigilatorUserId");

create index "IX_PlacementTests_LeadId"
    on public."PlacementTests" ("LeadId");

create index "IX_PlacementTests_StudentProfileId"
    on public."PlacementTests" ("StudentProfileId");

create index "IX_PlacementTests_LeadChildId"
    on public."PlacementTests" ("LeadChildId");

create index "IX_PlacementTests_OriginalPlacementTestId"
    on public."PlacementTests" ("OriginalPlacementTestId");

create index "IX_PlacementTests_ProgramRecommendationId"
    on public."PlacementTests" ("ProgramRecommendationId");

create index "IX_PlacementTests_SecondaryProgramRecommendationId"
    on public."PlacementTests" ("SecondaryProgramRecommendationId");

create index "IX_PlacementTests_RoomId"
    on public."PlacementTests" ("RoomId");

create index "IX_LeadChildren_ConvertedStudentProfileId"
    on public."LeadChildren" ("ConvertedStudentProfileId");

create index "IX_LeadChildren_LeadId"
    on public."LeadChildren" ("LeadId");

create table public."DeviceTokens"
(
    "Id"         uuid                     not null
        constraint "PK_DeviceTokens"
            primary key,
    "UserId"     uuid                     not null
        constraint "FK_DeviceTokens_Users_UserId"
            references public."Users"
            on delete cascade,
    "Token"      varchar(500)             not null,
    "DeviceType" varchar(50),
    "DeviceId"   varchar(200),
    "IsActive"   boolean                  not null,
    "CreatedAt"  timestamp with time zone not null,
    "UpdatedAt"  timestamp with time zone not null,
    "LastUsedAt" timestamp with time zone,
    "BranchId"   uuid,
    "Browser"    text,
    "Locale"     text,
    "Role"       text
);

alter table public."DeviceTokens"
    owner to postgres;

create index "IX_DeviceTokens_UserId"
    on public."DeviceTokens" ("UserId");

create unique index "IX_DeviceTokens_UserId_DeviceId_IsActive"
    on public."DeviceTokens" ("UserId", "DeviceId", "IsActive")
    where ("IsActive" = true);

create index "IX_DeviceTokens_UserId_IsActive"
    on public."DeviceTokens" ("UserId", "IsActive");

create table public."HomeworkQuestions"
(
    "Id"                   uuid        not null
        constraint "PK_HomeworkQuestions"
            primary key,
    "HomeworkAssignmentId" uuid        not null
        constraint "FK_HomeworkQuestions_HomeworkAssignments_HomeworkAssignmentId"
            references public."HomeworkAssignments"
            on delete cascade,
    "OrderIndex"           integer     not null,
    "QuestionText"         text        not null,
    "QuestionType"         varchar(20) not null,
    "Options"              jsonb,
    "CorrectAnswer"        text,
    "Points"               integer     not null,
    "Explanation"          text
);

alter table public."HomeworkQuestions"
    owner to postgres;

create index "IX_HomeworkQuestions_HomeworkAssignmentId"
    on public."HomeworkQuestions" ("HomeworkAssignmentId");

create table public."Registrations"
(
    "Id"                         uuid                     not null
        constraint "PK_Registrations"
            primary key,
    "StudentProfileId"           uuid                     not null
        constraint "FK_Registrations_Profiles_StudentProfileId"
            references public."Profiles"
            on delete cascade,
    "BranchId"                   uuid                     not null
        constraint "FK_Registrations_Branches_BranchId"
            references public."Branches"
            on delete restrict,
    "ProgramId"                  uuid                     not null
        constraint "FK_Registrations_Programs_ProgramId"
            references public."Programs"
            on delete restrict,
    "TuitionPlanId"              uuid                     not null
        constraint "FK_Registrations_TuitionPlans_TuitionPlanId"
            references public."TuitionPlans"
            on delete restrict,
    "RegistrationDate"           timestamp with time zone not null,
    "ExpectedStartDate"          timestamp with time zone,
    "ActualStartDate"            timestamp with time zone,
    "PreferredSchedule"          text,
    "Note"                       text,
    "Status"                     varchar(20)              not null,
    "ClassId"                    uuid
        constraint "FK_Registrations_Classes_ClassId"
            references public."Classes"
            on delete restrict,
    "ClassAssignedDate"          timestamp with time zone,
    "EntryType"                  varchar(20),
    "OriginalRegistrationId"     uuid
        constraint "FK_Registrations_Registrations_OriginalRegistrationId"
            references public."Registrations"
            on delete restrict,
    "OperationType"              varchar(20),
    "TotalSessions"              integer                  not null,
    "UsedSessions"               integer                  not null,
    "RemainingSessions"          integer                  not null,
    "ExpiryDate"                 timestamp with time zone,
    "CreatedAt"                  timestamp with time zone not null,
    "UpdatedAt"                  timestamp with time zone not null,
    "SecondaryClassAssignedDate" timestamp with time zone,
    "SecondaryClassId"           uuid
        constraint "FK_Registrations_Classes_SecondaryClassId"
            references public."Classes"
            on delete restrict,
    "SecondaryEntryType"         varchar(20),
    "SecondaryProgramId"         uuid
        constraint "FK_Registrations_Programs_SecondaryProgramId"
            references public."Programs"
            on delete restrict,
    "SecondaryProgramSkillFocus" varchar(50)
);

alter table public."Registrations"
    owner to postgres;

create table public."ClassEnrollments"
(
    "Id"                                   uuid                                      not null
        constraint "PK_ClassEnrollments"
            primary key,
    "ClassId"                              uuid                                      not null
        constraint "FK_ClassEnrollments_Classes_ClassId"
            references public."Classes"
            on delete cascade,
    "StudentProfileId"                     uuid                                      not null
        constraint "FK_ClassEnrollments_Profiles_StudentProfileId"
            references public."Profiles"
            on delete restrict,
    "EnrollDate"                           date                                      not null,
    "Status"                               varchar(20)                               not null,
    "TuitionPlanId"                        uuid
        constraint "FK_ClassEnrollments_TuitionPlans_TuitionPlanId"
            references public."TuitionPlans"
            on delete restrict,
    "CreatedAt"                            timestamp with time zone                  not null,
    "UpdatedAt"                            timestamp with time zone                  not null,
    "RegistrationId"                       uuid
        constraint "FK_ClassEnrollments_Registrations_RegistrationId"
            references public."Registrations"
            on delete set null,
    "SessionSelectionPattern"              text,
    "Track"                                varchar(20) default ''::character varying not null,
    "EnrollmentConfirmationPdfGeneratedAt" timestamp with time zone,
    "EnrollmentConfirmationPdfGeneratedBy" uuid,
    "EnrollmentConfirmationPdfUrl"         varchar(1000)
);

alter table public."ClassEnrollments"
    owner to postgres;

create index "IX_ClassEnrollments_ClassId"
    on public."ClassEnrollments" ("ClassId");

create index "IX_ClassEnrollments_StudentProfileId"
    on public."ClassEnrollments" ("StudentProfileId");

create index "IX_ClassEnrollments_TuitionPlanId"
    on public."ClassEnrollments" ("TuitionPlanId");

create index "IX_ClassEnrollments_RegistrationId"
    on public."ClassEnrollments" ("RegistrationId");

create table public."PauseEnrollmentRequests"
(
    "Id"                     uuid                     not null
        constraint "PK_PauseEnrollmentRequests"
            primary key,
    "StudentProfileId"       uuid                     not null
        constraint "FK_PauseEnrollmentRequests_Profiles_StudentProfileId"
            references public."Profiles"
            on delete cascade,
    "ClassId"                uuid
        constraint "FK_PauseEnrollmentRequests_Classes_ClassId"
            references public."Classes"
            on delete cascade,
    "PauseFrom"              date                     not null,
    "PauseTo"                date                     not null,
    "Reason"                 text,
    "Status"                 varchar(20)              not null,
    "RequestedAt"            timestamp with time zone not null,
    "ApprovedBy"             uuid
        constraint "FK_PauseEnrollmentRequests_Users_ApprovedBy"
            references public."Users"
            on delete restrict,
    "ApprovedAt"             timestamp with time zone,
    "Outcome"                varchar(40),
    "OutcomeNote"            text,
    "OutcomeBy"              uuid
        constraint "FK_PauseEnrollmentRequests_Users_OutcomeBy"
            references public."Users"
            on delete restrict,
    "OutcomeAt"              timestamp with time zone,
    "CancelledAt"            timestamp with time zone,
    "CancelledBy"            uuid
        constraint "FK_PauseEnrollmentRequests_Users_CancelledBy"
            references public."Users"
            on delete restrict,
    "OutcomeCompletedAt"     timestamp with time zone,
    "OutcomeCompletedBy"     uuid
        constraint "FK_PauseEnrollmentRequests_Users_OutcomeCompletedBy"
            references public."Users"
            on delete restrict,
    "ReassignedClassId"      uuid
        constraint "FK_PauseEnrollmentRequests_Classes_ReassignedClassId"
            references public."Classes"
            on delete restrict,
    "ReassignedEnrollmentId" uuid
        constraint "FK_PauseEnrollmentRequests_ClassEnrollments_ReassignedEnrollme~"
            references public."ClassEnrollments"
            on delete set null,
    "ReservationExpiresOn"   date,
    "ReservationSnapshotAt"  timestamp with time zone,
    "ReservedSessionCount"   integer default 0        not null
);

alter table public."PauseEnrollmentRequests"
    owner to postgres;

create index "IX_PauseEnrollmentRequests_ApprovedBy"
    on public."PauseEnrollmentRequests" ("ApprovedBy");

create index "IX_PauseEnrollmentRequests_ClassId"
    on public."PauseEnrollmentRequests" ("ClassId");

create index "IX_PauseEnrollmentRequests_OutcomeBy"
    on public."PauseEnrollmentRequests" ("OutcomeBy");

create index "IX_PauseEnrollmentRequests_StudentProfileId"
    on public."PauseEnrollmentRequests" ("StudentProfileId");

create index "IX_PauseEnrollmentRequests_CancelledBy"
    on public."PauseEnrollmentRequests" ("CancelledBy");

create index "IX_PauseEnrollmentRequests_OutcomeCompletedBy"
    on public."PauseEnrollmentRequests" ("OutcomeCompletedBy");

create index "IX_PauseEnrollmentRequests_ReassignedClassId"
    on public."PauseEnrollmentRequests" ("ReassignedClassId");

create index "IX_PauseEnrollmentRequests_ReassignedEnrollmentId"
    on public."PauseEnrollmentRequests" ("ReassignedEnrollmentId");

create table public."PauseEnrollmentRequestHistories"
(
    "Id"                       uuid                     not null
        constraint "PK_PauseEnrollmentRequestHistories"
            primary key,
    "PauseEnrollmentRequestId" uuid                     not null
        constraint "FK_PauseEnrollmentRequestHistories_PauseEnrollmentRequests_Pau~"
            references public."PauseEnrollmentRequests"
            on delete cascade,
    "StudentProfileId"         uuid                     not null
        constraint "FK_PauseEnrollmentRequestHistories_Profiles_StudentProfileId"
            references public."Profiles"
            on delete restrict,
    "ClassId"                  uuid                     not null
        constraint "FK_PauseEnrollmentRequestHistories_Classes_ClassId"
            references public."Classes"
            on delete restrict,
    "EnrollmentId"             uuid
        constraint "FK_PauseEnrollmentRequestHistories_ClassEnrollments_Enrollment~"
            references public."ClassEnrollments"
            on delete set null,
    "PreviousStatus"           varchar(20)              not null,
    "NewStatus"                varchar(20)              not null,
    "PauseFrom"                date                     not null,
    "PauseTo"                  date                     not null,
    "ChangedAt"                timestamp with time zone not null,
    "ChangedBy"                uuid
        constraint "FK_PauseEnrollmentRequestHistories_Users_ChangedBy"
            references public."Users"
            on delete restrict,
    "ReservedSessionCount"     integer default 0        not null
);

alter table public."PauseEnrollmentRequestHistories"
    owner to postgres;

create index "IX_PauseEnrollmentRequestHistories_ChangedBy"
    on public."PauseEnrollmentRequestHistories" ("ChangedBy");

create index "IX_PauseEnrollmentRequestHistories_ClassId"
    on public."PauseEnrollmentRequestHistories" ("ClassId");

create index "IX_PauseEnrollmentRequestHistories_EnrollmentId"
    on public."PauseEnrollmentRequestHistories" ("EnrollmentId");

create index "IX_PauseEnrollmentRequestHistories_PauseEnrollmentRequestId"
    on public."PauseEnrollmentRequestHistories" ("PauseEnrollmentRequestId");

create index "IX_PauseEnrollmentRequestHistories_StudentProfileId"
    on public."PauseEnrollmentRequestHistories" ("StudentProfileId");

create index "IX_Registrations_BranchId"
    on public."Registrations" ("BranchId");

create index "IX_Registrations_ClassId"
    on public."Registrations" ("ClassId");

create index "IX_Registrations_OriginalRegistrationId"
    on public."Registrations" ("OriginalRegistrationId");

create index "IX_Registrations_ProgramId"
    on public."Registrations" ("ProgramId");

create index "IX_Registrations_StudentProfileId"
    on public."Registrations" ("StudentProfileId");

create index "IX_Registrations_TuitionPlanId"
    on public."Registrations" ("TuitionPlanId");

create index "IX_Registrations_SecondaryClassId"
    on public."Registrations" ("SecondaryClassId");

create index "IX_Registrations_SecondaryProgramId"
    on public."Registrations" ("SecondaryProgramId");

create table public."QuestionBankItems"
(
    "Id"             uuid                     not null
        constraint "PK_QuestionBankItems"
            primary key,
    "ProgramId"      uuid                     not null
        constraint "FK_QuestionBankItems_Programs_ProgramId"
            references public."Programs"
            on delete cascade,
    "QuestionText"   text                     not null,
    "QuestionType"   varchar(20)              not null,
    "Options"        jsonb,
    "CorrectAnswer"  text,
    "Points"         integer                  not null,
    "Explanation"    text,
    "Level"          varchar(10)              not null,
    "CreatedBy"      uuid,
    "CreatedAt"      timestamp with time zone not null,
    "UpdatedAt"      timestamp with time zone,
    "GrammarTags"    jsonb,
    "Skill"          varchar(100),
    "Topic"          varchar(100),
    "VocabularyTags" jsonb,
    "IsDeleted"      boolean default false    not null,
    "AudioUrls"      jsonb,
    "ImageUrls"      jsonb,
    "VideoUrls"      jsonb
);

alter table public."QuestionBankItems"
    owner to postgres;

create index "IX_QuestionBankItems_ProgramId_Level_IsDeleted"
    on public."QuestionBankItems" ("ProgramId", "Level", "IsDeleted");

create table public."ProgramLeavePolicies"
(
    "Id"                uuid                     not null
        constraint "PK_ProgramLeavePolicies"
            primary key,
    "ProgramId"         uuid                     not null
        constraint "FK_ProgramLeavePolicies_Programs_ProgramId"
            references public."Programs"
            on delete cascade,
    "MaxLeavesPerMonth" integer                  not null,
    "UpdatedBy"         uuid
        constraint "FK_ProgramLeavePolicies_Users_UpdatedBy"
            references public."Users"
            on delete restrict,
    "CreatedAt"         timestamp with time zone not null,
    "UpdatedAt"         timestamp with time zone not null
);

alter table public."ProgramLeavePolicies"
    owner to postgres;

create unique index "IX_ProgramLeavePolicies_ProgramId"
    on public."ProgramLeavePolicies" ("ProgramId");

create index "IX_ProgramLeavePolicies_UpdatedBy"
    on public."ProgramLeavePolicies" ("UpdatedBy");

create table public."TeachingMaterials"
(
    "Id"                    uuid                     not null
        constraint "PK_TeachingMaterials"
            primary key,
    "ProgramId"             uuid                     not null
        constraint "FK_TeachingMaterials_Programs_ProgramId"
            references public."Programs"
            on delete cascade,
    "UnitNumber"            integer,
    "LessonNumber"          integer,
    "LessonTitle"           varchar(255),
    "RelativePath"          varchar(500),
    "DisplayName"           varchar(255)             not null,
    "OriginalFileName"      varchar(255)             not null,
    "StoragePath"           varchar(500)             not null,
    "MimeType"              varchar(255)             not null,
    "FileExtension"         varchar(20)              not null,
    "FileSize"              bigint                   not null,
    "FileType"              varchar(50)              not null,
    "Category"              varchar(50)              not null,
    "IsEncrypted"           boolean                  not null,
    "EncryptionAlgorithm"   varchar(50)              not null,
    "EncryptionKeyVersion"  varchar(20)              not null,
    "UploadedByUserId"      uuid                     not null
        constraint "FK_TeachingMaterials_Users_UploadedByUserId"
            references public."Users"
            on delete restrict,
    "CreatedAt"             timestamp with time zone not null,
    "UpdatedAt"             timestamp with time zone not null,
    "PdfPreviewFileSize"    bigint,
    "PdfPreviewGeneratedAt" timestamp with time zone,
    "PdfPreviewPath"        varchar(500)
);

alter table public."TeachingMaterials"
    owner to postgres;

create index "IX_TeachingMaterials_ProgramId"
    on public."TeachingMaterials" ("ProgramId");

create index "IX_TeachingMaterials_ProgramId_UnitNumber_LessonNumber"
    on public."TeachingMaterials" ("ProgramId", "UnitNumber", "LessonNumber");

create index "IX_TeachingMaterials_UploadedByUserId"
    on public."TeachingMaterials" ("UploadedByUserId");

create table public."GamificationSettings"
(
    "Id"                 integer generated by default as identity
        constraint "PK_GamificationSettings"
            primary key,
    "CheckInRewardStars" integer                  not null,
    "CheckInRewardExp"   integer                  not null,
    "CreatedAt"          timestamp with time zone not null,
    "UpdatedAt"          timestamp with time zone
);

alter table public."GamificationSettings"
    owner to postgres;

create table public."StudentSessionAssignments"
(
    "Id"                uuid                     not null
        constraint "PK_StudentSessionAssignments"
            primary key,
    "SessionId"         uuid                     not null
        constraint "FK_StudentSessionAssignments_Sessions_SessionId"
            references public."Sessions"
            on delete cascade,
    "StudentProfileId"  uuid                     not null
        constraint "FK_StudentSessionAssignments_Profiles_StudentProfileId"
            references public."Profiles"
            on delete cascade,
    "ClassEnrollmentId" uuid                     not null
        constraint "FK_StudentSessionAssignments_ClassEnrollments_ClassEnrollmentId"
            references public."ClassEnrollments"
            on delete cascade,
    "RegistrationId"    uuid
        constraint "FK_StudentSessionAssignments_Registrations_RegistrationId"
            references public."Registrations"
            on delete set null,
    "Track"             varchar(20)              not null,
    "Status"            varchar(20)              not null,
    "CreatedAt"         timestamp with time zone not null,
    "UpdatedAt"         timestamp with time zone not null
);

alter table public."StudentSessionAssignments"
    owner to postgres;

create index "IX_StudentSessionAssignments_ClassEnrollmentId"
    on public."StudentSessionAssignments" ("ClassEnrollmentId");

create index "IX_StudentSessionAssignments_RegistrationId"
    on public."StudentSessionAssignments" ("RegistrationId");

create index "IX_StudentSessionAssignments_SessionId"
    on public."StudentSessionAssignments" ("SessionId");

create unique index "IX_StudentSessionAssignments_SessionId_ClassEnrollmentId"
    on public."StudentSessionAssignments" ("SessionId", "ClassEnrollmentId");

create index "IX_StudentSessionAssignments_SessionId_Status"
    on public."StudentSessionAssignments" ("SessionId", "Status");

create index "IX_StudentSessionAssignments_StudentProfileId"
    on public."StudentSessionAssignments" ("StudentProfileId");

create index "IX_StudentSessionAssignments_StudentProfileId_Status"
    on public."StudentSessionAssignments" ("StudentProfileId", "Status");

create table public."ExtracurricularPrograms"
(
    "Id"              uuid                     not null
        constraint "PK_ExtracurricularPrograms"
            primary key,
    "BranchId"        uuid                     not null
        constraint "FK_ExtracurricularPrograms_Branches_BranchId"
            references public."Branches"
            on delete restrict,
    "Name"            varchar(255)             not null,
    "Type"            varchar(100)             not null,
    "Date"            date                     not null,
    "Capacity"        integer                  not null,
    "RegisteredCount" integer                  not null,
    "Fee"             numeric                  not null,
    "Location"        varchar(255),
    "IsActive"        boolean                  not null,
    "IsDeleted"       boolean                  not null,
    "CreatedAt"       timestamp with time zone not null,
    "UpdatedAt"       timestamp with time zone not null
);

alter table public."ExtracurricularPrograms"
    owner to postgres;

create index "IX_ExtracurricularPrograms_BranchId"
    on public."ExtracurricularPrograms" ("BranchId");

create table public."ReportRequests"
(
    "Id"                     uuid                     not null
        constraint "PK_ReportRequests"
            primary key,
    "ReportType"             varchar(20)              not null,
    "Status"                 varchar(20)              not null,
    "Priority"               varchar(20)              not null,
    "AssignedTeacherUserId"  uuid                     not null
        constraint "FK_ReportRequests_Users_AssignedTeacherUserId"
            references public."Users"
            on delete restrict,
    "RequestedByUserId"      uuid                     not null
        constraint "FK_ReportRequests_Users_RequestedByUserId"
            references public."Users"
            on delete restrict,
    "TargetStudentProfileId" uuid
        constraint "FK_ReportRequests_Profiles_TargetStudentProfileId"
            references public."Profiles"
            on delete set null,
    "TargetClassId"          uuid
        constraint "FK_ReportRequests_Classes_TargetClassId"
            references public."Classes"
            on delete set null,
    "TargetSessionId"        uuid
        constraint "FK_ReportRequests_Sessions_TargetSessionId"
            references public."Sessions"
            on delete set null,
    "Month"                  integer,
    "Year"                   integer,
    "Message"                varchar(1000),
    "DueAt"                  timestamp with time zone,
    "LinkedSessionReportId"  uuid
        constraint "FK_ReportRequests_SessionReports_LinkedSessionReportId"
            references public."SessionReports"
            on delete set null,
    "LinkedMonthlyReportId"  uuid
        constraint "FK_ReportRequests_StudentMonthlyReports_LinkedMonthlyReportId"
            references public."StudentMonthlyReports"
            on delete set null,
    "SubmittedAt"            timestamp with time zone,
    "CreatedAt"              timestamp with time zone not null,
    "UpdatedAt"              timestamp with time zone not null
);

alter table public."ReportRequests"
    owner to postgres;

create index "IX_ReportRequests_LinkedMonthlyReportId"
    on public."ReportRequests" ("LinkedMonthlyReportId");

create index "IX_ReportRequests_LinkedSessionReportId"
    on public."ReportRequests" ("LinkedSessionReportId");

create index "IX_ReportRequests_RequestedByUserId"
    on public."ReportRequests" ("RequestedByUserId");

create index "IX_ReportRequests_TargetClassId"
    on public."ReportRequests" ("TargetClassId");

create index "IX_ReportRequests_TargetSessionId"
    on public."ReportRequests" ("TargetSessionId");

create index "IX_ReportRequests_TargetStudentProfileId"
    on public."ReportRequests" ("TargetStudentProfileId");

create index report_request_teacher_queue_idx
    on public."ReportRequests" ("AssignedTeacherUserId", "Status", "Priority", "DueAt");

create index report_request_type_class_month_idx
    on public."ReportRequests" ("ReportType", "TargetClassId", "Month", "Year");

create index report_request_type_student_month_idx
    on public."ReportRequests" ("ReportType", "TargetStudentProfileId", "Month", "Year");

create table public."HomeworkSubmissionAttempts"
(
    "Id"                uuid                     not null
        constraint "PK_HomeworkSubmissionAttempts"
            primary key,
    "HomeworkStudentId" uuid                     not null
        constraint "FK_HomeworkSubmissionAttempts_HomeworkStudents_HomeworkStudent~"
            references public."HomeworkStudents"
            on delete cascade,
    "AttemptNumber"     integer                  not null,
    "Status"            varchar(20)              not null,
    "StartedAt"         timestamp with time zone,
    "SubmittedAt"       timestamp with time zone,
    "GradedAt"          timestamp with time zone,
    "Score"             numeric,
    "TeacherFeedback"   text,
    "AiFeedback"        jsonb,
    "TextAnswer"        text,
    "AttachmentUrl"     text,
    "CreatedAt"         timestamp with time zone not null
);

alter table public."HomeworkSubmissionAttempts"
    owner to postgres;

create unique index homework_submission_attempt_unique
    on public."HomeworkSubmissionAttempts" ("HomeworkStudentId", "AttemptNumber");

create table public."TeachingMaterialAnnotations"
(
    "Id"                 uuid                                             not null
        constraint "PK_TeachingMaterialAnnotations"
            primary key,
    "TeachingMaterialId" uuid                                             not null
        constraint "FK_TeachingMaterialAnnotations_TeachingMaterials_TeachingMater~"
            references public."TeachingMaterials"
            on delete cascade,
    "SlideNumber"        integer,
    "UserId"             uuid                                             not null
        constraint "FK_TeachingMaterialAnnotations_Users_UserId"
            references public."Users"
            on delete cascade,
    "Content"            varchar(2000)                                    not null,
    "Color"              varchar(20) default '#FFD700'::character varying,
    "PositionX"          double precision,
    "PositionY"          double precision,
    "Type"               varchar(20) default 'Note'::character varying    not null
        constraint "CK_Annotation_Type"
            check (("Type")::text = ANY
                   ((ARRAY ['Note'::character varying, 'Highlight'::character varying, 'Pin'::character varying])::text[])),
    "Visibility"         varchar(20) default 'Private'::character varying not null
        constraint "CK_Annotation_Visibility"
            check (("Visibility")::text = ANY
                   ((ARRAY ['Private'::character varying, 'Class'::character varying, 'Public'::character varying])::text[])),
    "CreatedAt"          timestamp with time zone                         not null,
    "UpdatedAt"          timestamp with time zone                         not null
);

alter table public."TeachingMaterialAnnotations"
    owner to postgres;

create index "IX_TeachingMaterialAnnotations_TeachingMaterialId"
    on public."TeachingMaterialAnnotations" ("TeachingMaterialId");

create index "IX_TeachingMaterialAnnotations_TeachingMaterialId_SlideNumber"
    on public."TeachingMaterialAnnotations" ("TeachingMaterialId", "SlideNumber");

create index "IX_TeachingMaterialAnnotations_UserId"
    on public."TeachingMaterialAnnotations" ("UserId");

create table public."TeachingMaterialBookmarks"
(
    "Id"                 uuid                     not null
        constraint "PK_TeachingMaterialBookmarks"
            primary key,
    "TeachingMaterialId" uuid                     not null
        constraint "FK_TeachingMaterialBookmarks_TeachingMaterials_TeachingMateria~"
            references public."TeachingMaterials"
            on delete cascade,
    "UserId"             uuid                     not null
        constraint "FK_TeachingMaterialBookmarks_Users_UserId"
            references public."Users"
            on delete cascade,
    "Note"               varchar(500),
    "CreatedAt"          timestamp with time zone not null
);

alter table public."TeachingMaterialBookmarks"
    owner to postgres;

create unique index "IX_TeachingMaterialBookmarks_TeachingMaterialId_UserId"
    on public."TeachingMaterialBookmarks" ("TeachingMaterialId", "UserId");

create index "IX_TeachingMaterialBookmarks_UserId"
    on public."TeachingMaterialBookmarks" ("UserId");

create table public."TeachingMaterialSlides"
(
    "Id"                 uuid                     not null
        constraint "PK_TeachingMaterialSlides"
            primary key,
    "TeachingMaterialId" uuid                     not null
        constraint "FK_TeachingMaterialSlides_TeachingMaterials_TeachingMaterialId"
            references public."TeachingMaterials"
            on delete cascade,
    "SlideNumber"        integer                  not null,
    "PreviewImagePath"   varchar(500)             not null,
    "ThumbnailImagePath" varchar(500)             not null,
    "Width"              integer default 1920     not null,
    "Height"             integer default 1080     not null,
    "Notes"              text,
    "GeneratedAt"        timestamp with time zone not null
);

alter table public."TeachingMaterialSlides"
    owner to postgres;

create index "IX_TeachingMaterialSlides_TeachingMaterialId"
    on public."TeachingMaterialSlides" ("TeachingMaterialId");

create unique index "IX_TeachingMaterialSlides_TeachingMaterialId_SlideNumber"
    on public."TeachingMaterialSlides" ("TeachingMaterialId", "SlideNumber");

create table public."TeachingMaterialViewProgresses"
(
    "Id"                 uuid                     not null
        constraint "PK_TeachingMaterialViewProgresses"
            primary key,
    "TeachingMaterialId" uuid                     not null
        constraint "FK_TeachingMaterialViewProgresses_TeachingMaterials_TeachingMa~"
            references public."TeachingMaterials"
            on delete cascade,
    "UserId"             uuid                     not null
        constraint "FK_TeachingMaterialViewProgresses_Users_UserId"
            references public."Users"
            on delete cascade,
    "ProgressPercent"    integer default 0        not null
        constraint "CK_ViewProgress_Percent"
            check (("ProgressPercent" >= 0) AND ("ProgressPercent" <= 100)),
    "LastSlideViewed"    integer,
    "TotalTimeSeconds"   integer default 0        not null,
    "ViewCount"          integer default 1        not null,
    "Completed"          boolean default false    not null,
    "FirstViewedAt"      timestamp with time zone not null,
    "LastViewedAt"       timestamp with time zone not null
);

alter table public."TeachingMaterialViewProgresses"
    owner to postgres;

create index "IX_TeachingMaterialViewProgresses_Completed"
    on public."TeachingMaterialViewProgresses" ("Completed");

create index "IX_TeachingMaterialViewProgresses_TeachingMaterialId"
    on public."TeachingMaterialViewProgresses" ("TeachingMaterialId");

create unique index "IX_TeachingMaterialViewProgresses_TeachingMaterialId_UserId"
    on public."TeachingMaterialViewProgresses" ("TeachingMaterialId", "UserId");

create index "IX_TeachingMaterialViewProgresses_UserId"
    on public."TeachingMaterialViewProgresses" ("UserId");

create table public."MissionRewardRules"
(
    "Id"            uuid                                           not null
        constraint "PK_MissionRewardRules"
            primary key,
    "MissionType"   varchar(50)                                    not null,
    "ProgressMode"  varchar(20) default 'Count'::character varying not null,
    "TotalRequired" integer                                        not null,
    "RewardStars"   integer                                        not null,
    "RewardExp"     integer                                        not null,
    "IsActive"      boolean                                        not null,
    "CreatedAt"     timestamp with time zone                       not null,
    "UpdatedAt"     timestamp with time zone                       not null
);

alter table public."MissionRewardRules"
    owner to postgres;

create unique index "IX_MissionRewardRules_MissionType_ProgressMode_TotalRequired"
    on public."MissionRewardRules" ("MissionType", "ProgressMode", "TotalRequired");

create table public."TeacherCompensationSettings"
(
    "Id"                                  integer generated by default as identity
        constraint "PK_TeacherCompensationSettings"
            primary key,
    "StandardSessionDurationMinutes"      integer                  not null,
    "ForeignTeacherDefaultSessionRate"    numeric                  not null,
    "VietnameseTeacherDefaultSessionRate" numeric                  not null,
    "AssistantDefaultSessionRate"         numeric                  not null,
    "CreatedAt"                           timestamp with time zone not null,
    "UpdatedAt"                           timestamp with time zone
);

alter table public."TeacherCompensationSettings"
    owner to postgres;

create table public."ClassEnrollmentScheduleSegments"
(
    "Id"                      uuid                     not null
        constraint "PK_ClassEnrollmentScheduleSegments"
            primary key,
    "ClassEnrollmentId"       uuid                     not null
        constraint "FK_ClassEnrollmentScheduleSegments_ClassEnrollments_ClassEnrol~"
            references public."ClassEnrollments"
            on delete cascade,
    "EffectiveFrom"           date                     not null,
    "EffectiveTo"             date,
    "SessionSelectionPattern" text,
    "CreatedAt"               timestamp with time zone not null,
    "UpdatedAt"               timestamp with time zone not null
);

alter table public."ClassEnrollmentScheduleSegments"
    owner to postgres;

create unique index "IX_ClassEnrollmentScheduleSegments_ClassEnrollmentId_Effective~"
    on public."ClassEnrollmentScheduleSegments" ("ClassEnrollmentId", "EffectiveFrom");

create table public."ClassScheduleSegments"
(
    "Id"              uuid                     not null
        constraint "PK_ClassScheduleSegments"
            primary key,
    "ClassId"         uuid                     not null
        constraint "FK_ClassScheduleSegments_Classes_ClassId"
            references public."Classes"
            on delete cascade,
    "EffectiveFrom"   date                     not null,
    "EffectiveTo"     date,
    "SchedulePattern" text                     not null,
    "CreatedAt"       timestamp with time zone not null,
    "UpdatedAt"       timestamp with time zone not null
);

alter table public."ClassScheduleSegments"
    owner to postgres;

create unique index "IX_ClassScheduleSegments_ClassId_EffectiveFrom"
    on public."ClassScheduleSegments" ("ClassId", "EffectiveFrom");

create table public."EnrollmentConfirmationPdfs"
(
    "Id"             uuid                     not null
        constraint "PK_EnrollmentConfirmationPdfs"
            primary key,
    "RegistrationId" uuid                     not null
        constraint "FK_EnrollmentConfirmationPdfs_Registrations_RegistrationId"
            references public."Registrations"
            on delete cascade,
    "EnrollmentId"   uuid                     not null
        constraint "FK_EnrollmentConfirmationPdfs_ClassEnrollments_EnrollmentId"
            references public."ClassEnrollments"
            on delete cascade,
    "Track"          varchar(20)              not null,
    "FormType"       varchar(30)              not null,
    "PdfUrl"         varchar(1000)            not null,
    "GeneratedAt"    timestamp with time zone not null,
    "GeneratedBy"    uuid,
    "IsActive"       boolean                  not null,
    "SnapshotJson"   jsonb
);

alter table public."EnrollmentConfirmationPdfs"
    owner to postgres;

create index "IX_EnrollmentConfirmationPdfs_EnrollmentId"
    on public."EnrollmentConfirmationPdfs" ("EnrollmentId");

create index "IX_EnrollmentConfirmationPdfs_EnrollmentId_Track_FormType_IsAc~"
    on public."EnrollmentConfirmationPdfs" ("EnrollmentId", "Track", "FormType", "IsActive");

create index "IX_EnrollmentConfirmationPdfs_GeneratedAt"
    on public."EnrollmentConfirmationPdfs" ("GeneratedAt");

create index "IX_EnrollmentConfirmationPdfs_RegistrationId"
    on public."EnrollmentConfirmationPdfs" ("RegistrationId");

create table public."EnrollmentConfirmationPaymentSettings"
(
    "Id"                    uuid                     not null
        constraint "PK_EnrollmentConfirmationPaymentSettings"
            primary key,
    "BranchId"              uuid
        constraint "FK_EnrollmentConfirmationPaymentSettings_Branches_BranchId"
            references public."Branches"
            on delete restrict,
    "ScopeKey"              varchar(64)              not null,
    "PaymentMethod"         varchar(100)             not null,
    "AccountName"           varchar(200)             not null,
    "AccountNumber"         varchar(50)              not null,
    "BankName"              varchar(200)             not null,
    "BankCode"              varchar(50),
    "BankBin"               varchar(20),
    "VietQrTemplate"        varchar(50)              not null,
    "LogoUrl"               varchar(1000),
    "IsActive"              boolean                  not null,
    "CreatedAt"             timestamp with time zone not null,
    "UpdatedAt"             timestamp with time zone not null,
    "UpdatedBy"             uuid,
    "NewStudentPolicyText"  text,
    "ReservationPolicyText" text
);

alter table public."EnrollmentConfirmationPaymentSettings"
    owner to postgres;

create index "IX_EnrollmentConfirmationPaymentSettings_BranchId"
    on public."EnrollmentConfirmationPaymentSettings" ("BranchId");

create unique index "IX_EnrollmentConfirmationPaymentSettings_ScopeKey"
    on public."EnrollmentConfirmationPaymentSettings" ("ScopeKey");

create table public."BranchPrograms"
(
    "Id"                   uuid                     not null
        constraint "PK_BranchPrograms"
            primary key,
    "BranchId"             uuid                     not null
        constraint "FK_BranchPrograms_Branches_BranchId"
            references public."Branches"
            on delete restrict,
    "ProgramId"            uuid                     not null
        constraint "FK_BranchPrograms_Programs_ProgramId"
            references public."Programs"
            on delete restrict,
    "IsActive"             boolean                  not null,
    "DefaultMakeupClassId" uuid
        constraint "FK_BranchPrograms_Classes_DefaultMakeupClassId"
            references public."Classes"
            on delete restrict,
    "CreatedAt"            timestamp with time zone not null,
    "UpdatedAt"            timestamp with time zone not null
);

alter table public."BranchPrograms"
    owner to postgres;

create unique index "IX_BranchPrograms_BranchId_ProgramId"
    on public."BranchPrograms" ("BranchId", "ProgramId");

create index "IX_BranchPrograms_DefaultMakeupClassId"
    on public."BranchPrograms" ("DefaultMakeupClassId");

create index "IX_BranchPrograms_ProgramId"
    on public."BranchPrograms" ("ProgramId");

create table public."FaqCategories"
(
    "Id"        uuid                     not null
        constraint "PK_FaqCategories"
            primary key,
    "Name"      varchar(200)             not null,
    "Icon"      varchar(100),
    "SortOrder" integer                  not null,
    "IsActive"  boolean                  not null,
    "IsDeleted" boolean                  not null,
    "CreatedAt" timestamp with time zone not null,
    "UpdatedAt" timestamp with time zone not null
);

alter table public."FaqCategories"
    owner to postgres;

create index "IX_FaqCategories_IsDeleted_IsActive_SortOrder"
    on public."FaqCategories" ("IsDeleted", "IsActive", "SortOrder");

create table public."FaqItems"
(
    "Id"          uuid                     not null
        constraint "PK_FaqItems"
            primary key,
    "CategoryId"  uuid                     not null
        constraint "FK_FaqItems_FaqCategories_CategoryId"
            references public."FaqCategories"
            on delete restrict,
    "Question"    varchar(500)             not null,
    "Answer"      text                     not null,
    "SortOrder"   integer                  not null,
    "IsPublished" boolean                  not null,
    "IsDeleted"   boolean                  not null,
    "PublishedAt" timestamp with time zone,
    "CreatedAt"   timestamp with time zone not null,
    "UpdatedAt"   timestamp with time zone not null
);

alter table public."FaqItems"
    owner to postgres;

create index "IX_FaqItems_CategoryId_SortOrder"
    on public."FaqItems" ("CategoryId", "SortOrder");

create index "IX_FaqItems_IsPublished_IsDeleted"
    on public."FaqItems" ("IsPublished", "IsDeleted");

create table public."LandingPageSettings"
(
    "Id"                              integer                  not null
        constraint "PK_LandingPageSettings"
            primary key,
    "LogoUrl"                         varchar(1000),
    "FooterAddress"                   varchar(500),
    "FooterContactPhone"              varchar(100),
    "FooterContactEmail"              varchar(255),
    "FeaturedProgramIdsJson"          text                     not null,
    "FeaturedClassIdsJson"            text                     not null,
    "FeaturedTeacherIdsJson"          text                     not null,
    "CreatedAt"                       timestamp with time zone not null,
    "UpdatedAt"                       timestamp with time zone,
    "FeaturedClassesSectionSubtitle"  varchar(1000),
    "FeaturedClassesSectionTitle"     varchar(255),
    "FeaturedClassConfigsJson"        text default '[]'::text  not null,
    "FeaturedProgramsSectionSubtitle" varchar(1000),
    "FeaturedProgramsSectionTitle"    varchar(255),
    "FeaturedProgramConfigsJson"      text default '[]'::text  not null,
    "FeaturedTeachersSectionSubtitle" varchar(1000),
    "FeaturedTeachersSectionTitle"    varchar(255),
    "FooterAddressesJson"             text default '[]'::text  not null,
    "FooterSocialLinksJson"           text default '[]'::text  not null,
    "FooterContactPhonesJson"         text default '[]'::text  not null
);

alter table public."LandingPageSettings"
    owner to postgres;



### 2.2. Script tao du lieu demo cho ung dung

-- Demo seed for the current Kidzgo BE schema.
-- Safe characteristics:
-- 1. Idempotent: uses fixed demo IDs and upserts.
-- 2. Non-destructive: does not truncate existing business data.
-- 3. Self-contained: creates a dedicated demo branch, programs, classes, users,
--    profiles, registrations, sessions, attendances, and notifications.
--
-- Suggested usage:
-- 1. Apply the latest EF Core migrations first.
-- 2. Run this script on the target PostgreSQL database in schema `public`.
--
-- Demo credentials:
-- - Password for all demo users: Password123!
-- - PIN for admin/staff/teacher users: 1234
-- - PIN for the parent profile: 1234
--
-- Demo accounts:
-- - admin.demo@kidzgo.local
-- - management.demo@kidzgo.local
-- - accountant.demo@kidzgo.local
-- - teacher.main.demo@kidzgo.local
-- - teacher.assistant.demo@kidzgo.local
-- - parent.demo@kidzgo.local
-- - student.an.demo@kidzgo.local
-- - student.binh.demo@kidzgo.local

BEGIN;

SELECT pg_advisory_xact_lock(62425002);

CREATE TEMP TABLE demo_context AS
SELECT
    timezone('UTC', now()) AS now_utc,
    CURRENT_DATE::date AS today,
    (CURRENT_DATE - INTERVAL '21 days')::date AS class_start_date;

-- Fixed hashes produced with the same PBKDF2-SHA512 scheme used by the BE.
-- Password123! + salt 0123456789ABCDEF0123456789ABCDEF
-- 1234 + salt ABCDEF0123456789ABCDEF0123456789
CREATE TEMP TABLE demo_hashes AS
SELECT
    'DE479F92E6B1E906ECE5CBB756062EDC6F680786DF32A1BE3551E1499DEBABD9-0123456789ABCDEF0123456789ABCDEF'::text AS password_hash,
    'F6DDB6A2352010187D0957FC9B749A01BD9131D7D13BA1962D596FEDBDB993C1-ABCDEF0123456789ABCDEF0123456789'::text AS pin_hash;

INSERT INTO public."Branches"
(
    "Id",
    "Code",
    "Name",
    "Address",
    "ContactPhone",
    "ContactEmail",
    "IsActive",
    "CreatedAt",
    "UpdatedAt"
)
SELECT
    '90000000-0000-0000-0000-000000000001'::uuid,
    'DEMO-HCM',
    'Kidzgo Demo Branch HCM',
    '123 Demo Street, District 1, Ho Chi Minh City',
    '0900000001',
    'branch.demo@kidzgo.local',
    TRUE,
    c.now_utc,
    c.now_utc
FROM demo_context c
ON CONFLICT ("Id") DO UPDATE
SET
    "Code" = EXCLUDED."Code",
    "Name" = EXCLUDED."Name",
    "Address" = EXCLUDED."Address",
    "ContactPhone" = EXCLUDED."ContactPhone",
    "ContactEmail" = EXCLUDED."ContactEmail",
    "IsActive" = EXCLUDED."IsActive",
    "UpdatedAt" = EXCLUDED."UpdatedAt";

INSERT INTO public."Programs"
(
    "Id",
    "Name",
    "Code",
    "Description",
    "IsActive",
    "IsDeleted",
    "IsMakeup",
    "IsSupplementary",
    "CreatedAt",
    "UpdatedAt"
)
SELECT
    v.program_id,
    v.name,
    v.code,
    v.description,
    TRUE,
    FALSE,
    FALSE,
    FALSE,
    c.now_utc,
    c.now_utc
FROM demo_context c
CROSS JOIN
(
    VALUES
        (
            '90000000-0000-0000-0000-000000000101'::uuid,
            'DSTAR',
            'Demo Starters',
            'Primary demo program for parent-student flow, registration, sessions, and notifications.'
        ),
        (
            '90000000-0000-0000-0000-000000000102'::uuid,
            'DMOVE',
            'Demo Movers',
            'Secondary demo program used to test a low-remaining-session registration.'
        )
) AS v(program_id, code, name, description)
ON CONFLICT ("Id") DO UPDATE
SET
    "Name" = EXCLUDED."Name",
    "Code" = EXCLUDED."Code",
    "Description" = EXCLUDED."Description",
    "IsActive" = EXCLUDED."IsActive",
    "IsDeleted" = EXCLUDED."IsDeleted",
    "IsMakeup" = EXCLUDED."IsMakeup",
    "IsSupplementary" = EXCLUDED."IsSupplementary",
    "UpdatedAt" = EXCLUDED."UpdatedAt";

INSERT INTO public."BranchPrograms"
(
    "Id",
    "BranchId",
    "ProgramId",
    "IsActive",
    "DefaultMakeupClassId",
    "CreatedAt",
    "UpdatedAt"
)
SELECT
    v.id,
    '90000000-0000-0000-0000-000000000001'::uuid,
    v.program_id,
    TRUE,
    NULL,
    c.now_utc,
    c.now_utc
FROM demo_context c
CROSS JOIN
(
    VALUES
        ('90000000-0000-0000-0000-000000000201'::uuid, '90000000-0000-0000-0000-000000000101'::uuid),
        ('90000000-0000-0000-0000-000000000202'::uuid, '90000000-0000-0000-0000-000000000102'::uuid)
) AS v(id, program_id)
ON CONFLICT ("BranchId", "ProgramId") DO UPDATE
SET
    "IsActive" = EXCLUDED."IsActive",
    "DefaultMakeupClassId" = EXCLUDED."DefaultMakeupClassId",
    "UpdatedAt" = EXCLUDED."UpdatedAt";

INSERT INTO public."ProgramLeavePolicies"
(
    "Id",
    "ProgramId",
    "MaxLeavesPerMonth",
    "UpdatedBy",
    "CreatedAt",
    "UpdatedAt"
)
SELECT
    v.id,
    v.program_id,
    v.max_leaves_per_month,
    NULL,
    c.now_utc,
    c.now_utc
FROM demo_context c
CROSS JOIN
(
    VALUES
        ('90000000-0000-0000-0000-000000000301'::uuid, '90000000-0000-0000-0000-000000000101'::uuid, 2),
        ('90000000-0000-0000-0000-000000000302'::uuid, '90000000-0000-0000-0000-000000000102'::uuid, 2)
) AS v(id, program_id, max_leaves_per_month)
ON CONFLICT ("ProgramId") DO UPDATE
SET
    "MaxLeavesPerMonth" = EXCLUDED."MaxLeavesPerMonth",
    "UpdatedBy" = EXCLUDED."UpdatedBy",
    "UpdatedAt" = EXCLUDED."UpdatedAt";

INSERT INTO public."TuitionPlans"
(
    "Id",
    "BranchId",
    "ProgramId",
    "Name",
    "TotalSessions",
    "TuitionAmount",
    "UnitPriceSession",
    "Currency",
    "IsActive",
    "IsDeleted",
    "CreatedAt",
    "UpdatedAt"
)
SELECT
    v.id,
    '90000000-0000-0000-0000-000000000001'::uuid,
    v.program_id,
    v.name,
    v.total_sessions,
    v.tuition_amount,
    v.unit_price_session,
    'VND',
    TRUE,
    FALSE,
    c.now_utc,
    c.now_utc
FROM demo_context c
CROSS JOIN
(
    VALUES
        (
            '90000000-0000-0000-0000-000000000401'::uuid,
            '90000000-0000-0000-0000-000000000101'::uuid,
            'Demo Starters 6 Sessions',
            6,
            1500000::numeric,
            250000::numeric
        ),
        (
            '90000000-0000-0000-0000-000000000402'::uuid,
            '90000000-0000-0000-0000-000000000102'::uuid,
            'Demo Movers 4 Sessions',
            4,
            1200000::numeric,
            300000::numeric
        )
) AS v(id, program_id, name, total_sessions, tuition_amount, unit_price_session)
ON CONFLICT ("Id") DO UPDATE
SET
    "BranchId" = EXCLUDED."BranchId",
    "ProgramId" = EXCLUDED."ProgramId",
    "Name" = EXCLUDED."Name",
    "TotalSessions" = EXCLUDED."TotalSessions",
    "TuitionAmount" = EXCLUDED."TuitionAmount",
    "UnitPriceSession" = EXCLUDED."UnitPriceSession",
    "Currency" = EXCLUDED."Currency",
    "IsActive" = EXCLUDED."IsActive",
    "IsDeleted" = EXCLUDED."IsDeleted",
    "UpdatedAt" = EXCLUDED."UpdatedAt";

INSERT INTO public."Classrooms"
(
    "Id",
    "BranchId",
    "Name",
    "Capacity",
    "Note",
    "Floor",
    "Area",
    "EquipmentJson",
    "IsActive"
)
VALUES
(
    '90000000-0000-0000-0000-000000000501'::uuid,
    '90000000-0000-0000-0000-000000000001'::uuid,
    'Demo Room 01',
    12,
    'Main demo classroom for Demo Starters.',
    '2',
    32,
    '["TV","Whiteboard","Projector"]',
    TRUE
),
(
    '90000000-0000-0000-0000-000000000502'::uuid,
    '90000000-0000-0000-0000-000000000001'::uuid,
    'Demo Room 02',
    12,
    'Main demo classroom for Demo Movers.',
    '2',
    28,
    '["TV","Whiteboard"]',
    TRUE
)
ON CONFLICT ("Id") DO UPDATE
SET
    "BranchId" = EXCLUDED."BranchId",
    "Name" = EXCLUDED."Name",
    "Capacity" = EXCLUDED."Capacity",
    "Note" = EXCLUDED."Note",
    "Floor" = EXCLUDED."Floor",
    "Area" = EXCLUDED."Area",
    "EquipmentJson" = EXCLUDED."EquipmentJson",
    "IsActive" = EXCLUDED."IsActive";

INSERT INTO public."Users"
(
    "Id",
    "Email",
    "PasswordHash",
    "Role",
    "Username",
    "Name",
    "PhoneNumber",
    "PinHash",
    "AvatarUrl",
    "AvatarMimeType",
    "AvatarFileSize",
    "TeacherCompensationType",
    "BranchId",
    "IsActive",
    "IsDeleted",
    "LastLoginAt",
    "LastSeenAt",
    "CreatedAt",
    "UpdatedAt"
)
SELECT
    v.id,
    v.email,
    h.password_hash,
    v.role,
    v.username,
    v.name,
    v.phone_number,
    CASE WHEN v.requires_pin THEN h.pin_hash ELSE NULL END,
    NULL,
    NULL,
    NULL,
    NULL,
    v.branch_id,
    TRUE,
    FALSE,
    NULL,
    NULL,
    c.now_utc,
    c.now_utc
FROM demo_context c
CROSS JOIN demo_hashes h
CROSS JOIN
(
    VALUES
        (
            '90000000-0000-0000-0000-000000000601'::uuid,
            'admin.demo@kidzgo.local',
            'Admin',
            'admin.demo',
            'Demo Admin',
            '0901000001',
            FALSE,
            NULL::uuid
        ),
        (
            '90000000-0000-0000-0000-000000000602'::uuid,
            'management.demo@kidzgo.local',
            'ManagementStaff',
            'management.demo',
            'Demo Management Staff',
            '0901000002',
            TRUE,
            '90000000-0000-0000-0000-000000000001'::uuid
        ),
        (
            '90000000-0000-0000-0000-000000000603'::uuid,
            'accountant.demo@kidzgo.local',
            'AccountantStaff',
            'accountant.demo',
            'Demo Accountant Staff',
            '0901000003',
            TRUE,
            '90000000-0000-0000-0000-000000000001'::uuid
        ),
        (
            '90000000-0000-0000-0000-000000000604'::uuid,
            'teacher.main.demo@kidzgo.local',
            'Teacher',
            'teacher.main.demo',
            'Demo Main Teacher',
            '0901000004',
            TRUE,
            '90000000-0000-0000-0000-000000000001'::uuid
        ),
        (
            '90000000-0000-0000-0000-000000000605'::uuid,
            'teacher.assistant.demo@kidzgo.local',
            'Teacher',
            'teacher.assistant.demo',
            'Demo Assistant Teacher',
            '0901000005',
            TRUE,
            '90000000-0000-0000-0000-000000000001'::uuid
        ),
        (
            '90000000-0000-0000-0000-000000000606'::uuid,
            'parent.demo@kidzgo.local',
            'Parent',
            'parent.demo',
            'Demo Parent',
            '0901000006',
            FALSE,
            NULL::uuid
        ),
        (
            '90000000-0000-0000-0000-000000000607'::uuid,
            'student.an.demo@kidzgo.local',
            'Student',
            'student.an.demo',
            'Nguyen Minh An',
            '0901000007',
            FALSE,
            NULL::uuid
        ),
        (
            '90000000-0000-0000-0000-000000000608'::uuid,
            'student.binh.demo@kidzgo.local',
            'Student',
            'student.binh.demo',
            'Tran Gia Binh',
            '0901000008',
            FALSE,
            NULL::uuid
        )
) AS v(id, email, role, username, name, phone_number, requires_pin, branch_id)
ON CONFLICT ("Email") DO UPDATE
SET
    "PasswordHash" = EXCLUDED."PasswordHash",
    "Role" = EXCLUDED."Role",
    "Username" = EXCLUDED."Username",
    "Name" = EXCLUDED."Name",
    "PhoneNumber" = EXCLUDED."PhoneNumber",
    "PinHash" = EXCLUDED."PinHash",
    "TeacherCompensationType" = EXCLUDED."TeacherCompensationType",
    "BranchId" = EXCLUDED."BranchId",
    "IsActive" = EXCLUDED."IsActive",
    "IsDeleted" = EXCLUDED."IsDeleted",
    "UpdatedAt" = EXCLUDED."UpdatedAt";

INSERT INTO public."Profiles"
(
    "Id",
    "UserId",
    "ProfileType",
    "DisplayName",
    "Name",
    "Gender",
    "DateOfBirth",
    "ZaloId",
    "PinHash",
    "AvatarUrl",
    "AvatarMimeType",
    "AvatarFileSize",
    "IsApproved",
    "IsActive",
    "IsDeleted",
    "LastLoginAt",
    "LastSeenAt",
    "CreatedAt",
    "UpdatedAt"
)
SELECT
    v.id,
    v.user_id,
    v.profile_type,
    v.display_name,
    v.name,
    v.gender,
    v.date_of_birth,
    NULL,
    CASE WHEN v.profile_type = 'Parent' THEN h.pin_hash ELSE NULL END,
    NULL,
    NULL,
    NULL,
    TRUE,
    TRUE,
    FALSE,
    NULL,
    NULL,
    c.now_utc,
    c.now_utc
FROM demo_context c
CROSS JOIN demo_hashes h
CROSS JOIN
(
    VALUES
        (
            '90000000-0000-0000-0000-000000000701'::uuid,
            '90000000-0000-0000-0000-000000000606'::uuid,
            'Parent',
            'Demo Parent',
            'Le Thi Demo',
            'Female',
            '1990-06-15'::date
        ),
        (
            '90000000-0000-0000-0000-000000000702'::uuid,
            '90000000-0000-0000-0000-000000000607'::uuid,
            'Student',
            'Nguyen Minh An',
            'Nguyen Minh An',
            'Male',
            '2016-03-12'::date
        ),
        (
            '90000000-0000-0000-0000-000000000703'::uuid,
            '90000000-0000-0000-0000-000000000608'::uuid,
            'Student',
            'Tran Gia Binh',
            'Tran Gia Binh',
            'Male',
            '2015-08-23'::date
        )
) AS v(id, user_id, profile_type, display_name, name, gender, date_of_birth)
ON CONFLICT ("Id") DO UPDATE
SET
    "UserId" = EXCLUDED."UserId",
    "ProfileType" = EXCLUDED."ProfileType",
    "DisplayName" = EXCLUDED."DisplayName",
    "Name" = EXCLUDED."Name",
    "Gender" = EXCLUDED."Gender",
    "DateOfBirth" = EXCLUDED."DateOfBirth",
    "ZaloId" = EXCLUDED."ZaloId",
    "PinHash" = EXCLUDED."PinHash",
    "IsApproved" = EXCLUDED."IsApproved",
    "IsActive" = EXCLUDED."IsActive",
    "IsDeleted" = EXCLUDED."IsDeleted",
    "UpdatedAt" = EXCLUDED."UpdatedAt";

INSERT INTO public."ParentStudentLinks"
(
    "Id",
    "ParentProfileId",
    "StudentProfileId",
    "CreatedAt"
)
SELECT
    v.id,
    '90000000-0000-0000-0000-000000000701'::uuid,
    v.student_profile_id,
    c.now_utc
FROM demo_context c
CROSS JOIN
(
    VALUES
        ('90000000-0000-0000-0000-000000000801'::uuid, '90000000-0000-0000-0000-000000000702'::uuid),
        ('90000000-0000-0000-0000-000000000802'::uuid, '90000000-0000-0000-0000-000000000703'::uuid)
) AS v(id, student_profile_id)
ON CONFLICT ("Id") DO UPDATE
SET
    "ParentProfileId" = EXCLUDED."ParentProfileId",
    "StudentProfileId" = EXCLUDED."StudentProfileId";

INSERT INTO public."Classes"
(
    "Id",
    "BranchId",
    "ProgramId",
    "Code",
    "Title",
    "RoomId",
    "MainTeacherId",
    "AssistantTeacherId",
    "StartDate",
    "EndDate",
    "Status",
    "Capacity",
    "SchedulePattern",
    "Description",
    "CreatedAt",
    "UpdatedAt"
)
SELECT
    v.id,
    '90000000-0000-0000-0000-000000000001'::uuid,
    v.program_id,
    v.code,
    v.title,
    v.room_id,
    '90000000-0000-0000-0000-000000000604'::uuid,
    '90000000-0000-0000-0000-000000000605'::uuid,
    c.class_start_date,
    NULL,
    'Active',
    12,
    v.schedule_pattern,
    v.description,
    c.now_utc,
    c.now_utc
FROM demo_context c
CROSS JOIN
(
    VALUES
        (
            '90000000-0000-0000-0000-000000000901'::uuid,
            '90000000-0000-0000-0000-000000000101'::uuid,
            '90000000-0000-0000-0000-000000000501'::uuid,
            'DEMO-STA-A',
            'Demo Starters A',
            '{"type":"weekly-pattern","entries":[{"dayOfWeeks":["TU"],"startTime":"18:00","durationMinutes":90}]}',
            'Demo class for a student with 3 remaining sessions.'
        ),
        (
            '90000000-0000-0000-0000-000000000902'::uuid,
            '90000000-0000-0000-0000-000000000102'::uuid,
            '90000000-0000-0000-0000-000000000502'::uuid,
            'DEMO-MOV-A',
            'Demo Movers A',
            '{"type":"weekly-pattern","entries":[{"dayOfWeeks":["TH"],"startTime":"19:00","durationMinutes":90}]}',
            'Demo class for a student with 1 remaining session.'
        )
) AS v(id, program_id, room_id, code, title, schedule_pattern, description)
ON CONFLICT ("Code") DO UPDATE
SET
    "BranchId" = EXCLUDED."BranchId",
    "ProgramId" = EXCLUDED."ProgramId",
    "Title" = EXCLUDED."Title",
    "RoomId" = EXCLUDED."RoomId",
    "MainTeacherId" = EXCLUDED."MainTeacherId",
    "AssistantTeacherId" = EXCLUDED."AssistantTeacherId",
    "StartDate" = EXCLUDED."StartDate",
    "EndDate" = EXCLUDED."EndDate",
    "Status" = EXCLUDED."Status",
    "Capacity" = EXCLUDED."Capacity",
    "SchedulePattern" = EXCLUDED."SchedulePattern",
    "Description" = EXCLUDED."Description",
    "UpdatedAt" = EXCLUDED."UpdatedAt";

INSERT INTO public."ClassScheduleSegments"
(
    "Id",
    "ClassId",
    "EffectiveFrom",
    "EffectiveTo",
    "SchedulePattern",
    "CreatedAt",
    "UpdatedAt"
)
SELECT
    v.id,
    v.class_id,
    c.class_start_date,
    NULL,
    v.schedule_pattern,
    c.now_utc,
    c.now_utc
FROM demo_context c
CROSS JOIN
(
    VALUES
        (
            '90000000-0000-0000-0000-000000001001'::uuid,
            '90000000-0000-0000-0000-000000000901'::uuid,
            '{"type":"weekly-pattern","entries":[{"dayOfWeeks":["TU"],"startTime":"18:00","durationMinutes":90}]}'
        ),
        (
            '90000000-0000-0000-0000-000000001002'::uuid,
            '90000000-0000-0000-0000-000000000902'::uuid,
            '{"type":"weekly-pattern","entries":[{"dayOfWeeks":["TH"],"startTime":"19:00","durationMinutes":90}]}'
        )
) AS v(id, class_id, schedule_pattern)
ON CONFLICT ("ClassId", "EffectiveFrom") DO UPDATE
SET
    "EffectiveTo" = EXCLUDED."EffectiveTo",
    "SchedulePattern" = EXCLUDED."SchedulePattern",
    "UpdatedAt" = EXCLUDED."UpdatedAt";

INSERT INTO public."Registrations"
(
    "Id",
    "StudentProfileId",
    "BranchId",
    "ProgramId",
    "TuitionPlanId",
    "SecondaryProgramId",
    "RegistrationDate",
    "ExpectedStartDate",
    "ActualStartDate",
    "PreferredSchedule",
    "Note",
    "Status",
    "ClassId",
    "ClassAssignedDate",
    "EntryType",
    "SecondaryClassId",
    "SecondaryClassAssignedDate",
    "SecondaryEntryType",
    "SecondaryProgramSkillFocus",
    "OriginalRegistrationId",
    "OperationType",
    "TotalSessions",
    "UsedSessions",
    "RemainingSessions",
    "ExpiryDate",
    "CreatedAt",
    "UpdatedAt"
)
SELECT
    v.id,
    v.student_profile_id,
    '90000000-0000-0000-0000-000000000001'::uuid,
    v.program_id,
    v.tuition_plan_id,
    NULL,
    ((c.today - INTERVAL '28 days')::date)::timestamp,
    (c.today - INTERVAL '21 days')::date::timestamp,
    (c.today - INTERVAL '21 days')::date::timestamp,
    v.preferred_schedule,
    v.note,
    'Studying',
    v.class_id,
    ((c.today - INTERVAL '22 days')::date)::timestamp,
    NULL,
    NULL,
    NULL,
    NULL,
    NULL,
    NULL,
    NULL,
    v.total_sessions,
    v.used_sessions,
    v.remaining_sessions,
    ((c.today + INTERVAL '45 days')::date)::timestamp,
    c.now_utc,
    c.now_utc
FROM demo_context c
CROSS JOIN
(
    VALUES
        (
            '90000000-0000-0000-0000-000000001101'::uuid,
            '90000000-0000-0000-0000-000000000702'::uuid,
            '90000000-0000-0000-0000-000000000101'::uuid,
            '90000000-0000-0000-0000-000000000401'::uuid,
            '90000000-0000-0000-0000-000000000901'::uuid,
            6,
            3,
            3,
            'Tue 18:00',
            'Demo registration at low-session threshold 3.'
        ),
        (
            '90000000-0000-0000-0000-000000001102'::uuid,
            '90000000-0000-0000-0000-000000000703'::uuid,
            '90000000-0000-0000-0000-000000000102'::uuid,
            '90000000-0000-0000-0000-000000000402'::uuid,
            '90000000-0000-0000-0000-000000000902'::uuid,
            4,
            3,
            1,
            'Thu 19:00',
            'Demo registration at low-session threshold 1.'
        )
) AS v(id, student_profile_id, program_id, tuition_plan_id, class_id, total_sessions, used_sessions, remaining_sessions, preferred_schedule, note)
ON CONFLICT ("Id") DO UPDATE
SET
    "StudentProfileId" = EXCLUDED."StudentProfileId",
    "BranchId" = EXCLUDED."BranchId",
    "ProgramId" = EXCLUDED."ProgramId",
    "TuitionPlanId" = EXCLUDED."TuitionPlanId",
    "SecondaryProgramId" = EXCLUDED."SecondaryProgramId",
    "RegistrationDate" = EXCLUDED."RegistrationDate",
    "ExpectedStartDate" = EXCLUDED."ExpectedStartDate",
    "ActualStartDate" = EXCLUDED."ActualStartDate",
    "PreferredSchedule" = EXCLUDED."PreferredSchedule",
    "Note" = EXCLUDED."Note",
    "Status" = EXCLUDED."Status",
    "ClassId" = EXCLUDED."ClassId",
    "ClassAssignedDate" = EXCLUDED."ClassAssignedDate",
    "EntryType" = EXCLUDED."EntryType",
    "SecondaryClassId" = EXCLUDED."SecondaryClassId",
    "SecondaryClassAssignedDate" = EXCLUDED."SecondaryClassAssignedDate",
    "SecondaryEntryType" = EXCLUDED."SecondaryEntryType",
    "SecondaryProgramSkillFocus" = EXCLUDED."SecondaryProgramSkillFocus",
    "OriginalRegistrationId" = EXCLUDED."OriginalRegistrationId",
    "OperationType" = EXCLUDED."OperationType",
    "TotalSessions" = EXCLUDED."TotalSessions",
    "UsedSessions" = EXCLUDED."UsedSessions",
    "RemainingSessions" = EXCLUDED."RemainingSessions",
    "ExpiryDate" = EXCLUDED."ExpiryDate",
    "UpdatedAt" = EXCLUDED."UpdatedAt";

INSERT INTO public."ClassEnrollments"
(
    "Id",
    "ClassId",
    "StudentProfileId",
    "EnrollDate",
    "Status",
    "TuitionPlanId",
    "RegistrationId",
    "Track",
    "SessionSelectionPattern",
    "EnrollmentConfirmationPdfUrl",
    "EnrollmentConfirmationPdfGeneratedAt",
    "EnrollmentConfirmationPdfGeneratedBy",
    "CreatedAt",
    "UpdatedAt"
)
SELECT
    v.id,
    v.class_id,
    v.student_profile_id,
    c.class_start_date,
    'Active',
    v.tuition_plan_id,
    v.registration_id,
    'Primary',
    v.session_selection_pattern,
    NULL,
    NULL,
    NULL,
    c.now_utc,
    c.now_utc
FROM demo_context c
CROSS JOIN
(
    VALUES
        (
            '90000000-0000-0000-0000-000000001201'::uuid,
            '90000000-0000-0000-0000-000000000901'::uuid,
            '90000000-0000-0000-0000-000000000702'::uuid,
            '90000000-0000-0000-0000-000000000401'::uuid,
            '90000000-0000-0000-0000-000000001101'::uuid,
            '{"type":"weekly-pattern","entries":[{"dayOfWeeks":["TU"],"startTime":"18:00","durationMinutes":90}]}'
        ),
        (
            '90000000-0000-0000-0000-000000001202'::uuid,
            '90000000-0000-0000-0000-000000000902'::uuid,
            '90000000-0000-0000-0000-000000000703'::uuid,
            '90000000-0000-0000-0000-000000000402'::uuid,
            '90000000-0000-0000-0000-000000001102'::uuid,
            '{"type":"weekly-pattern","entries":[{"dayOfWeeks":["TH"],"startTime":"19:00","durationMinutes":90}]}'
        )
) AS v(id, class_id, student_profile_id, tuition_plan_id, registration_id, session_selection_pattern)
ON CONFLICT ("Id") DO UPDATE
SET
    "ClassId" = EXCLUDED."ClassId",
    "StudentProfileId" = EXCLUDED."StudentProfileId",
    "EnrollDate" = EXCLUDED."EnrollDate",
    "Status" = EXCLUDED."Status",
    "TuitionPlanId" = EXCLUDED."TuitionPlanId",
    "RegistrationId" = EXCLUDED."RegistrationId",
    "Track" = EXCLUDED."Track",
    "SessionSelectionPattern" = EXCLUDED."SessionSelectionPattern",
    "EnrollmentConfirmationPdfUrl" = EXCLUDED."EnrollmentConfirmationPdfUrl",
    "EnrollmentConfirmationPdfGeneratedAt" = EXCLUDED."EnrollmentConfirmationPdfGeneratedAt",
    "EnrollmentConfirmationPdfGeneratedBy" = EXCLUDED."EnrollmentConfirmationPdfGeneratedBy",
    "UpdatedAt" = EXCLUDED."UpdatedAt";

INSERT INTO public."ClassEnrollmentScheduleSegments"
(
    "Id",
    "ClassEnrollmentId",
    "EffectiveFrom",
    "EffectiveTo",
    "SessionSelectionPattern",
    "CreatedAt",
    "UpdatedAt"
)
SELECT
    v.id,
    v.class_enrollment_id,
    c.class_start_date,
    NULL,
    v.session_selection_pattern,
    c.now_utc,
    c.now_utc
FROM demo_context c
CROSS JOIN
(
    VALUES
        (
            '90000000-0000-0000-0000-000000001301'::uuid,
            '90000000-0000-0000-0000-000000001201'::uuid,
            '{"type":"weekly-pattern","entries":[{"dayOfWeeks":["TU"],"startTime":"18:00","durationMinutes":90}]}'
        ),
        (
            '90000000-0000-0000-0000-000000001302'::uuid,
            '90000000-0000-0000-0000-000000001202'::uuid,
            '{"type":"weekly-pattern","entries":[{"dayOfWeeks":["TH"],"startTime":"19:00","durationMinutes":90}]}'
        )
) AS v(id, class_enrollment_id, session_selection_pattern)
ON CONFLICT ("ClassEnrollmentId", "EffectiveFrom") DO UPDATE
SET
    "EffectiveTo" = EXCLUDED."EffectiveTo",
    "SessionSelectionPattern" = EXCLUDED."SessionSelectionPattern",
    "UpdatedAt" = EXCLUDED."UpdatedAt";

INSERT INTO public."Sessions"
(
    "Id",
    "ClassId",
    "BranchId",
    "PlannedDatetime",
    "PlannedRoomId",
    "PlannedTeacherId",
    "PlannedAssistantId",
    "DurationMinutes",
    "ParticipationType",
    "Status",
    "ActualDatetime",
    "ActualRoomId",
    "ActualTeacherId",
    "ActualAssistantId",
    "Color",
    "CreatedAt",
    "UpdatedAt"
)
SELECT
    v.id,
    v.class_id,
    '90000000-0000-0000-0000-000000000001'::uuid,
    v.planned_datetime,
    v.room_id,
    '90000000-0000-0000-0000-000000000604'::uuid,
    '90000000-0000-0000-0000-000000000605'::uuid,
    90,
    'Main',
    v.status,
    v.actual_datetime,
    CASE WHEN v.actual_datetime IS NULL THEN NULL ELSE v.room_id END,
    CASE WHEN v.actual_datetime IS NULL THEN NULL ELSE '90000000-0000-0000-0000-000000000604'::uuid END,
    CASE WHEN v.actual_datetime IS NULL THEN NULL ELSE '90000000-0000-0000-0000-000000000605'::uuid END,
    v.color,
    c.now_utc,
    c.now_utc
FROM demo_context c
CROSS JOIN
(
    VALUES
        (
            '90000000-0000-0000-0000-000000001401'::uuid,
            '90000000-0000-0000-0000-000000000901'::uuid,
            '90000000-0000-0000-0000-000000000501'::uuid,
            ((CURRENT_DATE - INTERVAL '21 days')::date::timestamp + TIME '18:00'),
            'Completed',
            ((CURRENT_DATE - INTERVAL '21 days')::date::timestamp + TIME '18:00'),
            '#2E86DE'
        ),
        (
            '90000000-0000-0000-0000-000000001402'::uuid,
            '90000000-0000-0000-0000-000000000901'::uuid,
            '90000000-0000-0000-0000-000000000501'::uuid,
            ((CURRENT_DATE - INTERVAL '14 days')::date::timestamp + TIME '18:00'),
            'Completed',
            ((CURRENT_DATE - INTERVAL '14 days')::date::timestamp + TIME '18:00'),
            '#2E86DE'
        ),
        (
            '90000000-0000-0000-0000-000000001403'::uuid,
            '90000000-0000-0000-0000-000000000901'::uuid,
            '90000000-0000-0000-0000-000000000501'::uuid,
            ((CURRENT_DATE - INTERVAL '7 days')::date::timestamp + TIME '18:00'),
            'Completed',
            ((CURRENT_DATE - INTERVAL '7 days')::date::timestamp + TIME '18:00'),
            '#2E86DE'
        ),
        (
            '90000000-0000-0000-0000-000000001404'::uuid,
            '90000000-0000-0000-0000-000000000901'::uuid,
            '90000000-0000-0000-0000-000000000501'::uuid,
            ((CURRENT_DATE + INTERVAL '7 days')::date::timestamp + TIME '18:00'),
            'Scheduled',
            NULL::timestamp,
            '#54A0FF'
        ),
        (
            '90000000-0000-0000-0000-000000001405'::uuid,
            '90000000-0000-0000-0000-000000000901'::uuid,
            '90000000-0000-0000-0000-000000000501'::uuid,
            ((CURRENT_DATE + INTERVAL '14 days')::date::timestamp + TIME '18:00'),
            'Scheduled',
            NULL::timestamp,
            '#54A0FF'
        ),
        (
            '90000000-0000-0000-0000-000000001406'::uuid,
            '90000000-0000-0000-0000-000000000901'::uuid,
            '90000000-0000-0000-0000-000000000501'::uuid,
            ((CURRENT_DATE + INTERVAL '21 days')::date::timestamp + TIME '18:00'),
            'Scheduled',
            NULL::timestamp,
            '#54A0FF'
        ),
        (
            '90000000-0000-0000-0000-000000001407'::uuid,
            '90000000-0000-0000-0000-000000000902'::uuid,
            '90000000-0000-0000-0000-000000000502'::uuid,
            ((CURRENT_DATE - INTERVAL '21 days')::date::timestamp + TIME '19:00'),
            'Completed',
            ((CURRENT_DATE - INTERVAL '21 days')::date::timestamp + TIME '19:00'),
            '#10AC84'
        ),
        (
            '90000000-0000-0000-0000-000000001408'::uuid,
            '90000000-0000-0000-0000-000000000902'::uuid,
            '90000000-0000-0000-0000-000000000502'::uuid,
            ((CURRENT_DATE - INTERVAL '14 days')::date::timestamp + TIME '19:00'),
            'Completed',
            ((CURRENT_DATE - INTERVAL '14 days')::date::timestamp + TIME '19:00'),
            '#10AC84'
        ),
        (
            '90000000-0000-0000-0000-000000001409'::uuid,
            '90000000-0000-0000-0000-000000000902'::uuid,
            '90000000-0000-0000-0000-000000000502'::uuid,
            ((CURRENT_DATE - INTERVAL '7 days')::date::timestamp + TIME '19:00'),
            'Completed',
            ((CURRENT_DATE - INTERVAL '7 days')::date::timestamp + TIME '19:00'),
            '#10AC84'
        ),
        (
            '90000000-0000-0000-0000-000000001410'::uuid,
            '90000000-0000-0000-0000-000000000902'::uuid,
            '90000000-0000-0000-0000-000000000502'::uuid,
            ((CURRENT_DATE + INTERVAL '7 days')::date::timestamp + TIME '19:00'),
            'Scheduled',
            NULL::timestamp,
            '#1DD1A1'
        )
) AS v(id, class_id, room_id, planned_datetime, status, actual_datetime, color)
ON CONFLICT ("Id") DO UPDATE
SET
    "ClassId" = EXCLUDED."ClassId",
    "BranchId" = EXCLUDED."BranchId",
    "PlannedDatetime" = EXCLUDED."PlannedDatetime",
    "PlannedRoomId" = EXCLUDED."PlannedRoomId",
    "PlannedTeacherId" = EXCLUDED."PlannedTeacherId",
    "PlannedAssistantId" = EXCLUDED."PlannedAssistantId",
    "DurationMinutes" = EXCLUDED."DurationMinutes",
    "ParticipationType" = EXCLUDED."ParticipationType",
    "Status" = EXCLUDED."Status",
    "ActualDatetime" = EXCLUDED."ActualDatetime",
    "ActualRoomId" = EXCLUDED."ActualRoomId",
    "ActualTeacherId" = EXCLUDED."ActualTeacherId",
    "ActualAssistantId" = EXCLUDED."ActualAssistantId",
    "Color" = EXCLUDED."Color",
    "UpdatedAt" = EXCLUDED."UpdatedAt";

INSERT INTO public."StudentSessionAssignments"
(
    "Id",
    "SessionId",
    "StudentProfileId",
    "ClassEnrollmentId",
    "RegistrationId",
    "Track",
    "Status",
    "CreatedAt",
    "UpdatedAt"
)
SELECT
    v.id,
    v.session_id,
    v.student_profile_id,
    v.class_enrollment_id,
    v.registration_id,
    'Primary',
    'Assigned',
    c.now_utc,
    c.now_utc
FROM demo_context c
CROSS JOIN
(
    VALUES
        ('90000000-0000-0000-0000-000000001501'::uuid, '90000000-0000-0000-0000-000000001401'::uuid, '90000000-0000-0000-0000-000000000702'::uuid, '90000000-0000-0000-0000-000000001201'::uuid, '90000000-0000-0000-0000-000000001101'::uuid),
        ('90000000-0000-0000-0000-000000001502'::uuid, '90000000-0000-0000-0000-000000001402'::uuid, '90000000-0000-0000-0000-000000000702'::uuid, '90000000-0000-0000-0000-000000001201'::uuid, '90000000-0000-0000-0000-000000001101'::uuid),
        ('90000000-0000-0000-0000-000000001503'::uuid, '90000000-0000-0000-0000-000000001403'::uuid, '90000000-0000-0000-0000-000000000702'::uuid, '90000000-0000-0000-0000-000000001201'::uuid, '90000000-0000-0000-0000-000000001101'::uuid),
        ('90000000-0000-0000-0000-000000001504'::uuid, '90000000-0000-0000-0000-000000001404'::uuid, '90000000-0000-0000-0000-000000000702'::uuid, '90000000-0000-0000-0000-000000001201'::uuid, '90000000-0000-0000-0000-000000001101'::uuid),
        ('90000000-0000-0000-0000-000000001505'::uuid, '90000000-0000-0000-0000-000000001405'::uuid, '90000000-0000-0000-0000-000000000702'::uuid, '90000000-0000-0000-0000-000000001201'::uuid, '90000000-0000-0000-0000-000000001101'::uuid),
        ('90000000-0000-0000-0000-000000001506'::uuid, '90000000-0000-0000-0000-000000001406'::uuid, '90000000-0000-0000-0000-000000000702'::uuid, '90000000-0000-0000-0000-000000001201'::uuid, '90000000-0000-0000-0000-000000001101'::uuid),
        ('90000000-0000-0000-0000-000000001507'::uuid, '90000000-0000-0000-0000-000000001407'::uuid, '90000000-0000-0000-0000-000000000703'::uuid, '90000000-0000-0000-0000-000000001202'::uuid, '90000000-0000-0000-0000-000000001102'::uuid),
        ('90000000-0000-0000-0000-000000001508'::uuid, '90000000-0000-0000-0000-000000001408'::uuid, '90000000-0000-0000-0000-000000000703'::uuid, '90000000-0000-0000-0000-000000001202'::uuid, '90000000-0000-0000-0000-000000001102'::uuid),
        ('90000000-0000-0000-0000-000000001509'::uuid, '90000000-0000-0000-0000-000000001409'::uuid, '90000000-0000-0000-0000-000000000703'::uuid, '90000000-0000-0000-0000-000000001202'::uuid, '90000000-0000-0000-0000-000000001102'::uuid),
        ('90000000-0000-0000-0000-000000001510'::uuid, '90000000-0000-0000-0000-000000001410'::uuid, '90000000-0000-0000-0000-000000000703'::uuid, '90000000-0000-0000-0000-000000001202'::uuid, '90000000-0000-0000-0000-000000001102'::uuid)
) AS v(id, session_id, student_profile_id, class_enrollment_id, registration_id)
ON CONFLICT ("SessionId", "ClassEnrollmentId") DO UPDATE
SET
    "StudentProfileId" = EXCLUDED."StudentProfileId",
    "RegistrationId" = EXCLUDED."RegistrationId",
    "Track" = EXCLUDED."Track",
    "Status" = EXCLUDED."Status",
    "UpdatedAt" = EXCLUDED."UpdatedAt";

INSERT INTO public."Attendances"
(
    "Id",
    "SessionId",
    "StudentProfileId",
    "AttendanceStatus",
    "AbsenceType",
    "MarkedBy",
    "MarkedAt",
    "Note"
)
SELECT
    v.id,
    v.session_id,
    v.student_profile_id,
    'Present',
    NULL,
    '90000000-0000-0000-0000-000000000604'::uuid,
    c.now_utc,
    v.note
FROM demo_context c
CROSS JOIN
(
    VALUES
        ('90000000-0000-0000-0000-000000001601'::uuid, '90000000-0000-0000-0000-000000001401'::uuid, '90000000-0000-0000-0000-000000000702'::uuid, 'Completed session 1 for Nguyen Minh An.'),
        ('90000000-0000-0000-0000-000000001602'::uuid, '90000000-0000-0000-0000-000000001402'::uuid, '90000000-0000-0000-0000-000000000702'::uuid, 'Completed session 2 for Nguyen Minh An.'),
        ('90000000-0000-0000-0000-000000001603'::uuid, '90000000-0000-0000-0000-000000001403'::uuid, '90000000-0000-0000-0000-000000000702'::uuid, 'Completed session 3 for Nguyen Minh An.'),
        ('90000000-0000-0000-0000-000000001604'::uuid, '90000000-0000-0000-0000-000000001407'::uuid, '90000000-0000-0000-0000-000000000703'::uuid, 'Completed session 1 for Tran Gia Binh.'),
        ('90000000-0000-0000-0000-000000001605'::uuid, '90000000-0000-0000-0000-000000001408'::uuid, '90000000-0000-0000-0000-000000000703'::uuid, 'Completed session 2 for Tran Gia Binh.'),
        ('90000000-0000-0000-0000-000000001606'::uuid, '90000000-0000-0000-0000-000000001409'::uuid, '90000000-0000-0000-0000-000000000703'::uuid, 'Completed session 3 for Tran Gia Binh.')
) AS v(id, session_id, student_profile_id, note)
ON CONFLICT ("SessionId", "StudentProfileId") DO UPDATE
SET
    "AttendanceStatus" = EXCLUDED."AttendanceStatus",
    "AbsenceType" = EXCLUDED."AbsenceType",
    "MarkedBy" = EXCLUDED."MarkedBy",
    "MarkedAt" = EXCLUDED."MarkedAt",
    "Note" = EXCLUDED."Note";

INSERT INTO public."StudentLevels"
(
    "Id",
    "StudentProfileId",
    "CurrentLevel",
    "CurrentXp",
    "UpdatedAt"
)
SELECT
    v.id,
    v.student_profile_id,
    v.current_level,
    v.current_xp,
    c.now_utc
FROM demo_context c
CROSS JOIN
(
    VALUES
        ('90000000-0000-0000-0000-000000001701'::uuid, '90000000-0000-0000-0000-000000000702'::uuid, 'Bronze 2', 120),
        ('90000000-0000-0000-0000-000000001702'::uuid, '90000000-0000-0000-0000-000000000703'::uuid, 'Silver 1', 260)
) AS v(id, student_profile_id, current_level, current_xp)
ON CONFLICT ("StudentProfileId") DO UPDATE
SET
    "CurrentLevel" = EXCLUDED."CurrentLevel",
    "CurrentXp" = EXCLUDED."CurrentXp",
    "UpdatedAt" = EXCLUDED."UpdatedAt";

INSERT INTO public."Notifications"
(
    "Id",
    "RecipientUserId",
    "RecipientProfileId",
    "Channel",
    "Title",
    "Content",
    "Deeplink",
    "Status",
    "SentAt",
    "ReadAt",
    "TemplateId",
    "NotificationTemplateId",
    "CreatedAt",
    "TargetRole",
    "Kind",
    "Priority",
    "SenderRole",
    "SenderName",
    "ScopeBranchId",
    "ScopeClassId",
    "ScopeStudentProfileId"
)
SELECT
    v.id,
    v.recipient_user_id,
    v.recipient_profile_id,
    v.channel,
    v.title,
    v.content,
    v.deeplink,
    'Sent',
    c.now_utc,
    NULL,
    NULL,
    NULL,
    c.now_utc,
    v.target_role,
    'package',
    v.priority,
    'System',
    'Kidzgo',
    '90000000-0000-0000-0000-000000000001'::uuid,
    v.scope_class_id,
    v.scope_student_profile_id
FROM demo_context c
CROSS JOIN
(
    VALUES
        (
            '90000000-0000-0000-0000-000000001801'::uuid,
            '90000000-0000-0000-0000-000000000606'::uuid,
            '90000000-0000-0000-0000-000000000701'::uuid,
            'InApp',
            'Goi hoc con 3 buoi',
            'Hoc vien Nguyen Minh An con 3 buoi trong goi Demo Starters 6 Sessions.',
            '/parent/registrations/90000000-0000-0000-0000-000000001101',
            'Parent',
            'normal',
            '90000000-0000-0000-0000-000000000901'::uuid,
            '90000000-0000-0000-0000-000000000702'::uuid
        ),
        (
            '90000000-0000-0000-0000-000000001802'::uuid,
            '90000000-0000-0000-0000-000000000607'::uuid,
            '90000000-0000-0000-0000-000000000702'::uuid,
            'InApp',
            'Ban con 3 buoi hoc',
            'Dang ky Demo Starters cua ban con 3 buoi. Vui long theo doi de dang ky tiep.',
            '/student/registrations/90000000-0000-0000-0000-000000001101',
            'Student',
            'normal',
            '90000000-0000-0000-0000-000000000901'::uuid,
            '90000000-0000-0000-0000-000000000702'::uuid
        ),
        (
            '90000000-0000-0000-0000-000000001803'::uuid,
            '90000000-0000-0000-0000-000000000606'::uuid,
            '90000000-0000-0000-0000-000000000701'::uuid,
            'InApp',
            'Goi hoc con 1 buoi',
            'Hoc vien Tran Gia Binh chi con 1 buoi trong goi Demo Movers 4 Sessions.',
            '/parent/registrations/90000000-0000-0000-0000-000000001102',
            'Parent',
            'high',
            '90000000-0000-0000-0000-000000000902'::uuid,
            '90000000-0000-0000-0000-000000000703'::uuid
        ),
        (
            '90000000-0000-0000-0000-000000001804'::uuid,
            '90000000-0000-0000-0000-000000000606'::uuid,
            '90000000-0000-0000-0000-000000000701'::uuid,
            'Email',
            'Goi hoc con 1 buoi',
            'Hoc vien Tran Gia Binh chi con 1 buoi trong goi Demo Movers 4 Sessions. Nen tao dang ky moi som.',
            '/parent/registrations/90000000-0000-0000-0000-000000001102',
            'Parent',
            'high',
            '90000000-0000-0000-0000-000000000902'::uuid,
            '90000000-0000-0000-0000-000000000703'::uuid
        ),
        (
            '90000000-0000-0000-0000-000000001805'::uuid,
            '90000000-0000-0000-0000-000000000606'::uuid,
            '90000000-0000-0000-0000-000000000701'::uuid,
            'Push',
            'Goi hoc con 1 buoi',
            'Hoc vien Tran Gia Binh chi con 1 buoi trong goi Demo Movers 4 Sessions.',
            '/parent/registrations/90000000-0000-0000-0000-000000001102',
            'Parent',
            'high',
            '90000000-0000-0000-0000-000000000902'::uuid,
            '90000000-0000-0000-0000-000000000703'::uuid
        ),
        (
            '90000000-0000-0000-0000-000000001806'::uuid,
            '90000000-0000-0000-0000-000000000608'::uuid,
            '90000000-0000-0000-0000-000000000703'::uuid,
            'InApp',
            'Ban con 1 buoi hoc',
            'Dang ky Demo Movers cua ban chi con 1 buoi. Hay lien he trung tam de dang ky tiep.',
            '/student/registrations/90000000-0000-0000-0000-000000001102',
            'Student',
            'high',
            '90000000-0000-0000-0000-000000000902'::uuid,
            '90000000-0000-0000-0000-000000000703'::uuid
        ),
        (
            '90000000-0000-0000-0000-000000001807'::uuid,
            '90000000-0000-0000-0000-000000000608'::uuid,
            '90000000-0000-0000-0000-000000000703'::uuid,
            'Email',
            'Ban con 1 buoi hoc',
            'Dang ky Demo Movers cua ban chi con 1 buoi. Hay dang ky tiep de khong bi gian doan.',
            '/student/registrations/90000000-0000-0000-0000-000000001102',
            'Student',
            'high',
            '90000000-0000-0000-0000-000000000902'::uuid,
            '90000000-0000-0000-0000-000000000703'::uuid
        ),
        (
            '90000000-0000-0000-0000-000000001808'::uuid,
            '90000000-0000-0000-0000-000000000608'::uuid,
            '90000000-0000-0000-0000-000000000703'::uuid,
            'Push',
            'Ban con 1 buoi hoc',
            'Dang ky Demo Movers cua ban chi con 1 buoi.',
            '/student/registrations/90000000-0000-0000-0000-000000001102',
            'Student',
            'high',
            '90000000-0000-0000-0000-000000000902'::uuid,
            '90000000-0000-0000-0000-000000000703'::uuid
        )
) AS v(id, recipient_user_id, recipient_profile_id, channel, title, content, deeplink, target_role, priority, scope_class_id, scope_student_profile_id)
ON CONFLICT ("Id") DO UPDATE
SET
    "RecipientUserId" = EXCLUDED."RecipientUserId",
    "RecipientProfileId" = EXCLUDED."RecipientProfileId",
    "Channel" = EXCLUDED."Channel",
    "Title" = EXCLUDED."Title",
    "Content" = EXCLUDED."Content",
    "Deeplink" = EXCLUDED."Deeplink",
    "Status" = EXCLUDED."Status",
    "SentAt" = EXCLUDED."SentAt",
    "ReadAt" = EXCLUDED."ReadAt",
    "TemplateId" = EXCLUDED."TemplateId",
    "NotificationTemplateId" = EXCLUDED."NotificationTemplateId",
    "CreatedAt" = EXCLUDED."CreatedAt",
    "TargetRole" = EXCLUDED."TargetRole",
    "Kind" = EXCLUDED."Kind",
    "Priority" = EXCLUDED."Priority",
    "SenderRole" = EXCLUDED."SenderRole",
    "SenderName" = EXCLUDED."SenderName",
    "ScopeBranchId" = EXCLUDED."ScopeBranchId",
    "ScopeClassId" = EXCLUDED."ScopeClassId",
    "ScopeStudentProfileId" = EXCLUDED."ScopeStudentProfileId";

COMMIT;

SELECT 'Demo branch' AS check_name, COUNT(*) AS total
FROM public."Branches"
WHERE "Id" = '90000000-0000-0000-0000-000000000001'::uuid
UNION ALL
SELECT 'Demo users', COUNT(*)
FROM public."Users"
WHERE "Id" IN
(
    '90000000-0000-0000-0000-000000000601'::uuid,
    '90000000-0000-0000-0000-000000000602'::uuid,
    '90000000-0000-0000-0000-000000000603'::uuid,
    '90000000-0000-0000-0000-000000000604'::uuid,
    '90000000-0000-0000-0000-000000000605'::uuid,
    '90000000-0000-0000-0000-000000000606'::uuid,
    '90000000-0000-0000-0000-000000000607'::uuid,
    '90000000-0000-0000-0000-000000000608'::uuid
)
UNION ALL
SELECT 'Demo classes', COUNT(*)
FROM public."Classes"
WHERE "Id" IN
(
    '90000000-0000-0000-0000-000000000901'::uuid,
    '90000000-0000-0000-0000-000000000902'::uuid
)
UNION ALL
SELECT 'Demo registrations', COUNT(*)
FROM public."Registrations"
WHERE "Id" IN
(
    '90000000-0000-0000-0000-000000001101'::uuid,
    '90000000-0000-0000-0000-000000001102'::uuid
)
UNION ALL
SELECT 'Demo sessions', COUNT(*)
FROM public."Sessions"
WHERE "Id" IN
(
    '90000000-0000-0000-0000-000000001401'::uuid,
    '90000000-0000-0000-0000-000000001402'::uuid,
    '90000000-0000-0000-0000-000000001403'::uuid,
    '90000000-0000-0000-0000-000000001404'::uuid,
    '90000000-0000-0000-0000-000000001405'::uuid,
    '90000000-0000-0000-0000-000000001406'::uuid,
    '90000000-0000-0000-0000-000000001407'::uuid,
    '90000000-0000-0000-0000-000000001408'::uuid,
    '90000000-0000-0000-0000-000000001409'::uuid,
    '90000000-0000-0000-0000-000000001410'::uuid
)
UNION ALL
SELECT 'Demo notifications', COUNT(*)
FROM public."Notifications"
WHERE "Id" IN
(
    '90000000-0000-0000-0000-000000001801'::uuid,
    '90000000-0000-0000-0000-000000001802'::uuid,
    '90000000-0000-0000-0000-000000001803'::uuid,
    '90000000-0000-0000-0000-000000001804'::uuid,
    '90000000-0000-0000-0000-000000001805'::uuid,
    '90000000-0000-0000-0000-000000001806'::uuid,
    '90000000-0000-0000-0000-000000001807'::uuid,
    '90000000-0000-0000-0000-000000001808'::uuid
);


## 3. Module phần mềm

### 3.1. Cấu trúc các module BE

| Thành phần | Vai trò |
| --- | --- |
| `Kidzgo.API` | Presentation layer: Controllers, middleware, swagger, startup, auth pipeline |
| `Kidzgo.Application` | Use case layer: command/query handler, validator, DTO, business service |
| `Kidzgo.Domain` | Domain layer: entity, enum, error, domain event |
| `Kidzgo.Infrastructure` | Data access, auth, mail, push, zalo, payment, jobs, report/pdf, migrations |

### 3.2. Danh sách nhóm module nghiệp vụ đã có trong BE

| Nhóm module | Thư mục/chức năng chính | Controller/API tiêu biểu |
| --- | --- | --- |
| Xác thực và tài khoản | `Authentication`, `Users`, `Profiles` | `AuthenticateController`, `AdminUserController`, `UserController`, `ProfileController` |
| Tuyển sinh và CRM | `Leads`, `PlacementTests`, `Registrations`, `Enrollments` | `LeadController`, `PlacementTestController`, `RegistrationController`, `EnrollmentController` |
| Quản lý lớp học | `Classes`, `Classrooms`, `Sessions`, `Attendance` | `ClassController`, `ClassroomController`, `SessionController`, `AttendanceController` |
| Bảo lưu, nghỉ học, học bù | `PauseEnrollmentRequests`, `LeaveRequests`, `MakeupCredits` | `PauseEnrollmentRequestController`, `LeaveRequestController`, `MakeupController` |
| Bài tập, đề thi, giáo án | `Homework`, `Exams`, `LessonPlans`, `LessonPlanTemplates`, `QuestionBank` | `HomeworkController`, `ExamController`, `LessonPlanController`, `LessonPlanTemplateController`, `QuestionBankController` |
| Học liệu và media | `TeachingMaterials`, `Media`, `Files` | `TeachingMaterialsController`, `MediaController`, `FileUploadController` |
| Tài chính và thanh toán | `Invoices`, `Payments`, `Finance`, `TuitionPlans`, `PayOS` | `InvoiceController`, `FinanceController`, `TuitionPlanController`, `PayOSWebhookController` |
| Notification và template | `Notifications`, `NotificationTemplates`, `EmailTemplates` | `NotificationController`, `EmailController` |
| Báo cáo | `MonthlyReports`, `SessionReports`, `ReportRequests`, `Dashboard`, `AuditLogs` | `MonthlyReportController`, `SessionReportController`, `ReportRequestController`, `DashboardController`, `AuditLogController` |
| Gamification | `Gamification`, `Missions` | `GamificationController`, `MissionController` |
| Nội dung public/web | `Blogs`, `Faqs`, `FaqCategories`, `LandingPages` | `BlogController`, `FaqController`, `LandingPageController` |
| Support/nội bộ | `Tickets`, `IncidentReports`, `Lookups`, `Payroll`, `Branches`, `Programs` | `TicketController`, `IncidentReportController`, `LookupController`, `BranchController`, `ProgramController` |
| Portal theo role | `Parent`, `Teacher`, `Student`, `Staff`, `StaffManagement` | `ParentController`, `TeacherController`, `StudentController`, `StaffController`, `StaffManagementController` |

### 3.3. Background jobs đáng có

Thư mục: `Kidzgo.Infrastructure/BackgroundJobs`

- `SyncPlannedToActualSessionsJob`
- `AutoConfirmRewardRedemptionJob`
- `MarkOverdueHomeworkSubmissionsJob`
- `ResetMissedDailyCheckInMissionProgressJob`
- `SendNotificationRemindersJob`
- `ResumePausedEnrollmentsJob`
- `BackfillStudentSessionAssignmentsJob`

### 3.4. Các thư viện, framework, công cụ bên thứ 3 được sử dụng

#### Framework/runtime chính

- `.NET 9`
- `ASP.NET Core Web API`
- `Entity Framework Core 9`
- `PostgreSQL 15`
- `Npgsql.EntityFrameworkCore.PostgreSQL`

#### Thư viện application/backend

- `MediatR`
- `FluentValidation`
- `NodaTime`
- `Ical.Net`
- `System.Text.Encoding.CodePages`

#### Logging, monitoring, API docs

- `Serilog`
- `Serilog.Sinks.Console`
- `Serilog.Sinks.File`
- `Serilog.Sinks.EventLog`
- `Serilog.Sinks.Seq`
- `Swashbuckle.AspNetCore`
- `AspNetCore.HealthChecks.UI`
- `AspNetCore.HealthChecks.NpgSql`

#### Bảo mật và auth

- `Microsoft.AspNetCore.Authentication.JwtBearer`
- `Microsoft.IdentityModel.Tokens`
- `System.IdentityModel.Tokens.Jwt`
- `Google.Apis.Auth`

#### Tích hợp bên ngoài

- `MailKit` - gửi email
- `FirebaseAdmin` - push notification FCM
- `Zalo Official Account API` - webhook và messaging

#### Xử lý file, PDF, Office

- `PuppeteerSharp` - render HTML/PDF
- `DocumentFormat.OpenXml` - xuất file Office
- `ExcelDataReader` - đọc Excel
- `UglyToad.PdfPig` - đọc/trích xuất PDF

#### Công cụ deploy/vận hành

- `Caddy`
- `Seq`
- PowerShell deploy scripts trong repo



## 4. Cấu hình các thành phần bên trong phần mềm

### 4.1. File cấu hình chính

- `Kidzgo.API/appsettings.json`
- `Kidzgo.API/appsettings.Development.json`
- `Kidzgo.API/appsettings.Production.json`
- `Kidzgo.API/Properties/launchSettings.json`
- `Kidzgo.API/Dockerfile`
- `compose.yaml`
- `deploy-win.ps1`
- `deploy-dev-win.ps1`
- `deploy-main-win.ps1`

App còn nạp thêm các file override nếu có:

- `appsettings.Local.json`
- `appsettings.{ENV}.local.json`
- Biến môi trường hệ thống

### 4.2. Cấu hình nội bộ cần lưu ý

| Hạng mục | Key/nguồn | Giá trị/ghi chú |
| --- | --- | --- |
| Connection String | `ConnectionStrings:Database` | Dev đang trỏ tới `Host=localhost;Port=5432;Database=kidzgo;Username=postgres;Password=***` |
| JWT token | `Jwt:Secret`, `Jwt:Issuer`, `Jwt:Audience`, `Jwt:ExpirationInMinutes` | Dev: issuer `kidzgo-api`, audience `users`, exp `60`; Prod exp `1440` |
| API port khi `dotnet run` | `Kidzgo.API/Properties/launchSettings.json` | HTTP `5178`, HTTPS `7235` |
| API port trong Docker | `Kidzgo.API/Dockerfile` | App bind `8080` |
| Port map Docker Compose | `compose.yaml` | `80:8080` cho API, `5432:5432` cho Postgres, `8081:80` cho Seq |
| API bind khi deploy Windows service | `deploy-win.ps1` | Mặc định `http://0.0.0.0:5000` |
| API bind main VPS | `deploy-main-win.ps1` | `http://127.0.0.1:5000` |
| API bind dev VPS | `deploy-dev-win.ps1` | `http://127.0.0.1:5001` |
| Reverse proxy | `deploy/caddy/Caddyfile` | Caddy reverse proxy vào `127.0.0.1:5000` |
| CORS | `ClientSettings:ClientUrls` | Danh sách FE/local/zalo mini app origins được whitelist |
| File storage | `FileStorage:*` | Dev dùng `D:\Resource`, Prod dùng `C:\Users\Administrator\Desktop\Projects\Resource` |
| Logging | `Serilog:*` | Log file, event log, Seq optional |
| UserSecrets | `Kidzgo.API.csproj` | `UserSecretsId` đã được khai báo |

### 4.3. Nhận xét bảo mật về cấu hình

- `appsettings.Development.json` hiện đang chứa một số secret thật/gần thật trong repo (SMTP, Cloudinary, Zalo).
- Khi nộp cho hội đồng/lưu trữ công khai, nên:
  - mask hoặc xóa secret khỏi file config,
  - đưa secret vào `appsettings.Local.json`,
  - hoặc đưa vào environment variables / secret manager.

## 5. Cấu hình sử dụng dịch vụ bên thứ 3

| Dịch vụ | Key cấu hình | Vai trò trong hệ thống |
| --- | --- | --- |
| SMTP Mail | `MailSettings:*` | Gửi mail reset password, thông báo profile, notification email |
| Zalo OA | `Zalo:*` | Webhook lead, gửi notification qua Zalo OA, OTP/PIN reset flow |
| Firebase FCM | `FCM:ServiceAccountPath`, `FCM:ServiceAccountJson` | Push notification |
| AI service | `AiService:BaseUrl` | AI report generator, AI feedback enhancer, AI homework assistant |
| Seq | `Serilog:Seq:ServerUrl` | Quan sát log tập trung |

## 6. Danh sách roles và tài khoản đăng nhập demo

### 6.1. Roles tìm thấy trong code

Nguồn: `Kidzgo.Domain/Users/UserRole.cs`

- `Admin`
- `ManagementStaff`
- `AccountantStaff`
- `Teacher`
- `Student`
- `Parent`

### 6.2. Tài khoản demo

Admin: Admin@gamil.com, Pass: 123456  
ManagementStaff: Staffq9@gmail.com, Pass: 123456  
Teacher: teacher2@gmail.com, Pass: 123456  
Parent: thinhtdse182756@fpt.edu.vn, Pass: 123456, Pin: 1234  
Student: thinhtdse182756@fpt.edu.vn, Pass: 123456, Pin: Null



### 6.3. Giá trị mặc định có liên quan đến tài khoản

Nguồn: `Kidzgo.Application/Profiles/ApproveProfile/ApproveProfileCommandHandler.cs`

- Password mặc định khi approve profile: `123456`
- Parent PIN mặc định khi approve profile: `1234`


## 7. Các tài liệu liên quan khác có trong repo

- `docs/API-Usage-Guide.md`
- `docs/login-api-usage-guide.md`
- `docs/windows-vps-dual-deploy-worktree.md`
- `docs/db-schema.md`
- `db/db.md`
- `notification-flow.md`


