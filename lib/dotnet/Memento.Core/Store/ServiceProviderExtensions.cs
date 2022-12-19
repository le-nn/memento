using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Memento.Core;

public static class ServiceProviderExtensions {
    public static IEnumerable<IStore> GetAllStores(this IServiceProvider provider) {
        return provider.GetServices<IStore>();
    }

    public static IEnumerable<Middleware> GetAllMiddlewares(this IServiceProvider provider) {
        return provider.GetServices<Middleware>();
    }

    public static IEnumerable<T> GetServices<T>(this IServiceProvider provider) {
        if (provider.GetService(typeof(IEnumerable<T>)) is IEnumerable<T> services) {
            return services;
        }

        throw new InvalidOperationException("Service cannot resolve.");
    }
}