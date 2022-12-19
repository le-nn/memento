using Memento.Blazor.Devtools;
using Memento.Core;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using static System.Formats.Asn1.AsnWriter;

namespace Memento.Blazor.Devtools;

public record HistoryState {
    public required int Id { get; set; }
    public required Dictionary<string, IStore> StoreBag { get; init; }
    public required Command Message { get; init; }
    public required string StoreName { get; init; }
    public required Dictionary<string, object> RootState { get; init; }
    public required string Stacktrace { get; init; }
    public required long Timestamp { get; init; }
}

public record StoreAction {
    public required ActionItem Action { get; init; }
    public required long Timestamp { get; init; }
    public required object Stack { get; init; }
    public required string Type { get; init; }
}

public record ComputedState(object State);

public record Init : Command;

public record ActionItem(string Type, object Payload);

public record DevToolStateContext {
    public required Dictionary<int, StoreAction> ActionsById { get; init; }
    public required ImmutableArray<ComputedState> ComputedStates { get; init; }
    public required int CurrentStateIndex { get; init; }
    public required int NextActionId { get; init; }
    public required ImmutableArray<int> SkippedActionIds { get; init; }
    public required ImmutableArray<int> StagedActionIds { get; init; }
}

public class LiftedStore {
    Dictionary<int, HistoryState> _histories = new();
    List<int> _skippedActionIds = new();
    ImmutableArray<int> _stagedActionIds = new();
    int _currentCursor = 0;
    int _sequence = 0;

    int NextActionId => _sequence + 1;

    HistoryState CurrentHistory => _histories[_currentCursor];

    readonly StoreProvider _provider;
    readonly Dictionary<string, object> _rootState;
    readonly Dictionary<string, IStore> _storeBag;
    readonly object _devTool;

    public LiftedStore(
        StoreProvider provider,
        Dictionary<string, object> rootState,
        Dictionary<string, IStore> storeBag,
        object devTool
    ) {
        Reset();
        _provider = provider;
        _rootState = rootState;
        _storeBag = storeBag;
        _devTool = devTool;
    }

    public void Reset() {
        _currentCursor = 0;
        _sequence = 0;
        _stagedActionIds = new() { 0, };
        _skippedActionIds = new();
        _histories = new() {
            {
                0,
                new(){
                  Message =  new Init(),
                    StoreName= "",
                    RootState= _rootState,
                    Id= 0,
                    StoreBag= _storeBag,
                    Stacktrace= "",
                    Timestamp= 0
                }
            }
        };
    }

    public void Push(StateChangedEventArgs e, Dictionary<string, object> rootState) {
        if (_currentCursor != _sequence) {
            return;
        }

        _currentCursor++;
        _sequence++;
        _stagedActionIds = _stagedActionIds.Add(_sequence);
        var _nowTimestamp = (uint)((e.Timestamp.Ticks - DateTime.Parse("1970-01-01 00:00:00").Ticks) / 10000000);

        if (_histories.ContainsKey(_sequence)) {
            _histories[_sequence] = new() {
                Message = e.Command,
                StoreName = e.Sender?.GetType().Name ?? "Error",
                Id = _sequence,
                StoreBag = _storeBag,
                RootState = rootState,
                Stacktrace = "Stack trace",
                Timestamp = _nowTimestamp,
            };
        }

        SyncWithPlugin();
    }

    public void JumpTo(int id) {
        _currentCursor = id;
        var history = _histories[id];
        SetStatesToStore(history);
    }

    public void SetStatesToStore(HistoryState history) {
        if (history is not null) {
            foreach (var storeName in history.StoreBag.Keys) {
                if (storeName == history.StoreName
                    || history.Message is Init
                    || history.StoreBag[storeName].State != history.RootState[storeName]
                ) {
                    // target store should invoke change event
                    history.StoreBag[storeName].__setStateForce(history.RootState[storeName]);
                }
                else {
                    // ignore to invoke change event because update ui is heavy
                    history.StoreBag[storeName].__setStateForceSilently(history.RootState[storeName]);
                }
            }
        }
        else {
            throw new Exception("");
        }
    }

    public void Skip(int id) {
        if (_skippedActionIds.Contains(id)) {
            _skippedActionIds = _skippedActionIds.Where(x => x != id).ToList();
        }
        else {
            _skippedActionIds = _skippedActionIds.ToList();
            _skippedActionIds.Add(id);
        }

        CalcState();
        SyncWithPlugin();
        SetStatesToStore(CurrentHistory);
    }

    public void CalcState() {
        var histories = _histories;
        var newHistories = new Dictionary<int, HistoryState>();

        var beforeState = histories[0].RootState;
        if (beforeState is null) {
            throw new Exception("");
        }

        foreach (var key in _histories.Keys) {
            var history = histories[key];
            // initial or skipped history
            if (key is 0 || _skippedActionIds.Contains(history.Id)) {
                newHistories[key] = history with {
                    RootState = beforeState,
                };
                continue;
            }

            var store = history.StoreBag[history.StoreName];
            var state = store.Reducer(
                beforeState[history.StoreName],
                history.Message
            );

            beforeState[history.StoreName] = state;

            newHistories[key] = newHistories[key] with {
                RootState = beforeState
            };
        }

        _histories = newHistories;
    }

    public void SyncWithPlugin() {
        var sended = new DevToolStateContext() {
            ActionsById = _histories.Keys
                .Select(key => _histories[key])
                .Aggregate(
                    ImmutableDictionary.Create<int, StoreAction>(),
                    (x, y) => x.Add(
                        y.Id,
                        new StoreAction() {
                            Action = new(
                                y.Message.GetType().Name,
                                y.Message
                            ),
                            Type = "PERFORM_ACTION",
                            Stack = y.Stacktrace,
                            Timestamp = y.Timestamp,
                        }
                    )
                ).ToDictionary(x => x.Key, x => x.Value),
            ComputedStates = _histories.Keys
                .Select(key => _histories[key])
                .Select(history => new ComputedState(history.RootState))
                .ToImmutableArray(),
            NextActionId = NextActionId,
            CurrentStateIndex = _currentCursor,
            SkippedActionIds = _skippedActionIds.ToImmutableArray(),
            StagedActionIds = _stagedActionIds.ToImmutableArray(),
        };

        // this.devTool.send(null, sended);
    }
}
