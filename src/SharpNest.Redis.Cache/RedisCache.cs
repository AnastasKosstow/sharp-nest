using System.Text.Json;
using SharpNest.Redis.Cache.Exceptions;
using SharpNest.Redis.Cache.Options;
using StackExchange.Redis;

namespace SharpNest.Redis.Cache;

public sealed class RedisCache(IConnectionMultiplexer connection) : IRedisCache, IDisposable
{
    private IDatabase redisCache;

    private readonly SemaphoreSlim connectionLock = new(initialCount: 1, maxCount: 1);
    private bool _disposed;

    /*
        Use "volatile" instead of "readonly". 
        This is because the connection object is assigned in the constructor and can also be changed later in the ConnectAsync method.
        Using volatile in this case ensures that the most up-to-date value of connection is accessed across multiple threads and its state can be changed by any thread at any time,
        without causing unexpected behavior.
    */
    private volatile IConnectionMultiplexer connection = connection;

    #region PUBLIC

    public async Task SetAsync(string key, string value, CancellationToken cancellationToken = default)
    {
        await Set(key, value, null, cancellationToken);
    }

    public async Task SetAsync(string key, string value, Action<CacheEntryOptions> options, CancellationToken cancellationToken = default)
    {
        await Set(key, value, options, cancellationToken);
    }

    public async Task SetAsync<T>(string key, T value, CancellationToken cancellationToken = default)
    {
        string serializedValue = SerializeValue(value);
        await Set(key, serializedValue, null, cancellationToken);
    }

    public async Task SetAsync<T>(string key, T value, Action<CacheEntryOptions> options, CancellationToken cancellationToken = default)
    {
        string serializedValue = SerializeValue(value);
        await Set(key, serializedValue, options, cancellationToken);
    }

    public async Task<T> GetAsync<T>(string key, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key, nameof(key));

        var redisValue = await Get(key, cancellationToken);
        T result = DeserializeValue<T>(redisValue);
        return result;
    }

    public async Task<string> GetAsync(string key, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key, nameof(key));

        var result = await Get(key, cancellationToken);
        return result;
    }

    public async Task<(bool Success, T Result)> TryGetAsync<T>(string key, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key, nameof(key));

        var result = await TryGet(key, cancellationToken);
        if (!result.HasValue)
        {
            return (false, default(T));
        }

        T deserializeResult = DeserializeValue<T>(result);
        return (true, deserializeResult);
    }

    public async Task<(bool Success, string Result)> TryGetAsync(string key, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key, nameof(key));

        var result = await TryGet(key, cancellationToken);
        if (!result.HasValue)
        {
            return (false, null);
        }

        return (true, result);
    }

    public async Task RemoveAsync(string key, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key, nameof(key));
        await ConnectAsync(cancellationToken);
        await redisCache.KeyDeleteAsync(key);
    }

    #endregion

    #region PRIVATE

    private async Task Set(string key, string value, Action<CacheEntryOptions> options, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key, nameof(key));
        ArgumentException.ThrowIfNullOrWhiteSpace(value, nameof(value));

        cancellationToken.ThrowIfCancellationRequested();
        await ConnectAsync(cancellationToken);

        if (options != null)
        {
            var entryOptions = new CacheEntryOptions();
            options?.Invoke(entryOptions);

            await redisCache.StringSetAsync(key, value, entryOptions.Expiration);
        }
        else
        {
            await redisCache.StringSetAsync(key, value);
        }
    }

    private async Task<string> Get(string key, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        await ConnectAsync(cancellationToken);

        RedisValue result = await redisCache.StringGetAsync(key);
        if (!result.HasValue)
        {
            throw new MissingCacheValueException($"Redis cache does not contain a value for key: {key}");
        }

        return result;
    }

    private async Task<RedisValue> TryGet(string key, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        await ConnectAsync(cancellationToken);

        RedisValue redisValue = await redisCache.StringGetAsync(key);
        return redisValue;
    }

    private static string SerializeValue(object value)
    {
        try
        {
            return JsonSerializer.Serialize(value);
        }
        catch (JsonException ex)
        {
            throw new CacheSerializationException("Failed to serialize value.", ex);
        }
    }

    private static T DeserializeValue<T>(RedisValue redisValue)
    {
        try
        {
            return JsonSerializer.Deserialize<T>(redisValue);
        }
        catch (JsonException ex)
        {
            throw new CacheSerializationException("Failed to deserialize result.", ex);
        }
    }

    private async Task ConnectAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (redisCache != null)
        {
            return;
        }

        await connectionLock.WaitAsync(cancellationToken);
        try
        {
            redisCache ??= connection.GetDatabase();
        }
        finally
        {
            connectionLock.Release();
        }
    }

#endregion

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;

        if (connection != null)
        {
            connection.Close();
            connection.Dispose();
        }
        connectionLock.Dispose();
    }
}
