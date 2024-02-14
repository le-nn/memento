namespace Memento.Core.Internals;
internal readonly struct GeneralObserver<T>(Action<T> action) : IObserver<T> {
    readonly Action<T> _action = action;

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

internal readonly struct StoreObserver<TState, TMessage>(Action<IStateChangedEventArgs<TState, TMessage>> action)
    : IObserver<IStateChangedEventArgs<TState, TMessage>>
    where TState : class
    where TMessage : notnull {
    readonly Action<IStateChangedEventArgs<TState, TMessage>> _action = action;

    public void OnCompleted() {
        throw new NotImplementedException();
    }

    public void OnError(Exception error) {
        throw new NotImplementedException();
    }

    public void OnNext(IStateChangedEventArgs<TState, TMessage> value) {
        _action(value);
    }
}

internal readonly struct StoreProviderObserver(Action<RootStateChangedEventArgs> action) : IObserver<RootStateChangedEventArgs> {
    readonly Action<RootStateChangedEventArgs> _action = action;

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