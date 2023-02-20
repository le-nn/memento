using System.Collections.Immutable;

namespace Memento.Sample.Blazor.Stores;

public record AsyncCounterState {
    public int Count { get; init; } = 0;

    public bool IsLoading { get; init; } = false;

    public ImmutableArray<int> Histories { get; init; } = ImmutableArray.Create<int>();
}

public class AsyncCounterStore : Store<AsyncCounterState> {
    public AsyncCounterStore() : base(() => new()) { }

    public void CountUp() {
        Mutate(state => state with {
            Count = state.Count + 1,
            IsLoading = false,
            Histories = state.Histories.Add(state.Count + 1),
        });
    }

    public async Task CountUpAsync() {
        Mutate(state => state with { IsLoading = true, });
        await Task.Delay(800);
        Mutate(state => state with {
            Count = state.Count + 1,
            IsLoading = false,
            Histories = state.Histories.Add(state.Count + 1),
        });
    }

    public void CountUpManyTimes(int count) {
        for (var i = 0; i < count; i++) {
            Mutate(state => state with {
                Count = state.Count + 1,
            });
        }
    }

    public void SetCount(int count) {
        Mutate(state => state with {
            Count = count,
        });
    }

    public void CountUpWithAmount(int amount) {
        Mutate(state => state with {
            Count = state.Count + amount,
        });
    }
}