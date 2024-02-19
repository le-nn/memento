using Memento.Core.Executors;
using Memento.Core;
using Microsoft.AspNetCore.Components;
using System.Collections.Concurrent;

namespace Memento.Blazor;

/// <summary>
/// The base class for components that observe state changes in a store.
/// Injected stores that implement the <see cref="IStateObservable{TMessage}"/> interface will all be subscribed to state change events
/// and automatically call <see cref="ComponentBase.StateHasChanged"/>.
/// </summary>
public class ObserverComponent : ComponentBase, IDisposable {
    private bool _isDisposed;

    private readonly ThrottledExecutor<IStateChangedEventArgs> _stateHasChangedThrottler = new();
    private readonly ConcurrentBag<IDisposable> _disposables = [];
    private readonly ConcurrentBag<IWatcher> _watchers = [];

    /// <summary>
    /// Initializes a new instance of the <see cref="ObserverComponent"/> class.
    /// </summary>
    public ObserverComponent() {
        AddDisposable(_stateHasChangedThrottler.Subscribe(e => {
            if (!_isDisposed) {
                InvokeAsync(StateHasChanged);
            }
        }));
    }

    /// <summary>
    /// If greater than 0, the feature will not execute state changes
    /// more often than this many times per second. Additional notifications
    /// will be suppressed, and observers will be notified of the last
    /// state when the time window has elapsed to allow another notification.
    /// </summary>
    protected ushort LatencyMs { get; set; } = 100;

    /// <summary>
    /// Disposes of the component and unsubscribes from any state.
    /// </summary>
    public void Dispose() {
        if (_isDisposed is false) {
            Dispose(true);
            GC.SuppressFinalize(this);
            _isDisposed = true;
        }
    }

    /// <summary>
    /// Subscribes to state properties.
    /// </summary>
    protected override void OnInitialized() {
        base.OnInitialized();
        AddDisposable(StateSubscriber.Subscribe(this, e => {
            _stateHasChangedThrottler.LatencyMs = LatencyMs;
            _stateHasChangedThrottler.Invoke(e);
        }));
    }

    /// <inheritdoc />
    protected override void OnAfterRender(bool firstRender) {
        base.OnAfterRender(firstRender);

        foreach (var watcher in _watchers) {
            watcher.InvokeOnParameterSet();
        }
    }

    /// <summary>
    /// Disposes of the component and unsubscribes from any state.
    /// </summary>
    /// <param name="disposing">Indicates whether the component is being disposed.</param>
    /// <exception cref="NullReferenceException">Throws when you forgot to call base.InitializeAsync().</exception>
    protected virtual void Dispose(bool disposing) {
        if (disposing) {
            foreach (var d in _disposables ?? []) {
                d.Dispose();
            }
            _disposables?.Clear();
        }
    }

    /// <summary>
    /// Adds a disposable object to the list of disposables.
    /// </summary>
    /// <param name="disposable">The disposable object to add.</param>
    protected void AddDisposable(IDisposable disposable) {
        _disposables.Add(disposable);
    }

    /// <summary>
    /// Adds a collection of disposable objects to the list of disposables.
    /// </summary>
    /// <param name="disposables">The collection of disposable objects to add.</param>
    protected void AddDisposables(IEnumerable<IDisposable> disposables) {
        foreach (var item in disposables) {
            _disposables.Add(item);
        }
    }

    /// <summary>
    /// Adds a watcher to observe changes in the specified selector and perform the specified action.
    /// </summary>
    /// <typeparam name="T">The type of the selector.</typeparam>
    /// <param name="selector">The selector function that retrieves the value to observe.</param>
    /// <param name="action">The action to perform when the value changes.</param>
    /// <param name="once">Indicates whether the action should be performed only once.</param>
    protected void Watch<T>(Func<T> selector, Action<T> action, bool once = false) {
        _watchers.Add(new Watcher<T>(action, selector, once));
    }
}

/// <summary>
/// Represents a watcher that observes changes in a selector and performs an action.
/// </summary>
internal interface IWatcher {
    /// <summary>
    /// Invokes the action when the parameter is set.
    /// </summary>
    void InvokeOnParameterSet();
}

/// <summary>
/// Represents a watcher that observes changes in a selector and performs an action.
/// </summary>
/// <typeparam name="T">The type of the selector.</typeparam>
internal class Watcher<T> : IWatcher {
    private T? _last;
    private bool _invoked;
    private readonly Action<T> _action;
    private readonly Func<T> _selector;

    /// <summary>
    /// Initializes a new instance of the <see cref="Watcher{T}"/> class.
    /// </summary>
    /// <param name="action">The action to perform when the value changes.</param>
    /// <param name="selector">The selector function that retrieves the value to observe.</param>
    /// <param name="once">Indicates whether the action should be performed only once.</param>
    public Watcher(Action<T> action, Func<T> selector, bool once = false) {
        _action = action;
        _selector = selector;
        Once = once;
    }

    /// <summary>
    /// Gets a value indicating whether the action should be performed only once.
    /// </summary>
    public bool Once { get; }

    /// <inheritdoc />
    public void InvokeOnParameterSet() {
        if (Once && _invoked) {
            return;
        }

        var value = _selector.Invoke();
        if (!EqualityComparer<T>.Default.Equals(value, _last)) {
            _invoked = true;
            _last = value;
            _action(value);
        }
    }
}
