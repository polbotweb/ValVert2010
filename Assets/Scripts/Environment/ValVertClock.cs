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
    public Color nightAmbient = new Color(0.035f, 0.045f, 0.07f);
    public Color dayAmbient = new Color(0.55f, 0.58f, 0.62f);
    public float sunMaxIntensity = 1.25f, moonIntensity = 0.06f;
    public Gradient sunColor;

    // outputs
    public float SunAltitudeDeg { get; private set; }
    public float SunAzimuthDeg { get; private set; }
    public float Daylight01 { get; private set; }     // 0 night .. 1 full day (civil twilight smoothed)
    public DateTime Now => new DateTime(year, 1, 1).AddDays(DayOfYear - 1).AddHours(hour);
    public int DayOfYear => new DateTime(year, Mathf.Clamp(month,1,12), Mathf.Clamp(day,1,DateTime.DaysInMonth(year,Mathf.Clamp(month,1,12)))).DayOfYear;
    public float YearFraction => (DayOfYear - 1 + hour / 24f) / (DateTime.IsLeapYear(year) ? 366f : 365f);
    public event Action Changed;

    float baseSkyExposure = 1f; bool expCached;

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
            var g = GameObject.Find("Moon") ?? new GameObject("Moon");
            moon = g.GetComponent<Light>() ?? g.AddComponent<Light>();
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
            sun.enabled = sun.intensity > 0.001f;
        }
        if (moon)
        {
            // simple moon: opposite-ish to the sun, fixed fill (no lunar phase model)
            moon.transform.rotation = Quaternion.Euler(35f, SunAzimuthDeg, 0f);
            moon.intensity = moonIntensity * (1f - Daylight01);
            moon.enabled = moon.intensity > 0.002f;
            moon.shadows = moon.enabled ? LightShadows.Soft : LightShadows.None;
        }
        RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
        Color dusk = new Color(.30f, .24f, .26f);
        float twilight = 1f - Mathf.Abs(Daylight01 * 2f - 1f);
        RenderSettings.ambientLight = Color.Lerp(Color.Lerp(nightAmbient, dayAmbient, Daylight01), dusk, twilight * 0.35f);
        RenderSettings.fogColor = Color.Lerp(new Color(.02f, .025f, .04f), new Color(.70f, .73f, .76f), Daylight01);
        if (RenderSettings.skybox && RenderSettings.skybox.HasProperty("_Exposure"))
        {
            if (!expCached) { baseSkyExposure = RenderSettings.skybox.GetFloat("_Exposure"); expCached = true; }
            // HDRI with baked sun: darken to near-black at night so no daytime sun shows
            RenderSettings.skybox.SetFloat("_Exposure", baseSkyExposure * Mathf.Lerp(0.015f, 1f, Daylight01));
        }
        Changed?.Invoke();
    }

    void OnDisable()
    {
        if (expCached && RenderSettings.skybox && RenderSettings.skybox.HasProperty("_Exposure")) RenderSettings.skybox.SetFloat("_Exposure", baseSkyExposure);
        if (Instance == this) Instance = null;
    }
}
