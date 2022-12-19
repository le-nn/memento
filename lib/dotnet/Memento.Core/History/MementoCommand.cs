namespace Memento.Core;

public record MementoCommandContext<T> : IMementoCommandContext<T> {

    public bool IsDisposed { get; private set; }


    /* Unmerged change from project 'Memento.Core(net6.0)'
    Before:
        public T? State { get; set; } = default(T?);
    After:
        public T? State { get; set; } = default;
    */
    public T? State { get; set; } = default;

    object? IMementoStateContext.State {
        get => State;
        set => State = (T?)value;
    }

    public string Name { get; }

    private Func<T, ValueTask> Dataloader { get; }

    private Func<ValueTask<T>> Executed { get; }

    private Func<T, ValueTask> UnExecuted { get; init; }

    public Func<IMementoStateContext<T?>, ValueTask>? ContextSaved { get; init; }

    public Func<IMementoStateContext<T?>, ValueTask>? ContextLoaded { get; init; }

    public Action<IMementoCommandContext<T?>>? Disposed { get; init; }

    public MementoCommandContext(
        Func<T, ValueTask> dataloader,
        Func<ValueTask<T>> executed,
        Func<T, ValueTask> unexecuted,
        string name
    ) {
        Dataloader = dataloader;
        UnExecuted = unexecuted;
        Executed = executed;
        Name = name;
    }

    public async ValueTask CommitAsync() {
        State = await Executed();
    }

    public async ValueTask RestoreAsync() {
        if (State is null) {
            throw new NullReferenceException("State is null.");
        }

        await UnExecuted.Invoke(State);
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
        ContextLoaded?.Invoke(this!);
    }

    public ValueTask LoadDataAsync() {
        return Dataloader.Invoke(State!);
    }
}