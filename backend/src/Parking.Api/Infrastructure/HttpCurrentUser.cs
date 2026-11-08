using System.Globalization;
using System.Security.Claims;
using Parking.Application.Abstractions;
using Parking.Application.Common.Exceptions;
using Parking.Domain.Users;
using Parking.Infrastructure.Security;

namespace Parking.Api.Infrastructure;

internal sealed class HttpCurrentUser(IHttpContextAccessor accessor) : ICurrentUser
{
    private ClaimsPrincipal? Principal => accessor.HttpContext?.User;

    public int? UserId =>
        int.TryParse(Principal?.FindFirstValue(JwtClaimNames.Subject), NumberStyles.Integer, CultureInfo.InvariantCulture, out var id) ? id : null;

    public string? Username => Principal?.FindFirstValue(JwtClaimNames.Username);

    public Role? Role => Enum.TryParse<Role>(Principal?.FindFirstValue(JwtClaimNames.Role), out var role) ? role : null;

    public string? IpAddress => accessor.HttpContext?.Connection.RemoteIpAddress?.ToString();

    public string? CorrelationId => accessor.HttpContext?.Items[CorrelationIdMiddleware.ItemKey] as string;

    public int RequiredUserId => UserId ?? throw new UnauthorizedException("auth.required", "Autenticação necessária.");
}
