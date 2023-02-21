using Memento.Core;
using Memento.ReduxDevTool.Internal;
using Microsoft.JSInterop;
using System.Text.Json;

namespace Memento.ReduxDevTool.Browser;

/// <summary>
/// Interop for dev tools
/// </summary>
internal sealed class JavaScriptDevToolInteropHandler : IDevtoolInteropHandler, IDisposable {
    private const string _sendToReduxDevToolDirectly = "mementoReduxDispatch";
    private const string _toJsInitMethodName = "mementoReduxDevToolInit";
    private const string _reduxDevToolsVariableName = "mementoReduxDevTool";

    public const string DevToolsCallbackId = "mementoReduxDevToolsCallback";

    readonly JsonSerializerOptions _jsonSerializerOptions = new() {
        PropertyNameCaseInsensitive = true,
    };

    private bool _disposed;
    private bool _isInitializing;
    private readonly IJSRuntime _jsRuntime;
    private readonly DotNetObjectReference<JavaScriptDevToolInteropHandler> _dotNetRef;
    private readonly ReduxDevToolOption _option;

    public bool IsInitializing => _isInitializing;

    public Action<string>? MessageHandled { get; set; }
    public Action? SyncRequested { get; set; }

    /// <summary>
    /// Creates an instance of the dev tools interop.
    /// </summary>
    /// <param name="jsRuntime">The jsRuntime.</param>
    public JavaScriptDevToolInteropHandler(IJSRuntime jsRuntime, ReduxDevToolOption option) {
        _jsRuntime = jsRuntime;
        _dotNetRef = DotNetObjectReference.Create(this);
        _option = option;
    }

    public async Task InitializeAsync(RootState state) {
        _isInitializing = true;
        try {
            var script = GetClientScripts(_option);
            await _jsRuntime.InvokeVoidAsync("eval", script);
            await _jsRuntime.InvokeVoidAsync(
                _toJsInitMethodName,
                JsonSerializer.Serialize(state),
                _dotNetRef
            );
        }
        finally {
            _isInitializing = false;
        }
    }

    /// <inheritdoc/>
    public async Task SendAsync(Command? command, HistoryStateContextJson context) {
        await _jsRuntime.InvokeVoidAsync(
            _sendToReduxDevToolDirectly,
            JsonSerializer.Serialize(command, _jsonSerializerOptions),
            JsonSerializer.Serialize(context, _jsonSerializerOptions)
        );
    }

    /// <inheritdoc/>
    [JSInvokable(DevToolsCallbackId)]
    public void HandleMessage(string json) {
        MessageHandled?.Invoke(json);
    }

    /// <inheritdoc/>
    void IDisposable.Dispose() {
        if (!_disposed) {
            _dotNetRef.Dispose();
            _disposed = true;
        }
    }

    static string GetClientScripts(ReduxDevToolOption options) {
        var optionsJson = BuildOptionsJson(options);
        var isOpenDevtool = options.OpenDevTool ? "true" : "false";
        var code = $$"""
            const config = {
                {{optionsJson}},
                features: {
                    pause: true, // start/pause recording of dispatched actions
                    lock: true, // lock/unlock dispatching actions and side effects
                    persist: true, // persist states on page reloading
                    export: true, // export history of actions in a file
                    import: 'custom', // import history of actions from a file
                    jump: true, // jump back and forth (time travelling)
                    skip: true, // skip (cancel) actions
                    reorder: true, // drag and drop actions in the history list
                    dispatch: false, // dispatch custom actions or action creators
                    test: false, // generate tests for the selected actions
                },
            }

            var {{_reduxDevToolsVariableName}} = undefined;
            try {
                 {{_reduxDevToolsVariableName}} = window.__REDUX_DEVTOOLS_EXTENSION__
                    .connect({...config});
            }
            catch{
                console.error("failed to connect redux devtool")
            }

            if (!{{_reduxDevToolsVariableName}}) {
                console.error("failed to connect redux devtool")
            }

            function {{_toJsInitMethodName}}(stateJson, dotnetObj){
                if(!{{_reduxDevToolsVariableName}}){
                    console.error("Redux devtool is not connected");
                    return;
                }
            
                {{_reduxDevToolsVariableName}}.subscribe(message => {
                    const json = JSON.stringify(message);
                    dotnetObj.invokeMethodAsync('{{DevToolsCallbackId}}', json);
                });

                {{_reduxDevToolsVariableName}}.init(JSON.parse(stateJson));
            }

            function {{_sendToReduxDevToolDirectly}}(a, b) {
               {{_reduxDevToolsVariableName}}.send(JSON.parse(a), JSON.parse(b));
            }

            if({{isOpenDevtool}}) {
               window.__REDUX_DEVTOOLS_EXTENSION__.open();
            }
            """;

        return code;
    }

    static string BuildOptionsJson(ReduxDevToolOption options) {
        var stack = options.StackTraceEnabled ? "true" : "false";
        var values = new List<string> {
            $"name: \"{options.Name}\"",
            $"maxAge: {options.MaximumHistoryLength}",
            $"latency: {options.Latency.TotalMilliseconds}",
            $"trace: {stack}",
        };

        return string.Join(",", values);
    }

}
