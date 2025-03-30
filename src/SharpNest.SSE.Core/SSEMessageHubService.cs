using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SharpNest.SSE.Core.Abstractions;
using SharpNest.SSE.Core.Options;
using System.Collections.Concurrent;
using System.Runtime.CompilerServices;
using System.Threading.Channels;

namespace SharpNest.SSE.Core;

public class SSEMessageHubService<TPayload>(ILogger<SSEMessageHubService<TPayload>> logger, IOptions<SSEOptions> options) : ISSEMessageHubService<TPayload>
{
    private readonly ConcurrentDictionary<Guid, Channel<IMessage<TPayload>>> _subscribers = new();
    private readonly ILogger<SSEMessageHubService<TPayload>> _logger = logger;
    private readonly SSEOptions _options = options.Value;

    public async IAsyncEnumerable<IMessage<TPayload>> SubscribeAsync([EnumeratorCancellation] CancellationToken cancellationToken)
    {
        var id = Guid.NewGuid();
        var channel = Channel.CreateBounded<IMessage<TPayload>>(new BoundedChannelOptions(_options.ChannelCapacity)
        {
            SingleWriter = false,
            SingleReader = true,
            FullMode = BoundedChannelFullMode.Wait
        });

        _subscribers.TryAdd(id, channel);

        try
        {
            cancellationToken.Register(() =>
            {
                _subscribers.TryRemove(id, out var removedChannel);
                removedChannel?.Writer.TryComplete();
            });

            await foreach (var message in channel.Reader.ReadAllAsync(cancellationToken))
            {
                yield return message;
            }
        }
        finally
        {
            _subscribers.TryRemove(id, out var removedChannel);
            removedChannel?.Writer.TryComplete();
        }
    }

    public async Task BroadcastMessageAsync(IMessage<TPayload> message)
    {
        if (_subscribers.IsEmpty)
        {
            return;
        }

        var tasks = _subscribers.Select(async kvp =>
        {
            var (id, channel) = kvp;
            try
            {
                var writeTask = channel.Writer.WriteAsync(message).AsTask();
                if (await Task.WhenAny(writeTask, Task.Delay(_options.WriteTimeout)) != writeTask)
                {
                    _logger.LogWarning("Timeout writing to subscriber {Id}. Message: Id={MessageId}",
                        id, message.Id);
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
}