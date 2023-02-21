using Memento.Core;
using System.Collections.Immutable;

namespace Memento.Test.Core.Mock;

// Define state to manage in store
public record AsyncCounterState {
    public int Count { get; init; } = 0;
    public ImmutableArray<int> History { get; init; } = ImmutableArray.Create<int>();
    public bool IsLoading { get; init; } = false;
}

public class AsyncCounterStore : Store<AsyncCounterState> {
    readonly Random _random = new(1234);

    public AsyncCounterStore() : base(() => new()) {
    }

    public void CountUp() {
        Mutate(state => state with {
            Count = state.Count + 1,
            History = state.History.Add(state.Count + 1),
        });
    }

    // "Dispatch" method can called outside of store via action (public method)
    // Action can be async method.
    public async Task CountUpAsync() {
        Mutate(state => state with { IsLoading = true });
        await Task.Delay(_random.Next(1, 50));
        Mutate(state => state with {
            Count = state.Count + 1,
            History = state.History.Add(state.Count + 1),
        });
        Mutate(state => state with { IsLoading = false });
    }

    public void SetCount(int num) {
        Mutate(state => state with {
            Count = num,
            History = state.History.Add(num),
        });
    }

    public async Task SetCountWithRandomTimerAsync(int num) {
        Mutate(state => state with { IsLoading = true });
        await Task.Delay(_random.Next(1, 50));
        Mutate(state => state with {
            Count = num,
            History = state.History.Add(num),
        });
        Mutate(state => state with { IsLoading = false });
    }
}
