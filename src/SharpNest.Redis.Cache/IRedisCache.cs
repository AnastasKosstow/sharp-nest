using SharpNest.Redis.Cache.Options;

namespace SharpNest.Redis.Cache;

public interface IRedisCache
{
    /// <summary>
    /// Sets a string value in the Redis cache with the specified key.
    /// <code>
    /// await cache.SetAsync("key", "value");
    /// </code>
    /// </summary>
    /// <param name="key">The key to associate with the value.</param>
    /// <param name="value">The string value to store in the cache.</param>
    /// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
    Task SetAsync(string key, string value, CancellationToken cancellationToken = default);

    /// <summary>
    /// Sets a serialized value of type T in the Redis cache with the specified key.
    /// <code>
    /// await cache.SetAsync("key", new T(), cancellationToken);
    /// </code>
    /// </summary>
    /// <typeparam name="T">The type of the value to store.</typeparam>
    /// <param name="key">The key to associate with the value.</param>
    /// <param name="value">The value to store in the cache.</param>
    /// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
    Task SetAsync<T>(string key, T value, CancellationToken cancellationToken = default);

    /// <summary>
    /// Sets a serialized value of type T in the Redis cache with the specified key and cache options.
    /// <code>
    /// await cache.SetAsync("key", new MyObject(), options.Expiration = TimeSpan.FromMinutes(10), cancellationToken);
    /// </code>
    /// </summary>
    /// <typeparam name="T">The type of the value to store.</typeparam>
    /// <param name="key">The key to associate with the value.</param>
    /// <param name="value">The value to store in the cache.</param>
    /// <param name="options">Options to configure the cache entry.</param>
    /// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
    Task SetAsync<T>(string key, T value, Action<CacheEntryOptions> options, CancellationToken cancellationToken = default);

    /// <summary>
    /// Sets a string value in the Redis cache with the specified key and cache options.
    /// <code>
    /// await cache.SetAsync("key", "value", options.Expiration = TimeSpan.FromMinutes(10), cancellationToken);
    /// </code>
    /// </summary>
    /// <param name="key">The key to associate with the value.</param>
    /// <param name="value">The string value to store in the cache.</param>
    /// <param name="options">Options to configure the cache entry.</param>
    /// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
    Task SetAsync(string key, string value, Action<CacheEntryOptions> options, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves the string value associated with the specified key from the Redis cache.
    /// <code>
    /// string value = await cache.GetAsync("key");
    /// </code>
    /// </summary>
    /// <param name="key">The key whose value to retrieve.</param>
    /// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
    /// <returns>The string value associated with the specified key.</returns>
    Task<string> GetAsync(string key, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves the deserialized value of type T associated with the specified key from the Redis cache.
    /// <code>
    /// var value = await cache.GetAsync<MyObject>("key");
    /// </code>
    /// </summary>
    /// <typeparam name="T">The type of the value to retrieve.</typeparam>
    /// <param name="key">The key whose value to retrieve.</param>
    /// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
    /// <returns>The deserialized value associated with the specified key.</returns>
    Task<T> GetAsync<T>(string key, CancellationToken cancellationToken = default);

    /// <summary>
    /// Attempts to retrieve the string value associated with the specified key from the Redis cache.
    /// <code>
    /// var (success, value) = await cache.TryGetAsync("key");
    /// </code>
    /// </summary>
    /// <param name="key">The key whose value to retrieve.</param>
    /// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
    /// <returns>A tuple indicating success and the string value associated with the key, if found.</returns>
    Task<(bool Success, string Result)> TryGetAsync(string key, CancellationToken cancellationToken = default);

    /// <summary>
    /// Attempts to retrieve the deserialized value of type T associated with the specified key from the Redis cache.
    /// <code>
    /// var (success, obj) = await cache.TryGetAsync<MyObject>("key");
    /// </code>
    /// </summary>
    /// <typeparam name="T">The type of the value to retrieve.</typeparam>
    /// <param name="key">The key whose value to retrieve.</param>
    /// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
    /// <returns>A tuple indicating success and the deserialized value associated with the key, if found.</returns>
    Task<(bool Success, T Result)> TryGetAsync<T>(string key, CancellationToken cancellationToken = default);

    /// <summary>
    /// Removes the value associated with the specified key from the Redis cache.
    /// <code>
    /// await cache.RemoveAsync("key");
    /// </code>
    /// </summary>
    /// <param name="key">The key of the value to remove.</param>
    /// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
    Task RemoveAsync(string key, CancellationToken cancellationToken = default);
}
