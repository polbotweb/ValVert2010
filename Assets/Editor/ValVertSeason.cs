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
