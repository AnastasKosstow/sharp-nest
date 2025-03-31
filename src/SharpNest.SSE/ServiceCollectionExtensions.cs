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
