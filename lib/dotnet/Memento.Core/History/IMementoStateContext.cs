namespace Memento.Core;

public interface IMementoCommandContext : IDisposable, IMementoStateContext {
    ValueTask InvokeContextSavedAsync();

    ValueTask InvokeContextLoadedAsync();

    ValueTask CommitAsync();

    ValueTask RestoreAsync();

    ValueTask LoadDataAsync();
}

public interface IMementoCommandContext<T> : IMementoCommandContext, IMementoStateContext<T> {

}