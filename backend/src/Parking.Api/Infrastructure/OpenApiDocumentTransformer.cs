using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;

namespace Parking.Api.Infrastructure;

/// <summary>Document metadata and the JWT bearer security scheme.</summary>
internal sealed class OpenApiDocumentTransformer : IOpenApiDocumentTransformer
{
    private const string SchemeId = "Bearer";

    public Task TransformAsync(OpenApiDocument document, OpenApiDocumentTransformerContext context, CancellationToken cancellationToken)
    {
        document.Info = new OpenApiInfo
        {
            Title = "Xyami.Park API",
            Version = "v1",
            Description = """
                API de gestão do parque de estacionamento.

                * Autenticação: `POST /api/v1/auth/login` devolve um access token JWT (Bearer, 15 min); o refresh token
                  é entregue num cookie HttpOnly e renovado em `POST /api/v1/auth/refresh` (cabeçalho `X-Requested-With: XMLHttpRequest`).
                * Erros: `application/problem+json` (RFC 7807) com `code`, `errors`, `traceId` e `correlationId`.
                * Check-in/check-out aceitam o cabeçalho `Idempotency-Key`.
                * Limites de pedidos indicados nos cabeçalhos `RateLimit-*`.
                """,
        };

        document.Components ??= new OpenApiComponents();
        document.Components.SecuritySchemes ??= new Dictionary<string, IOpenApiSecurityScheme>();
        document.Components.SecuritySchemes[SchemeId] = new OpenApiSecurityScheme
        {
            Type = SecuritySchemeType.Http,
            Scheme = "bearer",
            BearerFormat = "JWT",
            Description = "Access token obtido em /api/v1/auth/login.",
        };
        document.Security = [new OpenApiSecurityRequirement { [new OpenApiSecuritySchemeReference(SchemeId, document)] = [] }];
        return Task.CompletedTask;
    }
}
