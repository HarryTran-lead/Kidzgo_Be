using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Services;
using Kidzgo.Application.Services;
using Kidzgo.Domain.Classes;
using Kidzgo.Domain.Users;
using Kidzgo.Domain.Website;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.LandingPages.Shared;

internal static class LandingPageReadModelBuilder
{
    private static readonly ClassStatus[] PublicClassStatuses =
    [
        ClassStatus.Planned,
        ClassStatus.Recruiting,
        ClassStatus.Active,
        ClassStatus.Full
    ];

    public static async Task<LandingPageResolvedContent> BuildAsync(
        IDbContext context,
        ISchedulePatternParser schedulePatternParser,
        LandingPageSettings? settings,
        bool publicOnly,
        CancellationToken cancellationToken)
    {
        var featuredProgramConfigs = LandingPageSettingsJsonHelper.DeserializeFeaturedItemConfigs(
            settings?.FeaturedProgramConfigsJson,
            settings?.FeaturedProgramIdsJson);
        var featuredClassConfigs = LandingPageSettingsJsonHelper.DeserializeFeaturedItemConfigs(
            settings?.FeaturedClassConfigsJson,
            settings?.FeaturedClassIdsJson);
        var featuredProgramIds = featuredProgramConfigs.Select(item => item.Id).ToList();
        var featuredClassIds = featuredClassConfigs.Select(item => item.Id).ToList();
        var featuredTeacherIds = LandingPageSettingsJsonHelper.DeserializeIds(settings?.FeaturedTeacherIdsJson);
        var footerAddresses = LandingPageSettingsJsonHelper.DeserializeStringList(settings?.FooterAddressesJson);
        if (footerAddresses.Count == 0 && !string.IsNullOrWhiteSpace(settings?.FooterAddress))
        {
            footerAddresses = [settings.FooterAddress.Trim()];
        }

        var footerContactPhones = LandingPageSettingsJsonHelper.DeserializeStringList(settings?.FooterContactPhonesJson);
        if (footerContactPhones.Count == 0 && !string.IsNullOrWhiteSpace(settings?.FooterContactPhone))
        {
            footerContactPhones = [settings.FooterContactPhone.Trim()];
        }

        var footerSocialLinks = LandingPageSettingsJsonHelper.DeserializeFooterSocialLinks(settings?.FooterSocialLinksJson);

        var featuredPrograms = await LoadFeaturedProgramsAsync(
            context,
            featuredProgramConfigs,
            publicOnly,
            cancellationToken);
        var featuredClasses = await LoadFeaturedClassesAsync(
            context,
            schedulePatternParser,
            featuredClassConfigs,
            publicOnly,
            cancellationToken);
        var featuredTeachers = await LoadFeaturedTeachersAsync(
            context,
            featuredTeacherIds,
            publicOnly,
            cancellationToken);

        return new LandingPageResolvedContent
        {
            LogoUrl = settings?.LogoUrl,
            FeaturedProgramsSection = new LandingPageSectionDto
            {
                Title = settings?.FeaturedProgramsSectionTitle,
                Subtitle = settings?.FeaturedProgramsSectionSubtitle
            },
            FeaturedClassesSection = new LandingPageSectionDto
            {
                Title = settings?.FeaturedClassesSectionTitle,
                Subtitle = settings?.FeaturedClassesSectionSubtitle
            },
            FeaturedTeachersSection = new LandingPageSectionDto
            {
                Title = settings?.FeaturedTeachersSectionTitle,
                Subtitle = settings?.FeaturedTeachersSectionSubtitle
            },
            Footer = new LandingPageFooterDto
            {
                Address = settings?.FooterAddress ?? footerAddresses.FirstOrDefault(),
                ContactPhone = settings?.FooterContactPhone ?? footerContactPhones.FirstOrDefault(),
                ContactPhones = footerContactPhones,
                ContactEmail = settings?.FooterContactEmail,
                Addresses = footerAddresses,
                SocialLinks = footerSocialLinks
            },
            FeaturedProgramIds = featuredProgramIds,
            FeaturedClassIds = featuredClassIds,
            FeaturedTeacherIds = featuredTeacherIds,
            FeaturedProgramConfigs = featuredProgramConfigs,
            FeaturedClassConfigs = featuredClassConfigs,
            FeaturedPrograms = featuredPrograms,
            FeaturedClasses = featuredClasses,
            FeaturedTeachers = featuredTeachers,
            UpdatedAt = settings?.UpdatedAt ?? settings?.CreatedAt
        };
    }

