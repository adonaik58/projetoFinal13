using Parking.Domain.Common;
using Parking.Domain.Customers;
using Parking.Domain.Spaces;
using Parking.Domain.Users;
using Parking.Domain.Vehicles;

namespace Parking.Domain.Tickets;

/// <summary>A parking session: opened at check-in, closed (and priced) at check-out.</summary>
public sealed class Ticket : AuditableEntity<long>
{
    public const int CodeMaxLength = 12;

    private Ticket()
    {
    }

    public string Code { get; private set; } = null!;

    public TicketStatus Status { get; private set; }

    public int SpaceId { get; private set; }

    public ParkingSpace Space { get; private set; } = null!;

    public int CustomerId { get; private set; }

    public Customer Customer { get; private set; } = null!;

    public int VehicleId { get; private set; }

    public Vehicle Vehicle { get; private set; } = null!;

    public DateTime EntryAt { get; private set; }

    public DateTime? ExitAt { get; private set; }

    public int? DurationMinutes { get; private set; }

    public decimal? Amount { get; private set; }

    /// <summary>Tariff snapshot taken at entry, so later tariff changes do not alter open tickets.</summary>
    public decimal RatePerMinute { get; private set; }

    public int FreeMinutes { get; private set; }

    public int OpenedByUserId { get; private set; }

    public User OpenedBy { get; private set; } = null!;

    public int? ClosedByUserId { get; private set; }

    public User? ClosedBy { get; private set; }

    public static Ticket Open(
        string code,
        ParkingSpace space,
        Customer customer,
        Vehicle vehicle,
        int openedByUserId,
        decimal ratePerMinute,
        int freeMinutes,
        DateTime now)
    {
        if (ratePerMinute < 0 || freeMinutes < 0)
        {
            throw new DomainException("ticket.invalid_tariff", "Tarifa inválida.");
        }

        space.Occupy();

        return new Ticket
        {
            Code = code,
            Status = TicketStatus.Open,
            Space = space,
            SpaceId = space.Id,
            Customer = customer,
            CustomerId = customer.Id,
            Vehicle = vehicle,
            VehicleId = vehicle.Id,
            EntryAt = now,
            RatePerMinute = ratePerMinute,
            FreeMinutes = freeMinutes,
            OpenedByUserId = openedByUserId,
        };
    }

    public int ElapsedMinutes(DateTime now) => PricingPolicy.ElapsedMinutes(EntryAt, ExitAt ?? now);

    public decimal AmountAt(DateTime now) =>
        Amount ?? PricingPolicy.Calculate(ElapsedMinutes(now), RatePerMinute, FreeMinutes);

    /// <summary>Closes the ticket and releases its space. Requires <see cref="Space"/> to be loaded.</summary>
    public void Close(int closedByUserId, DateTime now)
    {
        if (Status == TicketStatus.Closed)
        {
            throw new DomainException("ticket.already_closed", "O ticket já se encontra fechado.");
        }

        var exitAt = now < EntryAt ? EntryAt : now;
        DurationMinutes = PricingPolicy.ElapsedMinutes(EntryAt, exitAt);
        Amount = PricingPolicy.Calculate(DurationMinutes.Value, RatePerMinute, FreeMinutes);
        ExitAt = exitAt;
        ClosedByUserId = closedByUserId;
        Status = TicketStatus.Closed;
        Space.Release();
    }
}
