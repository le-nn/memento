using Memento.Core.Invokers;
using Microsoft.AspNetCore.Components;

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
        this.InvokerSubscription = this.StateHasChangedThrottler.Subscribe(e => {
            if (this.IsDisposed is false) {
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
        if (this.IsDisposed is false) {
            Dispose(true);
            GC.SuppressFinalize(this);
            this.IsDisposed = true;
        }
    }

    /// <summary>
    /// Subscribes to state properties
    /// </summary>
    protected override void OnInitialized() {
        base.OnInitialized();
        this.StateSubscription = StateSubscriber.Subscribe(this, _ => {
            StateHasChangedThrottler.Invoke(this.MaximumStateChangedNotificationsPerSecond);
        });
    }

    protected virtual void Dispose(bool disposing) {
        if (disposing) {
            if (this.StateSubscription is null) {
                throw new NullReferenceException("Have you forgotten to call base.OnInitialized() in your component?");
            }

            this.InvokerSubscription.Dispose();
            this.StateSubscription.Dispose();
        }
    }
}
