using System.Collections.Generic;
using UnityEngine;

// This keeps track of collected items.
public class InventorySystem : MonoBehaviour
{
    public static InventorySystem Instance;

    public List<CollectibleSO> items = new List<CollectibleSO>();
    public InventoryUI inventoryUI;


    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void AddItem(CollectibleSO newItem)
    {
        items.Add(newItem);
        inventoryUI.RefreshUI(items);
    }
}
