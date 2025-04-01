namespace SharpNest.SSE.Core.Abstractions;

/// <summary>
/// Represents a message in the Server-Sent Events system.
/// </summary>
public interface IMessage
{
    /// <summary>
    /// Gets the unique identifier for this message.
    /// </summary>
    string Id { get; }

    /// <summary>
    /// Gets the payload data for this message.
    /// </summary>
    object Payload { get; }

    /// <summary>
    /// Gets the metadata associated with this message.
    /// </summary>
    IDictionary<string, string> Metadata { get; }
}
