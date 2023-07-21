using Memento.Core.Executors;

namespace Memento.Core.History;

public class HistoryManager {
    private int _maxHistoryCount = 8;
    private readonly FutureHistoryStack<IHistoryCommandItem<object>> _future = new();
    private readonly PastHistoryStack<IHistoryCommandItem<object>> _past = new();
    private readonly ConcatAsyncOperationExecutor _concatAsyncOperationExecutor = new();
    private IHistoryCommandItem<object>? _present;

    public IHistoryItem<object>? Present => _present;

    public IReadOnlyCollection<IHistoryItem<object>> FutureHistories => _future.CloneAsReadOnly();

    public IReadOnlyCollection<IHistoryItem<object>> PastHistories => _past.CloneAsReadOnly();

    public bool CanReDo => _future.GetCount() is not 0;

    public bool CanUnDo => _past.GetCount() is not 0 || Present is not null;

    public int MaxHistoryCount {
        get => _maxHistoryCount;
        set {
            _maxHistoryCount = value;
            ReduceIfPastHistoriesOverflow();
        }
    }

    public async ValueTask CommitAsync<T>(
        Func<ValueTask<T>> onDo,
        Func<T, ValueTask> onUnDo,
        string? name = null,
        Func<IHistoryItem<T?>, ValueTask>? saved = null,
        Func<IHistoryItem<T?>, ValueTask>? loaded = null,
        Action<IHistoryItem<T?>>? onDispose = null
    ) where T : class {
        await CommitAsync(
            new HistoryCommandContext<T>(
                onDo,
                onUnDo,
                name ?? Guid.NewGuid().ToString()
            ) {
                ContextLoaded = loaded,
                ContextSaved = saved,
                Disposed = onDispose,
            }
        );
    }

    public async ValueTask CommitAsync<T>(IHistoryCommandItem<T> command) where T : class {
        await _concatAsyncOperationExecutor.ExecuteAsync(async () => {
            if (CanReDo) {
                ClearFutureHistories();
            }

            if (_present is not null) {
                await _present.InvokeContextSavedAsync();
                _past.Push(_present);
            }

            await command.CommitAsync();

            _present = command;

            ReduceIfPastHistoriesOverflow();
        });
    }

    public async ValueTask<bool> ReDoAsync() {
        return await _concatAsyncOperationExecutor.ExecuteAsync(async () => {
            if (CanReDo is false) {
                return false;
            }

            if (_present is not null) {
                await _present.InvokeContextSavedAsync();
                _past.Push(_present);
            }

            var item = _future.Pop()!;
            await item.InvokeContextLoadedAsync();
            await item.CommitAsync();

            _present = item;

            return true;
        });
    }

    public async ValueTask<bool> UnDoAsync() {
        return await _concatAsyncOperationExecutor.ExecuteAsync(async () => {
            if (CanUnDo is false) {
                return false;
            }

            if (_present is not null) {
                await _present.RestoreAsync();
                await _present.InvokeContextSavedAsync();
                _future.Push(_present);
            }

            if (_past.GetCount() is not 0) {
                var item = _past.Pop()!;
                await item.InvokeContextLoadedAsync();
                _present = item;
            }
            else {
                _present = null;
            }

            return true;
        });
    }

    private void ClearFutureHistories() {
        var items = _future.CloneAsReadOnly();
        _future.Clear();

        foreach (var item in items) {
            item.Dispose();
        }
    }

    private void ReduceIfPastHistoriesOverflow() {
        lock (_past) {
            if (_past.GetCount() > MaxHistoryCount) {
                for (var i = 0; i < _past.GetCount() - MaxHistoryCount; i++) {
                    _past.RemoveLast()
                        ?.Dispose();
                }
            }
        }
    }
}