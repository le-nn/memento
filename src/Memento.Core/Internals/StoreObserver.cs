namespace Memento.Core.Internals;

internal class GeneralObserver<T> : IObserver<T> {
    readonly Action<T> _action;

    public GeneralObserver(Action<T> action) {
        _action = action;
    }

    public void OnCompleted() {
        throw new NotImplementedException();
    }

    public void OnError(Exception error) {
        throw new NotImplementedException();
    }

    public void OnNext(T value) {
        _action(value);
    }
}

internal class StoreObserver
    : IObserver<StateChangedEventArgs> {
    readonly Action<StateChangedEventArgs> _action;

    public StoreObserver(
        Action<StateChangedEventArgs> action) {
        _action = action;
    }

    public void OnCompleted() {
        throw new NotImplementedException();
    }

    public void OnError(Exception error) {
        throw new NotImplementedException();
    }

    public void OnNext(StateChangedEventArgs value) {
        _action(value);
    }
}

internal class StoreProviderObserver : IObserver<RootStateChangedEventArgs> {
    readonly Action<RootStateChangedEventArgs> _action;

    public StoreProviderObserver(Action<RootStateChangedEventArgs> action) {
        _action = action;
    }

    public void OnCompleted() {
        throw new NotImplementedException();
    }

    public void OnError(Exception error) {
        throw new NotImplementedException();
    }

    public void OnNext(RootStateChangedEventArgs value) {
        _action(value);
    }
}
