namespace Memento.Core;

internal class FutureHistoryStack<T> : List<T> {
    public void Push(T item) {
        Insert(0, item);
    }

    public T? Pop() {
        if (Count > 0) {
            var item = this[0];
            RemoveAt(0);
            return item;
            /* Unmerged change from project 'Memento.Core(net6.0)'
            Before:
                    return default(T?);
            After:
                    return default;
            */

        }

        return default;
    }
}