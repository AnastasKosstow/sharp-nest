namespace SharpNest.Kafka.Core.Abstractions;

/// <summary>
/// Represents a message received from Kafka.
/// </summary>
public interface IMessage
{
    /// <summary>
    /// Gets the message key.
    /// </summary>
    string? Key { get; }

    /// <summary>
    /// Gets the message value.
    /// </summary>
    string? Value { get; }

    /// <summary>
    /// Gets the topic this message was received from.
    /// </summary>
    string Topic { get; }

    /// <summary>
    /// Gets the message headers.
    /// </summary>
    IDictionary<string, byte[]> Headers { get; }
}
