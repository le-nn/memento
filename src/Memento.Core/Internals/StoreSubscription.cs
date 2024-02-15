namespace Memento.Core.Internals;
internal sealed class StoreSubscription : IDisposable {
    private bool _isDisposed;

    readonly private string _id;
    readonly private Action _action;
    readonly private bool _wasCreated;

    /// <summary>
    /// Creates an instance of the class
    /// </summary>
    /// <param name="id">
    ///  An _id that is included in the command of exceptions that are thrown, this is useful
    ///  for helping to identify the source that created the instance that threw the exception.
    /// </param>
    /// <param name="action">The _action to execute when the instance is disposed</param>
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
    /// Executes the _action when disposed
    /// </summary>
    public void Dispose() {
        if (_isDisposed) {
            return;
        }

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
            throw new InvalidOperationException($"{nameof(StoreSubscription)} with id \"{_id}\" was not disposed. "+ new Exception().StackTrace);
    }
}