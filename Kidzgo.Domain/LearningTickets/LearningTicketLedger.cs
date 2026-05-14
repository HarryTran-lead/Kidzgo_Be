using Kidzgo.Domain.Common;
using Kidzgo.Domain.Registrations;
using Kidzgo.Domain.Sessions;
using Kidzgo.Domain.Users;

namespace Kidzgo.Domain.LearningTickets;

public class LearningTicketLedger : Entity
{
    public Guid Id { get; set; }
    public Guid StudentProfileId { get; set; }
    public Guid RegistrationId { get; set; }
    public Guid? LearningTicketItemId { get; set; }
    public Guid? SessionId { get; set; }
    public Guid? AttendanceId { get; set; }
    public LearningTicketTransactionType TransactionType { get; set; }
    public int Quantity { get; set; }
    public string Reason { get; set; } = null!;
    public Guid? CreatedByUserId { get; set; }
    public DateTime CreatedAt { get; set; }

    public Profile StudentProfile { get; set; } = null!;
    public Registration Registration { get; set; } = null!;
    public LearningTicketItem? LearningTicketItem { get; set; }
    public Session? Session { get; set; }
    public Attendance? Attendance { get; set; }
    public User? CreatedByUser { get; set; }
}
