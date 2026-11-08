using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Parking.Application.Abstractions;
using Parking.Domain.Users;

namespace Parking.Infrastructure.Time;

public sealed class BusinessOptions
{
    public const string SectionName = "Business";

    /// <summary>IANA time zone used for "today" and daily aggregations.</summary>
    public string TimeZone { get; set; } = "Africa/Luanda";

    /// <summary>Used only when the time zone database is unavailable in the runtime image.</summary>
    public int FallbackUtcOffsetMinutes { get; set; } = 60;
}

internal sealed class BusinessClock : IBusinessClock
{
    private readonly TimeProvider _timeProvider;

    public BusinessClock(TimeProvider timeProvider, IOptions<BusinessOptions> options, ILogger<BusinessClock> logger)
    {
        _timeProvider = timeProvider;
        var o = options.Value;
        try
        {
            TimeZone = TimeZoneInfo.FindSystemTimeZoneById(o.TimeZone);
        }
        catch (TimeZoneNotFoundException)
        {
            logger.LogWarning("Time zone {TimeZone} not found; using fixed offset {Offset} min", o.TimeZone, o.FallbackUtcOffsetMinutes);
            TimeZone = TimeZoneInfo.CreateCustomTimeZone(o.TimeZone, TimeSpan.FromMinutes(o.FallbackUtcOffsetMinutes), o.TimeZone, o.TimeZone);
        }
    }

    public TimeZoneInfo TimeZone { get; }

    public DateTime UtcNow => _timeProvider.GetUtcNow().UtcDateTime;

    public DateOnly Today => DateOnly.FromDateTime(TimeZoneInfo.ConvertTimeFromUtc(UtcNow, TimeZone));

    public DateTime StartOfDayUtc(DateOnly day) =>
        TimeZoneInfo.ConvertTimeToUtc(day.ToDateTime(TimeOnly.MinValue, DateTimeKind.Unspecified), TimeZone);

    public TimeSpan OffsetAt(DateTime utc) => TimeZone.GetUtcOffset(utc);
}

/// <summary>Caller identity for non-HTTP execution (migrator, background jobs).</summary>
internal sealed class SystemCurrentUser : ICurrentUser
{
    public int? UserId => null;

    public string? Username => "system";

    public Role? Role => null;

    public string? IpAddress => null;

    public string? CorrelationId => null;

    public int RequiredUserId => throw new InvalidOperationException("No authenticated user in the current context.");
}
