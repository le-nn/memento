using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Memento;

public class HistoryManager {
    private int maxHistoryCount = 8;
    private FutureHistoryStack<IMementoCommand> Future = new();
    private PastHistoryStack<IMementoCommand> Past = new();

    public IMementoCommand? Present { get; private set; }

    public IReadOnlyCollection<IMementoState> FutureHistories => this.Future.AsReadOnly();

    public IReadOnlyCollection<IMementoState> PastHistories => this.Past.AsReadOnly();

    public bool CanReDo => this.Future.Count is not 0;

    public bool CanUnDo => this.Past.Count is not 0;


    public int MaxHistoryCount {
        get => maxHistoryCount;
        set {
            this.maxHistoryCount = value;
            this.ReduceIfPastHistoriesOverflow();
        }
    }

    public async ValueTask ExcuteAsync<T>(
        T state,
        Action<IMementoCommand<T?>> loader,
        string? name = null,
        Func<IMementoCommand<T?>, ValueTask>? onSave = null,
        Func<IMementoCommand<T?>, ValueTask>? onLoad = null,
        Action<IMementoCommand<T?>>? onDispose = null
    ) where T : class {
        await this.ExcuteAsync(
            new MementoCommand<T>(
                state,
                loader,
                name ?? Guid.NewGuid().ToString()
            ) {
                OnSave = onSave,
                OnLoad = onLoad,
                OnDispose = onDispose,
            }
        );
    }

    public async ValueTask ExcuteAsync<T>(IMementoCommand<T> command) {
        if (this.CanReDo) {
            this.ClearFutureHistoriesAsync();
        }

        if (this.Present is not null) {
            await this.Present.SaveAsync();
            this.Past.Push(this.Present);
        }

        command.Execute();
        this.Present = command;

        this.ReduceIfPastHistoriesOverflow();
    }

    public async ValueTask<bool> RedoAsync() {
        if (this.CanReDo is false || this.Future.Count <= 0) {
            return false;
        }

        if (this.Present is not null) {
            await this.Present.SaveAsync();
            this.Past.Push(this.Present);
        }

        var item = this.Future.Pop()!;
        await item.LoadAsync();
        item.Execute();
        this.Present = item;

        return true;
    }

    public async ValueTask<bool> UndoAsync() {
        if (this.CanUnDo is false || this.Past.Count <= 0) {
            return false;
        }

        if (this.Present is not null) {
            await this.Present.SaveAsync();
            this.Future.Push(this.Present);
        }

        var item = this.Past.Pop()!;
        await item.LoadAsync();
        item.Execute();
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
