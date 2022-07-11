namespace Memento;

public interface IStore : IObservable<StateChangedEventArgs> {
    object State { get; }

    TStore ToStore<TStore>()
            where TStore : IStore;

    internal protected void OnInitialized(StoreProvider provider);
}
