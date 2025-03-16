using SharpNest.Utils.Serialization;
using System.Runtime.Serialization;
using StackExchange.Redis;
using Microsoft.Extensions.Logging;

namespace SharpNest.Redis.Stream;

public sealed class RedisStreamPublisher(IConnectionMultiplexer connectionMultiplexer, ISerializer serializer, ILogger<RedisStreamPublisher> logger) : IRedisStreamPublisher
{
    private readonly IConnectionMultiplexer _connectionMultiplexer = connectionMultiplexer;
    private readonly ISerializer _serializer = serializer;
    private readonly ILogger<RedisStreamPublisher> _logger = logger;
    private ISubscriber? _subscriber;

    private readonly SemaphoreSlim _semaphore = new(1, 1);
    private bool _disposed;

    public async Task<long> PublishAsync<T>(string channel,
        T message,
        RedisChannel.PatternMode patternMode = RedisChannel.PatternMode.Auto,
        CancellationToken cancellationToken = default) where T : class
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(channel, nameof(channel));

        ArgumentNullException.ThrowIfNull(message, nameof(message));

        ThrowIfDisposed();
        ISubscriber subscriber = await GetSubscriberAsync(cancellationToken);

        try
        {
            RedisValue payload;
            try
            {
                payload = _serializer.Serialize(message);
            }
            catch (Exception ex)
            {
                string errorMessage = $"Failed to serialize message of type {typeof(T).Name} for channel {channel}.";
                _logger.LogError(ex, "Error: {message}", errorMessage);

                throw new SerializationException(errorMessage, ex);
            }

            RedisChannel redisChannel = new(channel, patternMode);

            long subscriberCount = await subscriber.PublishAsync(redisChannel, payload);
            return subscriberCount;
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("Publish operation to channel {Channel} was canceled.", channel);
            throw;
        }
        catch (RedisException ex)
        {
            _logger.LogError(ex, "Redis error occurred while publishing to channel {Channel}.", channel);
            throw;
        }
        catch (Exception ex) when (ex is not SerializationException and not ArgumentException and not OperationCanceledException)
        {
            _logger.LogError(ex, "Unexpected error occurred while publishing to channel {Channel}.", channel);
            throw;
        }
    }

    private async Task<ISubscriber> GetSubscriberAsync(CancellationToken cancellationToken)
    {
        if (_subscriber != null)
        {
            return _subscriber;
        }

        await _semaphore.WaitAsync(cancellationToken);
        try
        {
            if (_subscriber != null)
            {
                return _subscriber;
            }

            _subscriber = _connectionMultiplexer.GetSubscriber();
            return _subscriber;
        }
        finally
        {
            _semaphore.Release();
        }
    }

    private void ThrowIfDisposed()
    {
        ObjectDisposedException.ThrowIf(_disposed, nameof(RedisStreamPublisher));
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _semaphore.Dispose();
        _disposed = true;
    }

    public async ValueTask DisposeAsync()
    {
        if (_disposed)
        {
            return;
        }

        _semaphore.Dispose();
        _disposed = true;

        await Task.CompletedTask;
    }
}
