using Memento.Core;

namespace Memento.ReduxDevTool.Remote;

/// <summary>
/// A middleware that connect to Redux devtool.
/// </summary>
public sealed class RemoteReduxDevToolMiddleware : Middleware {
    readonly ReduxDevToolOption _chromiumDevToolOption;

    public RemoteReduxDevToolMiddleware(ReduxDevToolOption? chromiumDevToolOption = null) {
        _chromiumDevToolOption = chromiumDevToolOption ?? new();
    }

    protected override MiddlewareHandler Create(IServiceProvider provider) {
        var webSocketConnection = (DevToolWebSocketConnection?)provider.GetService(typeof(DevToolWebSocketConnection))
            ?? new DevToolWebSocketConnection();
        var handler = new RemoteDevtoolInteropHandler(webSocketConnection);
        return new ReduxDevToolMiddlewareHandler(handler, provider, _chromiumDevToolOption);
    }
}
