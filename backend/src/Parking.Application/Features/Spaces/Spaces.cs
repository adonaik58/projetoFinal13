using System.Globalization;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Parking.Application.Abstractions;
using Parking.Application.Abstractions.Messaging;
using Parking.Application.Common;
using Parking.Application.Common.Behaviors;
using Parking.Application.Common.Exceptions;
using Parking.Application.Features.Settings;
using Parking.Domain.Customers;
using Parking.Domain.Spaces;
using Parking.Domain.Tickets;

namespace Parking.Application.Features.Spaces;

public sealed record OccupancyCustomerDto(int Id, string FullName, string NationalId, DateOnly BirthDate, int Age, string? Phone);

public sealed record OccupancyVehicleDto(int Id, string Plate, string Color, int BrandId, string BrandName, int? ModelId, string? ModelName);

/// <summary>
/// Open ticket of an occupied space. <c>FreeUntil</c> is the end of the free period from the tariff snapshot taken at
/// entry (equal to <c>EntryAt</c> when there is none); <c>OpenedBy</c> is the operator who registered the entry.
/// With <c>RatePerMinute</c> clients keep the amount live between pushes; the server prices the ticket at check-out.
/// </summary>
public sealed record OccupancyDto(
    long TicketId, string TicketCode, DateTime EntryAt, int ElapsedMinutes, decimal CurrentAmount,
    OccupancyCustomerDto Customer, OccupancyVehicleDto Vehicle, int FreeMinutes, DateTime FreeUntil, string OpenedBy, decimal RatePerMinute);

/// <summary><c>CreatedBy</c> is the full name of the user who added the space; <c>null</c> when unknown (imported or older spaces).</summary>
public sealed record SpaceDto(
    int Id, string Code, string Section, int Number, SpaceStatus Status, OccupancyDto? Occupancy, string? CreatedBy, DateTime CreatedAt);

public sealed record SpaceSummaryDto(int Total, int Free, int Occupied, int Maintenance);

public sealed record SpaceBoardDto(IReadOnlyList<SpaceDto> Items, SpaceSummaryDto Summary, DateTime ServerTime);

public sealed record BulkCreateSpacesResult(int Created, int Skipped);

/// <summary>Cached raw state of the parking (spaces + open tickets); live values are computed per request.</summary>
internal sealed record SpaceBoardSnapshot(IReadOnlyList<SpaceRow> Spaces, IReadOnlyList<OpenTicketRow> OpenTickets);

internal sealed record SpaceRow(int Id, string Code, string Section, int Number, SpaceStatus Status, string? CreatedBy, DateTime CreatedAt);

internal sealed record OpenTicketRow(
    int SpaceId, long TicketId, string TicketCode, DateTime EntryAt, decimal RatePerMinute, int FreeMinutes,
    int CustomerId, string CustomerName, string NationalId, DateOnly BirthDate, string? CustomerPhone,
    int VehicleId, string Plate, string Color, int BrandId, string BrandName, int? ModelId, string? ModelName, string OpenedBy);

internal static class SpaceBoard
{
    // Read-after-write matters here (an operator expects to see the space they just occupied), so the
    // entry lives only in Redis, shared by all replicas, and is invalidated on every change.
    private static readonly CacheEntrySettings CacheSettings = new(TimeSpan.FromSeconds(30));

    public static ValueTask<SpaceBoardSnapshot> GetSnapshotAsync(IAppDbContext db, ICacheService cache, CancellationToken cancellationToken) =>
        cache.GetOrCreateAsync(
            CacheKeys.SpaceBoard,
            async ct =>
            {
                var spaces = await db.ParkingSpaces.AsNoTracking()
                    .OrderBy(s => s.Section).ThenBy(s => s.Number)
                    .Select(s => new SpaceRow(s.Id, s.Code, s.Section, s.Number, s.Status, s.CreatedBy != null ? s.CreatedBy.FullName : null, s.CreatedAt))
                    .ToListAsync(ct);
                var open = await db.Tickets.AsNoTracking()
                    .Where(t => t.Status == TicketStatus.Open)
                    .Select(t => new OpenTicketRow(
                        t.SpaceId, t.Id, t.Code, t.EntryAt, t.RatePerMinute, t.FreeMinutes,
                        t.CustomerId, t.Customer.FullName, t.Customer.NationalId, t.Customer.BirthDate, t.Customer.Phone,
                        t.VehicleId, t.Vehicle.Plate, t.Vehicle.Color, t.Vehicle.BrandId, t.Vehicle.Brand.Name,
                        t.Vehicle.ModelId, t.Vehicle.Model != null ? t.Vehicle.Model.Name : null, t.OpenedBy.FullName))
                    .ToListAsync(ct);
                return new SpaceBoardSnapshot(spaces, open);
            },
            CacheSettings,
            cancellationToken);

