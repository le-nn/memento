using Memento.Core;
using Memento.ReduxDevTool.Internals;

namespace Memento.ReduxDevTool;

public sealed class ReduxDevToolMiddleware : Middleware {
    readonly ReduxDevToolOption _chromiumDevToolOption;
    readonly IDevtoolInteropHandler _devtoolInterop;

    public ReduxDevToolMiddleware(
        IDevtoolInteropHandler devtoolInterop,
        ReduxDevToolOption? chromiumDevToolOption = null
    ) {
        _chromiumDevToolOption = chromiumDevToolOption ?? new();
        _devtoolInterop = devtoolInterop;
    }

    protected override MiddlewareHandler Create(IServiceProvider provider) {
        return new ReduxDevToolMiddlewareHandler(_devtoolInterop, provider, _chromiumDevToolOption);
    }
}
