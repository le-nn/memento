# Tutorial for C# with Console Application

Take a look at an example of a simple console application to experience the basic concept.

We provide a Store that allows you to share state between components. All stores are managed by a single provider and can subscribe to state change notifications. Unidirectional flow and immutable change of state provide a predictable architecture. In addition, we offer a store that easily implements Redo/Undo by managing immutable states.

There are two ways to define a Store class that manages state.
First, there is a simple store that only fires immutable state and state change events.
Besides the simple store pattern, we also provide patterns inspired by MVU patterns such as Flux and Elm. Since you should change the state via the Reducer, you can change the state based on stricter rules and observe the state in detail.

### Store class

* Provides a way to change state directly.
* Suitable when simpler state management is required.
* State management may be intuitive and easy to understand because the state is changed by directly applying reducer functions.

### FluxStore class

* Based on the Flux architecture, this class is suitable when more rigorous state management is required.
* State changes via commands, so actions and state changes are separated. This facilitates logging and debugging of state changes.
* It facilitates consistent state management in complex applications and team development.

### Rules

* State should always be read-only.
* The UI then uses the new state to render its display.

#### For patterns like Flux

* Every Reducer that processes in the action will create new state to reflect the old state combined with the changes expected for the action.
* To change state our app should Dispatch via Reducer in the action method

## Install

Please install via package manager.

```
dotnet add package Memento.Core
dotnet add package Microsoft.Extensions.DependencyInjection
```

Or install from Nuget

https://www.nuget.org/packages/Memento.Core

## Define Store pattern

In this section, you will learn the basic patterns of state management.
Try to initialize state in your application, subscribe to it, and call actions to output state changes.
Create a store for a simple counter application. This store, named AsyncCounterStore, will handle the count-up process and set-count process.

### Define the state

It is preferable to make the state immutable, for example, by using a C# function record.
This is to ensure consistency even if the state changes unexpectedly or is referenced from various locations.
It is also necessary to use ReduxDevTools or similar tools to perform time travel.

First, define the state that the store will manage: create a record named AsyncCounterState. This record contains properties that store the current count, history, and loading state.

```cs
public record AsyncCounterState {
    public int Count { get; init; } = 0;
    public ImmutableArray<int> History { get; init; } = [];
    public bool IsLoading { get; init; } = false;
}
```

### Define Store

Next, create a store class named AsyncCounterStore. This class inherits from Store<AsyncCounterState>.

```cs
public class AsyncCounterStore() : Store<AsyncCounterState>(() => new()) {

}
```
    
### Add actions to store to change store state
    
Implement the CountUpAsync method to perform asynchronous count-up processing. This method first sets the loading state to true, then waits for a certain period of time using Task.Delay. It then calls the HandleIncrement method to increment the count and finally sets the loading state to false.
And implement the SetCount method to allow the user to set the count with a specified number. This method takes the current state and returns a new state with a new count value and updated history.
    
```cs
    public async Task CountUpAsync() {
        Mutate(state => state with { IsLoading = true });

        await Task.Delay(500);

        Mutate(HandleIncrement);
        Mutate(state => state with { IsLoading = false });
    }

    private static AsyncCounterState HandleIncrement(AsyncCounterState state) {
        var count = state.Count + 1;
        return state with {
            Count = count,
            History = state.History.Add(count),
        };
    }

    public void SetCount(int num) {
        Mutate(state => state with {
            Count = num,
            History = state.History.Add(num),
        });
    }
    
```

### Overview
    
Completes the implementation of the AsyncCounterStore class. This store simplifies application state management.
Below is the code for the completed AsyncCounterStore class.

```cs
// Define state to manage in store
public record AsyncCounterState {
    public int Count { get; init; } = 0;
    public ImmutableArray<int> History { get; init; } = [];
    public bool IsLoading { get; init; } = false;
}

public class AsyncCounterStore() : Store<AsyncCounterState>(() => new()) {
    public async Task CountUpAsync() {
        Mutate(state => state with { IsLoading = true });

        await Task.Delay(500);

        Mutate(HandleIncrement);
        Mutate(state => state with { IsLoading = false });
    }

    private static AsyncCounterState HandleIncrement(AsyncCounterState state) {
        var count = state.Count + 1;
        return state with {
            Count = count,
            History = state.History.Add(count),
        };
    }

    public void SetCount(int num) {
        Mutate(state => state with {
            Count = num,
            History = state.History.Add(num),
        });
    }
}

```

