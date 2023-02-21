using Memento.Blazor;
using Memento.Test.Core.Mock;
using Microsoft.Extensions.DependencyInjection;

namespace Memento.Test.Core;

public class StoreMiddlewareTest {
    [Fact]
    public async Task MiddlewareTest() {
        var collection = new ServiceCollection();
        collection.AddMemento()
            .AddStore<AsyncCounterStore>()
            .AddMiddleware<MockMiddleware>();
        using var provider = collection.BuildServiceProvider();

        var store = provider.GetRequiredService<AsyncCounterStore>();
        var middleware = provider.GetRequiredService<MockMiddleware>();
        var mementoProvider = provider.GetStoreProvider();

        await mementoProvider.InitializeAsync();

        store.CountUp();
        store.CountUp();
        store.CountUp();
        store.CountUp();
        store.CountUp();

        //Assert.Equal(5, middleware.Handler?.HandleStoreDispatchCalledCount);
        //Assert.Equal(5, middleware.Handler?.ProviderDispatchCalledCount);
    }
}
