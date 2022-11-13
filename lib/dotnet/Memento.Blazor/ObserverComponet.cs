using Microsoft.AspNetCore.Components;
using Memento.Core;

namespace Memento.Blazor;

public class ObserverComponet : ComponentBase, IDisposable {
    private bool IsDisposed;
    private IDisposable? StateSubscription;
    private IDisposable InvokerSubscription;
    private readonly ThrottledExecutor<StateChangedEventArgs> StateHasChangedThrottler = new();

    /// <summary>
    /// Creates a new instance
    /// </summary>
    public ObserverComponet() {
        InvokerSubscription = StateHasChangedThrottler.Subscribe(e => {
            if (IsDisposed is false) {
                base.InvokeAsync(StateHasChanged);
            }
        });
    }

    /// <summary>
    /// If greater than 0, the feature will not execute state changes
    /// more often than this many times per second. Additional notifications
    /// will be surpressed, and observers will be notified of the last.
    /// state when the time window has elapsed to allow another notification.
    /// </summary>
    protected byte MaximumStateChangedNotificationsPerSecond { get; set; } = 30;

    /// <summary>
    /// Disposes of the component and unsubscribes from any state
    /// </summary>
    public void Dispose() {
        if (IsDisposed is false) {
            Dispose(true);
            GC.SuppressFinalize(this);
            IsDisposed = true;
        }
    }

    /// <summary>
    /// Subscribes to state properties
    /// </summary>
    protected override void OnInitialized() {
        base.OnInitialized();
        StateSubscription = StateSubscriber.Subscribe(this, e => {
            StateHasChangedThrottler.Invoke(e, MaximumStateChangedNotificationsPerSecond);
        });
    }

    protected virtual void Dispose(bool disposing) {
        if (disposing) {
            if (StateSubscription is null) {
                throw new NullReferenceException("Have you forgotten to call base.OnInitialized() in your component?");
            }

            InvokerSubscription.Dispose();
            StateSubscription.Dispose();
        }
    }
}
