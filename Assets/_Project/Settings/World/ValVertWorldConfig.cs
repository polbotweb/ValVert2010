using UnityEngine;

[CreateAssetMenu(menuName = "ValVert/World Config", fileName = "ValVertWorldConfig")]
public class ValVertWorldConfig : ScriptableObject
{
    [Header("Streaming (region activation — not LOD)")]
    public float streamLoadRadius = 380f;
    public float streamUnloadRadius = 460f;

    [Header("Pack prerequisites (documentary for tools)")]
    public string grassPackAssetStoreId = "138810";
    public string grassPackStatus = "awaiting_acquisition";
}
