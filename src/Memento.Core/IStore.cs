namespace Memento.Core;

public interface IStore : IObservable<StateChangedEventArgs> {
    object State { get; }

    TStore ToStore<TStore>() where TStore : IStore;

    internal protected Task OnInitializedAsync(StoreProvider provider);

    internal void SetStateForceSilently(object state);

    internal void SetStateForce(object state);

    Func<object, Command, object> Reducer { get; }

    Type GetStateType();

    Type GetCommandType();
}