using Parking.Application.Abstractions;
using Parking.Application.Abstractions.Messaging;

namespace Parking.Application.Common.Behaviors;

/// <summary>Marks commands whose success changes what the parking views show (board, tickets, dashboard).</summary>
public interface IParkingChange;

/// <summary>Marks commands whose success changes user accounts or their online state.</summary>
public interface IUsersChange;

/// <summary>Notifies connected clients after a marked command succeeded (failed commands never notify).</summary>
internal sealed class RealtimeNotificationBehavior<TRequest, TResult>(IRealtimeNotifier notifier)
    : IPipelineBehavior<TRequest, TResult>
    where TRequest : IRequest<TResult>
{
    public async Task<TResult> Handle(TRequest request, RequestHandlerDelegate<TResult> next, CancellationToken cancellationToken)
    {
        var result = await next();
        if (request is IParkingChange)
        {
            await notifier.NotifyAsync(RealtimeTopic.Parking);
        }

        if (request is IUsersChange)
        {
            await notifier.NotifyAsync(RealtimeTopic.Users);
        }

        return result;
    }
}
