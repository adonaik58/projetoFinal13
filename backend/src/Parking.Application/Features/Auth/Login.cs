using System.Globalization;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Parking.Application.Abstractions;
using Parking.Application.Abstractions.Messaging;
using Parking.Application.Common;
using Parking.Application.Common.Behaviors;
using Parking.Application.Common.Exceptions;
using Parking.Domain.Users;

namespace Parking.Application.Features.Auth;

public sealed record LoginCommand(string Username, string Password) : ICommand<AuthResult>, IUsersChange;

internal sealed class LoginValidator : AbstractValidator<LoginCommand>
{
    public LoginValidator()
    {
        RuleFor(x => x.Username).NotEmpty().WithMessage("Indique o nome de utilizador.").MaximumLength(User.UsernameMaxLength);
        RuleFor(x => x.Password).NotEmpty().WithMessage("Indique a senha.").MaximumLength(PasswordRules.MaxLength);
    }
}

internal sealed class LoginHandler(
    IAppDbContext db,
    IPasswordHasher hasher,
    ILoginThrottle throttle,
    IAuditLogger audit,
    IBusinessClock clock,
    AuthSessionFactory sessions,
    ParkingMetrics metrics) : ICommandHandler<LoginCommand, AuthResult>
{
    // Verifying against a dummy hash for unknown users keeps response times uniform (no user enumeration).
    private static string? _dummyHash;

    public async Task<AuthResult> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var username = request.Username.Trim();

        var lockout = await throttle.GetLockoutAsync(username, cancellationToken);
        if (lockout is not null)
        {
            throw new TooManyRequestsException(
                "auth.locked_out", "Demasiadas tentativas falhadas. Tente novamente mais tarde.", lockout.Value);
        }

        var user = await db.Users.SingleOrDefaultAsync(u => u.Username == username, cancellationToken);
        var verification = user is null
            ? VerifyDummy(request.Password)
            : hasher.Verify(user.PasswordHash, request.Password);

        if (user is null || verification == PasswordVerification.Failed || !user.IsActive)
        {
            await throttle.RegisterFailureAsync(username, cancellationToken);
            var reason = user is null ? "unknown_user" : verification == PasswordVerification.Failed ? "invalid_password" : "inactive";
            audit.RecordFor(user?.Id, username, "auth.login.failed", nameof(User), user?.Id.ToString(CultureInfo.InvariantCulture), new { reason });
            await db.SaveChangesAsync(cancellationToken);
            metrics.LoginFailed();
            throw new UnauthorizedException("auth.invalid_credentials", "Nome de utilizador ou senha incorretos.");
        }

        if (verification == PasswordVerification.SuccessRehashNeeded)
        {
            user.SetPasswordHash(hasher.Hash(request.Password));
        }

        await throttle.ResetAsync(username, cancellationToken);
        user.RecordLogin(clock.UtcNow);

        var (result, _) = sessions.Issue(user, Guid.NewGuid());
        audit.RecordFor(user.Id, user.Username, "auth.login.succeeded", nameof(User), user.Id.ToString(CultureInfo.InvariantCulture));
        await db.SaveChangesAsync(cancellationToken);
        return result;
    }

    private PasswordVerification VerifyDummy(string password)
    {
        _dummyHash ??= hasher.Hash(Guid.NewGuid().ToString());
        hasher.Verify(_dummyHash, password);
        return PasswordVerification.Failed;
    }
}
