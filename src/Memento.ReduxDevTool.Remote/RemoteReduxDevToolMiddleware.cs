using Memento.Core;

namespace Memento.ReduxDevTool.Remote;

/// <summary>
/// Represents the Redux Developer Tool middleware used for debugging and profiling Redux stores via WebSocket.
/// Connect applications such as Blazor Hybrid, Native Application, and Blazor Server that do not directly use a browser to Redux Dev Tools.
/// </summary>
public sealed class RemoteReduxDevToolMiddleware : Middleware {
    readonly ReduxDevToolOption _chromiumDevToolOption;

    /// <summary>
    /// Initializes a new instance of the <see cref="RemoteReduxDevToolMiddleware" /> class with the specified options and interop handler.
    /// </summary>
    /// <param name="devToolOption">The configuration options for the Redux Developer Tool middleware (optional).</param>
    public RemoteReduxDevToolMiddleware(ReduxDevToolOption? devToolOption = null) {
        _chromiumDevToolOption = devToolOption ?? new();
    }

    protected override MiddlewareHandler Create(IServiceProvider provider) {
        var webSocketConnection = (DevToolWebSocketConnection?)provider.GetService(typeof(DevToolWebSocketConnection))
            ?? new DevToolWebSocketConnection();
        var handler = new RemoteDevToolInteropHandler(webSocketConnection);
        return new ReduxDevToolMiddlewareHandler(handler, provider, _chromiumDevToolOption);
    }
}
