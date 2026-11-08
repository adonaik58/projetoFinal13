using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Parking.Application.Abstractions;
using StackExchange.Redis;

namespace Parking.Infrastructure.Redis;

internal static class RedisScripts
{
    /// <summary>Atomic fixed-window counter: INCR, set expiry on first hit, return count and remaining TTL.</summary>
    public const string IncrementWindow = """
        local count = redis.call('INCR', KEYS[1])
        if count == 1 then redis.call('PEXPIRE', KEYS[1], ARGV[1]) end
        return { count, redis.call('PTTL', KEYS[1]) }
        """;
}

public sealed class LoginThrottleOptions
{
    public const string SectionName = "Security:LoginThrottle";

    public int MaxFailures { get; set; } = 5;

    public TimeSpan Window { get; set; } = TimeSpan.FromMinutes(15);
}

/// <summary>Locks a username for the rest of the window after too many failures (shared by all replicas).</summary>
internal sealed class RedisLoginThrottle(IConnectionMultiplexer redis, IOptions<LoginThrottleOptions> options, ILogger<RedisLoginThrottle> logger)
    : ILoginThrottle
{
    public async Task<TimeSpan?> GetLockoutAsync(string username, CancellationToken cancellationToken)
    {
        try
        {
            var db = redis.GetDatabase();
            var key = Key(username);
            var failures = (long?)await db.StringGetAsync(key);
            if (failures is null || failures < options.Value.MaxFailures)
            {
                return null;
            }

            return await db.KeyTimeToLiveAsync(key) ?? options.Value.Window;
        }
        catch (RedisException exception)
        {
            logger.LogWarning(exception, "Login throttle unavailable; allowing attempt");
            return null;
        }
    }

    public async Task RegisterFailureAsync(string username, CancellationToken cancellationToken)
    {
        try
        {
            await redis.GetDatabase().ScriptEvaluateAsync(
                RedisScripts.IncrementWindow, [Key(username)], [(long)options.Value.Window.TotalMilliseconds]);
        }
        catch (RedisException exception)
        {
            logger.LogWarning(exception, "Login throttle unavailable; failure not recorded");
        }
    }

    public async Task ResetAsync(string username, CancellationToken cancellationToken)
    {
        try
        {
            await redis.GetDatabase().KeyDeleteAsync(Key(username));
        }
        catch (RedisException exception)
        {
            logger.LogWarning(exception, "Login throttle unavailable; reset skipped");
        }
    }

    private static RedisKey Key(string username) => $"parking:auth:failures:{username.Trim().ToLowerInvariant()}";
}

public readonly record struct RateLimitCounter(long Count, TimeSpan ResetAfter);

public interface IRateLimitStore
{
    Task<RateLimitCounter> IncrementAsync(string key, TimeSpan window);
}

internal sealed class RedisRateLimitStore(IConnectionMultiplexer redis) : IRateLimitStore
{
    public async Task<RateLimitCounter> IncrementAsync(string key, TimeSpan window)
    {
        var result = (RedisResult[])(await redis.GetDatabase().ScriptEvaluateAsync(
            RedisScripts.IncrementWindow, [$"parking:ratelimit:{key}"], [(long)window.TotalMilliseconds]))!;
        var ttl = (long)result[1];
        return new RateLimitCounter((long)result[0], TimeSpan.FromMilliseconds(ttl > 0 ? ttl : window.TotalMilliseconds));
    }
}

public sealed record StoredResponse(int StatusCode, string? Body, string? Location);

public enum IdempotencyState
{
    Acquired,
    InProgress,
    Completed,
    FingerprintMismatch,
}

public sealed record IdempotencyBeginResult(IdempotencyState State, StoredResponse? Response = null);

public interface IIdempotencyStore
{
    Task<IdempotencyBeginResult> TryBeginAsync(string key, string fingerprint);

    Task CompleteAsync(string key, string fingerprint, StoredResponse response);

    Task ReleaseAsync(string key);
}

/// <summary>Remembers the outcome of non-idempotent requests per <c>Idempotency-Key</c> for 24 hours.</summary>
internal sealed class RedisIdempotencyStore(IConnectionMultiplexer redis) : IIdempotencyStore
{
    private static readonly TimeSpan ProcessingTtl = TimeSpan.FromSeconds(60);
    private static readonly TimeSpan CompletedTtl = TimeSpan.FromHours(24);

    public async Task<IdempotencyBeginResult> TryBeginAsync(string key, string fingerprint)
    {
        var db = redis.GetDatabase();
        var redisKey = Key(key);
        var processing = JsonSerializer.Serialize(new Entry("processing", fingerprint, null));
        if (await db.StringSetAsync(redisKey, processing, ProcessingTtl, When.NotExists))
        {
            return new IdempotencyBeginResult(IdempotencyState.Acquired);
        }

        var raw = await db.StringGetAsync(redisKey);
        if (raw.IsNullOrEmpty)
        {
            // Expired between SET and GET: one more attempt.
            return await db.StringSetAsync(redisKey, processing, ProcessingTtl, When.NotExists)
                ? new IdempotencyBeginResult(IdempotencyState.Acquired)
                : new IdempotencyBeginResult(IdempotencyState.InProgress);
        }

        var entry = JsonSerializer.Deserialize<Entry>(raw.ToString())!;
        if (entry.Fingerprint != fingerprint)
        {
            return new IdempotencyBeginResult(IdempotencyState.FingerprintMismatch);
        }

        return entry.State == "completed"
            ? new IdempotencyBeginResult(IdempotencyState.Completed, entry.Response)
            : new IdempotencyBeginResult(IdempotencyState.InProgress);
    }

    public Task CompleteAsync(string key, string fingerprint, StoredResponse response) =>
        redis.GetDatabase().StringSetAsync(Key(key), JsonSerializer.Serialize(new Entry("completed", fingerprint, response)), CompletedTtl);

    public Task ReleaseAsync(string key) => redis.GetDatabase().KeyDeleteAsync(Key(key));

    private static RedisKey Key(string key) => $"parking:idempotency:{key}";

    private sealed record Entry(string State, string Fingerprint, StoredResponse? Response);
}
