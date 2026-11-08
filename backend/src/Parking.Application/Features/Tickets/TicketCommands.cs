using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Parking.Application.Abstractions;
using Parking.Application.Abstractions.Messaging;
using Parking.Application.Common;
using Parking.Application.Common.Exceptions;
using Parking.Domain.Customers;
using Parking.Domain.Settings;
using Parking.Domain.Tickets;
using Parking.Domain.Vehicles;

namespace Parking.Application.Features.Tickets;

public sealed record CustomerInput(string FullName, string NationalId, DateOnly BirthDate, string? Phone = null);

public sealed record VehicleInput(string Plate, int BrandId, int? ModelId, string Color);

/// <summary>Check-in: occupies a free space for a customer's vehicle.</summary>
public sealed record OpenTicketCommand(int SpaceId, CustomerInput Customer, VehicleInput Vehicle) : ICommand<TicketDetailDto>;

internal sealed class OpenTicketValidator : AbstractValidator<OpenTicketCommand>
{
    public OpenTicketValidator(IBusinessClock clock)
    {
        RuleFor(x => x.SpaceId).GreaterThan(0).WithMessage("Indique o espaço.");
        RuleFor(x => x.Customer).NotNull();
        RuleFor(x => x.Vehicle).NotNull();

        RuleFor(x => x.Customer.FullName).NotEmpty().WithMessage("Indique o nome do cliente.")
            .Length(2, Customer.FullNameMaxLength).When(x => x.Customer is not null);
        RuleFor(x => x.Customer.NationalId)
            .Must(id => Customer.IsValidNationalId(Customer.NormalizeNationalId(id ?? string.Empty)))
            .WithMessage("Bilhete de Identidade inválido: 14 caracteres no formato 123456789LA123.")
            .When(x => x.Customer is not null);
        RuleFor(x => x.Customer.BirthDate)
            .Must(date => date < clock.Today && date >= clock.Today.AddYears(-120))
            .WithMessage("Data de nascimento inválida.")
            .When(x => x.Customer is not null);
        RuleFor(x => x.Customer.Phone)
            .Must(phone => Customer.IsValidPhone(Customer.NormalizePhone(phone)))
            .WithMessage("Contacto inválido: use 9 a 15 dígitos (ex.: 923456789).")
            .When(x => x.Customer is not null);

        RuleFor(x => x.Vehicle.Plate)
            .Must(plate => Vehicle.IsValidPlate(Vehicle.NormalizePlate(plate ?? string.Empty)))
            .WithMessage("Matrícula inválida: 2 a 20 letras, números, espaços ou hífenes.")
            .When(x => x.Vehicle is not null);
        RuleFor(x => x.Vehicle.BrandId).GreaterThan(0).WithMessage("Escolha a marca.").When(x => x.Vehicle is not null);
        RuleFor(x => x.Vehicle.ModelId).GreaterThan(0).When(x => x.Vehicle?.ModelId is not null);
        RuleFor(x => x.Vehicle.Color).Must(Vehicle.IsValidColor).WithMessage("Cor inválida (formato #RRGGBB).")
            .When(x => x.Vehicle is not null);
    }
}

