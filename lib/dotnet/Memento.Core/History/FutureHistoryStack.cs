namespace Memento;

internal class FutureHistoryStack<T> : List<T> {
    public void Push(T item) {
        this.Insert(0, item);
    }

    public T? Pop() {
        if (this.Count > 0) {
            var item = this[0];
            this.RemoveAt(0);
            return item;
        }

        return default(T?);
    }
}
