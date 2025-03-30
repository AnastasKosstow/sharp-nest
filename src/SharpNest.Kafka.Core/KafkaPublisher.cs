using System.Text.Json;
using Microsoft.Extensions.Logging;
using SharpNest.Kafka.Core.Abstractions;
using SharpNest.Kafka.Core.Exceptions;
using SharpNest.Kafka.Core.Models;
using SharpNest.Utils.Resilience;
using SharpNest.Utils.Serialization;
using Confluent.Kafka;
using Polly;

namespace SharpNest.Kafka.Core;

internal class KafkaPublisher : IPublisher, IDisposable
{
    private readonly KafkaConnection _connection;
    private readonly ResiliencePipeline _pipeline;
    private readonly ISerializer _serializer;
    private readonly ILogger<KafkaPublisher> _logger;

    private bool _disposed;

    public KafkaPublisher(KafkaConnection connection, ISerializer serializer, ILogger<KafkaPublisher> logger)
    {
        _connection = connection;
        _serializer = serializer;
        _logger = logger;

        _pipeline = new ResiliencePipelineBuilder()
            .AddRetryStrategy(predicate =>
                (PredicateBuilder)predicate.Handle<ProduceException<string, string>>(),
                args =>
                {
                    _logger.LogError(args.Outcome.Exception, "Could not publish event after {Timeout}s ({ExceptionMessage})", $"{args.RetryDelay.TotalSeconds:n1}", args.Outcome.Exception.Message);
                })
            .AddCircuitBreakerStrategy(_logger, predicate => (PredicateBuilder)predicate.Handle<ProduceException<string, string>>())
            .Build();
    }

    public async Task<IReadOnlyList<KafkaDeliveryResult>> PublishBatchAsync(string topic, IEnumerable<KeyValuePair<string, string>> keyValuePairs, CancellationToken cancellationToken = default, params KeyValuePair<string, byte[]>[] headers)
    {
        ArgumentException.ThrowIfNullOrEmpty(topic, nameof(topic));
        ArgumentNullException.ThrowIfNull(keyValuePairs, nameof(keyValuePairs));
        ThrowIfDisposed();

        var messages = keyValuePairs.Select(kv => new KafkaMessage
        {
            Topic = topic,
            Key = kv.Key,
            Value = kv.Value,
            Headers = headers?.ToDictionary(h => h.Key, h => h.Value)
        });

        return await PublishBatchAsync(messages, cancellationToken);
    }

    public async Task<IReadOnlyList<KafkaDeliveryResult>> PublishBatchAsync(IEnumerable<KafkaMessage> messages, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(messages);
        ThrowIfDisposed();

        var messagesList = messages.ToList();
        if (messagesList.Count == 0)
        {
            return [];
        }

        var kafkaDeliveryResults = new List<KafkaDeliveryResult>(messagesList.Count);
        var batchesByTopic = messagesList.GroupBy(kafkaMessage => kafkaMessage.Topic);

        foreach (var batch in batchesByTopic)
        {
            var topic = batch.Key;
            ArgumentException.ThrowIfNullOrEmpty(topic, nameof(topic));

            var producer = _connection.GetProducer<string, string>();
            var kafkaMessages = PrepareKafkaMessages(batch);

            try
            {
                var deliveryResults = new List<DeliveryResult<string, string>>();

                await ExecuteWithRetryAsync(async () =>
                {
                    var tasks = kafkaMessages
                        .Select(message => producer.ProduceAsync(topic, message, cancellationToken))
                        .ToList();

                    var deliveryReports = await Task.WhenAll(tasks);

                    deliveryResults.AddRange(deliveryReports);
                }, topic);

                kafkaDeliveryResults.AddRange(deliveryResults.Select(r => new KafkaDeliveryResult(
                    r.Topic,
                    r.Partition.Value,
                    r.Timestamp.UtcDateTime,
                    r.Status == PersistenceStatus.Persisted)));
            }
            catch (Exception ex)
            {
                throw new KafkaPublisherException($"Failed to publish batch of {kafkaMessages.Count} messages to topic {topic}", ex);
            }
        }

        return kafkaDeliveryResults;
    }

    public async Task<KafkaDeliveryResult> PublishAsync(string topic, string key, string value, CancellationToken cancellationToken = default, params KeyValuePair<string, byte[]>[] headers)
    {
        ArgumentException.ThrowIfNullOrEmpty(topic, nameof(topic));
        ThrowIfDisposed();

        var message = new KafkaMessage
        {
            Topic = topic,
            Key = key,
            Value = value,
            Headers = headers != null && headers.Length > 0
                ? new Dictionary<string, byte[]>(headers)
                : []
        };

        return await PublishAsync(message, cancellationToken);
    }

    public async Task<KafkaDeliveryResult> PublishAsync(KafkaMessage message, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(message);
        ArgumentNullException.ThrowIfNull(message.Topic);
        ThrowIfDisposed();

        if (message.Value == null)
        {
            throw new ArgumentNullException(nameof(message.Value), "Message value cannot be null");
        }
        var serializedValue = SerializePayload(message.Value);

        var headers = new Headers();
        foreach (var header in message.Headers ?? new Dictionary<string, byte[]>())
        {
            headers.Add(header.Key, header.Value);
        }

        var kafkaMessage = new Message<string, string>
        {
            Key = message.Key,
            Value = serializedValue,
            Headers = headers
        };

        try
        {
            var producer = _connection.GetProducer<string, string>();

            DeliveryResult<string, string> result = null;
            await ExecuteWithRetryAsync(async () =>
            {
                result = await producer.ProduceAsync(message.Topic, kafkaMessage, cancellationToken);
            },
            message.Topic);

            var kafkaDeliveryResult = new KafkaDeliveryResult(result.Topic, result.Partition.Value, result.Timestamp.UtcDateTime, result.Status == PersistenceStatus.Persisted);
            return kafkaDeliveryResult;
        }
        catch (Exception ex)
        {
            throw new KafkaPublisherException($"Failed to publish message to topic {message.Topic}", ex);
        }
    }

    private List<Message<string, string>> PrepareKafkaMessages(IEnumerable<KafkaMessage> messages)
    {
        return [.. messages.Select(message =>
        {
            if (message.Value == null)
            {
                throw new ArgumentNullException(nameof(message.Value), "Message value cannot be null");
            }

            var headers = new Headers();
            if (message.Headers?.Count > 0)
            {
                foreach (var header in message.Headers)
                {
                    headers.Add(header.Key, header.Value);
                }
            }

            return new Message<string, string>
            {
                Key = message.Key,
                Value = SerializePayload(message.Value),
                Headers = headers
            };
        })];
    }

    private async Task ExecuteWithRetryAsync(Func<Task> action, string topic)
    {
        try
        {
            await _pipeline.ExecuteAsync(async token => await action());
        }
        catch (ProduceException<string, string> ex)
        {
            _logger.LogError(ex, "Failed to publish message to topic {Topic}: {Error}", topic, ex.Error.Reason);

            throw new KafkaPublisherException($"Failed to publish message to topic {topic}", ex);
        }
    }

    private string SerializePayload(object data)
    {
        try
        {
            return _serializer.Serialize(data);
        }
        catch (Exception ex) when (ex is JsonException || ex is NotSupportedException)
        {
            _logger.LogError(ex, "Failed to serialize payload of type {PayloadType}: {Error}", data.GetType().Name, ex.Message);

            throw new KafkaSerializationException("Failed to serialize payload", ex);
        }
    }

    private void ThrowIfDisposed()
    {
        ObjectDisposedException.ThrowIf(_disposed, nameof(KafkaPublisher));
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;
    }
}
