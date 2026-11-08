using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using Parking.Application.Abstractions.Messaging;
using Parking.Application.Common.Behaviors;

namespace Parking.UnitTests.Application;

public sealed class DispatcherTests
{
    public sealed record Echo(string Value) : IQuery<string>;

    internal sealed class EchoHandler : IQueryHandler<Echo, string>
    {
        public Task<string> Handle(Echo request, CancellationToken cancellationToken) => Task.FromResult(request.Value.ToUpperInvariant());
    }

    internal sealed class EchoValidator : AbstractValidator<Echo>
    {
        public EchoValidator() => RuleFor(x => x.Value).NotEmpty();
    }

    internal sealed class RecordingBehavior<TRequest, TResult>(List<string> log) : IPipelineBehavior<TRequest, TResult>
        where TRequest : IRequest<TResult>
    {
        public async Task<TResult> Handle(TRequest request, RequestHandlerDelegate<TResult> next, CancellationToken cancellationToken)
        {
            log.Add("before");
            var result = await next();
            log.Add("after");
            return result;
        }
    }

    private static ServiceProvider BuildProvider(List<string> log)
    {
        var services = new ServiceCollection();
        services.AddSingleton(log);
        services.AddScoped<IDispatcher, Dispatcher>();
        services.AddScoped<IRequestHandler<Echo, string>, EchoHandler>();
        services.AddScoped<IValidator<Echo>, EchoValidator>();
        services.AddScoped(typeof(IPipelineBehavior<,>), typeof(RecordingBehavior<,>));
        services.AddScoped(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
        return services.BuildServiceProvider();
    }

    [Fact]
    public async Task Send_runs_behaviors_in_registration_order_around_the_handler()
    {
        var log = new List<string>();
        await using var provider = BuildProvider(log);

        var result = await provider.GetRequiredService<IDispatcher>().Send(new Echo("park"));

        Assert.Equal("PARK", result);
        Assert.Equal(["before", "after"], log);
    }

    [Fact]
    public async Task Send_rejects_invalid_requests_before_reaching_the_handler()
    {
        var log = new List<string>();
        await using var provider = BuildProvider(log);

        var error = await Assert.ThrowsAsync<ValidationException>(() => provider.GetRequiredService<IDispatcher>().Send(new Echo("")));

        Assert.Contains(error.Errors, e => e.PropertyName == "Value");
    }

    [Fact]
    public void AddApplication_registers_a_handler_for_every_request_type()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        Parking.Application.DependencyInjection.AddApplication(services);

        var requestTypes = typeof(Parking.Application.DependencyInjection).Assembly.GetTypes()
            .Where(t => t is { IsAbstract: false, IsInterface: false } && t.GetInterfaces().Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IRequest<>)));

        foreach (var requestType in requestTypes)
        {
            var resultType = requestType.GetInterfaces().First(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IRequest<>)).GetGenericArguments()[0];
            var handlerType = typeof(IRequestHandler<,>).MakeGenericType(requestType, resultType);
            Assert.True(services.Any(d => d.ServiceType == handlerType), $"No handler registered for {requestType.Name}");
        }
    }
}
