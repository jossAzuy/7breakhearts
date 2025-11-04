using UnityEngine;

public class CollectiblePreviewUI : MonoBehaviour
{
    public Transform previewAnchor; // Empty GameObject in UI to hold the preview
   private GameObject currentPreviewInstance;

    public void ShowPreview(CollectibleSO itemData)
    {   
        // Clear previous preview
        if (currentPreviewInstance != null)
        {
            Destroy(currentPreviewInstance);
        }

        // Instantiate the 3D model for the item
        if (itemData != null && itemData.worldModelPrefab != null)
        {
            currentPreviewInstance = Instantiate(itemData.worldModelPrefab, previewAnchor);

            // Reset transforms
            currentPreviewInstance.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.Euler(0, 180, 0));
            currentPreviewInstance.transform.localScale = Vector3.one * 100; // Scale up for UI visibility
        }
    }

    public void HidePreview()
    {
        if (currentPreviewInstance != null)
        {
            Destroy(currentPreviewInstance);
        }
    }
}
