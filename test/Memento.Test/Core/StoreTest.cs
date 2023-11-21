using FluentAssertions;
using Memento.Core;
using Memento.Test.Core.Mock;
using System.Collections.Concurrent;
using System.Reflection;

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

        var commands = new List<Command?>();
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
            Command.StateHasChanged,
            Command.StateHasChanged,
            Command.StateHasChanged,
            Command.StateHasChanged,
            Command.StateHasChanged,
            Command.StateHasChanged,
            Command.StateHasChanged,
        ]);
    }

    [Fact]
    public async Task Force_ReplaceState() {
        var store = new AsyncCounterStore();

        var commands = new List<IStateChangedEventArgs<object, Command>>();

        var lastState = store.State;
        using var subscription = store.Subscribe(e => {
            Assert.Equal(e.Sender, store);
            Assert.NotEqual(e.State, lastState);
            Assert.Equal(e.LastState, lastState);
            lastState = e.State;
            commands.Add(e);
        });

        await store.CountUpAsync();
        store.SetCount(1234);
        if (store is IStore<object, Command> iStore) {
            iStore.SetStateForce(store.State with {
                Count = 5678
            });
        }

        await store.CountUpAsync();

        Assert.True(commands is [
            { Command: Command.StateHasChanged },
            { Command: Command.StateHasChanged },
            { Command: Command.StateHasChanged },
            { Command: Command.StateHasChanged },
            { Command: null, State: AsyncCounterState { Count: 5678 }, StateChangeType: StateChangeType.ForceReplaced },
            { Command: Command.StateHasChanged },
            { Command: Command.StateHasChanged },
            { Command: Command.StateHasChanged },
        ]);
    }

    [Fact]
    public void Ensure_StateHasChangedInvoked() {
        var store = new AsyncCounterStore();
        var commands = new List<IStateChangedEventArgs<object, Command>>();
        var lastState = store.State;
        using var subscription = store.Subscribe(e => commands.Add(e));

        store.StateHasChanged();
        store.StateHasChanged();
        store.StateHasChanged();
        store.StateHasChanged();
        store.StateHasChanged();
        store.StateHasChanged();

        Assert.True(commands is [
            { StateChangeType: StateChangeType.StateHasChanged },
            { StateChangeType: StateChangeType.StateHasChanged },
            { StateChangeType: StateChangeType.StateHasChanged },
            { StateChangeType: StateChangeType.StateHasChanged },
            { StateChangeType: StateChangeType.StateHasChanged },
            { StateChangeType: StateChangeType.StateHasChanged },
        ]);
    }

    [Fact]
    public void Ensure_Observable() {
        var store = new AsyncCounterStore();
        var observers = (store.GetType()
            .BaseType
            ?.BaseType
            ?.BaseType
            ?.GetField(
                "_observers",
                BindingFlags.NonPublic | BindingFlags.Instance
            )
            ?.GetValue(store) as IDictionary<Guid, IObserver<IStateChangedEventArgs<AsyncCounterState, Command.StateHasChanged<AsyncCounterState, string>>>>
         ) ?? throw new Exception("_observers is not found.");

        var disposables = new BlockingCollection<IDisposable>();
        Parallel.For(0, 10000, i => {
            disposables.Add(store.Subscribe(e => { }));
        });
        observers.Count.Should().Be(10000);

        Parallel.ForEach(disposables, e => {
            e.Dispose();
        });
        observers.Count.Should().Be(0);
    }
}
