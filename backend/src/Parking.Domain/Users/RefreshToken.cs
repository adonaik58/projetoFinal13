using Parking.Domain.Common;

namespace Parking.Domain.Users;

/// <summary>
/// Rotating refresh token. Only the SHA-256 hash is stored; tokens issued from the same login share a
/// <see cref="FamilyId"/> so that reuse of a rotated token revokes the whole family.
/// </summary>
public sealed class RefreshToken : Entity<long>
{
    public const int TokenHashLength = 64;
    public const int IpMaxLength = 45;
    public const int ReasonMaxLength = 50;

    private RefreshToken()
    {
    }

    public int UserId { get; private set; }

    public User User { get; private set; } = null!;

    public string TokenHash { get; private set; } = null!;

    public Guid FamilyId { get; private set; }

    public DateTime CreatedAt { get; private set; }

    public DateTime ExpiresAt { get; private set; }

    public string? CreatedByIp { get; private set; }

    public DateTime? RevokedAt { get; private set; }

    public string? RevokedReason { get; private set; }

    public string? ReplacedByTokenHash { get; private set; }

    public static RefreshToken Issue(
        int userId, string tokenHash, Guid familyId, DateTime now, TimeSpan lifetime, string? ip) =>
        new()
        {
            UserId = userId,
            TokenHash = tokenHash,
            FamilyId = familyId,
            CreatedAt = now,
            ExpiresAt = now.Add(lifetime),
            CreatedByIp = ip is { Length: > IpMaxLength } ? ip[..IpMaxLength] : ip,
        };

    public bool IsActive(DateTime now) => RevokedAt is null && ExpiresAt > now;

    public void Revoke(DateTime now, string reason, string? replacedByTokenHash = null)
    {
        if (RevokedAt is not null)
        {
            return;
        }

        RevokedAt = now;
        RevokedReason = reason.Length > ReasonMaxLength ? reason[..ReasonMaxLength] : reason;
        ReplacedByTokenHash = replacedByTokenHash;
    }
}
