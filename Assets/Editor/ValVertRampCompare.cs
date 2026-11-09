// Ramp 9→13 + rear park compares (plan_ramp_9to13.jpg never committed).
using UnityEngine; using UnityEditor; using System.IO;
public static class ValVertRampCompare {
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
        var rt=new RenderTexture(1280,720,24); cam.targetTexture=rt; cam.Render();
        RenderTexture.active=rt; var tx=new Texture2D(1280,720,TextureFormat.RGB24,false);
        tx.ReadPixels(new Rect(0,0,1280,720),0,0); File.WriteAllBytes(path,tx.EncodeToPNG());
        RenderTexture.active=null; cam.targetTexture=null; Object.DestroyImmediate(rt); Object.DestroyImmediate(tx);
        Debug.Log("[VV] RAMPCMP wrote "+path);
    }
    public static void Run(){
        UnityEditor.SceneManagement.EditorSceneManager.OpenScene("Assets/Scenes/ValVert.unity");
        ValVertDress.RepairTreePrototypes();
        var go=ValVertEnvWire.Wire(); clk=go.GetComponent<ValVertClock>(); sea=go.GetComponent<ValVertSeasons>();
        sl=go.GetComponent<ValVertStreetLights>(); w=go.GetComponent<ValVertWeather>();
        var sa=GameObject.Find("V3_StairApproach"); if(sa) Object.DestroyImmediate(sa);
        var ld=GameObject.Find("V3_LogStairsDress"); if(ld) Object.DestroyImmediate(ld);
        var rp=GameObject.Find("V3_Ramp9to13"); if(rp) Object.DestroyImmediate(rp);
        var rr=GameObject.Find("V3_RearPark91113"); if(rr) Object.DestroyImmediate(rr);
        if(ValVertWoods.MATS==null) ValVertWoods.MATS=new System.Collections.Generic.Dictionary<string,Material>();
        var rock=AssetDatabase.LoadAssetAtPath<Material>("Assets/ValVert/Mats/rock.mat");
        if(rock!=null) ValVertWoods.MATS["rock"]=rock;
        ValVertWoods.StairApproach(); // LogStairsDress + Ramp9to13 + RearPark
        cam=new GameObject("rampcmp_cam").AddComponent<Camera>();
        sl.viewer=cam.transform; w.viewer=cam.transform; w.BuildShells();
        Directory.CreateDirectory(OUT);
        // 1) high viewpoint: ~3rd floor window of nº9 looking toward Val Vert / ramp
        // building west/south wing ~x 367-380, z 116-126; 3rd floor ~ +8 m eye
        Place(new Vector3(368.5f,0,125.5f), new Vector3(0.15f,-0.25f,0.96f), 58f, 8.2f);
        Shot(OUT+"cmp_ramp_from_n9_window.png");
        // 2) ground-level view of the ramp (looking up toward Val Vert)
        Place(new Vector3(362.8f,0,140.0f), new Vector3(0.25f,0.08f,0.96f), 60f, 1.65f);
        Shot(OUT+"cmp_ramp_ground.png");
        // 3) top-down of east lane + rear car park
        PlaceOrtho(new Vector3(385f,0,185f), 55f);
        Shot(OUT+"cmp_rear_park_topdown.png");
        w.ClearSnowVisuals();
        Debug.Log("[VV] RAMPCMP DONE");
        EditorApplication.Exit(0);
    }
}
