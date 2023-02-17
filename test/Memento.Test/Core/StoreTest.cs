using Memento.Core;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Memento.Test.Core.AsyncCounterCommands;
using static System.Formats.Asn1.AsnWriter;

namespace Memento.Test.Core;

public class StoreTest {
    [Theory]
    [InlineData(10)]
    [InlineData(50)]
    [InlineData(100)]
    public async Task Ensure_StateChanges(int count) {
        var store = new AsyncCounterStore();

        var list = new List<int>();

        var actual = 0;
        for (var i = 0; i < count; i++) {

            await store.CountUpAsync();
            actual++;
            list.Add(actual);

            Assert.Equal(actual, store.State.Count);
            Assert.Equal(list, store.State.History);

            store.SetCount(1234);
            actual = 1234;
            list.Add(actual);

            Assert.Equal(actual, store.State.Count);
            Assert.Equal(list, store.State.History);
        }

    }

    [Theory]
    [InlineData(5)]
    [InlineData(20)]
    [InlineData(100)]
    public async Task Ensure_CanChangeStateAsync(int count) {
        var store = new AsyncCounterStore();

        var list = new List<int>();
        for (var i = 0; i < count; i++) {
            list.Add(i);
            var task = store.SetCountWithRandomTimerAsync(i);
            Assert.True(store.State.IsLoading);
            await task;
            Assert.False(store.State.IsLoading);
            Assert.Equal(i, store.State.Count);
            Assert.Equal(list, store.State.History);
        }
    }
}

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
    readonly Random _random = new(1234);

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

        static AsyncCounterState HandleIncrement(AsyncCounterState state) {
            var count = state.Count + 1;
            return state with {
                Count = count,
                History = state.History.Add(count),
            };
        }
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
