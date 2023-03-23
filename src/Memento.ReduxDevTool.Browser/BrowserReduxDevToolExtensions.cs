using Memento.Core;
using Microsoft.Extensions.DependencyInjection;

namespace Memento.ReduxDevTool.Browser;

public static class BrowserReduxDevToolExtensions {
    /// <summary>
    /// Add <see cref="BrowserReduxDevToolMiddleware"/> to services.
    /// </summary>
    /// <param name="services"></param>
    /// <param name="isScoped">
    /// It will be registered with AddScoped when true specified, otherwise AddSingleton.
    /// </param>
    /// <returns>The service collection.</returns>
    public static IServiceCollection AddBrowserReduxDevToolMiddleware(
        this IServiceCollection services,
        ReduxDevToolOption? option = null,
        bool isScoped = false
    ) {
        if (isScoped) {
            services.AddScoped<Middleware>(_ => new BrowserReduxDevToolMiddleware(option));
        }
        else {
            services.AddSingleton<Middleware>(_ => new BrowserReduxDevToolMiddleware(option));
        }

        return services;
    }
}
