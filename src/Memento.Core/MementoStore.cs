using Memento.Core.History;

namespace Memento.Core;

public record MementoStoreContext<TState>(TState State);

public abstract class MementoStore<TState, TMessage>
    : Store<TState, TMessage>
        where TState : class
        where TMessage : notnull {
    readonly HistoryManager _historyManager;

    public bool CanReDo => _historyManager.CanReDo;

    public bool CanUnDo => _historyManager.CanUnDo;

    public IMementoStateContext<TState>? Present => _historyManager.Present as IMementoStateContext<TState>;

    public IReadOnlyCollection<IMementoStateContext<MementoStoreContext<TState>>> FutureHistories => _historyManager
        .FutureHistories
        .Select(x => x as IMementoStateContext<MementoStoreContext<TState>>)
        .Where(x => x is not null)
        .Select(x => x!)
        .ToArray()
        .AsReadOnly();

    public IReadOnlyCollection<IMementoStateContext<MementoStoreContext<TState>>> PastHistories => _historyManager
        .PastHistories
        .Select(x => x as IMementoStateContext<MementoStoreContext<TState>>)
        .Where(x => x is not null)
        .Select(x => x!)
        .ToArray()
        .AsReadOnly();

    public MementoStore(
        StateInitializer<TState> initializer,
        HistoryManager historyManager
    ) : base(initializer) {
        _historyManager = historyManager;
    }

    public virtual ValueTask OnContextSavedAsync(IMementoStateContext<MementoStoreContext<TState>> command) {
        if (IsInitialized is false) {
            throw new Exception("Store is not initialized.");
        }

        return ValueTask.CompletedTask;
    }

    public virtual ValueTask OnContextLoadedAsync(IMementoStateContext<MementoStoreContext<TState>> command) {
        if (IsInitialized is false) {
            throw new Exception("Store is not initialized.");
        }

        return ValueTask.CompletedTask;
    }

    public virtual void OnContextDisposed(IMementoStateContext<MementoStoreContext<TState>> command) {
        if (IsInitialized is false) {
            throw new Exception("Store is not initialized.");
        }
    }

    public async ValueTask CommitAsync<T>(
        T payload,
        Func<T, ValueTask> onDo,
        Func<T, ValueTask> onUndo,
        string? name = null
    ) {
        if (IsInitialized is false) {
            throw new Exception("Store is not initialized.");
        }

        var data = payload;
        await _historyManager.CommitAsync(
            async () => {
                var state = State;
                await onDo(data);
                return new MementoStoreContext<TState>(state);
            },
            async state => {
                await onUndo(data);

                var lastState = State;
                State = state.State;
                InvokeObserver(new StateChangedEventArgs<TState, Command.StateHasChanged<TState, TMessage>> {
                    LastState = lastState,
                    State = State,
                    Command = null,
                    StateChangeType = StateChangeType.Restored,
                    Sender = this,
                });
            },
            name ?? Guid.NewGuid().ToString(),
            context => OnContextSavedAsync(context!),
            context => OnContextLoadedAsync(context!),
            context => OnContextDisposed(context!)
        );
    }

    public ValueTask CommitAsync(
        Func<ValueTask> onDo,
        Func<ValueTask> onUndo,
        string? name = null
    ) => CommitAsync(
        0,
        async _ => await onDo(),
        async _ => await onUndo(),
        name
    );

    public async ValueTask UnDoAsync() {
        if (IsInitialized is false) {
            throw new Exception("Store is not initialized.");
        }

        await _historyManager.UnDoAsync();
    }

    public async ValueTask ReDoAsync() {
        if (IsInitialized is false) {
            throw new Exception("Store is not initialized.");
        }

        await _historyManager.ReDoAsync();
<<<<<<< Updated upstream
    }
}

public class MementoStore<TState> : MementoStore<TState, string>
    where TState : class {
    public MementoStore(StateInitializer<TState> initializer, HistoryManager historyManager) : base(initializer, historyManager) {
=======
>>>>>>> Stashed changes
    }
}