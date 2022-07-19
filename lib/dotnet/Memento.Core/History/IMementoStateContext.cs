namespace Memento;

public interface IMementoCommandContext : IDisposable, IMementoStateContext {
    ValueTask ExecuteLoaderAsync();

    ValueTask InvokeUnExecutedAsync();

    ValueTask InvokeContextSavedAsync();

    ValueTask InvokeContextLoadedAsync();
}

public interface IMementoCommandContext<T> : IMementoCommandContext, IMementoStateContext<T> {
}
