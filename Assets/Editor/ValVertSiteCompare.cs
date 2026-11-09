// Autumn midday photo compares. Fail hard on magenta/cyan error pixels and bad shaders.
using UnityEngine; using UnityEditor; using UnityEngine.Rendering; using System.IO;
public static class ValVertSiteCompare {
    const string OUT="/workspace/valvert/verify/";
    static Camera cam; static ValVertClock clk; static ValVertSeasons sea; static ValVertStreetLights sl; static ValVertWeather w;
    static void Place(Vector3 p, Vector3 dir, float fov=60f, float eye=1.65f){
        Terrain terr=Terrain.activeTerrain;
        foreach(var tt in Terrain.activeTerrains){ var tp=tt.transform.position; var sz=tt.terrainData.size;
            if(p.x>=tp.x&&p.x<=tp.x+sz.x&&p.z>=tp.z&&p.z<=tp.z+sz.z){ terr=tt; break; } }
        cam.orthographic=false; cam.fieldOfView=fov; cam.farClipPlane=2000; cam.nearClipPlane=0.2f;
        // Prefer terrain height so we don't sit on tree/rock colliders that bury the lens
        float gy=terr.SampleHeight(p)+terr.transform.position.y;
        cam.transform.position=new Vector3(p.x, gy+eye, p.z);
        cam.transform.rotation=Quaternion.LookRotation(dir.normalized);
        // If looking into a near wall (<2.5m), back the camera along -forward
        if(Physics.Raycast(cam.transform.position, cam.transform.forward, out var hit, 2.5f)){
            cam.transform.position -= cam.transform.forward * Mathf.Min(2.0f, 2.6f-hit.distance);
            cam.transform.position=new Vector3(cam.transform.position.x, gy+eye, cam.transform.position.z);
        }
    }
    static void PlaceOrtho(Vector3 p, float size){
        Terrain terr=Terrain.activeTerrain;
        foreach(var tt in Terrain.activeTerrains){ var tp=tt.transform.position; var sz=tt.terrainData.size;
            if(p.x>=tp.x&&p.x<=tp.x+sz.x&&p.z>=tp.z&&p.z<=tp.z+sz.z){ terr=tt; break; } }
        cam.orthographic=true; cam.orthographicSize=size; cam.farClipPlane=500; cam.nearClipPlane=0.1f;
        float y=terr.SampleHeight(p)+terr.transform.position.y+90f;
        cam.transform.position=new Vector3(p.x,y,p.z); cam.transform.rotation=Quaternion.Euler(90,0,0);
    }
    static void GuardMaterials(string tag){
        int bad=0;
        foreach(var r in Object.FindObjectsByType<Renderer>(FindObjectsInactive.Include,FindObjectsSortMode.None)){
            foreach(var m in r.sharedMaterials){
                if(m==null||m.shader==null||m.shader.name.Contains("InternalError")){
                    bad++; Debug.LogError($"[VV] SITECMP BAD MAT {tag} on {r.name}: {(m?m.name:"null")} shader={(m&&m.shader?m.shader.name:"null")}");
                }
            }
        }
        if(bad>0){ Debug.LogError($"[VV] SITECMP MATERIAL GUARD FAIL {tag} bad={bad}"); EditorApplication.Exit(4); }
    }
    static bool PixelError(Texture2D tx, out float magPct, out float cyanPct){
        var px=tx.GetPixels32(); int mag=0, cyan=0;
        for(int i=0;i<px.Length;i++){
            var c=px[i];
            if(c.r>200 && c.g<90 && c.b>200) mag++;
            if(c.r<90 && c.g>180 && c.b>180 && c.g+c.b>c.r*2+40) cyan++;
        }
        magPct=100f*mag/px.Length; cyanPct=100f*cyan/px.Length;
        return magPct>0.15f || cyanPct>0.15f;
    }
    static void Shot(string path){
        w.Force(ValVertWeather.Kind.Overcast); w.ClearSnowVisuals();
        // Nov 1 autumn midday — overcast to match photos
        clk.Set(11,1,13f);
        w.Tick(); w.ClearSnowVisuals(); sea.Tick(); clk.Apply(); sl.Tick();
        w.ClearSnowVisuals(); sea.snowCover=0f; sea.Tick();
        ValVertWoods.HealFoliage(); ValVertWoods.HealTreePrototypes();
        // Overcast ambient — cars under overhangs must not silhouette
        RenderSettings.ambientMode=UnityEngine.Rendering.AmbientMode.Flat;
        RenderSettings.ambientLight=new Color(0.72f,0.74f,0.76f);
        if(RenderSettings.sun){ RenderSettings.sun.intensity=0.55f; RenderSettings.sun.color=new Color(0.82f,0.84f,0.86f); }
        RenderSettings.reflectionIntensity=0.4f;
        // Prefer overcast skybox if present
        var osky=UnityEditor.AssetDatabase.LoadAssetAtPath<Material>("Assets/ValVert/Mats/sky_overcast.mat");
        if(osky) RenderSettings.skybox=osky;
        int W=1280,H=720;
        var rt=new RenderTexture(W,H,24); cam.targetTexture=rt; cam.Render();
        RenderTexture.active=rt; var tx=new Texture2D(W,H,TextureFormat.RGB24,false);
        tx.ReadPixels(new Rect(0,0,W,H),0,0); tx.Apply();
        if(PixelError(tx, out var mp, out var cp)){
            File.WriteAllBytes(path+".bad.png", tx.EncodeToPNG());
            Debug.LogError($"[VV] SITECMP PIXEL GUARD FAIL {path} magenta={mp:F2}% cyan={cp:F2}%");
            RenderTexture.active=null; cam.targetTexture=null; Object.DestroyImmediate(rt); Object.DestroyImmediate(tx);
            EditorApplication.Exit(5); return;
        }
        File.WriteAllBytes(path,tx.EncodeToPNG());
        RenderTexture.active=null; cam.targetTexture=null; Object.DestroyImmediate(rt); Object.DestroyImmediate(tx);
        Debug.Log($"[VV] SITECMP wrote {path} litter={sea.Litter:F2} season={sea.SeasonName} pos={cam.transform.position}");
    }
    public static void Run(){
        UnityEditor.SceneManagement.EditorSceneManager.OpenScene("Assets/Scenes/ValVert.unity");
        ValVertDress.RepairTreePrototypes();
        var go=ValVertEnvWire.Wire(); clk=go.GetComponent<ValVertClock>(); sea=go.GetComponent<ValVertSeasons>();
        sl=go.GetComponent<ValVertStreetLights>(); w=go.GetComponent<ValVertWeather>();
        foreach(var n in new[]{"V3_StairApproach","V3_LogStairsDress","V3_Ramp9to13","V3_RearPark91113","V3_Batch2Dress","V3_Entrances"}){
            var o=GameObject.Find(n); if(o) Object.DestroyImmediate(o); }
        if(ValVertWoods.MATS==null) ValVertWoods.MATS=new System.Collections.Generic.Dictionary<string,Material>();
        var rock=AssetDatabase.LoadAssetAtPath<Material>("Assets/ValVert/Mats/rock.mat");
        if(rock!=null){ ValVertWoods.EnsureUrp(rock); ValVertWoods.MATS["rock"]=rock; }
        ValVertWoods.StairApproach();
        int fixedN=ValVertWoods.FixBrokenMaterials();
        int heal=ValVertWoods.HealFoliage();
        int proto=ValVertWoods.HealTreePrototypes();
        foreach(var t in Terrain.activeTerrains){ if(t){ t.drawTreesAndFoliage=true; if(t.terrainData!=null) t.terrainData.RefreshPrototypes(); } }
        Debug.Log($"[VV] SITECMP material fixes={fixedN} foliageHeal={heal} protoHeal={proto} terrainTrees=on");
        GuardMaterials("post-dress");
        cam=new GameObject("sitecmp_cam").AddComponent<Camera>();
        sl.viewer=cam.transform; w.viewer=cam.transform; w.BuildShells();
        Directory.CreateDirectory(OUT);

        // stairs looking DOWN the sleeper flight toward the block
        Place(new Vector3(354.5f,0,136.5f), new Vector3(0.72f,-0.35f,-0.55f), 60f, 1.55f);
        Shot(OUT+"cmp_stairs_top_down_render.png");
        // path to carpark — along park edge toward parked cars / junction
        Place(new Vector3(354.0f,0,133.5f), new Vector3(0.85f,-0.05f,-0.50f), 55f, 1.65f);
        Shot(OUT+"cmp_path_to_carpark_render.png");
        // woods path + stump + dual rail fence
        Place(new Vector3(350.5f,0,132.0f), new Vector3(0.25f,0.05f,0.96f), 55f, 1.55f);
        Shot(OUT+"cmp_woods_path_fence_render.png");

        Place(new Vector3(368.0f,0,124.0f), new Vector3(0.10f,-0.20f,0.97f), 55f, 8.2f);
        Shot(OUT+"cmp_ramp_from_n9_window.png");
        // ramp ground — along ramp toward rue
        Place(new Vector3(364.5f,0,142.0f), new Vector3(0.20f,0.08f,0.97f), 58f, 1.65f);
        Shot(OUT+"cmp_ramp_ground.png");
        PlaceOrtho(new Vector3(378f,0,185f), 42f);
        Shot(OUT+"cmp_rear_park_topdown.png");

        // 01 entrance — close to door, Clear of trunks (shot clear near 361,122)
        Place(new Vector3(363.8f,0,122.4f), new Vector3(1f,0.0f,0.0f), 60f, 1.55f);
        Shot(OUT+"cmp_batch2_01_entrance_render.png");
        // 02 stair bottom — looking UP sleeper stairs, building on right
        Place(new Vector3(364.0f,0,128.5f), new Vector3(-0.05f,0.12f,0.99f), 55f, 1.60f);
        Shot(OUT+"cmp_batch2_02_stair_bottom_render.png");
        // 03 footpath — building RIGHT, embankment LEFT, looking north
        Place(new Vector3(365.6f,0,126.0f), new Vector3(0.02f,0.02f,0.99f), 55f, 1.55f);
        Shot(OUT+"cmp_batch2_03_footpath_render.png");
        // 04 rear park — mid OSM aisle looking south to apartments
        Place(new Vector3(379.0f,0,188.0f), new Vector3(0.05f,0.02f,-0.99f), 62f, 1.75f);
        Shot(OUT+"cmp_batch2_04_rear_park_render.png");
        // 05 junction — FIN DE ZONE / zebra / hedge / mountain view
        Place(new Vector3(352.0f,0,176.5f), new Vector3(0.55f,0.05f,0.83f), 55f, 1.60f);
        Shot(OUT+"cmp_batch2_05_junction_render.png");
        // 06 sidewalk — hedge left, road ahead, building+cars right
        Place(new Vector3(348.5f,0,176.0f), new Vector3(0.98f,0.02f,0.15f), 52f, 1.60f);
        Shot(OUT+"cmp_batch2_06_sidewalk_render.png");
        // 07 crossing — zebra foreground, looking up Val Vert
        Place(new Vector3(362.5f,0,174.5f), new Vector3(0.25f,0.05f,0.96f), 55f, 1.55f);
        Shot(OUT+"cmp_batch2_07_crossing_render.png");
        // 08 flat ramp — on asphalt west of nº9, looking north (building left)
        Place(new Vector3(360.0f,0,125.5f), new Vector3(0.18f,0.02f,0.98f), 58f, 1.70f);
        Shot(OUT+"cmp_batch2_08_flat_ramp_render.png");
        // 09 earth path through woods (clear of trunks)
        Place(new Vector3(353.5f,0,145.2f), new Vector3(-0.35f,0.02f,0.93f), 55f, 1.60f);
        Shot(OUT+"cmp_batch2_09_earth_path_render.png");
        // 10 park edge — asphalt curb looking into park (boulders + path)
        Place(new Vector3(359.5f,0,120.5f), new Vector3(-0.55f,0.05f,0.83f), 60f, 1.65f);
        Shot(OUT+"cmp_batch2_10_park_edge_render.png");

        // nº11 / nº13 entrance verification (same prefab, correct plaques)
        Place(new Vector3(363.8f,0,138.70f), new Vector3(1f,0.0f,0.0f), 60f, 1.55f);
        Shot(OUT+"cmp_entrance_n11_render.png");
        Place(new Vector3(363.8f,0,156.00f), new Vector3(1f,0.0f,0.0f), 60f, 1.55f);
        Shot(OUT+"cmp_entrance_n13_render.png");

        w.ClearSnowVisuals();
        Debug.Log("[VV] SITECMP DONE");
        EditorApplication.Exit(0);
    }
}
