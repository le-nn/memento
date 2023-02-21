using Memento.Blazor;
using Memento.Test.Core.Mock;
using Microsoft.Extensions.DependencyInjection;
using Memento.Core;
using System.Text.Json;
using System.Collections.Immutable;

namespace Memento.Test.Core;

public class ProviderTest {
    [Fact]
    public async Task Test() {
        var collection = new ServiceCollection();
        collection.AddMemento()
            .AddStore<Mock.AsyncCounterStore>();
        var provider = collection.BuildServiceProvider();

        var store = provider.GetRequiredService<Mock.AsyncCounterStore>();
        var mementoProvider = provider.GetStoreProvider();

        var events = new List<RootStateChangedEventArgs>();

        using var subscription = mementoProvider.Subscribe(e => {
            events.Add(e);
        });

        await mementoProvider.InitializeAsync();

        await Assert.ThrowsAnyAsync<InvalidOperationException>(async () => {
            await mementoProvider.InitializeAsync();
        });

        store.CountUp();
        store.CountUp();
        store.CountUp();
        store.CountUp();
        store.CountUp();

        Assert.Equal(5, events.Count);

        // Ensure root state is correct
        var root = mementoProvider.CaptureRootState();
        var expected = JsonSerializer.Serialize(new {
            AsyncCounterStore = new AsyncCounterState {
                Count = 5,
                History = ImmutableArray.Create(1, 2, 3, 4, 5),
            }
        });
        var actual = JsonSerializer.Serialize(root.AsDictionary());
        Assert.Equal(expected, actual);
    }
}
