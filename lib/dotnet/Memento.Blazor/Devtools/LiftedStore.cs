using Memento.Core;
using System.Collections.Immutable;
using System.Data;

namespace Memento.Blazor.Devtools;

internal record HistoryState {
    public required int Id { get; set; }
    public required Command Message { get; init; }
    public required string StoreBagKey { get; init; }
    public required ImmutableDictionary<string, object> RootState { get; init; }
    public required string Stacktrace { get; init; }
    public required long Timestamp { get; init; }

    public required bool IsSkipped { get; init; }
}

internal record Init : Command;

internal class LiftedStore : IDisposable {
    Dictionary<int, HistoryState> _histories = new();
    int _currentCursor = 0;
    int _sequence = 0;
    int NextActionId => _sequence + 1;
    IDisposable? _subscription;
    HistoryState CurrentHistory => _histories[_currentCursor];

    readonly StoreProvider _provider;
    readonly ChromiumDevToolOption _options;

    public Action<HistoryStateContextJson>? SyncReqested { get; set; }

    public LiftedStore(StoreProvider provider, ChromiumDevToolOption options) {
        _provider = provider;
        _options = options;
    }

    public void Reset() {
        _currentCursor = 0;
        _sequence = 0;
        _histories = new() {
            [0] = new() {
                Message = new Init(),
                StoreBagKey = "",
                RootState = _provider.CaptureRootState(),
                Id = 0,
                Stacktrace = "",
                Timestamp = 0,
                IsSkipped = false,
            }
        };
    }

    public async Task PushAsync(StateChangedEventArgs e, ImmutableDictionary<string, object> rootState) {
        if (_currentCursor != _sequence) {
            return;
        }

        var removeItr = _histories.Keys
            .OrderBy(x => x)
            .Skip(1)
            .Take(_histories.Keys.Count - _options.MaximumHistoryLength);
        foreach (var id in removeItr) {
            _histories.Remove(id);
        }

        _currentCursor++;
        _sequence++;
        var _nowTimestamp = (uint)((e.Timestamp.Ticks - DateTime.Parse("1970-01-01 00:00:00").Ticks) / 10000000);
        _histories[_sequence] = new() {
            Message = e.Command,
            StoreBagKey = e.Sender?.GetType().Name ?? "Error",
            Id = _sequence,
            RootState = rootState,
            Stacktrace = "Stack trace",
            Timestamp = _nowTimestamp,
            IsSkipped = false,
        };

        await SyncWithPlugin();
    }

    public void JumpTo(int id) {
        _currentCursor = id;
        var history = _histories[id];
        SetStatesToStore(history);
    }

    public void SetStatesToStore(HistoryState? history) {
        if (history is not null) {
            var storeBag = _provider.CaptureStoreBag();
            foreach (var storeName in storeBag.Keys) {
                if (storeName == history.StoreBagKey
                    || history.Message is Init
                    || history.RootState[storeName].Equals(storeBag[storeName].State) is false
                ) {
                    // target store should invoke change event
                    storeBag[storeName].__setStateForce(history.RootState[storeName]);
                }
                else {
                    // ignore to invoke change event because updating ui is heavy
                    storeBag[storeName].__setStateForceSilently(history.RootState[storeName]);
                }
            }
        }
        else {
            throw new Exception("");
        }
    }

    public async Task Skip(int id) {
        _histories[id] = _histories[id] with {
            IsSkipped = !_histories[id].IsSkipped,
        };

        CalcState();
        await SyncWithPlugin();
        SetStatesToStore(CurrentHistory);
    }

    public void CalcState() {
        var newHistories = new Dictionary<int, HistoryState>();
        var (firstHistoryKey, firstHistory) = _histories.First();
        var beforeState = firstHistory.RootState ?? throw new Exception("");
        var skippedActionIds = _histories
                .Where(x => x.Value.IsSkipped)
                .Select(x => x.Key)
                .ToHashSet();
        var storeBag = _provider.CaptureStoreBag();

        foreach (var (key, history) in _histories) {
            // initial or skipped history
            if (key == firstHistoryKey || skippedActionIds.Contains(history.Id)) {
                newHistories[key] = history with {
                    RootState = beforeState,
                };
            }
            else {
                var store = storeBag[history.StoreBagKey];
                var state = store.Reducer(
                    beforeState[history.StoreBagKey],
                    history.Message
                );

                beforeState = beforeState.SetItem(history.StoreBagKey, state);

                newHistories[key] = history with {
                    RootState = beforeState.SetItem(history.StoreBagKey, state)
                };
            }
        }

        _histories = newHistories;
    }

    public Task SyncWithPlugin() {
        var sended = new HistoryStateContextJson() {
            ActionsById = _histories.Keys
                .Select(key => _histories[key])
                .Aggregate(
                    ImmutableDictionary.Create<int, StoreAction>(),
                    (x, y) => x.Add(
                        y.Id,
                        new() {
                            Action = new(
                                y.Message.Type,
                                y.Message.Payload
                            ),
                            Type = "PERFORM_ACTION",
                            Stack = y.Stacktrace,
                            Timestamp = y.Timestamp,
                        }
                    )
                ).ToDictionary(x => x.Key, x => x.Value),
            ComputedStates = _histories.Keys
                .OrderBy(x => x)
                .Select(key => _histories[key])
                .Select(history => new ComputedState(history.RootState))
                .ToImmutableArray(),
            NextActionId = NextActionId,
            CurrentStateIndex = _currentCursor,
            SkippedActionIds = _histories
                .Where(x => x.Value.IsSkipped)
                .Select(x => x.Key)
                .ToImmutableArray(),
            StagedActionIds = _histories.Keys.OrderBy(x => x).ToImmutableArray(),
        };

        SyncReqested?.Invoke(sended);

        return Task.CompletedTask;
    }

    public void Dispose() {
        _subscription?.Dispose();
        _subscription = null;
    }
}