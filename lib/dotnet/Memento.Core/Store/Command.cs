namespace Memento.Core;

public abstract record Command {
    public record ForceReplace(object State) : Command;

    public record Restores : Command;

    public virtual string Type {
        get {
            var type = GetType();
            return type.FullName?.Replace(
                type.Assembly.GetName().Name + ".",
                string.Empty
            ) ?? "";
        }
    }

    public string? GetFullTypeName() {
        return GetType().FullName;
    }

    public Dictionary<string, object> Payload => GetPayloads()
        .ToDictionary(x => x.Key, x => x.Value);

    IEnumerable<KeyValuePair<string, object>> GetPayloads() {
        foreach (var property in GetType().GetProperties()) {
            if (property.Name is nameof(Payload) or nameof(Type)) {
                continue;
            }

            var value = property.GetValue(this);
            if (value is not null) {
                yield return new KeyValuePair<string, object>(property.Name, value);
            }
        }
    }
}