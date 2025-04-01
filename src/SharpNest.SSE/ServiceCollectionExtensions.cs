using Microsoft.Extensions.DependencyInjection;
using SharpNest.SSE.Core;
using SharpNest.SSE.Core.Abstractions;
using SharpNest.SSE.Core.Options;

namespace SharpNest.SSE;

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds Server-Sent Event (SSE) services to the specified <see cref="IServiceCollection"/>.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> to add services to.</param>
    /// <returns>An <see cref="IServerSentEventBuilder"/> that can be used to further configure SSE services.</returns>
    public static IServerSentEventBuilder AddServerSentEvent(this IServiceCollection services)
    {
        services.AddSingleton<ISSEMessageHubService, SSEMessageHubService>();
        services.AddSingleton(new SSEOptions());

        return new ServerSentEventBuilder(services);
    }
}
