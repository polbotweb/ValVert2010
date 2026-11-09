// Standalone smoke test: run the player with  -vvsmoke [-vvout <dir>]  -> scripted tour (woods/city/wide), season/time/weather
// changes, scripted walking, FPS/frame timing per scenario, screenshots, exception count, then quits. Inactive otherwise.
using UnityEngine; using System.Collections; using System.Collections.Generic; using System.Linq;
public class ValVertSmoke : MonoBehaviour {
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Boot(){ Debug.Log("[SMOKE] init args="+string.Join(" ",System.Environment.GetCommandLineArgs())); if(System.Environment.GetCommandLineArgs().Contains("-vvsmoke")) { Debug.Log("[SMOKE] boot"); new GameObject("vv_smoke").AddComponent<ValVertSmoke>(); } }
    int errors; string outDir;
    void Awake(){ DontDestroyOnLoad(gameObject); Application.logMessageReceived+=(c,st,t)=>{ if(t==LogType.Exception||t==LogType.Error) { errors++; Debug.Log("[SMOKE] ERR "+c); } };
        var a=System.Environment.GetCommandLineArgs().ToList(); int i=a.IndexOf("-vvout"); outDir=i>=0&&i+1<a.Count?a[i+1]:Application.persistentDataPath; System.IO.Directory.CreateDirectory(outDir); }
    IEnumerator Start(){
        Debug.Log("[SMOKE] coroutine start"); yield return null; Debug.Log($"[SMOKE] first frame dt={Time.unscaledDeltaTime:F2}s"); yield return null;
        var clk=ValVertClock.Instance?ValVertClock.Instance:FindAnyObjectByType<ValVertClock>(); var w=FindAnyObjectByType<ValVertWeather>(); var sea=FindAnyObjectByType<ValVertSeasons>();
        var fps=FindAnyObjectByType<SimpleFPS>(); var cc=fps?fps.GetComponent<CharacterController>():null; var cam=Camera.main;
        Debug.Log($"[SMOKE] start gpu={SystemInfo.graphicsDeviceName} api={SystemInfo.graphicsDeviceType} cpu={SystemInfo.processorType} x{SystemInfo.processorCount} res={Screen.width}x{Screen.height} player={(fps?"yes":"no")} clock={(clk?"yes":"no")} weather={(w?"yes":"no")}");
        var shots=new List<(string n,Vector3 p,float yaw,int mo,int d,float h,ValVertWeather.Kind k)>{
            ("woods_summer_noon",new Vector3(349,0,134.6f),19,7,15,13.5f,ValVertWeather.Kind.Clear),
            ("woods_winter_snow",new Vector3(349,0,134.6f),19,1,12,13f,ValVertWeather.Kind.Auto),
            ("woods_autumn_rain",new Vector3(349,0,134.6f),19,10,20,14f,ValVertWeather.Kind.Rain),
            ("city_night_lamps",new Vector3(470.5f,0,40f),14,10,20,22.5f,ValVertWeather.Kind.Clear),
            ("city_spring_day",new Vector3(470.5f,0,40f),14,4,20,11f,ValVertWeather.Kind.Overcast),
            ("midnight_woods",new Vector3(349,0,134.6f),19,6,21,0f,ValVertWeather.Kind.Clear)};
        foreach(var s in shots){
            if(fps){ cc.enabled=false; var p=s.p; p.y=Ground(p)+1.2f; fps.transform.position=p; fps.transform.rotation=Quaternion.Euler(0,s.yaw,0); cc.enabled=true; }
            if(w) w.Force(s.k); if(clk) clk.Set(s.mo,s.d,s.h);
            for(int f=0;f<30;f++) yield return null;
            // walk forward 2 s (scripted movement through the CharacterController)
            float t0=Time.realtimeSinceStartup; Vector3 st=fps?fps.transform.position:Vector3.zero; while(fps&&Time.realtimeSinceStartup-t0<2f){ cc.Move(fps.transform.forward*3f*Time.deltaTime+Vector3.down*2f*Time.deltaTime); yield return null; }
            float moved=fps?Vector3.Distance(st,fps.transform.position):0;
            var dts=new List<float>(); t0=Time.realtimeSinceStartup; while(Time.realtimeSinceStartup-t0<4f){ yield return null; dts.Add(Time.unscaledDeltaTime); }
            dts.Sort(); float avg=dts.Average(); float p95=dts[(int)(dts.Count*0.95f)];
            yield return new WaitForEndOfFrame(); ScreenCapture.CaptureScreenshot(System.IO.Path.Combine(outDir,"smoke_"+s.n+".png")); yield return null; yield return null;
            Debug.Log($"[SMOKE] {s.n} frames={dts.Count} avgFPS={1f/avg:F1} avgMs={avg*1000:F1} p95Ms={p95*1000:F1} moved={moved:F1}m weather={(w?w.Current.ToString():"-")} snow={(w?w.SnowCover:0):F2} wet={(w?w.Wetness:0):F2} sunAlt={(clk?clk.SunAltitudeDeg:0):F1}");
        }
        // gradual transition check: clear -> rain over in-game hours
        if(w&&clk){ w.Force(ValVertWeather.Kind.Clear); clk.Set(5,10,10f); yield return null; w.Force(ValVertWeather.Kind.Rain); float c0=w.Cloud; w.Tick();
            for(int k=0;k<6;k++){ clk.Set(5,10,10f+0.25f*(k+1)); w.Tick(); yield return null; Debug.Log($"[SMOKE] transition +{0.25f*(k+1):F2}h cloud={w.Cloud:F2} rain={w.Rain:F2}"); } }
        Debug.Log($"[SMOKE] DONE errors={errors}"); yield return null; Application.Quit(0);
    }
    static float Ground(Vector3 p){ foreach(var t in Terrain.activeTerrains){ var tp=t.transform.position; var sz=t.terrainData.size; if(p.x>=tp.x&&p.x<=tp.x+sz.x&&p.z>=tp.z&&p.z<=tp.z+sz.z) return t.SampleHeight(p)+tp.y; } return 0; }
}
