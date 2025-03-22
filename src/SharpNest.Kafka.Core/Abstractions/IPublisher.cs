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
}
