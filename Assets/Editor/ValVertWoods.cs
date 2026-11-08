using UnityEngine; using UnityEditor; using UnityEngine.Rendering; using System.IO; using System.Linq; using System.Collections.Generic;
public static class ValVertWoods {
    const string SRC="/workspace/valvert/clean/"; const string G="Assets/ValVert/Ground/"; const string GEN="Assets/ValVert/Gen/";
    [System.Serializable] public class MI{ public string name,mat; public float[] v; public int[] t; public float[] uv; }
    [System.Serializable] public class ML{ public MI[] items; public float[] trees; public float[] hide; public int[] hideLen; }
    static List<Vector2[]> hid;
    public static bool Hidden(Vector3 p){ if(hid==null){ hid=new List<Vector2[]>(); var ml=JsonUtility.FromJson<ML>(File.ReadAllText(SRC+"woods_meshes.json")); int o=0; if(ml.hideLen!=null) foreach(var n in ml.hideLen){ var q=new Vector2[n]; for(int i=0;i<n;i++) q[i]=new Vector2(ml.hide[2*(o+i)],ml.hide[2*(o+i)+1]); o+=n; hid.Add(q);} }
        foreach(var q in hid){ bool ins=false; for(int i=0,j=q.Length-1;i<q.Length;j=i++) if(((q[i].y>p.z)!=(q[j].y>p.z))&&(p.x<(q[j].x-q[i].x)*(p.z-q[i].y)/(q[j].y-q[i].y)+q[i].x)) ins=!ins; if(ins) return true; } return false; }
    [System.Serializable] public class VG{ public float[] trees; public float[] shrubs; }
    static byte[] lab;
    static void L(string s){ Debug.Log("[VV] "+s); }
    public static int Lab(float x,float z){ if(lab==null) lab=File.ReadAllBytes(SRC+"label_v3.raw");
        int i=Mathf.RoundToInt((x+532f)/1064f*2048), j=Mathf.RoundToInt((z+354.646f)/709.293f*2048); if(i<0||j<0||i>2048||j>2048) return 0; return lab[j*2049+i]; }
    // keep camera positions free of trunks
    public static Vector2[] Clear={ new Vector2(368.2f,126f), new Vector2(353.5f,145.2f), new Vector2(338f,222f), new Vector2(377.5f,113.6f), new Vector2(373f,108f), new Vector2(375f,103f), new Vector2(249f,313.4f), new Vector2(452f,104f), new Vector2(450f,112f), new Vector2(408f,116.5f), new Vector2(416f,116.5f), new Vector2(0,-56.15f), new Vector2(10,-72.7f), new Vector2(-120,-159.5f), new Vector2(37.4f,-41.56f), new Vector2(-15.07f,2.08f), new Vector2(338,-12), new Vector2(447,-8), new Vector2(300,-48), new Vector2(372,-2), new Vector2(400,-4), new Vector2(454.5f,27.5f), new Vector2(492,57), new Vector2(456,20), new Vector2(448,0), new Vector2(409,-7), new Vector2(453.5f,16.8f), new Vector2(436,9), new Vector2(442,11), new Vector2(418,-4), new Vector2(452,21), new Vector2(447,17), new Vector2(443,14), new Vector2(448,18), new Vector2(439,-4), new Vector2(436,2), new Vector2(458,104), new Vector2(458,110), new Vector2(488,58), new Vector2(478,60), new Vector2(592,-70), new Vector2(582,-68), new Vector2(336,222), new Vector2(336,229), new Vector2(336,236), new Vector2(453.5f,17.6f), new Vector2(446,15), new Vector2(440,12), new Vector2(470,113), new Vector2(348,203), new Vector2(348,210), new Vector2(455,98), new Vector2(449,18.5f), new Vector2(336,206f), new Vector2(336,210f), new Vector2(336,214f), new Vector2(336,218f), new Vector2(336,222f), new Vector2(336,226f), new Vector2(340,206f), new Vector2(340,210f), new Vector2(340,214f), new Vector2(340,218f), new Vector2(340,222f), new Vector2(340,226f), new Vector2(344,206f), new Vector2(344,210f), new Vector2(344,214f), new Vector2(344,218f), new Vector2(344,222f), new Vector2(344,226f), new Vector2(348,206f), new Vector2(348,210f), new Vector2(348,214f), new Vector2(348,218f), new Vector2(348,222f), new Vector2(348,226f), new Vector2(352,206f), new Vector2(352,210f), new Vector2(352,214f), new Vector2(352,218f), new Vector2(352,222f), new Vector2(352,226f), new Vector2(356,206f), new Vector2(356,210f), new Vector2(356,214f), new Vector2(356,218f), new Vector2(356,222f), new Vector2(356,226f), new Vector2(360,206f), new Vector2(360,210f), new Vector2(360,214f), new Vector2(360,218f), new Vector2(360,222f), new Vector2(360,226f), new Vector2(392,-22f), new Vector2(444.5f,14.6f), new Vector2(432,0.5f), new Vector2(427,-3.5f), new Vector2(436.5f,-3), new Vector2(386,212), new Vector2(386,217), new Vector2(410,318), new Vector2(405,318), new Vector2(401,-15), new Vector2(412,300), new Vector2(436,-5), new Vector2(462,13), new Vector2(457,12), new Vector2(413,-11), new Vector2(379,239), new Vector2(375,239), new Vector2(450,25), new Vector2(446,20), new Vector2(445,-12), new Vector2(443,-8), new Vector2(386,221), new Vector2(460,14), new Vector2(418,-12), new Vector2(585,90), new Vector2(7,112), new Vector2(590.6f,126.3f), new Vector2(37.4f,146), new Vector2(589.5f,129.9f), new Vector2(117,-119), new Vector2(250.7f,312.6f), new Vector2(-407.6f,-187f), new Vector2(450.6f,69.5f), new Vector2(450.6f,78f), new Vector2(245.2f,315.1f), new Vector2(264.7f,266.4f), new Vector2(357.5f,195f), new Vector2(455f,96f), new Vector2(425f,116.5f), new Vector2(352.5f,195f), new Vector2(452f,108f) };
    public static bool Lawn168(float x,float z){ return x>357f&&x<394f&&z>92.5f&&z<116.5f; }
    public static bool ShotClear(float x,float z){ if(Lawn168(x,z)) return true; var p=new Vector2(x,z); foreach(var c in Clear) if((p-c).magnitude<4.5f) return true; return false; }
    static Material Also(this Material m){ m.SetFloat("_Cull",0); m.doubleSidedGI=true; return m; }
    static Material M(string n,Texture2D d,Texture2D nm,Color c,float sm,float met=0,bool cut=false){ var m=ValVertDress.Mat(n,d,nm,cut,c,sm); m.SetFloat("_Metallic",met); return m; }
    static Material Glass(){ var m=M("glass_shelter",null,null,new Color(.75f,.85f,.88f,0.28f),0.9f).Also(); m.SetFloat("_Surface",1); m.SetFloat("_Blend",0); m.SetFloat("_SrcBlend",(float)BlendMode.SrcAlpha); m.SetFloat("_DstBlend",(float)BlendMode.OneMinusSrcAlpha); m.SetFloat("_ZWrite",0); m.EnableKeyword("_SURFACE_TYPE_TRANSPARENT"); m.renderQueue=3000; return m; }
    public static void Meshes(Transform parent){ Meshes(parent,"woods_meshes.json","V3_Woods"); }
    static Dictionary<string,Material> MATS;
    public static void Meshes(Transform parent,string file,string rootName){
        var mats=MATS??new Dictionary<string,Material>{
            {"water",M("water_murky",null,ValVertDress.Tex(G+"river_small_rocks_nor.jpg",true),new Color(.16f,.16f,.10f),0.55f)},
            {"concrete_wall",M("concrete_wall",ValVertDress.Tex(G+"concrete_wall_008_bright.jpg"),ValVertDress.Tex(G+"concrete_wall_008_nor.jpg",true),Color.white,0.1f)},
            {"metal",M("metal_grey",null,null,new Color(.42f,.44f,.45f),0.45f,0.6f)},
            {"green_metal",M("green_rail",null,null,new Color(.10f,.36f,.20f),0.55f,0.3f)},
            {"dark",M("dark_hole",null,null,new Color(.02f,.02f,.02f),0.0f).Also()},
            {"grate",M("grate",ValVertDress.Tex(G+"metal_grate_rusty_diff.jpg"),ValVertDress.Tex(G+"metal_grate_rusty_nor.jpg",true),Color.white,0.3f,0.5f)},
            {"concrete_board",M("concrete_board",ValVertDress.Tex(GEN+"concrete_board.jpg"),ValVertDress.Tex(G+"concrete_wall_008_nor.jpg",true),Color.white,0.08f).Also()},
            {"stone",M("stone_rim",ValVertDress.Tex(G+"rock_wall_08_diff.jpg"),ValVertDress.Tex(G+"rock_wall_08_nor.jpg",true),new Color(1.25f,1.22f,1.15f),0.05f).Also()},
            {"stone_house",M("stone_house",ValVertDress.Tex(GEN+"stone_house.jpg"),ValVertDress.Tex(G+"rock_wall_08_nor.jpg",true),Color.white,0.05f).Also()},
            {"roof_tiles",M("roof_tiles_red",ValVertDress.Tex(G+"grey_roof_tiles_diff.jpg"),ValVertDress.Tex(G+"grey_roof_tiles_nor.jpg",true),new Color(1.0f,0.55f,0.42f),0.1f).Also()},
            {"window",M("window_dark",null,null,new Color(.08f,.09f,.10f),0.8f).Also()},
            {"white_trim",M("white_trim",null,null,new Color(.85f,.83f,.78f),0.1f).Also()},
            {"fence_green",M("fence_green",ValVertDress.Tex(GEN+"fence_green.png"),null,Color.white,0.3f,0,true).Also()},
            {"bollard",M("bollard",ValVertDress.Tex(GEN+"bollard.png"),null,Color.white,0.4f).Also()},
            {"orn_grass",M("orn_grass",ValVertDress.Tex(GEN+"orn_grass.png"),null,Color.white,0.05f,0,true).Also()},
            {"sign_aldi",M("sign_aldi",ValVertDress.Tex(GEN+"sign_aldi.png"),null,Color.white,0.2f).Also()},
            {"sign_lidl",M("sign_lidl",ValVertDress.Tex(GEN+"sign_lidl.png"),null,Color.white,0.2f).Also()},
            {"sign_grandfrais",M("sign_grandfrais",ValVertDress.Tex(GEN+"sign_grandfrais.png"),null,Color.white,0.2f).Also()},
            {"sign_ecole",M("sign_ecole",ValVertDress.Tex(GEN+"sign_ecole.png"),null,Color.white,0.2f).Also()},
            {"sign_pharma",M("sign_pharma",ValVertDress.Tex(GEN+"sign_pharma.png"),null,Color.white,0.2f).Also()},
            {"sign_bus",M("sign_bus",ValVertDress.Tex(GEN+"sign_bus.png"),null,Color.white,0.2f).Also()},
            {"shopfront",M("shopfront",ValVertDress.Tex(GEN+"shopfront.png"),null,Color.white,0.7f).Also()},
            {"school_wall",M("school_wall",ValVertDress.Tex(GEN+"school_facade.png"),null,Color.white,0.1f).Also()},
            {"store_wall",M("store_wall",ValVertDress.Tex(GEN+"cladding_light_diff.png"),ValVertDress.Tex(GEN+"corrugated_iron_02_nor_gl_1k.jpg",true),Color.white,0.4f,0.35f).Also()},
            {"roof_flat",M("roof_flat",ValVertDress.Tex(G+"gravel_road_diff.jpg"),null,new Color(.75f,.75f,.75f),0.05f).Also()},
            {"roof_shelter",M("roof_shelter",null,null,new Color(.35f,.37f,.38f),0.4f,0.5f).Also()},
            {"lamp_head",M("lamp_head",null,null,new Color(.3f,.3f,.3f),0.5f,0.5f).Also()},
            {"glass",Glass()},
            {"asphalt",AsphaltWW("asphalt_road")},
            {"parking",ParkingMat()},
            {"ring",M("ring_ortho",ValVertDress.Tex(GEN+"ring_ortho2010.jpg"),null,new Color(.9f,.9f,.9f),0.0f)},
            {"boxwood",M("boxwood",null,null,new Color(.13f,.24f,.08f),0.15f).Also()},
            {"conifer",M("conifer",null,null,new Color(.07f,.18f,.09f),0.1f).Also()},
            {"school_yard",M("school_yard",ValVertDress.Tex(GEN+"school_yard.png"),ValVertDress.Tex(G+"asphalt_02_nor.jpg",true),Color.white,0.15f)},
            {"pitch",M("pitch",ValVertDress.Tex(GEN+"pitch.png"),null,Color.white,0.2f)},
            {"gym_wall",M("gym_wall",ValVertDress.Tex(GEN+"gym_wall.png"),null,Color.white,0.2f).Also()},
            {"roof_gym",M("roof_gym",ValVertDress.Tex(GEN+"roof_gym.png"),null,Color.white,0.1f).Also()},
            {"facade",M("facade_atlas",ValVertDress.Tex(GEN+"facade_atlas.png"),ValVertDress.Tex(G+"white_plaster_rough_01_nor.jpg",true),Color.white,0.08f)},
            {"roof_terracotta",M("roof_terracotta",ValVertDress.Tex(GEN+"clay_roof_tiles_diff_1k.jpg"),ValVertDress.Tex(GEN+"clay_roof_tiles_nor_gl_1k.jpg",true),new Color(1f,.92f,.88f),0.08f)},
            {"roof_brown",M("roof_brown",ValVertDress.Tex(GEN+"clay_roof_tiles_02_diff_1k.jpg"),ValVertDress.Tex(GEN+"clay_roof_tiles_02_nor_gl_1k.jpg",true),new Color(.62f,.48f,.40f),0.08f)},
            {"roof_slate",M("roof_slate",ValVertDress.Tex(GEN+"grey_roof_tiles_02_diff_1k.jpg"),ValVertDress.Tex(GEN+"grey_roof_tiles_02_nor_gl_1k.jpg",true),new Color(.85f,.85f,.88f),0.12f)},
            {"roof_gravel",M("roof_gravel",ValVertDress.Tex(GEN+"gravel_roof_var.png"),ValVertDress.Tex(GEN+"gravel_concrete_nor_gl_1k.jpg",true),new Color(1f,1f,1f),0.05f)},
            {"shopglass",M("shopglass",null,null,new Color(.10f,.13f,.15f),0.95f,0.4f)},
            {"canopy",M("canopy",null,null,new Color(.80f,.81f,.82f),0.5f,0.3f)},
            {"roof_gravel2",M("roof_gravel2",ValVertDress.Tex(GEN+"gravel_roof_var.png"),null,new Color(.86f,.85f,.82f),0.05f)},
            {"roof_gravel3",M("roof_gravel3",ValVertDress.Tex(GEN+"gravel_roof_var.png"),null,new Color(.93f,.9f,.86f),0.05f)},
            {"sign_entree",M("sign_entree",ValVertDress.Tex(GEN+"sign_entree.png"),null,Color.white,0.3f).Also()},
            {"sign_boulangerie",M("sign_boulangerie",ValVertDress.Tex(GEN+"sign_boulangerie.png"),null,Color.white,0.3f).Also()},
            {"sign_assedic",M("sign_assedic",ValVertDress.Tex(GEN+"sign_assedic.png"),null,Color.white,0.3f).Also()},
            {"vitrine_boulangerie",M("vitrine_boulangerie",ValVertDress.Tex(GEN+"vitrine_boulangerie.png"),null,Color.white,0.9f).Also()},
            {"bakery_wood",M("bakery_wood",null,null,new Color(.40f,.09f,.12f),0.4f)},
            {"awning",M("awning",null,null,new Color(.55f,.12f,.14f),0.15f)},
            {"housenum",M("housenum",ValVertDress.Tex(GEN+"housenum.png"),null,Color.white,0.4f).Also()},
            {"coping",M("coping",null,null,new Color(.62f,.62f,.60f),0.1f)},
            {"cladding",M("cladding",ValVertDress.Tex(GEN+"cladding_light_diff.png"),ValVertDress.Tex(GEN+"corrugated_iron_02_nor_gl_1k.jpg",true),Color.white,0.4f,0.35f)},
            {"paint",M("road_paint",null,null,new Color(.92f,.92f,.90f),0.2f).Also()},
            {"kerb",M("kerb",ValVertDress.Tex(G+"concrete_wall_008_bright.jpg"),null,new Color(.78f,.77f,.74f),0.1f)},
            {"lamp_pole",M("lamp_pole",null,null,new Color(.36f,.38f,.40f),0.5f,0.6f)},
            {"lamp_lens",M("lamp_lens",null,null,new Color(.95f,.93f,.85f),0.9f)},
            {"tl_head",M("tl_head",null,null,new Color(.06f,.06f,.06f),0.4f)},
            {"tl_red",M("tl_red",null,null,new Color(.8f,.05f,.05f),0.9f).Also()},
            {"tl_amber",M("tl_amber",null,null,new Color(.25f,.17f,.02f),0.9f).Also()},
            {"tl_green",M("tl_green",null,null,new Color(.03f,.2f,.08f),0.9f).Also()},
            {"sign_stop",M("sign_stop",ValVertDress.Tex(GEN+"sign_stop.png"),null,Color.white,0.3f).Also()},
            {"sign_give_way",M("sign_give_way",ValVertDress.Tex(GEN+"sign_give_way.png"),null,Color.white,0.3f).Also()},
            {"sign_limit",M("sign_limit",ValVertDress.Tex(GEN+"sign_limit.png"),null,Color.white,0.3f).Also()},
            {"sign_names",M("sign_names",ValVertDress.Tex(GEN+"sign_names.png"),null,Color.white,0.3f).Also()},
            {"bench_wood",M("bench_wood",null,null,new Color(.42f,.28f,.16f),0.2f)},
            {"bin_green",M("bin_green",null,null,new Color(.12f,.30f,.16f),0.3f)},
            {"bin_grey",M("bin_grey",null,null,new Color(.45f,.47f,.45f),0.3f)},
            {"cabinet",M("cabinet",null,null,new Color(.72f,.72f,.66f),0.2f)},
            {"pole_wood",M("pole_wood",null,null,new Color(.30f,.22f,.15f),0.1f)},
            {"bollard_m",M("bollard_m",null,null,new Color(.20f,.22f,.24f),0.4f,0.4f)},
            {"barrier_rw",M("barrier_rw",null,null,new Color(.85f,.15f,.12f),0.3f)},
            {"postbox",M("postbox",null,null,new Color(.95f,.78f,.05f),0.4f)},
            {"hedge",M("hedge",ValVertDress.Tex(G+"forest_leaves_02_diff.jpg"),null,new Color(.45f,.65f,.35f),0.05f)},
            {"clad_white",M("clad_white",ValVertDress.Tex(GEN+"cladding_light_diff.png"),ValVertDress.Tex(GEN+"corrugated_iron_02_nor_gl_1k.jpg",true),new Color(1f,1f,1f),0.35f,0.3f)},
            {"clad_grey",M("clad_grey",ValVertDress.Tex(GEN+"cladding_light_diff.png"),ValVertDress.Tex(GEN+"corrugated_iron_02_nor_gl_1k.jpg",true),new Color(.62f,.64f,.66f),0.35f,0.3f)},
            {"clad_beige",M("clad_beige",ValVertDress.Tex(GEN+"cladding_light_diff.png"),ValVertDress.Tex(GEN+"corrugated_iron_02_nor_gl_1k.jpg",true),new Color(.92f,.86f,.74f),0.35f,0.3f)},
            {"band_grey",M("band_grey",null,null,new Color(.66f,.67f,.68f),0.35f).Also()},
            {"glass_refl",M("glass_refl",ValVertDress.Tex(GEN+"window_interior.png"),null,new Color(.62f,.64f,.66f),0.93f,0.15f)},
            {"shutter",M("shutter_pvc",ValVertDress.Tex(GEN+"shutter_diff.png"),ValVertDress.Tex(GEN+"shutter_nor.png",true),Color.white,0.35f)},
            {"render_beige",M("render_beige",ValVertDress.Tex(G+"white_plaster_rough_01_bright.jpg"),ValVertDress.Tex(G+"white_plaster_rough_01_nor.jpg",true),new Color(.84f,.80f,.72f),0.1f)},
            {"glass_door",M("glass_door",null,null,new Color(.16f,.20f,.23f),0.9f,0.2f)},
            {"render_grey",M("render_grey",ValVertDress.Tex(GEN+"grey_plaster_diff_1k.jpg"),ValVertDress.Tex(GEN+"grey_plaster_nor_gl_1k.jpg",true),new Color(.78f,.77f,.74f),0.1f)},
            {"render_white",M("render_white",ValVertDress.Tex(G+"white_plaster_rough_01_bright.jpg"),ValVertDress.Tex(G+"white_plaster_rough_01_nor.jpg",true),new Color(.96f,.96f,.95f),0.1f).Also()},
            {"corr_leaves",M("corr_leaves",ValVertDress.Tex(GEN+"corrugated_leaves.png"),ValVertDress.Tex(GEN+"corrugated_iron_02_nor_gl_1k.jpg",true),new Color(1f,1f,1f),0.1f).Also()},
            {"leafground",M("leafground",ValVertDress.Tex(G+"forest_leaves_02_diff.jpg"),ValVertDress.Tex(G+"forest_leaves_02_nor.jpg",true),new Color(.9f,.8f,.7f),0.05f)},
            {"asphalt_leaves",M("asphalt_leaves",ValVertDress.Tex(GEN+"asphalt_leaves.png"),ValVertDress.Tex(G+"asphalt_02_nor.jpg",true),new Color(1f,1f,1f),0.12f)},
            {"stair_earth",M("stair_earth",ValVertDress.Tex(G+"forest_leaves_02_diff.jpg"),ValVertDress.Tex(G+"forest_leaves_02_nor.jpg",true),new Color(2.1f,1.7f,1.25f),0.05f)},
            {"sign_blue_bay",M("sign_blue_bay",null,null,new Color(.08f,.3f,.75f),0.2f)},
            {"forest_fg02",M("forest_fg02",ValVertDress.Tex("Assets/ValVert/Veg/pilot6/fg02_diff.jpg"),ValVertDress.Tex("Assets/ValVert/Veg/pilot6/fg02_nor.jpg",true),Color.white,0.1f)},   // pilot6: Yughues CC0 forest ground, #9 bank + beds only
            {"bed_soil",M("bed_soil",ValVertDress.Tex(G+"stony_dirt_path_diff.jpg"),null,new Color(.55f,.45f,.38f),0.05f)},
            {"glass_green",M("glass_green",null,null,new Color(.42f,.62f,.45f),0.8f,0.2f)},
            {"door_maroon",M("door_maroon",ValVertDress.Tex(GEN+"corrugated_iron_02_diff_1k.jpg"),null,new Color(.45f,.16f,.17f),0.3f)},
            {"rock",M("rock",ValVertDress.Tex(G+"rock_wall_08_diff.jpg"),ValVertDress.Tex(G+"rock_wall_08_nor.jpg",true),new Color(.8f,.8f,.78f),0.1f)},
            {"path_concrete",M("path_concrete",ValVertDress.Tex(G+"concrete_floor_02_diff.jpg"),null,new Color(.8f,.8f,.78f),0.1f)},
            {"lamp_black",M("lamp_black",null,null,new Color(.05f,.05f,.06f),0.5f,0.5f)},
            {"sign_danger",M("sign_danger",ValVertDress.Tex(GEN+"sign_danger.png"),null,Color.white,0.2f).Also()},
        };
        MATS=mats; var ml=JsonUtility.FromJson<ML>(File.ReadAllText(SRC+file));
        var root=new GameObject(rootName); root.transform.SetParent(parent,false);
        foreach(var it in ml.items){ if(it.v==null||it.v.Length==0) continue; if(it.name=="EastBuildings"||it.name=="EastRoofs") continue;
            var me=new Mesh{indexFormat=IndexFormat.UInt32,name=it.name};
            var vs=new Vector3[it.v.Length/3]; for(int i=0;i<vs.Length;i++) vs[i]=new Vector3(it.v[3*i],it.v[3*i+1],it.v[3*i+2]);
            var uv=new Vector2[vs.Length]; for(int i=0;i<vs.Length&&2*i+1<it.uv.Length;i++) uv[i]=new Vector2(it.uv[2*i],it.uv[2*i+1]);
            me.vertices=vs; me.uv=uv; me.triangles=it.t; me.RecalculateNormals(); me.RecalculateTangents(); me.RecalculateBounds();
            if(it.mat=="water"){ var n=me.normals; for(int i=0;i<n.Length;i++) n[i]=Vector3.up; me.normals=n; }
            AssetDatabase.CreateAsset(me,GEN+"m_"+it.name+".asset");
            var go=new GameObject(it.name); go.transform.SetParent(root.transform,false); go.AddComponent<MeshFilter>().sharedMesh=me; var mr=go.AddComponent<MeshRenderer>(); mr.sharedMaterial=mats[it.mat];
            if(it.mat=="ring") mr.shadowCastingMode=ShadowCastingMode.Off; else if(it.mat=="kerb"||it.mat=="paint"){ mr.shadowCastingMode=(it.mat=="paint"?ShadowCastingMode.Off:ShadowCastingMode.On); } else if(it.mat!="water") go.AddComponent<MeshCollider>().sharedMesh=me; else mr.shadowCastingMode=ShadowCastingMode.Off;
            L($"mesh {it.name} verts={vs.Length} mat={it.mat}"); }
    }
    public static Terrain East;
    public static Terrain BuildEast(Terrain main){
        int R=1025; byte[] raw=File.ReadAllBytes(SRC+"east_height.raw"); var h=new float[R,R];
        for(int y=0;y<R;y++)for(int x=0;x<R;x++) h[y,x]=(raw[(y*R+x)*2]|(raw[(y*R+x)*2+1]<<8))/65535f;
        var td=new TerrainData(); td.heightmapResolution=R; td.size=new Vector3(432f,80.056f,709.293f); td.SetHeights(0,0,h);
        var ti=(TextureImporter)AssetImporter.GetAtPath(GEN+"east_ortho.jpg"); ti.maxTextureSize=4096; ti.wrapMode=TextureWrapMode.Clamp; ti.SaveAndReimport();
        var tl=new TerrainLayer{diffuseTexture=AssetDatabase.LoadAssetAtPath<Texture2D>(GEN+"east_ortho.jpg"),tileSize=new Vector2(432f,709.293f),tileOffset=Vector2.zero,smoothness=0};
        AssetDatabase.CreateAsset(tl,GEN+"east_ortho.terrainlayer"); td.terrainLayers=new[]{tl};
        AssetDatabase.CreateAsset(td,"Assets/ValVert/ValVertTerrainEast.asset");
        var go=Terrain.CreateTerrainGameObject(td); go.name="ValVertTerrainEast"; go.transform.position=new Vector3(532f,0.002f,-354.646f);
        var t=go.GetComponent<Terrain>(); t.materialTemplate=main.materialTemplate; t.SetNeighbors(main,null,null,null); main.SetNeighbors(null,null,t,null);
        t.treeDistance=main.treeDistance; t.treeBillboardDistance=main.treeBillboardDistance; East=t; L("east terrain built"); return t;
    }
    // self-made crossed-quad grass clumps, chunked
    public static void Grass(Terrain terr){
        var ti=(TextureImporter)AssetImporter.GetAtPath(GEN+"grass_blades.png"); ti.alphaIsTransparency=true; ti.mipMapsPreserveCoverage=true; ti.alphaTestReferenceValue=0.5f; ti.wrapMode=TextureWrapMode.Clamp; ti.SaveAndReimport();
        var gm=ValVertDress.Mat("grass_blades",AssetDatabase.LoadAssetAtPath<Texture2D>(GEN+"grass_blades.png"),null,true,new Color(.95f,.95f,.85f),0.05f); gm.SetFloat("_Cutoff",0.5f);
        var ex=ValVertDress.Mask("veg_excl.png"); var fm=ValVertDress.Mask("canopy_orig.png"); var me=ValVertDress.Mask("veg_meadow.png");
        var rnd=new System.Random(5); System.Func<float> R=()=>(float)rnd.NextDouble();
        float CH=32f; var root=new GameObject("V3_Grass"); long total=0; int chunks=0;
        float tp=terr.transform.position.y;
        for(float cx=-532;cx<532;cx+=CH)for(float cz=-354.6f;cz<354.6f;cz+=CH){
            bool park = cx>-380&&cx<500&&cz>-340&&cz<140;
            float sp= park?0.75f:1.6f;
            var V=new List<Vector3>(); var N=new List<Vector3>(); var U=new List<Vector2>(); var C=new List<Color>(); var T=new List<int>();
            for(float x=cx;x<cx+CH;x+=sp)for(float z=cz;z<cz+CH;z+=sp){
                float px=x+R()*sp, pz=z+R()*sp; if(px>531||pz>354) continue;
                if(ValVertDress.S(ex,px,pz)>0.3f || Lab(px,pz)!=0 || Hidden(new Vector3(px,0,pz)) || ShotClear(px,pz)&&new Vector2(px,pz).magnitude<0) continue;
                float f=ValVertDress.S(fm,px,pz), m=ValVertDress.S(me,px,pz);
                float p = f>0.5f?0.45f:1f; if(R()>p) continue;
                float hgt=(m>0.5f?0.32f:0.22f)+R()*0.2f; float w=0.4f+R()*0.3f;
                float y=terr.SampleHeight(new Vector3(px,0,pz))+tp-0.04f;
                var tint=Color.Lerp(new Color(.85f,.95f,.75f),new Color(1.05f,.95f,.7f),R()); if(f>0.5f) tint*=0.85f; tint.a=1;
                float a0=R()*Mathf.PI; float u0=Mathf.Floor(R()*2)*0.5f;
                for(int q=0;q<3;q++){ float a=a0+q*Mathf.PI/3; var d=new Vector3(Mathf.Cos(a),0,Mathf.Sin(a))*w*0.5f; var b=new Vector3(px,y,pz); int k=V.Count;
                    V.Add(b-d); V.Add(b-d+Vector3.up*hgt); V.Add(b+d+Vector3.up*hgt); V.Add(b+d);
                    for(int r=0;r<4;r++){ N.Add(Vector3.up); C.Add(tint);} U.Add(new Vector2(u0,0)); U.Add(new Vector2(u0,1)); U.Add(new Vector2(u0+0.5f,1)); U.Add(new Vector2(u0+0.5f,0));
                    T.AddRange(new[]{k,k+1,k+2,k,k+2,k+3}); }
            }
            if(V.Count==0) continue;
            var mesh=new Mesh{indexFormat=IndexFormat.UInt32}; mesh.SetVertices(V); mesh.SetNormals(N); mesh.SetUVs(0,U); mesh.SetColors(C); mesh.SetTriangles(T,0); mesh.RecalculateBounds();
            AssetDatabase.CreateAsset(mesh,GEN+$"grass_{chunks}.asset");
            var go=new GameObject("grass_"+chunks); go.transform.SetParent(root.transform,false); go.AddComponent<MeshFilter>().sharedMesh=mesh; var mr=go.AddComponent<MeshRenderer>(); mr.sharedMaterial=gm; mr.shadowCastingMode=ShadowCastingMode.Off;
            var lg=go.AddComponent<LODGroup>(); lg.SetLODs(new[]{new LOD(0.06f,new Renderer[]{mr})}); lg.RecalculateBounds();
            total+=V.Count/12; chunks++; }
        ValVertDress.Counts["_grass_clumps"]=(int)total; L($"grass clumps={total} chunks={chunks}");
    }

