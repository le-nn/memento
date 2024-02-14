namespace Memento.Core.History;
public interface IHistoryItem<out T> {
    string Name { get; }

    T HistoryState { get; }
}