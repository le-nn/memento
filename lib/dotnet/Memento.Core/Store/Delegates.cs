namespace Memento;

public delegate TState Mutation<TState, TMessages>(TState state, TMessages message)
    where TState : class
    where TMessages : Message;

public delegate TState StateInitializer<TState>() where TState : class;
