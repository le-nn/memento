using Memento.Sample.Blazor.Todos;
using System.Collections.Immutable;

namespace Memento.Sample.Blazor.Stores;

using static RedoUndoTodoCommands;

public record RedoUndoTodoState {
    public ImmutableArray<Todo> Todos { get; init; } = ImmutableArray.Create<Todo>();

    public bool IsLoading { get; init; }
}

public record RedoUndoTodoCommands : Command {
    public record SetItems(ImmutableArray<Todo> Items) : RedoUndoTodoCommands;
    public record Append(Todo Item) : RedoUndoTodoCommands;
    public record Replace(Guid Id, Todo Item) : RedoUndoTodoCommands;
    public record BeginLoading : RedoUndoTodoCommands;
    public record EndLoading : RedoUndoTodoCommands;
}

public class RedoUndoTodoStore : MementoStore<RedoUndoTodoState, RedoUndoTodoCommands> {
    ITodoService TodoService { get; }

    public RedoUndoTodoStore(ITodoService todoService) : base(() => new(), Reducer, new() { MaxHistoryCount = 200 }) {
        TodoService = todoService;
    }

    static RedoUndoTodoState Reducer(RedoUndoTodoState state, RedoUndoTodoCommands command) {
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

    public async Task CreateNewAsync(string text) {
        await CommitAsync(
            async () => {
                return Guid.NewGuid();
            },
            async id => {
                var item = await TodoService.CreateItemAsync(id, text);
                Dispatch(new RedoUndoTodoCommands.Append(item));
            },
            async id => {
                await TodoService.RemoveAsync(id);
            }
        );
    }

    public async Task LoadAsync() {
        Dispatch(new BeginLoading());
        var items = await TodoService.FetchItemsAsync();
        Dispatch(new SetItems(items));
        Dispatch(new EndLoading());
    }

    public async Task ToggleIsCompletedAsync(Guid id) {
        await CommitAsync(
            async () => {
                var item = await TodoService.ToggleCompleteAsync(id)
                    ?? throw new Exception();
                Dispatch(new Replace(id, item));
            },
            async () => {
                var item = await TodoService.ToggleCompleteAsync(id)
                    ?? throw new Exception();
            }
        );
    }
}