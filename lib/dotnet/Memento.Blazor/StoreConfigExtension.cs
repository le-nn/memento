using Memento.Core;
using Memento.Core.Store;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace Memento.Blazor;

public static class StoreConfigExtension {
    public static IServiceCollection AddMemento(this IServiceCollection services) {
        services.AddScoped(p => new StoreProvider(p));
        return services;
    }

    public static IServiceCollection AddStore<TStore>(this IServiceCollection collection)
        where TStore : class, IStore {
        collection.AddScoped<TStore>()
            .AddScoped<IStore>(p => p.GetRequiredService<TStore>());
        return collection;
    }

    public static void ScanAssembyAndAddStores(this IServiceCollection services, Assembly assembly) {
        foreach (var type in assembly.GetTypes().Where(t => t.IsAssignableTo(typeof(IStore)))) {
            services.AddScoped(type)
                .AddScoped(p => (IStore)p.GetRequiredService(type));
        }
    }

    public static IServiceCollection AddMiddleware<TMiddleware>(
        this IServiceCollection collection,
        Func<TMiddleware> middlewareSelector
    ) where TMiddleware : Middleware {
        collection.AddScoped<Middleware>(p => middlewareSelector());
        return collection;
    }

    public static IServiceCollection AddMiddleware<TMiddleware>(this IServiceCollection collection)
        where TMiddleware : Middleware {
        collection.AddScoped<Middleware, TMiddleware>();
        return collection;
    }

    public static StoreProvider GetStoreProvider(this IServiceProvider provider) {
        return provider.GetRequiredService<StoreProvider>();
    }
}