    static Material CM(string n,Color c,float sm,float met=0,bool emis=false){ var m=ValVertDress.Mat(n,null,null,false,c,sm); m.SetFloat("_Metallic",met); m.SetFloat("_Cull",0); if(emis){ m.EnableKeyword("_EMISSION"); m.SetColor("_EmissionColor",c*0.35f); m.globalIlluminationFlags=MaterialGlobalIlluminationFlags.None; } EditorUtility.SetDirty(m); return m; }
    static Material ParkingMat(){ var m=M("parking_lot",ValVertDress.Tex(GEN+"parking.png"),ValVertDress.Tex(GEN+"parking_nor.jpg",true),Color.white,1f);
        var ms=ValVertDress.Tex(GEN+"parking_ms.png"); if(ms){ var ti=(TextureImporter)AssetImporter.GetAtPath(GEN+"parking_ms.png"); if(ti&&ti.sRGBTexture){ ti.sRGBTexture=false; ti.SaveAndReimport(); } m.SetTexture("_MetallicGlossMap",ms); m.EnableKeyword("_METALLICSPECGLOSSMAP"); }
        EditorUtility.SetDirty(m); return m; }
    public static Material AsphaltWW(string n){ var m=M(n,ValVertDress.Tex(G+"asphalt_ww_diff.jpg"),ValVertDress.Tex(G+"asphalt_ww_nor.jpg",true),Color.white,1f);
        var ms=ValVertDress.Tex(G+"asphalt_ww_ms.png"); if(ms){ var ti=(TextureImporter)AssetImporter.GetAtPath(G+"asphalt_ww_ms.png"); if(ti&&ti.sRGBTexture){ ti.sRGBTexture=false; ti.SaveAndReimport(); }
            m.SetTexture("_MetallicGlossMap",ms); m.EnableKeyword("_METALLICSPECGLOSSMAP"); m.SetFloat("_Smoothness",1f); }
        m.SetFloat("_BumpScale",1.2f); EditorUtility.SetDirty(m); return m; }
    static Material DecalMat(string n){ var t=AssetDatabase.LoadAssetAtPath<Texture2D>(GEN+"decals/"+n+".png"); var m=ValVertDress.Mat("decal_"+n,t,null,false,Color.white,n=="oil_0"||n=="oil_1"||n=="oil_2"?0.75f:(n=="manhole"?0.35f:0.1f));
        if(n=="manhole"){ m.SetFloat("_Metallic",0.6f); m.SetFloat("_AlphaClip",1); m.SetFloat("_Cutoff",0.5f); m.EnableKeyword("_ALPHATEST_ON"); m.renderQueue=2460; return m; }
        m.SetFloat("_Surface",1); m.SetFloat("_Blend",0); m.SetFloat("_SrcBlend",(float)BlendMode.SrcAlpha); m.SetFloat("_DstBlend",(float)BlendMode.OneMinusSrcAlpha); m.SetFloat("_ZWrite",0); m.EnableKeyword("_SURFACE_TYPE_TRANSPARENT"); m.renderQueue=2990; m.SetOverrideTag("RenderType","Transparent"); EditorUtility.SetDirty(m); return m; }
    public static void Decals(){
        var root=new GameObject("V3_RoadDecals"); var added=new List<Collider>();
        foreach(var r in Object.FindObjectsByType<MeshRenderer>(FindObjectsSortMode.None)){ var mn=r.sharedMaterial?r.sharedMaterial.name.ToLower():""; if(!(mn.Contains("asphalt")||mn=="road"||mn.Contains("roadgrey"))) continue; if(mn.Contains("leaves")) continue;
            var mf=r.GetComponent<MeshFilter>(); if(!mf||!mf.sharedMesh) continue; var c=r.gameObject.AddComponent<MeshCollider>(); c.sharedMesh=mf.sharedMesh; added.Add(c); }
        Physics.SyncTransforms(); Debug.Log("[VV] decal colliders "+added.Count);
        string[] kinds={"oil_0","oil_1","oil_2","crack_0","crack_1","crack_2","patch_0","patch_1","manhole"}; var mats=kinds.ToDictionary(k=>k,k=>DecalMat(k));
        var quad=GameObject.CreatePrimitive(PrimitiveType.Quad); var qm=quad.GetComponent<MeshFilter>().sharedMesh; Object.DestroyImmediate(quad);
        int mask=~0; var rnd=new System.Random(2010); int n=0; var cnt=new Dictionary<string,int>();
        System.Action<Vector3,string,float> put=(p,k,size)=>{ RaycastHit h=default; bool ok=false; float bd=1e9f; foreach(var hh in Physics.RaycastAll(new Vector3(p.x,400f,p.z),Vector3.down,900f,mask)){ if(added.Contains(hh.collider)&&hh.distance<bd){bd=hh.distance;h=hh;ok=true;} } if(!ok) return; if(h.normal.y<0.85f) return;
            var g=new GameObject("decal_"+k); g.transform.SetParent(root.transform); g.AddComponent<MeshFilter>().sharedMesh=qm; var mr=g.AddComponent<MeshRenderer>(); mr.sharedMaterial=mats[k]; mr.shadowCastingMode=UnityEngine.Rendering.ShadowCastingMode.Off;
            g.transform.position=h.point+h.normal*(k=="manhole"?0.012f:0.008f+0.001f*(n%5)); g.transform.rotation=Quaternion.LookRotation(-h.normal)*Quaternion.Euler(0,0,(float)rnd.NextDouble()*360f);
            float sx=size*(k.StartsWith("patch")?(0.8f+(float)rnd.NextDouble()*1.6f):1f); g.transform.localScale=new Vector3(sx,size,1); n++; cnt[k]=(cnt.ContainsKey(k)?cnt[k]:0)+1; };
        // oil drips under/behind every parked car
        var cars=GameObject.Find("V3_Cars2010"); if(cars) foreach(Transform c in cars.transform){ var cp=c.position; foreach(Collider cc in c.GetComponentsInChildren<Collider>()) cc.enabled=false;
            put(cp+c.right*0.6f*(rnd.Next(2)*2-1),"oil_"+rnd.Next(3),0.7f+(float)rnd.NextDouble()*0.6f); put(cp+c.right*(1.4f+(float)rnd.NextDouble()),"oil_"+rnd.Next(3),0.5f+(float)rnd.NextDouble()*0.5f); }
        // scatter across all asphalt of the map
        for(int i=0;i<60000;i++){ var p=new Vector3(-400f+(float)rnd.NextDouble()*1300f,0,-450f+(float)rnd.NextDouble()*1100f); double u=rnd.NextDouble();
            if(u<0.020) put(p,"crack_"+rnd.Next(3),2.0f+(float)rnd.NextDouble()*2.5f);
            else if(u<0.030) put(p,"oil_"+rnd.Next(3),0.6f+(float)rnd.NextDouble()*1.2f);
            else if(u<0.036) {}
            else if(u<0.0385) put(p,"manhole",0.68f); }
        foreach(var c in added) Object.DestroyImmediate(c);
        Debug.Log("[VV] decals placed "+n+" "+string.Join(",",cnt.Select(kv=>kv.Key+"="+kv.Value)));
    }
    public static void Cars(){
        var root=new GameObject("V3_Cars2010");
        string[] kinds={"clio2","p206","megane2","picasso","clio2","kangoo","twingo1","p206","megane2"};
        // muted 2000s French fleet colours: silver, dark blue, bordeaux, grey, white, black, gris platine, vert foncé
        Color[] paints={new Color(.55f,.56f,.57f),new Color(.06f,.09f,.18f),new Color(.24f,.04f,.06f),new Color(.28f,.29f,.31f),new Color(.80f,.80f,.78f),new Color(.03f,.03f,.035f),new Color(.66f,.67f,.68f),new Color(.07f,.14f,.10f)};
        float[] metal={0.75f,0.6f,0.55f,0.7f,0.05f,0.4f,0.8f,0.5f};
        var mGlass=CM("car2_glass",new Color(.30f,.33f,.36f),0.9f,0.0f);
        var mChrome=CM("car2_chrome",new Color(.8f,.8f,.82f),0.9f,1f);
        var mTyre=CM("car2_tyre",new Color(.035f,.035f,.035f),0.18f);
        var mRim=CM("car2_rim",new Color(.42f,.43f,.45f),0.6f,0.8f);
        var mHub=CM("car2_hubcap",new Color(.36f,.37f,.39f),0.45f,0.3f);
        var mTrim=CM("car2_trim",new Color(.045f,.045f,.05f),0.3f);
        var mSeam=CM("car2_seam",new Color(.01f,.01f,.01f),0.1f);
        var mUnder=CM("car2_under",new Color(.02f,.02f,.02f),0.1f);
        var mHead=CM("car2_headlamp",new Color(.85f,.88f,.92f),0.95f,0.3f,true);
        var mTail=CM("car2_taillamp",new Color(.55f,.02f,.02f),0.92f,0f,true);
        var mInd=CM("car2_indicator",new Color(.85f,.42f,.05f),0.9f,0f,true);
        var mDirt=CM("car2_dirt",new Color(.16f,.14f,.11f),0.15f);
        var mBump=CM("car2_bumper_grey",new Color(.22f,.22f,.23f),0.25f);
        float[,] P={{478,52,0},{478,63,180},{474.5f,52,0},{471,63,180},{467.5f,52,0},
                    {580,-76,90},{580,-63,95},{573,-74,88},{566,-66,92},
                    {467,103,90},{461,103,270},{443,103.5f,92},
                    {437.5f,112f,0},{453.5f,121f,180},{445.5f,112.2f,2},
                    {434f,111.4f,0},{444f,121.7f,180},{456.5f,111.5f,3}};
        float[,] PO={{333f,206.8f,92},{338.6f,207.2f,268},{344.2f,206.6f,90}};   // Orme car park (user photo 01M6E4PR55): 3 parked cars
        { var Pn=new float[P.GetLength(0)+3,3]; for(int i=0;i<3;i++)for(int k=0;k<3;k++) Pn[i,k]=PO[i,k]; for(int i=0;i<P.GetLength(0);i++)for(int k=0;k<3;k++) Pn[i+3,k]=P[i,k]; P=Pn; }  // last 3 = GF roof deck (OSM 62591761)
        int DECK0=P.GetLength(0)-3;
        var tm=Terrain.activeTerrain; int placed=0;
        for(int i=0;i<P.GetLength(0);i++){
            string k=kinds[(i*7+2)%kinds.Length];
            var p0=AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Cars/v2/"+k+"_lod0.obj"); var p1=AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Cars/v2/"+k+"_lod1.obj");
            if(p0==null){Debug.Log("[VV] car missing "+k);continue;}
            var g=new GameObject("car_"+i+"_"+k); g.transform.SetParent(root.transform,false);
            var l0=(GameObject)Object.Instantiate(p0,g.transform); l0.name="LOD0"; var l1=p1?(GameObject)Object.Instantiate(p1,g.transform):null; if(l1) l1.name="LOD1";
            int ci=(i*5+3)%paints.Length; var pc=paints[ci];
            var paint=CM("car2_paint_"+i,pc,0.86f,metal[ci]); paint.SetFloat("_ClearCoat",1); 
            bool oldbump=(k=="clio2"||k=="kangoo"||k=="twingo1")&&(i%2==0);
            int pl=i%6;
            var pF=ValVertDress.Mat("car2_plateF_"+i,AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Cars/v2/plateF_"+pl+".png"),null,false,Color.white,0.55f);
            var pR=ValVertDress.Mat("car2_plateR_"+i,AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Cars/v2/plateR_"+pl+".png"),null,false,Color.white,0.55f);
            foreach(var r in g.GetComponentsInChildren<Renderer>()){ var ms=r.sharedMaterials; for(int q=0;q<ms.Length;q++){ string n=ms[q]?ms[q].name.ToLower():"";
                Material m=paint;
                if(n.StartsWith("glass")) m=mGlass; else if(n.StartsWith("chrome")) m=mChrome; else if(n.StartsWith("tyre")) m=mTyre; else if(n.StartsWith("rim")) m=mRim; else if(n.StartsWith("hubcap")) m=mHub;
                else if(n.StartsWith("trim")) m=mTrim; else if(n.StartsWith("seam")) m=mSeam; else if(n.StartsWith("under")) m=mUnder; else if(n.StartsWith("headlamp")) m=mHead; else if(n.StartsWith("taillamp")) m=mTail;
                else if(n.StartsWith("indicator")) m=mInd; else if(n.StartsWith("dirt")) m=mDirt; else if(n.StartsWith("platef")) m=pF; else if(n.StartsWith("plater")) m=pR; else if(n.StartsWith("bumper")) m=oldbump?mBump:paint;
                ms[q]=m; } r.sharedMaterials=ms; r.shadowCastingMode=UnityEngine.Rendering.ShadowCastingMode.On; }
            if(l1){ var lg=g.AddComponent<LODGroup>(); lg.SetLODs(new[]{ new LOD(0.18f,l0.GetComponentsInChildren<Renderer>()), new LOD(0.015f,l1.GetComponentsInChildren<Renderer>()) }); lg.RecalculateBounds(); }
            var pos=new Vector3(P[i,0],0,P[i,1]); Terrain t=(pos.x>532f&&East)?East:null;
            if(t==null){ foreach(var tt in Terrain.activeTerrains){ var tp=tt.transform.position; if(pos.x>=tp.x&&pos.x<=tp.x+tt.terrainData.size.x&&pos.z>=tp.z&&pos.z<=tp.z+tt.terrainData.size.z) t=tt; } }
            float y=(t?t.SampleHeight(pos)+t.transform.position.y:0)+0.07f; if(i>=DECK0) y=8.36f+0.03f;
            g.transform.position=new Vector3(pos.x,y,pos.z); g.transform.rotation=Quaternion.Euler(0,P[i,2]+90f,0);
            var b2=new Bounds(); bool f=true; foreach(var r in l0.GetComponentsInChildren<Renderer>()){ if(f){b2=r.bounds;f=false;} else b2.Encapsulate(r.bounds);} g.transform.position+=Vector3.up*(y-b2.min.y-0.005f);
            GameObjectUtility.SetStaticEditorFlags(g,StaticEditorFlags.BatchingStatic); placed++;
        }
        Debug.Log("[VV] cars placed "+placed);
    }
}
