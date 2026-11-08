using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using Parking.Application.Abstractions.Messaging;
using Parking.Application.Common;
using Parking.Application.Common.Behaviors;

namespace Parking.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        var assembly = typeof(DependencyInjection).Assembly;

        services.AddScoped<IDispatcher, Dispatcher>();
        services.AddSingleton<ParkingMetrics>();
        services.AddScoped<Features.Auth.AuthSessionFactory>();
        services.AddValidatorsFromAssembly(assembly, ServiceLifetime.Scoped, includeInternalTypes: true);

        // Order matters: telemetry wraps validation, which wraps the handler.
        services.AddScoped(typeof(IPipelineBehavior<,>), typeof(TelemetryBehavior<,>));
        services.AddScoped(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
        services.AddScoped(typeof(IPipelineBehavior<,>), typeof(RealtimeNotificationBehavior<,>));

        var handlerInterface = typeof(IRequestHandler<,>);
        var registrations = assembly.GetTypes()
            .Where(type => type is { IsAbstract: false, IsInterface: false, IsGenericTypeDefinition: false })
            .SelectMany(type => type.GetInterfaces()
                .Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == handlerInterface)
                .Select(i => (Service: i, Implementation: type)));

        foreach (var (service, implementation) in registrations)
        {
            services.AddScoped(service, implementation);
        }

        return services;
    }
}
