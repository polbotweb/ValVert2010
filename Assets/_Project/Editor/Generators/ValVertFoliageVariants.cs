using UnityEngine; using UnityEditor; using UnityEngine.Rendering;
public static class ValVertFoliageVariants {
    public static void Build(){
        var leaves=ValVertWoods.LeavesUrpShader();
        var bark=ValVertWoods.BarkUrpShader();
        var lit=Shader.Find("Universal Render Pipeline/Lit");
        var svc=new ShaderVariantCollection();
        void add(Shader sh, string[] kws){
            if(!sh) return;
            foreach(var pt in new[]{ PassType.Normal, PassType.ScriptableRenderPipeline, PassType.ScriptableRenderPipelineDefaultUnlit, PassType.ShadowCaster }){
                try { svc.Add(new ShaderVariantCollection.ShaderVariant(sh, pt, kws)); } catch (System.Exception e){ Debug.LogWarning("[VV] variant skip "+sh.name+" "+pt+": "+e.Message); }
            }
        }
        add(leaves, new string[0]);
        add(bark, new string[0]);
        add(lit, new[]{"_ALPHATEST_ON"});
        add(lit, new[]{"_ALPHATEST_ON","_NORMALMAP"});
        AssetDatabase.CreateAsset(svc, "Assets/ValVert/FoliageVariants.shadervariants");
        AssetDatabase.SaveAssets();
        int tex=ValVertWoods.ForceLeafTextureAlpha();
        int n=ValVertWoods.RestoreEfLeavesUrp();
        ValVertWoods.WarmFoliageShaders();
        Debug.Log($"[VV] FoliageVariants saved shaders={svc.shaderCount} variants={svc.variantCount} texReimport={tex} restoredMats={n} lit={(lit?lit.name:"null")}");
        EditorApplication.Exit(0);
    }
}
