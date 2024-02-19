namespace Memento.Core;

/// <summary>
/// Represents the event arguments for a root state change.
/// </summary>
public record RootStateChangedEventArgs {
    /// <summary>
    /// Gets or sets the state changed event.
    /// </summary>
    public required IStateChangedEventArgs StateChangedEvent { get; init; }

    /// <summary>
    /// Gets or sets the store.
    /// </summary>
    public required IStore Store { get; init; }

    /// <summary>
    /// Gets or sets the root state.
    /// </summary>
    public required RootState RootState { get; init; }
}
