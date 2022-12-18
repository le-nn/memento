using Memento.Core;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Immutable;
using System.Text.Json;
using static AsyncCounterCommands;

var services = new ServiceCollection();
services.AddScoped<AsyncCounterStore>();

var serviceProvider = new ServiceCollection()
    .AddScoped<AsyncCounterStore>()
    .BuildServiceProvider();

var provider = new StoreProvider(serviceProvider);

// Observe all stores state
provider.Subscribe(e => {
    Console.WriteLine();
    Console.WriteLine($"// {e.StateChangedEvent.Command?.GetType().Name}");
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
    Console.WriteLine($"// {e.Command.GetType().Name}");
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

// Define state to manage in store
public record AsyncCounterState {
    public int Count { get; init; } = 0;
    public ImmutableArray<int> History { get; init; } = ImmutableArray.Create<int>();
    public bool IsLoading { get; init; } = false;
}

// Define command to change state and observe state change event in detail.
public record AsyncCounterCommands : Command {
    public record Increment : AsyncCounterCommands;
    public record BeginLoading : AsyncCounterCommands;
    public record EndLoading : AsyncCounterCommands;
    public record ModifyCount(int Value) : AsyncCounterCommands;
}

public class AsyncCounterStore : Store<AsyncCounterState, AsyncCounterCommands> {
    public AsyncCounterStore() : base(() => new(), Reducer) {
    }

    // State can change via Reducer and easy to observe state from command
    // Reducer generate new state from command and current state
    static AsyncCounterState Reducer(AsyncCounterState state, AsyncCounterCommands command) {
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
            _ => throw new CommandNotHandledException(command),
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
        Dispatch(new BeginLoading());

        await Task.Delay(500);

        Dispatch(new Increment());
        Dispatch(new EndLoading());
    }

    public void SetCount(int num) {
        Dispatch(new ModifyCount(num));
    }
}