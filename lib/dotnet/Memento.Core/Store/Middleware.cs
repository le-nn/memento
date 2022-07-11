namespace Memento;

public delegate object NextMiddlewareHandler(object state, Message message);

public abstract class Middleware : IDisposable {
    protected StoreProvider? Provider;

    /// <summary>
    ///  Called on the store initialized.
    /// </summary>
    /// <param name="provider">The StoreProvider.</param>
    internal protected virtual void OnInitialized(StoreProvider provider) {
        this.Provider = provider;
    }

    public virtual object? Handle(
        object state,
        Message message,
        NextMiddlewareHandler next
    ) => next(state, message);

    public virtual void Dispose() {

    }
}
