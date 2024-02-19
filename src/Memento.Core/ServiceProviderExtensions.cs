namespace Memento.Core;

public static class ServiceProviderExtensions {
    /// <summary>
    /// Retrieves all instances of <see cref="IStore{TKey, TValue}"/> from the specified <paramref name="provider"/>.
    /// </summary>
    /// <param name="provider">The service provider.</param>
    /// <returns>An enumerable collection of <see cref="IStore{TKey, TValue}"/> instances.</returns>
    public static IEnumerable<IStore> GetAllStores(this IServiceProvider provider) {
        return provider.GetServices<IStore>();
    }

    /// <summary>
    /// Retrieves all instances of <see cref="Middleware"/> from the specified <paramref name="provider"/>.
    /// </summary>
    /// <param name="provider">The service provider.</param>
    /// <returns>An enumerable collection of <see cref="Middleware"/> instances.</returns>
    public static IEnumerable<Middleware> GetAllMiddleware(this IServiceProvider provider) {
        return provider.GetServices<Middleware>();
    }

    /// <summary>
    /// Retrieves all instances of the specified type <typeparamref name="T"/> from the specified <paramref name="provider"/>.
    /// </summary>
    /// <typeparam name="T">The type of the instances to retrieve.</typeparam>
    /// <param name="provider">The service provider.</param>
    /// <returns>An enumerable collection of instances of type <typeparamref name="T"/>.</returns>
    /// <exception cref="InvalidOperationException">Thrown when the service cannot be resolved.</exception>
    public static IEnumerable<T> GetServices<T>(this IServiceProvider provider) {
        if (provider.GetService(typeof(IEnumerable<T>)) is IEnumerable<T> services) {
            return services;
        }

        throw new InvalidOperationException("Service cannot be resolved.");
    }
}
