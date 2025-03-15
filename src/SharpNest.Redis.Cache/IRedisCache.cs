using SharpNest.Redis.Cache.Options;

namespace SharpNest.Redis.Cache;

public interface IRedisCache
{
    /// <summary>
    /// Sets a string value in the Redis cache with the specified key.
    /// </summary>
    /// <param name="key">The key to associate with the value. Cannot be null or empty.</param>
    /// <param name="value">The string value to store in the cache.</param>
    /// <param name="cancellationToken">A token that can be used to request cancellation of the operation.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    /// <example>
    /// <code>
    /// // Store a simple string value
    /// await redisCache.SetAsync("user:last-login:1234", DateTime.UtcNow.ToString("o"));
    /// </code>
    /// </example>
    Task SetAsync(string key, string value, CancellationToken cancellationToken = default);

    /// <summary>
    /// Sets a serialized value of type <typeparamref name="T"/> in the Redis cache with the specified key.
    /// </summary>
    /// <typeparam name="T">The type of the value to store. Must be serializable.</typeparam>
    /// <param name="key">The key to associate with the value. Cannot be null or empty.</param>
    /// <param name="value">The value to store in the cache. Will be serialized automatically.</param>
    /// <param name="cancellationToken">A token that can be used to request cancellation of the operation.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    /// <example>
    /// <code>
    /// // Store a complex object
    /// var userProfile = new UserProfile 
    /// { 
    ///     Id = 1234, 
    ///     Name = "John Doe", 
    ///     LastLogin = DateTime.UtcNow 
    /// };
    /// await redisCache.SetAsync("user:profile:1234", userProfile);
    /// </code>
    /// </example>
    Task SetAsync<T>(string key, T value, CancellationToken cancellationToken = default);

    /// <summary>
    /// Sets a serialized value of type <typeparamref name="T"/> in the Redis cache with the specified key and cache options.
    /// </summary>
    /// <typeparam name="T">The type of the value to store. Must be serializable.</typeparam>
    /// <param name="key">The key to associate with the value. Cannot be null or empty.</param>
    /// <param name="value">The value to store in the cache. Will be serialized automatically.</param>
    /// <param name="options">An action to configure the cache entry options, such as expiration time and priority.</param>
    /// <param name="cancellationToken">A token that can be used to request cancellation of the operation.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    /// <example>
    /// <code>
    /// // Store a complex object with expiration
    /// var sessionData = new SessionData 
    /// { 
    ///     UserId = 1234, 
    ///     Permissions = new[] { "read", "write" },
    ///     LastActivity = DateTime.UtcNow 
    /// };
    /// 
    /// await redisCache.SetAsync("session:1234", sessionData, options => 
    /// {
    ///     options.AbsoluteExpiration = DateTimeOffset.Now.AddHours(1);
    ///     options.SlidingExpiration = TimeSpan.FromMinutes(20);
    ///     options.Priority = CacheItemPriority.High;
    /// });
    /// </code>
    /// </example>
    Task SetAsync<T>(string key, T value, Action<CacheEntryOptions> options, CancellationToken cancellationToken = default);

