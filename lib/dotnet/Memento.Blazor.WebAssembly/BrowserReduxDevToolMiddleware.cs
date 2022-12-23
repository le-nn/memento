using Memento.Core.Store;
using Memento.ReduxDevtool.Internals;

namespace Memento.Blazor.Devtools;

class BrowserReduxDevToolMiddleware : Middleware {
    readonly ReduxDevToolOption _chromiumDevToolOption;

    public BrowserReduxDevToolMiddleware(ReduxDevToolOption? chromiumDevToolOption = null) {
        _chromiumDevToolOption = chromiumDevToolOption ?? new();
    }

    protected override BrowserReduxDevToolMiddlewareHandler Create(IServiceProvider provider) {
        return new(provider, _chromiumDevToolOption);
    }
}