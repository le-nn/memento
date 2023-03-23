using Memento.Core;
using Memento.Core.Executors;
using Memento.ReduxDevTool.Internals;
using System.Diagnostics;
using System.Text.Json;
using System.Text.RegularExpressions;
using static Memento.Core.Command;

namespace Memento.ReduxDevTool;

/// <remarks>
/// Reference to the redux devtool instrument.
/// https://github.com/zalmoxisus/redux-devtools-instrument/blob/master/src/instrument.js
/// </remarks>
public class ReduxDevToolMiddlewareHandler : MiddlewareHandler {
    const string _stackTraceFilterExpression =
        @"^(?:(?!\b" +
        @"System" +
        @"|Microsoft" +
        @"|Memento.Blazor" +
        @"|Memento.Core" +
        @"|Memento.ReduxDevTool" +
        @"\b).)*$";

    readonly Regex _stackTraceFilterRegex = new(_stackTraceFilterExpression, RegexOptions.Compiled);
    readonly ConcatAsyncOperationExecutor _concatExecutor = new();
    readonly ThrottledExecutor<HistoryStateContextJson> _throttledExecutor = new() { LatencyMs = 1000 };
    readonly LiftedHistoryContainer _liftedStore;
    readonly StoreProvider _storeProvider;
    readonly IDevToolInteropHandler _interopHandler;
    readonly ReduxDevToolOption _option;

    IDisposable? _subscription;

    public ReduxDevToolMiddlewareHandler(IDevToolInteropHandler devtoolInteropHandler, IServiceProvider provider, ReduxDevToolOption option) {
        _option = option;
        _interopHandler = devtoolInteropHandler;
        _interopHandler.MessageHandled = HandleMessage;
        _interopHandler.SyncRequested = () => {
            _liftedStore?.SyncWithPlugin();
        };

        _storeProvider = (StoreProvider)(
            provider.GetService(typeof(StoreProvider))
                ?? throw new Exception("Please register 'StoreProvider' to ServiceProvider")
        );
        _liftedStore = new(_storeProvider, option) {
            SyncRequested = _throttledExecutor.Invoke
        };
        _subscription = _throttledExecutor.Subscribe(async sended => {
            await _interopHandler.SendAsync(null, sended);
        });
    }

    public override object? HandleStoreDispatch(object? state, Command command, NextStoreMiddlewareCallback next) {
        if (_liftedStore.IsJumping) {
            return next(null, command);
        }

        return next(state, command);
    }

    public override RootState? HandleProviderDispatch(
        RootState? state,
        StateChangedEventArgs e,
        NextProviderMiddlewareCallback next
    ) {
        if (state is null) {
            return next(state, e);
        }

        var stackTrace = _option.StackTraceEnabled
            ? string.Join(
                "\r\n",
                new StackTrace(fNeedFileInfo: true)
                    .GetFrames()
                    .Select(x => $"at {x.GetMethod()?.DeclaringType?.FullName}.{x.GetMethod()?.Name} ({x.GetFileName()}:{x.GetFileLineNumber()}:{x.GetFileColumnNumber()})")
                    .Where(x => _stackTraceFilterRegex?.IsMatch(x) is not false)
                    .Take(_option.StackTraceLinesLimit)
            )
            : "";

        _ = _concatExecutor.ExecuteAsync(async () => {
            await SendAsync(e, state, stackTrace);
        });

        if (_liftedStore.IsJumping) {
            return next(null, e);
        }

        return next(state, e);
    }

    protected override async Task OnInitializedAsync() {
        await _interopHandler.InitializeAsync(_storeProvider.CaptureRootState());
        await _liftedStore.ResetAsync();
    }

    public async Task SendAsync(StateChangedEventArgs e, RootState rootState, string stackTrace) {
        if (e.Command is ForceReplaced) {
            return;
        }

        await _liftedStore.PushAsync(e, rootState, stackTrace);
    }

    async void HandleMessage(string json) {
        try {
            var command = JsonSerializer.Deserialize<ActionItemFromDevtool>(
                json,
                new JsonSerializerOptions() {
                    PropertyNameCaseInsensitive = true,
                });

            if (
                command?.Payload is { } payload
                && command?.Payload?.TryGetProperty("type", out var p) is true
                && p.GetString() is { } val
            ) {
                switch (val.ToString()) {
                    case "RESET":
                        await _liftedStore.ResetAsync();
                        break;
                    case "COMMIT":
                        await _liftedStore.CommitAsync();
                        break;
                    case "SWEEP":
                        await _liftedStore.SweepAsync();
                        break;
                    case "ROLLBACK":
                        await _liftedStore.RollbackAsync();
                        break;
                    case "PAUSE_RECORDING":
                        _liftedStore.IsPaused = payload.GetProperty("status").GetBoolean();
                        break;
                    case "REORDER_ACTION":
                        await _liftedStore.ReorderActionsAsync(
                            payload.GetProperty("actionId").GetInt32(),
                           payload.GetProperty("beforeActionId").GetInt32()
                        );
                        break;
                    case "JUMP_TO_STATE":
                    case "TOGGLE_ACTION":
                        _ = _liftedStore.SkipAsync(
                            command.Payload?.GetProperty("id").GetInt32() ?? -1
                        );
                        break;
                    case "LOCK_CHANGES":
                        await _liftedStore.LockChangedAsync(payload.GetProperty("status").GetBoolean());
                        break;
                    case "JUMP_TO_ACTION":
                        _liftedStore.JumpTo(
                           payload.GetProperty("actionId").GetInt32()
                        );
                        break;
                    case "IMPORT_STATE":
                        await _liftedStore.ImportAsync(payload.GetProperty("nextLiftedState"));
                        break;
                }
            }
        }
        catch (Exception ex) {
            // TODO: HandleProviderDispatch logger
            Console.WriteLine("Redux DevTool Error :");
            Console.WriteLine(ex.Message);
            Console.WriteLine("StackTrace");
            Console.WriteLine(ex.StackTrace);
            Console.WriteLine("json :");
            Console.WriteLine(json);
        }
    }

    public override void Dispose() {
        _subscription?.Dispose();
        _subscription = null;
    }
}