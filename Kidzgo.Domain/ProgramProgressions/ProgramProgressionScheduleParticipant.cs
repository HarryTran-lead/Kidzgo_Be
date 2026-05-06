using Kidzgo.Domain.Classes;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.Registrations;
using Kidzgo.Domain.Users;

namespace Kidzgo.Domain.ProgramProgressions;

public class ProgramProgressionScheduleParticipant : Entity
{
    public Guid Id { get; set; }
    public Guid ScheduleId { get; set; }
    public Guid StudentProfileId { get; set; }
    public Guid SourceRegistrationId { get; set; }
    public Guid? SourceEnrollmentId { get; set; }
    public ProgramProgressionScheduleParticipantStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public ProgramProgressionSchedule Schedule { get; set; } = null!;
    public Profile StudentProfile { get; set; } = null!;
    public Registration SourceRegistration { get; set; } = null!;
    public ClassEnrollment? SourceEnrollment { get; set; }
    public ProgramProgressionAssessment? Assessment { get; set; }
}
