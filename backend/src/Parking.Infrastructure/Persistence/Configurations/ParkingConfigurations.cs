using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Parking.Domain.Audit;
using Parking.Domain.Customers;
using Parking.Domain.Settings;
using Parking.Domain.Spaces;
using Parking.Domain.Users;
using Parking.Domain.Vehicles;

namespace Parking.Infrastructure.Persistence.Configurations;

internal sealed class ParkingSpaceConfiguration : IEntityTypeConfiguration<ParkingSpace>
{
    public void Configure(EntityTypeBuilder<ParkingSpace> builder)
    {
        builder.ToTable("parking_spaces");
        builder.HasKey(s => s.Id);
        builder.Property(s => s.Code).HasMaxLength(ParkingSpace.CodeMaxLength).IsRequired();
        builder.Property(s => s.Section).HasMaxLength(ParkingSpace.SectionMaxLength).IsRequired();
        builder.Property(s => s.Version).IsConcurrencyToken();
        builder.HasIndex(s => s.Code).IsUnique().HasDatabaseName(IndexNames.SpaceCode);
        builder.HasIndex(s => new { s.Section, s.Number }).HasDatabaseName("ix_parking_spaces_section_number");
        builder.HasIndex(s => s.Status);
        builder.HasOne(s => s.CreatedBy).WithMany().HasForeignKey(s => s.CreatedByUserId).OnDelete(DeleteBehavior.Restrict);
    }
}

internal sealed class CustomerConfiguration : IEntityTypeConfiguration<Customer>
{
    public void Configure(EntityTypeBuilder<Customer> builder)
    {
        builder.ToTable("customers");
        builder.HasKey(c => c.Id);
        builder.Property(c => c.FullName).HasMaxLength(Customer.FullNameMaxLength).IsRequired();
        builder.Property(c => c.NationalId).HasMaxLength(Customer.NationalIdLength).IsFixedLength().IsRequired();
        builder.Property(c => c.Phone).HasMaxLength(Customer.PhoneMaxLength);
        builder.HasIndex(c => c.NationalId).IsUnique().HasDatabaseName("ux_customers_national_id");
        builder.HasIndex(c => c.FullName);
    }
}

internal sealed class VehicleBrandConfiguration : IEntityTypeConfiguration<VehicleBrand>
{
    public void Configure(EntityTypeBuilder<VehicleBrand> builder)
    {
        builder.ToTable("vehicle_brands");
        builder.HasKey(b => b.Id);
        builder.Property(b => b.Name).HasMaxLength(VehicleBrand.NameMaxLength).IsRequired();
        builder.HasIndex(b => b.Name).IsUnique().HasDatabaseName(IndexNames.BrandName);
    }
}

internal sealed class VehicleModelConfiguration : IEntityTypeConfiguration<VehicleModel>
{
    public void Configure(EntityTypeBuilder<VehicleModel> builder)
    {
        builder.ToTable("vehicle_models");
        builder.HasKey(m => m.Id);
        builder.Property(m => m.Name).HasMaxLength(VehicleModel.NameMaxLength).IsRequired();
        builder.HasOne<VehicleBrand>().WithMany().HasForeignKey(m => m.BrandId).OnDelete(DeleteBehavior.Restrict);
        builder.HasIndex(m => new { m.BrandId, m.Name }).IsUnique().HasDatabaseName(IndexNames.ModelBrandName);
    }
}

internal sealed class VehicleConfiguration : IEntityTypeConfiguration<Vehicle>
{
    public void Configure(EntityTypeBuilder<Vehicle> builder)
    {
        builder.ToTable("vehicles");
        builder.HasKey(v => v.Id);
        builder.Property(v => v.Plate).HasMaxLength(Vehicle.PlateMaxLength).IsRequired();
        builder.Property(v => v.Color).HasMaxLength(Vehicle.ColorLength).IsFixedLength().IsRequired();
        builder.HasOne(v => v.Brand).WithMany().HasForeignKey(v => v.BrandId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(v => v.Model).WithMany().HasForeignKey(v => v.ModelId).OnDelete(DeleteBehavior.Restrict);
        builder.HasIndex(v => v.Plate).IsUnique().HasDatabaseName("ux_vehicles_plate");
    }
}

internal sealed class ParkingSettingsConfiguration : IEntityTypeConfiguration<ParkingSettings>
{
    public void Configure(EntityTypeBuilder<ParkingSettings> builder)
    {
        builder.ToTable("parking_settings");
        builder.HasKey(s => s.Id);
        builder.Property(s => s.Id).ValueGeneratedNever();
        builder.Property(s => s.RatePerMinute).HasPrecision(10, 2);
        builder.Ignore(s => s.FreeMinutes);
        builder.HasOne<User>().WithMany().HasForeignKey(s => s.UpdatedByUserId).OnDelete(DeleteBehavior.Restrict);
    }
}

internal sealed class AuditLogConfiguration : IEntityTypeConfiguration<AuditLog>
{
    public void Configure(EntityTypeBuilder<AuditLog> builder)
    {
        builder.ToTable("audit_logs");
        builder.HasKey(a => a.Id);
        builder.Property(a => a.Action).HasMaxLength(AuditLog.ActionMaxLength).IsRequired();
        builder.Property(a => a.Username).HasMaxLength(AuditLog.UsernameMaxLength);
        builder.Property(a => a.EntityType).HasMaxLength(AuditLog.EntityTypeMaxLength);
        builder.Property(a => a.EntityId).HasMaxLength(AuditLog.EntityIdMaxLength);
        builder.Property(a => a.IpAddress).HasMaxLength(AuditLog.IpMaxLength);
        builder.Property(a => a.CorrelationId).HasMaxLength(AuditLog.CorrelationIdMaxLength);
        builder.Property(a => a.Details).HasColumnType("json");
        builder.HasIndex(a => a.OccurredAt).HasDatabaseName("ix_audit_logs_occurred_at");
        builder.HasIndex(a => new { a.Action, a.OccurredAt }).HasDatabaseName("ix_audit_logs_action_occurred_at");
        builder.HasIndex(a => new { a.Username, a.OccurredAt }).HasDatabaseName("ix_audit_logs_username_occurred_at");
    }
}
