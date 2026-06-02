namespace Kidzgo.Application.ReportsV3.Shared;

public sealed class ReportSnapshot
{
    public ReportSnapshotStudent Student { get; set; } = new();
    public ReportSnapshotAcademicContext AcademicContext { get; set; } = new();
    public ReportSnapshotPeriod Period { get; set; } = new();
    public ReportSnapshotAttendanceSummary AttendanceSummary { get; set; } = new();
    public ReportSnapshotTicketSummary TicketSummary { get; set; } = new();
    public ReportSnapshotRuntimeSummary RuntimeSummary { get; set; } = new();
    public ReportSnapshotLearningProgress LearningProgress { get; set; } = new();
    public ReportSnapshotAssessmentSummary AssessmentSummary { get; set; } = new();
    public ReportSnapshotTeacherEvaluation TeacherEvaluation { get; set; } = new();
    public List<string> Strengths { get; set; } = [];
    public List<string> Weaknesses { get; set; } = [];
    public List<string> Risks { get; set; } = [];
    public List<string> Recommendations { get; set; } = [];
    public string ParentMessage { get; set; } = string.Empty;
    public string InternalNotes { get; set; } = string.Empty;
}

public sealed class ReportSnapshotStudent
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Branch { get; set; } = string.Empty;
    public string Class { get; set; } = string.Empty;
}

public sealed class ReportSnapshotAcademicContext
{
    public string Program { get; set; } = string.Empty;
    public string Level { get; set; } = string.Empty;
    public string Module { get; set; } = string.Empty;
    public string Syllabus { get; set; } = string.Empty;
    public string SyllabusVersion { get; set; } = string.Empty;
}

public sealed class ReportSnapshotPeriod
{
    public string From { get; set; } = string.Empty;
    public string To { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
}

public sealed class ReportSnapshotAttendanceSummary
{
    public int TotalSections { get; set; }
    public int Present { get; set; }
    public int Late { get; set; }
    public int AbsentWithNotice { get; set; }
    public int AbsentWithoutNotice { get; set; }
    public decimal AttendanceRate { get; set; }
}

public sealed class ReportSnapshotTicketSummary
{
    public int Granted { get; set; }
    public int Consumed { get; set; }
    public int Remaining { get; set; }
    public bool PackageExpiring { get; set; }
}

public sealed class ReportSnapshotRuntimeSummary
{
    public int NormalSections { get; set; }
    public int ReviewSections { get; set; }
    public int MakeupSections { get; set; }
    public int RemedialSections { get; set; }
    public int AssessmentSections { get; set; }
}

public sealed class ReportSnapshotLearningProgress
{
    public decimal CompletionPercent { get; set; }
    public string CurrentStatus { get; set; } = string.Empty;
    public string PromotionStatus { get; set; } = string.Empty;
    public string CurrentLevel { get; set; } = string.Empty;
    public string CurrentModule { get; set; } = string.Empty;
    public string CurrentLesson { get; set; } = string.Empty;
}

public sealed class ReportSnapshotAssessmentSummary
{
    public decimal? LatestScore { get; set; }
    public string LatestResult { get; set; } = string.Empty;
    public string TeacherComment { get; set; } = string.Empty;
}

public sealed class ReportSnapshotTeacherEvaluation
{
    public int? Speaking { get; set; }
    public int? Listening { get; set; }
    public int? Reading { get; set; }
    public int? Writing { get; set; }
    public int? Participation { get; set; }
    public int? Confidence { get; set; }
    public string Notes { get; set; } = string.Empty;
}
