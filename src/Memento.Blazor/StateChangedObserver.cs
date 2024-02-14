using Memento.Blazor.Internal;
using Microsoft.AspNetCore.Components;
using System.Collections.Concurrent;

namespace Memento.Blazor;

public class Item(IDisposable disposable, bool isEnabled) {
    public bool IsEnabled { get; set; } = isEnabled;

    public IDisposable Disposable => disposable;
}

/// <summary>
/// Represents a component observer that subscribes to a collection of observables.
/// </summary>
public class StateChangedObserver : ComponentBase, IDisposable {
    readonly ConcurrentDictionary<IObservable<object>, Item> _events = new();
    /// <summary>
    /// Gets or sets the collection of observables to subscribe to.
    /// </summary>
    [Parameter]
    public IEnumerable<IObservable<object>> Observables { get; set; } = [];

    /// <summary>
    /// Gets or sets the event callback to invoke when the state has changed.
    /// </summary>
    [Parameter]
    public EventCallback OnStateHasChanged { get; set; }

    /// <inheritdoc/>
    protected override void OnParametersSet() {
        base.OnParametersSet();

        foreach (var (k, v) in _events) {
            _events[k].IsEnabled = false;
        }

        foreach (var observable in Observables) {
            if (_events.ContainsKey(observable)) {
                _events[observable].IsEnabled = true;
                continue;
            }
            else {
                var d = observable.Subscribe(new Observer<object>(_ => OnStateHasChanged.InvokeAsync()));
                _events.TryAdd(observable, new Item(d, true));
            }
        }

        foreach (var (k, v) in _events) {
            if (v.IsEnabled is false) {
                _events.TryRemove(k, out _);
                v.Disposable.Dispose();
            }
        }
    }

    /// <inheritdoc/>
    public void Dispose() {
        foreach (var (k, v) in _events) {
            v.Disposable.Dispose();
        }
        _events.Clear();
    }
}
