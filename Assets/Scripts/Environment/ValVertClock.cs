// Val Vert runtime environment clock (environment only: no gameplay).
// Calendar/time for Seynod (45.8904 N, 6.1127 E), year 2010, local time CET/CEST.
// Drives: sun position (approximate NOAA solar equations, ~0.5 deg), sun/moon light, ambient, fog, sky exposure,
// street lamps (ValVertStreetLights) and season blending (ValVertSeasons).
using UnityEngine;
using System;

[ExecuteAlways]
public class ValVertClock : MonoBehaviour
{
    public static ValVertClock Instance { get; private set; }

    [Header("Date & time (local civil time)")]
    public int year = 2010;
    [Range(1, 12)] public int month = 10;
    [Range(1, 31)] public int day = 20;
    [Range(0f, 24f)] public float hour = 11f;
    [Tooltip("Game seconds per real second (1 = real time, 60 = one game minute per second).")]
    public float timeScale = 60f;
    public bool running = true;

    [Header("Location")]
    public double latitude = 45.89041, longitude = 6.11269;

    [Header("Scene links (auto-found by name if empty)")]
    public Light sun;          // directional
    public Light moon;         // directional, created if missing
    public Material daySkybox; // existing HDRI/procedural sky
    public Material overcastSkybox; // grey cloud-deck panorama used when the sky is fully overcast with precipitation/fog
    public Color nightAmbient = new Color(0.030f, 0.040f, 0.070f);   // starlight/airglow + urban skyglow (Annecy basin)
    public Color dayAmbient = new Color(0.58f, 0.57f, 0.54f);   // neutral-warm (removed blue cast)
    public float sunMaxIntensity = 1.25f, moonIntensity = 0.12f;
    public Gradient sunColor;
    [Header("Weather inputs (set by ValVertWeather)")]
    [Range(0, 1)] public float cloudCover = 0.3f;
    [Range(0, 1)] public float fogAmount = 0f;
    [Range(0, 1)] public float precip = 0f;

    // outputs
    public float SunAltitudeDeg { get; private set; }
    public float SunAzimuthDeg { get; private set; }
    public float Daylight01 { get; private set; }     // 0 night .. 1 full day (civil twilight smoothed)
    public DateTime Now => new DateTime(year, 1, 1).AddDays(DayOfYear - 1).AddHours(hour);
    public int DayOfYear => new DateTime(year, Mathf.Clamp(month,1,12), Mathf.Clamp(day,1,DateTime.DaysInMonth(year,Mathf.Clamp(month,1,12)))).DayOfYear;
    public float YearFraction => (DayOfYear - 1 + hour / 24f) / (DateTime.IsLeapYear(year) ? 366f : 365f);
    public event Action Changed;

    float baseSkyExposure = 1f; bool expCached; Color baseTint = Color.gray; bool tintCached;

    void OnEnable()
    {
        Instance = this;
        if (sunColor == null || sunColor.colorKeys.Length < 2)
        {
            sunColor = new Gradient();
            sunColor.SetKeys(
                new[] { new GradientColorKey(new Color(1f, .45f, .25f), 0f), new GradientColorKey(new Color(1f, .78f, .58f), .25f), new GradientColorKey(new Color(1f, .96f, .9f), 1f) },
                new[] { new GradientAlphaKey(1, 0), new GradientAlphaKey(1, 1) });
        }
        if (!sun) { var g = GameObject.Find("Sun"); if (g) sun = g.GetComponent<Light>(); }
        if (!moon)
        {
            var g = GameObject.Find("Moon"); if (!g) g = new GameObject("Moon");
            moon = g.GetComponent<Light>(); if (!moon) moon = g.AddComponent<Light>();
            moon.type = LightType.Directional; moon.shadows = LightShadows.Soft; moon.color = new Color(.62f, .70f, .90f);
        }
        if (!daySkybox) daySkybox = RenderSettings.skybox;
        Apply();
    }

    void Update()
    {
        if (running && Application.isPlaying) Advance(Time.deltaTime * timeScale / 3600f);
        Apply();
    }

    public void Advance(float hours)
    {
        hour += hours;
        while (hour >= 24f) { hour -= 24f; var d = Now.Date.AddDays(1); if (d.Year == year) { month = d.Month; day = d.Day; } else { month = 1; day = 1; } }
        while (hour < 0f) { hour += 24f; var d = Now.Date.AddDays(-1); month = d.Month; day = d.Day; }
    }

    public void Set(int m, int d, float h) { month = m; day = d; hour = h; Apply(); }

    // UTC offset: CET (+1) / CEST (+2) — EU rule: last Sunday of March 01:00 UTC to last Sunday of October 01:00 UTC
    public int UtcOffset()
    {
        DateTime LastSun(int mo) { var t = new DateTime(year, mo, DateTime.DaysInMonth(year, mo)); while (t.DayOfWeek != DayOfWeek.Sunday) t = t.AddDays(-1); return t; }
        var n = Now; return (n >= LastSun(3).AddHours(2) && n < LastSun(10).AddHours(3)) ? 2 : 1;
    }

