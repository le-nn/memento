using Memento.Core.Store;
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

    public static IEnumerable<IMiddleware> GetAllMiddlewares(this IServiceProvider provider) {
        return provider.GetServices<IMiddleware>();
    }

    public static IEnumerable<T> GetServices<T>(this IServiceProvider provider) {
        if (provider.GetService(typeof(IEnumerable<T>)) is IEnumerable<T> services) {
            return services;
        }

        throw new InvalidOperationException("Service cannot resolve.");
    }
}