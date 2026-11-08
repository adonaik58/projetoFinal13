using FluentValidation;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Parking.Application.Common.Exceptions;
using Parking.Domain.Common;

namespace Parking.Api.Infrastructure;

/// <summary>Single translation point from exceptions to RFC 7807 problem details.</summary>
internal sealed class GlobalExceptionHandler(IProblemDetailsService problemDetailsService, ILogger<GlobalExceptionHandler> logger)
    : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        var (status, title, code) = exception switch
        {
            ValidationException => (StatusCodes.Status400BadRequest, "Dados inválidos.", "validation.failed"),
            BadHttpRequestException bad => (bad.StatusCode, "Pedido inválido.", "request.invalid"),
            UnauthorizedException e => (StatusCodes.Status401Unauthorized, "Não autenticado.", e.Code),
            ForbiddenException e => (StatusCodes.Status403Forbidden, "Operação não permitida.", e.Code),
            NotFoundException e => (StatusCodes.Status404NotFound, "Recurso não encontrado.", e.Code),
            ConflictException e => (StatusCodes.Status409Conflict, "Conflito.", e.Code),
            DomainException e => (StatusCodes.Status409Conflict, "Regra de negócio violada.", e.Code),
            TooManyRequestsException e => (StatusCodes.Status429TooManyRequests, "Demasiados pedidos.", e.Code),
            OperationCanceledException when httpContext.RequestAborted.IsCancellationRequested => (499, "Pedido cancelado.", "request.cancelled"),
            _ => (StatusCodes.Status500InternalServerError, "Ocorreu um erro inesperado.", "server.error"),
        };

        if (status >= 500)
        {
            logger.LogError(exception, "Unhandled exception processing {Method} {Path}", httpContext.Request.Method, httpContext.Request.Path);
        }
        else
        {
            logger.LogInformation("Request failed with {Status} {Code}: {Message}", status, code, exception.Message);
        }

        if (exception is TooManyRequestsException tooMany)
        {
            httpContext.Response.Headers.RetryAfter = ((int)Math.Ceiling(tooMany.RetryAfter.TotalSeconds)).ToString(System.Globalization.CultureInfo.InvariantCulture);
        }

        var problem = new ProblemDetails
        {
            Status = status,
            Title = title,
            Type = $"https://httpstatuses.io/{status}",
            Detail = status >= 500
                ? "Tente novamente. Se o problema persistir, contacte o suporte indicando o correlationId."
                : exception is BadHttpRequestException ? "O corpo ou os parâmetros do pedido são inválidos." : exception.Message,
        };
        problem.Extensions["code"] = code;
        if (exception is ValidationException validation)
        {
            problem.Detail = "Corrija os campos assinalados.";
            problem.Extensions["errors"] = validation.Errors
                .GroupBy(e => ToCamelPath(e.PropertyName))
                .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).Distinct().ToArray());
        }

        httpContext.Response.StatusCode = status;
        return await problemDetailsService.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = httpContext,
            Exception = exception,
            ProblemDetails = problem,
        });
    }

    /// <summary><c>Customer.NationalId</c> → <c>customer.nationalId</c> (matches the JSON body).</summary>
    internal static string ToCamelPath(string propertyName) =>
        string.Join('.', propertyName.Split('.').Select(segment =>
            segment.Length == 0 ? segment : char.ToLowerInvariant(segment[0]) + segment[1..]));
}
