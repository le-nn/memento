﻿using Memento.Core;
using Microsoft.JSInterop;
using System.Collections.Immutable;
using System.Text.Json;

using static Memento.Core.Command;

namespace Memento.Blazor.Devtools;

public record ActionItemFromDevtool(string Type, Dictionary<string, JsonElement>? Payload, string? Source);

/// <summary>
/// Interop for dev tools
/// </summary>
internal sealed class DevToolJsInterop : IDevtoolInteropHandler, IDisposable {
    private const string _sendToReduxDevToolDirectly = "mementoReduxDispatch";
    private const string _toJsInitMethodName = "mementoReduxDevToolInit";
    private const string _reduxDevToolsVariableName = "mementoReduxDevTool";

    public const string DevToolsCallbackId = "mementoReduxDevToolsCallback";

    readonly Action<string> _onMessageHandled;
    readonly JsonSerializerOptions _jsonSerializerOptions = new() {
        PropertyNameCaseInsensitive = true,
    };

    private bool _disposed;
    private bool _isInitializing;
    private readonly IJSRuntime _jsRuntime;
    private readonly DotNetObjectReference<DevToolJsInterop> _dotNetRef;

    public bool IsInitializing => _isInitializing;

    /// <summary>
    /// Creates an instance of the dev tools interop.
    /// </summary>
    /// <param name="jsRuntime">The jsRuntime.</param>
    /// <param name="onMessageHandled">The handler when the devtool message has recieved. </param>
    public DevToolJsInterop(
        IJSRuntime jsRuntime,
        Action<string> onMessageHandled
    ) {
        _jsRuntime = jsRuntime;
        _dotNetRef = DotNetObjectReference.Create(this);
        _onMessageHandled = onMessageHandled;
    }

    public async Task InitializeAsync(ImmutableDictionary<string, object> state) {
        _isInitializing = true;
        try {
            var script = GetClientScripts(new());
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
        await _jsRuntime.InvokeVoidAsync("console.log", context);
    }

    /// <inheritdoc/>
    [JSInvokable(DevToolsCallbackId)]
    public void HandleMessage(string json) {
        _onMessageHandled(json);
    }

    /// <inheritdoc/>
    void IDisposable.Dispose() {
        if (!_disposed) {
            _dotNetRef.Dispose();
            _disposed = true;
        }
    }

    static string GetClientScripts(ChromiumDevToolOption options) {
        var optionsJson = BuildOptionsJson(options);

        var code = $$"""
            var {{_reduxDevToolsVariableName}} = undefined;
            try {
                 {{_reduxDevToolsVariableName}} = window.__REDUX_DEVTOOLS_EXTENSION__
                    .connect({
                        {{optionsJson}},
                        features: {
                            pause: true, // start/pause recording of dispatched actions
                            lock: true, // lock/unlock dispatching actions and side effects
                            persist: false, // persist states on page reloading
                            export: true, // export history of actions in a file
                            import: 'custom', // import history of actions from a file
                            jump: true, // jump back and forth (time travelling)
                            skip: true, // skip (cancel) actions
                            reorder: true, // drag and drop actions in the history list
                            dispatch: false, // dispatch custom actions or action creators
                            test: false, // generate tests for the selected actions
                        },
                    });
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

            function {{_sendToReduxDevToolDirectly}}(a,b) {
                {{_reduxDevToolsVariableName}}.send(JSON.parse(a), JSON.parse(b));
            }
            """;

        return code;
    }

    static string BuildOptionsJson(ChromiumDevToolOption options) {
        var values = new List<string> {
            $"name:\"{options.Name}\"",
            $"maxAge:{options.MaximumHistoryLength}",
            $"latency:{options.Latency.TotalMilliseconds}"
        };
        if (options.StackTraceEnabled is true) {
            values.Add("trace: function() { return JSON.parse(window.mementoDevToolsDotNetInterop.stackTrace); }");
        }

        return string.Join(",", values);
    }

}
