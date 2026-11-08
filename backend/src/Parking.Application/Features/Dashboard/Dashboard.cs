using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Parking.Application.Abstractions;
using Parking.Application.Abstractions.Messaging;
using Parking.Application.Features.Spaces;
using Parking.Domain.Spaces;
using Parking.Domain.Tickets;

namespace Parking.Application.Features.Dashboard;

public sealed record DashboardSummaryDto(
    string Period,
    DateTime? From,
    DateTime To,
    decimal Revenue,
    int Entries,
    int Exits,
    double AverageStayMinutes,
    int OpenTickets,
    SpaceSummaryDto Spaces);

public sealed record RevenuePointDto(DateOnly Date, decimal Revenue, int Entries, int Exits);

public sealed record RevenueSeriesDto(int Days, IReadOnlyList<RevenuePointDto> Points);

public sealed record GetDashboardSummaryQuery(string Period = "today") : IQuery<DashboardSummaryDto>;

internal sealed class GetDashboardSummaryValidator : AbstractValidator<GetDashboardSummaryQuery>
{
    public GetDashboardSummaryValidator() =>
        RuleFor(x => x.Period).Must(p => CacheKeys.DashboardPeriods.Contains(p))
            .WithMessage($"Período inválido. Valores aceites: {string.Join(", ", CacheKeys.DashboardPeriods)}.");
}

internal sealed class GetDashboardSummaryHandler(IAppDbContext db, ICacheService cache, IBusinessClock clock)
    : IQueryHandler<GetDashboardSummaryQuery, DashboardSummaryDto>
{
    private static readonly CacheEntrySettings CacheSettings = new(TimeSpan.FromSeconds(30), TimeSpan.FromSeconds(5));

    public async Task<DashboardSummaryDto> Handle(GetDashboardSummaryQuery request, CancellationToken cancellationToken) =>
        await cache.GetOrCreateAsync(
            CacheKeys.DashboardSummary(request.Period),
            ct => new ValueTask<DashboardSummaryDto>(ComputeAsync(request.Period, ct)),
            CacheSettings,
            cancellationToken);

    private async Task<DashboardSummaryDto> ComputeAsync(string period, CancellationToken cancellationToken)
    {
        var now = clock.UtcNow;
        DateTime? from = period switch
        {
            "today" => clock.StartOfDayUtc(clock.Today),
            "7d" => clock.StartOfDayUtc(clock.Today.AddDays(-6)),
            "30d" => clock.StartOfDayUtc(clock.Today.AddDays(-29)),
            _ => null,
        };

        var closed = db.Tickets.AsNoTracking().Where(t => t.Status == TicketStatus.Closed);
        var entries = db.Tickets.AsNoTracking();
        if (from is { } start)
        {
            closed = closed.Where(t => t.ExitAt >= start);
            entries = entries.Where(t => t.EntryAt >= start);
        }

        var revenue = await closed.SumAsync(t => t.Amount, cancellationToken) ?? 0m;
        var exits = await closed.CountAsync(cancellationToken);
        var averageStay = await closed.AverageAsync(t => (double?)t.DurationMinutes, cancellationToken) ?? 0d;
        var entryCount = await entries.CountAsync(cancellationToken);
        var openTickets = await db.Tickets.CountAsync(t => t.Status == TicketStatus.Open, cancellationToken);
        var byStatus = await db.ParkingSpaces.AsNoTracking()
            .GroupBy(s => s.Status)
            .Select(g => new { Status = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.Status, x => x.Count, cancellationToken);

        var spaces = new SpaceSummaryDto(
            byStatus.Values.Sum(),
            byStatus.GetValueOrDefault(SpaceStatus.Free),
            byStatus.GetValueOrDefault(SpaceStatus.Occupied),
            byStatus.GetValueOrDefault(SpaceStatus.Maintenance));

        return new DashboardSummaryDto(period, from, now, revenue, entryCount, exits, Math.Round(averageStay, 1), openTickets, spaces);
    }
}

public sealed record GetRevenueSeriesQuery(int Days = 30) : IQuery<RevenueSeriesDto>;

internal sealed class GetRevenueSeriesValidator : AbstractValidator<GetRevenueSeriesQuery>
{
    public GetRevenueSeriesValidator() =>
        RuleFor(x => x.Days).Must(d => CacheKeys.SeriesDays.Contains(d))
            .WithMessage($"Intervalo inválido. Valores aceites: {string.Join(", ", CacheKeys.SeriesDays)}.");
}

internal sealed class GetRevenueSeriesHandler(IReportingQueries reporting, ICacheService cache, IBusinessClock clock)
    : IQueryHandler<GetRevenueSeriesQuery, RevenueSeriesDto>
{
    private static readonly CacheEntrySettings CacheSettings = new(TimeSpan.FromSeconds(60), TimeSpan.FromSeconds(10));

    public async Task<RevenueSeriesDto> Handle(GetRevenueSeriesQuery request, CancellationToken cancellationToken) =>
        await cache.GetOrCreateAsync(
            CacheKeys.RevenueSeries(request.Days),
            async ct =>
            {
                var firstDay = clock.Today.AddDays(-(request.Days - 1));
                var now = clock.UtcNow;
                var rows = await reporting.GetDailySeriesAsync(clock.StartOfDayUtc(firstDay), now, clock.OffsetAt(now), ct);
                var byDate = rows.ToDictionary(r => r.Date);
                var points = Enumerable.Range(0, request.Days)
                    .Select(firstDay.AddDays)
                    .Select(day => byDate.TryGetValue(day, out var row)
                        ? new RevenuePointDto(day, row.Revenue, row.Entries, row.Exits)
                        : new RevenuePointDto(day, 0m, 0, 0))
                    .ToList();
                return new RevenueSeriesDto(request.Days, points);
            },
            CacheSettings,
            cancellationToken);
}
