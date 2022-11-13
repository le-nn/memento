using Memento.Core.Executors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Memento.Core;

public class HistoryManager {
    private int maxHistoryCount = 8;
    private readonly FutureHistoryStack<IMementoCommandContext> Future = new();
    private readonly PastHistoryStack<IMementoCommandContext> Past = new();
    private readonly ConcatAsyncOperationExecutor ConcatAsyncOperationExecutor = new();

    public IMementoCommandContext? Present { get; private set; }

    public IReadOnlyCollection<IMementoStateContext> FutureHistories => Future.AsReadOnly();

    public IReadOnlyCollection<IMementoStateContext> PastHistories => Past.AsReadOnly();

    public bool CanReDo => Future.Count is not 0;

    public bool CanUnDo => Past.Count is not 0 || Present is not null;

    public int MaxHistoryCount {
        get => maxHistoryCount;
        set {
            maxHistoryCount = value;
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
        await ConcatAsyncOperationExecutor.ExecuteAsync(async () => {
            if (CanReDo) {
                ClearFutureHistoriesAsync();
            }

            if (Present is not null) {
                await Present.InvokeContextSavedAsync();
                Past.Push(Present);
            }

            await command.CommitAsync();
            await command.LoadDataAsync();

            Present = command;

            ReduceIfPastHistoriesOverflow();
        });
    }

    public async ValueTask<bool> ReExecuteAsync() {
        return await ConcatAsyncOperationExecutor.ExecuteAsync(async () => {
            if (CanReDo is false) {
                return false;
            }

            if (Present is not null) {
                await Present.InvokeContextSavedAsync();
                Past.Push(Present);
            }

            var item = Future.Pop()!;
            await item.InvokeContextLoadedAsync();
            await item.CommitAsync();
            await item.LoadDataAsync();
            Present = item;

            return true;
        });
    }

    public async ValueTask<bool> UnExecuteAsync() {
        return await ConcatAsyncOperationExecutor.ExecuteAsync(async () => {
            if (CanUnDo is false) {
                return false;
            }

            if (Present is not null) {
                await Present.RestoreAsync();
                await Present.InvokeContextSavedAsync();
                Future.Push(Present);
            }

            if (Past.Count is not 0) {
                var item = Past.Pop()!;
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
        var items = Future.ToArray();
        Future.Clear();
        foreach (var item in items) {
            item.Dispose();
        }
    }

    private void ReduceIfPastHistoriesOverflow() {
        if (Past.Count > MaxHistoryCount) {
            for (int i = 0; i < Past.Count - MaxHistoryCount; i++) {
                Past.RemoveLast()
                    ?.Dispose();
            }
        }
    }
}
