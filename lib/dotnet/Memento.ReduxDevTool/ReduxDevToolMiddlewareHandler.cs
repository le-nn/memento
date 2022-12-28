using Memento.Core;
using Memento.Core.Executors;
using Memento.Core.Store;
using Memento.ReduxDevTool.Internal;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using System.Text.Json;
using System.Text.RegularExpressions;
using static Memento.Core.Command;

namespace Memento.ReduxDevTool;

/// <remarks>
/// Reference to the redux devtool instrument.
/// https://github.com/zalmoxisus/redux-devtools-instrument/blob/master/src/instrument.js
/// </remarks>
public class ReduxDevToolMiddlewareHandler : MiddlewareHandler {
    const string stackTraceFilterExpression =
      @"^(?:(?!\b" +
      @"System" +
      @"|Microsoft" +
      @"|Memento.Blazor" +
      @"|Memento.Core" +
      @"|Memento.ReduxDevTool" +
      @"\b).)*$";

    readonly Regex _stackTraceFilterRegex = new(stackTraceFilterExpression, RegexOptions.Compiled);
    readonly ConcatAsyncOperationExecutor _concatExecutor = new();
    readonly ThrottledExecutor<HistoryStateContextJson> _throttledExecutor = new();
    readonly LiftedHistoryContainer _liftedStore;
    readonly StoreProvider _storeProvider;
    readonly IDevtoolInteropHandler _interopHandler;

    IDisposable? _subscription2;

    public bool IsStackTraceEnabled { get; set; } = true;
    public uint StackTraceLimit { get; set; } = 30;

    public ReduxDevToolMiddlewareHandler(IDevtoolInteropHandler devtoolInteropHandler, IServiceProvider provider, ReduxDevToolOption option) {
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
            SyncReqested = _throttledExecutor.Invoke
        };
        _subscription2 = _throttledExecutor.Subscribe(async sended => {
            await _interopHandler.SendAsync(null, sended);
        });
    }

    public override RootState? HandleProviderDispatch(
        RootState state,
        StateChangedEventArgs e,
        NextProviderMiddlewareCallback next
    ) {
        var stackTrace = string.Join(
            "\r\n",
            new StackTrace(fNeedFileInfo: true)
                .GetFrames()
                .Select(x => $"at {x.GetMethod()?.DeclaringType?.FullName}.{x.GetMethod()?.Name} ({x.GetFileName()}:{x.GetFileLineNumber()}:{x.GetFileColumnNumber()})")
                .Where(x => _stackTraceFilterRegex?.IsMatch(x) is not false)
                .Take((int)StackTraceLimit)
        );

        _ = _concatExecutor.ExecuteAsync(async () => {
            await SendAsync(e, state, stackTrace);
        });

        return next(state, e);
    }

    protected override async Task OnInitializedAsync() {
        await _liftedStore.ResetAsync();
        await _interopHandler.InitializeAsync(_storeProvider.CaptureRootState());
    }

    public async Task SendAsync(StateChangedEventArgs e, RootState rootState, string stackTrace) {
        if (e.Command is ForceReplace) {
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
            Console.WriteLine(ex.StackTrace);
        }
    }

    public override void Dispose() {
        _subscription2?.Dispose();
        _subscription2 = null;
    }
}