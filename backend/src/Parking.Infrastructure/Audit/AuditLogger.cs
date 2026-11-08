using System.Text.Json;
using Parking.Application.Abstractions;
using Parking.Domain.Audit;
using Parking.Infrastructure.Persistence;

namespace Parking.Infrastructure.Audit;

internal sealed class AuditLogger(AppDbContext db, ICurrentUser currentUser, TimeProvider timeProvider) : IAuditLogger
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public void Record(string action, string? entityType = null, string? entityId = null, object? details = null) =>
        RecordFor(currentUser.UserId, currentUser.Username, action, entityType, entityId, details);

    public void RecordFor(int? userId, string? username, string action, string? entityType = null, string? entityId = null, object? details = null) =>
        db.AuditLogs.Add(AuditLog.Record(
            timeProvider.GetUtcNow().UtcDateTime,
            action,
            userId,
            username,
            entityType,
            entityId,
            currentUser.IpAddress,
            currentUser.CorrelationId,
            details is null ? null : JsonSerializer.Serialize(details, JsonOptions)));
}
