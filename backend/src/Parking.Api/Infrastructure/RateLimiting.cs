using System.Globalization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Parking.Infrastructure.Redis;
using Parking.Infrastructure.Security;
using StackExchange.Redis;

namespace Parking.Api.Infrastructure;

public enum RateLimitPartition
{
    User,
    IpAddress,
}

public sealed class RateLimitPolicy
{
    public int PermitLimit { get; set; }

    public TimeSpan Window { get; set; }

    public RateLimitPartition PartitionBy { get; set; }
}

public sealed class RateLimitingOptions
{
    public const string SectionName = "RateLimiting";
    public const string DefaultPolicy = "default";
    public const string AuthPolicy = "auth";

    public bool Enabled { get; set; } = true;

    public Dictionary<string, RateLimitPolicy> Policies { get; set; } = new(StringComparer.OrdinalIgnoreCase)
    {
        [DefaultPolicy] = new() { PermitLimit = 600, Window = TimeSpan.FromMinutes(1), PartitionBy = RateLimitPartition.User },
        [AuthPolicy] = new() { PermitLimit = 10, Window = TimeSpan.FromMinutes(1), PartitionBy = RateLimitPartition.IpAddress },
    };
}

internal sealed record RateLimitPolicyMetadata(string PolicyName);

internal static class RateLimitingEndpointExtensions
{
    public static TBuilder WithDistributedRateLimit<TBuilder>(this TBuilder builder, string policyName)
        where TBuilder : IEndpointConventionBuilder =>
        builder.WithMetadata(new RateLimitPolicyMetadata(policyName));
}

/// <summary>
/// Fixed-window rate limiting shared by every API replica (counters in Redis). Applies the "default" policy
/// to all <c>/api</c> endpoints unless an endpoint names another policy. Fails open if Redis is unavailable.
/// </summary>
internal sealed class DistributedRateLimitMiddleware(
    RequestDelegate next,
    IRateLimitStore store,
    IOptionsMonitor<RateLimitingOptions> options,
    IProblemDetailsService problemDetails,
    ILogger<DistributedRateLimitMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        var settings = options.CurrentValue;
        var endpoint = context.GetEndpoint();
        if (!settings.Enabled || endpoint is null || !context.Request.Path.StartsWithSegments("/api"))
        {
            await next(context);
            return;
        }

        var policyName = endpoint.Metadata.GetMetadata<RateLimitPolicyMetadata>()?.PolicyName ?? RateLimitingOptions.DefaultPolicy;
        if (!settings.Policies.TryGetValue(policyName, out var policy))
        {
            await next(context);
            return;
        }

        var ip = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        var partition = policy.PartitionBy == RateLimitPartition.User
            ? context.User.FindFirst(JwtClaimNames.Subject)?.Value is { } userId ? $"user:{userId}" : $"ip:{ip}"
            : $"ip:{ip}";

        RateLimitCounter counter;
        try
        {
            counter = await store.IncrementAsync($"{policyName}:{partition}", policy.Window);
        }
        catch (RedisException exception)
        {
            logger.LogWarning(exception, "Rate limiter unavailable; request allowed");
            await next(context);
            return;
        }

        var resetSeconds = Math.Max(1, (int)Math.Ceiling(counter.ResetAfter.TotalSeconds)).ToString(CultureInfo.InvariantCulture);
        var limit = policy.PermitLimit.ToString(CultureInfo.InvariantCulture);
        var remaining = Math.Max(0, policy.PermitLimit - counter.Count).ToString(CultureInfo.InvariantCulture);

        // Added when the response starts: the exception handler clears headers set earlier on error responses.
        context.Response.OnStarting(() =>
        {
            context.Response.Headers["RateLimit-Limit"] = limit;
            context.Response.Headers["RateLimit-Remaining"] = remaining;
            context.Response.Headers["RateLimit-Reset"] = resetSeconds;
            return Task.CompletedTask;
        });

        if (counter.Count <= policy.PermitLimit)
        {
            await next(context);
            return;
        }

        logger.LogWarning("Rate limit {Policy} exceeded for {Partition}", policyName, partition);
        context.Response.StatusCode = StatusCodes.Status429TooManyRequests;
        context.Response.Headers.RetryAfter = resetSeconds;
        var problem = new ProblemDetails
        {
            Status = StatusCodes.Status429TooManyRequests,
            Title = "Demasiados pedidos.",
            Detail = "Excedeu o limite de pedidos. Aguarde alguns instantes e tente novamente.",
            Type = "https://httpstatuses.io/429",
        };
        problem.Extensions["code"] = "rate_limit.exceeded";
        await problemDetails.WriteAsync(new ProblemDetailsContext { HttpContext = context, ProblemDetails = problem });
    }
}
