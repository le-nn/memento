namespace Memento.Core;

internal class StoreObeserver : IObserver<StateChangedEventArgs> {
    readonly Action<StateChangedEventArgs> action;

    public StoreObeserver(Action<StateChangedEventArgs> action) {
        this.action = action;
    }

    public void OnCompleted() {
        throw new NotImplementedException();
    }

    public void OnError(Exception error) {
        throw new NotImplementedException();
    }

    public void OnNext(StateChangedEventArgs value) {
        action(value);
    }
}

internal class StoreObeserver<TState, TConnad> : IObserver<StateChangedEventArgs<TState, TConnad>>
    where TState : class
    where TConnad : Command {
    readonly Action<StateChangedEventArgs<TState, TConnad>> action;

    public StoreObeserver(Action<StateChangedEventArgs<TState, TConnad>> action) {
        this.action = action;
    }

    public void OnCompleted() {
        throw new NotImplementedException();
    }

    public void OnError(Exception error) {
        throw new NotImplementedException();
    }

    public void OnNext(StateChangedEventArgs<TState, TConnad> value) {
        action(value);
    }
}