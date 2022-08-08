namespace Memento.Core;

public class PastHistoryStack<T> : List<T> {
    public void Push(T item) {
        if (this.Count is 0)
            this.Add(item);
        else
            this.Insert(0, item);
    }

    public T? Pop() {
        if (this.Count > 0) {
            T temp = this[0];
            this.RemoveAt(0);
            return temp;
        }

        return default(T);
    }

    public T? Peek() => this.Count > 0 ? this[0] : default(T);

    public T? RemoveLast() {
        if (this.Count > 0) {
            var item = this[this.Count - 1];
            this.RemoveAt(this.Count - 1);

            return item;
        }

        return default(T);
    }
}

