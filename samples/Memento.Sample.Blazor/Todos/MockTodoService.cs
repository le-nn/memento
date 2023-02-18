using System.Collections.Immutable;

namespace Memento.Sample.Blazor.Todos;

public class MockTodoService : ITodoService {
    readonly List<Todo> _items = new() {
        new () {
            TodoId = Guid.NewGuid(),
            CreatedAt = DateTime.Now,
            IsCompleted = false,
            Text = "Test Item 1",
        },
        new () {
            TodoId = Guid.NewGuid(),
            CreatedAt = DateTime.Now,
            IsCompleted = false,
            Text = "Test Item 2",
        },
        new () {
            TodoId = Guid.NewGuid(),
            CreatedAt = DateTime.Now,
            IsCompleted = false,
            Text = "Test Item 3",
        },
    };

    public async Task<Todo> CreateItemAsync(Guid id, string text) {
        await Task.Delay(600);
        var todo = Todo.CreateNew(id, text);
        _items.Add(todo);
        return todo;
    }

    public async Task<Todo?> FetchItemAsync(Guid id) {
        await Task.Delay(600);
        return _items.Where(x => x.TodoId == id).FirstOrDefault();
    }

    public async Task<ImmutableArray<Todo>> FetchItemsAsync() {
        await Task.Delay(600);
        return _items.ToImmutableArray();
    }

    public async Task RemoveAsync(Guid id) {
        var item = await FetchItemAsync(id);
        if (item is not null) {
            _items.Remove(item);
        }
    }

    public async Task SaveAsync(Todo todo) {
        var item = await FetchItemAsync(todo.TodoId);
        if (item is null) {
            return;
        }

        var index = _items.IndexOf(item);
        if (index is not -1) {
            _items[index] = todo;
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

        var index = _items.IndexOf(item);
        var newItem = func(item);
        if (index is not -1) {
            _items[index] = newItem;
        }
        else {
            return null;
        }

        return newItem;
    }


}