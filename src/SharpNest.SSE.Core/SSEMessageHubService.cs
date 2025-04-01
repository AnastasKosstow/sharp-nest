using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SharpNest.SSE.Core.Abstractions;
using SharpNest.SSE.Core.Options;
using System.Collections.Concurrent;
using System.Runtime.CompilerServices;
using System.Threading.Channels;

namespace SharpNest.SSE.Core;

public class SSEMessageHubService(ILogger<SSEMessageHubService> logger, IOptions<SSEOptions> options) : ISSEMessageHubService
{
    private readonly ILogger<SSEMessageHubService> _logger = logger;
    private readonly SSEOptions _options = options.Value;

    private readonly ConcurrentDictionary<Guid, SubscriberInfo> _subscribers = new();

    public async IAsyncEnumerable<IMessage> SubscribeAsync([EnumeratorCancellation] CancellationToken cancellationToken)
    {
        var id = Guid.NewGuid();
        var channel = Channel.CreateBounded<IMessage>(new BoundedChannelOptions(_options.ChannelCapacity)
        {
            SingleWriter = false,
            SingleReader = true,
            FullMode = BoundedChannelFullMode.Wait
        });

        var subscriberInfo = new SubscriberInfo
        {
            Channel = channel,
            LastMessageTime = DateTime.UtcNow,
            FailedDeliveries = 0
        };

        _subscribers.TryAdd(id, subscriberInfo);

        try
        {
            cancellationToken.Register(() =>
            {
                _subscribers.TryRemove(id, out var removed);
                removed?.Channel.Writer.TryComplete();
            });

            await foreach (var message in channel.Reader.ReadAllAsync(cancellationToken))
            {
                yield return message;
            }
        }
        finally
        {
            _subscribers.TryRemove(id, out var removedChannel);
            removedChannel?.Channel.Writer.TryComplete();
        }
    }

    public async Task BroadcastMessageAsync(IMessage message)
    {
        if (_subscribers.IsEmpty)
        {
            return;
        }

        var tasks = _subscribers.Select(async kvp =>
        {
            var (id, info) = kvp;

            try
            {
                switch (_options.SlowConsumerStrategy)
                {
                    case SlowConsumerStrategy.Wait:
                        await HandleWaitStrategyAsync(info, message);
                        break;

                    case SlowConsumerStrategy.DropMessages:
                        await HandleDropMessagesStrategyAsync(info, message);
                        break;

                    case SlowConsumerStrategy.DisconnectSubscriber:
                        await HandleDisconnectSubscriberStrategyAsync(id, info, message);
                        break;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error writing to subscriber {Id}. Message: Id={MessageId}, Error={ErrorMessage}",
                    id, message.Id, ex.Message);
            }
        });

        await Task.WhenAll(tasks);
    }

    private async Task HandleWaitStrategyAsync(SubscriberInfo subscriberInfo, IMessage message)
    {
        await subscriberInfo.Channel.Writer.WriteAsync(message);
        subscriberInfo.FailedDeliveries = 0;
    }

    private async Task HandleDropMessagesStrategyAsync(SubscriberInfo subscriberInfo, IMessage message)
    {
        var writeTask = subscriberInfo.Channel.Writer.WriteAsync(message).AsTask();

        if (await Task.WhenAny(writeTask, Task.Delay(_options.WriteTimeout)) != writeTask)
        {
            subscriberInfo.FailedDeliveries++;
        }
        else
        {
            subscriberInfo.FailedDeliveries = 0;
        }
    }

    private async Task HandleDisconnectSubscriberStrategyAsync(Guid subscriberId, SubscriberInfo subscriberInfo, IMessage message)
    {
        var writeTask = subscriberInfo.Channel.Writer.WriteAsync(message).AsTask();

        if (await Task.WhenAny(writeTask, Task.Delay(_options.WriteTimeout)) != writeTask)
        {
            subscriberInfo.FailedDeliveries++;

            if (subscriberInfo.FailedDeliveries >= 3)
            {
                if (_subscribers.TryRemove(subscriberId, out var removedInfo))
                {
                    removedInfo.Channel.Writer.TryComplete();
                }
            }
        }
        else
        {
            subscriberInfo.FailedDeliveries = 0;
        }
    }

    private class SubscriberInfo
    {
        public Channel<IMessage> Channel { get; set; }
        public DateTime LastMessageTime { get; set; }
        public int FailedDeliveries { get; set; }
    }
}
