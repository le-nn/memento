using Memento.Core.Executors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Memento.Test.Core.Executors;

public class ConcatOperationExecutorTest {
    [Fact]
    public async Task RunTest1() {
        var executor = new ConcatAsyncOperationExecutor();
        var results = new List<int>();

        var task1 = executor.ExecuteAsync(async () => {
            await Task.Delay(50);
            results.Add(1);
        });

        var task2 = executor.ExecuteAsync(async () => {
            await Task.Delay(10);
            results.Add(2);
        });

        var task3 = executor.ExecuteAsync(async () => {
            await Task.Delay(60);
            results.Add(3);
        });

        var task4 = executor.ExecuteAsync(async () => {
            await Task.Delay(340);
            results.Add(4);
        });

        var task5 = executor.ExecuteAsync(async () => {
            await Task.Delay(5);
            results.Add(5);
        });

        var task6 = executor.ExecuteAsync(async () => {
            await Task.Delay(160);
            results.Add(6);
        });

        var task7 = executor.ExecuteAsync(async () => {
            await Task.Delay(70);
            results.Add(7);
        });

        await task1;
        await task2;
        await task3;
        await task4;
        await task5;
        await task6;
        await task7;

        Assert.True(results is [1, 2, 3, 4, 5, 6, 7]);
    }

    [Fact]
    public async Task RunTest2() {
        var executor = new ConcatAsyncOperationExecutor();
        var results = new List<int>();

        var task1 = executor.ExecuteAsync(async () => {
            await Task.Delay(1);
            results.Add(1);
            return 0;
        });

        var task2 = executor.ExecuteAsync(async () => {
            await Task.Delay(1);
            results.Add(2);
            return 0;
        });

        var task3 = executor.ExecuteAsync(async () => {
            await Task.Delay(1);
            results.Add(3);

            return 0;
        });

        var task4 = executor.ExecuteAsync(async () => {
            await Task.Delay(2);
            results.Add(4);

            return 0;
        });

        var task5 = executor.ExecuteAsync(async () => {
            await Task.Delay(3);
            results.Add(5);

            return 0;
        });

        var task6 = executor.ExecuteAsync(async () => {
            await Task.Delay(1);
            results.Add(6);

            return 0;
        });

        var task7 = executor.ExecuteAsync(async () => {
            await Task.Delay(1);
            results.Add(7);

            return 0;
        });

        await task1;
        await task2;
        await task3;
        await task4;
        await task5;
        await task6;
        await task7;

        Assert.True(results is [1, 2, 3, 4, 5, 6, 7]);
    }
}
