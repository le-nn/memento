using Memento.Core;
using Memento.ReduxDevTool.Internals;

namespace Memento.ReduxDevTool;

/// <summary>
/// Represents the Redux Developer Tool middleware used for debugging and profiling Redux stores.
/// </summary>
public sealed class ReduxDevToolMiddleware : Middleware {
    readonly ReduxDevToolOption _chromiumDevToolOption;
    readonly IDevToolInteropHandler _devToolInterop;

    /// <summary>
    /// Initializes a new instance of the <see cref="ReduxDevToolMiddleware" /> class with the specified options and interop handler.
    /// </summary>
    /// <param name="devtoolInterop">The interop handler for communication between the developer tool and the middleware.</param>
    /// <param name="chromiumDevToolOption">The configuration options for the Redux Developer Tool middleware (optional).</param>
    public ReduxDevToolMiddleware(
        IDevToolInteropHandler devToolInterop,
        ReduxDevToolOption? chromiumDevToolOption = null
    ) {
        _chromiumDevToolOption = chromiumDevToolOption ?? new();
        _devToolInterop = devToolInterop;
    }

    /// <summary>
    /// Creates a new <see cref="MiddlewareHandler"/> instance for the Redux Developer Tool middleware.
    /// </summary>
    /// <param name="provider">The service provider used to resolve dependencies.</param>
    /// <returns>A new <see cref="MiddlewareHandler"/> instance for the Redux Developer Tool middleware.</returns>
    protected override MiddlewareHandler Create(IServiceProvider provider) {
        return new ReduxDevToolMiddlewareHandler(_devToolInterop, provider, _chromiumDevToolOption);
    }
}
