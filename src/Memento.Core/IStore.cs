namespace Memento.Core;

public interface IStore : IObservable<StateChangedEventArgs>, IDisposable {
    object State { get; }

    TStore AsStore<TStore>() where TStore : IStore;

    internal Task InitializeAsync(StoreProvider provider);

    internal void SetStateForceSilently(object state);

    internal void SetStateForce(object state);

    void StateHasChanged();

    Func<object, Command, object> Reducer { get; }

    Type GetStateType();

    Type GetCommandType();
}