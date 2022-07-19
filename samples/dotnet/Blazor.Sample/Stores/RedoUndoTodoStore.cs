using Blazor.Sample.Todos;
using Memento;
using System.Collections.Immutable;

namespace Blazor.Sample.Stores;

public record RedoUndoTodoState {
    public ImmutableArray<Todo> Todos { get; init; } = ImmutableArray.Create<Todo>();
}

public record RedoUndoTodoMessages : Message {
    public record SetItems(ImmutableArray<Todo> Items) : RedoUndoTodoMessages;
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
            _ => throw new Exception("The message is not handled."),
        };
    }

    public async Task CreateNewAsync(string text) {
        await this.TodoService.CreateItemAsync(text);
    }

    public async Task FetchAsync() {
        this.CommitAsync();
        
       await TodoService.FetchItemsAsync();
    }
}
