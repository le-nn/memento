# Tutorial for C# with Console Application

Take a look at an example of a simple console application to experience the Basic Concept.

# Install

Prease install via nuget package manager.

```
dotnet add package Mement.Core
dotnet add package Microsoft.Extensions.DependencyInjection
```

## Define store, state and messages.

```cs
using System.Collections.Immutable;
using System.Text.Json;
using Memento.Core;
using Microsoft.Extensions.DependencyInjection;

using static AsyncCounterMessages;

// Define state to manage in store
public record AsyncCounterState {
    public int Count { get; init; } = 0;
    public ImmutableArray<int> History { get; init; } = ImmutableArray.Create<int>();
    public bool IsLoading { get; init; } = false;
}

// Define messages to change state and observe state change event in detail.
public record AsyncCounterMessages : Message {
    public record Increment : AsyncCounterMessages;
    public record BeginLoading : AsyncCounterMessages;
    public record EndLoading : AsyncCounterMessages;
    public record ModifyCount(int Value) : AsyncCounterMessages;
}

public class AsyncCounterStore : Store<AsyncCounterState, AsyncCounterMessages> {
    public AsyncCounterStore() : base(() => new(), Reducer) {
    }

    // State can change via Reducer and easy to observe state from command
    // Reducer generate new state from command and current state
    static AsyncCounterState Reducer(AsyncCounterState state, AsyncCounterMessages command) {
        return command switch {
            BeginLoading => state with {
                IsLoading = true
            },
            EndLoading => state with {
                IsLoading = false
            },
            Increment => HandleIncrement(state),
            ModifyCount payload => state with {
                Count = payload.Value,
                History = state.History.Add(payload.Value),
            },
            _ => throw new Exception("Message is not handled"),
        };
    }

    static AsyncCounterState HandleIncrement(AsyncCounterState state) {
        var count = state.Count + 1;
        return state with {
            Count = count,
            History = state.History.Add(count),
        };
    }

    // "Dispatch" method can called outside of store via action (public method)
    // Action can be async method.
    public async Task CountUpAsync() {
        this.Dispatch(new BeginLoading());

        await Task.Delay(500);

        this.Dispatch(new Increment());
        this.Dispatch(new EndLoading());
    }

    public void SetCount(int num) {
        this.Dispatch(new ModifyCount(num));
    }
}

```

### Usage

Register and initialize.

```cs
var services = new ServiceCollection();
services.AddScoped<AsyncCounterStore>();

var serviceProvider = new ServiceCollection()
    .AddScoped<AsyncCounterStore>()
    .BuildServiceProvider();

var provider = new StoreProvider(serviceProvider);
```

Process.

```cs
// Observe all stores state
provider.Subscribe(e => {
    Console.WriteLine();
    Console.WriteLine($"// {e.StateChangedEvent.Message?.GetType().Name}");
    Console.WriteLine(JsonSerializer.Serialize(
        e.StateChangedEvent.State,
        new JsonSerializerOptions() {
            WriteIndented = true
        })
    );
});

var store = provider.ResolveStore<AsyncCounterStore>();

// Observe a store state
store.Subscribe(e => {
    Console.WriteLine();
    Console.WriteLine($"// {e.Message.GetType().Name}");
    Console.WriteLine(JsonSerializer.Serialize(
        e.State,
        new JsonSerializerOptions() {
            WriteIndented = true
        })
    );
});

Console.WriteLine("// Initial state");
Console.WriteLine(JsonSerializer.Serialize(
    store.State,
    new JsonSerializerOptions() {
        WriteIndented = true
    })
);

// Call action and countup async.
await store.CountUpAsync();
// Call action and set count.
store.SetCount(5);
```

```store.Subscribe``` can be exptected output following

```json
// Initial state
{
  "Count": 0,
  "History": [],
  "IsLoading": false
}

// BeginLoading
{
  "Count": 0,
  "History": [],
  "IsLoading": true
}

// 500ms later
// Increment
{
  "Count": 1,
  "History": [
    1
  ],
  "IsLoading": true
}

// EndLoading
{
  "Count": 1,
  "History": [
    1
  ],
  "IsLoading": false
}

// ModifyCount
{
  "Count": 5,
  "History": [
    1,
    5
  ],
  "IsLoading": false
}
```
