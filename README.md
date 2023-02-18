# Memento

[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)

Easy unidirectional store and red/undo library for state management for frontend apps on Blazor/.NET

# Basic Concept

You can subscribe to state change notifications, allowing state to be shared between components.
Undirectional flow and immutable change of state to provide a predictable architecture.

### For patterns like Flux or MVU

Besides a simple store pattern, we also provide patterns inspired by MVU patterns such as Flux and Elm.
Since we change the state through the Reducer, we can change the state based on stricter rules and observe the state in detail.

## DEMO Page

https://le-nn.github.io/memento/

## React or TS/JS bindings

Currently, moved to here
https://github.com/le-nn/memento-js

## Features

* Less boilarplate, less rule and simple usage 
* Immutable and Unidirectional data flow
* Multiple stores but manged by single provider, so can observe and manage as one state tree
* Observe detailed status with command patterns and makes it easier to monitor what happened within the application 

## Concepts and Data Flow

<img width="800px" src="./Architecture.jpg"/>

## Rules

* State should always be read-only.
* The UI then uses the new state to render its display.

### For patterns like Flux
* Every Reducer that processes in the action will create new state to reflect the old state combined with the changes expected for the action.
* To change state our app should Dispatch via Reducer in the action method

## Overview

This is an C# and Blazor example that implements counter.

Store 
```csharp
using Memento.Core;
using System.Collections.Immutable;
using static Memento.Sample.Blazor.Stores.AsyncCounterCommands;

namespace Memento.Sample.Blazor.Stores;

public record AsyncCounterState {
    public int Count { get; init; } = 0;

    public bool IsLoading { get; init; } = false;

    public ImmutableArray<int> Histories { get; init; } = ImmutableArray.Create<int>();
}

public record AsyncCounterCommands: Command {
    public record CountUp : AsyncCounterCommands;
    public record Increment : AsyncCounterCommands;
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
            Increment => state with {
                Count = state.Count + 1,
            },
            BeginLoading => state with {
                IsLoading = true,
            },
            _ => throw new CommandNotHandledException(command),
        };
    }

    public async Task CountUpAsync() {
        Dispatch(new BeginLoading());
        await Task.Delay(800);
        Dispatch(new CountUp());
    }

    public void CountUpManyTimes(int count) {
        for (int i = 0; i < count; i++) {
            Dispatch(new Increment());
        }
    }

    public void SetCount(int c) {
        Dispatch(new SetCount(c));
    }
}

```

Razor view
```razor
@using Memento.Sample.Blazor.Stores
@using System.Text.Json

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
<button class="mt-3 btn btn-primary" @onclick="IncrementCount">Count up</button>
<button class="mt-3 btn btn-primary" @onclick="CountupMany">Count up 10000 times</button>

@code {
    async Task IncrementCount() {
        await this.AsyncCounterStore.CountUpAsync();
    }

    void CountupMany() {
        this.AsyncCounterStore.CountUpManyTimes(10000);
    }
}

```

## Compatibility and bindings

| Package Name    | Version | Lang       | Platform            | Package manager | Release Notes                      | Package provider                                       |
| --------------- | ------- | ---------- | ------------------- | --------------- | ---------------------------------- | ------------------------------------------------------ |
| Memento.Core    | 1.0.0   | C#         | .NET 6 or later     | NuGet           | [Notes](./release-notes.dotnet.md) | [NuGet](https://www.nuget.org/packages/Memento.Core)   |
| Memento.Blazor  | 1.0.0   | Blazor     | .NET 6 or later     | NuGet           | [Notes](./release-notes.dotnet.md) | [NuGet](https://www.nuget.org/packages/Memento.Blazor) |

# Documentation

[Basic Concept with C#](./docs/Tutorial.cs.md)

[Blazor](./docs/Blazor/GettingStandard.md)

# License
Designed with ♥ by le-nn. Licensed under the MIT License.
