using Parking.Domain.Common;
using Parking.Domain.Spaces;
using Parking.Domain.Tickets;

namespace Parking.UnitTests.Domain;

public sealed class TicketTests
{
    [Fact]
    public void Open_occupies_the_space_and_snapshots_the_tariff()
    {
        var space = TestData.Space();

        var ticket = Ticket.Open("ABC123", space, TestData.Customer(), TestData.Vehicle(), 1, 10m, 60, TestData.Now);

        Assert.Equal(SpaceStatus.Occupied, space.Status);
        Assert.Equal(TicketStatus.Open, ticket.Status);
        Assert.Equal(10m, ticket.RatePerMinute);
        Assert.Equal(60, ticket.FreeMinutes);
        Assert.Equal(1, space.Version);
    }

    [Fact]
    public void Open_fails_when_the_space_is_not_free()
    {
        var space = TestData.Space();
        space.SetMaintenance();

        var error = Assert.Throws<DomainException>(() =>
            Ticket.Open("ABC123", space, TestData.Customer(), TestData.Vehicle(), 1, 10m, 0, TestData.Now));
        Assert.Equal("ticket.space_not_free", error.Code);
    }

    [Fact]
    public void Close_prices_the_stay_releases_the_space_and_cannot_be_repeated()
    {
        var space = TestData.Space();
        var ticket = Ticket.Open("ABC123", space, TestData.Customer(), TestData.Vehicle(), 1, 10m, 60, TestData.Now);

        ticket.Close(2, TestData.Now.AddMinutes(90).AddSeconds(30));

        Assert.Equal(TicketStatus.Closed, ticket.Status);
        Assert.Equal(90, ticket.DurationMinutes);
        Assert.Equal(300m, ticket.Amount); // (90 - 60) × 10
        Assert.Equal(2, ticket.ClosedByUserId);
        Assert.Equal(SpaceStatus.Free, space.Status);
        Assert.Equal("ticket.already_closed", Assert.Throws<DomainException>(() => ticket.Close(2, TestData.Now.AddHours(3))).Code);
    }

    [Fact]
    public void AmountAt_reports_the_running_amount_of_an_open_ticket()
    {
        var ticket = Ticket.Open("ABC123", TestData.Space(), TestData.Customer(), TestData.Vehicle(), 1, 5m, 0, TestData.Now);

        Assert.Equal(50m, ticket.AmountAt(TestData.Now.AddMinutes(10)));
        Assert.Equal(10, ticket.ElapsedMinutes(TestData.Now.AddMinutes(10)));
    }

    [Fact]
    public void Ticket_codes_use_an_unambiguous_alphabet()
    {
        var codes = Enumerable.Range(0, 200).Select(_ => TicketCode.New()).ToList();

        Assert.All(codes, code => Assert.Matches("^[0-9A-HJKMNP-TV-Z]{10}$", code));
        Assert.Equal(codes.Count, codes.Distinct().Count());
    }
}
