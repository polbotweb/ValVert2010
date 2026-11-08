// Environment-only season/weather controls (no gameplay/time simulation).
// Select with CLI args  -vvSeason autumn|spring|summer|winter  -vvWeather dry|wet   (or env VV_SEASON / VV_WEATHER).
// Defaults: autumn + dry overcast (matches the 2010 autumn reference photos).
// Base surfaces (asphalt, soil) are season-neutral; autumn leaf litter is an overlay swapped in only for autumn.
using UnityEngine; using UnityEditor; using System.Linq;
public static class ValVertSeason {
    public enum Season{Spring,Summer,Autumn,Winter} public enum Weather{Dry,Wet}
    const string M="Assets/ValVert/Mats/", GEN="Assets/ValVert/Gen/", G="Assets/ValVert/Ground/", T="Assets/ValVert/Veg/tex/", P6="Assets/ValVert/Veg/pilot6/";
    static string Arg(string k,string env,string def){ var a=System.Environment.GetCommandLineArgs(); int i=System.Array.IndexOf(a,k); if(i>=0&&i+1<a.Length) return a[i+1]; var e=System.Environment.GetEnvironmentVariable(env); return string.IsNullOrEmpty(e)?def:e; }
    public static Season SelSeason => (Season)System.Enum.Parse(typeof(Season),Arg("-vvSeason","VV_SEASON","autumn"),true);
    public static Weather SelWeather => (Weather)System.Enum.Parse(typeof(Weather),Arg("-vvWeather","VV_WEATHER","dry"),true);
    static Material Mt(string n)=>AssetDatabase.LoadAssetAtPath<Material>(M+n+".mat");
    static Texture2D Tx(string p)=>AssetDatabase.LoadAssetAtPath<Texture2D>(p);
    static void Set(string mat,string tex,string nor=null,Color? c=null){ var m=Mt(mat); if(!m) return; var t=Tx(tex); if(t) m.SetTexture("_BaseMap",t); if(nor!=null){ var n=Tx(nor); if(n){ m.SetTexture("_BumpMap",n); m.EnableKeyword("_NORMALMAP"); } } if(c.HasValue) m.SetColor("_BaseColor",c.Value); EditorUtility.SetDirty(m); }
    static readonly System.Collections.Generic.Dictionary<string,Color> baseCol=new System.Collections.Generic.Dictionary<string,Color>();
    static readonly System.Collections.Generic.Dictionary<string,float> baseSm=new System.Collections.Generic.Dictionary<string,float>();
    public static void Apply(Season s,Weather w){
        bool aut=s==Season.Autumn;
        // 1) ground overlays: autumn litter vs season-neutral base
        if(aut) Set("asphalt_leaves",GEN+"asphalt_leaves.png",G+"asphalt_02_nor.jpg",Color.white); else Set("asphalt_leaves",G+"asphalt_ww_diff.jpg",G+"asphalt_ww_nor.jpg",Color.white);
        if(aut) Set("corr_leaves",GEN+"corrugated_leaves.png"); else Set("corr_leaves",GEN+"corrugated_iron_02_diff_1k.jpg");
        // woodland floor: Yughues photogrammetric forest ground (year-round decomposed litter/soil) everywhere in woods
        foreach(var n in new[]{"leafground","stair_earth","forest_fg02"}) Set(n,P6+"fg02_diff.jpg",P6+"fg02_nor.jpg",Color.white);
        var tl=AssetDatabase.LoadAssetAtPath<TerrainLayer>(G+"forest_leaves_02.terrainlayer");
        if(tl){ if(aut){ tl.diffuseTexture=Tx(G+"forest_leaves_02_diff.jpg"); tl.normalMapTexture=Tx(G+"forest_leaves_02_nor.jpg"); tl.tileSize=new Vector2(3,3); }
                else { tl.diffuseTexture=Tx(P6+"fg02_diff.jpg"); tl.normalMapTexture=Tx(P6+"fg02_nor.jpg"); tl.tileSize=new Vector2(2.5f,2.5f); } EditorUtility.SetDirty(tl); }
        // 2) foliage colour/coverage of deciduous species (fir/laurel are evergreen)
        foreach(var sp in new[]{"tree_small_02","fir_tree_01","shrub_02"}) foreach(var v in new[]{"autumnA","autumnB","green"}){
            var m=Mt(sp+"_leaves_"+v)??Mt(sp+"_"+v); if(!m) continue;
            string tv=aut?v:"green"; var t=Tx(T+sp+"_leaves_"+tv+".png"); if(t) m.SetTexture("_BaseMap",t);
            bool bare=s==Season.Winter&&sp!="fir_tree_01"; m.SetFloat("_Cutoff",bare?0.93f:0.45f);   // winter: deciduous canopy mostly bare (not verified in renders)
            m.SetColor("_BaseColor",bare?new Color(.55f,.45f,.35f):(s==Season.Spring?new Color(.95f,1.05f,.85f):Color.white)); EditorUtility.SetDirty(m); }
        // 3) weather: wet ground darkens + glossier (no rain particles)
        foreach(var n in new[]{"asphalt_road","road","parking_lot","asphalt_leaves","leafground","forest_fg02","stair_earth","path_concrete"}){ var m=Mt(n); if(!m) continue;
            if(!baseCol.ContainsKey(n)){ baseCol[n]=m.GetColor("_BaseColor"); baseSm[n]=m.GetFloat("_Smoothness"); }
            bool wet=w==Weather.Wet; m.SetColor("_BaseColor",baseCol[n]*(wet?0.72f:1f)); m.SetFloat("_Smoothness",wet?Mathf.Min(1f,baseSm[n]+0.35f):baseSm[n]); EditorUtility.SetDirty(m); }
        AssetDatabase.SaveAssets(); Debug.Log("[VV] season "+s+" weather "+w);
    }
}

