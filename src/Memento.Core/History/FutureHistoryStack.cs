namespace Memento.Core.History;

internal class FutureHistoryStack<T> {
    readonly List<T> _values = new();

    public int GetCount() {
        lock (_values) {
            return _values.Count;
        }
    }

    public IReadOnlyCollection<T> CloneAsReadOnly() {
        lock (_values) {
            return _values.ToArray();
        }
    }

    public void Push(T item) {
        lock (_values) {
                _values.Insert(0, item);
        }
    }

    public T? Pop() {
        lock (_values) {
            if (_values.Count > 0) {
                var temp = _values[0];
                _values.RemoveAt(0);
                return temp;
            }

            return default;
        }
    }

    public void Clear() {
        lock (_values) {
            _values.Clear();
        }
    }
}