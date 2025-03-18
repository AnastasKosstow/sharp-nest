<img src="https://img.shields.io/badge/version-9.0-CC0000?style=for-the-badge&logo=.NET" 
     alt="NET badge" 
     width="170">

![NuGet Version](https://img.shields.io/nuget/v/SharpNest.Decorator)
![Build](https://github.com/AnastasKosstow/sharp-nest/actions/workflows/build.yml/badge.svg)
![Tests](https://github.com/AnastasKosstow/sharp-nest/actions/workflows/tests.yml/badge.svg)

# Intro
This repository contains implementations of various tools or design patterns that simplify integration of .net projects with these technologies.

* [Fluent Decorator](#fluent-decorator)
* [Redis](#redis)

### Installation

```bash
dotnet add package SharpNest
```

---

## Fluent Decorator

Lightweight and extensible tool that simplifies the implementation of the Decorator Pattern in .NET applications.
<br>
It provides a fluent API for registering services with multiple decorators in Dependency Injection (DI) container while ensuring proper lifetime management and correct disposal of decorated services.
<br>

### Basic usage

```cs
using SharpNest.Decorator;

...
services.AddDecorator();
```

```cs
services.AddSingletonService<IWeatherService>(
    configurator =>
    {
        configurator
            .AddDecorator<WeatherServiceLoggingDecorator>()
            .AddService<WeatherService>();
    });
```

> [!TIP]
> #### Documentation: [here](https://github.com/AnastasKosstow/sharp-nest/blob/main/docs/fluent-decorator.md) <br>
> #### Sample app: [here](https://github.com/AnastasKosstow/sharp-nest/tree/main/samples/decorator/src/SharpNest.Samples.Decorator)

---

## Redis

Powerful Redis client for .NET applications that provides a clean, strongly-typed API for Redis operations.
<br>
It offers seamless integration with dependency injection and supports both caching and pub/sub messaging patterns with minimal configuration.
<br>

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

> [!TIP]
> #### Documentation: [here](https://github.com/AnastasKosstow/sharp-nest/blob/main/docs/redis.md) <br>
> #### Sample app: [here](https://github.com/AnastasKosstow/sharp-nest/tree/main/samples/redis/src/SharpNest.Samples.Redis.Api)

---

