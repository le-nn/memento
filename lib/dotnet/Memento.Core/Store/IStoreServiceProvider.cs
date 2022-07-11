namespace Memento;

public interface IServiceContainer : IServiceProvider {
    IEnumerable<IStore> GetAllStores();

    IEnumerable<Middleware> GetAllMiddlewares();

    TService GetService<TService>();
}
