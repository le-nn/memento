# Getting Standard

In this tutorial, you will learn how to update the actual UI in Blazor !

Install `Memento.Blazor` in the Blazor component to update the Store state.

`Memento.Blazor` contains components that allow UI components to know about state updates and automatically update the UI.
`Memento.Core` is included in the dependencies and does not need to be installed additionally.
`Memento.Core` is a core library consisting of pure .NET code only.
NET class library or for console apps that do not use a UI framework such as Blazor, install `Memento.Core`.

Depends on Microsoft.AspNetCore.Components.
It can also be used with native UI frameworks such as MobileBlazorBindings and BlazorBindings.Maui because it does not depend on Microsoft.AspNetCore.Components.Web.

Blazor

[https://docs.microsoft.com/ja-jp/aspnet/core/blazor/?view=aspnetcore-6.0](https://dotnet.microsoft.com/ja-jp/apps/aspnet/web-apps/blazor)

MobileBlazorBindings

https://github.com/dotnet/MobileBlazorBindings

BlazorBindings.Maui

https://github.com/Dreamescaper/BlazorBindings.Maui

## Install

Install `Memento.Blazor` for the Blazor projects.
Install `Memento.Core` for the pure NET project that do not use the UI framework like .NET Class library or Console App.

### Install with CLI

Memento.Blazor

```
dotnet add package Memento.Blazor
```

Memento.Core

```
dotnet add package Memento.Core
```

### Install from Nuget

Memento.Blazor Nuget

https://www.nuget.org/packages/Memento.Blazor

Memento.Core Nuget

https://www.nuget.org/packages/Memento.Core