// ---- Runtime environment wiring (clock + seasons + street lights) and batch verification ----
public static class ValVertEnvWire {
    const string M="Assets/ValVert/Mats/", GEN="Assets/ValVert/Gen/", G="Assets/ValVert/Ground/", T="Assets/ValVert/Veg/tex/", P6="Assets/ValVert/Veg/pilot6/";
    static Texture2D Tx(string p)=>AssetDatabase.LoadAssetAtPath<Texture2D>(p);
    static Material Mt(string n)=>AssetDatabase.LoadAssetAtPath<Material>(M+n+".mat");
    public static GameObject Wire(){
        var go=GameObject.Find("ValVertEnvironment")??new GameObject("ValVertEnvironment");
        var clk=go.GetComponent<ValVertClock>()??go.AddComponent<ValVertClock>();
        var sea=go.GetComponent<ValVertSeasons>()??go.AddComponent<ValVertSeasons>();
        var sl=go.GetComponent<ValVertStreetLights>()??go.AddComponent<ValVertStreetLights>();
        sea.foliage.Clear(); sea.overlays.Clear(); sea.wetSurfaces.Clear();
        foreach(var sp in new[]{"tree_small_02","fir_tree_01"}) foreach(var v in new[]{"autumnA","autumnB","green"}){ var m=Mt(sp+"_leaves_"+v); if(m) sea.foliage.Add(new ValVertSeasons.Foliage{mat=m,evergreen=sp=="fir_tree_01",greenTex=Tx(T+sp+"_leaves_green.png"),autumnTex=Tx(T+sp+"_leaves_"+v+".png")}); }
        foreach(var v in new[]{"autumnA","green"}){ var m=Mt("shrub_02_"+v); if(m) sea.foliage.Add(new ValVertSeasons.Foliage{mat=m,greenTex=Tx(T+"shrub_02_leaves_green.png"),autumnTex=Tx(T+"shrub_02_leaves_"+v+".png")}); }
        { var m=Mt("laurel_leaves_acg"); if(m) sea.foliage.Add(new ValVertSeasons.Foliage{mat=m,evergreen=true}); }
        // Ultimate Nature Starter adapter: classify its foliage materials once the package is imported (vendor files stay local)
        foreach(var guid in AssetDatabase.FindAssets("t:Material",new[]{"Assets"})){ var p=AssetDatabase.GUIDToAssetPath(guid); var pl=p.ToLower();
            if(!(pl.Contains("ultimate nature")||pl.Contains("ultimatenature")||pl.Contains("/uns")||pl.Contains("innerverse"))) continue;
            var n=System.IO.Path.GetFileNameWithoutExtension(pl); bool leafy=n.Contains("leaf")||n.Contains("leaves")||n.Contains("foliage")||n.Contains("bush")||n.Contains("branch")||n.Contains("grass")||n.Contains("flower");
            if(!leafy) continue; bool ever=n.Contains("spruce")||n.Contains("pine")||n.Contains("fir")||n.Contains("conifer")||n.Contains("grass");
            sea.foliage.Add(new ValVertSeasons.Foliage{mat=AssetDatabase.LoadAssetAtPath<Material>(p),evergreen=ever}); Debug.Log("[VV] UNS foliage adapter: "+p+(ever?" (evergreen)":" (deciduous)")); }
        sea.overlays.Add(new ValVertSeasons.Overlay{mat=Mt("asphalt_leaves"),baseTex=Tx(G+"asphalt_ww_diff.jpg"),baseNor=Tx(G+"asphalt_ww_nor.jpg"),litterTex=Tx(GEN+"asphalt_leaves.png"),litterNor=Tx(G+"asphalt_02_nor.jpg")});
        sea.overlays.Add(new ValVertSeasons.Overlay{mat=Mt("corr_leaves"),baseTex=Tx(GEN+"corrugated_iron_02_diff_1k.jpg"),litterTex=Tx(GEN+"corrugated_leaves.png")});
        sea.forestLayer=AssetDatabase.LoadAssetAtPath<TerrainLayer>(G+"forest_leaves_02.terrainlayer");
        sea.forestLitterTex=Tx(G+"forest_leaves_02_diff.jpg"); sea.forestLitterNor=Tx(G+"forest_leaves_02_nor.jpg"); sea.forestBaseTex=Tx(P6+"fg02_diff.jpg"); sea.forestBaseNor=Tx(P6+"fg02_nor.jpg");
        foreach(var n in new[]{"asphalt_road","road","parking_lot","asphalt_leaves","leafground","forest_fg02","stair_earth","path_concrete"}){ var m=Mt(n); if(m) sea.wetSurfaces.Add(new ValVertSeasons.Wet{mat=m}); }
        clk.month=10; clk.day=20; clk.hour=11f; clk.timeScale=60f;   // default: autumn 2010, late morning (reference photos)
        sl.Collect(); Debug.Log("[VV] environment wired: foliage "+sea.foliage.Count+" overlays "+sea.overlays.Count+" wet "+sea.wetSurfaces.Count);
        return go;
    }
    // Batch test: open the saved scene, step the clock, log state, and render same-camera frames.
    // unity run /workspace/ValVertGame ... -- -executeMethod ValVertEnvWire.Test
    public static void Test(){
        var scene=UnityEditor.SceneManagement.EditorSceneManager.OpenScene("Assets/Scenes/ValVert.unity");
        var go=Wire(); var clk=go.GetComponent<ValVertClock>(); var sea=go.GetComponent<ValVertSeasons>(); var sl=go.GetComponent<ValVertStreetLights>();
        // 1) clock advances: simulate 30 h in 10-min steps, log transitions
        clk.Set(6,21,0f); float prevAlt=-99; bool lampsOn=clk.SunAltitudeDeg<sl.switchAltitude;
        for(int i=0;i<180;i++){ clk.Advance(1f/6f); clk.Apply(); sl.Tick(); bool on=clk.SunAltitudeDeg<sl.switchAltitude; if(on!=lampsOn){ Debug.Log($"[VV] ENVTEST lamps {(on?"ON":"OFF")} at {clk.Now:yyyy-MM-dd HH:mm} alt={clk.SunAltitudeDeg:F1}"); lampsOn=on; } if(prevAlt<0&&clk.SunAltitudeDeg>=0) Debug.Log($"[VV] ENVTEST sunrise {clk.Now:MM-dd HH:mm} az={clk.SunAzimuthDeg:F0}"); if(prevAlt>=0&&clk.SunAltitudeDeg<0) Debug.Log($"[VV] ENVTEST sunset {clk.Now:MM-dd HH:mm} az={clk.SunAzimuthDeg:F0}"); prevAlt=clk.SunAltitudeDeg; }
        foreach(var d in new[]{(3,20),(6,21),(9,22),(12,21)}){ clk.Set(d.Item1,d.Item2,12.5f); Debug.Log($"[VV] ENVTEST noon {d.Item1:00}-{d.Item2:00} alt={clk.SunAltitudeDeg:F1} utc+{clk.UtcOffset()}"); }
        // 2) renders
        var cam=new GameObject("envcam").AddComponent<Camera>(); cam.fieldOfView=70; sl.viewer=cam.transform; var cams=Camera.allCameras;
        string D="/workspace/valvert/env_runtime/"; System.IO.Directory.CreateDirectory(D);
        var terr=Terrain.activeTerrain;
        void Shot(string n,Vector3 p,Vector3 look,float up){ foreach(var tt in Terrain.activeTerrains){ var tp=tt.transform.position; var sz=tt.terrainData.size; if(p.x>=tp.x&&p.x<=tp.x+sz.x&&p.z>=tp.z&&p.z<=tp.z+sz.z){ terr=tt; break; } } float gy=terr.SampleHeight(p)+terr.transform.position.y; cam.transform.position=new Vector3(p.x,gy+up,p.z); cam.transform.rotation=Quaternion.LookRotation(look); sl.Tick(); Debug.Log($"[VV] ENVSHOT {n} terr={terr.name} pos={cam.transform.position} rot={cam.transform.eulerAngles} parent={(cam.transform.parent?cam.transform.parent.name:"-")} ortho={cam.orthographic} n={Terrain.activeTerrains.Length}");
            var rt=new RenderTexture(1024,576,24); cam.targetTexture=rt; cam.Render(); RenderTexture.active=rt; var t=new Texture2D(1024,576,TextureFormat.RGB24,false); t.ReadPixels(new Rect(0,0,1024,576),0,0); t.Apply(); System.IO.File.WriteAllBytes(D+n,t.EncodeToPNG()); RenderTexture.active=null; cam.targetTexture=null; Object.DestroyImmediate(rt); Object.DestroyImmediate(t); }
        Vector3 pA=new Vector3(369.9f,0,128.0f), lA=new Vector3(0f,0.04f,1f);       // alley below #9
        Vector3 pS=new Vector3(470.5f,0,40.0f), lS=new Vector3(0.25f,-0.08f,1f);    // street with lamps (Aldi car park)
        foreach(var h in new[]{(8.3f,"0818"),(12.5f,"1230"),(18.7f,"1842"),(22.5f,"2230")}){ clk.Set(10,20,h.Item1); sl.Tick(); sea.Tick(); Shot("day_"+h.Item2+"_alley.png",pA,lA,1.6f); Shot("day_"+h.Item2+"_street.png",pS,lS,1.6f); Debug.Log($"[VV] ENVTEST render {h.Item2} alt={clk.SunAltitudeDeg:F1} daylight={clk.Daylight01:F2}"); }
        foreach(var d in new[]{(4,20,"spring"),(7,15,"summer"),(10,25,"autumn"),(1,20,"winter")}){ clk.Set(d.Item1,d.Item2,13f); sea.Tick(); sl.Tick(); Shot("season_"+d.Item3+"_stairs.png",new Vector3(366.3f,0,131.6f),new Vector3(-1f,0.30f,0.18f),1.6f); Shot("season_"+d.Item3+"_alley.png",pA,lA,1.6f); Debug.Log($"[VV] ENVTEST season {d.Item3} leafCover={sea.LeafCover:F2} autumn={sea.Autumn:F2} litter={sea.Litter:F2}"); }
        clk.Set(10,20,11f); sea.Tick(); sl.viewer=null; sl.Tick(); Object.DestroyImmediate(cam.gameObject);
        UnityEditor.SceneManagement.EditorSceneManager.SaveScene(scene); AssetDatabase.SaveAssets();
        Debug.Log("[VV] ENVTEST DONE");
    }
}
