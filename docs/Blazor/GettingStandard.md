# Getting Standard

This tutorials is for [BlazorWeb](https://docs.microsoft.com/ja-jp/aspnet/core/blazor/?view=aspnetcore-6.0) or [MobileBlazorBindings](https://github.com/dotnet/MobileBlazorBindings).

# Install

Blazor Project
```
dotnet add package Memento.Blazor
```

.NET Project
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
Memento is supported observe detailed with message pattern.
To mutate the state, you must dispatch a Message and go through Mutation.
So, you can observe state detailed.

[See](./Flux.md)

## Dependency Injection

[See](./DependencyInjection.md)

## API Refelence

Comming soon.

## DevTools

Comming soon.