namespace Memento;

public abstract record Message {
    public string Name() => this.GetType().Name;
}