### Includes the typed Message explaining what the change has been in StateHasChangedEventArgs 

```Store<TState, TMessage>``` allows you to have a typed message explaining what the change has been in StateHasChangedEventArgs when mutating the State of the Store.
The default message type is string if unspecified, such as in ```Store<TState>```.

## Usage

Define the message type.

```cs

public enum StateChangedType {
    BeginLoading,
    EndLoading,
    SetCount,
    Increment
}

```

The message type is specified in Store Type params in the following way.

```Store<AsyncCounterState, StateChangedType>```

If you set message as the second argument of ```Mutate(..., StateChangedType.Increment)```, you can get the message from the StateHasChangedEventArgs.

```cs

store.Subscribe(e => {
    Console.WriteLine(e.Command.Message.StateChangedType); // Specified Paylaod
});

```

### Sample CounterStore Overview

```cs

public record CounterStoreState {
    public int Count { get; init; } = 0;
    public ImmutableArray<int> History { get; init; } = [];
    public bool IsLoading { get; init; } = false;
}

public enum StateChangedType {
    BeginLoading,
    EndLoading,
    SetCount,
    Increment
}

public class CounterStore() : Store<CounterStoreState, StateChangedType>(() => new()) {

    public async Task CountUpAsync() {
        Mutate(state => state with { IsLoading = true }, StateChangedType.BeginLoading);

        await Task.Delay(500);

        Mutate(HandleIncrement, StateChangedType.Increment);
        Mutate(state => state with { IsLoading = false }, StateChangedType.EndLoading);
    }

    private static CounterStoreState HandleIncrement(CounterStoreState state) {
        var count = state.Count + 1;
        return state with {
            Count = count,
            History = state.History.Add(count),
        };
    }

    public void SetCount(int num) {
        Mutate(state => state with {
            Count = num,
            History = state.History.Add(num),
        }, StateChangedType.SetCount);
    }
}


```

#### Sample Source

https://github.com/le-nn/memento/blob/main/samples/Memento.Sample.Blazor/Stores/AsyncCounterStore.cs

---
    
## Define FluxStore pattern

In the previous section, we implemented AsyncCounterStore using the Store pattern. This time, let's implement the same features using the FluxStore pattern.

In this section, you will learn how to use the AsyncCounterStore to manage the state of your counters.
Skip this section if you do not use the FluxStore pattern.

FluxPattern is one of the architectural patterns for managing application data flow in React applications. This pattern uses unidirectional data flow to manage application state changes.
These elements are similar to the Model-View-Update (MVU) architectural pattern, which represents the application state as a model and uses it to display it in the View. State changes are managed by the Update function, which is then reflected in the View.

FluxStore is inspired by React's Flux and MVU patterns.
If you want to manage state with stricter rules and observe more detailed state change events, FluxStore is the right choice.
By mutating via Command, we have tighter control over all state changes.
Let's take a look at the tutorial.

### Define the state

First, define the state to be managed in the store. 
In this example, create a record named AsyncCounterState.

```cs
public record AsyncCounterState {
    public int Count { get; init; } = 0;
    public ImmutableArray<int> History { get; init; } = [];
    public bool IsLoading { get; init; } = false;
}
```

### Define commands

Define the commands used to change the state.
create an AsyncCounterCommand record and add subrecords to define each command.

```cs

public record AsyncCounterCommand : Command {
    public record Increment : AsyncCounterCommand;
    public record BeginLoading : AsyncCounterCommand;
    public record EndLoading : AsyncCounterCommand;
    public record ModifyCount(int Value) : AsyncCounterCommand;
}

```

### Create a store
Create an AsyncCounterStore class and extend FluxStore. In this class, define the state initialization and reducer functions.

```cs
public class AsyncCounterStore() : FluxStore<AsyncCounterState, AsyncCounterCommand>(() => new(), Reducer) {
}
```

### Denine Reducer

The Reducer uses the current state and command to create a new state; define a Reducer function in the AsyncCounterStore class.

