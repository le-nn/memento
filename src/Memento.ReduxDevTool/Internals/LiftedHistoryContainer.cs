using Memento.Core;
using System.Collections.Immutable;
using System.Data;
using System.Reflection;
using System.Text.Json;

namespace Memento.ReduxDevTool.Internals;
internal record HistoryState {
    public required int Id { get; set; }
    public required IStateChangedEventArgs<object>? Command { get; init; }
    public required string StoreBagKey { get; init; }
    public required Dictionary<string, object> RootState { get; init; }
    public required string? Stacktrace { get; init; }
    public required long Timestamp { get; init; }
    public required bool IsSkipped { get; init; }

    public HistoryState AsInitial() {
        return this with {
            Command = new StateChangedEventArgs<Init> {
                Message = new Init(),
                StateChangeType = StateChangeType.ForceReplaced,
                Sender = this,
            },
            StoreBagKey = "@@INIT",
            Id = 0,
            Timestamp = LiftedHistoryContainer.ToUnixTimeStamp(DateTime.UtcNow),
        };
    }
}

internal record Init {
    public string Type { get; } = "@@INIT";
}

internal sealed class LiftedHistoryContainer : IDisposable {
    ImmutableArray<HistoryState> _histories = [];
    int _currentCursorId = 0;
    int _sequence = 0;
    IDisposable? _subscription;

    readonly RootState _rootState;
    readonly StoreProvider _provider;
    readonly ReduxDevToolOption _options;

    int NextActionId => _sequence + 1;

    HistoryState CurrentHistory => _histories.Where(x => x.Id == _currentCursorId).First();

    public Action<HistoryStateContextJson>? SyncRequested { get; set; }

    public bool IsPaused { get; set; }

    public bool IsLocked { get; set; }

    public bool IsJumping => _histories is not [] && _histories.Max(x => x.Id) > _currentCursorId;

    public LiftedHistoryContainer(StoreProvider provider, ReduxDevToolOption options) {
        _provider = provider;
        _options = options;
        _rootState = _provider.CaptureRootState();
    }

    public async Task ResetAsync() {
        _currentCursorId = 0;
        _sequence = 0;
        _histories = [
            new HistoryState() {
                Command = new StateChangedEventArgs<Init> {
                    Message = new Init(),
                    StateChangeType = StateChangeType.ForceReplaced,
                    Sender = this,
                },
                StoreBagKey = "",
                Id = 0,
                RootState = _rootState.AsDictionary(),
                Stacktrace = "",
                Timestamp = ToUnixTimeStamp(DateTime.UtcNow),
                IsSkipped = false,
            }
        ];
        await SyncWithPlugin();
        SetStatesToStore(CurrentHistory);
    }

    public async Task LockChangedAsync(bool isLocked) {
        IsLocked = isLocked;
        await SyncWithPlugin();
    }

