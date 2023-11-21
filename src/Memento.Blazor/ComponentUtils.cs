using Memento.Core;
using System.Collections.Concurrent;
using System.Reflection;

namespace Memento.Blazor;

using GetStateChangedPropertyDelegate = Func<object, IStore<object, Command>>;

/// <summary>
/// A utility class that automatically subscribes to all <see cref="IStateChangedNotifier"/> properties
/// on a specific object
/// </summary>
public static class StateSubscriber {
    private static readonly ConcurrentDictionary<Type, IEnumerable<GetStateChangedPropertyDelegate>> _valueDelegatesByType = new();

    /// <summary>
    /// Subscribes to all <see cref="IStateChangedNotifier"/> properties on the specified <paramref name="subject"/>
    /// to ensure <paramref name="callback"/> is called whenever a state is modified
    /// </summary>
    /// <param name="subject">The object to scan for <see cref="IStateChangedNotifier"/> properties.</param>
    /// <param name="callback">The action to execute when one of the states are modified</param>
    /// <returns></returns>
    public static IDisposable Subscribe(object subject, Action<IStateChangedEventArgs<object, Command>> callback) {
        _ = subject ?? throw new ArgumentNullException(nameof(subject));
        _ = callback ?? throw new ArgumentNullException(nameof(callback));

        var subscriptions = (
            from getStateChangedNotifierPropertyValue in GetStateChangedNotifierPropertyDelegatesForType(subject.GetType())
            let store = getStateChangedNotifierPropertyValue(subject)
            select store.Subscribe(new StoreObserver(e => callback(e)))
        ).ToArray();

        return new StoreSubscription(
            id: $"{nameof(StateSubscriber)}.{nameof(Subscribe)}",
            () => {
                foreach (var subscription in subscriptions) {
                    subscription.Dispose();
                }
            }
        );
    }

    private static IEnumerable<PropertyInfo> GetStateChangedNotifierProperties(Type t)
        => t == typeof(object)
        ? Enumerable.Empty<PropertyInfo>()
        : GetStateChangedNotifierProperties(t.BaseType!)
            .Union(
                t.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly)
                    .Where(p => typeof(IStore<object, Command>).IsAssignableFrom(p.PropertyType))
            );

    private static IEnumerable<GetStateChangedPropertyDelegate> GetStateChangedNotifierPropertyDelegatesForType(Type type)
        => _valueDelegatesByType.GetOrAdd(
            type,
            _ =>
                from currentProperty in GetStateChangedNotifierProperties(type)
                let getterMethod = typeof(Func<,>).MakeGenericType(type, currentProperty.PropertyType)
                let stronglyTypedDelegate = Delegate.CreateDelegate(getterMethod, currentProperty.GetGetMethod(true)!)
                select new GetStateChangedPropertyDelegate(
                    x => (IStore<object, Command>)stronglyTypedDelegate.DynamicInvoke(x)!
                )
        );
}