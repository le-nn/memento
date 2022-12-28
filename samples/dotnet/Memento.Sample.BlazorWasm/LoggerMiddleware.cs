using Memento.Core;
using Memento.Core.Store;
using Microsoft.JSInterop;

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

        public override RootState? HandleProviderDispatch(RootState state, StateChangedEventArgs e, NextProviderMiddlewareCallback next) {
            _ = HandleLog(state, e);
            return base.HandleProviderDispatch(state, e, next);
        }

        public async Task HandleLog(object state, StateChangedEventArgs e) {
            await _jSRuntime.InvokeVoidAsync("console.log", new {
                StateName = state.GetType().Name,
                State = state,
                Event = e,
            });
        }
    }
}