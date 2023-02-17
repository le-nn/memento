using Memento.Core.Executors;

namespace Memento.Core;

public class HistoryManager {
    private int _maxHistoryCount = 8;
    private readonly FutureHistoryStack<IMementoCommandContext> _future = new();
    private readonly PastHistoryStack<IMementoCommandContext> _past = new();
    private readonly ConcatAsyncOperationExecutor _concatAsyncOperationExecutor = new();

    public IMementoCommandContext? Present { get; private set; }

    public IReadOnlyCollection<IMementoStateContext> FutureHistories => _future.AsReadOnly();

    public IReadOnlyCollection<IMementoStateContext> PastHistories => _past.AsReadOnly();

    public bool CanReDo => _future.Count is not 0;

    public bool CanUnDo => _past.Count is not 0 || Present is not null;

    public int MaxHistoryCount {
        get => _maxHistoryCount;
        set {
            _maxHistoryCount = value;
            ReduceIfPastHistoriesOverflow();
        }
    }

    public async ValueTask ExcuteCommitAsync<T>(
        Func<ValueTask<T>> execute,
        Func<T, ValueTask> unexecute,
        Func<T, ValueTask> dataloader,
        string? name = null,
        Func<IMementoStateContext<T?>, ValueTask>? saved = null,
        Func<IMementoStateContext<T?>, ValueTask>? loaded = null,
        Action<IMementoStateContext<T?>>? onDispose = null
    ) {
        await ExcuteAsync(
            new MementoCommandContext<T>(
                dataloader,
                execute,
                unexecute,
                name ?? Guid.NewGuid().ToString()
            ) {
                ContextLoaded = loaded,
                ContextSaved = saved,
                Disposed = onDispose,
            }
        );
    }

    public async ValueTask ExcuteAsync<T>(IMementoCommandContext<T> command) {
        await _concatAsyncOperationExecutor.ExecuteAsync(async () => {
            if (CanReDo) {
                ClearFutureHistoriesAsync();
            }

            if (Present is not null) {
                await Present.InvokeContextSavedAsync();
                _past.Push(Present);
            }

            await command.CommitAsync();
            await command.LoadDataAsync();

            Present = command;

            ReduceIfPastHistoriesOverflow();
        });
    }

    public async ValueTask<bool> ReExecuteAsync() {
        return await _concatAsyncOperationExecutor.ExecuteAsync(async () => {
            if (CanReDo is false) {
                return false;
            }

            if (Present is not null) {
                await Present.InvokeContextSavedAsync();
                _past.Push(Present);
            }

            var item = _future.Pop()!;
            await item.InvokeContextLoadedAsync();
            await item.CommitAsync();
            await item.LoadDataAsync();
            Present = item;

            return true;
        });
    }

    public async ValueTask<bool> UnExecuteAsync() {
        return await _concatAsyncOperationExecutor.ExecuteAsync(async () => {
            if (CanUnDo is false) {
                return false;
            }

            if (Present is not null) {
                await Present.RestoreAsync();
                await Present.InvokeContextSavedAsync();
                _future.Push(Present);
            }

            if (_past.Count is not 0) {
                var item = _past.Pop()!;
                await item.InvokeContextLoadedAsync();
                await item.LoadDataAsync();
                Present = item;
            }
            else {
                Present = null;
            }

            return true;
        });
    }

    private void ClearFutureHistoriesAsync() {
        var items = _future.ToArray();
        _future.Clear();
        foreach (var item in items) {
            item.Dispose();
        }
    }

    private void ReduceIfPastHistoriesOverflow() {
        if (_past.Count > MaxHistoryCount) {
            for (var i = 0; i < _past.Count - MaxHistoryCount; i++) {
                _past.RemoveLast()
                    ?.Dispose();
            }
        }
    }
}