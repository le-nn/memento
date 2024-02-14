namespace Memento.Core;

/// <summary>
/// Represents a delegate for the next store middleware callback.
/// </summary>
/// <param name="state">The current state.</param>
/// <param name="command">The command being dispatched.</param>
/// <returns>The result of the next middleware callback.</returns>
public delegate object NextStoreMiddlewareCallback(object? state, object? command);

/// <summary>
/// Represents a delegate for the next provider middleware callback.
/// </summary>
/// <param name="state">The current state.</param>
/// <param name="e">The event arguments.</param>
/// <returns>The result of the next middleware callback.</returns>
public delegate RootState NextProviderMiddlewareCallback(RootState? state, IStateChangedEventArgs<object, object> e);

/// <summary>
/// Represents an abstract class for middleware handlers.
/// </summary>
public abstract class MiddlewareHandler : IDisposable {
    /// <summary>
    /// Called when the store is initialized.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    internal protected virtual async Task InitializedAsync() {
        await OnInitializedAsync();
    }

    /// <summary>
    /// Called when the store is initialized.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    protected virtual Task OnInitializedAsync() {
        return Task.CompletedTask;
    }

    /// <summary>
    /// Handles the provider dispatch.
    /// </summary>
    /// <param name="state">The current state.</param>
    /// <param name="e">The event arguments.</param>
    /// <param name="next">The next middleware callback.</param>
    /// <returns>The result of the next middleware callback.</returns>
    public virtual RootState? HandleProviderDispatch(
        RootState? state,
        IStateChangedEventArgs<object, object> e,
        NextProviderMiddlewareCallback next
    ) => next(state, e);

    /// <summary>
    /// Handles the store dispatch.
    /// </summary>
    /// <param name="state">The current state.</param>
    /// <param name="command">The command being dispatched.</param>
    /// <param name="next">The next middleware callback.</param>
    /// <returns>The result of the next middleware callback.</returns>
    public virtual object? HandleStoreDispatch(
        object? state,
        object? command,
        NextStoreMiddlewareCallback next
    ) => next(state, command);

    /// <summary>
    /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
    /// </summary>
    public virtual void Dispose() {

    }
}
