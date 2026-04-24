using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Kidzgo.API.Extensions;

public sealed class SchedulePatternSchemaFilter : ISchemaFilter
{
    private const string SessionSelectionExample = "FREQ=WEEKLY;BYDAY=WE;BYHOUR=8;BYMINUTE=30";
    private const string SessionSelectionDescription =
        "Optional subset of the class schedule for this student. " +
        "Example: if the class runs Wednesday 08:30, use FREQ=WEEKLY;BYDAY=WE;BYHOUR=8;BYMINUTE=30. " +
        "The selection pattern must match both weekday and time of the class slot. " +
        "Leave empty to attend all sessions of the class.";

    public void Apply(OpenApiSchema schema, SchemaFilterContext context)
    {
        if (schema?.Properties is not { Count: > 0 })
        {
            return;
        }

        if (schema.Properties.TryGetValue("sessionSelectionPattern", out var sessionSelectionProperty) ||
            schema.Properties.TryGetValue("SessionSelectionPattern", out sessionSelectionProperty))
        {
            sessionSelectionProperty.Example ??= new OpenApiString(SessionSelectionExample);
            sessionSelectionProperty.Description ??= SessionSelectionDescription;
        }
    }
}
