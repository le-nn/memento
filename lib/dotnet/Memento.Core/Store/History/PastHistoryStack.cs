namespace Memento;

public class PastHistoryStack<T> : List<T> {
    public void Push(T item) {
        this.Add(item);
    }

    public T? Pop() {
        if (this.Count > 0) {
            T temp = this[this.Count - 1];
            this.RemoveAt(this.Count - 1);
            return temp;
        }

        return default(T);
    }


    public T? Peek() {
        if (this.Count > 0) {
            T temp = this[this.Count - 1];
            return temp;
        }

        return default(T);
    }

    public T? RemoveLast() {
        if (this.Count > 0) {
            var item = this[0];
            this.RemoveAt(0);

            return item;
        }

        return default(T);
    }
}

