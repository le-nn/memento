using System.Collections.Immutable;

namespace Memento.Test.Core.Mock;

public interface ITodoService {
    Task<ImmutableArray<Todo>> FetchItemsAsync();
    Task<Todo?> FetchItemAsync(Guid id);
    Task<Todo> CreateItemAsync(Guid id, string text);
    Task<Todo?> SetIsCompletedAsync(Guid id, bool isCompleted);
    Task<Todo?> ToggleCompleteAsync(Guid id);
    Task SaveAsync(Todo todo);
    Task RemoveAsync(Guid id);
}