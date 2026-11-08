using FluentValidation;
using FluentValidation.Results;
using Microsoft.EntityFrameworkCore;
using Parking.Application.Abstractions;
using Parking.Application.Abstractions.Messaging;
using Parking.Application.Common;
using Parking.Application.Common.Behaviors;
using Parking.Application.Common.Exceptions;
using Parking.Domain.Users;

namespace Parking.Application.Features.Auth;

public sealed record GetCurrentUserQuery : IQuery<AuthUserDto>;

internal sealed class GetCurrentUserHandler(IAppDbContext db, ICurrentUser currentUser) : IQueryHandler<GetCurrentUserQuery, AuthUserDto>
{
    public async Task<AuthUserDto> Handle(GetCurrentUserQuery request, CancellationToken cancellationToken)
    {
        var user = await db.Users.AsNoTracking()
            .SingleOrDefaultAsync(u => u.Id == currentUser.RequiredUserId && u.IsActive, cancellationToken);
        return user is null
            ? throw new UnauthorizedException("auth.session_expired", "A sessão expirou. Inicie sessão novamente.")
            : AuthUserDto.From(user);
    }
}

/// <summary>Changes the caller's password and revokes every other session.</summary>
/// <param name="CurrentPassword">Password currently in use.</param>
/// <param name="NewPassword">Replacement password.</param>
/// <param name="CurrentRefreshToken">Session kept alive; every other session of the user is revoked.</param>
public sealed record ChangePasswordCommand(string CurrentPassword, string NewPassword, string? CurrentRefreshToken = null)
    : ICommand, IUsersChange;

internal sealed class ChangePasswordValidator : AbstractValidator<ChangePasswordCommand>
{
    public ChangePasswordValidator()
    {
        RuleFor(x => x.CurrentPassword).NotEmpty().WithMessage("Indique a senha atual.");
        RuleFor(x => x.NewPassword).StrongPassword()
            .NotEqual(x => x.CurrentPassword).WithMessage("A nova senha deve ser diferente da atual.");
    }
}

internal sealed class ChangePasswordHandler(
    IAppDbContext db,
    IPasswordHasher hasher,
    ITokenService tokens,
    ICurrentUser currentUser,
    IAuditLogger audit,
    IBusinessClock clock) : ICommandHandler<ChangePasswordCommand>
{
    public async Task<Unit> Handle(ChangePasswordCommand request, CancellationToken cancellationToken)
    {
        var user = await db.Users.SingleOrDefaultAsync(u => u.Id == currentUser.RequiredUserId, cancellationToken)
            ?? throw new UnauthorizedException("auth.session_expired", "A sessão expirou. Inicie sessão novamente.");

        if (hasher.Verify(user.PasswordHash, request.CurrentPassword) == PasswordVerification.Failed)
        {
            throw new ValidationException([new ValidationFailure(nameof(request.CurrentPassword), "A senha atual está incorreta.")]);
        }

        user.SetPasswordHash(hasher.Hash(request.NewPassword));

        var keepHash = string.IsNullOrWhiteSpace(request.CurrentRefreshToken) ? null : tokens.HashRefreshToken(request.CurrentRefreshToken);
        await SessionRevocation.RevokeAllAsync(db, user.Id, clock.UtcNow, "password_changed", keepHash, cancellationToken);

        audit.Record("auth.password_changed", nameof(User), user.Id.ToString(System.Globalization.CultureInfo.InvariantCulture));
        await db.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }
}

internal static class SessionRevocation
{
    public static async Task RevokeAllAsync(
        IAppDbContext db, int userId, DateTime now, string reason, string? exceptTokenHash, CancellationToken cancellationToken)
    {
        var active = await db.RefreshTokens
            .Where(r => r.UserId == userId && r.RevokedAt == null && r.ExpiresAt > now && r.TokenHash != exceptTokenHash)
            .ToListAsync(cancellationToken);
        foreach (var token in active)
        {
            token.Revoke(now, reason);
        }
    }
}
