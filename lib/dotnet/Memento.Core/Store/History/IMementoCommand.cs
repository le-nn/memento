namespace Memento;

public interface IMementoCommand : IDisposable, IMementoState {
    void Execute();

    void UnExecute();

    ValueTask SaveAsync();

    ValueTask LoadAsync();
}

public interface IMementoCommand<T> : IMementoCommand, IMementoState<T> {
}
