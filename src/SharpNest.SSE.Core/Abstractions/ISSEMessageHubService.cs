namespace SharpNest.SSE.Core.Abstractions;

public interface ISSEMessageHubService<TPayload>
{
    IAsyncEnumerable<IMessage<TPayload>> SubscribeAsync(CancellationToken cancellationToken);
    Task BroadcastMessageAsync(IMessage<TPayload> message);
}
