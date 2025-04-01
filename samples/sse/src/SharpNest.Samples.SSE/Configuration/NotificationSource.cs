using SharpNest.SSE.Core.Abstractions;

namespace SharpNest.Samples.SSE.Configuration;

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

