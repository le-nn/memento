using Memento.Core.Internals;
using Memento.Core.Store.Internals;
using System.Collections.Immutable;

namespace Memento.Core;

public record RootStateChangedEventArgs {
    public required StateChangedEventArgs StateChangedEvent { get; init; }
    public required IStore Store { get; init; }
}

public class StoreProvider : IObservable<RootStateChangedEventArgs> {
    IServiceProvider ServiceContainer { get; }
    List<IDisposable> Subscriptions { get; } = new();
    private List<IObserver<RootStateChangedEventArgs>> Observers { get; } = new();
    object locker = new();

    public ImmutableDictionary<string, object> RootState() {
        return ResolveAllStores().Aggregate(
            ImmutableDictionary.Create<string, object>(),
            (x, y) => x.Add(y.GetType().Name, y.State)
        );
    }

    public StoreProvider(IServiceProvider container) {
        ServiceContainer = container;

        // observe all stores.
        foreach (var store in ResolveAllStores()) {
            var subscription = store.Subscribe(new StoreObeserver(e => {
                InvokeObserver(new RootStateChangedEventArgs() {
                    StateChangedEvent = e,
                    Store = store,
                });
            }));
            Subscriptions.Add(subscription);
        }

        // Initalize all middlewares.
        foreach (var middleware in ResolveAllMiddlewares()) {
            try {
                middleware.OnInitialized(this);
            }
            catch (Exception ex) {
                throw new Exception(@"Failed to initalize memento middleware ""{ex.command}""", ex);
            }
        }

        // Initialize all stores.
        foreach (var store in ResolveAllStores()) {
            try {
                store.OnInitialized(this);
            }
            catch (Exception ex) {
                throw new Exception(@$"Failed to initalize memento provider ""{ex.Message}""", ex);
            }
        }
    }

    public IReadOnlyDictionary<string, object> MapRootStateTreeDictionary() {
        return new Dictionary<string, object>();
    }

    public TStore ResolveStore<TStore>()
        where TStore : IStore {
        if (ServiceContainer.GetService(typeof(TStore)) is TStore store) {
            return store;
        }

        throw new ArgumentException($"{typeof(TStore).Name} is not registered in provider.");
    }

    public IEnumerable<IStore> ResolveAllStores() {
        return ServiceContainer.GetAllStores();
    }

    public IEnumerable<Middleware> ResolveAllMiddlewares() {
        return ServiceContainer.GetAllMiddlewares();
    }

    public IDisposable Subscribe(IObserver<RootStateChangedEventArgs> observer) {
        lock (locker) {
            Observers.Add(observer);
        }

        return new StoreSubscription($"Store.Subscribe", () => {
            lock (locker) {
                Observers.Remove(observer);
            }
        });
    }

    public IDisposable Subscribe(Action<RootStateChangedEventArgs> observer)
        => Subscribe(new StoreProviderObserver(observer));

    private void InvokeObserver(RootStateChangedEventArgs e) {
        foreach (var observer in Observers) {
            observer.OnNext(e);
        }
    }
}
