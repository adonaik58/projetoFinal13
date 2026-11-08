using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Net;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Parking.Application.Abstractions;
using Parking.Domain.Customers;

namespace Parking.Infrastructure.Identity;

public sealed class NationalIdRegistryOptions
{
    public const string SectionName = "NationalIdRegistry";

    public bool Enabled { get; set; } = true;

    /// <summary>Base URL of the BI lookup service; the BI number is appended as <c>consultar/{bi}</c>.</summary>
    [Required]
    [Url]
    public string BaseUrl { get; set; } = "https://consulta.edgarsingui.ao/";

    /// <summary>Kept short: the operator is waiting at the gate and can always type the data by hand.</summary>
    [Range(1, 30)]
    public int TimeoutSeconds { get; set; } = 6;
}

/// <summary>
/// Client for the external BI lookup service. Failures never block a check-in: anything other than a usable answer
/// (disabled, not found, timeout, unexpected payload) yields <c>null</c> and the operator fills the form manually.
/// </summary>
internal sealed class NationalIdRegistryClient(
    HttpClient http, IOptions<NationalIdRegistryOptions> options, ILogger<NationalIdRegistryClient> logger) : INationalIdRegistry
{
    public async Task<NationalIdRecord?> FindAsync(string nationalId, CancellationToken cancellationToken)
    {
        if (!options.Value.Enabled)
        {
            return null;
        }

        try
        {
            using var response = await http.GetAsync(new Uri($"consultar/{Uri.EscapeDataString(nationalId)}", UriKind.Relative), cancellationToken);
            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                return null;
            }

            if (!response.IsSuccessStatusCode)
            {
                logger.LogWarning("BI registry answered {StatusCode}; check-in continues with manual data", (int)response.StatusCode);
                return null;
            }

            await using var body = await response.Content.ReadAsStreamAsync(cancellationToken);
            using var document = await JsonDocument.ParseAsync(body, cancellationToken: cancellationToken);
            var record = NationalIdRecordParser.Parse(document.RootElement);
            if (record is null)
            {
                logger.LogInformation("BI registry returned no usable holder data");
            }

            return record;
        }
        catch (Exception exception) when (exception is HttpRequestException or JsonException
            || (exception is TaskCanceledException && !cancellationToken.IsCancellationRequested))
        {
            logger.LogWarning(exception, "BI registry unavailable; check-in continues with manual data");
            return null;
        }
    }
}

/// <summary>
/// Extracts the holder's name and birth date from the registry's JSON. The service publishes no schema, so fields are
/// matched by the usual Portuguese/English names (case, '_' and '-' ignored), searching the shallowest levels first.
/// </summary>
internal static class NationalIdRecordParser
{
    private static readonly string[] FullNameKeys = ["nomecompleto", "fullname", "nome", "name"];
    private static readonly string[] FirstNameKeys = ["primeironome", "firstname"];
    private static readonly string[] LastNameKeys = ["ultimonome", "apelido", "sobrenome", "lastname", "surname"];
    private static readonly string[] BirthDateKeys =
        ["datanascimento", "datadenascimento", "datanasc", "nascimento", "birthdate", "dateofbirth", "dob"];

    private static readonly string[] DateFormats = ["yyyy-MM-dd", "dd/MM/yyyy", "dd-MM-yyyy", "yyyy/MM/dd", "dd.MM.yyyy"];
    private static readonly string[] NameParticles = ["da", "das", "de", "do", "dos", "e"];

    public static NationalIdRecord? Parse(JsonElement root)
    {
        if (root.ValueKind != JsonValueKind.Object || IsFailure(root))
        {
            return null;
        }

        var fullName = FindString(root, FullNameKeys);
        if (string.IsNullOrWhiteSpace(fullName))
        {
            var first = FindString(root, FirstNameKeys);
            var last = FindString(root, LastNameKeys);
            fullName = string.Join(' ', new[] { first, last }.Where(part => !string.IsNullOrWhiteSpace(part)));
        }

        fullName = NormalizeName(fullName);
        return fullName.Length < 2 ? null : new NationalIdRecord(fullName, ParseDate(FindString(root, BirthDateKeys)));
    }

    private static bool IsFailure(JsonElement root) =>
        IsFlag(root, "error", JsonValueKind.True) || IsFlag(root, "erro", JsonValueKind.True)
        || IsFlag(root, "success", JsonValueKind.False) || IsFlag(root, "status", JsonValueKind.False);

    private static bool IsFlag(JsonElement element, string name, JsonValueKind kind) =>
        element.EnumerateObject().Any(p => NormalizeKey(p.Name) == name && p.Value.ValueKind == kind);

    private static string? FindString(JsonElement root, string[] keys)
    {
        foreach (var key in keys)
        {
            var queue = new Queue<JsonElement>([root]);
            while (queue.TryDequeue(out var current))
            {
                foreach (var property in current.EnumerateObject())
                {
                    if (NormalizeKey(property.Name) == key && property.Value.ValueKind == JsonValueKind.String
                        && !string.IsNullOrWhiteSpace(property.Value.GetString()))
                    {
                        return property.Value.GetString();
                    }

                    if (property.Value.ValueKind == JsonValueKind.Object)
                    {
                        queue.Enqueue(property.Value);
                    }
                    else if (property.Value.ValueKind == JsonValueKind.Array
                        && property.Value.GetArrayLength() > 0 && property.Value[0].ValueKind == JsonValueKind.Object)
                    {
                        queue.Enqueue(property.Value[0]);
                    }
                }
            }
        }

        return null;
    }

    private static string NormalizeKey(string key) =>
        string.Concat(key.Where(c => c is not ('_' or '-' or ' '))).ToLowerInvariant();

    /// <summary>Registries usually answer in capitals: "MARIA DA SILVA" → "Maria da Silva".</summary>
    private static string NormalizeName(string? name)
    {
        var words = (name ?? string.Empty).Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        var normalized = string.Join(' ', words);
        if (normalized.Any(char.IsLower))
        {
            return Truncate(normalized);
        }

        var culture = CultureInfo.GetCultureInfo("pt-PT");
        var titled = words.Select((word, index) =>
        {
            var lower = word.ToLower(culture);
            return index > 0 && NameParticles.Contains(lower) ? lower : culture.TextInfo.ToTitleCase(lower);
        });
        return Truncate(string.Join(' ', titled));
    }

    private static string Truncate(string value) =>
        value.Length <= Customer.FullNameMaxLength ? value : value[..Customer.FullNameMaxLength].TrimEnd();

    private static DateOnly? ParseDate(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        value = value.Trim();
        if (DateOnly.TryParseExact(value, DateFormats, CultureInfo.InvariantCulture, DateTimeStyles.None, out var date))
        {
            return date;
        }

        return DateTime.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out var dateTime)
            ? DateOnly.FromDateTime(dateTime)
            : null;
    }
}
