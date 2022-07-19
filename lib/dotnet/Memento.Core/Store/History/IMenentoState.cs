namespace Memento;

public interface IMementoState {
    string Name { get; }
    object? State { get; }
}

public interface IMementoState<T> : IMementoState {
    new T? State { get; }
}

