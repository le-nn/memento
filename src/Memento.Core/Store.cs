namespace Memento.Core;

public class Store<TState> : AbstractStore<TState, Command.StateHasChanged>
        where TState : class {

    public Store(StateInitializer<TState> initializer) : base(initializer, Reducer) {
    }

    static TState Reducer(TState state, Command.StateHasChanged command) {
        return (TState)command.State;
    }

    public void Mutate(Func<TState, TState> reducer) {
        var state = State;
        ComputedAndApplyState(state, new Command.StateHasChanged(reducer(state)));
    }

    public void Mutate(TState state) {
        ComputedAndApplyState(State, new Command.StateHasChanged(state));
    }
}
