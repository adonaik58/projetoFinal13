using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Parking.Infrastructure.Persistence;

/// <summary>Used by <c>dotnet ef</c>; generating migrations does not require a live database.</summary>
internal sealed class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        var connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__Database")
                               ?? "Server=localhost;Port=3306;Database=parking;User=parking;Password=design-time-only";
        var options = new DbContextOptionsBuilder<AppDbContext>().UseMySQL(connectionString).Options;
        return new AppDbContext(options);
    }
}
