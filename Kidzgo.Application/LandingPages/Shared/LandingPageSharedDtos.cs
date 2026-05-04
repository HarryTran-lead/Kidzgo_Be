using Kidzgo.Application.Abstraction.Services;

namespace Kidzgo.Application.LandingPages.Shared;

public sealed class LandingPageSectionDto
{
    public string? Title { get; init; }
    public string? Subtitle { get; init; }
}

public sealed class LandingPageFooterSocialLinkDto
{
    public string Label { get; init; } = null!;
    public string Url { get; init; } = null!;
    public string? IconKey { get; init; }
}

public sealed class LandingPageFooterDto
{
    public string? Address { get; init; }
    public string? ContactPhone { get; init; }
    public IReadOnlyList<string> ContactPhones { get; init; } = Array.Empty<string>();
    public string? ContactEmail { get; init; }
    public IReadOnlyList<string> Addresses { get; init; } = Array.Empty<string>();
    public IReadOnlyList<LandingPageFooterSocialLinkDto> SocialLinks { get; init; } = Array.Empty<LandingPageFooterSocialLinkDto>();
}

public sealed class LandingPageFeaturedItemConfigDto
{
    public Guid Id { get; init; }
    public IReadOnlyList<string> Tags { get; init; } = Array.Empty<string>();
}

public sealed class LandingPageTuitionPlanDto
{
    public Guid Id { get; init; }
    public string Name { get; init; } = null!;
    public int TotalSessions { get; init; }
    public decimal TuitionAmount { get; init; }
    public decimal UnitPriceSession { get; init; }
    public string Currency { get; init; } = null!;
    public bool IsActive { get; init; }
}

public sealed class LandingPageProgramDto
{
    public Guid Id { get; init; }
    public string Name { get; init; } = null!;
    public string? Code { get; init; }
    public string? Description { get; init; }
    public bool IsMakeup { get; init; }
    public bool IsSupplementary { get; init; }
    public bool IsActive { get; init; }
    public IReadOnlyList<string> Tags { get; init; } = Array.Empty<string>();
    public IReadOnlyList<LandingPageTuitionPlanDto> TuitionPlans { get; init; } = Array.Empty<LandingPageTuitionPlanDto>();
}

public sealed class LandingPageClassDto
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
    public IReadOnlyList<ScheduleSlot> WeeklyScheduleSlots { get; init; } = Array.Empty<ScheduleSlot>();
    public string? ScheduleText { get; init; }
    public string? Description { get; init; }
    public Guid? RoomId { get; init; }
    public string? RoomName { get; init; }
    public IReadOnlyList<string> Tags { get; init; } = Array.Empty<string>();
}

public sealed class LandingPageTeacherDto
{
    public Guid Id { get; init; }
    public string Name { get; init; } = null!;
    public string? AvatarUrl { get; init; }
    public Guid? BranchId { get; init; }
    public string? BranchName { get; init; }
    public bool IsActive { get; init; }
    public int TeachingClassCount { get; init; }
    public IReadOnlyList<string> ProgramNames { get; init; } = Array.Empty<string>();
}
