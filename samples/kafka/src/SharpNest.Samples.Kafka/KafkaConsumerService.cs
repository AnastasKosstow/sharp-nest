using SharpNest.Kafka.Core.Abstractions;

namespace SharpNest.Samples.Kafka;

public class KafkaConsumerService(ISubscriber subscriber, ILogger<KafkaConsumerService> logger) : IHostedService
{
    private readonly ISubscriber _subscriber = subscriber;
    private readonly ILogger<KafkaConsumerService> _logger = logger;

    private Task _executingTask;
    private CancellationTokenSource _cancellationTokenSource;

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _cancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        _executingTask = Task.Run(() => ExecuteAsync(_cancellationTokenSource.Token), _cancellationTokenSource.Token);
        return Task.CompletedTask;
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        if (_executingTask == null)
        {
            return;
        }

        try
        {
            _cancellationTokenSource?.Cancel();
        }
        finally
        {
            await Task.WhenAny(_executingTask, Task.Delay(TimeSpan.FromSeconds(5), cancellationToken));
        }
    }

    private async Task ExecuteAsync(CancellationToken cancellationToken)
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
                cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in Kafka consumer service");
        }
    }
}
