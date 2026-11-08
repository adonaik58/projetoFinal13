using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Time.Testing;
using StackExchange.Redis;
using Testcontainers.MySql;
using Testcontainers.Redis;

namespace Parking.IntegrationTests;

/// <summary>
/// Runs the real API (all middleware, EF Core migrations, Redis) against disposable MySQL 8.4 and Redis
/// containers. Time is controlled through <see cref="FakeTimeProvider"/> so pricing and expiry are deterministic.
/// </summary>
public sealed class ParkingApiFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    public const string AdminUsername = "admin";
    public const string AdminPassword = "Admin12345";

    public static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web) { Converters = { new JsonStringEnumConverter() } };

    private readonly MySqlContainer _mysql = new MySqlBuilder("mysql:8.4")
        .WithDatabase("parking")
        .WithUsername("parking")
        .WithPassword("parking_test_password")
        .Build();

    private readonly RedisContainer _redis = new RedisBuilder("redis:7.4-alpine").Build();

    public FakeTimeProvider Clock { get; } = new(DateTimeOffset.UtcNow);

    public string MySqlConnectionString => _mysql.GetConnectionString();

    public async Task InitializeAsync()
    {
        await Task.WhenAll(_mysql.StartAsync(), _redis.StartAsync());
        _ = Services; // build the host: runs migrations + bootstrap (Database:MigrateOnStartup)
    }

    public new async Task DisposeAsync()
    {
        await base.DisposeAsync();
        await Task.WhenAll(_mysql.DisposeAsync().AsTask(), _redis.DisposeAsync().AsTask());
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.UseSetting("ConnectionStrings:Database", _mysql.GetConnectionString());
        builder.UseSetting("ConnectionStrings:Redis", _redis.GetConnectionString());
        builder.UseSetting("Jwt:SigningKey", "integration-tests-signing-key-0123456789abcdef");
        builder.UseSetting("Auth:RefreshCookieSecure", "false");
        builder.UseSetting("Bootstrap:AdminUsername", AdminUsername);
        builder.UseSetting("Bootstrap:AdminPassword", AdminPassword);
        builder.UseSetting("Bootstrap:RatePerMinute", "10");
        builder.UseSetting("Bootstrap:FreeHours", "1");
        builder.UseSetting("Bootstrap:MaxSpaces", "1000");
        builder.UseSetting("Database:MigrateOnStartup", "true");
        builder.UseSetting("OpenApi:Enabled", "true");
        builder.UseSetting("RateLimiting:Policies:auth:PermitLimit", "10000");
        builder.UseSetting("Serilog:MinimumLevel:Default", "Warning");
        builder.ConfigureTestServices(services =>
        {
            services.RemoveAll<TimeProvider>();
            services.AddSingleton<TimeProvider>(Clock);
        });
    }

    public HttpClient CreateApiClient()
    {
        var client = CreateClient(new WebApplicationFactoryClientOptions { HandleCookies = true, BaseAddress = new Uri("http://localhost/") });
        client.DefaultRequestHeaders.Add("X-Requested-With", "XMLHttpRequest");
        return client;
    }

    public async Task<HttpClient> LoginAsync(string username = AdminUsername, string password = AdminPassword)
    {
        var client = CreateApiClient();
        var response = await client.PostAsJsonAsync("api/v1/auth/login", new { username, password });
        response.EnsureSuccessStatusCode();
        var auth = await response.Content.ReadFromJsonAsync<JsonElement>(Json);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", auth.GetProperty("accessToken").GetString());
        return client;
    }

    public async Task FlushRedisAsync(string pattern)
    {
        await using var redis = await ConnectionMultiplexer.ConnectAsync(_redis.GetConnectionString() + ",allowAdmin=true");
        var server = redis.GetServers()[0];
        var db = redis.GetDatabase();
        await foreach (var key in server.KeysAsync(pattern: pattern))
        {
            await db.KeyDeleteAsync(key);
        }
    }
}

[CollectionDefinition(Name)]
public sealed class ApiCollection : ICollectionFixture<ParkingApiFactory>
{
    public const string Name = "api";
}