    public static SpaceDto ToDto(SpaceRow space, OpenTicketRow? ticket, DateTime now, DateOnly today)
    {
        OccupancyDto? occupancy = null;
        if (ticket is not null)
        {
            var elapsed = PricingPolicy.ElapsedMinutes(ticket.EntryAt, now);
            occupancy = new OccupancyDto(
                ticket.TicketId,
                ticket.TicketCode,
                ticket.EntryAt,
                elapsed,
                PricingPolicy.Calculate(elapsed, ticket.RatePerMinute, ticket.FreeMinutes),
                new OccupancyCustomerDto(
                    ticket.CustomerId, ticket.CustomerName, ticket.NationalId, ticket.BirthDate, Customer.AgeOn(ticket.BirthDate, today), ticket.CustomerPhone),
                new OccupancyVehicleDto(ticket.VehicleId, ticket.Plate, ticket.Color, ticket.BrandId, ticket.BrandName, ticket.ModelId, ticket.ModelName),
                ticket.FreeMinutes,
                ticket.EntryAt.AddMinutes(ticket.FreeMinutes),
                ticket.OpenedBy,
                ticket.RatePerMinute);
        }

        return new SpaceDto(space.Id, space.Code, space.Section, space.Number, space.Status, occupancy, space.CreatedBy, space.CreatedAt);
    }

    public static SpaceDto ToDto(ParkingSpace space, string? createdBy) =>
        new(space.Id, space.Code, space.Section, space.Number, space.Status, null, createdBy, space.CreatedAt);
}

// ---- Queries ----

public sealed record GetSpaceBoardQuery(string? Search = null, SpaceStatus? Status = null, string? Plate = null, SortDirection SortDir = SortDirection.Asc)
    : IQuery<SpaceBoardDto>;

internal sealed class GetSpaceBoardValidator : AbstractValidator<GetSpaceBoardQuery>
{
    public GetSpaceBoardValidator()
    {
        RuleFor(x => x.Search).MaximumLength(ParkingSpace.CodeMaxLength);
        RuleFor(x => x.Plate).MaximumLength(20);
    }
}

internal sealed class GetSpaceBoardHandler(IAppDbContext db, ICacheService cache, IBusinessClock clock)
    : IQueryHandler<GetSpaceBoardQuery, SpaceBoardDto>
{
    public async Task<SpaceBoardDto> Handle(GetSpaceBoardQuery request, CancellationToken cancellationToken)
    {
        var snapshot = await SpaceBoard.GetSnapshotAsync(db, cache, cancellationToken);
        var now = clock.UtcNow;
        var today = clock.Today;
        var ticketsBySpace = snapshot.OpenTickets.ToDictionary(t => t.SpaceId);

        IEnumerable<SpaceRow> spaces = snapshot.Spaces;
        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = request.Search.Trim();
            spaces = spaces.Where(s => s.Code.Contains(search, StringComparison.OrdinalIgnoreCase));
        }

        if (request.Status is { } status)
        {
            spaces = spaces.Where(s => s.Status == status);
        }

        if (!string.IsNullOrWhiteSpace(request.Plate))
        {
            var plate = request.Plate.Trim();
            spaces = spaces.Where(s => ticketsBySpace.TryGetValue(s.Id, out var t) && t.Plate.Contains(plate, StringComparison.OrdinalIgnoreCase));
        }

        if (request.SortDir == SortDirection.Desc)
        {
            spaces = spaces.Reverse();
        }

        var items = spaces.Select(s => SpaceBoard.ToDto(s, ticketsBySpace.GetValueOrDefault(s.Id), now, today)).ToList();
        var summary = new SpaceSummaryDto(
            snapshot.Spaces.Count,
            snapshot.Spaces.Count(s => s.Status == SpaceStatus.Free),
            snapshot.Spaces.Count(s => s.Status == SpaceStatus.Occupied),
            snapshot.Spaces.Count(s => s.Status == SpaceStatus.Maintenance));
        return new SpaceBoardDto(items, summary, now);
    }
}

