using Memento.Core.Internals;
using Memento.Core.Store.Internals;

namespace Memento.Core.Invokers;

public class ThrottledExecutor<T> : IObservable<T> {
    volatile int LockFlag;
    volatile bool InvokingSuspended;
    DateTime LastInvokeTime;
    Timer? ThrottleTimer;

    readonly List<IObserver<T>> observers = new();
    readonly object locker = new();

    public ushort ThrottleWindowMs { get; private set; }

    public ThrottledExecutor() {
        LastInvokeTime = DateTime.UtcNow - TimeSpan.FromMilliseconds(ushort.MaxValue);
    }

    public IDisposable Subscribe(IObserver<T> action) {
        lock (this.locker) {
            this.observers.Add(action);
        }

        return new StoreSubscription(nameof(ThrottledExecutor<T>), () => {
            lock (this.locker) {
                this.observers.Remove(action);
            }
        });
    }

    public IDisposable Subscribe(Action<T> action) {
        var observer = new GeneralObeserver<T>(action);
        lock (this.locker) {
            this.observers.Add(observer);
        }

        return new StoreSubscription(nameof(ThrottledExecutor<T>), () => {
            lock (this.locker) {
                this.observers.Remove(observer);
            }
        });
    }

    public void Invoke(byte maximumInvokesPerSecond = 0) {
        ThrottleWindowMs = maximumInvokesPerSecond switch {
            0 => 0,
            _ => (ushort)(1000 / maximumInvokesPerSecond),
        };

        Invoke();
    }

    public void Invoke(T value) {
        // If no throttle window then bypass throttling
        if (ThrottleWindowMs is 0) {
            this.InvokeObservers(value);
            return;
        }

        LockAndExecuteOnlyIfNotAlreadyLocked(() => {
            // If waiting for a previously throttled notification to execute
            // then ignore this notification request
            if (InvokingSuspended)
                return;

            int millisecondsSinceLastInvoke =
                (int)(DateTime.UtcNow - LastInvokeTime).TotalMilliseconds;

            // If last execute was outside the throttle window then execute immediately
            if (millisecondsSinceLastInvoke >= ThrottleWindowMs) {
                ExecuteThrottledAction(value, this.InvokeObservers);
                return;
            }

            // This is exactly the second invoke within the time window,
            // so set a timer that will trigger at the start of the next
            // time window and prevent further invokes until
            // the timer has triggered
            InvokingSuspended = true;
            ThrottleTimer = new Timer(
                callback: _ => ExecuteThrottledAction(value, this.InvokeObservers),
                state: null,
                dueTime: ThrottleWindowMs - millisecondsSinceLastInvoke,
                period: 0
            );
        });
    }

    private void LockAndExecuteOnlyIfNotAlreadyLocked(Action action) {
        if (Interlocked.CompareExchange(ref LockFlag, 1, 0) is 0) {
            try {
                action();
            }
            finally {
                LockFlag = 0;
            }
        }
    }

    private void ExecuteThrottledAction(T value, Action<T> action) {
        try {
            action(value);
        }
        finally {
            ThrottleTimer?.Dispose();
            ThrottleTimer = null;
            LastInvokeTime = DateTime.UtcNow;
            // This must be set last, as it is the circuit breaker within the lock code
            InvokingSuspended = false;
        }
    }

    void InvokeObservers(T value) {
        foreach (var o in this.observers) {
            o.OnNext(value);
        }
    }
}
