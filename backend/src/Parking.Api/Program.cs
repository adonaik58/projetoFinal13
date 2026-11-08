using Parking.Api.Endpoints;
using Parking.Api.Infrastructure;
using Parking.Application;
using Parking.Infrastructure;
using Parking.Infrastructure.Persistence;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.ConfigureKestrel(options =>
{
    options.AddServerHeader = false;
    options.Limits.MaxRequestBodySize = 1024 * 1024;
});

builder.Services.AddSerilog((services, logger) => logger
    .ReadFrom.Configuration(builder.Configuration)
    .ReadFrom.Services(services)
    .Enrich.FromLogContext()
    .Enrich.WithMachineName()
    .Enrich.WithProperty("Application", "parking-api"));

builder.Services
    .AddApiServices(builder.Configuration)
    .AddApplication()
    .AddInfrastructure(builder.Configuration);

var app = builder.Build();

// One-shot mode used by the "migrator" container: apply migrations + bootstrap data, then exit.
if (args.Contains("--migrate"))
{
    try
    {
        await using var scope = app.Services.CreateAsyncScope();
        await scope.ServiceProvider.GetRequiredService<DatabaseInitializer>().InitializeAsync(CancellationToken.None);
        Log.Information("Database initialization completed");
        return 0;
    }
#pragma warning disable CA1031 // Any failure must be logged and turned into a non-zero exit code.
    catch (Exception exception)
#pragma warning restore CA1031
    {
        app.Services.GetRequiredService<ILogger<Program>>().LogCritical(exception, "Database initialization failed");
        return 1;
    }
}

if (app.Configuration.GetValue<bool>("Database:MigrateOnStartup"))
{
    await using var scope = app.Services.CreateAsyncScope();
    await scope.ServiceProvider.GetRequiredService<DatabaseInitializer>().InitializeAsync(CancellationToken.None);
}

app.UseMiddleware<CorrelationIdMiddleware>();
app.UseForwardedHeaders();
app.UseSerilogRequestLogging(options =>
{
    options.GetLevel = (context, _, exception) =>
        exception is not null || context.Response.StatusCode >= 500 ? Serilog.Events.LogEventLevel.Error
        : context.Request.Path.StartsWithSegments("/health") ? Serilog.Events.LogEventLevel.Verbose
        : Serilog.Events.LogEventLevel.Information;
    options.EnrichDiagnosticContext = (diagnostics, context) =>
    {
        diagnostics.Set("ClientIp", context.Connection.RemoteIpAddress?.ToString());
        diagnostics.Set("UserId", context.User.FindFirst("sub")?.Value);
    };
});
app.UseExceptionHandler();
app.UseStatusCodePages();

app.Use(async (context, next) =>
{
    // API responses carry personal data: never cache them, never sniff them.
    context.Response.OnStarting(() =>
    {
        context.Response.Headers.XContentTypeOptions = "nosniff";
        if (context.Request.Path.StartsWithSegments("/api"))
        {
            context.Response.Headers.CacheControl = "no-store";
        }

        return Task.CompletedTask;
    });
    await next(context);
});

var openApiEnabled = app.Configuration.GetValue<bool>("OpenApi:Enabled");
if (openApiEnabled)
{
    // Static UI files only (no API data): must run before authorization, whose fallback policy
    // also applies to requests that match no endpoint. Calls made from the UI still need a Bearer token.
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/openapi/v1.json", "Xyami.Park API v1");
        options.RoutePrefix = "swagger";
        options.DocumentTitle = "Xyami.Park API";
    });
}

app.UseRouting();
app.UseCors(ServiceRegistration.CorsPolicy);
app.UseAuthentication();
app.UseMiddleware<DistributedRateLimitMiddleware>();
app.UseAuthorization();

if (openApiEnabled)
{
    app.MapOpenApi().AllowAnonymous();
}

app.MapHealthChecks("/health/live", new() { Predicate = _ => false, ResponseWriter = ServiceRegistration.WriteHealthResponse }).AllowAnonymous();
app.MapHealthChecks("/health/ready", new() { Predicate = check => check.Tags.Contains("ready"), ResponseWriter = ServiceRegistration.WriteHealthResponse }).AllowAnonymous();

var api = app.MapGroup("/api/v1");
api.MapAuthEndpoints();
api.MapUserEndpoints();
api.MapSpaceEndpoints();
api.MapTicketEndpoints();
api.MapCatalogEndpoints();
api.MapSettingsEndpoints();
api.MapDashboardEndpoints();
api.MapAuditEndpoints();

await app.RunAsync();
return 0;

/// <summary>Entry point; public for WebApplicationFactory-based tests.</summary>
public partial class Program;
