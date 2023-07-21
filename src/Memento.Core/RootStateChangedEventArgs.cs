namespace Memento.Core;

public record RootStateChangedEventArgs {
    public required IStateChangedEventArgs<object, Command> StateChangedEvent { get; init; }
    public required IStore<object, Command> Store { get; init; }
    public required RootState RootState { get; init; }
}