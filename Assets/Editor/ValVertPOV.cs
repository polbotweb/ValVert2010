// First-person views from the woods path (OSM 260433017, top of the log stair): 4 seasons x dawn/noon/dusk/night (auto weather),
// plus weather extras (snowfall, snow cover, rainy autumn day, foggy morning, rain at night under street lamps). Same camera.
using UnityEngine; using UnityEditor; using System;
public static class ValVertPOV {
    static Camera cam; static ValVertClock clk; static ValVertSeasons sea; static ValVertStreetLights sl; static ValVertWeather w;
    const string D="/workspace/valvert/pov_seasons/";
    static void Place(Vector3 p,Vector3 dir){ Terrain terr=Terrain.activeTerrain; foreach(var tt in Terrain.activeTerrains){ var tp=tt.transform.position; var sz=tt.terrainData.size; if(p.x>=tp.x&&p.x<=tp.x+sz.x&&p.z>=tp.z&&p.z<=tp.z+sz.z){ terr=tt; break; } }
        p.y=terr.SampleHeight(p)+terr.transform.position.y+1.7f; cam.transform.position=p; cam.transform.rotation=Quaternion.LookRotation(dir); }
    static void Shot(string n,int mo,int d,float h){
        clk.Set(mo,d,h); w.Tick(); sea.Tick(); sl.Tick(); w.Prewarm(4f);
        var rt=new RenderTexture(1600,900,24); cam.targetTexture=rt; cam.Render(); RenderTexture.active=rt; var tx=new Texture2D(1600,900,TextureFormat.RGB24,false); tx.ReadPixels(new Rect(0,0,1600,900),0,0);
        System.IO.File.WriteAllBytes(D+n,tx.EncodeToPNG()); RenderTexture.active=null; cam.targetTexture=null; UnityEngine.Object.DestroyImmediate(rt); UnityEngine.Object.DestroyImmediate(tx);
        int hh=(int)h, mm=(int)Math.Round((h-hh)*60); if(mm==60){hh++;mm=0;}
        Debug.Log($"[VV] POV {n} date=2010-{mo:00}-{d:00} time={hh:00}:{mm:00} utc+{clk.UtcOffset()} sunAlt={clk.SunAltitudeDeg:F1} moonAlt={clk.MoonAltitudeDeg:F0} moonIll={clk.MoonIllumination:F2} weather={w.Current} T={w.TemperatureC:F0}C cloud={w.Cloud:F2} rain={w.Rain:F2} snowfall={w.Snowfall:F2} snowCover={w.SnowCover:F2} wet={w.Wetness:F2} fog={w.Fog:F2} leaf={sea.LeafCover:F2} autumn={sea.Autumn:F2} litter={sea.Litter:F2}");
    }
    public static void Run(){
        UnityEditor.SceneManagement.EditorSceneManager.OpenScene("Assets/Scenes/ValVert.unity");
        var go=ValVertEnvWire.Wire(); clk=go.GetComponent<ValVertClock>(); sea=go.GetComponent<ValVertSeasons>(); sl=go.GetComponent<ValVertStreetLights>(); w=go.GetComponent<ValVertWeather>();
        cam=new GameObject("povcam").AddComponent<Camera>(); cam.fieldOfView=65; cam.nearClipPlane=0.1f; sl.viewer=cam.transform; w.viewer=cam.transform;
        System.IO.Directory.CreateDirectory(D); w.Force(ValVertWeather.Kind.Auto);
        Vector3 P=new Vector3(349.0f,0,134.6f), V=new Vector3(0.35f,0.02f,1f); Place(P,V);
        // winter date with real (simulated) snow on the ground and no snowfall at noon
        int wm=1,wd=20; for(int m=12;m<=14&&wm==1&&wd==20;m++) for(int d=1;d<=28;d++){ int mo=(m-1)%12+1; if(mo==12) continue; clk.Set(mo,d,12.5f); w.Settle(clk.Now); if(w.SnowCover>0.6f&&w.Current!=ValVertWeather.Kind.Snow){ wm=mo; wd=d; break; } }
        bool only=System.Environment.GetEnvironmentVariable("VV_POV_ONLY")=="weather";
        if(only) Shot("pov_winter_2midday.png",wm,wd,12.75f);
        if(!only) foreach(var d in new[]{(4,20,"spring"),(7,15,"summer"),(10,20,"autumn"),(wm,wd,"winter")}){
            float rise=6,set=18,noon=12,maxAlt=-90,prev=-90;
            for(float h=0;h<24;h+=1f/12f){ clk.Set(d.Item1,d.Item2,h); float a=clk.SunAltitudeDeg; if(prev<0&&a>=0) rise=h; if(prev>=0&&a<0) set=h; if(a>maxAlt){maxAlt=a;noon=h;} prev=a; }
            foreach(var t in new[]{(rise+0.25f,"1dawn"),(noon,"2midday"),(set+0.25f,"3dusk"),(23f,"4night")}) Shot($"pov_{d.Item3}_{t.Item2}.png",d.Item1,d.Item2,t.Item1);
        }
        // extras
        int sm=0,sd=0; float sh=0; for(int mo=1;mo<=2&&sm==0;mo++) for(int d=1;d<=28&&sm==0;d++) for(float h=9;h<=16;h+=1f){ clk.Set(mo,d,h); w.Settle(clk.Now); if(w.Current==ValVertWeather.Kind.Snow&&w.SnowCover>0.3f){ sm=mo; sd=d; sh=h; break; } }
        if(sm>0) Shot("extra_winter_snowfall_auto.png",sm,sd,sh); else { w.Force(ValVertWeather.Kind.Snow); Shot("extra_winter_snowfall_forced.png",1,20,11f); w.Force(ValVertWeather.Kind.Auto); }
        w.Force(ValVertWeather.Kind.Rain); Shot("extra_autumn_rain_day.png",10,20,14f);
        w.Force(ValVertWeather.Kind.Fog); Shot("extra_fog_morning.png",11,10,8.75f);
        Place(new Vector3(470.5f,0,40.0f),new Vector3(0.25f,-0.08f,1f)); w.Force(ValVertWeather.Kind.Rain); Shot("extra_rain_night_lamps.png",10,20,22.5f);
        w.Force(ValVertWeather.Kind.Clear); Shot("extra_clear_night_lamps.png",10,20,22.5f);
        w.Force(ValVertWeather.Kind.Auto); clk.Set(10,20,11f); w.Tick(); sea.Tick(); sl.viewer=null; w.viewer=null; sl.Tick(); UnityEngine.Object.DestroyImmediate(cam.gameObject);
        UnityEditor.SceneManagement.EditorSceneManager.SaveScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene()); AssetDatabase.SaveAssets();
        Debug.Log("[VV] POV DONE");
    }
}
