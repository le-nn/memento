namespace Memento.Core.History;

public record MementoCommandContext<T> : IMementoCommandContext<T> {
    public bool IsDisposed { get; private set; }

    public T? State { get; set; } = default;

    object? IMementoStateContext.State {
        get => State;
        set => State = (T?)value;
    }

    public string Name { get; }

    private Func<ValueTask<T>> DoHandler { get; }

    private Func<T, ValueTask> UnDoHandler { get; init; }

    public Func<IMementoStateContext<T?>, ValueTask>? ContextSaved { get; init; }

    public Func<IMementoStateContext<T?>, ValueTask>? ContextLoaded { get; init; }

    public Action<IMementoCommandContext<T?>>? Disposed { get; init; }

    public MementoCommandContext(
        Func<ValueTask<T>> doHandler,
        Func<T, ValueTask> undoHandler,
        string name
    ) {
        UnDoHandler = undoHandler;
        DoHandler = doHandler;
        Name = name;
    }

    public async ValueTask CommitAsync() {
        State = await DoHandler();
    }

    public async ValueTask RestoreAsync() {
        if (State is null) {
            throw new NullReferenceException("State is null.");
        }

        await UnDoHandler.Invoke(State);
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