namespace Memento.Blazor.Internal;

/// <summary>
/// Represents an observer that receives notifications of type <typeparamref name="T"/>.
/// </summary>
/// <typeparam name="T">The type of the notifications.</typeparam>
internal readonly struct Observer<T>(Action<T> onNext) : IObserver<T> {
    private readonly Action<T> _onNext = onNext;

    /// <summary>
    /// Notifies the observer that the provider has finished sending push-based notifications.
    /// </summary>
    public void OnCompleted() {
        // noop
    }

    /// <summary>
    /// Notifies the observer that the provider has experienced an error condition.
    /// </summary>
    /// <param name="error">The error that occurred.</param>
    public void OnError(Exception error) {
        // noop
    }

    /// <summary>
    /// Provides the observer with new data.
    /// </summary>
    /// <param name="value">The current notification information.</param>
    public void OnNext(T value) {
        _onNext(value);
    }
}
