using FluentValidation;
using System.Text.Json;

namespace Kidzgo.Application.ReportsV3.ReportTemplates.UpdateReportTemplate;

public sealed class UpdateReportTemplateCommandValidator : AbstractValidator<UpdateReportTemplateCommand>
{
    public UpdateReportTemplateCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty();

        RuleFor(x => x.Code)
            .NotEmpty()
            .Must(code => !string.IsNullOrWhiteSpace(code))
            .WithMessage("Code is required.")
            .MaximumLength(50);

        RuleFor(x => x.Name)
            .NotEmpty()
            .Must(name => !string.IsNullOrWhiteSpace(name))
            .WithMessage("Name is required.")
            .MaximumLength(200);

        RuleFor(x => x.Type)
            .IsInEnum();

        RuleFor(x => x.ContentSchema)
            .Must(BeValidJsonObject)
            .When(x => !string.IsNullOrWhiteSpace(x.ContentSchema))
            .WithMessage("ContentSchema must be a valid JSON object.");
    }

    private static bool BeValidJsonObject(string? json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return true;
        }

        try
        {
            using var document = JsonDocument.Parse(json);
            return document.RootElement.ValueKind == JsonValueKind.Object;
        }
        catch
        {
            return false;
        }
    }
}
