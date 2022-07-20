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

    public IMementoCommandContext? Present { get; private set; }

    public IReadOnlyCollection<IMementoStateContext> FutureHistories => this.Future.AsReadOnly();

    public IReadOnlyCollection<IMementoStateContext> PastHistories => this.Past.AsReadOnly();

    public bool CanReDo => this.Future.Count is not 0;

    public bool CanUnDo => this.Past.Count is not 0;


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
        string? name = null,
        Func<IMementoStateContext<T?>, ValueTask>? saved = null,
        Func<IMementoStateContext<T?>, ValueTask>? loaded = null,
        Action<IMementoStateContext<T?>>? onDispose = null
    ) where T : class {
        await this.ExcuteAsync(
            new MementoCommandContext<T>(
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
        if (this.CanReDo) {
            this.ClearFutureHistoriesAsync();
        }

        if (this.Present is not null) {
            await this.Present.InvokeContextSavedAsync();
            this.Past.Push(this.Present);
        }

        await command.CommitAsync();
        this.Present = command;

        this.ReduceIfPastHistoriesOverflow();
    }

    public async ValueTask<bool> ReExecuteAsync() {
        if (this.CanReDo is false || this.Future.Count <= 0) {
            return false;
        }

        if (this.Present is not null) {
            await this.Present.InvokeContextSavedAsync();
            this.Past.Push(this.Present);
        }

        var item = this.Future.Pop()!;
        await item.InvokeContextLoadedAsync();
        await item.CommitAsync();
        this.Present = item;

        return true;
    }

    public async ValueTask<bool> UnExecuteAsync() {
        if (this.CanUnDo is false || this.Past.Count <= 0) {
            return false;
        }

        if (this.Present is not null) {
            await this.Present.InvokeContextSavedAsync();
            this.Future.Push(this.Present);
        }

        var item = this.Past.Pop()!;
        await item.InvokeContextLoadedAsync();
        await item.RestoreAsync();
        this.Present = item;

        return true;

    }

    private void ClearFutureHistoriesAsync() {
        foreach (var item in this.Future) {
            item.Dispose();
        }

        this.Future.Clear();
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
