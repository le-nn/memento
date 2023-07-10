namespace Memento.Core.History;

public interface IMementoCommandContext : IDisposable, IMementoStateContext {
    ValueTask InvokeContextSavedAsync();

    ValueTask InvokeContextLoadedAsync();

    ValueTask CommitAsync();

    ValueTask RestoreAsync();
}

public interface IMementoCommandContext<T> : IMementoCommandContext, IMementoStateContext<T> {

}