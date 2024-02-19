public enum StateChangeType {
    StateHasChanged,
    ForceReplaced,
    Restored,
}

/// <summary>
/// Represents the event arguments for a state change with a message.
/// </summary>
/// <typeparam name="TMessage">The type of the message.</typeparam>
public interface IStateChangedEventArgs {
    /// <summary>
    /// Gets or sets the type of the state change.
    /// </summary>
    public StateChangeType StateChangeType { get; init; }

    /// <summary>
    /// Gets the timestamp of the state change.
    /// </summary>
    DateTime Timestamp { get; }

    /// <summary>
    /// Gets the message associated with the state change.
    /// </summary>
    object? Message { get; }

    /// <summary>
    /// Gets the sender of the state change.
    /// </summary>
    object? Sender { get; }
}

/// <summary>
/// Represents the event arguments for a state change with a message.
/// </summary>
/// <typeparam name="TMessage">The type of the message.</typeparam>
public record StateChangedEventArgs : IStateChangedEventArgs {
    /// <summary>
    /// Gets or sets the type of the state change.
    /// </summary>
    public StateChangeType StateChangeType { get; init; }

    /// <inheritdoc/>
    public required object? Message { get; init; }

    /// <inheritdoc/>
    public DateTime Timestamp { get; } = DateTime.UtcNow;

    /// <inheritdoc/>
    public required object? Sender { get; init; }
}

/// <summary>
/// Represents the event arguments for a state change with a state and a message.
/// </summary>
/// <typeparam name="TState">The type of the state.</typeparam>
/// <typeparam name="TMessage">The type of the message.</typeparam>
public interface IStateChangedEventArgs<out TState, out TMessage> : IStateChangedEventArgs
    where TState : class
    where TMessage : notnull {
    /// <summary>
    /// Gets the type of the state change.
    /// </summary>
    StateChangeType StateChangeType { get; }

    /// <summary>
    /// Gets the last state before the change.
    /// </summary>
    TState? LastState { get; }

    /// <summary>
    /// Gets the current state after the change.
    /// </summary>
    TState? State { get; }
}

/// <summary>
/// Represents the event arguments for a state change with a state and a message.
/// </summary>
/// <typeparam name="TState">The type of the state.</typeparam>
/// <typeparam name="TMessage">The type of the message.</typeparam>
public record StateChangedEventArgs<TState, TMessage> : IStateChangedEventArgs<TState, TMessage>
    where TState : class
    where TMessage : notnull {
    /// <summary>
    /// Gets or sets the type of the state change.
    /// </summary>
    public StateChangeType StateChangeType { get; init; }

    /// <inheritdoc/>
    public required TMessage? Message { get; init; }

    /// <inheritdoc/>
    public required TState? LastState { get; init; }

    /// <inheritdoc/>
    public required TState? State { get; init; }

    /// <inheritdoc/>
    public DateTime Timestamp { get; } = DateTime.UtcNow;

    /// <inheritdoc/>
    public object? Sender { get; init; }

    /// <inheritdoc/>
    object? IStateChangedEventArgs.Message => Message;
}