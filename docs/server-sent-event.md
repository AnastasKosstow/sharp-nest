# SharpNest.SSE

SharpNest.SSE is a lightweight and powerful library that simplifies the integration of Server-Sent Events (SSE) into .NET applications.
<br>
It provides a fluent API for configuring and managing SSE connections, with built-in support for different message sources and consumer handling strategies.
<br>
With SharpNest.SSE, you can easily implement real-time, one-way communication from server to client without the complexity of WebSockets or the overhead of polling.
<br>
It supports configurable channel capacity, timeout handling, and various strategies for managing slow consumers, ensuring robust real-time communication in your applications.

> [!IMPORTANT]
> Key Features:<br>
> &nbsp;&nbsp;&nbsp;✅ Fluent API – Configure SSE services with an expressive, readable syntax.<br>
> &nbsp;&nbsp;&nbsp;✅ Custom Message Sources – Easily implement and register your own message sources.<br>
> &nbsp;&nbsp;&nbsp;✅ Configurable Options – Control channel capacity, timeouts, and slow consumer strategies.<br>
> &nbsp;&nbsp;&nbsp;✅ Background Processing – Automatic background service for message handling.<br>
> &nbsp;&nbsp;&nbsp;✅ Thread-Safe – Ensures safe concurrent message distribution with proper synchronization.<br>

## 🔍 What are Server-Sent Events?

Server-Sent Events (SSE) is a standard that enables servers to push real-time updates to clients over a single HTTP connection. Unlike WebSockets, SSE:

- Provides one-way communication (server to client only)
- Uses standard HTTP, making it firewall-friendly
- Automatically reconnects if the connection is dropped
- Is simpler to implement than WebSockets
- Works natively in most modern browsers

SSE is ideal for scenarios like notifications, live feeds, status updates, and any application where clients need to receive real-time updates from the server.

## 🔧 Installation

```bash
dotnet add package SharpNest.SSE
```
<br>

## 🛠️ How to Register and use SharpNest.SSE

### 1️⃣ Add Server-Sent Event Services

To use SharpNest.SSE, add the service to your application's dependency injection container using the extension method from the `SharpNest.SSE` namespace.

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
<br>

### 2️⃣ Create a Message Source

Implement the `IMessageSource` interface to define how messages are generated:

```cs
public class NotificationSource : IMessageSource
{
    private Timer _timer;
    private int _notificationCounter = 0;
    
    public async Task StartAsync(Func<IMessage, Task> messageHandler, CancellationToken cancellationToken)
    {
        _timer = new Timer(async (state) =>
        {
            await GenerateNotificationAsync(messageHandler);
        },
        null, TimeSpan.Zero, TimeSpan.FromSeconds(20));
        
        var taskCompletionSource = new TaskCompletionSource<bool>();
        cancellationToken.Register(() => {
            _timer?.Dispose();
            taskCompletionSource.TrySetResult(true);
        });
        
        await taskCompletionSource.Task;
    }
    
    private async Task GenerateNotificationAsync(Func<IMessage, Task> messageHandler)
    {
        var notificationId = Interlocked.Increment(ref _notificationCounter);
        var notification = new Notification
        {
            Id = Guid.NewGuid().ToString(),
            Payload = $"Notification #{notificationId} at {DateTime.UtcNow}",
            Metadata = new Dictionary<string, string>
            {
                ["Type"] = "System",
                ["Importance"] = notificationId % 3 == 0 ? "High" : "Normal"
            }
        };
        
        await messageHandler(notification);
    }
}
```
<br>

### 3️⃣ Create a Controller to Stream SSE Messages

You need to create a controller endpoint that clients can connect to for receiving SSE messages:

