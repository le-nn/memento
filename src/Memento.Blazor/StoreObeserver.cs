namespace Memento.Blazor;

internal readonly struct StateObserver(Action<IStateChangedEventArgs<object>> action)
        : IObserver<IStateChangedEventArgs<object>> {
    readonly Action<IStateChangedEventArgs<object>> _action = action;

    public void OnCompleted() {
        throw new NotSupportedException();
    }

    public void OnError(Exception error) {
        throw new NotSupportedException();
    }

    public void OnNext(IStateChangedEventArgs<object> value) {
        _action(value);
    }
}