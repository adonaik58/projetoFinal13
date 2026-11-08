namespace Parking.Domain.Tickets;

/// <summary>
/// Single pricing rule for the whole system: the first <c>freeMinutes</c> are free; once that period
/// is reached every elapsed whole minute beyond it is charged at <c>ratePerMinute</c>.
/// </summary>
public static class PricingPolicy
{
    public static int ElapsedMinutes(DateTime entryAt, DateTime until) =>
        until <= entryAt ? 0 : (int)Math.Floor((until - entryAt).TotalMinutes);

    public static decimal Calculate(int elapsedMinutes, decimal ratePerMinute, int freeMinutes)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(elapsedMinutes);
        ArgumentOutOfRangeException.ThrowIfNegative(ratePerMinute);
        ArgumentOutOfRangeException.ThrowIfNegative(freeMinutes);

        return elapsedMinutes < freeMinutes
            ? 0m
            : decimal.Round((elapsedMinutes - freeMinutes) * ratePerMinute, 2, MidpointRounding.AwayFromZero);
    }
}
