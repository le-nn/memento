using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Memento;

public class HistoryManager {
    private int maxHistoryCount = 20;
    private FutureHistoryStack<IMementoCommand> Future = new();
    private PastHistoryStack<IMementoCommand> Past = new();

    public IMementoCommand? Present => this.Past.Peek();

    public IReadOnlyCollection<IMementoState> FutureHistories => this.Future.AsReadOnly();

    public IReadOnlyCollection<IMementoState> PastHistories => this.Past.AsReadOnly();

    public bool IsCanReDo => this.Future.Count is not 0;

    public bool IsCanUnDo => this.Past.Count is not 0;

    public int PresentIndex => this.Past.Count - 1;

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
        if (this.IsCanReDo) {
            this.ClearFutureHistoriesAsync();
        }

        if (this.Present is not null) {
            await this.Present.SaveAsync();
        }

        command.Execute();
        this.Past.Push(command);
        this.ReduceIfPastHistoriesOverflow();
    }

    public async ValueTask ReExecuteAsync() {
        if (this.IsCanReDo) {
            var item = this.Future.Pop()!;
            await item.LoadAsync();
            item.Execute();

            if (this.Present is not null) {
                await this.Present.SaveAsync();
            }

            this.Past.Push(item);
        }
    }

    public async ValueTask UnExecuteAsync() {
        if (this.IsCanUnDo) {
            var item = this.Past.Pop()!;
            await item.LoadAsync();
            item.Execute();

            if (this.Present is not null) {
                await this.Present.SaveAsync();
            }

            this.Future.Push(item);
        }
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
