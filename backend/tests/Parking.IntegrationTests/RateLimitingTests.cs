using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Hosting;

namespace Parking.IntegrationTests;

[Collection(ApiCollection.Name)]
public sealed class RateLimitingTests(ParkingApiFactory factory)
{
    [Fact]
    public async Task Auth_endpoints_are_limited_per_client_with_standard_headers()
    {
        await factory.FlushRedisAsync("parking:ratelimit:*");
        using var limited = factory.WithWebHostBuilder(b => b.UseSetting("RateLimiting:Policies:auth:PermitLimit", "3"));
        var client = limited.CreateClient();

        for (var i = 0; i < 3; i++)
        {
            using var allowed = await client.PostAsJsonAsync("api/v1/auth/login", new { username = "nobody", password = "Nobody123" });
            Assert.Equal(HttpStatusCode.Unauthorized, allowed.StatusCode);
            Assert.Equal("3", allowed.Headers.GetValues("RateLimit-Limit").Single());
        }

        using var blocked = await client.PostAsJsonAsync("api/v1/auth/login", new { username = "nobody", password = "Nobody123" });
        Assert.Equal(HttpStatusCode.TooManyRequests, blocked.StatusCode);
        Assert.Equal("0", blocked.Headers.GetValues("RateLimit-Remaining").Single());
        Assert.NotNull(blocked.Headers.RetryAfter);

        await factory.FlushRedisAsync("parking:ratelimit:*");
        await factory.FlushRedisAsync("parking:auth:failures:*");
    }
}
