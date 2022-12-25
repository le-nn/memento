using Memento.Core;
using Memento.Core.Store;

namespace Memento.ReduxDevTool.Remote;

/// <summary>
/// A middleware that connect to Redux devtool.
/// </summary>
public sealed class RemoteReduxDevToolMiddleware : Middleware {
    readonly ReduxDevToolOption _chromiumDevToolOption;
    readonly int _port;

    public RemoteReduxDevToolMiddleware(int port ,ReduxDevToolOption? chromiumDevToolOption = null) {
        _chromiumDevToolOption = chromiumDevToolOption ?? new();
        _port = port;
    }

    protected override MiddlewareHandler Create(IServiceProvider provider) {
        var handler = new SocketDevtoolInteropHandler();

        return new ReduxDevToolMiddlewareHandler(handler, provider, _chromiumDevToolOption);
    }
}
