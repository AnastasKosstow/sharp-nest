using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SharpNest.SSE.Core.Abstractions;

namespace SharpNest.SSE.Core;

public class SSEMessageBackgroundService(IMessageSource messageSource, ISSEMessageHubService messageHub, ILogger<SSEMessageBackgroundService> logger) 
    : IHostedService, IDisposable
{
    private readonly IMessageSource _messageSource = messageSource;
    private readonly ISSEMessageHubService _messageHub = messageHub;
    private readonly ILogger<SSEMessageBackgroundService> _logger = logger;

    private Task _executingTask;
    private CancellationTokenSource _cancellationTokenSource;

    private int _processedMessageCount = 0;
    private int _errorCount = 0;
    private bool _disposed;

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _cancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        _executingTask = Task.Run(() => ExecuteAsync(_cancellationTokenSource.Token), _cancellationTokenSource.Token);
        return Task.CompletedTask;
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        if (_executingTask == null)
        {
            return;
        }

        try
        {
            _cancellationTokenSource?.Cancel();
        }
        finally
        {
            await Task.WhenAny(_executingTask, Task.Delay(TimeSpan.FromSeconds(5), cancellationToken));
        }
    }

    private async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        try
        {
            await _messageSource.StartAsync(HandleMessageAsync, _cancellationTokenSource.Token);
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("Subscription service was cancelled. Messages processed: {Count}, Errors: {ErrorCount}",
                _processedMessageCount, _errorCount);
        }
        catch (Exception ex)
        {
            _logger.LogCritical(ex, "Fatal error in subscription service after processing {Count} messages.",
                _processedMessageCount);
            throw;
        }
    }

    private async Task HandleMessageAsync(IMessage message)
    {
        try
        {
            await _messageHub.BroadcastMessageAsync(message);

            Interlocked.Increment(ref _processedMessageCount);
        }
        catch (Exception ex)
        {
            Interlocked.Increment(ref _errorCount);

            _logger.LogError(ex, "Error broadcasting message: {MessageId}", message.Id);
            throw;
        }
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _cancellationTokenSource?.Cancel();
        _cancellationTokenSource?.Dispose();

        _disposed = true;
    }
}
