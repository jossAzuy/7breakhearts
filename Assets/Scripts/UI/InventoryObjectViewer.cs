using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class InventoryObjectViewer : MonoBehaviour, IBeginDragHandler, IDragHandler, IScrollHandler
{
    [Header("References")]
    public Transform previewAnchor;   // Empty parent of the 3D object
    public Camera renderCamera;       // Camera rendering to RenderTexture
    public RawImage targetRawImage;   // The UI RawImage that shows the object

    [Header("Controls")]
    public float rotationSensitivity = 0.5f;
    public float zoomSensitivity = 0.1f;      // Higher = stronger zoom
    public float minScale = 0.5f;             // Smallest allowed zoom
    public float maxScale = 2.0f;             // Largest allowed zoom

    private bool isDraggingObject = false;
    private Vector3 baseScale;

    private void Start()
    {
        if (previewAnchor != null)
            baseScale = previewAnchor.localScale;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (IsPointerOverRawImage(eventData))
        {
            isDraggingObject = true;
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!isDraggingObject) return;

        float rotY = -eventData.delta.x * rotationSensitivity;
        previewAnchor.Rotate(Vector3.up, rotY, Space.World);
    }

    public void OnScroll(PointerEventData eventData)
    {
        if (!IsPointerOverRawImage(eventData)) return;

        // Scale zoom instead of moving camera
        float scrollAmount = eventData.scrollDelta.y * zoomSensitivity;
        Vector3 newScale = previewAnchor.localScale + Vector3.one * scrollAmount;
        float clamped = Mathf.Clamp(newScale.x, baseScale.x * minScale, baseScale.x * maxScale);

        previewAnchor.localScale = Vector3.one * clamped;
    }

    private bool IsPointerOverRawImage(PointerEventData eventData)
    {
        if (targetRawImage == null) return false;

        RectTransform rectTransform = targetRawImage.rectTransform;
        Vector2 localMousePosition;

        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
            rectTransform,
            eventData.position,
            eventData.pressEventCamera,
            out localMousePosition))
        {
            return rectTransform.rect.Contains(localMousePosition);
        }

        return false;
    }

    private void Update()
    {
        if (Input.GetMouseButtonUp(0))
        {
            isDraggingObject = false;
        }
    }
    
    public void ShowObject(CollectibleSO itemData)
{
    // Clear old preview
    foreach (Transform child in previewAnchor)
    {
        Destroy(child.gameObject);
    }

    if (itemData == null || itemData.worldModelPrefab == null) return;

    // Spawn the new preview object
    GameObject obj = Instantiate(itemData.worldModelPrefab, previewAnchor);

    // Reset transform
    obj.transform.localPosition = Vector3.zero;
    obj.transform.localRotation = Quaternion.identity;
    obj.transform.localScale = Vector3.one;

    // Reset zoom
    previewAnchor.localScale = baseScale;
}
}
