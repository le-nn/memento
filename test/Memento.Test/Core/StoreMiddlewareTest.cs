using Memento.Blazor;
using Memento.Test.Core.Mock;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace Memento.Test.Core;

public class StoreMiddlewareTest {
    [Fact]
    public async Task MiddlewareTest() {
        var collection = new ServiceCollection();
        collection.AddMemento()
            .AddStore<Mock.AsyncCounterStore>()
            .AddMiddleware<MockMiddleware>();
        var provider = collection.BuildServiceProvider();

        var store = provider.GetRequiredService<Mock.AsyncCounterStore>();
        var middleware = provider.GetRequiredService<Mock.MockMiddleware>();
        var mementoProvider = provider.GetStoreProvider();

        await mementoProvider.InitializeAsync();

        store.CountUp();
        store.CountUp();
        store.CountUp();
        store.CountUp();
        store.CountUp();

        Assert.Equal(5, middleware.Handler?.HandleStoreDispatchCalledCount);
        Assert.Equal(5, middleware.Handler?.ProviderDispatchCalledCount);
    }
}
