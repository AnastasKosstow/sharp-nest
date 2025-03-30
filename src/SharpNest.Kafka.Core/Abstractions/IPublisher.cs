using SharpNest.Kafka.Core.Models;

namespace SharpNest.Kafka.Core.Abstractions;

/// <summary>
/// Provides functionality to publish messages to Kafka.
/// </summary>
public interface IPublisher : IDisposable
{
    /// <summary>
    /// Publishes a message to the specified topic.
    /// </summary>
    /// <param name="topic">The topic to publish to.</param>
    /// <param name="key">The message key.</param>
    /// <param name="value">The message value.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <param name="headers">Optional message headers.</param>
    /// <returns>A task that represents the asynchronous operation, containing the delivery result.</returns>
    Task<KafkaDeliveryResult> PublishAsync(string topic, string key, string value, CancellationToken cancellationToken = default, params KeyValuePair<string, byte[]>[] headers);

    /// <summary>
    /// Publishes a message to the specified topic.
    /// </summary>
    /// <param name="message">The message to publish.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    Task<KafkaDeliveryResult> PublishAsync(KafkaMessage message, CancellationToken cancellationToken = default);

    /// <summary>
    /// Publishes a batch of messages to the specified topic.
    /// </summary>
    /// <param name="topic">The topic to publish to.</param>
    /// <param name="keyValuePairs">A collection of key-value pairs representing the messages.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <param name="headers">Optional headers to apply to all messages in the batch.</param>
    /// <returns>A task that represents the asynchronous operation, containing a list of delivery results.</returns>
    Task<IReadOnlyList<KafkaDeliveryResult>> PublishBatchAsync(string topic, IEnumerable<KeyValuePair<string, string>> keyValuePairs, CancellationToken cancellationToken = default, params KeyValuePair<string, byte[]>[] headers);

    /// <summary>
    /// Publishes a batch of pre-configured messages, potentially to different topics.
    /// </summary>
    /// <param name="messages">A collection of messages to publish.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous operation, containing a list of delivery results.</returns>
    /// <remarks>
    /// Messages are grouped by topic for efficient batch publishing. Each message's topic property
    /// must be set to a non-empty value.
    /// </remarks>
    Task<IReadOnlyList<KafkaDeliveryResult>> PublishBatchAsync(IEnumerable<KafkaMessage> messages, CancellationToken cancellationToken = default);
}
