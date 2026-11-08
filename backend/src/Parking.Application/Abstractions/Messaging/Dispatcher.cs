using System.Collections.Concurrent;
using Microsoft.Extensions.DependencyInjection;

namespace Parking.Application.Abstractions.Messaging;

/// <summary>
/// Resolves the handler for a request and runs it through the registered pipeline behaviors.
/// Closed generic wrappers are built once per request type and cached.
/// </summary>
internal sealed class Dispatcher(IServiceProvider serviceProvider) : IDispatcher
{
    private static readonly ConcurrentDictionary<Type, object> Wrappers = new();

    public Task<TResult> Send<TResult>(IRequest<TResult> request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var wrapper = (RequestHandlerWrapper<TResult>)Wrappers.GetOrAdd(
            request.GetType(),
            static requestType => Activator.CreateInstance(
                typeof(RequestHandlerWrapper<,>).MakeGenericType(requestType, typeof(TResult)))!);

        return wrapper.Handle(request, serviceProvider, cancellationToken);
    }
}

internal abstract class RequestHandlerWrapper<TResult>
{
    public abstract Task<TResult> Handle(IRequest<TResult> request, IServiceProvider services, CancellationToken cancellationToken);
}

internal sealed class RequestHandlerWrapper<TRequest, TResult> : RequestHandlerWrapper<TResult>
    where TRequest : IRequest<TResult>
{
    public override Task<TResult> Handle(IRequest<TResult> request, IServiceProvider services, CancellationToken cancellationToken)
    {
        var typedRequest = (TRequest)request;
        var handler = services.GetRequiredService<IRequestHandler<TRequest, TResult>>();

        RequestHandlerDelegate<TResult> pipeline = () => handler.Handle(typedRequest, cancellationToken);
        foreach (var behavior in services.GetServices<IPipelineBehavior<TRequest, TResult>>().Reverse())
        {
            var next = pipeline;
            pipeline = () => behavior.Handle(typedRequest, next, cancellationToken);
        }

        return pipeline();
    }
}
