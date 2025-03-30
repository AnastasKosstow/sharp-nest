using Microsoft.Extensions.DependencyInjection;
using SharpNest.SSE.Core.Abstractions;

namespace SharpNest.SSE;

public interface IServerSendEventSourceBuilder<TPayload>
{
    IServerSendEventSourceBuilder<TPayload> WithSource<TSource>() 
        where TSource : class, IMessageSource<TPayload>;
    IServerSendEventSourceBuilder<TPayload> WithSource<TSource>(Func<IServiceProvider, TSource> factory) 
        where TSource : class, IMessageSource<TPayload>;
}
