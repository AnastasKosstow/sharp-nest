namespace SharpNest.Redis;

public interface IRedisConfiguration
{
    /// <summary>
    /// Add Redis streaming functionality to the service configuration.
    /// Redis streaming allows publishing and subscribing to streams of data within Redis.
    /// </summary>
    /// <returns>An instance of <see cref="IRedisConfiguration"/> for method chaining.</returns>
    IRedisConfiguration AddRedisStreaming();

    /// <summary>
    /// Add Redis cache functionality to the service configuration.
    /// Redis caching enables fast retrieval and storage of frequently accessed data in Redis.
    /// </summary>
    /// <returns>An instance of <see cref="IRedisConfiguration"/> for method chaining.</returns>
    IRedisConfiguration AddRedisCache();
}
