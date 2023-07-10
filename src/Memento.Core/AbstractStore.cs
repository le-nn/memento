using Memento.Core.Internals;
using System.Collections.Immutable;

namespace Memento.Core;

/// <summary>
/// Represents an abstract store that maintains state and handles commands.
/// Implements the IStore, IObservable, and IDisposable interfaces.
/// </summary>
/// <typeparam name="TState">The type of the state managed by the store.</typeparam>
/// <typeparam name="TCommand">The type of the commands used to mutate the state.</typeparam>
public abstract class AbstractStore<TState, TCommand>
    : IStore, IObservable<StateChangedEventArgs<TState, TCommand>>, IDisposable
        where TState : class
        where TCommand : Command {
    readonly List<IObserver<StateChangedEventArgs>> _observers = new();
    readonly object _locker = new();
    readonly Reducer<TState, TCommand> _reducer;

    StoreProvider? _provider;
    Func<TState, TCommand, object?>? _middlewareHandler;
    ImmutableArray<IDisposable>? _disposables;

    Func<object, Command, object> IStore.Reducer => (o, m) => _reducer((TState)o, (TCommand)m);

    Func<TState, TCommand, object?> MiddlewareHandler => _middlewareHandler ??= GetMiddlewareInvokeHandler();

    object IStore.State => State;

    /// <summary>
    /// Gets the state initializer for the store.
    /// </summary>
    protected StateInitializer<TState> Initializer { get; }

    /// <summary>
    /// Gets the current state of the store.
    /// </summary>
    public TState State { get; internal set; }

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

    /// <summary>
    /// Initializes a new instance of the <see cref="AbstractStore{TState, TCommand}"/> class.
    /// </summary>
    /// <param name="initializer">An initializer that creates an initial state.</param>
    /// <param name="reducer">A reducer that changes a store state.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="initializer"/> returns null.</exception>
    public AbstractStore(
        StateInitializer<TState> initializer,
        Reducer<TState, TCommand> reducer
    ) {
        Initializer = initializer;
        State = initializer()
            ?? throw new ArgumentNullException("initializer must be returned not null.");

        _reducer = (state, command) => {
            return reducer(state, command);
        };
    }

    /// <summary>
    /// Disposes the store and its resources.
    /// </summary>
    public void Dispose() {
        foreach (var d in _disposables ?? ImmutableArray.Create<IDisposable>()) {
            d.Dispose();
        }

        OnDisposed();
    }

    /// <summary>
    /// Handles disposable resources of the store.
    /// </summary>
    /// <returns>An enumerable of disposable resources.</returns>
    protected virtual IEnumerable<IDisposable> OnHandleDisposable() {
        return Enumerable.Empty<IDisposable>();
    }

    /// <summary>
    /// Subscribes to the store with the provided observer.
    /// </summary>
    /// <param name="observer">The observer to subscribe to the store.</param>
    /// <returns>An IDisposable instance that can be used to unsubscribe from the store.</returns>
    public IDisposable Subscribe(Action<StateChangedEventArgs<TState, TCommand>> observer) {
        return Subscribe(new GeneralObserver<StateChangedEventArgs<TState, TCommand>>(observer));
    }

    /// <summary>
    /// Casts the current store to the specified store type.
    /// </summary>
    /// <typeparam name="TStore">The store type to cast to.</typeparam>
    /// <returns>The current store cast to the specified store type.</returns>
    /// <exception cref="InvalidCastException">Thrown when the current store cannot be cast to the specified store type.</exception>
    public TStore AsStore<TStore>() where TStore : IStore {
        if (this is TStore store) {
            return store;
        }

        throw new InvalidCastException();
    }

    /// <summary>
    /// Subscribes to the store with the provided observer.
    /// </summary>
    /// <param name="observer">The observer to subscribe to the store.</param>
    /// <returns>An IDisposable instance that can be used to unsubscribe from the store.</returns>
    public IDisposable Subscribe(IObserver<StateChangedEventArgs<TState, TCommand>> observer) {
        var obs = new StoreObserver(e => {
            if (e is StateChangedEventArgs<TState, TCommand> o) {
                observer.OnNext(o);
            }
        });

        lock (_locker) {
            _observers.Add(obs);
        }

        return new StoreSubscription(GetType().FullName ?? "FluxStore.Subscribe", () => {
            lock (_locker) {
                _observers.Remove(obs);
            }
        });
    }

    /// <summary>
    /// Notifies that the state of the Store has changed.
    /// Usually, you don't need to call when you change the state
    /// via <see cref="FluxStore{TState, TCommand}.Dispatch"/> or <see cref="Store{TState}.Mutate"/>.
    /// </summary>
    public void StateHasChanged() {
        InvokeObserver(new StateChangedEventArgs<TState, TCommand>() {
            State = State,
            LastState = State,
            Command = null,
            StateChangeType = StateChangeType.StateHasChanged,
            Sender = this,
        });
    }

    /// <summary>
    /// Gets the type of the state managed by the store.
    /// </summary>
    /// <returns>The type of the state.</returns>
    public Type GetStateType() {
        return typeof(TState);
    }

    /// <summary>
    /// Gets the type of the command managed by the store.
    /// </summary>
    /// <returns>The type of the command.</returns>
    public Type GetCommandType() {
        return typeof(TCommand);
    }

    void IStore.SetStateForceSilently(object state) {
        if (state is not TState tState) {
            throw new InvalidDataException($"'{state.GetType().FullName}' is not compatible with '{typeof(TState).FullName}'.");
        }

        State = tState;
    }

    void IStore.SetStateForce(object state) {
        if (state is not TState tState) {
            throw new InvalidDataException($"'{state.GetType().FullName}' is not compatible with '{typeof(TState).FullName}'.");
        }

        var previous = State;
        State = tState;
        InvokeObserver(new StateChangedEventArgs<TState, TCommand>() {
            Command = null,
            StateChangeType = StateChangeType.ForceReplaced,
            LastState = previous,
            Sender = this,
            State = State,
        });
    }

    /// <summary>
    /// Called when the store is initialized asynchronously.
    /// </summary>
    /// <param name="provider">The store provider.</param>
    /// <returns>A Task representing the initialization process.</returns>
    protected virtual Task OnInitializedAsync(StoreProvider provider) {
        return Task.CompletedTask;
    }

    /// <summary>
    /// Called before dispatching a command and can be used to modify the state.
    /// </summary>
    /// <param name="state">The current state.</param>
    /// <param name="command">The command for mutating the state.</param>
    /// <returns>An overridden state.</returns>
    protected virtual TState OnBeforeDispatch(TState state, TCommand command) {
        return state;
    }

    /// <summary>
    /// Called before invoke state changed and return value overrides current state.
    /// </summary>
    /// <param name="state"> The computed state via _reducer.</param>
    /// <param name="command"> The command for mutating the state.</param>
    /// <returns> override state.</returns>
    protected virtual TState OnAfterDispatch(TState state, TCommand command) {
        return state;
    }


    /// <summary>
    /// Called when the store is disposed.
    /// </summary>
    protected virtual void OnDisposed() {

    }

    internal void ComputedAndApplyState(TState state, TCommand command) {
        if (ComputeNewState() is ( { } s, { } e)) {
            lock (_locker) {
                State = s;
                InvokeObserver(e);
            }
        }

        (TState?, StateChangedEventArgs<TState, TCommand>?) ComputeNewState() {
            var previous = state;
            var postState = OnBeforeDispatch(previous, command);

            if (MiddlewareHandler.Invoke(postState, command) is TState s) {
                var newState = OnAfterDispatch(s, command);
                var e = new StateChangedEventArgs<TState, TCommand> {
                    LastState = previous,
                    Command = command,
                    State = newState,
                    Sender = this,
                };

                return (newState, e);
            }

            return (null, null);
        }
    }

    IDisposable IObservable<StateChangedEventArgs>.Subscribe(IObserver<StateChangedEventArgs> observer) {
        lock (_locker) {
            _observers.Add(observer);
        }

        return new StoreSubscription(GetType().FullName ?? "FluxStore.Subscribe", () => {
            lock (_locker) {
                _observers.Remove(observer);
            }
        });
    }

    async Task IStore.InitializeAsync(StoreProvider provider) {
        _provider = provider;
        _disposables = OnHandleDisposable().ToImmutableArray();
        try {
            await OnInitializedAsync(provider);
        }
        catch {
            throw;
        }
        finally {
            IsInitialized = true;
        }
    }

    internal Func<TState?, TCommand, object?> GetMiddlewareInvokeHandler() {
        // process middleware
        var middleware = _provider?.GetAllMiddleware()
            ?? Array.Empty<Middleware>();
        return middleware.Aggregate(
            (object? s, Command m) => {
                if ((s, m) is (TState ss, TCommand cc)) {
                    return (object?)_reducer.Invoke(ss, cc);
                }

                return null;
            },
            (before, middleware) =>
                (object? s, Command m) => middleware.Handler.HandleStoreDispatch(
                    s,
                    m,
                    (_s, _m) => before(_s, m)
                )
        );
    }

    internal void InvokeObserver(StateChangedEventArgs<TState, TCommand> e) {
        foreach (var obs in _observers.ToArray()) {
            obs.OnNext(e);
        }
    }
}