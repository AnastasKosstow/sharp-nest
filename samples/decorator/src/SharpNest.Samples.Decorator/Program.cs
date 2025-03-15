using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SharpNest.Samples.Decorator.Services;
using SharpNest.FluentDecorator;

var services = new ServiceCollection();
services.AddLogging(builder =>
{
    builder.AddConsole();
    builder.SetMinimumLevel(LogLevel.Information);
});

services.AddDecorator();
services.AddScopedService<IWeatherService>(
    configurator =>
    {
        configurator
            .AddDecorator<WeatherServiceLoggingDecorator>()
            .AddService<WeatherService>();
    });

var serviceProvider = services.BuildServiceProvider();

var service = serviceProvider.GetRequiredService<IWeatherService>();
service.Run();
