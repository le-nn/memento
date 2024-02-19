namespace Memento.Core;

/// <summary>
/// Represents the event arguments for a state change.
/// </summary>
public interface IStore : IDisposable, IObservable<IStateChangedEventArgs> {
    /// <summary>
    /// Initializes the store asynchronously with the provided StoreProvider.
    /// </summary>
    /// <param name="provider">The StoreProvider used for initialization.</param>
    /// <returns>A Task representing the initialization process.</returns>
    internal ValueTask InitializeAsync(StoreProvider provider);

    /// <summary>
    /// Silently sets the state without invoking state change observers.
    /// </summary>
    /// <param name="state">The new state object to be set.</param>
    internal void SetStateForceSilently(object state);

    /// <summary>
    /// Sets the state and forces a state change, invoking state change observers.
    /// </summary>
    /// <param name="state">The new state object to be set.</param>
    internal void SetStateForce(object state);

    /// <summary>
    /// Gets the reducer function that takes a state object and a command object and returns a new state object.
    /// </summary>
    Reducer<object, object> ReducerHandle { get; }

    /// <summary>
    /// Gets the type of the state managed by the store.
    /// </summary>
    /// <returns>The Type of the state.</returns>
    Type GetStateType();

    /// <summary>
    /// Gets the current state of the store.
    /// </summary>
    object State { get; }
}

/// <summary>
/// Represents a store interface that maintains state and handles commands.
/// Implements the IObservable and IDisposable interfaces.
/// </summary>
public interface IStore<TState, TMessage>
    : IStore, IObservable<IStateChangedEventArgs<TState, TMessage>>
        where TState : class
        where TMessage : notnull {
    /// <summary>
    /// Gets the current state of the store.
    /// </summary>
    new TState State { get; }
}