// Ultimate Nature Starter (Innerverse Interactive, Asset Store EULA - vendor files are git-ignored) : preview + runtime env test.
using UnityEngine; using UnityEditor;
public static class ValVertUNS {
    const string U="Assets/InnerverseInteractive/Ultimate Nature – Starter/Environment/";
    public static void PreviewAndTest(){
        var scene=UnityEditor.SceneManagement.EditorSceneManager.OpenScene("Assets/Scenes/ValVert.unity");
        var terr=Terrain.activeTerrain; var root=new GameObject("uns_preview");
        string[] pf={"Trees/Fir/Prefabs/UNS_Spruce_01.prefab","Trees/Fir/Prefabs/UNS_Spruce_02.prefab","Vegetation/Bushes/Prefabs/UNS_Bush.prefab","Vegetation/Grass/Prefabs/UNS_Grass.prefab","Vegetation/Flowers/Prefabs/UNS_Flower.prefab"};
        float x=360f;
        foreach(var p in pf){ var a=AssetDatabase.LoadAssetAtPath<GameObject>(U+p); if(!a){Debug.Log("[VV] UNS missing "+p);continue;}
            var g=(GameObject)PrefabUtility.InstantiatePrefab(a); g.transform.SetParent(root.transform); var pos=new Vector3(x,0,112f); pos.y=terr.SampleHeight(pos)+terr.transform.position.y; g.transform.position=pos;
            var b=new Bounds(pos,Vector3.zero); foreach(var r in g.GetComponentsInChildren<Renderer>()) b.Encapsulate(r.bounds);
            Debug.Log($"[VV] UNS {p} size={b.size} mats={string.Join(",",System.Array.ConvertAll(g.GetComponentsInChildren<Renderer>()[0].sharedMaterials,m=>m?m.name+"/"+m.shader.name:"null"))}"); x+=4f; }
        var cam=new GameObject("c").AddComponent<Camera>(); cam.fieldOfView=60;
        var cp=new Vector3(368f,0,100f); cp.y=terr.SampleHeight(cp)+terr.transform.position.y+2f; cam.transform.position=cp; cam.transform.LookAt(new Vector3(368f,cp.y+1f,112f));
        var rt=new RenderTexture(1280,720,24); cam.targetTexture=rt; cam.Render(); RenderTexture.active=rt; var t=new Texture2D(1280,720,TextureFormat.RGB24,false); t.ReadPixels(new Rect(0,0,1280,720),0,0);
        System.IO.Directory.CreateDirectory("/workspace/valvert/uns_eval"); System.IO.File.WriteAllBytes("/workspace/valvert/uns_eval/uns_lineup.png",t.EncodeToPNG()); RenderTexture.active=null;
        Object.DestroyImmediate(cam.gameObject); Object.DestroyImmediate(root);
        ValVertEnvWire.Test();
    }
}
