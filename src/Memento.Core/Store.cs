namespace Memento.Core;

/// <summary>
/// Represents a store for managing state of type TState.
/// You can observe the state by subscribing to the StateChanged event.
/// </summary>
/// <typeparam name="TState">The type of state managed by the store.</typeparam>
/// <typeparam name="TMessage">The type of message that describes what state change has occurred.</typeparam>
public class Store<TState, TMessage> : AbstractStore<TState, Command.StateHasChanged<TState, TMessage>>
    where TState : class
    where TMessage : notnull {

    /// <summary>
    /// Initializes a new instance of the Store class.
    /// </summary>
    /// <param name="initializer">The state initializer for creating the initial state.</param>
    /// <param name="command">The type of message that describes what state change has occurred.</param>
    public Store(StateInitializer<TState> initializer) : base(initializer, Reducer) {
    }

    /// <summary>
    /// Reduces the state using the provided StateHasChanged command.
    /// </summary>
    /// <param name="state">The current state.</param>
    /// <param name="message">The StateHasChanged command to apply.</param>
    /// <returns>The new state after applying the command.</returns>
    static TState Reducer(TState state, Command.StateHasChanged<TState, TMessage> message) {
        return (TState)message.State;
    }

    /// <summary>
    /// Mutates the state using a reducer function.
    /// </summary>
    /// <param name="reducer">The reducer function to apply.</param>
    /// <param name="message">The message that describes what state change has occurred.</param>
    public void Mutate(Func<TState, TState> reducer, TMessage? message = default) {
        var state = State;
        var type = GetType();
        ComputedAndApplyState(state, new Command.StateHasChanged<TState, TMessage>(reducer(state), message, type));
    }

    /// <summary>
    /// Mutates the state using a new state.
    /// </summary>
    /// <param name="state">The new state to apply.</param>
    /// <param name="message">The message that describes what state change has occurred.</param>
    public void Mutate(TState state, TMessage? command = default) {
        ComputedAndApplyState(State, new Command.StateHasChanged<TState, TMessage>(state, command, GetType()));
    }
}

/// <summary>
/// Represents a store for managing state of type TState.
/// You can observe the state by subscribing to the StateChanged event.
/// </summary>
/// <typeparam name="TState">The type of state managed by the store.</typeparam>
public class Store<TState> : Store<TState, Command.StateHasChanged<TState, string>>
        where TState : class {
    public Store(StateInitializer<TState> initializer) : base(initializer) {
    }
}
