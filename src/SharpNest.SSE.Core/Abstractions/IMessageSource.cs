namespace SharpNest.SSE.Core.Abstractions;

/// <summary>
/// Represents a source of messages for Server-Sent Events.
/// </summary>
/// <typeparam name="TPayload">The type of the payload in the messages.</typeparam>
public interface IMessageSource
{
    /// <summary>
    /// Starts the message source and begins processing messages.
    /// </summary>
    /// <param name="messageHandler">A callback to handle messages produced by this source.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous start operation.</returns>
    Task StartAsync(Func<IMessage, Task> messageHandler, CancellationToken cancellationToken);
}
