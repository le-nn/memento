using Memento.Core.Internals;
using System.Collections.Concurrent;

namespace Memento.Core;

public interface IStateObservable<out TMessage> : IObservable<IStateChangedEventArgs>
    where TMessage : notnull {
    /// <summary>
    /// Notifies observers that the state of the store has changed.
    /// </summary>
    void StateHasChanged();
}

public abstract class StateObservable<TMessage> : IStateObservable<TMessage>
    where TMessage : notnull {
    readonly ConcurrentDictionary<Guid, IObserver<IStateChangedEventArgs>> _observers = new();

    /// <summary>
    /// Notifies observers that the state of the store has changed.
    /// </summary>
    /// <param name="message">The optional message associated with the state change.</param>
    public void StateHasChanged(TMessage? message = default) {
        InvokeObserver(new StateChangedEventArgs() {
            Message = message,
            Sender = this,
            StateChangeType = StateChangeType.StateHasChanged,
        });
    }

    void IStateObservable<TMessage>.StateHasChanged() {
        StateHasChanged(default);
    }

    /// <summary>
    /// Subscribes an observer to receive state change notifications.
    /// </summary>
    /// <param name="observer">The observer to subscribe.</param>
    /// <returns>An IDisposable object that can be used to unsubscribe the observer.</returns>
    public IDisposable Subscribe(IObserver<IStateChangedEventArgs> observer) {
        var id = Guid.NewGuid();
        if (_observers.TryAdd(id, observer) is false) {
            throw new InvalidOperationException("Failed to subscribe observer");
        }

        return new StoreSubscription(GetType().FullName ?? "StateObservable.Subscribe", () => {
            if (_observers.TryRemove(new(id, observer)) is false) {
                throw new InvalidOperationException("Failed to unsubscribe observer");
            }
        });
    }

    /// <summary>
    /// Subscribes an action as an observer to receive state change notifications.
    /// </summary>
    /// <param name="observer">The action to subscribe as an observer.</param>
    /// <returns>An IDisposable object that can be used to unsubscribe the observer.</returns>
    public IDisposable Subscribe(Action<IStateChangedEventArgs> observer) {
        return Subscribe(new GeneralObserver<IStateChangedEventArgs>(observer));
    }

    internal void InvokeObserver(IStateChangedEventArgs e) {
        foreach (var (_, obs) in _observers) {
            obs.OnNext(e);
        }
    }
}
