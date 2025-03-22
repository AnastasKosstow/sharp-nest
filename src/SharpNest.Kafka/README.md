# SharpNest.Kafka

SharpNest.Kafka is a robust and flexible .NET library that simplifies Apache Kafka integration for your .NET applications.
It provides a clean abstraction over the Confluent.Kafka client with an intuitive API for publishing messages and consuming from topics.

With SharpNest.Kafka, you can easily implement resilient message handling with built-in retry strategies, automatic topic creation, and proper resource management.

> [!IMPORTANT]
> Key Features:
> &nbsp;&nbsp;&nbsp;✅ Fluent Configuration API – Configure Kafka settings in a readable, expressive manner.<br>
> &nbsp;&nbsp;&nbsp;✅ Resilient Messaging – Built-in retry strategies for handling transient failures.<br>
> &nbsp;&nbsp;&nbsp;✅ Auto Topic Creation – Topics are created automatically if they don't exist.<br>
> &nbsp;&nbsp;&nbsp;✅ Proper Resource Management – Ensures correct disposal of Kafka connections and resources.<br>
> &nbsp;&nbsp;&nbsp;✅ Lifetime Control – Supports Singleton, Scoped, and Transient service lifetimes.<br>
> &nbsp;&nbsp;&nbsp;✅ Thread-Safe – Ensures safe concurrent execution with proper synchronization.<br>

## 🔧 Installation

```bash
dotnet add package SharpNest.Kafka
```

## 🛠️ How to Register and Use SharpNest.Kafka

### 1️⃣ Add `SharpNest.Kafka` services

There are two ways to configure SharpNest.Kafka services:

📌 **Using configuration**
```cs
// Program.cs
using SharpNest.Kafka;

builder.Services
    .AddKafka(builder.Configuration)
    .AddPublisher()
    .AddSingletonSubscriber();
```

📌 **Using programmatic configuration**
```cs
services.AddKafka(options => 
{
    options.BootstrapServers = "localhost:9092";
    options.DefaultGroup = "my-consumer-group";
    options.Partitions = 3;
    options.ReplicationFactor = 2;
})
.AddPublisher()
.AddScopedSubscriber();
```

### 2️⃣ Configure Kafka Settings

Add the following to your `appsettings.json`:

```json
{
  "Kafka": {
    "BootstrapServers": "localhost:9092",
    "DefaultGroup": "default-group",
    "Partitions": 1,
    "ReplicationFactor": 1,
    "Subscriber": {
      "CommitEmptyMessages": true
    },
    "Publisher": {
      // Publisher-specific settings
    },
    "Security": {
      // Optional security settings
    }
  }
}
```

### 3️⃣ Publishing Messages

Inject the `IPublisher` interface to publish messages to Kafka:

```cs
public class MessageService(IPublisher publisher)
{
    private readonly IPublisher _publisher = publisher;

    ...
}
```

Simple message publishing
```cs
var result = await _publisher.PublishAsync(
    "my-topic",  // Topic name
    key,         // Message key
    content      // Message content
);
```

Publishing with custom headers
```cs
var headers = new KeyValuePair<string, byte[]>[] 
{ 
    new("content-type", Encoding.UTF8.GetBytes("application/json")),
    new("correlation-id", Encoding.UTF8.GetBytes(Guid.NewGuid().ToString()))
};

await _publisher.PublishAsync(
    "my-topic", 
    key, 
    content, 
    CancellationToken.None, 
    headers
);
```

Using KafkaMessage object
```cs
var message = new KafkaMessage
{
    Topic = "my-topic",
    Key = key,
    Value = content,
    Headers = new Dictionary<string, byte[]>
    {
        ["timestamp"] = Encoding.UTF8.GetBytes(DateTime.UtcNow.ToString("O"))
    }
};

await _publisher.PublishAsync(message);
```

### 4️⃣ Subscribing to Topics

#### 📌 Single Service Subscription

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

#### 📌 Background Service for Continuous Consumption

Create a background service to continuously consume messages:

```csharp
public class KafkaConsumerBackgroundService(ISubscriber subscriber, ILogger<KafkaConsumerBackgroundService> logger) : BackgroundService
{
    private readonly ISubscriber _subscriber = subscriber;
    private readonly ILogger<KafkaConsumerBackgroundService> _logger = logger;
    
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            await _subscriber.SubscribeManyAsync(
                new[] { "topic1", "topic2", "topic3" },
                async message =>
                {
                    _logger.LogInformation(
                        "Message received: Topic={Topic}, Key={Key}",
                        message.Topic,
                        message.Key
                    );
                    
                    // Process message
                    await ProcessMessageAsync(message);
                },
                "multi-topic-consumer-group",
                stoppingToken
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in Kafka consumer service");
        }
    }
    
    private Task ProcessMessageAsync(IMessage message)
    {
        // Process message

        return Task.CompletedTask;
    }
}

// Register the background service
services.AddHostedService<KafkaConsumerBackgroundService>();
```

### 5️⃣ Advanced Configuration

#### 📌 Configure Publisher Settings

