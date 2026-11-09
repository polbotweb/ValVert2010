// Val Vert — field grass generator (architecture: World/Vegetation).
// Placement/batching/exclusions extracted from ValVertWoods.Grass.
// Config SOURCE: Assets/_Project/Settings/World/Vegetation/grass_field_config.json
// Pack texture filenames / atlas UV layout are NEVER invented here.
using UnityEngine;
using UnityEditor;
using UnityEngine.Rendering;
using System.Collections.Generic;
using System.Globalization;
using System.IO;

public static class ValVertGrassField
{
    const string GEN = "Assets/ValVert/Gen/";
    public const string ConfigPath = "Assets/_Project/Settings/World/Vegetation/grass_field_config.json";
    public const string ExpectedPackId = "138810";
    public const string ExpectedSchema = "1";

    [System.Serializable]
    public class PackSlot
    {
        public string assetStoreId;
        public string assetStoreUrl;
        public string status;
        public string localRoot;
        public string kindExpected;
        public string albedoRelative;
        public string note;
    }

    [System.Serializable]
    public class GrassConfig
    {
        public string schemaVersion;
        public string materialMode;
        public string legacyAlbedoPath;
        public string rootName;
        public string meshAssetPrefix;
        public string materialName;
        public int seed;
        public float chunkSize;
        public float parkSpacing;
        public float woodsSpacing;
        public float exclThreshold;
        public float canopyThresh;
        public float meadowThresh;
        public PackSlot pack;
        public string[] exclusionMasks; // documentary — Build uses same hard-wired masks as historical Grass
        public string exclusionMasksNote;
        public string notes;
    }

    static void L(string s) { Debug.Log("[VV][GrassField] " + s); }

    public static GrassConfig LoadConfig()
    {
        if (!File.Exists(ConfigPath))
            throw new System.InvalidOperationException("grass config missing: " + ConfigPath);
        return JsonUtility.FromJson<GrassConfig>(File.ReadAllText(ConfigPath));
    }

    static bool FinitePositive(float v) => !float.IsNaN(v) && !float.IsInfinity(v) && v > 0f;

    /// <summary>Non-mutating preflight.</summary>
    public static List<string> Preflight(GrassConfig cfg = null)
    {
        cfg = cfg ?? LoadConfig();
        var errs = new List<string>();
        if (cfg == null) { errs.Add("missing config object"); return errs; }
        if (cfg.schemaVersion != ExpectedSchema)
            errs.Add($"schemaVersion '{cfg.schemaVersion}' != expected '{ExpectedSchema}'");
        if (!FinitePositive(cfg.chunkSize)) errs.Add("chunkSize must be finite > 0");
        if (!FinitePositive(cfg.parkSpacing)) errs.Add("parkSpacing must be finite > 0");
        if (!FinitePositive(cfg.woodsSpacing)) errs.Add("woodsSpacing must be finite > 0");
        if (string.IsNullOrEmpty(cfg.rootName)) errs.Add("rootName unset");
        if (string.IsNullOrEmpty(cfg.meshAssetPrefix)) errs.Add("meshAssetPrefix unset");

        if (cfg.pack == null) errs.Add("pack slot missing");
        else if (cfg.pack.assetStoreId != ExpectedPackId)
            errs.Add($"pack.assetStoreId '{cfg.pack.assetStoreId}' != {ExpectedPackId}");

        if (cfg.materialMode == "legacy_homemade")
        {
            if (string.IsNullOrEmpty(cfg.legacyAlbedoPath) || !File.Exists(cfg.legacyAlbedoPath))
                errs.Add("legacy albedo missing: " + cfg.legacyAlbedoPath);
        }
        else if (cfg.materialMode == "pack_cutout")
        {
            if (cfg.pack == null) { /* already flagged */ }
            else
            {
                if (string.IsNullOrEmpty(cfg.pack.localRoot))
                    errs.Add("pack_cutout requires pack.localRoot");
                else if (!Directory.Exists(cfg.pack.localRoot))
                    errs.Add("pack localRoot absent: " + cfg.pack.localRoot);
                if (string.IsNullOrEmpty(cfg.pack.albedoRelative))
                    errs.Add("pack albedoRelative not set — refuse to invent texture filenames; inspect genuine package first");
                else if (!string.IsNullOrEmpty(cfg.pack.localRoot))
                {
                    var full = Path.Combine(cfg.pack.localRoot, cfg.pack.albedoRelative).Replace('\\', '/');
                    if (!File.Exists(full)) errs.Add("pack albedo file missing: " + full);
                }
            }
        }
        else errs.Add("unknown materialMode: " + cfg.materialMode);
        return errs;
    }

