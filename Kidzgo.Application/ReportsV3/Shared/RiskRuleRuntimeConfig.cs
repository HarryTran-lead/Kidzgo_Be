using System.Text.Json;
using System.Globalization;
using Kidzgo.Domain.Reports;

namespace Kidzgo.Application.ReportsV3.Shared;

internal sealed class RiskRuleRuntimeConfig
{
    private readonly IReadOnlyDictionary<string, JsonElement> _parameters;

    private RiskRuleRuntimeConfig(
        RiskType riskType,
        int score,
        bool isActive,
        string parametersJson,
        IReadOnlyDictionary<string, JsonElement> parameters)
    {
        RiskType = riskType;
        Score = score;
        IsActive = isActive;
        ParametersJson = parametersJson;
        _parameters = parameters;
    }

    public RiskType RiskType { get; }
    public int Score { get; }
    public bool IsActive { get; }
    public string ParametersJson { get; }

    public static IReadOnlyDictionary<RiskType, RiskRuleRuntimeConfig> Build(
        IReadOnlyCollection<RiskRuleConfig> configuredRules)
    {
        var configuredByType = configuredRules
            .GroupBy(x => x.RiskType)
            .ToDictionary(x => x.Key, x => x.OrderByDescending(y => y.UpdatedAt).First());

        var result = new Dictionary<RiskType, RiskRuleRuntimeConfig>();
        foreach (var riskType in Enum.GetValues<RiskType>())
        {
            configuredByType.TryGetValue(riskType, out var configured);

            var score = configured?.Score ?? RiskRuleDefaults.GetDefaultScore(riskType);
            var isActive = configured?.IsActive ?? true;
            var parametersJson = configured?.ParametersJson;
            if (string.IsNullOrWhiteSpace(parametersJson))
            {
                parametersJson = RiskRuleDefaults.GetDefaultParametersJson(riskType);
            }

            result[riskType] = new RiskRuleRuntimeConfig(
                riskType,
                score,
                isActive,
                parametersJson,
                ParseParameters(parametersJson));
        }

        return result;
    }

    public int GetInt(string key, int defaultValue)
    {
        if (!_parameters.TryGetValue(key, out var value))
        {
            return defaultValue;
        }

        if (value.ValueKind == JsonValueKind.Number && value.TryGetInt32(out var intValue))
        {
            return intValue;
        }

        if (value.ValueKind == JsonValueKind.String &&
            int.TryParse(
                value.GetString(),
                NumberStyles.Integer,
                CultureInfo.InvariantCulture,
                out var parsedInt))
        {
            return parsedInt;
        }

        return defaultValue;
    }

    public decimal GetDecimal(string key, decimal defaultValue)
    {
        if (!_parameters.TryGetValue(key, out var value))
        {
            return defaultValue;
        }

        if (value.ValueKind == JsonValueKind.Number && value.TryGetDecimal(out var decimalValue))
        {
            return decimalValue;
        }

        if (value.ValueKind == JsonValueKind.String &&
            decimal.TryParse(
                value.GetString(),
                NumberStyles.Number,
                CultureInfo.InvariantCulture,
                out var parsedDecimal))
        {
            return parsedDecimal;
        }

        return defaultValue;
    }

    public string GetString(string key, string defaultValue)
    {
        if (!_parameters.TryGetValue(key, out var value))
        {
            return defaultValue;
        }

        if (value.ValueKind == JsonValueKind.String)
        {
            return value.GetString() ?? defaultValue;
        }

        return value.ToString();
    }

    private static IReadOnlyDictionary<string, JsonElement> ParseParameters(string parametersJson)
    {
        if (string.IsNullOrWhiteSpace(parametersJson))
        {
            return new Dictionary<string, JsonElement>(StringComparer.OrdinalIgnoreCase);
        }

        try
        {
            using var document = JsonDocument.Parse(parametersJson);
            if (document.RootElement.ValueKind != JsonValueKind.Object)
            {
                return new Dictionary<string, JsonElement>(StringComparer.OrdinalIgnoreCase);
            }

            var dictionary = new Dictionary<string, JsonElement>(StringComparer.OrdinalIgnoreCase);
            foreach (var property in document.RootElement.EnumerateObject())
            {
                dictionary[property.Name] = property.Value.Clone();
            }

            return dictionary;
        }
        catch
        {
            return new Dictionary<string, JsonElement>(StringComparer.OrdinalIgnoreCase);
        }
    }
}
