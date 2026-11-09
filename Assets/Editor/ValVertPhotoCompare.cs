// Autumn midday compares vs user photos (never commit the photos).
using UnityEngine; using UnityEditor; using System.IO;
public static class ValVertPhotoCompare {
    const string OUT="/workspace/valvert/verify/";
    static Camera cam; static ValVertClock clk; static ValVertSeasons sea; static ValVertStreetLights sl; static ValVertWeather w;
    static void Place(Vector3 p, Vector3 dir, float fov=65f){
        Terrain terr=Terrain.activeTerrain;
        foreach(var tt in Terrain.activeTerrains){ var tp=tt.transform.position; var sz=tt.terrainData.size;
            if(p.x>=tp.x&&p.x<=tp.x+sz.x&&p.z>=tp.z&&p.z<=tp.z+sz.z){ terr=tt; break; } }
        cam.orthographic=false; cam.fieldOfView=fov; cam.farClipPlane=2000; cam.nearClipPlane=0.08f;
        p.y=terr.SampleHeight(p)+terr.transform.position.y+1.65f;
        cam.transform.position=p; cam.transform.rotation=Quaternion.LookRotation(dir);
    }
    static void Shot(string path){
        w.Force(ValVertWeather.Kind.Clear); w.ClearSnowVisuals();
        // Nov 9: Litter~0.74 so earth-path leaf overlays engage (autumn only)
        clk.Set(11,9,13f); w.Tick(); w.ClearSnowVisuals(); sea.Tick(); clk.Apply(); sl.Tick();
        w.ClearSnowVisuals(); sea.snowCover=0f; sea.Tick();
        var rt=new RenderTexture(1280,720,24); cam.targetTexture=rt; cam.Render();
        RenderTexture.active=rt; var tx=new Texture2D(1280,720,TextureFormat.RGB24,false);
        tx.ReadPixels(new Rect(0,0,1280,720),0,0); File.WriteAllBytes(path,tx.EncodeToPNG());
        RenderTexture.active=null; cam.targetTexture=null; Object.DestroyImmediate(rt); Object.DestroyImmediate(tx);
        Debug.Log($"[VV] PHOTOCMP wrote {path} litter={sea.Litter:F2} autumn={sea.Autumn:F2} season={sea.SeasonName}");
    }
    public static void Run(){
        UnityEditor.SceneManagement.EditorSceneManager.OpenScene("Assets/Scenes/ValVert.unity");
        ValVertDress.RepairTreePrototypes();
        var go=ValVertEnvWire.Wire(); clk=go.GetComponent<ValVertClock>(); sea=go.GetComponent<ValVertSeasons>();
        sl=go.GetComponent<ValVertStreetLights>(); w=go.GetComponent<ValVertWeather>();
        // EnvWire already ran WeatherWire + autumn litter overlays (stair_earth/path_dirt/leafground)
        w=go.GetComponent<ValVertWeather>();
        var sa=GameObject.Find("V3_StairApproach"); if(sa) Object.DestroyImmediate(sa);
        var ld=GameObject.Find("V3_LogStairsDress"); if(ld) Object.DestroyImmediate(ld);
        if(ValVertWoods.MATS==null) ValVertWoods.MATS=new System.Collections.Generic.Dictionary<string,Material>();
        var rock=AssetDatabase.LoadAssetAtPath<Material>("Assets/ValVert/Mats/rock.mat");
        if(rock!=null) ValVertWoods.MATS["rock"]=rock;
        ValVertWoods.StairApproach(); // includes LogStairsDress
        cam=new GameObject("photocmp_cam").AddComponent<Camera>();
        sl.viewer=cam.transform; w.viewer=cam.transform; w.BuildShells();
        Directory.CreateDirectory(OUT);
        // 1) stairs top-down: camera at stair TOP (west/high) looking DOWN toward alley/apartment (east)
        Place(new Vector3(355.6f,0,133.05f), new Vector3(0.92f,-0.38f,0.08f), 58f);
        Shot(OUT+"cmp_stairs_top_down_render.png");
        // 2) path to carpark from stair-top junction looking SE toward park + boulder row
        Place(new Vector3(351.8f,0,132.8f), new Vector3(0.78f,-0.12f,-0.55f), 55f);
        Shot(OUT+"cmp_path_to_carpark_render.png");
        // 3) woods path: fence left (west), stump right (east), looking into woods (+Z)
        Place(new Vector3(349.0f,0,134.6f), new Vector3(0.28f,0.02f,0.96f), 56f);
        Shot(OUT+"cmp_woods_path_fence_render.png");
        w.ClearSnowVisuals();
        Debug.Log("[VV] PHOTOCMP DONE");
        EditorApplication.Exit(0);
    }
}
