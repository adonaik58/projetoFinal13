using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Extensions.Options;
using Parking.Api.Infrastructure;
using Parking.Application.Abstractions.Messaging;
using Parking.Application.Features.Auth;

namespace Parking.Api.Endpoints;

public sealed record AuthResponse(string AccessToken, DateTime AccessTokenExpiresAt, AuthUserDto User);

public sealed record ChangePasswordRequest(string CurrentPassword, string NewPassword);

internal static class AuthEndpoints
{
    public static void MapAuthEndpoints(this RouteGroupBuilder api)
    {
        var group = api.MapGroup("/auth").WithTags("Autenticação");

        group.MapPost("/login", async Task<Ok<AuthResponse>> (
                LoginCommand command, IDispatcher dispatcher, HttpResponse response, IOptions<AuthCookieOptions> cookie, CancellationToken ct) =>
            {
                var result = await dispatcher.Send(command, ct);
                RefreshTokenCookie.Write(response, result.RefreshToken, result.RefreshTokenExpiresAt, cookie.Value.RefreshCookieSecure);
                return TypedResults.Ok(new AuthResponse(result.AccessToken, result.AccessTokenExpiresAt, result.User));
            })
            .AllowAnonymous()
            .WithDistributedRateLimit(RateLimitingOptions.AuthPolicy)
            .WithSummary("Inicia sessão; devolve o access token e define o cookie de refresh (HttpOnly).")
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status429TooManyRequests);

        group.MapPost("/refresh", async Task<Ok<AuthResponse>> (
                IDispatcher dispatcher, HttpRequest request, HttpResponse response, IOptions<AuthCookieOptions> cookie, CancellationToken ct) =>
            {
                try
                {
                    var result = await dispatcher.Send(new RefreshSessionCommand(RefreshTokenCookie.Read(request)), ct);
                    RefreshTokenCookie.Write(response, result.RefreshToken, result.RefreshTokenExpiresAt, cookie.Value.RefreshCookieSecure);
                    return TypedResults.Ok(new AuthResponse(result.AccessToken, result.AccessTokenExpiresAt, result.User));
                }
                catch (Application.Common.Exceptions.UnauthorizedException)
                {
                    RefreshTokenCookie.Clear(response, cookie.Value.RefreshCookieSecure);
                    throw;
                }
            })
            .AllowAnonymous()
            .AddEndpointFilter<RequireAjaxHeaderFilter>()
            .WithDistributedRateLimit(RateLimitingOptions.AuthPolicy)
            .WithSummary("Roda o refresh token (cookie) e emite um novo access token.")
            .ProducesProblem(StatusCodes.Status401Unauthorized);

        group.MapPost("/logout", async Task<NoContent> (
                IDispatcher dispatcher, HttpRequest request, HttpResponse response, IOptions<AuthCookieOptions> cookie, CancellationToken ct) =>
            {
                await dispatcher.Send(new LogoutCommand(RefreshTokenCookie.Read(request)), ct);
                RefreshTokenCookie.Clear(response, cookie.Value.RefreshCookieSecure);
                return TypedResults.NoContent();
            })
            .AllowAnonymous()
            .AddEndpointFilter<RequireAjaxHeaderFilter>()
            .WithSummary("Termina a sessão atual (revoga o refresh token).");

        group.MapGet("/me", async (IDispatcher dispatcher, CancellationToken ct) =>
                TypedResults.Ok(await dispatcher.Send(new GetCurrentUserQuery(), ct)))
            .WithSummary("Utilizador autenticado e respetivas permissões.");

        group.MapPost("/change-password", async Task<NoContent> (
                ChangePasswordRequest body, IDispatcher dispatcher, HttpRequest request, CancellationToken ct) =>
            {
                await dispatcher.Send(new ChangePasswordCommand(body.CurrentPassword, body.NewPassword, RefreshTokenCookie.Read(request)), ct);
                return TypedResults.NoContent();
            })
            .WithSummary("Altera a senha do próprio utilizador e termina as restantes sessões.")
            .ProducesValidationProblem();
    }
}
