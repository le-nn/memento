﻿using Memento.Core.Internals;
using System.Collections.Immutable;

namespace Memento.Core;

public abstract class AbstractStore<TState, TCommand>
    : IStore, IObservable<StateChangedEventArgs<TState>>, IDisposable
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

    protected StateInitializer<TState> Initializer { get; }

    public TState State { get; internal set; }

    public bool IsInitialized { get; private set; }

    public StoreProvider Provider => _provider
        ?? throw new InvalidDataException("Store has not initialized.");

    /// <summary>
    /// Initializes a new instance of the <see cref="FluxStore{TState, TCommand}"/> class.
    /// </summary>
    /// <param name="initializer">An initializer that creates a initial state.</param>
    /// <param name="Reducer">An reducer that changes a store state.</param>
    /// <exception cref="ArgumentNullException"> Throws when <see cref="initializer"/> returns null. </exception>
    public AbstractStore(
        StateInitializer<TState> initializer,
        Reducer<TState, TCommand> Reducer
    ) {
        Initializer = initializer;
        State = initializer()
            ?? throw new ArgumentNullException("initializer must be returned not null.");

        _reducer = (state, command) => {
            return Reducer(state, command);
        };
    }

    public void Dispose() {
        foreach (var d in _disposables ?? ImmutableArray.Create<IDisposable>()) {
            d.Dispose();
        }

        OnDisposed();
    }

    protected virtual IEnumerable<IDisposable> OnHandleDisposable() {
        return Enumerable.Empty<IDisposable>();
    }

    public IDisposable Subscribe(Action<StateChangedEventArgs<TState>> observer) {
        return Subscribe(new GeneralObserver<StateChangedEventArgs<TState>>(observer));
    }

    public TStore AsStore<TStore>() where TStore : IStore {
        if (this is TStore store) {
            return store;
        }

        throw new InvalidCastException();
    }

    public IDisposable Subscribe(IObserver<StateChangedEventArgs<TState>> observer) {
        var obs = new StoreObserver(e => {
            if (e is StateChangedEventArgs<TState> o) {
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
        InvokeObserver(new StateChangedEventArgs<TState>() {
            State = State,
            LastState = State,
            Command = new Command.StateHasChanged(State),
            Sender = this,
        });
    }

    public Type GetStateType() {
        return typeof(TState);
    }

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
        var command = new Command.ForceReplaced(State);
        InvokeObserver(new StateChangedEventArgs<TState>() {
            Command = command,
            LastState = previous,
            Sender = this,
            State = State,
        });
    }

    protected virtual Task OnInitializedAsync(StoreProvider provider) {
        return Task.CompletedTask;
    }

    /// <summary>
    /// Called before invoke state changed and return value overrides current state.
    /// </summary>
    /// <param name="state"> The current state.</param>
    /// <param name="command"> The command for mutating the state.</param>
    /// <returns> override state.</returns>
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

    protected virtual void OnDisposed() {

    }

    internal void ComputedAndApplyState(TState state, TCommand command) {
        if (ComputeNewState() is ( { } s, { } e)) {
            lock (_locker) {
                State = s;
                InvokeObserver(e);
            }
        }

        (TState?, StateChangedEventArgs<TState>?) ComputeNewState() {
            var previous = state;
            var postState = OnBeforeDispatch(previous, command);

            if (MiddlewareHandler.Invoke(postState, command) is TState s) {
                var newState = OnAfterDispatch(s, command);
                var e = new StateChangedEventArgs<TState> {
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

    internal void InvokeObserver(StateChangedEventArgs<TState> e) {
        foreach (var obs in _observers) {
            obs.OnNext(e);
        }
    }
}