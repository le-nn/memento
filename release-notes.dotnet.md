# Release notes for .NET

### Memento

## v0.0.4

* First release

## v0.0.5

* Change namespace ```Memento``` to ```Memento.Core```
* Add .NET 6 support

## v0.1.0

* Changed ```ThrottledExecutor``` to fire events with the last value invoked.
* Changed the name ```SortedAsyncOperationExecutor``` to ```ConcatAsyncOperationExecutor```.

#### Example

```cs
var executor = new ThrottledExecutor();

executor.Subscribe(value => {
    Console.WriteLine(value);
});

executor.Invoke(0, 30);
await Task.Deley(5);
executor.Invoke(1, 30);
await Task.Deley(5);
executor.Invoke(2, 30);
await Task.Deley(5);
executor.Invoke(3, 30);
await Task.Deley(5);
executor.Invoke(4, 30);
await Task.Deley(5);
executor.Invoke(5, 30);
await Task.Deley(5);
executor.Invoke(6, 30);
await Task.Deley(5);
executor.Invoke(7, 30);
await Task.Deley(5);
executor.Invoke(8, 30);
await Task.Deley(5);
executor.Invoke(9, 30);
await Task.Deley(5);
executor.Invoke(10, 30);
```

#### Before

It doesn't always output 10 at last.
It outputs following.

```
..
..
4
..
..
7
```

#### After

It doesn't always output 10 at last.
It outputs following.

```
..
..
4
..
..
10
```

## v0.2.0

* Add .NET7 support.
* Change name Message -> Command, Reducer -> Reducer, Mutate -> Dispatch

## v1.0.0

* Add support for Redux devtools
* Some apis are breaking changed
