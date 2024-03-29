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

```

```razor

@using Memento.Sample.Blazor.Stores
@using System.Text.Json

@inherits ObserverComponent
@inject AsyncCounterStore AsyncCounterStore

<div class="p-4 mt-4 bg-opacity-10 bg-dark rounded-2">
    <h1 class="">Async Counter Component</h1>
    <h2>Current count: @AsyncCounterStore.State.Count</h2>
    <p>Loading: @AsyncCounterStore.State.IsLoading</p>

    <div>
        <button class="mt-3 btn btn-primary" @onclick="IncrementCount">Count up</button>
        <button class="mt-3 btn btn-primary" @onclick="CountUpMany">Count up 100 times</button>
    </div>

    <div class="mt-5">
        <h3>Count up async with histories</h3>
        <button class="mt-3 btn btn-primary" @onclick="IncrementCountAsync">Count up async</button>
        <p class="mt-3 mb-0">Histories</p>
        <div class="d-flex">
            @foreach (var item in string.Join(", ", AsyncCounterStore.State.Histories)) {
                @item
            }
        </div>
    </div>

    <div class="mt-5">
        <h3>Count up with Amount</h3>
        <input @bind-value="_amount" />
    </div>
    <button class="mt-3 btn btn-primary" @onclick="CountUpWithAmount">Count up with amount</button>

    <div class="mt-5">
        <h3>Set count</h3>
        <input @bind-value="_countToSet" />
    </div>
    <button class="mt-3 btn btn-primary" @onclick="SetCount">Count up with amount</button>
</div>

@code {
    int _amount = 5;
    int _countToSet = 100;

    void IncrementCount() {
        AsyncCounterStore.CountUp();
    }

    async Task IncrementCountAsync() {
        await AsyncCounterStore.CountUpAsync();
    }

    void CountUpMany() {
        AsyncCounterStore.CountUpManyTimes(100);
    }

    void CountUpWithAmount() {
        AsyncCounterStore.CountUpWithAmount(_amount);
    }

    void SetCount() {
        AsyncCounterStore.SetCount(_countToSet);
    }
}

```

Sample Source

https://github.com/le-nn/memento/blob/main/samples/Memento.Sample.Blazor/Stores/RedoUndoTodoStore.cs
https://github.com/le-nn/memento/blob/main/samples/Memento.Sample.Blazor/Components/Counter.razor

DEMO

https://le-nn.github.io/memento/todo

