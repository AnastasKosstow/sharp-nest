using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SharpNest.Kafka.Core.Abstractions;
using SharpNest.Kafka.Core.Models;
using SharpNest.Kafka.Core.Settings;
using SharpNest.Utils.Resilience;
using Confluent.Kafka.Admin;
using Confluent.Kafka;
using Polly;

namespace SharpNest.Kafka.Core;

internal class KafkaSubscriber : ISubscriber, IDisposable
{
    private readonly KafkaSettings _settings;
    private readonly KafkaConnection _kafkaConnection;
    private readonly ResiliencePipeline _pipeline;
    private readonly ILogger<KafkaSubscriber> _logger;
    private readonly SemaphoreSlim _topicCreationLock = new(1, 1);

    private IConsumer<string, string> _kafkaConsumer;
    private bool _disposed;

    public KafkaSubscriber(KafkaConnection kafkaConnection, IOptions<KafkaSettings> options, ILogger<KafkaSubscriber> logger)
    {
        _kafkaConnection = kafkaConnection;
        _logger = logger;
        _settings = options.Value;
        _pipeline = new ResiliencePipelineBuilder()
            .AddRetryStrategy(predicate =>
                (PredicateBuilder)predicate.Handle<ConsumeException>(),
                args =>
                {
                    _logger.LogError(args.Outcome.Exception, "Error consuming message after {Timeout}s ({ExceptionMessage})", $"{args.RetryDelay.TotalSeconds:n1}", args.Outcome.Exception?.Message);
                })
            .AddCircuitBreakerStrategy(_logger, predicate => (PredicateBuilder)predicate.Handle<ConsumeException>())
            .Build();
    }

    public async Task SubscribeAsync(string topic, Func<IMessage, Task> messageHandler, string? groupId = null, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(topic);
        ArgumentNullException.ThrowIfNull(messageHandler);

        await SubscribeManyAsync([topic], messageHandler, groupId, cancellationToken);
    }

    public async Task SubscribeManyAsync(IEnumerable<string> topics, Func<IMessage, Task> messageHandler, string? groupId = null, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(topics);
        ArgumentNullException.ThrowIfNull(messageHandler);

        await EnsureTopicsExistAsync(topics);
        try
        {
            _kafkaConsumer = await _kafkaConnection.GetConsumerAsync<string, string>(groupId, cancellationToken);
            _kafkaConsumer.Subscribe(topics);

            while (!cancellationToken.IsCancellationRequested)
            {
                await ConsumeMessagesAsync(messageHandler, cancellationToken);
            }
        }
        finally
        {
            _kafkaConsumer.Close();
            _kafkaConsumer.Dispose();
        }
    }

    private async Task ConsumeMessagesAsync(Func<IMessage, Task> messageHandler, CancellationToken cancellationToken)
    {
        try
        {
            ConsumeResult<string, string> result = _kafkaConsumer.Consume(cancellationToken);

            if (result != null)
            {
                await _pipeline.ExecuteAsync(async token =>
                {
                    if (string.IsNullOrEmpty(result.Message.Value))
                    {
                        if (_settings.Subscriber.CommitEmptyMessages)
                        {
                            _kafkaConsumer.Commit(result);
                        }
                        return;
                    }

                    var message = new KafkaMessage()
                    {
                        Key = result.Message.Key,
                        Value = result.Message.Value,
                        Headers = result.Message.Headers?.ToDictionary(
                                         header => header.Key, 
                                         header => header.GetValueBytes()) ?? [],
                        Topic = result.Topic
                    };

                    try
                    {
                        await messageHandler(message);
                        _kafkaConsumer.Commit(result);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error processing message from topic {Topic}, partition {Partition}: {Error}", result.Topic, result.Partition.Value, ex.Message);

                        switch (_settings.Subscriber.ErrorHandlingStrategy)
                        {
                            case ErrorHandlingStrategy.Commit:
                                _kafkaConsumer.Commit(result);
                                break;
                            case ErrorHandlingStrategy.Retry:
                                break;
                            case ErrorHandlingStrategy.DeadLetter:
                                await PublishToDeadLetterQueueAsync(message, ex, cancellationToken);
                                _kafkaConsumer.Commit(result);
                                break;
                            case ErrorHandlingStrategy.Throw:
                                throw;
                        }
                    }
                }, cancellationToken);
            }
        }
        catch (ConsumeException ex)
        {
            _logger.LogError(ex, "Error processing message from topic: {Topic}\nError: {Message}", string.Join(",", _kafkaConsumer.Subscription), ex.Message);
        }
    }

    private async Task EnsureTopicsExistAsync(IEnumerable<string> topics)
    {
        await _topicCreationLock.WaitAsync();

        try
        {
            using var admin = new AdminClientBuilder(new AdminClientConfig
            {
                BootstrapServers = _settings.BootstrapServers
            }).Build();

            var existingTopics = admin.GetMetadata(TimeSpan.FromSeconds(10))
                                      .Topics
                                      .Select(t => t.Topic)
                                      .ToHashSet();

            var newTopics = topics.Except(existingTopics);

            if (!newTopics.Any())
                return;

            await admin.CreateTopicsAsync(newTopics.Select(topic => new TopicSpecification
            {
                Name = topic,
                ReplicationFactor = (short)_settings.ReplicationFactor,
                NumPartitions = _settings.Partitions
            }));
        }
        finally
        {
            _topicCreationLock.Release();
        }
    }

    private async Task PublishToDeadLetterQueueAsync(KafkaMessage message, Exception exception, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(_settings.Subscriber.DeadLetterTopic))
        {
            _logger.LogWarning("Dead letter queue strategy selected but no dead letter topic configured");
            return;
        }

        try
        {
            var headers = new Dictionary<string, byte[]>(message.Headers)
            {
                ["X-Error-Type"] = System.Text.Encoding.UTF8.GetBytes(exception.GetType().FullName),
                ["X-Error-Message"] = System.Text.Encoding.UTF8.GetBytes(exception.Message),
                ["X-Original-Topic"] = System.Text.Encoding.UTF8.GetBytes(message.Topic),
                ["X-Failure-Timestamp"] = System.Text.Encoding.UTF8.GetBytes(DateTime.UtcNow.ToString("O"))
            };

            var deadLetterMessage = new KafkaMessage
            {
                Key = message.Key,
                Value = message.Value,
                Headers = headers,
                Topic = _settings.Subscriber.DeadLetterTopic
            };

            using var producer = await _kafkaConnection.GetProducerAsync<string, string>(cancellationToken: cancellationToken);
            var kafkaMessage = new Message<string, string>
            {
                Key = deadLetterMessage.Key,
                Value = deadLetterMessage.Value,
                Headers = []
            };

            foreach (var header in deadLetterMessage.Headers)
            {
                kafkaMessage.Headers.Add(header.Key, header.Value);
            }

            await producer.ProduceAsync(deadLetterMessage.Topic, kafkaMessage);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to publish message to dead letter queue: {Error}", ex.Message);
        }
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        try
        {
            _kafkaConsumer?.Close();
            _kafkaConsumer?.Dispose();
            _topicCreationLock.Dispose();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while disposing KafkaSubscriber");
        }
        finally
        {
            _disposed = true;
        }
    }
}
