# SharpNest.Redis

SharpNest.Redis is a lightweight and powerful Redis client for .NET applications that simplifies Redis interactions with a clean and intuitive API.
<br>
It provides seamless integration with dependency injection, strongly-typed operations, and support for both caching and pub/sub messaging patterns.
<br>
With SharpNest.Redis, you can easily implement distributed caching, session management, and real-time communication without worrying about connection management or serialization details.
<br>

> [!IMPORTANT]
> Key Features:<br>
> &nbsp;&nbsp;&nbsp;✅ Simple Integration – Easily register Redis services with dependency injection.<br>
> &nbsp;&nbsp;&nbsp;✅ Type-Safe Operations – Strongly-typed cache operations with automatic serialization.<br>
> &nbsp;&nbsp;&nbsp;✅ Expiration Policies – Flexible cache expiration with absolute and sliding options.<br>
> &nbsp;&nbsp;&nbsp;✅ Pub/Sub Messaging – Asynchronous communication with Redis streams.<br>
> &nbsp;&nbsp;&nbsp;✅ Exception Handling – Robust error handling with meaningful exceptions.<br>

## 🔧 Installation

```bash
dotnet add package SharpNest.Redis
```

## 🛠️ How to Register and Use SharpNest.Redis

1️⃣ Add configuration in your appsettings.json file:
```json
"SharpNest.Redis": {
  "ConnectionString": "localhost:6379"
}
```

2️⃣ Add SharpNest.Redis services<br>
To use SharpNest.Redis, you need to add the Redis services you want to use in your Program.cs or Startup.cs:

```cs
// Program.cs

using SharpNest.Redis;

// Add both cache and streaming
builder.Services
    .AddRedis(config =>
    {
        config
            .AddRedisCache()       // Add Redis cache
            .AddRedisStreaming();  // Add Redis pub/sub
    });

// Or add only what you need
builder.Services.AddRedis(config => config.AddRedisCache());
builder.Services.AddRedis(config => config.AddRedisStreaming());
```

## 📦 Cache Operations

The Redis cache functionality provides a robust and type-safe way to interact with Redis for caching purposes.

1️⃣ Inject the IRedisCache interface:

```cs
private readonly IRedisCache cache;

public MyService(IRedisCache cache)
{
    this.cache = cache;
}
```

2️⃣ Use cache operations:

📌 Set values in cache
```cs
// Set a simple string value
await cache.SetAsync("user:login:status", "active");

// Set a complex object with automatic serialization
var user = new UserProfile 
{ 
    Id = 1234, 
    Name = "John Doe" 
};
await cache.SetAsync("user:profile:1234", user);

// Set with expiration
await cache.SetAsync("session:token", sessionToken, options => 
{
    options.AbsoluteExpiration = DateTimeOffset.Now.AddHours(1);
    options.SlidingExpiration = TimeSpan.FromMinutes(20);
});
```

📌 Get values from cache
```cs
// Get a string value
string status = await cache.GetAsync("user:login:status");

// Get a complex object with automatic deserialization
var profile = await cache.GetAsync<UserProfile>("user:profile:1234");

// Safely try to get a value
var (found, token) = await cache.TryGetAsync("session:token");
if (found)
{
    // Use the token
}
```

📌 Remove values from cache
```cs
await cache.RemoveAsync("session:token");
```

## 📡 Streaming (Pub/Sub)

SharpNest.Redis provides a convenient way to implement Publish/Subscribe patterns using Redis streams.

1️⃣ Publishing messages:

```cs
private readonly IRedisStreamPublisher publisher;

public PublisherService(IRedisStreamPublisher publisher)
{
    this.publisher = publisher;
}

public async Task SendNotification(Notification notification)
{
    // Publish a complex object
    await publisher.PublishAsync("notifications", notification);
    
    // Publish a simple string
    await publisher.PublishAsync("status_updates", "System maintenance completed");
}
```

2️⃣ Subscribing to messages:

```cs
internal sealed class NotificationService : BackgroundService
{
    private readonly IRedisStreamSubscriber subscriber;

    public NotificationService(IRedisStreamSubscriber subscriber)
    {
        this.subscriber = subscriber;
    }

    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        // Subscribe asynchronously
        await subscriber.SubscribeAsync<Notification>("notifications", notification =>
        {
            Console.WriteLine($"Received: {notification.Message}");
            // Process the notification
        });
        
        // Or subscribe synchronously
        subscriber.Subscribe<string>("status_updates", status =>
        {
            Console.WriteLine($"Status update: {status}");
            // Process the status update
        });
    }
}
```

## 🧩 Advanced Usage

You can combine SharpNest.Redis with SharpNest.Decorator to add cross-cutting concerns:

```cs
services.AddScopedService<IUserRepository>(
    configurator =>
    {
        configurator
            .AddDecorator<UserRepositoryCacheDecorator>() // Add caching decorator
            .AddDecorator<UserRepositoryLoggingDecorator>() // Add logging decorator
            .AddService<UserRepository>(); // Add actual implementation
    });
```

The `UserRepositoryCacheDecorator` could use the `IRedisCache` to cache user data, reducing database load.