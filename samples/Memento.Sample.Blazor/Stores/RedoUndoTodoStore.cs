using Memento.Sample.Blazor.Todos;
using System.Collections.Immutable;

namespace Memento.Sample.Blazor.Stores;

public record RedoUndoTodoState {
    public ImmutableArray<Todo> Todos { get; init; } = ImmutableArray.Create<Todo>();

    public bool IsLoading { get; init; }
}

public class RedoUndoTodoStore : MementoStore<RedoUndoTodoState> {
    ITodoService TodoService { get; }

    public RedoUndoTodoStore(ITodoService todoService)
        : base(() => new(), new() { MaxHistoryCount = 20 }) {
        TodoService = todoService;
    }

    public async Task CreateNewAsync(string text) {
        await CommitAsync(
            () => ValueTask.FromResult(Guid.NewGuid()),
            async id => {
                var item = await TodoService.CreateItemAsync(id, text);
                Mutate(state => state with {
                    Todos = state.Todos.Add(item),
                });
            },
            async id => {
                await TodoService.RemoveAsync(id);
            }
        );
    }

    public async Task LoadAsync() {
        Mutate(state => state with { IsLoading = true });
        var items = await TodoService.FetchItemsAsync();
        Mutate(state => state with { Todos = items, });
        Mutate(state => state with { IsLoading = false });
    }

    public async Task ToggleIsCompletedAsync(Guid id) {
        await CommitAsync(
            async () => {
                var item = await TodoService.ToggleCompleteAsync(id)
                    ?? throw new Exception();
                Mutate(state => state with {
                    Todos = state.Todos.Replace(
                        state.Todos.Where(x => id == x.TodoId).First(),
                        item
                    )
                });
            },
            async () => {
                var item = await TodoService.ToggleCompleteAsync(id)
                    ?? throw new Exception();
            }
        );
    }
}