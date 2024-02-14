using Memento.Core.History;

namespace Memento.Core;

/// <summary>
/// Represents the context of a memento store.
/// </summary>
/// <typeparam name="TState">The type of the state.</typeparam>
public interface IMementoStoreContext<TState> {
    /// <summary>
    /// Gets the state.
    /// </summary>
    TState State { get; }

    /// <summary>
    /// Gets the payload.
    /// </summary>
    object Payload { get; }
}

/// <summary>
/// Represents the context of a memento store with payload.
/// </summary>
/// <typeparam name="TState">The type of the state.</typeparam>
/// <typeparam name="TPayload">The type of the payload.</typeparam>
public record MementoStoreContext<TState, TPayload> : IMementoStoreContext<TState> where TPayload : notnull {
    /// <summary>
    /// Gets the payload.
    /// </summary>
    public TPayload Payload { get; }

    /// <summary>
    /// Gets the state.
    /// </summary>
    public TState State { get; }

    object IMementoStoreContext<TState>.Payload => Payload;

    /// <summary>
    /// Initializes a new instance of the <see cref="MementoStoreContext{TState, TPayload}"/> class.
    /// </summary>
    /// <param name="state">The state.</param>
    /// <param name="payload">The payload.</param>
    public MementoStoreContext(TState state, TPayload payload) {
        State = state;
        Payload = payload;
    }
}

public abstract class AbstractMementoStore<TState, TMessage>(
    Func<TState> initializer,
    HistoryManager historyManager,
    Reducer<TState, TMessage> reducer
    )
    : AbstractStore<TState, TMessage>(initializer, reducer)
      where TState : class
      where TMessage : class {
    readonly HistoryManager _historyManager = historyManager;

    public bool CanReDo => _historyManager.CanReDo;

    public bool CanUnDo => _historyManager.CanUnDo;

    public IHistoryItem<IMementoStoreContext<TState>>? Present => (IHistoryItem<IMementoStoreContext<TState>>?)_historyManager.Present;

    public IReadOnlyList<IHistoryItem<IMementoStoreContext<TState>>> FutureHistories => _historyManager
        .FutureHistories
        .Select(x => (IHistoryItem<IMementoStoreContext<TState>>)x)
        .ToArray();

    public IReadOnlyList<IHistoryItem<IMementoStoreContext<TState>>> PastHistories => _historyManager
        .PastHistories
        .Select(x => (IHistoryItem<IMementoStoreContext<TState>>)x)
        .ToArray();

    public virtual ValueTask OnContextSavedAsync(IHistoryItem<IMementoStoreContext<TState>> command) {
        if (IsInitialized is false) {
            throw new Exception("Store is not initialized.");
        }

        return ValueTask.CompletedTask;
    }

    public virtual ValueTask OnContextLoadedAsync(IHistoryItem<IMementoStoreContext<TState>> command) {
        if (IsInitialized is false) {
            throw new Exception("Store is not initialized.");
        }

        return ValueTask.CompletedTask;
    }

    public virtual void OnContextDisposed(IHistoryItem<IMementoStoreContext<TState>> command) {
        if (IsInitialized is false) {
            throw new Exception("Store is not initialized.");
        }
    }

    public async ValueTask CommitAsync<TPayload>(
        Func<ValueTask<TPayload>> onDo,
        Func<MementoStoreContext<TState, TPayload>, ValueTask> onUndo,
        string? name = null
    ) where TPayload : notnull {
        if (IsInitialized is false) {
            throw new Exception("Store is not initialized.");
        }

        await _historyManager.CommitAsync(
            async () => {
                var state = State;
                var payload = await onDo.Invoke();
                return new MementoStoreContext<TState, TPayload>(state, payload);
            },
            async context => {
                State = context.State;
                StateHasChanged();
                await onUndo.Invoke(context);
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
    ) => CommitAsync<byte>(
        async () => {
            await onDo();
            return 0;
        },
        async _ => {
            await onUndo();
        },
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
    }
}