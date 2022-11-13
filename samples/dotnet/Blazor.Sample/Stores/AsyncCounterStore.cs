using Memento.Core;
using System.Collections.Immutable;
using static Blazor.Sample.Stores.AsyncCounterCommands;

namespace Blazor.Sample.Stores;

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
        Mutate(new BeginLoading());
        await Task.Delay(800);
        Mutate(new CountUp());
    }

    public void CountUpManyTimes(int count) {
        for (int i = 0; i < count; i++) {
            Mutate(new Increment());
        }
    }

    public void SetCount(int c) {
        Mutate(new SetCount(c));
    }
}
