using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Memento.Core.Store;

public record RootState : IReadOnlyDictionary<string, object?> {
    readonly ImmutableDictionary<string, object> _rootState;

    public ImmutableDictionary<string, object> AsImmutableDictionary() => _rootState;

    internal RootState(ImmutableDictionary<string, object> rootState) {
        _rootState = rootState;
    }

    public object? this[string key] => _rootState.TryGetValue(key, out var value)
        ? value
        : null;

    public IEnumerable<string> Keys => _rootState.Keys;

    public IEnumerable<object> Values => _rootState.Values;

    public int Count => _rootState.Count;

    public TState? GetState<TState>(string key) => (TState?)this[key];

    public bool ContainsKey(string key) {
        throw new NotImplementedException();
    }

    public IEnumerator<KeyValuePair<string, object?>> GetEnumerator()
        => _rootState.GetEnumerator();

    public bool TryGetValue(string key, [MaybeNullWhen(false)] out object value)
        => _rootState.TryGetValue(key, out value);

    IEnumerator IEnumerable.GetEnumerator()
        => _rootState.GetEnumerator();

    public override string ToString() {
        return JsonSerializer.Serialize(this, new JsonSerializerOptions {
            WriteIndented = true,
        });
    }
}