// ---- Commands ----

public sealed record CreateSpaceCommand(string Code) : ICommand<SpaceDto>, IParkingChange;

internal sealed class CreateSpaceValidator : AbstractValidator<CreateSpaceCommand>
{
    public CreateSpaceValidator() =>
        RuleFor(x => x.Code).Must(ParkingSpace.IsValidCode).WithMessage("Código inválido: use 1 a 3 letras seguidas de um número (ex.: A1).");
}

internal sealed class CreateSpaceHandler(IAppDbContext db, ICacheService cache, IAuditLogger audit, ICurrentUser currentUser)
    : ICommandHandler<CreateSpaceCommand, SpaceDto>
{
    public async Task<SpaceDto> Handle(CreateSpaceCommand request, CancellationToken cancellationToken)
    {
        var space = ParkingSpace.Create(request.Code, currentUser.RequiredUserId);
        await SpaceCapacity.EnsureAvailableAsync(db, cache, 1, cancellationToken);
        if (await db.ParkingSpaces.AnyAsync(s => s.Code == space.Code, cancellationToken))
        {
            throw new ConflictException("space.code_taken", $"Já existe o espaço {space.Code}.");
        }

        db.ParkingSpaces.Add(space);
        audit.Record("space.created", nameof(ParkingSpace), space.Code);
        await db.SaveChangesAsync(cancellationToken);
        await cache.RemoveAsync(CacheKeys.OccupancyDependent(), cancellationToken);
        var createdBy = await db.Users.Where(u => u.Id == space.CreatedByUserId).Select(u => u.FullName).SingleOrDefaultAsync(cancellationToken);
        return SpaceBoard.ToDto(space, createdBy);
    }
}

public sealed record BulkCreateSpacesCommand(IReadOnlyList<string> Sections, int FromNumber, int ToNumber)
    : ICommand<BulkCreateSpacesResult>, IParkingChange;

internal sealed class BulkCreateSpacesValidator : AbstractValidator<BulkCreateSpacesCommand>
{
    public const int MaxBatch = 5_000;

    public BulkCreateSpacesValidator()
    {
        RuleFor(x => x.Sections).NotEmpty().WithMessage("Indique pelo menos uma secção.");
        RuleForEach(x => x.Sections).Matches("^[A-Za-z]{1,3}$").WithMessage("Secção inválida: use 1 a 3 letras.");
        RuleFor(x => x.FromNumber).InclusiveBetween(1, ParkingSpace.MaxNumber);
        RuleFor(x => x.ToNumber).InclusiveBetween(1, ParkingSpace.MaxNumber)
            .GreaterThanOrEqualTo(x => x.FromNumber).WithMessage("O número final deve ser maior ou igual ao inicial.");
        RuleFor(x => x)
            .Must(x => x.Sections.Distinct(StringComparer.OrdinalIgnoreCase).Count() * (long)(x.ToNumber - x.FromNumber + 1) <= MaxBatch)
            .WithName("sections")
            .WithMessage($"No máximo {MaxBatch} espaços por operação.");
    }
}

internal sealed class BulkCreateSpacesHandler(IAppDbContext db, ICacheService cache, IAuditLogger audit, ICurrentUser currentUser)
    : ICommandHandler<BulkCreateSpacesCommand, BulkCreateSpacesResult>
{
    public async Task<BulkCreateSpacesResult> Handle(BulkCreateSpacesCommand request, CancellationToken cancellationToken)
    {
        var candidates = request.Sections
            .Select(s => s.Trim().ToUpperInvariant())
            .Distinct()
            .SelectMany(section => Enumerable.Range(request.FromNumber, request.ToNumber - request.FromNumber + 1)
                .Select(number => ParkingSpace.Create(section, number, currentUser.RequiredUserId)))
            .ToList();

        var codes = candidates.Select(c => c.Code).ToList();
        var existing = (await db.ParkingSpaces.Where(s => codes.Contains(s.Code)).Select(s => s.Code).ToListAsync(cancellationToken))
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
        var toCreate = candidates.Where(c => !existing.Contains(c.Code)).ToList();

        if (toCreate.Count > 0)
        {
            await SpaceCapacity.EnsureAvailableAsync(db, cache, toCreate.Count, cancellationToken);
            db.ParkingSpaces.AddRange(toCreate);
            audit.Record("space.bulk_created", nameof(ParkingSpace), null,
                new { request.Sections, request.FromNumber, request.ToNumber, created = toCreate.Count });
            await db.SaveChangesAsync(cancellationToken);
            await cache.RemoveAsync(CacheKeys.OccupancyDependent(), cancellationToken);
        }

        return new BulkCreateSpacesResult(toCreate.Count, candidates.Count - toCreate.Count);
    }
}

