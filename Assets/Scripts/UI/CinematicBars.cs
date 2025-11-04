using UnityEngine;
using UnityEngine.UI;
using System.Collections;

/// <summary>
/// Simple letterbox (black bars) controller.
/// Create an empty GameObject under a Canvas (Screen Space - Overlay recommended)
/// add this component and assign two UI Images (top & bottom). They should anchor/stretch horizontally.
/// Initially they can be size 0 height. This script animates their height for a cinematic effect.
/// </summary>
public class CinematicBars : MonoBehaviour
{
    [Header("References")]
    [Tooltip("Top bar Image (UI)")] public RectTransform topBar;
    [Tooltip("Bottom bar Image (UI)")] public RectTransform bottomBar;

    [Header("Settings")] 
    [Tooltip("Target bar height in pixels when fully shown.")] public float targetHeight = 160f;
    [Tooltip("Animation duration seconds.")] public float animationDuration = 0.6f;
    [Tooltip("Optional curve for easing.")] public AnimationCurve ease = AnimationCurve.EaseInOut(0,0,1,1);
    [Tooltip("Disable raycast so bars don't block UI under them.")] public bool disableRaycastTargets = true;

    [Header("Auto")] public bool showOnStart = false;

    public bool IsShown { get; private set; }

    private Coroutine _currentRoutine;
    private float _current; // current animated height

    void Awake()
    {
        if (topBar == null || bottomBar == null)
        {
            Debug.LogWarning("[CinematicBars] Missing references.");
            enabled = false; return;
        }
        if (disableRaycastTargets)
        {
            var tImg = topBar.GetComponent<Image>(); if (tImg) tImg.raycastTarget = false;
            var bImg = bottomBar.GetComponent<Image>(); if (bImg) bImg.raycastTarget = false;
        }
        SetBarHeight(0f);
        if (showOnStart) ShowImmediate();
    }

    // Public API
    public void Show() => Play(true);
    public void Hide() => Play(false);
    public void Toggle() => Play(!IsShown);

    public void ShowImmediate()
    {
        KillRoutine();
        SetBarHeight(targetHeight);
        IsShown = true;
    }

    public void HideImmediate()
    {
        KillRoutine();
        SetBarHeight(0f);
        IsShown = false;
    }

    private void Play(bool show)
    {
        if (show == IsShown) return;
        KillRoutine();
        _currentRoutine = StartCoroutine(Animate(show));
    }

    private IEnumerator Animate(bool show)
    {
        IsShown = show; // state we are moving towards
        float start = _current;
        float end = show ? targetHeight : 0f;
        float t = 0f;
        float dur = Mathf.Max(0.0001f, animationDuration);
        while (t < 1f)
        {
            t += Time.unscaledDeltaTime / dur; // use unscaled so it works while paused
            float eased = ease.Evaluate(Mathf.Clamp01(t));
            float h = Mathf.Lerp(start, end, eased);
            SetBarHeight(h);
            yield return null;
        }
        SetBarHeight(end);
    }

    private void SetBarHeight(float h)
    {
        _current = h;
        if (topBar) topBar.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, h);
        if (bottomBar) bottomBar.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, h);
    }

    private void KillRoutine()
    {
        if (_currentRoutine != null)
        {
            StopCoroutine(_currentRoutine);
            _currentRoutine = null;
        }
    }
}
