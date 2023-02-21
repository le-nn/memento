using Memento.Sample.Blazor.Todos;
using System.Collections.Immutable;

namespace Memento.Sample.Blazor.Stores;

using static FluxRedoUndoTodoCommand;

public record FluxRedoUndoTodoState {
    public ImmutableArray<Todo> Todos { get; init; } = ImmutableArray.Create<Todo>();

    public bool IsLoading { get; init; }
}

public record FluxRedoUndoTodoCommand : Command {
    public record SetItems(ImmutableArray<Todo> Items) : FluxRedoUndoTodoCommand;
    public record Append(Todo Item) : FluxRedoUndoTodoCommand;
    public record Replace(Guid Id, Todo Item) : FluxRedoUndoTodoCommand;
    public record BeginLoading : FluxRedoUndoTodoCommand;
    public record EndLoading : FluxRedoUndoTodoCommand;
}

public class FluxRedoUndoTodoStore : FluxMementoStore<FluxRedoUndoTodoState, FluxRedoUndoTodoCommand> {
    ITodoService TodoService { get; }

    public FluxRedoUndoTodoStore(ITodoService todoService)
        : base(
            () => new(),
            Reducer,
            new() { MaxHistoryCount = 20 }
        ) {
        TodoService = todoService;
    }

    static FluxRedoUndoTodoState Reducer(FluxRedoUndoTodoState state, FluxRedoUndoTodoCommand command) {
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
            () => {
                return ValueTask.FromResult(Guid.NewGuid());
            },
            async id => {
                var item = await TodoService.CreateItemAsync(id, text);
                Dispatch(new Append(item));
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