internal static class SpaceCapacity
{
    public static async Task EnsureAvailableAsync(IAppDbContext db, ICacheService cache, int adding, CancellationToken cancellationToken)
    {
        var settings = await SettingsReader.GetAsync(db, cache, cancellationToken);
        var count = await db.ParkingSpaces.CountAsync(cancellationToken);
        if (count + adding > settings.MaxSpaces)
        {
            var available = Math.Max(0, settings.MaxSpaces - count);
            throw new ConflictException(
                "space.capacity_reached",
                string.Create(CultureInfo.InvariantCulture,
                    $"Capacidade máxima de {settings.MaxSpaces} espaços atingida (disponíveis: {available}). Ajuste a capacidade nas configurações."));
        }
    }
}

public sealed record ChangeSpaceStatusCommand(int Id, SpaceStatus Status) : ICommand<SpaceDto>, IParkingChange;

internal sealed class ChangeSpaceStatusValidator : AbstractValidator<ChangeSpaceStatusCommand>
{
    public ChangeSpaceStatusValidator() =>
        RuleFor(x => x.Status).Must(s => s is SpaceStatus.Free or SpaceStatus.Maintenance)
            .WithMessage("Só é possível alterar o estado para Livre ou Manutenção.");
}

internal sealed class ChangeSpaceStatusHandler(IAppDbContext db, ICacheService cache, IAuditLogger audit)
    : ICommandHandler<ChangeSpaceStatusCommand, SpaceDto>
{
    public async Task<SpaceDto> Handle(ChangeSpaceStatusCommand request, CancellationToken cancellationToken)
    {
        var space = await db.ParkingSpaces.Include(s => s.CreatedBy).SingleOrDefaultAsync(s => s.Id == request.Id, cancellationToken)
            ?? throw new NotFoundException("space.not_found", "Espaço não encontrado.");
        var previous = space.Status;
        if (request.Status == SpaceStatus.Maintenance)
        {
            space.SetMaintenance();
        }
        else
        {
            space.SetAvailable();
        }

        audit.Record("space.status_changed", nameof(ParkingSpace), space.Code, new { from = previous.ToString(), to = space.Status.ToString() });
        await db.SaveChangesAsync(cancellationToken);
        await cache.RemoveAsync(CacheKeys.OccupancyDependent(), cancellationToken);
        return SpaceBoard.ToDto(space, space.CreatedBy?.FullName);
    }
}

public sealed record DeleteSpaceCommand(int Id) : ICommand, IParkingChange;

internal sealed class DeleteSpaceHandler(IAppDbContext db, ICacheService cache, IAuditLogger audit) : ICommandHandler<DeleteSpaceCommand>
{
    public async Task<Unit> Handle(DeleteSpaceCommand request, CancellationToken cancellationToken)
    {
        var space = await db.ParkingSpaces.SingleOrDefaultAsync(s => s.Id == request.Id, cancellationToken)
            ?? throw new NotFoundException("space.not_found", "Espaço não encontrado.");
        space.EnsureNotOccupied();
        if (await db.Tickets.AnyAsync(t => t.SpaceId == space.Id, cancellationToken))
        {
            throw new ConflictException(
                "space.has_history", "O espaço tem histórico de tickets e não pode ser eliminado. Coloque-o em manutenção.");
        }

        db.ParkingSpaces.Remove(space);
        audit.Record("space.deleted", nameof(ParkingSpace), space.Code);
        await db.SaveChangesAsync(cancellationToken);
        await cache.RemoveAsync(CacheKeys.OccupancyDependent(), cancellationToken);
        return Unit.Value;
    }
}