    private static async Task<IReadOnlyList<LandingPageProgramDto>> LoadFeaturedProgramsAsync(
        IDbContext context,
        IReadOnlyList<LandingPageFeaturedItemConfigDto> programConfigs,
        bool publicOnly,
        CancellationToken cancellationToken)
    {
        var programIds = programConfigs.Select(item => item.Id).ToList();
        if (programIds.Count == 0)
        {
            return Array.Empty<LandingPageProgramDto>();
        }

        var tagsByProgramId = programConfigs.ToDictionary(item => item.Id, item => item.Tags);

        var programs = await context.Programs
            .AsNoTracking()
            .Where(p => programIds.Contains(p.Id) &&
                        !p.IsDeleted &&
                        (!publicOnly || p.IsActive))
            .Select(p => new LandingPageProgramDto
            {
                Id = p.Id,
                Name = p.Name,
                Code = p.Code,
                Description = p.Description,
                IsMakeup = p.IsMakeup,
                IsSupplementary = p.IsSupplementary,
                IsActive = p.IsActive
            })
            .ToListAsync(cancellationToken);

        var tuitionPlans = await context.TuitionPlans
            .AsNoTracking()
            .Where(tp => programIds.Contains(tp.ProgramId) &&
                         !tp.IsDeleted &&
                         (!publicOnly || tp.IsActive))
            .Select(tp => new
            {
                tp.ProgramId,
                Dto = new LandingPageTuitionPlanDto
                {
                    Id = tp.Id,
                    Name = tp.Name,
                    TotalSessions = tp.TotalSessions,
                    TuitionAmount = tp.TuitionAmount,
                    UnitPriceSession = tp.UnitPriceSession,
                    Currency = tp.Currency,
                    IsActive = tp.IsActive
                }
            })
            .ToListAsync(cancellationToken);

        var tuitionPlansByProgramId = tuitionPlans
            .GroupBy(item => item.ProgramId)
            .ToDictionary(
                group => group.Key,
                group => (IReadOnlyList<LandingPageTuitionPlanDto>)group
                    .Select(item => item.Dto)
                    .OrderByDescending(item => item.IsActive)
                    .ThenBy(item => item.TotalSessions)
                    .ThenBy(item => item.TuitionAmount)
                    .ToList());

        var finalizedPrograms = programs
            .Select(program => new LandingPageProgramDto
            {
                Id = program.Id,
                Name = program.Name,
                Code = program.Code,
                Description = program.Description,
                IsMakeup = program.IsMakeup,
                IsSupplementary = program.IsSupplementary,
                IsActive = program.IsActive,
                Tags = tagsByProgramId.GetValueOrDefault(program.Id) ?? Array.Empty<string>(),
                TuitionPlans = tuitionPlansByProgramId.GetValueOrDefault(program.Id) ?? Array.Empty<LandingPageTuitionPlanDto>()
            });

        return OrderByConfiguredIds(finalizedPrograms, programIds, item => item.Id);
    }

