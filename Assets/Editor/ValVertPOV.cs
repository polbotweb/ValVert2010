// First-person views from the woods path (OSM 260433017, top of the log stair): 4 seasons x dawn/noon/dusk/night, same camera.
using UnityEngine; using UnityEditor;
public static class ValVertPOV {
    public static void Run(){
        UnityEditor.SceneManagement.EditorSceneManager.OpenScene("Assets/Scenes/ValVert.unity");
        var go=ValVertEnvWire.Wire(); var clk=go.GetComponent<ValVertClock>(); var sea=go.GetComponent<ValVertSeasons>(); var sl=go.GetComponent<ValVertStreetLights>();
        var cam=new GameObject("povcam").AddComponent<Camera>(); cam.fieldOfView=65; cam.nearClipPlane=0.1f; sl.viewer=cam.transform;
        Vector3 p=new Vector3(349.0f,0,134.6f); Terrain terr=Terrain.activeTerrain;
        foreach(var tt in Terrain.activeTerrains){ var tp=tt.transform.position; var sz=tt.terrainData.size; if(p.x>=tp.x&&p.x<=tp.x+sz.x&&p.z>=tp.z&&p.z<=tp.z+sz.z){ terr=tt; break; } }
        p.y=terr.SampleHeight(p)+terr.transform.position.y+1.7f; cam.transform.position=p; cam.transform.rotation=Quaternion.LookRotation(new Vector3(0.35f,0.02f,1f));
        Debug.Log($"[VV] POV cam {p} terrain={terr.name}");
        string D="/workspace/valvert/pov_seasons/"; System.IO.Directory.CreateDirectory(D);
        foreach(var d in new[]{(4,20,"spring"),(7,15,"summer"),(10,20,"autumn"),(1,20,"winter")}){
            // find sunrise/sunset/solar noon for that date (10-min steps)
            float rise=6,set=18,noon=12,maxAlt=-90; float prev=-90;
            for(float h=0;h<24;h+=1f/12f){ clk.Set(d.Item1,d.Item2,h); float a=clk.SunAltitudeDeg; if(prev<0&&a>=0) rise=h; if(prev>=0&&a<0) set=h; if(a>maxAlt){maxAlt=a;noon=h;} prev=a; }
            foreach(var t in new[]{(rise+0.25f,"1dawn"),(noon,"2midday"),(set+0.25f,"3dusk"),(23f,"4night")}){
                clk.Set(d.Item1,d.Item2,t.Item1); sea.Tick(); sl.Tick();
                var rt=new RenderTexture(1600,900,24); cam.targetTexture=rt; cam.Render(); RenderTexture.active=rt; var tx=new Texture2D(1600,900,TextureFormat.RGB24,false); tx.ReadPixels(new Rect(0,0,1600,900),0,0);
                string n=$"pov_{d.Item3}_{t.Item2}.png"; System.IO.File.WriteAllBytes(D+n,tx.EncodeToPNG()); RenderTexture.active=null; cam.targetTexture=null; Object.DestroyImmediate(rt); Object.DestroyImmediate(tx);
                int hh=(int)t.Item1, mm=(int)((t.Item1-hh)*60);
                Debug.Log($"[VV] POV {n} date=2010-{d.Item1:00}-{d.Item2:00} time={hh:00}:{mm:00} utc+{clk.UtcOffset()} sunAlt={clk.SunAltitudeDeg:F1} leaf={sea.LeafCover:F2} autumn={sea.Autumn:F2} litter={sea.Litter:F2} lampsOn={(clk.SunAltitudeDeg<sl.switchAltitude)}");
            }
        }
        clk.Set(10,20,11f); sea.Tick(); sl.viewer=null; sl.Tick(); Object.DestroyImmediate(cam.gameObject);
        Debug.Log("[VV] POV DONE");
    }
}
