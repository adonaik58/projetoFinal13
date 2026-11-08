using Parking.Domain.Common;

namespace Parking.Domain.Audit;

/// <summary>Append-only record of a security-relevant or business-critical operation.</summary>
public sealed class AuditLog : Entity<long>
{
    public const int ActionMaxLength = 64;
    public const int UsernameMaxLength = 50;
    public const int EntityTypeMaxLength = 64;
    public const int EntityIdMaxLength = 64;
    public const int IpMaxLength = 45;
    public const int CorrelationIdMaxLength = 64;

    private AuditLog()
    {
    }

    public DateTime OccurredAt { get; private set; }

    public int? UserId { get; private set; }

    public string? Username { get; private set; }

    public string Action { get; private set; } = null!;

    public string? EntityType { get; private set; }

    public string? EntityId { get; private set; }

    public string? IpAddress { get; private set; }

    public string? CorrelationId { get; private set; }

    /// <summary>JSON document with operation-specific details.</summary>
    public string? Details { get; private set; }

    public static AuditLog Record(
        DateTime occurredAt,
        string action,
        int? userId,
        string? username,
        string? entityType,
        string? entityId,
        string? ipAddress,
        string? correlationId,
        string? details) =>
        new()
        {
            OccurredAt = occurredAt,
            Action = Truncate(action, ActionMaxLength)!,
            UserId = userId,
            Username = Truncate(username, UsernameMaxLength),
            EntityType = Truncate(entityType, EntityTypeMaxLength),
            EntityId = Truncate(entityId, EntityIdMaxLength),
            IpAddress = Truncate(ipAddress, IpMaxLength),
            CorrelationId = Truncate(correlationId, CorrelationIdMaxLength),
            Details = details,
        };

    private static string? Truncate(string? value, int max) => value is { Length: > 0 } && value.Length > max ? value[..max] : value;
}
