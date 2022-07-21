namespace Memento;

public record MementoCommandContext<T> : IMementoCommandContext<T> {

    public bool IsDisposed { get; private set; }

    public T? State { get; set; } = default(T?);

    object? IMementoStateContext.State {
        get => this.State;
        set => this.State = (T?)value;
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
        this.Dataloader = dataloader;
        this.UnExecuted = unexecuted;
        this.Executed = executed;
        this.Name = name;
    }

    public async ValueTask CommitAsync() {
        this.State = await this.Executed();
    }

    public async ValueTask RestoreAsync() {
        if (this.State is null) {
            throw new NullReferenceException("State is null.");
        }

        await this.UnExecuted.Invoke(this.State);
    }

    public async ValueTask InvokeContextSavedAsync() {
        if (this.ContextSaved is null) {
            return;
        }

        await this.ContextSaved.Invoke(this!);
    }

    public async ValueTask InvokeContextLoadedAsync() {
        if (this.ContextLoaded is null) {
            return;
        }

        await this.ContextLoaded.Invoke(this!);
    }

    public void Dispose() {
        if (this.IsDisposed) {
            throw new Exception("Command has been disposed.");
        }

        this.IsDisposed = true;
        this.ContextLoaded?.Invoke(this!);
    }

    public ValueTask LoadDataAsync() {
        return Dataloader.Invoke(this.State!);
    }
}
