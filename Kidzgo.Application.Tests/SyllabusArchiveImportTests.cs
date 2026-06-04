using Kidzgo.Application.LessonPlanTemplates.Shared;
using Kidzgo.Application.Syllabuses.ImportCurriculumArchive;
using Kidzgo.Application.Syllabuses.Shared;
using Kidzgo.Domain.LessonPlans;
using Xunit;

namespace Kidzgo.Application.Tests;

public sealed class SyllabusArchiveImportTests
{
    [Fact]
    public void SyllabusEntryPriority_prefers_excel_in_ppct_folder()
    {
        var excelPath = "PPCT GET STARTER/get_ready_for_starters_import_ready.xlsx";
        var docxPath = "PPCT GET STARTER/The Syllabus of Get Ready for Starters full (1).docx";

        var excelPriority = CurriculumArchiveImportEntryRules.GetSyllabusEntryPriority(excelPath);
        var docxPriority = CurriculumArchiveImportEntryRules.GetSyllabusEntryPriority(docxPath);

        Assert.True(CurriculumArchiveImportEntryRules.IsSyllabusEntry(excelPath));
        Assert.True(CurriculumArchiveImportEntryRules.IsSyllabusEntry(docxPath));
        Assert.True(excelPriority > docxPriority);
        Assert.Equal("SyllabusDocument", CurriculumArchiveImportEntryRules.ResolveSourceType(excelPath));
    }

    [Fact]
    public void SupportedEntryFilter_ignores_word_temporary_files()
    {
        Assert.False(CurriculumArchiveImportEntryRules.IsSupportedImportEntry("PPCT/~$temp.docx"));
        Assert.False(CurriculumArchiveImportEntryRules.IsSupportedImportEntry("UNIT 1/~$lesson 1.docx"));
        Assert.True(CurriculumArchiveImportEntryRules.IsSupportedImportEntry("PPCT/syllabus.xlsx"));
    }

    [Fact]
    public void ParserVersion_returns_excel_for_excel_files()
    {
        Assert.Equal("excel-v1", SyllabusImportFileMetadata.ResolveParserVersion("syllabus.xlsx"));
        Assert.Equal("excel-v1", SyllabusImportFileMetadata.ResolveParserVersion("syllabus.xls"));
        Assert.Equal("docx-v1", SyllabusImportFileMetadata.ResolveParserVersion("syllabus.docx"));
    }

    [Fact]
    public void BuildFromParsedImport_keeps_all_curriculum_rows_and_summary_consistent()
    {
        var lessons = Enumerable.Range(1, 6)
            .Select(index => new ParsedSyllabusLesson(
                OrderIndex: index,
                PeriodFrom: index,
                PeriodTo: index,
                Topic: index <= 3 ? "Unit 1" : "Unit 2",
                LessonNumber: index <= 3 ? index : index - 3,
                ContentSummary: $"Content {index}",
                StructureSummary: $"Grammar {index}",
                Components: null,
                StudentBookPages: $"SB {index}",
                TeacherBookPages: $"TB {index}",
                ModuleHint: index <= 3 ? "UNIT 1" : "UNIT 2"))
            .ToList();

        var parsed = new ParsedSyllabusDocument(
            Title: "GET READY FOR STARTERS",
            Edition: "Second edition",
            Overview: "Overview",
            OverallObjectives: null,
            SpecificObjectives: null,
            EthicsAndAttitudes: null,
            BookOverview: null,
            MinutesPerPeriod: 45,
            Units:
            [
                new ParsedSyllabusUnit("Unit 1", 1, 3, 3, null, "UNIT 1"),
                new ParsedSyllabusUnit("Unit 2", 2, 3, 3, null, "UNIT 2")
            ],
            Lessons: lessons,
            Resources: [],
            RawText: "raw");

        var (sections, _, totalPeriods, totalLessons) = SyllabusDocumentMapper.BuildFromParsedImport(parsed);
        var curriculum = sections.Single(x => x.Type == SyllabusDocumentSectionTypes.Table);

        Assert.NotNull(curriculum.Table);
        Assert.Equal(6, curriculum.Table!.Rows.Count);
        Assert.Equal(6, totalLessons);
        Assert.Equal(6, totalPeriods);

        var summary = SyllabusDocumentMapper.ComputeSummaryFromSections(sections);
        Assert.Equal(6, summary.TotalLessons);
        Assert.Equal(6, summary.TotalSessions);
        Assert.Equal(2, summary.TotalUnits);
        Assert.Equal(6, summary.TotalPeriods);
    }

    [Fact]
    public void LessonPlanUnitNameNormalizer_treats_unit_0_as_starter_alias()
    {
        var identity = LessonPlanUnitNameNormalizer.ExtractUnitIdentity("unit 0 lesson 1.docx");

        Assert.NotNull(identity);
        Assert.Equal("UNIT|STARTER", identity!.NormalizedKey);
        Assert.Equal(0, identity.UnitNumber);
        Assert.Equal("UNIT STARTER", identity.CanonicalDisplayName);
    }

    [Fact]
    public void CurriculumImportRuleResolver_maps_unit_0_to_starter_rule()
    {
        var starterModuleId = Guid.NewGuid();
        var regularModuleId = Guid.NewGuid();
        var rules = new List<CurriculumImportModuleRule>
        {
            new()
            {
                ModuleId = starterModuleId,
                IncludeStarterUnit = true,
                UnitFrom = 1,
                UnitTo = 2,
                OrderIndex = 1
            },
            new()
            {
                ModuleId = regularModuleId,
                IncludeStarterUnit = false,
                UnitFrom = 3,
                UnitTo = 5,
                OrderIndex = 2
            }
        };
        var configuration = new CurriculumImportConfiguration
        {
            StarterUnitLessonPlanCount = 3,
            RegularUnitLessonPlanCount = 4,
            RevisionLessonPlanCount = 2
        };

        var rule = CurriculumImportRuleResolver.ResolveRule(rules, "unit 0 lesson 2");
        var sessionIndex = CurriculumImportRuleResolver.ResolveSessionIndex(configuration, rule!, "unit 0 lesson 2");

        Assert.NotNull(rule);
        Assert.Equal(starterModuleId, rule!.ModuleId);
        Assert.Equal(2, sessionIndex);
    }
}
