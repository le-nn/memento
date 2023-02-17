using Memento.Core;
using Memento.Core.Store;
using System.Collections.Immutable;
using System.Data;
using System.Text.Json;

namespace Memento.ReduxDevTool.Internal;

internal record HistoryState {
    public required int Id { get; set; }
    public required Command Command { get; init; }
    public required string StoreBagKey { get; init; }
    public required ImmutableDictionary<string, object> RootState { get; init; }
    public required string? Stacktrace { get; init; }
    public required long Timestamp { get; init; }
    public required bool IsSkipped { get; init; }

    public HistoryState AsInitial() {
        return this with {
            Command = new Init(),
            Id = 0,
            Timestamp = LiftedHistoryContainer.ToUnixTimeStamp(DateTime.UtcNow),
        };
    }
}

internal record Init : Command {
    public override string Type => "@@INIT";
}

internal sealed class LiftedHistoryContainer : IDisposable {
    ImmutableArray<HistoryState> _histories = ImmutableArray.Create<HistoryState>();
    int _currentCursorId = 0;
    int _sequence = 0;
    int NextActionId => _sequence + 1;
    IDisposable? _subscription;
    HistoryState CurrentHistory => _histories.Where(x => x.Id == _currentCursorId).First();
    readonly RootState _rootState;
    readonly StoreProvider _provider;
    readonly ReduxDevToolOption _options;
    public Action<HistoryStateContextJson>? SyncReqested { get; set; }

    public bool IsPaused { get; set; }

    public bool IsLocked { get; set; }

    public static long ToUnixTimeStamp(DateTime dateTime) {
        return (long)(dateTime - new DateTime(1970, 1, 1)).TotalMilliseconds;
    }

    public LiftedHistoryContainer(StoreProvider provider, ReduxDevToolOption options) {
        _provider = provider;
        _options = options;
        _rootState = _provider.CaptureRootState();
    }

    public async Task ResetAsync() {
        _currentCursorId = 0;
        _sequence = 0;
        _histories = ImmutableArray.Create(
            new HistoryState() {
                Command = new Init(),
                StoreBagKey = "",
                Id = 0,
                RootState = _rootState.AsImmutableDictionary(),
                Stacktrace = "",
                Timestamp = ToUnixTimeStamp(DateTime.UtcNow),
                IsSkipped = false,
            }
        );
        await SyncWithPlugin();
        SetStatesToStore(CurrentHistory);
    }

    public async Task LockChangedAsync(bool isLocked) {
        IsLocked = isLocked;
        await SyncWithPlugin();
    }

    public async Task PushAsync(StateChangedEventArgs e, RootState rootState, string stackTrace) {
        if (IsLocked) {
            await SyncWithPlugin();
            SetStatesToStore(CurrentHistory);
            return;
        }

        if (IsPaused) {
            return;
        }

        // adjust maxAge
        _histories = _histories
            .Skip(_histories.Length - (int)_options.MaximumHistoryLength)
            .ToImmutableArray();
        _histories = _histories.SetItem(0, _histories[0].AsInitial());

        var isJumping = _currentCursorId == _histories.Last().Id;

        // add new history
        _sequence++;
        _histories = _histories.Add(new() {
            Command = e.Command,
            StoreBagKey = e.Sender?.GetType().Name ?? "Error",
            Id = _sequence,
            RootState = rootState.AsImmutableDictionary(),
            Stacktrace = stackTrace,
            Timestamp = ToUnixTimeStamp(DateTime.UtcNow),
            IsSkipped = false,
        });

        var nextCursor = _histories.LastOrDefault()?.Id ?? _currentCursorId;
        if (isJumping) {
            _currentCursorId = nextCursor;
        }
        else {
            CalcState();
        }

        await SyncWithPlugin();
        SetStatesToStore(CurrentHistory);
    }

    public void UpdateStoreStateWithCurrent() {
        SetStatesToStore(CurrentHistory);
    }

    public void JumpTo(int id) {
        _currentCursorId = id;

        var history = _histories.Where(x => id == x.Id).FirstOrDefault()
            ?? throw new InvalidOperationException($"id '{id}' is not found.");
        SetStatesToStore(history);
    }

    public void SetStatesToStore(HistoryState? history) {
        if (history is not null) {
            var storeBag = _provider.CaptureStoreBag();
            foreach (var storeName in storeBag.Keys) {
                if (storeName == history.StoreBagKey
                    || history.Command is Init
                    || history.RootState[storeName].Equals(storeBag[storeName].State) is false
                ) {
                    // target store should invoke change event
                    storeBag[storeName].SetStateForce(history.RootState[storeName]);
                }
                else {
                    // ignore to invoke change event because updating ui is heavy
                    storeBag[storeName].SetStateForceSilently(history.RootState[storeName]);
                }
            }
        }
        else {
            throw new Exception("");
        }
    }

    public async Task CommitAsync() {
        var history = CurrentHistory;
        _histories = ImmutableArray.Create(history.AsInitial());

        await SyncWithPlugin();
        _currentCursorId = 0;
        SetStatesToStore(CurrentHistory);
    }

    public async Task RollbackAsync() {
        var history = _histories.First();
        _histories = ImmutableArray.Create(history.AsInitial());

        await SyncWithPlugin();
        _currentCursorId = 0;
        SetStatesToStore(CurrentHistory);
    }

    public async Task ReorderActionsAsync(int actionId, int beforeActionId) {
        var histories = _histories.Where(x => x.Id != actionId).ToImmutableArray();
        var target = _histories.Where(x => x.Id == actionId).First();
        var before = _histories.Where(x => x.Id == beforeActionId).First();

        var index = histories.IndexOf(before);
        _histories = histories.Insert(index, target);

        CalcState();
        await SyncWithPlugin();
    }

