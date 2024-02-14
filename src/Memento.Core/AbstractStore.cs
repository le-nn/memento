using Memento.Core.Internals;
using System.Collections.Concurrent;
using System.Runtime.CompilerServices;

namespace Memento.Core;

/// <summary>
/// Represents an abstract store that maintains state and handles commands.
/// Implements the IStore, IObservable, and IDisposable interfaces.
/// </summary>
/// <typeparam name="TState">The type of the state managed by the store.</typeparam>
/// <typeparam name="TCommand">The type of the commands used to mutate the state.</typeparam>
/// <remarks>
/// Initializes a new instance of the <see cref="AbstractStore{TState, TCommand}"/> class.
/// </remarks>
/// <param name="initializer">An initializer that creates an initial state.</param>
/// <param name="reducer">A reducer that changes a store state.</param>
/// <exception cref="ArgumentNullException">Thrown when <paramref name="initializer"/> returns null.</exception>
public abstract class AbstractStore<TState, TMessage>(
    Func<TState> initializer,
    Reducer<TState, TMessage> reducer
) : StateObservable<TMessage>,
        IStore<TState, TMessage>,
        IDisposable
            where TState : class
            where TMessage : class {
    readonly Reducer<TState, TMessage> _reducer = (state, command) => reducer(state, command);
    readonly ConcurrentBag<IDisposable> _disposables = [];

    private StoreProvider? _provider;
    private Func<TState, TMessage?, object?>? _middlewareHandler;

    private Func<TState, TMessage?, object?> MiddlewareHandler => _middlewareHandler ??= GetMiddlewareInvokeHandler();

    /// <summary>
    /// Gets the state initializer for the store.
    /// </summary>
    protected Func<TState> Initializer { get; } = initializer;

    /// <summary>
    /// Gets the current state of the store.
    /// </summary>
    public TState State { get; internal set; } = initializer()
        ?? throw new ArgumentNullException("initializer must be returned not null.");

    /// <summary>
    /// Gets a value indicating whether the store is initialized.
    /// </summary>
    public bool IsInitialized { get; private set; }

    /// <summary>
    /// Gets the store provider instance for the store.
    /// </summary>
    /// <exception cref="InvalidDataException">Thrown when the store has not been initialized.</exception>
    public StoreProvider Provider => _provider
        ?? throw new InvalidDataException("Store has not initialized.");

    public Reducer<object, object> ReducerHandle => (s, c) => _reducer.Invoke((TState)s, (TMessage?)c);

    /// <summary>
    /// Disposes the store and its resources.
    /// </summary>
    public void Dispose() {
        foreach (var d in _disposables) {
            d.Dispose();
        }

        _disposables.Clear();
        OnDisposed();
    }

    /// <summary>
    /// Handles disposable resources of the store.
    /// </summary>
    /// <returns>An enumerable of disposable resources.</returns>
    [Obsolete]
    protected virtual IEnumerable<IDisposable> OnHandleDisposable() {
        return [];
    }

    /// <summary>
    /// Subscribes to the store with the provided observer.
    /// </summary>
    /// <param name="observer">The observer to subscribe to the store.</param>
    /// <returns>An IDisposable instance that can be used to unsubscribe from the store.</returns>
    public IDisposable Subscribe(IObserver<IStateChangedEventArgs<TState, TMessage>> observer) {
        return base.Subscribe(new GeneralObserver<IStateChangedEventArgs<TMessage>>(e => {
            if (e is IStateChangedEventArgs<TState, TMessage> e2) {
                observer.OnNext(e2);
            }
        }));
    }

    /// <summary>
    /// Subscribes to the store with the provided observer.
    /// </summary>
    /// <param name="observer">The observer to subscribe to the store.</param>
    /// <returns>An IDisposable instance that can be used to unsubscribe from the store.</returns>
    public IDisposable Subscribe(Action<IStateChangedEventArgs<TState, TMessage>> observer) {
        return Subscribe(new GeneralObserver<IStateChangedEventArgs<TState, TMessage>>(observer));
    }

    /// <summary>
    /// Casts the current store to the specified store type.
    /// </summary>
    /// <typeparam name="TStore">The store type to cast to.</typeparam>
    /// <returns>The current store cast to the specified store type.</returns>
    /// <exception cref="InvalidCastException">Thrown when the current store cannot be cast to the specified store type.</exception>
    public TStore AsStore<TStore>() where TStore : IStore<TState, TMessage> {
        if (this is TStore store) {
            return store;
        }

        throw new InvalidCastException();
    }

    /// <summary>
    /// Gets the type of the state managed by the store.
    /// </summary>
    /// <returns>The type of the state.</returns>
    public Type GetStateType() {
        return typeof(TState);
    }

    void IStore<TState, TMessage>.SetStateForceSilently(object state) {
        if (state is not TState tState) {
            throw new InvalidDataException($"'{state.GetType().FullName}' is not compatible with '{typeof(TState).FullName}'.");
        }

        State = tState;
    }

    void IStore<TState, TMessage>.SetStateForce(object state) {
        if (state is not TState tState) {
            throw new InvalidDataException($"'{state.GetType().FullName}' is not compatible with '{typeof(TState).FullName}'.");
        }

        var previous = State;
        State = tState;
        InvokeObserver(new StateChangedEventArgs<TState, TMessage>() {
            Message = default,
            StateChangeType = StateChangeType.ForceReplaced,
            LastState = previous,
            Sender = this,
            State = State,
        });
    }

    internal void ComputedAndApplyState(TState state, TMessage? command) {
        if (ComputeNewState() is ( { } s, { } e)) {
            State = s;
            InvokeObserver(e);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        (TState?, StateChangedEventArgs<TState, TMessage>?) ComputeNewState() {
            var previous = state;
            var postState = OnBeforeDispatch(previous, command);

            if (MiddlewareHandler.Invoke(postState, command) is TState s) {
                var newState = OnAfterDispatch(s, command);
                var e = new StateChangedEventArgs<TState, TMessage> {
                    LastState = previous,
                    Message = command,
                    State = newState,
                    Sender = this,
                };

                return (newState, e);
            }

            return (null, null);
        }
    }
    internal Func<TState?, TMessage?, object?> GetMiddlewareInvokeHandler() {
        // process middleware
        var middleware = _provider?.GetAllMiddleware() ?? [];
        return middleware.Aggregate(
            (object? s, TMessage? m) => {
                if (s is TState ss) {
                    return (object?)_reducer.Invoke(ss, m);
                }

                return null;
            },
            (before, middleware) =>
                (object? s, TMessage? m) => middleware.Handler.HandleStoreDispatch(s, m, (_s, _m) => before(_s, m))
        );
    }

    /// <summary>
    /// Called when the store is initialized asynchronously.
    /// </summary>
    /// <param name="provider">The store provider.</param>
    /// <returns>A Task representing the initialization process.</returns>
    protected virtual ValueTask OnInitializedAsync() {
        return ValueTask.CompletedTask;
    }

    /// <summary>
    /// Called before dispatching a command and can be used to modify the state.
    /// </summary>
    /// <param name="state">The current state.</param>
    /// <param name="command">The command for mutating the state.</param>
    /// <returns>An overridden state.</returns>
    protected virtual TState OnBeforeDispatch(TState state, TMessage? command) {
        return state;
    }

    /// <summary>
    /// Called before invoke state changed and return value overrides current state.
    /// </summary>
    /// <param name="state"> The computed state via _reducer.</param>
    /// <param name="command"> The command for mutating the state.</param>
    /// <returns> override state.</returns>
    protected virtual TState OnAfterDispatch(TState state, TMessage? command) {
        return state;
    }

    /// <summary>
    /// Called when the store is disposed.
    /// </summary>
    protected virtual void OnDisposed() {

    }

    /// <summary>
    /// Adds a disposable resource to the store.
    /// </summary>
    /// <param name="disposable">The disposable resource to add.</param>
    protected void AddDisposable(IDisposable disposable) {
        _disposables.Add(disposable);
    }

    /// <summary>
    /// Adds a collection of disposable resources to the store.
    /// </summary>
    /// <param name="disposable">The collection of disposable resources to add.</param>
    protected void AddDisposable(IEnumerable<IDisposable> disposable) {
        foreach (var item in disposable) {
            _disposables.Add(item);
        }
    }

    async ValueTask IStore<TState, TMessage>.InitializeAsync(StoreProvider provider) {
        _provider = provider;

        foreach (var item in OnHandleDisposable()) {
            _disposables.Add(item);
        }

        try {
            await OnInitializedAsync();
        }
        finally {
            IsInitialized = true;
        }
    }
}