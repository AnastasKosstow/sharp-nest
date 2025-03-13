namespace SharpNest.Decorator;

public interface IServiceDecoratorBuilder<out TServiceInterface>
{
    TServiceInterface Build(IServiceProvider serviceProvider);
}
