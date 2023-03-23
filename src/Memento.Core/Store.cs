namespace Memento.Core;

/// <summary>
/// Represents a store for managing state of type TState.
/// You can observe the state by subscribing to the StateChanged event.
/// </summary>
/// <typeparam name="TState">The type of state managed by the store.</typeparam>
public class Store<TState> : AbstractStore<TState, Command.StateHasChanged>
        where TState : class {
    /// <summary>
    /// Initializes a new instance of the Store class.
    /// </summary>
    /// <param name="initializer">The state initializer for creating the initial state.</param>
    public Store(StateInitializer<TState> initializer) : base(initializer, Reducer) {
    }

    /// <summary>
    /// Reduces the state using the provided StateHasChanged command.
    /// </summary>
    /// <param name="state">The current state.</param>
    /// <param name="command">The StateHasChanged command to apply.</param>
    /// <returns>The new state after applying the command.</returns>
    static TState Reducer(TState state, Command.StateHasChanged command) {
        return (TState)command.State;
    }

    /// <summary>
    /// Mutates the state using a reducer function.
    /// </summary>
    /// <param name="reducer">The reducer function to apply.</param>
    public void Mutate(Func<TState, TState> reducer) {
        var state = State;
        ComputedAndApplyState(state, new Command.StateHasChanged(reducer(state)));
    }

    /// <summary>
    /// Mutates the state using a new state.
    /// </summary>
    /// <param name="state">The new state to apply.</param>
    public void Mutate(TState state) {
        ComputedAndApplyState(State, new Command.StateHasChanged(state));
    }
}
