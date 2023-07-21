namespace Memento.Core;

public delegate object NextStoreMiddlewareCallback(object? state, Command command);

public delegate RootState NextProviderMiddlewareCallback(RootState? state, IStateChangedEventArgs<object, Command> e);

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

    public virtual RootState? HandleProviderDispatch(
        RootState? state,
        IStateChangedEventArgs<object,Command> e,
        NextProviderMiddlewareCallback next
    ) => next(state, e);

    public virtual object? HandleStoreDispatch(
        object? state,
        Command command,
        NextStoreMiddlewareCallback next
    ) => next(state, command);

    public virtual void Dispose() {

    }
}