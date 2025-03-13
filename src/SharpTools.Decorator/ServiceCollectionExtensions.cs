using SharpTools.Decorator.DisposeRegister.DisposeScopes;
using SharpTools.Decorator.DisposeRegister.Scoped;
using SharpTools.Decorator.DisposeRegister.Singleton;
using Microsoft.Extensions.DependencyInjection;

namespace SharpTools.Decorator;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddDecorator(this IServiceCollection services)
    {
        return services
            .AddSingleton<ISingletonDisposableRegistry, SingletonDisposableRegistry>()
            .AddScoped<IScopedDisposableRegistry, ScopedDisposableRegistry>();
    }

    public static IServiceCollection AddSingletonService<TServiceInterface>(this IServiceCollection services, Action<IServiceDecoratorConfigurator<TServiceInterface>> configCallback)
        where TServiceInterface : class
    {
        return services.AddSingleton(serviceProvider => BuildDecoratedService(serviceProvider, configCallback, new LifetimeScope()));
    }

    public static IServiceCollection AddScopedService<TServiceInterface>(this IServiceCollection services, Action<IServiceDecoratorConfigurator<TServiceInterface>> configCallback)
        where TServiceInterface : class
    {
        return services.AddScoped(serviceProvider => BuildDecoratedService(serviceProvider, configCallback, new ShorterLivedScope()));
    }

    public static IServiceCollection AddTransientService<TServiceInterface>(this IServiceCollection services, Action<IServiceDecoratorConfigurator<TServiceInterface>> configCallback)
        where TServiceInterface : class
    {
        return services.AddTransient(serviceProvider => BuildDecoratedService(serviceProvider, configCallback, new ShorterLivedScope()));
    }

    private static TServiceInterface BuildDecoratedService<TServiceInterface>(IServiceProvider serviceProvider, Action<IServiceDecoratorConfigurator<TServiceInterface>> configCallback, IScope scope)
    {
        var builder = new DefaultDecoratorBuilder<TServiceInterface>(scope);
        configCallback(builder);

        return builder.Build(serviceProvider);
    }
}
