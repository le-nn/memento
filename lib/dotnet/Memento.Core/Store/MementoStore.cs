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

    public IMementoCommandContext<TState>? Present =>
        this.HistoryManager.Present as IMementoCommandContext<TState>;

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
        string? name = null,
        Func<TState, ValueTask<TMessage>>? onExcecuted = null,
        Func<TState, ValueTask>? onUnexecuted = null
    ) where TMessage : Message {
        await this.HistoryManager.ExcuteAsync(
            this.State,
            async context => {
                var last = this.State;
                this.State = context.State;
                this.InvokeObserver(new StateChangedEventArgs {
                    LastState = last,
                    State = context.State,
                    Message = new Restores(),
                    Sender = this,
                });

                if (onExcecuted is not null) {
                    await onExcecuted.Invoke(context.State);
                }
            },
            name ?? Guid.NewGuid().ToString(),
            context => onUnexecuted?.Invoke(context.State) ?? ValueTask.CompletedTask,
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
