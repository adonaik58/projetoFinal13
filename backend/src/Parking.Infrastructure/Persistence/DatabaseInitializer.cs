using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Parking.Application.Abstractions;
using Parking.Application.Common;
using Parking.Domain.Settings;
using Parking.Domain.Users;

namespace Parking.Infrastructure.Persistence;

public sealed class BootstrapOptions
{
    public const string SectionName = "Bootstrap";

    public string? AdminUsername { get; set; }

    public string? AdminPassword { get; set; }

    public string AdminFullName { get; set; } = "Administrador";

    public decimal RatePerMinute { get; set; } = 10m;

    public int FreeHours { get; set; }

    public int MaxSpaces { get; set; } = 250;
}

/// <summary>
/// Applies migrations and the minimum data required to operate (tariff row, first administrator).
/// Runs as a one-shot "migrator" process before API replicas start.
/// </summary>
public sealed class DatabaseInitializer(
    AppDbContext db,
    IPasswordHasher hasher,
    IAuditLogger audit,
    IOptions<BootstrapOptions> options,
    TimeProvider timeProvider,
    ILogger<DatabaseInitializer> logger)
{
    private const int MaxAttempts = 10;

    public async Task InitializeAsync(CancellationToken cancellationToken)
    {
        await MigrateAsync(cancellationToken);
        await EnsureSettingsAsync(cancellationToken);
        await EnsureAdministratorAsync(cancellationToken);
    }

    private async Task MigrateAsync(CancellationToken cancellationToken)
    {
        for (var attempt = 1; ; attempt++)
        {
            try
            {
                var pending = (await db.Database.GetPendingMigrationsAsync(cancellationToken)).ToList();
                logger.LogInformation("Applying {Count} pending migration(s): {Migrations}", pending.Count, pending);
                await db.Database.MigrateAsync(cancellationToken);
                return;
            }
            catch (Exception exception) when (attempt < MaxAttempts && exception is not OperationCanceledException && IsTransient(exception))
            {
                logger.LogWarning(exception, "Database not reachable (attempt {Attempt}/{Max}); retrying", attempt, MaxAttempts);
                await Task.Delay(TimeSpan.FromSeconds(Math.Min(30, attempt * 3)), cancellationToken);
            }
        }
    }

    private static bool IsTransient(Exception exception) =>
        exception is MySql.Data.MySqlClient.MySqlException or System.Net.Sockets.SocketException or TimeoutException
        || exception.InnerException is not null && IsTransient(exception.InnerException);

    private async Task EnsureSettingsAsync(CancellationToken cancellationToken)
    {
        if (await db.ParkingSettings.AnyAsync(cancellationToken))
        {
            return;
        }

        var o = options.Value;
        db.ParkingSettings.Add(ParkingSettings.Create(o.RatePerMinute, o.FreeHours, o.MaxSpaces, timeProvider.GetUtcNow().UtcDateTime));
        await db.SaveChangesAsync(cancellationToken);
        logger.LogInformation("Created parking settings (rate {Rate}/min, {FreeHours} free hour(s), capacity {MaxSpaces})", o.RatePerMinute, o.FreeHours, o.MaxSpaces);
    }

    private async Task EnsureAdministratorAsync(CancellationToken cancellationToken)
    {
        if (await db.Users.AnyAsync(cancellationToken))
        {
            return;
        }

        var o = options.Value;
        if (string.IsNullOrWhiteSpace(o.AdminUsername) || string.IsNullOrWhiteSpace(o.AdminPassword))
        {
            throw new InvalidOperationException(
                "No users exist. Set Bootstrap__AdminUsername and Bootstrap__AdminPassword to create the first administrator.");
        }

        var passwordValid = o.AdminPassword.Length >= PasswordRules.MinLength
                            && o.AdminPassword.Any(char.IsLetter) && o.AdminPassword.Any(char.IsDigit);
        if (!passwordValid)
        {
            throw new InvalidOperationException(
                $"Bootstrap__AdminPassword must have at least {PasswordRules.MinLength} characters with letters and digits.");
        }

        var admin = User.Create(o.AdminUsername, o.AdminFullName, hasher.Hash(o.AdminPassword), Role.Admin);
        db.Users.Add(admin);
        await db.SaveChangesAsync(cancellationToken);
        audit.RecordFor(null, "system", "system.bootstrap_admin_created", nameof(User), admin.Username);
        await db.SaveChangesAsync(cancellationToken);
        logger.LogInformation("Created bootstrap administrator {Username}", admin.Username);
    }
}
