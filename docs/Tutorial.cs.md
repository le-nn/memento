# Tutorial for typescript on node.js or browser

Let's take a look at an example of a simple console application to experience the Basic Concept.

# Install

Prease install via nuget package manager.

```
dotnet add package Mement.Core
dotnet add package Microsoft.Extensions.DependencyInjection
```

```Microsoft.Extensions.DependencyInjection``` is an Dependency Inhjection Library.
Blazor is provided by ```Microsoft.Extensions.DependencyInjection``` as a standard helper with Dependency Injection,
 but it needs to be installed in the Console App project.

## Define store, state and messages.

```ts
import {
    meta,
    FluxStore,
    State,
    Message,
    createProvider
} from "memento.js"

const delay = (timeout: number) =>
    new Promise(resolve => setTimeout(resolve, timeout))

// Define state to manage in store
class FluxAsyncCounterState extends State<FluxAsyncCounterState> {
    count = 0
    history: number[] = []
    isLoading = false
}

// Define messages to mutate state and observe state change event in detail.
class Increment extends Message { }
class BeginLoading extends Message { }
class EndLoading extends Message { }
class ModifyCount extends Message<{ count: number }> { }

type FluxAsyncCounterMessages =
    Increment
    | BeginLoading
    | EndLoading
    | ModifyCount


// can be omitted this.
// if you want to omit, you should be following 
// "class FluxAsyncCounterStore extends FluxStore<FluxAsyncCounterState>"

/**
 * 
 */
@meta({ name: "FluxAsyncCounterStore" }) // Specify store name
export class FluxAsyncCounterStore extends FluxStore<FluxAsyncCounterState, FluxAsyncCounterMessages> {
    constructor() {
        super(new FluxAsyncCounterState(), FluxAsyncCounterStore.mutation)
    }

    // State can change via mutation and easy to observe state from message
    // Mutation generate new state from message and current state
    static mutation(
        state: FluxAsyncCounterState,
        message: FluxAsyncCounterMessages
        ) :FluxAsyncCounterState {
        switch (message.comparer) {
            case BeginLoading: {
                return state.clone({
                    isLoading: true
                })
            }
            case EndLoading: {
                return state.clone({
                    isLoading: false
                })
            }
            case Increment: {
                const count = state.count + 1
                return state.clone({
                    count,
                    history: [...state.history, count]
                })
            }
            case ModifyCount: {
                const { payload } = message as ModifyCount
                return state.clone({
                    count: payload.count,
                    history: [...state.history, payload.count]
                })
            }
            default: throw new Error("Message is not handled")
        }
    }

    // "mutate" method can called outside of store via action (pub lic method)
    // Action can be async method.
    async countUpAsync() {
        this.mutate(new BeginLoading())

        await delay(500)

        this.mutate(new Increment())
        this.mutate(new EndLoading())
    }

    setCount(num: number) {
        this.mutate(new ModifyCount({ count: num, }))
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
