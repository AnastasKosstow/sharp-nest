using StackExchange.Redis;

namespace SharpNest.Redis.Stream;

/// <summary>
/// Redis stream subscriber for subscribing to messages on a specified channel.
/// </summary>
public interface IRedisStreamSubscriber
{
    /// <summary>
    /// Asynchronously subscribes to the specified channel in the Redis stream and specifies a handler to process received messages.
    /// <code>
    /// await streamSubscriber.SubscribeAsync("channel", message => ProcessMessage(message));
    /// </code>
    /// </summary>
    /// <typeparam name="T">The type of data expected in the messages.</typeparam>
    /// <param name="channel">The name of the channel to subscribe to.</param>
    /// <param name="handler">The handler to process received messages.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task SubscribeAsync<T>(string channel, Action<T> handler, RedisChannel.PatternMode patternMode = RedisChannel.PatternMode.Auto) where T : class;

    /// <summary>
    /// Subscribes to the specified channel in the Redis stream and specifies a handler to process received messages.
    /// <code>
    /// streamSubscriber.Subscribe("channel", message => ProcessMessage(message));
    /// </code>
    /// </summary>
    /// <typeparam name="T">The type of data expected in the messages.</typeparam>
    /// <param name="channel">The name of the channel to subscribe to.</param>
    /// <param name="handler">The handler to process received messages.</param>
    void Subscribe<T>(string channel, Action<T> handler, RedisChannel.PatternMode patternMode = RedisChannel.PatternMode.Auto) where T : class;
}
