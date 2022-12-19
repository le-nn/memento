namespace Memento.Core;

public interface IMementoStateContext {
    string Name { get; }
    object State { get; set; }
}

public interface IMementoStateContext<T> : IMementoStateContext {
    new T State { get; set; }
}