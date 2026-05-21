using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Application.Abstraction.Services;

namespace Kidzgo.Application.Classes.PreviewClassSessions;

public sealed class PreviewClassSessionsCommand : ICommand<PreviewClassSessionsResponse>
{
    public Guid ProgramId { get; init; }
    public Guid LevelId { get; init; }
    public Guid StartModuleId { get; init; }
    public int StartSessionIndex { get; init; }
    public DateOnly StartDate { get; init; }
    public DateOnly? EndDate { get; init; }
    public int SessionsToGenerate { get; init; }
    public bool SkipHolidays { get; init; } = true;
    public List<ScheduleSlot>? WeeklyScheduleSlots { get; init; }
}
