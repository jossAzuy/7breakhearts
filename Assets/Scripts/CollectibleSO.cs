using UnityEngine;

[CreateAssetMenu(menuName = "Collectible/Collectible Item", fileName = "NewCollectibleItem")]
public class CollectibleSO : ScriptableObject
{
    [Tooltip("Unique ID for save/logic")]
    public string id;
    public string itemName;
    public string ItemDescription;
    public Sprite icon;
    public GameObject worldModelPrefab; // 3D model to represent the item in the world
}
