namespace Kidzgo.API.Requests;

public sealed class UpdateTuitionPlanRequest
{
    public Guid ProgramId { get; set; }
    public Guid? LevelId { get; set; }
    public Guid? LearningTicketTypeId { get; set; }
    public string Name { get; set; } = null!;
    public int TotalSessions { get; set; }
    public decimal TuitionAmount { get; set; }
    public string Currency { get; set; } = null!;
}
