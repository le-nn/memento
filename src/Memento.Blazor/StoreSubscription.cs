namespace Memento.Blazor;

internal sealed class StoreSubscription : IDisposable {
    private readonly string _id;
    private readonly Action _action;
    private bool _isDisposed;
    private readonly bool _wasCreated;

    /// <summary>
    /// Creates an instance of the class
    /// </summary>
    /// <param name="id">
    ///  An Id that is included in the command of exceptions that are thrown, this is useful
    ///  for helping to identify the source that created the instance that threw the exception.
    /// </param>
    /// <param name="action">The action to execute when the instance is disposed</param>
    public StoreSubscription(string id, Action action) {
        if (string.IsNullOrWhiteSpace(id))
            throw new ArgumentNullException(nameof(id));
        if (action is null)
            throw new ArgumentNullException(nameof(action));

        _id = id;
        _action = action;
        _wasCreated = true;
    }

    /// <summary>
    /// Executes the action when disposed
    /// </summary>
    public void Dispose() {
        if (_isDisposed)
            throw new ObjectDisposedException(
                nameof(StoreSubscription),
                $"Attempt to call {nameof(Dispose)} twice on {nameof(StoreSubscription)} with Id \"{_id}\".");

        _isDisposed = true;
        GC.SuppressFinalize(this);
        _action();
    }

    /// <summary>
    /// Throws an exception if this object is collected without being disposed
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown if the object is collected without being disposed</exception>
    ~StoreSubscription() {
        if (!_isDisposed && _wasCreated)
            throw new InvalidOperationException($"{nameof(StoreSubscription)} with Id \"{_id}\" was not disposed. ");
    }
}