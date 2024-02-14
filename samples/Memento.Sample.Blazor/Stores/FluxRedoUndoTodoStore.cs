using Memento.Sample.Blazor.Todos;
using System.Collections.Immutable;

namespace Memento.Sample.Blazor.Stores;

using static FluxRedoUndoTodoCommand;

public record FluxRedoUndoTodoState {
    /// <summary>
    /// Gets or sets the list of todos.
    /// </summary>
    public ImmutableArray<Todo> Todos { get; init; } = [];

    /// <summary>
    /// Gets or sets a value indicating whether the store is currently loading.
    /// </summary>
    public bool IsLoading { get; init; }
}

public record FluxRedoUndoTodoCommand : Command {
    public record SetItems(ImmutableArray<Todo> Items) : FluxRedoUndoTodoCommand;
    public record Append(Todo Item) : FluxRedoUndoTodoCommand;
    public record Replace(Guid Id, Todo Item) : FluxRedoUndoTodoCommand;
    public record BeginLoading : FluxRedoUndoTodoCommand;
    public record EndLoading : FluxRedoUndoTodoCommand;
}

/// <summary>
/// Initializes a new instance of the <see cref="FluxRedoUndoTodoStore"/> class.
/// </summary>
/// <param name="todoService">The todo service.</param>
public class FluxRedoUndoTodoStore(ITodoService todoService) 
    : FluxMementoStore<FluxRedoUndoTodoState, FluxRedoUndoTodoCommand>(
        () => new(), 
        new() { MaxHistoryCount = 20 },
        Reducer
    ) {
    readonly ITodoService _todoService = todoService;

    static FluxRedoUndoTodoState Reducer(FluxRedoUndoTodoState state, FluxRedoUndoTodoCommand? command) {
        return command switch {
            SetItems payload => state with {
                Todos = payload.Items,
            },
            Append payload => state with {
                Todos = state.Todos.Add(payload.Item)
            },
            Replace payload => state with {
                Todos = state.Todos.Replace(
                    state.Todos.Where(x => payload.Id == x.TodoId).First(),
                    payload.Item
                )
            },
            BeginLoading => state with { IsLoading = true },
            EndLoading => state with { IsLoading = false },
            _ => throw new Exception("The command is not handled."),
        };
    }

    /// <summary>
    /// Creates a new todo item asynchronously.
    /// </summary>
    /// <param name="text">The text of the todo item.</param>
    /// <returns>The created todo item.</returns>
    public async Task CreateNewAsync(string text) {
        await CommitAsync(
            async () => {
                var id = Guid.NewGuid();
                var item = await _todoService.CreateItemAsync(id, text);
                Dispatch(new Append(item));
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
        Dispatch(new BeginLoading());
        var items = await _todoService.FetchItemsAsync();
        Dispatch(new SetItems(items));
        Dispatch(new EndLoading());
    }

    /// <summary>
    /// Toggles the completion status of a todo item asynchronously.
    /// </summary>
    /// <param name="id">The ID of the todo item.</param>
    /// <returns>The updated todo item.</returns>
    public async Task ToggleIsCompletedAsync(Guid id) {
        await CommitAsync(
            async () => {
                var item = await _todoService.ToggleCompleteAsync(id)
                    ?? throw new Exception();
                Dispatch(new Replace(id, item));
                return item;
            },
            async todo => {
                var item = await _todoService.ToggleCompleteAsync(todo.Payload.TodoId)
                    ?? throw new Exception();
            }
        );
    }
}
