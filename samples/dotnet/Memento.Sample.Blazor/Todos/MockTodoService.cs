using System.Collections.Immutable;

namespace Memento.Sample.Blazor.Todos;

public class MockTodoService : ITodoService {
    readonly List<Todo> Items = new();

    public async Task<Todo> CreateItemAsync(Guid id, string text) {
        await Task.Delay(600);
        var todo = Todo.CreateNew(id, text);
        Items.Add(todo);
        return todo;
    }

    public async Task<Todo?> FetchItemAsync(Guid id) {
        await Task.Delay(600);
        return Items.Where(x => x.TodoId == id).FirstOrDefault();
    }

    public async Task<ImmutableArray<Todo>> FetchItemsAsync() {
        await Task.Delay(600);
        return Items.ToImmutableArray();
    }

    public async Task RemoveAsync(Guid id) {
        var item = await FetchItemAsync(id);
        if (item is not null) {
            Items.Remove(item);
        }
    }

    public async Task SaveAsync(Todo todo) {
        var item = await FetchItemAsync(todo.TodoId);
        if (item is null) {
            return;
        }

        var index = Items.IndexOf(item);
        if (index is not -1) {
            Items[index] = todo;
        }
    }

    public async Task<Todo?> SetIsCompletedAsync(Guid id, bool isCompleted) {
        return await Replace(id, todo => todo with {
            IsCompleted = isCompleted,
        });
    }

    public async Task<Todo?> ToggleCompleteAsync(Guid id) {
        return await Replace(
            id,
            todo => todo with {
                IsCompleted = !todo.IsCompleted,
            }
        );
    }

    private async Task<Todo?> Replace(Guid id, Func<Todo, Todo> func) {
        var item = await FetchItemAsync(id);
        if (item is null) {
            return null;
        }

        var index = Items.IndexOf(item);
        var newItem = func(item);
        if (index is not -1) {
            Items[index] = newItem;
        }
        else {
            return null;
        }

        return newItem;
    }


}