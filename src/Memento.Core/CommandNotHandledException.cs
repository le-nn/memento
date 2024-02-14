namespace Memento.Core;

/// <summary>
/// Represents the exception that is thrown when a command is not handled.
/// </summary>
/// <typeparam name="TMessage">Type of message.</typeparam>
/// <param name="command">the command.</param>
public class CommandNotHandledException<TMessage>(TMessage? command)
    : Exception($"The command is not handled. {command?.GetType().Name}")
    where TMessage : notnull {
    public TMessage? Command { get; } = command;
}