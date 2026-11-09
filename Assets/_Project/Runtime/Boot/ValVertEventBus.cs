using System;
using System.Collections.Generic;

/// <summary>Minimal in-process pub/sub. Not a networked bus.</summary>
public static class ValVertEventBus
{
    static readonly Dictionary<Type, List<Delegate>> _map = new Dictionary<Type, List<Delegate>>();

    public static void Subscribe<T>(Action<T> handler)
    {
        var t = typeof(T);
        if (!_map.TryGetValue(t, out var list)) { list = new List<Delegate>(); _map[t] = list; }
        if (!list.Contains(handler)) list.Add(handler);
    }

    public static void Unsubscribe<T>(Action<T> handler)
    {
        if (_map.TryGetValue(typeof(T), out var list)) list.Remove(handler);
    }

    public static void Publish<T>(T evt)
    {
        if (!_map.TryGetValue(typeof(T), out var list)) return;
        // copy to allow re-entrant unsubscribe
        var snap = list.ToArray();
        for (int i = 0; i < snap.Length; i++)
            ((Action<T>)snap[i])?.Invoke(evt);
    }

    public static void ClearAll() => _map.Clear();

    public static int ListenerCount<T>()
        => _map.TryGetValue(typeof(T), out var list) ? list.Count : 0;
}

public readonly struct ValVertTimeChanged
{
    public readonly int Month, Day;
    public readonly float Hour, Daylight01;
    public ValVertTimeChanged(int month, int day, float hour, float daylight01)
    { Month = month; Day = day; Hour = hour; Daylight01 = daylight01; }
}

public readonly struct ValVertSeasonChanged
{
    public readonly float YearFraction;
    public ValVertSeasonChanged(float yearFraction) { YearFraction = yearFraction; }
}

public readonly struct ValVertWeatherChanged
{
    public readonly float Cloud, Fog, Precip, Snow;
    public ValVertWeatherChanged(float cloud, float fog, float precip, float snow)
    { Cloud = cloud; Fog = fog; Precip = precip; Snow = snow; }
}
