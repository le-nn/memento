namespace Memento.Core;

/// <summary>
/// Represents a delegate that defines a reducer function.
/// </summary>
/// <typeparam name="TState">The type of the state.</typeparam>
/// <typeparam name="TMessage">The type of the message.</typeparam>
/// <param name="state">The current state.</param>
/// <param name="command">The message/command to be processed.</param>
/// <returns>The updated state.</returns>
public delegate TState Reducer<TState, TMessage>(TState state, TMessage? command)
    where TState : class
    where TMessage : notnull;
