using Memento.Sample.Blazor.Todos;
using System.Collections.Immutable;

namespace Memento.Sample.Blazor.Stores;

/// <summary>
/// Represents the state of the RedoUndoTodoStore.
/// </summary>
public record RedoUndoTodoState {
    /// <summary>
    /// Gets or sets the list of todos.
    /// </summary>
    public ImmutableArray<Todo> Todos { get; init; } = [];

    /// <summary>
    /// Gets or sets a value indicating whether the store is currently loading.
    /// </summary>
    public bool IsLoading { get; init; }
}

/// <summary>
/// Represents a store for managing the RedoUndoTodoState.
/// </summary>
/// <remarks>
/// Initializes a new instance of the RedoUndoTodoStore class.
/// </remarks>
/// <param name="todoService">The ITodoService instance.</param>
public class RedoUndoTodoStore(ITodoService todoService)
    : MementoStore<RedoUndoTodoState>(() => new(), new() { MaxHistoryCount = 20 }) {
    readonly ITodoService _todoService = todoService;

    /// <summary>
    /// Creates a new todo item asynchronously.
    /// </summary>
    /// <param name="text">The text of the todo item.</param>
    /// <returns>The created todo item.</returns>
    public async Task CreateNewAsync(string text) {
        var id = Guid.NewGuid();
        await CommitAsync(
            async () => {
                var item = await _todoService.CreateItemAsync(id, text);
                Mutate(state => state with {
                    Todos = [.. state.Todos, item],
                });

                return item;
            },
            async todo => {
                await _todoService.RemoveAsync(todo.Payload.TodoId);
            }
        );
    }

    /// <summary>
    /// Loads the todo items asynchronously.
    /// </summary>
    public async Task LoadAsync() {
        Mutate(state => state with { IsLoading = true });
        var items = await _todoService.FetchItemsAsync();
        Mutate(state => state with { Todos = items, });
        Mutate(state => state with { IsLoading = false });
    }

    /// <summary>
    /// Toggles the completion status of a todo item asynchronously.
    /// </summary>
    /// <param name="id">The ID of the todo item.</param>
    /// <returns>The updated todo item.</returns>
    public async Task ToggleIsCompletedAsync(Guid id) {
        await CommitAsync(
            async () => {
                var state = State;
                var item = await _todoService.ToggleCompleteAsync(id)
                    ?? throw new Exception("Failed to toggle an item in Do or ReDo.");
                Mutate(state => state with {
                    Todos = state.Todos.Replace(
                        state.Todos.Where(x => id == x.TodoId).First(),
                        item
                    )
                });

                return item;
            },
            async p => {
                var item = await _todoService.ToggleCompleteAsync(id)
                    ?? throw new Exception("Failed to toggle an item in UnDo.");
            }
        );
    }
}
