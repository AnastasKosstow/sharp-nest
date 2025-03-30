using Microsoft.Extensions.DependencyInjection;
using SharpNest.SSE.Core;
using SharpNest.SSE.Core.Abstractions;
using SharpNest.SSE.Core.Options;

namespace SharpNest.SSE;

public class ServerSendEventBuilder(IServiceCollection services) : IServerSendEventBuilder
{
    public IServiceCollection Services { get; } = services;

    public IServerSendEventSourceBuilder<TPayload> WithMessageType<TPayload>()
    {
        Services.AddSingleton<ISSEMessageHubService<TPayload>, SSEMessageHubService<TPayload>>();
        return new ServerSendEventSourceBuilder<TPayload>(Services);
    }

    public IServerSendEventBuilder ConfigureServerSendEvent(Action<SSEOptions> configure)
    {
        Services.Configure(configure);
        return this;
    }
}
