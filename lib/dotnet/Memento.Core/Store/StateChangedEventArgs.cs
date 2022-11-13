namespace Memento.Core;

public record StateChangedEventArgs {
    protected object? sender;

    public Command? Command { get; init; }

    public object? LastState { get; init; }

    public object? State { get; init; }

    public IStore? Sender {
        get {
            return (IStore?)sender;
        }
        init {
            sender = value;
        }
    }
}

public record StateChangedEventArgs<TState, TCommand> : StateChangedEventArgs
    where TState : class
    where TCommand : Command {
    public new required TCommand Command {
        get => (TCommand)base.Command!;
        init => base.Command = value;
    }

    public new required TState LastState {
        get => (TState)base.LastState!;
        init => base.LastState = value;
    }

    public new required TState State {
        get => (TState)base.State!;
        init => base.State = value;
    }

    public new required Store<TState, TCommand> Sender {
        get => (Store<TState, TCommand>)base.sender!;
        init => base.sender = value;
    }
}
