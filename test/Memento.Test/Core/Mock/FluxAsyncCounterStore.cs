using Memento.Core;
using System.Collections.Immutable;

namespace Memento.Test.Core.Mock;

using static Memento.Test.Core.Mock.FluxAsyncCounterCommands;

// Define state to manage in store
public record FluxAsyncCounterState {
    public int Count { get; init; } = 0;
    public ImmutableArray<int> History { get; init; } = [];
    public bool IsLoading { get; init; } = false;
}

// Define command to change state and observe state change event in detail.
public record FluxAsyncCounterCommands : Command {
    public record Increment : FluxAsyncCounterCommands;
    public record IncrementWithoutHistory : FluxAsyncCounterCommands;
    public record BeginLoading : FluxAsyncCounterCommands;
    public record EndLoading : FluxAsyncCounterCommands;
    public record ModifyCount(int Value) : FluxAsyncCounterCommands;
}

public class FluxAsyncCounterStore : FluxStore<FluxAsyncCounterState, FluxAsyncCounterCommands> {
    readonly Random _random = new(1234);

    public FluxAsyncCounterStore() : base(() => new(), Reducer) {
    }

    // State can change via Reducer and easy to observe state from command
    // Reducer generate new state from command and current state
    static FluxAsyncCounterState Reducer(FluxAsyncCounterState state, FluxAsyncCounterCommands command) {
        return command switch {
            BeginLoading => state with {
                IsLoading = true
            },
            EndLoading => state with {
                IsLoading = false
            },
            Increment => HandleIncrement(state),
            IncrementWithoutHistory => state with {
                Count = state.Count + 1,
            },
            ModifyCount payload => state with {
                Count = payload.Value,
                History = state.History.Add(payload.Value),
            },
            _ => throw new CommandNotHandledException<FluxAsyncCounterCommands>(command),
        };

        static FluxAsyncCounterState HandleIncrement(FluxAsyncCounterState state) {
            var count = state.Count + 1;
            return state with {
                Count = count,
                History = state.History.Add(count),
            };
        }
    }


    public void CountUp() {
        Dispatch(new Increment());
    }

    public void CountUpWithoutHistory() {
        Dispatch(new IncrementWithoutHistory());
    }

    // "Dispatch" method can called outside of store via action (public method)
    // Action can be async method.
    public async Task CountUpAsync() {
        Dispatch(new BeginLoading());

        await Task.Delay(_random.Next(1, 50));

        Dispatch(new Increment());
        Dispatch(new EndLoading());
    }

    public void SetCount(int num) {
        Dispatch(new ModifyCount(num));
    }

    public async Task SetCountWithRandomTimerAsync(int num) {
        Dispatch(new BeginLoading());
        await Task.Delay(_random.Next(1, 50));
        Dispatch(new ModifyCount(num));
        Dispatch(new EndLoading());
    }
}
