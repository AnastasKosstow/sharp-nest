namespace SharpNest.Kafka.Core.Abstractions;

/// <summary>
/// Provides functionality to subscribe to Kafka topics.
/// </summary>
public interface ISubscriber : IDisposable
{
    /// <summary>
    /// Subscribes to the specified topic and processes messages using the provided handler.
    /// </summary>
    /// <param name="topic">The topic to subscribe to.</param>
    /// <param name="messageHandler">The handler for processing messages.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <param name="groupId">Optional consumer group ID. If not provided, the default group ID is used.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    Task SubscribeAsync(string topic, Func<IMessage, Task> messageHandler, string? groupId = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Subscribes to multiple topics and processes messages using the provided handler.
    /// </summary>
    /// <param name="topics">The topics to subscribe to.</param>
    /// <param name="messageHandler">The handler for processing messages.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <param name="groupId">Optional consumer group ID. If not provided, the default group ID is used.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    Task SubscribeManyAsync(IEnumerable<string> topics, Func<IMessage, Task> messageHandler, string? groupId = null, CancellationToken cancellationToken = default);
}
