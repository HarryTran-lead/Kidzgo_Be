using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Kidzgo.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class FixDeploy : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "public");

            migrationBuilder.CreateTable(
                name: "Branches",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Code = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    Name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    Address = table.Column<string>(type: "text", nullable: true),
                    ContactPhone = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: true),
                    ContactEmail = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Branches", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "EmailTemplates",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Code = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Subject = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    Body = table.Column<string>(type: "text", nullable: true),
                    Placeholders = table.Column<string>(type: "jsonb", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmailTemplates", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "FaqCategories",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Icon = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    SortOrder = table.Column<int>(type: "integer", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FaqCategories", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "GamificationSettings",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CheckInRewardStars = table.Column<int>(type: "integer", nullable: false),
                    CheckInRewardExp = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GamificationSettings", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Holidays",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    StartDate = table.Column<DateOnly>(type: "date", nullable: false),
                    EndDate = table.Column<DateOnly>(type: "date", nullable: false),
                    Description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Holidays", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "LandingPageSettings",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    LogoUrl = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    FeaturedProgramsSectionTitle = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    FeaturedProgramsSectionSubtitle = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    FeaturedClassesSectionTitle = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    FeaturedClassesSectionSubtitle = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    FeaturedTeachersSectionTitle = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    FeaturedTeachersSectionSubtitle = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    FooterAddress = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    FooterContactPhone = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    FooterContactPhonesJson = table.Column<string>(type: "text", nullable: false),
                    FooterContactEmail = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    FooterAddressesJson = table.Column<string>(type: "text", nullable: false),
                    FooterSocialLinksJson = table.Column<string>(type: "text", nullable: false),
                    FeaturedProgramIdsJson = table.Column<string>(type: "text", nullable: false),
                    FeaturedClassIdsJson = table.Column<string>(type: "text", nullable: false),
                    FeaturedProgramConfigsJson = table.Column<string>(type: "text", nullable: false),
                    FeaturedClassConfigsJson = table.Column<string>(type: "text", nullable: false),
                    FeaturedTeacherIdsJson = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LandingPageSettings", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "MakeupSettings",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CreditExpiryDays = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MakeupSettings", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "MissionRewardRules",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    MissionType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    ProgressMode = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false, defaultValue: "Count"),
                    TotalRequired = table.Column<int>(type: "integer", nullable: false),
                    RewardStars = table.Column<int>(type: "integer", nullable: false),
                    RewardExp = table.Column<int>(type: "integer", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MissionRewardRules", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "NotificationTemplates",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Code = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Channel = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Title = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    Content = table.Column<string>(type: "text", nullable: true),
                    Placeholders = table.Column<string>(type: "jsonb", nullable: true),
                    Category = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NotificationTemplates", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PauseEnrollmentSettings",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ReservationLimitMonths = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PauseEnrollmentSettings", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Programs",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    Code = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    IsMakeup = table.Column<bool>(type: "boolean", nullable: false),
                    IsSupplementary = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Programs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "RewardStoreItems",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Title = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
                    ImageUrl = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    CostStars = table.Column<int>(type: "integer", nullable: false),
                    Quantity = table.Column<int>(type: "integer", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RewardStoreItems", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TeacherCompensationSettings",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    StandardSessionDurationMinutes = table.Column<int>(type: "integer", nullable: false),
                    ForeignTeacherDefaultSessionRate = table.Column<decimal>(type: "numeric", nullable: false),
                    VietnameseTeacherDefaultSessionRate = table.Column<decimal>(type: "numeric", nullable: false),
                    AssistantDefaultSessionRate = table.Column<decimal>(type: "numeric", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TeacherCompensationSettings", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Classrooms",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    BranchId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Capacity = table.Column<int>(type: "integer", nullable: false),
                    Note = table.Column<string>(type: "text", nullable: true),
                    Floor = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Area = table.Column<decimal>(type: "numeric", nullable: true),
                    EquipmentJson = table.Column<string>(type: "text", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Classrooms", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Classrooms_Branches_BranchId",
                        column: x => x.BranchId,
                        principalSchema: "public",
                        principalTable: "Branches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "EnrollmentConfirmationPaymentSettings",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    BranchId = table.Column<Guid>(type: "uuid", nullable: true),
                    ScopeKey = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    PaymentMethod = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    AccountName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    AccountNumber = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    BankName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    BankCode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    BankBin = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    VietQrTemplate = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    LogoUrl = table.Column<string>(type: "text", nullable: true),
                    NewStudentPolicyText = table.Column<string>(type: "text", nullable: true),
                    ReservationPolicyText = table.Column<string>(type: "text", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EnrollmentConfirmationPaymentSettings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EnrollmentConfirmationPaymentSettings_Branches_BranchId",
                        column: x => x.BranchId,
                        principalSchema: "public",
                        principalTable: "Branches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ExtracurricularPrograms",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    BranchId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    Type = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Date = table.Column<DateOnly>(type: "date", nullable: false),
                    Capacity = table.Column<int>(type: "integer", nullable: false),
                    RegisteredCount = table.Column<int>(type: "integer", nullable: false),
                    Fee = table.Column<decimal>(type: "numeric", nullable: false),
                    Location = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExtracurricularPrograms", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ExtracurricularPrograms_Branches_BranchId",
                        column: x => x.BranchId,
                        principalSchema: "public",
                        principalTable: "Branches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Email = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    PasswordHash = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    Role = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Username = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    PhoneNumber = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    PinHash = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    AvatarUrl = table.Column<string>(type: "text", nullable: true),
                    AvatarMimeType = table.Column<string>(type: "text", nullable: true),
                    AvatarFileSize = table.Column<long>(type: "bigint", nullable: true),
                    TeacherCompensationType = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    BranchId = table.Column<Guid>(type: "uuid", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    LastLoginAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastSeenAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Users_Branches_BranchId",
                        column: x => x.BranchId,
                        principalSchema: "public",
                        principalTable: "Branches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "FaqItems",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CategoryId = table.Column<Guid>(type: "uuid", nullable: false),
                    Question = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    Answer = table.Column<string>(type: "text", nullable: false),
                    SortOrder = table.Column<int>(type: "integer", nullable: false),
                    IsPublished = table.Column<bool>(type: "boolean", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    PublishedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FaqItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FaqItems_FaqCategories_CategoryId",
                        column: x => x.CategoryId,
                        principalSchema: "public",
                        principalTable: "FaqCategories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ProgramProgressionRules",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SourceProgramId = table.Column<Guid>(type: "uuid", nullable: false),
                    TargetProgramId = table.Column<Guid>(type: "uuid", nullable: true),
                    Method = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    MinimumShieldCount = table.Column<int>(type: "integer", nullable: true),
                    MinimumSkillShieldCount = table.Column<int>(type: "integer", nullable: true),
                    MinimumOverallScore = table.Column<decimal>(type: "numeric", nullable: true),
                    CarryOverRemainingSessions = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    StopCurrentEnrollmentOnApproval = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    ShieldMappingJson = table.Column<string>(type: "text", nullable: true),
                    ClassificationBandsJson = table.Column<string>(type: "text", nullable: true),
                    PracticeTestScoreMappingsJson = table.Column<string>(type: "text", nullable: true),
                    Notes = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProgramProgressionRules", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProgramProgressionRules_Programs_SourceProgramId",
                        column: x => x.SourceProgramId,
                        principalSchema: "public",
                        principalTable: "Programs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProgramProgressionRules_Programs_TargetProgramId",
                        column: x => x.TargetProgramId,
                        principalSchema: "public",
                        principalTable: "Programs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "QuestionBankItems",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ProgramId = table.Column<Guid>(type: "uuid", nullable: false),
                    QuestionText = table.Column<string>(type: "text", nullable: false),
                    QuestionType = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Options = table.Column<string>(type: "jsonb", nullable: true),
                    CorrectAnswer = table.Column<string>(type: "text", nullable: true),
                    Points = table.Column<int>(type: "integer", nullable: false),
                    Explanation = table.Column<string>(type: "text", nullable: true),
                    ImageUrls = table.Column<string>(type: "jsonb", nullable: true),
                    VideoUrls = table.Column<string>(type: "jsonb", nullable: true),
                    AudioUrls = table.Column<string>(type: "jsonb", nullable: true),
                    Topic = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Skill = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    GrammarTags = table.Column<string>(type: "jsonb", nullable: true),
                    VocabularyTags = table.Column<string>(type: "jsonb", nullable: true),
                    Level = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QuestionBankItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_QuestionBankItems_Programs_ProgramId",
                        column: x => x.ProgramId,
                        principalSchema: "public",
                        principalTable: "Programs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TuitionPlans",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ProgramId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    TotalSessions = table.Column<int>(type: "integer", nullable: false),
                    TuitionAmount = table.Column<decimal>(type: "numeric", nullable: false),
                    UnitPriceSession = table.Column<decimal>(type: "numeric", nullable: false),
                    Currency = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TuitionPlans", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TuitionPlans_Programs_ProgramId",
                        column: x => x.ProgramId,
                        principalSchema: "public",
                        principalTable: "Programs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Blogs",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Title = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    Summary = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Content = table.Column<string>(type: "text", nullable: false),
                    FeaturedImageUrl = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    AttachmentImageUrl = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    AttachmentFileUrl = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: false),
                    IsPublished = table.Column<bool>(type: "boolean", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    PublishedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Blogs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Blogs_Users_CreatedBy",
                        column: x => x.CreatedBy,
                        principalSchema: "public",
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CashbookEntries",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    BranchId = table.Column<Guid>(type: "uuid", nullable: false),
                    Type = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    Amount = table.Column<decimal>(type: "numeric", nullable: false),
                    Currency = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
                    RelatedType = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    RelatedId = table.Column<Guid>(type: "uuid", nullable: true),
                    EntryDate = table.Column<DateOnly>(type: "date", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    AttachmentUrl = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CashbookEntries", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CashbookEntries_Branches_BranchId",
                        column: x => x.BranchId,
                        principalSchema: "public",
                        principalTable: "Branches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CashbookEntries_Users_CreatedBy",
                        column: x => x.CreatedBy,
                        principalSchema: "public",
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Classes",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    BranchId = table.Column<Guid>(type: "uuid", nullable: false),
                    ProgramId = table.Column<Guid>(type: "uuid", nullable: false),
                    Code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Title = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    RoomId = table.Column<Guid>(type: "uuid", nullable: true),
                    MainTeacherId = table.Column<Guid>(type: "uuid", nullable: true),
                    AssistantTeacherId = table.Column<Guid>(type: "uuid", nullable: true),
                    StartDate = table.Column<DateOnly>(type: "date", nullable: false),
                    EndDate = table.Column<DateOnly>(type: "date", nullable: true),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Capacity = table.Column<int>(type: "integer", nullable: false),
                    WeeklyScheduleJson = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Classes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Classes_Branches_BranchId",
                        column: x => x.BranchId,
                        principalSchema: "public",
                        principalTable: "Branches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Classes_Classrooms_RoomId",
                        column: x => x.RoomId,
                        principalSchema: "public",
                        principalTable: "Classrooms",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Classes_Programs_ProgramId",
                        column: x => x.ProgramId,
                        principalSchema: "public",
                        principalTable: "Programs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Classes_Users_AssistantTeacherId",
                        column: x => x.AssistantTeacherId,
                        principalSchema: "public",
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Classes_Users_MainTeacherId",
                        column: x => x.MainTeacherId,
                        principalSchema: "public",
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Contracts",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    StaffUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    ContractType = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    StartDate = table.Column<DateOnly>(type: "date", nullable: false),
                    EndDate = table.Column<DateOnly>(type: "date", nullable: true),
                    BaseSalary = table.Column<decimal>(type: "numeric", nullable: true),
                    HourlyRate = table.Column<decimal>(type: "numeric", nullable: true),
                    AllowanceFixed = table.Column<decimal>(type: "numeric", nullable: true),
                    MinimumMonthlyHours = table.Column<decimal>(type: "numeric", nullable: true),
                    OvertimeRateMultiplier = table.Column<decimal>(type: "numeric", nullable: true),
                    BranchId = table.Column<Guid>(type: "uuid", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Contracts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Contracts_Branches_BranchId",
                        column: x => x.BranchId,
                        principalSchema: "public",
                        principalTable: "Branches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Contracts_Users_StaffUserId",
                        column: x => x.StaffUserId,
                        principalSchema: "public",
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "DeviceTokens",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Token = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    DeviceType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    DeviceId = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    Role = table.Column<string>(type: "text", nullable: true),
                    Browser = table.Column<string>(type: "text", nullable: true),
                    Locale = table.Column<string>(type: "text", nullable: true),
                    BranchId = table.Column<Guid>(type: "uuid", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    LastUsedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DeviceTokens", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DeviceTokens_Users_UserId",
                        column: x => x.UserId,
                        principalSchema: "public",
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Leads",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Source = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    Campaign = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    ContactName = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    Phone = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    ZaloId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Email = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    Company = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    Subject = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    BranchPreference = table.Column<Guid>(type: "uuid", nullable: true),
                    Notes = table.Column<string>(type: "text", nullable: true),
                    Status = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    OwnerStaffId = table.Column<Guid>(type: "uuid", nullable: true),
                    FirstResponseAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    TouchCount = table.Column<int>(type: "integer", nullable: false),
                    NextActionAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Leads", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Leads_Branches_BranchPreference",
                        column: x => x.BranchPreference,
                        principalSchema: "public",
                        principalTable: "Branches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Leads_Users_OwnerStaffId",
                        column: x => x.OwnerStaffId,
                        principalSchema: "public",
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "LessonPlanTemplates",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ProgramId = table.Column<Guid>(type: "uuid", nullable: false),
                    Level = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Title = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    SessionIndex = table.Column<int>(type: "integer", nullable: false),
                    SyllabusMetadata = table.Column<string>(type: "text", nullable: true),
                    SyllabusContent = table.Column<string>(type: "text", nullable: true),
                    SourceFileName = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    AttachmentUrl = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    AttachmentMimeType = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    AttachmentFileSize = table.Column<long>(type: "bigint", nullable: true),
                    AttachmentOriginalFileName = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LessonPlanTemplates", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LessonPlanTemplates_Programs_ProgramId",
                        column: x => x.ProgramId,
                        principalSchema: "public",
                        principalTable: "Programs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_LessonPlanTemplates_Users_CreatedBy",
                        column: x => x.CreatedBy,
                        principalSchema: "public",
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "MonthlyReportJobs",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Month = table.Column<int>(type: "integer", nullable: false),
                    Year = table.Column<int>(type: "integer", nullable: false),
                    BranchId = table.Column<Guid>(type: "uuid", nullable: false),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    StartedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    FinishedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    AiPayloadRef = table.Column<string>(type: "text", nullable: true),
                    ErrorMessage = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    RetryCount = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MonthlyReportJobs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MonthlyReportJobs_Branches_BranchId",
                        column: x => x.BranchId,
                        principalSchema: "public",
                        principalTable: "Branches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MonthlyReportJobs_Users_CreatedBy",
                        column: x => x.CreatedBy,
                        principalSchema: "public",
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PasswordResetTokens",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Token = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UsedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PasswordResetTokens", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PasswordResetTokens_Users_UserId",
                        column: x => x.UserId,
                        principalSchema: "public",
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PayrollRuns",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PeriodStart = table.Column<DateOnly>(type: "date", nullable: false),
                    PeriodEnd = table.Column<DateOnly>(type: "date", nullable: false),
                    BranchId = table.Column<Guid>(type: "uuid", nullable: false),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    ApprovedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    PaidAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PayrollRuns", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PayrollRuns_Branches_BranchId",
                        column: x => x.BranchId,
                        principalSchema: "public",
                        principalTable: "Branches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PayrollRuns_Users_ApprovedBy",
                        column: x => x.ApprovedBy,
                        principalSchema: "public",
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Profiles",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    ProfileType = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    DisplayName = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    Name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    Gender = table.Column<string>(type: "text", nullable: true),
                    DateOfBirth = table.Column<DateOnly>(type: "date", nullable: true),
                    ZaloId = table.Column<string>(type: "text", nullable: true),
                    PinHash = table.Column<string>(type: "character varying(97)", maxLength: 97, nullable: true),
                    AvatarUrl = table.Column<string>(type: "text", nullable: true),
                    AvatarMimeType = table.Column<string>(type: "text", nullable: true),
                    AvatarFileSize = table.Column<long>(type: "bigint", nullable: true),
                    IsApproved = table.Column<bool>(type: "boolean", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    LastLoginAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastSeenAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Profiles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Profiles_Users_UserId",
                        column: x => x.UserId,
                        principalSchema: "public",
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ProgramLeavePolicies",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ProgramId = table.Column<Guid>(type: "uuid", nullable: false),
                    MaxLeavesPerMonth = table.Column<int>(type: "integer", nullable: false),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProgramLeavePolicies", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProgramLeavePolicies_Programs_ProgramId",
                        column: x => x.ProgramId,
                        principalSchema: "public",
                        principalTable: "Programs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ProgramLeavePolicies_Users_UpdatedBy",
                        column: x => x.UpdatedBy,
                        principalSchema: "public",
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "RefreshTokens",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Token = table.Column<string>(type: "text", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Expires = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RefreshTokens", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RefreshTokens_Users_UserId",
                        column: x => x.UserId,
                        principalSchema: "public",
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TeachingMaterials",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ProgramId = table.Column<Guid>(type: "uuid", nullable: false),
                    UnitNumber = table.Column<int>(type: "integer", nullable: true),
                    LessonNumber = table.Column<int>(type: "integer", nullable: true),
                    LessonTitle = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    RelativePath = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    DisplayName = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    OriginalFileName = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    StoragePath = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    MimeType = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    FileExtension = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    FileSize = table.Column<long>(type: "bigint", nullable: false),
                    FileType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Category = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    IsEncrypted = table.Column<bool>(type: "boolean", nullable: false),
                    EncryptionAlgorithm = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    EncryptionKeyVersion = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    PdfPreviewPath = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    PdfPreviewGeneratedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    PdfPreviewFileSize = table.Column<long>(type: "bigint", nullable: true),
                    UploadedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TeachingMaterials", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TeachingMaterials_Programs_ProgramId",
                        column: x => x.ProgramId,
                        principalSchema: "public",
                        principalTable: "Programs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TeachingMaterials_Users_UploadedByUserId",
                        column: x => x.UploadedByUserId,
                        principalSchema: "public",
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "RegistrationDiscountCampaigns",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Code = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Description = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    BranchId = table.Column<Guid>(type: "uuid", nullable: true),
                    ProgramId = table.Column<Guid>(type: "uuid", nullable: true),
                    TuitionPlanId = table.Column<Guid>(type: "uuid", nullable: true),
                    DiscountType = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    DiscountValue = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    Priority = table.Column<int>(type: "integer", nullable: false),
                    StartDate = table.Column<DateOnly>(type: "date", nullable: false),
                    EndDate = table.Column<DateOnly>(type: "date", nullable: false),
                    ApplyForInitialRegistration = table.Column<bool>(type: "boolean", nullable: false),
                    ApplyForRenewal = table.Column<bool>(type: "boolean", nullable: false),
                    ApplyForUpgrade = table.Column<bool>(type: "boolean", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RegistrationDiscountCampaigns", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RegistrationDiscountCampaigns_Branches_BranchId",
                        column: x => x.BranchId,
                        principalSchema: "public",
                        principalTable: "Branches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RegistrationDiscountCampaigns_Programs_ProgramId",
                        column: x => x.ProgramId,
                        principalSchema: "public",
                        principalTable: "Programs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RegistrationDiscountCampaigns_TuitionPlans_TuitionPlanId",
                        column: x => x.TuitionPlanId,
                        principalSchema: "public",
                        principalTable: "TuitionPlans",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "BranchPrograms",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    BranchId = table.Column<Guid>(type: "uuid", nullable: false),
                    ProgramId = table.Column<Guid>(type: "uuid", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    DefaultMakeupClassId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BranchPrograms", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BranchPrograms_Branches_BranchId",
                        column: x => x.BranchId,
                        principalSchema: "public",
                        principalTable: "Branches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_BranchPrograms_Classes_DefaultMakeupClassId",
                        column: x => x.DefaultMakeupClassId,
                        principalSchema: "public",
                        principalTable: "Classes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_BranchPrograms_Programs_ProgramId",
                        column: x => x.ProgramId,
                        principalSchema: "public",
                        principalTable: "Programs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ClassScheduleSegments",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ClassId = table.Column<Guid>(type: "uuid", nullable: false),
                    EffectiveFrom = table.Column<DateOnly>(type: "date", nullable: false),
                    EffectiveTo = table.Column<DateOnly>(type: "date", nullable: true),
                    SchedulePattern = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClassScheduleSegments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ClassScheduleSegments_Classes_ClassId",
                        column: x => x.ClassId,
                        principalSchema: "public",
                        principalTable: "Classes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Exams",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ClassId = table.Column<Guid>(type: "uuid", nullable: false),
                    ExamType = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    Date = table.Column<DateOnly>(type: "date", nullable: false),
                    MaxScore = table.Column<decimal>(type: "numeric", nullable: true),
                    Description = table.Column<string>(type: "text", nullable: true),
                    ScheduledStartTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    TimeLimitMinutes = table.Column<int>(type: "integer", nullable: true),
                    AllowLateStart = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    LateStartToleranceMinutes = table.Column<int>(type: "integer", nullable: true),
                    AutoSubmitOnTimeLimit = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    PreventCopyPaste = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    PreventNavigation = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    ShowResultsImmediately = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Exams", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Exams_Classes_ClassId",
                        column: x => x.ClassId,
                        principalSchema: "public",
                        principalTable: "Classes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Exams_Users_CreatedBy",
                        column: x => x.CreatedBy,
                        principalSchema: "public",
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Missions",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Title = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
                    Scope = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    TargetClassId = table.Column<Guid>(type: "uuid", nullable: true),
                    TargetStudentId = table.Column<Guid>(type: "uuid", nullable: true),
                    TargetGroup = table.Column<string>(type: "jsonb", nullable: true),
                    MissionType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    ProgressMode = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false, defaultValue: "Count"),
                    StartAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    EndAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    RewardStars = table.Column<int>(type: "integer", nullable: true),
                    RewardExp = table.Column<int>(type: "integer", nullable: true),
                    TotalRequired = table.Column<int>(type: "integer", nullable: true),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Missions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Missions_Classes_TargetClassId",
                        column: x => x.TargetClassId,
                        principalSchema: "public",
                        principalTable: "Classes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Missions_Users_CreatedBy",
                        column: x => x.CreatedBy,
                        principalSchema: "public",
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ProgramProgressionSchedules",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SourceClassId = table.Column<Guid>(type: "uuid", nullable: false),
                    SourceProgramId = table.Column<Guid>(type: "uuid", nullable: false),
                    BranchId = table.Column<Guid>(type: "uuid", nullable: false),
                    ScheduledAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DurationMinutes = table.Column<int>(type: "integer", nullable: false),
                    RoomId = table.Column<Guid>(type: "uuid", nullable: true),
                    AssignedTeacherUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Status = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    Notes = table.Column<string>(type: "text", nullable: true),
                    CreatedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProgramProgressionSchedules", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProgramProgressionSchedules_Branches_BranchId",
                        column: x => x.BranchId,
                        principalSchema: "public",
                        principalTable: "Branches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProgramProgressionSchedules_Classes_SourceClassId",
                        column: x => x.SourceClassId,
                        principalSchema: "public",
                        principalTable: "Classes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProgramProgressionSchedules_Classrooms_RoomId",
                        column: x => x.RoomId,
                        principalSchema: "public",
                        principalTable: "Classrooms",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProgramProgressionSchedules_Programs_SourceProgramId",
                        column: x => x.SourceProgramId,
                        principalSchema: "public",
                        principalTable: "Programs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProgramProgressionSchedules_Users_AssignedTeacherUserId",
                        column: x => x.AssignedTeacherUserId,
                        principalSchema: "public",
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProgramProgressionSchedules_Users_CreatedByUserId",
                        column: x => x.CreatedByUserId,
                        principalSchema: "public",
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Sessions",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ClassId = table.Column<Guid>(type: "uuid", nullable: false),
                    BranchId = table.Column<Guid>(type: "uuid", nullable: false),
                    PlannedDatetime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    PlannedRoomId = table.Column<Guid>(type: "uuid", nullable: true),
                    PlannedTeacherId = table.Column<Guid>(type: "uuid", nullable: true),
                    PlannedAssistantId = table.Column<Guid>(type: "uuid", nullable: true),
                    DurationMinutes = table.Column<int>(type: "integer", nullable: false),
                    ParticipationType = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    ActualDatetime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ActualRoomId = table.Column<Guid>(type: "uuid", nullable: true),
                    ActualTeacherId = table.Column<Guid>(type: "uuid", nullable: true),
                    ActualAssistantId = table.Column<Guid>(type: "uuid", nullable: true),
                    Color = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Sessions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Sessions_Branches_BranchId",
                        column: x => x.BranchId,
                        principalSchema: "public",
                        principalTable: "Branches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Sessions_Classes_ClassId",
                        column: x => x.ClassId,
                        principalSchema: "public",
                        principalTable: "Classes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Sessions_Classrooms_ActualRoomId",
                        column: x => x.ActualRoomId,
                        principalSchema: "public",
                        principalTable: "Classrooms",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Sessions_Classrooms_PlannedRoomId",
                        column: x => x.PlannedRoomId,
                        principalSchema: "public",
                        principalTable: "Classrooms",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Sessions_Users_ActualAssistantId",
                        column: x => x.ActualAssistantId,
                        principalSchema: "public",
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Sessions_Users_ActualTeacherId",
                        column: x => x.ActualTeacherId,
                        principalSchema: "public",
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Sessions_Users_PlannedAssistantId",
                        column: x => x.PlannedAssistantId,
                        principalSchema: "public",
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Sessions_Users_PlannedTeacherId",
                        column: x => x.PlannedTeacherId,
                        principalSchema: "public",
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "MonthlyWorkHours",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    StaffUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    ContractId = table.Column<Guid>(type: "uuid", nullable: false),
                    BranchId = table.Column<Guid>(type: "uuid", nullable: false),
                    Year = table.Column<int>(type: "integer", nullable: false),
                    Month = table.Column<int>(type: "integer", nullable: false),
                    TotalHours = table.Column<decimal>(type: "numeric", nullable: false),
                    TeachingHours = table.Column<decimal>(type: "numeric", nullable: false),
                    RegularHours = table.Column<decimal>(type: "numeric", nullable: false),
                    OvertimeHours = table.Column<decimal>(type: "numeric", nullable: false),
                    TeachingSessions = table.Column<int>(type: "integer", nullable: false),
                    IsLocked = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MonthlyWorkHours", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MonthlyWorkHours_Branches_BranchId",
                        column: x => x.BranchId,
                        principalSchema: "public",
                        principalTable: "Branches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MonthlyWorkHours_Contracts_ContractId",
                        column: x => x.ContractId,
                        principalSchema: "public",
                        principalTable: "Contracts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MonthlyWorkHours_Users_StaffUserId",
                        column: x => x.StaffUserId,
                        principalSchema: "public",
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ShiftAttendances",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    StaffUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    ContractId = table.Column<Guid>(type: "uuid", nullable: true),
                    ShiftDate = table.Column<DateOnly>(type: "date", nullable: false),
                    ShiftHours = table.Column<decimal>(type: "numeric", nullable: false),
                    Role = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    ApprovedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    ApprovedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ShiftAttendances", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ShiftAttendances_Contracts_ContractId",
                        column: x => x.ContractId,
                        principalSchema: "public",
                        principalTable: "Contracts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ShiftAttendances_Users_ApprovedBy",
                        column: x => x.ApprovedBy,
                        principalSchema: "public",
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ShiftAttendances_Users_StaffUserId",
                        column: x => x.StaffUserId,
                        principalSchema: "public",
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "LeadActivities",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    LeadId = table.Column<Guid>(type: "uuid", nullable: false),
                    ActivityType = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Content = table.Column<string>(type: "text", nullable: true),
                    NextActionAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LeadActivities", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LeadActivities_Leads_LeadId",
                        column: x => x.LeadId,
                        principalSchema: "public",
                        principalTable: "Leads",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_LeadActivities_Users_CreatedBy",
                        column: x => x.CreatedBy,
                        principalSchema: "public",
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PayrollLines",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PayrollRunId = table.Column<Guid>(type: "uuid", nullable: false),
                    StaffUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    ComponentType = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    SourceId = table.Column<Guid>(type: "uuid", nullable: true),
                    Amount = table.Column<decimal>(type: "numeric", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
                    IsPaid = table.Column<bool>(type: "boolean", nullable: false),
                    PaidAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PayrollLines", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PayrollLines_PayrollRuns_PayrollRunId",
                        column: x => x.PayrollRunId,
                        principalSchema: "public",
                        principalTable: "PayrollRuns",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PayrollLines_Users_StaffUserId",
                        column: x => x.StaffUserId,
                        principalSchema: "public",
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PayrollPayments",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PayrollRunId = table.Column<Guid>(type: "uuid", nullable: false),
                    StaffUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Amount = table.Column<decimal>(type: "numeric", nullable: false),
                    Method = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    PaidAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CashbookEntryId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PayrollPayments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PayrollPayments_CashbookEntries_CashbookEntryId",
                        column: x => x.CashbookEntryId,
                        principalSchema: "public",
                        principalTable: "CashbookEntries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PayrollPayments_PayrollRuns_PayrollRunId",
                        column: x => x.PayrollRunId,
                        principalSchema: "public",
                        principalTable: "PayrollRuns",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PayrollPayments_Users_StaffUserId",
                        column: x => x.StaffUserId,
                        principalSchema: "public",
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AttendanceStreaks",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    StudentProfileId = table.Column<Guid>(type: "uuid", nullable: false),
                    AttendanceDate = table.Column<DateOnly>(type: "date", nullable: false),
                    CurrentStreak = table.Column<int>(type: "integer", nullable: false),
                    RewardStars = table.Column<int>(type: "integer", nullable: false),
                    RewardExp = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AttendanceStreaks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AttendanceStreaks_Profiles_StudentProfileId",
                        column: x => x.StudentProfileId,
                        principalSchema: "public",
                        principalTable: "Profiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AuditLogs",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ActorUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    ActorProfileId = table.Column<Guid>(type: "uuid", nullable: true),
                    Action = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    EntityType = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    EntityId = table.Column<Guid>(type: "uuid", nullable: true),
                    DataBefore = table.Column<string>(type: "jsonb", nullable: true),
                    DataAfter = table.Column<string>(type: "jsonb", nullable: true),
                    IpAddress = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuditLogs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AuditLogs_Profiles_ActorProfileId",
                        column: x => x.ActorProfileId,
                        principalSchema: "public",
                        principalTable: "Profiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AuditLogs_Users_ActorUserId",
                        column: x => x.ActorUserId,
                        principalSchema: "public",
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Invoices",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    BranchId = table.Column<Guid>(type: "uuid", nullable: false),
                    StudentProfileId = table.Column<Guid>(type: "uuid", nullable: false),
                    ClassId = table.Column<Guid>(type: "uuid", nullable: true),
                    Type = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    Amount = table.Column<decimal>(type: "numeric", nullable: false),
                    Currency = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    DueDate = table.Column<DateOnly>(type: "date", nullable: true),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
                    PayosPaymentLink = table.Column<string>(type: "text", nullable: true),
                    PayosQr = table.Column<string>(type: "text", nullable: true),
                    PayosOrderCode = table.Column<long>(type: "bigint", nullable: true),
                    IssuedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IssuedBy = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Invoices", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Invoices_Branches_BranchId",
                        column: x => x.BranchId,
                        principalSchema: "public",
                        principalTable: "Branches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Invoices_Classes_ClassId",
                        column: x => x.ClassId,
                        principalSchema: "public",
                        principalTable: "Classes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Invoices_Profiles_StudentProfileId",
                        column: x => x.StudentProfileId,
                        principalSchema: "public",
                        principalTable: "Profiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Invoices_Users_IssuedBy",
                        column: x => x.IssuedBy,
                        principalSchema: "public",
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "LeadChildren",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    LeadId = table.Column<Guid>(type: "uuid", nullable: false),
                    ChildName = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    Dob = table.Column<DateOnly>(type: "date", nullable: true),
                    Gender = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    ProgramInterest = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    Notes = table.Column<string>(type: "text", nullable: true),
                    Status = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    ConvertedStudentProfileId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LeadChildren", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LeadChildren_Leads_LeadId",
                        column: x => x.LeadId,
                        principalSchema: "public",
                        principalTable: "Leads",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_LeadChildren_Profiles_ConvertedStudentProfileId",
                        column: x => x.ConvertedStudentProfileId,
                        principalSchema: "public",
                        principalTable: "Profiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "MediaAssets",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UploaderId = table.Column<Guid>(type: "uuid", nullable: false),
                    BranchId = table.Column<Guid>(type: "uuid", nullable: false),
                    ClassId = table.Column<Guid>(type: "uuid", nullable: true),
                    StudentProfileId = table.Column<Guid>(type: "uuid", nullable: true),
                    MonthTag = table.Column<string>(type: "character varying(7)", maxLength: 7, nullable: true),
                    Type = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    ContentType = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Url = table.Column<string>(type: "text", nullable: false),
                    MimeType = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    FileSize = table.Column<long>(type: "bigint", nullable: false),
                    OriginalFileName = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    DisplayOrder = table.Column<int>(type: "integer", nullable: true),
                    OwnershipScope = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Caption = table.Column<string>(type: "text", nullable: true),
                    Visibility = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    ApprovalStatus = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    ApprovedById = table.Column<Guid>(type: "uuid", nullable: true),
                    ApprovedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    RejectReason = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    IsPublished = table.Column<bool>(type: "boolean", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MediaAssets", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MediaAssets_Branches_BranchId",
                        column: x => x.BranchId,
                        principalSchema: "public",
                        principalTable: "Branches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MediaAssets_Classes_ClassId",
                        column: x => x.ClassId,
                        principalSchema: "public",
                        principalTable: "Classes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MediaAssets_Profiles_StudentProfileId",
                        column: x => x.StudentProfileId,
                        principalSchema: "public",
                        principalTable: "Profiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MediaAssets_Users_ApprovedById",
                        column: x => x.ApprovedById,
                        principalSchema: "public",
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MediaAssets_Users_UploaderId",
                        column: x => x.UploaderId,
                        principalSchema: "public",
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Notifications",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    RecipientUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    RecipientProfileId = table.Column<Guid>(type: "uuid", nullable: true),
                    Channel = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Title = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    Content = table.Column<string>(type: "text", nullable: true),
                    Deeplink = table.Column<string>(type: "text", nullable: true),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    SentAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ReadAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    TemplateId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    NotificationTemplateId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    TargetRole = table.Column<string>(type: "text", nullable: true),
                    Kind = table.Column<string>(type: "text", nullable: true),
                    Priority = table.Column<string>(type: "text", nullable: true),
                    SenderRole = table.Column<string>(type: "text", nullable: true),
                    SenderName = table.Column<string>(type: "text", nullable: true),
                    ScopeBranchId = table.Column<Guid>(type: "uuid", nullable: true),
                    ScopeClassId = table.Column<Guid>(type: "uuid", nullable: true),
                    ScopeStudentProfileId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Notifications", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Notifications_Branches_ScopeBranchId",
                        column: x => x.ScopeBranchId,
                        principalSchema: "public",
                        principalTable: "Branches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Notifications_Classes_ScopeClassId",
                        column: x => x.ScopeClassId,
                        principalSchema: "public",
                        principalTable: "Classes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Notifications_NotificationTemplates_NotificationTemplateId",
                        column: x => x.NotificationTemplateId,
                        principalSchema: "public",
                        principalTable: "NotificationTemplates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Notifications_Profiles_RecipientProfileId",
                        column: x => x.RecipientProfileId,
                        principalSchema: "public",
                        principalTable: "Profiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Notifications_Users_RecipientUserId",
                        column: x => x.RecipientUserId,
                        principalSchema: "public",
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ParentPinResetTokens",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ProfileId = table.Column<Guid>(type: "uuid", nullable: false),
                    Token = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    OtpCodeHash = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    OtpExpiresAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    OtpVerifiedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    OtpAttemptCount = table.Column<int>(type: "integer", nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UsedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ParentPinResetTokens", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ParentPinResetTokens_Profiles_ProfileId",
                        column: x => x.ProfileId,
                        principalSchema: "public",
                        principalTable: "Profiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ParentStudentLinks",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ParentProfileId = table.Column<Guid>(type: "uuid", nullable: false),
                    StudentProfileId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ParentStudentLinks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ParentStudentLinks_Profiles_ParentProfileId",
                        column: x => x.ParentProfileId,
                        principalSchema: "public",
                        principalTable: "Profiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ParentStudentLinks_Profiles_StudentProfileId",
                        column: x => x.StudentProfileId,
                        principalSchema: "public",
                        principalTable: "Profiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RewardRedemptions",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ItemId = table.Column<Guid>(type: "uuid", nullable: false),
                    ItemName = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    Quantity = table.Column<int>(type: "integer", nullable: false, defaultValue: 1),
                    StarsDeducted = table.Column<int>(type: "integer", nullable: true),
                    StudentProfileId = table.Column<Guid>(type: "uuid", nullable: false),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    CancelReason = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    HandledBy = table.Column<Guid>(type: "uuid", nullable: true),
                    HandledAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeliveredAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ReceivedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RewardRedemptions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RewardRedemptions_Profiles_StudentProfileId",
                        column: x => x.StudentProfileId,
                        principalSchema: "public",
                        principalTable: "Profiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RewardRedemptions_RewardStoreItems_ItemId",
                        column: x => x.ItemId,
                        principalSchema: "public",
                        principalTable: "RewardStoreItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RewardRedemptions_Users_HandledBy",
                        column: x => x.HandledBy,
                        principalSchema: "public",
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "StarTransactions",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    StudentProfileId = table.Column<Guid>(type: "uuid", nullable: false),
                    Amount = table.Column<int>(type: "integer", nullable: false),
                    Reason = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    SourceType = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    SourceId = table.Column<Guid>(type: "uuid", nullable: true),
                    BalanceAfter = table.Column<int>(type: "integer", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StarTransactions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StarTransactions_Profiles_StudentProfileId",
                        column: x => x.StudentProfileId,
                        principalSchema: "public",
                        principalTable: "Profiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_StarTransactions_Users_CreatedBy",
                        column: x => x.CreatedBy,
                        principalSchema: "public",
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "StudentLevels",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    StudentProfileId = table.Column<Guid>(type: "uuid", nullable: false),
                    CurrentLevel = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    CurrentXp = table.Column<int>(type: "integer", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StudentLevels", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StudentLevels_Profiles_StudentProfileId",
                        column: x => x.StudentProfileId,
                        principalSchema: "public",
                        principalTable: "Profiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "StudentMonthlyReports",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    StudentProfileId = table.Column<Guid>(type: "uuid", nullable: false),
                    ClassId = table.Column<Guid>(type: "uuid", nullable: true),
                    JobId = table.Column<Guid>(type: "uuid", nullable: true),
                    Month = table.Column<int>(type: "integer", nullable: false),
                    Year = table.Column<int>(type: "integer", nullable: false),
                    DraftContent = table.Column<string>(type: "jsonb", nullable: true),
                    FinalContent = table.Column<string>(type: "jsonb", nullable: true),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    AiVersion = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    PdfUrl = table.Column<string>(type: "text", nullable: true),
                    PdfGeneratedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    SubmittedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    ReviewedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    ReviewedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    PublishedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StudentMonthlyReports", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StudentMonthlyReports_Classes_ClassId",
                        column: x => x.ClassId,
                        principalSchema: "public",
                        principalTable: "Classes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_StudentMonthlyReports_MonthlyReportJobs_JobId",
                        column: x => x.JobId,
                        principalSchema: "public",
                        principalTable: "MonthlyReportJobs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_StudentMonthlyReports_Profiles_StudentProfileId",
                        column: x => x.StudentProfileId,
                        principalSchema: "public",
                        principalTable: "Profiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_StudentMonthlyReports_Users_ReviewedBy",
                        column: x => x.ReviewedBy,
                        principalSchema: "public",
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_StudentMonthlyReports_Users_SubmittedBy",
                        column: x => x.SubmittedBy,
                        principalSchema: "public",
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Tickets",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    OpenedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    OpenedByProfileId = table.Column<Guid>(type: "uuid", nullable: true),
                    BranchId = table.Column<Guid>(type: "uuid", nullable: false),
                    ClassId = table.Column<Guid>(type: "uuid", nullable: true),
                    Category = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    Subject = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Message = table.Column<string>(type: "text", nullable: false),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Type = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    AssignedToUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    IsIncidentReport = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    IncidentCategory = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    IncidentStatus = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    IncidentEvidenceUrl = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tickets", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Tickets_Branches_BranchId",
                        column: x => x.BranchId,
                        principalSchema: "public",
                        principalTable: "Branches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Tickets_Classes_ClassId",
                        column: x => x.ClassId,
                        principalSchema: "public",
                        principalTable: "Classes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Tickets_Profiles_OpenedByProfileId",
                        column: x => x.OpenedByProfileId,
                        principalSchema: "public",
                        principalTable: "Profiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Tickets_Users_AssignedToUserId",
                        column: x => x.AssignedToUserId,
                        principalSchema: "public",
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Tickets_Users_OpenedByUserId",
                        column: x => x.OpenedByUserId,
                        principalSchema: "public",
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TeachingMaterialAnnotations",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TeachingMaterialId = table.Column<Guid>(type: "uuid", nullable: false),
                    SlideNumber = table.Column<int>(type: "integer", nullable: true),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Content = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    Color = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true, defaultValue: "#FFD700"),
                    PositionX = table.Column<double>(type: "double precision", nullable: true),
                    PositionY = table.Column<double>(type: "double precision", nullable: true),
                    Type = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false, defaultValue: "Note"),
                    Visibility = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false, defaultValue: "Private"),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TeachingMaterialAnnotations", x => x.Id);
                    table.CheckConstraint("CK_Annotation_Type", "\"Type\" IN ('Note', 'Highlight', 'Pin')");
                    table.CheckConstraint("CK_Annotation_Visibility", "\"Visibility\" IN ('Private', 'Class', 'Public')");
                    table.ForeignKey(
                        name: "FK_TeachingMaterialAnnotations_TeachingMaterials_TeachingMater~",
                        column: x => x.TeachingMaterialId,
                        principalSchema: "public",
                        principalTable: "TeachingMaterials",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TeachingMaterialAnnotations_Users_UserId",
                        column: x => x.UserId,
                        principalSchema: "public",
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TeachingMaterialBookmarks",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TeachingMaterialId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Note = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TeachingMaterialBookmarks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TeachingMaterialBookmarks_TeachingMaterials_TeachingMateria~",
                        column: x => x.TeachingMaterialId,
                        principalSchema: "public",
                        principalTable: "TeachingMaterials",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TeachingMaterialBookmarks_Users_UserId",
                        column: x => x.UserId,
                        principalSchema: "public",
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TeachingMaterialSlides",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TeachingMaterialId = table.Column<Guid>(type: "uuid", nullable: false),
                    SlideNumber = table.Column<int>(type: "integer", nullable: false),
                    PreviewImagePath = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    ThumbnailImagePath = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    Width = table.Column<int>(type: "integer", nullable: false, defaultValue: 1920),
                    Height = table.Column<int>(type: "integer", nullable: false, defaultValue: 1080),
                    Notes = table.Column<string>(type: "text", nullable: true),
                    GeneratedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TeachingMaterialSlides", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TeachingMaterialSlides_TeachingMaterials_TeachingMaterialId",
                        column: x => x.TeachingMaterialId,
                        principalSchema: "public",
                        principalTable: "TeachingMaterials",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TeachingMaterialViewProgresses",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TeachingMaterialId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    ProgressPercent = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    LastSlideViewed = table.Column<int>(type: "integer", nullable: true),
                    TotalTimeSeconds = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    ViewCount = table.Column<int>(type: "integer", nullable: false, defaultValue: 1),
                    Completed = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    FirstViewedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    LastViewedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TeachingMaterialViewProgresses", x => x.Id);
                    table.CheckConstraint("CK_ViewProgress_Percent", "\"ProgressPercent\" >= 0 AND \"ProgressPercent\" <= 100");
                    table.ForeignKey(
                        name: "FK_TeachingMaterialViewProgresses_TeachingMaterials_TeachingMa~",
                        column: x => x.TeachingMaterialId,
                        principalSchema: "public",
                        principalTable: "TeachingMaterials",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TeachingMaterialViewProgresses_Users_UserId",
                        column: x => x.UserId,
                        principalSchema: "public",
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Registrations",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    StudentProfileId = table.Column<Guid>(type: "uuid", nullable: false),
                    BranchId = table.Column<Guid>(type: "uuid", nullable: false),
                    ProgramId = table.Column<Guid>(type: "uuid", nullable: false),
                    TuitionPlanId = table.Column<Guid>(type: "uuid", nullable: false),
                    SecondaryProgramId = table.Column<Guid>(type: "uuid", nullable: true),
                    RegistrationDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ExpectedStartDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ActualStartDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    PreferredSchedule = table.Column<string>(type: "text", nullable: true),
                    Note = table.Column<string>(type: "text", nullable: true),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    ClassId = table.Column<Guid>(type: "uuid", nullable: true),
                    ClassAssignedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    EntryType = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    SecondaryClassId = table.Column<Guid>(type: "uuid", nullable: true),
                    SecondaryClassAssignedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    SecondaryEntryType = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    SecondaryProgramSkillFocus = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    OriginalRegistrationId = table.Column<Guid>(type: "uuid", nullable: true),
                    OperationType = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    DiscountCampaignId = table.Column<Guid>(type: "uuid", nullable: true),
                    DiscountCampaignName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    DiscountType = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    DiscountValue = table.Column<decimal>(type: "numeric(18,2)", nullable: true),
                    OriginalTuitionAmount = table.Column<decimal>(type: "numeric(18,2)", nullable: true),
                    DiscountAmount = table.Column<decimal>(type: "numeric(18,2)", nullable: true),
                    CarryOverCreditAmount = table.Column<decimal>(type: "numeric(18,2)", nullable: true),
                    FinalTuitionAmount = table.Column<decimal>(type: "numeric(18,2)", nullable: true),
                    PricingAppliedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    TotalSessions = table.Column<int>(type: "integer", nullable: false),
                    UsedSessions = table.Column<int>(type: "integer", nullable: false),
                    RemainingSessions = table.Column<int>(type: "integer", nullable: false),
                    ExpiryDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Registrations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Registrations_Branches_BranchId",
                        column: x => x.BranchId,
                        principalSchema: "public",
                        principalTable: "Branches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Registrations_Classes_ClassId",
                        column: x => x.ClassId,
                        principalSchema: "public",
                        principalTable: "Classes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Registrations_Classes_SecondaryClassId",
                        column: x => x.SecondaryClassId,
                        principalSchema: "public",
                        principalTable: "Classes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Registrations_Profiles_StudentProfileId",
                        column: x => x.StudentProfileId,
                        principalSchema: "public",
                        principalTable: "Profiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Registrations_Programs_ProgramId",
                        column: x => x.ProgramId,
                        principalSchema: "public",
                        principalTable: "Programs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Registrations_Programs_SecondaryProgramId",
                        column: x => x.SecondaryProgramId,
                        principalSchema: "public",
                        principalTable: "Programs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Registrations_RegistrationDiscountCampaigns_DiscountCampaig~",
                        column: x => x.DiscountCampaignId,
                        principalSchema: "public",
                        principalTable: "RegistrationDiscountCampaigns",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Registrations_Registrations_OriginalRegistrationId",
                        column: x => x.OriginalRegistrationId,
                        principalSchema: "public",
                        principalTable: "Registrations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Registrations_TuitionPlans_TuitionPlanId",
                        column: x => x.TuitionPlanId,
                        principalSchema: "public",
                        principalTable: "TuitionPlans",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ExamQuestions",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ExamId = table.Column<Guid>(type: "uuid", nullable: false),
                    OrderIndex = table.Column<int>(type: "integer", nullable: false),
                    QuestionText = table.Column<string>(type: "text", nullable: false),
                    QuestionType = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Options = table.Column<string>(type: "jsonb", nullable: true),
                    CorrectAnswer = table.Column<string>(type: "text", nullable: true),
                    Points = table.Column<int>(type: "integer", nullable: false),
                    Explanation = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExamQuestions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ExamQuestions_Exams_ExamId",
                        column: x => x.ExamId,
                        principalSchema: "public",
                        principalTable: "Exams",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ExamResults",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ExamId = table.Column<Guid>(type: "uuid", nullable: false),
                    StudentProfileId = table.Column<Guid>(type: "uuid", nullable: false),
                    Score = table.Column<decimal>(type: "numeric", nullable: true),
                    Comment = table.Column<string>(type: "text", nullable: true),
                    AttachmentUrls = table.Column<string>(type: "jsonb", nullable: true),
                    GradedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    GradedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExamResults", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ExamResults_Exams_ExamId",
                        column: x => x.ExamId,
                        principalSchema: "public",
                        principalTable: "Exams",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ExamResults_Profiles_StudentProfileId",
                        column: x => x.StudentProfileId,
                        principalSchema: "public",
                        principalTable: "Profiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ExamResults_Users_GradedBy",
                        column: x => x.GradedBy,
                        principalSchema: "public",
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ExamSubmissions",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ExamId = table.Column<Guid>(type: "uuid", nullable: false),
                    StudentProfileId = table.Column<Guid>(type: "uuid", nullable: false),
                    ActualStartTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    SubmittedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    AutoSubmittedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    TimeSpentMinutes = table.Column<int>(type: "integer", nullable: true),
                    AutoScore = table.Column<decimal>(type: "numeric(18,2)", nullable: true),
                    FinalScore = table.Column<decimal>(type: "numeric(18,2)", nullable: true),
                    GradedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    GradedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    TeacherComment = table.Column<string>(type: "text", nullable: true),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExamSubmissions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ExamSubmissions_Exams_ExamId",
                        column: x => x.ExamId,
                        principalSchema: "public",
                        principalTable: "Exams",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ExamSubmissions_Profiles_StudentProfileId",
                        column: x => x.StudentProfileId,
                        principalSchema: "public",
                        principalTable: "Profiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ExamSubmissions_Users_GradedBy",
                        column: x => x.GradedBy,
                        principalSchema: "public",
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "MissionProgresses",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    MissionId = table.Column<Guid>(type: "uuid", nullable: false),
                    StudentProfileId = table.Column<Guid>(type: "uuid", nullable: false),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    ProgressValue = table.Column<decimal>(type: "numeric", nullable: true),
                    CompletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    VerifiedBy = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MissionProgresses", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MissionProgresses_Missions_MissionId",
                        column: x => x.MissionId,
                        principalSchema: "public",
                        principalTable: "Missions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MissionProgresses_Profiles_StudentProfileId",
                        column: x => x.StudentProfileId,
                        principalSchema: "public",
                        principalTable: "Profiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MissionProgresses_Users_VerifiedBy",
                        column: x => x.VerifiedBy,
                        principalSchema: "public",
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Attendances",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SessionId = table.Column<Guid>(type: "uuid", nullable: false),
                    StudentProfileId = table.Column<Guid>(type: "uuid", nullable: false),
                    AttendanceStatus = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    AbsenceType = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    MarkedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    MarkedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Note = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Attendances", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Attendances_Profiles_StudentProfileId",
                        column: x => x.StudentProfileId,
                        principalSchema: "public",
                        principalTable: "Profiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Attendances_Sessions_SessionId",
                        column: x => x.SessionId,
                        principalSchema: "public",
                        principalTable: "Sessions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Attendances_Users_MarkedBy",
                        column: x => x.MarkedBy,
                        principalSchema: "public",
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "HomeworkAssignments",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ClassId = table.Column<Guid>(type: "uuid", nullable: false),
                    SessionId = table.Column<Guid>(type: "uuid", nullable: true),
                    Title = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
                    StartDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DueAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Book = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    Pages = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Skills = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Topic = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    GrammarTags = table.Column<string>(type: "jsonb", nullable: true),
                    VocabularyTags = table.Column<string>(type: "jsonb", nullable: true),
                    SubmissionType = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    MaxScore = table.Column<decimal>(type: "numeric", nullable: true),
                    RewardStars = table.Column<int>(type: "integer", nullable: true),
                    TimeLimitMinutes = table.Column<int>(type: "integer", nullable: true),
                    MaxAttempts = table.Column<int>(type: "integer", nullable: false, defaultValue: 1),
                    AiHintEnabled = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    AiRecommendEnabled = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    MissionId = table.Column<Guid>(type: "uuid", nullable: true),
                    Instructions = table.Column<string>(type: "text", nullable: true),
                    ExpectedAnswer = table.Column<string>(type: "text", nullable: true),
                    Rubric = table.Column<string>(type: "text", nullable: true),
                    SpeakingMode = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    TargetWords = table.Column<string>(type: "jsonb", nullable: true),
                    SpeakingExpectedText = table.Column<string>(type: "text", nullable: true),
                    AttachmentUrl = table.Column<string>(type: "text", nullable: true),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HomeworkAssignments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HomeworkAssignments_Classes_ClassId",
                        column: x => x.ClassId,
                        principalSchema: "public",
                        principalTable: "Classes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_HomeworkAssignments_Missions_MissionId",
                        column: x => x.MissionId,
                        principalSchema: "public",
                        principalTable: "Missions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_HomeworkAssignments_Sessions_SessionId",
                        column: x => x.SessionId,
                        principalSchema: "public",
                        principalTable: "Sessions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_HomeworkAssignments_Users_CreatedBy",
                        column: x => x.CreatedBy,
                        principalSchema: "public",
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "LeaveRequests",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    StudentProfileId = table.Column<Guid>(type: "uuid", nullable: false),
                    ClassId = table.Column<Guid>(type: "uuid", nullable: false),
                    SessionId = table.Column<Guid>(type: "uuid", nullable: true),
                    SessionDate = table.Column<DateOnly>(type: "date", nullable: false),
                    EndDate = table.Column<DateOnly>(type: "date", nullable: true),
                    Reason = table.Column<string>(type: "text", nullable: true),
                    NoticeHours = table.Column<int>(type: "integer", nullable: true),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    RequestedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ApprovedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    ApprovedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CancelledAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LeaveRequests", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LeaveRequests_Classes_ClassId",
                        column: x => x.ClassId,
                        principalSchema: "public",
                        principalTable: "Classes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_LeaveRequests_Profiles_StudentProfileId",
                        column: x => x.StudentProfileId,
                        principalSchema: "public",
                        principalTable: "Profiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_LeaveRequests_Sessions_SessionId",
                        column: x => x.SessionId,
                        principalSchema: "public",
                        principalTable: "Sessions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_LeaveRequests_Users_ApprovedBy",
                        column: x => x.ApprovedBy,
                        principalSchema: "public",
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "LessonPlans",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ClassId = table.Column<Guid>(type: "uuid", nullable: false),
                    SessionId = table.Column<Guid>(type: "uuid", nullable: false),
                    TemplateId = table.Column<Guid>(type: "uuid", nullable: true),
                    PlannedContent = table.Column<string>(type: "text", nullable: true),
                    ActualContent = table.Column<string>(type: "text", nullable: true),
                    ActualHomework = table.Column<string>(type: "text", nullable: true),
                    TeacherNotes = table.Column<string>(type: "text", nullable: true),
                    CoverImageUrl = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    CoverImageMimeType = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    CoverImageFileSize = table.Column<long>(type: "bigint", nullable: true),
                    MediaUrl = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    MediaMimeType = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    MediaFileSize = table.Column<long>(type: "bigint", nullable: true),
                    MediaType = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    SubmittedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    SubmittedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LessonPlans", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LessonPlans_Classes_ClassId",
                        column: x => x.ClassId,
                        principalSchema: "public",
                        principalTable: "Classes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_LessonPlans_LessonPlanTemplates_TemplateId",
                        column: x => x.TemplateId,
                        principalSchema: "public",
                        principalTable: "LessonPlanTemplates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_LessonPlans_Sessions_SessionId",
                        column: x => x.SessionId,
                        principalSchema: "public",
                        principalTable: "Sessions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_LessonPlans_Users_SubmittedBy",
                        column: x => x.SubmittedBy,
                        principalSchema: "public",
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "MakeupCredits",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    StudentProfileId = table.Column<Guid>(type: "uuid", nullable: false),
                    SourceSessionId = table.Column<Guid>(type: "uuid", nullable: false),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    CreatedReason = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UsedSessionId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MakeupCredits", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MakeupCredits_Profiles_StudentProfileId",
                        column: x => x.StudentProfileId,
                        principalSchema: "public",
                        principalTable: "Profiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MakeupCredits_Sessions_SourceSessionId",
                        column: x => x.SourceSessionId,
                        principalSchema: "public",
                        principalTable: "Sessions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MakeupCredits_Sessions_UsedSessionId",
                        column: x => x.UsedSessionId,
                        principalSchema: "public",
                        principalTable: "Sessions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "SessionReports",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SessionId = table.Column<Guid>(type: "uuid", nullable: false),
                    StudentProfileId = table.Column<Guid>(type: "uuid", nullable: false),
                    TeacherUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    ReportDate = table.Column<DateOnly>(type: "date", nullable: false),
                    Feedback = table.Column<string>(type: "text", nullable: false),
                    AiGeneratedSummary = table.Column<string>(type: "text", nullable: true),
                    Status = table.Column<string>(type: "varchar(20)", nullable: false),
                    DraftContent = table.Column<string>(type: "text", nullable: true),
                    FinalContent = table.Column<string>(type: "text", nullable: true),
                    SubmittedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    ReviewedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    ReviewedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    PublishedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    AiVersion = table.Column<string>(type: "text", nullable: true),
                    IsMonthlyCompiled = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SessionReports", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SessionReports_Profiles_StudentProfileId",
                        column: x => x.StudentProfileId,
                        principalSchema: "public",
                        principalTable: "Profiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SessionReports_Sessions_SessionId",
                        column: x => x.SessionId,
                        principalSchema: "public",
                        principalTable: "Sessions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SessionReports_Users_ReviewedByUserId",
                        column: x => x.ReviewedByUserId,
                        principalSchema: "public",
                        principalTable: "Users",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_SessionReports_Users_SubmittedByUserId",
                        column: x => x.SubmittedByUserId,
                        principalSchema: "public",
                        principalTable: "Users",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_SessionReports_Users_TeacherUserId",
                        column: x => x.TeacherUserId,
                        principalSchema: "public",
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "SessionRoles",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SessionId = table.Column<Guid>(type: "uuid", nullable: false),
                    StaffUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    RoleType = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    PayableUnitPrice = table.Column<decimal>(type: "numeric", nullable: true),
                    PayableAllowance = table.Column<decimal>(type: "numeric", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SessionRoles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SessionRoles_Sessions_SessionId",
                        column: x => x.SessionId,
                        principalSchema: "public",
                        principalTable: "Sessions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SessionRoles_Users_StaffUserId",
                        column: x => x.StaffUserId,
                        principalSchema: "public",
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "InvoiceLines",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    InvoiceId = table.Column<Guid>(type: "uuid", nullable: false),
                    ItemType = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    Quantity = table.Column<int>(type: "integer", nullable: false),
                    UnitPrice = table.Column<decimal>(type: "numeric", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
                    SessionIds = table.Column<string>(type: "jsonb", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InvoiceLines", x => x.Id);
                    table.ForeignKey(
                        name: "FK_InvoiceLines_Invoices_InvoiceId",
                        column: x => x.InvoiceId,
                        principalSchema: "public",
                        principalTable: "Invoices",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Payments",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    InvoiceId = table.Column<Guid>(type: "uuid", nullable: false),
                    Method = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Amount = table.Column<decimal>(type: "numeric", nullable: false),
                    PaidAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ReferenceCode = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    ConfirmedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    EvidenceUrl = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Payments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Payments_Invoices_InvoiceId",
                        column: x => x.InvoiceId,
                        principalSchema: "public",
                        principalTable: "Invoices",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Payments_Users_ConfirmedBy",
                        column: x => x.ConfirmedBy,
                        principalSchema: "public",
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PlacementTests",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    LeadId = table.Column<Guid>(type: "uuid", nullable: true),
                    LeadChildId = table.Column<Guid>(type: "uuid", nullable: true),
                    StudentProfileId = table.Column<Guid>(type: "uuid", nullable: true),
                    ClassId = table.Column<Guid>(type: "uuid", nullable: true),
                    ScheduledAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DurationMinutes = table.Column<int>(type: "integer", nullable: false, defaultValue: 60),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    RoomId = table.Column<Guid>(type: "uuid", nullable: true),
                    Room = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    InvigilatorUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    OriginalPlacementTestId = table.Column<Guid>(type: "uuid", nullable: true),
                    ResultScore = table.Column<decimal>(type: "numeric", nullable: true),
                    ListeningScore = table.Column<decimal>(type: "numeric", nullable: true),
                    SpeakingScore = table.Column<decimal>(type: "numeric", nullable: true),
                    ReadingScore = table.Column<decimal>(type: "numeric", nullable: true),
                    WritingScore = table.Column<decimal>(type: "numeric", nullable: true),
                    LevelRecommendation = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    ProgramRecommendationId = table.Column<Guid>(type: "uuid", nullable: true),
                    SecondaryProgramRecommendationId = table.Column<Guid>(type: "uuid", nullable: true),
                    SecondaryProgramSkillFocus = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Notes = table.Column<string>(type: "text", nullable: true),
                    AttachmentUrl = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlacementTests", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PlacementTests_Classes_ClassId",
                        column: x => x.ClassId,
                        principalSchema: "public",
                        principalTable: "Classes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PlacementTests_Classrooms_RoomId",
                        column: x => x.RoomId,
                        principalSchema: "public",
                        principalTable: "Classrooms",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PlacementTests_LeadChildren_LeadChildId",
                        column: x => x.LeadChildId,
                        principalSchema: "public",
                        principalTable: "LeadChildren",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PlacementTests_Leads_LeadId",
                        column: x => x.LeadId,
                        principalSchema: "public",
                        principalTable: "Leads",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PlacementTests_PlacementTests_OriginalPlacementTestId",
                        column: x => x.OriginalPlacementTestId,
                        principalSchema: "public",
                        principalTable: "PlacementTests",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_PlacementTests_Profiles_StudentProfileId",
                        column: x => x.StudentProfileId,
                        principalSchema: "public",
                        principalTable: "Profiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PlacementTests_Programs_ProgramRecommendationId",
                        column: x => x.ProgramRecommendationId,
                        principalSchema: "public",
                        principalTable: "Programs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PlacementTests_Programs_SecondaryProgramRecommendationId",
                        column: x => x.SecondaryProgramRecommendationId,
                        principalSchema: "public",
                        principalTable: "Programs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PlacementTests_Users_InvigilatorUserId",
                        column: x => x.InvigilatorUserId,
                        principalSchema: "public",
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "MonthlyReportData",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ReportId = table.Column<Guid>(type: "uuid", nullable: false),
                    StudentProfileId = table.Column<Guid>(type: "uuid", nullable: false),
                    Month = table.Column<int>(type: "integer", nullable: false),
                    Year = table.Column<int>(type: "integer", nullable: false),
                    AttendanceData = table.Column<string>(type: "jsonb", nullable: true),
                    HomeworkData = table.Column<string>(type: "jsonb", nullable: true),
                    TestData = table.Column<string>(type: "jsonb", nullable: true),
                    MissionData = table.Column<string>(type: "jsonb", nullable: true),
                    NotesData = table.Column<string>(type: "jsonb", nullable: true),
                    TopicsData = table.Column<string>(type: "jsonb", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MonthlyReportData", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MonthlyReportData_Profiles_StudentProfileId",
                        column: x => x.StudentProfileId,
                        principalSchema: "public",
                        principalTable: "Profiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MonthlyReportData_StudentMonthlyReports_ReportId",
                        column: x => x.ReportId,
                        principalSchema: "public",
                        principalTable: "StudentMonthlyReports",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TicketComments",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TicketId = table.Column<Guid>(type: "uuid", nullable: false),
                    CommenterUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    CommenterProfileId = table.Column<Guid>(type: "uuid", nullable: true),
                    Message = table.Column<string>(type: "text", nullable: false),
                    AttachmentUrl = table.Column<string>(type: "text", nullable: true),
                    IncidentCommentType = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TicketComments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TicketComments_Profiles_CommenterProfileId",
                        column: x => x.CommenterProfileId,
                        principalSchema: "public",
                        principalTable: "Profiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TicketComments_Tickets_TicketId",
                        column: x => x.TicketId,
                        principalSchema: "public",
                        principalTable: "Tickets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TicketComments_Users_CommenterUserId",
                        column: x => x.CommenterUserId,
                        principalSchema: "public",
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ClassEnrollments",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ClassId = table.Column<Guid>(type: "uuid", nullable: false),
                    StudentProfileId = table.Column<Guid>(type: "uuid", nullable: false),
                    EnrollDate = table.Column<DateOnly>(type: "date", nullable: false),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    TuitionPlanId = table.Column<Guid>(type: "uuid", nullable: true),
                    RegistrationId = table.Column<Guid>(type: "uuid", nullable: true),
                    Track = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    SessionSelectionPattern = table.Column<string>(type: "text", nullable: true),
                    EnrollmentConfirmationPdfUrl = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    EnrollmentConfirmationPdfGeneratedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    EnrollmentConfirmationPdfGeneratedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClassEnrollments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ClassEnrollments_Classes_ClassId",
                        column: x => x.ClassId,
                        principalSchema: "public",
                        principalTable: "Classes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ClassEnrollments_Profiles_StudentProfileId",
                        column: x => x.StudentProfileId,
                        principalSchema: "public",
                        principalTable: "Profiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ClassEnrollments_Registrations_RegistrationId",
                        column: x => x.RegistrationId,
                        principalSchema: "public",
                        principalTable: "Registrations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_ClassEnrollments_TuitionPlans_TuitionPlanId",
                        column: x => x.TuitionPlanId,
                        principalSchema: "public",
                        principalTable: "TuitionPlans",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ExamSubmissionAnswers",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SubmissionId = table.Column<Guid>(type: "uuid", nullable: false),
                    QuestionId = table.Column<Guid>(type: "uuid", nullable: false),
                    Answer = table.Column<string>(type: "text", nullable: false),
                    IsCorrect = table.Column<bool>(type: "boolean", nullable: false),
                    PointsAwarded = table.Column<decimal>(type: "numeric(18,2)", nullable: true),
                    TeacherFeedback = table.Column<string>(type: "text", nullable: true),
                    AnsweredAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExamSubmissionAnswers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ExamSubmissionAnswers_ExamQuestions_QuestionId",
                        column: x => x.QuestionId,
                        principalSchema: "public",
                        principalTable: "ExamQuestions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ExamSubmissionAnswers_ExamSubmissions_SubmissionId",
                        column: x => x.SubmissionId,
                        principalSchema: "public",
                        principalTable: "ExamSubmissions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "HomeworkQuestions",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    HomeworkAssignmentId = table.Column<Guid>(type: "uuid", nullable: false),
                    OrderIndex = table.Column<int>(type: "integer", nullable: false),
                    QuestionText = table.Column<string>(type: "text", nullable: false),
                    QuestionType = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Options = table.Column<string>(type: "jsonb", nullable: true),
                    CorrectAnswer = table.Column<string>(type: "text", nullable: true),
                    Points = table.Column<int>(type: "integer", nullable: false),
                    Explanation = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HomeworkQuestions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HomeworkQuestions_HomeworkAssignments_HomeworkAssignmentId",
                        column: x => x.HomeworkAssignmentId,
                        principalSchema: "public",
                        principalTable: "HomeworkAssignments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "HomeworkStudents",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AssignmentId = table.Column<Guid>(type: "uuid", nullable: false),
                    StudentProfileId = table.Column<Guid>(type: "uuid", nullable: false),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    StartedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    SubmittedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    GradedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Score = table.Column<decimal>(type: "numeric", nullable: true),
                    TeacherFeedback = table.Column<string>(type: "text", nullable: true),
                    AiFeedback = table.Column<string>(type: "jsonb", nullable: true),
                    TextAnswer = table.Column<string>(type: "text", nullable: true),
                    AttachmentUrl = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HomeworkStudents", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HomeworkStudents_HomeworkAssignments_AssignmentId",
                        column: x => x.AssignmentId,
                        principalSchema: "public",
                        principalTable: "HomeworkAssignments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_HomeworkStudents_Profiles_StudentProfileId",
                        column: x => x.StudentProfileId,
                        principalSchema: "public",
                        principalTable: "Profiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MakeupAllocations",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    MakeupCreditId = table.Column<Guid>(type: "uuid", nullable: false),
                    TargetSessionId = table.Column<Guid>(type: "uuid", nullable: false),
                    Status = table.Column<string>(type: "text", nullable: false),
                    AssignedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    AssignedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MakeupAllocations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MakeupAllocations_MakeupCredits_MakeupCreditId",
                        column: x => x.MakeupCreditId,
                        principalSchema: "public",
                        principalTable: "MakeupCredits",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MakeupAllocations_Sessions_TargetSessionId",
                        column: x => x.TargetSessionId,
                        principalSchema: "public",
                        principalTable: "Sessions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MakeupAllocations_Users_AssignedBy",
                        column: x => x.AssignedBy,
                        principalSchema: "public",
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ReportComments",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ReportId = table.Column<Guid>(type: "uuid", nullable: true),
                    SessionReportId = table.Column<Guid>(type: "uuid", nullable: true),
                    CommenterId = table.Column<Guid>(type: "uuid", nullable: false),
                    Content = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReportComments", x => x.Id);
                    table.CheckConstraint("CK_ReportComment_AtLeastOneReportId", "(\"ReportId\" IS NOT NULL OR \"SessionReportId\" IS NOT NULL)");
                    table.ForeignKey(
                        name: "FK_ReportComments_SessionReports_SessionReportId",
                        column: x => x.SessionReportId,
                        principalSchema: "public",
                        principalTable: "SessionReports",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ReportComments_StudentMonthlyReports_ReportId",
                        column: x => x.ReportId,
                        principalSchema: "public",
                        principalTable: "StudentMonthlyReports",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ReportComments_Users_CommenterId",
                        column: x => x.CommenterId,
                        principalSchema: "public",
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ReportRequests",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ReportType = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Priority = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    AssignedTeacherUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    RequestedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    TargetStudentProfileId = table.Column<Guid>(type: "uuid", nullable: true),
                    TargetClassId = table.Column<Guid>(type: "uuid", nullable: true),
                    TargetSessionId = table.Column<Guid>(type: "uuid", nullable: true),
                    Month = table.Column<int>(type: "integer", nullable: true),
                    Year = table.Column<int>(type: "integer", nullable: true),
                    Message = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    DueAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LinkedSessionReportId = table.Column<Guid>(type: "uuid", nullable: true),
                    LinkedMonthlyReportId = table.Column<Guid>(type: "uuid", nullable: true),
                    SubmittedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReportRequests", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ReportRequests_Classes_TargetClassId",
                        column: x => x.TargetClassId,
                        principalSchema: "public",
                        principalTable: "Classes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_ReportRequests_Profiles_TargetStudentProfileId",
                        column: x => x.TargetStudentProfileId,
                        principalSchema: "public",
                        principalTable: "Profiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_ReportRequests_SessionReports_LinkedSessionReportId",
                        column: x => x.LinkedSessionReportId,
                        principalSchema: "public",
                        principalTable: "SessionReports",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_ReportRequests_Sessions_TargetSessionId",
                        column: x => x.TargetSessionId,
                        principalSchema: "public",
                        principalTable: "Sessions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_ReportRequests_StudentMonthlyReports_LinkedMonthlyReportId",
                        column: x => x.LinkedMonthlyReportId,
                        principalSchema: "public",
                        principalTable: "StudentMonthlyReports",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_ReportRequests_Users_AssignedTeacherUserId",
                        column: x => x.AssignedTeacherUserId,
                        principalSchema: "public",
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ReportRequests_Users_RequestedByUserId",
                        column: x => x.RequestedByUserId,
                        principalSchema: "public",
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ClassEnrollmentScheduleSegments",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ClassEnrollmentId = table.Column<Guid>(type: "uuid", nullable: false),
                    EffectiveFrom = table.Column<DateOnly>(type: "date", nullable: false),
                    EffectiveTo = table.Column<DateOnly>(type: "date", nullable: true),
                    SessionSelectionPattern = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClassEnrollmentScheduleSegments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ClassEnrollmentScheduleSegments_ClassEnrollments_ClassEnrol~",
                        column: x => x.ClassEnrollmentId,
                        principalSchema: "public",
                        principalTable: "ClassEnrollments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "EnrollmentConfirmationPdfs",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    RegistrationId = table.Column<Guid>(type: "uuid", nullable: false),
                    EnrollmentId = table.Column<Guid>(type: "uuid", nullable: false),
                    Track = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    FormType = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    PdfUrl = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    GeneratedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    GeneratedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    SnapshotJson = table.Column<string>(type: "jsonb", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EnrollmentConfirmationPdfs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EnrollmentConfirmationPdfs_ClassEnrollments_EnrollmentId",
                        column: x => x.EnrollmentId,
                        principalSchema: "public",
                        principalTable: "ClassEnrollments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_EnrollmentConfirmationPdfs_Registrations_RegistrationId",
                        column: x => x.RegistrationId,
                        principalSchema: "public",
                        principalTable: "Registrations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PauseEnrollmentRequests",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    StudentProfileId = table.Column<Guid>(type: "uuid", nullable: false),
                    ClassId = table.Column<Guid>(type: "uuid", nullable: true),
                    PauseFrom = table.Column<DateOnly>(type: "date", nullable: false),
                    PauseTo = table.Column<DateOnly>(type: "date", nullable: false),
                    Reason = table.Column<string>(type: "text", nullable: true),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    RequestedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ApprovedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    ApprovedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CancelledBy = table.Column<Guid>(type: "uuid", nullable: true),
                    CancelledAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Outcome = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: true),
                    OutcomeNote = table.Column<string>(type: "text", nullable: true),
                    OutcomeBy = table.Column<Guid>(type: "uuid", nullable: true),
                    OutcomeAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ReassignedClassId = table.Column<Guid>(type: "uuid", nullable: true),
                    ReassignedEnrollmentId = table.Column<Guid>(type: "uuid", nullable: true),
                    OutcomeCompletedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    OutcomeCompletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ReservedSessionCount = table.Column<int>(type: "integer", nullable: false),
                    ReservationExpiresOn = table.Column<DateOnly>(type: "date", nullable: true),
                    ReservationSnapshotAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PauseEnrollmentRequests", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PauseEnrollmentRequests_ClassEnrollments_ReassignedEnrollme~",
                        column: x => x.ReassignedEnrollmentId,
                        principalSchema: "public",
                        principalTable: "ClassEnrollments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_PauseEnrollmentRequests_Classes_ClassId",
                        column: x => x.ClassId,
                        principalSchema: "public",
                        principalTable: "Classes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PauseEnrollmentRequests_Classes_ReassignedClassId",
                        column: x => x.ReassignedClassId,
                        principalSchema: "public",
                        principalTable: "Classes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PauseEnrollmentRequests_Profiles_StudentProfileId",
                        column: x => x.StudentProfileId,
                        principalSchema: "public",
                        principalTable: "Profiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PauseEnrollmentRequests_Users_ApprovedBy",
                        column: x => x.ApprovedBy,
                        principalSchema: "public",
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PauseEnrollmentRequests_Users_CancelledBy",
                        column: x => x.CancelledBy,
                        principalSchema: "public",
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PauseEnrollmentRequests_Users_OutcomeBy",
                        column: x => x.OutcomeBy,
                        principalSchema: "public",
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PauseEnrollmentRequests_Users_OutcomeCompletedBy",
                        column: x => x.OutcomeCompletedBy,
                        principalSchema: "public",
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ProgramProgressionScheduleParticipants",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ScheduleId = table.Column<Guid>(type: "uuid", nullable: false),
                    StudentProfileId = table.Column<Guid>(type: "uuid", nullable: false),
                    SourceRegistrationId = table.Column<Guid>(type: "uuid", nullable: false),
                    SourceEnrollmentId = table.Column<Guid>(type: "uuid", nullable: true),
                    Status = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProgramProgressionScheduleParticipants", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProgramProgressionScheduleParticipants_ClassEnrollments_Sou~",
                        column: x => x.SourceEnrollmentId,
                        principalSchema: "public",
                        principalTable: "ClassEnrollments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProgramProgressionScheduleParticipants_Profiles_StudentProf~",
                        column: x => x.StudentProfileId,
                        principalSchema: "public",
                        principalTable: "Profiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProgramProgressionScheduleParticipants_ProgramProgressionSc~",
                        column: x => x.ScheduleId,
                        principalSchema: "public",
                        principalTable: "ProgramProgressionSchedules",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ProgramProgressionScheduleParticipants_Registrations_Source~",
                        column: x => x.SourceRegistrationId,
                        principalSchema: "public",
                        principalTable: "Registrations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "StudentSessionAssignments",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SessionId = table.Column<Guid>(type: "uuid", nullable: false),
                    StudentProfileId = table.Column<Guid>(type: "uuid", nullable: false),
                    ClassEnrollmentId = table.Column<Guid>(type: "uuid", nullable: false),
                    RegistrationId = table.Column<Guid>(type: "uuid", nullable: true),
                    Track = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StudentSessionAssignments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StudentSessionAssignments_ClassEnrollments_ClassEnrollmentId",
                        column: x => x.ClassEnrollmentId,
                        principalSchema: "public",
                        principalTable: "ClassEnrollments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_StudentSessionAssignments_Profiles_StudentProfileId",
                        column: x => x.StudentProfileId,
                        principalSchema: "public",
                        principalTable: "Profiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_StudentSessionAssignments_Registrations_RegistrationId",
                        column: x => x.RegistrationId,
                        principalSchema: "public",
                        principalTable: "Registrations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_StudentSessionAssignments_Sessions_SessionId",
                        column: x => x.SessionId,
                        principalSchema: "public",
                        principalTable: "Sessions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "HomeworkSubmissionAttempts",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    HomeworkStudentId = table.Column<Guid>(type: "uuid", nullable: false),
                    AttemptNumber = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    StartedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    SubmittedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    GradedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Score = table.Column<decimal>(type: "numeric", nullable: true),
                    TeacherFeedback = table.Column<string>(type: "text", nullable: true),
                    AiFeedback = table.Column<string>(type: "jsonb", nullable: true),
                    TextAnswer = table.Column<string>(type: "text", nullable: true),
                    AttachmentUrl = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HomeworkSubmissionAttempts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HomeworkSubmissionAttempts_HomeworkStudents_HomeworkStudent~",
                        column: x => x.HomeworkStudentId,
                        principalSchema: "public",
                        principalTable: "HomeworkStudents",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PauseEnrollmentRequestHistories",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PauseEnrollmentRequestId = table.Column<Guid>(type: "uuid", nullable: false),
                    StudentProfileId = table.Column<Guid>(type: "uuid", nullable: false),
                    ClassId = table.Column<Guid>(type: "uuid", nullable: false),
                    EnrollmentId = table.Column<Guid>(type: "uuid", nullable: true),
                    PreviousStatus = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    NewStatus = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    PauseFrom = table.Column<DateOnly>(type: "date", nullable: false),
                    PauseTo = table.Column<DateOnly>(type: "date", nullable: false),
                    ReservedSessionCount = table.Column<int>(type: "integer", nullable: false),
                    ChangedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ChangedBy = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PauseEnrollmentRequestHistories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PauseEnrollmentRequestHistories_ClassEnrollments_Enrollment~",
                        column: x => x.EnrollmentId,
                        principalSchema: "public",
                        principalTable: "ClassEnrollments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_PauseEnrollmentRequestHistories_Classes_ClassId",
                        column: x => x.ClassId,
                        principalSchema: "public",
                        principalTable: "Classes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PauseEnrollmentRequestHistories_PauseEnrollmentRequests_Pau~",
                        column: x => x.PauseEnrollmentRequestId,
                        principalSchema: "public",
                        principalTable: "PauseEnrollmentRequests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PauseEnrollmentRequestHistories_Profiles_StudentProfileId",
                        column: x => x.StudentProfileId,
                        principalSchema: "public",
                        principalTable: "Profiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PauseEnrollmentRequestHistories_Users_ChangedBy",
                        column: x => x.ChangedBy,
                        principalSchema: "public",
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ProgramProgressionAssessments",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    RuleId = table.Column<Guid>(type: "uuid", nullable: false),
                    ScheduleParticipantId = table.Column<Guid>(type: "uuid", nullable: true),
                    StudentProfileId = table.Column<Guid>(type: "uuid", nullable: false),
                    SourceProgramId = table.Column<Guid>(type: "uuid", nullable: false),
                    TargetProgramId = table.Column<Guid>(type: "uuid", nullable: true),
                    SourceRegistrationId = table.Column<Guid>(type: "uuid", nullable: false),
                    SourceEnrollmentId = table.Column<Guid>(type: "uuid", nullable: true),
                    AssessmentDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Method = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    Status = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    PassedInClass = table.Column<bool>(type: "boolean", nullable: true),
                    ListeningPracticeScore = table.Column<int>(type: "integer", nullable: true),
                    SpeakingPracticeScore = table.Column<int>(type: "integer", nullable: true),
                    ReadingPracticeScore = table.Column<int>(type: "integer", nullable: true),
                    WritingPracticeScore = table.Column<int>(type: "integer", nullable: true),
                    ListeningScore = table.Column<decimal>(type: "numeric", nullable: true),
                    SpeakingScore = table.Column<decimal>(type: "numeric", nullable: true),
                    ReadingWritingScore = table.Column<decimal>(type: "numeric", nullable: true),
                    ReadingScore = table.Column<decimal>(type: "numeric", nullable: true),
                    WritingScore = table.Column<decimal>(type: "numeric", nullable: true),
                    OverallScore = table.Column<decimal>(type: "numeric", nullable: true),
                    ListeningShieldCount = table.Column<int>(type: "integer", nullable: true),
                    SpeakingShieldCount = table.Column<int>(type: "integer", nullable: true),
                    ReadingWritingShieldCount = table.Column<int>(type: "integer", nullable: true),
                    TotalShieldCount = table.Column<int>(type: "integer", nullable: true),
                    IsEligible = table.Column<bool>(type: "boolean", nullable: false),
                    ResultBand = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    ResultLevel = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Comment = table.Column<string>(type: "text", nullable: true),
                    AttachmentUrls = table.Column<string>(type: "text", nullable: true),
                    RecordedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    ApprovedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    ApprovedTuitionPlanId = table.Column<Guid>(type: "uuid", nullable: true),
                    GeneratedRegistrationId = table.Column<Guid>(type: "uuid", nullable: true),
                    ApprovalNote = table.Column<string>(type: "text", nullable: true),
                    ApprovedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProgramProgressionAssessments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProgramProgressionAssessments_ClassEnrollments_SourceEnroll~",
                        column: x => x.SourceEnrollmentId,
                        principalSchema: "public",
                        principalTable: "ClassEnrollments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProgramProgressionAssessments_Profiles_StudentProfileId",
                        column: x => x.StudentProfileId,
                        principalSchema: "public",
                        principalTable: "Profiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProgramProgressionAssessments_ProgramProgressionRules_RuleId",
                        column: x => x.RuleId,
                        principalSchema: "public",
                        principalTable: "ProgramProgressionRules",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProgramProgressionAssessments_ProgramProgressionSchedulePar~",
                        column: x => x.ScheduleParticipantId,
                        principalSchema: "public",
                        principalTable: "ProgramProgressionScheduleParticipants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProgramProgressionAssessments_Programs_SourceProgramId",
                        column: x => x.SourceProgramId,
                        principalSchema: "public",
                        principalTable: "Programs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProgramProgressionAssessments_Programs_TargetProgramId",
                        column: x => x.TargetProgramId,
                        principalSchema: "public",
                        principalTable: "Programs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProgramProgressionAssessments_Registrations_GeneratedRegist~",
                        column: x => x.GeneratedRegistrationId,
                        principalSchema: "public",
                        principalTable: "Registrations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProgramProgressionAssessments_Registrations_SourceRegistrat~",
                        column: x => x.SourceRegistrationId,
                        principalSchema: "public",
                        principalTable: "Registrations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProgramProgressionAssessments_TuitionPlans_ApprovedTuitionP~",
                        column: x => x.ApprovedTuitionPlanId,
                        principalSchema: "public",
                        principalTable: "TuitionPlans",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProgramProgressionAssessments_Users_ApprovedBy",
                        column: x => x.ApprovedBy,
                        principalSchema: "public",
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProgramProgressionAssessments_Users_RecordedBy",
                        column: x => x.RecordedBy,
                        principalSchema: "public",
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                schema: "public",
                table: "EmailTemplates",
                columns: new[] { "Id", "Body", "Code", "CreatedAt", "IsActive", "IsDeleted", "Placeholders", "Subject", "UpdatedAt" },
                values: new object[,]
                {
                    { new Guid("aaaaaaaa-bbbb-cccc-dddd-eeeeeeeeeeee"), "<p>Xin chào {{user_name}},</p>\r\n<p>Chúng tôi nhận được yêu cầu đặt lại mật khẩu cho tài khoản của bạn.</p>\r\n<p>Vui lòng bấm vào đường dẫn sau để đặt lại mật khẩu:</p>\r\n<p><a href=\"{{reset_link}}\">Đặt lại mật khẩu</a></p>\r\n<p>Nếu bạn không thực hiện yêu cầu này, hãy bỏ qua email.</p>\r\n<p>Trân trọng,<br/>Rex English Team</p>", "FORGOT_PASSWORD", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), true, false, "[\"user_name\",\"reset_link\"]", "Rex English - Đặt lại mật khẩu của bạn", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("b9f6c8a1-3f57-45c6-8f4b-9f0c2b7d7f10"), "<div style=\"margin:0;padding:0;background:#f4f7fb;font-family:Segoe UI,Roboto,Arial,sans-serif;color:#1f2937;\">\r\n  <table role=\"presentation\" width=\"100%\" cellspacing=\"0\" cellpadding=\"0\" style=\"background:#f4f7fb;padding:24px 12px;\">\r\n    <tr>\r\n      <td align=\"center\">\r\n        <table role=\"presentation\" width=\"640\" cellspacing=\"0\" cellpadding=\"0\" style=\"max-width:640px;background:#ffffff;border-radius:18px;overflow:hidden;box-shadow:0 10px 30px rgba(15,23,42,.08);\">\r\n          <tr>\r\n            <td style=\"padding:0;background:linear-gradient(135deg,#0ea5e9 0%,#2563eb 100%);\">\r\n              <div style=\"padding:28px 30px 24px 30px;color:#ffffff;\">\r\n                <p style=\"margin:0 0 8px 0;font-size:13px;letter-spacing:.08em;text-transform:uppercase;opacity:.9;\">KidzGo Learning Center</p>\r\n                <h1 style=\"margin:0;font-size:28px;line-height:1.3;font-weight:700;\">Hồ sơ mới đã sẵn sàng</h1>\r\n                <p style=\"margin:10px 0 0 0;font-size:15px;line-height:1.6;opacity:.95;\">\r\n                  Xin chào {{recipient_name}}, tài khoản của bạn hiện có {{profile_count}} hồ sơ đã được phê duyệt và sẵn sàng cho bước xác minh.\r\n                </p>\r\n              </div>\r\n            </td>\r\n          </tr>\r\n          <tr>\r\n            <td style=\"padding:26px 30px 12px 30px;\">\r\n              <p style=\"margin:0 0 14px 0;font-size:14px;line-height:1.7;color:#475569;\">\r\n                Vui lòng kiểm tra thông tin bên dưới. Mật khẩu đăng nhập và mã PIN phụ huynh hiện đang là mặc định, vui lòng đổi lại sau khi đăng nhập.\r\n              </p>\r\n            </td>\r\n          </tr>\r\n          <tr>\r\n            <td style=\"padding:0 30px 20px 30px;\">\r\n              <table role=\"presentation\" width=\"100%\" cellspacing=\"0\" cellpadding=\"0\" style=\"border:1px solid #dbeafe;border-radius:12px;background:#eff6ff;\">\r\n                <tr>\r\n                  <td style=\"padding:16px 18px;\">\r\n                    <p style=\"margin:0 0 10px 0;font-size:13px;color:#1d4ed8;text-transform:uppercase;letter-spacing:.04em;\">Thông tin tài khoản</p>\r\n                    <p style=\"margin:0 0 6px 0;font-size:14px;\"><strong>Email đăng nhập:</strong> {{email}}</p>\r\n                    <p style=\"margin:0 0 6px 0;font-size:14px;\"><strong>Số điện thoại:</strong> {{phone}}</p>\r\n                    <p style=\"margin:0 0 6px 0;font-size:14px;\"><strong>Mật khẩu mặc định:</strong> {{password}}</p>\r\n                    <p style=\"margin:0;font-size:14px;\"><strong>PIN phụ huynh mặc định:</strong> {{pin}}</p>\r\n                  </td>\r\n                </tr>\r\n              </table>\r\n            </td>\r\n          </tr>\r\n          <tr>\r\n            <td style=\"padding:0 30px 8px 30px;\">\r\n              <p style=\"margin:0 0 12px 0;font-size:13px;color:#64748b;\">Danh sách hồ sơ đã được duyệt</p>\r\n              {{profiles_html}}\r\n            </td>\r\n          </tr>\r\n          <tr>\r\n            <td style=\"padding:8px 30px 28px 30px;\">\r\n              <a href=\"{{verify_link}}\" style=\"display:inline-block;background:#2563eb;color:#ffffff;text-decoration:none;font-weight:600;font-size:14px;padding:12px 18px;border-radius:10px;\">Xác minh tất cả hồ sơ</a>\r\n              <p style=\"margin:14px 0 0 0;font-size:12px;line-height:1.6;color:#64748b;\">\r\n                Nút xác minh sẽ kích hoạt toàn bộ hồ sơ đã được duyệt của tài khoản này.\r\n              </p>\r\n            </td>\r\n          </tr>\r\n          <tr>\r\n            <td style=\"padding:18px 30px;background:#f8fafc;border-top:1px solid #e2e8f0;\">\r\n              <p style=\"margin:0;font-size:12px;line-height:1.7;color:#64748b;\">\r\n                Nếu bạn không thực hiện thao tác này, vui lòng bỏ qua email hoặc liên hệ bộ phận hỗ trợ của KidzGo.\r\n              </p>\r\n            </td>\r\n          </tr>\r\n        </table>\r\n      </td>\r\n    </tr>\r\n  </table>\r\n</div>", "PROFILE_CREATED", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), true, false, "[\"recipient_name\",\"profile_count\",\"profiles_html\",\"email\",\"phone\",\"password\",\"pin\",\"verify_link\",\"profile_names\"]", "KidzGo | Hồ sơ mới đã sẵn sàng xác minh", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("c1f73d87-8d4c-45c2-bf8f-3d79e2f4b6a1"), "<div style=\"margin:0;padding:0;background:#f4f7fb;font-family:Segoe UI,Roboto,Arial,sans-serif;color:#1f2937;\">\r\n  <table role=\"presentation\" width=\"100%\" cellspacing=\"0\" cellpadding=\"0\" style=\"background:#f4f7fb;padding:24px 12px;\">\r\n    <tr>\r\n      <td align=\"center\">\r\n        <table role=\"presentation\" width=\"640\" cellspacing=\"0\" cellpadding=\"0\" style=\"max-width:640px;background:#ffffff;border-radius:18px;overflow:hidden;box-shadow:0 10px 30px rgba(15,23,42,.08);\">\r\n          <tr>\r\n            <td style=\"padding:0;background:linear-gradient(135deg,#f97316 0%,#ea580c 100%);\">\r\n              <div style=\"padding:28px 30px 24px 30px;color:#ffffff;\">\r\n                <p style=\"margin:0 0 8px 0;font-size:13px;letter-spacing:.08em;text-transform:uppercase;opacity:.9;\">KidzGo Learning Center</p>\r\n                <h1 style=\"margin:0;font-size:28px;line-height:1.3;font-weight:700;\">Đặt lại PIN phụ huynh</h1>\r\n                <p style=\"margin:10px 0 0 0;font-size:15px;line-height:1.6;opacity:.95;\">\r\n                  Xin chào {{user_name}}, chúng tôi đã nhận được yêu cầu đặt lại PIN cho hồ sơ {{profile_name}}.\r\n                </p>\r\n              </div>\r\n            </td>\r\n          </tr>\r\n          <tr>\r\n            <td style=\"padding:26px 30px 12px 30px;\">\r\n              <p style=\"margin:0 0 14px 0;font-size:14px;line-height:1.7;color:#475569;\">\r\n                Để tiếp tục, vui lòng bấm vào nút bên dưới. Liên kết này chỉ có hiệu lực trong 1 giờ.\r\n              </p>\r\n            </td>\r\n          </tr>\r\n          <tr>\r\n            <td style=\"padding:8px 30px 28px 30px;\">\r\n              <a href=\"{{reset_link}}\" style=\"display:inline-block;background:#ea580c;color:#ffffff;text-decoration:none;font-weight:600;font-size:14px;padding:12px 18px;border-radius:10px;\">Đặt lại PIN</a>\r\n              <p style=\"margin:14px 0 0 0;font-size:12px;line-height:1.6;color:#64748b;\">\r\n                Nếu bạn không thực hiện yêu cầu này, vui lòng bỏ qua email hoặc liên hệ bộ phận hỗ trợ của KidzGo.\r\n              </p>\r\n            </td>\r\n          </tr>\r\n        </table>\r\n      </td>\r\n    </tr>\r\n  </table>\r\n</div>", "PARENT_PIN_RESET", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), true, false, "[\"profile_name\",\"user_name\",\"reset_link\"]", "KidzGo | Yêu cầu đặt lại PIN phụ huynh", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) }
                });

            migrationBuilder.InsertData(
                schema: "public",
                table: "NotificationTemplates",
                columns: new[] { "Id", "Category", "Channel", "Code", "Content", "CreatedAt", "IsActive", "IsDeleted", "Placeholders", "Title", "UpdatedAt" },
                values: new object[,]
                {
                    { new Guid("11111111-1111-1111-1111-111111111111"), null, "Email", "SESSION_REMINDER", "<p>Xin chào,</p>\r\n<p>Đây là thông báo nhắc nhở về buổi học sắp tới của học sinh <strong>{{student_name}}</strong>:</p>\r\n<ul>\r\n    <li><strong>Học sinh:</strong> {{student_name}}</li>\r\n    <li><strong>Lớp học:</strong> {{class_name}}</li>\r\n    <li><strong>Thời gian:</strong> {{session_start_time}}</li>\r\n    <li><strong>Địa điểm:</strong> Rex English Center</li>\r\n    <li><strong>Phòng học:</strong> {{classroom_name}}</li>\r\n</ul>\r\n<p>Vui lòng có mặt đúng giờ để không bỏ lỡ buổi học.</p>\r\n<p>Trân trọng,<br/>Rex English Team</p>", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), true, false, "[\"session_title\",\"session_start_time\",\"class_name\",\"location\",\"student_name\",\"classroom_name\"]", "Nhắc nhở: Buổi học {{session_title}} sắp bắt đầu", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("22222222-2222-2222-2222-222222222222"), null, "Email", "HOMEWORK_REMINDER", "<p>Xin chào,</p>\r\n<p>Học sinh <strong>{{student_name}}</strong> có bài tập sắp đến hạn:</p>\r\n<ul>\r\n    <li><strong>Học sinh:</strong> {{student_name}}</li>\r\n    <li><strong>Tên bài tập:</strong> {{homework_title}}</li>\r\n    <li><strong>Lớp học:</strong> {{class_name}}</li>\r\n    <li><strong>Hạn nộp:</strong> {{due_date}}</li>\r\n</ul>\r\n<p>Vui lòng hoàn thành và nộp bài tập trước thời hạn.</p>\r\n<p>Trân trọng,<br/>Rex English Team</p>", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), true, false, "[\"homework_title\",\"due_date\",\"class_name\",\"student_name\"]", "Nhắc nhở: Bài tập {{homework_title}} sắp đến hạn", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("33333333-3333-3333-3333-333333333333"), null, "Email", "TUITION_REMINDER", "<p>Xin chào,</p>\r\n<p>Đây là thông báo nhắc nhở về học phí sắp đến hạn của học sinh <strong>{{student_name}}</strong>:</p>\r\n<ul>\r\n    <li><strong>Học sinh:</strong> {{student_name}}</li>\r\n    <li><strong>Số tiền:</strong> {{amount}} VNĐ</li>\r\n    <li><strong>Hạn thanh toán:</strong> {{due_date}}</li>\r\n</ul>\r\n<p>Vui lòng thanh toán học phí trước thời hạn để đảm bảo việc học không bị gián đoạn.</p>\r\n<p>Trân trọng,<br/>Rex English Team</p>", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), true, false, "[\"student_name\",\"amount\",\"due_date\"]", "Nhắc nhở: Học phí của {{student_name}} sắp đến hạn", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("44444444-4444-4444-4444-444444444444"), null, "Email", "MAKEUP_REMINDER", "<p>Xin chào,</p>\r\n<p>Đây là thông báo nhắc nhở về buổi bù sắp tới của học sinh <strong>{{student_name}}</strong>:</p>\r\n<ul>\r\n    <li><strong>Học sinh:</strong> {{student_name}}</li>\r\n    <li><strong>Lớp học:</strong> {{class_name}}</li>\r\n    <li><strong>Thời gian:</strong> {{session_start_time}}</li>\r\n    <li><strong>Địa điểm:</strong> Rex English Center</li>\r\n    <li><strong>Phòng học:</strong> {{classroom_name}}</li>\r\n</ul>\r\n<p>Vui lòng có mặt đúng giờ để tham gia buổi bù.</p>\r\n<p>Trân trọng,<br/>Rex English Team</p>", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), true, false, "[\"session_title\",\"session_start_time\",\"class_name\",\"location\",\"student_name\",\"classroom_name\"]", "Nhắc nhở: Buổi bù {{session_title}} sắp bắt đầu", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("55555555-5555-5555-5555-555555555555"), null, "Email", "MISSION_REMINDER", "<p>Xin chào,</p>\r\n<p>Học sinh <strong>{{student_name}}</strong> có nhiệm vụ sắp đến hạn:</p>\r\n<ul>\r\n    <li><strong>Học sinh:</strong> {{student_name}}</li>\r\n    <li><strong>Tên nhiệm vụ:</strong> {{mission_title}}</li>\r\n    <li><strong>Lớp học:</strong> {{class_name}}</li>\r\n    <li><strong>Hạn hoàn thành:</strong> {{due_date}}</li>\r\n</ul>\r\n<p>Vui lòng hoàn thành nhiệm vụ trước thời hạn để nhận phần thưởng.</p>\r\n<p>Trân trọng,<br/>Rex English Team</p>", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), true, false, "[\"mission_title\",\"due_date\",\"class_name\",\"student_name\"]", "Nhắc nhở: Nhiệm vụ {{mission_title}} sắp kết thúc", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("66666666-6666-6666-6666-666666666666"), null, "Email", "MEDIA_REMINDER", "<p>Xin chào,</p>\r\n<p>Lớp học của học sinh <strong>{{student_name}}</strong> vừa có nội dung mới:</p>\r\n<ul>\r\n    <li><strong>Học sinh:</strong> {{student_name}}</li>\r\n    <li><strong>Tiêu đề:</strong> {{media_title}}</li>\r\n    <li><strong>Loại:</strong> {{media_type}}</li>\r\n    <li><strong>Lớp học:</strong> {{class_name}}</li>\r\n</ul>\r\n<p>Hãy đăng nhập vào ứng dụng để xem nội dung mới này.</p>\r\n<p>Trân trọng,<br/>Rex English Team</p>", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), true, false, "[\"media_title\",\"media_type\",\"class_name\",\"student_name\"]", "Thông báo: Có {{media_type}} mới từ lớp {{class_name}}", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("77777777-7777-7777-7777-777777777777"), null, "Email", "PAUSE_ENROLLMENT_APPROVED_EMAIL", "<div style=\"font-family: Arial, sans-serif; color:#222; line-height:1.6;\">\r\n  <h2 style=\"color:#2b6cb0;\">Yêu cầu bảo lưu đã được duyệt</h2>\r\n  <p>Xin chào,</p>\r\n  <p>Yêu cầu bảo lưu của <strong>{{student_name}}</strong> đã được duyệt.</p>\r\n  <div style=\"background:#f7fafc;border:1px solid #e2e8f0;border-radius:8px;padding:12px;\">\r\n    <p><strong>Thời gian bảo lưu:</strong> {{pause_from}} - {{pause_to}}</p>\r\n  </div>\r\n  <p>Vui lòng theo dõi lịch học sau thời gian bảo lưu.</p>\r\n  <p>Trân trọng,<br/>KidzGo Team</p>\r\n</div>", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), true, false, "[\"student_name\",\"pause_from\",\"pause_to\"]", "Yêu cầu bảo lưu đã được duyệt", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("88888888-8888-8888-8888-888888888888"), null, "Email", "PAUSE_ENROLLMENT_REJECTED_EMAIL", "<div style=\"font-family: Arial, sans-serif; color:#222; line-height:1.6;\">\r\n  <h2 style=\"color:#c53030;\">Yêu cầu bảo lưu bị từ chối</h2>\r\n  <p>Xin chào,</p>\r\n  <p>Yêu cầu bảo lưu của <strong>{{student_name}}</strong> đã bị từ chối.</p>\r\n  <div style=\"background:#fff5f5;border:1px solid #fed7d7;border-radius:8px;padding:12px;\">\r\n    <p><strong>Thời gian bảo lưu:</strong> {{pause_from}} - {{pause_to}}</p>\r\n  </div>\r\n  <p>Vui lòng liên hệ trung tâm nếu cần hỗ trợ thêm.</p>\r\n  <p>Trân trọng,<br/>KidzGo Team</p>\r\n</div>", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), true, false, "[\"student_name\",\"pause_from\",\"pause_to\"]", "Yêu cầu bảo lưu bị từ chối", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("99999999-9999-9999-9999-999999999999"), null, "Email", "PAUSE_ENROLLMENT_OUTCOME_EMAIL", "<div style=\"font-family: Arial, sans-serif; color:#222; line-height:1.6;\">\r\n  <h2 style=\"color:#2f855a;\">Kết quả bảo lưu đã được cập nhật</h2>\r\n  <p>Xin chào,</p>\r\n  <p>Kết quả bảo lưu của <strong>{{student_name}}</strong> đã được cập nhật.</p>\r\n  <div style=\"background:#f0fff4;border:1px solid #c6f6d5;border-radius:8px;padding:12px;\">\r\n    <p><strong>Kết quả:</strong> {{outcome}}</p>\r\n    <p><strong>Ghi chú:</strong> {{outcome_note}}</p>\r\n  </div>\r\n  <p>Trân trọng,<br/>KidzGo Team</p>\r\n</div>", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), true, false, "[\"student_name\",\"outcome\",\"outcome_note\"]", "Kết quả bảo lưu đã được cập nhật", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"), null, "Push", "PAUSE_ENROLLMENT_APPROVED_PUSH", "Yêu cầu bảo lưu {{pause_from}} - {{pause_to}} đã được duyệt.", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), true, false, "[\"pause_from\",\"pause_to\"]", "Bảo lưu đã được duyệt", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"), null, "Push", "PAUSE_ENROLLMENT_REJECTED_PUSH", "Yêu cầu bảo lưu {{pause_from}} - {{pause_to}} đã bị từ chối.", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), true, false, "[\"pause_from\",\"pause_to\"]", "Bảo lưu bị từ chối", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("cccccccc-cccc-cccc-cccc-cccccccccccc"), null, "Push", "PAUSE_ENROLLMENT_OUTCOME_PUSH", "Kết quả bảo lưu: {{outcome}}.", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), true, false, "[\"outcome\"]", "Kết quả bảo lưu đã cập nhật", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("dddddddd-dddd-dddd-dddd-dddddddddddd"), null, "ZaloOa", "PAUSE_ENROLLMENT_APPROVED_ZALO", "Yêu cầu bảo lưu của {{student_name}} từ {{pause_from}} đến {{pause_to}} đã được duyệt.", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), true, false, "[\"student_name\",\"pause_from\",\"pause_to\"]", "Bảo lưu đã được duyệt", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("eeeeeeee-eeee-eeee-eeee-eeeeeeeeeeee"), null, "ZaloOa", "PAUSE_ENROLLMENT_REJECTED_ZALO", "Yêu cầu bảo lưu của {{student_name}} từ {{pause_from}} đến {{pause_to}} đã bị từ chối.", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), true, false, "[\"student_name\",\"pause_from\",\"pause_to\"]", "Bảo lưu bị từ chối", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("ffffffff-ffff-ffff-ffff-ffffffffffff"), null, "ZaloOa", "PAUSE_ENROLLMENT_OUTCOME_ZALO", "Kết quả bảo lưu của {{student_name}}: {{outcome}}. {{outcome_note}}", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), true, false, "[\"student_name\",\"outcome\",\"outcome_note\"]", "Kết quả bảo lưu đã cập nhật", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) }
                });

            migrationBuilder.CreateIndex(
                name: "attendance_unique",
                schema: "public",
                table: "Attendances",
                columns: new[] { "SessionId", "StudentProfileId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Attendances_MarkedBy",
                schema: "public",
                table: "Attendances",
                column: "MarkedBy");

            migrationBuilder.CreateIndex(
                name: "IX_Attendances_StudentProfileId",
                schema: "public",
                table: "Attendances",
                column: "StudentProfileId");

            migrationBuilder.CreateIndex(
                name: "attendance_streak_unique",
                schema: "public",
                table: "AttendanceStreaks",
                columns: new[] { "StudentProfileId", "AttendanceDate" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AuditLogs_ActorProfileId",
                schema: "public",
                table: "AuditLogs",
                column: "ActorProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_AuditLogs_ActorUserId",
                schema: "public",
                table: "AuditLogs",
                column: "ActorUserId");

            migrationBuilder.CreateIndex(
                name: "blog_published_idx",
                schema: "public",
                table: "Blogs",
                columns: new[] { "IsPublished", "PublishedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_Blogs_CreatedBy",
                schema: "public",
                table: "Blogs",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_Branches_Code",
                schema: "public",
                table: "Branches",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_BranchPrograms_BranchId_ProgramId",
                schema: "public",
                table: "BranchPrograms",
                columns: new[] { "BranchId", "ProgramId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_BranchPrograms_DefaultMakeupClassId",
                schema: "public",
                table: "BranchPrograms",
                column: "DefaultMakeupClassId");

            migrationBuilder.CreateIndex(
                name: "IX_BranchPrograms_ProgramId",
                schema: "public",
                table: "BranchPrograms",
                column: "ProgramId");

            migrationBuilder.CreateIndex(
                name: "IX_CashbookEntries_BranchId",
                schema: "public",
                table: "CashbookEntries",
                column: "BranchId");

            migrationBuilder.CreateIndex(
                name: "IX_CashbookEntries_CreatedBy",
                schema: "public",
                table: "CashbookEntries",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_ClassEnrollments_ClassId",
                schema: "public",
                table: "ClassEnrollments",
                column: "ClassId");

            migrationBuilder.CreateIndex(
                name: "IX_ClassEnrollments_RegistrationId",
                schema: "public",
                table: "ClassEnrollments",
                column: "RegistrationId");

            migrationBuilder.CreateIndex(
                name: "IX_ClassEnrollments_StudentProfileId",
                schema: "public",
                table: "ClassEnrollments",
                column: "StudentProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_ClassEnrollments_TuitionPlanId",
                schema: "public",
                table: "ClassEnrollments",
                column: "TuitionPlanId");

            migrationBuilder.CreateIndex(
                name: "IX_ClassEnrollmentScheduleSegments_ClassEnrollmentId_Effective~",
                schema: "public",
                table: "ClassEnrollmentScheduleSegments",
                columns: new[] { "ClassEnrollmentId", "EffectiveFrom" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Classes_AssistantTeacherId",
                schema: "public",
                table: "Classes",
                column: "AssistantTeacherId");

            migrationBuilder.CreateIndex(
                name: "IX_Classes_BranchId",
                schema: "public",
                table: "Classes",
                column: "BranchId");

            migrationBuilder.CreateIndex(
                name: "IX_Classes_Code",
                schema: "public",
                table: "Classes",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Classes_MainTeacherId",
                schema: "public",
                table: "Classes",
                column: "MainTeacherId");

            migrationBuilder.CreateIndex(
                name: "IX_Classes_ProgramId",
                schema: "public",
                table: "Classes",
                column: "ProgramId");

            migrationBuilder.CreateIndex(
                name: "IX_Classes_RoomId",
                schema: "public",
                table: "Classes",
                column: "RoomId");

            migrationBuilder.CreateIndex(
                name: "IX_Classrooms_BranchId",
                schema: "public",
                table: "Classrooms",
                column: "BranchId");

            migrationBuilder.CreateIndex(
                name: "IX_ClassScheduleSegments_ClassId_EffectiveFrom",
                schema: "public",
                table: "ClassScheduleSegments",
                columns: new[] { "ClassId", "EffectiveFrom" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Contracts_BranchId",
                schema: "public",
                table: "Contracts",
                column: "BranchId");

            migrationBuilder.CreateIndex(
                name: "IX_Contracts_StaffUserId",
                schema: "public",
                table: "Contracts",
                column: "StaffUserId");

            migrationBuilder.CreateIndex(
                name: "IX_DeviceTokens_UserId",
                schema: "public",
                table: "DeviceTokens",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_DeviceTokens_UserId_DeviceId_IsActive",
                schema: "public",
                table: "DeviceTokens",
                columns: new[] { "UserId", "DeviceId", "IsActive" },
                unique: true,
                filter: "\"IsActive\" = true");

            migrationBuilder.CreateIndex(
                name: "IX_DeviceTokens_UserId_IsActive",
                schema: "public",
                table: "DeviceTokens",
                columns: new[] { "UserId", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_EmailTemplates_Code",
                schema: "public",
                table: "EmailTemplates",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_EnrollmentConfirmationPaymentSettings_BranchId",
                schema: "public",
                table: "EnrollmentConfirmationPaymentSettings",
                column: "BranchId");

            migrationBuilder.CreateIndex(
                name: "IX_EnrollmentConfirmationPaymentSettings_ScopeKey",
                schema: "public",
                table: "EnrollmentConfirmationPaymentSettings",
                column: "ScopeKey",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_EnrollmentConfirmationPdfs_EnrollmentId",
                schema: "public",
                table: "EnrollmentConfirmationPdfs",
                column: "EnrollmentId");

            migrationBuilder.CreateIndex(
                name: "IX_EnrollmentConfirmationPdfs_EnrollmentId_Track_FormType_IsAc~",
                schema: "public",
                table: "EnrollmentConfirmationPdfs",
                columns: new[] { "EnrollmentId", "Track", "FormType", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_EnrollmentConfirmationPdfs_GeneratedAt",
                schema: "public",
                table: "EnrollmentConfirmationPdfs",
                column: "GeneratedAt");

            migrationBuilder.CreateIndex(
                name: "IX_EnrollmentConfirmationPdfs_RegistrationId",
                schema: "public",
                table: "EnrollmentConfirmationPdfs",
                column: "RegistrationId");

            migrationBuilder.CreateIndex(
                name: "IX_ExamQuestions_ExamId",
                schema: "public",
                table: "ExamQuestions",
                column: "ExamId");

            migrationBuilder.CreateIndex(
                name: "IX_ExamResults_ExamId",
                schema: "public",
                table: "ExamResults",
                column: "ExamId");

            migrationBuilder.CreateIndex(
                name: "IX_ExamResults_GradedBy",
                schema: "public",
                table: "ExamResults",
                column: "GradedBy");

            migrationBuilder.CreateIndex(
                name: "IX_ExamResults_StudentProfileId",
                schema: "public",
                table: "ExamResults",
                column: "StudentProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_Exams_ClassId",
                schema: "public",
                table: "Exams",
                column: "ClassId");

            migrationBuilder.CreateIndex(
                name: "IX_Exams_CreatedBy",
                schema: "public",
                table: "Exams",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_ExamSubmissionAnswers_QuestionId",
                schema: "public",
                table: "ExamSubmissionAnswers",
                column: "QuestionId");

            migrationBuilder.CreateIndex(
                name: "IX_ExamSubmissionAnswers_SubmissionId",
                schema: "public",
                table: "ExamSubmissionAnswers",
                column: "SubmissionId");

            migrationBuilder.CreateIndex(
                name: "IX_ExamSubmissions_ExamId",
                schema: "public",
                table: "ExamSubmissions",
                column: "ExamId");

            migrationBuilder.CreateIndex(
                name: "IX_ExamSubmissions_GradedBy",
                schema: "public",
                table: "ExamSubmissions",
                column: "GradedBy");

            migrationBuilder.CreateIndex(
                name: "IX_ExamSubmissions_StudentProfileId",
                schema: "public",
                table: "ExamSubmissions",
                column: "StudentProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_ExtracurricularPrograms_BranchId",
                schema: "public",
                table: "ExtracurricularPrograms",
                column: "BranchId");

            migrationBuilder.CreateIndex(
                name: "IX_FaqCategories_IsDeleted_IsActive_SortOrder",
                schema: "public",
                table: "FaqCategories",
                columns: new[] { "IsDeleted", "IsActive", "SortOrder" });

            migrationBuilder.CreateIndex(
                name: "IX_FaqItems_CategoryId_SortOrder",
                schema: "public",
                table: "FaqItems",
                columns: new[] { "CategoryId", "SortOrder" });

            migrationBuilder.CreateIndex(
                name: "IX_FaqItems_IsPublished_IsDeleted",
                schema: "public",
                table: "FaqItems",
                columns: new[] { "IsPublished", "IsDeleted" });

            migrationBuilder.CreateIndex(
                name: "IX_Holidays_StartDate_EndDate",
                schema: "public",
                table: "Holidays",
                columns: new[] { "StartDate", "EndDate" });

            migrationBuilder.CreateIndex(
                name: "IX_HomeworkAssignments_ClassId",
                schema: "public",
                table: "HomeworkAssignments",
                column: "ClassId");

            migrationBuilder.CreateIndex(
                name: "IX_HomeworkAssignments_CreatedBy",
                schema: "public",
                table: "HomeworkAssignments",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_HomeworkAssignments_MissionId",
                schema: "public",
                table: "HomeworkAssignments",
                column: "MissionId");

            migrationBuilder.CreateIndex(
                name: "IX_HomeworkAssignments_SessionId",
                schema: "public",
                table: "HomeworkAssignments",
                column: "SessionId");

            migrationBuilder.CreateIndex(
                name: "IX_HomeworkQuestions_HomeworkAssignmentId",
                schema: "public",
                table: "HomeworkQuestions",
                column: "HomeworkAssignmentId");

            migrationBuilder.CreateIndex(
                name: "homework_student_unique",
                schema: "public",
                table: "HomeworkStudents",
                columns: new[] { "AssignmentId", "StudentProfileId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_HomeworkStudents_StudentProfileId",
                schema: "public",
                table: "HomeworkStudents",
                column: "StudentProfileId");

            migrationBuilder.CreateIndex(
                name: "homework_submission_attempt_unique",
                schema: "public",
                table: "HomeworkSubmissionAttempts",
                columns: new[] { "HomeworkStudentId", "AttemptNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_InvoiceLines_InvoiceId",
                schema: "public",
                table: "InvoiceLines",
                column: "InvoiceId");

            migrationBuilder.CreateIndex(
                name: "IX_Invoices_BranchId",
                schema: "public",
                table: "Invoices",
                column: "BranchId");

            migrationBuilder.CreateIndex(
                name: "IX_Invoices_ClassId",
                schema: "public",
                table: "Invoices",
                column: "ClassId");

            migrationBuilder.CreateIndex(
                name: "IX_Invoices_IssuedBy",
                schema: "public",
                table: "Invoices",
                column: "IssuedBy");

            migrationBuilder.CreateIndex(
                name: "IX_Invoices_StudentProfileId",
                schema: "public",
                table: "Invoices",
                column: "StudentProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_LeadActivities_CreatedBy",
                schema: "public",
                table: "LeadActivities",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_LeadActivities_LeadId",
                schema: "public",
                table: "LeadActivities",
                column: "LeadId");

            migrationBuilder.CreateIndex(
                name: "IX_LeadChildren_ConvertedStudentProfileId",
                schema: "public",
                table: "LeadChildren",
                column: "ConvertedStudentProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_LeadChildren_LeadId",
                schema: "public",
                table: "LeadChildren",
                column: "LeadId");

            migrationBuilder.CreateIndex(
                name: "IX_Leads_BranchPreference",
                schema: "public",
                table: "Leads",
                column: "BranchPreference");

            migrationBuilder.CreateIndex(
                name: "IX_Leads_OwnerStaffId",
                schema: "public",
                table: "Leads",
                column: "OwnerStaffId");

            migrationBuilder.CreateIndex(
                name: "IX_LeaveRequests_ApprovedBy",
                schema: "public",
                table: "LeaveRequests",
                column: "ApprovedBy");

            migrationBuilder.CreateIndex(
                name: "IX_LeaveRequests_ClassId",
                schema: "public",
                table: "LeaveRequests",
                column: "ClassId");

            migrationBuilder.CreateIndex(
                name: "IX_LeaveRequests_SessionId",
                schema: "public",
                table: "LeaveRequests",
                column: "SessionId");

            migrationBuilder.CreateIndex(
                name: "IX_LeaveRequests_StudentProfileId",
                schema: "public",
                table: "LeaveRequests",
                column: "StudentProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_LessonPlans_ClassId",
                schema: "public",
                table: "LessonPlans",
                column: "ClassId");

            migrationBuilder.CreateIndex(
                name: "IX_LessonPlans_SubmittedBy",
                schema: "public",
                table: "LessonPlans",
                column: "SubmittedBy");

            migrationBuilder.CreateIndex(
                name: "IX_LessonPlans_TemplateId",
                schema: "public",
                table: "LessonPlans",
                column: "TemplateId");

            migrationBuilder.CreateIndex(
                name: "session_unique",
                schema: "public",
                table: "LessonPlans",
                column: "SessionId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_LessonPlanTemplates_CreatedBy",
                schema: "public",
                table: "LessonPlanTemplates",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_LessonPlanTemplates_ProgramId",
                schema: "public",
                table: "LessonPlanTemplates",
                column: "ProgramId");

            migrationBuilder.CreateIndex(
                name: "IX_MakeupAllocations_AssignedBy",
                schema: "public",
                table: "MakeupAllocations",
                column: "AssignedBy");

            migrationBuilder.CreateIndex(
                name: "IX_MakeupAllocations_MakeupCreditId",
                schema: "public",
                table: "MakeupAllocations",
                column: "MakeupCreditId");

            migrationBuilder.CreateIndex(
                name: "IX_MakeupAllocations_TargetSessionId",
                schema: "public",
                table: "MakeupAllocations",
                column: "TargetSessionId");

            migrationBuilder.CreateIndex(
                name: "IX_MakeupCredits_SourceSessionId",
                schema: "public",
                table: "MakeupCredits",
                column: "SourceSessionId");

            migrationBuilder.CreateIndex(
                name: "IX_MakeupCredits_StudentProfileId",
                schema: "public",
                table: "MakeupCredits",
                column: "StudentProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_MakeupCredits_UsedSessionId",
                schema: "public",
                table: "MakeupCredits",
                column: "UsedSessionId");

            migrationBuilder.CreateIndex(
                name: "IX_MediaAssets_ApprovedById",
                schema: "public",
                table: "MediaAssets",
                column: "ApprovedById");

            migrationBuilder.CreateIndex(
                name: "IX_MediaAssets_BranchId",
                schema: "public",
                table: "MediaAssets",
                column: "BranchId");

            migrationBuilder.CreateIndex(
                name: "IX_MediaAssets_ClassId",
                schema: "public",
                table: "MediaAssets",
                column: "ClassId");

            migrationBuilder.CreateIndex(
                name: "IX_MediaAssets_StudentProfileId",
                schema: "public",
                table: "MediaAssets",
                column: "StudentProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_MediaAssets_UploaderId",
                schema: "public",
                table: "MediaAssets",
                column: "UploaderId");

            migrationBuilder.CreateIndex(
                name: "IX_MissionProgresses_StudentProfileId",
                schema: "public",
                table: "MissionProgresses",
                column: "StudentProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_MissionProgresses_VerifiedBy",
                schema: "public",
                table: "MissionProgresses",
                column: "VerifiedBy");

            migrationBuilder.CreateIndex(
                name: "mission_progress_unique",
                schema: "public",
                table: "MissionProgresses",
                columns: new[] { "MissionId", "StudentProfileId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MissionRewardRules_MissionType_ProgressMode_TotalRequired",
                schema: "public",
                table: "MissionRewardRules",
                columns: new[] { "MissionType", "ProgressMode", "TotalRequired" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Missions_CreatedBy",
                schema: "public",
                table: "Missions",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_Missions_TargetClassId",
                schema: "public",
                table: "Missions",
                column: "TargetClassId");

            migrationBuilder.CreateIndex(
                name: "IX_MonthlyReportData_ReportId",
                schema: "public",
                table: "MonthlyReportData",
                column: "ReportId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MonthlyReportData_StudentProfileId",
                schema: "public",
                table: "MonthlyReportData",
                column: "StudentProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_MonthlyReportJobs_BranchId",
                schema: "public",
                table: "MonthlyReportJobs",
                column: "BranchId");

            migrationBuilder.CreateIndex(
                name: "IX_MonthlyReportJobs_CreatedBy",
                schema: "public",
                table: "MonthlyReportJobs",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_MonthlyWorkHours_ContractId",
                schema: "public",
                table: "MonthlyWorkHours",
                column: "ContractId");

            migrationBuilder.CreateIndex(
                name: "monthly_work_hours_payroll_idx",
                schema: "public",
                table: "MonthlyWorkHours",
                columns: new[] { "BranchId", "Year", "Month", "IsLocked" });

            migrationBuilder.CreateIndex(
                name: "monthly_work_hours_unique",
                schema: "public",
                table: "MonthlyWorkHours",
                columns: new[] { "StaffUserId", "ContractId", "Year", "Month" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_NotificationTemplateId",
                schema: "public",
                table: "Notifications",
                column: "NotificationTemplateId");

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_RecipientProfileId",
                schema: "public",
                table: "Notifications",
                column: "RecipientProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_RecipientUserId",
                schema: "public",
                table: "Notifications",
                column: "RecipientUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_ScopeBranchId",
                schema: "public",
                table: "Notifications",
                column: "ScopeBranchId");

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_ScopeClassId",
                schema: "public",
                table: "Notifications",
                column: "ScopeClassId");

            migrationBuilder.CreateIndex(
                name: "IX_NotificationTemplates_Code",
                schema: "public",
                table: "NotificationTemplates",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ParentPinResetTokens_ProfileId",
                schema: "public",
                table: "ParentPinResetTokens",
                column: "ProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_ParentStudentLinks_ParentProfileId",
                schema: "public",
                table: "ParentStudentLinks",
                column: "ParentProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_ParentStudentLinks_StudentProfileId",
                schema: "public",
                table: "ParentStudentLinks",
                column: "StudentProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_PasswordResetTokens_UserId",
                schema: "public",
                table: "PasswordResetTokens",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_PauseEnrollmentRequestHistories_ChangedBy",
                schema: "public",
                table: "PauseEnrollmentRequestHistories",
                column: "ChangedBy");

            migrationBuilder.CreateIndex(
                name: "IX_PauseEnrollmentRequestHistories_ClassId",
                schema: "public",
                table: "PauseEnrollmentRequestHistories",
                column: "ClassId");

            migrationBuilder.CreateIndex(
                name: "IX_PauseEnrollmentRequestHistories_EnrollmentId",
                schema: "public",
                table: "PauseEnrollmentRequestHistories",
                column: "EnrollmentId");

            migrationBuilder.CreateIndex(
                name: "IX_PauseEnrollmentRequestHistories_PauseEnrollmentRequestId",
                schema: "public",
                table: "PauseEnrollmentRequestHistories",
                column: "PauseEnrollmentRequestId");

            migrationBuilder.CreateIndex(
                name: "IX_PauseEnrollmentRequestHistories_StudentProfileId",
                schema: "public",
                table: "PauseEnrollmentRequestHistories",
                column: "StudentProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_PauseEnrollmentRequests_ApprovedBy",
                schema: "public",
                table: "PauseEnrollmentRequests",
                column: "ApprovedBy");

            migrationBuilder.CreateIndex(
                name: "IX_PauseEnrollmentRequests_CancelledBy",
                schema: "public",
                table: "PauseEnrollmentRequests",
                column: "CancelledBy");

            migrationBuilder.CreateIndex(
                name: "IX_PauseEnrollmentRequests_ClassId",
                schema: "public",
                table: "PauseEnrollmentRequests",
                column: "ClassId");

            migrationBuilder.CreateIndex(
                name: "IX_PauseEnrollmentRequests_OutcomeBy",
                schema: "public",
                table: "PauseEnrollmentRequests",
                column: "OutcomeBy");

            migrationBuilder.CreateIndex(
                name: "IX_PauseEnrollmentRequests_OutcomeCompletedBy",
                schema: "public",
                table: "PauseEnrollmentRequests",
                column: "OutcomeCompletedBy");

            migrationBuilder.CreateIndex(
                name: "IX_PauseEnrollmentRequests_ReassignedClassId",
                schema: "public",
                table: "PauseEnrollmentRequests",
                column: "ReassignedClassId");

            migrationBuilder.CreateIndex(
                name: "IX_PauseEnrollmentRequests_ReassignedEnrollmentId",
                schema: "public",
                table: "PauseEnrollmentRequests",
                column: "ReassignedEnrollmentId");

            migrationBuilder.CreateIndex(
                name: "IX_PauseEnrollmentRequests_StudentProfileId",
                schema: "public",
                table: "PauseEnrollmentRequests",
                column: "StudentProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_Payments_ConfirmedBy",
                schema: "public",
                table: "Payments",
                column: "ConfirmedBy");

            migrationBuilder.CreateIndex(
                name: "IX_Payments_InvoiceId",
                schema: "public",
                table: "Payments",
                column: "InvoiceId");

            migrationBuilder.CreateIndex(
                name: "IX_PayrollLines_PayrollRunId",
                schema: "public",
                table: "PayrollLines",
                column: "PayrollRunId");

            migrationBuilder.CreateIndex(
                name: "IX_PayrollLines_StaffUserId",
                schema: "public",
                table: "PayrollLines",
                column: "StaffUserId");

            migrationBuilder.CreateIndex(
                name: "IX_PayrollPayments_CashbookEntryId",
                schema: "public",
                table: "PayrollPayments",
                column: "CashbookEntryId");

            migrationBuilder.CreateIndex(
                name: "IX_PayrollPayments_PayrollRunId",
                schema: "public",
                table: "PayrollPayments",
                column: "PayrollRunId");

            migrationBuilder.CreateIndex(
                name: "IX_PayrollPayments_StaffUserId",
                schema: "public",
                table: "PayrollPayments",
                column: "StaffUserId");

            migrationBuilder.CreateIndex(
                name: "IX_PayrollRuns_ApprovedBy",
                schema: "public",
                table: "PayrollRuns",
                column: "ApprovedBy");

            migrationBuilder.CreateIndex(
                name: "IX_PayrollRuns_BranchId",
                schema: "public",
                table: "PayrollRuns",
                column: "BranchId");

            migrationBuilder.CreateIndex(
                name: "IX_PlacementTests_ClassId",
                schema: "public",
                table: "PlacementTests",
                column: "ClassId");

            migrationBuilder.CreateIndex(
                name: "IX_PlacementTests_InvigilatorUserId",
                schema: "public",
                table: "PlacementTests",
                column: "InvigilatorUserId");

            migrationBuilder.CreateIndex(
                name: "IX_PlacementTests_LeadChildId",
                schema: "public",
                table: "PlacementTests",
                column: "LeadChildId");

            migrationBuilder.CreateIndex(
                name: "IX_PlacementTests_LeadId",
                schema: "public",
                table: "PlacementTests",
                column: "LeadId");

            migrationBuilder.CreateIndex(
                name: "IX_PlacementTests_OriginalPlacementTestId",
                schema: "public",
                table: "PlacementTests",
                column: "OriginalPlacementTestId");

            migrationBuilder.CreateIndex(
                name: "IX_PlacementTests_ProgramRecommendationId",
                schema: "public",
                table: "PlacementTests",
                column: "ProgramRecommendationId");

            migrationBuilder.CreateIndex(
                name: "IX_PlacementTests_RoomId",
                schema: "public",
                table: "PlacementTests",
                column: "RoomId");

            migrationBuilder.CreateIndex(
                name: "IX_PlacementTests_SecondaryProgramRecommendationId",
                schema: "public",
                table: "PlacementTests",
                column: "SecondaryProgramRecommendationId");

            migrationBuilder.CreateIndex(
                name: "IX_PlacementTests_StudentProfileId",
                schema: "public",
                table: "PlacementTests",
                column: "StudentProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_Profiles_UserId",
                schema: "public",
                table: "Profiles",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_ProgramLeavePolicies_ProgramId",
                schema: "public",
                table: "ProgramLeavePolicies",
                column: "ProgramId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProgramLeavePolicies_UpdatedBy",
                schema: "public",
                table: "ProgramLeavePolicies",
                column: "UpdatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_ProgramProgressionAssessments_ApprovedBy",
                schema: "public",
                table: "ProgramProgressionAssessments",
                column: "ApprovedBy");

            migrationBuilder.CreateIndex(
                name: "IX_ProgramProgressionAssessments_ApprovedTuitionPlanId",
                schema: "public",
                table: "ProgramProgressionAssessments",
                column: "ApprovedTuitionPlanId");

            migrationBuilder.CreateIndex(
                name: "IX_ProgramProgressionAssessments_GeneratedRegistrationId",
                schema: "public",
                table: "ProgramProgressionAssessments",
                column: "GeneratedRegistrationId");

            migrationBuilder.CreateIndex(
                name: "IX_ProgramProgressionAssessments_IsEligible",
                schema: "public",
                table: "ProgramProgressionAssessments",
                column: "IsEligible");

            migrationBuilder.CreateIndex(
                name: "IX_ProgramProgressionAssessments_RecordedBy",
                schema: "public",
                table: "ProgramProgressionAssessments",
                column: "RecordedBy");

            migrationBuilder.CreateIndex(
                name: "IX_ProgramProgressionAssessments_RuleId",
                schema: "public",
                table: "ProgramProgressionAssessments",
                column: "RuleId");

            migrationBuilder.CreateIndex(
                name: "IX_ProgramProgressionAssessments_ScheduleParticipantId",
                schema: "public",
                table: "ProgramProgressionAssessments",
                column: "ScheduleParticipantId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProgramProgressionAssessments_SourceEnrollmentId",
                schema: "public",
                table: "ProgramProgressionAssessments",
                column: "SourceEnrollmentId");

            migrationBuilder.CreateIndex(
                name: "IX_ProgramProgressionAssessments_SourceProgramId",
                schema: "public",
                table: "ProgramProgressionAssessments",
                column: "SourceProgramId");

            migrationBuilder.CreateIndex(
                name: "IX_ProgramProgressionAssessments_SourceRegistrationId",
                schema: "public",
                table: "ProgramProgressionAssessments",
                column: "SourceRegistrationId");

            migrationBuilder.CreateIndex(
                name: "IX_ProgramProgressionAssessments_Status",
                schema: "public",
                table: "ProgramProgressionAssessments",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_ProgramProgressionAssessments_StudentProfileId",
                schema: "public",
                table: "ProgramProgressionAssessments",
                column: "StudentProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_ProgramProgressionAssessments_TargetProgramId",
                schema: "public",
                table: "ProgramProgressionAssessments",
                column: "TargetProgramId");

            migrationBuilder.CreateIndex(
                name: "IX_ProgramProgressionRules_SourceProgramId",
                schema: "public",
                table: "ProgramProgressionRules",
                column: "SourceProgramId");

            migrationBuilder.CreateIndex(
                name: "IX_ProgramProgressionRules_SourceProgramId_IsActive",
                schema: "public",
                table: "ProgramProgressionRules",
                columns: new[] { "SourceProgramId", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_ProgramProgressionRules_TargetProgramId",
                schema: "public",
                table: "ProgramProgressionRules",
                column: "TargetProgramId");

            migrationBuilder.CreateIndex(
                name: "IX_ProgramProgressionScheduleParticipants_ScheduleId",
                schema: "public",
                table: "ProgramProgressionScheduleParticipants",
                column: "ScheduleId");

            migrationBuilder.CreateIndex(
                name: "IX_ProgramProgressionScheduleParticipants_ScheduleId_SourceReg~",
                schema: "public",
                table: "ProgramProgressionScheduleParticipants",
                columns: new[] { "ScheduleId", "SourceRegistrationId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProgramProgressionScheduleParticipants_SourceEnrollmentId",
                schema: "public",
                table: "ProgramProgressionScheduleParticipants",
                column: "SourceEnrollmentId");

            migrationBuilder.CreateIndex(
                name: "IX_ProgramProgressionScheduleParticipants_SourceRegistrationId",
                schema: "public",
                table: "ProgramProgressionScheduleParticipants",
                column: "SourceRegistrationId");

            migrationBuilder.CreateIndex(
                name: "IX_ProgramProgressionScheduleParticipants_Status",
                schema: "public",
                table: "ProgramProgressionScheduleParticipants",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_ProgramProgressionScheduleParticipants_StudentProfileId",
                schema: "public",
                table: "ProgramProgressionScheduleParticipants",
                column: "StudentProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_ProgramProgressionSchedules_AssignedTeacherUserId",
                schema: "public",
                table: "ProgramProgressionSchedules",
                column: "AssignedTeacherUserId");

            migrationBuilder.CreateIndex(
                name: "IX_ProgramProgressionSchedules_BranchId",
                schema: "public",
                table: "ProgramProgressionSchedules",
                column: "BranchId");

            migrationBuilder.CreateIndex(
                name: "IX_ProgramProgressionSchedules_CreatedByUserId",
                schema: "public",
                table: "ProgramProgressionSchedules",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_ProgramProgressionSchedules_RoomId",
                schema: "public",
                table: "ProgramProgressionSchedules",
                column: "RoomId");

            migrationBuilder.CreateIndex(
                name: "IX_ProgramProgressionSchedules_ScheduledAt",
                schema: "public",
                table: "ProgramProgressionSchedules",
                column: "ScheduledAt");

            migrationBuilder.CreateIndex(
                name: "IX_ProgramProgressionSchedules_SourceClassId",
                schema: "public",
                table: "ProgramProgressionSchedules",
                column: "SourceClassId");

            migrationBuilder.CreateIndex(
                name: "IX_ProgramProgressionSchedules_SourceProgramId",
                schema: "public",
                table: "ProgramProgressionSchedules",
                column: "SourceProgramId");

            migrationBuilder.CreateIndex(
                name: "IX_ProgramProgressionSchedules_Status",
                schema: "public",
                table: "ProgramProgressionSchedules",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_QuestionBankItems_ProgramId_Level_IsDeleted",
                schema: "public",
                table: "QuestionBankItems",
                columns: new[] { "ProgramId", "Level", "IsDeleted" });

            migrationBuilder.CreateIndex(
                name: "IX_RefreshTokens_Token",
                schema: "public",
                table: "RefreshTokens",
                column: "Token",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RefreshTokens_UserId",
                schema: "public",
                table: "RefreshTokens",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_RegistrationDiscountCampaigns_BranchId",
                schema: "public",
                table: "RegistrationDiscountCampaigns",
                column: "BranchId");

            migrationBuilder.CreateIndex(
                name: "IX_RegistrationDiscountCampaigns_IsActive_StartDate_EndDate_Pr~",
                schema: "public",
                table: "RegistrationDiscountCampaigns",
                columns: new[] { "IsActive", "StartDate", "EndDate", "Priority" });

            migrationBuilder.CreateIndex(
                name: "IX_RegistrationDiscountCampaigns_ProgramId",
                schema: "public",
                table: "RegistrationDiscountCampaigns",
                column: "ProgramId");

            migrationBuilder.CreateIndex(
                name: "IX_RegistrationDiscountCampaigns_TuitionPlanId",
                schema: "public",
                table: "RegistrationDiscountCampaigns",
                column: "TuitionPlanId");

            migrationBuilder.CreateIndex(
                name: "IX_Registrations_BranchId",
                schema: "public",
                table: "Registrations",
                column: "BranchId");

            migrationBuilder.CreateIndex(
                name: "IX_Registrations_ClassId",
                schema: "public",
                table: "Registrations",
                column: "ClassId");

            migrationBuilder.CreateIndex(
                name: "IX_Registrations_DiscountCampaignId",
                schema: "public",
                table: "Registrations",
                column: "DiscountCampaignId");

            migrationBuilder.CreateIndex(
                name: "IX_Registrations_OriginalRegistrationId",
                schema: "public",
                table: "Registrations",
                column: "OriginalRegistrationId");

            migrationBuilder.CreateIndex(
                name: "IX_Registrations_ProgramId",
                schema: "public",
                table: "Registrations",
                column: "ProgramId");

            migrationBuilder.CreateIndex(
                name: "IX_Registrations_SecondaryClassId",
                schema: "public",
                table: "Registrations",
                column: "SecondaryClassId");

            migrationBuilder.CreateIndex(
                name: "IX_Registrations_SecondaryProgramId",
                schema: "public",
                table: "Registrations",
                column: "SecondaryProgramId");

            migrationBuilder.CreateIndex(
                name: "IX_Registrations_StudentProfileId",
                schema: "public",
                table: "Registrations",
                column: "StudentProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_Registrations_TuitionPlanId",
                schema: "public",
                table: "Registrations",
                column: "TuitionPlanId");

            migrationBuilder.CreateIndex(
                name: "IX_ReportComments_CommenterId",
                schema: "public",
                table: "ReportComments",
                column: "CommenterId");

            migrationBuilder.CreateIndex(
                name: "IX_ReportComments_ReportId",
                schema: "public",
                table: "ReportComments",
                column: "ReportId");

            migrationBuilder.CreateIndex(
                name: "IX_ReportComments_SessionReportId",
                schema: "public",
                table: "ReportComments",
                column: "SessionReportId");

            migrationBuilder.CreateIndex(
                name: "IX_ReportRequests_LinkedMonthlyReportId",
                schema: "public",
                table: "ReportRequests",
                column: "LinkedMonthlyReportId");

            migrationBuilder.CreateIndex(
                name: "IX_ReportRequests_LinkedSessionReportId",
                schema: "public",
                table: "ReportRequests",
                column: "LinkedSessionReportId");

            migrationBuilder.CreateIndex(
                name: "IX_ReportRequests_RequestedByUserId",
                schema: "public",
                table: "ReportRequests",
                column: "RequestedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_ReportRequests_TargetClassId",
                schema: "public",
                table: "ReportRequests",
                column: "TargetClassId");

            migrationBuilder.CreateIndex(
                name: "IX_ReportRequests_TargetSessionId",
                schema: "public",
                table: "ReportRequests",
                column: "TargetSessionId");

            migrationBuilder.CreateIndex(
                name: "IX_ReportRequests_TargetStudentProfileId",
                schema: "public",
                table: "ReportRequests",
                column: "TargetStudentProfileId");

            migrationBuilder.CreateIndex(
                name: "report_request_teacher_queue_idx",
                schema: "public",
                table: "ReportRequests",
                columns: new[] { "AssignedTeacherUserId", "Status", "Priority", "DueAt" });

            migrationBuilder.CreateIndex(
                name: "report_request_type_class_month_idx",
                schema: "public",
                table: "ReportRequests",
                columns: new[] { "ReportType", "TargetClassId", "Month", "Year" });

            migrationBuilder.CreateIndex(
                name: "report_request_type_student_month_idx",
                schema: "public",
                table: "ReportRequests",
                columns: new[] { "ReportType", "TargetStudentProfileId", "Month", "Year" });

            migrationBuilder.CreateIndex(
                name: "IX_RewardRedemptions_HandledBy",
                schema: "public",
                table: "RewardRedemptions",
                column: "HandledBy");

            migrationBuilder.CreateIndex(
                name: "IX_RewardRedemptions_ItemId",
                schema: "public",
                table: "RewardRedemptions",
                column: "ItemId");

            migrationBuilder.CreateIndex(
                name: "IX_RewardRedemptions_StudentProfileId",
                schema: "public",
                table: "RewardRedemptions",
                column: "StudentProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_SessionReports_ReviewedByUserId",
                schema: "public",
                table: "SessionReports",
                column: "ReviewedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_SessionReports_StudentProfileId",
                schema: "public",
                table: "SessionReports",
                column: "StudentProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_SessionReports_SubmittedByUserId",
                schema: "public",
                table: "SessionReports",
                column: "SubmittedByUserId");

            migrationBuilder.CreateIndex(
                name: "session_report_teacher_date_idx",
                schema: "public",
                table: "SessionReports",
                columns: new[] { "TeacherUserId", "ReportDate" });

            migrationBuilder.CreateIndex(
                name: "session_report_unique",
                schema: "public",
                table: "SessionReports",
                columns: new[] { "SessionId", "StudentProfileId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SessionRoles_SessionId",
                schema: "public",
                table: "SessionRoles",
                column: "SessionId");

            migrationBuilder.CreateIndex(
                name: "IX_SessionRoles_StaffUserId",
                schema: "public",
                table: "SessionRoles",
                column: "StaffUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Sessions_ActualAssistantId",
                schema: "public",
                table: "Sessions",
                column: "ActualAssistantId");

            migrationBuilder.CreateIndex(
                name: "IX_Sessions_ActualRoomId",
                schema: "public",
                table: "Sessions",
                column: "ActualRoomId");

            migrationBuilder.CreateIndex(
                name: "IX_Sessions_ActualTeacherId",
                schema: "public",
                table: "Sessions",
                column: "ActualTeacherId");

            migrationBuilder.CreateIndex(
                name: "IX_Sessions_BranchId",
                schema: "public",
                table: "Sessions",
                column: "BranchId");

            migrationBuilder.CreateIndex(
                name: "IX_Sessions_ClassId",
                schema: "public",
                table: "Sessions",
                column: "ClassId");

            migrationBuilder.CreateIndex(
                name: "IX_Sessions_PlannedAssistantId",
                schema: "public",
                table: "Sessions",
                column: "PlannedAssistantId");

            migrationBuilder.CreateIndex(
                name: "IX_Sessions_PlannedRoomId",
                schema: "public",
                table: "Sessions",
                column: "PlannedRoomId");

            migrationBuilder.CreateIndex(
                name: "IX_Sessions_PlannedTeacherId",
                schema: "public",
                table: "Sessions",
                column: "PlannedTeacherId");

            migrationBuilder.CreateIndex(
                name: "IX_ShiftAttendances_ApprovedBy",
                schema: "public",
                table: "ShiftAttendances",
                column: "ApprovedBy");

            migrationBuilder.CreateIndex(
                name: "IX_ShiftAttendances_ContractId",
                schema: "public",
                table: "ShiftAttendances",
                column: "ContractId");

            migrationBuilder.CreateIndex(
                name: "IX_ShiftAttendances_StaffUserId",
                schema: "public",
                table: "ShiftAttendances",
                column: "StaffUserId");

            migrationBuilder.CreateIndex(
                name: "IX_StarTransactions_CreatedBy",
                schema: "public",
                table: "StarTransactions",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_StarTransactions_StudentProfileId",
                schema: "public",
                table: "StarTransactions",
                column: "StudentProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_StudentLevels_StudentProfileId",
                schema: "public",
                table: "StudentLevels",
                column: "StudentProfileId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_StudentMonthlyReports_ClassId",
                schema: "public",
                table: "StudentMonthlyReports",
                column: "ClassId");

            migrationBuilder.CreateIndex(
                name: "IX_StudentMonthlyReports_JobId",
                schema: "public",
                table: "StudentMonthlyReports",
                column: "JobId");

            migrationBuilder.CreateIndex(
                name: "IX_StudentMonthlyReports_ReviewedBy",
                schema: "public",
                table: "StudentMonthlyReports",
                column: "ReviewedBy");

            migrationBuilder.CreateIndex(
                name: "IX_StudentMonthlyReports_StudentProfileId_ClassId_Month_Year",
                schema: "public",
                table: "StudentMonthlyReports",
                columns: new[] { "StudentProfileId", "ClassId", "Month", "Year" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_StudentMonthlyReports_SubmittedBy",
                schema: "public",
                table: "StudentMonthlyReports",
                column: "SubmittedBy");

            migrationBuilder.CreateIndex(
                name: "IX_StudentSessionAssignments_ClassEnrollmentId",
                schema: "public",
                table: "StudentSessionAssignments",
                column: "ClassEnrollmentId");

            migrationBuilder.CreateIndex(
                name: "IX_StudentSessionAssignments_RegistrationId",
                schema: "public",
                table: "StudentSessionAssignments",
                column: "RegistrationId");

            migrationBuilder.CreateIndex(
                name: "IX_StudentSessionAssignments_SessionId",
                schema: "public",
                table: "StudentSessionAssignments",
                column: "SessionId");

            migrationBuilder.CreateIndex(
                name: "IX_StudentSessionAssignments_SessionId_ClassEnrollmentId",
                schema: "public",
                table: "StudentSessionAssignments",
                columns: new[] { "SessionId", "ClassEnrollmentId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_StudentSessionAssignments_SessionId_Status",
                schema: "public",
                table: "StudentSessionAssignments",
                columns: new[] { "SessionId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_StudentSessionAssignments_StudentProfileId",
                schema: "public",
                table: "StudentSessionAssignments",
                column: "StudentProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_StudentSessionAssignments_StudentProfileId_Status",
                schema: "public",
                table: "StudentSessionAssignments",
                columns: new[] { "StudentProfileId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_TeachingMaterialAnnotations_TeachingMaterialId",
                schema: "public",
                table: "TeachingMaterialAnnotations",
                column: "TeachingMaterialId");

            migrationBuilder.CreateIndex(
                name: "IX_TeachingMaterialAnnotations_TeachingMaterialId_SlideNumber",
                schema: "public",
                table: "TeachingMaterialAnnotations",
                columns: new[] { "TeachingMaterialId", "SlideNumber" });

            migrationBuilder.CreateIndex(
                name: "IX_TeachingMaterialAnnotations_UserId",
                schema: "public",
                table: "TeachingMaterialAnnotations",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_TeachingMaterialBookmarks_TeachingMaterialId_UserId",
                schema: "public",
                table: "TeachingMaterialBookmarks",
                columns: new[] { "TeachingMaterialId", "UserId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TeachingMaterialBookmarks_UserId",
                schema: "public",
                table: "TeachingMaterialBookmarks",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_TeachingMaterials_ProgramId",
                schema: "public",
                table: "TeachingMaterials",
                column: "ProgramId");

            migrationBuilder.CreateIndex(
                name: "IX_TeachingMaterials_ProgramId_UnitNumber_LessonNumber",
                schema: "public",
                table: "TeachingMaterials",
                columns: new[] { "ProgramId", "UnitNumber", "LessonNumber" });

            migrationBuilder.CreateIndex(
                name: "IX_TeachingMaterials_UploadedByUserId",
                schema: "public",
                table: "TeachingMaterials",
                column: "UploadedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_TeachingMaterialSlides_TeachingMaterialId",
                schema: "public",
                table: "TeachingMaterialSlides",
                column: "TeachingMaterialId");

            migrationBuilder.CreateIndex(
                name: "IX_TeachingMaterialSlides_TeachingMaterialId_SlideNumber",
                schema: "public",
                table: "TeachingMaterialSlides",
                columns: new[] { "TeachingMaterialId", "SlideNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TeachingMaterialViewProgresses_Completed",
                schema: "public",
                table: "TeachingMaterialViewProgresses",
                column: "Completed");

            migrationBuilder.CreateIndex(
                name: "IX_TeachingMaterialViewProgresses_TeachingMaterialId",
                schema: "public",
                table: "TeachingMaterialViewProgresses",
                column: "TeachingMaterialId");

            migrationBuilder.CreateIndex(
                name: "IX_TeachingMaterialViewProgresses_TeachingMaterialId_UserId",
                schema: "public",
                table: "TeachingMaterialViewProgresses",
                columns: new[] { "TeachingMaterialId", "UserId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TeachingMaterialViewProgresses_UserId",
                schema: "public",
                table: "TeachingMaterialViewProgresses",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_TicketComments_CommenterProfileId",
                schema: "public",
                table: "TicketComments",
                column: "CommenterProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_TicketComments_CommenterUserId",
                schema: "public",
                table: "TicketComments",
                column: "CommenterUserId");

            migrationBuilder.CreateIndex(
                name: "IX_TicketComments_TicketId",
                schema: "public",
                table: "TicketComments",
                column: "TicketId");

            migrationBuilder.CreateIndex(
                name: "IX_Tickets_AssignedToUserId",
                schema: "public",
                table: "Tickets",
                column: "AssignedToUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Tickets_BranchId",
                schema: "public",
                table: "Tickets",
                column: "BranchId");

            migrationBuilder.CreateIndex(
                name: "IX_Tickets_ClassId",
                schema: "public",
                table: "Tickets",
                column: "ClassId");

            migrationBuilder.CreateIndex(
                name: "IX_Tickets_OpenedByProfileId",
                schema: "public",
                table: "Tickets",
                column: "OpenedByProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_Tickets_OpenedByUserId",
                schema: "public",
                table: "Tickets",
                column: "OpenedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_TuitionPlans_ProgramId",
                schema: "public",
                table: "TuitionPlans",
                column: "ProgramId");

            migrationBuilder.CreateIndex(
                name: "IX_Users_BranchId",
                schema: "public",
                table: "Users",
                column: "BranchId");

            migrationBuilder.CreateIndex(
                name: "IX_Users_Email",
                schema: "public",
                table: "Users",
                column: "Email",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Attendances",
                schema: "public");

            migrationBuilder.DropTable(
                name: "AttendanceStreaks",
                schema: "public");

            migrationBuilder.DropTable(
                name: "AuditLogs",
                schema: "public");

            migrationBuilder.DropTable(
                name: "Blogs",
                schema: "public");

            migrationBuilder.DropTable(
                name: "BranchPrograms",
                schema: "public");

            migrationBuilder.DropTable(
                name: "ClassEnrollmentScheduleSegments",
                schema: "public");

            migrationBuilder.DropTable(
                name: "ClassScheduleSegments",
                schema: "public");

            migrationBuilder.DropTable(
                name: "DeviceTokens",
                schema: "public");

            migrationBuilder.DropTable(
                name: "EmailTemplates",
                schema: "public");

            migrationBuilder.DropTable(
                name: "EnrollmentConfirmationPaymentSettings",
                schema: "public");

            migrationBuilder.DropTable(
                name: "EnrollmentConfirmationPdfs",
                schema: "public");

            migrationBuilder.DropTable(
                name: "ExamResults",
                schema: "public");

            migrationBuilder.DropTable(
                name: "ExamSubmissionAnswers",
                schema: "public");

            migrationBuilder.DropTable(
                name: "ExtracurricularPrograms",
                schema: "public");

            migrationBuilder.DropTable(
                name: "FaqItems",
                schema: "public");

            migrationBuilder.DropTable(
                name: "GamificationSettings",
                schema: "public");

            migrationBuilder.DropTable(
                name: "Holidays",
                schema: "public");

            migrationBuilder.DropTable(
                name: "HomeworkQuestions",
                schema: "public");

            migrationBuilder.DropTable(
                name: "HomeworkSubmissionAttempts",
                schema: "public");

            migrationBuilder.DropTable(
                name: "InvoiceLines",
                schema: "public");

            migrationBuilder.DropTable(
                name: "LandingPageSettings",
                schema: "public");

            migrationBuilder.DropTable(
                name: "LeadActivities",
                schema: "public");

            migrationBuilder.DropTable(
                name: "LeaveRequests",
                schema: "public");

            migrationBuilder.DropTable(
                name: "LessonPlans",
                schema: "public");

            migrationBuilder.DropTable(
                name: "MakeupAllocations",
                schema: "public");

            migrationBuilder.DropTable(
                name: "MakeupSettings",
                schema: "public");

            migrationBuilder.DropTable(
                name: "MediaAssets",
                schema: "public");

            migrationBuilder.DropTable(
                name: "MissionProgresses",
                schema: "public");

            migrationBuilder.DropTable(
                name: "MissionRewardRules",
                schema: "public");

            migrationBuilder.DropTable(
                name: "MonthlyReportData",
                schema: "public");

            migrationBuilder.DropTable(
                name: "MonthlyWorkHours",
                schema: "public");

            migrationBuilder.DropTable(
                name: "Notifications",
                schema: "public");

            migrationBuilder.DropTable(
                name: "ParentPinResetTokens",
                schema: "public");

            migrationBuilder.DropTable(
                name: "ParentStudentLinks",
                schema: "public");

            migrationBuilder.DropTable(
                name: "PasswordResetTokens",
                schema: "public");

            migrationBuilder.DropTable(
                name: "PauseEnrollmentRequestHistories",
                schema: "public");

            migrationBuilder.DropTable(
                name: "PauseEnrollmentSettings",
                schema: "public");

            migrationBuilder.DropTable(
                name: "Payments",
                schema: "public");

            migrationBuilder.DropTable(
                name: "PayrollLines",
                schema: "public");

            migrationBuilder.DropTable(
                name: "PayrollPayments",
                schema: "public");

            migrationBuilder.DropTable(
                name: "PlacementTests",
                schema: "public");

            migrationBuilder.DropTable(
                name: "ProgramLeavePolicies",
                schema: "public");

            migrationBuilder.DropTable(
                name: "ProgramProgressionAssessments",
                schema: "public");

            migrationBuilder.DropTable(
                name: "QuestionBankItems",
                schema: "public");

            migrationBuilder.DropTable(
                name: "RefreshTokens",
                schema: "public");

            migrationBuilder.DropTable(
                name: "ReportComments",
                schema: "public");

            migrationBuilder.DropTable(
                name: "ReportRequests",
                schema: "public");

            migrationBuilder.DropTable(
                name: "RewardRedemptions",
                schema: "public");

            migrationBuilder.DropTable(
                name: "SessionRoles",
                schema: "public");

            migrationBuilder.DropTable(
                name: "ShiftAttendances",
                schema: "public");

            migrationBuilder.DropTable(
                name: "StarTransactions",
                schema: "public");

            migrationBuilder.DropTable(
                name: "StudentLevels",
                schema: "public");

            migrationBuilder.DropTable(
                name: "StudentSessionAssignments",
                schema: "public");

            migrationBuilder.DropTable(
                name: "TeacherCompensationSettings",
                schema: "public");

            migrationBuilder.DropTable(
                name: "TeachingMaterialAnnotations",
                schema: "public");

            migrationBuilder.DropTable(
                name: "TeachingMaterialBookmarks",
                schema: "public");

            migrationBuilder.DropTable(
                name: "TeachingMaterialSlides",
                schema: "public");

            migrationBuilder.DropTable(
                name: "TeachingMaterialViewProgresses",
                schema: "public");

            migrationBuilder.DropTable(
                name: "TicketComments",
                schema: "public");

            migrationBuilder.DropTable(
                name: "ExamQuestions",
                schema: "public");

            migrationBuilder.DropTable(
                name: "ExamSubmissions",
                schema: "public");

            migrationBuilder.DropTable(
                name: "FaqCategories",
                schema: "public");

            migrationBuilder.DropTable(
                name: "HomeworkStudents",
                schema: "public");

            migrationBuilder.DropTable(
                name: "LessonPlanTemplates",
                schema: "public");

            migrationBuilder.DropTable(
                name: "MakeupCredits",
                schema: "public");

            migrationBuilder.DropTable(
                name: "NotificationTemplates",
                schema: "public");

            migrationBuilder.DropTable(
                name: "PauseEnrollmentRequests",
                schema: "public");

            migrationBuilder.DropTable(
                name: "Invoices",
                schema: "public");

            migrationBuilder.DropTable(
                name: "CashbookEntries",
                schema: "public");

            migrationBuilder.DropTable(
                name: "PayrollRuns",
                schema: "public");

            migrationBuilder.DropTable(
                name: "LeadChildren",
                schema: "public");

            migrationBuilder.DropTable(
                name: "ProgramProgressionRules",
                schema: "public");

            migrationBuilder.DropTable(
                name: "ProgramProgressionScheduleParticipants",
                schema: "public");

            migrationBuilder.DropTable(
                name: "SessionReports",
                schema: "public");

            migrationBuilder.DropTable(
                name: "StudentMonthlyReports",
                schema: "public");

            migrationBuilder.DropTable(
                name: "RewardStoreItems",
                schema: "public");

            migrationBuilder.DropTable(
                name: "Contracts",
                schema: "public");

            migrationBuilder.DropTable(
                name: "TeachingMaterials",
                schema: "public");

            migrationBuilder.DropTable(
                name: "Tickets",
                schema: "public");

            migrationBuilder.DropTable(
                name: "Exams",
                schema: "public");

            migrationBuilder.DropTable(
                name: "HomeworkAssignments",
                schema: "public");

            migrationBuilder.DropTable(
                name: "Leads",
                schema: "public");

            migrationBuilder.DropTable(
                name: "ClassEnrollments",
                schema: "public");

            migrationBuilder.DropTable(
                name: "ProgramProgressionSchedules",
                schema: "public");

            migrationBuilder.DropTable(
                name: "MonthlyReportJobs",
                schema: "public");

            migrationBuilder.DropTable(
                name: "Missions",
                schema: "public");

            migrationBuilder.DropTable(
                name: "Sessions",
                schema: "public");

            migrationBuilder.DropTable(
                name: "Registrations",
                schema: "public");

            migrationBuilder.DropTable(
                name: "Classes",
                schema: "public");

            migrationBuilder.DropTable(
                name: "Profiles",
                schema: "public");

            migrationBuilder.DropTable(
                name: "RegistrationDiscountCampaigns",
                schema: "public");

            migrationBuilder.DropTable(
                name: "Classrooms",
                schema: "public");

            migrationBuilder.DropTable(
                name: "Users",
                schema: "public");

            migrationBuilder.DropTable(
                name: "TuitionPlans",
                schema: "public");

            migrationBuilder.DropTable(
                name: "Branches",
                schema: "public");

            migrationBuilder.DropTable(
                name: "Programs",
                schema: "public");
        }
    }
}
