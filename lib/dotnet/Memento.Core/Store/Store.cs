namespace Memento;

public abstract class Store<TState, TMessages>
    : IStore, IObservable<StateChangedEventArgs<TState, TMessages>>
    where TState : class
    where TMessages : Message {
    protected StateInitializer<TState> Initializer { get; }

    private Mutation<TState, TMessages> Mutation { get; }

    private List<IObserver<StateChangedEventArgs>> Observers { get; } = new();

    public TState State { get; private set; }

    object IStore.State => this.State;

    private StoreProvider? Provider { get; set; }
    object locker = new();


    public Store(
        StateInitializer<TState> initializer,
        Mutation<TState, TMessages> mutation) {
        this.Initializer = initializer;
        this.State = initializer()
            ?? throw new ArgumentNullException("initializer must be returned not null.");

        this.Mutation = (state, message) => {
            return mutation(state, message);
        };
    }

    public TStore ToStore<TStore>() {
        throw new InvalidCastException();
    }

    TStore IStore.ToStore<TStore>() {
        if (this is TStore t) {
            return t;
        }

        throw new InvalidCastException();
    }

    protected void Mutate(TMessages message) {
        var previous = this.State;
        var postState = this.OnBeforeMutate(this.State, message);


        // process middlewares
        var middlewares = this.Provider?.ResolveAllMiddlewares()
            ?? Array.Empty<Middleware>();

        var middlewareProcessedState = middlewares.Aggregate(
            (object s, Message m) => {
                if ((s, m) is not (TState _s, TMessages _m)) {
                    throw new Exception();
                }

                return (object)this.Mutation.Invoke(_s, _m);
            },
            (before, middleware) => (object s, Message m) => middleware.Handle(s, m, (_s, _m) => before(_s, m))
        )(postState, message);

        if (middlewareProcessedState is TState s) {
            this.State = s;
            this.State = this.OnAfterMutate(s, message);

            var e = new StateChangedEventArgs<TState, TMessages> {
                LastState = previous,
                Message = message,
                State = this.State,
                Sender = this,
            };

            this.InvokeObserver(e);
        }
        else {
            throw new Exception("State is invalid.");
        }
    }

    public IDisposable Subscribe(IObserver<StateChangedEventArgs<TState, TMessages>> observer) {
        var obs = new StoreObeserver(e => {
            if (e is StateChangedEventArgs<TState, TMessages> o) {
                observer.OnNext(o);
            }
        });

        lock (this.locker) {
            this.Observers.Add(obs);
        }

        return new StoreSubscription($"Store.Subscribe", () => {
            lock (this.locker) {
                this.Observers.Remove(obs);
            }
        });
    }

    IDisposable IObservable<StateChangedEventArgs>.Subscribe(IObserver<StateChangedEventArgs> observer) {
        lock (this.locker) {
            this.Observers.Add(observer);
        }

        return new StoreSubscription($"Store.Subscribe", () => {
            lock (this.locker) {
                this.Observers.Remove(observer);
            }
        });
    }

    public IDisposable Subscribe(Action<StateChangedEventArgs<TState, TMessages>> observer) {
        return this.Subscribe(new StoreObeserver<TState, TMessages>(observer));
    }

    void IStore.OnInitialized(StoreProvider provider) {
        this.Provider = provider;
        this.OnInitialized(provider);
    }

    protected virtual void OnInitialized(StoreProvider provider) {

    }

    /// <summary>
    /// Called before invoke mutation and return value overrides current state.
    /// </summary>
    /// <param name="state"> The current state.</param>
    /// <param name="message"> The message for mutating the state.</param>
    /// <returns> override state.</returns>
    protected TState OnBeforeMutate(TState state, TMessages message) {
        return state;
    }

    /// <summary>
    /// Called before invoke mutation and return value overrides current state.
    /// </summary>
    /// <param name="state"> The computed state via mutation.</param>
    /// <param name="message"> The message for mutating the state.</param>
    /// <returns> override state.</returns>
    protected TState OnAfterMutate(TState state, TMessages message) {
        return state;
    }

    private void InvokeObserver(StateChangedEventArgs<TState, TMessages> e) {
        foreach (var observer in this.Observers) {
            observer.OnNext(e);
        }
    }
}
