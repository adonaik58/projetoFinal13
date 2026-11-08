using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace Parking.IntegrationTests;

internal static class TestHelpers
{
    private static int _sequence = Random.Shared.Next(100, 900);

    public static int Next() => Interlocked.Increment(ref _sequence);

    public static string NationalId() => $"{Next():D9}LA{Next() % 1000:D3}";

    public static string Plate() => $"LD-{Next():D4}-AB";

    public static async Task<JsonElement> ReadJsonAsync(this HttpResponseMessage response) =>
        await response.Content.ReadFromJsonAsync<JsonElement>(ParkingApiFactory.Json);

    public static async Task<JsonElement> ExpectAsync(this Task<HttpResponseMessage> request, HttpStatusCode status)
    {
        using var response = await request;
        var body = await response.Content.ReadAsStringAsync();
        Assert.True(response.StatusCode == status, $"Expected {(int)status} but got {(int)response.StatusCode}: {body}");
        return body.Length == 0 ? default : JsonDocument.Parse(body).RootElement.Clone();
    }

    public static async Task<(int BrandId, int ModelId)> CreateBrandWithModelAsync(this HttpClient admin)
    {
        var brand = await admin.PostAsJsonAsync("api/v1/catalog/brands", new { name = $"Marca {Next()}" }).ExpectAsync(HttpStatusCode.Created);
        var brandId = brand.GetProperty("id").GetInt32();
        var model = await admin.PostAsJsonAsync($"api/v1/catalog/brands/{brandId}/models", new { name = "Modelo X" }).ExpectAsync(HttpStatusCode.Created);
        return (brandId, model.GetProperty("id").GetInt32());
    }

    public static async Task<int> CreateSpaceAsync(this HttpClient admin)
    {
        var section = new string((char)('A' + (Next() % 26)), 2);
        var space = await admin.PostAsJsonAsync("api/v1/spaces", new { code = $"{section}{Next()}" }).ExpectAsync(HttpStatusCode.Created);
        return space.GetProperty("id").GetInt32();
    }

    public static object CheckIn(int spaceId, int brandId, int? modelId, string? nationalId = null, string? plate = null) => new
    {
        spaceId,
        customer = new { fullName = "Cliente Teste", nationalId = nationalId ?? NationalId(), birthDate = "1990-05-20" },
        vehicle = new { plate = plate ?? Plate(), brandId, modelId, color = "#3366FF" },
    };
}
