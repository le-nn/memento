namespace Memento;
public interface IMementoState {
    string Name { get; }
    object? State { get; }
}