```csharp
services.AddKafka(Configuration.GetSection("Kafka"))
    .ConfigurePublisher(options => 
    {
        // Configure publisher-specific settings
    })
    .WithAdvancedProducerConfig(config => 
    {
        config.MessageTimeoutMs = 10000;
        config.RequestTimeoutMs = 5000;
        config.Acks = Acks.All;
    })
    .AddPublisher();
```

#### 📌 Configure Subscriber Settings

```csharp
services.AddKafka(Configuration.GetSection("Kafka"))
    .ConfigureSubscriber(options => 
    {
        options.CommitEmptyMessages = true;
        // Configure other subscriber-specific settings
    })
    .WithAdvancedConsumerConfig(config => 
    {
        config.AutoOffsetReset = AutoOffsetReset.Earliest;
        config.EnableAutoCommit = false;
        config.MaxPollIntervalMs = 300000;
    })
    .AddSingletonSubscriber();
```

## 🚀 Complete Example: ASP.NET Core Web API with Kafka Integration: [here](https://github.com/AnastasKosstow/sharp-nest/tree/main/samples/kafka/src/SharpNest.Samples.Kafka)

# SharpNest.Kafka Documentation

## 📋 Available Options

### KafkaSettings

| Property | Description | Default |
|----------|-------------|---------|
| `BootstrapServers` | Comma-separated list of Kafka broker addresses | `localhost:9092` |
| `DefaultGroup` | Default consumer group ID | `default-group` |
| `Partitions` | Number of partitions for new topics | `1` |
| `ReplicationFactor` | Replication factor for new topics | `1` |
| `Security` | Security settings object | `null` |
| `Subscriber` | Subscriber-specific settings | `new KafkaSubscriberSettings()` |
| `Publisher` | Publisher-specific settings | `new KafkaPublisherSettings()` |

### KafkaSubscriberSettings

| Property | Description | Default |
|----------|-------------|---------|
| `AutoOffsetReset` | Offset reset behavior (earliest, latest, error) | `earliest` |
| `EnableAutoCommit` | Whether to enable auto-commit of offsets | `true` |
| `CommitEmptyMessages` | Whether to commit empty messages | `false` |
| `AutoCommitIntervalMs` | Auto-commit interval in milliseconds | `5000` |
| `SessionTimeoutMs` | Session timeout in milliseconds | `30000` |
| `MaxPollIntervalMs` | Maximum poll interval in milliseconds | `300000` |
| `MaxPartitionFetchBytes` | Number of messages to request in each fetch | `1048576` |

### KafkaPublisherSettings

| Property | Description | Default |
|----------|-------------|---------|
| `Acks` | Required acknowledgments (0, 1, all) | `all` |
| `MessageTimeoutMs` | Message timeout in milliseconds | `30000` |
| `CompressionType` | Compression type (none, gzip, snappy, lz4, zstd) | `none` |
| `BatchSize` | Maximum size of a batch in bytes | `16384` |
| `LingerMs` | Linger time in milliseconds | `5` |
| `MaxInFlight` | Maximum number of in-flight requests | `5` |

### KafkaSecuritySettings

| Property | Description | Default |
|----------|-------------|---------|
| `Protocol` | Security protocol | `SecurityProtocol.Plaintext` |
| `SaslMechanism` | SASL mechanism | `SaslMechanism.Plain` |
| `Username` | SASL username | `null` |
| `Password` | SASL password | `null` |
| `SslCaLocation` | SSL CA certificate location | Not set in class definition |
| `SslCertificateLocation` | SSL certificate location | Not set in class definition |
| `SslKeyLocation` | SSL key location | Not set in class definition |

## 🔒 Secure Kafka Connections

SharpNest.Kafka supports secure connections to Kafka brokers:

```cs
services.AddKafka(options => 
{
    options.BootstrapServers = "kafka-broker:9093";
    options.Security = new KafkaSecuritySettings
    {
        Protocol = SecurityProtocol.Ssl,
        SslCaLocation = "/path/to/ca.pem",
        SslCertificateLocation = "/path/to/certificate.pem",
        SslKeyLocation = "/path/to/key.pem"
    };
})
.AddPublisher()
.AddSingletonSubscriber();
```

Or in `appsettings.json`:

```json
{
  "Kafka": {
    "BootstrapServers": "kafka-broker:9093",
    "Security": {
      "Protocol": "Ssl",
      "Username": "usr",
      "Password": "pswrd",
      "SslCaLocation": "/path/to/ca.pem",
      "SslCertificateLocation": "/path/to/certificate.pem",
      "SslKeyLocation": "/path/to/key.pem"
    }
  }
}
```

**Note:** The SSL certificate properties (`SslCaLocation`, `SslCertificateLocation`, `SslKeyLocation`) are referenced in the examples but appear to be handled differently in your current KafkaSecuritySettings class definition. You may need to update your class to include these properties if they're needed for SSL connections.
```

## 📜 Additional Notes

- SharpNest.Kafka automatically creates topics if they don't exist when subscribing
- The library handles proper disposal of resources to prevent memory leaks
- Thread safety is ensured through semaphore locks for shared resources
- Serialization is handled by the library, with default SystemTextJsonSerializer support
- Custom serializers can be registered if needed
