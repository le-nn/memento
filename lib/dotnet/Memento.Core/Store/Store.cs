using Memento.Core.Internals;
using Memento.Core.Store.Internals;
using System.Net.Sockets;

namespace Memento.Core;

public abstract class Store<TState, TCommand>
    : IStore, IObservable<StateChangedEventArgs<TState, TCommand>>
    where TState : class
    where TCommand : Command {
    readonly readonly object _locker = new();

    private StoreProvider? Provider { get; set; }

    protected StateInitializer<TState> Initializer { get; }

    private Reducer<TState, TCommand> Reducer { get; }

    private List<IObserver<StateChangedEventArgs>> Observers { get; } = new();

    public TState State { get; internal set; }

    object IStore.State => State;

    public bool IsInitialized { get; private set; }

    public Store(
        StateInitializer<TState> initializer,
        Reducer<TState, TCommand> Reducer) {
        Initializer = initializer;
        State = initializer()
            ?? throw new ArgumentNullException("initializer must be returned not null.");

        this.Reducer = (state, command) => {
            return Reducer(state, command);
        };
    }

    public TStore ToStore<TStore>() where TStore : IStore {
        if (this is TStore t) {
            return t;
        }

        throw new InvalidCastException();
    }

    internal virtual (TState? NewState, StateChangedEventArgs<TState, TCommand>? Event) ComputeNewState(
        TState state,
        TCommand command
    ) {
        var previous = state;
        var postState = OnBeforeDispatch(previous, command);

        var middlewareProcessedState = GetMiddlewareInvokeHandler()(postState, command);
        if (middlewareProcessedState is TState s) {
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

    internal void ApplyComputedState(TState state, TCommand command) {
        var (newstate, e) = ComputeNewState(state, command);
        if (newstate is not null && e is not null) {
            lock (_locker) {
                State = newstate;
                InvokeObserver(e);
            }
        }
        else {
            throw new Exception("State is invalid.");
        }
    }

    protected virtual void Dispatch(TCommand command) {
        ApplyComputedState(State, command);
    }

    protected virtual void Dispatch(Func<TState, TCommand> messageLoader) {
        ApplyComputedState(State, messageLoader(State));
    }

    public IDisposable Subscribe(IObserver<StateChangedEventArgs<TState, TCommand>> observer) {
        var obs = new StoreObeserver(e => {
            if (e is StateChangedEventArgs<TState, TCommand> o) {
                observer.OnNext(o);
            }
        });

        lock (_locker) {
            Observers.Add(obs);
        }

        return new StoreSubscription($"Store.Subscribe", () => {
            lock (_locker) {
                Observers.Remove(obs);
            }
        });
    }

    IDisposable IObservable<StateChangedEventArgs>.Subscribe(IObserver<StateChangedEventArgs> observer) {
        lock (_locker) {
            Observers.Add(observer);
        }

        return new StoreSubscription($"Store.Subscribe", () => {
            lock (_locker) {
                Observers.Remove(observer);
            }
        });
    }

    public IDisposable Subscribe(Action<StateChangedEventArgs<TState, TCommand>> observer) {
        return Subscribe(new StoreObeserver<TState, TCommand>(observer));
    }

    void IStore.OnInitialized(StoreProvider provider) {
        Provider = provider;
        OnInitialized(provider);
    }

    internal Func<TState, TCommand, object?> GetMiddlewareInvokeHandler() {
        // process middlewares
        var middlewares = Provider?.ResolveAllMiddlewares()
            ?? Array.Empty<Middleware>();
        return middlewares.Aggregate(
            (object s, Command m) => {
                if ((s, m) is not (TState _s, TCommand _m)) {
                    throw new Exception();
                }

                return (object)Reducer.Invoke(_s, _m);
            },
            (before, middleware) =>
                (object s, Command m) => middleware.Handle(
                    s,
                    m,
                    (_s, _m) => before(_s, m)
                )
        );
    }

    protected virtual void OnInitialized(StoreProvider provider) {
        IsInitialized = true;
    }

    /// <summary>
    /// Called before invoke Reducer and return value overrides current state.
    /// </summary>
    /// <param name="state"> The current state.</param>
    /// <param name="command"> The command for mutating the state.</param>
    /// <returns> override state.</returns>
    protected TState OnBeforeDispatch(TState state, TCommand command) {
        return state;
    }

    /// <summary>
    /// Called before invoke Reducer and return value overrides current state.
    /// </summary>
    /// <param name="state"> The computed state via Reducer.</param>
    /// <param name="command"> The command for mutating the state.</param>
    /// <returns> override state.</returns>
    protected TState OnAfterDispatch(TState state, TCommand command) {
        return state;
    }

    internal void InvokeObserver(StateChangedEventArgs<TState, TCommand> e) {
        foreach (var obs in Observers) {
            obs.OnNext(e);
        }
    }

    internal void InvokeObserver(StateChangedEventArgs e) {
        foreach (var obs in Observers) {
            obs.OnNext(e);
        }
    }
}