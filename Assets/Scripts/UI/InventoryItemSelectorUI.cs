using UnityEngine;

public class InventoryItemSelectorUI : MonoBehaviour
{
    [Header("UI References")]
    public RectTransform selector;
    public RectTransform gridContainer;
    public CollectibleShowInfoUI collectibleInfo;

    //public CollectiblePreviewUI previewUI;
    public InventoryObjectViewer previewUI;

    private RectTransform currentSelectedItem;


    void Start()
    {
        if (selector != null)
        {
            selector.gameObject.SetActive(false); // Hide until an item is clicked.
        }
    }

    public void Select(RectTransform target, CollectibleSO itemData)
    {
        currentSelectedItem = target;
        selector.gameObject.SetActive(true);
        //selector.SetParent(gridContainer);
        //selector.SetAsFirstSibling(); // Ensure it's on top of other UI elements.

        //selector.SetAsLastSibling(); // Ensure it's on top of other UI elements.


        selector.position = target.position; // Move selector to the target's position.
        //selector.sizeDelta = target.sizeDelta; // Match the size of the target.

        if (collectibleInfo != null)
        {
            collectibleInfo.Show(itemData);

        }

        if (previewUI != null)
        {
            previewUI.ShowObject(itemData);
        }
        {
            // Additional logic for the preview UI can go here
        }
    }
}
