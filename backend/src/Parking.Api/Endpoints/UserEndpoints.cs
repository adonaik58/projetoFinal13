using Parking.Api.Infrastructure;
using Parking.Application.Abstractions.Messaging;
using Parking.Application.Features.Users;
using Parking.Domain.Users;

namespace Parking.Api.Endpoints;

public sealed record UpdateUserRequest(string Username, string FullName, Role Role);

public sealed record ResetPasswordRequest(string NewPassword);

public sealed record ListUsersRequest(int? Page, int? PageSize, string? Search, string? Role, bool? IsActive);

internal static class UserEndpoints
{
    public static void MapUserEndpoints(this RouteGroupBuilder api)
    {
        var group = api.MapGroup("/users").WithTags("Utilizadores").RequireAuthorization(Permissions.UsersManage);

        group.MapGet("/", async ([AsParameters] ListUsersRequest r, IDispatcher dispatcher, CancellationToken ct) =>
            TypedResults.Ok(await dispatcher.Send(
                new ListUsersQuery(r.Page ?? 1, r.PageSize ?? 20, r.Search, QueryParsing.Enum<Role>(r.Role, "role"), r.IsActive), ct)));

        group.MapGet("/{id:int}", async (int id, IDispatcher dispatcher, CancellationToken ct) =>
            TypedResults.Ok(await dispatcher.Send(new GetUserQuery(id), ct)));

        group.MapPost("/", async (CreateUserCommand command, IDispatcher dispatcher, CancellationToken ct) =>
            {
                var user = await dispatcher.Send(command, ct);
                return TypedResults.Created($"/api/v1/users/{user.Id}", user);
            })
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status409Conflict);

        group.MapPut("/{id:int}", async (int id, UpdateUserRequest body, IDispatcher dispatcher, CancellationToken ct) =>
                TypedResults.Ok(await dispatcher.Send(new UpdateUserCommand(id, body.Username, body.FullName, body.Role), ct)))
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status409Conflict);

        group.MapPost("/{id:int}/reset-password", async (int id, ResetPasswordRequest body, IDispatcher dispatcher, CancellationToken ct) =>
            {
                await dispatcher.Send(new ResetUserPasswordCommand(id, body.NewPassword), ct);
                return TypedResults.NoContent();
            })
            .ProducesValidationProblem();

        group.MapPost("/{id:int}/deactivate", async (int id, IDispatcher dispatcher, CancellationToken ct) =>
        {
            await dispatcher.Send(new SetUserActiveCommand(id, false), ct);
            return TypedResults.NoContent();
        });

        group.MapPost("/{id:int}/activate", async (int id, IDispatcher dispatcher, CancellationToken ct) =>
        {
            await dispatcher.Send(new SetUserActiveCommand(id, true), ct);
            return TypedResults.NoContent();
        });
    }
}
