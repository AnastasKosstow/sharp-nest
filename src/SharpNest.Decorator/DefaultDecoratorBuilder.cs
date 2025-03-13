using SharpNest.Decorator.DisposeRegister.DisposeScopes;
using Microsoft.Extensions.DependencyInjection;

namespace SharpNest.Decorator;

public class DefaultDecoratorBuilder<TServiceInterface>(IScope scope) : IServiceDecoratorConfigurator<TServiceInterface>, IServiceDecoratorBuilder<TServiceInterface>
{
    private readonly SemaphoreSlim _buildLock = new(1, 1);
    private readonly List<Type> _decorators = [];
    private Type? _serviceType;

    private readonly IScope _scope = scope;

    public IServiceDecoratorConfigurator<TServiceInterface> AddDecorator<TDecorator>() where TDecorator : TServiceInterface
    {
        _decorators.Add(typeof(TDecorator));
        return this;
    }

    public void AddService<TService>() where TService : TServiceInterface
    {
        _serviceType = typeof(TService);
    }

    public TServiceInterface Build(IServiceProvider provider)
    {
        try
        {
            _buildLock.Wait();

            var disposableRegistry = _scope.GetDisposableRegistry(provider);

            if (_serviceType == null)
            {
                throw new InvalidOperationException($"Service type for interface {typeof(TServiceInterface).Name} is not configured!");
            }

            var service = ActivatorUtilities.CreateInstance(provider, _serviceType);
            if (service is IDisposable disposable && _decorators.Count > 0)
            {
                disposableRegistry.Register(disposable);
            }

            if (_decorators.Count > 0)
            {
                for (var idx = _decorators.Count - 1; idx >= 0; idx--)
                {
                    var decorator = _decorators[idx];

                    service = (TServiceInterface)ActivatorUtilities.CreateInstance(provider, decorator, service);
                    if (service is IDisposable disposableInner && idx != 0)
                    {
                        disposableRegistry.Register(disposableInner);
                    }
                }
            }

            return (TServiceInterface)service;
        }
        finally
        {
            _buildLock.Release();
        }
    }
}
