namespace Memento.Core;

public static class ServiceProviderExtensions {
    public static IEnumerable<IStore<object, Command>> GetAllStores(this IServiceProvider provider) {
        return provider.GetServices<IStore<object, Command>>();
    }

    public static IEnumerable<Middleware> GetAllMiddleware(this IServiceProvider provider) {
        return provider.GetServices<Middleware>();
    }

    public static IEnumerable<T> GetServices<T>(this IServiceProvider provider) {
        if (provider.GetService(typeof(IEnumerable<T>)) is IEnumerable<T> services) {
            return services;
        }

        throw new InvalidOperationException("Service cannot resolve.");
    }
}