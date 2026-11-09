using UnityEngine; using UnityEditor; using System.IO;
public static class ValVertFoliageEvidence {
    public static void Run(){
        UnityEditor.SceneManagement.EditorSceneManager.OpenScene("Assets/Scenes/ValVert.unity");
        ValVertDress.RepairTreePrototypes();
        var go=ValVertEnvWire.Wire();
        var clk=go.GetComponent<ValVertClock>();
        var sea=go.GetComponent<ValVertSeasons>();
        var sl=go.GetComponent<ValVertStreetLights>();
        var w=go.GetComponent<ValVertWeather>();
        ValVertWoods.ForceLeafTextureAlpha();
        ValVertWoods.RestoreEfLeavesUrp();
        ValVertWoods.HealFoliage();
        ValVertWoods.HealTreePrototypes();
        int hid=ValVertWoods.DisableBrokenLeafCards();
        foreach(var t in Terrain.activeTerrains){ if(t){ t.drawTreesAndFoliage=true; if(t.terrainData!=null) t.terrainData.RefreshPrototypes(); } }
        Debug.Log("[VV] EVIDENCE DisableBrokenLeafCards="+hid);
        var cam=new GameObject("ev_cam").AddComponent<Camera>();
        cam.allowHDR=true; cam.farClipPlane=2000; cam.nearClipPlane=0.2f; cam.fieldOfView=52f;
        sl.viewer=cam.transform; w.viewer=cam.transform; w.BuildShells();
        // woods path fence framing
        var p=new Vector3(352.5f,0,136.0f);
        Terrain terr=Terrain.activeTerrain;
        foreach(var tt in Terrain.activeTerrains){ var tp=tt.transform.position; var sz=tt.terrainData.size;
            if(p.x>=tp.x&&p.x<=tp.x+sz.x&&p.z>=tp.z&&p.z<=tp.z+sz.z){ terr=tt; break; } }
        float gy=terr.SampleHeight(p)+terr.transform.position.y;
        cam.transform.position=new Vector3(p.x, gy+1.65f, p.z);
        cam.transform.rotation=Quaternion.LookRotation(new Vector3(0.15f,0.02f,0.98f).normalized);
        w.Force(ValVertWeather.Kind.Overcast); w.ClearSnowVisuals();
        clk.Set(11,1,13f); w.Tick(); sea.snowCover=0f; sea.Tick(); clk.Apply(); sl.Tick();
        RenderSettings.ambientMode=UnityEngine.Rendering.AmbientMode.Flat;
        RenderSettings.ambientLight=new Color(0.72f,0.74f,0.76f);
        Directory.CreateDirectory("/workspace/valvert/verify");
        int W=1280,H=720;
        var rt=new RenderTexture(W,H,24); cam.targetTexture=rt; cam.Render();
        RenderTexture.active=rt; var tx=new Texture2D(W,H,TextureFormat.RGB24,false);
        tx.ReadPixels(new Rect(0,0,W,H),0,0); tx.Apply();
        var path="/workspace/valvert/verify/evidence_woods_no_cards.png";
        File.WriteAllBytes(path, tx.EncodeToPNG());
        // pixel guard
        var px=tx.GetPixels32(); int mag=0,cyan=0;
        for(int i=0;i<px.Length;i++){ var c=px[i]; if(c.r>200&&c.g<90&&c.b>200) mag++; if(c.r<90&&c.g>180&&c.b>180) cyan++; }
        Debug.Log($"[VV] EVIDENCE wrote {path} nulled={hid} magenta={100f*mag/px.Length:F3}% cyan={100f*cyan/px.Length:F3}% pos={cam.transform.position}");
        RenderTexture.active=null; cam.targetTexture=null;
        if(mag+cyan>px.Length/500){ Debug.LogError("[VV] EVIDENCE pixel guard fail"); EditorApplication.Exit(5);} else EditorApplication.Exit(0);
    }
}
