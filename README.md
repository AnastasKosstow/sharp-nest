<img src="https://img.shields.io/badge/version-9.0-CC0000?style=for-the-badge&logo=.NET" 
     alt="NET badge" 
     width="170">

![NuGet Version](https://img.shields.io/nuget/v/SharpNest.Decorator)
![Build](https://github.com/AnastasKosstow/sharp-nest/actions/workflows/build.yml/badge.svg)
![Tests](https://github.com/AnastasKosstow/sharp-nest/actions/workflows/tests.yml/badge.svg)

# Intro
This repository contains implementations of various tools or design patterns that simplify integration of .net projects with these technologies.

* [Fluent Decorator](#fluent-decorator)
* [Server-Sent Event](#server-sent-event)
* [Redis](#redis)
* [Kafka](#kafka)
* [Cache](#cache)


### Installation

```bash
dotnet add package SharpNest.Decorator
dotnet add package SharpNest.Redis
dotnet add package SharpNest.Kafka
dotnet add package SharpNest.SSE
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

> [!Important]
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

> [!Important]
> #### Documentation: [here](https://github.com/AnastasKosstow/sharp-nest/blob/main/docs/redis.md) <br>
> #### Sample app: [here](https://github.com/AnastasKosstow/sharp-nest/tree/main/samples/redis/src/SharpNest.Samples.Redis.Api)

---

## Kafka

SharpNest.Kafka is a robust and flexible .NET library that simplifies Apache Kafka integration for your .NET applications.
<br>
It provides a clean abstraction over the Confluent.Kafka client with an intuitive API for publishing messages and consuming from topics.
<br>
With SharpNest.Kafka, you can easily implement resilient message handling with built-in retry strategies, automatic topic creation, and proper resource management.
<br>

### Basic usage

```cs
using SharpNest.Kafka;

...
builder.Services
    .AddKafka(builder.Configuration)
    .AddPublisher()
    .AddSingletonSubscriber();
```

### Publish
```cs
public class MessageService(IPublisher publisher)
{
    private readonly IPublisher _publisher = publisher;

    ...
}
    public async Task SendMessageAsync(string key, string content)
    {
        var result = await _publisher.PublishAsync(
            "my-topic",  // Topic name
            key,         // Message key
            content      // Message content
        );

        ...   
    }
```

### Subscribe

```csharp
public class MessageConsumerService(ISubscriber subscriber, ILogger<MessageConsumerService> logger)
{
    private readonly ISubscriber _subscriber = subscriber;
    private readonly ILogger<MessageConsumerService> _logger = logger;
    
    public async Task StartConsumingAsync(CancellationToken cancellationToken)
    {
        await _subscriber.SubscribeAsync(
            "my-topic",
            async message => 
            {
                _logger.LogInformation(
                    "Received message: Key={Key}, Value={Value}", 
                    message.Key, 
                    message.Value
                );
                
                await ProcessMessageAsync(message);
            },
            "my-consumer-group",
            cancellationToken
        );
    }
    
    private Task ProcessMessageAsync(IMessage message)
    {
        // Process message

        return Task.CompletedTask;
    }
}
```

> [!Important]
> #### Documentation: [here](https://github.com/AnastasKosstow/sharp-nest/blob/main/docs/kafka.md) <br>
> #### Sample app: [here](https://github.com/AnastasKosstow/sharp-nest/tree/main/samples/kafka/src/SharpNest.Samples.Kafka)


## Server-Sent Event

SharpNest.SSE is a lightweight and powerful library that simplifies the integration of Server-Sent Events (SSE) into .NET applications.
<br>
It provides a fluent API for configuring and managing SSE connections, with built-in support for different message sources and consumer handling strategies.
<br>
With SharpNest.SSE, you can easily implement real-time, one-way communication from server to client without the complexity of WebSockets or the overhead of polling.
<br>
It supports configurable channel capacity, timeout handling, and various strategies for managing slow consumers, ensuring robust real-time communication in your applications.

#### 🔍 What are Server-Sent Events?

Server-Sent Events (SSE) is a standard that enables servers to push real-time updates to clients over a single HTTP connection. Unlike WebSockets, SSE:

- Provides one-way communication (server to client only)
- Uses standard HTTP, making it firewall-friendly
- Automatically reconnects if the connection is dropped
- Is simpler to implement than WebSockets
- Works natively in most modern browsers

SSE is ideal for scenarios like notifications, live feeds, status updates, and any application where clients need to receive real-time updates from the server.

```cs
// Program.cs
using SharpNest.SSE;

builder.Services.AddServerSentEvent()
    .Configure(options =>
    {
        options.ChannelCapacity = 500;
        options.WriteTimeout = TimeSpan.FromSeconds(2);
        options.SlowConsumerStrategy = SlowConsumerStrategy.DropMessages;
    })
    .WithSource<YourCustomMessageSource>();
```

> [!Important]
> #### Documentation: [here](https://github.com/AnastasKosstow/sharp-nest/blob/main/docs/server-sent-event.md) <br>
> #### Sample app: [here](https://github.com/AnastasKosstow/sharp-nest/tree/main/samples/sse/src/SharpNest.Samples.SSE)







---
