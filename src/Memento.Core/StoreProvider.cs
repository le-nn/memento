using Memento.Core.Internals;
using Memento.Core.Store.Internals;
using System.Collections.Immutable;

namespace Memento.Core;

public class StoreProvider : IObservable<RootStateChangedEventArgs>, IDisposable {
    readonly IServiceProvider _serviceContainer;
    readonly List<IDisposable> _subscriptions = new();
    readonly List<IObserver<RootStateChangedEventArgs>> _observers = new();
    readonly object _locker = new();

    readonly ImmutableArray<IStore> _stores;
    readonly ImmutableArray<Middleware> _middlewares;

    public bool IsInitialized { get; private set; }

    public StoreProvider(IServiceProvider container) {
        _serviceContainer = container;
        _stores = _serviceContainer.GetAllStores().ToImmutableArray();
        _middlewares = _serviceContainer.GetAllMiddlewares().ToImmutableArray();
    }

    public RootState CaptureRootState() {
        return new(ResolveAllStores().Aggregate(
            ImmutableDictionary.Create<string, object>(),
            (x, y) => x.Add(y.GetType().Name, y.State)
        ));
    }

    public ImmutableDictionary<string, IStore> CaptureStoreBag() => ResolveAllStores().Aggregate(
        ImmutableDictionary.Create<string, IStore>(),
        (x, y) => x.Add(y.GetType().Name, y)
    );

    public async Task InitializAsync() {
        if (IsInitialized) {
            throw new InvalidOperationException("Already initialized.");
        }

        IsInitialized = true;
        // observe all stores.
        foreach (var store in ResolveAllStores()) {
            var subscription = store.Subscribe(new StoreObeserver(e => {
                var handler = GetMiddlewareInvokeHandler();
                var rootState = handler(CaptureRootState(), e);
                if (rootState is not null) {
                    InvokeObserver(new RootStateChangedEventArgs() {
                        StateChangedEvent = e,
                        Store = store,
                        RootState = rootState,
                    });
                }
            }));
            _subscriptions.Add(subscription);
        }

        // Initalize all middlewares.
        foreach (var middleware in GetAllMiddlewares()) {
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
        return _stores;
    }

    public IEnumerable<Middleware> GetAllMiddlewares() {
        return _middlewares;
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

    internal Func<RootState, StateChangedEventArgs, RootState?> GetMiddlewareInvokeHandler() {
        // process middlewares
        var middlewares = GetAllMiddlewares()
            ?? Array.Empty<Middleware>();
        return middlewares.Aggregate(
            (RootState s, StateChangedEventArgs _) => s,
            (before, middleware) =>
                (RootState s, StateChangedEventArgs m) => middleware.Handler.HandleProviderDispatch(
                    s,
                    m,
                    (_s, _m) => before(_s, m)
                )
        );
    }

    private void InvokeObserver(RootStateChangedEventArgs e) {
        foreach (var observer in _observers) {
            observer.OnNext(e);
        }
    }

    public void Dispose() {
        foreach (var subscription in _subscriptions) {
            subscription.Dispose();
        }

        // Initalize all middlewares.
        foreach (var middleware in GetAllMiddlewares()) {
            middleware.Dispose();
        }
    }
}