using System.Reflection;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Parking.Application.Abstractions;
using Parking.Application.Common;
using Parking.Domain.Users;
using Parking.Infrastructure.Persistence;
using Parking.Infrastructure.Security;

namespace Parking.Api.Infrastructure;

internal static class ServiceRegistration
{
    public const string CorsPolicy = "frontend";

    public static IServiceCollection AddApiServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentUser, HttpCurrentUser>();

        services.ConfigureHttpJsonOptions(options =>
        {
            options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
            options.SerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.Never;
        });

        services.AddProblemDetails(options => options.CustomizeProblemDetails = context =>
        {
            context.ProblemDetails.Instance ??= $"{context.HttpContext.Request.Method} {context.HttpContext.Request.Path}";
            context.ProblemDetails.Extensions["traceId"] = System.Diagnostics.Activity.Current?.TraceId.ToString() ?? context.HttpContext.TraceIdentifier;
            if (context.HttpContext.Items[CorrelationIdMiddleware.ItemKey] is string correlationId)
            {
                context.ProblemDetails.Extensions["correlationId"] = correlationId;
            }
        });
        services.AddExceptionHandler<GlobalExceptionHandler>();

        services.Configure<AuthCookieOptions>(configuration.GetSection(AuthCookieOptions.SectionName));
        services.Configure<RateLimitingOptions>(configuration.GetSection(RateLimitingOptions.SectionName));

        services.AddSecurity(configuration);
        services.AddReverseProxySupport(configuration);
        services.AddCors(options =>
        {
            var origins = configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [];
            options.AddPolicy(CorsPolicy, policy => policy
                .WithOrigins(origins)
                .AllowAnyHeader()
                .AllowAnyMethod()
                .AllowCredentials()
                .WithExposedHeaders("RateLimit-Limit", "RateLimit-Remaining", "RateLimit-Reset", "Retry-After",
                    CorrelationIdMiddleware.HeaderName, IdempotencyFilter.ReplayedHeader, "Location"));
        });

        services.AddHealthChecks()
            .AddDbContextCheck<AppDbContext>("mysql", tags: ["ready"])
            .AddCheck<RedisHealthCheck>("redis", tags: ["ready"]);

        services.AddOpenApi(options => options.AddDocumentTransformer<OpenApiDocumentTransformer>());
        services.AddTelemetry(configuration);
        return services;
    }

    private static void AddSecurity(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer();
        services.AddOptions<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme)
            .Configure<IOptions<JwtOptions>, TimeProvider>((options, jwt, timeProvider) =>
            {
                options.MapInboundClaims = false;
                var clockSkew = TimeSpan.FromSeconds(30);
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    // Lifetime is checked against the injected TimeProvider (same clock as token issuance).
                    LifetimeValidator = (notBefore, expires, _, _) =>
                    {
                        var now = timeProvider.GetUtcNow().UtcDateTime;
                        return (notBefore is null || notBefore.Value <= now.Add(clockSkew))
                               && expires is not null && expires.Value > now.Subtract(clockSkew);
                    },
                    ValidIssuer = jwt.Value.Issuer,
                    ValidAudience = jwt.Value.Audience,
                    IssuerSigningKey = jwt.Value.CreateSigningKey(),
                    ValidAlgorithms = [SecurityAlgorithms.HmacSha256],
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ClockSkew = clockSkew,
                    NameClaimType = JwtClaimNames.Username,
                    RoleClaimType = JwtClaimNames.Role,
                };
            });

        services.AddAuthorizationBuilder()
            // Secure by default: every endpoint requires an authenticated user unless it opts out explicitly.
            .SetFallbackPolicy(new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build())
            .AddPermissionPolicies();
    }

    /// <summary>One policy per permission, satisfied by the roles granted that permission.</summary>
    private static AuthorizationBuilder AddPermissionPolicies(this AuthorizationBuilder builder)
    {
        foreach (var permission in Permissions.All)
        {
            var roles = Permissions.RolesWith(permission).Select(r => r.ToString()).ToArray();
            builder.AddPolicy(permission, policy => policy.RequireAuthenticatedUser().RequireRole(roles));
        }

        return builder;
    }

    private static void AddReverseProxySupport(this IServiceCollection services, IConfiguration configuration) =>
        services.Configure<ForwardedHeadersOptions>(options =>
        {
            options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
            options.ForwardLimit = 2;
            options.KnownIPNetworks.Clear();
            options.KnownProxies.Clear();
            foreach (var network in configuration.GetSection("ReverseProxy:KnownNetworks").Get<string[]>() ?? [])
            {
                options.KnownIPNetworks.Add(System.Net.IPNetwork.Parse(network));
            }
        });

    private static void AddTelemetry(this IServiceCollection services, IConfiguration configuration)
    {
        var otlpEndpoint = configuration["OTEL_EXPORTER_OTLP_ENDPOINT"];
        var version = Assembly.GetExecutingAssembly().GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion ?? "dev";

        services.AddOpenTelemetry()
            .ConfigureResource(resource => resource.AddService(
                serviceName: configuration["OTEL_SERVICE_NAME"] ?? "parking-api",
                serviceVersion: version,
                serviceInstanceId: Environment.MachineName))
            .WithTracing(tracing =>
            {
                tracing
                    .AddSource(ApplicationDiagnostics.SourceName, "connector-net")
                    .AddAspNetCoreInstrumentation(options => options.Filter = context =>
                        !context.Request.Path.StartsWithSegments("/health"))
                    .AddHttpClientInstrumentation();
                if (!string.IsNullOrWhiteSpace(otlpEndpoint))
                {
                    tracing.AddOtlpExporter();
                }
            })
            .WithMetrics(metrics =>
            {
                metrics
                    .AddAspNetCoreInstrumentation()
                    .AddHttpClientInstrumentation()
                    .AddRuntimeInstrumentation()
                    .AddMeter(ParkingMetrics.MeterName);
                if (!string.IsNullOrWhiteSpace(otlpEndpoint))
                {
                    metrics.AddOtlpExporter();
                }
            });
    }

    public static async Task WriteHealthResponse(HttpContext context, HealthReport report)
    {
        context.Response.ContentType = "application/json; charset=utf-8";
        await context.Response.WriteAsJsonAsync(new
        {
            status = report.Status.ToString(),
            durationMs = report.TotalDuration.TotalMilliseconds,
            checks = report.Entries.ToDictionary(
                e => e.Key,
                e => new { status = e.Value.Status.ToString(), description = e.Value.Description, durationMs = e.Value.Duration.TotalMilliseconds }),
        });
    }
}