    // NOAA approximate solar position
    public static void SolarPosition(int doy, double utcHour, double lat, double lon, int daysInYear, out double altDeg, out double azDeg)
    {
        double g = 2 * Math.PI / daysInYear * (doy - 1 + (utcHour - 12) / 24.0);
        double eqt = 229.18 * (0.000075 + 0.001868 * Math.Cos(g) - 0.032077 * Math.Sin(g) - 0.014615 * Math.Cos(2 * g) - 0.040849 * Math.Sin(2 * g));
        double decl = 0.006918 - 0.399912 * Math.Cos(g) + 0.070257 * Math.Sin(g) - 0.006758 * Math.Cos(2 * g) + 0.000907 * Math.Sin(2 * g) - 0.002697 * Math.Cos(3 * g) + 0.00148 * Math.Sin(3 * g);
        double tst = utcHour * 60 + eqt + 4 * lon;               // true solar time, minutes
        double ha = (tst / 4 - 180) * Math.PI / 180;
        double la = lat * Math.PI / 180;
        double cosZ = Math.Sin(la) * Math.Sin(decl) + Math.Cos(la) * Math.Cos(decl) * Math.Cos(ha);
        double zen = Math.Acos(Math.Max(-1, Math.Min(1, cosZ)));
        altDeg = 90 - zen * 180 / Math.PI;
        double az = Math.Atan2(Math.Sin(ha), Math.Cos(ha) * Math.Sin(la) - Math.Tan(decl) * Math.Cos(la)); // from south, west positive
        azDeg = (az * 180 / Math.PI + 180) % 360;                 // from north, clockwise
    }

    public float MoonAltitudeDeg { get; private set; }
    public float MoonAzimuthDeg { get; private set; }
    public float MoonIllumination { get; private set; }   // 0 new .. 1 full

    // Low-precision lunar position (Meeus-style truncated series, ~1 deg) and phase from elongation.
    public static void MoonPosition(DateTime utc, double lat, double lon, out double alt, out double az, out double illum)
    {
        double d = (utc - new DateTime(2000, 1, 1, 12, 0, 0)).TotalDays, R = Math.PI / 180;
        double L = 218.316 + 13.176396 * d, M = 134.963 + 13.064993 * d, F = 93.272 + 13.229350 * d;
        double lam = (L + 6.289 * Math.Sin(M * R)) * R, bet = 5.128 * Math.Sin(F * R) * R, eps = 23.439 * R;
        double ra = Math.Atan2(Math.Sin(lam) * Math.Cos(eps) - Math.Tan(bet) * Math.Sin(eps), Math.Cos(lam));
        double dec = Math.Asin(Math.Sin(bet) * Math.Cos(eps) + Math.Cos(bet) * Math.Sin(eps) * Math.Sin(lam));
        double lst = (280.16 + 360.9856235 * d + lon) * R, H = lst - ra, la = lat * R;
        alt = Math.Asin(Math.Sin(la) * Math.Sin(dec) + Math.Cos(la) * Math.Cos(dec) * Math.Cos(H)) / R;
        az = (Math.Atan2(Math.Sin(H), Math.Cos(H) * Math.Sin(la) - Math.Tan(dec) * Math.Cos(la)) / R + 180 + 360) % 360;
        double g = (357.528 + 0.9856003 * d) * R, sunLam = (280.46 + 0.9856474 * d) * R + (1.915 * Math.Sin(g) + 0.02 * Math.Sin(2 * g)) * R;
        double e = Math.Acos(Math.Cos(bet) * Math.Cos(lam - sunLam));
        illum = (1 - Math.Cos(e)) / 2;
    }

