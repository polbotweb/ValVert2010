using UnityEngine; using UnityEditor; using UnityEditor.SceneManagement; using System.IO; using System.Linq; using System.Collections.Generic;
public static class ValVertSetup {
    const string SRC="/workspace/valvert/clean/"; const string SHOTS="/workspace/valvert/unity_shots/";
    const int R=2049;
    static void L(string s){ Debug.Log("[VV] "+s); }
    public static void Build(){
        string fbx="Assets/ValVert/valvert_clean.fbx";
        var mi=(ModelImporter)AssetImporter.GetAtPath(fbx);
        mi.globalScale=1; mi.useFileUnits=true; mi.bakeAxisConversion=false; mi.addCollider=true; mi.importAnimation=false; mi.SaveAndReimport();
        foreach(var d in new[]{"Assets/ValVert/Mats","Assets/ValVert/Prefabs"}) AssetDatabase.DeleteAsset(d); foreach(var g in AssetDatabase.FindAssets("t:TerrainLayer",new[]{"Assets/ValVert/Ground"})) AssetDatabase.DeleteAsset(AssetDatabase.GUIDToAssetPath(g));
        var scene=EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        var root=(GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(fbx));
        root.transform.position=Vector3.zero; root.transform.rotation=Quaternion.Euler(0,180,0); // FBX arrives rotated 180 vs heightmap (README step a4)
        foreach(var t in root.GetComponentsInChildren<Transform>(true)){ var r=t.GetComponent<Renderer>(); L("node "+t.name+(r?" bounds "+r.bounds:""));}
        // heights
        byte[] raw=File.ReadAllBytes(SRC+"heightmap_v3.raw");
        float[,] h=new float[R,R], hf=new float[R,R];
        for(int y=0;y<R;y++)for(int x=0;x<R;x++){ float v=(raw[(y*R+x)*2]|(raw[(y*R+x)*2+1]<<8))/65535f; h[y,x]=v; hf[R-1-y,x]=v; }
        var td=new TerrainData(); td.heightmapResolution=R; td.size=new Vector3(1064f,80.056f,709.293f);
        td.SetHeights(0,0,h);
        var tgo=Terrain.CreateTerrainGameObject(td); tgo.name="ValVertTerrain"; tgo.transform.position=new Vector3(-532f,0.002f,-354.646f);
        var terr=tgo.GetComponent<Terrain>();
        // alignment: compare FBX Terrain mesh vertices (world space) against both heightmap orientations
        var tmf=root.GetComponentsInChildren<MeshFilter>(true).First(m=>m.name=="Terrain");
        var tv=tmf.sharedMesh.vertices; var tm=tmf.transform.localToWorldMatrix;
        float eN=0,eF=0; int cnt=0;
        for(int k=0;k<tv.Length;k+=7){ var w=tm.MultiplyPoint3x4(tv[k]);
            int gx=Mathf.Clamp(Mathf.RoundToInt((w.x+532f)/1064f*(R-1)),0,R-1), gz=Mathf.Clamp(Mathf.RoundToInt((w.z+354.646f)/709.293f*(R-1)),0,R-1);
            eN+=Mathf.Abs(h[gz,gx]*80.056f+0.002f-w.y); eF+=Mathf.Abs(hf[gz,gx]*80.056f+0.002f-w.y); cnt++; }
        eN/=cnt; eF/=cnt;
        L($"align mean abs err (m) over {cnt} FBX terrain verts: normal={eN:F2} flipped={eF:F2}");
        if(eF<eN){ td.SetHeights(0,0,hf); L("FLIPPED heightmap"); } else L("kept heightmap unflipped");
        string tdPath="Assets/ValVert/ValVertTerrain.asset"; AssetDatabase.CreateAsset(td,tdPath);
        // hide area layers
        foreach(var t in root.GetComponentsInChildren<Transform>(true)) if(t!=root.transform && Hidden(t.name)) { t.gameObject.SetActive(false); L("hidden "+t.name);}
        foreach(var t in root.GetComponentsInChildren<Transform>(true)) if(t.name=="Water"){ t.gameObject.SetActive(false); L("hidden old Water (replaced by v3 stream ribbon)"); }
        foreach(var nm in new[]{"Buildings","Paths","Roads","Courts"}){
            var mf=root.GetComponentsInChildren<MeshFilter>(true).FirstOrDefault(m=>m.name==nm); if(!mf) continue;
            var vv=mf.sharedMesh.vertices; var mm=mf.transform.localToWorldMatrix; var d=new List<float>();
            for(int k=0;k<vv.Length;k+=3){ var w=mm.MultiplyPoint3x4(vv[k]); d.Add(w.y-(terr.SampleHeight(w)+tgo.transform.position.y)); }
            d.Sort(); int n=d.Count; L($"{nm}: vertex height above Unity terrain n={n} p10={d[n/10]:F2} median={d[n/2]:F2} p90={d[n*9/10]:F2} min={d[0]:F2}");
        }
        ValVertDress.PaintPBR(td); ValVertDress.Surfaces(root); ValVertWoods.BuildEast(terr); ValVertDress.Vegetation(terr); ValVertWoods.Meshes(null); foreach(var t in root.GetComponentsInChildren<Transform>(true)) if(t.name=="Buildings"){ t.gameObject.SetActive(false); L("hidden FBX Buildings (replaced by procedural V3 buildings)"); } ValVertWoods.Meshes(null,"east_meshes.json","V3_East"); ValVertWoods.Cars(); ValVertWoods.Grass(terr); ValVertDress.SkyPost();
        // light
        var lg=new GameObject("Sun"); var li=lg.AddComponent<Light>(); li.type=LightType.Directional; li.color=new Color(1f,0.86f,0.7f); li.intensity=1.35f; li.shadows=LightShadows.Soft;
        lg.transform.rotation=Quaternion.Euler(18f,230f,0f);
        RenderSettings.ambientMode=UnityEngine.Rendering.AmbientMode.Trilight; RenderSettings.ambientSkyColor=new Color(.68f,.72f,.82f); RenderSettings.ambientEquatorColor=new Color(.58f,.55f,.5f); RenderSettings.ambientGroundColor=new Color(.25f,.22f,.18f);
        RenderSettings.fog=true; RenderSettings.fogColor=new Color(.75f,.72f,.68f); RenderSettings.fogMode=FogMode.ExponentialSquared; RenderSettings.fogDensity=0.0015f;
        // player
        float gy=terr.SampleHeight(new Vector3(0,0,-56.15f))+tgo.transform.position.y;
        L($"spawn ground y={gy:F2} (requested 34.6)");
        var p=new GameObject("Player"); p.transform.position=new Vector3(0,gy+0.05f,-56.15f); p.transform.rotation=Quaternion.Euler(0,71,0);
        var cc=p.AddComponent<CharacterController>(); cc.height=1.8f; cc.radius=0.3f; cc.center=new Vector3(0,0.9f,0);
        var cg=new GameObject("Main Camera"); cg.tag="MainCamera"; cg.transform.SetParent(p.transform,false); cg.transform.localPosition=new Vector3(0,1.7f,0);
        var cam=cg.AddComponent<Camera>(); cam.nearClipPlane=0.1f; cam.farClipPlane=3000; cg.AddComponent<AudioListener>();
        var fps=p.AddComponent<SimpleFPS>(); fps.cam=cg.transform;
        EditorSceneManager.SaveScene(scene,"Assets/Scenes/ValVert.unity");
        EditorBuildSettings.scenes=new[]{new EditorBuildSettingsScene("Assets/Scenes/ValVert.unity",true)};
        AssetDatabase.SaveAssets();
        Directory.CreateDirectory(SHOTS);
        ValVertDress.Post(cam); var oc=new GameObject("ShotCam").AddComponent<Camera>(); oc.farClipPlane=5000; oc.nearClipPlane=0.1f; ValVertDress.Post(oc);
        System.Action<string,Vector3,Vector3> sh=(n,pos,look)=>{ var tt2=(pos.x>532f&&ValVertWoods.East)?ValVertWoods.East:terr; float gy2=tt2.SampleHeight(pos)+0.002f; oc.transform.position=new Vector3(pos.x,gy2+1.7f,pos.z); oc.transform.rotation=Quaternion.LookRotation(look); oc.fieldOfView=60; Shot(oc,SHOTS+n); };
        terr.Flush(); Shot(cam,SHOTS+"v3_spawn_path.png"); Shot(cam,SHOTS+"v3_spawn_path.png");
        System.Action<string,Vector3,Vector3,float> shH=(n,pos,look,up)=>{ float gy2=terr.SampleHeight(pos)+tgo.transform.position.y; oc.transform.position=new Vector3(pos.x,gy2+up,pos.z); oc.transform.rotation=Quaternion.LookRotation(look); oc.fieldOfView=60; Shot(oc,SHOTS+n); };
        sh("v3_woods_stream.png",new Vector3(10f,0,-72.7f)+new Vector3(-1.2f,0,2.4f),new Vector3(-0.9f,-0.12f,-0.42f));
        sh("v3_woods_stream_b.png",new Vector3(-120f,0,-159.5f)+new Vector3(-1.5f,0,2.6f),new Vector3(0.88f,-0.1f,0.46f));
        sh("v3_woods_path.png",new Vector3(37.4f,0,-41.56f),new Vector3(-0.936f,-0.05f,-0.352f));
        sh("v3_piranha_head.png",new Vector3(372f,0,-2f),new Vector3(1f,0.07f,-0.02f));
        sh("v3_piranha_dam_zoom.png",new Vector3(400f,0,-4f),new Vector3(1f,0.08f,0.03f));
        shH("v3_piranha_wide.png",new Vector3(300f,0,-48f),new Vector3(0.8f,-0.32f,0.5f),9f);
        sh("v3_piranha_head2.png",new Vector3(409f,0,-7f),new Vector3(1f,0.1f,0.37f));
        sh("v3_piranha_tail2.png",new Vector3(436f,0,-5f),new Vector3(-0.15f,0.1f,1f));
        sh("v3_piranha_stairs.png",new Vector3(460f,0,14f),new Vector3(-0.9f,0.08f,-0.42f));
        sh("v3_piranha_basin_whole.png",new Vector3(418f,0,-12f),new Vector3(0.73f,0.12f,0.69f));
        sh("v3_piranha_tail.png",new Vector3(439f,0,-4f),new Vector3(-0.25f,0.1f,1f));
        shH("v3_grand_frais.png",new Vector3(452f,0,108f),new Vector3(0f,0.08f,1f),1.7f); sh("v3_boulangerie.png",new Vector3(249.0f,0,313.4f),new Vector3(-0.88f,0.1f,0.47f)); sh("v3_assedic.png",new Vector3(262.4f,0,269.6f),new Vector3(0.58f,0.1f,-0.81f)); shH("v3_assedic_y.png",new Vector3(222f,0,300f),new Vector3(0.47f,-0.40f,-0.79f),30f); shH("v3_gf_parking_lower.png",new Vector3(450f,0,112f),new Vector3(0.83f,0.02f,0.55f),1.6f); shH("v3_gf_parking_deck.png",new Vector3(408f,0,116.5f),new Vector3(1f,-0.05f,0.02f),1.7f); sh("v3_maison_orme.png",new Vector3(346f,0,213f),new Vector3(-0.05f,0.1f,-1f)); sh("v3_168_binstore.png",new Vector3(368.2f,0,126f),new Vector3(-0.42f,0.12f,0.9f)); sh("v3_168_logstairs.png",new Vector3(353.5f,0,145.2f),new Vector3(0.95f,-0.12f,-0.15f)); shH("v3_168_ruelle_haut.png",new Vector3(371.5f,0,121f),new Vector3(-0.05f,-0.45f,1f),10f); shH("v3_stepped_from168.png",new Vector3(371f,0,110.5f),new Vector3(-0.1f,-0.3f,-1f),9f); sh("v3_orme_from_path.png",new Vector3(338f,0,222f),new Vector3(0.05f,0.05f,-1f));
        sh("v3_aldi.png",new Vector3(488f,0,58f),new Vector3(-1f,0.06f,0.15f));
        sh("v3_trois_fontaines.png",new Vector3(452f,0,-14f),new Vector3(0.45f,0.03f,0.9f));
        shH("v3_st_feux.png",new Vector3(296f,0,171f),new Vector3(0.85f,-0.2f,0.5f),5f);
        sh("v3_st_abribus.png",new Vector3(503f,0,39f),new Vector3(0.85f,-0.03f,0.53f));
        sh("v3_st_gifi.png",new Vector3(505.6f,0,146.7f),new Vector3(0.57f,0.17f,0.82f));
        sh("v3_st_armataffet.png",new Vector3(498f,0,188.3f),new Vector3(-1f,0.22f,0f));
        sh("v3_st_parking_ouest.png",new Vector3(-468f,0,-150f),new Vector3(-1f,-0.1f,0.05f));
        sh("v3_st_ouest.png",new Vector3(-468f,0,-150f),new Vector3(-1f,-0.02f,0.05f));
        sh("v3_st_rue.png",new Vector3(243f,0,300f),new Vector3(0.3f,-0.05f,-1f));
        shH("v3_st_air.png",new Vector3(300f,0,120f),new Vector3(0.5f,-0.55f,0.65f),55f);
        sh("v3_lidl.png",new Vector3(592f,0,-70f),new Vector3(-1f,0.05f,0.18f));
        sh("v3_bld_houses.png",new Vector3(589.5f,0,129.9f),new Vector3(0.85f,0.04f,-0.52f)); sh("v3_bld_houses2.png",new Vector3(117f,0,-119f),new Vector3(0.49f,0.04f,-0.87f));
        sh("v3_bld_apartments.png",new Vector3(37.4f,0,146f),new Vector3(-1f,0.2f,0f));
        sh("v3_ecole_barral.png",new Vector3(348f,0,212f),new Vector3(-0.05f,0.08f,1f));
        sh("v3_barral_stade.png",new Vector3(386f,0,221f),new Vector3(-0.35f,0.08f,1f));
        sh("v3_barral_preau.png",new Vector3(379f,0,239f),new Vector3(-1f,0.06f,0f));
        sh("v3_barral_gymnase.png",new Vector3(412f,0,300f),new Vector3(-0.8f,0.12f,0.6f));
        sh("v3_danger_dam.png",new Vector3(417f,0,-12f),new Vector3(0.6f,-0.05f,0.4f));
        float fd=RenderSettings.fogDensity; RenderSettings.fogDensity=0.0006f; oc.fieldOfView=60;
        oc.transform.position=new Vector3(-60,110,-200); oc.transform.LookAt(new Vector3(0,30,-50)); Shot(oc,SHOTS+"v3_woods_overview.png");
        oc.transform.position=new Vector3(392,175,-60); oc.transform.LookAt(new Vector3(392,6,-28)); Shot(oc,SHOTS+"v3_piranha_aerial.png");
        RenderSettings.fogDensity=0.00012f; oc.transform.position=new Vector3(220,820,-980); oc.transform.LookAt(new Vector3(220,0,-20)); Shot(oc,SHOTS+"v3_overview_full.png"); RenderSettings.fogDensity=0.0006f;
        oc.transform.position=new Vector3(400,70,-50); oc.transform.LookAt(new Vector3(435,5,10)); Shot(oc,SHOTS+"v3_aerial_vovray.png");
        { var sun=GameObject.Find("Sun").GetComponent<Light>(); float si=sun.intensity, ai=RenderSettings.ambientIntensity; sun.intensity=2.0f; RenderSettings.ambientIntensity=1.6f; oc.transform.position=new Vector3(445,95,170); oc.transform.LookAt(new Vector3(370,16,255)); Shot(oc,SHOTS+"v3_aerial_barral.png"); sun.intensity=si; RenderSettings.ambientIntensity=ai; }
        oc.transform.position=new Vector3(60,130,-170); oc.transform.LookAt(new Vector3(-20,15,0)); Shot(oc,SHOTS+"v3_aerial_woods.png");
        oc.transform.position=new Vector3(555,75,50); oc.transform.LookAt(new Vector3(605,10,104)); Shot(oc,SHOTS+"v3_aerial_houses.png");
        { var tA=Terrain.activeTerrain; float hA=tA.SampleHeight(new Vector3(-25,0,105))+tA.transform.position.y; float hB=tA.SampleHeight(new Vector3(7,0,146))+tA.transform.position.y; oc.transform.position=new Vector3(-25,hA+38,105); oc.transform.LookAt(new Vector3(7,hB+8,146)); } Shot(oc,SHOTS+"v3_aerial_apts.png");
        RenderSettings.fogDensity=fd; Object.DestroyImmediate(oc.gameObject);
        foreach(var kv in ValVertDress.Counts) L("COUNT "+kv.Key+" = "+kv.Value);
        EditorSceneManager.SaveScene(scene,"Assets/Scenes/ValVert.unity"); AssetDatabase.SaveAssets();
        L("DONE");
    }
    static bool Hidden(string n){ string l=n.ToLower(); return l=="land"||l=="urban"||l.Contains("terrain")||l.Contains("forest")||l.Contains("park")||l.Contains("managed")||l.Contains("agricult")||l=="base"||l.StartsWith("land")||l.StartsWith("base"); }
    static float AlignErr(Terrain terr,List<MeshCollider> cols,bool flip,float[,]h,float[,]hf){
        Physics.SyncTransforms(); float s=0; int n=0; var a=flip?hf:h;
        for(int i=1;i<20;i++)for(int j=1;j<20;j++){ float x=-532+1064f*i/20, z=-354.646f+709.293f*j/20;
            foreach(var c in cols){ if(c.Raycast(new Ray(new Vector3(x,500,z),Vector3.down),out var hit,1000)){
                int gx=Mathf.RoundToInt(i/20f*(R-1)), gz=Mathf.RoundToInt(j/20f*(R-1)); float th=a[gz,gx]*80.056f+0.002f;
                s+=Mathf.Abs(hit.point.y-th); n++; break;}}}
        L($"align samples={n} flip={flip}"); return n>0?s/n:999;
    }
    static Texture2D Mask(string f){ var t=new Texture2D(2,2); t.LoadImage(File.ReadAllBytes(SRC+f)); return t; }
    static TerrainLayer Layer(string name,Color c,float noise){
        var tex=new Texture2D(256,256,TextureFormat.RGBA32,true);
        for(int y=0;y<256;y++)for(int x=0;x<256;x++){ float nn=Mathf.PerlinNoise(x*.08f,y*.08f)*.6f+Random.value*.4f; {var cc=c*(1-noise+noise*2*nn); cc.a=0f; tex.SetPixel(x,y,cc);}}
        tex.Apply(); string p="Assets/ValVert/"+name+".png"; File.WriteAllBytes(p,tex.EncodeToPNG()); AssetDatabase.ImportAsset(p);
        var tl=new TerrainLayer(); tl.diffuseTexture=AssetDatabase.LoadAssetAtPath<Texture2D>(p); tl.tileSize=new Vector2(4,4); tl.smoothness=0f; tl.metallic=0f; tl.specular=Color.black;
        AssetDatabase.CreateAsset(tl,"Assets/ValVert/"+name+".terrainlayer"); return tl;
    }
    static void Paint(TerrainData td){
        td.terrainLayers=new[]{ Layer("Grass",new Color(.36f,.42f,.2f),.3f), Layer("ForestFloor",new Color(.38f,.27f,.15f),.35f), Layer("PathDirt",new Color(.55f,.48f,.38f),.2f) };
        int A=1024; td.alphamapResolution=A; var fm=Mask("forest_mask.png"); var pm=Mask("paths_mask.png"); var rm=Mask("roads_mask.png");
        var al=new float[A,A,3];
        for(int y=0;y<A;y++)for(int x=0;x<A;x++){ float u=x/(float)(A-1), v=y/(float)(A-1); // v=0 south; Texture2D UV v=0 is bottom = south for north-up png
            float f=fm.GetPixelBilinear(u,v).r, pa=Mathf.Max(pm.GetPixelBilinear(u,v).r,rm.GetPixelBilinear(u,v).r);
            pa=Mathf.Clamp01(pa); f=Mathf.Clamp01(f)*(1-pa); al[y,x,0]=1-f-pa; al[y,x,1]=f; al[y,x,2]=pa; }
        td.SetAlphamaps(0,0,al);
    }
    static void Shot(Camera c,string path){
        var rt=new RenderTexture(1600,900,24); c.targetTexture=rt; c.Render(); RenderTexture.active=rt;
        var t=new Texture2D(1600,900,TextureFormat.RGB24,false); t.ReadPixels(new Rect(0,0,1600,900),0,0); t.Apply();
        File.WriteAllBytes(path,t.EncodeToPNG()); c.targetTexture=null; RenderTexture.active=null; L("shot "+path);
    }
}