internal sealed class OpenTicketHandler(
    IAppDbContext db,
    ICacheService cache,
    ICurrentUser currentUser,
    IAuditLogger audit,
    IBusinessClock clock,
    IDispatcher dispatcher,
    ParkingMetrics metrics) : ICommandHandler<OpenTicketCommand, TicketDetailDto>
{
    public async Task<TicketDetailDto> Handle(OpenTicketCommand request, CancellationToken cancellationToken)
    {
        var now = clock.UtcNow;
        var today = clock.Today;

        var space = await db.ParkingSpaces.SingleOrDefaultAsync(s => s.Id == request.SpaceId, cancellationToken)
            ?? throw new NotFoundException("space.not_found", "Espaço não encontrado.");
        var settings = await db.ParkingSettings.AsNoTracking().SingleOrDefaultAsync(s => s.Id == ParkingSettings.SingletonId, cancellationToken)
            ?? throw new NotFoundException("settings.not_found", "A configuração do parque não foi inicializada.");

        var customer = await UpsertCustomerAsync(request.Customer, today, cancellationToken);
        var vehicle = await UpsertVehicleAsync(request.Vehicle, cancellationToken);

        // Friendly pre-checks; the unique indexes on the tickets table remain the source of truth under concurrency.
        await EnsureNoOpenTicketAsync(t => t.CustomerId == customer.Id, customer.Id, "ticket.customer_has_open_ticket", "Este cliente já tem um ticket aberto no espaço {0}.", cancellationToken);
        await EnsureNoOpenTicketAsync(t => t.VehicleId == vehicle.Id, vehicle.Id, "ticket.vehicle_has_open_ticket", "Esta viatura já tem um ticket aberto no espaço {0}.", cancellationToken);

        var ticket = Ticket.Open(TicketCode.New(), space, customer, vehicle, currentUser.RequiredUserId, settings.RatePerMinute, settings.FreeMinutes, now);
        db.Tickets.Add(ticket);
        audit.Record("ticket.opened", nameof(Ticket), ticket.Code, new { space = space.Code, plate = vehicle.Plate, nationalId = customer.NationalId });
        await db.SaveChangesAsync(cancellationToken);

        await cache.RemoveAsync(CacheKeys.OccupancyDependent(), cancellationToken);
        metrics.TicketOpened();
        return await dispatcher.Send(new GetTicketQuery(ticket.Id), cancellationToken);
    }

    private async Task<Customer> UpsertCustomerAsync(CustomerInput input, DateOnly today, CancellationToken cancellationToken)
    {
        var nationalId = Customer.NormalizeNationalId(input.NationalId);
        var customer = await db.Customers.SingleOrDefaultAsync(c => c.NationalId == nationalId, cancellationToken);
        if (customer is null)
        {
            customer = Customer.Create(input.FullName, nationalId, input.BirthDate, today, input.Phone);
            db.Customers.Add(customer);
        }
        else
        {
            customer.Update(input.FullName, input.BirthDate, today, input.Phone);
        }

        return customer;
    }

    private async Task<Vehicle> UpsertVehicleAsync(VehicleInput input, CancellationToken cancellationToken)
    {
        var brand = await db.VehicleBrands.SingleOrDefaultAsync(b => b.Id == input.BrandId, cancellationToken)
            ?? throw new NotFoundException("catalog.brand_not_found", "Marca não encontrada.");
        VehicleModel? model = null;
        if (input.ModelId is { } modelId)
        {
            model = await db.VehicleModels.SingleOrDefaultAsync(m => m.Id == modelId, cancellationToken)
                ?? throw new NotFoundException("catalog.model_not_found", "Modelo não encontrado.");
        }

        var plate = Vehicle.NormalizePlate(input.Plate);
        var vehicle = await db.Vehicles.SingleOrDefaultAsync(v => v.Plate == plate, cancellationToken);
        if (vehicle is null)
        {
            vehicle = Vehicle.Create(plate, brand, model, input.Color);
            db.Vehicles.Add(vehicle);
        }
        else
        {
            vehicle.Update(brand, model, input.Color);
        }

        return vehicle;
    }

    private async Task EnsureNoOpenTicketAsync(
        System.Linq.Expressions.Expression<Func<Ticket, bool>> belongsTo, int id, string code, string messageFormat, CancellationToken cancellationToken)
    {
        if (id == 0)
        {
            return; // new customer/vehicle: cannot have tickets yet
        }

        var spaceCode = await db.Tickets.Where(t => t.Status == TicketStatus.Open).Where(belongsTo)
            .Select(t => t.Space.Code).FirstOrDefaultAsync(cancellationToken);
        if (spaceCode is not null)
        {
            throw new ConflictException(code, string.Format(System.Globalization.CultureInfo.InvariantCulture, messageFormat, spaceCode));
        }
    }
}

/// <summary>Check-out: closes the ticket, prices it and releases the space.</summary>
public sealed record CloseTicketCommand(long Id) : ICommand<TicketDetailDto>;

internal sealed class CloseTicketHandler(
    IAppDbContext db,
    ICacheService cache,
    ICurrentUser currentUser,
    IAuditLogger audit,
    IBusinessClock clock,
    IDispatcher dispatcher,
    ParkingMetrics metrics) : ICommandHandler<CloseTicketCommand, TicketDetailDto>
{
    public async Task<TicketDetailDto> Handle(CloseTicketCommand request, CancellationToken cancellationToken)
    {
        var ticket = await db.Tickets.Include(t => t.Space).SingleOrDefaultAsync(t => t.Id == request.Id, cancellationToken)
            ?? throw new NotFoundException("ticket.not_found", "Ticket não encontrado.");

        ticket.Close(currentUser.RequiredUserId, clock.UtcNow);
        audit.Record("ticket.closed", nameof(Ticket), ticket.Code,
            new { space = ticket.Space.Code, ticket.DurationMinutes, ticket.Amount });
        await db.SaveChangesAsync(cancellationToken);

        await cache.RemoveAsync(CacheKeys.OccupancyDependent(), cancellationToken);
        metrics.TicketClosed(ticket.Amount!.Value);
        return await dispatcher.Send(new GetTicketQuery(ticket.Id), cancellationToken);
    }
}
