using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Basic world region streaming: enables/disables registered GameObject roots by distance
/// from a viewer (player/camera). This is NOT render-LOD culling and NOT Addressables scene loads.
/// </summary>
public class ValVertWorldStreaming : MonoBehaviour
{
    [System.Serializable]
    public class Region
    {
        public string id;
        public Transform root;
        public float loadRadius = 350f;
        public float unloadRadius = 420f;
        [Tooltip("If true, region starts active regardless of distance until first update.")]
        public bool keepWarm;
        internal bool loaded;
    }

    public Transform viewer;
    public List<Region> regions = new List<Region>();
    public float updateInterval = 0.5f;
    float _t;

    void OnEnable()
    {
        ValVertServices.Register(this);
        if (!viewer)
        {
            var cam = Camera.main;
            if (cam) viewer = cam.transform;
        }
    }

    void OnDisable()
    {
        if (ValVertServices.TryGet<ValVertWorldStreaming>(out var s) && ReferenceEquals(s, this))
            ValVertServices.Unregister<ValVertWorldStreaming>();
    }

    public void Register(string id, Transform root, float loadRadius, float unloadRadius, bool keepWarm = false)
    {
        if (!root) return;
        for (int i = 0; i < regions.Count; i++)
            if (regions[i].id == id) { regions[i].root = root; regions[i].loadRadius = loadRadius; regions[i].unloadRadius = unloadRadius; regions[i].keepWarm = keepWarm; return; }
        regions.Add(new Region { id = id, root = root, loadRadius = loadRadius, unloadRadius = unloadRadius, keepWarm = keepWarm, loaded = root.gameObject.activeSelf });
    }

    void Update()
    {
        _t += Time.unscaledDeltaTime;
        if (_t < updateInterval) return;
        _t = 0f;
        Tick();
    }

    public void Tick()
    {
        if (!viewer) return;
        var p = viewer.position;
        for (int i = 0; i < regions.Count; i++)
        {
            var r = regions[i];
            if (!r.root) continue;
            float d = Vector3.Distance(p, r.root.position);
            if (!r.loaded && (r.keepWarm || d <= r.loadRadius))
            {
                r.root.gameObject.SetActive(true);
                r.loaded = true;
            }
            else if (r.loaded && !r.keepWarm && d >= r.unloadRadius)
            {
                r.root.gameObject.SetActive(false);
                r.loaded = false;
            }
            regions[i] = r;
        }
    }
}
