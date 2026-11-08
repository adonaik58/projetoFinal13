using Microsoft.EntityFrameworkCore;
using Parking.Application.Abstractions;
using Parking.Application.Common.Exceptions;
using Parking.Domain.Audit;
using Parking.Domain.Customers;
using Parking.Domain.Settings;
using Parking.Domain.Spaces;
using Parking.Domain.Tickets;
using Parking.Domain.Users;
using Parking.Domain.Vehicles;

namespace Parking.Infrastructure.Persistence;

public sealed class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options), IAppDbContext
{
    public DbSet<User> Users => Set<User>();

    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

    public DbSet<ParkingSpace> ParkingSpaces => Set<ParkingSpace>();

    public DbSet<Customer> Customers => Set<Customer>();

    public DbSet<Vehicle> Vehicles => Set<Vehicle>();

    public DbSet<VehicleBrand> VehicleBrands => Set<VehicleBrand>();

    public DbSet<VehicleModel> VehicleModels => Set<VehicleModel>();

    public DbSet<Ticket> Tickets => Set<Ticket>();

    public DbSet<ParkingSettings> ParkingSettings => Set<ParkingSettings>();

    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            return await base.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            throw new ConflictException(
                "conflict.concurrent_update", "O registo foi alterado por outro utilizador. Atualize e tente novamente.");
        }
        catch (DbUpdateException exception) when (DbExceptionTranslator.TryTranslate(exception, out var conflict))
        {
            throw conflict;
        }
    }

    public async Task<TResult> InTransactionAsync<TResult>(
        Func<CancellationToken, Task<TResult>> operation, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(operation);
        var strategy = Database.CreateExecutionStrategy();
        return await strategy.ExecuteAsync(
            operation,
            static async (context, op, ct) =>
            {
                var db = (AppDbContext)context;
                await using var transaction = await db.Database.BeginTransactionAsync(ct);
                var result = await op(ct);
                await transaction.CommitAsync(ct);
                return result;
            },
            null,
            cancellationToken);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
        modelBuilder.ApplySnakeCaseNames();
    }

    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        configurationBuilder.Properties<DateTime>().HaveConversion<UtcDateTimeConverter>().HavePrecision(6);
        configurationBuilder.Properties<DateTime?>().HaveConversion<NullableUtcDateTimeConverter>().HavePrecision(6);
        configurationBuilder.Properties<DateOnly>().HaveConversion<DateOnlyConverter>().HaveColumnType("date");
        configurationBuilder.Properties<decimal>().HavePrecision(12, 2);
        configurationBuilder.Properties<string>().HaveMaxLength(255);
        configurationBuilder.Properties<Role>().HaveConversion<string>().HaveMaxLength(16);
        configurationBuilder.Properties<SpaceStatus>().HaveConversion<string>().HaveMaxLength(16);
        configurationBuilder.Properties<TicketStatus>().HaveConversion<string>().HaveMaxLength(16);
    }
}
