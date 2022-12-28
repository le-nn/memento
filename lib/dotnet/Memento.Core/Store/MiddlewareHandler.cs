namespace Memento.Core;

public delegate object NextMiddlewareCallback(object state, Command command);

public abstract class MiddlewareHandler : IDisposable {
    /// <summary>
    ///  Called on the store initialized.
    /// </summary>
    /// <param name="provider">The StoreProvider.</param>
    internal protected virtual async Task InitializedAsync() {
        await OnInitializedAsync();
    }

    protected virtual Task OnInitializedAsync() {
        return Task.CompletedTask;
    }

    public virtual object? Handle(
        object state,
        Command command,
        NextMiddlewareCallback next
    ) => next(state, command);

    public virtual void Dispose() {

    }
}