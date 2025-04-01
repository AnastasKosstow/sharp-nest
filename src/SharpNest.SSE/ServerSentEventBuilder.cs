using Microsoft.Extensions.DependencyInjection;
using SharpNest.SSE.Core;
using SharpNest.SSE.Core.Abstractions;
using SharpNest.SSE.Core.Options;

namespace SharpNest.SSE;

public class ServerSentEventBuilder(IServiceCollection services) : IServerSentEventBuilder
{
    public IServiceCollection Services { get; } = services;

    public IServerSentEventBuilder Configure(Action<SSEOptions> configure)
    {
        Services.Configure(configure);
        return this;
    }

    public IServerSentEventBuilder WithSource<TSource>() where TSource : class, IMessageSource
    {
        Services.AddSingleton<IMessageSource, TSource>();
        Services.AddHostedService<SSEMessageBackgroundService>();
        return this;
    }

    public IServerSentEventBuilder WithSource<TSource>(Func<IServiceProvider, TSource> factory) where TSource : class, IMessageSource
    {
        Services.AddSingleton<IMessageSource>(factory);
        Services.AddHostedService<SSEMessageBackgroundService>();
        return this;
    }
}
