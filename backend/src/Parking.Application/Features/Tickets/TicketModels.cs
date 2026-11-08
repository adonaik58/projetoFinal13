using System.Linq.Expressions;
using Parking.Domain.Customers;
using Parking.Domain.Tickets;

namespace Parking.Application.Features.Tickets;

public record TicketDto(
    long Id,
    string Code,
    TicketStatus Status,
    int SpaceId,
    string SpaceCode,
    string CustomerName,
    string NationalId,
    int CustomerAge,
    string Plate,
    string BrandName,
    string? ModelName,
    string Color,
    DateTime EntryAt,
    DateTime? ExitAt,
    int DurationMinutes,
    decimal Amount,
    string OpenedBy,
    string? ClosedBy);

public sealed record TicketDetailDto(
    long Id,
    string Code,
    TicketStatus Status,
    int SpaceId,
    string SpaceCode,
    string CustomerName,
    string NationalId,
    int CustomerAge,
    string Plate,
    string BrandName,
    string? ModelName,
    string Color,
    DateTime EntryAt,
    DateTime? ExitAt,
    int DurationMinutes,
    decimal Amount,
    string OpenedBy,
    string? ClosedBy,
    DateOnly CustomerBirthDate,
    decimal RatePerMinute,
    int FreeMinutes)
    : TicketDto(Id, Code, Status, SpaceId, SpaceCode, CustomerName, NationalId, CustomerAge, Plate, BrandName, ModelName, Color,
        EntryAt, ExitAt, DurationMinutes, Amount, OpenedBy, ClosedBy);

public enum TicketSortBy
{
    EntryAt,
    ExitAt,
    Amount,
    Duration,
    CustomerName,
    SpaceCode,
    Plate,
    Brand,
    Model,
}

/// <summary>Flat database projection; open tickets get their live duration/amount computed on mapping.</summary>
internal sealed record TicketRow(
    long Id,
    string Code,
    TicketStatus Status,
    int SpaceId,
    string SpaceCode,
    string CustomerName,
    string NationalId,
    DateOnly BirthDate,
    string Plate,
    string BrandName,
    string? ModelName,
    string Color,
    DateTime EntryAt,
    DateTime? ExitAt,
    int? DurationMinutes,
    decimal? Amount,
    decimal RatePerMinute,
    int FreeMinutes,
    string OpenedBy,
    string? ClosedBy)
{
    public static readonly Expression<Func<Ticket, TicketRow>> Projection = t => new TicketRow(
        t.Id,
        t.Code,
        t.Status,
        t.SpaceId,
        t.Space.Code,
        t.Customer.FullName,
        t.Customer.NationalId,
        t.Customer.BirthDate,
        t.Vehicle.Plate,
        t.Vehicle.Brand.Name,
        t.Vehicle.Model != null ? t.Vehicle.Model.Name : null,
        t.Vehicle.Color,
        t.EntryAt,
        t.ExitAt,
        t.DurationMinutes,
        t.Amount,
        t.RatePerMinute,
        t.FreeMinutes,
        t.OpenedBy.FullName,
        t.ClosedBy != null ? t.ClosedBy.FullName : null);

    public TicketDto ToDto(DateTime now, DateOnly today)
    {
        var (duration, amount) = Live(now);
        return new TicketDto(Id, Code, Status, SpaceId, SpaceCode, CustomerName, NationalId, Customer.AgeOn(BirthDate, today),
            Plate, BrandName, ModelName, Color, EntryAt, ExitAt, duration, amount, OpenedBy, ClosedBy);
    }

    public TicketDetailDto ToDetailDto(DateTime now, DateOnly today)
    {
        var (duration, amount) = Live(now);
        return new TicketDetailDto(Id, Code, Status, SpaceId, SpaceCode, CustomerName, NationalId, Customer.AgeOn(BirthDate, today),
            Plate, BrandName, ModelName, Color, EntryAt, ExitAt, duration, amount, OpenedBy, ClosedBy, BirthDate, RatePerMinute, FreeMinutes);
    }

    private (int Duration, decimal Amount) Live(DateTime now)
    {
        var duration = DurationMinutes ?? PricingPolicy.ElapsedMinutes(EntryAt, now);
        return (duration, Amount ?? PricingPolicy.Calculate(duration, RatePerMinute, FreeMinutes));
    }
}
