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
    [Serializable] public class Foliage { public Material mat; public Texture greenTex, autumnTex; public bool evergreen; public string colorProp = "_BaseColor", texProp = "_BaseMap", cutProp = "_Cutoff"; public Color autumnTint = new Color(1f, 1f, 1f, 1f), autumnTint2 = new Color(1f, 1f, 1f, 1f); public bool hasRest; public Color restColor = Color.white; public float restCut = 0.45f; /* pristine values captured at wiring time (avoid drift from saved, already-tinted materials) */ [NonSerialized] public Color baseCol; [NonSerialized] public float baseCut; [NonSerialized] public Texture origTex; }
    [Serializable] public class Overlay { public Material mat; public Texture baseTex, baseNor, litterTex, litterNor; public Texture[] steps; [NonSerialized] public Texture o1, o2; }
    [Serializable] public class Wet { public Material mat; [NonSerialized] public Color c; [NonSerialized] public float s; [NonSerialized] public bool cached; }

    public List<Foliage> foliage = new List<Foliage>();
    public List<Overlay> overlays = new List<Overlay>();
    public TerrainLayer forestLayer; public Texture forestLitterTex, forestLitterNor, forestBaseTex, forestBaseNor; public Texture[] forestSteps;
    public List<Wet> wetSurfaces = new List<Wet>();
    [Serializable] public class WetLayer { public TerrainLayer layer; public Vector4 restMax = Vector4.one; public float restSmooth; }
    public List<WetLayer> wetLayers = new List<WetLayer>();   // terrain paths / soil darken when wet (rest values captured at wiring)
    [Range(0, 1)] public float wetness = 0f;
    [Range(0, 1)] public float snowCover = 0f;   // set by ValVertWeather; ground overlays are then owned by the snow steps
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
        Litter = Ramp(doy, 278, 325) * (1 - Ramp(doy, 335, 366));   // fades in with leaf fall (early Oct..late Nov), decomposes/fades through December
        SeasonName = doy < 79 || doy >= 355 ? "winter" : doy < 172 ? "spring" : doy < 266 ? "summer" : "autumn";
    }

    void Cache()
    {
        if (cached) return; cached = true;
        foreach (var f in foliage) if (f.mat) { f.baseCol = f.mat.HasProperty(f.colorProp) ? f.mat.GetColor(f.colorProp) : Color.white; f.baseCut = f.mat.HasProperty(f.cutProp) ? f.mat.GetFloat(f.cutProp) : 0.45f; if (f.hasRest) { f.baseCol = f.restColor; f.baseCut = f.restCut; } f.origTex = f.mat.HasProperty(f.texProp) ? f.mat.GetTexture(f.texProp) : null; }
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
                bool w = SeasonName == "winter"; f.mat.SetColor(f.colorProp, f.baseCol * (w ? 0.85f : 1f)); continue;
            }
            var tex = Autumn > 0.5f && f.autumnTex ? f.autumnTex : (f.greenTex ? f.greenTex : f.origTex);
            if (tex) f.mat.SetTexture(f.texProp, tex);
            Color spring = new Color(1.06f, 1.08f, .80f);   // fresh yellow-green (was bluish)
            float fresh = Mathf.Clamp01(1 - Mathf.Abs(doy - 130) / 30f);          // fresh light green around early May
            var tint = Color.Lerp(Color.white, spring, fresh); if (!f.autumnTex) { var at = Autumn < 0.5f ? Color.Lerp(Color.white, f.autumnTint, Autumn * 2f) : Color.Lerp(f.autumnTint, f.autumnTint2 == Color.white ? f.autumnTint : f.autumnTint2, Autumn * 2f - 1f); tint = new Color(tint.r * at.r, tint.g * at.g, tint.b * at.b, 1f); } f.mat.SetColor(f.colorProp, f.baseCol * tint);   // keep the material's own tint (UNS leaves are white masks tinted by _BaseColor)
            // canopy thinning via alpha cutoff: 0.45 full .. 0.97 almost bare
            f.mat.SetFloat(f.cutProp, Mathf.Lerp(0.995f, Mathf.Min(f.baseCut, 0.45f), LeafCover));
        }
        bool lit = Litter > 0.5f; bool snowy = snowCover > 0.05f;
        if (!snowy) foreach (var o in overlays)
        {
            if (!o.mat) continue;
            var t = lit ? o.litterTex : o.baseTex; var n = lit ? o.litterNor : o.baseNor;
            if (o.steps != null && o.steps.Length > 1) t = o.steps[Mathf.Clamp(Mathf.RoundToInt(Litter * (o.steps.Length - 1)), 0, o.steps.Length - 1)];   // progressive coverage
            if (t) o.mat.SetTexture("_BaseMap", t); if (n) o.mat.SetTexture("_BumpMap", n);
        }
        if (forestLayer && !snowy)
        {
            var t = lit ? forestLitterTex : forestBaseTex; var n = lit ? forestLitterNor : forestBaseNor;
            if (forestSteps != null && forestSteps.Length > 1) t = forestSteps[Mathf.Clamp(Mathf.RoundToInt(Litter * (forestSteps.Length - 1)), 0, forestSteps.Length - 1)];
            if (t && forestLayer.diffuseTexture != t) { forestLayer.diffuseTexture = (Texture2D)t; if (n) forestLayer.normalMapTexture = (Texture2D)n; }
        }
        foreach (var wl in wetLayers)
        {
            if (!wl.layer) continue; float k = Mathf.Lerp(1f, 0.58f, wetness);
            var mx = new Vector4(wl.restMax.x * k, wl.restMax.y * k, wl.restMax.z * k, wl.restMax.w); if (wl.layer.diffuseRemapMax != mx) wl.layer.diffuseRemapMax = mx;
            float sm = Mathf.Min(0.5f, wl.restSmooth + 0.2f * wetness); if (Mathf.Abs(wl.layer.smoothness - sm) > 0.005f) wl.layer.smoothness = sm;
        }
        foreach (var w in wetSurfaces)
        {
            if (!w.mat) continue;
            if (!w.cached) { w.c = w.mat.GetColor("_BaseColor"); w.s = w.mat.GetFloat("_Smoothness"); w.cached = true; }
            w.mat.SetColor("_BaseColor", w.c * Mathf.Lerp(1f, 0.6f, wetness));   // wet = darker albedo
            w.mat.SetFloat("_Smoothness", Mathf.Min(0.6f, w.s + 0.22f * wetness));   // subtle sheen, no milky grazing-angle glare
        }
    }

    void OnDisable()
    {
        // restore shared materials so editor assets are not left in a play-mode state
        if (!cached) return;
        foreach (var f in foliage) if (f.mat) { f.mat.SetColor(f.colorProp, f.baseCol); f.mat.SetFloat(f.cutProp, f.baseCut); if (f.origTex) f.mat.SetTexture(f.texProp, f.origTex); }
        foreach (var o in overlays) if (o.mat) { if (o.o1) o.mat.SetTexture("_BaseMap", o.o1); if (o.o2) o.mat.SetTexture("_BumpMap", o.o2); }
        foreach (var wl in wetLayers) if (wl.layer) { wl.layer.diffuseRemapMax = wl.restMax; wl.layer.smoothness = wl.restSmooth; }
        foreach (var w in wetSurfaces) if (w.mat && w.cached) { w.mat.SetColor("_BaseColor", w.c); w.mat.SetFloat("_Smoothness", w.s); }
        cached = false;
    }
}
