namespace Memento.Core.Internals;
internal class GeneralObeserver<T> : IObserver<T> {
    readonly Action<T> _action;

    public GeneralObeserver(Action<T> action) {
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

internal class StoreObeserver : IObserver<StateChangedEventArgs> {
    readonly Action<StateChangedEventArgs> _action;

    public StoreObeserver(Action<StateChangedEventArgs> action) {
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

internal class StoreObeserver<TState, TMessages> : IObserver<StateChangedEventArgs<TState, TMessages>>
    where TState : class
    where TMessages : Command {
    readonly Action<StateChangedEventArgs<TState, TMessages>> _action;

    public StoreObeserver(Action<StateChangedEventArgs<TState, TMessages>> action) {
        this._action = action;
    }

    public void OnCompleted() {
        throw new NotImplementedException();
    }

    public void OnError(Exception error) {
        throw new NotImplementedException();
    }

    public void OnNext(StateChangedEventArgs<TState, TMessages> value) {
        _action(value);
    }
}