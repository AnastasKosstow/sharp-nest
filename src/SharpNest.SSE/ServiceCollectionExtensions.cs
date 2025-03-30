using Microsoft.Extensions.DependencyInjection;
using SharpNest.SSE.Core.Options;

namespace SharpNest.SSE;

public static class ServiceCollectionExtensions
{
    public static IServerSendEventBuilder AddServerSendEvent(this IServiceCollection services)
    {
        services.AddSingleton(new SSEOptions());
        return new ServerSendEventBuilder(services);
    }
}



public class Build
{
    IServiceCollection services;

    public void BuildSSE()
    {
        services.AddServerSendEvent()
            .ConfigureServerSendEvent(x =>
            {
                x.WriteTimeout = TimeSpan.FromDays(1);
                x.ChannelCapacity = 100;
            })
            .WithMessageType<AlertInfo>()
            .WithSource();
    }
}

//public class AlertInfo
//{

//}