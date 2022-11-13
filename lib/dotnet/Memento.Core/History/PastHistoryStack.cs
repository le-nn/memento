namespace Memento.Core;

public class PastHistoryStack<T> : List<T> {
    public void Push(T item) {
        if (Count is 0)
            Add(item);
        else
            Insert(0, item);
    }

    public T? Pop() {
        if (Count > 0) {
            T temp = this[0];
            RemoveAt(0);
            return temp;
        }

        return default(T);
    }

    public T? Peek() => Count > 0 ? this[0] : default(T);

    public T? RemoveLast() {
        if (Count > 0) {
            var item = this[Count - 1];
            RemoveAt(Count - 1);

            return item;
        }

        return default(T);
    }
}

