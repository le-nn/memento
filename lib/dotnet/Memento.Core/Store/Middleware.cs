using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;

namespace Memento.Core.Store;

public abstract class Middleware {
    MiddlewareHandler? _handler;

    public MiddlewareHandler Handler => _handler
        ?? throw new InvalidOperationException($"Middleware '{GetType().FullName}' has not initialized.");

    internal async Task InitializeAsync(IServiceProvider provider) {
        var handler = Create(provider);
        _handler = handler;
        await handler.InitializedAsync();
    }

    protected abstract MiddlewareHandler Create(IServiceProvider provider);
}