    private static async Task<IReadOnlyList<LandingPageClassDto>> LoadFeaturedClassesAsync(
        IDbContext context,
        ISchedulePatternParser schedulePatternParser,
        IReadOnlyList<LandingPageFeaturedItemConfigDto> classConfigs,
        bool publicOnly,
        CancellationToken cancellationToken)
    {
        var classIds = classConfigs.Select(item => item.Id).ToList();
        if (classIds.Count == 0)
        {
            return Array.Empty<LandingPageClassDto>();
        }

        var tagsByClassId = classConfigs.ToDictionary(item => item.Id, item => item.Tags);

        var classRows = await context.Classes
            .AsNoTracking()
            .Where(c => classIds.Contains(c.Id) &&
                        (!publicOnly || PublicClassStatuses.Contains(c.Status)))
            .Select(c => new LandingPageClassRow
            {
                Id = c.Id,
                BranchId = c.BranchId,
                BranchName = c.Branch.Name,
                ProgramId = c.ProgramId,
                ProgramName = c.Program.Name,
                Code = c.Code,
                Title = c.Title,
                MainTeacherId = c.MainTeacherId,
                MainTeacherName = c.MainTeacher != null ? c.MainTeacher.Name : null,
                AssistantTeacherId = c.AssistantTeacherId,
                AssistantTeacherName = c.AssistantTeacher != null ? c.AssistantTeacher.Name : null,
                StartDate = c.StartDate,
                EndDate = c.EndDate,
                Status = c.Status.ToString(),
                Capacity = c.Capacity,
                CurrentEnrollmentCount = c.ClassEnrollments.Count(ce => ce.Status == EnrollmentStatus.Active),
                WeeklyScheduleJson = c.WeeklyScheduleJson,
                Description = c.Description,
                RoomId = c.RoomId,
                RoomName = c.Room != null ? c.Room.Name : null
            })
            .ToListAsync(cancellationToken);

        var effectiveClassIds = classRows.Select(item => item.Id).ToList();
        var scheduleSegments = await context.ClassScheduleSegments
            .AsNoTracking()
            .Where(segment => effectiveClassIds.Contains(segment.ClassId))
            .OrderBy(segment => segment.EffectiveFrom)
            .Select(segment => new
            {
                segment.ClassId,
                segment.EffectiveFrom,
                segment.EffectiveTo,
                segment.WeeklyScheduleJson
            })
            .ToListAsync(cancellationToken);

        var scheduleSegmentsByClassId = scheduleSegments
            .GroupBy(segment => segment.ClassId)
            .ToDictionary(
                group => group.Key,
                group => group.Select(segment => new WeeklyScheduleSegmentWindow(
                        segment.EffectiveFrom,
                        segment.EffectiveTo,
                        segment.WeeklyScheduleJson))
                    .ToList());

        var today = VietnamTime.TodayDateOnly();
        var classes = new List<LandingPageClassDto>(classRows.Count);

        foreach (var row in classRows)
        {
            scheduleSegmentsByClassId.TryGetValue(row.Id, out var segments);
            var effectiveWeeklyScheduleJson = SchedulePatternSupport.ResolveEffectiveWeeklyScheduleJson(
                row.WeeklyScheduleJson,
                segments ?? [],
                today);

            var weeklyScheduleSlots = Array.Empty<ScheduleSlot>();
            if (!string.IsNullOrWhiteSpace(effectiveWeeklyScheduleJson))
            {
                var parseResult = schedulePatternParser.ParseScheduleSlots(effectiveWeeklyScheduleJson);
                if (parseResult.IsSuccess)
                {
                    weeklyScheduleSlots = parseResult.Value.ToArray();
                }
            }

            classes.Add(new LandingPageClassDto
            {
                Id = row.Id,
                BranchId = row.BranchId,
                BranchName = row.BranchName,
                ProgramId = row.ProgramId,
                ProgramName = row.ProgramName,
                Code = row.Code,
                Title = row.Title,
                MainTeacherId = row.MainTeacherId,
                MainTeacherName = row.MainTeacherName,
                AssistantTeacherId = row.AssistantTeacherId,
                AssistantTeacherName = row.AssistantTeacherName,
                StartDate = row.StartDate,
                EndDate = row.EndDate,
                Status = row.Status,
                Capacity = row.Capacity,
                CurrentEnrollmentCount = row.CurrentEnrollmentCount,
                WeeklyScheduleSlots = weeklyScheduleSlots,
                ScheduleText = SchedulePatternSupport.BuildDisplayText(effectiveWeeklyScheduleJson),
                Description = row.Description,
                RoomId = row.RoomId,
                RoomName = row.RoomName,
                Tags = tagsByClassId.GetValueOrDefault(row.Id) ?? Array.Empty<string>()
            });
        }

        return OrderByConfiguredIds(classes, classIds, item => item.Id);
    }

