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

    public IConsumer<TKey, TValue> GetConsumer<TKey, TValue>(string? groupId = null)
    {
        ThrowIfDisposed();

        var effectiveGroupId = groupId ?? _defaultGroupId;

        return (IConsumer<TKey, TValue>)_consumers.GetOrAdd(
            (typeof(TKey), typeof(TValue), effectiveGroupId),
            key =>
            {
                _consumerLock.Wait();
                try
                {
                    return new ConsumerBuilder<TKey, TValue>(_configBuilder.BuildConsumerConfig(effectiveGroupId))
                        .Build();
                }
                finally
                {
                    _consumerLock.Release();
                }
            });
    }

    public IProducer<TKey, TValue> GetProducer<TKey, TValue>(Action<ProducerBuilder<TKey, TValue>>? configurator = null)
    {
        ThrowIfDisposed();

        return (IProducer<TKey, TValue>)_producers.GetOrAdd(
            (typeof(TKey), typeof(TValue)),
            keyTuple =>
            {
                _producerLock.Wait();
                try
                {
                    if (_producers.TryGetValue(keyTuple, out var existingProducer))
                    {
                        return existingProducer;
                    }

                    var builder = new ProducerBuilder<TKey, TValue>(_configBuilder.BuildProducerConfig());

                    configurator?.Invoke(builder);

                    return builder.Build();
                }
                finally
                {
                    _producerLock.Release();
                }
            });
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
