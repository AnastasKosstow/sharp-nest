using SharpNest.Kafka.Core.Abstractions;

namespace SharpNest.Samples.Kafka;

public class KafkaConsumerService(ISubscriber subscriber, ILogger<KafkaConsumerService> logger) : BackgroundService
{
    private readonly ISubscriber _subscriber = subscriber;
    private readonly ILogger<KafkaConsumerService> _logger = logger;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            await _subscriber.SubscribeAsync(
                "messages-topic",
                async message =>
                {
                    _logger.LogInformation(
                        "Message received: Topic={Topic}, Key={Key}, Value={Value}",
                        message.Topic,
                        message.Key,
                        message.Value);

                    await Task.CompletedTask;
                },
                "consumer-group-1",
                stoppingToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in Kafka consumer service");
        }
    }
}