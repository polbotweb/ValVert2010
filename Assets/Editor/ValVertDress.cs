using UnityEngine; using UnityEditor; using UnityEngine.Rendering; using UnityEngine.Rendering.Universal; using System.IO; using System.Linq; using System.Collections.Generic;
public static class ValVertDress {
    const string SRC="/workspace/valvert/clean/"; const string G="Assets/ValVert/Ground/"; const string V="Assets/ValVert/Veg/"; const string T="Assets/ValVert/Veg/tex/";
    static void L(string s){ Debug.Log("[VV] "+s); }
    public static Dictionary<string,int> Counts=new Dictionary<string,int>();
    public static Texture2D Mask(string f){ var t=new Texture2D(2,2); t.LoadImage(File.ReadAllBytes(SRC+f)); return t; }
    public static float S(Texture2D m,float x,float z)=>m.GetPixelBilinear((x+532f)/1064f,(z+354.646f)/709.293f).r;
    public static Texture2D Tex(string p,bool normal=false,bool linear=false){
        var ti=(TextureImporter)AssetImporter.GetAtPath(p); bool ch=false;
        if(normal&&ti.textureType!=TextureImporterType.NormalMap){ti.textureType=TextureImporterType.NormalMap;ch=true;}
        if(linear&&ti.sRGBTexture){ti.sRGBTexture=false;ch=true;}
        if(!normal&&ti.anisoLevel<8){ti.anisoLevel=16;ch=true;}
        if(p.EndsWith(".png")&&!normal&&!linear&&!ti.mipMapsPreserveCoverage){ti.alphaIsTransparency=true;ti.mipMapsPreserveCoverage=true;ti.alphaTestReferenceValue=0.45f;ch=true;}
        if(ch) ti.SaveAndReimport(); var t=AssetDatabase.LoadAssetAtPath<Texture2D>(p); if(!t) L("MISSING TEX "+p); return t; }
    public static Material Mat(string name,Texture2D d,Texture2D n,bool cut,Color col,float smooth=0.1f){
        var m=new Material(Shader.Find("Universal Render Pipeline/Lit")); m.name=name;
        if(d) m.SetTexture("_BaseMap",d); m.SetColor("_BaseColor",col); m.SetFloat("_Smoothness",smooth);
        if(n){ m.SetTexture("_BumpMap",n); m.EnableKeyword("_NORMALMAP"); }
        if(cut){ m.SetFloat("_AlphaClip",1); m.SetFloat("_Cutoff",0.45f); m.EnableKeyword("_ALPHATEST_ON"); m.SetFloat("_Cull",0); m.renderQueue=2450; m.SetOverrideTag("RenderType","TransparentCutout"); }
        m.enableInstancing=true; AssetDatabase.CreateAsset(m,"Assets/ValVert/Mats/"+name+".mat"); return m; }
    static TerrainLayer TL(string n,float tile){
        var tl=new TerrainLayer(); tl.diffuseTexture=Tex(G+n+"_diff.jpg"); tl.normalMapTexture=Tex(G+n+"_nor.jpg",true); tl.maskMapTexture=Tex(G+n+"_mask.png",false,true);
        tl.tileSize=new Vector2(tile,tile); if(n=="leafy_grass") tl.diffuseRemapMax=new Vector4(0.8f,1.0f,0.65f,1); tl.normalScale=1; tl.metallic=0; tl.smoothness=0; tl.maskMapRemapMax=new Vector4(0,1,1,0.5f);
        AssetDatabase.CreateAsset(tl,"Assets/ValVert/Ground/"+n+".terrainlayer"); return tl; }
    static TerrainLayer TL2(string n,float tile){ var tl=new TerrainLayer(); tl.diffuseTexture=Tex(G+n+"_diff.jpg"); tl.normalMapTexture=Tex(G+n+"_nor.jpg",true); tl.tileSize=new Vector2(tile,tile); tl.smoothness=0.05f; tl.metallic=0; AssetDatabase.CreateAsset(tl,"Assets/ValVert/Ground/"+n+".terrainlayer"); return tl; }
    public static void PaintPBR(TerrainData td){
        td.terrainLayers=new[]{TL("leafy_grass",3f),TL("forest_leaves_02",3f),TL("stony_dirt_path",2.5f),TL2("roadgrey",4f),TL2("concrete_floor_02",2.5f),TL2("rock_wall_08",3f),TL2("asphalt_02",4f),TL2("river_small_rocks",1.5f)};
        int A=1024; td.alphamapResolution=A; var fm=Mask("canopy_orig.png"); var fm2=Mask("veg_forest.png"); var pm=Mask("paths_mask.png"); var rm=Mask("roads_mask.png");
        var al=new float[A,A,8];
        for(int y=0;y<A;y++)for(int x=0;x<A;x++){ float u=x/(float)(A-1), v=y/(float)(A-1);
            float f=0; for(int k=0;k<5;k++){ float du=(k%2==0?1:-1)*(k/2)*0.003f; f+=Mathf.Max(fm.GetPixelBilinear(u+du,v-du*0.7f).r,0.7f*fm2.GetPixelBilinear(u+du,v+du).r);} f/=5;
            f=Mathf.Clamp01(f*1.3f+(Mathf.PerlinNoise(u*90,v*90)-0.5f)*0.3f);
            float pa=pm.GetPixelBilinear(u,v).r, ro=rm.GetPixelBilinear(u,v).r; pa=Mathf.Clamp01(pa); ro=Mathf.Clamp01(ro)*(1-pa);
            int lb=ValVertWoods.Lab(-532f+1064f*u,-354.646f+709.293f*v);
            if(lb>0 && pa<0.5f){ int li= lb==1?4: lb==2?5: lb==3?6: 7; float w= lb==4?0.85f:1f; al[y,x,li]=w; al[y,x,1]=(1-w); continue; }
            float rest=1-pa-ro; al[y,x,0]=rest*(1-f); al[y,x,1]=rest*f; al[y,x,2]=pa; al[y,x,3]=ro; }
        td.SetAlphamaps(0,0,al); L("terrain PBR layers painted (8 layers, LiDAR canopy)");
    }
    static GameObject Fbx(string p){ var mi=(ModelImporter)AssetImporter.GetAtPath(p); mi.bakeAxisConversion=true; mi.importAnimation=false; mi.addCollider=false; mi.SaveAndReimport(); return AssetDatabase.LoadAssetAtPath<GameObject>(p); }
    static void Assign(GameObject go,System.Func<string,Material> pick){ foreach(var r in go.GetComponentsInChildren<MeshRenderer>()){ var ms=r.sharedMaterials.Select(m=>pick(m?m.name.ToLower():"")).ToArray(); r.sharedMaterials=ms; r.shadowCastingMode=ShadowCastingMode.On; } }
    static float baseH(GameObject go){ var b=go.GetComponentsInChildren<Renderer>().Select(r=>r.bounds).Aggregate((a,c)=>{a.Encapsulate(c);return a;}); return b.size.y; }
    static GameObject TreePrefab(string sp,string variant,Material bark,Material br){
        var leaf=Mat(sp+"_leaves_"+variant,Tex(T+sp+"_leaves_"+variant+".png"),null,true,Color.white,0.15f);
        var root=new GameObject(sp+"_"+variant); var lods=new List<LOD>(); float[] th={0.30f,0.05f}; int i=0;
        foreach(var r in new[]{"_r25","_r6"}){ var src=Fbx(V+sp+r+".fbx"); var inst=(GameObject)Object.Instantiate(src); inst.name="LOD"+i; inst.transform.SetParent(root.transform,false);
            Assign(inst,n=>n.Contains("leaves")?leaf:(n.Contains("branch")?br:bark));
            lods.Add(new LOD(th[i],inst.GetComponentsInChildren<Renderer>())); i++; }
        var lg=root.AddComponent<LODGroup>(); lg.SetLODs(lods.ToArray()); lg.RecalculateBounds();
        float h=baseH(root); var pf=PrefabUtility.SaveAsPrefabAsset(root,"Assets/ValVert/Prefabs/"+root.name+".prefab"); Object.DestroyImmediate(root); L($"tree prefab {pf.name} baseH={h:F2}"); hts[pf.name]=h; return pf; }
    static Dictionary<string,float> hts=new Dictionary<string,float>();
    static GameObject Simple(string fbx,Material m,string name){ var inst=(GameObject)Object.Instantiate(Fbx(V+fbx+".fbx")); inst.name=name; Assign(inst,n=>m);
        float h=baseH(inst); var pf=PrefabUtility.SaveAsPrefabAsset(inst,"Assets/ValVert/Prefabs/"+name+".prefab"); Object.DestroyImmediate(inst); hts[name]=h; L($"prefab {name} h={h:F2}"); return pf; }

