using Memento;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Memento;

public record Restores : Message;

public abstract class MementoStore<TState, TMessages>
    : Store<TState, TMessages>
        where TState : class
        where TMessages : Message {

    HistoryManager HistoryManager { get; } = new();

    public bool CanReDo => this.HistoryManager.CanReDo;

    public bool CanUnDo => this.HistoryManager.CanUnDo;

    public IMementoStateContext<TState>? Present => this.HistoryManager.Present as IMementoStateContext<TState>;

    public IReadOnlyCollection<IMementoStateContext<TState>> FutureHistories => this.HistoryManager
        .FutureHistories
        .Select(x => x as IMementoStateContext<TState>)
        .Where(x => x is not null)
        .Select(x => x!)
        .ToList()
        .AsReadOnly();

    public IReadOnlyCollection<IMementoStateContext<TState>> PastHistories => this.HistoryManager
        .PastHistories
        .Select(x => x as IMementoStateContext<TState>)
        .Where(x => x is not null)
        .Select(x => x!)
        .ToList()
        .AsReadOnly();

    public MementoStore(
        StateInitializer<TState> initializer,
        Mutation<TState, TMessages> mutation
    ) : base(initializer, mutation) {

    }

    public virtual ValueTask OnContextSavedAsync(IMementoStateContext<TState?> command) {
        return ValueTask.CompletedTask;
    }

    public virtual ValueTask OnContextLoadedAsync(IMementoStateContext<TState?> command) {
        return ValueTask.CompletedTask;
    }

    public virtual void OnContextDisposed(IMementoStateContext<TState?> command) {
    }

    public async ValueTask CommitAsync<TMessage>(
        Func<TState, ValueTask<TMessage>> onExcecuted,
        Func<TState, ValueTask> onUnexecuted,
        string? name = null
    ) where TMessage : TMessages {
        await this.HistoryManager.ExcuteCommitAsync(
            async () => {
                var lastState = this.State;
                var message = await onExcecuted.Invoke(lastState);
                this.Mutate(message);

                return message;
            },
            async state => {
                await onUnexecuted(this.State);
                this.Mutate(state);
            },
            name ?? Guid.NewGuid().ToString(),
            context => this.OnContextSavedAsync(context),
            context => this.OnContextLoadedAsync(context),
            context => this.OnContextDisposed(context)
        );
    }

    public async ValueTask UnExecuteAsync() {
        await this.HistoryManager.UnExecuteAsync();
    }

    public async ValueTask ReExecuteAsync() {
        await this.HistoryManager.ReExecuteAsync();
    }
}
