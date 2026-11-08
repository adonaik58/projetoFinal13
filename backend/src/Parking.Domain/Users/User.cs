using Parking.Domain.Common;

namespace Parking.Domain.Users;

public sealed class User : AuditableEntity<int>
{
    public const int UsernameMaxLength = 50;
    public const int FullNameMaxLength = 150;
    public const int PasswordHashMaxLength = 255;

    /// <summary>Prefix of password hashes migrated from the legacy system (unsalted MD5).</summary>
    public const string LegacyMd5Prefix = "md5:";

    private User()
    {
    }

    public string Username { get; private set; } = null!;

    public string FullName { get; private set; } = null!;

    public string PasswordHash { get; private set; } = null!;

    public Role Role { get; private set; }

    public bool IsActive { get; private set; }

    public DateTime? LastLoginAt { get; private set; }

    public bool HasLegacyPassword => PasswordHash.StartsWith(LegacyMd5Prefix, StringComparison.Ordinal);

    public IReadOnlyList<string> Permissions => Users.Permissions.ForRole(Role);

    public static User Create(string username, string fullName, string passwordHash, Role role)
    {
        var user = new User { IsActive = true };
        user.UpdateProfile(username, fullName, role);
        user.SetPasswordHash(passwordHash);
        return user;
    }

    public void UpdateProfile(string username, string fullName, Role role)
    {
        Username = Guard.Text(username, 3, UsernameMaxLength, "user.invalid_username", "Nome de utilizador inválido.");
        FullName = Guard.Text(fullName, 2, FullNameMaxLength, "user.invalid_full_name", "Nome completo inválido.");
        if (!Enum.IsDefined(role))
        {
            throw new DomainException("user.invalid_role", "Perfil inválido.");
        }

        Role = role;
    }

    public void SetPasswordHash(string passwordHash)
    {
        if (string.IsNullOrWhiteSpace(passwordHash) || passwordHash.Length > PasswordHashMaxLength)
        {
            throw new DomainException("user.invalid_password_hash", "Hash de senha inválido.");
        }

        PasswordHash = passwordHash;
    }

    public void Activate() => IsActive = true;

    public void Deactivate() => IsActive = false;

    public void RecordLogin(DateTime now) => LastLoginAt = now;
}
