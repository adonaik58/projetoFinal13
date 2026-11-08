using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.StackExchangeRedis;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Parking.Application.Abstractions;
using Parking.Infrastructure.Audit;
using Parking.Infrastructure.Caching;
using Parking.Infrastructure.Persistence;
using Parking.Infrastructure.Redis;
using Parking.Infrastructure.Security;
using Parking.Infrastructure.Time;
using StackExchange.Redis;

namespace Parking.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.TryAddSingleton(TimeProvider.System);
        services.TryAddScoped<ICurrentUser, SystemCurrentUser>();

        services.AddOptions<JwtOptions>().Bind(configuration.GetSection(JwtOptions.SectionName)).ValidateDataAnnotations().ValidateOnStart();
        services.AddOptions<BusinessOptions>().Bind(configuration.GetSection(BusinessOptions.SectionName));
        services.AddOptions<BootstrapOptions>().Bind(configuration.GetSection(BootstrapOptions.SectionName));
        services.AddOptions<LoginThrottleOptions>().Bind(configuration.GetSection(LoginThrottleOptions.SectionName));

        AddPersistence(services, configuration);
        AddRedis(services, configuration);

        services.AddSingleton<IPasswordHasher, PasswordHasherService>();
        services.AddSingleton<ITokenService, JwtTokenService>();
        services.AddSingleton<IBusinessClock, BusinessClock>();
        services.AddScoped<IAuditLogger, AuditLogger>();
        return services;
    }

    private static void AddPersistence(IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("Database")
            ?? throw new InvalidOperationException("ConnectionStrings:Database is not configured.");

        services.AddSingleton<AuditableEntityInterceptor>();
        services.AddDbContextPool<AppDbContext>((provider, options) => options
            .UseMySQL(connectionString, mysql =>
            {
                mysql.EnableRetryOnFailure(5);
                mysql.CommandTimeout(30);
            })
            .AddInterceptors(provider.GetRequiredService<AuditableEntityInterceptor>()));
        services.AddScoped<IAppDbContext>(provider => provider.GetRequiredService<AppDbContext>());
        services.AddScoped<IReportingQueries, ReportingQueries>();
        services.AddScoped<DatabaseInitializer>();
    }

    private static void AddRedis(IServiceCollection services, IConfiguration configuration)
    {
        var redisConnection = configuration.GetConnectionString("Redis")
            ?? throw new InvalidOperationException("ConnectionStrings:Redis is not configured.");

        services.AddSingleton<IConnectionMultiplexer>(_ =>
        {
            var options = ConfigurationOptions.Parse(redisConnection);
            options.AbortOnConnectFail = false; // start even if Redis is briefly unavailable; Redis is not the source of truth
            options.ClientName ??= "parking-api";
            return ConnectionMultiplexer.Connect(options);
        });

        services.AddStackExchangeRedisCache(options => options.InstanceName = "parking:cache:");
        services.AddOptions<RedisCacheOptions>().Configure<IServiceProvider>((options, provider) =>
            options.ConnectionMultiplexerFactory = () => Task.FromResult(provider.GetRequiredService<IConnectionMultiplexer>()));

        services.AddHybridCache(options =>
        {
            options.MaximumPayloadBytes = 8 * 1024 * 1024;
            options.DefaultEntryOptions = new() { Expiration = TimeSpan.FromMinutes(5), LocalCacheExpiration = TimeSpan.FromSeconds(30) };
        });

        services.AddSingleton<ICacheService, HybridCacheService>();
        services.AddSingleton<ILoginThrottle, RedisLoginThrottle>();
        services.AddSingleton<IRateLimitStore, RedisRateLimitStore>();
        services.AddSingleton<IIdempotencyStore, RedisIdempotencyStore>();
    }
}
