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
    public static Dictionary<string,Material> MATS;
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
            {"roof_flat",M("roof_flat",ValVertDress.Tex("Assets/ValVert/Ground/cc0/Gravel022_col.jpg"),ValVertDress.Tex("Assets/ValVert/Ground/cc0/Gravel022_nor.jpg",true),new Color(.75f,.75f,.75f),0.05f).Also()},
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
            {"kerb",M("kerb",ValVertDress.Tex("Assets/ValVert/Ground/cc0/Concrete034_col.jpg"),ValVertDress.Tex("Assets/ValVert/Ground/cc0/Concrete034_nor.jpg",true),new Color(.78f,.77f,.74f),0.1f)},
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
            {"render_beige",M("render_beige",ValVertDress.Tex("Assets/ValVert/Ground/cc0/Plaster007_col.jpg"),ValVertDress.Tex("Assets/ValVert/Ground/cc0/Plaster007_nor.jpg",true),new Color(.84f,.80f,.72f),0.1f)},
            {"glass_door",M("glass_door",null,null,new Color(.16f,.20f,.23f),0.9f,0.2f)},
            {"render_grey",M("render_grey",ValVertDress.Tex("Assets/ValVert/Ground/cc0/PaintedPlaster017_col.jpg"),ValVertDress.Tex("Assets/ValVert/Ground/cc0/PaintedPlaster017_nor.jpg",true),new Color(.78f,.77f,.74f),0.1f)},
            {"render_white",M("render_white",ValVertDress.Tex("Assets/ValVert/Ground/cc0/Plaster001_col.jpg"),ValVertDress.Tex("Assets/ValVert/Ground/cc0/Plaster001_nor.jpg",true),new Color(.80f,.79f,.76f),0.1f).Also()},
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
            {"path_concrete",M("path_concrete",ValVertDress.Tex("Assets/ValVert/Ground/cc0/Concrete047A_col.jpg"),ValVertDress.Tex("Assets/ValVert/Ground/cc0/Concrete047A_nor.jpg",true),new Color(.8f,.8f,.78f),0.1f)},
            {"lamp_black",M("lamp_black",null,null,new Color(.05f,.05f,.06f),0.5f,0.5f)},
            {"sign_danger",M("sign_danger",ValVertDress.Tex(GEN+"sign_danger.png"),null,Color.white,0.2f).Also()},
        };
        MATS=mats; var ml=JsonUtility.FromJson<ML>(File.ReadAllText(SRC+file));
        // Aldi car park (user correction t164u): only parallel spaces along the Aldi wall -> the Aldi patch of the generic
        // "Parking" mesh (painted grid texture) becomes plain asphalt; parallel stalls are drawn separately (AldiParking)
        { var lst=new List<MI>(ml.items); foreach(var it in ml.items){ if(it.name!="Parking"||it.mat!="parking"||it.t==null) continue;
            var keep=new List<int>(); var al=new List<int>();
            for(int i=0;i+2<it.t.Length;i+=3){ float cx=0,cz=0; for(int k=0;k<3;k++){ cx+=it.v[3*it.t[i+k]]; cz+=it.v[3*it.t[i+k]+2]; } cx/=3; cz/=3; (AldiLot(cx,cz)?al:keep).AddRange(new[]{it.t[i],it.t[i+1],it.t[i+2]}); }
            if(al.Count>0){ it.t=keep.ToArray(); lst.Add(new MI{name="Parking_Aldi",mat="asphalt",v=it.v,uv=it.uv,t=al.ToArray()}); L($"Aldi lot: {al.Count/3} parking tris -> plain asphalt"); } }
          ml.items=lst.ToArray(); }
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
    // Aldi car park rebuilt from user satellite (t172u supersedes parallel-only):
    // building west (x~467), Val Vert road north (+z), main road curves east. NO bays against facade.
    public const float ALDI_WALL_X=467.0f, ALDI_Z0=45.9f, ALDI_Z1=76.2f;
    public static bool AldiLot(float x,float z)=>x>ALDI_WALL_X-2f&&x<520f&&z>38f&&z<86f;  // generous — catch large tris
    // bay size (FR perpendicular): 2.5 x 5.0 m; disabled 3.3 x 5.0; aisle ~6 m
    public const float BAY_W=2.5f, BAY_D=5.0f, DIS_W=3.3f;
    // valid car slots filled by Cars() — world XZ + yaw (degrees, 0 = +Z)
    public static readonly float[,] AldiCarSlots = {
        // MESH euler Y (applied as-is). World AABB: yaw0 => sizeXZ~(4.2,2.0)=E-W sideways; yaw90 => N-S nose-in along bay long axis.
        // top row: nose-in toward north hedge (+Z via local +X at yaw90)
        {477.5f,75.5f,90},{480.0f,75.5f,90},{485.0f,75.5f,90},
        // island north row: nose toward island centre (-Z)
        {479.5f,64.5f,-90},{484.5f,64.5f,-90},
        // island south row: nose toward island centre (+Z)
        {479.5f,55.0f,90},{482.0f,55.0f,90},
        // bottom near entrance: nose-in toward south (-Z)
        {481.0f,49.0f,-90},
    };
    public static void AldiParking(){
        var root=new GameObject("V3_AldiParking"); Physics.SyncTransforms();
        var am=AssetDatabase.LoadAssetAtPath<Material>("Assets/ValVert/Mats/asphalt_road.mat");
        if(am==null&&MATS!=null&&MATS.ContainsKey("asphalt")) am=MATS["asphalt"];
        var pm=AssetDatabase.LoadAssetAtPath<Material>("Assets/ValVert/Mats/road_paint.mat");
        if(pm!=null) pm.SetColor("_BaseColor",new Color(.92f,.92f,.90f));
        else pm=M("aldi_bay_paint",null,null,new Color(.92f,.92f,.90f),0.15f);
        var blue=M("aldi_disabled_blue",null,null,new Color(.12f,.35f,.78f),0.35f);
        var grass=M("aldi_strip_grass",ValVertDress.Tex("Assets/ValVert/Ground/leafy_grass_diff.jpg"),ValVertDress.Tex("Assets/ValVert/Ground/leafy_grass_nor.jpg",true),new Color(.45f,.72f,.32f),0.05f);
        var bollardMat=AssetDatabase.LoadAssetAtPath<Material>("Assets/ValVert/Mats/metal_grey.mat")
            ?? AssetDatabase.LoadAssetAtPath<Material>("Assets/ValVert/Mats/bollard_m.mat")
            ?? ValVertDress.Mat("aldi_bollard",null,null,false,new Color(.55f,.55f,.52f),0.35f);
        // 1) plain asphalt cover — hides every baked parking.png marking on this lot
        float x0=ALDI_WALL_X+0.05f, x1=520f, z0l=40.0f, z1l=83.0f;
        { var me=new Mesh{name="aldi_cover"}; var vs=new List<Vector3>(); var uv=new List<Vector2>(); var ts=new List<int>(); int gx=16,gz=14;
          for(int iz=0;iz<=gz;iz++) for(int ix=0;ix<=gx;ix++){ float x=Mathf.Lerp(x0,x1,ix/(float)gx), z=Mathf.Lerp(z0l,z1l,iz/(float)gz); vs.Add(new Vector3(x,Hgt(x,z)+0.10f,z)); uv.Add(new Vector2((x-x0)*0.25f,(z-z0l)*0.25f)); }
          for(int iz=0;iz<gz;iz++) for(int ix=0;ix<gx;ix++){ int a=iz*(gx+1)+ix; ts.AddRange(new[]{a,a+gx+1,a+1, a+1,a+gx+1,a+gx+2}); }  // winding so normals face +Y
          me.SetVertices(vs); me.SetUVs(0,uv); me.SetTriangles(ts,0); me.RecalculateNormals(); me.RecalculateBounds(); AssetDatabase.CreateAsset(me,GEN+"m_aldi_cover.asset");
          var go=new GameObject("aldi_cover"); go.transform.SetParent(root.transform,false); go.AddComponent<MeshFilter>().sharedMesh=me; var mr=go.AddComponent<MeshRenderer>(); if(am) mr.sharedMaterial=am; mr.shadowCastingMode=ShadowCastingMode.Off; }
        // helpers
        System.Action<string,float,float,float,float,float> line=(nm,ax,az,bx,bz,w)=>{ var a=new Vector3(ax,0,az); var b=new Vector3(bx,0,bz); var mid=(a+b)*0.5f; float len=Vector3.Distance(a,b);
            float y=Hgt(mid.x,mid.z); var g=GameObject.CreatePrimitive(PrimitiveType.Quad); Object.DestroyImmediate(g.GetComponent<Collider>()); g.name=nm; g.transform.SetParent(root.transform,false);
            g.transform.position=new Vector3(mid.x,y+0.22f,mid.z); g.transform.rotation=Quaternion.LookRotation(Vector3.down,(b-a).normalized); g.transform.localScale=new Vector3(w,len,1);
            var mr=g.GetComponent<MeshRenderer>(); if(pm) mr.sharedMaterial=pm; mr.shadowCastingMode=ShadowCastingMode.Off; };
        System.Action<string,float,float,float,float,Material,float> patch=(nm,cx,cz,sx,sz,mat,dy)=>{ float y=Hgt(cx,cz); var g=GameObject.CreatePrimitive(PrimitiveType.Quad); Object.DestroyImmediate(g.GetComponent<Collider>()); g.name=nm; g.transform.SetParent(root.transform,false);
            g.transform.position=new Vector3(cx,y+dy+0.16f,cz); g.transform.rotation=Quaternion.Euler(90,0,0); g.transform.localScale=new Vector3(sx,sz,1); var mr=g.GetComponent<MeshRenderer>(); mr.sharedMaterial=mat; mr.shadowCastingMode=ShadowCastingMode.Off; };
        // draw one perpendicular bay: nose toward +Z (dirZ=+1) or -Z (dirZ=-1); left edge at x0b, nose line at znose
        System.Action<string,float,float,float,bool> bay=(nm,x0b,znose,dirZ,disabled)=>{
            float x1b=x0b+(disabled?DIS_W:BAY_W); float zBack=znose-dirZ*BAY_D;
            if(disabled) patch(nm+"_blue",(x0b+x1b)*0.5f,(znose+zBack)*0.5f,DIS_W-0.15f,BAY_D-0.2f,blue,0.035f);
            line(nm+"_L",x0b,znose,x0b,zBack,0.10f); line(nm+"_R",x1b,znose,x1b,zBack,0.10f); line(nm+"_N",x0b,znose,x1b,znose,0.10f); };
        // 2) facade green strip + bollards (NO parking against the wall)
        float stripX0=ALDI_WALL_X+0.15f, stripX1=ALDI_WALL_X+2.3f;
        patch("aldi_facade_strip",(stripX0+stripX1)*0.5f,(ALDI_Z0+ALDI_Z1)*0.5f,stripX1-stripX0,ALDI_Z1-ALDI_Z0-0.5f,grass,0.06f);
        for(float z=ALDI_Z0+1.5f; z<ALDI_Z1-1f; z+=3.2f){ float x=stripX1-0.25f; float y=Hgt(x,z); var b=GameObject.CreatePrimitive(PrimitiveType.Cylinder); Object.DestroyImmediate(b.GetComponent<Collider>());
            b.name="aldi_bollard"; b.transform.SetParent(root.transform,false); b.transform.position=new Vector3(x,y+0.45f,z); b.transform.localScale=new Vector3(0.18f,0.45f,0.18f);
            b.GetComponent<MeshRenderer>().sharedMaterial=bollardMat; }
        // 3) top row — 6 nose-in bays against north hedge (znose≈78), starting east of the facade strip
        float topZ=78.0f, topX0=474.5f; for(int i=0;i<7;i++) bay($"aldi_top_{i}",topX0+i*BAY_W,topZ,+1f,false);
        // 4) central island — two back-to-back rows (~6 each), blue disabled at LEFT (west) end of both
        float islX0=475.0f; // west end = disabled, after facade aisle
        float islNorthNose=66.5f, islSouthNose=52.5f;
        bay("aldi_islN_0",islX0,islNorthNose,+1f,true);
        for(int i=1;i<6;i++) bay($"aldi_islN_{i}",islX0+DIS_W+(i-1)*BAY_W,islNorthNose,+1f,false);
        bay("aldi_islS_0",islX0,islSouthNose,-1f,true);
        for(int i=1;i<6;i++) bay($"aldi_islS_{i}",islX0+DIS_W+(i-1)*BAY_W,islSouthNose,-1f,false);
        // island centre = asphalt (bay backs meet); no grass rectangle
        { float ix0=islX0, ix1=islX0+DIS_W+5*BAY_W; float zc=(islSouthNose+islNorthNose)*0.5f;
          float gap=islNorthNose-islSouthNose-2*BAY_D;
          if(gap>0.3f&&am!=null) patch("aldi_island_asphalt",(ix0+ix1)*0.5f,zc,ix1-ix0-0.3f,Mathf.Max(0.4f,gap-0.1f),am,0.02f);
          // back-to-back centre line
          line("aldi_island_centre",ix0+0.2f,zc,ix1-0.2f,zc,0.10f); }
        // 5) bottom bays near entrance (south) — 3 perpendicular, nose -Z toward road/entrance
        float botZ=47.5f, botX0=475.0f; for(int i=0;i<5;i++) bay($"aldi_bot_{i}",botX0+i*BAY_W,botZ,-1f,false);
        // 6) EV charger post near bottom entrance
        { float ex=485.0f, ez=48.5f; float y=Hgt(ex,ez); var p=GameObject.CreatePrimitive(PrimitiveType.Cube); Object.DestroyImmediate(p.GetComponent<Collider>());
          p.name="aldi_ev_charger"; p.transform.SetParent(root.transform,false); p.transform.position=new Vector3(ex,y+0.7f,ez); p.transform.localScale=new Vector3(0.25f,1.4f,0.2f);
          p.GetComponent<MeshRenderer>().sharedMaterial=M("aldi_ev",null,null,new Color(.15f,.15f,.16f),0.4f,0.3f);
          var h=GameObject.CreatePrimitive(PrimitiveType.Cube); Object.DestroyImmediate(h.GetComponent<Collider>()); h.name="aldi_ev_head"; h.transform.SetParent(root.transform,false);
          h.transform.position=new Vector3(ex,y+1.35f,ez-0.05f); h.transform.localScale=new Vector3(0.35f,0.35f,0.15f);
          h.GetComponent<MeshRenderer>().sharedMaterial=M("aldi_ev_head",null,null,new Color(.85f,.85f,.2f),0.5f); }
        // 7) lower-right grass verge (curved-road corner)
        // grass verge along curved road (east) — thin strip, not a mid-lot rectangle
        patch("aldi_verge_se",508.5f,52.0f,6.0f,22.0f,grass,0.03f);
        // single tree on verge (OSM-style) — use existing terrain tree if available; else skip
        
        // disable scene Parking materials over the Aldi AABB so baked UV markings cannot show through the cover
        foreach(var r in Object.FindObjectsByType<MeshRenderer>(FindObjectsSortMode.None)){
          if(!r||!r.sharedMaterial) continue; var n=r.sharedMaterial.name.ToLower();
          if(r.name=="Parking_Aldi"){
                var asphaltPlain=AssetDatabase.LoadAssetAtPath<Material>("Assets/ValVert/Mats/asphalt_road.mat");
                if(asphaltPlain!=null) r.sharedMaterial=asphaltPlain;
                r.enabled=true; L("Aldi: Parking_Aldi asphalt ON");
            }
        }
        // Wipe old diagonal fan / facade stubs: destroy orphan paint quads; force asphalt on any parking_lot in lot
        var asphaltWipe=AssetDatabase.LoadAssetAtPath<Material>("Assets/ValVert/Mats/asphalt_road.mat");
        foreach(var r in Object.FindObjectsByType<MeshRenderer>(FindObjectsSortMode.None)){
          if(!r) continue;
          var t=r.transform; if(t.root!=null&&t.root.name=="V3_AldiParking") continue;
          string gn=r.gameObject.name.ToLower(); var mat=r.sharedMaterial; string mn=mat?mat.name.ToLower():"";
          var ctr=r.bounds.center;
          bool inLot=ctr.x>ALDI_WALL_X-1f&&ctr.x<522f&&ctr.z>38f&&ctr.z<86f;
          if(!inLot){ var p=t.position; inLot=p.x>ALDI_WALL_X-1f&&p.x<522f&&p.z>38f&&p.z<86f; }
          if(!inLot) continue;
          if(gn=="parking"||gn=="parking_aldi"){ if(asphaltWipe) r.sharedMaterial=asphaltWipe; r.enabled=true; continue; }
          bool paintish=mn.Contains("paint")||mn.Contains("parking")||gn.Contains("aldi_")||gn.Contains("stub")||gn.Contains("bay")||gn.Contains("stall")||gn.Contains("mark");
          if(paintish) Object.DestroyImmediate(r.gameObject);
          else if(mn.Contains("parking")&&asphaltWipe!=null) r.sharedMaterial=asphaltWipe;
        }
        // Extra opaque asphalt patches over the east fan zone + facade stub zone
        if(asphaltWipe!=null){
          System.Action<string,float,float,float,float> coverPatch=(nm,cx,cz,sx,sz)=>{
            float y=Hgt(cx,cz); var g=GameObject.CreatePrimitive(PrimitiveType.Quad); Object.DestroyImmediate(g.GetComponent<Collider>());
            g.name=nm; g.transform.SetParent(root.transform,false);
            g.transform.position=new Vector3(cx,y+0.11f,cz); g.transform.rotation=Quaternion.Euler(90,0,0);
            g.transform.localScale=new Vector3(sx,sz,1); var mr=g.GetComponent<MeshRenderer>(); mr.sharedMaterial=asphaltWipe; mr.shadowCastingMode=ShadowCastingMode.Off;
          };
          System.Action<string,float,float,float,float> coverHi=(nm,cx,cz,sx,sz)=>{
            float y=Hgt(cx,cz); var g=GameObject.CreatePrimitive(PrimitiveType.Quad); Object.DestroyImmediate(g.GetComponent<Collider>());
            g.name=nm; g.transform.SetParent(root.transform,false);
            g.transform.position=new Vector3(cx,y+0.18f,cz); g.transform.rotation=Quaternion.Euler(90,0,0);
            g.transform.localScale=new Vector3(sx,sz,1); var mr=g.GetComponent<MeshRenderer>(); mr.sharedMaterial=asphaltWipe; mr.shadowCastingMode=ShadowCastingMode.Off;
          };
          // east of our bays only (our bays end ~x=492): bury diagonal fan + top-right grid
          coverHi("aldi_fan_cover",506f,58f,16f,34f);
          coverHi("aldi_tr_grid_cover",505f,76f,14f,10f);
          // west of our bays (facade stubs between green strip and aisle) — keep clear of bay x>=474.5
          coverHi("aldi_facade_stub_cover",ALDI_WALL_X+3.2f,61f,4.0f,32f);
        }
                L($"Aldi parking: satellite layout — facade strip+bollards, top7, island 2x6 (disabled W), bottom5 + EV, no fan/stubs, no facade bays"); }
    static float Hgt(float x,float z){ if(Physics.Raycast(new Vector3(x,400,z),Vector3.down,out var h,800f)) return h.point.y; var t=(x>532f&&East)?East:Terrain.activeTerrain; return t.SampleHeight(new Vector3(x,0,z))+t.transform.position.y; }
    // #9 bin store (user t165u + photo 01M6E4PNA47): grey metal door on the bin store's south wall (z=134.4, faces the log stairs / alley end),
    // nearer the alley end as in the photo, with a small Vigik-type badge reader beside it (geometry only, no branding)
    public static void BinStoreDoor(){
        // south wall runs (361.40,134.59) -> (364.90,134.41): 3.5 m; door 1.4 m centred 363.5 (alley/camera end), reader just east of it
        var root=new GameObject("V3_BinStoreDoor"); var A=new Vector3(361.40f,0,134.59f); var B=new Vector3(364.90f,0,134.41f); var dir=(B-A).normalized;
        float xc=363.5f, w=1.4f, h=2.05f; var c0=A+dir*((xc-A.x)/dir.x);
        float y0=Physics.Raycast(new Vector3(c0.x,14.6f,c0.z-0.6f),Vector3.down,out var gh,6f)?gh.point.y:12.41f;
        root.transform.position=new Vector3(c0.x,y0,c0.z); root.transform.rotation=Quaternion.FromToRotation(Vector3.right,dir);   // local +x along wall, local -z = outward (south)
        var door=M("bin_door_metal",null,null,new Color(.40f,.42f,.43f),0.35f,0.35f); var frame=M("bin_door_frame",null,null,new Color(.30f,.31f,.32f),0.3f,0.4f);
        var plate=M("bin_door_notice",null,null,new Color(.86f,.85f,.80f),0.2f); var rdr=M("vigik_body",null,null,new Color(.10f,.10f,.11f),0.45f,0.2f); var pad=M("vigik_pad",null,null,new Color(.18f,.20f,.24f),0.85f,0f);
        System.Action<string,Material,Vector3,Vector3> box=(nm,m,c,sz)=>{ var g=GameObject.CreatePrimitive(PrimitiveType.Cube); Object.DestroyImmediate(g.GetComponent<Collider>()); g.name=nm; g.transform.SetParent(root.transform,false); g.transform.localPosition=c; g.transform.localScale=sz; g.GetComponent<MeshRenderer>().sharedMaterial=m; };
        box("door_frame",frame,new Vector3(0,h/2+0.04f,-0.02f),new Vector3(w+0.12f,h+0.08f,0.05f));
        box("door_leaf",door,new Vector3(0,h/2,-0.045f),new Vector3(w,h,0.03f));
        box("door_seam",frame,new Vector3(-w*0.30f,h/2,-0.062f),new Vector3(0.010f,h-0.02f,0.006f));
        box("door_handle",frame,new Vector3(-w*0.30f+0.07f,1.05f,-0.075f),new Vector3(0.03f,0.40f,0.03f));   // narrow dark vertical pull bar, left side
        box("door_notice",plate,new Vector3(0.02f,1.62f,-0.063f),new Vector3(0.20f,0.28f,0.005f));            // small pale notice, upper panel
        box("vigik_reader",rdr,new Vector3(w/2+0.25f,1.25f,-0.03f),new Vector3(0.085f,0.13f,0.035f));
        box("vigik_pad",pad,new Vector3(w/2+0.25f,1.27f,-0.049f),new Vector3(0.06f,0.06f,0.004f));
        float WZ=c0.z;
        L($"bin store door at x={xc} z={WZ} y0={y0:F2}"); }
    // approach to the log stairs (photo 01M6E4PNA47 / user t167u): round-log barrier on the LEFT (south) of the narrow walkway,
    // round vertical posts + two round rails, and an irregular grey boulder at its foot in the foreground
    public static void StairApproach(){
        var root=new GameObject("V3_StairApproach"); Physics.SyncTransforms();
        var wood=M("log_fence",ValVertDress.Tex("Assets/ValVert/Veg/tex/fir_tree_01_bark_diff_1k.jpg"),ValVertDress.Tex("Assets/ValVert/Veg/tex/fir_tree_01_bark_nor_gl_1k.jpg",true),new Color(.62f,.55f,.48f),0.08f);
        var rockM=MATS!=null&&MATS.ContainsKey("rock")?MATS["rock"]:wood;
        System.Func<float,float,float> gy=(x,z)=>{ var t=Terrain.activeTerrain; float ty=t.SampleHeight(new Vector3(x,0,z))+t.transform.position.y; if(Physics.Raycast(new Vector3(x,ty+3f,z),Vector3.down,out var hh,6f)&&!(hh.collider is TerrainCollider)) return Mathf.Max(ty,hh.point.y); return ty; };
        System.Action<string,Vector3,Vector3,float> log=(nm,a,b,r)=>{ var g=GameObject.CreatePrimitive(PrimitiveType.Cylinder); Object.DestroyImmediate(g.GetComponent<Collider>()); g.name=nm; g.transform.SetParent(root.transform,false);
            g.transform.position=(a+b)/2; g.transform.up=(b-a).normalized; g.transform.localScale=new Vector3(r*2,Vector3.Distance(a,b)/2,r*2); g.GetComponent<MeshRenderer>().sharedMaterial=wood; };
        // fence line: along the wooded slope edge (alley west edge, x~361.0), from the stair foot (z 131.9) towards the viewer (south) - alley stays clear
        float fx=362.15f; var posts=new List<Vector3>(); for(float zz=131.6f;zz>=123.5f;zz-=1.35f) posts.Add(new Vector3(fx,gy(fx,zz),zz));
        for(int i=0;i<posts.Count;i++){ var p=posts[i]; log("post_"+i,p+Vector3.down*0.25f,p+Vector3.up*1.0f,0.07f); }
        for(int i=0;i+1<posts.Count;i++) foreach(var hgt in new[]{0.45f,0.88f}) log($"rail_{i}_{hgt}",posts[i]+Vector3.up*hgt,posts[i+1]+Vector3.up*hgt,0.055f);
        // boulder: UNS forest rock (CC0 pack already imported), low+wide, partly sunk, left foreground by fence
        float bx=363.15f, bz=125.6f;
        var rockPrefab=AssetDatabase.LoadAssetAtPath<GameObject>("Assets/InnerverseInteractive/Ultimate Nature – Starter/Environment/Rocks/Forest/Prefabs/UNS_Standard_Rock_03.prefab");
        GameObject bo;
        if(rockPrefab!=null){ bo=(GameObject)Object.Instantiate(rockPrefab); foreach(var c in bo.GetComponentsInChildren<Collider>()) Object.DestroyImmediate(c); }
        else { bo=GameObject.CreatePrimitive(PrimitiveType.Sphere); Object.DestroyImmediate(bo.GetComponent<Collider>()); if(rockM) bo.GetComponent<MeshRenderer>().sharedMaterial=rockM; }
        bo.name="boulder"; bo.transform.SetParent(root.transform,false);
        // low wide, sink ~40% into ground
        bo.transform.localScale=new Vector3(1.6f,0.85f,1.35f);
        bo.transform.rotation=Quaternion.Euler(8,48,-12);
        { float gy0=gy(bx,bz); var rend=bo.GetComponentsInChildren<MeshRenderer>();
          Bounds bb=rend.Length>0?rend[0].bounds:new Bounds(bo.transform.position,Vector3.one);
          foreach(var r in rend) bb.Encapsulate(r.bounds);
          bo.transform.position=new Vector3(bx, gy0 - bb.size.y*0.35f, bz); }
        L($"stair approach: {posts.Count} posts, boulder at {bx},{bz}");
        LogStairsDress(); Ramp9to13(); Batch2Dress(); }

    // Photo refs Nov 2026 (stairs top-down, path-to-carpark, woods-path-fence): sleeper steps + dual
    // handrails on the nº9 log stair, car-park edge boulders, woods-path stump. No Aldi changes.
    public static void LogStairsDress(){
        var root=GameObject.Find("V3_LogStairsDress"); if(root) Object.DestroyImmediate(root);
        root=new GameObject("V3_LogStairsDress"); Physics.SyncTransforms();
        var wood=M("log_fence",ValVertDress.Tex("Assets/ValVert/Veg/tex/fir_tree_01_bark_diff_1k.jpg"),ValVertDress.Tex("Assets/ValVert/Veg/tex/fir_tree_01_bark_nor_gl_1k.jpg",true),new Color(.55f,.48f,.40f),0.08f);
        var gravel=AssetDatabase.LoadAssetAtPath<Material>("Assets/ValVert/Mats/stair_earth.mat")
            ?? M("stair_gravel",ValVertDress.Tex("Assets/ValVert/Ground/forest_leaves_02_diff.jpg"),null,new Color(1.6f,1.4f,1.1f),0.05f);
        var rockM=MATS!=null&&MATS.ContainsKey("rock")?MATS["rock"]:wood;
        System.Func<float,float,float> gy=(x,z)=>{ var t=Terrain.activeTerrain; float ty=t.SampleHeight(new Vector3(x,0,z))+t.transform.position.y; if(Physics.Raycast(new Vector3(x,ty+4f,z),Vector3.down,out var hh,8f)&&!(hh.collider is TerrainCollider)) return Mathf.Max(ty,hh.point.y); return ty; };
        System.Action<string,Vector3,Vector3,float> log=(nm,a,b,r)=>{ var g=GameObject.CreatePrimitive(PrimitiveType.Cylinder); Object.DestroyImmediate(g.GetComponent<Collider>()); g.name=nm; g.transform.SetParent(root.transform,false);
            g.transform.position=(a+b)/2; g.transform.up=(b-a).normalized; g.transform.localScale=new Vector3(r*2,Vector3.Distance(a,b)/2,r*2); g.GetComponent<MeshRenderer>().sharedMaterial=wood; };
        // --- nº9 log staircase: sleepers + gravel treads, handrails BOTH sides ---
        // Slope runs roughly west(high, woods) -> east(low, alley). Match Bld_stair_earth AABB.
        float zC=133.2f; float xTop=356.4f, xBot=361.5f; int nSteps=14;
        for(int i=0;i<nSteps;i++){
            float t=(i+0.5f)/nSteps; float x=Mathf.Lerp(xTop,xBot,t);
            float y=gy(x,zC);
            // timber sleeper across the stair (N-S)
            float z0=zC-0.85f, z1=zC+0.85f;
            var a=new Vector3(x,y+0.06f,z0); var b=new Vector3(x,y+0.06f,z1);
            log($"sleeper_{i}",a,b,0.09f);
            // thin gravel/earth tread behind sleeper (west side)
            if(i+1<nSteps){
                float x2=Mathf.Lerp(xTop,xBot,(i+1.0f)/nSteps);
                var q=GameObject.CreatePrimitive(PrimitiveType.Cube); Object.DestroyImmediate(q.GetComponent<Collider>());
                q.name=$"tread_{i}"; q.transform.SetParent(root.transform,false);
                float xm=(x+x2)*0.5f; float ym=gy(xm,zC)+0.02f;
                q.transform.position=new Vector3(xm,ym,zC); q.transform.localScale=new Vector3(Mathf.Abs(x2-x)*0.92f,0.04f,1.55f);
                q.GetComponent<MeshRenderer>().sharedMaterial=gravel;
            }
        }
        // Handrails: posts + two rails, BOTH sides (south z~132.2 and north z~134.2)
        foreach(var (side,zR) in new[]{("S",zC-0.95f),("N",zC+0.95f)}){
            var posts=new List<Vector3>();
            for(int i=0;i<=nSteps;i+=2){
                float t=i/(float)nSteps; float x=Mathf.Lerp(xTop,xBot,t);
                posts.Add(new Vector3(x,gy(x,zR),zR));
            }
            for(int i=0;i<posts.Count;i++){ var p=posts[i]; log($"hr_{side}_post_{i}",p+Vector3.down*0.15f,p+Vector3.up*0.95f,0.065f); }
            for(int i=0;i+1<posts.Count;i++) foreach(var hgt in new[]{0.42f,0.82f})
                log($"hr_{side}_rail_{i}_{hgt}",posts[i]+Vector3.up*hgt,posts[i+1]+Vector3.up*hgt,0.05f);
        }
        // --- Car-park edge boulders (path-to-carpark photo): row along park edge near stair-top junction ---
        // Path from ~349,134 toward car park SE (~370,120). Boulders along asphalt edge ~x 368-378, z~118-126
        var rockPrefab=AssetDatabase.LoadAssetAtPath<GameObject>("Assets/InnerverseInteractive/Ultimate Nature – Starter/Environment/Rocks/Forest/Prefabs/UNS_Standard_Rock_03.prefab");
        var rockPrefab2=AssetDatabase.LoadAssetAtPath<GameObject>("Assets/InnerverseInteractive/Ultimate Nature – Starter/Environment/Rocks/Forest/Prefabs/UNS_Standard_Rock_01.prefab");
        float[,] boulders={{368.2f,125.5f,1.1f},{369.5f,123.8f,0.95f},{370.8f,122.2f,1.25f},{372.2f,120.8f,1.0f},{373.6f,119.5f,1.15f},{375.0f,118.4f,0.9f},{376.4f,117.5f,1.05f}};
        for(int i=0;i<boulders.GetLength(0);i++){
            float bx=boulders[i,0], bz=boulders[i,1], sc=boulders[i,2];
            GameObject bo;
            var pref=(i%2==0?rockPrefab:rockPrefab2)??rockPrefab;
            if(pref!=null){ bo=(GameObject)Object.Instantiate(pref); foreach(var c in bo.GetComponentsInChildren<Collider>()) Object.DestroyImmediate(c); }
            else { bo=GameObject.CreatePrimitive(PrimitiveType.Sphere); Object.DestroyImmediate(bo.GetComponent<Collider>()); if(rockM) bo.GetComponent<MeshRenderer>().sharedMaterial=rockM; }
            bo.name=$"park_boulder_{i}"; bo.transform.SetParent(root.transform,false);
            bo.transform.localScale=new Vector3(sc,sc*0.65f,sc*0.9f);
            bo.transform.rotation=Quaternion.Euler(5+i*7, i*37, -8+i*3);
            float gy0=gy(bx,bz);
            var rend=bo.GetComponentsInChildren<MeshRenderer>();
            Bounds bb=rend.Length>0?rend[0].bounds:new Bounds(bo.transform.position,Vector3.one);
            foreach(var r in rend) bb.Encapsulate(r.bounds);
            bo.transform.position=new Vector3(bx, gy0 - bb.size.y*0.30f, bz);
        }
        // --- Woods path stump (photo_woods_path_fence): right side of path near 350,136 ---
        float sx=351.2f, sz=136.8f;
        var stump=GameObject.CreatePrimitive(PrimitiveType.Cylinder); Object.DestroyImmediate(stump.GetComponent<Collider>());
        stump.name="woods_stump"; stump.transform.SetParent(root.transform,false);
        float sy=gy(sx,sz);
        stump.transform.position=new Vector3(sx,sy+0.28f,sz);
        stump.transform.localScale=new Vector3(0.85f,0.28f,0.85f);
        stump.GetComponent<MeshRenderer>().sharedMaterial=wood;
        // stump top disc slightly darker
        var top=GameObject.CreatePrimitive(PrimitiveType.Cylinder); Object.DestroyImmediate(top.GetComponent<Collider>());
        top.name="woods_stump_top"; top.transform.SetParent(stump.transform,false);
        top.transform.localPosition=new Vector3(0,1.05f,0); top.transform.localScale=new Vector3(1.05f,0.08f,1.05f);
        top.GetComponent<MeshRenderer>().sharedMaterial=M("stump_top",null,null,new Color(.45f,.38f,.28f),0.15f);
        // Extra fence segment along woods path left edge if missing (paved path / lawn separation)
        // Align with existing approach fence x~362 but extend north along woods path x~348-352, z~132-140
        float fxx=347.6f; var fposts=new List<Vector3>();
        for(float zz=132.5f; zz<=140.5f; zz+=1.4f) fposts.Add(new Vector3(fxx,gy(fxx,zz),zz));
        for(int i=0;i<fposts.Count;i++){ var p=fposts[i]; log($"wfence_post_{i}",p+Vector3.down*0.2f,p+Vector3.up*0.9f,0.06f); }
        for(int i=0;i+1<fposts.Count;i++) foreach(var hgt in new[]{0.40f,0.78f})
            log($"wfence_rail_{i}_{hgt}",fposts[i]+Vector3.up*hgt,fposts[i+1]+Vector3.up*hgt,0.048f);
        L($"log stairs dress: {nSteps} sleepers, {boulders.GetLength(0)} park boulders, stump+woods fence");
    }


    // Path/ramp nº9→nº13 joining rue du Val Vert (user plan_ramp_9to13 + OSM).
    // Flat along bin store (parking side), then ramps up to Val Vert; left log fence,
    // right sleeper stairs+gravel+boulder+manhole; hedge/bed/brick footpath by the block.
    // Rear car park + east lane from OSM parking_aisle ways 1378070581 / 1378070590.
    // No Aldi / car edits.
    public static void Ramp9to13(){
        var old=GameObject.Find("V3_Ramp9to13"); if(old) Object.DestroyImmediate(old);
        var root=new GameObject("V3_Ramp9to13"); Physics.SyncTransforms();
        var wood=M("log_fence",ValVertDress.Tex("Assets/ValVert/Veg/tex/fir_tree_01_bark_diff_1k.jpg"),ValVertDress.Tex("Assets/ValVert/Veg/tex/fir_tree_01_bark_nor_gl_1k.jpg",true),new Color(.55f,.48f,.40f),0.08f);
        var asp=AssetDatabase.LoadAssetAtPath<Material>("Assets/ValVert/Mats/asphalt_leaves.mat")
            ?? AssetDatabase.LoadAssetAtPath<Material>("Assets/ValVert/Mats/asphalt_road.mat");
        var gravel=AssetDatabase.LoadAssetAtPath<Material>("Assets/ValVert/Mats/stair_earth.mat")
            ?? M("stair_gravel",ValVertDress.Tex("Assets/ValVert/Ground/forest_leaves_02_diff.jpg"),null,new Color(1.6f,1.4f,1.1f),0.05f);
        var grass=M("ramp_island_grass",ValVertDress.Tex("Assets/ValVert/Ground/leafy_grass_diff.jpg"),ValVertDress.Tex("Assets/ValVert/Ground/leafy_grass_nor.jpg",true),new Color(.42f,.68f,.30f),0.05f);
        var hedge=AssetDatabase.LoadAssetAtPath<Material>("Assets/ValVert/Mats/hedge.mat")??wood;
        var brick=M("brick_path_red",ValVertDress.Tex("Assets/ValVert/Ground/cc0/Concrete047A_col.jpg"),ValVertDress.Tex("Assets/ValVert/Ground/cc0/Concrete047A_nor.jpg",true),new Color(.72f,.32f,.24f),0.12f);
        var rockM=MATS!=null&&MATS.ContainsKey("rock")?MATS["rock"]:wood;
        System.Func<float,float,float> gy=(x,z)=>{ var t=Terrain.activeTerrain; float ty=t.SampleHeight(new Vector3(x,0,z))+t.transform.position.y; if(Physics.Raycast(new Vector3(x,ty+5f,z),Vector3.down,out var hh,10f)&&!(hh.collider is TerrainCollider)) return Mathf.Max(ty,hh.point.y); return ty; };
        System.Action<string,Vector3,Vector3,float> log=(nm,a,b,r)=>{ var g=GameObject.CreatePrimitive(PrimitiveType.Cylinder); Object.DestroyImmediate(g.GetComponent<Collider>()); g.name=nm; g.transform.SetParent(root.transform,false);
            g.transform.position=(a+b)/2; g.transform.up=(b-a).normalized; g.transform.localScale=new Vector3(r*2,Vector3.Distance(a,b)/2,r*2); g.GetComponent<MeshRenderer>().sharedMaterial=wood; };
        System.Action<string,float,float,float,float,Material,float> patch=(nm,cx,cz,sx,sz,mat,yOff)=>{
            var q=GameObject.CreatePrimitive(PrimitiveType.Cube); Object.DestroyImmediate(q.GetComponent<Collider>());
            q.name=nm; q.transform.SetParent(root.transform,false);
            float y=gy(cx,cz)+yOff; q.transform.position=new Vector3(cx,y,cz); q.transform.localScale=new Vector3(sx,0.06f,sz);
            if(mat) q.GetComponent<MeshRenderer>().sharedMaterial=mat; };

        // --- Asphalt path centreline: Marche (parking edge ~z 132) flat along bin store, then ramp to Val Vert (~z 178) ---
        // Width ~3.5 m. Existing Env_asphalt_leaves covers much of this; reinforce flat+ramp strip and join.
        var pathPts=new List<Vector3>();
        // flat section west of / along bin store (OSM 62592236 x361-366 z134-147)
        for(float z=131.5f; z<=147f; z+=1.5f) pathPts.Add(new Vector3(362.4f,0,z));
        // ramp: shift slightly east toward building west court, climb to Val Vert
        for(float z=148.5f; z<=177.5f; z+=1.5f){
            float t=(z-148.5f)/(177.5f-148.5f);
            float x=Mathf.Lerp(363.5f,369.5f,t); // drifts east toward join near car park
            pathPts.Add(new Vector3(x,0,z));
        }
        for(int i=0;i+1<pathPts.Count;i++){
            var a=pathPts[i]; var b=pathPts[i+1];
            float cx=(a.x+b.x)*0.5f, cz=(a.z+b.z)*0.5f;
            float dx=b.x-a.x, dz=b.z-a.z; float len=Mathf.Sqrt(dx*dx+dz*dz);
            var q=GameObject.CreatePrimitive(PrimitiveType.Cube); Object.DestroyImmediate(q.GetComponent<Collider>());
            q.name=$"ramp_asp_{i}"; q.transform.SetParent(root.transform,false);
            float y=gy(cx,cz)+0.03f;
            q.transform.position=new Vector3(cx,y,cz);
            q.transform.rotation=Quaternion.LookRotation(new Vector3(dx,0,dz));
            q.transform.localScale=new Vector3(3.6f,0.05f,len+0.05f);
            if(asp) q.GetComponent<MeshRenderer>().sharedMaterial=asp;
        }

        // --- LEFT: two-rail round-log fence along wooded/parking bank (west of path) ---
        var fposts=new List<Vector3>();
        for(float z=132f; z<=176f; z+=1.45f){
            float t=Mathf.InverseLerp(132f,176f,z);
            float x=Mathf.Lerp(360.6f,367.2f,Mathf.Max(0f,(z-147f)/(176f-147f))); // track path west edge
            if(z<=147f) x=360.6f;
            fposts.Add(new Vector3(x,gy(x,z),z));
        }
        for(int i=0;i<fposts.Count;i++){ var p=fposts[i]; log($"ramp_fence_post_{i}",p+Vector3.down*0.2f,p+Vector3.up*0.95f,0.065f); }
        for(int i=0;i+1<fposts.Count;i++) foreach(var hgt in new[]{0.42f,0.82f})
            log($"ramp_fence_rail_{i}_{hgt}",fposts[i]+Vector3.up*hgt,fposts[i+1]+Vector3.up*hgt,0.05f);

        // --- RIGHT (upper ramp): timber sleeper stairs on gravel + round-log edging ---
        int nSleep=10;
        for(int i=0;i<nSleep;i++){
            float t=(i+0.5f)/nSleep;
            float z=Mathf.Lerp(152f,172f,t);
            float x=Mathf.Lerp(365.8f,372.0f,t); // east of path centre
            float y=gy(x,z);
            // sleeper across (E-W short)
            log($"ramp_sleeper_{i}", new Vector3(x-0.55f,y+0.05f,z), new Vector3(x+0.55f,y+0.05f,z), 0.085f);
            // gravel tread
            patch($"ramp_tread_{i}", x, z, 1.15f, 0.55f, gravel, 0.02f);
            // log edging east side
            if(i%2==0) log($"ramp_edge_{i}", new Vector3(x+0.7f,y-0.05f,z-0.4f), new Vector3(x+0.7f,y+0.55f,z+0.4f), 0.055f);
        }
        // grey boulder near bottom of these stairs
        {
            float bx=365.2f, bz=151.5f;
            var rockPrefab=AssetDatabase.LoadAssetAtPath<GameObject>("Assets/InnerverseInteractive/Ultimate Nature – Starter/Environment/Rocks/Forest/Prefabs/UNS_Standard_Rock_03.prefab");
            GameObject bo;
            if(rockPrefab!=null){ bo=(GameObject)Object.Instantiate(rockPrefab); foreach(var c in bo.GetComponentsInChildren<Collider>()) Object.DestroyImmediate(c); }
            else { bo=GameObject.CreatePrimitive(PrimitiveType.Sphere); Object.DestroyImmediate(bo.GetComponent<Collider>()); if(rockM) bo.GetComponent<MeshRenderer>().sharedMaterial=rockM; }
            bo.name="ramp_boulder"; bo.transform.SetParent(root.transform,false);
            bo.transform.localScale=new Vector3(1.15f,0.7f,1.0f); bo.transform.rotation=Quaternion.Euler(10,55,-8);
            float gy0=gy(bx,bz); var rend=bo.GetComponentsInChildren<MeshRenderer>();
            Bounds bb=rend.Length>0?rend[0].bounds:new Bounds(bo.transform.position,Vector3.one);
            foreach(var r in rend) bb.Encapsulate(r.bounds);
            bo.transform.position=new Vector3(bx, gy0 - bb.size.y*0.32f, bz);
        }
        // manhole cover on path near stairs bottom
        {
            var mhMat=AssetDatabase.LoadAssetAtPath<Material>("Assets/ValVert/Gen/decals/manhole.mat");
            if(mhMat==null){
                var tex=AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/ValVert/Gen/decals/manhole.png");
                mhMat=M("manhole_cover",tex,null,new Color(.35f,.36f,.37f),0.4f);
            }
            patch("ramp_manhole", 364.8f, 150.2f, 0.7f, 0.7f, mhMat, 0.045f);
        }

        // --- Further right: evergreen hedge + planted bed + log fence + brick-red footpath along block ---
        for(float z=148f; z<=168f; z+=1.2f){
            float x=373.2f;
            var h=GameObject.CreatePrimitive(PrimitiveType.Cube); Object.DestroyImmediate(h.GetComponent<Collider>());
            h.name=$"ramp_hedge_{z:F0}"; h.transform.SetParent(root.transform,false);
            h.transform.position=new Vector3(x, gy(x,z)+0.55f, z); h.transform.localScale=new Vector3(0.55f,1.1f,1.15f);
            h.GetComponent<MeshRenderer>().sharedMaterial=hedge;
        }
        // planted bed (laurel/ivy strip)
        patch("ramp_bed", 374.3f, 158f, 1.4f, 18f, gravel, 0.01f);
        // short log fence by bed
        var bposts=new List<Vector3>();
        for(float z=150f; z<=166f; z+=1.5f) bposts.Add(new Vector3(375.1f,gy(375.1f,z),z));
        for(int i=0;i<bposts.Count;i++){ var p=bposts[i]; log($"bed_fence_post_{i}",p+Vector3.down*0.15f,p+Vector3.up*0.7f,0.05f); }
        for(int i=0;i+1<bposts.Count;i++) log($"bed_fence_rail_{i}",bposts[i]+Vector3.up*0.55f,bposts[i+1]+Vector3.up*0.55f,0.04f);
        // narrow brick-red paved footpath along white apartment west face (~x 366.9 south wing / ~373 north)
        for(float z=128f; z<=167f; z+=1.4f){
            float x=(z<134f)?367.2f:373.8f;
            if(z>=134f&&z<148f) x=Mathf.Lerp(367.2f,373.8f,(z-134f)/14f);
            patch($"brick_fp_{z:F0}", x, z, 1.1f, 1.35f, brick, 0.025f);
        }

        // --- Val Vert join: grass island + white arrow on road ---
        patch("valvert_grass_island", 372.5f, 176.8f, 4.5f, 2.2f, grass, 0.04f);
        {
            var arrow=GameObject.CreatePrimitive(PrimitiveType.Cube); Object.DestroyImmediate(arrow.GetComponent<Collider>());
            arrow.name="valvert_white_arrow"; arrow.transform.SetParent(root.transform,false);
            float ax=370.2f, az=178.0f; float y=gy(ax,az)+0.04f;
            arrow.transform.position=new Vector3(ax,y,az);
            arrow.transform.rotation=Quaternion.Euler(90f, 90f, 0f); // flat, pointing roughly along street/join
            arrow.transform.localScale=new Vector3(1.8f, 0.7f, 0.02f);
            var am=M("road_arrow_white",null,null,new Color(.92f,.92f,.90f),0.15f);
            arrow.GetComponent<MeshRenderer>().sharedMaterial=am;
            // simple chevron tip
            var tip=GameObject.CreatePrimitive(PrimitiveType.Cube); Object.DestroyImmediate(tip.GetComponent<Collider>());
            tip.name="valvert_arrow_tip"; tip.transform.SetParent(root.transform,false);
            tip.transform.position=new Vector3(ax+1.1f,y,az); tip.transform.rotation=Quaternion.Euler(90f,45f,0f);
            tip.transform.localScale=new Vector3(0.7f,0.7f,0.02f); tip.GetComponent<MeshRenderer>().sharedMaterial=am;
        }
        L($"ramp 9to13: path segs={pathPts.Count-1}, fence posts={fposts.Count}, sleepers={nSleep}");
        RearPark91113();
    }

    // Lane around 9/11/13 + rear car park north of the block — footprints from OSM parking_aisle
    // ways 1378070581 (rear loop) and 1378070590 (east N-S lane). No invented bays beyond OSM centreline.
    public static void RearPark91113(){
        var old=GameObject.Find("V3_RearPark91113"); if(old) Object.DestroyImmediate(old);
        var root=new GameObject("V3_RearPark91113"); Physics.SyncTransforms();
        var asp=AssetDatabase.LoadAssetAtPath<Material>("Assets/ValVert/Mats/asphalt_road.mat")
            ?? AssetDatabase.LoadAssetAtPath<Material>("Assets/ValVert/Mats/parking_lot.mat");
        System.Func<float,float,float> gy=(x,z)=>{ var t=Terrain.activeTerrain; float ty=t.SampleHeight(new Vector3(x,0,z))+t.transform.position.y; if(Physics.Raycast(new Vector3(x,ty+5f,z),Vector3.down,out var hh,10f)&&!(hh.collider is TerrainCollider)) return Mathf.Max(ty,hh.point.y); return ty; };
        System.Action<string,float,float,float,float,float> strip=(nm,x0,z0,x1,z1,w)=>{
            float cx=(x0+x1)*0.5f, cz=(z0+z1)*0.5f; float dx=x1-x0, dz=z1-z0; float len=Mathf.Sqrt(dx*dx+dz*dz);
            if(len<0.05f) return;
            var q=GameObject.CreatePrimitive(PrimitiveType.Cube); Object.DestroyImmediate(q.GetComponent<Collider>());
            q.name=nm; q.transform.SetParent(root.transform,false);
            q.transform.position=new Vector3(cx, gy(cx,cz)+0.025f, cz);
            q.transform.rotation=Quaternion.LookRotation(new Vector3(dx,0,dz));
            q.transform.localScale=new Vector3(w,0.05f,len+0.08f);
            if(asp) q.GetComponent<MeshRenderer>().sharedMaterial=asp;
        };
        // East lane OSM 1378070590: (397.56,178.9) → (398.27,113.36), ~6 m wide service
        strip("east_lane", 397.56f,178.9f, 398.27f,113.36f, 5.5f);
        // Rear parking aisle OSM 1378070581 centreline (full loop)
        var rear=new Vector2[]{
            new Vector2(371.42f,178.11f), new Vector2(371.43f,206.0f), new Vector2(372.16f,207.79f),
            new Vector2(373.54f,208.75f), new Vector2(376.24f,209.65f), new Vector2(378.89f,209.7f),
            new Vector2(382.33f,208.64f), new Vector2(383.33f,207.75f), new Vector2(385.4f,205.95f),
            new Vector2(386.19f,203.88f), new Vector2(386.51f,200.77f), new Vector2(386.67f,189.82f)
        };
        for(int i=0;i+1<rear.Length;i++)
            strip($"rear_aisle_{i}", rear[i].x,rear[i].y, rear[i+1].x,rear[i+1].y, 5.0f);
        // Parking pad: fill between aisle west/east legs and building north wall (z~168.3) — OSM evidence of aisle enclosure
        // West leg x~371.4 z 178→206; east leg x~386.6 z 190→206; south = building north ~z 168.3
        {
            float x0=371.4f+2.5f, x1=386.6f-2.5f, z0=168.5f, z1=205.5f;
            // main pad as a few slabs (avoid covering aisle strips entirely)
            float slabW=(x1-x0);
            for(float z=z0; z<z1; z+=6f){
                float zh=Mathf.Min(6f, z1-z);
                var q=GameObject.CreatePrimitive(PrimitiveType.Cube); Object.DestroyImmediate(q.GetComponent<Collider>());
                q.name=$"rear_pad_{z:F0}"; q.transform.SetParent(root.transform,false);
                float cz=z+zh*0.5f, cx=(x0+x1)*0.5f;
                q.transform.position=new Vector3(cx, gy(cx,cz)+0.02f, cz);
                q.transform.localScale=new Vector3(slabW, 0.045f, zh);
                if(asp) q.GetComponent<MeshRenderer>().sharedMaterial=asp;
            }
        }
        L("rear park 9/11/13: east lane + OSM aisle loop + pad north of block");
    }


    // Batch2 photo refs (Nov 2026): nº9 entrance, ramp details, footpath, rear park lamps,
    // Val Vert junction/crossing/sidewalk, wood edge paths. 2010-safe (skip clearly post-2010 signage text).
    // No Aldi / car edits.
    public static void Batch2Dress(){
        var old=GameObject.Find("V3_Batch2Dress"); if(old) Object.DestroyImmediate(old);
        var root=new GameObject("V3_Batch2Dress"); Physics.SyncTransforms();
        var wood=M("log_fence",ValVertDress.Tex("Assets/ValVert/Veg/tex/fir_tree_01_bark_diff_1k.jpg"),ValVertDress.Tex("Assets/ValVert/Veg/tex/fir_tree_01_bark_nor_gl_1k.jpg",true),new Color(.55f,.48f,.40f),0.08f);
        var metal=M("door_alum_grey",null,null,new Color(.42f,.44f,.46f),0.45f,0.35f);
        var glass=M("door_glass",null,null,new Color(.55f,.62f,.70f,0.35f),0.9f,0f);
        var plaster=AssetDatabase.LoadAssetAtPath<Material>("Assets/ValVert/Mats/plaster.mat");
        var panel=M("facade_panel_grey",null,null,new Color(.62f,.63f,.64f),0.25f);
        var asp=AssetDatabase.LoadAssetAtPath<Material>("Assets/ValVert/Mats/asphalt_leaves.mat")
            ?? AssetDatabase.LoadAssetAtPath<Material>("Assets/ValVert/Mats/asphalt_road.mat");
        var pebble=M("pebble_bed",ValVertDress.Tex("Assets/ValVert/Ground/forest_leaves_02_diff.jpg"),null,new Color(1.4f,1.35f,1.3f),0.05f);
        var hedge=AssetDatabase.LoadAssetAtPath<Material>("Assets/ValVert/Mats/hedge.mat")??wood;
        var concrete=AssetDatabase.LoadAssetAtPath<Material>("Assets/ValVert/Mats/path_concrete.mat")
            ?? M("agg_path",ValVertDress.Tex("Assets/ValVert/Ground/cc0/Concrete047A_col.jpg"),ValVertDress.Tex("Assets/ValVert/Ground/cc0/Concrete047A_nor.jpg",true),new Color(.75f,.74f,.70f),0.12f);
        var rockM=MATS!=null&&MATS.ContainsKey("rock")?MATS["rock"]:wood;
        var white=M("sign_white",null,null,new Color(.92f,.92f,.90f),0.2f);
        var zebra=M("zebra_white",null,null,new Color(.95f,.95f,.93f),0.15f);
        System.Func<float,float,float> gy=(x,z)=>{ var t=Terrain.activeTerrain; float ty=t.SampleHeight(new Vector3(x,0,z))+t.transform.position.y; if(Physics.Raycast(new Vector3(x,ty+5f,z),Vector3.down,out var hh,10f)&&!(hh.collider is TerrainCollider)) return Mathf.Max(ty,hh.point.y); return ty; };
        System.Action<string,Vector3,Vector3,float> log=(nm,a,b,r)=>{ var g=GameObject.CreatePrimitive(PrimitiveType.Cylinder); Object.DestroyImmediate(g.GetComponent<Collider>()); g.name=nm; g.transform.SetParent(root.transform,false);
            g.transform.position=(a+b)/2; g.transform.up=(b-a).normalized; g.transform.localScale=new Vector3(r*2,Vector3.Distance(a,b)/2,r*2); g.GetComponent<MeshRenderer>().sharedMaterial=wood; };
        System.Action<string,Vector3,Vector3,Material> box=(nm,pos,sc,mat)=>{ var q=GameObject.CreatePrimitive(PrimitiveType.Cube); Object.DestroyImmediate(q.GetComponent<Collider>()); q.name=nm; q.transform.SetParent(root.transform,false); q.transform.position=pos; q.transform.localScale=sc; if(mat) q.GetComponent<MeshRenderer>().sharedMaterial=mat; };

        // ===== 01: nº9 entrance (south wing facing parking/forecourt ~x 372, z 116) =====
        // OSM apartments south face z~116; place entrance near center of south wall
        float ex=374.5f, ez=116.15f; float ey=gy(ex,ez);
        // canopy band
        box("n9_canopy", new Vector3(ex, ey+2.55f, ez-0.15f), new Vector3(4.2f, 0.35f, 0.55f), metal);
        // '9' plaque
        box("n9_plaque", new Vector3(ex, ey+2.55f, ez-0.42f), new Vector3(0.28f, 0.28f, 0.04f), M("n9_blue",null,null,new Color(.12f,.22f,.55f),0.3f));
        // round wall lamp
        { var lamp=GameObject.CreatePrimitive(PrimitiveType.Sphere); Object.DestroyImmediate(lamp.GetComponent<Collider>());
          lamp.name="n9_wall_lamp"; lamp.transform.SetParent(root.transform,false);
          lamp.transform.position=new Vector3(ex+0.85f, ey+2.55f, ez-0.45f); lamp.transform.localScale=Vector3.one*0.22f;
          lamp.GetComponent<MeshRenderer>().sharedMaterial=M("globe_lamp",null,null,new Color(.95f,.95f,.90f),0.85f); }
        // door leaf + angled panel + sidelights
        box("n9_door", new Vector3(ex-0.15f, ey+1.1f, ez-0.12f), new Vector3(1.0f, 2.15f, 0.08f), metal);
        box("n9_door_glass", new Vector3(ex-0.15f, ey+1.25f, ez-0.16f), new Vector3(0.75f, 1.5f, 0.02f), glass);
        box("n9_door_angle", new Vector3(ex+0.25f, ey+1.15f, ez-0.14f), new Vector3(0.55f, 1.7f, 0.06f), metal);
        box("n9_sideL", new Vector3(ex-0.95f, ey+1.2f, ez-0.12f), new Vector3(0.55f, 1.9f, 0.06f), metal);
        box("n9_sideL_g", new Vector3(ex-0.95f, ey+1.25f, ez-0.15f), new Vector3(0.4f, 1.6f, 0.02f), glass);
        box("n9_sideR", new Vector3(ex+0.95f, ey+1.2f, ez-0.12f), new Vector3(0.9f, 1.9f, 0.06f), metal);
        box("n9_sideR_g", new Vector3(ex+0.95f, ey+1.25f, ez-0.15f), new Vector3(0.7f, 1.6f, 0.02f), glass);
        box("n9_interphone", new Vector3(ex+0.55f, ey+1.35f, ez-0.18f), new Vector3(0.18f, 0.35f, 0.05f), M("interphone",null,null,new Color(.15f,.15f,.16f),0.4f));
        // handle
        box("n9_handle", new Vector3(ex-0.55f, ey+1.05f, ez-0.22f), new Vector3(0.04f, 0.55f, 0.04f), M("handle_brushed",null,null,new Color(.7f,.72f,.74f),0.6f));
        // roller shutter window left of door
        box("n9_shutter", new Vector3(ex-2.4f, ey+1.4f, ez-0.1f), new Vector3(1.3f, 1.4f, 0.08f), M("shutter_grey",null,null,new Color(.72f,.73f,.74f),0.25f));
        // asphalt forecourt + kerbs + manhole + pebbles right + log fence left
        box("n9_forecourt", new Vector3(ex, ey+0.02f, ez-3.2f), new Vector3(7.5f, 0.05f, 5.5f), asp);
        box("n9_kerbL", new Vector3(ex-3.6f, ey+0.08f, ez-3.0f), new Vector3(0.2f, 0.15f, 5.0f), concrete);
        box("n9_kerbR", new Vector3(ex+3.6f, ey+0.08f, ez-3.0f), new Vector3(0.2f, 0.15f, 5.0f), concrete);
        box("n9_pebbles", new Vector3(ex+2.8f, ey+0.06f, ez-1.2f), new Vector3(1.4f, 0.08f, 2.2f), pebble);
        box("n9_manhole", new Vector3(ex+1.8f, ey+0.045f, ez-2.5f), new Vector3(0.7f, 0.03f, 0.7f), M("mh",null,null,new Color(.35f,.36f,.37f),0.4f));
        // log fence left of forecourt
        for(int i=0;i<4;i++){ float zz=ez-1.5f-i*1.2f; float xx=ex-3.9f; var p0=new Vector3(xx,gy(xx,zz),zz);
            log($"n9_fence_post_{i}", p0+Vector3.down*0.15f, p0+Vector3.up*0.85f, 0.06f);
            if(i>0){ var prev=new Vector3(xx,gy(xx,zz+1.2f),zz+1.2f); foreach(var h in new[]{0.4f,0.75f}) log($"n9_fence_rail_{i}_{h}",prev+Vector3.up*h,p0+Vector3.up*h,0.045f); } }

        // ===== 03: aggregate footpath along block west/base + kerb + shed + lamp + CCTV =====
        for(float z=117f; z<=155f; z+=1.2f){
            float x=366.9f; if(z>130f) x=Mathf.Lerp(366.9f,373.5f,Mathf.Clamp01((z-130f)/20f));
            box($"agg_fp_{z:F0}", new Vector3(x, gy(x,z)+0.03f, z), new Vector3(1.35f, 0.05f, 1.15f), concrete);
            box($"agg_kerb_{z:F0}", new Vector3(x-0.85f, gy(x,z)+0.12f, z), new Vector3(0.18f, 0.22f, 1.1f), concrete);
        }
        // grey panel strip along base (visual cue)
        for(float z=118f; z<=150f; z+=2.0f){
            float x=367.4f; if(z>130f) x=Mathf.Lerp(367.4f,374.0f,Mathf.Clamp01((z-130f)/20f));
            box($"base_panel_{z:F0}", new Vector3(x+0.35f, gy(x,z)+0.7f, z), new Vector3(0.08f, 1.3f, 1.9f), panel);
        }
        box("path_shed", new Vector3(365.5f, gy(365.5f,154f)+0.7f, 154f), new Vector3(1.6f, 1.4f, 1.2f), panel);
        { var lp=GameObject.CreatePrimitive(PrimitiveType.Cylinder); Object.DestroyImmediate(lp.GetComponent<Collider>());
          lp.name="path_lamp"; lp.transform.SetParent(root.transform,false);
          float lx=366.2f, lz=155.5f; float ly=gy(lx,lz);
          lp.transform.position=new Vector3(lx, ly+2.2f, lz); lp.transform.localScale=new Vector3(0.12f,2.2f,0.12f);
          lp.GetComponent<MeshRenderer>().sharedMaterial=metal;
          var globe=GameObject.CreatePrimitive(PrimitiveType.Sphere); Object.DestroyImmediate(globe.GetComponent<Collider>());
          globe.name="path_lamp_globe"; globe.transform.SetParent(root.transform,false);
          globe.transform.position=new Vector3(lx, ly+4.5f, lz); globe.transform.localScale=Vector3.one*0.35f;
          globe.GetComponent<MeshRenderer>().sharedMaterial=M("globe_lamp2",null,null,new Color(.95f,.95f,.88f),0.85f); }
        box("cctv", new Vector3(367.6f, gy(367.6f,130f)+3.2f, 130f), new Vector3(0.15f, 0.12f, 0.25f), white);

        // ===== 08 refine: planted beds with log fences along block (flat ramp left) =====
        for(int b=0;b<3;b++){
            float bz=122f+b*6f; float bx=368.2f;
            box($"plant_bed_{b}", new Vector3(bx, gy(bx,bz)+0.05f, bz), new Vector3(2.2f, 0.15f, 3.5f), pebble);
            var posts=new List<Vector3>();
            foreach(var (px,pz) in new[]{(bx-1.0f,bz-1.5f),(bx+1.0f,bz-1.5f),(bx+1.0f,bz+1.5f),(bx-1.0f,bz+1.5f)})
                posts.Add(new Vector3(px,gy(px,pz),pz));
            for(int i=0;i<posts.Count;i++){ var p0=posts[i]; log($"bed2_post_{b}_{i}",p0+Vector3.down*0.1f,p0+Vector3.up*0.65f,0.05f);
                var p1=posts[(i+1)%posts.Count]; log($"bed2_rail_{b}_{i}",p0+Vector3.up*0.5f,p1+Vector3.up*0.5f,0.04f); }
        }
        // dual fence on flat ramp section (east side of path too) — photo 08
        for(float z=132f; z<=146f; z+=1.4f){
            float x=364.2f; var p0=new Vector3(x,gy(x,z),z);
            log($"flat_fenceE_post_{z:F0}", p0+Vector3.down*0.15f, p0+Vector3.up*0.9f, 0.06f);
            if(z>132.5f){ var prev=new Vector3(x,gy(x,z-1.4f),z-1.4f); foreach(var h in new[]{0.4f,0.78f}) log($"flat_fenceE_rail_{z:F0}_{h}",prev+Vector3.up*h,p0+Vector3.up*h,0.045f); }
        }

        // ===== 04: rear park globe lamps (no cars) along OSM aisle =====
        foreach(var (lx,lz) in new[]{(373f,185f),(373f,195f),(373f,205f),(382f,200f),(382f,190f)}){
            var pole=GameObject.CreatePrimitive(PrimitiveType.Cylinder); Object.DestroyImmediate(pole.GetComponent<Collider>());
            pole.name=$"rear_globe_pole_{lx}_{lz}"; pole.transform.SetParent(root.transform,false);
            float y=gy(lx,lz); pole.transform.position=new Vector3(lx,y+2.0f,lz); pole.transform.localScale=new Vector3(0.1f,2.0f,0.1f);
            pole.GetComponent<MeshRenderer>().sharedMaterial=metal;
            var g=GameObject.CreatePrimitive(PrimitiveType.Sphere); Object.DestroyImmediate(g.GetComponent<Collider>());
            g.name=$"rear_globe_{lx}_{lz}"; g.transform.SetParent(root.transform,false);
            g.transform.position=new Vector3(lx,y+4.15f,lz); g.transform.localScale=Vector3.one*0.4f;
            g.GetComponent<MeshRenderer>().sharedMaterial=M("globe_w",null,null,new Color(.95f,.95f,.9f),0.85f);
        }

        // ===== 05/07: Val Vert junction — hedge, ivy-tree proxy, raised zebra crossing, simple sign post =====
        // Crossing near OSM Val Vert x~364-371 z~178
        for(int i=0;i<5;i++){
            float x=360f+i*1.6f;
            box($"zebra_{i}", new Vector3(x, gy(x,178.1f)+0.04f, 178.1f), new Vector3(1.1f, 0.03f, 3.2f), zebra);
        }
        // raised plateau tint under crossing
        box("crossing_plateau", new Vector3(364f, gy(364f,178.1f)+0.02f, 178.1f), new Vector3(9f, 0.06f, 4.5f), M("plateau_tan",null,null,new Color(.62f,.55f,.42f),0.1f));
        // hedge opposite
        for(float x=350f; x<=375f; x+=1.3f){
            float z=182.5f; box($"vv_hedge_{x:F0}", new Vector3(x, gy(x,z)+1.0f, z), new Vector3(1.2f, 2.0f, 0.7f), hedge);
        }
        // sign post (generic 2010 pole + blank white plate — no modern FIN DE ZONE 30 lettering)
        box("vv_sign_pole", new Vector3(358.5f, gy(358.5f,176.5f)+1.6f, 176.5f), new Vector3(0.08f, 3.2f, 0.08f), metal);
        box("vv_sign_plate", new Vector3(358.5f, gy(358.5f,176.5f)+2.8f, 176.5f), new Vector3(0.7f, 0.55f, 0.04f), white);
        // direction fingers
        box("vv_dir1", new Vector3(355.0f, gy(355f,177f)+2.6f, 177f), new Vector3(1.2f, 0.18f, 0.05f), M("dir_grey",null,null,new Color(.55f,.55f,.52f),0.25f));
        box("vv_dir2", new Vector3(355.0f, gy(355f,177f)+2.35f, 177f), new Vector3(1.0f, 0.18f, 0.05f), M("dir_grey2",null,null,new Color(.5f,.5f,.48f),0.25f));
        // verge boulders at crossing (photo 07)
        var rockPrefab=AssetDatabase.LoadAssetAtPath<GameObject>("Assets/InnerverseInteractive/Ultimate Nature – Starter/Environment/Rocks/Forest/Prefabs/UNS_Standard_Rock_03.prefab");
        foreach(var (bx,bz,sc) in new[]{(368.5f,175.2f,1.0f),(370.2f,174.8f,0.85f),(372.0f,175.5f,1.1f)}){
            GameObject bo;
            if(rockPrefab!=null){ bo=(GameObject)Object.Instantiate(rockPrefab); foreach(var c in bo.GetComponentsInChildren<Collider>()) Object.DestroyImmediate(c); }
            else { bo=GameObject.CreatePrimitive(PrimitiveType.Sphere); Object.DestroyImmediate(bo.GetComponent<Collider>()); if(rockM) bo.GetComponent<MeshRenderer>().sharedMaterial=rockM; }
            bo.name=$"vv_verge_boulder_{bx}"; bo.transform.SetParent(root.transform,false);
            bo.transform.localScale=new Vector3(sc,sc*0.65f,sc*0.9f);
            float gy0=gy(bx,bz); var rend=bo.GetComponentsInChildren<MeshRenderer>();
            Bounds bb=rend.Length>0?rend[0].bounds:new Bounds(bo.transform.position,Vector3.one);
            foreach(var r in rend) bb.Encapsulate(r.bounds);
            bo.transform.position=new Vector3(bx, gy0 - bb.size.y*0.3f, bz);
        }
        // small grey utility box (photo 07)
        box("vv_util_box", new Vector3(356.5f, gy(356.5f,175.5f)+0.35f, 175.5f), new Vector3(0.7f, 0.7f, 0.5f), panel);

        // ===== 06: Val Vert sidewalk strip under hedge (west of road toward woods) =====
        for(float x=340f; x<=358f; x+=1.5f){
            float z=176.2f;
            box($"vv_sidewalk_{x:F0}", new Vector3(x, gy(x,z)+0.03f, z), new Vector3(1.4f, 0.05f, 1.6f), asp);
            box($"vv_side_hedge_{x:F0}", new Vector3(x, gy(x,z+2.2f)+1.1f, z+2.2f), new Vector3(1.3f, 2.2f, 0.6f), hedge);
        }

        // ===== 09: earth/gravel path into trees (near woods / parking edge) =====
        for(float t=0;t<=1.001f;t+=0.08f){
            float x=Mathf.Lerp(355f,348f,t); float z=Mathf.Lerp(128f,145f,t);
            box($"earth_path9_{t:F2}", new Vector3(x, gy(x,z)+0.025f, z), new Vector3(1.0f, 0.04f, 1.3f),
                AssetDatabase.LoadAssetAtPath<Material>("Assets/ValVert/Mats/path_dirt.mat")??pebble);
        }

        // ===== 10: extra mossy boulders at car-park edge (complements LogStairsDress row) =====
        foreach(var (bx,bz,sc) in new[]{(366.5f,126.8f,1.2f),(367.8f,125.0f,0.95f),(378.0f,118.0f,1.05f)}){
            GameObject bo;
            if(rockPrefab!=null){ bo=(GameObject)Object.Instantiate(rockPrefab); foreach(var c in bo.GetComponentsInChildren<Collider>()) Object.DestroyImmediate(c); }
            else { bo=GameObject.CreatePrimitive(PrimitiveType.Sphere); Object.DestroyImmediate(bo.GetComponent<Collider>()); if(rockM) bo.GetComponent<MeshRenderer>().sharedMaterial=rockM; }
            bo.name=$"park_edge_boulder_{bx}"; bo.transform.SetParent(root.transform,false);
            bo.transform.localScale=new Vector3(sc,sc*0.6f,sc*0.85f); bo.transform.rotation=Quaternion.Euler(8,40,-10);
            float gy0=gy(bx,bz); var rend=bo.GetComponentsInChildren<MeshRenderer>();
            Bounds bb=rend.Length>0?rend[0].bounds:new Bounds(bo.transform.position,Vector3.one);
            foreach(var r in rend) bb.Encapsulate(r.bounds);
            bo.transform.position=new Vector3(bx, gy0 - bb.size.y*0.32f, bz);
        }
        L("batch2 dress: n9 entrance, agg path, rear lamps, Val Vert crossing/hedge, earth path, park-edge boulders");
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
    static bool Built(float x,float z){ for(int k=0;k<5;k++){ var o=k==0?Vector3.zero:Quaternion.Euler(0,k*90,0)*Vector3.forward*0.6f; if(Physics.Raycast(new Vector3(x+o.x,400f,z+o.z),Vector3.down,out var h,800f)&&!(h.collider is TerrainCollider)) return true; } return false; }
    public static void Grass(Terrain terr){ Physics.SyncTransforms();
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
                if(Built(px,pz)) continue;   // keep stair treads / built surfaces clear (no tufts poking through steps)
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
        // Aldi cars from satellite slots (facade clear); then other lots unchanged
        float[,] Prest={{580,-76,90},{580,-63,95},{573,-74,88},{566,-66,92},
                    {467,103,90},{461,103,270},{443,103.5f,92},
                    {437.5f,112f,0},{453.5f,121f,180},{445.5f,112.2f,2},
                    {434f,111.4f,0},{444f,121.7f,180},{456.5f,111.5f,3}};
        float[,] P=new float[AldiCarSlots.GetLength(0)+Prest.GetLength(0),3];
        for(int ai=0;ai<AldiCarSlots.GetLength(0);ai++) for(int k=0;k<3;k++) P[ai,k]=AldiCarSlots[ai,k];
        for(int ai=0;ai<Prest.GetLength(0);ai++) for(int k=0;k<3;k++) P[AldiCarSlots.GetLength(0)+ai,k]=Prest[ai,k];
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
            int nAldi=AldiCarSlots.GetLength(0); bool isAldi=(i>=3&&i<3+nAldi);
            float yRot=isAldi?P[i,2]:P[i,2]+90f;
            g.transform.position=new Vector3(pos.x,y,pos.z); g.transform.rotation=Quaternion.Euler(0,yRot,0);
            var b2=new Bounds(); bool f=true; foreach(var r in l0.GetComponentsInChildren<Renderer>()){ if(f){b2=r.bounds;f=false;} else b2.Encapsulate(r.bounds);} g.transform.position+=Vector3.up*(y-b2.min.y-0.005f);
            GameObjectUtility.SetStaticEditorFlags(g,StaticEditorFlags.BatchingStatic); placed++;
        }
        Debug.Log("[VV] cars placed "+placed);
    }
}
