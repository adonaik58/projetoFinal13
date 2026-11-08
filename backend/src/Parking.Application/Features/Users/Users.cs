using System.Globalization;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Parking.Application.Abstractions;
using Parking.Application.Abstractions.Messaging;
using Parking.Application.Common;
using Parking.Application.Common.Behaviors;
using Parking.Application.Common.Exceptions;
using Parking.Application.Common.Paging;
using Parking.Application.Features.Auth;
using Parking.Domain.Users;

namespace Parking.Application.Features.Users;

public sealed record UserDto(
    int Id, string Username, string FullName, Role Role, bool IsActive, bool IsOnline, DateTime? LastLoginAt, DateTime CreatedAt);

internal static class UserProjection
{
    /// <summary>"Online" = holds at least one valid (non-revoked, non-expired) refresh session.</summary>
    public static IQueryable<UserDto> ToDto(this IQueryable<User> users, IAppDbContext db, DateTime now) =>
        users.Select(u => new UserDto(
            u.Id,
            u.Username,
            u.FullName,
            u.Role,
            u.IsActive,
            db.RefreshTokens.Any(r => r.UserId == u.Id && r.RevokedAt == null && r.ExpiresAt > now),
            u.LastLoginAt,
            u.CreatedAt));
}

internal static class UserRules
{
    public const string UsernamePattern = "^[A-Za-z0-9._-]+$";

    public static void ApplyProfileRules<T>(this AbstractValidator<T> validator, Func<T, string> username, Func<T, string> fullName, Func<T, Role> role)
    {
        validator.RuleFor(x => username(x)).NotEmpty().WithMessage("O nome de utilizador é obrigatório.")
            .Length(3, User.UsernameMaxLength).WithMessage($"O nome de utilizador deve ter entre 3 e {User.UsernameMaxLength} caracteres.")
            .Matches(UsernamePattern).WithMessage("Use apenas letras, números, ponto, hífen ou sublinhado.")
            .OverridePropertyName("username");
        validator.RuleFor(x => fullName(x)).NotEmpty().WithMessage("O nome completo é obrigatório.")
            .Length(2, User.FullNameMaxLength)
            .OverridePropertyName("fullName");
        validator.RuleFor(x => role(x)).IsInEnum().WithMessage("Perfil inválido.").OverridePropertyName("role");
    }

    public static async Task EnsureUsernameAvailableAsync(IAppDbContext db, string username, int? exceptId, CancellationToken cancellationToken)
    {
        var normalized = username.Trim();
        if (await db.Users.AnyAsync(u => u.Username == normalized && u.Id != exceptId, cancellationToken))
        {
            throw new ConflictException("user.username_taken", "Já existe um utilizador com este nome.");
        }
    }

    /// <summary>Prevents locking everyone out by removing the last active administrator.</summary>
    public static async Task EnsureAnotherActiveAdminAsync(IAppDbContext db, int userId, CancellationToken cancellationToken)
    {
        if (!await db.Users.AnyAsync(u => u.Id != userId && u.Role == Role.Admin && u.IsActive, cancellationToken))
        {
            throw new ConflictException("user.last_admin", "Não é possível remover o último administrador ativo.");
        }
    }

    public static async Task<User> FindAsync(IAppDbContext db, int id, CancellationToken cancellationToken) =>
        await db.Users.SingleOrDefaultAsync(u => u.Id == id, cancellationToken)
        ?? throw new NotFoundException("user.not_found", "Utilizador não encontrado.");
}

// ---- Queries ----

public sealed record ListUsersQuery(int Page = 1, int PageSize = 20, string? Search = null, Role? Role = null, bool? IsActive = null)
    : IQuery<PagedResult<UserDto>>, IPagedRequest;

internal sealed class ListUsersValidator : AbstractValidator<ListUsersQuery>
{
    public ListUsersValidator()
    {
        this.ApplyPagingRules();
        RuleFor(x => x.Search).MaximumLength(100);
    }
}

internal sealed class ListUsersHandler(IAppDbContext db, IBusinessClock clock) : IQueryHandler<ListUsersQuery, PagedResult<UserDto>>
{
    public Task<PagedResult<UserDto>> Handle(ListUsersQuery request, CancellationToken cancellationToken)
    {
        var query = db.Users.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = request.Search.Trim();
            query = query.Where(u => u.Username.Contains(search) || u.FullName.Contains(search));
        }

        if (request.Role is { } role)
        {
            query = query.Where(u => u.Role == role);
        }

        if (request.IsActive is { } isActive)
        {
            query = query.Where(u => u.IsActive == isActive);
        }

        return query.OrderBy(u => u.FullName).ThenBy(u => u.Id)
            .ToDto(db, clock.UtcNow)
            .ToPagedResultAsync(request, cancellationToken);
    }
}

public sealed record GetUserQuery(int Id) : IQuery<UserDto>;

internal sealed class GetUserHandler(IAppDbContext db, IBusinessClock clock) : IQueryHandler<GetUserQuery, UserDto>
{
    public async Task<UserDto> Handle(GetUserQuery request, CancellationToken cancellationToken) =>
        await db.Users.AsNoTracking().Where(u => u.Id == request.Id).ToDto(db, clock.UtcNow).SingleOrDefaultAsync(cancellationToken)
        ?? throw new NotFoundException("user.not_found", "Utilizador não encontrado.");
}

// ---- Commands ----

public sealed record CreateUserCommand(string Username, string FullName, Role Role, string Password) : ICommand<UserDto>, IUsersChange;

