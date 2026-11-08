using Parking.Application.Abstractions;
using Parking.Domain.Users;

namespace Parking.Application.Features.Auth;

public sealed record AuthUserDto(int Id, string Username, string FullName, Role Role, IReadOnlyList<string> Permissions)
{
    public static AuthUserDto From(User user) => new(user.Id, user.Username, user.FullName, user.Role, user.Permissions);
}

/// <summary>Result of a login/refresh. The refresh token is delivered as an HttpOnly cookie, never in the body.</summary>
public sealed record AuthResult(
    string AccessToken,
    DateTime AccessTokenExpiresAt,
    string RefreshToken,
    DateTime RefreshTokenExpiresAt,
    AuthUserDto User);

/// <summary>Issues an access token plus a new refresh token in the given family.</summary>
internal sealed class AuthSessionFactory(IAppDbContext db, ITokenService tokens, IBusinessClock clock, ICurrentUser currentUser)
{
    public (AuthResult Result, RefreshToken Entity) Issue(User user, Guid familyId)
    {
        var access = tokens.CreateAccessToken(user);
        var refresh = tokens.GenerateRefreshToken();
        var entity = RefreshToken.Issue(
            user.Id, refresh.Hash, familyId, clock.UtcNow, tokens.RefreshTokenLifetime, currentUser.IpAddress);
        db.RefreshTokens.Add(entity);

        var result = new AuthResult(access.Token, access.ExpiresAt, refresh.Token, entity.ExpiresAt, AuthUserDto.From(user));
        return (result, entity);
    }
}
