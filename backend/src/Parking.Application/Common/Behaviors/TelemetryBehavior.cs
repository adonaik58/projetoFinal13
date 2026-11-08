using System.Diagnostics;
using Microsoft.Extensions.Logging;
using Parking.Application.Abstractions.Messaging;

namespace Parking.Application.Common.Behaviors;

/// <summary>Creates a trace span per command/query and logs slow requests.</summary>
internal sealed class TelemetryBehavior<TRequest, TResult>(ILogger<TelemetryBehavior<TRequest, TResult>> logger)
    : IPipelineBehavior<TRequest, TResult>
    where TRequest : IRequest<TResult>
{
    private static readonly TimeSpan SlowThreshold = TimeSpan.FromMilliseconds(500);

    public async Task<TResult> Handle(TRequest request, RequestHandlerDelegate<TResult> next, CancellationToken cancellationToken)
    {
        var requestName = typeof(TRequest).Name;
        using var activity = ApplicationDiagnostics.ActivitySource.StartActivity(requestName);
        activity?.SetTag("parking.request.kind", request is IQuery<TResult> ? "query" : "command");

        var started = Stopwatch.GetTimestamp();
        try
        {
            return await next();
        }
        catch (Exception exception)
        {
            activity?.SetStatus(ActivityStatusCode.Error, exception.GetType().Name);
            throw;
        }
        finally
        {
            var elapsed = Stopwatch.GetElapsedTime(started);
            if (elapsed > SlowThreshold)
            {
                logger.LogWarning("Slow request {RequestName} took {ElapsedMs} ms", requestName, (long)elapsed.TotalMilliseconds);
            }
        }
    }
}
