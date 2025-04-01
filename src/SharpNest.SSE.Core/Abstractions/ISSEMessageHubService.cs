namespace SharpNest.SSE.Core.Abstractions;

/// <summary>
/// Provides services for broadcasting and subscribing to SSE messages.
/// </summary>
public interface ISSEMessageHubService
{
    /// <summary>
    /// Subscribes to the message stream.
    /// </summary>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>An asynchronous stream of messages.</returns>
    IAsyncEnumerable<IMessage> SubscribeAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Broadcasts a message to all subscribers.
    /// </summary>
    /// <param name="message">The message to broadcast.</param>
    /// <returns>A task that represents the asynchronous broadcast operation.</returns>
    Task BroadcastMessageAsync(IMessage message);
}