    public async Task SweepAsync() {
        var history = _histories.First();
        _histories = _histories
            .Where(x => x.IsSkipped is false)
            .ToImmutableArray();

        // decrement current cursor id until history is found
        while (_histories.Where(x => x.Id == _currentCursorId).Any() is false) {
            _currentCursorId--;
        }

        await SyncWithPlugin();
        SetStatesToStore(CurrentHistory);
    }

    public async Task SkipAsync(int id) {
        var history = _histories.Where(x => id == x.Id).FirstOrDefault()
            ?? throw new InvalidOperationException($"id '{id}' is not found.");

        _histories = _histories.SetItem(
            _histories.IndexOf(history),
            history with {
                IsSkipped = !history.IsSkipped,
            }
        );

        CalcState();
        await SyncWithPlugin();
        SetStatesToStore(CurrentHistory);
    }

    public void CalcState() {
        var newHistories = ImmutableArray.CreateBuilder<HistoryState>();
        var firstHistory = _histories.First();
        var beforeState = firstHistory.RootState ?? throw new Exception("");
        var skippedActionIds = _histories
            .Where(x => x.IsSkipped)
            .Select(x => x.Id)
            .ToHashSet();
        var storeBag = _provider.CaptureStoreBag();

        foreach (var (i, history) in _histories.Select((x, i) => (i, x))) {
            // initial or skipped history
            if (i is 0 || skippedActionIds.Contains(history.Id)) {
                newHistories.Add(history with {
                    RootState = beforeState,
                });
            }
            else {
                var store = storeBag[history.StoreBagKey];
                var state = store.Reducer(
                    beforeState[history.StoreBagKey],
                    history.Command
                );

                beforeState = beforeState.SetItem(history.StoreBagKey, state);

                newHistories.Add(history with {
                    RootState = beforeState
                });
            }
        }

        _histories = newHistories.ToImmutable();
    }

    public Task SyncWithPlugin() {
        SyncReqested?.Invoke(Serialize());

        return Task.CompletedTask;
    }

    public HistoryStateContextJson Serialize() {
        return new HistoryStateContextJson() {
            ActionsById = _histories
                  .Aggregate(
                      ImmutableDictionary.Create<int, StoreAction>(),
                      (x, y) => x.Add(
                          y.Id,
                          new() {
                              Action = new(
                                  y.Command.Type,
                                  y.Command.Payload,
                                  y.Command.GetFullTypeName(),
                                  y.StoreBagKey
                              ),
                              Type = "PERFORM_ACTION",
                              Stack = y.Stacktrace,
                              Timestamp = y.Timestamp,
                          }
                      )
                  )
                  .ToDictionary(x => x.Key, x => x.Value),
            ComputedStates = _histories
                  .Select(history => new ComputedState(history.RootState))
                  .ToImmutableArray(),
            NextActionId = NextActionId,
            CurrentStateIndex = _currentCursorId,
            SkippedActionIds = _histories
                  .Where(x => x.IsSkipped)
                  .Select(x => x.Id)
                  .ToImmutableArray(),
            StagedActionIds = _histories
                  .Select(x => x.Id)
                  .ToImmutableArray(),
            IsLocked = IsLocked,
            IsPaused = IsPaused,
        };
    }

    public async Task ImportAsync(JsonElement jsonElement) {
        var historyStateContextJson = jsonElement.Deserialize<HistoryStateContextJson>()!;
        var skipActions = historyStateContextJson.SkippedActionIds.ToHashSet();
        var computedStates = jsonElement.GetProperty("computedStates").EnumerateArray().ToArray();
        var storeBag = _provider.CaptureStoreBag();

        _histories = historyStateContextJson.StagedActionIds
            .Select((id, i) => {
                var action = historyStateContextJson.ActionsById[id];
                var command = DeserializeCommand(action.Action.DeclaredType, action.Action.Payload);
                return new HistoryState() {
                    StoreBagKey = action.Action.StoreName,
                    RootState = DeserializeStates(storeBag, computedStates[i].GetProperty("state")),
                    Id = id,
                    Command = DeserializeCommand(action.Action.DeclaredType, action.Action.Payload),
                    Stacktrace = action.Stack,
                    Timestamp = action.Timestamp,
                    IsSkipped = skipActions.Contains(id),
                };
            }).ToImmutableArray();

        _currentCursorId = _histories[historyStateContextJson.CurrentStateIndex].Id;
        IsLocked = historyStateContextJson.IsLocked;
        IsPaused = historyStateContextJson.IsPaused;
        _sequence = historyStateContextJson.NextActionId - 1;

        CalcState();
        await SyncWithPlugin();
        SetStatesToStore(CurrentHistory);
    }

    public void Dispose() {
        _subscription?.Dispose();
        _subscription = null;
    }

    static ImmutableDictionary<string, object> DeserializeStates(ImmutableDictionary<string, IStore> storeBag, JsonElement stateJson) {
        var rootState = ImmutableDictionary.CreateBuilder<string, object>();
        foreach (var key in stateJson.EnumerateObject()) {
            var storeState = key.Value.Deserialize(storeBag[key.Name].GetStateType())
                ?? throw new Exception("failed to deserialize state.");

            rootState.Add(key.Name, storeState);
        }

        return rootState.ToImmutable();
    }

    static Command DeserializeCommand(string? typeName, object? payload) {
        if (typeName is null) {
            return new Init();
        }

        foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies()) {
            var type = assembly.GetType(typeName);
            if (type is not null) {
                var command = (Command?)JsonSerializer.Deserialize(
                    JsonSerializer.Serialize(payload),
                    type
                );

                return command ?? throw new Exception($"Failed to serialize historyStateContextJson.");
            }
        }

        throw new Exception($"type '{typeName}' is not found.");
    }
}