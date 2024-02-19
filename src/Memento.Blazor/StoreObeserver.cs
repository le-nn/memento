namespace Memento.Blazor;

internal readonly struct StateObserver(Action<IStateChangedEventArgs> action)
        : IObserver<IStateChangedEventArgs> {
    readonly Action<IStateChangedEventArgs> _action = action;

    public void OnCompleted() {
        throw new NotSupportedException();
    }

    public void OnError(Exception error) {
        throw new NotSupportedException();
    }

    public void OnNext(IStateChangedEventArgs value) {
        _action(value);
    }
}