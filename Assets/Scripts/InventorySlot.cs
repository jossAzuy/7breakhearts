using UnityEngine;
using UnityEngine.UI;

public class InventorySlot : MonoBehaviour
{
    public CollectibleSO itemData;
    private Button button;
    private RectTransform rectTransform;
    private InventoryItemSelectorUI inventorySelector;

    void Awake()
    {
        button = GetComponent<Button>();
        rectTransform = GetComponent<RectTransform>();
        inventorySelector = FindFirstObjectByType<InventoryItemSelectorUI>();

        if (button != null)
        {
            button.onClick.AddListener(OnClick);
        }
    } 

    public void SetItem(CollectibleSO newItemData)
    {
        itemData = newItemData;
    }


    private void OnClick()
    {
        if (inventorySelector != null)
        {
            inventorySelector.Select(rectTransform, itemData);
        }
    }
}