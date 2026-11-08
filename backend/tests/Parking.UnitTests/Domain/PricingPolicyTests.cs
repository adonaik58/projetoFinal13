using Parking.Domain.Tickets;

namespace Parking.UnitTests.Domain;

public sealed class PricingPolicyTests
{
    [Theory]
    [InlineData(0, 10, 0, 0)]
    [InlineData(1, 10, 0, 10)]
    [InlineData(59, 10, 60, 0)] // inside the free period
    [InlineData(60, 10, 60, 0)] // exactly at the end of the free period
    [InlineData(61, 10, 60, 10)] // first charged minute
    [InlineData(125, 2.5, 60, 162.5)]
    public void Calculate_charges_only_minutes_beyond_the_free_period(int minutes, decimal rate, int freeMinutes, decimal expected) =>
        Assert.Equal(expected, PricingPolicy.Calculate(minutes, rate, freeMinutes));

    [Fact]
    public void ElapsedMinutes_floors_partial_minutes_and_never_goes_negative()
    {
        var entry = new DateTime(2026, 11, 7, 10, 0, 0, DateTimeKind.Utc);

        Assert.Equal(1, PricingPolicy.ElapsedMinutes(entry, entry.AddSeconds(119)));
        Assert.Equal(0, PricingPolicy.ElapsedMinutes(entry, entry.AddMinutes(-5)));
    }

    [Fact]
    public void Calculate_rejects_negative_inputs() =>
        Assert.Throws<ArgumentOutOfRangeException>(() => PricingPolicy.Calculate(-1, 10, 0));
}
