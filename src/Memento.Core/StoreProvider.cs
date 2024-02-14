using Memento.Core.Internals;
using System.Collections.Concurrent;

namespace Memento.Core;

/// <summary>
/// Represents a provider that manages all stores and middlewares.
/// The root state tree is provided from this class.
/// Implements the IObservable and IDisposable interfaces.
/// </summary>
public class StoreProvider : IObservable<RootStateChangedEventArgs>, IDisposable {
    readonly IServiceProvider _serviceContainer;
    readonly ConcurrentDictionary<Guid, IDisposable> _subscriptions = new();
    readonly ConcurrentDictionary<Guid, IObserver<RootStateChangedEventArgs>> _observers = new();
    readonly IReadOnlyList<IStore<object, object>> _stores;
    readonly IReadOnlyList<Middleware> _middleware;

    /// <summary>
    /// Gets a value indicating whether the provider has initialized.
    /// </summary>
    public bool IsInitialized { get; private set; }

    public bool HasDisposed { get; private set; }

    /// <summary>
    /// Initializes a new instance of the StoreProvider class.
    /// </summary>
    /// <param name="container">The service container used to resolve stores and middleware.</param>
    /// <param name="stores">The stores.</param>
    /// <param name="middlewares">The middlewares.</param>
    public StoreProvider(
        IServiceProvider container,
        IReadOnlyCollection<IStore<object, object>>? stores = null,
        IReadOnlyCollection<Middleware>? middlewares = null
    ) {
        _serviceContainer = container;
        _stores = [
            .. _serviceContainer.GetAllStores(),
            .. stores ?? []
        ];
        _middleware = [
            .. _serviceContainer.GetAllMiddleware(),
            .. middlewares ?? []
        ];
    }

    /// <summary>
    /// Disposes the store provider and its resources.
    /// </summary>
    public void Dispose() {
        if (HasDisposed) {
            return;
        }

        HasDisposed = true;

        foreach (var (_, subscription) in _subscriptions) {
            subscription.Dispose();
        }

        foreach (var middleware in GetAllMiddleware()) {
            middleware.Dispose();
        }

        foreach (var store in _stores) {
            store.Dispose();
        }
    }

    /// <summary>
    /// Captures the current root state of all stores.
    /// </summary>
    /// <returns>A RootState instance representing the current state of all stores.</returns>
    public RootState CaptureRootState() {
        var map = new Dictionary<string, object>();
        foreach (var item in ResolveAllStores()) {
            map.Add(item.GetType().Name, item.State);
        }

        return new RootState(map);
    }

    /// <summary>
    /// Captures a dictionary containing all stores keyed by their type name.
    /// </summary>
    /// <returns>A dictionary containing all stores keyed by their type name.</returns>
    public Dictionary<string, IStore<object, object>> CaptureStoreBag() {
        var map = new Dictionary<string, IStore<object, object>>();
        foreach (var item in ResolveAllStores()) {
            map.Add(item.GetType().Name, item);
        }

        return map;
    }

    /// <summary>
    /// Initializes a provider.
    /// You must invoke once.
    /// </summary>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException">Throws when provider has initialized.</exception>
    /// <exception cref="InvalidDataException">Throws when registered middleware and stores are incorrect.</exception>
    public async Task InitializeAsync() {
        if (IsInitialized) {
            return;
        }

        IsInitialized = true;
        // observe all stores.
        foreach (var store in ResolveAllStores()) {
            var subscription = store.Subscribe(new StoreObserver<object, object>(e => {
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
            if (_subscriptions.TryAdd(Guid.NewGuid(), subscription) is false) {
                throw new InvalidOperationException("Failed to add subscription.");
            }
        }

        // Initialize all middleware.
        foreach (var middleware in GetAllMiddleware()) {
            try {
                await middleware.InitializeAsync(_serviceContainer);
            }
            catch (Exception ex) {
                throw new InvalidDataException($@"Failed to initialize memento middleware ""{ex.Message}""", ex);
            }
        }

        // InitializeAsync all stores.
        foreach (var store in ResolveAllStores()) {
            try {
                await store.InitializeAsync(this);
            }
            catch (Exception ex) {
                throw new InvalidDataException(@$"Failed to initialize memento provider ""{ex.Message}""", ex);
            }
        }
    }

    /// <summary>
    /// Resolves a store of the specified type.
    /// </summary>
    /// <typeparam name="TStore">The type of the store to resolve.</typeparam>
    /// <returns>An instance of the specified store type.</returns>
    /// <exception cref="ArgumentException">Thrown when the specified store type is not registered in the provider.</exception>
    public TStore ResolveStore<TStore>()
        where TStore : IStore<object, object> {
        if (_serviceContainer.GetService(typeof(TStore)) is TStore store) {
            return store;
        }

        throw new ArgumentException($"{typeof(TStore).Name} is not registered in provider.");
    }

    /// <summary>
    /// Resolves all stores registered in the provider.
    /// </summary>
    /// <returns>An IEnumerable containing all registered stores.</returns>
    public IEnumerable<IStore<object, object>> ResolveAllStores() {
        return _stores;
    }

    /// <summary>
    /// Gets all middleware registered in the provider.
    /// </summary>
    /// <returns>An IEnumerable containing all registered middleware.</returns>
    public IEnumerable<Middleware> GetAllMiddleware() {
        return _middleware;
    }

    /// <summary>
    /// Subscribes the provided observer to the store provider.
    /// </summary>
    /// <param name="observer">The observer to subscribe to the store provider.</param>
    /// <returns>An IDisposable instance that can be used to unsubscribe from the store provider.</returns>
    public IDisposable Subscribe(IObserver<RootStateChangedEventArgs> observer) {
        var id = Guid.NewGuid();
        if (_observers.TryAdd(id, observer) is false) {
            throw new InvalidOperationException("Failed to add observer.");
        }

        return new StoreSubscription(GetType().FullName ?? "Store", () => {
            if (_observers.TryRemove(new(id, observer)) is false) {
                throw new InvalidOperationException("Failed to remove observer.");
            }
        });
    }

    /// <summary>
    /// Subscribes an action to the store provider.
    /// </summary>
    /// <param name="observer">The action to subscribe to the store provider.</param>
    /// <returns>An IDisposable instance that can be used to unsubscribe from the store provider.</returns>
    public IDisposable Subscribe(Action<RootStateChangedEventArgs> observer)
        => Subscribe(new StoreProviderObserver(observer));

    internal Func<RootState?, IStateChangedEventArgs<object, object>, RootState?> GetMiddlewareInvokeHandler() {
        // process middleware
        var middleware = GetAllMiddleware()
            ?? Array.Empty<Middleware>();
        return middleware.Aggregate(
            (RootState? s, IStateChangedEventArgs<object, object> _) => s,
            (before, middleware) =>
                (RootState? s, IStateChangedEventArgs<object, object> m) => middleware.Handler.HandleProviderDispatch(
                    s,
                    m,
                    (_s, _m) => before(_s, m)
                )
        );
    }

    private void InvokeObserver(RootStateChangedEventArgs e) {
        foreach (var (_, observer) in _observers) {
            observer.OnNext(e);
        }
    }
}