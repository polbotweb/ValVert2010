// Val Vert runtime weather (environment only). Deterministic, seeded 2010 weather sequence for the Annecy basin (~450 m):
// clear / overcast / drizzle / rain / fog-mist / snow, with smooth transitions, temperature model, snow accumulation & melt,
// wetness & drying. Drives: ValVertClock (cloud cover, fog), ValVertSeasons (wetness), precipitation particles following the camera,
// snow cover on terrain layers / ground / roads (slush) / roofs / evergreen foliage via pre-blended step textures.
// Climate numbers are approximate monthly normals (MeteoFrance Annecy-Meythet order of magnitude), not observed 2010 data.
// Debug: F6 cycles forced weather (Auto -> Clear -> Overcast -> Drizzle -> Rain -> Fog -> Snow), or call Force().
using UnityEngine;
using System;
using System.Collections.Generic;

[ExecuteAlways]
public class ValVertWeather : MonoBehaviour
{
    public enum Kind { Auto = -1, Clear, Overcast, Drizzle, Rain, Fog, Snow }
    public Kind forced = Kind.Auto;
    public int seed = 2010;

    [Serializable] public class SnowTex { public Material mat; public string texProp = "_BaseMap"; public Texture[] steps; public float factor = 1f; public bool seasonal; [NonSerialized] public Texture orig; }
    [Serializable] public class SnowLayer { public TerrainLayer layer; public Texture[] steps; public float factor = 1f; [NonSerialized] public Texture orig; }
    [Serializable] public class SnowTint { public Material mat; public string colorProp = "_BaseColor"; public Color snowColor = new Color(.92f, .94f, .98f); public float amount = 0.6f; [NonSerialized] public Color orig; [NonSerialized] public bool cached; }
    public List<SnowTex> snowMats = new List<SnowTex>();
    public List<SnowLayer> snowLayers = new List<SnowLayer>();
    public List<SnowTint> snowTints = new List<SnowTint>();
    public Material rainMat, snowMat;

    // current state (0..1)
    public Kind Current { get; private set; }
    public float Cloud { get; private set; }
    public float Rain { get; private set; }
    public float Snowfall { get; private set; }
    public float Fog { get; private set; }
    public float SnowCover { get; private set; }
    public float Wetness { get; private set; }
    public float TemperatureC { get; private set; }

    static readonly float[] Tmean = { 1.5f, 3f, 7f, 10f, 14.5f, 18f, 20.5f, 20f, 16f, 11.5f, 5.5f, 2.5f };
    static readonly float[] Pwet = { .33f, .31f, .34f, .38f, .42f, .37f, .32f, .33f, .33f, .37f, .37f, .36f };

    ParticleSystem rainPS, snowPS; DateTime lastSim = DateTime.MinValue;
    public Transform viewer;

    float H(int a, int b) { unchecked { uint h = (uint)(a * 73856093) ^ (uint)(b * 19349663) ^ (uint)(seed * 83492791); h ^= h >> 13; h *= 0x5bd1e995; h ^= h >> 15; return (h & 0xFFFFFF) / 16777216f; } }

    // target weather for a given local time
    public void Target(DateTime t, out Kind k, out float temp, out float intensity)
    {
        int day = t.DayOfYear + 1000 * t.Year; int m = t.Month - 1; float h = t.Hour + t.Minute / 60f;
        float anom = (H(day, 1) + H(day, 2) + H(day, 3) - 1.5f) * 5f;                 // daily anomaly ~ +-4 C
        float diurnal = -Mathf.Cos((h - 3f) / 24f * Mathf.PI * 2f) * 4f;               // min ~03-06h, max ~15h
        bool wetDay = H(day, 4) < Pwet[m];
        float start = H(day, 5) * 20f, dur = 3f + H(day, 6) * 14f;
        bool inPrecip = wetDay && ((h >= start && h < start + dur) || (h + 24f >= start && h + 24f < start + dur));
        temp = Tmean[m] + anom + diurnal + (wetDay ? -1.5f : 0f);
        intensity = 0.4f + 0.6f * H(day, 7);
        bool cloudy = wetDay || H(day, 8) < 0.45f;
        bool fogMorning = !inPrecip && (m >= 9 || m <= 1) && h > 5f && h < 11.5f && H(day, 9) < 0.30f;   // valley fog Oct-Feb
        if (inPrecip) k = temp < 1.0f ? Kind.Snow : (intensity < 0.55f ? Kind.Drizzle : Kind.Rain);
        else if (fogMorning) k = Kind.Fog;
        else k = cloudy ? Kind.Overcast : Kind.Clear;
    }

