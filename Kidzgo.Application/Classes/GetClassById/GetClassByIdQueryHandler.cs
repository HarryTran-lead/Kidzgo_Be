using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Services;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Application.Services;
using Kidzgo.Domain.Classes.Errors;
using Kidzgo.Domain.Common;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.Classes.GetClassById;

public sealed class GetClassByIdQueryHandler(
    IDbContext context,
    ISchedulePatternParser schedulePatternParser
) : IQueryHandler<GetClassByIdQuery, GetClassByIdResponse>
{
    public async Task<Result<GetClassByIdResponse>> Handle(GetClassByIdQuery query, CancellationToken cancellationToken)
    {
        var classEntity = await context.Classes
            .Include(c => c.Branch)
            .Include(c => c.Program)
            .Include(c => c.Level)
            .Include(c => c.StartModule)
            .Include(c => c.CurrentModule)
            .Include(c => c.Room)
            .Include(c => c.MainTeacher)
            .Include(c => c.AssistantTeacher)
            .Include(c => c.SlotType)
            .Include(c => c.ModuleProgresses)
                .ThenInclude(x => x.Module)
            .Include(c => c.ScheduleSegments)
            .FirstOrDefaultAsync(c => c.Id == query.Id, cancellationToken);

        if (classEntity is null)
        {
            return Result.Failure<GetClassByIdResponse>(
                ClassErrors.NotFound(query.Id));
        }

        var currentEnrollmentCount = await context.ClassEnrollments
            .CountAsync(
                ce => ce.ClassId == query.Id &&
                      ce.Status == Domain.Classes.EnrollmentStatus.Active,
                cancellationToken);

        var totalSessions = await context.Sessions
            .CountAsync(s => s.ClassId == query.Id, cancellationToken);

        var completedSessions = await context.Sessions
            .CountAsync(
                s => s.ClassId == query.Id &&
                     s.Status == Domain.Sessions.SessionStatus.Completed,
                cancellationToken);

        var effectiveWeeklyScheduleJson = SchedulePatternSupport.ResolveEffectiveWeeklyScheduleJson(
            classEntity.WeeklyScheduleJson,
            classEntity.ScheduleSegments.Select(segment => new WeeklyScheduleSegmentWindow(
                segment.EffectiveFrom,
                segment.EffectiveTo,
                segment.WeeklyScheduleJson)),
            VietnamTime.TodayDateOnly());

        return new GetClassByIdResponse
        {
            Id = classEntity.Id,
            BranchId = classEntity.BranchId,
            BranchName = classEntity.Branch.Name,
            ProgramId = classEntity.ProgramId,
            ProgramName = classEntity.Program.Name,
            LevelId = classEntity.LevelId,
            LevelName = classEntity.Level.Name,
            StartModuleId = classEntity.StartModuleId,
            StartModuleName = classEntity.StartModule.Name,
            CurrentModuleId = classEntity.CurrentModuleId,
            CurrentModuleName = classEntity.CurrentModule.Name,
            SlotTypeId = classEntity.SlotTypeId,
            SlotTypeCode = classEntity.SlotType?.Code,
            Code = classEntity.Code,
            Title = classEntity.Title,
            RoomId = classEntity.RoomId,
            RoomName = classEntity.Room?.Name,
            Description = classEntity.Description,
            MainTeacherId = classEntity.MainTeacherId,
            MainTeacherName = classEntity.MainTeacher?.Name,
            AssistantTeacherId = classEntity.AssistantTeacherId,
            AssistantTeacherName = classEntity.AssistantTeacher?.Name,
            StartDate = classEntity.StartDate,
            EndDate = classEntity.EndDate,
            Status = classEntity.Status.ToString(),
            Capacity = classEntity.Capacity,
            CurrentEnrollmentCount = currentEnrollmentCount,
            WeeklyScheduleSlots = effectiveWeeklyScheduleJson is null
                ? []
                : ParseSlots(effectiveWeeklyScheduleJson),
            TeacherIds = new[] { classEntity.MainTeacherId, classEntity.AssistantTeacherId }
                .Where(x => x.HasValue)
                .Select(x => x!.Value)
                .ToList(),
            TeacherNames = new[] { classEntity.MainTeacher?.Name, classEntity.AssistantTeacher?.Name }
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Select(x => x!)
                .ToList(),
            TotalSessions = totalSessions,
            CompletedSessions = completedSessions,
            ModuleProgresses = classEntity.ModuleProgresses
                .OrderBy(x => x.OrderIndex)
                .Select(x => new ClassModuleProgressDto
                {
                    ModuleId = x.ModuleId,
                    ModuleName = x.Module.Name,
                    OrderIndex = x.OrderIndex,
                    RequiredSessions = x.RequiredSessions,
                    CompletedSessions = x.CompletedSessions,
                    Status = x.Status.ToString(),
                    StartedAt = x.StartedAt,
                    CompletedAt = x.CompletedAt
                })
                .ToList(),
            ScheduleSegments = classEntity.ScheduleSegments
                .OrderBy(segment => segment.EffectiveFrom)
                .Select(segment => new ClassScheduleSegmentDto
                {
                    Id = segment.Id,
                    EffectiveFrom = segment.EffectiveFrom,
                    EffectiveTo = segment.EffectiveTo,
                    WeeklyScheduleSlots = ParseSlots(segment.WeeklyScheduleJson)
                })
                .ToList(),
            CreatedAt = classEntity.CreatedAt,
            UpdatedAt = classEntity.UpdatedAt
        };
    }

    private List<ScheduleSlot> ParseSlots(string weeklyScheduleJson)
    {
        var parseResult = schedulePatternParser.ParseScheduleSlots(weeklyScheduleJson);
        return parseResult.IsSuccess ? parseResult.Value : [];
    }
}

