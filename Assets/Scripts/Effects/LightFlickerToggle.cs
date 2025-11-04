using UnityEngine;
using System.Collections;

/// <summary>
/// Toggles two target objects on/off to simulate light flicker.
/// Place this on any GameObject and assign two targets (e.g., a Light GameObject and a Mesh/Emissive object).
/// </summary>
[DisallowMultipleComponent]
public class LightFlickerToggle : MonoBehaviour
{
    [Header("Targets")]
    public GameObject targetA; // e.g., a Light GameObject
    public GameObject targetB; // e.g., an emissive mesh or secondary object

    [Header("State")]
    public bool startOn = true;

    [Header("Durations (seconds)")]
    [Tooltip("Random duration range while ON.")]
    public Vector2 onDurationRange = new Vector2(0.08f, 0.30f);
    [Tooltip("Random duration range while OFF.")]
    public Vector2 offDurationRange = new Vector2(0.02f, 0.12f);

    [Header("Bursts (quick flicks)")]
    [Tooltip("Chance [0..1] to trigger a short burst of quick toggles when changing state.")]
    [Range(0f, 1f)] public float burstChance = 0.35f;
    [Tooltip("Min/Max number of quick toggles in a burst.")]
    public Vector2Int burstTogglesRange = new Vector2Int(2, 5);
    [Tooltip("Duration range of each quick toggle in a burst.")]
    public Vector2 burstIntervalRange = new Vector2(0.02f, 0.06f);

    [Header("Timing")]
    [Tooltip("Use unscaled time so flicker works when timeScale=0 (e.g., menus/pause).")]
    public bool useUnscaledTime = false;

    private Coroutine _loop;
    private bool _current;

    private void OnEnable()
    {
        ValidateRanges();
        _current = startOn;
        ApplyState(_current);
        _loop = StartCoroutine(FlickerLoop());
    }

    private void OnDisable()
    {
        if (_loop != null)
        {
            StopCoroutine(_loop);
            _loop = null;
        }
        // Optional: leave targets in last state or restore start state
        ApplyState(startOn);
    }

    private IEnumerator FlickerLoop()
    {
        while (true)
        {
            // Hold current state for a random duration
            float hold = _current ? Random.Range(onDurationRange.x, onDurationRange.y)
                                   : Random.Range(offDurationRange.x, offDurationRange.y);
            if (useUnscaledTime) yield return new WaitForSecondsRealtime(hold);
            else yield return new WaitForSeconds(hold);

            // Flip state
            _current = !_current;
            ApplyState(_current);

            // Optional burst of quick toggles to simulate electrical jitter
            if (burstChance > 0f && Random.value < burstChance)
            {
                int toggles = Mathf.Clamp(Random.Range(burstTogglesRange.x, burstTogglesRange.y + 1), 0, 20);
                for (int i = 0; i < toggles; i++)
                {
                    float d = Mathf.Clamp(Random.Range(burstIntervalRange.x, burstIntervalRange.y), 0.005f, 1f);
                    if (useUnscaledTime) yield return new WaitForSecondsRealtime(d);
                    else yield return new WaitForSeconds(d);

                    _current = !_current;
                    ApplyState(_current);
                }
            }
        }
    }

    private void ApplyState(bool on)
    {
        if (targetA != null) targetA.SetActive(on);
        if (targetB != null) targetB.SetActive(on);

        // Convenience: if targets have Light components, mirror their enabled state
        // (useful when you want the GameObject active for other reasons)
        if (targetA != null)
        {
            var la = targetA.GetComponent<Light>();
            if (la != null) la.enabled = on;
        }
        if (targetB != null)
        {
            var lb = targetB.GetComponent<Light>();
            if (lb != null) lb.enabled = on;
        }
    }

    private void ValidateRanges()
    {
        onDurationRange.x = Mathf.Max(0f, onDurationRange.x);
        onDurationRange.y = Mathf.Max(onDurationRange.x, onDurationRange.y);
        offDurationRange.x = Mathf.Max(0f, offDurationRange.x);
        offDurationRange.y = Mathf.Max(offDurationRange.x, offDurationRange.y);

        burstIntervalRange.x = Mathf.Max(0f, burstIntervalRange.x);
        burstIntervalRange.y = Mathf.Max(burstIntervalRange.x, burstIntervalRange.y);

        burstTogglesRange.x = Mathf.Max(0, burstTogglesRange.x);
        if (burstTogglesRange.y < burstTogglesRange.x) burstTogglesRange.y = burstTogglesRange.x;
    }
}
