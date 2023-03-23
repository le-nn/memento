using Memento.Core;
using Memento.Core.Executors;
using Microsoft.AspNetCore.Components;
using System.Collections.Immutable;

namespace Memento.Blazor;

/// <summary>
/// The base class for components that observe state changes in a store.
/// Injected stores that implements <see cref="IStore"/> interface will all be subscribed state change events
/// and call <see cref="ComponentBase.StateHasChanged"/> automatically.
/// </summary>
public class ObserverComponent : ComponentBase, IDisposable {
    private bool _isDisposed;
    private IDisposable? _stateSubscription;
    private readonly IDisposable _invokerSubscription;
    private readonly ThrottledExecutor<StateChangedEventArgs> _stateHasChangedThrottler = new();
    private ImmutableArray<IDisposable>? _disposables;

    /// <summary>
    /// Initializes a new instance of <see cref="ObserverComponent"/> class.
    /// </summary>
    public ObserverComponent() {
        _invokerSubscription = _stateHasChangedThrottler.Subscribe(e => {
            if (_isDisposed is false) {
                InvokeAsync(StateHasChanged);
            }
        });
    }

    /// <summary>
    /// If greater than 0, the feature will not execute state changes
    /// more often than this many times per second. Additional notifications
    /// will be suppressed, and observers will be notified of the last.
    /// state when the time window has elapsed to allow another notification.
    /// </summary>
    protected ushort LatencyMs { get; set; } = 100;

    /// <summary>
    /// Disposes of the component and unsubscribes from any state
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
        _stateSubscription = StateSubscriber.Subscribe(this, e => {
            _stateHasChangedThrottler.LatencyMs = LatencyMs;
            _stateHasChangedThrottler.Invoke(e);
        });
        _disposables = OnHandleDisposable().ToImmutableArray();
    }

    /// <summary>
    /// It will be called when 
    /// </summary>
    /// <param name="disposing"></param>
    /// <exception cref="NullReferenceException">Throws when you forgot to call base.InitializeAsync().</exception>
    protected virtual void Dispose(bool disposing) {
        if (disposing) {
            if (_stateSubscription is null) {
                throw new NullReferenceException("Have you forgotten to call base.InitializeAsync() in your component?");
            }

            _invokerSubscription.Dispose();
            _stateSubscription.Dispose();

            foreach (var d in _disposables ?? ImmutableArray.Create<IDisposable>()) {
                d.Dispose();
            }
        }
    }

    /// <summary>
    /// Handles IDisposable. Generated disposables will be Disposed when the component is destroyed
    /// </summary>
    /// <returns>The disposables.</returns>
    protected virtual IEnumerable<IDisposable> OnHandleDisposable() {
        return Enumerable.Empty<IDisposable>();
    }
}