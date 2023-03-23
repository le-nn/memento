using System;

namespace Memento.Core;

internal class StoreObserver
    : IObserver<StateChangedEventArgs> {
    readonly Action<StateChangedEventArgs> _action;

    public StoreObserver(Action<StateChangedEventArgs> action) {
        _action = action;
    }

    public void OnCompleted() {
        throw new NotSupportedException();
    }

    public void OnError(Exception error) {
        throw new NotSupportedException();
    }

    public void OnNext(StateChangedEventArgs value) {
        _action(value);
    }
}
