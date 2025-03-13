namespace SharpTools.Decorator.DisposeRegister.DisposeScopes;

public interface IScope
{
    IDisposableRegistry GetDisposableRegistry(IServiceProvider provider);
}
