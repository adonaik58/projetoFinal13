using Parking.Api.Infrastructure;
using Parking.Application.Abstractions.Messaging;
using Parking.Application.Common;
using Parking.Application.Features.Customers;
using Parking.Application.Features.Spaces;
using Parking.Application.Features.Tickets;
using Parking.Domain.Spaces;
using Parking.Domain.Tickets;
using Parking.Domain.Users;

namespace Parking.Api.Endpoints;

public sealed record SpaceBoardRequest(string? Search, string? Status, string? Plate, string? SortDir);

public sealed record ChangeSpaceStatusRequest(SpaceStatus Status);

public sealed record ListTicketsRequest(
    int? Page,
    int? PageSize,
    string? Status,
    string? Search,
    string? CustomerName,
    string? NationalId,
    string? Plate,
    int? BrandId,
    int? ModelId,
    string? SpaceCode,
    string? From,
    string? To,
    string? SortBy,
    string? SortDir);

internal static class ParkingEndpoints
{
    public static void MapSpaceEndpoints(this RouteGroupBuilder api)
    {
        var group = api.MapGroup("/spaces").WithTags("Espaços");

        group.MapGet("/", async ([AsParameters] SpaceBoardRequest r, IDispatcher dispatcher, CancellationToken ct) =>
                TypedResults.Ok(await dispatcher.Send(new GetSpaceBoardQuery(
                    r.Search,
                    QueryParsing.Enum<SpaceStatus>(r.Status, "status"),
                    r.Plate,
                    QueryParsing.Enum<SortDirection>(r.SortDir, "sortDir") ?? SortDirection.Asc), ct)))
            .RequireAuthorization(Permissions.SpacesView)
            .WithSummary("Grelha de espaços com ocupação atual, tempo decorrido e valor acumulado.");

        group.MapPost("/", async (CreateSpaceCommand command, IDispatcher dispatcher, CancellationToken ct) =>
            {
                var space = await dispatcher.Send(command, ct);
                return TypedResults.Created($"/api/v1/spaces/{space.Id}", space);
            })
            .RequireAuthorization(Permissions.SpacesManage)
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status409Conflict);

        group.MapPost("/bulk", async (BulkCreateSpacesCommand command, IDispatcher dispatcher, CancellationToken ct) =>
                TypedResults.Ok(await dispatcher.Send(command, ct)))
            .RequireAuthorization(Permissions.SpacesManage)
            .WithSummary("Cria espaços em lote (secções × intervalo de números); ignora códigos existentes.")
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status409Conflict);

        group.MapPatch("/{id:int}/status", async (int id, ChangeSpaceStatusRequest body, IDispatcher dispatcher, CancellationToken ct) =>
                TypedResults.Ok(await dispatcher.Send(new ChangeSpaceStatusCommand(id, body.Status), ct)))
            .RequireAuthorization(Permissions.SpacesManage)
            .ProducesProblem(StatusCodes.Status409Conflict);

        group.MapDelete("/{id:int}", async (int id, IDispatcher dispatcher, CancellationToken ct) =>
            {
                await dispatcher.Send(new DeleteSpaceCommand(id), ct);
                return TypedResults.NoContent();
            })
            .RequireAuthorization(Permissions.SpacesManage)
            .ProducesProblem(StatusCodes.Status409Conflict);
    }

    public static void MapTicketEndpoints(this RouteGroupBuilder api)
    {
        var group = api.MapGroup("/tickets").WithTags("Tickets");

        group.MapGet("/", async ([AsParameters] ListTicketsRequest r, IDispatcher dispatcher, CancellationToken ct) =>
                TypedResults.Ok(await dispatcher.Send(new ListTicketsQuery(
                    r.Page ?? 1,
                    r.PageSize ?? 20,
                    QueryParsing.Enum<TicketStatus>(r.Status, "status"),
                    r.Search,
                    r.CustomerName,
                    r.NationalId,
                    r.Plate,
                    r.BrandId,
                    r.ModelId,
                    r.SpaceCode,
                    QueryParsing.Instant(r.From, "from"),
                    QueryParsing.Instant(r.To, "to"),
                    QueryParsing.Enum<TicketSortBy>(r.SortBy, "sortBy") ?? TicketSortBy.EntryAt,
                    QueryParsing.Enum<SortDirection>(r.SortDir, "sortDir") ?? SortDirection.Desc), ct)))
            .RequireAuthorization(Permissions.TicketsView)
            .WithSummary("Lista paginada de tickets (ativos e histórico) com filtros e ordenação no servidor.");

        group.MapGet("/{id:long}", async (long id, IDispatcher dispatcher, CancellationToken ct) =>
                TypedResults.Ok(await dispatcher.Send(new GetTicketQuery(id), ct)))
            .RequireAuthorization(Permissions.TicketsView);

        group.MapPost("/", async (OpenTicketCommand command, IDispatcher dispatcher, CancellationToken ct) =>
            {
                var ticket = await dispatcher.Send(command, ct);
                return TypedResults.Created($"/api/v1/tickets/{ticket.Id}", ticket);
            })
            .RequireAuthorization(Permissions.TicketsOpen)
            .WithIdempotency()
            .WithSummary("Check-in: ocupa um espaço livre. Aceita o cabeçalho Idempotency-Key.")
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status409Conflict);

        group.MapPost("/{id:long}/close", async (long id, IDispatcher dispatcher, CancellationToken ct) =>
                TypedResults.Ok(await dispatcher.Send(new CloseTicketCommand(id), ct)))
            .RequireAuthorization(Permissions.TicketsClose)
            .WithIdempotency()
            .WithSummary("Check-out: fecha o ticket, calcula o valor e liberta o espaço. Aceita Idempotency-Key.")
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status409Conflict);

        api.MapGet("/customers/lookup", async (string nationalId, IDispatcher dispatcher, CancellationToken ct) =>
                TypedResults.Ok(await dispatcher.Send(new LookupCustomerQuery(nationalId), ct)))
            .WithTags("Clientes")
            .RequireAuthorization(Permissions.TicketsOpen)
            .WithSummary("Procura um cliente pelo BI para pré-preencher o check-in.")
            .ProducesProblem(StatusCodes.Status404NotFound);
    }
}
