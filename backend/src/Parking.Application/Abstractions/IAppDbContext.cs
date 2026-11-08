using Microsoft.EntityFrameworkCore;
using Parking.Domain.Audit;
using Parking.Domain.Customers;
using Parking.Domain.Settings;
using Parking.Domain.Spaces;
using Parking.Domain.Tickets;
using Parking.Domain.Users;
using Parking.Domain.Vehicles;

namespace Parking.Application.Abstractions;

public interface IAppDbContext
{
    DbSet<User> Users { get; }

    DbSet<RefreshToken> RefreshTokens { get; }

    DbSet<ParkingSpace> ParkingSpaces { get; }

    DbSet<Customer> Customers { get; }

    DbSet<Vehicle> Vehicles { get; }

    DbSet<VehicleBrand> VehicleBrands { get; }

    DbSet<VehicleModel> VehicleModels { get; }

    DbSet<Ticket> Tickets { get; }

    DbSet<ParkingSettings> ParkingSettings { get; }

    DbSet<AuditLog> AuditLogs { get; }

    /// <summary>Persists all pending changes atomically (single transaction).</summary>
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Runs an operation that saves more than once (e.g. needs a generated id) inside one database
    /// transaction, wrapped in the provider's retrying execution strategy.
    /// </summary>
    Task<TResult> InTransactionAsync<TResult>(Func<CancellationToken, Task<TResult>> operation, CancellationToken cancellationToken);
}
