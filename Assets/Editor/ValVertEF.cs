// European Forests - Realistic Trees (MysticForge, Asset Store EULA; vendor files git-ignored) inspection.
using UnityEngine; using UnityEditor; using System.Linq;
public static class ValVertEF {
    public const string EF="Assets/Realistic Tree/Prefabs/URP/";
    public static void Inspect(){
        foreach(var p in AssetDatabase.FindAssets("t:Prefab",new[]{EF}).Select(AssetDatabase.GUIDToAssetPath)){
            var g=(GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(p));
            var rs=g.GetComponentsInChildren<Renderer>().Where(r=>!(r is ParticleSystemRenderer)).ToArray(); var b=rs[0].bounds; foreach(var r in rs) b.Encapsulate(r.bounds);
            var lg=g.GetComponentInChildren<LODGroup>(); int tris=0; var mf=rs[0].GetComponent<MeshFilter>(); if(mf&&mf.sharedMesh) tris=mf.sharedMesh.triangles.Length/3;
            Debug.Log($"[VV] EF {System.IO.Path.GetFileNameWithoutExtension(p)} size={b.size} lod={(lg?lg.lodCount:0)} rends={rs.Length} ps={g.GetComponentsInChildren<ParticleSystem>().Length} tri0={tris} mats={string.Join(",",rs.SelectMany(r=>r.sharedMaterials).Where(m=>m).Select(m=>m.name+"/"+m.shader.name).Distinct())}");
            Object.DestroyImmediate(g); }
    }
}
