using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UICursorBounds : MonoBehaviour
{
    public RectTransform crosshair; // Asigna el objeto de la mira en el inspector
    public GraphicRaycaster raycaster; // Asigna el GraphicRaycaster del Canvas
    public EventSystem eventSystem; // Asigna el EventSystem de la escena

    [Header("Screen Margins (pixels)")]
    [Min(0)] public float leftMargin = 0f;
    [Min(0)] public float rightMargin = 0f;
    [Min(0)] public float topMargin = 0f;
    [Min(0)] public float bottomMargin = 0f;

    [Header("Cursor Settings")]
    public bool confineCursor = true; // Confine cursor to Game view (Play Mode)

    [SerializeField] bool crosshairVisibleAtStart = false;

    void Start()
    {
        Cursor.visible = false;
        if (confineCursor)
        {
            Cursor.lockState = CursorLockMode.Confined;
        }

        crosshair.gameObject.SetActive(crosshairVisibleAtStart);
    }

    void Update()
    {
        Vector3 mousePos = Input.mousePosition;

        // Clamp mouse within margins relative to screen size (Screen Space - Overlay)
        float minX = leftMargin;
        float maxX = Screen.width - rightMargin;
        float minY = bottomMargin;
        float maxY = Screen.height - topMargin;

        // Ensure ranges are valid even if margins are too large
        if (minX > maxX) { float mid = (minX + maxX) * 0.5f; minX = maxX = mid; }
        if (minY > maxY) { float mid = (minY + maxY) * 0.5f; minY = maxY = mid; }

        Vector3 clamped = new Vector3(
            Mathf.Clamp(mousePos.x, minX, maxX),
            Mathf.Clamp(mousePos.y, minY, maxY),
            0f
        );

        crosshair.position = clamped;

        if (Input.GetMouseButtonDown(0))
        {
            PointerEventData pointerData = new PointerEventData(eventSystem);
            pointerData.position = clamped;

            var results = new System.Collections.Generic.List<RaycastResult>();
            raycaster.Raycast(pointerData, results);

            foreach (RaycastResult result in results)
            {
                var button = result.gameObject.GetComponent<Button>();
                if (button != null)
                {
                    button.onClick.Invoke();
                    break;
                }
            }
        }
    }
}
