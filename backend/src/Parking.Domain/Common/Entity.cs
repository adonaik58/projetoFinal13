namespace Parking.Domain.Common;

public abstract class Entity<TId>
    where TId : struct
{
    public TId Id { get; protected set; }
}

/// <summary>Marks entities whose <c>CreatedAt</c>/<c>UpdatedAt</c> are maintained by the persistence layer.</summary>
#pragma warning disable CA1040 // Marker interface used by the persistence interceptor.
public interface IAuditableEntity;
#pragma warning restore CA1040

/// <summary>Entity whose creation/update instants are maintained by the persistence layer.</summary>
public abstract class AuditableEntity<TId> : Entity<TId>, IAuditableEntity
    where TId : struct
{
    public DateTime CreatedAt { get; private set; }

    public DateTime UpdatedAt { get; private set; }
}
