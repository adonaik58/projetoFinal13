using Microsoft.EntityFrameworkCore;
using Parking.Application.Abstractions;

namespace Parking.Infrastructure.Persistence;

internal sealed class ReportingQueries(AppDbContext db) : IReportingQueries
{
    public async Task<IReadOnlyList<DailySeriesRow>> GetDailySeriesAsync(
        DateTime fromUtc, DateTime toUtc, TimeSpan businessOffset, CancellationToken cancellationToken)
    {
        // Days are bucketed in the business time zone (fixed offset over the requested window). All values are parameters.
        var offsetMinutes = (int)businessOffset.TotalMinutes;

        var exits = await db.Database.SqlQuery<DayAggregate>($"""
            SELECT DATE(DATE_ADD(exit_at, INTERVAL {offsetMinutes} MINUTE)) AS Day, COUNT(*) AS Count, COALESCE(SUM(amount), 0) AS Total
            FROM tickets
            WHERE status = 'Closed' AND exit_at >= {fromUtc} AND exit_at <= {toUtc}
            GROUP BY Day
            """).ToListAsync(cancellationToken);

        var entries = await db.Database.SqlQuery<DayAggregate>($"""
            SELECT DATE(DATE_ADD(entry_at, INTERVAL {offsetMinutes} MINUTE)) AS Day, COUNT(*) AS Count, 0 AS Total
            FROM tickets
            WHERE entry_at >= {fromUtc} AND entry_at <= {toUtc}
            GROUP BY Day
            """).ToListAsync(cancellationToken);

        var exitsByDay = exits.ToDictionary(e => DateOnly.FromDateTime(e.Day));
        var entriesByDay = entries.ToDictionary(e => DateOnly.FromDateTime(e.Day));
        return [.. exitsByDay.Keys.Union(entriesByDay.Keys).Order().Select(day => new DailySeriesRow(
            day,
            exitsByDay.TryGetValue(day, out var exit) ? exit.Total : 0m,
            entriesByDay.TryGetValue(day, out var entry) ? (int)entry.Count : 0,
            exit is null ? 0 : (int)exit.Count))];
    }

    internal sealed class DayAggregate
    {
        public DateTime Day { get; set; }

        public long Count { get; set; }

        public decimal Total { get; set; }
    }
}
