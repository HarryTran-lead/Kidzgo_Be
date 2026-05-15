using Kidzgo.Domain.Common;
using Kidzgo.Domain.Registrations;
using Kidzgo.Domain.Sessions;
using Kidzgo.Domain.Users;

namespace Kidzgo.Domain.LearningTickets;

public class LearningTicketItem : Entity
{
    public Guid Id { get; set; }
    public Guid StudentProfileId { get; set; }
    public Guid RegistrationId { get; set; }
    public Guid? LearningTicketTypeId { get; set; }
    public LearningTicketItemStatus Status { get; set; }
    public LearningTicketSource Source { get; set; }
    public Guid? ConsumedBySessionId { get; set; }
    public Guid? ConsumedByAttendanceId { get; set; }
    public DateTime? ConsumedAt { get; set; }
    public DateTime CreatedAt { get; set; }

    public Profile StudentProfile { get; set; } = null!;
    public Registration Registration { get; set; } = null!;
    public LearningTicketType? LearningTicketType { get; set; }
    public Session? ConsumedBySession { get; set; }
    public Attendance? ConsumedByAttendance { get; set; }
}
