using SharpTools.Decorator.DisposeRegister.Singleton;
using Microsoft.Extensions.DependencyInjection;

namespace SharpTools.Decorator.DisposeRegister.DisposeScopes;

public class ShorterLivedScope : IScope
{
    public IDisposableRegistry GetDisposableRegistry(IServiceProvider provider)
    {
        return provider.GetService<ISingletonDisposableRegistry>();
    }
}
