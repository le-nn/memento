using Memento;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Memento;

public abstract class MementoStore<TState, TMessages>
    : Store<TState, TMessages>
        where TState : class
        where TMessages : Message {

    HistoryManager HistoryManager { get; } = new();

    public IReadOnlyCollection<IMementoState<TState>> FutureHistories => this.HistoryManager
        .FutureHistories
        .Select(x => x as IMementoState<TState>)
        .Where(x => x is not null)
        .Select(x => x!)
        .ToList()
        .AsReadOnly();

    public IReadOnlyCollection<IMementoState<TState>> PastHistories => this.HistoryManager
        .PastHistories
        .Select(x => x as IMementoState<TState>)
        .Where(x => x is not null)
        .Select(x => x!)
        .ToList()
        .AsReadOnly();

    public IMementoState<TState>? Present => this.HistoryManager.Present as IMementoState<TState>;

    public MementoStore(
        StateInitializer<TState> initializer,
        Mutation<TState, TMessages> mutation
    ) : base(initializer, mutation) {

    }

    public virtual ValueTask OnStateSavedAsync(IMementoCommand<TState?> command) {
        return ValueTask.CompletedTask;
    }

    public virtual ValueTask OnStateLoadAsync(IMementoCommand<TState?> command) {
        return ValueTask.CompletedTask;
    }

    public virtual void OnStateDisposed(IMementoCommand<TState?> command) {
    }

    public async ValueTask CommitAsync(string? name = null) {
        await this.HistoryManager.ExcuteAsync(
            this.State,
            context => {
                if (context.State is null) {
                    throw new Exception("State is not set in IMementoCommand<T>");
                }

                this.State = context.State;
            },
            name ?? Guid.NewGuid().ToString(),
            context => this.OnStateSavedAsync(context),
            context => this.OnStateLoadAsync(context),
            context => this.OnStateDisposed(context)
        );
    }

    public async ValueTask UndoAsync() {
        await this.HistoryManager.UndoAsync();
    }

    public async ValueTask ReooAsync() {
        await this.HistoryManager.RedoAsync();
    }
}