    /// <summary>
    /// Sets a string value in the Redis cache with the specified key and cache options.
    /// </summary>
    /// <param name="key">The key to associate with the value. Cannot be null or empty.</param>
    /// <param name="value">The string value to store in the cache.</param>
    /// <param name="options">An action to configure the cache entry options, such as expiration time and priority.</param>
    /// <param name="cancellationToken">A token that can be used to request cancellation of the operation.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    /// <example>
    /// <code>
    /// // Store a rate-limited value with expiration
    /// await redisCache.SetAsync("rate-limit:ip:192.168.1.1", "5", options => 
    /// {
    ///     options.AbsoluteExpiration = DateTimeOffset.Now.AddMinutes(15);
    /// });
    /// </code>
    /// </example>
    Task SetAsync(string key, string value, Action<CacheEntryOptions> options, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves the string value associated with the specified key from the Redis cache.
    /// </summary>
    /// <param name="key">The key whose value to retrieve. Cannot be null or empty.</param>
    /// <param name="cancellationToken">A token that can be used to request cancellation of the operation.</param>
    /// <returns>
    /// A task that represents the asynchronous operation. The task result contains the string value 
    /// associated with the specified key, or null if the key does not exist in the cache.
    /// </returns>
    /// <example>
    /// <code>
    /// // Retrieve a string value from the cache
    /// string lastLoginTime = await redisCache.GetAsync("user:last-login:1234");
    /// if (lastLoginTime != null)
    /// {
    ///     DateTime lastLogin = DateTime.Parse(lastLoginTime);
    ///     // Process the login timestamp
    /// }
    /// </code>
    /// </example>
    Task<string> GetAsync(string key, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves the deserialized value of type <typeparamref name="T"/> associated with the specified key from the Redis cache.
    /// </summary>
    /// <typeparam name="T">The type of the value to retrieve.</typeparam>
    /// <param name="key">The key whose value to retrieve. Cannot be null or empty.</param>
    /// <param name="cancellationToken">A token that can be used to request cancellation of the operation.</param>
    /// <returns>
    /// A task that represents the asynchronous operation. The task result contains the deserialized value 
    /// of type <typeparamref name="T"/> associated with the specified key, or the default value for <typeparamref name="T"/> 
    /// if the key does not exist in the cache.
    /// </returns>
    /// <example>
    /// <code>
    /// // Retrieve and use a complex object from the cache
    /// var userProfile = await redisCache.GetAsync&lt;UserProfile&gt;("user:profile:1234");
    /// if (userProfile != null)
    /// {
    ///     Console.WriteLine($"User: {userProfile.Name}, Last Login: {userProfile.LastLogin}");
    /// }
    /// </code>
    /// </example>
    Task<T> GetAsync<T>(string key, CancellationToken cancellationToken = default);

    /// <summary>
    /// Attempts to retrieve the string value associated with the specified key from the Redis cache.
    /// </summary>
    /// <param name="key">The key whose value to retrieve. Cannot be null or empty.</param>
    /// <param name="cancellationToken">A token that can be used to request cancellation of the operation.</param>
    /// <returns>
    /// A task that represents the asynchronous operation. The task result contains a tuple with:
    /// <list type="bullet">
    ///   <item><description>A boolean indicating whether the key was found in the cache (Success)</description></item>
    ///   <item><description>The string value associated with the key, or null if the key was not found (Result)</description></item>
    /// </list>
    /// </returns>
    /// <example>
    /// <code>
    /// // Safely attempt to retrieve a value from the cache
    /// var (found, apiToken) = await redisCache.TryGetAsync("api:token:service1");
    /// if (found)
    /// {
    ///     // Use the token for API requests
    ///     httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiToken);
    /// }
    /// else
    /// {
    ///     // Token not found, request a new one
    ///     apiToken = await tokenService.RequestNewTokenAsync();
    ///     await redisCache.SetAsync("api:token:service1", apiToken, options => 
    ///         options.AbsoluteExpiration = DateTimeOffset.Now.AddHours(1));
    /// }
    /// </code>
    /// </example>
    Task<(bool Success, string Result)> TryGetAsync(string key, CancellationToken cancellationToken = default);

    /// <summary>
    /// Attempts to retrieve the deserialized value of type <typeparamref name="T"/> associated with the specified key from the Redis cache.
    /// </summary>
    /// <typeparam name="T">The type of the value to retrieve.</typeparam>
    /// <param name="key">The key whose value to retrieve. Cannot be null or empty.</param>
    /// <param name="cancellationToken">A token that can be used to request cancellation of the operation.</param>
    /// <returns>
    /// A task that represents the asynchronous operation. The task result contains a tuple with:
    /// <list type="bullet">
    ///   <item><description>A boolean indicating whether the key was found in the cache and was successfully deserialized (Success)</description></item>
    ///   <item><description>The deserialized value of type <typeparamref name="T"/> associated with the key, or the default value for <typeparamref name="T"/> if the key was not found or deserialization failed (Result)</description></item>
    /// </list>
    /// </returns>
    /// <remarks>
    /// Unlike <see cref="GetAsync{T}(string, CancellationToken)"/>, this method does not throw exceptions for serialization or type compatibility issues.
    /// Instead, it returns Success = false if the value cannot be properly retrieved or deserialized.
    /// </remarks>
    /// <example>
    /// <code>
    /// // Safely attempt to retrieve a complex object from the cache
    /// var (found, config) = await redisCache.TryGetAsync&lt;AppConfiguration&gt;("app:config:v1");
    /// if (found)
    /// {
    ///     // Use the cached configuration
    ///     app.Configure(config);
    /// }
    /// else
    /// {
    ///     // Configuration not found or invalid, load from database
    ///     config = await configRepository.LoadConfigurationAsync();
    ///     await redisCache.SetAsync("app:config:v1", config);
    /// }
    /// </code>
    /// </example>
    Task<(bool Success, T Result)> TryGetAsync<T>(string key, CancellationToken cancellationToken = default);

    /// <summary>
    /// Removes the value associated with the specified key from the Redis cache.
    /// </summary>
    /// <param name="key">The key of the value to remove. Cannot be null or empty.</param>
    /// <param name="cancellationToken">A token that can be used to request cancellation of the operation.</param>
    /// <returns>
    /// A task that represents the asynchronous operation. The task will complete when the cache entry 
    /// is removed or if the key does not exist in the cache.
    /// </returns>
    /// <example>
    /// <code>
    /// // Remove a cache entry upon user logout
    /// await redisCache.RemoveAsync($"user:session:{userId}");
    /// </code>
    /// </example>
    Task RemoveAsync(string key, CancellationToken cancellationToken = default);
}
