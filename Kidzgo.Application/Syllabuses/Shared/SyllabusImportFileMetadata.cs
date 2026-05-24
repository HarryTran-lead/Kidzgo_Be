namespace Kidzgo.Application.Syllabuses.Shared;

internal static class SyllabusImportFileMetadata
{
    public static string ResolveParserVersion(string fileName)
    {
        var extension = Path.GetExtension(fileName);

        return extension.ToLowerInvariant() switch
        {
            ".xlsx" => "excel-v1",
            ".xls" => "excel-v1",
            ".pdf" => "pdf-v1",
            _ => "docx-v1"
        };
    }
}
