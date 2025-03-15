using StackExchange.Redis;

namespace SharpNest.Redis.Stream;

public readonly struct ChannelSubscription(RedisChannel channel) : IEquatable<ChannelSubscription>
{
    public RedisChannel Channel { get; } = channel;

    public override bool Equals(object? obj) 
        => 
        obj is ChannelSubscription subscription && Equals(subscription);

    public bool Equals(ChannelSubscription other) 
        => Channel.Equals(other.Channel);

    public override int GetHashCode() 
        => Channel.GetHashCode();

    public static bool operator ==(ChannelSubscription left, ChannelSubscription right) => left.Equals(right);

    public static bool operator !=(ChannelSubscription left, ChannelSubscription right) => !(left == right);
}
