namespace Memento.Core;

public record RootStateChangedEventArgs {
    public required StateChangedEventArgs StateChangedEvent { get; init; }
    public required IStore Store { get; init; }
    public required RootState RootState { get; init; }
}