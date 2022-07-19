namespace Memento;

public record MementoCommandContext<T> : IMementoCommandContext<T>
    where T : class {
    private Func<IMementoCommandContext<T>, ValueTask> OnDataLoaded { get; }


    public bool IsDisposed { get; private set; }

    public T? State { get; set; }

    object? IMementoStateContext.State {
        get => this.State;
        set => this.State = (T?)value;
    }

    public string Name { get; }

    public Func<IMementoCommandContext<T>, ValueTask>? UnExecuted { get; init; }

    public Func<IMementoCommandContext<T?>, ValueTask>? ContextSaved { get; init; }

    public Func<IMementoCommandContext<T?>, ValueTask>? ContextLoaded { get; init; }

    public Action<IMementoCommandContext<T?>>? Disposed { get; init; }

    public MementoCommandContext(
        T state,
        Func<IMementoCommandContext<T>, ValueTask> dataLoader,
        string name
    ) {
        this.State = state;
        this.OnDataLoaded = dataLoader;
        this.Name = name;
    }

    public async ValueTask ExecuteLoaderAsync() {
        if (this.State is null) {
            throw new NullReferenceException("State is null.");
        }

        await this.OnDataLoaded(this);
    }

    public async ValueTask InvokeUnExecutedAsync() {
        if (this.State is null) {
            throw new NullReferenceException("State is null.");
        }

        await this.OnDataLoaded(this!);
        await (this.UnExecuted?.Invoke(this!) ?? ValueTask.CompletedTask);
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
}
