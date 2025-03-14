![NuGet Version](https://img.shields.io/nuget/v/SharpNest)

# Intro
This repository contains implementations of various tools or design patterns that simplify integration of .net projects with these technologies.

* [Fluent Decorator](#fluent-decorator)

### Installation

```bash
dotnet add package SharpNest
```

## Fluent Decorator

Lightweight and extensible tool that simplifies the implementation of the Decorator Pattern in .NET applications.
<br>

### Basic usage

```cs
services.AddDecorator();
```

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
> [!IMPORTANT]
> Documentation: [here](https://github.com/AnastasKosstow/sharp-nest/blob/main/docs/fluent-decorator.md)
>

<br/>

