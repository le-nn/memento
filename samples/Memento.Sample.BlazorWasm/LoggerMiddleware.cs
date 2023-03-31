using Memento.Core;
using Microsoft.JSInterop;

namespace Memento.Sample.Blazor;

/// <summary>
/// Middleware for logging state changes.
/// </summary>
public sealed class LoggerMiddleware : Middleware {
    /// <summary>
    /// Creates a new instance of the LoggerMiddlewareHandler.
    /// </summary>
    /// <param name="provider">The service provider used to resolve required services.</param>
    /// <returns>A new LoggerMiddlewareHandler instance.</returns>
    protected override MiddlewareHandler Create(IServiceProvider provider) {
        return new LoggerMiddlewareHandler(
            provider.GetRequiredService<IJSRuntime>()
        );
    }

    /// <summary>
    /// Handler for logging state changes in the LoggerMiddleware.
    /// </summary>
    public class LoggerMiddlewareHandler : MiddlewareHandler {
        private readonly IJSRuntime _jSRuntime;

        /// <summary>
        /// Creates a new instance of the LoggerMiddlewareHandler.
        /// </summary>
        /// <param name="jSRuntime">The JavaScript runtime to be used for logging.</param>
        public LoggerMiddlewareHandler(IJSRuntime jSRuntime) {
            _jSRuntime = jSRuntime;
        }

        /// <summary>
        /// Handles logging the state changes before passing them to the next middleware.
        /// </summary>
        /// <param name="state">The current state of the application.</param>
        /// <param name="e">The state change event arguments.</param>
        /// <param name="next">The next middleware in the pipeline.</param>
        /// <returns>The updated state after processing by the middleware pipeline.</returns>
        public override RootState? HandleProviderDispatch(RootState? state, StateChangedEventArgs e, NextProviderMiddlewareCallback next) {
            _ = HandleLog(state, e);
            return next(state, e);
        }

        /// <summary>
        /// Logs the state changes using the JavaScript console.
        /// </summary>
        /// <param name="state">The current state of the application.</param>
        /// <param name="e">The state change event arguments.</param>
        /// <returns>A task representing the logging operation.</returns>
        public async Task HandleLog(object? state, StateChangedEventArgs e) {
            if (state is null) {
                return;
            }

            await _jSRuntime.InvokeVoidAsync("console.log", new {
                StateName = state.GetType().Name,
                State = state,
                EventArgs = e,
            });
        }
    }
}