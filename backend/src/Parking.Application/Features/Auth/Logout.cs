using System.Globalization;
using Microsoft.EntityFrameworkCore;
using Parking.Application.Abstractions;
using Parking.Application.Abstractions.Messaging;
using Parking.Domain.Users;

namespace Parking.Application.Features.Auth;

public sealed record LogoutCommand(string? RefreshToken) : ICommand;

internal sealed class LogoutHandler(IAppDbContext db, ITokenService tokens, IAuditLogger audit, IBusinessClock clock)
    : ICommandHandler<LogoutCommand>
{
    public async Task<Unit> Handle(LogoutCommand request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.RefreshToken))
        {
            return Unit.Value;
        }

        var hash = tokens.HashRefreshToken(request.RefreshToken);
        var stored = await db.RefreshTokens.Include(r => r.User).SingleOrDefaultAsync(r => r.TokenHash == hash, cancellationToken);
        if (stored is null || stored.RevokedAt is not null)
        {
            return Unit.Value;
        }

        stored.Revoke(clock.UtcNow, "logout");
        audit.RecordFor(stored.UserId, stored.User.Username, "auth.logout", nameof(User), stored.UserId.ToString(CultureInfo.InvariantCulture));
        await db.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }
}
