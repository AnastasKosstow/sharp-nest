<img src="https://img.shields.io/badge/version-9.0-CC0000?style=for-the-badge&logo=.NET" 
     alt="NET badge" 
     width="170">

![NuGet Version](https://img.shields.io/nuget/v/SharpNest)
![Build](https://github.com/AnastasKosstow/sharp-nest/actions/workflows/build.yml/badge.svg)
![Tests](https://github.com/AnastasKosstow/sharp-nest/actions/workflows/tests.yml/badge.svg)




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

