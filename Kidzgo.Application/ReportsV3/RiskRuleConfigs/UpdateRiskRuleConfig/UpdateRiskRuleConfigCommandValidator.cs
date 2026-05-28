using FluentValidation;
using System.Text.Json;

namespace Kidzgo.Application.ReportsV3.RiskRuleConfigs;

public sealed class UpdateRiskRuleConfigCommandValidator : AbstractValidator<UpdateRiskRuleConfigCommand>
{
    public UpdateRiskRuleConfigCommandValidator()
    {
        RuleFor(x => x.RiskType)
            .IsInEnum();

        RuleFor(x => x.Score)
            .InclusiveBetween(0, 100);

        RuleFor(x => x.ParametersJson)
            .Must(BeValidJsonObject)
            .When(x => !string.IsNullOrWhiteSpace(x.ParametersJson))
            .WithMessage("ParametersJson must be a valid JSON object.");
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
