using System.Buffers.Text;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using Parking.Application.Abstractions;
using Parking.Domain.Users;

namespace Parking.Infrastructure.Security;

public sealed class JwtOptions
{
    public const string SectionName = "Jwt";

    [Required]
    public string Issuer { get; set; } = "parking-api";

    [Required]
    public string Audience { get; set; } = "parking-web";

    /// <summary>HMAC-SHA256 key, at least 32 bytes. Supplied through secrets/environment only.</summary>
    [Required]
    [MinLength(32)]
    public string SigningKey { get; set; } = string.Empty;

    [Range(1, 60)]
    public int AccessTokenMinutes { get; set; } = 15;

    [Range(1, 90)]
    public int RefreshTokenDays { get; set; } = 7;

    public SymmetricSecurityKey CreateSigningKey() => new(Encoding.UTF8.GetBytes(SigningKey));
}

public static class JwtClaimNames
{
    public const string Subject = JwtRegisteredClaimNames.Sub;
    public const string Username = "name";
    public const string Role = "role";
}

internal sealed class JwtTokenService(IOptions<JwtOptions> options, TimeProvider timeProvider) : ITokenService
{
    private readonly JsonWebTokenHandler _handler = new();

    public TimeSpan RefreshTokenLifetime => TimeSpan.FromDays(options.Value.RefreshTokenDays);

    public AccessToken CreateAccessToken(User user)
    {
        ArgumentNullException.ThrowIfNull(user);
        var o = options.Value;
        var now = timeProvider.GetUtcNow().UtcDateTime;
        var expires = now.AddMinutes(o.AccessTokenMinutes);
        var token = _handler.CreateToken(new SecurityTokenDescriptor
        {
            Issuer = o.Issuer,
            Audience = o.Audience,
            IssuedAt = now,
            NotBefore = now,
            Expires = expires,
            SigningCredentials = new SigningCredentials(o.CreateSigningKey(), SecurityAlgorithms.HmacSha256),
            Claims = new Dictionary<string, object>
            {
                [JwtClaimNames.Subject] = user.Id.ToString(CultureInfo.InvariantCulture),
                [JwtClaimNames.Username] = user.Username,
                [JwtClaimNames.Role] = user.Role.ToString(),
                [JwtRegisteredClaimNames.Jti] = Guid.NewGuid().ToString("N"),
            },
        });
        return new AccessToken(token, expires);
    }

    public GeneratedRefreshToken GenerateRefreshToken()
    {
        var token = Base64Url.EncodeToString(RandomNumberGenerator.GetBytes(64));
        return new GeneratedRefreshToken(token, HashRefreshToken(token));
    }

    public string HashRefreshToken(string token) => Convert.ToHexStringLower(SHA256.HashData(Encoding.UTF8.GetBytes(token)));
}
