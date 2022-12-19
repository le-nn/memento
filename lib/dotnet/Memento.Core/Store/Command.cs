namespace Memento.Core;

public abstract record Command {
    public string Name() => GetType().Name;
}