    void TargetLevels(Kind k, float inten, out float cloud, out float rain, out float snow, out float fog)
    {
        cloud = k == Kind.Clear ? 0.15f : k == Kind.Overcast ? 0.8f : k == Kind.Fog ? 0.9f : 1f;
        rain = k == Kind.Rain ? 0.5f + 0.5f * inten : k == Kind.Drizzle ? 0.2f : 0f;
        snow = k == Kind.Snow ? 0.4f + 0.6f * inten : 0f;
        fog = k == Kind.Fog ? 1f : k == Kind.Snow ? 0.45f : k == Kind.Rain ? 0.3f : k == Kind.Drizzle ? 0.35f : k == Kind.Overcast ? 0.1f : 0f;
    }

    // integrate state over game time (hours), smooth transitions
    void Step(DateTime t, float dh)
    {
        Target(t, out var k, out float temp, out float inten);
        if (forced != Kind.Auto) { k = forced; inten = 0.8f; if (k == Kind.Snow) temp = Mathf.Min(temp, -1f); }
        Current = k; TemperatureC = temp;
        TargetLevels(k, inten, out float c, out float r, out float s, out float f);
        float a = 1f - Mathf.Exp(-dh / 0.75f);                                           // ~45 min transitions
        Cloud = Mathf.Lerp(Cloud, c, a); Rain = Mathf.Lerp(Rain, r, a); Snowfall = Mathf.Lerp(Snowfall, s, a); Fog = Mathf.Lerp(Fog, f, a);
        SnowCover = Mathf.Clamp01(SnowCover + Snowfall * 0.10f * dh - (temp > 0.5f ? (0.012f * temp + 0.25f * Rain) * dh : 0f));
        if (forced == Kind.Snow) SnowCover = Mathf.Max(SnowCover, 0.85f);
        float dry = 0.05f + 0.01f * Mathf.Max(0, temp);
        Wetness = Mathf.Clamp01(Wetness + (Rain * 0.8f + Snowfall * 0.1f) * dh - dry * dh * (1f - Cloud * 0.5f));
        if (SnowCover > 0.05f && temp > 0.5f) Wetness = Mathf.Max(Wetness, 0.6f);      // melt water / slush
    }

    // jump to a date: replay the previous 10 days hourly so snow cover / wetness are consistent with the history
    public void Settle(DateTime t)
    {
        Cloud = Rain = Snowfall = Fog = SnowCover = Wetness = 0;
        for (var u = t.AddDays(-10); u < t; u = u.AddMinutes(30)) Step(u, 0.5f);
        Step(t, 2f); lastSim = t;
    }

    public void Force(Kind k) { forced = k; lastSim = DateTime.MinValue; }

    void Update()
    {
#if ENABLE_INPUT_SYSTEM
        if (Application.isPlaying && UnityEngine.InputSystem.Keyboard.current != null && UnityEngine.InputSystem.Keyboard.current.f6Key.wasPressedThisFrame) Force((Kind)(((int)forced + 2) % 7 - 1));
#endif
        Tick();
    }

    public void Tick()
    {
        var clk = ValVertClock.Instance; if (!clk) return;
        var now = clk.Now;
        if (lastSim == DateTime.MinValue || Math.Abs((now - lastSim).TotalHours) > 6) Settle(now);
        else if (now > lastSim) { Step(now, (float)(now - lastSim).TotalHours); lastSim = now; }
        ApplyEffects(clk);
    }

    static int StepIndex(float v, int n) => Mathf.Clamp(Mathf.RoundToInt(v * (n - 1)), 0, n - 1);

