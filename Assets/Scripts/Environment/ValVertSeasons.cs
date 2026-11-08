// Continuous seasonal environment driven by ValVertClock's date (phenology approximations for Seynod, ~450 m):
//  - deciduous leaf-out mid-April..early May, summer green, colouring October, leaf fall mid-Oct..late Nov, bare Dec..Mar
//  - evergreens (laurel, firs/spruces) keep foliage; slight winter darkening only
//  - fallen-leaf litter overlay only from leaf fall until mid-December (no litter baked into base asphalt/soil)
//  - weather: wetness 0..1 (darker, glossier ground; no rain particles). No snow (not modelled).
// Material references are filled by the editor build (ValVertSeason.Wire) so this also works in player builds.
using UnityEngine;
using System;
using System.Collections.Generic;

[ExecuteAlways]
public class ValVertSeasons : MonoBehaviour
{
    [Serializable] public class Foliage { public Material mat; public Texture greenTex, autumnTex; public bool evergreen; [NonSerialized] public Color baseCol; [NonSerialized] public float baseCut; [NonSerialized] public Texture origTex; }
    [Serializable] public class Overlay { public Material mat; public Texture baseTex, baseNor, litterTex, litterNor; [NonSerialized] public Texture o1, o2; }
    [Serializable] public class Wet { public Material mat; [NonSerialized] public Color c; [NonSerialized] public float s; [NonSerialized] public bool cached; }

    public List<Foliage> foliage = new List<Foliage>();
    public List<Overlay> overlays = new List<Overlay>();
    public TerrainLayer forestLayer; public Texture forestLitterTex, forestLitterNor, forestBaseTex, forestBaseNor;
    public List<Wet> wetSurfaces = new List<Wet>();
    [Range(0, 1)] public float wetness = 0f;
    [Tooltip("Override automatic date-driven season (-1 = follow clock).")]
    [Range(-1, 1)] public float yearFractionOverride = -1f;

    // outputs (0..1)
    public float LeafCover { get; private set; }   // deciduous canopy amount
    public float Autumn { get; private set; }      // colouring amount
    public float Litter { get; private set; }      // fallen leaves on ground
    public string SeasonName { get; private set; }
    bool cached;

    static float Ramp(float x, float a, float b) => Mathf.SmoothStep(0, 1, Mathf.InverseLerp(a, b, x));

    public void Evaluate(float doy)
    {
        // day-of-year thresholds (non-leap)
        float leafOut = Ramp(doy, 100, 125);                 // 10 Apr .. 5 May
        float fall = Ramp(doy, 290, 328);                    // 17 Oct .. 24 Nov
        LeafCover = leafOut * (1 - fall);
        Autumn = Ramp(doy, 265, 300) * (1 - fall * 0.3f);    // 22 Sep .. 27 Oct
        if (doy < 200) Autumn = 0;
        Litter = Ramp(doy, 285, 300) * (1 - Ramp(doy, 340, 355)); // 12 Oct .. ~ 20 Dec
        SeasonName = doy < 79 || doy >= 355 ? "winter" : doy < 172 ? "spring" : doy < 266 ? "summer" : "autumn";
    }

    void Cache()
    {
        if (cached) return; cached = true;
        foreach (var f in foliage) if (f.mat) { f.baseCol = f.mat.HasProperty("_BaseColor") ? f.mat.GetColor("_BaseColor") : Color.white; f.baseCut = f.mat.HasProperty("_Cutoff") ? f.mat.GetFloat("_Cutoff") : 0.45f; f.origTex = f.mat.GetTexture("_BaseMap"); }
        foreach (var o in overlays) if (o.mat) { o.o1 = o.mat.GetTexture("_BaseMap"); o.o2 = o.mat.GetTexture("_BumpMap"); }
    }

    void Update() { Tick(); }
    public void Tick()
    {
        var clk = ValVertClock.Instance; if (!clk && yearFractionOverride < 0) return;
        Cache();
        float doy = yearFractionOverride >= 0 ? yearFractionOverride * 365f : clk.YearFraction * 365f;
        Evaluate(doy);
        foreach (var f in foliage)
        {
            if (!f.mat) continue;
            if (f.evergreen)
            {
                bool w = SeasonName == "winter"; f.mat.SetColor("_BaseColor", f.baseCol * (w ? 0.85f : 1f)); continue;
            }
            var tex = Autumn > 0.5f && f.autumnTex ? f.autumnTex : (f.greenTex ? f.greenTex : f.origTex);
            if (tex) f.mat.SetTexture("_BaseMap", tex);
            Color spring = new Color(.95f, 1.05f, .85f);
            float fresh = Mathf.Clamp01(1 - Mathf.Abs(doy - 130) / 30f);          // fresh light green around early May
            f.mat.SetColor("_BaseColor", Color.Lerp(Color.white, spring, fresh));
            // canopy thinning via alpha cutoff: 0.45 full .. 0.97 almost bare
            f.mat.SetFloat("_Cutoff", Mathf.Lerp(0.97f, f.baseCut, LeafCover));
        }
        bool lit = Litter > 0.5f;
        foreach (var o in overlays)
        {
            if (!o.mat) continue;
            var t = lit ? o.litterTex : o.baseTex; var n = lit ? o.litterNor : o.baseNor;
            if (t) o.mat.SetTexture("_BaseMap", t); if (n) o.mat.SetTexture("_BumpMap", n);
        }
        if (forestLayer)
        {
            var t = lit ? forestLitterTex : forestBaseTex; var n = lit ? forestLitterNor : forestBaseNor;
            if (t && forestLayer.diffuseTexture != t) { forestLayer.diffuseTexture = (Texture2D)t; if (n) forestLayer.normalMapTexture = (Texture2D)n; }
        }
        foreach (var w in wetSurfaces)
        {
            if (!w.mat) continue;
            if (!w.cached) { w.c = w.mat.GetColor("_BaseColor"); w.s = w.mat.GetFloat("_Smoothness"); w.cached = true; }
            w.mat.SetColor("_BaseColor", w.c * Mathf.Lerp(1f, 0.72f, wetness));
            w.mat.SetFloat("_Smoothness", Mathf.Min(0.85f, w.s + 0.35f * wetness));   // capped: wet, not mirror
        }
    }

    void OnDisable()
    {
        // restore shared materials so editor assets are not left in a play-mode state
        if (!cached) return;
        foreach (var f in foliage) if (f.mat) { f.mat.SetColor("_BaseColor", f.baseCol); f.mat.SetFloat("_Cutoff", f.baseCut); if (f.origTex) f.mat.SetTexture("_BaseMap", f.origTex); }
        foreach (var o in overlays) if (o.mat) { if (o.o1) o.mat.SetTexture("_BaseMap", o.o1); if (o.o2) o.mat.SetTexture("_BumpMap", o.o2); }
        foreach (var w in wetSurfaces) if (w.mat && w.cached) { w.mat.SetColor("_BaseColor", w.c); w.mat.SetFloat("_Smoothness", w.s); }
        cached = false;
    }
}
