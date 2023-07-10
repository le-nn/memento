using System.Text.Json.Serialization;

namespace Memento.Core;

public enum StateHasChangedType {
    StateHasChanged,
    ForceReplaced,
    Restored,
}

/// <summary>
/// Represents an abstract base class for commands that mutate the state of a store.
/// </summary>
public record Command {
    /// <summary>
    /// Represents a command that indicates a state change has occurred.
    /// </summary>
    public record StateHasChanged : Command {
        public StateHasChangedType StateHasChangedType { get; internal set; }

        public object State { get; }

        public object? Message { get; }

        [JsonIgnore]
        public Type? StoreType { get; } = null;

        public override string Type => $"{StoreType?.Name ?? "Store"}+{GetType().Name}";

        public StateHasChanged(object state, object? message = null, Type? storeType = null) {
            State = state;
            Message = message;
            StoreType = storeType;
        }

        public static StateHasChanged CreateForceReplaced(object State) => new(State) {
            StateHasChangedType = StateHasChangedType.ForceReplaced
        };

        public static StateHasChanged CreateRestored(object State) => new(State) {
            StateHasChangedType = StateHasChangedType.Restored
        };
    }

    /// <summary>
    /// Represents a command that indicates a state change has occurred.
    /// </summary>
    public record StateHasChanged<TState, TMessage> : StateHasChanged
        where TState : notnull
        where TMessage : notnull {

        public new TState State => (TState)base.State;

        public new TMessage? Message => (TMessage?)base.Message;

        public override string Type => $"{StoreType?.Name ?? "Store"}+{GetType().Name}";

        public StateHasChanged(TState mutateState, TMessage? message = default, Type? storeType = null)
            : base(mutateState, message, storeType) {

        }

        public static StateHasChanged<TState, TMessage> CreateForceReplaced(TState State) => new(State) {
            StateHasChangedType = StateHasChangedType.ForceReplaced
        };

        public static StateHasChanged<TState, TMessage> CreateRestored(TState State) => new(State) {
            StateHasChangedType = StateHasChangedType.Restored
        };
    }

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