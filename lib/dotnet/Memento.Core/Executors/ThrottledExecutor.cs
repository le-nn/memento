using Memento.Core.Internals;
using Memento.Core.Store.Internals;

namespace Memento.Core;

public class ThrottledExecutor<T> : IObservable<T> {
    volatile int LockFlag;
/* Unmerged change from project 'Memento.Core(net7.0)'
Before:
    volatile bool InvokingSuspended;
After:
    readonly bool InvokingSuspended;
*/

    readonly bool InvokingSuspended;
    DateTime LastInvokeTime;
    Timer? ThrottleTimer;

    readonly List<IObserver<T>> observers = new();
    readonly object locker = new();

    public ushort ThrottleWindowMs { get; private set; }

    public ThrottledExecutor() {
        LastInvokeTime = DateTime.UtcNow - TimeSpan.FromMilliseconds(ushort.MaxValue);
    }

    public IDisposable Subscribe(IObserver<T> action) {
        lock (locker) {
            observers.Add(action);
        }

        return new StoreSubscription(nameof(ThrottledExecutor<T>), () => {
            lock (locker) {
                observers.Remove(action);
            }
        });
    }

    public IDisposable Subscribe(Action<T> action) {
        var observer = new GeneralObeserver<T>(action);
        return Subscribe(observer);
    }

    public void Invoke(T value, byte maximumInvokesPerSecond = 0) {
        
/* Unmerged change from project 'Memento.Core(net7.0)'
Before:
                int millisecondsSinceLastInvoke =
After:
                var millisecondsSinceLastInvoke =
*/
ThrottleWindowMs = maximumInvokesPerSecond switch {
            0 => 0,
            _ => (ushort)(1000 / maximumInvokesPerSecond),
        };

        Invoke(value);
    }

    public void Invoke(T value) {
        // If no throttle window then bypass throttling
        if (ThrottleWindowMs is 0) {
            ExecuteThrottledAction(value);
        }
        else {
            LockAndExecuteOnlyIfNotAlreadyLocked(() => {
                // If waiting for a previously throttled notification to execute
                // then ignore this notification request
                //if (InvokingSuspended)
                //    return;

                var millisecondsSinceLastInvoke =
                    (int)(DateTime.UtcNow - LastInvokeTime).TotalMilliseconds;



                // If last execute was outside the throttle window then execute immediately
                if (millisecondsSinceLastInvoke >= ThrottleWindowMs) {
                    ExecuteThrottledAction(value);
                }
                else {
                    // This is exactly the second invoke within the time window,
                    // so set a timer that will trigger at the start of the next
                    // time window and prevent further invokes until
                    // the timer has triggered
                    ThrottleTimer?.Dispose();
                    ThrottleTimer = new Timer(
                        callback: _ => ExecuteThrottledAction(value),
                        state: null,
                        dueTime: ThrottleWindowMs - millisecondsSinceLastInvoke,
                        period: 0
                    );
                }
            });
        }
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

    private void ExecuteThrottledAction(T value) {
        try {
            lock (locker) {
                foreach (var observer in observers) {
                    observer.OnNext(value);
                }
            }
        }
        finally {
            ThrottleTimer?.Dispose();
            ThrottleTimer = null;
            LastInvokeTime = DateTime.UtcNow;
        }
    }

}