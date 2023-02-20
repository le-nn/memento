namespace Memento.Core;

public record StateChangedEventArgs {
    protected object? sender;

    public Command Command { get; init; } = default!;

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

public record StateChangedEventArgs<TState> : StateChangedEventArgs
    where TState : class{

    public new required TState LastState {
        get => (TState)base.LastState!;
        init => base.LastState = value;
    }

    public new required TState State {
        get => (TState)base.State!;
        init => base.State = value;
    }
}