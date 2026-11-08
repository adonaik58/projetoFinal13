using System.Net;
using System.Net.Http.Json;
using Microsoft.Extensions.DependencyInjection;
using Parking.Infrastructure.Persistence;

namespace Parking.IntegrationTests;

[Collection(ApiCollection.Name)]
public sealed class AuthTests(ParkingApiFactory factory)
{
    [Fact]
    public async Task Readiness_reports_mysql_and_redis()
    {
        var health = await factory.CreateApiClient().GetAsync("health/ready").ExpectAsync(HttpStatusCode.OK);

        Assert.Equal("Healthy", health.GetProperty("status").GetString());
        Assert.Equal("Healthy", health.GetProperty("checks").GetProperty("mysql").GetProperty("status").GetString());
        Assert.Equal("Healthy", health.GetProperty("checks").GetProperty("redis").GetProperty("status").GetString());
    }

    [Fact]
    public async Task Login_returns_an_access_token_and_an_http_only_refresh_cookie()
    {
        using var response = await factory.CreateApiClient().PostAsJsonAsync("api/v1/auth/login",
            new { username = ParkingApiFactory.AdminUsername, password = ParkingApiFactory.AdminPassword });

        response.EnsureSuccessStatusCode();
        var cookie = Assert.Single(response.Headers.GetValues("Set-Cookie"));
        Assert.Contains("parking_rt=", cookie, StringComparison.Ordinal);
        Assert.Contains("httponly", cookie, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("samesite=strict", cookie, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("path=/api/v1/auth", cookie, StringComparison.OrdinalIgnoreCase);

        var body = await response.ReadJsonAsync();
        Assert.False(body.TryGetProperty("refreshToken", out _));
        Assert.Equal("Admin", body.GetProperty("user").GetProperty("role").GetString());
        Assert.Contains("users.manage", body.GetProperty("user").GetProperty("permissions").EnumerateArray().Select(p => p.GetString()));
    }

    [Fact]
    public async Task Wrong_credentials_are_rejected_with_problem_details_and_audited()
    {
        var problem = await factory.CreateApiClient()
            .PostAsJsonAsync("api/v1/auth/login", new { username = ParkingApiFactory.AdminUsername, password = "wrong-password1" })
            .ExpectAsync(HttpStatusCode.Unauthorized);

        Assert.Equal("auth.invalid_credentials", problem.GetProperty("code").GetString());
        Assert.True(problem.TryGetProperty("correlationId", out _));

        var admin = await factory.LoginAsync();
        var audit = await admin.GetAsync("api/v1/audit-logs?action=auth.login.failed").ExpectAsync(HttpStatusCode.OK);
        Assert.True(audit.GetProperty("totalCount").GetInt32() >= 1);
    }

    [Fact]
    public async Task Repeated_failures_lock_the_username()
    {
        var client = factory.CreateApiClient();
        var username = $"ghost{TestHelpers.Next()}";
        for (var i = 0; i < 5; i++)
        {
            await client.PostAsJsonAsync("api/v1/auth/login", new { username, password = "Nope12345" }).ExpectAsync(HttpStatusCode.Unauthorized);
        }

        using var locked = await client.PostAsJsonAsync("api/v1/auth/login", new { username, password = "Nope12345" });
        Assert.Equal(HttpStatusCode.TooManyRequests, locked.StatusCode);
        Assert.NotNull(locked.Headers.RetryAfter);
    }

    [Fact]
    public async Task Requests_without_a_token_get_401_problem_details()
    {
        using var response = await factory.CreateApiClient().GetAsync("api/v1/spaces");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
    }

    [Fact]
    public async Task Refresh_rotates_the_token_and_reuse_after_the_grace_period_revokes_the_family()
    {
        var client = await factory.LoginAsync();
        var original = client.DefaultRequestHeaders.Authorization;

        // Capture the first refresh cookie so it can be replayed later (as a thief would).
        using var first = await client.PostAsync("api/v1/auth/refresh", null);
        first.EnsureSuccessStatusCode();
        var stolen = CookieValue(first);

        await client.PostAsync("api/v1/auth/refresh", null).ExpectAsync(HttpStatusCode.OK);

        factory.Clock.Advance(TimeSpan.FromSeconds(31));
        var thief = factory.CreateApiClient();
        thief.DefaultRequestHeaders.Add("Cookie", $"parking_rt={stolen}");
        var reuse = await thief.PostAsync("api/v1/auth/refresh", null).ExpectAsync(HttpStatusCode.Unauthorized);
        Assert.Equal("auth.session_expired", reuse.GetProperty("code").GetString());

        // The legitimate session belonged to the same family and is now revoked too.
        await client.PostAsync("api/v1/auth/refresh", null).ExpectAsync(HttpStatusCode.Unauthorized);
        Assert.NotNull(original);
    }

    [Fact]
    public async Task Refresh_requires_the_anti_csrf_header()
    {
        var client = await factory.LoginAsync();
        client.DefaultRequestHeaders.Remove("X-Requested-With");

        var problem = await client.PostAsync("api/v1/auth/refresh", null).ExpectAsync(HttpStatusCode.Forbidden);
        Assert.Equal("csrf.header_missing", problem.GetProperty("code").GetString());
    }

    [Fact]
    public async Task Logout_revokes_the_refresh_session()
    {
        var client = await factory.LoginAsync();

        await client.PostAsync("api/v1/auth/logout", null).ExpectAsync(HttpStatusCode.NoContent);

        await client.PostAsync("api/v1/auth/refresh", null).ExpectAsync(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Legacy_md5_passwords_are_accepted_once_and_upgraded()
    {
        var username = $"legacy{TestHelpers.Next()}";
        using (var scope = factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            // md5("1234") as stored by the legacy PHP system
            db.Users.Add(Domain.Users.User.Create(username, "Utilizador Legado", "md5:81dc9bdb52d04dc20036dbd8313ed055", Domain.Users.Role.Operator));
            await db.SaveChangesAsync();
        }

        await factory.LoginAsync(username, "1234");

        using var verify = factory.Services.CreateScope();
        var user = verify.ServiceProvider.GetRequiredService<AppDbContext>().Users.Single(u => u.Username == username);
        Assert.False(user.HasLegacyPassword);
        Assert.StartsWith("AQAAAA", user.PasswordHash, StringComparison.Ordinal);
    }

    [Fact]
    public async Task Correlation_id_from_the_proxy_is_echoed()
    {
        var client = factory.CreateApiClient();
        client.DefaultRequestHeaders.Add("X-Correlation-ID", "req-12345");

        using var response = await client.GetAsync("health/live");

        Assert.Equal("req-12345", response.Headers.GetValues("X-Correlation-ID").Single());
    }

    [Fact]
    public async Task OpenApi_document_describes_the_api_with_bearer_security()
    {
        var document = await factory.CreateApiClient().GetAsync("openapi/v1.json").ExpectAsync(HttpStatusCode.OK);

        Assert.True(document.GetProperty("paths").TryGetProperty("/api/v1/tickets", out _));
        Assert.True(document.GetProperty("components").GetProperty("securitySchemes").TryGetProperty("Bearer", out _));
    }

    [Fact]
    public async Task Swagger_ui_is_served_without_authentication_when_enabled()
    {
        using var response = await factory.CreateApiClient().GetAsync("swagger/index.html");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("swagger-ui", await response.Content.ReadAsStringAsync(), StringComparison.Ordinal);
    }

    private static string CookieValue(HttpResponseMessage response)
    {
        var header = response.Headers.GetValues("Set-Cookie").Single(h => h.StartsWith("parking_rt=", StringComparison.Ordinal));
        return header["parking_rt=".Length..header.IndexOf(';', StringComparison.Ordinal)];
    }
}
