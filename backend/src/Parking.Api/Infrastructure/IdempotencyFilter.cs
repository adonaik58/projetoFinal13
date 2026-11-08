using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Extensions.Options;
using Parking.Application.Abstractions;
using Parking.Application.Abstractions.Messaging;
using Parking.Infrastructure.Redis;
using StackExchange.Redis;

namespace Parking.Api.Infrastructure;

/// <summary>
/// Honors the <c>Idempotency-Key</c> header: the first successful response is stored and replayed for repeats
/// of the same request (same user, route and payload). Failures are not stored so clients can retry.
/// </summary>
internal sealed class IdempotencyFilter(
    IIdempotencyStore store,
    ICurrentUser currentUser,
    IOptions<Microsoft.AspNetCore.Http.Json.JsonOptions> jsonOptions,
    ILogger<IdempotencyFilter> logger) : IEndpointFilter
{
    public const string HeaderName = "Idempotency-Key";
    public const string ReplayedHeader = "Idempotent-Replayed";

    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        var http = context.HttpContext;
        var key = http.Request.Headers[HeaderName].ToString();
        if (string.IsNullOrEmpty(key))
        {
            return await next(context);
        }

        if (key.Length > 100 || !key.All(c => char.IsAsciiLetterOrDigit(c) || c is '-' or '_'))
        {
            return Problem(StatusCodes.Status400BadRequest, "idempotency.invalid_key", "Cabeçalho Idempotency-Key inválido.");
        }

        var scope = $"{currentUser.UserId}:{http.Request.Method}:{http.Request.Path}:{key}";
        var fingerprint = Fingerprint(context.Arguments);

        IdempotencyBeginResult begin;
        try
        {
            begin = await store.TryBeginAsync(scope, fingerprint);
        }
        catch (RedisException exception)
        {
            logger.LogWarning(exception, "Idempotency store unavailable; processing request without replay protection");
            return await next(context);
        }

        switch (begin.State)
        {
            case IdempotencyState.InProgress:
                return Problem(StatusCodes.Status409Conflict, "idempotency.in_progress", "Um pedido idêntico ainda está a ser processado.");
            case IdempotencyState.FingerprintMismatch:
                return Problem(StatusCodes.Status422UnprocessableEntity, "idempotency.key_reused", "A chave de idempotência já foi usada com outros dados.");
            case IdempotencyState.Completed:
                http.Response.Headers[ReplayedHeader] = "true";
                return new ReplayResult(begin.Response!);
        }

        try
        {
            var result = await next(context);
            if (result is IStatusCodeHttpResult { StatusCode: >= 200 and < 300 } success)
            {
                var body = result is IValueHttpResult value ? JsonSerializer.Serialize(value.Value, jsonOptions.Value.SerializerOptions) : null;
                var location = result.GetType().GetProperty("Location")?.GetValue(result) as string;
                await store.CompleteAsync(scope, fingerprint, new StoredResponse(success.StatusCode!.Value, body, location));
            }
            else
            {
                await store.ReleaseAsync(scope);
            }

            return result;
        }
        catch
        {
            await TryReleaseAsync(scope);
            throw;
        }
    }

    private async Task TryReleaseAsync(string scope)
    {
        try
        {
            await store.ReleaseAsync(scope);
        }
        catch (RedisException exception)
        {
            logger.LogWarning(exception, "Could not release idempotency key");
        }
    }

    private string Fingerprint(IList<object?> arguments)
    {
        var payload = arguments.Where(a => a is not (null or CancellationToken or HttpContext or IDispatcher)).ToArray();
        var json = JsonSerializer.Serialize(payload, jsonOptions.Value.SerializerOptions);
        return Convert.ToHexStringLower(SHA256.HashData(Encoding.UTF8.GetBytes(json)));
    }

    private static ProblemHttpResult Problem(int status, string code, string detail) =>
        TypedResults.Problem(detail: detail, statusCode: status, extensions: new Dictionary<string, object?> { ["code"] = code });

    private sealed class ReplayResult(StoredResponse response) : IResult
    {
        public async Task ExecuteAsync(HttpContext httpContext)
        {
            httpContext.Response.StatusCode = response.StatusCode;
            if (response.Location is not null)
            {
                httpContext.Response.Headers.Location = response.Location;
            }

            if (response.Body is not null)
            {
                httpContext.Response.ContentType = "application/json; charset=utf-8";
                await httpContext.Response.WriteAsync(response.Body);
            }
        }
    }
}

internal static class IdempotencyEndpointExtensions
{
    public static RouteHandlerBuilder WithIdempotency(this RouteHandlerBuilder builder) =>
        builder.AddEndpointFilter<IdempotencyFilter>()
            .WithMetadata(new IdempotencyHeaderMetadata());
}

/// <summary>Marks endpoints that accept <c>Idempotency-Key</c> (documented in OpenAPI).</summary>
internal sealed record IdempotencyHeaderMetadata;
