using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class InventoryUI : MonoBehaviour
{
    public RectTransform contentPanel;
    public GameObject itemSlotPrefab;

    public void RefreshUI(List<CollectibleSO> items)
    {
        // Clear existing UI
        foreach (Transform child in contentPanel)
        {
            Destroy(child.gameObject);
        }

        // Create new UI elements
        foreach (var item in items)
        {
            GameObject newSlot = Instantiate(itemSlotPrefab, contentPanel);
            InventorySlot slotComponent = newSlot.GetComponent<InventorySlot>();
            slotComponent.SetItem(item); // Set the item data for the slot

            newSlot.GetComponent<UnityEngine.UI.Image>().sprite = item.icon;
            // newSlot.GetComponentInChildren<UnityEngine.UI.Text>().text = item.itemName;
        }

    }
}
