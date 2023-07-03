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

### Initialization

Blazor uses Dependency Injection in the framework.

Here is how to use Memento in your application.

Registers all required modules with ```AddMemento()`` .
Registers all the stores present in the specified assembly with ```ScanAssemblyAndAddStores()``` .
Specifically, all classes that inherit from ````IStore``` will be registered.

registers an individual Store specified by ```AddStore<TStore>()``` .
Register the required services in ``Program.cs`` etc.
Add middleware with ```AddMiddleware(...)`` .
Add middleware if necessary.

If ```isScoped: false```, it will be registered as a Singleton, if ```isScoped: true```, it will be registered as a Scoped service.
In the case of Blazor Server, the state is separated for each user session, so the argument ```isScoped: true``` must be set or the state will be shared by all users.
The default args is ```isScoped: true```.

```cs
using Memento.Blazor;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services
    // Add necessary modules to Memento
    .AddMemento(isScoped: false)
     // Add middleware
    .AddMiddleware<LoggerMiddleware>(isScoped: false)
     // Add by specifying Store
    .AddStore<AsyncCounterStore>(isScoped: false)
    // Scan the assembly and register all classes that implement the IStore interface
    .ScanAssemblyAndAddStores(typeof(Program).Assembly, isScoped: false);

await builder.Build().RunAsync();
```

Next,  ```MementoInitializer``` to the component root, such as ``App.razor``.
This will initialize the necessary Memento modules.

```razor
<MementoInitializer />

<Router AppAssembly="@typeof(App).Assembly">
    <Found Context="routeData">
        <RouteView RouteData="@routeData" DefaultLayout="@typeof(MainLayout)" />
        <FocusOnNavigate RouteData="@routeData" Selector="h1" />
    </Found>
    <NotFound>
        <PageTitle>Not found</PageTitle>
        <LayoutView Layout="@typeof(MainLayout)">
            <p role="alert">Sorry, there's nothing at this address.</p>
        </LayoutView>
    </NotFound>
</Router>
```

### Component usage

Using ``@inject`` in Store will be automatically resolved by the DI container.
By inheriting from ObserverComponent.
Injected Stores that extend ``Store<TState>`` or ``FluxStore<TState, TCommand>`` are automatically observed by reflection.
This allows the View to automatically update the state of the Store automatically

This is an example of Counter.

```razor

@using Memento.Sample.Blazor.Stores
@using System.Text.Json

@page "/counter"
@inherits ObserverComponent
@inject AsyncCounterStore AsyncCounterStore

<PageTitle>Counter</PageTitle>

<div>
    <h1 class="mt-5">Async Counter</h1>
    <h2>Current count: @AsyncCounterStore.State.Count</h2>
    <p>Loading: @AsyncCounterStore.State.IsLoading</p>

    <div>
        <button class="mt-3 btn btn-primary" @onclick="IncrementCount">Count up</button>
        <button class="mt-3 btn btn-primary" @onclick="CountupMany">Count up 100 times</button>
    </div>

    <div class="mt-5">
        <h3>Count up async with histories</h3>
        <button class="mt-3 btn btn-primary" @onclick="IncrementCountAsync">Count up async</button>
        <p class="mt-3 mb-0">Histories</p>
        <div class="d-flex">
            @foreach (var item in string.Join(", ", AsyncCounterStore.State.Histories)) {
                @item
            }
        </div>
    </div>

    <div class="mt-5">
        <h3>Count up with Amount</h3>
        <input @bind-value="_amount" />
    </div>
    <button class="mt-3 btn btn-primary" @onclick="CountupWithAmount">Count up with amount</button>

    <div class="mt-5">
        <h3>Set count</h3>
        <input @bind-value="_countToSet" />
    </div>
    <button class="mt-3 btn btn-primary" @onclick="SetCount">Count up with amount</button>
</div>

@code {
    int _amount = 5;
    int _countToSet = 100;

    void IncrementCount() {
        AsyncCounterStore.CountUp();
    }

    async Task IncrementCountAsync() {
        await AsyncCounterStore.CountUpAsync();
    }

    void CountupMany() {
        AsyncCounterStore.CountUpManyTimes(100);
    }

    void CountupWithAmount() {
        AsyncCounterStore.CountUpWithAmount(_amount);
    }

    void SetCount() {
        AsyncCounterStore.SetCount(_countToSet);
    }
}

```
