using UnityEngine;

/// <summary>
/// Loop continuo de un clip mientras el jugador está tocando suelo y (opcionalmente) moviéndose.
/// SIN variaciones ni pasos individuales: solo reproduce un clip en loop y lo detiene.
/// </summary>
[RequireComponent(typeof(Transform))]
public class FootstepPlayer : MonoBehaviour
{
    [Header("Detección (controller requerido)")]
    [Tooltip("Referencia al controlador para obtener grounded y velocidad.")]
    public RigidbodyFPSController controller;
    [Tooltip("Velocidad mínima horizontal para considerar que se está moviendo.")]
    public float minMoveSpeed = 0.1f;
    [Tooltip("Requerir movimiento además de estar en el suelo.")]
    public bool requireMovement = true;

    // Eliminado fallback por raycast: dependemos de controller.IsGrounded

    [Header("Audio")]
    public AudioClip loopClip; // el clip que será loopeado
    public AudioSource audioSource; // si null se crea
    [Range(0f, 1f)] public float volume = 1f;
    // Siempre 2D (spatialBlend = 0)
    [Tooltip("Aplicar fade suave al iniciar/detener.")]
    public bool useFade = true;
    public float fadeTime = 0.15f;

    private float _currentVolumeTarget;
    private bool _lastPaused;

    void Awake()
    {
        if (controller == null) controller = GetComponent<RigidbodyFPSController>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
        }
        audioSource.loop = true;
        audioSource.clip = loopClip;
        audioSource.volume = 0f; // empezamos en silencio si usamos fade
        audioSource.spatialBlend = 0f; // 2D absoluto
        audioSource.dopplerLevel = 0f;
        audioSource.rolloffMode = AudioRolloffMode.Linear; // irrelevante pero explícito
        audioSource.minDistance = 1f;
        audioSource.maxDistance = 1f; // al ser 2D no afecta, mantenido por claridad
        _lastPaused = IsGamePaused();
    }

    // Ya no usamos suscripciones de eventos. Polling directo.

    void Update()
    {
        bool paused = IsGamePaused();
        if (paused != _lastPaused)
        {
            if (paused)
            {
                // Transición a pausa: forzamos fade out o stop inmediato
                _currentVolumeTarget = 0f;
                if (!useFade && audioSource.isPlaying)
                    audioSource.Stop();
            }
            // (Al salir de pausa NO forzamos reproducción; se evaluará más abajo con shouldPlay)
            _lastPaused = paused;
        }
        if (paused)
        {
            if (useFade)
            {
                float newVolPaused = Mathf.MoveTowards(audioSource.volume, 0f, (volume / Mathf.Max(0.0001f, fadeTime)) * Time.unscaledDeltaTime);
                audioSource.volume = newVolPaused;
                if (audioSource.volume <= 0.001f && audioSource.isPlaying)
                    audioSource.Stop();
            }
            else if (audioSource.isPlaying)
            {
                audioSource.Stop();
            }
            return; // no procesamos movimiento mientras está en pausa
        }
        bool shouldPlay = ShouldPlay();

        if (loopClip == null)
        {
            if (audioSource.isPlaying) audioSource.Stop();
            return;
        }

        if (shouldPlay)
        {
            if (!audioSource.isPlaying)
            {
                audioSource.clip = loopClip;
                audioSource.Play();
            }
            _currentVolumeTarget = volume;
        }
        else
        {
            _currentVolumeTarget = 0f;
            if (!useFade && audioSource.isPlaying)
            {
                audioSource.Stop();
            }
        }

        if (useFade)
        {
            // Smooth damping hacia el volumen objetivo
            float newVol = Mathf.MoveTowards(audioSource.volume, _currentVolumeTarget, (volume / Mathf.Max(0.0001f, fadeTime)) * Time.unscaledDeltaTime);
            audioSource.volume = newVol;
            if (audioSource.volume <= 0.001f && _currentVolumeTarget == 0f && audioSource.isPlaying)
                audioSource.Stop();
        }
        else if (shouldPlay)
        {
            audioSource.volume = volume;
        }
    }


    private bool IsGamePaused()
    {
        // Si existe PauseManager lo usamos, sino fallback a Time.timeScale
        var pm = PauseManager.Instance;
        if (pm != null) return pm.IsPaused;
        return Time.timeScale == 0f;
    }

    private bool ShouldPlay()
    {
        if (controller == null) return false; // controller obligatorio
        if (!controller.IsGrounded) return false;
        if (requireMovement && !controller.IsMoving(minMoveSpeed)) return false;
        return true;
    }
}
