using Microsoft.AspNetCore.Http.HttpResults;

namespace Parking.Api.Infrastructure;

public sealed class AuthCookieOptions
{
    public const string SectionName = "Auth";

    /// <summary>Must be true outside local HTTP development.</summary>
    public bool RefreshCookieSecure { get; set; } = true;
}

/// <summary>The refresh token travels only in this HttpOnly cookie, scoped to the auth endpoints.</summary>
internal static class RefreshTokenCookie
{
    public const string Name = "parking_rt";
    public const string Path = "/api/v1/auth";

    public static string? Read(HttpRequest request) => request.Cookies[Name];

    public static void Write(HttpResponse response, string token, DateTime expiresAt, bool secure) =>
        response.Cookies.Append(Name, token, Options(secure, expiresAt));

    public static void Clear(HttpResponse response, bool secure) => response.Cookies.Delete(Name, Options(secure, null));

    private static CookieOptions Options(bool secure, DateTime? expiresAt) => new()
    {
        HttpOnly = true,
        Secure = secure,
        SameSite = SameSiteMode.Strict,
        Path = Path,
        Expires = expiresAt,
        IsEssential = true,
    };
}

/// <summary>
/// Cookie-authenticated endpoints require a custom header: browsers cannot send it cross-site without a CORS
/// preflight, which (with SameSite=Strict) blocks CSRF.
/// </summary>
internal sealed class RequireAjaxHeaderFilter : IEndpointFilter
{
    public const string HeaderName = "X-Requested-With";
    public const string ExpectedValue = "XMLHttpRequest";

    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        if (!string.Equals(context.HttpContext.Request.Headers[HeaderName], ExpectedValue, StringComparison.Ordinal))
        {
            return TypedResults.Problem(
                detail: $"Cabeçalho {HeaderName} em falta.",
                statusCode: StatusCodes.Status403Forbidden,
                extensions: new Dictionary<string, object?> { ["code"] = "csrf.header_missing" });
        }

        return await next(context);
    }
}

internal sealed class RedisHealthCheck(StackExchange.Redis.IConnectionMultiplexer redis) : Microsoft.Extensions.Diagnostics.HealthChecks.IHealthCheck
{
    public async Task<Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckResult> CheckHealthAsync(
        Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            var latency = await redis.GetDatabase().PingAsync();
            return Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckResult.Healthy($"ping {latency.TotalMilliseconds:F1} ms");
        }
        catch (Exception exception) when (exception is StackExchange.Redis.RedisException or TimeoutException)
        {
            return Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckResult.Unhealthy("Redis unreachable", exception);
        }
    }
}

internal static class ProblemResults
{
    public static ProblemHttpResult Of(int status, string code, string detail) =>
        TypedResults.Problem(detail: detail, statusCode: status, extensions: new Dictionary<string, object?> { ["code"] = code });
}
