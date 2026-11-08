using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Parking.Domain.Tickets;

namespace Parking.Infrastructure.Persistence.Configurations;

internal sealed class TicketConfiguration : IEntityTypeConfiguration<Ticket>
{
    public void Configure(EntityTypeBuilder<Ticket> builder)
    {
        builder.ToTable("tickets");
        builder.HasKey(t => t.Id);
        builder.Property(t => t.Code).HasMaxLength(Ticket.CodeMaxLength).IsRequired();
        builder.Property(t => t.RatePerMinute).HasPrecision(10, 2);
        builder.Property(t => t.Amount).HasPrecision(12, 2);

        // Base columns of generated columns may only use RESTRICT/NO ACTION referential actions (MySQL rule).
        builder.HasOne(t => t.Space).WithMany().HasForeignKey(t => t.SpaceId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(t => t.Customer).WithMany().HasForeignKey(t => t.CustomerId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(t => t.Vehicle).WithMany().HasForeignKey(t => t.VehicleId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(t => t.OpenedBy).WithMany().HasForeignKey(t => t.OpenedByUserId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(t => t.ClosedBy).WithMany().HasForeignKey(t => t.ClosedByUserId).OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(t => t.Code).IsUnique().HasDatabaseName("ux_tickets_code");
        builder.HasIndex(t => t.EntryAt).HasDatabaseName("ix_tickets_entry_at");
        builder.HasIndex(t => new { t.Status, t.ExitAt }).HasDatabaseName("ix_tickets_status_exit_at");
        builder.HasIndex(t => new { t.Status, t.EntryAt }).HasDatabaseName("ix_tickets_status_entry_at");

        // One open ticket per space / customer / vehicle, enforced by the database even across API replicas:
        // each generated column holds the id while the ticket is open and NULL once closed (NULLs never collide).
        MapActiveColumn(builder, "ActiveSpaceId", "space_id", IndexNames.TicketActiveSpace);
        MapActiveColumn(builder, "ActiveCustomerId", "customer_id", IndexNames.TicketActiveCustomer);
        MapActiveColumn(builder, "ActiveVehicleId", "vehicle_id", IndexNames.TicketActiveVehicle);
    }

    private static void MapActiveColumn(EntityTypeBuilder<Ticket> builder, string property, string sourceColumn, string indexName)
    {
        builder.Property<int?>(property)
            .HasComputedColumnSql($"CASE WHEN `status` = 'Open' THEN `{sourceColumn}` END", stored: true);
        builder.HasIndex(property).IsUnique().HasDatabaseName(indexName);
    }
}
