// Combined autumn midday compares vs all user photo refs (never commit the photos).
using UnityEngine; using UnityEditor; using System.IO;
public static class ValVertSiteCompare {
    const string OUT="/workspace/valvert/verify/";
    static Camera cam; static ValVertClock clk; static ValVertSeasons sea; static ValVertStreetLights sl; static ValVertWeather w;
    static void Place(Vector3 p, Vector3 dir, float fov=65f, float eye=1.65f){
        Terrain terr=Terrain.activeTerrain;
        foreach(var tt in Terrain.activeTerrains){ var tp=tt.transform.position; var sz=tt.terrainData.size;
            if(p.x>=tp.x&&p.x<=tp.x+sz.x&&p.z>=tp.z&&p.z<=tp.z+sz.z){ terr=tt; break; } }
        cam.orthographic=false; cam.fieldOfView=fov; cam.farClipPlane=2000; cam.nearClipPlane=0.08f;
        p.y=terr.SampleHeight(p)+terr.transform.position.y+eye;
        cam.transform.position=p; cam.transform.rotation=Quaternion.LookRotation(dir);
    }
    static void PlaceOrtho(Vector3 p, float size){
        Terrain terr=Terrain.activeTerrain;
        foreach(var tt in Terrain.activeTerrains){ var tp=tt.transform.position; var sz=tt.terrainData.size;
            if(p.x>=tp.x&&p.x<=tp.x+sz.x&&p.z>=tp.z&&p.z<=tp.z+sz.z){ terr=tt; break; } }
        cam.orthographic=true; cam.orthographicSize=size; cam.farClipPlane=500; cam.nearClipPlane=0.1f;
        float y=terr.SampleHeight(p)+terr.transform.position.y+80f;
        cam.transform.position=new Vector3(p.x,y,p.z); cam.transform.rotation=Quaternion.Euler(90,0,0);
    }
    static void Shot(string path){
        w.Force(ValVertWeather.Kind.Clear); w.ClearSnowVisuals();
        clk.Set(11,9,13f); w.Tick(); w.ClearSnowVisuals(); sea.Tick(); clk.Apply(); sl.Tick();
        w.ClearSnowVisuals(); sea.snowCover=0f; sea.Tick();
        int W=1280,H=720;
        var rt=new RenderTexture(W,H,24); cam.targetTexture=rt; cam.Render();
        RenderTexture.active=rt; var tx=new Texture2D(W,H,TextureFormat.RGB24,false);
        tx.ReadPixels(new Rect(0,0,W,H),0,0); File.WriteAllBytes(path,tx.EncodeToPNG());
        RenderTexture.active=null; cam.targetTexture=null; Object.DestroyImmediate(rt); Object.DestroyImmediate(tx);
        Debug.Log($"[VV] SITECMP wrote {path} litter={sea.Litter:F2} season={sea.SeasonName}");
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
        if(rock!=null) ValVertWoods.MATS["rock"]=rock;
        ValVertWoods.StairApproach();
        cam=new GameObject("sitecmp_cam").AddComponent<Camera>();
        sl.viewer=cam.transform; w.viewer=cam.transform; w.BuildShells();
        Directory.CreateDirectory(OUT);

        // Batch1
        Place(new Vector3(355.6f,0,133.05f), new Vector3(0.92f,-0.38f,0.08f), 58f);
        Shot(OUT+"cmp_stairs_top_down_render.png");
        Place(new Vector3(351.8f,0,132.8f), new Vector3(0.78f,-0.12f,-0.55f), 55f);
        Shot(OUT+"cmp_path_to_carpark_render.png");
        Place(new Vector3(349.0f,0,134.6f), new Vector3(0.28f,0.02f,0.96f), 56f);
        Shot(OUT+"cmp_woods_path_fence_render.png");

        // Ramp / rear
        Place(new Vector3(368.5f,0,125.5f), new Vector3(0.15f,-0.25f,0.96f), 58f, 8.2f);
        Shot(OUT+"cmp_ramp_from_n9_window.png");
        Place(new Vector3(362.8f,0,140.0f), new Vector3(0.25f,0.08f,0.96f), 60f, 1.65f);
        Shot(OUT+"cmp_ramp_ground.png");
        PlaceOrtho(new Vector3(385f,0,185f), 55f);
        Shot(OUT+"cmp_rear_park_topdown.png");

        // Batch2
        Place(new Vector3(374.5f,0,112.5f), new Vector3(0f,0.05f,1f), 55f); // 01 entrance
        Shot(OUT+"cmp_batch2_01_entrance_render.png");
        Place(new Vector3(364.5f,0,148.5f), new Vector3(0.55f,0.05f,0.75f), 58f); // 02 stair bottom
        Shot(OUT+"cmp_batch2_02_stair_bottom_render.png");
        Place(new Vector3(366.5f,0,120.0f), new Vector3(0.15f,0.02f,1f), 55f); // 03 footpath
        Shot(OUT+"cmp_batch2_03_footpath_render.png");
        Place(new Vector3(378.0f,0,182.0f), new Vector3(0.05f,0.02f,1f), 60f); // 04 rear park
        Shot(OUT+"cmp_batch2_04_rear_park_render.png");
        Place(new Vector3(358.0f,0,174.0f), new Vector3(0.6f,0.05f,0.7f), 58f); // 05 junction
        Shot(OUT+"cmp_batch2_05_junction_render.png");
        Place(new Vector3(345.0f,0,176.0f), new Vector3(1f,0.02f,0.15f), 55f); // 06 sidewalk
        Shot(OUT+"cmp_batch2_06_sidewalk_render.png");
        Place(new Vector3(362.0f,0,174.5f), new Vector3(0.4f,0.02f,0.9f), 58f); // 07 crossing
        Shot(OUT+"cmp_batch2_07_crossing_render.png");
        Place(new Vector3(363.5f,0,138.0f), new Vector3(0.1f,-0.05f,-0.95f), 58f); // 08 flat ramp looking south
        Shot(OUT+"cmp_batch2_08_flat_ramp_render.png");
        Place(new Vector3(354.5f,0,129.0f), new Vector3(-0.35f,0.02f,0.9f), 55f); // 09 earth path
        Shot(OUT+"cmp_batch2_09_earth_path_render.png");
        Place(new Vector3(368.0f,0,124.0f), new Vector3(-0.5f,-0.05f,0.85f), 55f); // 10 park edge
        Shot(OUT+"cmp_batch2_10_park_edge_render.png");

        w.ClearSnowVisuals();
        Debug.Log("[VV] SITECMP DONE");
        EditorApplication.Exit(0);
    }
}