    public void Apply()
    {
        int diy = DateTime.IsLeapYear(year) ? 366 : 365;
        double utc = hour - UtcOffset();
        SolarPosition(DayOfYear, utc, latitude, longitude, diy, out double alt, out double az);
        SunAltitudeDeg = (float)alt; SunAzimuthDeg = (float)az;
        // civil twilight: full night below -6 deg, full day above +6 deg
        Daylight01 = Mathf.SmoothStep(0, 1, Mathf.InverseLerp(-6f, 6f, SunAltitudeDeg));

        // scene axes: +Z = north, +X = east (map built from lon/lat that way)
        if (sun)
        {
            sun.transform.rotation = Quaternion.Euler(Mathf.Max(SunAltitudeDeg, -10f), SunAzimuthDeg + 180f, 0f);
            float aboveHorizon = Mathf.Clamp01(Mathf.InverseLerp(-1f, 10f, SunAltitudeDeg));
            sun.intensity = sunMaxIntensity * aboveHorizon * Mathf.Lerp(0.35f, 1f, Mathf.Clamp01(SunAltitudeDeg / 40f));
            sun.color = sunColor.Evaluate(Mathf.Clamp01(SunAltitudeDeg / 30f));
            sun.intensity *= Mathf.Lerp(1f, 0.28f, cloudCover);                                   // overcast: diffuse light, weak direct sun
            sun.shadowStrength = Mathf.Lerp(0.9f, 0.25f, cloudCover);
            sun.enabled = sun.intensity > 0.001f;
        }
        MoonPosition(Now.AddHours(-UtcOffset()), latitude, longitude, out double malt, out double maz, out double mil);
        MoonAltitudeDeg = (float)malt; MoonAzimuthDeg = (float)maz; MoonIllumination = (float)mil;
        if (moon)
        {
            // moonlight: real 2010 position & phase; ~0.25 lux full moon -> scaled to a readable but dark directional fill
            moon.transform.rotation = Quaternion.Euler(Mathf.Max(MoonAltitudeDeg, 2f), MoonAzimuthDeg + 180f, 0f);
            moon.intensity = (1f - cloudCover * 0.85f) * moonIntensity * MoonIllumination * Mathf.Clamp01(MoonAltitudeDeg / 10f) * (1f - Daylight01);
            moon.enabled = moon.intensity > 0.002f;
            moon.shadows = moon.enabled ? LightShadows.Soft : LightShadows.None;
        }
        RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
        Color dusk = new Color(.30f, .24f, .26f);
        float twilight = 1f - Mathf.Abs(Daylight01 * 2f - 1f);
        // sky light follows twilight more slowly than direct sun: civil twilight (sun -6..0) stays readable
        float sky = Mathf.SmoothStep(0, 1, Mathf.InverseLerp(-9f, 3f, SunAltitudeDeg));
        Color moonAmb = new Color(0.05f, 0.065f, 0.11f) * MoonIllumination * Mathf.Clamp01(MoonAltitudeDeg / 15f);
        RenderSettings.ambientLight = Color.Lerp(Color.Lerp(nightAmbient + moonAmb, dayAmbient, sky), dusk, twilight * 0.35f);
        RenderSettings.ambientLight = Color.Lerp(RenderSettings.ambientLight, new Color(.6f,.61f,.62f) * Mathf.Lerp(0.15f, 1f, sky), cloudCover * 0.5f * sky);   // overcast: flatter grey skylight
        RenderSettings.fogColor = Color.Lerp(new Color(.03f, .035f, .045f), Color.Lerp(new Color(.72f, .72f, .70f), new Color(.62f, .63f, .64f), cloudCover), Daylight01);
        RenderSettings.fog = true; RenderSettings.fogMode = FogMode.ExponentialSquared;
        RenderSettings.fogDensity = 0.0008f + 0.004f * cloudCover * precip + 0.045f * fogAmount;   // light haze .. valley fog (~100 m visibility)
        if (daySkybox && overcastSkybox)
        {
            bool oc = cloudCover > 0.88f && (precip > 0.15f || fogAmount > 0.4f);
            var want = oc ? overcastSkybox : daySkybox;
            if (RenderSettings.skybox != want) { if (expCached && RenderSettings.skybox == daySkybox) { daySkybox.SetFloat("_Exposure", baseSkyExposure); if (tintCached) daySkybox.SetColor("_Tint", baseTint); } RenderSettings.skybox = want; }
            if (oc) overcastSkybox.SetFloat("_Exposure", Mathf.Lerp(0.03f, 0.9f, sky));
        }
        if (RenderSettings.skybox && RenderSettings.skybox == daySkybox && RenderSettings.skybox.HasProperty("_Exposure"))
        {
            if (!expCached) { baseSkyExposure = RenderSettings.skybox.GetFloat("_Exposure"); expCached = true; }
            // HDRI with baked sun: darken to near-black at night so no daytime sun shows
            float skyE = Mathf.Lerp(0.03f + 0.05f * MoonIllumination * Mathf.Clamp01(MoonAltitudeDeg / 15f) * (1f - cloudCover), Mathf.Lerp(1f, 0.55f, cloudCover * precip + 0.3f * cloudCover), sky);
            RenderSettings.skybox.SetFloat("_Exposure", baseSkyExposure * skyE);
            if (RenderSettings.skybox.HasProperty("_Tint")) { if (!tintCached) { baseTint = RenderSettings.skybox.GetColor("_Tint"); tintCached = true; } RenderSettings.skybox.SetColor("_Tint", Color.Lerp(new Color(.30f, .38f, .62f), Color.Lerp(baseTint, new Color(.5f, .5f, .5f), cloudCover * 0.8f), sky)); }
        }
        Changed?.Invoke();
    }

    void OnDisable()
    {
        if (daySkybox && RenderSettings.skybox == overcastSkybox) RenderSettings.skybox = daySkybox;
        if (expCached && RenderSettings.skybox && RenderSettings.skybox.HasProperty("_Exposure")) RenderSettings.skybox.SetFloat("_Exposure", baseSkyExposure); if (tintCached && RenderSettings.skybox) RenderSettings.skybox.SetColor("_Tint", baseTint);
        if (Instance == this) Instance = null;
    }
}