internal sealed class CreateUserValidator : AbstractValidator<CreateUserCommand>
{
    public CreateUserValidator()
    {
        this.ApplyProfileRules(x => x.Username, x => x.FullName, x => x.Role);
        RuleFor(x => x.Password).StrongPassword();
    }
}

internal sealed class CreateUserHandler(IAppDbContext db, IPasswordHasher hasher, IAuditLogger audit, IDispatcher dispatcher)
    : ICommandHandler<CreateUserCommand, UserDto>
{
    public async Task<UserDto> Handle(CreateUserCommand request, CancellationToken cancellationToken)
    {
        await UserRules.EnsureUsernameAvailableAsync(db, request.Username, null, cancellationToken);
        var user = User.Create(request.Username, request.FullName, hasher.Hash(request.Password), request.Role);

        var id = await db.InTransactionAsync(
            async ct =>
            {
                db.Users.Add(user);
                await db.SaveChangesAsync(ct);
                audit.Record("user.created", nameof(User), user.Id.ToString(CultureInfo.InvariantCulture), new { user.Username, role = user.Role.ToString() });
                await db.SaveChangesAsync(ct);
                return user.Id;
            },
            cancellationToken);
        return await dispatcher.Send(new GetUserQuery(id), cancellationToken);
    }
}

public sealed record UpdateUserCommand(int Id, string Username, string FullName, Role Role) : ICommand<UserDto>, IUsersChange;

internal sealed class UpdateUserValidator : AbstractValidator<UpdateUserCommand>
{
    public UpdateUserValidator() => this.ApplyProfileRules(x => x.Username, x => x.FullName, x => x.Role);
}

internal sealed class UpdateUserHandler(IAppDbContext db, ICurrentUser currentUser, IAuditLogger audit, IDispatcher dispatcher)
    : ICommandHandler<UpdateUserCommand, UserDto>
{
    public async Task<UserDto> Handle(UpdateUserCommand request, CancellationToken cancellationToken)
    {
        var user = await UserRules.FindAsync(db, request.Id, cancellationToken);
        var demoting = user.Role == Role.Admin && request.Role != Role.Admin;
        if (demoting && user.Id == currentUser.RequiredUserId)
        {
            throw new ForbiddenException("user.cannot_demote_self", "Não pode retirar o seu próprio perfil de administrador.");
        }

        if (demoting && user.IsActive)
        {
            await UserRules.EnsureAnotherActiveAdminAsync(db, user.Id, cancellationToken);
        }

        await UserRules.EnsureUsernameAvailableAsync(db, request.Username, user.Id, cancellationToken);

        var before = new { user.Username, user.FullName, role = user.Role.ToString() };
        user.UpdateProfile(request.Username, request.FullName, request.Role);
        audit.Record("user.updated", nameof(User), user.Id.ToString(CultureInfo.InvariantCulture),
            new { before, after = new { user.Username, user.FullName, role = user.Role.ToString() } });
        await db.SaveChangesAsync(cancellationToken);
        return await dispatcher.Send(new GetUserQuery(user.Id), cancellationToken);
    }
}

public sealed record ResetUserPasswordCommand(int Id, string NewPassword) : ICommand, IUsersChange;

internal sealed class ResetUserPasswordValidator : AbstractValidator<ResetUserPasswordCommand>
{
    public ResetUserPasswordValidator() => RuleFor(x => x.NewPassword).StrongPassword();
}

internal sealed class ResetUserPasswordHandler(IAppDbContext db, IPasswordHasher hasher, IAuditLogger audit, IBusinessClock clock)
    : ICommandHandler<ResetUserPasswordCommand>
{
    public async Task<Unit> Handle(ResetUserPasswordCommand request, CancellationToken cancellationToken)
    {
        var user = await UserRules.FindAsync(db, request.Id, cancellationToken);
        user.SetPasswordHash(hasher.Hash(request.NewPassword));
        await SessionRevocation.RevokeAllAsync(db, user.Id, clock.UtcNow, "password_reset", null, cancellationToken);
        audit.Record("user.password_reset", nameof(User), user.Id.ToString(CultureInfo.InvariantCulture));
        await db.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }
}

public sealed record SetUserActiveCommand(int Id, bool IsActive) : ICommand, IUsersChange;

internal sealed class SetUserActiveHandler(IAppDbContext db, ICurrentUser currentUser, IAuditLogger audit, IBusinessClock clock)
    : ICommandHandler<SetUserActiveCommand>
{
    public async Task<Unit> Handle(SetUserActiveCommand request, CancellationToken cancellationToken)
    {
        var user = await UserRules.FindAsync(db, request.Id, cancellationToken);
        if (user.IsActive == request.IsActive)
        {
            return Unit.Value;
        }

        if (request.IsActive)
        {
            user.Activate();
        }
        else
        {
            if (user.Id == currentUser.RequiredUserId)
            {
                throw new ForbiddenException("user.cannot_deactivate_self", "Não pode desativar a sua própria conta.");
            }

            if (user.Role == Role.Admin)
            {
                await UserRules.EnsureAnotherActiveAdminAsync(db, user.Id, cancellationToken);
            }

            user.Deactivate();
            await SessionRevocation.RevokeAllAsync(db, user.Id, clock.UtcNow, "user_deactivated", null, cancellationToken);
        }

        audit.Record(request.IsActive ? "user.activated" : "user.deactivated", nameof(User), user.Id.ToString(CultureInfo.InvariantCulture));
        await db.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }
}
