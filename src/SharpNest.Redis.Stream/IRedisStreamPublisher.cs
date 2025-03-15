using StackExchange.Redis;

namespace SharpNest.Redis.Stream;

/// <summary>
/// Redis stream publisher for publishing data to a specified queue.
/// </summary>
public interface IRedisStreamPublisher
{
    /// <summary>
    /// Publishes data of type T to the specified queue in the Redis stream.
    /// <code>
    /// await streamPublisher.PublishAsync("queue", data);
    /// </code>
    /// </summary>
    /// <typeparam name="T">The type of data to publish.</typeparam>
    /// <param name="queue">The name of the queue to publish the data to.</param>
    /// <param name="data">The data to publish.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task PublishAsync<T>(string queue, T data, RedisChannel.PatternMode patternMode = RedisChannel.PatternMode.Auto) where T : class;
}
