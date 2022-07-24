using Memento;
using Memento.Core.Executors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Memento;

public record Restores : Message;

public record Context<TState, TMessage>(TState State, TMessage Message);

public abstract class MementoStore<TState, TMessages>
    : Store<TState, TMessages>
        where TState : class
        where TMessages : Message, new() {
    HistoryManager HistoryManager { get; }

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
        Mutation<TState, TMessages> mutation,
        HistoryManager historyManager
    ) : base(
        initializer,
        (state, message) => message switch {
            TMessages => mutation(state, message),
            _ => state,
        }
    ) {
        this.HistoryManager = historyManager;
    }

    public virtual ValueTask OnContextSavedAsync(IMementoStateContext<Context<TState, TMessages>> command) {
        if (this.IsInitialized is false) {
            throw new Exception("Store is not initialized.");
        }

        return ValueTask.CompletedTask;
    }

    public virtual ValueTask OnContextLoadedAsync(IMementoStateContext<Context<TState, TMessages>> command) {
        if (this.IsInitialized is false) {
            throw new Exception("Store is not initialized.");
        }

        return ValueTask.CompletedTask;
    }

    public virtual void OnContextDisposed(IMementoStateContext<Context<TState, TMessages>> command) {
        if (this.IsInitialized is false) {
            throw new Exception("Store is not initialized.");
        }
    }

    public async ValueTask CommitAsync<T>(
        Func<ValueTask<T>> dataCreator,
        Func<T, ValueTask> onExecuted,
        Func<T, ValueTask> onUnexecuted,
        string? name = null
    ) {
        if(this.IsInitialized is false) {
            throw new Exception("Store is not initialized.");
        }

        var data = await dataCreator();
        await this.HistoryManager.ExcuteCommitAsync(
            async () => {
                var state = this.State;
                await onExecuted(data);
                return new Context<TState, TMessages>(state, new TMessages());
            },
            async state => {
                await onUnexecuted(data);

                var lastState = this.State;
                this.State = state.State;
                this.InvokeObserver(new StateChangedEventArgs {
                    LastState = lastState,
                    State = this.State,
                    Message = new Restores(),
                    Sender = this,
                });
            },
            async (state) => {
                //this.ApplyComputedState(state.State, state.Message);
            },
            name ?? Guid.NewGuid().ToString(),
            context => this.OnContextSavedAsync(context),
            context => this.OnContextLoadedAsync(context),
            context => this.OnContextDisposed(context)
        );
    }

    public async ValueTask CommitAsync(
        Func<ValueTask> onExecuted,
        Func<ValueTask> onUnexecuted,
        string? name = null
    ) {
        await this.CommitAsync(
            async () => {
                return (byte)0;
            },
            async _ => {
                await onExecuted();
            },
            async _ => {
                await onUnexecuted();
            },
            name
        );
    }

    public async ValueTask CommitAsync(TMessages message, string? name = null) {
        if (this.IsInitialized is false) {
            throw new Exception("Store is not initialized.");
        }

        await this.CommitAsync(async () => message, async (s) => { }, async (s) => { }, name);
    }

    public async ValueTask UnExecuteAsync() {
        if (this.IsInitialized is false) {
            throw new Exception("Store is not initialized.");
        }

        await this.HistoryManager.UnExecuteAsync();
    }

    public async ValueTask ReExecuteAsync() {
        if (this.IsInitialized is false) {
            throw new Exception("Store is not initialized.");
        }

        await this.HistoryManager.ReExecuteAsync();
    }
}