    void ApplyEffects(ValVertClock clk)
    {
        clk.cloudCover = Cloud; clk.fogAmount = Fog; clk.precip = Mathf.Max(Rain, Snowfall);
        var sea = GetComponent<ValVertSeasons>(); if (sea) { sea.wetness = Wetness; sea.snowCover = SnowCover; }
        foreach (var s in snowMats)
        {
            if (!s.mat || s.steps == null || s.steps.Length < 2) continue;
            if (!s.orig) s.orig = s.mat.GetTexture(s.texProp);
            int i = StepIndex(SnowCover * s.factor, s.steps.Length);
            if (i > 0) s.mat.SetTexture(s.texProp, s.steps[i]); else if (!s.seasonal) s.mat.SetTexture(s.texProp, s.orig);
        }
        foreach (var l in snowLayers)
        {
            if (!l.layer || l.steps == null || l.steps.Length < 2) continue;
            if (!l.orig) l.orig = l.layer.diffuseTexture;
            int i = StepIndex(SnowCover * l.factor, l.steps.Length);
            var t = i == 0 ? null : l.steps[i];
            if (t && l.layer.diffuseTexture != t) l.layer.diffuseTexture = (Texture2D)t;
            else if (!t && sea == null && l.layer.diffuseTexture != l.orig) l.layer.diffuseTexture = (Texture2D)l.orig;
        }
        foreach (var s in snowTints)
        {
            if (!s.mat) continue;
            if (!s.cached) { s.orig = s.mat.GetColor(s.colorProp); s.cached = true; }
            s.mat.SetColor(s.colorProp, Color.Lerp(s.orig, s.snowColor, SnowCover * s.amount));
        }
        Particles();
    }

    void Particles()
    {
        var cam = viewer ? viewer : (Camera.main ? Camera.main.transform : null);
        if (!rainPS) rainPS = MakePS("vv_rain", rainMat, true);
        if (!snowPS) snowPS = MakePS("vv_snow", snowMat, false);
        foreach (var (ps, amt, rate) in new[] { (rainPS, Rain, 9000f), (snowPS, Snowfall, 6000f) })
        {
            if (!ps) continue;
            if (cam) ps.transform.position = cam.position + Vector3.up * 12f + cam.forward * 6f;
            var em = ps.emission; em.rateOverTime = amt * rate;
            if (amt > 0.02f) { if (!ps.isPlaying) ps.Play(); } else { ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear); ps.Clear(true); }
        }
    }

    ParticleSystem MakePS(string n, Material m, bool rain)
    {
        var t = transform.Find(n); var go = t ? t.gameObject : new GameObject(n); go.transform.SetParent(transform, false); go.hideFlags = HideFlags.DontSave;
        var ps = go.GetComponent<ParticleSystem>(); if (!ps) ps = go.AddComponent<ParticleSystem>();
        ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        var main = ps.main; main.loop = true; main.playOnAwake = false; main.simulationSpace = ParticleSystemSimulationSpace.World; main.maxParticles = rain ? 12000 : 8000;
        main.startLifetime = rain ? 1.6f : 9f; main.startSpeed = rain ? 9.5f : 1.1f; main.startSize = rain ? 0.03f : 0.06f;
        main.startColor = rain ? new Color(.8f, .83f, .88f, .6f) : new Color(1f, 1f, 1f, .95f); main.gravityModifier = rain ? 0.4f : 0.02f;
        var sh = ps.shape; sh.shapeType = ParticleSystemShapeType.Box; sh.scale = new Vector3(40f, 0.5f, 40f); sh.rotation = new Vector3(90f, 0, 0);
        var em = ps.emission; em.rateOverTime = 0;
        if (!rain) { var no = ps.noise; no.enabled = true; no.strength = 0.6f; no.frequency = 0.3f; }
        var r = go.GetComponent<ParticleSystemRenderer>(); r.sharedMaterial = m;
        r.renderMode = rain ? ParticleSystemRenderMode.Stretch : ParticleSystemRenderMode.Billboard; r.lengthScale = rain ? 9f : 1f; r.velocityScale = rain ? 0.03f : 0f;
        r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        return ps;
    }

    // batch rendering helper: pre-warm particles so a single frame shows falling precipitation
    public void Prewarm(float seconds = 4f)
    {
        Particles();
        foreach (var ps in new[] { rainPS, snowPS }) { if (!ps) continue; ps.Clear(true); if (ps.emission.rateOverTime.constant > 0) ps.Simulate(seconds, true, true); }
    }

    void OnDisable()
    {
        foreach (var s in snowMats) if (s.mat && s.orig) s.mat.SetTexture(s.texProp, s.orig);
        foreach (var l in snowLayers) if (l.layer && l.orig) l.layer.diffuseTexture = (Texture2D)l.orig;
        foreach (var s in snowTints) if (s.mat && s.cached) s.mat.SetColor(s.colorProp, s.orig);
    }
}
