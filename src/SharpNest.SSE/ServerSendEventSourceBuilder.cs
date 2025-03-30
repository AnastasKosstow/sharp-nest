using Microsoft.Extensions.DependencyInjection;
using SharpNest.SSE.Core.Abstractions;
using SharpNest.SSE.Core;

namespace SharpNest.SSE;

public class ServerSendEventSourceBuilder<TPayload>(IServiceCollection services) : IServerSendEventSourceBuilder<TPayload>
{
    public IServiceCollection Services { get; } = services;

    public IServerSendEventSourceBuilder<TPayload> WithSource<TSource>() where TSource : class, IMessageSource<TPayload>
    {
        Services.AddSingleton<IMessageSource<TPayload>, TSource>();
        Services.AddHostedService<SSEMessageBackgroundService<TPayload>>();
        return this;
    }

    public IServerSendEventSourceBuilder<TPayload> WithSource<TSource>(Func<IServiceProvider, TSource> factory) where TSource : class, IMessageSource<TPayload>
    {
        Services.AddSingleton<IMessageSource<TPayload>>(factory);
        Services.AddHostedService<SSEMessageBackgroundService<TPayload>>();
        return this;
    }
}
