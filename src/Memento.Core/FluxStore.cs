using Memento.Core.Internals;
using Memento.Core.Store.Internals;

namespace Memento.Core;

public class FluxStore<TState, TCommand>
    : AbstractStore<TState, TCommand>
        where TState : class
        where TCommand : Command {
    protected FluxStore(
        StateInitializer<TState> initializer,
        Reducer<TState, TCommand> reducer
    ) : base(initializer, reducer) {
    }

    public void Dispatch(TCommand command) {
        ComputedAndApplyState(State, command);
    }

    public void Dispatch(Func<TState, TCommand> messageLoader) {
        ComputedAndApplyState(State, messageLoader(State));
    }
}