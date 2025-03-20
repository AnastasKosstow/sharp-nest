namespace SharpNest.Kafka.Core.Models;

/// <summary>
/// Represents the result of a Kafka message delivery.
/// </summary>
/// <param name="topic">The topic.</param>
/// <param name="partition">The partition.</param>
/// <param name="timestamp">The timestamp.</param>
/// <param name="isPersisted">A value indicating whether the message was persisted.</param>
public class KafkaDeliveryResult(string topic, int partition, DateTimeOffset timestamp, bool isPersisted)
{
    /// <summary>
    /// Gets the topic the message was delivered to.
    /// </summary>
    public string Topic { get; } = topic;

    /// <summary>
    /// Gets the partition the message was delivered to.
    /// </summary>
    public int Partition { get; } = partition;

    /// <summary>
    /// Gets the timestamp of the message.
    /// </summary>
    public DateTimeOffset Timestamp { get; } = timestamp;

    /// <summary>
    /// Gets a value indicating whether the message was persisted.
    /// </summary>
    public bool IsPersisted { get; } = isPersisted;
}
