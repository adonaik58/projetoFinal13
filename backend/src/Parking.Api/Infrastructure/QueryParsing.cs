using System.Globalization;
using FluentValidation;
using FluentValidation.Results;

namespace Parking.Api.Infrastructure;

/// <summary>Strict, case-insensitive parsing of query-string values with field-level validation errors.</summary>
internal static class QueryParsing
{
    public static TEnum? Enum<TEnum>(string? value, string field)
        where TEnum : struct, System.Enum
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        return System.Enum.TryParse<TEnum>(value.Trim(), ignoreCase: true, out var parsed)
               && System.Enum.IsDefined(parsed)
               && !int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out _)
            ? parsed
            : throw Invalid(field, $"Valor inválido. Valores aceites: {string.Join(", ", System.Enum.GetNames<TEnum>())}.");
    }

    public static DateTime? Instant(string? value, string field)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        return DateTimeOffset.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out var parsed)
            ? parsed.UtcDateTime
            : throw Invalid(field, "Data inválida (use ISO-8601, ex.: 2026-11-07T00:00:00Z).");
    }

    private static ValidationException Invalid(string field, string message) => new([new ValidationFailure(field, message)]);
}
