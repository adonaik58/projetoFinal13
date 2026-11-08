using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Parking.Application.Abstractions;
using Parking.Application.Abstractions.Messaging;
using Parking.Application.Common.Exceptions;
using Parking.Domain.Customers;

namespace Parking.Application.Features.Customers;

public sealed record LastVehicleDto(string Plate, int BrandId, int? ModelId, string Color);

public enum CustomerLookupSource
{
    /// <summary>Returning customer stored in this system.</summary>
    Customer,

    /// <summary>New customer, pre-filled from the external BI registry.</summary>
    Registry,
}

public sealed record CustomerLookupDto(
    int? Id, string FullName, string NationalId, DateOnly? BirthDate, string? Phone, LastVehicleDto? LastVehicle, CustomerLookupSource Source);

/// <summary>Pre-fills the check-in form: returning customers first, then the external BI registry.</summary>
public sealed record LookupCustomerQuery(string NationalId) : IQuery<CustomerLookupDto>;

internal sealed class LookupCustomerValidator : AbstractValidator<LookupCustomerQuery>
{
    public LookupCustomerValidator() =>
        RuleFor(x => x.NationalId).Must(id => Customer.IsValidNationalId(Customer.NormalizeNationalId(id ?? string.Empty)))
            .WithMessage("Bilhete de Identidade inválido.");
}

internal sealed class LookupCustomerHandler(IAppDbContext db, INationalIdRegistry registry) : IQueryHandler<LookupCustomerQuery, CustomerLookupDto>
{
    public async Task<CustomerLookupDto> Handle(LookupCustomerQuery request, CancellationToken cancellationToken)
    {
        var nationalId = Customer.NormalizeNationalId(request.NationalId);
        var known = await db.Customers.AsNoTracking()
            .Where(c => c.NationalId == nationalId)
            .Select(c => new CustomerLookupDto(
                c.Id,
                c.FullName,
                c.NationalId,
                c.BirthDate,
                c.Phone,
                db.Tickets.Where(t => t.CustomerId == c.Id)
                    .OrderByDescending(t => t.EntryAt)
                    .Select(t => new LastVehicleDto(t.Vehicle.Plate, t.Vehicle.BrandId, t.Vehicle.ModelId, t.Vehicle.Color))
                    .FirstOrDefault(),
                CustomerLookupSource.Customer))
            .SingleOrDefaultAsync(cancellationToken);
        if (known is not null)
        {
            return known;
        }

        var record = await registry.FindAsync(nationalId, cancellationToken)
            ?? throw new NotFoundException("customer.not_found", "Cliente não encontrado.");
        return new CustomerLookupDto(null, record.FullName, nationalId, record.BirthDate, null, null, CustomerLookupSource.Registry);
    }
}
