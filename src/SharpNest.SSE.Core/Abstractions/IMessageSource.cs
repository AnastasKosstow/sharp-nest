namespace SharpNest.SSE.Core.Abstractions;

/// <summary>
/// 
/// </summary>
public interface IMessageSource<TPayload>
{
    Task StartAsync(Func<IMessage<TPayload>, Task> messageHandler, CancellationToken cancellationToken);

    Task StopAsync(CancellationToken cancellationToken);
}
