
Powerful Redis client for .NET applications that provides a clean, strongly-typed API for Redis operations.
It offers seamless integration with dependency injection and supports both caching and pub/sub messaging patterns with minimal configuration.

### Basic usage

 ```cs
 services
    .AddRedis(config =>
    {
        config
            .AddRedisCache() // Add redis cache
            .AddRedisStreaming(); // Add redis Pub/Sub
    });
 ```


```cs
private readonly IRedisCache _cache;
...

string key = "redis:key";

await _cache.SetAsync(key, value, cacheEntryOptions =>
{
    cacheEntryOptions.Expiration = new TimeSpan(1, 0, 0);
});

var (Success, Result) = await _cache.TryGetAsync(key, cancellationToken);
if (!Success)
{
    // handle null result
}
```

> #### Documentation: [here](https://github.com/AnastasKosstow/sharp-nest/blob/main/docs/redis.md)
> #### Sample app: [here](https://github.com/AnastasKosstow/sharp-nest/tree/main/samples/redis/src/SharpNest.Samples.Redis.Api)