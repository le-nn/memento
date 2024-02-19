using Memento.Core.History;

namespace Memento.Core;

public abstract class MementoStore<TState, TMessage>(
    Func<TState> initializer,
    HistoryManager historyManager
) : AbstractMementoStore<TState, TMessage>(initializer, historyManager, Reducer)
        where TState : class
        where TMessage : class {

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
        ComputedAndApplyState(reducer(State), message);
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
public class MementoStore<TState>(
    Func<TState> initializer,
    HistoryManager historyManager
) : MementoStore<TState, string>(
    initializer,
    historyManager
) where TState : class {
}