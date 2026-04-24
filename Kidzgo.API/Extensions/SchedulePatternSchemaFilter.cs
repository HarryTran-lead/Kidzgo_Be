using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Kidzgo.API.Extensions;

public sealed class SchedulePatternSchemaFilter : ISchemaFilter
{
    private static readonly IOpenApiAny WeeklyPatternExample = new OpenApiArray
    {
        new OpenApiObject
        {
            ["dayOfWeeks"] = new OpenApiArray
            {
                new OpenApiString("TU"),
                new OpenApiString("TH")
            },
            ["startTime"] = new OpenApiString("18:00")
        }
    };

    private const string WeeklyPatternDescription =
        "Optional subset of the class schedule for this student. " +
        "Group days with the same startTime into one entry. " +
        "Use multiple entries when the student attends different time slots in the same week. " +
        "Leave empty to attend all sessions of the class.";

    public void Apply(OpenApiSchema schema, SchemaFilterContext context)
    {
        if (schema?.Properties is not { Count: > 0 })
        {
            return;
        }

        if (schema.Properties.TryGetValue("weeklyPattern", out var weeklyPatternProperty) ||
            schema.Properties.TryGetValue("WeeklyPattern", out weeklyPatternProperty))
        {
            weeklyPatternProperty.Example ??= WeeklyPatternExample;
            weeklyPatternProperty.Description ??= WeeklyPatternDescription;
        }
    }
}
