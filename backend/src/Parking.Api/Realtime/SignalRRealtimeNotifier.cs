using Microsoft.AspNetCore.SignalR;
using Parking.Application.Abstractions;

namespace Parking.Api.Realtime;

/// <summary>Publishes through the Redis backplane, so clients connected to any API replica are notified.</summary>
internal sealed class SignalRRealtimeNotifier(IHubContext<ParkingHub> hub, ILogger<SignalRRealtimeNotifier> logger) : IRealtimeNotifier
{
    public async Task NotifyAsync(RealtimeTopic topic)
    {
        var (clients, name) = topic switch
        {
            RealtimeTopic.Users => (hub.Clients.Group(ParkingHub.UserManagersGroup), "users"),
            _ => (hub.Clients.All, "parking"),
        };

        try
        {
            // Not tied to the request: the change is already committed even if the caller disconnects now.
            await clients.SendAsync(ParkingHub.ChangedMethod, name, CancellationToken.None);
        }
#pragma warning disable CA1031 // Best effort: a backplane outage must not fail the committed operation; clients fall back to polling.
        catch (Exception exception)
#pragma warning restore CA1031
        {
            logger.LogWarning(exception, "Realtime notification {Topic} not delivered", name);
        }
    }
}
