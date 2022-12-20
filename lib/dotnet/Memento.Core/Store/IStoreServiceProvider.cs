namespace Memento.Core;

public interface IServiceContainer : IServiceProvider {
    IEnumerable<IStore> GetAllStores();

    IEnumerable<MiddlewareHandler> GetAllMiddlewares();

    TService GetService<TService>();
}