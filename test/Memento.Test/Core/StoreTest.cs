using Memento.Core;
using Memento.Test.Core.Mock;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

    [Fact]
    public async Task Command_CouldBeSubscribeCorrectly() {
        var store = new AsyncCounterStore();

        var commands = new List<Command>();
        var lastState = store.State;
        using var subscription = store.Subscribe(e => {
            Assert.Equal(e.Sender, store);
            Assert.NotEqual(e.State, lastState);
            Assert.Equal(e.LastState, lastState);
            lastState = e.State;
            commands.Add(e.Command);
        });

        await store.CountUpAsync();
        store.SetCount(1234);
        await store.CountUpAsync();

        Assert.True(commands is [
            AsyncCounterCommands.BeginLoading,
            AsyncCounterCommands.Increment,
            AsyncCounterCommands.EndLoading,
            AsyncCounterCommands.ModifyCount(1234),
            AsyncCounterCommands.BeginLoading,
            AsyncCounterCommands.Increment,
            AsyncCounterCommands.EndLoading
        ]);
    }

    [Fact]
    public async Task Force_ReplaceState() {
        var store = new AsyncCounterStore();

        var commands = new List<Command>();

        var lastState = store.State;
        using var subscription = store.Subscribe(e => {
            Assert.Equal(e.Sender, store);
            Assert.NotEqual(e.State, lastState);
            Assert.Equal(e.LastState, lastState);
            lastState = e.State;
            commands.Add(e.Command);
        });

        await store.CountUpAsync();
        store.SetCount(1234);
        if (store is IStore iStore) {
            iStore.SetStateForce(store.State with {
                Count = 5678
            });
        }

        await store.CountUpAsync();

        Assert.True(commands is [
            AsyncCounterCommands.BeginLoading,
            AsyncCounterCommands.Increment,
            AsyncCounterCommands.EndLoading,
            AsyncCounterCommands.ModifyCount(1234),
            Command.ForceReplaced { State: AsyncCounterState { Count: 5678 } },
            AsyncCounterCommands.BeginLoading,
            AsyncCounterCommands.Increment,
            AsyncCounterCommands.EndLoading,
        ]);
    }

    [Fact]
    public async Task Performance() {
        var store = new AsyncCounterStore();

        var commands = new List<Command>();

        var lastState = store.State;
        using var subscription = store.Subscribe(e => {
            commands.Add(e.Command);
        });

        var sw = Stopwatch.StartNew();

        for (var i = 0; i < 10000; i++) {
            store.CountUp();
        }

        sw.Stop();

        Assert.True(sw.ElapsedMilliseconds < 100);
    }
}
