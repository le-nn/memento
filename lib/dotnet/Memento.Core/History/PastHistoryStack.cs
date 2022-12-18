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
/* Unmerged change from project 'Memento.Core(net7.0)'
Before:
            T temp = this[0];
After:
            var temp = this[0];
*/

            var temp = this[0];
            RemoveAt(0);
            return temp;
/* Unmerged change from project 'Memento.Core(net7.0)'
Before:
        return default(T);
After:
        return default;
*/

        }

        return default;
    }


/* Unmerged change from project 'Memento.Core(net7.0)'
Before:
    public T? Peek() => Count > 0 ? this[0] : default(T);
After:
    public T? Peek() => Count > 0 ? this[0] : default;
*/
    public T? Peek() => Count > 0 ? this[0] : default;

    public T? RemoveLast() {
        if (Count > 0) {
            var item = this[Count - 1];
            RemoveAt(Count - 1);

            return item;
/* Unmerged change from project 'Memento.Core(net7.0)'
Before:
        return default(T);
After:
        return default;
*/

        }

        return default;
    }
}