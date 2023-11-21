namespace Memento.Core;

internal class StoreObserver
    : IObserver<IStateChangedEventArgs<object, Command>> {
    readonly Action<IStateChangedEventArgs<object, Command>> _action;

    public StoreObserver(Action<IStateChangedEventArgs<object, Command>> action) {
        _action = action;
    }

    public void OnCompleted() {
        throw new NotSupportedException();
    }

    public void OnError(Exception error) {
        throw new NotSupportedException();
    }

    public void OnNext(IStateChangedEventArgs<object, Command> value) {
        _action(value);
    }
}
