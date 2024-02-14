namespace Memento.Core;

public abstract class Middleware : IDisposable {
    MiddlewareHandler? _handler;

    /// <summary>
    /// Gets the handler for this middleware.
    /// </summary>
    public MiddlewareHandler Handler => _handler
        ?? throw new InvalidOperationException($"Middleware '{GetType().FullName}' has not initialized.");

    /// <summary>
    /// Initializes the middleware asynchronously.
    /// </summary>
    /// <param name="provider">The service provider.</param>
    internal async Task InitializeAsync(IServiceProvider provider) {
        var handler = Create(provider);
        _handler = handler;
        await handler.InitializedAsync();
    }

    /// <summary>
    /// Creates the handler for this middleware.
    /// </summary>
    /// <param name="provider">The service provider.</param>
    /// <returns>The created middleware handler.</returns>
    protected abstract MiddlewareHandler Create(IServiceProvider provider);

    /// <summary>
    /// Disposes the middleware.
    /// </summary>
    public void Dispose() {
        _handler?.Dispose();
    }
}
