using System.Collections.Immutable;
using System.Text.Json;
using Memento;
using Microsoft.Extensions.DependencyInjection;

using static AsyncCounterMessages;

var services = new ServiceCollection();
services.AddScoped<AsyncCounterStore>();

var serviceProvider = new ServiceCollection()
    .AddScoped<AsyncCounterStore>()
    .BuildServiceProvider();

var provider = new StoreProvider(serviceProvider);

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

// Define state to manage in store
public record AsyncCounterState {
    public int Count { get; init; } = 0;
    public ImmutableArray<int> History { get; init; } = ImmutableArray.Create<int>();
    public bool IsLoading { get; init; } = false;
}

// Define messages to mutate state and observe state change event in detail.
public record AsyncCounterMessages : Message {
    public record Increment : AsyncCounterMessages;
    public record BeginLoading : AsyncCounterMessages;
    public record EndLoading : AsyncCounterMessages;
    public record ModifyCount(int Value) : AsyncCounterMessages;
}

public class AsyncCounterStore : Store<AsyncCounterState, AsyncCounterMessages> {
    public AsyncCounterStore() : base(() => new(), Mutation) {
    }

    // State can change via mutation and easy to observe state from message
    // Mutation generate new state from message and current state
    static AsyncCounterState Mutation(AsyncCounterState state, AsyncCounterMessages message) {
        return message switch {
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

    // "Mutate" method can called outside of store via action (public method)
    // Action can be async method.
    public async Task CountUpAsync() {
        this.Mutate(new BeginLoading());

        await Task.Delay(500);

        this.Mutate(new Increment());
        this.Mutate(new EndLoading());
    }

    public void SetCount(int num) {
        this.Mutate(new ModifyCount(num));
    }
}
