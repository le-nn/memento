using Memento.Core;
using Microsoft.Extensions.DependencyInjection;

namespace Memento.ReduxDevTool.Remote;

public static class RemoveReduxDevToolExtensions {
    /// <summary>
    /// Add <see cref="RemoteReduxDevToolMiddleware"/> to services.
    /// </summary>
    /// <param name="services">The services.</param>
    /// <param name="isScoped">
    /// It will be registered with AddScoped when true specified, otherwise AddSingleton.
    /// </param>
    /// <param name="chromiumDevToolOption">The middleware options.</param>
    /// <param name="hostName">The proxy server hostname.</param>
    /// <param name="port">The proxy server port.</param>
    /// <param name="secure">Https if true specified,otherwise Http.</param>
    /// <returns>The service collection.</returns>
    public static IServiceCollection AddRemoteReduxDevToolMiddleware(
        this IServiceCollection services,
        bool isScoped = false,
        ReduxDevToolOption? chromiumDevToolOption = null,
        string hostName = "0.0.0.0",
        ushort port = 8000,
        bool secure = false
    ) {
        services.AddSingleton(_ => new DevToolWebSocketConnection(hostName, port, secure));

        if (isScoped) {
            services.AddScoped<Middleware>(_ => new RemoteReduxDevToolMiddleware(chromiumDevToolOption));
        }
        else {
            services.AddSingleton<Middleware>(_ => new RemoteReduxDevToolMiddleware(chromiumDevToolOption));
        }

        return services;
    }
}
