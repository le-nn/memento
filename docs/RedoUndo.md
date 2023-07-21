# ReDo/UnDo

```Memento.Core.History.HistoryManager``` is a class that manages the redo/undo state context.
It gives you a generic ReDo/UnDo.

Here is  an example of how to use it.

```cs

@using Memento.Core.History;

<PageTitle>Index</PageTitle>

<h1>Redo / Undo Counter</h1>

<button disabled="@(!_historyManager.CanUnDo)" @onclick="() => _historyManager.UnDoAsync()">UnDo</button>
<button disabled="@(!_historyManager.CanReDo)" @onclick="() => _historyManager.ReDoAsync()">ReDo</button>

<h2>@_count</h2>

<button @onclick="CountUp">Count Up</button>

@code {
    readonly HistoryManager _historyManager = new() { MaxHistoryCount = 20 };

    int _count = 0;

    async Task CountUp() {
        await _historyManager.CommitAsync(
            async () => {
                var count = _count;
                _count++;
                return new {
                    Count = count,
                };
            },
            async state => {
                _count = state.Count;
            }
        );
    }
}

```


MaxHistoryCount is the maximum number that can be saved.
Disabling button with CanUnDo/CanReDo.

Take a snapshot of the current State with CommitAsync.

The first argument callback is called when "Do" or "ReDo" is performed, and retains the returned Context as a Snapshot.
The first callback should be implemented to create and take snapshot of the state when "Do" or "ReDo" performed.

The second argument callback is called when "UnDo" is performed.
The second callback should be implemented to Restore the state from the snapshot of the state when "UnDo" performed.

# With Store

```Memento.Core.MementoStore``` or ```Memento.Core.FluxMementoStore``` allows you to manage the redo/undo state context in the store.
Specify the HistoryManager instance as the second argument of base() constructor. You can share context across stores.
Invoking CommitAsync preserves the current State, which can be restored with UnDoAsync and ReDoAsync.

Unlike HistoryManager.CommitAsync, there is no need to manually assign state. 
The state at the time Store.CommitAsync was invoked is automatically restored.

The first argument callback is called when "Do" or "ReDo" is performed,
The returned value is retained as a payload to receive when the second argument callback is called.
The first callback should be implemented to create a payload when "Do" or "ReDo" performed.

The second argument callback is called when "UnDo" is performed.
The second callback should be implemented handling such as removing items with a received payload from the DB or Server etc.

## Sample with ToDo

```cs

public record RedoUndoTodoState {
    public ImmutableArray<Todo> Todos { get; init; } = ImmutableArray.Create<Todo>();

    public bool IsLoading { get; init; }
}

public class RedoUndoTodoStore : MementoStore<RedoUndoTodoState> {
    readonly ITodoService _todoService;

    public RedoUndoTodoStore(ITodoService todoService) : base(() => new(), new() { MaxHistoryCount = 20 }) {
        _todoService = todoService;
    }

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

```

DEMO

https://le-nn.github.io/memento/todo