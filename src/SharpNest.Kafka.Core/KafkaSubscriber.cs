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
            _kafkaConsumer = _kafkaConnection.GetConsumer<string, string>(groupId);
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
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error processing message from topic {Topic}, partition {Partition}: {Error}", result.Topic, result.Partition.Value, ex.Message);
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
            _logger.LogError(ex, "Error disposing Kafka consumer");
        }

        _disposed = true;
    }
}