    public async Task PushAsync(IStateChangedEventArgs<object, object> e, RootState rootState, string stackTrace) {
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
            Command = e,
            StoreBagKey = e.Sender?.GetType().Name ?? "Error",
            Id = _sequence,
            RootState = rootState.AsDictionary(),
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
                var targetStore = storeBag[storeName];
                if (storeName == history.StoreBagKey
                    || history.Command is Init
                    || history.RootState[storeName].Equals(targetStore.State) is false
                ) {
                    // target store should invoke change event
                    targetStore.SetStateForce(history.RootState[storeName]);
                }
                else {
                    // ignore to invoke change event because updating ui is heavy
                    targetStore.SetStateForceSilently(history.RootState[storeName]);
                }
            }
        }
        else {
            throw new Exception("");
        }
    }

    public async Task CommitAsync() {
        var history = CurrentHistory;
        _histories = [history.AsInitial()];

        await SyncWithPlugin();
        _currentCursorId = 0;
        SetStatesToStore(CurrentHistory);
    }

    public async Task RollbackAsync() {
        var history = _histories.First();
        _histories = [history.AsInitial()];

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
                var state = store.ReducerHandle(
                    beforeState[history.StoreBagKey],
                    history.Command!
                );

                beforeState[history.StoreBagKey] = state;

                newHistories.Add(history with {
                    RootState = beforeState
                });
            }
        }

        _histories = newHistories.ToImmutable();
    }

    public Task SyncWithPlugin() {
        SyncRequested?.Invoke(Serialize());

        return Task.CompletedTask;
    }

    public HistoryStateContextJson Serialize() {
        var dic = new Dictionary<int, StoreAction>();
        foreach (var h in _histories) {
            dic.Add(
                h.Id,
                new() {
                    Action = new(
                        h.StoreBagKey,
                        h.Command?.Message?.Payload(),
                        h.Command?.Message?.GetFullTypeName(),
                        h.StoreBagKey
                    ),
                    Type = "PERFORM_ACTION",
                    Stack = h.Stacktrace,
                    Timestamp = h.Timestamp,
                }
            );
        }

        return new HistoryStateContextJson() {
            ActionsById = dic,
            ComputedStates = _histories
                  .Select(history => new ComputedState(history.RootState))
                  .ToArray(),
            NextActionId = NextActionId,
            CurrentStateIndex = _currentCursorId,
            SkippedActionIds = _histories
                  .Where(x => x.IsSkipped)
                  .Select(x => x.Id)
                  .ToArray(),
            StagedActionIds = _histories
                  .Select(x => x.Id)
                  .ToArray(),
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
                    Command = new StateChangedEventArgs<object>() {
                        Message = DeserializeCommand(action.Action.DeclaredType, action.Action.Payload),
                        Sender = null,
                        StateChangeType = StateChangeType.Restored,
                    },
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

    static Dictionary<string, object> DeserializeStates(Dictionary<string, IStore<object, object>> storeBag, JsonElement stateJson) {
        var rootState = new Dictionary<string, object>();
        foreach (var key in stateJson.EnumerateObject()) {
            var storeState = key.Value.Deserialize(storeBag[key.Name].GetStateType())
                ?? throw new Exception("failed to deserialize state.");

            rootState.Add(key.Name, storeState);
        }

        return rootState;
    }

    static object DeserializeCommand(string? typeName, object? payload) {
        if (typeName is null) {
            return new Init();
        }

        foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies()) {
            var type = assembly.GetType(typeName);
            if (type is not null) {
                var command = JsonSerializer.Deserialize(
                    JsonSerializer.Serialize(payload),
                    type
                );

                return command ?? throw new Exception($"Failed to serialize historyStateContextJson.");
            }
        }

        throw new Exception($"type '{typeName}' is not found.");
    }

    public static long ToUnixTimeStamp(DateTime dateTime) {
        return (long)(dateTime - new DateTime(1970, 1, 1)).TotalMilliseconds;
    }
}

internal static class Ex {
    /// <summary>
    /// Gets the full type name of the command.
    /// </summary>
    /// <returns>The full type name as a string.</returns>
    public static string? GetFullTypeName(this object obj) {
        return obj.GetType().FullName;
    }

    /// <summary>
    /// Gets the payload properties of the command as a dictionary.
    /// </summary>
    public static Dictionary<string, object> Payload(this object obj) => GetPayloads(obj);

    /// <summary>
    /// Gets the payload properties of the command as a collection of key-value pairs.
    /// </summary>
    /// <returns>An enumerable collection of key-value pairs representing the payload properties.</returns>
    static Dictionary<string, object> GetPayloads(this object obj) {
        var dic = new Dictionary<string, object>();
        foreach (var property in obj.GetType().GetProperties(BindingFlags.Instance)) {
            if (property.Name is nameof(Payload) or nameof(Type)) {
                continue;
            }

            var value = property.GetValue(obj);
            if (value is not null && dic.ContainsKey(property.Name)) {
                dic.Add(property.Name, value);
            }
        }

        return dic;
    }
}