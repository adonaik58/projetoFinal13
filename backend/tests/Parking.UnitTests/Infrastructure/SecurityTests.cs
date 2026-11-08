using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Time.Testing;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using Parking.Application.Abstractions;
using Parking.Domain.Users;
using Parking.Infrastructure.Persistence;
using Parking.Infrastructure.Security;

namespace Parking.UnitTests.Infrastructure;

public sealed class SecurityTests
{
    private readonly PasswordHasherService _hasher = new();

    [Fact]
    public void Passwords_are_hashed_with_a_random_salt_and_verified()
    {
        var first = _hasher.Hash("Secret123");
        var second = _hasher.Hash("Secret123");

        Assert.NotEqual(first, second);
        Assert.Equal(PasswordVerification.Success, _hasher.Verify(first, "Secret123"));
        Assert.Equal(PasswordVerification.Failed, _hasher.Verify(first, "secret123"));
    }

    [Fact]
    public void Legacy_md5_hashes_verify_once_and_request_an_upgrade()
    {
#pragma warning disable CA5351 // reproducing the legacy format under test
        var legacy = User.LegacyMd5Prefix + Convert.ToHexStringLower(MD5.HashData(Encoding.UTF8.GetBytes("1234")));
#pragma warning restore CA5351

        Assert.Equal(PasswordVerification.SuccessRehashNeeded, _hasher.Verify(legacy, "1234"));
        Assert.Equal(PasswordVerification.Failed, _hasher.Verify(legacy, "12345"));
    }

    [Fact]
    public async Task Access_tokens_carry_identity_claims_and_expire()
    {
        var time = new FakeTimeProvider(new DateTimeOffset(2026, 11, 7, 10, 0, 0, TimeSpan.Zero));
        var options = Options.Create(new JwtOptions { SigningKey = new string('k', 48), AccessTokenMinutes = 15 });
        var service = new JwtTokenService(options, time);
        var user = User.Create("operador1", "Operador Um", "hash", Role.Operator);

        var token = service.CreateAccessToken(user);

        Assert.Equal(time.GetUtcNow().UtcDateTime.AddMinutes(15), token.ExpiresAt);
        var validation = await new JsonWebTokenHandler().ValidateTokenAsync(token.Token, new TokenValidationParameters
        {
            ValidIssuer = options.Value.Issuer,
            ValidAudience = options.Value.Audience,
            IssuerSigningKey = options.Value.CreateSigningKey(),
            ValidateLifetime = false,
        });
        Assert.True(validation.IsValid);
        Assert.Equal("operador1", validation.Claims[JwtClaimNames.Username]);
        Assert.Equal("Operator", validation.Claims[JwtClaimNames.Role]);
    }

    [Fact]
    public void Refresh_tokens_are_random_and_only_their_hash_is_stored()
    {
        var service = new JwtTokenService(Options.Create(new JwtOptions { SigningKey = new string('k', 48) }), TimeProvider.System);

        var first = service.GenerateRefreshToken();
        var second = service.GenerateRefreshToken();

        Assert.NotEqual(first.Token, second.Token);
        Assert.Equal(64, first.Hash.Length);
        Assert.Equal(first.Hash, service.HashRefreshToken(first.Token));
        Assert.DoesNotContain(first.Token, first.Hash, StringComparison.Ordinal);
    }

    [Theory]
    [InlineData("ParkingSpaces", "parking_spaces")]
    [InlineData("PK_users", "pk_users")]
    [InlineData("FK_tickets_users_ClosedByUserId", "fk_tickets_users_closed_by_user_id")]
    [InlineData("ux_tickets_active_space", "ux_tickets_active_space")]
    public void Database_names_are_converted_to_snake_case(string input, string expected) =>
        Assert.Equal(expected, SnakeCaseNamingExtensions.ToSnakeCase(input));
}
