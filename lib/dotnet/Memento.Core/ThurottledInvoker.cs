namespace Memento;

public class ThrottledInvoker {
    public ushort ThrottleWindowMs { get; set; }

    private volatile int LockFlag;
    private volatile bool InvokingSuspended;
    private DateTime LastInvokeTime;
    private readonly Action Action;
    private Timer? ThrottleTimer;

    public ThrottledInvoker(Action action) {
        this.Action = action ?? throw new ArgumentNullException(nameof(action));
        this.LastInvokeTime = DateTime.UtcNow - TimeSpan.FromMilliseconds(ushort.MaxValue);
    }

    public void Invoke(byte maximumInvokesPerSecond = 0) {
        this.ThrottleWindowMs = maximumInvokesPerSecond switch {
            0 => 0,
            _ => (ushort)(1000 / maximumInvokesPerSecond),
        };

        this.Invoke();
    }

    public void Invoke() {
        // If no throttle window then bypass throttling
        if (this.ThrottleWindowMs == 0) {
            this.Action();
            return;
        }

        LockAndExecuteOnlyIfNotAlreadyLocked(() => {
            // If waiting for a previously throttled notification to execute
            // then ignore this notification request
            if (this.InvokingSuspended)
                return;

            int millisecondsSinceLastInvoke =
                (int)(DateTime.UtcNow - this.LastInvokeTime).TotalMilliseconds;

            // If last execute was outside the throttle window then execute immediately
            if (millisecondsSinceLastInvoke >= this.ThrottleWindowMs) {
                ExecuteThrottledAction();
                return;
            }

            // This is exactly the second invoke within the time window,
            // so set a timer that will trigger at the start of the next
            // time window and prevent further invokes until
            // the timer has triggered
            this.InvokingSuspended = true;
            int delay = this.ThrottleWindowMs - millisecondsSinceLastInvoke;
            ThrottleTimer = new Timer(
                callback: _ => ExecuteThrottledAction(),
                state: null,
                dueTime: delay,
                period: 0);
        });
    }

    private void LockAndExecuteOnlyIfNotAlreadyLocked(Action action) {
        bool lockTaken =
            (Interlocked.CompareExchange(ref this.LockFlag, 1, 0) == 0);
        if (!lockTaken)
            return;

        try {
            action();
        }
        finally {
            this.LockFlag = 0;
        }
    }

    private void ExecuteThrottledAction() {
        try {
            Action();
        }
        finally {
            this.ThrottleTimer?.Dispose();
            this.ThrottleTimer = null;
            LastInvokeTime = DateTime.UtcNow;
            // This must be set last, as it is the circuit breaker within the lock code
            InvokingSuspended = false;
        }
    }
}
