using Microsoft.Extensions.Logging;

namespace SharpNest.Samples.Decorator.Services;

internal class WeatherServiceLoggingDecorator(IWeatherService service, ILogger<WeatherServiceLoggingDecorator> logger) : IWeatherService
{
    private readonly IWeatherService _service = service;
    private readonly ILogger<WeatherServiceLoggingDecorator> _logger = logger;

    public void Run()
    {
        _logger.LogInformation("----------Before service execution log.----------\n");
        _service.Run();
        _logger.LogInformation("----------After service execution log.----------\n");
    }
}
