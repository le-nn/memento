namespace Memento;

public record MementoCommand<T> : IMementoCommand<T>
    where T : class {
    private Action<IMementoCommand<T?>> OnDataLoaded { get; }

    public bool IsDisposed { get; private set; }

    public T? State { get; set; }

    object? IMementoState.State => this.State;

    public string Name { get; }

    public Func<IMementoCommand<T?>, ValueTask>? OnSave { get; init; }

    public Func<IMementoCommand<T?>, ValueTask>? OnLoad { get; init; }

    public Action<IMementoCommand<T?>>? OnDispose { get; init; }

    public MementoCommand(
        T state,
        Action<IMementoCommand<T?>> loader,
        string name
    ) {
        this.State = state;
        this.OnDataLoaded = loader;
        this.Name = name;
    }

    public void Execute() {
        this.OnDataLoaded(this!);
    }

    public async ValueTask SaveAsync() {
        if (this.OnSave is null) {
            return;
        }

        await this.OnSave.Invoke(this!);
    }

    public async ValueTask LoadAsync() {
        if (this.OnLoad is null) {
            return;
        }

        await this.OnLoad.Invoke(this!);
    }

    public void Dispose() {
        if (this.IsDisposed) {
            throw new Exception("Command has been disposed.");
        }

        this.IsDisposed = true;
        this.OnDispose?.Invoke(this!);
    }
}
