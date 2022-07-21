using Memento;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Memento;

public record Restores : Message;

public record Context<TState, TMessage>(TState State, TMessage Message);

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

    public virtual ValueTask OnContextSavedAsync(IMementoStateContext<Context<TState, TMessages>> command) {
        return ValueTask.CompletedTask;
    }

    public virtual ValueTask OnContextLoadedAsync(IMementoStateContext<Context<TState, TMessages>> command) {
        return ValueTask.CompletedTask;
    }

    public virtual void OnContextDisposed(IMementoStateContext<Context<TState, TMessages>> command) {
    }

    public async ValueTask CommitAsync(
        Func<ValueTask<TMessages>> messageCreator,
        Func<TMessages, ValueTask> onUnexecuted,
        string? name = null
    ) {
        await this.HistoryManager.ExcuteCommitAsync<Context<TState, TMessages>>(
            async () => {
                var message = await messageCreator.Invoke();
                return new Context<TState, TMessages>(this.State, message);
            },
            async state => {
                await onUnexecuted(state.Message);
            },
            async (state) => {
                this.ApplyComputedState(state.State, state.Message);
            },
            name ?? Guid.NewGuid().ToString(),
            context => this.OnContextSavedAsync(context),
            context => this.OnContextLoadedAsync(context),
            context => this.OnContextDisposed(context)
        );
    }

    public async ValueTask CommitAsync(TMessages message, string? name = null) {
        await this.CommitAsync(async () => message, async (s) => { }, name);
    }

    static Context<TState, TMessages> UpCast<TMessage>(Context<TState, TMessage> ctx)
        where TMessage : TMessages {
        return new Context<TState, TMessages>(ctx.State, ctx.Message);
    }

    public async ValueTask UnExecuteAsync() {
        await this.HistoryManager.UnExecuteAsync();
    }

    public async ValueTask ReExecuteAsync() {
        await this.HistoryManager.ReExecuteAsync();
    }
}
