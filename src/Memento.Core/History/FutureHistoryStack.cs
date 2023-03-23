namespace Memento.Core.History;

internal class FutureHistoryStack<T> : List<T> {
    public void Push(T item) {
        Insert(0, item);
    }

    public T? Pop() {
        if (Count > 0) {
            var item = this[0];
            RemoveAt(0);
            return item;
        }

        return default;
    }
}