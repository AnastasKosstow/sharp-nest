using SharpTools.Decorator.DisposeRegister.Scoped;
using Microsoft.Extensions.DependencyInjection;

namespace SharpTools.Decorator.DisposeRegister.DisposeScopes;

public class LifetimeScope : IScope
{
    public IDisposableRegistry GetDisposableRegistry(IServiceProvider provider)
    {
        return provider.GetService<IScopedDisposableRegistry>();
    }
}
