using StackExchange.Redis;

namespace SharpNest.Redis.Stream;

public interface IRedisStreamSubscriber : IAsyncDisposable, IDisposable
{
    /// <summary>
    /// Subscribes to a Redis channel and processes messages using the specified handler.
    /// </summary>
    /// <typeparam name="T">The type of messages expected from the channel.</typeparam>
    /// <param name="channel">The channel name to subscribe to.</param>
    /// <param name="handler">The delegate that handles received messages.</param>
    /// <param name="patternMode">The pattern matching mode for the channel name.</param>
    /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous subscribe operation. The task result contains a <see cref="ChannelSubscription"/> that can be used to unsubscribe.</returns>
    Task<ChannelSubscription> SubscribeAsync<T>(string channel,
        Action<T> handler,
        RedisChannel.PatternMode patternMode = RedisChannel.PatternMode.Auto,
        CancellationToken cancellationToken = default) where T : class;

    /// <summary>
    /// Unsubscribes from a Redis channel.
    /// </summary>
    /// <param name="subscription">The subscription to cancel.</param>
    /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous unsubscribe operation.</returns>
    Task UnsubscribeAsync(ChannelSubscription subscription, CancellationToken cancellationToken = default);
}
