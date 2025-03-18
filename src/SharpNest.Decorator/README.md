
Lightweight and extensible tool that simplifies the implementation of the Decorator Pattern in .NET applications.
It provides a fluent API for registering services with multiple decorators in Dependency Injection (DI) container while ensuring proper lifetime management and correct disposal of decorated services.

### Basic usage

```cs
using SharpNest.Decorator;

...
services.AddDecorator();
```

```cs
services.AddSingletonService<IWeatherService>(
    configurator =>
    {
        configurator
            .AddDecorator<WeatherServiceLoggingDecorator>()
            .AddService<WeatherService>();
    });
```

> #### Documentation: [here](https://github.com/AnastasKosstow/sharp-nest/blob/main/docs/fluent-decorator.md)
> #### Sample app: [here](https://github.com/AnastasKosstow/sharp-nest/tree/main/samples/decorator/src/SharpNest.Samples.Decorator)