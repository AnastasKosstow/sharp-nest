using Microsoft.Extensions.Options;
using System.Collections.Concurrent;
using SharpNest.Kafka.Core.Settings;
using Confluent.Kafka;

namespace SharpNest.Kafka.Core;

internal class KafkaConnection : IDisposable
{
    private readonly SemaphoreSlim _producerLock = new(1, 1);
    private readonly SemaphoreSlim _consumerLock = new(1, 1);
    private readonly ConcurrentDictionary<(Type KeyType, Type ValueType), object> _producers = new();
    private readonly ConcurrentDictionary<(Type KeyType, Type ValueType, string GroupId), object> _consumers = new();
    private readonly KafkaConfigBuilder _configBuilder;
    private readonly KafkaSettings _settings;
    private readonly string _defaultGroupId;

    private bool _disposed;

    public KafkaConnection(KafkaConfigBuilder configBuilder, IOptions<KafkaSettings> options, string? groupId = null)
    {
        _settings = options.Value;
        _configBuilder = configBuilder;
        _defaultGroupId = groupId ?? _settings.DefaultGroup;
    }

    public async Task<IConsumer<TKey, TValue>> GetConsumerAsync<TKey, TValue>(string? groupId = null, CancellationToken cancellationToken = default)
    {
        ThrowIfDisposed();

        var effectiveGroupId = groupId ?? _defaultGroupId;
        var key = (typeof(TKey), typeof(TValue), effectiveGroupId);

        if (_consumers.TryGetValue(key, out var existingConsumer))
        {
            return (IConsumer<TKey, TValue>)existingConsumer;
        }

        await _consumerLock.WaitAsync(cancellationToken);
        try
        {
            if (_consumers.TryGetValue(key, out existingConsumer))
            {
                return (IConsumer<TKey, TValue>)existingConsumer;
            }

            var consumer = new ConsumerBuilder<TKey, TValue>(_configBuilder.BuildConsumerConfig(effectiveGroupId))
                .Build();

            _consumers[key] = consumer;
            return consumer;
        }
        finally
        {
            _consumerLock.Release();
        }
    }

    public async Task<IProducer<TKey, TValue>> GetProducerAsync<TKey, TValue>(Action<ProducerBuilder<TKey, TValue>>? configurator = null, CancellationToken cancellationToken = default)
    {
        ThrowIfDisposed();

        if (_producers.TryGetValue((typeof(TKey), typeof(TValue)), out var existingProducer))
        {
            return (IProducer<TKey, TValue>)existingProducer;
        }

        await _producerLock.WaitAsync(cancellationToken);
        try
        {
            if (_producers.TryGetValue((typeof(TKey), typeof(TValue)), out existingProducer))
            {
                return (IProducer<TKey, TValue>)existingProducer;
            }

            var builder = new ProducerBuilder<TKey, TValue>(_configBuilder.BuildProducerConfig());
            configurator?.Invoke(builder);
            var producer = builder.Build();

            _producers[(typeof(TKey), typeof(TValue))] = producer;
            return producer;
        }
        finally
        {
            _producerLock.Release();
        }
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        foreach (var producer in _producers.Values)
        {
            (producer as IDisposable)?.Dispose();
        }

        foreach (var consumer in _consumers.Values)
        {
            if (consumer is IDisposable disposableConsumer && consumer is IClient clientConsumer)
            {
                consumer.GetType().GetMethod("Close")?.Invoke(consumer, null); // Important: close before dispose
                disposableConsumer.Dispose();
            }
        }

        _producers.Clear();
        _consumers.Clear();
        _producerLock.Dispose();
        _consumerLock.Dispose();

        _disposed = true;
    }

    private void ThrowIfDisposed()
    {
        ObjectDisposedException.ThrowIf(_disposed, nameof(KafkaConnection));
    }
}
