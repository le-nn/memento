using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Memento.Core.Executors;

public class SortedAsyncOperationExecutor {
    object locker = new();
    ConcurrentQueue<IOperation> operations = new();
    volatile int processingCount = 0;

    public Task<T> ExecuteAsync<T>(Func<Task<T>> operation) {
        var source = new TaskCompletionSource<T>();
        this.operations.Enqueue(new Operation<T>(operation, source));

        this.Hadle();

        return source.Task;
    }

    public Task ExecuteAsync(Func<Task> operation) {
        var source = new TaskCompletionSource<byte>();
        this.operations.Enqueue(new Operation<byte>(async () => {
            await operation.Invoke();
            return 0;
        }, source));

        this.Hadle();

        return source.Task;
    }

    async void Hadle() {
        if (this.processingCount is not 0) {
            return;
        }

        Interlocked.Increment(ref this.processingCount);

        try {
            while (this.operations.TryDequeue(out var operation)) {
                await operation.HandleAsync();
            }
        }
        finally {
            Interlocked.Decrement(ref this.processingCount);
        }

    }

    interface IOperation {
        Task HandleAsync();
    }

    class Operation<T> : IOperation {
        Func<Task<T>> func;
        TaskCompletionSource<T> taskSource;

        public Operation(Func<Task<T>> func, TaskCompletionSource<T> taskSource) {
            this.func = func;
            this.taskSource = taskSource;
        }

        public async Task HandleAsync() {
            try {
                var reslut = await this.func.Invoke();
                this.taskSource.SetResult(reslut);
            }
            catch (Exception ex) {
                this.taskSource.SetException(ex);
            }
        }
    }
}