```cs
    // State can change via Reducer and easy to observe state from command
    // Reducer generate new state from command and current state
    static AsyncCounterState Reducer(AsyncCounterState state, AsyncCounterCommand? command) {
        return command switch {
            AsyncCounterCommand.BeginLoading => state with {
                IsLoading = true
            },
            AsyncCounterCommand.EndLoading => state with {
                IsLoading = false
            },
            AsyncCounterCommand.Increment => HandleIncrement(state),
            AsyncCounterCommand.ModifyCount(var val) => state with {
                Count = val,
                History = state.History.Add(val),
            },
            _ => throw new CommandNotHandledException<AsyncCounterCommand>(command),
        };
    }
    
    static AsyncCounterState HandleIncrement(AsyncCounterState state) {
        var count = state.Count + 1;
        return state with {
            Count = count,
            History = state.History.Add(count),
        };
    }

```

## Add actions to store

As with the Store section add actions to change the state from outside the store.
In this example, two actions are defined, CountUpAsync and SetCount.
Since the creation of the new state is done by the Reducer, the command is dispatched to entrust it to the Reducer.
    
```cs

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

```

### Overview

```cs
// Define state to manage in store
public record AsyncCounterState {
    public int Count { get; init; } = 0;
    public ImmutableArray<int> History { get; init; } = ImmutableArray.Create<int>();
    public bool IsLoading { get; init; } = false;
}

// Define messages to change state and observe state change event in detail.
public record AsyncCounterCommand : Command {
    public record Increment : AsyncCounterCommand;
    public record BeginLoading : AsyncCounterCommand;
    public record EndLoading : AsyncCounterCommand;
    public record ModifyCount(int Value) : AsyncCounterCommand;
}

public class AsyncCounterStore() : FluxStore<AsyncCounterState, AsyncCounterCommand>(() => new(), Reducer) {
    // State can change via Reducer and easy to observe state from command
    // Reducer generate new state from command and current state
    static AsyncCounterState Reducer(AsyncCounterState state, AsyncCounterCommand command) {
        return command switch {
            BeginLoading => state with {
                IsLoading = true
            },
            EndLoading => state with {
                IsLoading = false
            },
            Increment => HandleIncrement(state),
            ModifyCount(var val) => state with {
                Count = val,
                History = state.History.Add(val),
            },
            _ => throw new CommandNotHandledException<AsyncCounterCommand>(command),
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
 
## Usage of Store

You seemed to create Store or FluxStore the usage is the same for both.

### StoreProvider

The ```StoreProvider``` provides centralized management of related Stores and a single StateTree representation of the related Stores' States. It also centrally manages Stores' state change notifications and events.

### Initializing Store and StoreProvider

StoreProvider initialization requires an IServiceProvider with the associated services and stores registered.

```cs
var services = new ServiceCollection();
services.AddScoped<AsyncCounterStore>();

var serviceProvider = new ServiceCollection()
    .AddScoped<AsyncCounterStore>()
    .BuildServiceProvider();

var provider = new StoreProvider(serviceProvider);

```

Capture the StateTree that consists from all aggregated store states
The StateTree is represented by a ```IDictionary<string, object>```.
The key is store name and the value is store state.

```cs

var rootState = provider.CaptureRootState();

```

rootState is following

```json

{
    "Store1" : {
       "Count1" : 1234,
       "Count2" : 4567,
    },

    // other stores ...

    "AsyncCounterStore" : {
        "Count": 0,
        "History": [],
        "IsLoading": false
    }
}

```

### Subscribing

A listener is registered for state change events so that processing can be performed each time the state changes. In this example, logs are output to the console each time the state changes.
The provider subscribes to all store state change events and outputs in JSON to the console.
The store subscribes to the store state change events and output in JSON to console.
Whenever a state change occurs, an event is emitted from the monitored store (all stores or a specific store) and information related to that event is printed to the console. This output includes event types, state changes, and is presented in formatted JSON using the JsonSerializer.


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
```
## Call actions

The application calls the store's actions to change the state. In this example, two actions are called, CountUpAsync and SetCount.

```cs
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

Now you know how to use AsyncCounterStore to manage application state. If the state becomes complex, consider creating multiple stores and splitting the state or sharing state between stores. These approaches make application state management more effective and efficient.

As an application grows and different features and components are added, state management can become complex. In such cases, state management can be facilitated by using multiple stores to partition state. Individual stores can be used to manage state related to specific features or components.

When sharing state among multiple stores or UI components, state can be shared using parent components or contexts. This ensures consistency of state and synchronizes state across the application.

Applying these methods makes application state management scalable and maintainable, improves code reusability, and simplifies development. Depending on the requirements of each application, choose the best state management approach.

## Next step

In the Next tutorial you will learn how to update the actual UI in Blazor !

[See](./Blazor.md)