    private static async Task<IReadOnlyList<LandingPageTeacherDto>> LoadFeaturedTeachersAsync(
        IDbContext context,
        IReadOnlyList<Guid> teacherIds,
        bool publicOnly,
        CancellationToken cancellationToken)
    {
        if (teacherIds.Count == 0)
        {
            return Array.Empty<LandingPageTeacherDto>();
        }

        var teachers = await context.Users
            .AsNoTracking()
            .Where(user => teacherIds.Contains(user.Id) &&
                           user.Role == UserRole.Teacher &&
                           !user.IsDeleted &&
                           (!publicOnly || user.IsActive))
            .Select(user => new LandingPageTeacherDto
            {
                Id = user.Id,
                Name = string.IsNullOrWhiteSpace(user.Name) ? user.Email : user.Name,
                AvatarUrl = user.AvatarUrl,
                BranchId = user.BranchId,
                BranchName = user.Branch != null ? user.Branch.Name : null,
                IsActive = user.IsActive
            })
            .ToListAsync(cancellationToken);

        var classAssignments = await context.Classes
            .AsNoTracking()
            .Where(c => ((c.MainTeacherId.HasValue && teacherIds.Contains(c.MainTeacherId.Value)) ||
                         (c.AssistantTeacherId.HasValue && teacherIds.Contains(c.AssistantTeacherId.Value))) &&
                        (!publicOnly || PublicClassStatuses.Contains(c.Status)))
            .Select(c => new
            {
                c.Id,
                c.MainTeacherId,
                c.AssistantTeacherId,
                ProgramName = c.Program.Name
            })
            .ToListAsync(cancellationToken);

        var teachingAggregates = new Dictionary<Guid, TeacherAggregate>();

        foreach (var assignment in classAssignments)
        {
            if (assignment.MainTeacherId.HasValue)
            {
                AddTeacherAssignment(teachingAggregates, assignment.MainTeacherId.Value, assignment.Id, assignment.ProgramName);
            }

            if (assignment.AssistantTeacherId.HasValue)
            {
                AddTeacherAssignment(teachingAggregates, assignment.AssistantTeacherId.Value, assignment.Id, assignment.ProgramName);
            }
        }

        var resolvedTeachers = teachers
            .Select(teacher =>
            {
                teachingAggregates.TryGetValue(teacher.Id, out var aggregate);

                return new LandingPageTeacherDto
                {
                    Id = teacher.Id,
                    Name = teacher.Name,
                    AvatarUrl = teacher.AvatarUrl,
                    BranchId = teacher.BranchId,
                    BranchName = teacher.BranchName,
                    IsActive = teacher.IsActive,
                    TeachingClassCount = aggregate?.ClassIds.Count ?? 0,
                    ProgramNames = aggregate is null
                        ? Array.Empty<string>()
                        : aggregate.ProgramNames
                            .OrderBy(name => name)
                            .ToList()
                };
            });

        return OrderByConfiguredIds(resolvedTeachers, teacherIds, item => item.Id);
    }

    private static void AddTeacherAssignment(
        IDictionary<Guid, TeacherAggregate> aggregates,
        Guid teacherId,
        Guid classId,
        string programName)
    {
        if (!aggregates.TryGetValue(teacherId, out var aggregate))
        {
            aggregate = new TeacherAggregate();
            aggregates[teacherId] = aggregate;
        }

        aggregate.ClassIds.Add(classId);

        if (!string.IsNullOrWhiteSpace(programName))
        {
            aggregate.ProgramNames.Add(programName);
        }
    }

    private static IReadOnlyList<T> OrderByConfiguredIds<T>(
        IEnumerable<T> items,
        IReadOnlyList<Guid> configuredIds,
        Func<T, Guid> idSelector)
    {
        var orderById = configuredIds
            .Select((id, index) => new { id, index })
            .ToDictionary(item => item.id, item => item.index);

        return items
            .OrderBy(item => orderById.GetValueOrDefault(idSelector(item), int.MaxValue))
            .ToList();
    }

    private sealed class LandingPageClassRow
    {
        public Guid Id { get; init; }
        public Guid BranchId { get; init; }
        public string BranchName { get; init; } = null!;
        public Guid ProgramId { get; init; }
        public string ProgramName { get; init; } = null!;
        public string Code { get; init; } = null!;
        public string Title { get; init; } = null!;
        public Guid? MainTeacherId { get; init; }
        public string? MainTeacherName { get; init; }
        public Guid? AssistantTeacherId { get; init; }
        public string? AssistantTeacherName { get; init; }
        public DateOnly StartDate { get; init; }
        public DateOnly? EndDate { get; init; }
        public string Status { get; init; } = null!;
        public int Capacity { get; init; }
        public int CurrentEnrollmentCount { get; init; }
        public string? WeeklyScheduleJson { get; init; }
        public string? Description { get; init; }
        public Guid? RoomId { get; init; }
        public string? RoomName { get; init; }
    }

    private sealed class TeacherAggregate
    {
        public HashSet<Guid> ClassIds { get; } = [];
        public HashSet<string> ProgramNames { get; } = new(StringComparer.OrdinalIgnoreCase);
    }
}
