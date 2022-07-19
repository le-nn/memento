using System.Collections.Immutable;

namespace Blazor.Sample.Todos;

public class MockTodoService : ITodoService {
    List<Todo> Items = new();

    public async Task<Todo> CreateItemAsync(string text) {
        await Task.Delay(600);
        var todo = Todo.CreateNew(text);
        this.Items.Add(todo);
        return todo;
    }

    public async Task<Todo?> FetchItemAsync(Guid id) {
        await Task.Delay(600);
        return this.Items.Where(x => x.TodoId == id).FirstOrDefault();
    }

    public async Task<ImmutableArray<Todo>> FetchItemsAsync() {
        await Task.Delay(600);
        return this.Items.ToImmutableArray();
    }

    public async Task RemoveAsync(Guid id) {
        var item = await this.FetchItemAsync(id);
        if (item is not null) {
            this.Items.Remove(item);
        }
    }

    public async Task SaveAsync(Todo todo) {
        var item = await this.FetchItemAsync(todo.TodoId);
        if (item is null) {
            return;
        }

        var index = this.Items.IndexOf(item);
        if (index is not -1) {
            this.Items[index] = todo;
        }
    }

    public async Task<Todo?> SetIsCompletedAsync(Guid id, bool isCompleted) {
        return await this.Replace(id, todo => todo with {
            IsCompleted = isCompleted,
        });
    }

    public async Task<Todo?> ToggleCompleteAsync(Guid id) {
        return await this.Replace(id, todo => todo with {
            IsCompleted = !todo.IsCompleted,
        });
    }

    private async Task<Todo?> Replace(Guid id, Func<Todo, Todo> func) {
        var item = await this.FetchItemAsync(id);
        if (item is null) {
            return null;
        }

        var newItem = func(item);
        var index = this.Items.IndexOf(item);
        if (index is not -1) {
            this.Items[index] = func(newItem);
        }

        return newItem;
    }
}
