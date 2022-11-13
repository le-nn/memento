namespace Memento.Core;

public delegate object NextMiddlewareHandler(object state, Command command);

public abstract class Middleware : IDisposable {
    protected StoreProvider? Provider;

    /// <summary>
    ///  Called on the store initialized.
    /// </summary>
    /// <param name="provider">The StoreProvider.</param>
    internal protected virtual void OnInitialized(StoreProvider provider) {
        Provider = provider;
    }

    public virtual object? Handle(
        object state,
        Command command,
        NextMiddlewareHandler next
    ) => next(state, command);

    public virtual void Dispose() {

    }
}
