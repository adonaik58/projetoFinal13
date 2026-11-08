using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Parking.Application.Abstractions;
using Parking.Domain.Users;

namespace Parking.Infrastructure.Security;

/// <summary>
/// PBKDF2-HMAC-SHA512 (ASP.NET Core Identity v3 format) with 210 000 iterations (OWASP 2023+).
/// Legacy unsalted MD5 hashes (<c>md5:</c> prefix) are accepted once and reported as needing a rehash.
/// </summary>
internal sealed class PasswordHasherService : IPasswordHasher
{
    public const int Iterations = 210_000;

    private static readonly object Subject = new();

    private readonly PasswordHasher<object> _hasher = new(Options.Create(new PasswordHasherOptions
    {
        CompatibilityMode = PasswordHasherCompatibilityMode.IdentityV3,
        IterationCount = Iterations,
    }));

    public string Hash(string password) => _hasher.HashPassword(Subject, password);

    public PasswordVerification Verify(string hash, string password)
    {
        if (hash.StartsWith(User.LegacyMd5Prefix, StringComparison.Ordinal))
        {
            return VerifyLegacyMd5(hash[User.LegacyMd5Prefix.Length..], password);
        }

        return _hasher.VerifyHashedPassword(Subject, hash, password) switch
        {
            PasswordVerificationResult.Success => PasswordVerification.Success,
            PasswordVerificationResult.SuccessRehashNeeded => PasswordVerification.SuccessRehashNeeded,
            _ => PasswordVerification.Failed,
        };
    }

    private static PasswordVerification VerifyLegacyMd5(string expectedHex, string password)
    {
#pragma warning disable CA5351 // MD5 is only used to verify hashes migrated from the legacy system, which are upgraded immediately.
        var actual = Convert.ToHexStringLower(MD5.HashData(Encoding.UTF8.GetBytes(password)));
#pragma warning restore CA5351
        var matches = CryptographicOperations.FixedTimeEquals(
            Encoding.ASCII.GetBytes(actual), Encoding.ASCII.GetBytes(expectedHex.Trim().ToLowerInvariant()));
        return matches ? PasswordVerification.SuccessRehashNeeded : PasswordVerification.Failed;
    }
}
