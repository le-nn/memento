using Blazor.Sample.Todos;
using Memento;
using System.Collections.Immutable;

namespace Blazor.Sample.Stores;

public record RedoUndoTodoState {
    public ImmutableArray<Todo> Todos { get; init; } = ImmutableArray.Create<Todo>();
}

public abstract record RedoUndoTodoMessages : Message {
    public record SetItems(ImmutableArray<Todo> Items) : RedoUndoTodoMessages;
    public record Append(Todo Item) : RedoUndoTodoMessages;
    public record Replace(Guid Id, Todo Item) : RedoUndoTodoMessages;
    public record BeginLoading : RedoUndoTodoMessages;
    public record EndLoading : RedoUndoTodoMessages;
    public record Commit : RedoUndoTodoMessages;
}

public class RedoUndoTodoStore : MementoStore<RedoUndoTodoState, RedoUndoTodoMessages> {
    ITodoService TodoService { get; }

    public RedoUndoTodoStore(ITodoService todoService) : base(() => new(), Mutation) {
        TodoService = todoService;
    }

    static RedoUndoTodoState Mutation(RedoUndoTodoState state, RedoUndoTodoMessages message) {
        return message switch {
            RedoUndoTodoMessages.SetItems payload => state with {
                Todos = payload.Items,
            },
            RedoUndoTodoMessages.Append payload => state with {
                Todos = state.Todos.Add(payload.Item)
            },
            RedoUndoTodoMessages.Replace payload => state with {
                Todos = state.Todos.Replace(
                    state.Todos.Where(x => payload.Id == x.TodoId).First(),
                    payload.Item
                )
            },
            _ => throw new Exception("The message is not handled."),
        };
    }

    public async Task CreateNewAsync(string text) {
        await this.CommitAsync(
            async () => {
                var item = await this.TodoService.CreateItemAsync(text);
                return new RedoUndoTodoMessages.Append(item);
            },
            async message => {
                var m = message as RedoUndoTodoMessages.Append;
                await this.TodoService.RemoveAsync(m.Item.TodoId);
            });
    }

    public async Task FetchAsync() {
        this.Mutate(new RedoUndoTodoMessages.BeginLoading());
        var items = await this.TodoService.FetchItemsAsync();
        this.Mutate(new RedoUndoTodoMessages.SetItems(items));
        this.Mutate(new RedoUndoTodoMessages.EndLoading());

        //await this.CommitAsync(new RedoUndoTodoMessages.Commit(), "initialize/store");
    }
}
