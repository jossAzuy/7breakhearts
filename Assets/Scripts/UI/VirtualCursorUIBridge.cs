using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// Option B: Virtual Cursor UI Bridge
/// 
/// Bridges a custom on‑screen cursor (a RectTransform you move yourself) with Unity's normal UI event system.
/// This lets Buttons / Selectables receive proper hover, press, and click events (so their Transition animations work)
/// even when the hardware cursor is hidden or locked.
/// 
/// HOW TO USE:
/// 1. Create a crosshair (UI Image / RectTransform) in a Screen Space - Overlay canvas.
/// 2. Ensure its Graphic (Image) has raycastTarget = false (so it does NOT block the raycast).
/// 3. A separate script (e.g. your UICursorBounds) moves that RectTransform each frame.
/// 4. Add this component to your Canvas (or another GameObject) and assign:
///       - crosshair
///       - raycaster (the Canvas's GraphicRaycaster)
///       - eventSystem (or leave empty to auto-fill current EventSystem)
/// 5. Hide the OS cursor elsewhere (Cursor.visible = false) if you want only the custom one.
/// 
/// FEATURES:
///  - Hover enter / exit events
///  - Left click (down / up / click) and optional right click
///  - Proper pressed state so Button "Pressed" transition shows while holding
///  - ForceClick API
///  - Optional debug logging
/// 
/// LIMITATIONS / EXTENSIONS:
///  - For drag & drop you would also send BeginDrag / Drag / EndDrag events.
///  - For scroll wheel you'd set pointer.scrollDelta and ExecuteEvents.scrollHandler.
///  - For gamepad virtual cursor, drive crosshair position from stick input.
/// </summary>
[DefaultExecutionOrder(110)]
public class VirtualCursorUIBridge : MonoBehaviour
{
    [Header("Virtual Cursor References")]
    public RectTransform crosshair;            // Visual cursor (screen-space position)
    public GraphicRaycaster raycaster;         // Canvas GraphicRaycaster
    public EventSystem eventSystem;            // EventSystem (auto-filled if null)

    [Header("Event Options")] 
    public bool sendHover = true;              // Fire PointerEnter / PointerExit
    public bool sendClick = true;              // Fire full down/up/click on Mouse0
    public bool sendPressStates = true;        // Keep pointerDown target while holding
    public bool sendRightClick = false;        // Optional right click support

    [Header("Behavior")] 
    [Tooltip("Only consider first raycast result (typical for UI). If false you could extend multi-target logic.")]
    public bool firstResultOnly = true;

    [Header("Debug")] 
    public bool logEvents = false;

    private PointerEventData _pointer;
    private readonly List<RaycastResult> _results = new();
    private GameObject _currentHover;
    private GameObject _pressedTargetLeft;
    private GameObject _pressedTargetRight;

    void Awake()
    {
        if (!eventSystem)
            eventSystem = EventSystem.current;
        if (eventSystem != null)
            _pointer = new PointerEventData(eventSystem);
    }

    void Update()
    {
        if (_pointer == null || crosshair == null || raycaster == null)
            return;

        // Position pointer at crosshair
        Vector2 screenPos = crosshair.position; // Screen Space - Overlay assumption
        _pointer.Reset();
        _pointer.position = screenPos;

        // Raycast UI
        _results.Clear();
        raycaster.Raycast(_pointer, _results);

        GameObject newHover = null;
        if (_results.Count > 0)
        {
            newHover = _results[0].gameObject;
            if (!firstResultOnly)
            {
                // (Extension point) iterate others if needed
            }
        }

        HandleHover(newHover);
        HandleClicks();
    }

    private void HandleHover(GameObject newHover)
    {
        if (!sendHover) return;
        if (newHover == _currentHover) return;

        if (_currentHover != null)
        {
            ExecuteEvents.Execute(_currentHover, _pointer, ExecuteEvents.pointerExitHandler);
            if (logEvents) Debug.Log($"[VirtualCursorUIBridge] Exit: {_currentHover.name}");
        }
        if (newHover != null)
        {
            ExecuteEvents.Execute(newHover, _pointer, ExecuteEvents.pointerEnterHandler);
            if (logEvents) Debug.Log($"[VirtualCursorUIBridge] Enter: {newHover.name}");
        }
        _currentHover = newHover;
    }

    private void HandleClicks()
    {
        // LEFT BUTTON
        if (sendClick || sendPressStates)
        {
            if (Input.GetMouseButtonDown(0))
            {
                if (_currentHover != null)
                {
                    _pressedTargetLeft = _currentHover;
                    ExecuteEvents.Execute(_pressedTargetLeft, _pointer, ExecuteEvents.pointerDownHandler);
                    if (logEvents) Debug.Log($"[VirtualCursorUIBridge] PointerDown (L): {_pressedTargetLeft.name}");
                }
            }
            if (Input.GetMouseButtonUp(0))
            {
                if (_pressedTargetLeft != null)
                {
                    ExecuteEvents.Execute(_pressedTargetLeft, _pointer, ExecuteEvents.pointerUpHandler);
                    if (logEvents) Debug.Log($"[VirtualCursorUIBridge] PointerUp (L): {_pressedTargetLeft.name}");
                    if (sendClick && _currentHover == _pressedTargetLeft)
                    {
                        ExecuteEvents.Execute(_pressedTargetLeft, _pointer, ExecuteEvents.pointerClickHandler);
                        if (logEvents) Debug.Log($"[VirtualCursorUIBridge] Click (L): {_pressedTargetLeft.name}");
                    }
                }
                _pressedTargetLeft = null;
            }
        }

        // RIGHT BUTTON (optional)
        if (sendRightClick)
        {
            if (Input.GetMouseButtonDown(1) && _currentHover != null)
            {
                _pressedTargetRight = _currentHover;
                ExecuteEvents.Execute(_pressedTargetRight, _pointer, ExecuteEvents.pointerDownHandler);
                if (logEvents) Debug.Log($"[VirtualCursorUIBridge] PointerDown (R): {_pressedTargetRight.name}");
            }
            if (Input.GetMouseButtonUp(1) && _pressedTargetRight != null)
            {
                ExecuteEvents.Execute(_pressedTargetRight, _pointer, ExecuteEvents.pointerUpHandler);
                if (_currentHover == _pressedTargetRight)
                {
                    ExecuteEvents.Execute(_pressedTargetRight, _pointer, ExecuteEvents.pointerClickHandler);
                    if (logEvents) Debug.Log($"[VirtualCursorUIBridge] Click (R): {_pressedTargetRight.name}");
                }
                _pressedTargetRight = null;
            }
        }
    }

    /// <summary>
    /// Programmatically click whatever is currently hovered (if anything)
    /// </summary>
    public void ForceClickCurrentHover()
    {
        if (_currentHover == null) return;
        ExecuteEvents.Execute(_currentHover, _pointer, ExecuteEvents.pointerDownHandler);
        ExecuteEvents.Execute(_currentHover, _pointer, ExecuteEvents.pointerUpHandler);
        ExecuteEvents.Execute(_currentHover, _pointer, ExecuteEvents.pointerClickHandler);
    }
}
