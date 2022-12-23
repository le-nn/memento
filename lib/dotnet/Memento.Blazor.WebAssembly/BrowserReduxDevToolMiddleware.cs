using Memento.Core;
using Memento.Core.Store;
using Memento.ReduxDevtool.Internals;

namespace Memento.Blazor.Devtools;

public class BrowserReduxDevToolMiddleware : Middleware {
    readonly ReduxDevToolOption _chromiumDevToolOption;

    public BrowserReduxDevToolMiddleware(ReduxDevToolOption? chromiumDevToolOption = null) {
        _chromiumDevToolOption = chromiumDevToolOption ?? new();
    }

    protected override MiddlewareHandler Create(IServiceProvider provider) {
        return new BrowserReduxDevToolMiddlewareHandler(provider, _chromiumDevToolOption);
    }
}