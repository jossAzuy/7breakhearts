using UnityEngine;

/// <summary>
/// Subtle, configurable Perlin-noise camera motion for menus or idle screens.
/// Attach to a Camera (or any Transform). Restores original transform on disable.
/// </summary>
[DisallowMultipleComponent]
public class MenuCameraNoise : MonoBehaviour
{
    [Header("Time")]
    [Tooltip("Use unscaled time so it works even if Time.timeScale = 0 in menus.")]
    public bool useUnscaledTime = true;
    [Tooltip("Global speed multiplier for the noise time.")]
    [Min(0f)] public float timeScale = 1f;

    [Header("Position Noise (local)")]
    public bool positionNoise = true;
    [Tooltip("Amplitude of local position noise in units per axis.")]
    public Vector3 posAmplitude = new Vector3(0.05f, 0.05f, 0.0f);
    [Tooltip("Frequency (cycles per second) per axis.")]
    public Vector3 posFrequency = new Vector3(0.15f, 0.2f, 0.1f);

    [Header("Rotation Noise (local)")]
    public bool rotationNoise = true;
    [Tooltip("Amplitude of local rotation noise in degrees per axis (pitch, yaw, roll).")]
    public Vector3 rotAmplitude = new Vector3(0.5f, 1.0f, 0.25f);
    [Tooltip("Frequency (cycles per second) per axis.")]
    public Vector3 rotFrequency = new Vector3(0.1f, 0.12f, 0.08f);

    [Header("Smoothing")]
    [Tooltip("How quickly to interpolate towards the current noise target. Higher = snappier.")]
    [Range(0f, 30f)] public float smoothing = 8f;

    [Header("Randomization")]
    [Tooltip("Randomize seeds on enable.")]
    public bool autoSeed = true;
    public int seed = 12345;

    // Original local transform
    private Vector3 _baseLocalPos;
    private Quaternion _baseLocalRot;

    // For smoothing
    private Vector3 _currentPosOffset;
    private Vector3 _targetPosOffset;
    private Vector3 _currentEulerOffset;
    private Vector3 _targetEulerOffset;

    // Seeds per axis
    private float _px, _py, _pz, _rx, _ry, _rz;

    private void OnEnable()
    {
        _baseLocalPos = transform.localPosition;
        _baseLocalRot = transform.localRotation;

        if (autoSeed)
            seed = Random.Range(int.MinValue / 2, int.MaxValue / 2);

        // Use different seeds per component to decorrelate axes
        _px = seed * 0.017f + 11.3f; _py = seed * 0.019f + 23.7f; _pz = seed * 0.013f + 7.1f;
        _rx = seed * 0.031f + 3.9f;  _ry = seed * 0.029f + 17.2f; _rz = seed * 0.027f + 41.6f;

        _currentPosOffset = Vector3.zero;
        _targetPosOffset = Vector3.zero;
        _currentEulerOffset = Vector3.zero;
        _targetEulerOffset = Vector3.zero;
    }

    private void OnDisable()
    {
        // Restore original transform
        transform.localPosition = _baseLocalPos;
        transform.localRotation = _baseLocalRot;
    }

    private void LateUpdate()
    {
        float dt = useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
        float t = (useUnscaledTime ? Time.unscaledTime : Time.time) * Mathf.Max(0f, timeScale);

        // Compute new targets from Perlin noise (0..1 -> -1..1)
        if (positionNoise)
        {
            _targetPosOffset = new Vector3(
                Noise01(t * posFrequency.x, _px) * 2f - 1f,
                Noise01(t * posFrequency.y, _py) * 2f - 1f,
                Noise01(t * posFrequency.z, _pz) * 2f - 1f
            );
            _targetPosOffset = Vector3.Scale(_targetPosOffset, posAmplitude);
        }
        else
        {
            _targetPosOffset = Vector3.zero;
        }

        if (rotationNoise)
        {
            _targetEulerOffset = new Vector3(
                Noise01(t * rotFrequency.x, _rx) * 2f - 1f,
                Noise01(t * rotFrequency.y, _ry) * 2f - 1f,
                Noise01(t * rotFrequency.z, _rz) * 2f - 1f
            );
            _targetEulerOffset = Vector3.Scale(_targetEulerOffset, rotAmplitude);
        }
        else
        {
            _targetEulerOffset = Vector3.zero;
        }

        // Smoothly interpolate toward targets
        float lerp = smoothing > 0f ? 1f - Mathf.Exp(-smoothing * dt) : 1f;
        _currentPosOffset = Vector3.Lerp(_currentPosOffset, _targetPosOffset, lerp);
        _currentEulerOffset = Vector3.Lerp(_currentEulerOffset, _targetEulerOffset, lerp);

        // Apply local offsets
        transform.localPosition = _baseLocalPos + _currentPosOffset;
        transform.localRotation = _baseLocalRot * Quaternion.Euler(_currentEulerOffset);
    }

    // 2D Perlin noise sampler: uses time (x) and a seed offset (y)
    private static float Noise01(float x, float seed)
    {
        return Mathf.PerlinNoise(x, seed);
    }
}