    static bool Built(float x, float z)
    {
        for (int k = 0; k < 5; k++)
        {
            var o = k == 0 ? Vector3.zero : Quaternion.Euler(0, k * 90, 0) * Vector3.forward * 0.6f;
            if (Physics.Raycast(new Vector3(x + o.x, 400f, z + o.z), Vector3.down, out var h, 800f) && !(h.collider is TerrainCollider))
                return true;
        }
        return false;
    }

    static Texture2D ResolveAlbedo(GrassConfig cfg)
    {
        string path;
        if (cfg.materialMode == "pack_cutout")
            path = Path.Combine(cfg.pack.localRoot, cfg.pack.albedoRelative).Replace('\\', '/');
        else
            path = cfg.legacyAlbedoPath;
        var ti = AssetImporter.GetAtPath(path) as TextureImporter;
        if (ti != null)
        {
            ti.alphaIsTransparency = true;
            ti.mipMapsPreserveCoverage = true;
            ti.alphaTestReferenceValue = 0.5f;
            ti.wrapMode = TextureWrapMode.Clamp;
            ti.SaveAndReimport();
        }
        return AssetDatabase.LoadAssetAtPath<Texture2D>(path);
    }

    /// <summary>
    /// Idempotent-ish: destroys existing rootName in open scene, reuses mesh assets at stable paths
    /// (CreateAsset only when missing; otherwise overwrite contents to preserve .meta GUIDs).
    /// Not claimed byte-proven across machines.
    /// </summary>
    public static void Build(Terrain terr)
    {
        var cfg = LoadConfig();
        var errs = Preflight(cfg);
        if (errs.Count > 0)
            throw new System.InvalidOperationException("ValVertGrassField preflight failed:\n  " + string.Join("\n  ", errs));

        // Remove prior roots to avoid duplicates on rerun
        foreach (var existing in Object.FindObjectsByType<Transform>(FindObjectsSortMode.None))
        {
            if (existing && existing.name == cfg.rootName && existing.parent == null)
                Object.DestroyImmediate(existing.gameObject);
        }

        Physics.SyncTransforms();
        var albedo = ResolveAlbedo(cfg);
        if (albedo == null) throw new System.InvalidOperationException("albedo texture failed to load");
        var gm = ValVertDress.Mat(cfg.materialName, albedo, null, true, new Color(.95f, .95f, .85f), 0.05f);
        gm.SetFloat("_Cutoff", 0.5f);

        var ex = ValVertDress.Mask("veg_excl.png");
        var fm = ValVertDress.Mask("canopy_orig.png");
        var me = ValVertDress.Mask("veg_meadow.png");
        var rnd = new System.Random(cfg.seed);
        System.Func<float> R = () => (float)rnd.NextDouble();
        float CH = cfg.chunkSize;
        var root = new GameObject(cfg.rootName);
        long total = 0; int chunks = 0;
        float tp = terr.transform.position.y;
        for (float cx = -532; cx < 532; cx += CH)
        for (float cz = -354.6f; cz < 354.6f; cz += CH)
        {
            bool park = cx > -380 && cx < 500 && cz > -340 && cz < 140;
            float sp = park ? cfg.parkSpacing : cfg.woodsSpacing;
            var V = new List<Vector3>(); var N = new List<Vector3>(); var U = new List<Vector2>(); var C = new List<Color>(); var T = new List<int>();
            for (float x = cx; x < cx + CH; x += sp)
            for (float z = cz; z < cz + CH; z += sp)
            {
                float px = x + R() * sp, pz = z + R() * sp;
                if (px > 531 || pz > 354) continue;
                // Removed dead: ShotClear && magnitude<0 (always false)
                if (ValVertDress.S(ex, px, pz) > cfg.exclThreshold || ValVertWoods.Lab(px, pz) != 0 || ValVertWoods.Hidden(new Vector3(px, 0, pz))) continue;
                if (Built(px, pz)) continue;
                float f = ValVertDress.S(fm, px, pz), m = ValVertDress.S(me, px, pz);
                float p = f > cfg.canopyThresh ? 0.45f : 1f;
                if (R() > p) continue;
                float hgt = (m > cfg.meadowThresh ? 0.32f : 0.22f) + R() * 0.2f;
                float w = 0.4f + R() * 0.3f;
                float y = terr.SampleHeight(new Vector3(px, 0, pz)) + tp - 0.04f;
                var tint = Color.Lerp(new Color(.85f, .95f, .75f), new Color(1.05f, .95f, .7f), R());
                if (f > cfg.canopyThresh) tint *= 0.85f;
                tint.a = 1;
                float a0 = R() * Mathf.PI;
                // Pack UV atlas pending genuine inspect — keep legacy 2-column homemade layout for now.
                float u0 = Mathf.Floor(R() * 2) * 0.5f;
                float uW = 0.5f;
                for (int q = 0; q < 3; q++)
                {
                    float a = a0 + q * Mathf.PI / 3;
                    var d = new Vector3(Mathf.Cos(a), 0, Mathf.Sin(a)) * w * 0.5f;
                    var b = new Vector3(px, y, pz);
                    int k = V.Count;
                    V.Add(b - d); V.Add(b - d + Vector3.up * hgt); V.Add(b + d + Vector3.up * hgt); V.Add(b + d);
                    for (int r = 0; r < 4; r++) { N.Add(Vector3.up); C.Add(tint); }
                    U.Add(new Vector2(u0, 0)); U.Add(new Vector2(u0, 1)); U.Add(new Vector2(u0 + uW, 1)); U.Add(new Vector2(u0 + uW, 0));
                    T.AddRange(new[] { k, k + 1, k + 2, k, k + 2, k + 3 });
                }
            }
            if (V.Count == 0) continue;
            var assetPath = GEN + cfg.meshAssetPrefix + chunks + ".asset";
            Mesh mesh = AssetDatabase.LoadAssetAtPath<Mesh>(assetPath);
            bool created = false;
            if (mesh == null)
            {
                mesh = new Mesh { indexFormat = IndexFormat.UInt32 };
                created = true;
            }
            else
            {
                mesh.Clear();
                mesh.indexFormat = IndexFormat.UInt32;
            }
            mesh.SetVertices(V); mesh.SetNormals(N); mesh.SetUVs(0, U); mesh.SetColors(C); mesh.SetTriangles(T, 0); mesh.RecalculateBounds();
            if (created) AssetDatabase.CreateAsset(mesh, assetPath);
            else EditorUtility.SetDirty(mesh);

            var go = new GameObject(cfg.meshAssetPrefix + chunks);
            go.transform.SetParent(root.transform, false);
            go.AddComponent<MeshFilter>().sharedMesh = mesh;
            var mr = go.AddComponent<MeshRenderer>();
            mr.sharedMaterial = gm;
            mr.shadowCastingMode = ShadowCastingMode.Off;
            var lg = go.AddComponent<LODGroup>();
            lg.SetLODs(new[] { new LOD(0.06f, new Renderer[] { mr }) });
            lg.RecalculateBounds();
            total += V.Count / 12;
            chunks++;
        }
        AssetDatabase.SaveAssets();
        ValVertDress.Counts["_grass_clumps"] = (int)total;
        L($"mode={cfg.materialMode} clumps={total} chunks={chunks}");
    }
}
