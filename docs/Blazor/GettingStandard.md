# Getting Standard

This tutorials is for [BlazorWeb](https://docs.microsoft.com/ja-jp/aspnet/core/blazor/?view=aspnetcore-6.0) or [MobileBlazorBindings](https://github.com/dotnet/MobileBlazorBindings).

# Install

### Blazor Project

Memento.Blazor includes helpers and DI support for use with Blazor.
It depends on Memento.Core.

```
dotnet add package Memento.Blazor
```

### .NET Project

Memento.Core is a core library that consists of pure .NET code only.
Install this for the .NET Class library or Console App.

```
dotnet add package Memento.Core
```

# Supported platform

.NET 7 or later

Currently requires following options in ```Project.csproj```

```xml
<PropertyGroup>
    <LangVersion>Preview</LangVersion>
    <EnablePreviewFeatures>True</EnablePreviewFeatures>
</PropertyGroup>
```

# Tutorial Overview

As a example, implement counters in various patterns.

## Usage

The standard usecase is as a flux like store container.
Memento is supported observe detailed with command pattern.
To change the state, you must dispatch a Message and go through Reducer.
So, you can observe state detailed.

[See](./Flux.md)

## Dependency Injection

[See](./DependencyInjection.md)

## API Refelence

Comming soon.

## DevTools

Comming soon.
