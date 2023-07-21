namespace Memento.Core.History;

public interface IHistoryCommandItem<out T> : IHistoryItem<T>, IDisposable {
    ValueTask InvokeContextSavedAsync();

    ValueTask InvokeContextLoadedAsync();

    ValueTask CommitAsync();

    ValueTask RestoreAsync();
}