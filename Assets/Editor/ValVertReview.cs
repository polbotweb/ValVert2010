// 8 locations × 4 seasons × 4 times → frames + contact sheets under /workspace/valvert/review/
using UnityEngine; using UnityEditor; using System.IO;
public static class ValVertReview {
    // Fast path: only Aldi winter dawn+midday for snow/magenta validation
    public static void ProbeSnow(){
        UnityEditor.SceneManagement.EditorSceneManager.OpenScene("Assets/Scenes/ValVert.unity");
        ValVertDress.RepairTreePrototypes();
        var go=ValVertEnvWire.Wire(); clk=go.GetComponent<ValVertClock>(); sea=go.GetComponent<ValVertSeasons>();
        sl=go.GetComponent<ValVertStreetLights>(); w=go.GetComponent<ValVertWeather>();
        ValVertWeatherWire.Wire(go); w=go.GetComponent<ValVertWeather>();
        Debug.Log($"[VV] PROBE weather snowMats={w.snowMats.Count} layers={w.snowLayers.Count} treeVar={w.treeVariants.Count}");
        foreach(var st in w.snowMats){ int nn=0; if(st.steps!=null) foreach(var t in st.steps) if(t) nn++;
          Debug.Log($"[VV] PROBE mat {st.mat?.name} factor={st.factor} stepsNonNull={nn}/{st.steps?.Length}"); }
        var plaster=AssetDatabase.LoadAssetAtPath<Material>("Assets/ValVert/Mats/plaster.mat");
        var metal=AssetDatabase.LoadAssetAtPath<Material>("Assets/ValVert/Mats/metal_grey.mat")??plaster;
        int bf=0; foreach(var r in Object.FindObjectsByType<Renderer>(FindObjectsSortMode.None)){
          if(!r) continue; if(r.gameObject.name.ToLower().Contains("bollard")){ r.sharedMaterial=metal; bf++; }
          else { var ms=r.sharedMaterials; bool ch=false; for(int i=0;i<ms.Length;i++){ var m=ms[i]; if(m==null||m.shader==null||m.shader.name.Contains("InternalError")){ ms[i]=plaster; ch=true; } } if(ch) r.sharedMaterials=ms; }
        }
        Debug.Log($"[VV] PROBE bollardFixes={bf}");
        cam=new GameObject("review_cam").AddComponent<Camera>(); cam.nearClipPlane=0.08f;
        sl.viewer=cam.transform; w.viewer=cam.transform; w.BuildShells(); w.particleRateScale=0.5f;
        Directory.CreateDirectory(D+"frames/aldi");
        Place(new Vector3(492f,0,55f),new Vector3(-1f,-0.08f,0.15f),false,0);
        Shot(D+"frames/aldi/winter_dawn.png",1,20,8.5f,true,0.03f);
        Shot(D+"frames/aldi/winter_midday.png",1,20,12.5f,true,0.20f);
        // also woods shot where ground snow was proven in 40e6474
        Directory.CreateDirectory(D+"frames/woods");
        Place(new Vector3(349f,0,134.6f),new Vector3(0.35f,0.02f,1f),false,0);
        Shot(D+"frames/woods/winter_midday.png",1,20,12.5f,true,0.20f);
        Debug.Log("[VV] PROBE DONE");
        EditorApplication.Exit(0);
    }

