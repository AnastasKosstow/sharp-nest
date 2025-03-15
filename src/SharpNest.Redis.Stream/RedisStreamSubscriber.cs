using System.Runtime.Serialization;
using SharpNest.Shared.Serialization;
using StackExchange.Redis;

namespace SharpNest.Redis.Stream;

public sealed class RedisStreamSubscriber(IConnectionMultiplexer connectionMultiplexer, ISerializer serializer) : IRedisStreamSubscriber
{
    private readonly ISubscriber _subscriber = connectionMultiplexer.GetSubscriber();
    private readonly ISerializer _serializer = serializer;

    public Task SubscribeAsync<T>(string channel, Action<T> handler, RedisChannel.PatternMode patternMode = RedisChannel.PatternMode.Auto) where T : class
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(channel, nameof(channel));

        RedisChannel redisChannel = new(channel, patternMode);

        var result = _subscriber.SubscribeAsync(redisChannel, (_, data) =>
        {
            ConsumeMessage(data, handler);
        });

        return result;
    }

    public void Subscribe<T>(string channel, Action<T> handler, RedisChannel.PatternMode patternMode = RedisChannel.PatternMode.Auto) where T : class
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(channel, nameof(channel));

        RedisChannel redisChannel = new(channel, patternMode);

        _subscriber.Subscribe(redisChannel, (_, data) =>
        {
            ConsumeMessage(data, handler);
        });
    }

    private void ConsumeMessage<T>(RedisValue data, Action<T> handler) where T : class
    {
        ArgumentNullException.ThrowIfNull(handler);

        var payload = _serializer.Deserialize<T>(data);
        if (payload is null)
        {
            string message = $"In {nameof(Subscribe)}\nCannot deserialize payload: {data}";
            throw new SerializationException(message);
        }

        handler(payload);
    }
}
