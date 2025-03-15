using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SharpNest.Redis.Exceptions;
using StackExchange.Redis;

namespace SharpNest.Redis;

public static class ServiceCollectionExtensions
{
    private static readonly string sectionName = "redis";

    public static IServiceCollection AddRedLens(this IServiceCollection services, IConfiguration configuration, Action<IRedisConfiguration> configAction)
    {
        var section = configuration.GetSection(sectionName);
        var options = new RedisOptions();
        section.Bind(options);

        if (string.IsNullOrEmpty(options.ConnectionString))
        {
            throw new RedisConfigurationOptionsException(
                $"Cannot get RedisOptions section from {nameof(IConfiguration)}. " +
                $"\"RedLens\" section must be provided with property \"ConnectionString\" with a valid redis database connection string.");
        }

        services.AddSingleton<IConnectionMultiplexer>(ConnectionMultiplexer.Connect(options.ConnectionString));

        var config = new RedisConfiguration(services);
        configAction?.Invoke(config);
        return services;
    }
}
