namespace Memento.Core;

/// <summary>
/// Represents a store for managing state of type TState.
/// You can observe the state by subscribing to the StateChanged event.
/// Realize state management like Flux or MVU.
/// You should mutate the state via reducer function specified to constructor params.
/// The Reducer function generates a new state from the current state and a command.
/// </summary>
/// <typeparam name="TState">The type of state managed by the store.</typeparam>
public class FluxStore<TState, TCommand>
    : AbstractStore<TState, TCommand>
        where TState : class
        where TCommand : Command {
    /// <summary>
    /// Initializes a new instance of the FluxStore class.
    /// </summary>
    /// <param name="initializer">The state initializer for creating the initial state.</param>
    /// <param name="reducer">The reducer function for applying commands to the state.</param>
    protected FluxStore(
        StateInitializer<TState> initializer,
        Reducer<TState, TCommand> reducer
    ) : base(initializer, reducer) {
    }

    /// <summary>
    /// Dispatches a command to the store, which updates the state accordingly.
    /// </summary>
    /// <param name="command">The command to dispatch.</param>
    public void Dispatch(TCommand command) {
        ComputedAndApplyState(State, command);
    }

    /// <summary>
    /// Dispatches a command to the store using a message loader function, which updates the state accordingly.
    /// </summary>
    /// <param name="messageLoader">The function to generate a command based on the current state.</param>
    public void Dispatch(Func<TState, TCommand> messageLoader) {
        ComputedAndApplyState(State, messageLoader(State));
    }
}