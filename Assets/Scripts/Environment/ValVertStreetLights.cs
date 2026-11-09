// Street lamps: emission on lamp lenses + a bounded pool of real point lights near the camera.
// Lamp positions are recovered at runtime from the existing lamp lens / head meshes (no new layout invented).
using UnityEngine;
using System.Collections.Generic;

[ExecuteAlways]
public class ValVertStreetLights : MonoBehaviour
{
    public string[] lensMaterialNames = { "lamp_lens", "lamp_head", "car_lamp" };
    public Color lampColor = new Color(1f, .78f, .48f);   // 2010 high-pressure sodium / warm
    public float emission = 6f, lightIntensity = 70f, lightRange = 20f;
    public int maxRealLights = 24;
    public Transform viewer;   // defaults to Camera.main                        // pooled, nearest to camera, no shadows
    [Tooltip("Sun altitude (deg) below which lamps switch on (dusk) / off (dawn).")]
    public float switchAltitude = -1.5f;

    readonly List<Vector3> lamps = new List<Vector3>();
    readonly List<Material> lensMats = new List<Material>();
    readonly List<Light> pool = new List<Light>();
    float on01;

    void OnEnable() { Collect(); }

    public void Collect()
    {
        lamps.Clear(); lensMats.Clear();
        var cells = new Dictionary<Vector3Int, (Vector3 sum, int n)>();
        foreach (var r in FindObjectsByType<MeshRenderer>(FindObjectsSortMode.None))
        {
            bool hit = false;
            foreach (var m in r.sharedMaterials) if (m && System.Array.Exists(lensMaterialNames, s => m.name.StartsWith(s) && s != "car_lamp")) { hit = true; if (!lensMats.Contains(m)) lensMats.Add(m); }
            if (!hit) continue;
            var mf = r.GetComponent<MeshFilter>(); if (!mf || !mf.sharedMesh || !mf.sharedMesh.isReadable) continue;
            var mesh = mf.sharedMesh; var vs = mesh.vertices;
            // submesh(es) using the lamp material only
            for (int sm = 0; sm < mesh.subMeshCount && sm < r.sharedMaterials.Length; sm++)
            {
                var m = r.sharedMaterials[sm]; if (!m || !lensMats.Contains(m)) continue;
                foreach (int i in mesh.GetTriangles(sm))
                {
                    var w = r.transform.TransformPoint(vs[i]); var k = new Vector3Int(Mathf.FloorToInt(w.x / 1.5f), Mathf.FloorToInt(w.y / 3f), Mathf.FloorToInt(w.z / 1.5f));
                    cells.TryGetValue(k, out var c); cells[k] = (c.sum + w, c.n + 1);
                }
            }
        }
        foreach (var c in cells.Values) if (c.n > 0) lamps.Add(c.sum / c.n);
        // merge cells belonging to the same lamp head
        for (int i = 0; i < lamps.Count; i++) for (int j = lamps.Count - 1; j > i; j--) if ((lamps[i] - lamps[j]).sqrMagnitude < 2.5f) { lamps[i] = (lamps[i] + lamps[j]) * .5f; lamps.RemoveAt(j); }
        Debug.Log("[VV] street lamps found " + lamps.Count + " (lens materials " + lensMats.Count + ")");
    }

    void Update() { Tick(); }
    public void Tick()
    {
        var clk = ValVertClock.Instance; if (!clk) return;
        float target = clk.SunAltitudeDeg < switchAltitude ? 1f : 0f;
        on01 = Application.isPlaying ? Mathf.MoveTowards(on01, target, Time.deltaTime * 0.5f) : target;  // ~2 s ramp (sodium warm-up)
        foreach (var m in lensMats)
        {
            if (on01 > 0.01f) { m.EnableKeyword("_EMISSION"); m.SetColor("_EmissionColor", lampColor * emission * on01); }
            else { m.SetColor("_EmissionColor", Color.black); }
        }
        UpdatePool();
    }

    void UpdatePool()
    {
        while (pool.Count < maxRealLights)
        {
            var g = new GameObject("vv_lamp_light"); g.hideFlags = HideFlags.DontSave; g.transform.SetParent(transform, false);
            var l = g.AddComponent<Light>(); l.type = LightType.Point; l.shadows = LightShadows.None; l.color = lampColor; l.range = lightRange; l.renderMode = LightRenderMode.ForcePixel;
            pool.Add(l);
        }
        var cam = viewer ? viewer.position : (Camera.main ? Camera.main.transform.position : Vector3.zero);
        if (on01 <= 0.01f || lamps.Count == 0) { foreach (var l in pool) l.enabled = false; return; }
        lamps.Sort((a, b) => (a - cam).sqrMagnitude.CompareTo((b - cam).sqrMagnitude));
        for (int i = 0; i < pool.Count; i++)
        {
            bool use = i < lamps.Count; pool[i].enabled = use;
            if (use) { pool[i].transform.position = lamps[i] + Vector3.down * 0.4f; pool[i].intensity = lightIntensity * on01; }
        }
    }

    void OnDisable()
    {
        foreach (var l in pool) if (l) DestroyImmediate(l.gameObject); pool.Clear();
        foreach (var m in lensMats) if (m) m.SetColor("_EmissionColor", Color.black);
    }
}
