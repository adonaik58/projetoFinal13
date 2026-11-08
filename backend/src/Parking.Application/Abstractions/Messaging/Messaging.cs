namespace Parking.Application.Abstractions.Messaging;

/// <summary>Marker for any request dispatched through <see cref="IDispatcher"/>.</summary>
#pragma warning disable CA1040 // Marker interfaces carry the result type for the dispatcher.
public interface IRequest<TResult>;

/// <summary>A state-changing request.</summary>
public interface ICommand<TResult> : IRequest<TResult>;

/// <summary>A state-changing request without a meaningful result.</summary>
public interface ICommand : ICommand<Unit>;

/// <summary>A read-only request.</summary>
public interface IQuery<TResult> : IRequest<TResult>;
#pragma warning restore CA1040

public interface IRequestHandler<in TRequest, TResult>
    where TRequest : IRequest<TResult>
{
    Task<TResult> Handle(TRequest request, CancellationToken cancellationToken);
}

public interface ICommandHandler<in TCommand, TResult> : IRequestHandler<TCommand, TResult>
    where TCommand : ICommand<TResult>;

public interface ICommandHandler<in TCommand> : ICommandHandler<TCommand, Unit>
    where TCommand : ICommand;

public interface IQueryHandler<in TQuery, TResult> : IRequestHandler<TQuery, TResult>
    where TQuery : IQuery<TResult>;

public delegate Task<TResult> RequestHandlerDelegate<TResult>();

/// <summary>Cross-cutting step wrapped around every handler (validation, logging, ...).</summary>
public interface IPipelineBehavior<in TRequest, TResult>
    where TRequest : IRequest<TResult>
{
    Task<TResult> Handle(TRequest request, RequestHandlerDelegate<TResult> next, CancellationToken cancellationToken);
}

public interface IDispatcher
{
    Task<TResult> Send<TResult>(IRequest<TResult> request, CancellationToken cancellationToken = default);
}

public readonly record struct Unit
{
    public static readonly Unit Value;
}
