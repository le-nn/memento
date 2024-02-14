using System.Collections.Immutable;

using static Memento.Sample.Blazor.Stores.FluxAsyncCounterCommand;

namespace Memento.Sample.Blazor.Stores;

public record FluxAsyncCounterState {
    public int Count { get; init; } = 0;

    public bool IsLoading { get; init; } = false;

    public ImmutableArray<int> Histories { get; init; } = [];
}

public record FluxAsyncCounterCommand : Command {
    public record IncrementAndEndLoading : FluxAsyncCounterCommand;
    public record Increment : FluxAsyncCounterCommand;
    public record AddWithAmount(int Amount) : FluxAsyncCounterCommand;
    public record SetCount(int Count) : FluxAsyncCounterCommand;
    public record BeginLoading : FluxAsyncCounterCommand;
}

public class FluxAsyncCounterStore : FluxStore<FluxAsyncCounterState, FluxAsyncCounterCommand> {
    public FluxAsyncCounterStore() : base(() => new(), Reducer) { }

    static FluxAsyncCounterState Reducer(FluxAsyncCounterState state, FluxAsyncCounterCommand command) {
        return command switch {
            IncrementAndEndLoading => state with {
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
            AddWithAmount payload => state with {
                Count = state.Count + payload.Amount,
            },
            _ => throw new CommandNotHandledException<FluxAsyncCounterCommand>(command),
        };
    }

    public void CountUp() {
        Dispatch(new Increment());
    }

    public async Task CountUpAsync() {
        Dispatch(new BeginLoading());
        await Task.Delay(800);
        Dispatch(new IncrementAndEndLoading());
    }

    public void CountUpManyTimes(int count) {
        for (var i = 0; i < count; i++) {
            Dispatch(new Increment());
        }
    }

    public void SetCount(int count) {
        Dispatch(new SetCount(count));
    }

    public void CountUpWithAmount(int amount) {
        Dispatch(new AddWithAmount(amount));
    }
}