```cs
[ApiController]
[Route("[controller]")]
public class NotificationsController(ISSEMessageHubService hub) : ControllerBase
{
    private readonly ISSEMessageHubService _hub = hub;
    
    [HttpGet("stream")]
    public async Task Stream(CancellationToken cancellationToken)
    {
        // Set required SSE headers
        Response.Headers.ContentType = "text/event-stream";
        Response.Headers.CacheControl = "no-cache";
        Response.Headers.Connection = "keep-alive";
        
        var clientClosedToken = HttpContext.RequestAborted;
        using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(clientClosedToken, cancellationToken);
        
        try
        {
            await foreach (var message in _hub.SubscribeAsync(linkedCts.Token))
            {
                if (linkedCts.Token.IsCancellationRequested)
                {
                    break;
                }
                
                await WriteMessageAsync(HttpContext, message, linkedCts.Token);
                await Response.Body.FlushAsync(linkedCts.Token);
            }
        }
        finally
        {
            linkedCts.Dispose();
        }
    }
    
    private async Task WriteMessageAsync(HttpContext context, IMessage message, CancellationToken cancellationToken)
    {
        var writer = context.Response.Body;
        
        if (!string.IsNullOrEmpty(message.Id))
        {
            await writer.WriteAsync(Encoding.UTF8.GetBytes($"id: {message.Id}\n"), cancellationToken);
        }
        
        if (message.Metadata != null && message.Metadata.TryGetValue("event", out string eventType))
        {
            await writer.WriteAsync(Encoding.UTF8.GetBytes($"event: {eventType}\n"), cancellationToken);
        }
        
        string data = JsonSerializer.Serialize(message.Payload);
        await writer.WriteAsync(Encoding.UTF8.GetBytes($"data: {data}\n\n"), cancellationToken);
    }
}
```
> [!IMPORTANT]  
> The following HTTP headers are required for proper SSE functioning. These headers ensure the connection stays open and browsers recognize it as a valid Server-Sent Events stream:
>  - Content-Type: text/event-stream - Identifies the connection as an SSE stream
>  - Cache-Control: no-cache - Prevents caching of events
>  - Connection: keep-alive - Maintains the connection open for streaming
<br>

## ⚙️ Configuration Options

SharpNest.SSE provides several configuration options:

| Option | Description | Default |
|--------|-------------|---------|
| `ChannelCapacity` | Maximum number of messages that can be buffered before applying slow consumer strategy | 200 |
| `WriteTimeout` | Maximum time allowed for writing a message to a consumer | 1 second |
| `SlowConsumerStrategy` | Strategy for handling slow consumers (`Wait`, `DropMessages`, `Disconnect`) | `Wait` |

Configure these options during service registration:

```cs
builder.Services.AddServerSentEvent()
    .Configure(options =>
    {
        options.ChannelCapacity = 1000;
        options.WriteTimeout = TimeSpan.FromSeconds(5);
        options.SlowConsumerStrategy = SlowConsumerStrategy.DropMessages;
    })
    .WithSource<NotificationSource>();
```
<br>

## 🔄 Advanced Usage

### Using Multiple Message Sources

Register multiple message sources to broadcast different types of events:

```cs
builder.Services.AddServerSentEvent()
    .Configure(options => { /* ... */ })
    .WithSource<SystemNotificationSource>()
    .WithSource<UserActivitySource>()
    .WithSource<MetricsSource>();
```

### Creating Message Sources with Dependencies

Use the factory method to create message sources with dependencies:

```cs
builder.Services.AddServerSentEvent()
    .WithSource(sp => new DatabaseChangeSource(
        sp.GetRequiredService<IDbConnection>(),
        sp.GetRequiredService<ILogger<DatabaseChangeSource>>()
    ));
```
<br>

## 📚 Best Practices

1. **Set appropriate channel capacity**: Match your expected message rate to avoid memory issues.
2. **Choose the right slow consumer strategy**: For critical applications, consider `Wait` or `Disconnect` instead of `DropMessages`.
3. **Implement proper error handling**: Both on server and client sides.
4. **Keep messages small**: Large payloads can impact performance.
5. **Use message IDs**: This helps with reconnection and ensuring message delivery.