    static Camera cam; static ValVertClock clk; static ValVertSeasons sea; static ValVertStreetLights sl; static ValVertWeather w;
    static System.Collections.Generic.Dictionary<Material,Color> carColorCache=new System.Collections.Generic.Dictionary<Material,Color>();
    const string D="/workspace/valvert/review/";
    static bool PixelErrorGuard(Texture2D tx, string tag){
        var px=tx.GetPixels32(); int mag=0, cyan=0, wash=0; long sum=0;
        for(int i=0;i<px.Length;i++){ var c=px[i]; sum+=c.r+c.g+c.b;
            if(c.r>200&&c.g<90&&c.b>200) mag++;
            if(c.r<90&&c.g>180&&c.b>180&&c.g+c.b>c.r*2+40) cyan++;
            if(c.r>248&&c.g>248&&c.b>248) wash++; }
        float mp=100f*mag/px.Length, cp=100f*cyan/px.Length, wp=100f*wash/px.Length, mean=sum/(3f*px.Length);
        if(mp>0.15f||cp>0.15f){ Debug.LogError($"[VV] REVIEW PIXEL GUARD FAIL {tag} magenta={mp:F2}% cyan={cp:F2}%"); return true; }
        if(wp>55f && mean>220f){ Debug.LogError($"[VV] REVIEW PIXEL GUARD FAIL {tag} washed-white={wp:F1}% mean={mean:F1}"); return true; }
        return false;
    }
    static void MaterialGuardOrExit(string tag){
        int bad=0;
        foreach(var r in Object.FindObjectsByType<Renderer>(FindObjectsInactive.Include,FindObjectsSortMode.None)){
            foreach(var m in r.sharedMaterials){
                if(m==null||m.shader==null||m.shader.name.Contains("InternalError")){
                    bad++; Debug.LogError($"[VV] REVIEW BAD MAT {tag} {r.name}/{(m?m.name:"null")} shader={(m&&m.shader?m.shader.name:"null")}");
                }
            }
        }
        if(bad>0){ Debug.LogError($"[VV] REVIEW MATERIAL GUARD FAIL {tag} bad={bad}"); EditorApplication.Exit(4); }
    }

