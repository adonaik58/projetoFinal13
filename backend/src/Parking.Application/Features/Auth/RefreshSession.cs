using System.Globalization;
using Microsoft.EntityFrameworkCore;
using Parking.Application.Abstractions;
using Parking.Application.Abstractions.Messaging;
using Parking.Application.Common.Exceptions;
using Parking.Domain.Users;

namespace Parking.Application.Features.Auth;

public sealed record RefreshSessionCommand(string? RefreshToken) : ICommand<AuthResult>;

internal sealed class RefreshSessionHandler(
    IAppDbContext db,
    ITokenService tokens,
    IAuditLogger audit,
    IBusinessClock clock,
    AuthSessionFactory sessions) : ICommandHandler<RefreshSessionCommand, AuthResult>
{
    /// <summary>
    /// A token rotated moments ago may legitimately be presented again (parallel tabs refreshing at once);
    /// inside this window it is not treated as theft.
    /// </summary>
    internal static readonly TimeSpan ReuseGracePeriod = TimeSpan.FromSeconds(30);

    public async Task<AuthResult> Handle(RefreshSessionCommand request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.RefreshToken))
        {
            throw SessionExpired();
        }

        var hash = tokens.HashRefreshToken(request.RefreshToken);
        var stored = await db.RefreshTokens.Include(r => r.User).SingleOrDefaultAsync(r => r.TokenHash == hash, cancellationToken);
        if (stored is null)
        {
            throw SessionExpired();
        }

        var now = clock.UtcNow;
        if (stored.RevokedAt is not null)
        {
            var withinGrace = stored.RevokedReason == "rotated" && now - stored.RevokedAt.Value <= ReuseGracePeriod;
            if (!withinGrace)
            {
                if (stored.ReplacedByTokenHash is not null)
                {
                    await RevokeFamilyAsync(stored, now, cancellationToken);
                }

                throw SessionExpired();
            }
        }
        else if (!stored.IsActive(now))
        {
            throw SessionExpired();
        }

        if (!stored.User.IsActive)
        {
            stored.Revoke(now, "user_inactive");
            await db.SaveChangesAsync(cancellationToken);
            throw SessionExpired();
        }

        var (result, replacement) = sessions.Issue(stored.User, stored.FamilyId);
        stored.Revoke(now, "rotated", replacement.TokenHash);
        await db.SaveChangesAsync(cancellationToken);
        return result;
    }

    private async Task RevokeFamilyAsync(RefreshToken stored, DateTime now, CancellationToken cancellationToken)
    {
        var family = await db.RefreshTokens
            .Where(r => r.FamilyId == stored.FamilyId && r.RevokedAt == null)
            .ToListAsync(cancellationToken);
        foreach (var token in family)
        {
            token.Revoke(now, "reuse_detected");
        }

        audit.RecordFor(
            stored.UserId, stored.User.Username, "auth.refresh.reuse_detected", nameof(User),
            stored.UserId.ToString(CultureInfo.InvariantCulture), new { family = stored.FamilyId, revoked = family.Count });
        await db.SaveChangesAsync(cancellationToken);
    }

    private static UnauthorizedException SessionExpired() =>
        new("auth.session_expired", "A sessão expirou. Inicie sessão novamente.");
}
