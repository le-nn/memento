using Memento.Core;
using Memento.Core.Executors;
using Memento.Core.Store;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.JSInterop;
using System.Collections.Immutable;
using System.Text.Json;
using static Memento.Core.Command;

namespace Memento.Blazor.Devtools;

public class ChromiumDevToolMiddleware : Middleware<ChromiumDevToolMiddlewareHandler> {
    readonly ChromiumDevToolOption _chromiumDevToolOption;

    public ChromiumDevToolMiddleware(ChromiumDevToolOption? chromiumDevToolOption = null) {
        _chromiumDevToolOption = chromiumDevToolOption ?? new();
    }

    protected override ChromiumDevToolMiddlewareHandler Create(IServiceProvider provider) {
        return new(provider, _chromiumDevToolOption);
    }
}

public class ChromiumDevToolMiddlewareHandler : MiddlewareHandler {
    IDisposable? _subscription1;
    IDisposable? _subscription2;

    readonly DevToolJsInterop _jsInterop;
    readonly StoreProvider _storeProvider;
    readonly ConcatAsyncOperationExecutor _executor = new();
    readonly LiftedStore _liftedStore;
    readonly ThrottledExecutor<HistoryStateContextJson> _throttledExecutor = new();

    public ChromiumDevToolMiddlewareHandler(IServiceProvider provider, ChromiumDevToolOption option) {
        _jsInterop = new(
            provider.GetRequiredService<IJSRuntime>(),
            HandleMessage
        );
        _storeProvider = provider.GetRequiredService<StoreProvider>();
        _liftedStore = new(_storeProvider, option) {
            SyncReqested = _throttledExecutor.Invoke
        };
        _subscription2 = _throttledExecutor.Subscribe(async sended => {
            await _jsInterop.SendAsync(null, sended);
        });
    }

    protected override async Task OnInitializedAsync() {
        _liftedStore?.Reset();
        await _jsInterop.InitializeAsync(_storeProvider.CaptureRootState());

        _subscription1 = _storeProvider.Subscribe(e => {
            _ = _executor.ExecuteAsync(async () => {
                await SendAsync(e, e.RootState);
            });
        });
    }

    public async Task SendAsync(RootStateChangedEventArgs e, ImmutableDictionary<string, object> rootState) {
        if (e.StateChangedEvent.Command is ForceReplace) {
            return;
        }

        await (
            _liftedStore?.PushAsync(e.StateChangedEvent, rootState)
                ?? Task.CompletedTask
        );
    }

    void HandleMessage(string json) {
        try {
            var command = JsonSerializer.Deserialize<ActionItemFromDevtool>(
                json,
                new JsonSerializerOptions() {
                    PropertyNameCaseInsensitive = true,
                });

            if (command?.Payload?.TryGetValue("type", out var val) is true) {
                switch (val.ToString()) {
                    case "COMMIT":
                        break;
                    case "JUMP_TO_STATE":
                    case "TOGGLE_ACTION":
                        _liftedStore?.Skip(
                            (command.Payload["id"]).GetInt32()
                        );
                        break;
                    case "JUMP_TO_ACTION":
                        _liftedStore?.JumpTo(
                            (command.Payload["actionId"]).GetInt32()
                        );
                        break;
                }
            }
        }
        catch (Exception ex) {
            Console.WriteLine(ex.Message);
        }
    }

    public override void Dispose() {
        _subscription1?.Dispose();
        _subscription2?.Dispose();
        _subscription1 = null;
        _subscription2 = null;
    }
}