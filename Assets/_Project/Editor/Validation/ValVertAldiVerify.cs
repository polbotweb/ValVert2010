using UnityEditor; using UnityEngine; using System.IO;
public static class ValVertAldiVerify {
    public static void Run(){
        ValVertAldi.Rebuild();
        var go=ValVertEnvWire.Wire();
        var clk=go.GetComponent<ValVertClock>(); var sea=go.GetComponent<ValVertSeasons>();
        var sl=go.GetComponent<ValVertStreetLights>(); var w=go.GetComponent<ValVertWeather>();
        var cam=new GameObject("aldi_cam").AddComponent<Camera>(); cam.nearClipPlane=0.05f; cam.fieldOfView=65;
        sl.viewer=cam.transform; if(w) w.viewer=cam.transform;
        string D="/workspace/valvert/verify/"; Directory.CreateDirectory(D);
        System.Action<string> shot=(n)=>{
            var rt=new RenderTexture(1600,900,24); cam.targetTexture=rt; cam.Render();
            RenderTexture.active=rt; var tx=new Texture2D(1600,900,TextureFormat.RGB24,false);
            tx.ReadPixels(new Rect(0,0,1600,900),0,0); File.WriteAllBytes(D+n,tx.EncodeToPNG());
            RenderTexture.active=null; cam.targetTexture=null; Object.DestroyImmediate(rt); Object.DestroyImmediate(tx);
            Debug.Log("[VV] ALDI SHOT "+n);
        };
        w.Force(ValVertWeather.Kind.Clear); clk.Set(6,20,12.5f); sea.Tick(); clk.Apply(); sl.Tick();  // June: summer foliage (topdown tree mat check)
        // topdown — building left, Val Vert top
        cam.orthographic=true; cam.orthographicSize=22f; cam.farClipPlane=200;
        cam.transform.position=new Vector3(484f,80f,61f); cam.transform.rotation=Quaternion.Euler(90,0,0);
        shot("aldi_topdown.png");
        // ground day across lot
        cam.orthographic=false; cam.fieldOfView=65;
        Terrain terr=Terrain.activeTerrain;
        System.Action<Vector3,Vector3> place=(p,dir)=>{
            foreach(var tt in Terrain.activeTerrains){ var tp=tt.transform.position; var sz=tt.terrainData.size;
                if(p.x>=tp.x&&p.x<=tp.x+sz.x&&p.z>=tp.z&&p.z<=tp.z+sz.z){ terr=tt; break; } }
            p.y=terr.SampleHeight(p)+terr.transform.position.y+1.6f; cam.transform.position=p; cam.transform.rotation=Quaternion.LookRotation(dir);
        };
        place(new Vector3(492f,0,55f),new Vector3(-1f,-0.08f,0.15f)); clk.Set(6,20,12f); sea.Tick(); sl.Tick(); shot("aldi_ground_lot_day.png");
        place(new Vector3(470.5f,0,40f),new Vector3(0.25f,-0.08f,1f)); clk.Set(10,20,11f); sea.Tick(); sl.Tick(); shot("aldi_ground_day.png");
        // stair approach photo angle
        place(new Vector3(364.3f,0,123.2f),new Vector3(-0.15f,0.05f,0.99f)); cam.fieldOfView=70; clk.Set(10,20,13f); sea.Tick(); sl.Tick(); shot("stair_approach_fence_boulder.png");
        place(new Vector3(363.5f,0,131.0f),new Vector3(0.05f,0.15f,1f)); shot("door_vigik_closeup.png");
        Object.DestroyImmediate(cam.gameObject);
        Debug.Log("[VV] ALDI VERIFY DONE");
    }
}
