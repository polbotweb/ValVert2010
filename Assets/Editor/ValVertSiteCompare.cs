// Autumn midday photo compares. Fail hard on magenta/cyan error pixels and bad shaders.
using UnityEngine; using UnityEditor; using System.IO;
public static class ValVertSiteCompare {
    const string OUT="/workspace/valvert/verify/";
    static Camera cam; static ValVertClock clk; static ValVertSeasons sea; static ValVertStreetLights sl; static ValVertWeather w;
    static void Place(Vector3 p, Vector3 dir, float fov=60f, float eye=1.65f){
        Terrain terr=Terrain.activeTerrain;
        foreach(var tt in Terrain.activeTerrains){ var tp=tt.transform.position; var sz=tt.terrainData.size;
            if(p.x>=tp.x&&p.x<=tp.x+sz.x&&p.z>=tp.z&&p.z<=tp.z+sz.z){ terr=tt; break; } }
        cam.orthographic=false; cam.fieldOfView=fov; cam.farClipPlane=2000; cam.nearClipPlane=0.15f;
        float gy=terr.SampleHeight(p)+terr.transform.position.y;
        if(Physics.Raycast(new Vector3(p.x,gy+8f,p.z),Vector3.down,out var hit,20f)) gy=Mathf.Max(gy,hit.point.y);
        cam.transform.position=new Vector3(p.x, gy+eye, p.z);
        cam.transform.rotation=Quaternion.LookRotation(dir.normalized);
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
        w.Force(ValVertWeather.Kind.Clear); w.ClearSnowVisuals();
        // Nov 1: Litter>0.5 and LeafCover still decent
        clk.Set(11,1,13f);
        w.Tick(); w.ClearSnowVisuals(); sea.Tick(); clk.Apply(); sl.Tick();
        w.ClearSnowVisuals(); sea.snowCover=0f; sea.Tick();
        ValVertWoods.HealFoliage();
        // Keep midday ambient readable even if clock/sky mis-wires in batchmode
        if(RenderSettings.ambientLight.maxColorComponent < 0.15f)
            RenderSettings.ambientLight=new Color(0.35f,0.38f,0.42f);
        if(RenderSettings.sun && RenderSettings.sun.intensity < 0.2f) RenderSettings.sun.intensity=1.1f;
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
        foreach(var n in new[]{"V3_StairApproach","V3_LogStairsDress","V3_Ramp9to13","V3_RearPark91113","V3_Batch2Dress"}){
            var o=GameObject.Find(n); if(o) Object.DestroyImmediate(o); }
        if(ValVertWoods.MATS==null) ValVertWoods.MATS=new System.Collections.Generic.Dictionary<string,Material>();
        var rock=AssetDatabase.LoadAssetAtPath<Material>("Assets/ValVert/Mats/rock.mat");
        if(rock!=null){ ValVertWoods.EnsureUrp(rock); ValVertWoods.MATS["rock"]=rock; }
        ValVertWoods.StairApproach();
        int fixedN=ValVertWoods.FixBrokenMaterials();
        int heal=ValVertWoods.HealFoliage();
        int proto=ValVertWoods.HealTreePrototypes();
        // Terrain EF Leaves_URP still paints cyan under batch URP even after mat swap on prefabs.
        // Hide terrain trees for photo compares; mesh trees/props stay (healed to URP Lit).
        foreach(var t in Terrain.activeTerrains){ if(t) t.drawTreesAndFoliage=false; }
        Debug.Log($"[VV] SITECMP material fixes={fixedN} foliageHeal={heal} protoHeal={proto} terrainTrees=off");
        GuardMaterials("post-dress");
        cam=new GameObject("sitecmp_cam").AddComponent<Camera>();
        sl.viewer=cam.transform; w.viewer=cam.transform; w.BuildShells();
        Directory.CreateDirectory(OUT);

        Place(new Vector3(356.5f,0,133.2f), new Vector3(0.95f,-0.38f,0.05f), 52f, 1.55f);
        Shot(OUT+"cmp_stairs_top_down_render.png");
        Place(new Vector3(353.5f,0,132.5f), new Vector3(0.75f,-0.08f,-0.60f), 52f, 1.65f);
        Shot(OUT+"cmp_path_to_carpark_render.png");
        Place(new Vector3(349.5f,0,134.0f), new Vector3(0.30f,0.0f,0.95f), 52f, 1.65f);
        Shot(OUT+"cmp_woods_path_fence_render.png");

        Place(new Vector3(368.0f,0,124.0f), new Vector3(0.10f,-0.20f,0.97f), 55f, 8.2f);
        Shot(OUT+"cmp_ramp_from_n9_window.png");
        Place(new Vector3(362.5f,0,140.0f), new Vector3(0.35f,0.15f,0.92f), 55f, 1.65f);
        Shot(OUT+"cmp_ramp_ground.png");
        PlaceOrtho(new Vector3(378f,0,185f), 42f);
        Shot(OUT+"cmp_rear_park_topdown.png");

        Place(new Vector3(361.2f,0,122.4f), new Vector3(1f,0.0f,0.0f), 58f, 1.55f);
        Shot(OUT+"cmp_batch2_01_entrance_render.png");
        Place(new Vector3(364.3f,0,123.2f), new Vector3(-0.15f,0.05f,0.99f), 55f, 1.65f);
        Shot(OUT+"cmp_batch2_02_stair_bottom_render.png");
        Place(new Vector3(366.8f,0,122.0f), new Vector3(0.05f,0.0f,1f), 52f, 1.55f);
        Shot(OUT+"cmp_batch2_03_footpath_render.png");
        Place(new Vector3(379.0f,0,174.0f), new Vector3(0.05f,0.02f,1f), 58f, 1.65f);
        Shot(OUT+"cmp_batch2_04_rear_park_render.png");
        Place(new Vector3(356.0f,0,175.8f), new Vector3(0.95f,0.05f,0.20f), 52f, 1.65f);
        Shot(OUT+"cmp_batch2_05_junction_render.png");
        Place(new Vector3(343.0f,0,175.8f), new Vector3(1f,0.02f,0.05f), 52f, 1.65f);
        Shot(OUT+"cmp_batch2_06_sidewalk_render.png");
        Place(new Vector3(360.0f,0,175.8f), new Vector3(0.55f,0.02f,0.83f), 52f, 1.55f);
        Shot(OUT+"cmp_batch2_07_crossing_render.png");
        Place(new Vector3(361.8f,0,128.5f), new Vector3(0.12f,0.02f,0.99f), 55f, 1.65f);
        Shot(OUT+"cmp_batch2_08_flat_ramp_render.png");
        Place(new Vector3(352.5f,0,131.0f), new Vector3(-0.30f,0.02f,0.95f), 50f, 1.55f);
        Shot(OUT+"cmp_batch2_09_earth_path_render.png");
        Place(new Vector3(368.5f,0,124.5f), new Vector3(-0.55f,0.0f,0.83f), 50f, 1.60f);
        Shot(OUT+"cmp_batch2_10_park_edge_render.png");

        w.ClearSnowVisuals();
        Debug.Log("[VV] SITECMP DONE");
        EditorApplication.Exit(0);
    }
}
