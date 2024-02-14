using System.Collections.Immutable;

namespace Memento.Sample.Blazor.Stores;



public record AsyncCounterState {
    public int Count { get; init; } = 0;

    public bool IsLoading { get; init; } = false;

    public ImmutableArray<int> Histories { get; init; } = [];
}

public enum StateChangedType {
    CountUp,
    Loading,
    CountUpAsync,
    SetCount,
    CountUpWithAmount
}

public record Message(StateChangedType StateChangedType);

public class AsyncCounterStore : Store<AsyncCounterState, Message> {
    public AsyncCounterStore() : base(() => new()) {
    }

    public void CountUp() {
        Mutate(state => state with {
            Count = state.Count + 1,
            IsLoading = false,
            Histories = [.. state.Histories, state.Count + 1],
        }, new(StateChangedType.CountUp));
    }

    public async Task CountUpAsync() {
        Mutate(state => state with { IsLoading = true, }, new(StateChangedType.Loading));
        await Task.Delay(800);
        Mutate(state => state with {
            Count = state.Count + 1,
            IsLoading = false,
            Histories = [.. state.Histories, state.Count + 1],
        }, new(StateChangedType.CountUp));
    }

    public void CountUpManyTimes(int count) {
        for (var i = 0; i < count; i++) {
            Mutate(state => state with {
                Count = state.Count + 1,
            }, new(StateChangedType.CountUpAsync));
        }
    }

    public void SetCount(int count) {
        Mutate(state => state with {
            Count = count,
        }, new(StateChangedType.SetCount));
    }

    public void CountUpWithAmount(int amount) {
        Mutate(state => state with {
            Count = state.Count + amount,
        }, new(StateChangedType.CountUpWithAmount));
    }
}
