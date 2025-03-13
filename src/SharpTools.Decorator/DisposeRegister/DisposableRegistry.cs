namespace SharpTools.Decorator.DisposeRegister;

public class DisposableRegistry : IDisposableRegistry, IDisposable
{
    private readonly List<IDisposable> _disposables = [];
    private readonly object _lock = new();

    public void Register(IDisposable disposable)
    {
        lock (_lock)
        {
            _disposables.Add(disposable);
        }
    }

    public void Dispose()
    {
        lock (_lock)
        {
            foreach (var disposable in _disposables)
            {
                try
                {
                    disposable.Dispose();
                }
                catch (Exception)
                {
                    // Log exception but continue disposing other resources
                }
            }
            _disposables.Clear();
        }
    }
}
