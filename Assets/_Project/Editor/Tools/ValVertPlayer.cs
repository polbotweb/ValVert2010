using UnityEditor; using UnityEngine;
public static class ValVertPlayer {
    public static void BuildLinux(){
        var o=new BuildPlayerOptions{ scenes=new[]{"Assets/Scenes/ValVert.unity"}, locationPathName="/workspace/valvert/build/linux/ValVert2010.x86_64", target=BuildTarget.StandaloneLinux64, options=BuildOptions.None };
        var r=BuildPipeline.BuildPlayer(o); Debug.Log($"[VV] PLAYER {r.summary.result} errors={r.summary.totalErrors} warnings={r.summary.totalWarnings} size={r.summary.totalSize/1048576}MB time={r.summary.totalTime}");
        if(r.summary.result!=UnityEditor.Build.Reporting.BuildResult.Succeeded) EditorApplication.Exit(1);
    }
}
