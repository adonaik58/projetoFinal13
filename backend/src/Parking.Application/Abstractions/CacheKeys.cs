using System.Globalization;

namespace Parking.Application.Abstractions;

public static class CacheKeys
{
    public const string Settings = "settings:current";
    public const string Brands = "catalog:brands";
    public const string SpaceBoard = "spaces:board";

    public static readonly IReadOnlyList<string> DashboardPeriods = ["today", "7d", "30d", "all"];
    public static readonly IReadOnlyList<int> SeriesDays = [7, 30, 90];

    public static string Models(int brandId) => string.Create(CultureInfo.InvariantCulture, $"catalog:brands:{brandId}:models");

    public static string DashboardSummary(string period) => $"dashboard:summary:{period}";

    public static string RevenueSeries(int days) => string.Create(CultureInfo.InvariantCulture, $"dashboard:series:{days}");

    /// <summary>Everything derived from tickets/spaces: invalidated after check-in, check-out and space changes.</summary>
    public static IEnumerable<string> OccupancyDependent() =>
        [SpaceBoard, .. DashboardPeriods.Select(DashboardSummary), .. SeriesDays.Select(RevenueSeries)];
}
