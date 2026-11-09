using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>Lightweight service registry for Val Vert runtime systems. Not a DI container.</summary>
public static class ValVertServices
{
    static readonly Dictionary<Type, object> _services = new Dictionary<Type, object>();

    public static void Register<T>(T instance) where T : class
    {
        if (instance == null) throw new ArgumentNullException(nameof(instance));
        _services[typeof(T)] = instance;
    }

    public static bool TryGet<T>(out T service) where T : class
    {
        if (_services.TryGetValue(typeof(T), out var o) && o is T t) { service = t; return true; }
        service = null; return false;
    }

    public static T Get<T>() where T : class
    {
        if (TryGet<T>(out var s)) return s;
        throw new InvalidOperationException("Service not registered: " + typeof(T).Name);
    }

    public static void Unregister<T>() where T : class => _services.Remove(typeof(T));

    public static void Clear() => _services.Clear();

    public static int Count => _services.Count;
}
