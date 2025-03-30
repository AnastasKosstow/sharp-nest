using Microsoft.Extensions.DependencyInjection;
using SharpNest.SSE.Core.Options;

namespace SharpNest.SSE;

public interface IServerSendEventBuilder
{
    IServerSendEventSourceBuilder<TPayload> WithMessageType<TPayload>();
    IServerSendEventBuilder ConfigureServerSendEvent(Action<SSEOptions> configure);
}
