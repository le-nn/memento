namespace Memento.Core;

public enum StateChangeType {
    Command,
    StateHasChanged,
    ForceReplaced,
    Restored,
}

public record StateChangedEventArgs {
    protected object? sender;

    public StateChangeType StateChangeType { get; init; }

    public Command? Command { get; init; } = default!;

    public object? LastState { get; init; }

    public object? State { get; init; }

    public DateTime Timestamp { get; } = DateTime.UtcNow;

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

    public new TCommand? Command {
        get => (TCommand)base.Command!;
        init => base.Command = value;
    }

    public new required TState? LastState {
        get => (TState)base.LastState!;
        init => base.LastState = value;
    }

    public new required TState? State {
        get => (TState)base.State!;
        init => base.State = value;
    }
}