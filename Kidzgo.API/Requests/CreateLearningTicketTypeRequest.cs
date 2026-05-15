namespace Kidzgo.API.Requests;

public sealed class CreateLearningTicketTypeRequest
{
    public string Code { get; set; } = null!;
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;
}

