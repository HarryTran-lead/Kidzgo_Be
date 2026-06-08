using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Kidzgo.API.Requests;

internal static class SyllabusVersionParser
{
    internal const string ValidationMessage =
        "Syllabus version must be a positive integer like 1. Legacy input like 'v1' is also accepted.";

    internal static bool TryParse(string? rawValue, out int version)
    {
        version = 0;

        if (string.IsNullOrWhiteSpace(rawValue))
        {
            return false;
        }

        var normalized = rawValue.Trim();
        if (normalized.StartsWith("v", StringComparison.OrdinalIgnoreCase))
        {
            normalized = normalized[1..];
        }

        return int.TryParse(normalized, NumberStyles.None, CultureInfo.InvariantCulture, out version) &&
               version > 0;
    }

    internal static string BuildValidationMessage(string? rawValue)
    {
        if (string.IsNullOrWhiteSpace(rawValue))
        {
            return ValidationMessage;
        }

        return $"The value '{rawValue}' is not valid for syllabus version. {ValidationMessage}";
    }
}

public sealed class SyllabusVersionJsonConverter : JsonConverter<int>
{
    public override int Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.Number &&
            reader.TryGetInt32(out var numericVersion) &&
            numericVersion > 0)
        {
            return numericVersion;
        }

        if (reader.TokenType == JsonTokenType.String)
        {
            var rawValue = reader.GetString();
            if (SyllabusVersionParser.TryParse(rawValue, out var parsedVersion))
            {
                return parsedVersion;
            }

            throw new JsonException(SyllabusVersionParser.BuildValidationMessage(rawValue));
        }

        throw new JsonException(SyllabusVersionParser.ValidationMessage);
    }

    public override void Write(Utf8JsonWriter writer, int value, JsonSerializerOptions options)
        => writer.WriteNumberValue(value);
}

public sealed class NullableSyllabusVersionJsonConverter : JsonConverter<int?>
{
    public override int? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.Null)
        {
            return null;
        }

        if (reader.TokenType == JsonTokenType.Number &&
            reader.TryGetInt32(out var numericVersion) &&
            numericVersion > 0)
        {
            return numericVersion;
        }

        if (reader.TokenType == JsonTokenType.String)
        {
            var rawValue = reader.GetString();
            if (string.IsNullOrWhiteSpace(rawValue))
            {
                return null;
            }

            if (SyllabusVersionParser.TryParse(rawValue, out var parsedVersion))
            {
                return parsedVersion;
            }

            throw new JsonException(SyllabusVersionParser.BuildValidationMessage(rawValue));
        }

        throw new JsonException(SyllabusVersionParser.ValidationMessage);
    }

    public override void Write(Utf8JsonWriter writer, int? value, JsonSerializerOptions options)
    {
        if (value.HasValue)
        {
            writer.WriteNumberValue(value.Value);
            return;
        }

        writer.WriteNullValue();
    }
}
