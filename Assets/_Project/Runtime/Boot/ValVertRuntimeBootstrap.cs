using UnityEngine;

/// <summary>
/// Wires existing clock/season/weather into EventBus + Services and registers known world
/// streaming regions. Preserves prior component behavior; adds observable lifecycle only.
/// </summary>
[DefaultExecutionOrder(-1000)]
public class ValVertRuntimeBootstrap : MonoBehaviour
{
    public ValVertWorldConfig config;
    public bool autoFindEnvSystems = true;
    public bool autoRegisterStreamingRegions = true;

    ValVertClock _clock;
    ValVertSeasons _seasons;
    ValVertWeather _weather;
    ValVertWorldStreaming _streaming;

    float _lastYearFrac = float.NaN;
    float _lastCloud = float.NaN, _lastFog = float.NaN, _lastPrecip = float.NaN, _lastSnow = float.NaN;

    void OnEnable()
    {
        ValVertServices.Clear();
        ValVertEventBus.ClearAll();
        if (autoFindEnvSystems) FindSystems();
        RegisterServices();
        HookClock();
        if (autoRegisterStreamingRegions) EnsureStreaming();
    }

    void OnDisable()
    {
        UnhookClock();
        ValVertServices.Clear();
        ValVertEventBus.ClearAll();
    }

    void FindSystems()
    {
        _clock = FindFirstObjectByType<ValVertClock>();
        _seasons = FindFirstObjectByType<ValVertSeasons>();
        _weather = FindFirstObjectByType<ValVertWeather>();
    }

    void RegisterServices()
    {
        if (_clock) ValVertServices.Register(_clock);
        if (_seasons) ValVertServices.Register(_seasons);
        if (_weather) ValVertServices.Register(_weather);
        ValVertServices.Register(this);
    }

    void HookClock()
    {
        if (!_clock) return;
        _clock.Changed -= OnClockChanged;
        _clock.Changed += OnClockChanged;
        OnClockChanged(); // publish initial
    }

    void UnhookClock()
    {
        if (_clock != null) _clock.Changed -= OnClockChanged;
    }

    void OnClockChanged()
    {
        if (!_clock) return;
        ValVertEventBus.Publish(new ValVertTimeChanged(_clock.month, _clock.day, _clock.hour, _clock.Daylight01));
        float yf = _clock.YearFraction;
        if (float.IsNaN(_lastYearFrac) || Mathf.Abs(yf - _lastYearFrac) > 0.0005f)
        {
            _lastYearFrac = yf;
            ValVertEventBus.Publish(new ValVertSeasonChanged(yf));
        }
        PublishWeatherIfChanged();
    }

    void LateUpdate()
    {
        // Weather may mutate without Clock.Changed — poll lightly
        PublishWeatherIfChanged();
    }

    void PublishWeatherIfChanged()
    {
        if (!_clock) return;
        float cloud = _clock.cloudCover, fog = _clock.fogAmount, precip = _clock.precip;
        float snow = _weather ? _weather.SnowCover : 0f;
        if (float.IsNaN(_lastCloud) || cloud != _lastCloud || fog != _lastFog || precip != _lastPrecip || snow != _lastSnow)
        {
            _lastCloud = cloud; _lastFog = fog; _lastPrecip = precip; _lastSnow = snow;
            ValVertEventBus.Publish(new ValVertWeatherChanged(cloud, fog, precip, snow));
        }
    }

    void EnsureStreaming()
    {
        _streaming = GetComponent<ValVertWorldStreaming>() ?? gameObject.AddComponent<ValVertWorldStreaming>();
        if (!_streaming.viewer)
        {
            var fps = FindFirstObjectByType<SimpleFPS>();
            if (fps) _streaming.viewer = fps.transform;
            else if (Camera.main) _streaming.viewer = Camera.main.transform;
        }
        float load = config ? config.streamLoadRadius : 380f;
        float unload = config ? config.streamUnloadRadius : 460f;
        TryReg("terrain_main", "ValVertTerrain", load, unload, keepWarm: true);
        TryReg("terrain_east", "ValVertTerrainEast", load, unload, keepWarm: false);
        TryReg("east_meshes", "V3_East", load, unload, keepWarm: false);
        TryReg("grass", "V3_Grass", load * 0.85f, unload * 0.85f, keepWarm: true);
    }

    void TryReg(string id, string objectName, float load, float unload, bool keepWarm)
    {
        var go = GameObject.Find(objectName);
        if (go) _streaming.Register(id, go.transform, load, unload, keepWarm);
    }

    /// <summary>Editor/wire entry: ensure bootstrap exists on Env host.</summary>
    public static ValVertRuntimeBootstrap EnsureOn(GameObject host, ValVertWorldConfig cfg = null)
    {
        var b = host.GetComponent<ValVertRuntimeBootstrap>() ?? host.AddComponent<ValVertRuntimeBootstrap>();
        if (cfg) b.config = cfg;
        b.autoFindEnvSystems = true;
        b.autoRegisterStreamingRegions = true;
        return b;
    }
}
