using SharpNest.Decorator.DisposeRegister.Singleton;
using Microsoft.Extensions.DependencyInjection;

namespace SharpNest.Decorator.DisposeRegister.DisposeScopes;

public class ShorterLivedScope : IScope
{
    public IDisposableRegistry GetDisposableRegistry(IServiceProvider provider)
    {
        return provider.GetService<ISingletonDisposableRegistry>();
    }
}
