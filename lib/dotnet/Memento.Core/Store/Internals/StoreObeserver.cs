namespace Memento.Core.Store.Internals;

internal class StoreObeserver : IObserver<StateChangedEventArgs>
{
    readonly Action<StateChangedEventArgs> action;

    public StoreObeserver(Action<StateChangedEventArgs> action)
    {
        this.action = action;
    }

    public void OnCompleted()
    {
        throw new NotImplementedException();
    }

    public void OnError(Exception error)
    {
        throw new NotImplementedException();
    }

    public void OnNext(StateChangedEventArgs value)
    {
        action(value);
    }
}

internal class StoreProviderObserver : IObserver<RootStateChangedEventArgs>
{
    readonly Action<RootStateChangedEventArgs> action;

    public StoreProviderObserver(Action<RootStateChangedEventArgs> action)
    {
        this.action = action;
    }

    public void OnCompleted()
    {
        throw new NotImplementedException();
    }

    public void OnError(Exception error)
    {
        throw new NotImplementedException();
    }

    public void OnNext(RootStateChangedEventArgs value)
    {
        action(value);
    }
}

internal class StoreObeserver<TState, TMessages> : IObserver<StateChangedEventArgs<TState, TMessages>>
    where TState : class
    where TMessages : Message
{
    readonly Action<StateChangedEventArgs<TState, TMessages>> action;

    public StoreObeserver(Action<StateChangedEventArgs<TState, TMessages>> action)
    {
        this.action = action;
    }

    public void OnCompleted()
    {
        throw new NotImplementedException();
    }

    public void OnError(Exception error)
    {
        throw new NotImplementedException();
    }

    public void OnNext(StateChangedEventArgs<TState, TMessages> value)
    {
        action(value);
    }
}
