using Memento.Core;
using Memento.Core.Store;
using Memento.ReduxDevtool;
using Memento.ReduxDevtool.Browser;
using Microsoft.JSInterop;

namespace Memento.Blazor.Devtools.Browser;

/// <summary>
/// A middleware that connect to Redux devtool.
/// </summary>
public sealed class BrowserReduxDevToolMiddleware : Middleware {
    readonly ReduxDevToolOption _chromiumDevToolOption;

    public BrowserReduxDevToolMiddleware(ReduxDevToolOption? chromiumDevToolOption = null) {
        _chromiumDevToolOption = chromiumDevToolOption ?? new();
    }

    protected override MiddlewareHandler Create(IServiceProvider provider) {
        var jsHandler = new JavascriptDevToolInteropHandler((IJSRuntime)(
            provider.GetService(typeof(IJSRuntime)
        ) ?? throw new Exception()));
        return new ReduxDevToolMiddlewareHandler(jsHandler, provider, _chromiumDevToolOption);
    }
}