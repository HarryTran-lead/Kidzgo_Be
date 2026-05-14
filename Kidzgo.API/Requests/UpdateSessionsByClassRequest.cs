namespace Kidzgo.API.Requests;

public sealed class UpdateSessionsByClassRequest
{
    /// Class ID để update tất cả sessions của class này
    public Guid ClassId { get; set; }
    
    /// Danh sách Session IDs cụ thể cần update (nếu có, sẽ chỉ update các sessions này thay vì tất cả)
    public List<Guid>? SessionIds { get; set; }
    
    /// Chỉ update các sessions có status này (nếu null thì update tất cả). Giá trị: Scheduled, Completed, Cancelled
    public string? FilterByStatus { get; set; }
    
    /// Chỉ update các sessions từ ngày này trở đi (nếu null thì update tất cả)
    public DateTime? FromDate { get; set; }
    
    /// Các field cần update (nếu null thì không update field đó)
    public DateTime? PlannedDatetime { get; set; }
    public int? DurationMinutes { get; set; }
    public Guid? PlannedRoomId { get; set; }
    public Guid? PlannedTeacherId { get; set; }
    public Guid? PlannedAssistantId { get; set; }
    public string? ParticipationType { get; set; }
    public string? SectionType { get; set; }
}

