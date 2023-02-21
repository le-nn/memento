namespace Memento.Core;

public record Context<TState, TMessage>(TState State, TMessage Message);

public abstract class FluxMementoStore<TState, TCommand>
    : FluxStore<TState, TCommand>
        where TState : class
        where TCommand : Command, new() {
    readonly HistoryManager _historyManager;

    public bool CanReDo => _historyManager.CanReDo;

    public bool CanUnDo => _historyManager.CanUnDo;

    public IMementoStateContext<TState>? Present => _historyManager.Present as IMementoStateContext<TState>;

    public IReadOnlyCollection<IMementoStateContext<Context<TState, TCommand>>> FutureHistories => _historyManager
        .FutureHistories
        .Select(x => x as IMementoStateContext<Context<TState, TCommand>>)
        .Where(x => x is not null)
        .Select(x => x!)
        .ToList()
        .AsReadOnly();

    public IReadOnlyCollection<IMementoStateContext<Context<TState, TCommand>>> PastHistories => _historyManager
        .PastHistories
        .Select(x => x as IMementoStateContext<Context<TState, TCommand>>)
        .Where(x => x is not null)
        .Select(x => x!)
        .ToList()
        .AsReadOnly();

    public FluxMementoStore(
        StateInitializer<TState> initializer,
        Reducer<TState, TCommand> Reducer,
        HistoryManager historyManager
    ) : base(
        initializer,
        (state, command) => command switch {
            TCommand => Reducer(state, command),
            _ => state,
        }
    ) {
        _historyManager = historyManager;
    }

    public virtual ValueTask OnContextSavedAsync(IMementoStateContext<Context<TState, TCommand>> command) {
        if (IsInitialized is false) {
            throw new Exception("Store is not initialized.");
        }

        return ValueTask.CompletedTask;
    }

    public virtual ValueTask OnContextLoadedAsync(IMementoStateContext<Context<TState, TCommand>> command) {
        if (IsInitialized is false) {
            throw new Exception("Store is not initialized.");
        }

        return ValueTask.CompletedTask;
    }

    public virtual void OnContextDisposed(IMementoStateContext<Context<TState, TCommand>> command) {
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
        await _historyManager.ExcuteCommitAsync(
            async () => {
                var state = State;
                await onExecuted(data);
                return new Context<TState, TCommand>(state, new TCommand());
            },
            async state => {
                await onUnexecuted(data);

                var lastState = State;
                State = state.State;
                InvokeObserver(new StateChangedEventArgs<TState> {
                    LastState = lastState,
                    State = State,
                    Command = new Command.Restored(),
                    Sender = this,
                });
            },
            state => {
                //this.ComputedAndApplyState(state.State, state.Command);
                return ValueTask.CompletedTask;
            },
            name ?? Guid.NewGuid().ToString(),
            context => OnContextSavedAsync(context!),
            context => OnContextLoadedAsync(context!),
            context => OnContextDisposed(context!)
        );
    }

    public ValueTask CommitAsync(
        Func<ValueTask> onExecuted,
        Func<ValueTask> onUnexecuted,
        string? name = null
    ) => CommitAsync(
        () => ValueTask.FromResult(0),
        async _ => await onExecuted(),
        async _ => await onUnexecuted(),
        name
    );

    public async ValueTask CommitAsync(TCommand command, string? name = null) {
        if (IsInitialized is false) {
            throw new Exception("Store is not initialized.");
        }

        await CommitAsync(
            () => ValueTask.FromResult(command),
            _ => ValueTask.CompletedTask,
            _ => ValueTask.CompletedTask,
            name
        );
    }

    public async ValueTask UnExecuteAsync() {
        if (IsInitialized is false) {
            throw new Exception("Store is not initialized.");
        }

        await _historyManager.UnExecuteAsync();
    }

    public async ValueTask ReExecuteAsync() {
        if (IsInitialized is false) {
            throw new Exception("Store is not initialized.");
        }

        await _historyManager.ReExecuteAsync();
    }
}