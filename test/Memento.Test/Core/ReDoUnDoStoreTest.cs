using FluentAssertions;
using Memento.Core;
using Memento.Test.Core.Mock;
using Microsoft.Extensions.DependencyInjection;

namespace Memento.Test.Core;

public class ReDoUnDoStoreTest {
    [Fact]
    public async Task Test1() {
        var services = new ServiceCollection().BuildServiceProvider();

        using var store = new RedoUndoTodoStore(new MockTodoService());
        using var provider = new StoreProvider(services, [store]);
        await provider.InitializeAsync();

        Assert.Equal(store.State, new()); // Should().Be() is not working
        await store.LoadAsync();

        store.PastHistories.Count.Should().Be(0);
        store.FutureHistories.Count.Should().Be(0);
        store.Present.Should().BeNull();

        var todoStates = new List<RedoUndoTodoState>();
        await store.CreateNewAsync("test1");
        todoStates.Add(store.State);

        await store.CreateNewAsync("test2");
        todoStates.Add(store.State);

        await store.CreateNewAsync("test3");
        todoStates.Add(store.State);

        store.PastHistories.Count.Should().Be(2);
        store.FutureHistories.Count.Should().Be(0);
        store.Present.Should().NotBeNull();

        Assert.True(todoStates[1] == store.Present?.HistoryState.State);
        Assert.True(store.PastHistories is [_, _]);

        await store.CreateNewAsync("test5");
        todoStates.Add(store.State);
        Assert.True(todoStates[2] == store.Present?.HistoryState.State);
        Assert.True(store.PastHistories is [_, _, _]);

        await store.CreateNewAsync("test6");
        todoStates.Add(store.State);
        Assert.True(todoStates[3] == store.Present?.HistoryState.State);
        Assert.True(store.PastHistories is [_, _, _, _]);

        await store.CreateNewAsync("test7");
        todoStates.Add(store.State);
        Assert.True(todoStates[4] == store.Present?.HistoryState.State);
        Assert.True(store.PastHistories is [_, _, _, _, _]);

        // UnDo
        await store.UnDoAsync();
        todoStates.Remove(todoStates.Last());
        Assert.True(todoStates[3] == store.Present?.HistoryState.State);
        Assert.True(store.PastHistories is [_, _, _, _]);
        Assert.True(store.FutureHistories is [_]);

        // UnDo
        await store.UnDoAsync();
        todoStates.Remove(todoStates.Last());
        Assert.True(todoStates[2] == store.Present?.HistoryState.State);
        Assert.True(store.PastHistories is [_, _, _]);
        Assert.True(store.FutureHistories is [_, _]);

        // ReDo
        await store.ReDoAsync();
        todoStates.Add(store.State);
        Assert.True(todoStates[3] == store.Present?.HistoryState.State);
        Assert.True(store.PastHistories is [_, _, _, _]);
        Assert.True(store.FutureHistories is [_,]);

        // UnDo
        await store.UnDoAsync();
        todoStates.Remove(todoStates.Last());
        Assert.True(todoStates[2] == store.Present?.HistoryState.State);
        Assert.True(store.PastHistories is [_, _, _]);
        Assert.True(store.FutureHistories is [_, _]);

        // UnDo
        await store.UnDoAsync();
        todoStates.Remove(todoStates.Last());
        Assert.True(todoStates[1] == store.Present?.HistoryState.State);
        Assert.True(store.PastHistories is [_, _]);
        Assert.True(store.FutureHistories is [_, _, _]);

        await store.CreateNewAsync("test8");
        todoStates.Add(store.State);
        Assert.True(todoStates[2] == store.Present?.HistoryState.State);
        Assert.True(store.PastHistories is [_, _, _]);
        Assert.True(store.FutureHistories is []);
    }
}
