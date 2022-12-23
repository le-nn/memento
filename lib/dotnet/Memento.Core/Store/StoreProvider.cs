using Memento.Core.Internals;
using Memento.Core.Store;
using Memento.Core.Store.Internals;
using System.Collections.Immutable;

namespace Memento.Core;

public record RootStateChangedEventArgs {
    public required StateChangedEventArgs StateChangedEvent { get; init; }
    public required IStore Store { get; init; }
    public required ImmutableDictionary<string, object> RootState { get; init; }
}

public class StoreProvider : IObservable<RootStateChangedEventArgs> {
    readonly IServiceProvider _serviceContainer;
    readonly List<IDisposable> _subscriptions = new();
    readonly List<IObserver<RootStateChangedEventArgs>> _observers = new();
    readonly object _locker = new();

    public ImmutableDictionary<string, object> CaptureRootState() {
        return ResolveAllStores().Aggregate(
            ImmutableDictionary.Create<string, object>(),
            (x, y) => x.Add(y.GetType().Name, y.State)
        );
    }

    public ImmutableDictionary<string, IStore> CaptureStoreBag() => ResolveAllStores().Aggregate(
        ImmutableDictionary.Create<string, IStore>(),
        (x, y) => x.Add(y.GetType().Name, y)
    );


    public StoreProvider(IServiceProvider container) {
        _serviceContainer = container;
    }

    public async Task InitializAsync() {
        // observe all stores.
        foreach (var store in ResolveAllStores()) {
            var subscription = store.Subscribe(new StoreObeserver(e => {
                InvokeObserver(new RootStateChangedEventArgs() {
                    StateChangedEvent = e,
                    Store = store,
                    RootState = CaptureRootState(),
                });
            }));
            _subscriptions.Add(subscription);
        }

        // Initalize all middlewares.
        foreach (var middleware in ResolveAllMiddlewares()) {
            try {
                await middleware.InitializeAsync(_serviceContainer);
            }
            catch (Exception ex) {
                throw new Exception($@"Failed to initalize memento middleware ""{ex.Message}""", ex);
            }
        }

        // InitializeAsync all stores.
        foreach (var store in ResolveAllStores()) {
            try {
                await store.OnInitializedAsync(this);
            }
            catch (Exception ex) {
                throw new Exception(@$"Failed to initalize memento provider ""{ex.Message}""", ex);
            }
        }
    }

    public TStore ResolveStore<TStore>()
        where TStore : IStore {
        if (_serviceContainer.GetService(typeof(TStore)) is TStore store) {
            return store;
        }

        throw new ArgumentException($"{typeof(TStore).Name} is not registered in provider.");
    }

    public IEnumerable<IStore> ResolveAllStores() {
        return _serviceContainer.GetAllStores();
    }

    public IEnumerable<Middleware> ResolveAllMiddlewares() {
        return _serviceContainer.GetAllMiddlewares();
    }

    public IDisposable Subscribe(IObserver<RootStateChangedEventArgs> observer) {
        lock (_locker) {
            _observers.Add(observer);
        }

        return new StoreSubscription(GetType().FullName ?? "Store", () => {
            lock (_locker) {
                _observers.Remove(observer);
            }
        });
    }

    public IDisposable Subscribe(Action<RootStateChangedEventArgs> observer)
        => Subscribe(new StoreProviderObserver(observer));

    private void InvokeObserver(RootStateChangedEventArgs e) {
        foreach (var observer in _observers) {
            observer.OnNext(e);
        }
    }
}