using Memento.Core.Internals;
using Memento.Core.Store.Internals;

namespace Memento.Core;

public abstract class Store<TState, TCommand>
    : IStore, IObservable<StateChangedEventArgs<TState>>
    where TState : class
    where TCommand : Command {
    StoreProvider? _provider;

    readonly List<IObserver<StateChangedEventArgs>> _observers = new();
    readonly object _locker = new();
    readonly Reducer<TState, TCommand> _reducer;

    Func<object, Command, object> IStore.Reducer => ((o, m) => _reducer((TState)o, (TCommand)m));

    object IStore.State => State;

    protected StateInitializer<TState> Initializer { get; }

    public TState State { get; internal set; }

    public bool IsInitialized { get; private set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="Store{TState, TCommand}"/> class.
    /// </summary>
    /// <param name="initializer">An initializer that creates a initial state.</param>
    /// <param name="Reducer">An reducer that changes a store state.</param>
    /// <exception cref="ArgumentNullException"> Throws when <see cref="initializer"/> returns null. </exception>
    public Store(
        StateInitializer<TState> initializer,
        Reducer<TState, TCommand> Reducer) {
        Initializer = initializer;
        State = initializer()
            ?? throw new ArgumentNullException("initializer must be returned not null.");

        _reducer = (state, command) => {
            return Reducer(state, command);
        };
    }

    public TStore ToStore<TStore>() where TStore : IStore {
        if (this is TStore t) {
            return t;
        }

        throw new InvalidCastException();
    }

    internal virtual (TState? NewState, StateChangedEventArgs<TState>? Event) ComputeNewState(
        TState state,
        TCommand command
    ) {
        var previous = state;
        var postState = OnBeforeDispatch(previous, command);

        var middlewareProcessedState = GetMiddlewareInvokeHandler()(postState, command);
        if (middlewareProcessedState is TState s) {
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

    internal void ApplyComputedState(TState state, TCommand command) {
        var (newState, e) = ComputeNewState(state, command);
        if (newState is not null && e is not null) {
            lock (_locker) {
                State = newState;
                InvokeObserver(e);
            }
        }
        else {
            throw new Exception("State is invalid.");
        }
    }

    public virtual void Dispatch(TCommand command) {
        ApplyComputedState(State, command);
    }

    public virtual void Dispatch(Func<TState, TCommand> messageLoader) {
        ApplyComputedState(State, messageLoader(State));
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

        return new StoreSubscription(GetType().FullName ?? "Store.Subscribe", () => {
            lock (_locker) {
                _observers.Remove(obs);
            }
        });
    }

    IDisposable IObservable<StateChangedEventArgs>.Subscribe(IObserver<StateChangedEventArgs> observer) {
        lock (_locker) {
            _observers.Add(observer);
        }

        return new StoreSubscription(GetType().FullName ?? "Store.Subscribe", () => {
            lock (_locker) {
                _observers.Remove(observer);
            }
        });
    }

    public IDisposable Subscribe(Action<StateChangedEventArgs<TState>> observer) {
        return Subscribe(new GeneralObserver<StateChangedEventArgs<TState>>(observer));
    }

    async Task IStore.OnInitializedAsync(StoreProvider provider) {
        _provider = provider;
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

    internal Func<TState, TCommand, object?> GetMiddlewareInvokeHandler() {
        // process middleware
        var middleware = _provider?.GetAllMiddleware()
            ?? Array.Empty<Middleware>();
        return middleware.Aggregate(
            (object s, Command m) => {
                if ((s, m) is not (TState _s, TCommand _m)) {
                    throw new Exception();
                }

                return (object)_reducer.Invoke(_s, _m);
            },
            (before, middleware) =>
                (object s, Command m) => middleware.Handler.HandleStoreDispatch(
                    s,
                    m,
                    (_s, _m) => before(_s, m)
                )
        );
    }

    protected virtual Task OnInitializedAsync(StoreProvider provider) {
        return Task.CompletedTask;
    }

    /// <summary>
    /// Called before invoke _reducer and return value overrides current state.
    /// </summary>
    /// <param name="state"> The current state.</param>
    /// <param name="command"> The command for mutating the state.</param>
    /// <returns> override state.</returns>
    protected virtual TState OnBeforeDispatch(TState state, TCommand command) {
        return state;
    }

    /// <summary>
    /// Called before invoke _reducer and return value overrides current state.
    /// </summary>
    /// <param name="state"> The computed state via _reducer.</param>
    /// <param name="command"> The command for mutating the state.</param>
    /// <returns> override state.</returns>
    protected virtual TState OnAfterDispatch(TState state, TCommand command) {
        return state;
    }

    internal void InvokeObserver(StateChangedEventArgs<TState> e) {
        foreach (var obs in _observers) {
            obs.OnNext(e);
        }
    }

    internal void InvokeObserver(StateChangedEventArgs e) {
        foreach (var obs in _observers) {
            obs.OnNext(e);
        }
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
        var command = new Command.ForceReplaced(state);
        InvokeObserver(new StateChangedEventArgs<TState>() {
            Command = command,
            LastState = previous,
            Sender = this,
            State = tState,
        });
    }

    public Type GetStateType() {
        return typeof(TState);
    }

    public Type GetCommandType() {
        return typeof(TCommand);
    }
}