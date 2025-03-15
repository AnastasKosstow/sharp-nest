using StackExchange.Redis;

namespace SharpNest.Redis.Stream;

public interface IRedisStreamPublisher : IAsyncDisposable, IDisposable
{
    /// <summary>
    /// Publishes a message to the specified Redis channel.
    /// </summary>
    /// <typeparam name="T">The type of the message to publish.</typeparam>
    /// <param name="channel">The channel name to publish to.</param>
    /// <param name="message">The message to publish.</param>
    /// <param name="patternMode">The pattern matching mode for the channel name.</param>
    /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
    Task<long> PublishAsync<T>(string channel, 
        T message, 
        RedisChannel.PatternMode patternMode = RedisChannel.PatternMode.Auto,
        CancellationToken cancellationToken = default) where T : class;
}
