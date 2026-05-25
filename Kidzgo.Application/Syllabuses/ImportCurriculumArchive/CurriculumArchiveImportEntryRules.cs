using System.Text.RegularExpressions;

namespace Kidzgo.Application.Syllabuses.ImportCurriculumArchive;

internal static class CurriculumArchiveImportEntryRules
{
    public static bool IsSyllabusEntry(string fullName)
    {
        var normalized = fullName.Replace('\\', '/');
        var fileName = Path.GetFileNameWithoutExtension(normalized);
        var extension = Path.GetExtension(normalized);

        return normalized.Contains("PPCT", StringComparison.OrdinalIgnoreCase) &&
               (extension.Equals(".docx", StringComparison.OrdinalIgnoreCase) ||
                extension.Equals(".xlsx", StringComparison.OrdinalIgnoreCase) ||
                extension.Equals(".xls", StringComparison.OrdinalIgnoreCase)) &&
               (fileName.Contains("syllabus", StringComparison.OrdinalIgnoreCase) ||
                fileName.Contains("curriculum", StringComparison.OrdinalIgnoreCase) ||
                fileName.Contains("ppct", StringComparison.OrdinalIgnoreCase) ||
                fileName.Contains("import_ready", StringComparison.OrdinalIgnoreCase));
    }

    public static int GetSyllabusEntryPriority(string fullName)
    {
        var fileName = Path.GetFileNameWithoutExtension(fullName);
        var extension = Path.GetExtension(fullName);
        var normalized = fullName.Replace('\\', '/');
        var priority = 0;

        if (normalized.Contains("/PPCT", StringComparison.OrdinalIgnoreCase))
        {
            priority += 1_000;
        }

        if (extension.Equals(".xlsx", StringComparison.OrdinalIgnoreCase) ||
            extension.Equals(".xls", StringComparison.OrdinalIgnoreCase))
        {
            priority += 500;
        }

        if (fileName.Contains("full", StringComparison.OrdinalIgnoreCase))
        {
            priority += 100;
        }

        if (fileName.Contains("the syllabus", StringComparison.OrdinalIgnoreCase))
        {
            priority += 50;
        }

        if (fileName.Contains("course syllabus", StringComparison.OrdinalIgnoreCase))
        {
            priority += 10;
        }

        if (fileName.Contains("syllabus", StringComparison.OrdinalIgnoreCase))
        {
            priority += 5;
        }

        return priority;
    }

    public static bool IsSupportedImportEntry(string fullName)
    {
        var fileName = Path.GetFileName(fullName);
        if (fileName.StartsWith("~$", StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        var extension = Path.GetExtension(fileName);
        return extension.Equals(".docx", StringComparison.OrdinalIgnoreCase) ||
               extension.Equals(".xlsx", StringComparison.OrdinalIgnoreCase) ||
               extension.Equals(".xls", StringComparison.OrdinalIgnoreCase);
    }

    public static string ResolveSourceType(string fullName)
    {
        var normalized = NormalizeArchivePath(fullName);

        if (IsSyllabusEntry(normalized))
        {
            return "SyllabusDocument";
        }

        if (Regex.IsMatch(normalized, @"(?:^|/)\s*REVISION(?:\s|/|$)", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant))
        {
            return "RevisionLesson";
        }

        return "UnitLesson";
    }

    public static int GetLessonEntryPriority(string fullName)
    {
        var normalized = NormalizeArchivePath(fullName);
        var fileName = Path.GetFileNameWithoutExtension(normalized);
        var priority = 0;

        if (Regex.IsMatch(
                fileName,
                @"^Unit\s*starter\s*lesson\s*0*\d+$",
                RegexOptions.IgnoreCase | RegexOptions.CultureInvariant))
        {
            priority += 1_000;
        }
        else if (Regex.IsMatch(
                     fileName,
                     @"^Unit\s*0*\d+\s*lesson\s*0*\d+$",
                     RegexOptions.IgnoreCase | RegexOptions.CultureInvariant))
        {
            priority += 1_000;
        }
        else if (Regex.IsMatch(
                     fileName,
                     @"^Revision\s*0*\d+$",
                     RegexOptions.IgnoreCase | RegexOptions.CultureInvariant))
        {
            priority += 1_000;
        }

        if (!fileName.Contains("done", StringComparison.OrdinalIgnoreCase))
        {
            priority += 100;
        }

        priority -= fileName.Length;
        priority -= normalized.Length / 10;

        return priority;
    }

    public static string NormalizeArchivePath(string value)
    {
        return Regex.Replace(value.Replace('\\', '/'), @"\s+", " ").Trim();
    }
}
