using System.Text.Json;

namespace Kidzgo.Application.ReportsV3.Shared;

internal static class ReportJson
{
    public static readonly JsonSerializerOptions SnapshotOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
        PropertyNameCaseInsensitive = true,
        WriteIndented = false
    };
}
