using Memento.Core;
using System.Collections.Concurrent;
using System.Reflection;

namespace Memento.Blazor;

using GetStateChangedPropertyDelegate = Func<object, IStateObservable<object>>;

using TStateObservable = IStateObservable<object>;

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
    public static IDisposable Subscribe(object subject, Action<IStateChangedEventArgs> callback) {
        _ = subject ?? throw new ArgumentNullException(nameof(subject));
        _ = callback ?? throw new ArgumentNullException(nameof(callback));

        var subscriptions = GetStateChangedNotifierPropertyDelegatesForType(subject.GetType())
            .Select(x => x(subject).Subscribe(new StateObserver(e => callback(e))))
            .ToArray();

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
        ? []
        : GetStateChangedNotifierProperties(t.BaseType!)
            .Union(
                t.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly)
                    .Where(p => typeof(TStateObservable).IsAssignableFrom(p.PropertyType))
            );

    private static IEnumerable<GetStateChangedPropertyDelegate> GetStateChangedNotifierPropertyDelegatesForType(Type type)
        => _valueDelegatesByType.GetOrAdd(
            type,
            _ =>
                from currentProperty in GetStateChangedNotifierProperties(type)
                let getterMethod = typeof(Func<,>).MakeGenericType(type, currentProperty.PropertyType)
                let stronglyTypedDelegate = Delegate.CreateDelegate(getterMethod, currentProperty.GetGetMethod(true)!)
                select new GetStateChangedPropertyDelegate(
                    x => (TStateObservable)stronglyTypedDelegate.DynamicInvoke(x)!
                )
        );
}