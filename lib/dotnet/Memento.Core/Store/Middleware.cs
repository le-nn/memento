using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;

namespace Memento.Core.Store;

public interface IMiddleware {
    MiddlewareHandler Handler { get; }

    internal Task InitializeAsync(IServiceProvider provide);
}

public abstract class Middleware : IMiddleware {
    MiddlewareHandler? _handler;

    public MiddlewareHandler Handler => _handler
        ?? throw new InvalidOperationException($"Middleware '{GetType().FullName}' has not initialized.");

    async Task IMiddleware.InitializeAsync(IServiceProvider provider) {
        var handler = Create(provider);
        _handler = handler;
        await handler.InitializedAsync();
    }

    protected abstract MiddlewareHandler Create(IServiceProvider provider);
}
