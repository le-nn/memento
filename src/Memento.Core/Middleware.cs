﻿namespace Memento.Core;

public abstract class Middleware : IDisposable {
    MiddlewareHandler? _handler;

    public MiddlewareHandler Handler => _handler
        ?? throw new InvalidOperationException($"Middleware '{GetType().FullName}' has not initialized.");

    internal void Initalize(IServiceProvider provider) {
        var handler = Create(provider);
        _handler = handler;
    }

    internal async Task InvokeInitializedAsync() {
        await Handler.InitializedAsync();
    }

    protected abstract MiddlewareHandler Create(IServiceProvider provider);

    public void Dispose() {
        _handler?.Dispose();
    }
}