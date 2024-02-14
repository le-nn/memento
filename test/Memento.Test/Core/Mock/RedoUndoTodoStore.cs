using Memento.Core;
using Memento.Test.Core.Mock;
using System.Collections.Immutable;

namespace Memento.Sample.Blazor.Stores;

public record RedoUndoTodoState {
    public ImmutableArray<Todo> Todos { get; init; } = [];

    public bool IsLoading { get; init; }
}

public class RedoUndoTodoStore(ITodoService todoService) : MementoStore<RedoUndoTodoState>(() => new(), new() { MaxHistoryCount = 20 }) {
    readonly ITodoService _todoService = todoService;

    public async Task CreateNewAsync(string text) {
        var id = Guid.NewGuid();
        await CommitAsync(
            async () => {
                var item = await _todoService.CreateItemAsync(id, text);
                Mutate(state => state with {
                    Todos = state.Todos.Add(item),
                });

                return item;
            },
            async todo => {
                await _todoService.RemoveAsync(todo.Payload.TodoId);
            }
        );
    }

    public async Task LoadAsync() {
        Mutate(state => state with { IsLoading = true });
        var items = await _todoService.FetchItemsAsync();
        Mutate(state => state with { Todos = items, });
        Mutate(state => state with { IsLoading = false });
    }

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
