using Parking.Api.Infrastructure;
using Parking.Application.Abstractions.Messaging;
using Parking.Application.Features.Audit;
using Parking.Application.Features.Catalog;
using Parking.Application.Features.Dashboard;
using Parking.Application.Features.Settings;
using Parking.Domain.Users;

namespace Parking.Api.Endpoints;

public sealed record CreateModelRequest(string Name);

public sealed record ListAuditLogsRequest(int? Page, int? PageSize, string? Action, string? Username, string? From, string? To);

internal static class ManagementEndpoints
{
    public static void MapCatalogEndpoints(this RouteGroupBuilder api)
    {
        var group = api.MapGroup("/catalog/brands").WithTags("Catálogo");

        group.MapGet("/", async (IDispatcher dispatcher, CancellationToken ct) =>
                TypedResults.Ok(await dispatcher.Send(new ListBrandsQuery(), ct)))
            .RequireAuthorization(Permissions.CatalogView);

        group.MapGet("/{brandId:int}/models", async (int brandId, IDispatcher dispatcher, CancellationToken ct) =>
                TypedResults.Ok(await dispatcher.Send(new ListModelsQuery(brandId), ct)))
            .RequireAuthorization(Permissions.CatalogView);

        group.MapPost("/", async (CreateBrandCommand command, IDispatcher dispatcher, CancellationToken ct) =>
            {
                var brand = await dispatcher.Send(command, ct);
                return TypedResults.Created($"/api/v1/catalog/brands/{brand.Id}", brand);
            })
            .RequireAuthorization(Permissions.CatalogManage)
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status409Conflict);

        group.MapPost("/{brandId:int}/models", async (int brandId, CreateModelRequest body, IDispatcher dispatcher, CancellationToken ct) =>
            {
                var model = await dispatcher.Send(new CreateModelCommand(brandId, body.Name), ct);
                return TypedResults.Created($"/api/v1/catalog/brands/{brandId}/models/{model.Id}", model);
            })
            .RequireAuthorization(Permissions.CatalogManage)
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status409Conflict);
    }

    public static void MapSettingsEndpoints(this RouteGroupBuilder api)
    {
        var group = api.MapGroup("/settings").WithTags("Configuração");

        group.MapGet("/", async (IDispatcher dispatcher, CancellationToken ct) =>
                TypedResults.Ok(await dispatcher.Send(new GetSettingsQuery(), ct)))
            .RequireAuthorization(Permissions.SettingsView);

        group.MapPut("/", async (UpdateSettingsCommand command, IDispatcher dispatcher, CancellationToken ct) =>
                TypedResults.Ok(await dispatcher.Send(command, ct)))
            .RequireAuthorization(Permissions.SettingsManage)
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status409Conflict);

        group.MapGet("/tariff", async (IDispatcher dispatcher, CancellationToken ct) =>
                TypedResults.Ok(await dispatcher.Send(new GetTariffQuery(), ct)))
            .RequireAuthorization(Permissions.SettingsView)
            .WithSummary("Tarifa em vigor com exemplos calculados pela regra de preço do servidor.");
    }

    public static void MapDashboardEndpoints(this RouteGroupBuilder api)
    {
        var group = api.MapGroup("/dashboard").WithTags("Dashboard").RequireAuthorization(Permissions.DashboardView);

        group.MapGet("/summary", async (string? period, IDispatcher dispatcher, CancellationToken ct) =>
            TypedResults.Ok(await dispatcher.Send(new GetDashboardSummaryQuery(period ?? "today"), ct)));

        group.MapGet("/revenue-series", async (int? days, IDispatcher dispatcher, CancellationToken ct) =>
            TypedResults.Ok(await dispatcher.Send(new GetRevenueSeriesQuery(days ?? 30), ct)));
    }

    public static void MapAuditEndpoints(this RouteGroupBuilder api) =>
        api.MapGet("/audit-logs", async ([AsParameters] ListAuditLogsRequest r, IDispatcher dispatcher, CancellationToken ct) =>
                TypedResults.Ok(await dispatcher.Send(new ListAuditLogsQuery(
                    r.Page ?? 1, r.PageSize ?? 50, r.Action, r.Username,
                    QueryParsing.Instant(r.From, "from"), QueryParsing.Instant(r.To, "to")), ct)))
            .WithTags("Auditoria")
            .RequireAuthorization(Permissions.AuditView);
}
