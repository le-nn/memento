using Memento.Core.Store.Internals;
using System.Collections.Immutable;

namespace Memento;

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
        return this.ResolveAllStores().Aggregate(
            ImmutableDictionary.Create<string, object>(),
            (x, y) => x.Add(y.GetType().Name, y.State)
        );
    }

    public StoreProvider(IServiceProvider container) {
        this.ServiceContainer = container;

        // observe all stores.
        foreach (var store in this.ResolveAllStores()) {
            var subscription = store.Subscribe(new StoreObeserver(e => {
                this.InvokeObserver(new RootStateChangedEventArgs() {
                    StateChangedEvent = e,
                    Store = store,
                });
            }));
            Subscriptions.Add(subscription);
        }

        // Initalize all middlewares.
        foreach (var middleware in this.ResolveAllMiddlewares()) {
            try {
                middleware.OnInitialized(this);
            }
            catch (Exception ex) {
                throw new Exception(@"Failed to initalize memento middleware ""{ex.message}""", ex);
            }
        }

        // Initialize all stores.
        foreach (var store in this.ResolveAllStores()) {
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
        if (this.ServiceContainer.GetService(typeof(TStore)) is TStore store) {
            return store;
        }

        throw new ArgumentException($"{typeof(TStore).Name} is not registered in provider.");
    }

    public IEnumerable<IStore> ResolveAllStores() {
        return this.ServiceContainer.GetAllStores();
    }

    public IEnumerable<Middleware> ResolveAllMiddlewares() {
        return this.ServiceContainer.GetAllMiddlewares();
    }

    public IDisposable Subscribe(IObserver<RootStateChangedEventArgs> observer) {
        lock (this.locker) {
            this.Observers.Add(observer);
        }

        return new StoreSubscription($"Store.Subscribe", () => {
            lock (this.locker) {
                this.Observers.Remove(observer);
            }
        });
    }

    public IDisposable Subscribe(Action<RootStateChangedEventArgs> observer)
        => this.Subscribe(new StoreProviderObserver(observer));

    private void InvokeObserver(RootStateChangedEventArgs e) {
        foreach (var observer in this.Observers) {
            observer.OnNext(e);
        }
    }
}
