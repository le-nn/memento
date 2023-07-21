using Memento.Core;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace Memento.Blazor;

/// <summary>
/// Extension methods for configuring store-related services.
/// </summary>
public static class StoreConfigExtension {
    /// <summary>
    /// Adds the Memento service to the IServiceCollection.
    /// </summary>
    /// <param name="isScoped">If true, registers the StoreProvider with a scoped lifetime. Otherwise, registers with a singleton lifetime.</param>
    /// <returns>The registered IServiceCollection instance from the IServiceCollection.</returns>
    public static IServiceCollection AddMemento(this IServiceCollection services, bool isScoped = true) {
        if (isScoped) {
            services.AddScoped<StoreProvider>();
        }
        else {
            services.AddSingleton<StoreProvider>();
        }
        return services;
    }

    /// <summary>
    /// Adds a custom store to the IServiceCollection.
    /// </summary>
    /// <param name="isScoped">If true, registers the store with a scoped lifetime. Otherwise, registers with a singleton lifetime.</param>
    /// <returns>>The registered IServiceCollection instance from the IServiceCollection.</returns>
    public static IServiceCollection AddStore<TStore>(this IServiceCollection collection, bool isScoped = true)
        where TStore : class,  IStore<object,Command> {
        if (isScoped) {
            collection.AddScoped<TStore>()
                .AddScoped< IStore<object,Command>>(p => p.GetRequiredService<TStore>());
        }
        else {
            collection.AddSingleton<TStore>()
                .AddSingleton< IStore<object,Command>>(p => p.GetRequiredService<TStore>());
        }
        return collection;
    }

    /// <summary>
    /// Scans the assembly and adds all stores that implement <see cref="IStore"/> to the IServiceCollection.
    /// </summary>
    /// <param name="isScoped">If true, registers the stores with a scoped lifetime. Otherwise, registers with a singleton lifetime.</param>
    /// <returns>>The registered IServiceCollection instance from the IServiceCollection.</returns>
    public static void ScanAssemblyAndAddStores(this IServiceCollection services, Assembly assembly, bool isScoped = true) {
        foreach (var type in assembly.GetTypes().Where(t => t.IsAssignableTo(typeof( IStore<object,Command>)))) {
            if (isScoped) {
                services.AddScoped(type)
                    .AddScoped(p => ( IStore<object,Command>)p.GetRequiredService(type));
            }
            else {
                services.AddSingleton(type)
                    .AddSingleton(p => ( IStore<object,Command>)p.GetRequiredService(type));
            }
        }
    }

    /// <summary>
    /// Adds a custom middleware to the IServiceCollection.
    /// </summary>
    /// <param name="isScoped">If true, registers the middleware with a scoped lifetime. Otherwise, registers with a singleton lifetime.</param>
    /// <returns>>The registered IServiceCollection instance from the IServiceCollection.</returns>
    public static IServiceCollection AddMiddleware<TMiddleware>(
        this IServiceCollection collection,
        Func<TMiddleware> middlewareSelector,
              bool isScoped = true
    ) where TMiddleware : Middleware {
        if (isScoped) {
            collection.AddScoped<Middleware>(p => middlewareSelector());
        }
        else {
            collection.AddSingleton<Middleware>(p => middlewareSelector());
        }
        return collection;
    }

    /// <summary>
    /// Adds a custom middleware to the IServiceCollection.
    /// </summary>
    /// <param name="isScoped">If true, registers the middleware with a scoped lifetime. Otherwise, registers with a singleton lifetime.</param>
    /// <returns>>The registered IServiceCollection instance from the IServiceCollection.</returns>
    public static IServiceCollection AddMiddleware<TMiddleware>(this IServiceCollection collection, bool isScoped = true)
        where TMiddleware : Middleware {
        if (isScoped) {
            collection.AddScoped<TMiddleware>();
            collection.AddScoped<Middleware, TMiddleware>(p => p.GetRequiredService<TMiddleware>());
        }
        else {
            collection.AddSingleton<TMiddleware>();
            collection.AddSingleton<Middleware, TMiddleware>(p => p.GetRequiredService<TMiddleware>());
        }
        return collection;
    }

    /// <summary>
    /// Gets the StoreProvider from the IServiceProvider
    /// </summary>
    /// <returns>The StoreProvider instance from the IServiceProvider.</returns>
    public static StoreProvider GetStoreProvider(this IServiceProvider provider) {
        return provider.GetRequiredService<StoreProvider>();
    }
}