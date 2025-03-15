using SharpNest.Shared.Serialization;
using System.Runtime.Serialization;
using StackExchange.Redis;

namespace SharpNest.Redis.Stream;

public sealed class RedisStreamPublisher(IConnectionMultiplexer connectionMultiplexer, ISerializer serializer) : IRedisStreamPublisher
{
    private readonly ISubscriber _subscriber = connectionMultiplexer.GetSubscriber();
    private readonly ISerializer _serializer = serializer;

    public Task PublishAsync<T>(string queue, T data, RedisChannel.PatternMode patternMode = RedisChannel.PatternMode.Auto) where T : class
    {
        var payload = _serializer.Serialize(data);
        if (payload is null)
        {
            string message = $"In {nameof(PublishAsync)}\nCannot serialize payload: {data}";
            throw new SerializationException(message);
        }

        RedisChannel redisChannel = new(queue, patternMode);

        return _subscriber.PublishAsync(redisChannel, payload);
    }
}
