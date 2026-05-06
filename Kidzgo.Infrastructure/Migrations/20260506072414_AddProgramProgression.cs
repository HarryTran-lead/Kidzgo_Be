using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Kidzgo.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddProgramProgression : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
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
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ProgramProgressionAssessments",
                schema: "public");

            migrationBuilder.DropTable(
                name: "ProgramProgressionRules",
                schema: "public");

            migrationBuilder.DropTable(
                name: "ProgramProgressionScheduleParticipants",
                schema: "public");

            migrationBuilder.DropTable(
                name: "ProgramProgressionSchedules",
                schema: "public");
        }
    }
}