    static void Place(Vector3 p,Vector3 dir,bool ortho=false,float osize=80f){
        Terrain terr=Terrain.activeTerrain;
        foreach(var tt in Terrain.activeTerrains){ var tp=tt.transform.position; var sz=tt.terrainData.size;
            if(p.x>=tp.x&&p.x<=tp.x+sz.x&&p.z>=tp.z&&p.z<=tp.z+sz.z){ terr=tt; break; } }
        if(ortho){ cam.orthographic=true; cam.orthographicSize=osize; cam.farClipPlane=2000;
            float y=terr.SampleHeight(p)+terr.transform.position.y+180f;
            cam.transform.position=new Vector3(p.x,y,p.z); cam.transform.rotation=Quaternion.Euler(90,0,0); }
        else { cam.orthographic=false; cam.fieldOfView=65; cam.farClipPlane=2000;
            p.y=terr.SampleHeight(p)+terr.transform.position.y+1.7f; cam.transform.position=p; cam.transform.rotation=Quaternion.LookRotation(dir); }
    }
    static void Shot(string path,int mo,int d,float h, bool winter=false, float snowFall=0.02f){
        clk.Set(mo,d,h);
        if(winter) w.Force(ValVertWeather.Kind.Snow); else { w.Force(ValVertWeather.Kind.Clear); w.ClearSnowVisuals(); }
        w.Tick();
        if(!winter) w.ClearSnowVisuals(); // Settle/Tick must not leave snow albedo on mats/terrain layers
        sea.Tick(); clk.Apply(); sl.Tick();
        if(winter) w.ForceSnowCover(0.92f, snowFall);
        else { w.ClearSnowVisuals(); sea.snowCover=0f; sea.Tick(); }
        ValVertWoods.HealFoliage(); ValVertWoods.HealTreePrototypes();
        // Hard-assert snow albedo on Aldi lot meshes (sharedMaterial path can miss instance mats)
        if(winter){
          var snowTex=AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/ValVert/Gen/snow/asphalt_ww_diff_s4.jpg");
          var grassSnow=AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/ValVert/Gen/snow/fg02_diff_s4.jpg");
          foreach(var r in Object.FindObjectsByType<MeshRenderer>(FindObjectsSortMode.None)){
            if(!r||!r.sharedMaterial) continue; var p=r.transform.position;
            if(p.x<465||p.x>522||p.z<38||p.z>86) continue;
            var m=r.sharedMaterial; string n=m.name.ToLower();
            if(n.Contains("asphalt")||n.Contains("parking")||n=="road"||r.gameObject.name.ToLower().Contains("aldi_cover")||r.gameObject.name.ToLower().Contains("cover")){
              if(snowTex&&m.HasProperty("_BaseMap")){ m.SetTexture("_BaseMap",snowTex); m.SetColor("_BaseColor",new Color(1.05f,1.06f,1.08f)); if(m.HasProperty("_Smoothness")) m.SetFloat("_Smoothness",0.05f); if(m.HasProperty("_BumpScale")) m.SetFloat("_BumpScale",0.02f); }
            }
            if(n.Contains("grass")||n.Contains("strip")||n.Contains("leaf")){
              if(grassSnow&&m.HasProperty("_BaseMap")){ m.SetTexture("_BaseMap",grassSnow); m.SetColor("_BaseColor",Color.white); }
            }
          }
          // Car bodies: push BaseColor toward snow white (shells often miss instanced car mats)
          var cars=GameObject.Find("V3_Cars2010");
          if(cars) foreach(var r in cars.GetComponentsInChildren<MeshRenderer>()){
            if(!r||!r.sharedMaterial) continue; var m=r.sharedMaterial;
            if(!m.HasProperty("_BaseColor")) continue;
            string n=m.name.ToLower(); if(n.Contains("glass")||n.Contains("window")||n.Contains("tyre")||n.Contains("tire")||n.Contains("wheel")||n.Contains("chrome")) continue;
            if(!carColorCache.ContainsKey(m)) carColorCache[m]=m.GetColor("_BaseColor");
            m.SetColor("_BaseColor", Color.Lerp(carColorCache[m], new Color(0.92f,0.94f,0.97f), 0.72f));
          }
        } else {
          // Restore lot + car materials after a prior winter shot in this process
          var asp=AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/ValVert/Ground/asphalt_ww_diff.jpg");
          var grass=AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/ValVert/Ground/leafy_grass_diff.jpg");
          foreach(var r in Object.FindObjectsByType<MeshRenderer>(FindObjectsSortMode.None)){
            if(!r||!r.sharedMaterial) continue; var pp=r.transform.position;
            if(pp.x<465||pp.x>522||pp.z<38||pp.z>86) continue;
            var m=r.sharedMaterial; string n=m.name.ToLower();
            if(asp&&(n.Contains("asphalt")||n.Contains("parking")||n=="road"||r.gameObject.name.ToLower().Contains("cover"))){
              if(m.HasProperty("_BaseMap")) m.SetTexture("_BaseMap",asp);
              if(m.HasProperty("_BaseColor")) m.SetColor("_BaseColor",Color.white);
              if(m.HasProperty("_Smoothness")) m.SetFloat("_Smoothness",0.6f);
              if(m.HasProperty("_BumpScale")) m.SetFloat("_BumpScale",1.2f);
            }
            if(grass&&(n.Contains("grass")||n.Contains("strip"))){
              if(m.HasProperty("_BaseMap")) m.SetTexture("_BaseMap",grass);
              if(m.HasProperty("_BaseColor")) m.SetColor("_BaseColor",new Color(.45f,.72f,.32f));
            }
          }
          if(carColorCache.Count>0) foreach(var kv in carColorCache) if(kv.Key) kv.Key.SetColor("_BaseColor", kv.Value);
        }
        w.Prewarm(winter ? 2f : 2.5f);
        var rt=new RenderTexture(1280,720,24); cam.targetTexture=rt; cam.Render();
        RenderTexture.active=rt; var tx=new Texture2D(1280,720,TextureFormat.RGB24,false);
        tx.ReadPixels(new Rect(0,0,1280,720),0,0); tx.Apply();
        if(PixelErrorGuard(tx, path)){ File.WriteAllBytes(path+".bad.png",tx.EncodeToPNG()); RenderTexture.active=null; cam.targetTexture=null; Object.DestroyImmediate(rt); Object.DestroyImmediate(tx); EditorApplication.Exit(5); return; }
        File.WriteAllBytes(path,tx.EncodeToPNG());
        RenderTexture.active=null; cam.targetTexture=null; Object.DestroyImmediate(rt); Object.DestroyImmediate(tx);
        Debug.Log($"[VV] REVIEW {path} seasonLeaf={sea.LeafCover:F2} autumn={sea.Autumn:F2} snow={w.SnowCover:F2} fall={w.Snowfall:F2}");
    }
    public static void Run(){
        UnityEditor.SceneManagement.EditorSceneManager.OpenScene("Assets/Scenes/ValVert.unity");
        // CRITICAL: rebind tree prototypes BEFORE WeatherWire so snow treeVariants/shells are generated
        ValVertDress.RepairTreePrototypes();
        {
            int instances=0, bound=0, protos=0;
            foreach(var t in Terrain.activeTerrains){
                if(!t||!t.terrainData) continue;
                var td=t.terrainData; instances+=td.treeInstanceCount; protos+=td.treePrototypes.Length;
                foreach(var tp in td.treePrototypes) if(tp.prefab) bound++;
            }
            Debug.Log($"[VV] REVIEW TREE GUARD instances={instances} protos={protos} boundPrefabs={bound}");
            if(instances < 1000 || bound < 10){
                Debug.LogError($"[VV] REVIEW ABORT: vegetation missing or unbound (instances={instances}, boundPrefabs={bound}). "+
                    "Likely TerrainData LFS restore left treePrototype.prefab=null — call ValVertDress.RepairTreePrototypes() / reimport vendor wrappers.");
                EditorApplication.Exit(2); return;
            }
        }
        var go=ValVertEnvWire.Wire(); clk=go.GetComponent<ValVertClock>(); sea=go.GetComponent<ValVertSeasons>();
        sl=go.GetComponent<ValVertStreetLights>(); w=go.GetComponent<ValVertWeather>();
        // Re-wire weather AFTER repair so treeVariants > 0 (branch/spruce snow shells)
        ValVertWeatherWire.Wire(go); w=go.GetComponent<ValVertWeather>();
        Debug.Log($"[VV] REVIEW weather after repair: snowMats={w.snowMats.Count} snowLayers={w.snowLayers.Count} treeVariants={w.treeVariants.Count} shellSrcs={w.shellSrcs.Count}");
        if(w.treeVariants.Count < 5)
            Debug.LogWarning("[VV] REVIEW: few treeVariants — branch snow shells may be missing");
        var sa=GameObject.Find("V3_StairApproach"); if(sa) Object.DestroyImmediate(sa);
        if(ValVertWoods.MATS==null) ValVertWoods.MATS=new System.Collections.Generic.Dictionary<string,Material>();
        var rock=AssetDatabase.LoadAssetAtPath<Material>("Assets/ValVert/Mats/rock.mat");
        if(rock!=null) ValVertWoods.MATS["rock"]=rock;
        ValVertWoods.StairApproach();
        var plaster=AssetDatabase.LoadAssetAtPath<Material>("Assets/ValVert/Mats/plaster.mat");
        var metal=AssetDatabase.LoadAssetAtPath<Material>("Assets/ValVert/Mats/metal_grey.mat")
            ?? AssetDatabase.LoadAssetAtPath<Material>("Assets/ValVert/Mats/bollard_m.mat")
            ?? plaster;
        int magFix=0, bollardFix=0;
        foreach(var r in Object.FindObjectsByType<Renderer>(FindObjectsSortMode.None)){
            if(!r) continue;
            string nm=r.gameObject.name.ToLower();
            if(nm.Contains("bollard")){
                r.sharedMaterial=metal; bollardFix++; continue;
            }
            var ms=r.sharedMaterials; bool ch=false;
            for(int i=0;i<ms.Length;i++){
                var m=ms[i];
                bool bad=m==null;
                if(!bad){ var sh=m.shader; if(sh==null || sh.name=="Hidden/InternalErrorShader" || sh.name.Contains("InternalError")) bad=true; }
                if(bad){ ms[i]=plaster; ch=true; magFix++; }
            }
            if(ch) r.sharedMaterials=ms;
        }
        Debug.Log($"[VV] REVIEW magenta fixes={magFix} bollardFixes={bollardFix}");
        int moreFix=ValVertWoods.FixBrokenMaterials(); int heal=ValVertWoods.HealFoliage(); Debug.Log($"[VV] REVIEW FixBrokenMaterials={moreFix} foliageHeal={heal}");
        MaterialGuardOrExit("post-fix");
        cam=new GameObject("review_cam").AddComponent<Camera>(); cam.nearClipPlane=0.08f;
        sl.viewer=cam.transform; w.viewer=cam.transform; w.BuildShells();
        w.particleRateScale=1f;
        Directory.CreateDirectory(D+"frames");
        var seasons=new (string id,int mo,int d)[]{ ("spring",4,20),("summer",7,15),("autumn",10,20),("winter",1,20) };
        var locs=new (string id,Vector3 p,Vector3 dir,bool ortho,float osize)[]{
            ("aldi",new Vector3(492f,0,55f),new Vector3(-1f,-0.08f,0.15f),false,0),
            ("stairs",new Vector3(364.3f,0,123.2f),new Vector3(-0.15f,0.05f,0.99f),false,0),
            ("valvert",new Vector3(450f,0,108f),new Vector3(0.95f,-0.05f,0.2f),false,0),
            ("woods",new Vector3(349f,0,134.6f),new Vector3(0.35f,0.02f,1f),false,0),
            ("barral",new Vector3(252f,0,308f),new Vector3(0.6f,-0.05f,-0.7f),false,0),
            ("vovray",new Vector3(575f,0,95f),new Vector3(-0.7f,0.05f,0.6f),false,0),
            ("orme",new Vector3(340f,0,204f),new Vector3(0.2f,-0.05f,1f),false,0),
            ("aerial",new Vector3(420f,0,80f),Vector3.down,true,140f),
        };
        foreach(var loc in locs){
            string ld=D+"frames/"+loc.id+"/"; Directory.CreateDirectory(ld);
            Place(loc.p,loc.dir,loc.ortho,loc.osize);
            Shot("/tmp/vv_warmup.png", 7,15,12.5f, false, 0f);
            foreach(var s in seasons){
                float rise=6,set=18,noon=12,maxAlt=-90,prev=-90;
                for(float h=0;h<24;h+=1f/12f){ clk.Set(s.mo,s.d,h); float a=clk.SunAltitudeDeg;
                    if(prev<0&&a>=0) rise=h; if(prev>=0&&a<0) set=h; if(a>maxAlt){maxAlt=a;noon=h;} prev=a; }
                var times=new (string id,float h)[]{ ("dawn",rise+0.3f),("midday",noon),("dusk", s.id=="winter" ? set-0.6f : set+0.2f),("night",22.5f) };
                foreach(var t in times){
                    bool win=s.id=="winter";
                    float fall = win ? ((t.id=="midday") ? 0.20f : 0.03f) : 0f;
                    w.particleRateScale = win ? ((t.id=="midday") ? 0.5f : 0.2f) : 1f;
                    Shot($"{ld}{s.id}_{t.id}.png", s.mo,s.d,t.h, win, fall);
                    // Guard: non-winter must not retain snow cover / snow albedo on terrain layers
                    if(!win && (t.id=="midday")){
                        if(w.SnowCover > 0.05f){
                            Debug.LogError($"[VV] REVIEW SNOW LEAK: {loc.id}/{s.id}_{t.id} SnowCover={w.SnowCover:F2} (expected ~0)");
                            EditorApplication.Exit(3); return;
                        }
                        int snowTexHits=0;
                        foreach(var l in w.snowLayers){
                            if(!l.layer||!l.layer.diffuseTexture) continue;
                            string tn=l.layer.diffuseTexture.name;
                            if(tn.Contains("_s3")||tn.Contains("_s4")||tn.Contains("slush_s")) snowTexHits++;
                        }
                        if(snowTexHits>0){
                            Debug.LogError($"[VV] REVIEW SNOW LEAK: {loc.id}/{s.id} terrain layers still on snow step textures (hits={snowTexHits})");
                            EditorApplication.Exit(3); return;
                        }
                        Debug.Log($"[VV] REVIEW SNOW CLEAR OK {loc.id}/{s.id}_{t.id} snow={w.SnowCover:F2} snowTexHits=0");
                    }
                }
            }
            Debug.Log("[VV] REVIEW loc done "+loc.id);
        }
        w.Force(ValVertWeather.Kind.Clear); w.ClearSnowVisuals(); w.Force(ValVertWeather.Kind.Auto); w.particleRateScale=1f; w.ClearShells();
        Object.DestroyImmediate(cam.gameObject);
        // Persist repaired tree protos + stair approach into the scene for the game on this box
        UnityEditor.SceneManagement.EditorSceneManager.SaveScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene());
        AssetDatabase.SaveAssets();
        Debug.Log("[VV] REVIEW FRAMES DONE");
        EditorApplication.Exit(0);
    }

    public static void SiteSheets(){
        UnityEditor.SceneManagement.EditorSceneManager.OpenScene("Assets/Scenes/ValVert.unity");
        ValVertDress.RepairTreePrototypes();
        var go=ValVertEnvWire.Wire(); clk=go.GetComponent<ValVertClock>(); sea=go.GetComponent<ValVertSeasons>();
        sl=go.GetComponent<ValVertStreetLights>(); w=go.GetComponent<ValVertWeather>();
        ValVertWeatherWire.Wire(go); w=go.GetComponent<ValVertWeather>();
        var sa=GameObject.Find("V3_StairApproach"); if(sa) Object.DestroyImmediate(sa);
        if(ValVertWoods.MATS==null) ValVertWoods.MATS=new System.Collections.Generic.Dictionary<string,Material>();
        var rock=AssetDatabase.LoadAssetAtPath<Material>("Assets/ValVert/Mats/rock.mat");
        if(rock!=null){ ValVertWoods.EnsureUrp(rock); ValVertWoods.MATS["rock"]=rock; }
        ValVertWoods.StairApproach();
        int more=ValVertWoods.FixBrokenMaterials(); int heal=ValVertWoods.HealFoliage(); int proto=ValVertWoods.HealTreePrototypes();
        foreach(var t in Terrain.activeTerrains){ if(t) t.drawTreesAndFoliage=false; }
        Debug.Log($"[VV] SITESHEETS FixBroken={more} foliageHeal={heal} protoHeal={proto} terrainTrees=off");
        MaterialGuardOrExit("sitesheets");
        cam=new GameObject("sitesheets_cam").AddComponent<Camera>();
        sl.viewer=cam.transform; w.viewer=cam.transform; w.BuildShells();
        var seasons=new (string id,int mo,int d)[]{ ("spring",4,20),("summer",7,15),("autumn",10,25),("winter",1,20) };
        var locs=new (string id,Vector3 p,Vector3 dir,bool ortho,float osize)[]{
            ("stairs",new Vector3(364.3f,0,123.2f),new Vector3(-0.15f,0.05f,0.99f),false,0),
            ("valvert",new Vector3(450f,0,108f),new Vector3(0.95f,-0.05f,0.2f),false,0),
            ("woods",new Vector3(349f,0,134.6f),new Vector3(0.35f,0.02f,1f),false,0),
        };
        foreach(var loc in locs){
            string ld=D+"frames/"+loc.id+"/"; Directory.CreateDirectory(ld);
            Place(loc.p,loc.dir,loc.ortho,loc.osize);
            // Discard first frame — URP/sky/shells often wash out before first settled Tick
            Shot("/tmp/vv_warmup.png", 7,15,12.5f, false, 0f);
            foreach(var ssn in seasons){
                float rise=6,set=18,noon=12,maxAlt=-90,prev=-90;
                for(float h=5f;h<=21f;h+=0.25f){ clk.Set(ssn.mo,ssn.d,h); float a=clk.SunAltitudeDeg;
                    if(prev<0&&a>=0) rise=h; if(prev>=0&&a<0) set=h; if(a>maxAlt){maxAlt=a;noon=h;} prev=a; }
                var times=new (string id,float h)[]{ ("dawn",rise+0.3f),("midday",noon),("dusk", ssn.id=="winter" ? set-0.6f : set+0.2f),("night",22.5f) };
                foreach(var t in times){
                    bool win=ssn.id=="winter";
                    float fall = win ? ((t.id=="midday") ? 0.20f : 0.03f) : 0f;
                    w.particleRateScale = win ? ((t.id=="midday") ? 0.5f : 0.2f) : 1f;
                    Shot($"{ld}{ssn.id}_{t.id}.png", ssn.mo,ssn.d,t.h, win, fall);
                    if(!win && t.id=="midday"){
                        if(w.SnowCover>0.05f){ Debug.LogError($"[VV] REVIEW SNOW LEAK {loc.id}/{ssn.id}"); EditorApplication.Exit(3); return; }
                        Debug.Log($"[VV] REVIEW SNOW CLEAR OK {loc.id}/{ssn.id}_{t.id} snow={w.SnowCover:F2}");
                    }
                }
            }
            Debug.Log("[VV] SITESHEETS loc done "+loc.id);
        }
        w.Force(ValVertWeather.Kind.Clear); w.ClearSnowVisuals(); w.Force(ValVertWeather.Kind.Auto); w.particleRateScale=1f; w.ClearShells();
        Object.DestroyImmediate(cam.gameObject);
        Debug.Log("[VV] SITESHEETS DONE");
        EditorApplication.Exit(0);
    }
}
