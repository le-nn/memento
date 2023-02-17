namespace Memento.Core;

public record Context<TState, TMessage>(TState State, TMessage Message);

public abstract class MementoStore<TState, TMessages>
    : Store<TState, TMessages>
        where TState : class
        where TMessages : Command, new() {
    HistoryManager HistoryManager { get; }

    public bool CanReDo => HistoryManager.CanReDo;

    public bool CanUnDo => HistoryManager.CanUnDo;

    public IMementoStateContext<TState>? Present => HistoryManager.Present as IMementoStateContext<TState>;

    public IReadOnlyCollection<IMementoStateContext<Context<TState, TMessages>>> FutureHistories => HistoryManager
        .FutureHistories
        .Select(x => x as IMementoStateContext<Context<TState, TMessages>>)
        .Where(x => x is not null)
        .Select(x => x!)
        .ToList()
        .AsReadOnly();

    public IReadOnlyCollection<IMementoStateContext<Context<TState, TMessages>>> PastHistories => HistoryManager
        .PastHistories
        .Select(x => x as IMementoStateContext<Context<TState, TMessages>>)
        .Where(x => x is not null)
        .Select(x => x!)
        .ToList()
        .AsReadOnly();

    public MementoStore(
        StateInitializer<TState> initializer,
        Reducer<TState, TMessages> Reducer,
        HistoryManager historyManager
    ) : base(
        initializer,
        (state, command) => command switch {
            TMessages => Reducer(state, command),
            _ => state,
        }
    ) {
        HistoryManager = historyManager;
    }

    public virtual ValueTask OnContextSavedAsync(IMementoStateContext<Context<TState, TMessages>> command) {
        if (IsInitialized is false) {
            throw new Exception("Store is not initialized.");
        }

        return ValueTask.CompletedTask;
    }

    public virtual ValueTask OnContextLoadedAsync(IMementoStateContext<Context<TState, TMessages>> command) {
        if (IsInitialized is false) {
            throw new Exception("Store is not initialized.");
        }

        return ValueTask.CompletedTask;
    }

    public virtual void OnContextDisposed(IMementoStateContext<Context<TState, TMessages>> command) {
        if (IsInitialized is false) {
            throw new Exception("Store is not initialized.");
        }
    }

    public async ValueTask CommitAsync<T>(
        Func<ValueTask<T>> dataCreator,
        Func<T, ValueTask> onExecuted,
        Func<T, ValueTask> onUnexecuted,
        string? name = null
    ) {
        if (IsInitialized is false) {
            throw new Exception("Store is not initialized.");
        }

        var data = await dataCreator();
        await HistoryManager.ExcuteCommitAsync(
            async () => {
                var state = State;
                await onExecuted(data);
                return new Context<TState, TMessages>(state, new TMessages());
            },
            async state => {
                await onUnexecuted(data);

                var lastState = State;
                State = state.State;
                InvokeObserver(new StateChangedEventArgs {
                    LastState = lastState,
                    State = State,
                    Command = new Command.Restores(),
                    Sender = this,
                });
            },
            state => {
                //this.ApplyComputedState(state.State, state.Command);
                return ValueTask.CompletedTask;
            },
            name ?? Guid.NewGuid().ToString(),
            context => OnContextSavedAsync(context!),
            context => OnContextLoadedAsync(context!),
            context => OnContextDisposed(context!)
        );
    }

    public async ValueTask CommitAsync(
        Func<ValueTask> onExecuted,
        Func<ValueTask> onUnexecuted,
        string? name = null
    ) {
        await CommitAsync(
             () => {
                 return ValueTask.FromResult((byte)0);
             },
            async _ => {
                await onExecuted();
            },
            async _ => {
                await onUnexecuted();
            },
            name
        );
    }

    public async ValueTask CommitAsync(TMessages command, string? name = null) {
        if (IsInitialized is false) {
            throw new Exception("Store is not initialized.");
        }

        await CommitAsync(
            () => ValueTask.FromResult(command),
            s => ValueTask.CompletedTask,
            s => ValueTask.CompletedTask,
            name
        );
    }

    public async ValueTask UnExecuteAsync() {
        if (IsInitialized is false) {
            throw new Exception("Store is not initialized.");
        }

        await HistoryManager.UnExecuteAsync();
    }

    public async ValueTask ReExecuteAsync() {
        if (IsInitialized is false) {
            throw new Exception("Store is not initialized.");
        }

        await HistoryManager.ReExecuteAsync();
    }
}