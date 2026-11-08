using Parking.Domain.Users;

namespace Parking.Application.Abstractions;

/// <summary>Identity and request context of the caller.</summary>
public interface ICurrentUser
{
    int? UserId { get; }

    string? Username { get; }

    Role? Role { get; }

    string? IpAddress { get; }

    string? CorrelationId { get; }

    /// <summary>The authenticated user id; throws when the request is anonymous.</summary>
    int RequiredUserId { get; }
}

public enum PasswordVerification
{
    Failed,
    Success,
    SuccessRehashNeeded,
}

public interface IPasswordHasher
{
    string Hash(string password);

    PasswordVerification Verify(string hash, string password);
}

public sealed record AccessToken(string Token, DateTime ExpiresAt);

public sealed record GeneratedRefreshToken(string Token, string Hash);

public interface ITokenService
{
    TimeSpan RefreshTokenLifetime { get; }

    AccessToken CreateAccessToken(User user);

    GeneratedRefreshToken GenerateRefreshToken();

    string HashRefreshToken(string token);
}

/// <summary>Per-username brute-force protection (distributed).</summary>
public interface ILoginThrottle
{
    /// <summary>Remaining lockout, or <c>null</c> when the username may attempt a login.</summary>
    Task<TimeSpan?> GetLockoutAsync(string username, CancellationToken cancellationToken);

    Task RegisterFailureAsync(string username, CancellationToken cancellationToken);

    Task ResetAsync(string username, CancellationToken cancellationToken);
}

/// <summary>Adds audit entries to the current unit of work (persisted on the next save).</summary>
public interface IAuditLogger
{
    void Record(string action, string? entityType = null, string? entityId = null, object? details = null);

    void RecordFor(int? userId, string? username, string action, string? entityType = null, string? entityId = null, object? details = null);
}

/// <summary>Business-time helpers in the configured parking time zone.</summary>
public interface IBusinessClock
{
    DateTime UtcNow { get; }

    DateOnly Today { get; }

    TimeZoneInfo TimeZone { get; }

    DateTime StartOfDayUtc(DateOnly day);

    /// <summary>Offset of the business time zone from UTC at the given instant.</summary>
    TimeSpan OffsetAt(DateTime utc);
}

public sealed record CacheEntrySettings(TimeSpan Expiration, TimeSpan? LocalExpiration = null);

/// <summary>
/// Distributed cache with stampede protection. Local (in-process) caching is opt-in through
/// <see cref="CacheEntrySettings.LocalExpiration"/> so that read-after-write stays consistent across replicas.
/// </summary>
public interface ICacheService
{
    ValueTask<T> GetOrCreateAsync<T>(
        string key, Func<CancellationToken, ValueTask<T>> factory, CacheEntrySettings settings, CancellationToken cancellationToken);

    ValueTask RemoveAsync(IEnumerable<string> keys, CancellationToken cancellationToken);
}

public sealed record DailySeriesRow(DateOnly Date, decimal Revenue, int Entries, int Exits);

/// <summary>Aggregations that need database-specific SQL.</summary>
public interface IReportingQueries
{
    Task<IReadOnlyList<DailySeriesRow>> GetDailySeriesAsync(
        DateTime fromUtc, DateTime toUtc, TimeSpan businessOffset, CancellationToken cancellationToken);
}

/// <summary>Data returned by the external identity-card (BI) registry; only what the check-in form needs.</summary>
public sealed record NationalIdRecord(string FullName, DateOnly? BirthDate);

/// <summary>External BI registry used to pre-fill the check-in of customers not yet known to the system.</summary>
public interface INationalIdRegistry
{
    /// <returns>The holder's data, or <c>null</c> when not found, disabled or temporarily unavailable.</returns>
    Task<NationalIdRecord?> FindAsync(string nationalId, CancellationToken cancellationToken);
}
