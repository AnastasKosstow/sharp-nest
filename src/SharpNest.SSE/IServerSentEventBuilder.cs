using SharpNest.SSE.Core.Abstractions;
using SharpNest.SSE.Core.Options;

namespace SharpNest.SSE;

/// <summary>
/// Provides methods to configure Server-Sent Event (SSE) services.
/// </summary>
public interface IServerSentEventBuilder
{
    /// <summary>
    /// Configures the SSE options.
    /// </summary>
    /// <param name="configure">An action to configure the SSE options.</param>
    /// <returns>The same builder instance for chaining.</returns>
    IServerSentEventBuilder Configure(Action<SSEOptions> configure);

    /// <summary>
    /// Adds a message source of the specified type to the SSE service.
    /// </summary>
    /// <typeparam name="TSource">The type of the message source to add.</typeparam>
    /// <returns>The same builder instance for chaining.</returns>
    /// <remarks>
    /// The source will be registered as a singleton service in the dependency injection container.
    /// </remarks>
    IServerSentEventBuilder WithSource<TSource>() where TSource : class, IMessageSource;

    /// <summary>
    /// Adds a message source created by the specified factory to the SSE service.
    /// </summary>
    /// <typeparam name="TSource">The type of the message source to add.</typeparam>
    /// <param name="factory">A factory function that creates the message source instance.</param>
    /// <returns>The same builder instance for chaining.</returns>
    /// <remarks>
    /// This method allows for custom initialization of the message source using the service provider.
    /// The source will be registered as a singleton service in the dependency injection container.
    /// </remarks>
    IServerSentEventBuilder WithSource<TSource>(Func<IServiceProvider, TSource> factory) where TSource : class, IMessageSource;
}
