using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Parking.Application.Abstractions;
using Parking.Application.Abstractions.Messaging;
using Parking.Application.Common;
using Parking.Application.Common.Exceptions;
using Parking.Application.Common.Paging;
using Parking.Domain.Customers;
using Parking.Domain.Tickets;
using Parking.Domain.Vehicles;

namespace Parking.Application.Features.Tickets;

public sealed record ListTicketsQuery(
    int Page = 1,
    int PageSize = 20,
    TicketStatus? Status = null,
    string? Search = null,
    string? CustomerName = null,
    string? NationalId = null,
    string? Plate = null,
    int? BrandId = null,
    int? ModelId = null,
    string? SpaceCode = null,
    DateTime? From = null,
    DateTime? To = null,
    TicketSortBy SortBy = TicketSortBy.EntryAt,
    SortDirection SortDir = SortDirection.Desc) : IQuery<PagedResult<TicketDto>>, IPagedRequest;

internal sealed class ListTicketsValidator : AbstractValidator<ListTicketsQuery>
{
    public ListTicketsValidator()
    {
        this.ApplyPagingRules();
        RuleFor(x => x.Search).MaximumLength(100);
        RuleFor(x => x.CustomerName).MaximumLength(Customer.FullNameMaxLength);
        RuleFor(x => x.NationalId).MaximumLength(Customer.NationalIdLength);
        RuleFor(x => x.Plate).MaximumLength(Vehicle.PlateMaxLength);
        RuleFor(x => x.SpaceCode).MaximumLength(7);
        RuleFor(x => x.To).GreaterThanOrEqualTo(x => x.From).When(x => x.From is not null && x.To is not null)
            .WithMessage("A data final deve ser posterior à data inicial.");
    }
}

internal sealed class ListTicketsHandler(IAppDbContext db, IBusinessClock clock) : IQueryHandler<ListTicketsQuery, PagedResult<TicketDto>>
{
    public async Task<PagedResult<TicketDto>> Handle(ListTicketsQuery request, CancellationToken cancellationToken)
    {
        var query = Sort(Filter(db.Tickets.AsNoTracking(), request), request.SortBy, request.SortDir);
        var page = await query.Select(TicketRow.Projection).ToPagedResultAsync(request, cancellationToken);
        var now = clock.UtcNow;
        var today = clock.Today;
        return page.Map(row => row.ToDto(now, today));
    }

    private static IQueryable<Ticket> Filter(IQueryable<Ticket> query, ListTicketsQuery r)
    {
        if (r.Status is { } status)
        {
            query = query.Where(t => t.Status == status);
        }

        if (!string.IsNullOrWhiteSpace(r.Search))
        {
            var s = r.Search.Trim();
            query = query.Where(t => t.Code.Contains(s) || t.Customer.FullName.Contains(s) || t.Customer.NationalId.Contains(s)
                                     || t.Vehicle.Plate.Contains(s) || t.Space.Code == s);
        }

        if (!string.IsNullOrWhiteSpace(r.CustomerName))
        {
            var name = r.CustomerName.Trim();
            query = query.Where(t => t.Customer.FullName.Contains(name));
        }

        if (!string.IsNullOrWhiteSpace(r.NationalId))
        {
            var nationalId = Customer.NormalizeNationalId(r.NationalId);
            query = query.Where(t => t.Customer.NationalId.StartsWith(nationalId));
        }

        if (!string.IsNullOrWhiteSpace(r.Plate))
        {
            var plate = Vehicle.NormalizePlate(r.Plate);
            query = query.Where(t => t.Vehicle.Plate.Contains(plate));
        }

        if (r.BrandId is { } brandId)
        {
            query = query.Where(t => t.Vehicle.BrandId == brandId);
        }

        if (r.ModelId is { } modelId)
        {
            query = query.Where(t => t.Vehicle.ModelId == modelId);
        }

        if (!string.IsNullOrWhiteSpace(r.SpaceCode))
        {
            var code = r.SpaceCode.Trim().ToUpperInvariant();
            query = query.Where(t => t.Space.Code.Contains(code));
        }

        if (r.From is { } from)
        {
            query = query.Where(t => t.EntryAt >= from);
        }

        if (r.To is { } to)
        {
            query = query.Where(t => t.EntryAt <= to);
        }

        return query;
    }

    private static IQueryable<Ticket> Sort(IQueryable<Ticket> query, TicketSortBy sortBy, SortDirection direction)
    {
        var asc = direction == SortDirection.Asc;
        IOrderedQueryable<Ticket> ordered = sortBy switch
        {
            TicketSortBy.ExitAt => asc ? query.OrderBy(t => t.ExitAt) : query.OrderByDescending(t => t.ExitAt),
            TicketSortBy.Amount => asc ? query.OrderBy(t => t.Amount) : query.OrderByDescending(t => t.Amount),
            // Closed tickets carry their duration; open ones are ordered by entry (earlier entry = longer stay).
            TicketSortBy.Duration => asc
                ? query.OrderBy(t => t.DurationMinutes).ThenByDescending(t => t.EntryAt)
                : query.OrderByDescending(t => t.DurationMinutes).ThenBy(t => t.EntryAt),
            TicketSortBy.CustomerName => asc ? query.OrderBy(t => t.Customer.FullName) : query.OrderByDescending(t => t.Customer.FullName),
            TicketSortBy.SpaceCode => asc
                ? query.OrderBy(t => t.Space.Section).ThenBy(t => t.Space.Number)
                : query.OrderByDescending(t => t.Space.Section).ThenByDescending(t => t.Space.Number),
            TicketSortBy.Plate => asc ? query.OrderBy(t => t.Vehicle.Plate) : query.OrderByDescending(t => t.Vehicle.Plate),
            TicketSortBy.Brand => asc ? query.OrderBy(t => t.Vehicle.Brand.Name) : query.OrderByDescending(t => t.Vehicle.Brand.Name),
            TicketSortBy.Model => asc ? query.OrderBy(t => t.Vehicle.Model!.Name) : query.OrderByDescending(t => t.Vehicle.Model!.Name),
            _ => asc ? query.OrderBy(t => t.EntryAt) : query.OrderByDescending(t => t.EntryAt),
        };
        return asc ? ordered.ThenBy(t => t.Id) : ordered.ThenByDescending(t => t.Id);
    }
}

public sealed record GetTicketQuery(long Id) : IQuery<TicketDetailDto>;

internal sealed class GetTicketHandler(IAppDbContext db, IBusinessClock clock) : IQueryHandler<GetTicketQuery, TicketDetailDto>
{
    public async Task<TicketDetailDto> Handle(GetTicketQuery request, CancellationToken cancellationToken)
    {
        var row = await db.Tickets.AsNoTracking().Where(t => t.Id == request.Id).Select(TicketRow.Projection).SingleOrDefaultAsync(cancellationToken)
            ?? throw new NotFoundException("ticket.not_found", "Ticket não encontrado.");
        return row.ToDetailDto(clock.UtcNow, clock.Today);
    }
}
