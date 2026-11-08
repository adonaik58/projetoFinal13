using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Parking.Domain.Common;

namespace Parking.Infrastructure.Persistence;

/// <summary>All instants are stored in UTC; values read back are flagged as UTC so they serialize with "Z".</summary>
internal sealed class UtcDateTimeConverter() : ValueConverter<DateTime, DateTime>(v => ToUtc(v), v => DateTime.SpecifyKind(v, DateTimeKind.Utc))
{
    internal static DateTime ToUtc(DateTime value) => value.Kind switch
    {
        DateTimeKind.Utc => value,
        DateTimeKind.Local => value.ToUniversalTime(),
        _ => DateTime.SpecifyKind(value, DateTimeKind.Utc),
    };
}

internal sealed class NullableUtcDateTimeConverter()
    : ValueConverter<DateTime?, DateTime?>(
        v => v.HasValue ? UtcDateTimeConverter.ToUtc(v.Value) : v,
        v => v.HasValue ? DateTime.SpecifyKind(v.Value, DateTimeKind.Utc) : v);

/// <summary>MySql.Data cannot materialize <see cref="DateOnly"/> directly; it is exchanged as a <c>date</c> via DateTime.</summary>
internal sealed class DateOnlyConverter()
    : ValueConverter<DateOnly, DateTime>(d => d.ToDateTime(TimeOnly.MinValue), d => DateOnly.FromDateTime(d));

/// <summary>Maintains <c>created_at</c>/<c>updated_at</c> on auditable entities.</summary>
internal sealed class AuditableEntityInterceptor(TimeProvider timeProvider) : SaveChangesInterceptor
{
    private const string CreatedAt = "CreatedAt";
    private const string UpdatedAt = "UpdatedAt";

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData, InterceptionResult<int> result, CancellationToken cancellationToken = default)
    {
        Stamp(eventData.Context);
        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
    {
        Stamp(eventData.Context);
        return base.SavingChanges(eventData, result);
    }

    private void Stamp(DbContext? context)
    {
        if (context is null)
        {
            return;
        }

        var now = timeProvider.GetUtcNow().UtcDateTime;
        foreach (var entry in context.ChangeTracker.Entries())
        {
            if (entry.Entity is not IAuditableEntity || entry.State is not (EntityState.Added or EntityState.Modified))
            {
                continue;
            }

            if (entry.State == EntityState.Added)
            {
                entry.Property(CreatedAt).CurrentValue = now;
            }

            entry.Property(UpdatedAt).CurrentValue = now;
        }
    }
}

internal static class SnakeCaseNamingExtensions
{
    /// <summary>Applies snake_case to tables, columns, keys, foreign keys and indexes.</summary>
    public static void ApplySnakeCaseNames(this ModelBuilder modelBuilder)
    {
        foreach (var entity in modelBuilder.Model.GetEntityTypes())
        {
            entity.SetTableName(ToSnakeCase(entity.GetTableName()!));
            foreach (var property in entity.GetProperties())
            {
                property.SetColumnName(ToSnakeCase(property.GetColumnName()));
            }

            foreach (var key in entity.GetKeys())
            {
                key.SetName(ToSnakeCase(key.GetName()!));
            }

            foreach (var foreignKey in entity.GetForeignKeys())
            {
                foreignKey.SetConstraintName(ToSnakeCase(foreignKey.GetConstraintName()!));
            }

            foreach (var index in entity.GetIndexes())
            {
                index.SetDatabaseName(ToSnakeCase(index.GetDatabaseName()!));
            }
        }
    }

    internal static string ToSnakeCase(string name)
    {
        var builder = new StringBuilder(name.Length + 8);
        for (var i = 0; i < name.Length; i++)
        {
            var c = name[i];
            if (char.IsUpper(c))
            {
                var previous = i > 0 ? name[i - 1] : '_';
                var next = i + 1 < name.Length ? name[i + 1] : '_';
                if (i > 0 && previous != '_' && (char.IsLower(previous) || char.IsDigit(previous) || char.IsLower(next)))
                {
                    builder.Append('_');
                }

                builder.Append(char.ToLowerInvariant(c));
            }
            else
            {
                builder.Append(c);
            }
        }

        return builder.ToString();
    }
}
