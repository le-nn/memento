using Memento;
using System.Collections.Immutable;
using static Blazor.Sample.Stores.AsyncCounterMessage;

namespace Blazor.Sample.Stores;

public record AsyncCounterState : State {
    public int Count { get; init; } = 0;

    public bool IsLoading { get; init; } = false;

    public ImmutableArray<int> Histories { get; init; } = ImmutableArray.Create<int>();
}

public record AsyncCounterMessage : Message {
    public record CountUp : AsyncCounterMessage;
    public record SetCount(int Count) : AsyncCounterMessage;
    public record BeginLoading : AsyncCounterMessage;
}

public class AsyncCounterStore : Store<AsyncCounterState, AsyncCounterMessage> {
    public AsyncCounterStore() : base(() => new(), Mutation) { }

    static AsyncCounterState Mutation(AsyncCounterState state, AsyncCounterMessage message) {
        return message switch {
            CountUp => state with {
                Count = state.Count + 1,
                IsLoading = false,
                Histories = state.Histories.Add(state.Count + 1),
            },
            SetCount payload => state with {
                Count = payload.Count,
            },
            BeginLoading => state with {
                IsLoading = true,
            },
            _ => throw new Exception("The message is not handled."),
        };
    }

    public async Task CountUpAsync() {
        this.Mutate(new BeginLoading());
        await Task.Delay(800);
        this.Mutate(new CountUp());
    }

    public void SetCount(int c) {
        this.Mutate(new SetCount(c));
    }
}
