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
    // pristine (shipped) colour/cutoff per foliage material: never re-capture from saved materials (season state drifts into them)
    static bool RestTable(ValVertSeasons.Foliage f,out Color c,out float cut){
        string n=f.mat.name; c=Color.white; cut=0.45f;
        var ef=new System.Collections.Generic.Dictionary<string,float>{{"Ash_Leaves_1",0.116f},{"Ash_Leaves_2",0.575f},{"Birch_Leaves_1",0.11f},{"Birch_Leaves_2",0.105f},{"Birch_Leaves_3",0.081f},{"Chestnut_1",0.088f},{"Chestnut_2",0.088f},{"Chestnut_3",0.088f},{"Spruce_1",0.518f},{"Spruce_2",0.495f}};
        if(ef.TryGetValue(n,out cut)){ c=Color.white; return true; }
        if(n=="UNS_Spruce_Tree_Branch"){ c=new Color(.32f,.43f,.22f,1); cut=0.5f; return true; }
        if(n=="UNS_Bush_Leaves"){ c=new Color(.27f,.37f,.15f,1); cut=0.5f; return true; }
        if(f.colorProp=="_BaseColor"&&!n.StartsWith("UNS")){ c=Color.white; cut=0.45f; return true; }
        return false; }
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
    // pristine (shipped) colour/cutoff per foliage material: never re-capture from saved materials (season state drifts into them)
    static bool RestTable(ValVertSeasons.Foliage f,out Color c,out float cut){
        string n=f.mat.name; c=Color.white; cut=0.45f;
        var ef=new System.Collections.Generic.Dictionary<string,float>{{"Ash_Leaves_1",0.116f},{"Ash_Leaves_2",0.575f},{"Birch_Leaves_1",0.11f},{"Birch_Leaves_2",0.105f},{"Birch_Leaves_3",0.081f},{"Chestnut_1",0.088f},{"Chestnut_2",0.088f},{"Chestnut_3",0.088f},{"Spruce_1",0.518f},{"Spruce_2",0.495f}};
        if(ef.TryGetValue(n,out cut)){ c=Color.white; return true; }
        if(n=="UNS_Spruce_Tree_Branch"){ c=new Color(.32f,.43f,.22f,1); cut=0.5f; return true; }
        if(n=="UNS_Bush_Leaves"){ c=new Color(.27f,.37f,.15f,1); cut=0.5f; return true; }
        if(f.colorProp=="_BaseColor"&&!n.StartsWith("UNS")){ c=Color.white; cut=0.45f; return true; }
        return false; }
    public static GameObject Wire(){
        var go=GameObject.Find("ValVertEnvironment")??new GameObject("ValVertEnvironment");
        var clk=go.GetComponent<ValVertClock>()??go.AddComponent<ValVertClock>();
        var sea=go.GetComponent<ValVertSeasons>()??go.AddComponent<ValVertSeasons>();
        var sl=go.GetComponent<ValVertStreetLights>()??go.AddComponent<ValVertStreetLights>();
        var prevRest=new System.Collections.Generic.Dictionary<Material,(Color,float)>(); foreach(var f in sea.foliage) if(f.mat&&f.hasRest) prevRest[f.mat]=(f.restColor,f.restCut);
        sea.foliage.Clear(); sea.overlays.Clear(); sea.wetSurfaces.Clear();
        foreach(var sp in new[]{"tree_small_02","fir_tree_01"}) foreach(var v in new[]{"autumnA","autumnB","green"}){ var m=Mt(sp+"_leaves_"+v); if(m) sea.foliage.Add(new ValVertSeasons.Foliage{mat=m,evergreen=sp=="fir_tree_01",greenTex=Tx(T+sp+"_leaves_green.png"),autumnTex=Tx(T+sp+"_leaves_"+v+".png")}); }
        foreach(var v in new[]{"autumnA","green"}){ var m=Mt("shrub_02_"+v); if(m) sea.foliage.Add(new ValVertSeasons.Foliage{mat=m,greenTex=Tx(T+"shrub_02_leaves_green.png"),autumnTex=Tx(T+"shrub_02_leaves_"+v+".png")}); }
        { var m=Mt("laurel_leaves_acg"); if(m) sea.foliage.Add(new ValVertSeasons.Foliage{mat=m,evergreen=true}); }
        // European Forests (MysticForge) Leaves_URP shader-graph: properties have generated reference names
        foreach(var sp in new[]{("Ash",new Color(1.2f,1.0f,0.5f),new Color(1.8f,1.05f,0.25f)),("Birch",new Color(1.4f,1.05f,0.35f),new Color(1.95f,1.05f,0.18f)),("Chestnut",new Color(1.3f,0.9f,0.35f),new Color(1.7f,0.72f,0.25f))})
            foreach(var guid in AssetDatabase.FindAssets("t:Material",new[]{"Assets/Realistic Tree/Source/Materials/URP/Leaves/"+sp.Item1})){
                var m=AssetDatabase.LoadAssetAtPath<Material>(AssetDatabase.GUIDToAssetPath(guid)); if(!m) continue;
                sea.foliage.Add(new ValVertSeasons.Foliage{mat=m,colorProp="_BaseColor",texProp="_BaseMap",cutProp="_Cutoff",autumnTint=sp.Item2,autumnTint2=sp.Item3}); }
        // Ultimate Nature Starter adapter: classify its foliage materials once the package is imported (vendor files stay local)
        foreach(var guid in AssetDatabase.FindAssets("t:Material",new[]{"Assets"})){ var p=AssetDatabase.GUIDToAssetPath(guid); var pl=p.ToLower();
            if(!(pl.Contains("ultimate nature")||pl.Contains("ultimatenature")||pl.Contains("/uns")||pl.Contains("innerverse"))) continue;
            var n=System.IO.Path.GetFileNameWithoutExtension(pl); bool leafy=n.Contains("leaf")||n.Contains("leaves")||n.Contains("foliage")||n.Contains("bush")||n.Contains("branch")||n.Contains("grass")||n.Contains("flower");
            if(!leafy) continue; bool ever=n.Contains("spruce")||n.Contains("pine")||n.Contains("fir")||n.Contains("conifer")||n.Contains("grass");
            sea.foliage.Add(new ValVertSeasons.Foliage{mat=AssetDatabase.LoadAssetAtPath<Material>(p),evergreen=ever}); Debug.Log("[VV] UNS foliage adapter: "+p+(ever?" (evergreen)":" (deciduous)")); }
        System.Func<string,Texture[]> steps=nm=>Enumerable.Range(0,5).Select(k=>(Texture)Tx(GEN+"litter/"+nm+"_litter_"+k+".jpg")).ToArray();
        sea.forestSteps=steps("forest");
        sea.overlays.Add(new ValVertSeasons.Overlay{steps=steps("asph"),mat=Mt("asphalt_leaves"),baseTex=Tx(G+"asphalt_ww_diff.jpg"),baseNor=Tx(G+"asphalt_ww_nor.jpg"),litterTex=Tx(GEN+"asphalt_leaves.png"),litterNor=Tx(G+"asphalt_02_nor.jpg")});
        sea.overlays.Add(new ValVertSeasons.Overlay{mat=Mt("corr_leaves"),baseTex=Tx(GEN+"corrugated_iron_02_diff_1k.jpg"),litterTex=Tx(GEN+"corrugated_leaves.png")});
        // Autumn-only leaf carpet on earth paths / stair fill (photos Nov 2026)
        sea.overlays.Add(new ValVertSeasons.Overlay{mat=Mt("stair_earth"),baseTex=Tx(P6+"fg02_diff.jpg"),baseNor=Tx(P6+"fg02_nor.jpg"),litterTex=Tx(G+"forest_leaves_02_diff.jpg"),litterNor=Tx(G+"forest_leaves_02_nor.jpg")});
        sea.overlays.Add(new ValVertSeasons.Overlay{mat=Mt("path_dirt"),baseTex=Tx(G+"stony_dirt_path_diff.jpg"),litterTex=Tx(G+"forest_leaves_02_diff.jpg")});
        sea.overlays.Add(new ValVertSeasons.Overlay{mat=Mt("leafground"),baseTex=Tx(P6+"fg02_diff.jpg"),baseNor=Tx(P6+"fg02_nor.jpg"),litterTex=Tx(G+"forest_leaves_02_diff.jpg"),litterNor=Tx(G+"forest_leaves_02_nor.jpg")});
        sea.forestLayer=AssetDatabase.LoadAssetAtPath<TerrainLayer>(G+"forest_leaves_02.terrainlayer");
        sea.forestLitterTex=Tx(G+"forest_leaves_02_diff.jpg"); sea.forestLitterNor=Tx(G+"forest_leaves_02_nor.jpg"); sea.forestBaseTex=Tx(P6+"fg02_diff.jpg"); sea.forestBaseNor=Tx(P6+"fg02_nor.jpg");
        { var prev=new System.Collections.Generic.Dictionary<TerrainLayer,ValVertSeasons.WetLayer>(); foreach(var x in sea.wetLayers) if(x.layer) prev[x.layer]=x; sea.wetLayers.Clear();
          foreach(var t in Terrain.activeTerrains){ if(!t||t.terrainData==null) continue; foreach(var l in t.terrainData.terrainLayers){ if(!l||sea.wetLayers.Exists(x=>x.layer==l)) continue;
            sea.wetLayers.Add(prev.TryGetValue(l,out var pw)?pw:new ValVertSeasons.WetLayer{layer=l,restMax=l.diffuseRemapMax,restSmooth=l.smoothness}); } } }
        foreach(var n in new[]{"asphalt_road","road","parking_lot","asphalt_leaves","leafground","forest_fg02","stair_earth","path_concrete"}){ var m=Mt(n); if(m) sea.wetSurfaces.Add(new ValVertSeasons.Wet{mat=m}); }
        foreach(var f in sea.foliage) if(f.mat){ f.hasRest=true; if(RestTable(f,out var rc,out var rcut)){ f.restColor=rc; f.restCut=rcut; continue; } if(prevRest.TryGetValue(f.mat,out var pr)){ f.restColor=pr.Item1; f.restCut=pr.Item2; continue; } f.restColor=f.mat.HasProperty(f.colorProp)?f.mat.GetColor(f.colorProp):Color.white; f.restCut=f.mat.HasProperty(f.cutProp)?f.mat.GetFloat(f.cutProp):0.45f; if(f.colorProp=="_BaseColor"&&!f.mat.name.StartsWith("UNS")){ f.restColor=Color.white; f.restCut=0.45f; } }
        clk.month=10; clk.day=20; clk.hour=11f; clk.timeScale=60f;   // default: autumn 2010, late morning (reference photos)
        ValVertWeatherWire.Wire(go);
        var cfg=AssetDatabase.LoadAssetAtPath<ValVertWorldConfig>("Assets/_Project/Settings/World/ValVertWorldConfig.asset");
        ValVertRuntimeBootstrap.EnsureOn(go, cfg);

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

// ---- Weather wiring: snow step textures (CPU blend, cached in Assets/ValVert/Gen/snow), particle materials ----
public static class ValVertWeatherWire {
    const string SN="Assets/ValVert/Gen/snow/";
    static Texture2D snowSrc;
    static Texture2D Load(string p,int max){ var t=new Texture2D(2,2,TextureFormat.RGBA32,false); t.LoadImage(System.IO.File.ReadAllBytes(p)); if(t.width>max){ var rt=RenderTexture.GetTemporary(max,max*t.height/t.width); Graphics.Blit(t,rt); var r=new Texture2D(rt.width,rt.height,TextureFormat.RGBA32,false); RenderTexture.active=rt; r.ReadPixels(new Rect(0,0,rt.width,rt.height),0,0); r.Apply(); RenderTexture.active=null; RenderTexture.ReleaseTemporary(rt); Object.DestroyImmediate(t); t=r; } return t; }
    // returns 5 textures: [0]=original, [1..4]=25/50/75/100 % snow
    public static Texture[] Steps(Texture src,bool slush=false){
        if(!src) return null; var sp=AssetDatabase.GetAssetPath(src); if(string.IsNullOrEmpty(sp)||!System.IO.File.Exists(sp)) return null;
        System.IO.Directory.CreateDirectory(SN);
        // Peel ANY generated snow step suffix so we always key off the original albedo basename
        string baseName=System.IO.Path.GetFileNameWithoutExtension(sp);
        // Prefer resolving to the Ground/ original when src lives under Gen/snow
        if(sp.Replace("\\","/").Contains("/Gen/snow/")){
          string candidate="Assets/ValVert/Ground/"+baseName+".jpg";
          // strip trailing _slush_sN / _sN repeatedly
          string bn=baseName;
          while(true){
            int i=bn.LastIndexOf("_slush_s");
            if(i>0 && i+8<bn.Length && char.IsDigit(bn[i+8])){ bn=bn.Substring(0,i); continue; }
            // _sN but not part of names like ..._diff_1k (no trailing _s digit alone after peel of slush)
            System.Text.RegularExpressions.Match m=System.Text.RegularExpressions.Regex.Match(bn,@"^(.*)_s([1-4])$");
            if(m.Success){ bn=m.Groups[1].Value; continue; }
            break;
          }
          if(bn.EndsWith("_slush")) bn=bn.Substring(0,bn.Length-6);
          baseName=bn;
          foreach(var root in new[]{"Assets/ValVert/Ground/","Assets/ValVert/Veg/pilot6/","Assets/ValVert/Gen/"}){
            foreach(var ext in new[]{".jpg",".png"}){ var c=root+baseName+ext; if(System.IO.File.Exists(c)){ sp=c; break; } }
          }
        } else {
          string bn=baseName;
          while(true){
            int i=bn.LastIndexOf("_slush_s");
            if(i>0 && i+8<bn.Length && char.IsDigit(bn[i+8])){ bn=bn.Substring(0,i); continue; }
            System.Text.RegularExpressions.Match m=System.Text.RegularExpressions.Regex.Match(bn,@"^(.*)_s([1-4])$");
            if(m.Success){ bn=m.Groups[1].Value; continue; }
            break;
          }
          if(bn.EndsWith("_slush")) bn=bn.Substring(0,bn.Length-6);
          baseName=bn;
        }
        string key=baseName+(slush?"_slush":"");
        var outp=new string[5]; outp[0]=sp; bool need=false; for(int k=1;k<5;k++){ outp[k]=SN+key+"_s"+k+".jpg"; if(!System.IO.File.Exists(outp[k])) need=true; }
        if(need){
            var b=Load(sp,1024); if(!snowSrc) snowSrc=Load("Assets/ValVert/Ground/cc0/Snow006_col.jpg",1024);
            int w=b.width,h=b.height; var bp=b.GetPixels(); var res=new Color[w*h];
            float ox=Mathf.Abs(key.GetHashCode()%97); var spx=snowSrc.GetPixels(); int sw=snowSrc.width, shh=snowSrc.height;
            for(int k=1;k<5;k++){ float cov=k/4f; float thr=1f-cov;
                for(int y=0;y<h;y++)for(int x=0;x<w;x++){
                    float n=0.65f*Mathf.PerlinNoise(ox+x*6f/w,y*6f/h)+0.35f*Mathf.PerlinNoise(ox+x*40f/w,y*40f/h);
                    float a=k==4?1f:Mathf.Clamp01((n-thr)*10f+0.5f); if(slush) a*=0.85f;
                    var sc=spx[(y*shh/h)*sw+(x*sw/w)]; if(slush) sc=Color.Lerp(sc*0.72f,bp[y*w+x],0.25f);
                    res[y*w+x]=Color.Lerp(bp[y*w+x],sc,a); }
                var o=new Texture2D(w,h,TextureFormat.RGB24,false); o.SetPixels(res); o.Apply(); System.IO.File.WriteAllBytes(outp[k],o.EncodeToJPG(85)); Object.DestroyImmediate(o); }
            Object.DestroyImmediate(b); AssetDatabase.Refresh(); }
        var arr=new Texture[5];
        for(int k=0;k<5;k++){ arr[k]=AssetDatabase.LoadAssetAtPath<Texture2D>(outp[k]);
          if(arr[k]==null && System.IO.File.Exists(outp[k])){ AssetDatabase.ImportAsset(outp[k]); arr[k]=AssetDatabase.LoadAssetAtPath<Texture2D>(outp[k]); }
          if(arr[k]==null) Debug.LogWarning("[VV] Steps missing tex "+outp[k]); }
        return arr;
    }
    static float Fbm(float x,float y){ return 0.55f*Mathf.PerlinNoise(x,y)+0.3f*Mathf.PerlinNoise(x*2.7f+11f,y*2.7f+7f)+0.15f*Mathf.PerlinNoise(x*7.3f+3f,y*7.3f+5f); }
    static Texture2D Readable(Texture src,int max){ int w=Mathf.Min(max,src.width), h=Mathf.Max(1,src.height*w/src.width); var rt=RenderTexture.GetTemporary(w,h,0,RenderTextureFormat.ARGB32,RenderTextureReadWrite.sRGB); Graphics.Blit(src,rt); var prev=RenderTexture.active; RenderTexture.active=rt; var r=new Texture2D(w,h,TextureFormat.RGBA32,false); r.ReadPixels(new Rect(0,0,w,h),0,0); r.Apply(); RenderTexture.active=prev; RenderTexture.ReleaseTemporary(rt); return r; }
    // snow material for a shell: RGB snow, alpha = noise (bark) or leaf-mask*noise (foliage) so the cutoff ramp grows patches progressively
    static Material ShellMat(Material src,bool leaf){
        System.IO.Directory.CreateDirectory(SN); string key="shell_"+src.name.Replace(" ","_"); string tp=SN+key+".png", mp=SN+key+".mat";
        if(!System.IO.File.Exists(tp)){
            Texture lt=leaf&&src.HasProperty("_BaseMap")?src.GetTexture("_BaseMap"):null; if(leaf&&!lt) return null;
            Texture2D L=lt?Readable(lt,512):null; int w=L?L.width:256, h=L?L.height:256; var lp=L?L.GetPixels():null; var px=new Color[w*h]; float sc=leaf?5f:8f;
            for(int y=0;y<h;y++)for(int x=0;x<w;x++){ float n=Fbm(x*sc/w,y*sc/h); float a=0.08f+0.9f*Mathf.Clamp01((n-0.2f)/0.6f); if(lp!=null&&lp[y*w+x].a<0.45f) a=0f; float sh=0.9f+0.1f*Mathf.PerlinNoise(x*30f/w,y*30f/h); px[y*w+x]=new Color(0.93f*sh,0.95f*sh,0.98f*sh,a); }
            var o=new Texture2D(w,h,TextureFormat.RGBA32,false); o.SetPixels(px); o.Apply(); System.IO.File.WriteAllBytes(tp,o.EncodeToPNG()); Object.DestroyImmediate(o); if(L) Object.DestroyImmediate(L);
            AssetDatabase.ImportAsset(tp); var ti=(TextureImporter)AssetImporter.GetAtPath(tp); ti.alphaIsTransparency=true; ti.mipmapEnabled=true; ti.mipMapsPreserveCoverage=true; ti.alphaTestReferenceValue=0.5f; ti.SaveAndReimport(); }
        var m=AssetDatabase.LoadAssetAtPath<Material>(mp); if(!m){ m=new Material(Shader.Find("Universal Render Pipeline/Lit")); AssetDatabase.CreateAsset(m,mp); }
        m.SetTexture("_BaseMap",AssetDatabase.LoadAssetAtPath<Texture2D>(tp)); if(src.HasProperty("_BaseMap")&&leaf){ m.SetTextureScale("_BaseMap",src.GetTextureScale("_BaseMap")); m.SetTextureOffset("_BaseMap",src.GetTextureOffset("_BaseMap")); }
        m.SetColor("_BaseColor",new Color(1.12f,1.13f,1.16f)); m.SetFloat("_AlphaClip",1); m.EnableKeyword("_ALPHATEST_ON"); m.SetFloat("_Cutoff",0.98f); m.SetFloat("_Cull",0); m.SetFloat("_Smoothness",0.18f); m.SetFloat("_Metallic",0);
        m.SetOverrideTag("RenderType","TransparentCutout"); m.renderQueue=2450; EditorUtility.SetDirty(m); return m; }
    static Material OvercastSky(){
        string tp="Assets/ValVert/Gen/sky_overcast.png", mp="Assets/ValVert/Mats/sky_overcast.mat";
        if(!System.IO.File.Exists(tp)){ int w=2048,h=1024; var px=new Color[w*h];
            for(int y=0;y<h;y++){ float el=(y/(float)h-0.5f)*Mathf.PI; for(int x=0;x<w;x++){ float u=x/(float)w; float n=Fbm(Mathf.Cos(u*2*Mathf.PI)*3f+3f+Mathf.Sin(el)*2f, Mathf.Sin(u*2*Mathf.PI)*3f+y*6f/h);
                float top=Mathf.Clamp01(Mathf.Sin(el)); float v=el<0?0.55f:Mathf.Lerp(0.80f,0.66f,top)+(n-0.5f)*0.12f*(0.3f+top); px[y*w+x]=new Color(v,v*1.005f,v*1.02f,1); } }
            var o=new Texture2D(w,h,TextureFormat.RGB24,false); o.SetPixels(px); o.Apply(); System.IO.File.WriteAllBytes(tp,o.EncodeToPNG()); Object.DestroyImmediate(o); AssetDatabase.ImportAsset(tp); }
        var m=AssetDatabase.LoadAssetAtPath<Material>(mp); if(!m){ m=new Material(Shader.Find("Skybox/Panoramic")); AssetDatabase.CreateAsset(m,mp); }
        m.SetTexture("_MainTex",AssetDatabase.LoadAssetAtPath<Texture2D>(tp)); m.SetColor("_Tint",new Color(.5f,.5f,.5f)); m.SetFloat("_Exposure",0.9f); EditorUtility.SetDirty(m); return m; }
    static Material PMat(string n,bool rain){
        string p="Assets/ValVert/Mats/"+n+".mat"; var m=AssetDatabase.LoadAssetAtPath<Material>(p);
        if(!m){ m=new Material(Shader.Find("Universal Render Pipeline/Particles/Unlit")); AssetDatabase.CreateAsset(m,p); }
        string tp="Assets/ValVert/Gen/"+n+"_tex.png";
        if(!System.IO.File.Exists(tp)){ var t=new Texture2D(64,64,TextureFormat.RGBA32,false); for(int y=0;y<64;y++)for(int x=0;x<64;x++){ float dx=(x-31.5f)/32f, dy=(y-31.5f)/32f; float a=rain?Mathf.Clamp01(1f-Mathf.Abs(dx)*3f)*Mathf.Clamp01(1f-Mathf.Abs(dy)):Mathf.Clamp01(1f-Mathf.Sqrt(dx*dx+dy*dy)); t.SetPixel(x,y,new Color(1,1,1,a*a)); } t.Apply(); System.IO.File.WriteAllBytes(tp,t.EncodeToPNG()); AssetDatabase.ImportAsset(tp); var ti=(TextureImporter)AssetImporter.GetAtPath(tp); ti.alphaIsTransparency=true; ti.SaveAndReimport(); }
        m.SetTexture("_BaseMap",AssetDatabase.LoadAssetAtPath<Texture2D>(tp)); m.SetFloat("_Surface",1); m.SetFloat("_Blend",0);
        m.SetFloat("_SrcBlend",(float)UnityEngine.Rendering.BlendMode.SrcAlpha); m.SetFloat("_DstBlend",(float)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha); m.SetFloat("_ZWrite",0);
        m.EnableKeyword("_SURFACE_TYPE_TRANSPARENT"); m.renderQueue=3000; m.SetColor("_BaseColor",Color.white); EditorUtility.SetDirty(m); return m; }
    public static void Wire(GameObject go){
        var w=go.GetComponent<ValVertWeather>()??go.AddComponent<ValVertWeather>(); w.snowMats.Clear(); w.snowLayers.Clear(); w.snowTints.Clear();
        w.rainMat=PMat("vv_rain",true); w.snowMat=PMat("vv_snow",false);
        string M="Assets/ValVert/Mats/";
        void Mat(string n,float f,bool seasonal=false,bool slush=false){ var m=AssetDatabase.LoadAssetAtPath<Material>(M+n+".mat"); if(!m||!m.HasProperty("_BaseMap")) return; var st=Steps(m.GetTexture("_BaseMap"),slush); if(st!=null) w.snowMats.Add(new ValVertWeather.SnowTex{mat=m,steps=st,factor=f,seasonal=seasonal,orig=st[0]}); }
        foreach(var n in new[]{"leafground","forest_fg02","stair_earth","bed_soil","path_dirt","school_yard","roof","roof_flat","roof_gravel","roof_gravel2","roof_gravel3","roof_tiles_red","roof_terracotta","roof_brown","roof_slate","roof_gym"}) Mat(n,1f);
        Mat("path_concrete",0.8f); Mat("kerb",0.7f);
        foreach(var n in new[]{"asphalt_road","road","parking_lot"}) Mat(n,1f,false,false);
        Mat("asphalt_leaves",1f,true,false);
        // Aldi lot verges / facade strip / store cladding so roofs & verges go white
        foreach(var n in new[]{"aldi_strip_grass","store_wall","cladding","clad_white","clad_grey","corr_leaves"}) Mat(n,1f);
        foreach(var t in Terrain.activeTerrains) foreach(var l in t.terrainData.terrainLayers){ if(!l||w.snowLayers.Exists(x=>x.layer==l)) continue;
            bool asph=l.name.Contains("asphalt"); bool forest=l.name.Contains("forest");
            var src=forest?AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/ValVert/Veg/pilot6/fg02_diff.jpg"):l.diffuseTexture;
            var st=Steps(src,false); if(st!=null){ /* restore layer to clean base before review/play mutates it */ if(st[0]) l.diffuseTexture=(Texture2D)st[0]; w.snowLayers.Add(new ValVertWeather.SnowLayer{layer=l,steps=st,factor=1f,orig=st[0]}); } }
        // Colour-tint snow for materials that have no albedo map (metal roofs, EV posts, etc.)
        w.snowTints.Clear();
        void Tint(string n,float amt=0.85f){ var m=AssetDatabase.LoadAssetAtPath<Material>(M+n+".mat"); if(!m||!m.HasProperty("_BaseColor")) return;
          w.snowTints.Add(new ValVertWeather.SnowTint{mat=m,snowColor=new Color(0.93f,0.95f,0.98f),amount=amt}); }
        foreach(var n in new[]{"roof_shelter","aldi_ev","aldi_ev_head","metal_grey","bollard_m","aldi_bollard"}) Tint(n);
        w.shellSrcs.Clear();
        System.Action<Material,bool> Sh=(m,leaf)=>{ if(!m||w.shellSrcs.Exists(x=>x.src==m)) return; var sm=ShellMat(m,leaf); if(sm) w.shellSrcs.Add(new ValVertWeather.ShellSrc{src=m,snow=sm,leaf=leaf,minUp=leaf?0.05f:0.3f,offset=leaf?0.02f:0.025f,steep=m.name.StartsWith("laurel")?0.32f:0.5f}); };
        foreach(var guid in AssetDatabase.FindAssets("t:Material",new[]{"Assets"})){ var p=AssetDatabase.GUIDToAssetPath(guid); var n=System.IO.Path.GetFileNameWithoutExtension(p).ToLower(); var pl=p.ToLower();
            if(pl.Contains("/demo")||pl.Contains("/gen/snow")||pl.Contains("hdrp")) continue;
            bool barkEF=pl.Contains("realistic tree/source/materials/urp/bark/");
            bool barkVV=pl.StartsWith("assets/valvert/mats/")&&(n.EndsWith("_trunk")||n.EndsWith("_branch")||n.EndsWith("_bark"));
            bool unsPal=n=="uns_color_palette";
            bool leaf=n=="uns_spruce_tree_branch"||n=="uns_bush_leaves"||n=="laurel_leaves_acg"||n.StartsWith("shrub_02_")||n.StartsWith("fir_tree_01_leaves")||n=="boxwood";
            bool car=pl.Contains("/cars")||n.Contains("carpaint")||n.Contains("car_paint")||n.StartsWith("v2_car")||n.Contains("body_paint");
            if(barkEF||barkVV||unsPal) Sh(AssetDatabase.LoadAssetAtPath<Material>(p),false);
            else if(leaf) Sh(AssetDatabase.LoadAssetAtPath<Material>(p),true);
            else if(car) Sh(AssetDatabase.LoadAssetAtPath<Material>(p),false); }
        // snow-shell variants of the terrain tree prototypes (derived from vendor meshes -> git-ignored folder)
        w.treeVariants.Clear(); { string TD="Assets/ValVert/Gen/snowtrees/"; System.IO.Directory.CreateDirectory(TD);
            var map=new System.Collections.Generic.Dictionary<Material,ValVertWeather.ShellSrc>(); foreach(var x in w.shellSrcs) map[x.src]=x;
            var cache=new System.Collections.Generic.Dictionary<(Mesh,int,Vector3Int),Mesh>(); var done=new System.Collections.Generic.HashSet<GameObject>();
            foreach(var t in Terrain.activeTerrains) foreach(var p in t.terrainData.treePrototypes){ if(!p.prefab||!done.Add(p.prefab)) continue;
                var inst=(GameObject)Object.Instantiate(p.prefab); var list=new System.Collections.Generic.List<Renderer>();
                ValVertWeather.AddShells(inst.GetComponentsInChildren<MeshRenderer>(true),inst.GetComponentsInChildren<LODGroup>(true),map,cache,list,true);
                if(list.Count==0){ Object.DestroyImmediate(inst); continue; }
                foreach(var r in list) r.enabled=true;
                // terrain trees without a LODGroup draw only the root mesh: wrap original + shells in a single-LOD group
                if(!inst.GetComponent<LODGroup>()){ var lg=inst.AddComponent<LODGroup>(); lg.SetLODs(new[]{ new LOD(0.004f,inst.GetComponentsInChildren<Renderer>(true)) }); lg.RecalculateBounds(); }
                string mp=TD+p.prefab.name+"_snowmesh.asset"; AssetDatabase.DeleteAsset(mp); bool first=true;
                foreach(var r in list){ var m=r.GetComponent<MeshFilter>().sharedMesh; if(AssetDatabase.Contains(m)) continue; if(first){ AssetDatabase.CreateAsset(m,mp); first=false; } else AssetDatabase.AddObjectToAsset(m,mp); }
                if(!first) AssetDatabase.SaveAssetIfDirty(AssetDatabase.LoadMainAssetAtPath(mp));
                var sp=PrefabUtility.SaveAsPrefabAsset(inst,TD+p.prefab.name+"_snow.prefab"); Object.DestroyImmediate(inst);
                w.treeVariants.Add(new ValVertWeather.TreeVariant{orig=p.prefab,snow=sp}); } }
        // overcast sky deck for snowfall / rain / fog
        var clk=go.GetComponent<ValVertClock>(); if(clk){ clk.overcastSkybox=OvercastSky();
            var bp="Assets/ValVert/Mats/sky_blend.mat"; var bm=AssetDatabase.LoadAssetAtPath<Material>(bp); var sh=Shader.Find("ValVert/SkyBlend");
            if(sh){ if(!bm){ bm=new Material(sh); AssetDatabase.CreateAsset(bm,bp); } var day=AssetDatabase.LoadAssetAtPath<Material>("Assets/ValVert/Mats/sky.mat");
                bm.SetTexture("_TexA",day.GetTexture("_MainTex")); bm.SetTexture("_TexB",clk.overcastSkybox.GetTexture("_MainTex")); bm.SetFloat("_Rotation",day.GetFloat("_Rotation")); EditorUtility.SetDirty(bm); clk.blendSkybox=bm; clk.daySkybox=day; } }
        Debug.Log($"[VV] weather wired: snowMats {w.snowMats.Count} snowLayers {w.snowLayers.Count} shellSrcs {w.shellSrcs.Count} treeVariants {w.treeVariants.Count}");
    }
}
