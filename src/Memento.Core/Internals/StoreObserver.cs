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

internal class StoreObserver<TState, TCommand>
    : IObserver<IStateChangedEventArgs<TState, TCommand>> 
    where TState:class 
    where TCommand :Command {
    readonly Action<IStateChangedEventArgs<TState, TCommand>> _action;

    public StoreObserver(Action<IStateChangedEventArgs<TState, TCommand>> action) {
        _action = action;
    }

    public void OnCompleted() {
        throw new NotImplementedException();
    }

    public void OnError(Exception error) {
        throw new NotImplementedException();
    }

    public void OnNext(IStateChangedEventArgs<TState, TCommand> value) {
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
