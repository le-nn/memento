namespace Memento.Core.History;
internal class PastHistoryStack<T> {
    readonly List<T> _values = [];

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
            if (_values.Count is 0)
                _values.Add(item);
            else
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

    public T? RemoveLast() {
        lock (_values) {
            if (_values.Count > 0) {
                var item = _values[^1];
                _values.RemoveAt(_values.Count - 1);

                return item;
            }

            return default;
        }
    }
}