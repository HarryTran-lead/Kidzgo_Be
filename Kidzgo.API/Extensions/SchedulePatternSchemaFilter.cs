using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using Kidzgo.Application.Abstraction.Services;

namespace Kidzgo.API.Extensions;

public sealed class SchedulePatternSchemaFilter : ISchemaFilter
{
    private static readonly OpenApiObject WeeklyScheduleSlotObjectExample = new()
    {
        ["dayOfWeek"] = new OpenApiString("MO"),
        ["startTime"] = new OpenApiString("18:00"),
        ["durationMinutes"] = new OpenApiInteger(90)
    };

    private static readonly IOpenApiAny WeeklyScheduleSlotsExample = new OpenApiArray
    {
        WeeklyScheduleSlotObjectExample,
        new OpenApiObject
        {
            ["dayOfWeek"] = new OpenApiString("WE"),
            ["startTime"] = new OpenApiString("18:00"),
            ["durationMinutes"] = new OpenApiInteger(90)
        }
    };

    private static readonly IOpenApiAny WeeklyPatternExample = new OpenApiArray
    {
        new OpenApiObject
        {
            ["dayOfWeeks"] = new OpenApiArray
            {
                new OpenApiString("TU"),
                new OpenApiString("TH")
            },
            ["startTime"] = new OpenApiString("18:00"),
            ["durationMinutes"] = new OpenApiInteger(90)
        }
    };

    private const string WeeklyScheduleSlotsDescription =
        "Class weekly schedule. " +
        "Send one item per study slot. " +
        "Use multiple items when the class has different start times in the same week.";

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

        if (context.Type == typeof(ScheduleSlot))
        {
            schema.Example ??= WeeklyScheduleSlotObjectExample;

            if (schema.Properties.TryGetValue("dayOfWeek", out var dayOfWeekProperty))
            {
                dayOfWeekProperty.Example ??= new OpenApiString("MO");
                dayOfWeekProperty.Description ??= "Day code: MO, TU, WE, TH, FR, SA, or SU.";
            }

            if (schema.Properties.TryGetValue("startTime", out var startTimeProperty))
            {
                startTimeProperty.Example ??= new OpenApiString("18:00");
                startTimeProperty.Description ??= "Class start time in HH:mm format.";
            }

            if (schema.Properties.TryGetValue("durationMinutes", out var durationProperty))
            {
                durationProperty.Example ??= new OpenApiInteger(90);
                durationProperty.Description ??= "Duration of this study slot in minutes.";
            }
        }

        if (context.Type == typeof(WeeklyPatternEntry))
        {
            schema.Example ??= new OpenApiObject
            {
                ["dayOfWeeks"] = new OpenApiArray
                {
                    new OpenApiString("TU"),
                    new OpenApiString("TH")
                },
                ["startTime"] = new OpenApiString("18:00"),
                ["durationMinutes"] = new OpenApiInteger(90)
            };

            if (schema.Properties.TryGetValue("dayOfWeeks", out var dayOfWeeksProperty))
            {
                dayOfWeeksProperty.Example ??= new OpenApiArray
                {
                    new OpenApiString("TU"),
                    new OpenApiString("TH")
                };
                dayOfWeeksProperty.Description ??= "One or more day codes sharing the same startTime.";
            }

            if (schema.Properties.TryGetValue("startTime", out var startTimeProperty))
            {
                startTimeProperty.Example ??= new OpenApiString("18:00");
                startTimeProperty.Description ??= "Class start time in HH:mm format.";
            }

            if (schema.Properties.TryGetValue("durationMinutes", out var durationProperty))
            {
                durationProperty.Example ??= new OpenApiInteger(90);
                durationProperty.Description ??= "Duration of this study slot in minutes.";
            }
        }

        if (schema.Properties.TryGetValue("weeklyScheduleSlots", out var weeklyScheduleSlotsProperty) ||
            schema.Properties.TryGetValue("WeeklyScheduleSlots", out weeklyScheduleSlotsProperty))
        {
            weeklyScheduleSlotsProperty.Example ??= WeeklyScheduleSlotsExample;
            weeklyScheduleSlotsProperty.Description ??= WeeklyScheduleSlotsDescription;
        }

        if (schema.Properties.TryGetValue("weeklyPattern", out var weeklyPatternProperty) ||
            schema.Properties.TryGetValue("WeeklyPattern", out weeklyPatternProperty))
        {
            weeklyPatternProperty.Example ??= WeeklyPatternExample;
            weeklyPatternProperty.Description ??= WeeklyPatternDescription;
        }
    }
}
