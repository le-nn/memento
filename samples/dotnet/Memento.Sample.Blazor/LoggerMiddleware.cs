using Memento.Core.Store;
using Microsoft.JSInterop;
using System.Text.Json;

namespace Memento.Sample.Blazor;

public sealed class LoggerMiddleware : Middleware {
    protected override MiddlewareHandler Create(IServiceProvider provider) {
        return new LoggerMiddlewareHandler(
            provider.GetRequiredService<IJSRuntime>()
        );
    }

    public class LoggerMiddlewareHandler : MiddlewareHandler {
        readonly IJSRuntime _jSRuntime;

        public LoggerMiddlewareHandler(IJSRuntime jSRuntime) {
            _jSRuntime = jSRuntime;
        }

        public override object? Handle(object state, Command command, NextMiddlewareCallback next) {
            _ = HandleLog(state, command);
            return next(state, command);
        }

        public async Task HandleLog(object state, Command command) {
            await _jSRuntime.InvokeVoidAsync("console.log", new {
                StateName = state.GetType().Name,
                State = state,
                Command = command,
            });
        }
    }
}