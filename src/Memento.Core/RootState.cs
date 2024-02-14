using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;

namespace Memento.Core;

/// <summary>
/// Represents the root state of the application.
/// </summary>
public record RootState : IReadOnlyDictionary<string, object?> {
    readonly Dictionary<string, object> _rootState;

    /// <summary>
    /// Initializes a new instance of the <see cref="RootState"/> class.
    /// </summary>
    /// <param name="rootState">The root state dictionary.</param>
    internal RootState(Dictionary<string, object> rootState) {
        _rootState = rootState;
    }

    /// <summary>
    /// Gets the root state as a dictionary.
    /// </summary>
    /// <returns>The root state as a dictionary.</returns>
    public Dictionary<string, object> AsDictionary() => _rootState;

    /// <inheritdoc/>
    public object? this[string key] => _rootState.TryGetValue(key, out var value)
        ? value
        : null;

    /// <inheritdoc/>
    public IEnumerable<string> Keys => _rootState.Keys;

    /// <inheritdoc/>
    public IEnumerable<object> Values => _rootState.Values;

    /// <inheritdoc/>
    public int Count => _rootState.Count;

    /// <summary>
    /// Gets the state of the specified type with the specified key.
    /// </summary>
    /// <typeparam name="TState">The type of the state.</typeparam>
    /// <param name="key">The key.</param>
    /// <returns>The state of the specified type with the specified key.</returns>
    public TState? GetState<TState>(string key) => (TState?)this[key];

    /// <inheritdoc/>
    public bool ContainsKey(string key) {
        throw new NotImplementedException();
    }

    /// <inheritdoc/>
    public IEnumerator<KeyValuePair<string, object?>> GetEnumerator()
        => _rootState.GetEnumerator();

    /// <inheritdoc/>
    public bool TryGetValue(string key, [MaybeNullWhen(false)] out object value)
        => _rootState.TryGetValue(key, out value);

    /// <inheritdoc/>
    IEnumerator IEnumerable.GetEnumerator()
        => _rootState.GetEnumerator();

    /// <inheritdoc/>
    public override string ToString() {
        return JsonSerializer.Serialize(this, new JsonSerializerOptions {
            WriteIndented = true,
        });
    }
}
