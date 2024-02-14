namespace Memento.Core;

/// <summary>
/// Represents a store for managing state of type TState.
/// You can observe the state by subscribing to the StateChanged event.
/// </summary>
/// <typeparam name="TState">The type of state managed by the store.</typeparam>
/// <typeparam name="TMessage">The type of message that describes what state change has occurred.</typeparam>
/// <remarks>
/// Initializes a new instance of the Store class.
/// </remarks>
/// <param name="initializer">The state initializer for creating the initial state.</param>
/// <param name="command">The type of message that describes what state change has occurred.</param>
public class Store<TState, TMessage>(Func<TState> initializer)
    : AbstractStore<TState, TMessage>(initializer, Reducer)
    where TState : class
    where TMessage : class {

    /// <summary>
    /// Initializes a new instance of the Store class with the specified initial state.
    /// </summary>
    /// <param name="state">The initial state.</param>
    public Store(TState state) : this(() => state) {
    }

    /// <summary>
    /// Reduces the state using the provided StateHasChanged command.
    /// </summary>
    /// <param name="state">The current state.</param>
    /// <param name="message">The StateHasChanged command to apply.</param>
    /// <returns>The new state after applying the command.</returns>
    static TState Reducer(TState state, TMessage? message) {
        return state;
    }

    /// <summary>
    /// Mutates the state using a reducer function.
    /// </summary>
    /// <param name="reducer">The reducer function to apply.</param>
    /// <param name="message">The message that describes what state change has occurred.</param>
    public void Mutate(Func<TState, TState> reducer, TMessage? message = default) {
        var state = State;
        ComputedAndApplyState(reducer(state), message);
    }

    /// <summary>
    /// Mutates the state using a new state.
    /// </summary>
    /// <param name="state">The new state to apply.</param>
    /// <param name="message">The message that describes what state change has occurred.</param>
    public void Mutate(TState state, TMessage? command = default) {
        ComputedAndApplyState(state, command);
    }
}

/// <summary>
/// Represents a store for managing state of type TState.
/// You can observe the state by subscribing to the StateChanged event.
/// </summary>
/// <typeparam name="TState">The type of state managed by the store.</typeparam>
public class Store<TState>(Func<TState> initializer) : Store<TState, string>(initializer)
        where TState : class {
    /// <summary>
    /// Initializes a new instance of the Store class with the specified initial state.
    /// </summary>
    /// <param name="state">The initial state.</param>
    public Store(TState state) : this(() => state) {
    }
}
