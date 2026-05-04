namespace Kidzgo.API.Requests;

public sealed class ChangeSessionTeacherRequest
{
    public Guid? SessionId { get; set; }
    public List<Guid>? SessionIds { get; set; }
    public Guid TeacherId { get; set; }
    public string Role { get; set; } = "MainTeacher";
}

