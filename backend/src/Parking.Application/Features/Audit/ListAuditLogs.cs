using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Parking.Application.Abstractions;
using Parking.Application.Abstractions.Messaging;
using Parking.Application.Common.Paging;

namespace Parking.Application.Features.Audit;

public sealed record AuditLogDto(
    long Id,
    DateTime OccurredAt,
    int? UserId,
    string? Username,
    string Action,
    string? EntityType,
    string? EntityId,
    string? IpAddress,
    string? CorrelationId,
    string? Details);

public sealed record ListAuditLogsQuery(
    int Page = 1, int PageSize = 50, string? Action = null, string? Username = null, DateTime? From = null, DateTime? To = null)
    : IQuery<PagedResult<AuditLogDto>>, IPagedRequest;

internal sealed class ListAuditLogsValidator : AbstractValidator<ListAuditLogsQuery>
{
    public ListAuditLogsValidator()
    {
        this.ApplyPagingRules();
        RuleFor(x => x.Action).MaximumLength(64);
        RuleFor(x => x.Username).MaximumLength(50);
    }
}

internal sealed class ListAuditLogsHandler(IAppDbContext db) : IQueryHandler<ListAuditLogsQuery, PagedResult<AuditLogDto>>
{
    public Task<PagedResult<AuditLogDto>> Handle(ListAuditLogsQuery request, CancellationToken cancellationToken)
    {
        var query = db.AuditLogs.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(request.Action))
        {
            var action = request.Action.Trim();
            query = query.Where(a => a.Action.StartsWith(action));
        }

        if (!string.IsNullOrWhiteSpace(request.Username))
        {
            var username = request.Username.Trim();
            query = query.Where(a => a.Username == username);
        }

        if (request.From is { } from)
        {
            query = query.Where(a => a.OccurredAt >= from);
        }

        if (request.To is { } to)
        {
            query = query.Where(a => a.OccurredAt <= to);
        }

        return query.OrderByDescending(a => a.OccurredAt).ThenByDescending(a => a.Id)
            .Select(a => new AuditLogDto(a.Id, a.OccurredAt, a.UserId, a.Username, a.Action, a.EntityType, a.EntityId, a.IpAddress, a.CorrelationId, a.Details))
            .ToPagedResultAsync(request, cancellationToken);
    }
}
