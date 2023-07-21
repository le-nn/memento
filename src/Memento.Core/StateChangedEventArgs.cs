using System.Data;

namespace Memento.Core;

public enum StateChangeType {
    Command,
    StateHasChanged,
    ForceReplaced,
    Restored,
}

public interface IStateChangedEventArgs<out TState, out TCommand>
    where TState : class
    where TCommand : Command {

    StateChangeType StateChangeType { get; }

    TCommand? Command { get; }


    TState? LastState { get; }

    TState? State { get; }

    DateTime Timestamp { get; }

    IStore<TState, TCommand>? Sender { get; }
}

record StateChangedEventArgs<TState, TCommand> : IStateChangedEventArgs<TState, TCommand>
    where TState : class
    where TCommand : Command {
    protected object? sender;

    public StateChangeType StateChangeType { get; init; }

    public required TCommand? Command { get; init; }

    public required TState? LastState { get; init; }

    public required TState? State { get; init; }

    public DateTime Timestamp { get; } = DateTime.UtcNow;

    public IStore<TState, TCommand>? Sender { get; init; }

}