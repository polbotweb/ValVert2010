using UnityEditor; using UnityEngine; using System.Collections.Generic;
public static class ValVertAldi {
    public static void Rebuild(){
        UnityEditor.SceneManagement.EditorSceneManager.OpenScene("Assets/Scenes/ValVert.unity");
        if(ValVertWoods.MATS==null) ValVertWoods.MATS=new Dictionary<string,Material>();
        var asp=AssetDatabase.LoadAssetAtPath<Material>("Assets/ValVert/Mats/asphalt_road.mat");
        if(asp!=null) ValVertWoods.MATS["asphalt"]=asp;
        var paint=AssetDatabase.LoadAssetAtPath<Material>("Assets/ValVert/Mats/road_paint.mat");
        if(paint!=null){ paint.SetColor("_BaseColor",new Color(.92f,.92f,.90f)); EditorUtility.SetDirty(paint); ValVertWoods.MATS["paint"]=paint; }
        var metal=AssetDatabase.LoadAssetAtPath<Material>("Assets/ValVert/Mats/metal_grey.mat");
        var rock=AssetDatabase.LoadAssetAtPath<Material>("Assets/ValVert/Mats/rock.mat");
        if(rock!=null) ValVertWoods.MATS["rock"]=rock;

                // Parking_Aldi = lot asphalt (plain). Never touch city-wide Parking mat (would wipe other lots).
        var asphalt=AssetDatabase.LoadAssetAtPath<Material>("Assets/ValVert/Mats/asphalt_road.mat");
        foreach(var r in Object.FindObjectsByType<MeshRenderer>(FindObjectsSortMode.None)){
            if(!r) continue;
            string gn=r.gameObject.name;
            if(gn!="Parking_Aldi"&&!gn.EndsWith("_Aldi")) continue;
            if(asphalt!=null) r.sharedMaterial=asphalt;
            r.enabled=true;
            Debug.Log($"[VV] Aldi asphalt ON: {gn} enabled bounds={r.bounds.min}-{r.bounds.max}");
        }

        var old=GameObject.Find("V3_AldiParking"); if(old) Object.DestroyImmediate(old);
        AssetDatabase.DeleteAsset("Assets/ValVert/Gen/m_aldi_cover.asset");
        ValVertWoods.AldiParking();
        var ap=GameObject.Find("V3_AldiParking");
        if(ap&&metal) foreach(var r in ap.GetComponentsInChildren<MeshRenderer>())
            if(r.gameObject.name.ToLower().Contains("bollard")) r.sharedMaterial=metal;

        // Respawn cars from committed v2 OBJs (fixes floating body/chassis), then ONLY root XZ+yaw for Aldi slots
        { var oldCars=GameObject.Find("V3_Cars2010"); if(oldCars) Object.DestroyImmediate(oldCars); }
        ValVertWoods.Cars();
        var cars=GameObject.Find("V3_Cars2010");
        if(cars){
            // Cars() already placed Aldi slots at indices 3..; re-assert root transform only (no child/bounds edits)
            var list=new List<Transform>();
            foreach(Transform c in cars.transform) list.Add(c);
            // Aldi cars are those whose Cars()-placed XZ falls in lot (after Cars uses AldiCarSlots)
            int n=ValVertWoods.AldiCarSlots.GetLength(0), hit=0;
            foreach(var c in list){
                var p=c.position;
                if(!(p.x>ValVertWoods.ALDI_WALL_X-1f&&p.x<506f&&p.z>40f&&p.z<84f)) continue;
                if(hit>=n){ c.position=new Vector3(0,-50,0); hit++; continue; }
                float x=ValVertWoods.AldiCarSlots[hit,0], z=ValVertWoods.AldiCarSlots[hit,1], yaw=ValVertWoods.AldiCarSlots[hit,2];
                // keep the Y Cars() computed (wheels on ground); only overwrite XZ + yaw on ROOT
                c.SetPositionAndRotation(new Vector3(x, c.position.y, z), Quaternion.Euler(0f, yaw, 0f));
                var mrs=c.GetComponentsInChildren<MeshRenderer>();
                if(mrs.Length>0){ var bb=mrs[0].bounds; for(int qi=1;qi<mrs.Length;qi++) bb.Encapsulate(mrs[qi].bounds);
                  Debug.Log($"[VV] Aldi car[{hit}] {c.name} rootYaw={yaw} worldXZ=({bb.size.x:F2},{bb.size.z:F2}) {(bb.size.z>bb.size.x*1.2f?"N-S":"E-W")}"); }
                hit++;
            }
            Debug.Log($"[VV] Aldi cars root-only placed {Mathf.Min(hit,n)}/{n}");
        }
        var sa=GameObject.Find("V3_StairApproach"); if(sa) Object.DestroyImmediate(sa);
        ValVertWoods.StairApproach();
        if(!GameObject.Find("V3_BinStoreDoor")) ValVertWoods.BinStoreDoor();

        UnityEditor.SceneManagement.EditorSceneManager.SaveScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene());
        AssetDatabase.SaveAssets();
        Debug.Log("[VV] ALDI REBUILT");
    }
}
