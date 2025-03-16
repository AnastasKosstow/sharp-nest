using Microsoft.Extensions.DependencyInjection;
using SharpNest.Redis.Cache;
using SharpNest.Redis.Stream;
using SharpNest.Utils.Serialization;

namespace SharpNest.Redis;

public class RedisConfiguration : IRedisConfiguration
{
    private readonly IServiceCollection services;

    public RedisConfiguration(IServiceCollection services)
    {
        this.services = services;
    }

    public IRedisConfiguration AddRedisCache()
    {
        services.AddSingleton<IRedisCache, RedisCache>();
        return this;
    }

    public IRedisConfiguration AddRedisStreaming()
    {
        services
            .AddSingleton<ISerializer, SystemTextJsonSerializer>()
            .AddSingleton<IRedisStreamPublisher, RedisStreamPublisher>()
            .AddSingleton<IRedisStreamSubscriber, RedisStreamSubscriber>();
        return this;
    }
}
