using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;
using SharpNest.Shared.Serialization;
using StackExchange.Redis;

namespace SharpNest.Redis.Stream;

public sealed class RedisStreamSubscriber(IConnectionMultiplexer connectionMultiplexer, ISerializer serializer, ILogger<RedisStreamSubscriber> logger) : IRedisStreamSubscriber
{
    private readonly IConnectionMultiplexer _connectionMultiplexer = connectionMultiplexer;
    private readonly ISerializer _serializer = serializer;
    private readonly ILogger<RedisStreamSubscriber> _logger = logger;
    private ISubscriber? _subscriber;

    private readonly ConcurrentDictionary<RedisChannel, ConcurrentBag<Delegate>> _channelHandlers = new();
    private readonly SemaphoreSlim _semaphore = new(1, 1);
    private bool _disposed;

    public async Task<ChannelSubscription> SubscribeAsync<T>(
        string channel,
        Action<T> handler,
        RedisChannel.PatternMode patternMode = RedisChannel.PatternMode.Auto,
        CancellationToken cancellationToken = default) where T : class
    {
        if (string.IsNullOrWhiteSpace(channel))
        {
            throw new ArgumentException("Channel name cannot be null or whitespace.", nameof(channel));
        }

        if (handler == null)
        {
            throw new ArgumentNullException(nameof(handler));
        }

        ThrowIfDisposed();

        ISubscriber subscriber = await GetSubscriberAsync(cancellationToken).ConfigureAwait(false);
        RedisChannel redisChannel = new(channel, patternMode);

        try
        {
            _channelHandlers.AddOrUpdate(redisChannel, [.. new[] { handler }],
                (_, existingHandlers) =>
                {
                    existingHandlers.Add(handler);
                    return existingHandlers;
                });

            if (_channelHandlers[redisChannel].Count == 1)
            {
                await subscriber.SubscribeAsync(redisChannel, (channel, message) => HandleMessage<T>(channel, message), CommandFlags.FireAndForget);
            }

            return new ChannelSubscription(redisChannel);
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("Subscribe operation to channel {Channel} was canceled.", channel);
            throw;
        }
        catch (RedisException ex)
        {
            _logger.LogError(ex, "Redis error occurred while subscribing to channel {Channel}.", channel);
            throw;
        }
        catch (Exception ex) when (ex is not ArgumentException and not OperationCanceledException)
        {
            _logger.LogError(ex, "Unexpected error occurred while subscribing to channel {Channel}.", channel);
            throw;
        }
    }

    public async Task UnsubscribeAsync(ChannelSubscription subscription, CancellationToken cancellationToken = default)
    {
        if (subscription.Equals(default))
        {
            throw new ArgumentNullException(nameof(subscription));
        }

        ThrowIfDisposed();

        var channel = subscription.Channel;

        if (!_channelHandlers.TryGetValue(channel, out _))
        {
            _logger.LogDebug("No active subscription found for channel {Channel}.", channel.ToString());
            return;
        }

        try
        {
            ISubscriber subscriber = await GetSubscriberAsync(cancellationToken).ConfigureAwait(false);

            if (_channelHandlers.TryRemove(channel, out _))
            {
                await subscriber.UnsubscribeAsync(channel, flags: CommandFlags.FireAndForget);
            }
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("Unsubscribe operation from channel {Channel} was canceled.", channel.ToString());
            throw;
        }
        catch (RedisException ex)
        {
            _logger.LogError(ex, "Redis error occurred while unsubscribing from channel {Channel}.", channel.ToString());
            throw;
        }
        catch (Exception ex) when (ex is not ArgumentException and not OperationCanceledException)
        {
            _logger.LogError(ex, "Unexpected error occurred while unsubscribing from channel {Channel}.", channel.ToString());
            throw;
        }
    }

    private void HandleMessage<T>(RedisChannel channel, RedisValue message) where T : class
    {
        if (message.IsNull)
        {
            _logger.LogWarning("Received null message from channel {Channel}.", channel.ToString());
            return;
        }

        if (!_channelHandlers.TryGetValue(channel, out var handlers) || handlers.IsEmpty)
        {
            _logger.LogWarning("No handlers registered for channel {Channel}.", channel.ToString());
            return;
        }

        try
        {
            T? payload;
            try
            {
                payload = _serializer.Deserialize<T>(message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to deserialize message from channel {Channel} to type {MessageType}.", channel.ToString(), typeof(T).Name);
                return;
            }

            foreach (var handler in handlers.ToArray())
            {
                try
                {
                    if (handler is Action<T> typedHandler)
                    {
                        typedHandler(payload);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Handler for channel {Channel} threw an exception.", channel.ToString());
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error occurred while handling message from channel {Channel}.", channel.ToString());
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

        if (_subscriber != null && _channelHandlers.Count > 0)
        {
            foreach (var channel in _channelHandlers.Keys.ToArray())
            {
                try
                {
                    _subscriber.Unsubscribe(channel, flags: CommandFlags.FireAndForget);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error unsubscribing from channel {Channel} during disposal.", channel.ToString());
                }
            }
        }

        _channelHandlers.Clear();
        _semaphore.Dispose();
        _disposed = true;
    }

    public async ValueTask DisposeAsync()
    {
        if (_disposed)
        {
            return;
        }

        if (_subscriber != null && _channelHandlers.Count > 0)
        {
            foreach (var channel in _channelHandlers.Keys.ToArray())
            {
                try
                {
                    await _subscriber.UnsubscribeAsync(channel, flags: CommandFlags.FireAndForget);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error unsubscribing from channel {Channel} during async disposal.", channel.ToString());
                }
            }
        }

        _channelHandlers.Clear();
        _semaphore.Dispose();
        _disposed = true;
    }
}
