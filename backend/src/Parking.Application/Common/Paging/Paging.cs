using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace Parking.Application.Common.Paging;

public sealed record PagedResult<T>(IReadOnlyList<T> Items, int Page, int PageSize, int TotalCount)
{
    public int TotalPages => PageSize == 0 ? 0 : (int)Math.Ceiling(TotalCount / (double)PageSize);
}

public interface IPagedRequest
{
    int Page { get; }

    int PageSize { get; }
}

public static class PagingExtensions
{
    public const int MaxPageSize = 100;

    public static async Task<PagedResult<T>> ToPagedResultAsync<T>(
        this IQueryable<T> query, IPagedRequest request, CancellationToken cancellationToken)
    {
        var total = await query.CountAsync(cancellationToken);
        var items = total == 0
            ? []
            : await query.Skip((request.Page - 1) * request.PageSize).Take(request.PageSize).ToListAsync(cancellationToken);
        return new PagedResult<T>(items, request.Page, request.PageSize, total);
    }

    public static PagedResult<TOut> Map<TIn, TOut>(this PagedResult<TIn> page, Func<TIn, TOut> map) =>
        new([.. page.Items.Select(map)], page.Page, page.PageSize, page.TotalCount);

    public static void ApplyPagingRules<T>(this AbstractValidator<T> validator)
        where T : IPagedRequest
    {
        validator.RuleFor(x => x.Page).GreaterThanOrEqualTo(1);
        validator.RuleFor(x => x.PageSize).InclusiveBetween(1, MaxPageSize);
    }
}
