# Flux

The standard usecase is as a flux like store container.
This is supported observe detailed with command pattern.
To change the state, you must dispatch a Command and go through Reducer.
So, you can observe state detailed.

# Store Overview

Let's take a look at the completed store first.
Dig into step by step from there.

```cs
using Memento.Core;
using System.Collections.Immutable;

using static Memento.Sample.Blazor.Stores.AsyncCounterCommands;

namespace Memento.Sample.Blazor.Stores;

public record AsyncCounterState {
    public int Count { get; init; } = 0;

    public bool IsLoading { get; init; } = false;

    public int Next => this.Count + 1;

    public ImmutableArray<int> Histories { get; init; } = ImmutableArray.Create<int>();
}

public record AsyncCounterCommands : Command {
    public record CountUp : AsyncCounterCommands;
    public record SetCount(int Count) : AsyncCounterCommands;
    public record BeginLoading : AsyncCounterCommands;
}

public class AsyncCounterStore : Store<AsyncCounterState, AsyncCounterCommands> {
    public AsyncCounterStore() : base(() => new(), Reducer) { }

    static AsyncCounterState Reducer(AsyncCounterState state, AsyncCounterCommands command) {
        return command switch {
            CountUp => state with {
                Count = state.Count + 1,
                IsLoading = false,
                Histories = state.Histories.Add(state.Count + 1),
            },
            SetCount payload => state with {
                Count = payload.Count,
            },
            BeginLoading => state with {
                IsLoading = true,
            },
            _ => throw new Exception("The command is not handled."),
        };
    }

    public async Task CountUpAsync() {
        this.Dispatch(new BeginLoading());
        await Task.Delay(800);
        this.Dispatch(new CountUp());
    }

    public void SetCount(int c) {
        this.Dispatch(new SetCount(c));
    }
}

```

## State

You should define state to manage in store before create store.
T with ```Store<T, M>``` is the state type.
M with ```Store<T, M>``` is the command type.

The State define is following.

```cs
public record AsyncCounterState {
    public int Count { get; init; } = 0;

    public bool IsLoading { get; init; } = false;

    public int Next => this.Count + 1;

    public ImmutableArray<int> Histories { get; init; } = ImmutableArray.Create<int>();
}

```

The ```State<T>``` behavior is following. 

```cs
var state = new AsyncCounterState();
Console.WriteLine(state); // { count: 0, test: "foo" }

var state2 = state with {
    count = 5,
};
Console.WriteLine(state2); // { count: 5,test: "foo" }

var state3 = state2 with {
    count = state2.count * 3,
    test = "bar",
};
Console.WriteLine(state3) // { count: 15, test: "bar" }
```

The Command define is following.
Currently, there is no best way to express a Command in C # syntax. Therefore, we make use of record and polymorphism.

```cs
public record AsyncCounterCommands : Command {
    public record CountUp : AsyncCounterCommands;
    public record SetCount(int Count) : AsyncCounterCommands;
    public record BeginLoading : AsyncCounterCommands;
}
```

If the following proposal is realized in the future, it can be expressed as follows.
It has been designated as a milestone in the Working Set and will be realized in the near future.

https://github.com/dotnet/csharplang/issues/3179
https://github.com/dotnet/csharplang/issues/113

```cs
// Doesn't work in C# 11 

enum class Commands {
    CountUp,
    SetCount(int Count),
    BeginLoading,
}

return command switch {
    CountUp => ...,
    SetCount payload => ...,
    ...
};

this.Dispatch(CountUp);
this.Dispatch(SetCount(1234));
```

## Define Store

Create class and extends ```Store<T, M>``` and specify state initializer and Reducer to base constructor.
Specify state initializer into args of base constructor.
Specify Reducer into args of base constructor.

```cs
public class AsyncCounterStore : Store<AsyncCounterState, AsyncCounterCommands> {
    public AsyncCounterStore() : base(() => new(), Reducer) { }

    static AsyncCounterState Reducer(AsyncCounterState state, AsyncCounterCommands command) {
        ...
    }
}
```

### State changes

To muate the state, define a method as an action in the store.
Dispatch the state by calling it from the outside.
And generate new state via Reducer with handling command.

* Call ```this.Dispatch(T state)``` to update store state

The following example defines ```CountUpAsync```.

#### State change steps

* Loading state to true
* Run async timer
* Set the final state 

An method as action
```cs
public async Task CountUpAsync() {
    this.Dispatch(new BeginLoading());
    await Task.Delay(800);
    this.Dispatch(new CountUp());
}

```

The Reducer is following.
The Reducer should not include side effects such as asynchronous processing.
The Reducer  always be a pure function, so must be the static method in C#.
In the Reducer, describe the process to handle the command and generate a new state.

```cs
static AsyncCounterState Reducer(AsyncCounterState state, AsyncCounterCommands command) {
    return command switch {
        CountUp => state with {
            Count = state.Count + 1,
            IsLoading = false,
            Histories = state.Histories.Add(state.Count + 1),
        },
        SetCount payload => state with {
            Count = payload.Count,
        },
        BeginLoading => state with {
            IsLoading = true,
        },
        _ => throw new Exception("The command is not handled."),
    };
}
```

# With Blazor App

### Initialization

Blazor makes use of the framework Dependency Injection.
```Program.cs``` looks like following.
Don't forget to invoke ```UseStores```.
When you invoke ```UseStores```, the Memento initialization process will be performed.

```cs
using Memento.Blazor;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddMemento()
    .AddMiddleware<LoggerMiddleware>()
    .AddStore<AsyncCounterStore>()
    .AddStore<FetchDataStore>();

var app = builder.Build();
app.UseStores()
    .RunAsync();
```

### Component usage

If you use ```@inject``` in the Store, it will be solved automatically by the DI container.
Inherit ObserverComponet to observe the state of Store in View.
The injected store that extends ```Store<T, M>``` will be automatically observed by reflection.

This is an example of Counter.

```razor

@using Memento.Sample.Blazor.Stores

@page "/counter"
@inherits ObserverComponet
@inject AsyncCounterStore AsyncCounterStore

<PageTitle>Counter</PageTitle>

<h1>Async Counter</h1>

<p role="status">Current count: @AsyncCounterStore.State.Count</p>
<p role="status">Loading: @AsyncCounterStore.State.IsLoading</p>

<p role="status" class="mb-0">History</p>
<div class="d-flex">
    [
    @foreach (var item in string.Join(", ", AsyncCounterStore.State.Histories)) {
        @item
    }
    ]
</div>

<button class="mt-3 btn btn-primary" @onclick="IncrementCount">Click me</button>

@code {
    async Task IncrementCount() {
        await this.AsyncCounterStore.CountUpAsync();
    }
}

```

# API Refelences

[API Refelences](./API.md)
