using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Domain.Sessions;

namespace Kidzgo.Application.Sessions.UpdateSessionsByClass;

public sealed class UpdateSessionsByClassCommand : ICommand<UpdateSessionsByClassResponse>
{

    /// Class ID để update tất cả sessions của class này
    public Guid ClassId { get; init; }

    /// Danh sách Session IDs cụ thể cần update (nếu có, sẽ chỉ update các sessions này thay vì tất cả)
    public List<Guid>? SessionIds { get; init; }

    /// Chỉ update các sessions có status này (nếu null thì update tất cả)
    public SessionStatus? FilterByStatus { get; init; }

    /// Chỉ update các sessions từ ngày này trở đi (nếu null thì update tất cả)
    public DateTime? FromDate { get; init; }

    /// Các field cần update (nếu null thì không update field đó)
    public DateTime? PlannedDatetime { get; init; }
    public int? DurationMinutes { get; init; }
    public Guid? PlannedRoomId { get; init; }
    public Guid? PlannedTeacherId { get; init; }
    public Guid? PlannedAssistantId { get; init; }
    public Guid? SlotTypeId { get; init; }
    public ParticipationType? ParticipationType { get; init; }
    public SectionType? SectionType { get; init; }
}

