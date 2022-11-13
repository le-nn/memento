namespace Memento.Core;

public delegate TState Reducer<TState, TCommand>(TState state, TCommand command)
    where TState : class
    where TCommand : Command;

public delegate TState StateInitializer<TState>() where TState : class;
