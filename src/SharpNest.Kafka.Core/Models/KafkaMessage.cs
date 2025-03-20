using SharpNest.Kafka.Core.Abstractions;

namespace SharpNest.Kafka.Core.Models;

/// <summary>
/// Represents a Kafka message.
/// </summary>
public class KafkaMessage : IMessage
{
    /// <summary>
    /// Gets or sets the message key.
    /// </summary>
    public string? Key { get; set; }

    /// <summary>
    /// Gets or sets the message value.
    /// </summary>
    public string? Value { get; set; }

    /// <summary>
    /// Gets or sets the message Topic.
    /// </summary>
    public string Topic { get; set; }

    /// <summary>
    /// Gets or sets the message headers.
    /// </summary>
    public IDictionary<string, byte[]> Headers { get; set; } = new Dictionary<string, byte[]>();
}
