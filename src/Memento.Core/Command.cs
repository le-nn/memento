namespace Memento.Core;

/// <summary>
/// Represents an abstract base class for commands that mutate the state of a store.
/// </summary>
public abstract record Command {
    /// <summary>
    /// Represents a command that forces a state replacement.
    /// </summary>
    public record ForceReplaced(object State) : Command;

    /// <summary>
    /// Represents a command that restores the previous state.
    /// </summary>
    public record Restored : Command;

    /// <summary>
    /// Represents a command that indicates a state change has occurred.
    /// </summary>
    public record StateHasChanged(object State) : Command;

    /// <summary>
    /// Gets the type of the command, excluding the assembly name.
    /// </summary>
    public virtual string Type {
        get {
            var type = GetType();
            return type.FullName?.Replace(
                type.Assembly.GetName().Name + ".",
                string.Empty
            ) ?? "";
        }
    }

    /// <summary>
    /// Gets the full type name of the command.
    /// </summary>
    /// <returns>The full type name as a string.</returns>
    public string? GetFullTypeName() {
        return GetType().FullName;
    }

    /// <summary>
    /// Gets the payload properties of the command as a dictionary.
    /// </summary>
    public Dictionary<string, object> Payload => GetPayloads()
        .ToDictionary(x => x.Key, x => x.Value);

    /// <summary>
    /// Gets the payload properties of the command as a collection of key-value pairs.
    /// </summary>
    /// <returns>An enumerable collection of key-value pairs representing the payload properties.</returns>
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