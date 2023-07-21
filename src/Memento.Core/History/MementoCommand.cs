namespace Memento.Core.History;

public record HistoryCommandContext<T> : IHistoryCommandItem<T> where T : notnull {
    private T? _historyState;

    public bool IsDisposed { get; private set; }

    public T HistoryState => _historyState
        ?? throw new NullReferenceException("The state is null because CommitAsync is not called.");

    public string Name { get; }

    private Func<ValueTask<T>> DoHandler { get; }

    private Func<T, ValueTask> UnDoHandler { get; init; }

    public Func<IHistoryItem<T?>, ValueTask>? ContextSaved { get; init; }

    public Func<IHistoryItem<T?>, ValueTask>? ContextLoaded { get; init; }

    public Action<IHistoryCommandItem<T?>>? Disposed { get; init; }

    public HistoryCommandContext(
        Func<ValueTask<T>> doHandler,
        Func<T, ValueTask> undoHandler,
        string name
    ) {
        UnDoHandler = undoHandler;
        DoHandler = doHandler;
        Name = name;
    }

    public async ValueTask CommitAsync() {
        _historyState = await DoHandler();
    }

    public async ValueTask RestoreAsync() {
        await UnDoHandler.Invoke(HistoryState);
    }

    public async ValueTask InvokeContextSavedAsync() {
        if (ContextSaved is null) {
            return;
        }

        await ContextSaved.Invoke(this!);
    }

    public async ValueTask InvokeContextLoadedAsync() {
        if (ContextLoaded is null) {
            return;
        }

        await ContextLoaded.Invoke(this!);
    }

    public void Dispose() {
        if (IsDisposed) {
            throw new Exception("This Command has been disposed.");
        }

        IsDisposed = true;
        _ = ContextLoaded?.Invoke(this!);
    }
}