using Memento.Core.Executors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Memento;

public class HistoryManager {
    private int maxHistoryCount = 8;
    private FutureHistoryStack<IMementoCommandContext> Future = new();
    private PastHistoryStack<IMementoCommandContext> Past = new();
    SortedAsyncOperationExecutor SortedOperationExecutor { get; } = new();

    public IMementoCommandContext? Present { get; private set; }

    public IReadOnlyCollection<IMementoStateContext> FutureHistories => this.Future.AsReadOnly();

    public IReadOnlyCollection<IMementoStateContext> PastHistories => this.Past.AsReadOnly();

    public bool CanReDo => this.Future.Count is not 0;

    public bool CanUnDo => this.Past.Count is not 0 || this.Present is not null;

    public int MaxHistoryCount {
        get => maxHistoryCount;
        set {
            this.maxHistoryCount = value;
            this.ReduceIfPastHistoriesOverflow();
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
        await this.ExcuteAsync(
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
        await this.SortedOperationExecutor.ExecuteAsync(async () => {
            if (this.CanReDo) {
                this.ClearFutureHistoriesAsync();
            }

            if (this.Present is not null) {
                await this.Present.InvokeContextSavedAsync();
                this.Past.Push(this.Present);
            }

            await command.CommitAsync();
            await command.LoadDataAsync();

            this.Present = command;

            this.ReduceIfPastHistoriesOverflow();
        });
    }

    public async ValueTask<bool> ReExecuteAsync() {
        return await this.SortedOperationExecutor.ExecuteAsync(async () => {
            if (this.CanReDo is false) {
                return false;
            }

            if (this.Present is not null) {
                await this.Present.InvokeContextSavedAsync();
                this.Past.Push(this.Present);
            }

            var item = this.Future.Pop()!;
            await item.InvokeContextLoadedAsync();
            await item.CommitAsync();
            await item.LoadDataAsync();
            this.Present = item;

            return true;
        });
    }

    public async ValueTask<bool> UnExecuteAsync() {
        return await this.SortedOperationExecutor.ExecuteAsync(async () => {
            if (this.CanUnDo is false) {
                return false;
            }

            if (this.Present is not null) {
                await this.Present.RestoreAsync();
                await this.Present.InvokeContextSavedAsync();
                this.Future.Push(this.Present);
            }

            if (this.Past.Count is not 0) {
                var item = this.Past.Pop()!;
                await item.InvokeContextLoadedAsync();
                await item.LoadDataAsync();
                this.Present = item;
            }
            else {
                this.Present = null;
            }

            return true;
        });
    }

    private void ClearFutureHistoriesAsync() {
        var items = this.Future.ToArray();
        this.Future.Clear();
        foreach (var item in items) {
            item.Dispose();
        }
    }

    private void ReduceIfPastHistoriesOverflow() {
        if (this.Past.Count > this.MaxHistoryCount) {
            for (int i = 0; i < this.Past.Count - this.MaxHistoryCount; i++) {
                this.Past.RemoveLast()
                    ?.Dispose();
            }
        }
    }
}