    public static void Vegetation(Terrain terr){
        Directory.CreateDirectory("Assets/ValVert/Mats"); Directory.CreateDirectory("Assets/ValVert/Prefabs"); AssetDatabase.Refresh();
        var td=terr.terrainData;
        var tsBark=Mat("tree_small_02_trunk",Tex(T+"tree_small_02_diff_1k.jpg"),Tex(T+"tree_small_02_nor_gl_1k.jpg",true),false,Color.white);
        var tsBr=Mat("tree_small_02_branch",Tex(T+"tree_small_02_branch_diff_1k.jpg"),Tex(T+"tree_small_02_branch_nor_gl_1k.jpg",true),true,Color.white);
        var itBark=Mat("island_tree_02_trunk",Tex(T+"island_tree_02_diff_1k.jpg"),Tex(T+"island_tree_02_nor_gl_1k.jpg",true),false,Color.white);
        var itBr=Mat("island_tree_02_branch",Tex(T+"island_tree_02_branches_diff_1k.jpg"),Tex(T+"island_tree_02_branches_nor_gl_1k.jpg",true),true,Color.white);
        var trees=new List<GameObject>();
        foreach(var v in new[]{"autumnA","autumnB","green"}){ trees.Add(TreePrefab("tree_small_02",v,tsBark,tsBr)); trees.Add(TreePrefab("island_tree_02",v,itBark,itBr)); }
        var shA=Mat("shrub_02_autumnA",Tex(T+"shrub_02_leaves_autumnA.png"),null,true,Color.white); var shG=Mat("shrub_02_green",Tex(T+"shrub_02_leaves_green.png"),null,true,Color.white);
        var fern=Mat("fern_02",Tex(T+"fern_02_leaves_green.png"),null,true,new Color(.85f,.8f,.6f));
        var grass=Mat("grass_medium_01",Tex(T+"grass_medium_01_leaves_green.png"),null,true,new Color(.85f,.82f,.65f));
        var small=new List<GameObject>{ Simple("shrub_02__a",shA,"shrub_a_autumn"), Simple("shrub_02__c",shG,"shrub_c_green"), Simple("fern_02__b",fern,"fern_b"), Simple("fern_02__c",fern,"fern_c") };
        var protos=trees.Concat(small).ToList();
        td.treePrototypes=protos.Select(p=>new TreePrototype{prefab=p}).ToArray();
        var fm=Mask("canopy_orig.png"); var ex=Mask("veg_excl.png"); var me=Mask("veg_meadow.png");
        var rnd=new System.Random(42); System.Func<float> R=()=>(float)rnd.NextDouble();
        var list=new List<TreeInstance>();
        System.Func<float,float,bool> bad=(x,z)=> x<-531||x>531||z<-354||z>354|| S(ex,x,z)>0.35f || ValVertWoods.Lab(x,z)!=0 || ValVertWoods.Hidden(new Vector3(x,0,z));
        System.Func<float,float,float,bool> clear=(x,z,rad)=>{ if(bad(x,z)) return false; for(int k=0;k<8;k++){ float a=k*Mathf.PI/4; if(bad(x+Mathf.Cos(a)*rad,z+Mathf.Sin(a)*rad)) return false;} return true; };
        System.Func<float,float,bool> spawnZone=(x,z)=> ValVertWoods.ShotClear(x,z);
        System.Action<int,float,float,float> add=(pi,x,z,targetH)=>{ if(protos[pi].name.StartsWith("island")) targetH*=0.7f; float s=targetH/hts[protos[pi].name];
            list.Add(new TreeInstance{prototypeIndex=pi,position=new Vector3((x+532f)/1064f,0,(z+354.646f)/709.293f),heightScale=s,widthScale=s*(0.85f+0.3f*R()),rotation=R()*Mathf.PI*2,color=Color.Lerp(Color.white,new Color(.85f,.9f,.8f),R()),lightmapColor=Color.white});
            Counts[protos[pi].name]=(Counts.TryGetValue(protos[pi].name,out var c)?c:0)+1; };
        int pickVar(){ float r=R(); return r<0.72f?2:(r<0.9f?0:1); }
        var vg=JsonUtility.FromJson<ValVertWoods.VG>(File.ReadAllText(SRC+"veg3_flat.json"));
        int nt=0,ns=0,nd=0;
        for(int i=0;i<vg.trees.Length;i+=4){ float x=vg.trees[i], z=vg.trees[i+1], h=vg.trees[i+2]; int k=(int)vg.trees[i+3];
            if(x>531) continue; bool ok=clear(x,z,0.7f)&&!spawnZone(x,z);
            if(!ok){ for(int t=0;t<16&&!ok;t++){ float a=t*0.785f, r=t<8?1.6f:3.0f; float nx=x+Mathf.Cos(a)*r, nz=z+Mathf.Sin(a)*r; if(clear(nx,nz,0.7f)&&!spawnZone(nx,nz)){x=nx;z=nz;ok=true;} } }
            if(!ok){ nd++; continue; }
            float th=Mathf.Clamp(h*0.92f,3.5f,30f); int sp=(th>15f && R()<0.75f)?1:(R()<0.25f?1:0);
            add(pickVar()*2+sp,x,z,th); nt++; }
        // canopy fill: closed deciduous wood inside the LiDAR/ortho canopy mask
        int nf=0; float cf=3.1f;
        for(float x=-530;x<530;x+=cf)for(float z=-352;z<352;z+=cf){
            float px=x+R()*cf, pz=z+R()*cf; if(S(fm,px,pz)<0.5f||spawnZone(px,pz)||!clear(px,pz,1.0f)) continue;
            float th=8f+R()*14f; add(pickVar()*2+(R()<0.6f?1:0),px,pz,th); nf++; }
        L("canopy fill "+nf);
        // LiDAR low vegetation (0.8-2.5 m) -> shrubs
        for(int i=0;i<vg.shrubs.Length;i+=2){ float x=vg.shrubs[i], z=vg.shrubs[i+1]; if(x>531||!clear(x,z,0.4f)||spawnZone(x,z)) continue; add(R()<0.6f?6:7,x,z,1.0f+R()*1.3f); ns++; }
        // forest-floor undergrowth inside LiDAR canopy
        float c2=1.7f;
        for(float x=-530;x<530;x+=c2)for(float z=-352;z<352;z+=c2){
            float px=x+R()*c2, pz=z+R()*c2; float f=S(fm,px,pz); if(f<0.5f || spawnZone(px,pz)) continue;
            bool edge=false; for(int k=0;k<6;k++){ float a=k*1.047f; if(S(fm,px+Mathf.Cos(a)*4,pz+Mathf.Sin(a)*4)<0.5f) edge=true; }
            float p=edge?0.6f:0.42f; if(R()<p && clear(px,pz,0.5f)){ int i=R()<0.45f?(6+(R()<0.5f?0:1)):(8+(R()<0.5f?0:1)); float hh=i<8?0.8f+R()*1.2f:0.5f+R()*0.5f; add(i,px,pz,hh); ns++; }
        }
        // OSM-mapped street trees (natural=tree nodes, natural=tree_row ways) in the urban area
        var ot=JsonUtility.FromJson<ValVertWoods.VG>(File.ReadAllText(SRC+"osm_trees.json")); int no=0;
        var otE=new List<float>();
        for(int i=0;i<ot.trees.Length;i+=4){ float x=ot.trees[i], z=ot.trees[i+1], th=Mathf.Clamp(ot.trees[i+2],6f,14f);
            if(x>531){ otE.Add(x); otE.Add(z); otE.Add(th); continue; }
            if(!clear(x,z,0.6f)) continue; add(pickVar()*2,x,z,th); no++; }
        L("osm street trees west "+no+" east "+otE.Count/3);
        { int nb=0; for(float bx=352.5f;bx<361.0f;bx+=1.3f) for(float bz=117.5f;bz<166f;bz+=1.3f){ float px=bx+(R()-0.5f)*0.9f, pz=bz+(R()-0.5f)*0.9f;   // #168 bank behind the alley (user photos): dense evergreen understorey + tall green trees
              if(pz>129.6f&&pz<135.6f&&px>340f) continue; add(7,px,pz,2.4f+R()*1.8f); nb++;
              if(R()<0.14f) { add(R()<0.5f?4:5,px,pz,16f+R()*8f); nb++; } } L("bank168 veg "+nb); }
        td.SetTreeInstances(list.ToArray(),true);
        if(ValVertWoods.East){ var et=ValVertWoods.East.terrainData; et.treePrototypes=td.treePrototypes; var el=new List<TreeInstance>();
            var em=JsonUtility.FromJson<ValVertWoods.ML>(File.ReadAllText(SRC+"east_meshes.json"));
            for(int i=0;i<em.trees.Length;i+=3){ float x=em.trees[i], z=em.trees[i+1], th=Mathf.Clamp(em.trees[i+2]*0.92f,3.5f,30f); int pi=pickVar()*2+((th>15f&&R()<0.75f)?1:(R()<0.25f?1:0)); float hh=th*(protos[pi].name.StartsWith("island")?0.7f:1f); float s=hh/hts[protos[pi].name];
                el.Add(new TreeInstance{prototypeIndex=pi,position=new Vector3((x-532f)/432f,0,(z+354.646f)/709.293f),heightScale=s,widthScale=s*(0.85f+0.3f*R()),rotation=R()*Mathf.PI*2,color=Color.white,lightmapColor=Color.white}); }
            for(int i=0;i<otE.Count;i+=3){ float x=otE[i], z=otE[i+1], th=otE[i+2]; int pi=pickVar()*2; float s=th/hts[protos[pi].name];
                el.Add(new TreeInstance{prototypeIndex=pi,position=new Vector3((x-532f)/432f,0,(z+354.646f)/709.293f),heightScale=s,widthScale=s,rotation=R()*Mathf.PI*2,color=Color.white,lightmapColor=Color.white}); }
            et.SetTreeInstances(el.ToArray(),true); L("east trees "+el.Count); }
        terr.treeDistance=800; terr.treeBillboardDistance=800; terr.treeMaximumFullLODCount=20000; terr.treeLODBiasMultiplier=1;
        Counts["_trees_lidar"]=nt; Counts["_trees_dropped"]=nd; Counts["_shrubs_ferns"]=ns;
        L($"placed LiDAR trees={nt} dropped={nd} shrubs/ferns={ns} total={list.Count}");
        terr.detailObjectDistance=0;
    }
    static void World(Mesh src,Matrix4x4 m,out Vector3[] v,out int[] t){ v=src.vertices.Select(p=>m.MultiplyPoint3x4(p)).ToArray(); t=src.triangles; }
    public static void Surfaces(GameObject root){
        Directory.CreateDirectory("Assets/ValVert/Mats"); AssetDatabase.Refresh();
        var plaster=Mat("plaster",Tex(G+"white_plaster_rough_01_bright.jpg"),Tex(G+"white_plaster_rough_01_nor.jpg",true),false,Color.white,0.05f);
        var roof=Mat("roof",Tex(G+"grey_roof_tiles_diff.jpg"),Tex(G+"grey_roof_tiles_nor.jpg",true),false,new Color(.62f,.42f,.36f),0.15f);
        var dirt=Mat("path_dirt",Tex(G+"stony_dirt_path_diff.jpg"),Tex(G+"stony_dirt_path_nor.jpg",true),false,Color.white,0.05f);
        var asph=Mat("road",Tex(G+"asphalt_02_diff.jpg"),Tex(G+"asphalt_02_nor.jpg",true),false,new Color(.78f,.78f,.78f),0.12f);
        var water=Mat("water",null,null,false,new Color(.12f,.15f,.13f),0.8f);
        foreach(var mf in root.GetComponentsInChildren<MeshFilter>(true)){ var n=mf.name; var mr=mf.GetComponent<MeshRenderer>(); if(!mr||!mf.gameObject.activeInHierarchy) continue;
            if(n=="Buildings"){ var src=mf.sharedMesh; var tr=mf.transform; World(src,tr.localToWorldMatrix,out var wv,out var wt);
                var bmk=Mask("buildings_mask.png"); int flips=0; var nv=new List<Vector3>(); var uv=new List<Vector2>(); var walls=new List<int>(); var roofs=new List<int>(); var inv=tr.worldToLocalMatrix;
                for(int i=0;i<wt.Length;i+=3){ Vector3 a=wv[wt[i]],b=wv[wt[i+1]],c=wv[wt[i+2]]; if(ValVertWoods.Hidden((a+b+c)/3f)) continue; var nn=Vector3.Cross(b-a,c-a).normalized; bool isRoof=Mathf.Abs(nn.y)>0.35f; bool flip=false; if(isRoof) flip=nn.y<0; else { var ce=(a+b+c)/3f; float pin=S(bmk,ce.x+nn.x*1.2f,ce.z+nn.z*1.2f), pout=S(bmk,ce.x-nn.x*1.2f,ce.z-nn.z*1.2f); flip=pin>pout+0.2f; } if(flip){ var t_=b; b=c; c=t_; nn=-nn; flips++; }
                    int bi=nv.Count; foreach(var p in new[]{a,b,c}){ nv.Add(inv.MultiplyPoint3x4(p)); Vector2 u= isRoof? new Vector2(p.x,p.z)/2.5f : (Mathf.Abs(nn.x)>Mathf.Abs(nn.z)? new Vector2(p.z,p.y):new Vector2(p.x,p.y))/2.5f; uv.Add(u);} 
                    (isRoof?roofs:walls).AddRange(new[]{bi,bi+1,bi+2}); }
                var me=new Mesh{indexFormat=IndexFormat.UInt32}; me.SetVertices(nv); me.SetUVs(0,uv); me.subMeshCount=2; me.SetTriangles(walls,0); me.SetTriangles(roofs,1); me.RecalculateNormals(); me.RecalculateTangents(); me.RecalculateBounds();
                AssetDatabase.CreateAsset(me,"Assets/ValVert/Buildings_split.asset"); mf.sharedMesh=me; mr.sharedMaterials=new[]{plaster,roof}; var mc=mf.GetComponent<MeshCollider>(); if(mc) mc.sharedMesh=me;
                Counts["_building_wall_tris"]=walls.Count/3; Counts["_building_roof_tris"]=roofs.Count/3; L($"flips={flips} buildings split walls={walls.Count/3} roofs={roofs.Count/3}"); }
            else if(n=="Paths"||n=="Roads"){ var me=Object.Instantiate(mf.sharedMesh); var w=me.vertices.Select(p=>mf.transform.TransformPoint(p)).ToArray(); me.uv=w.Select(p=>new Vector2(p.x,p.z)/(n=="Paths"?2.5f:3f)).ToArray(); me.RecalculateTangents();
                AssetDatabase.CreateAsset(me,"Assets/ValVert/"+n+"_uv.asset"); mf.sharedMesh=me; mr.sharedMaterials=Enumerable.Repeat(n=="Paths"?dirt:asph,mr.sharedMaterials.Length).ToArray(); }
            else if(n=="Water"){ mr.sharedMaterials=Enumerable.Repeat(water,mr.sharedMaterials.Length).ToArray(); }
        }
    }
    public static void SkyPost(){ RenderSettings.ambientIntensity=1.25f; RenderSettings.reflectionIntensity=0.8f;
        string hp="Assets/ValVert/Sky/kloofendal_48d_partly_cloudy_puresky_2k.hdr"; if(!File.Exists(hp)) hp=Directory.GetFiles("Assets/ValVert/Sky","*.hdr")[0];
        var ti=(TextureImporter)AssetImporter.GetAtPath(hp); ti.textureShape=TextureImporterShape.Texture2D; ti.mipmapEnabled=false; ti.wrapModeU=TextureWrapMode.Repeat; ti.SaveAndReimport();
        var sky=new Material(Shader.Find("Skybox/Panoramic")); sky.SetTexture("_MainTex",AssetDatabase.LoadAssetAtPath<Texture2D>(hp)); sky.SetColor("_Tint",new Color(.62f,.63f,.64f)); sky.SetFloat("_Exposure",0.9f); sky.SetFloat("_Rotation",40);
        AssetDatabase.CreateAsset(sky,"Assets/ValVert/Mats/sky.mat"); RenderSettings.skybox=sky;
        var prof=ScriptableObject.CreateInstance<VolumeProfile>(); AssetDatabase.CreateAsset(prof,"Assets/ValVert/PostProfile.asset");
        var b=prof.Add<Bloom>(true); b.intensity.Override(0.35f); b.threshold.Override(1.0f); b.scatter.Override(0.6f);
        var ca=prof.Add<ColorAdjustments>(true); ca.contrast.Override(10f); ca.saturation.Override(-12f); ca.colorFilter.Override(new Color(.97f,1f,1f)); ca.postExposure.Override(0.1f);
        var vg=prof.Add<Vignette>(true); vg.intensity.Override(0.22f); vg.smoothness.Override(0.45f);
        var tm=prof.Add<Tonemapping>(true); tm.mode.Override(TonemappingMode.Neutral);
        foreach(var c in prof.components) AssetDatabase.AddObjectToAsset(c,prof);
        var go=new GameObject("PostVolume"); var vol=go.AddComponent<Volume>(); vol.isGlobal=true; vol.sharedProfile=prof;
        var urp=GraphicsSettings.currentRenderPipeline as UniversalRenderPipelineAsset; if(urp){ urp.shadowDistance=180; urp.supportsHDR=true; EditorUtility.SetDirty(urp);} 
        L("sky + post set");
    }
    public static void Post(Camera c){ var d=c.GetUniversalAdditionalCameraData(); d.renderPostProcessing=true; d.antialiasing=AntialiasingMode.SubpixelMorphologicalAntiAliasing; }
}
