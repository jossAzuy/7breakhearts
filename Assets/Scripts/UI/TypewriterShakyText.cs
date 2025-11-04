using System.Collections;
using System.Collections.Generic; // Para soportar listas de párrafos
using UnityEngine;
using UnityEngine.Events;
#if TMP_PRESENT || UNITY_EDITOR
using TMPro;
#endif

/// <summary>
/// Efecto de escritura (typewriter) con ligero temblor de caracteres.
/// Diseñado para TextMeshProUGUI. Si no hay TMP, hace un fallback básico con UI.Text.
/// Coloca este script en un GameObject y asigna el componente de texto.
/// Llama a StartTypewriter("tu frase") para iniciar con un solo bloque.
/// Versión simplificada: solo escritura básica + secuencia de prefabs (con jitter, pausas y autoplay opcional).
/// </summary>
public class TypewriterShakyText : MonoBehaviour
{
    [Header("Referencia (TextMeshProUGUI o Text)")]
#if TMP_PRESENT || UNITY_EDITOR
    public TMP_Text tmpText; // Usar la clase base para aceptar tanto TextMeshProUGUI como variantes
#endif
    public UnityEngine.UI.Text uiTextFallback;

    [Header("Typewriter")] 
    [Tooltip("Caracteres por segundo.")] public float charsPerSecond = 35f;
    [Tooltip("Pausa extra tras signos de puntuación (.,!?). Segundos.")] public float punctuationPause = 0.15f;
    [Tooltip("Iniciar automáticamente con el texto inicial del componente.")] public bool playOnStart = false;

    [Header("AutoPlay (Prefab Sequence)")]
    [Tooltip("Reproduce automáticamente la secuencia de prefabs al iniciar.")] public bool autoPlayPrefabSequenceOnStart = false;
    [Tooltip("Retardo antes de iniciar el autoplay de la secuencia (segundos). ")] public float autoPlayInitialDelay = 0f;

    [Header("Temblor")] 
    [Tooltip("Amplitud horizontal / vertical del jitter en unidades de fuente.")] public Vector2 shakeAmplitude = new Vector2(0.5f, 0.5f);
    [Tooltip("Frecuencia del temblor.")] public float shakeSpeed = 8f;
    [Tooltip("Solo temblar mientras aparece cada letra.")] public bool shakeOnlyWhileAppearing = false;

    [Header("Secuencia (Prefabs)")]
    [Tooltip("Retraso entre cada prefab (si useSequenceDelayForPrefabs está activo o no se pasa custom). ")] public float sequenceDelayBetweenPrefabs = 1f;
    [Tooltip("Limpiar el texto antes de iniciar cada nuevo prefab.")] public bool sequenceClearOnNextStart = false;

    [Header("Prefabs (Control layout)")]
    [Tooltip("Lista de prefabs (cada uno con TMP_Text o UI.Text) que se instanciarán y escribirán secuencialmente.")]
    public List<GameObject> paragraphPrefabs = new List<GameObject>();
    [Tooltip("Parent donde se instancian los prefabs (si es null, se usa este mismo transform).")] public Transform prefabParent;
    [Tooltip("Destruir el prefab anterior antes de crear el siguiente. Si no, simplemente se desactiva.")] public bool destroyPreviousPrefab = true;
    [Tooltip("Usar el delay 'sequenceDelayBetweenPrefabs' entre prefabs.")] public bool useSequenceDelay = true;
    [Tooltip("Limpiar referencias internas al terminar la secuencia de prefabs.")] public bool clearPrefabInstanceOnEnd = true;

    [Header("Eventos")] public UnityEvent onTypewriterCompleted;

    private string _fullText;
    private Coroutine _typeRoutine;
    private Coroutine _autoPlayRoutine;
    private Coroutine _prefabSequenceRoutine;
    private int _visibleChars;
    private bool _finished;
    private GameObject _currentPrefabInstance;

#if TMP_PRESENT || UNITY_EDITOR
    // Seeds por carácter para variación estable
    private System.Random _rng = new System.Random();
    private float[] _charSeeds; // un valor aleatorio por caracter
#endif

    void Awake()
    {
#if TMP_PRESENT || UNITY_EDITOR
        if (tmpText == null) tmpText = GetComponent<TMP_Text>();
#endif
        if (uiTextFallback == null) uiTextFallback = GetComponent<UnityEngine.UI.Text>();
    }

    void Start()
    {
#if TMP_PRESENT || UNITY_EDITOR
        if (tmpText != null) _fullText = tmpText.text;
        else if (uiTextFallback != null) _fullText = uiTextFallback.text;
#else
        if (uiTextFallback != null) _fullText = uiTextFallback.text;
#endif
        if (autoPlayPrefabSequenceOnStart)
        {
            if (_autoPlayRoutine != null) StopCoroutine(_autoPlayRoutine);
            _autoPlayRoutine = StartCoroutine(AutoPlayRoutine());
        }
        else if (playOnStart && !string.IsNullOrEmpty(_fullText))
        {
            StartTypewriter(_fullText);
        }
    }

    /// <summary>
    /// Inicia el efecto de escritura con un nuevo texto.
    /// Nota: Cuando forma parte de la secuencia de prefabs no debemos detener la propia coroutine de secuencia.
    /// </summary>
    /// <param name="newText">Texto a escribir.</param>
    /// <param name="partOfPrefabSequence">Si true, NO se detiene la coroutine de secuencia (_prefabSequenceRoutine).</param>
    public void StartTypewriter(string newText, bool partOfPrefabSequence = false)
    {
        if (!partOfPrefabSequence)
        {
            // Detiene TODO (incluida la secuencia) como antes
            StopTypewriter();
        }
        else
        {
            // Solo detener rutinas de tipeo / autoplay, pero NO la secuencia que nos llama.
            if (_typeRoutine != null) { StopCoroutine(_typeRoutine); _typeRoutine = null; }
            if (_autoPlayRoutine != null) { StopCoroutine(_autoPlayRoutine); _autoPlayRoutine = null; }
            // No tocamos _prefabSequenceRoutine
        }

        _finished = false;
        _visibleChars = 0;
        _fullText = newText;
#if TMP_PRESENT || UNITY_EDITOR
        if (tmpText != null)
        {
            tmpText.text = newText;
            tmpText.maxVisibleCharacters = 0;
            tmpText.ForceMeshUpdate();
            _charSeeds = new float[tmpText.textInfo.characterCount];
            for (int i = 0; i < _charSeeds.Length; i++) _charSeeds[i] = (float)_rng.NextDouble() * 1000f;
        }
        else if (uiTextFallback != null)
        {
            uiTextFallback.text = "";
        }
#else
        if (uiTextFallback != null) uiTextFallback.text = "";
#endif
        _typeRoutine = StartCoroutine(TypeRoutine());
    }

    // (Se removió soporte de multipárrafos por texto plano / presets; usar prefabs para control de layout)

    public void StopTypewriter()
    {
        if (_typeRoutine != null)
        {
            StopCoroutine(_typeRoutine);
            _typeRoutine = null;
        }
        if (_autoPlayRoutine != null)
        {
            StopCoroutine(_autoPlayRoutine);
            _autoPlayRoutine = null;
        }
        if (_prefabSequenceRoutine != null)
        {
            StopCoroutine(_prefabSequenceRoutine);
            _prefabSequenceRoutine = null;
        }
    }

    private System.Collections.IEnumerator AutoPlayRoutine()
    {
        if (autoPlayInitialDelay > 0f)
            yield return new WaitForSeconds(autoPlayInitialDelay);
        StartTypewriterPrefabSequence();
        _autoPlayRoutine = null;
    }
    // (Removida la secuencia de strings; solo queda la de prefabs)

    /// <summary>
    /// Inicia una secuencia basada en prefabs (cada prefab define su propio layout / font / tamaño). Se toma el texto del TMP_Text/UI.Text del prefab.
    /// </summary>
    public void StartTypewriterPrefabSequence(float? customDelay = null)
    {
        StopTypewriter();
        if (paragraphPrefabs == null || paragraphPrefabs.Count == 0) return;
        _prefabSequenceRoutine = StartCoroutine(PrefabSequenceRoutine(customDelay));
    }

    private IEnumerator PrefabSequenceRoutine(float? customDelay)
    {
    float delay = customDelay.HasValue ? Mathf.Max(0f, customDelay.Value) : (useSequenceDelay ? Mathf.Max(0f, sequenceDelayBetweenPrefabs) : 0f);
        Transform parent = prefabParent != null ? prefabParent : transform;
        for (int i = 0; i < paragraphPrefabs.Count; i++)
        {
            var prefab = paragraphPrefabs[i];
            if (prefab == null) continue;
            // Limpiar anterior
            if (_currentPrefabInstance != null)
            {
                if (destroyPreviousPrefab) Destroy(_currentPrefabInstance); else _currentPrefabInstance.SetActive(false);
            }
            _currentPrefabInstance = Instantiate(prefab, parent);

            // Intentar obtener TMP o fallback y extraer texto
            string blockText = null;
#if TMP_PRESENT || UNITY_EDITOR
            var prefabTMP = _currentPrefabInstance.GetComponentInChildren<TMP_Text>();
            if (prefabTMP != null)
            {
                blockText = prefabTMP.text;
                tmpText = prefabTMP; // Reapuntar el typewriter al nuevo componente
            }
            else
#endif
            {
                var ui = _currentPrefabInstance.GetComponentInChildren<UnityEngine.UI.Text>();
                if (ui != null)
                {
                    blockText = ui.text;
                    uiTextFallback = ui;
#if TMP_PRESENT || UNITY_EDITOR
                    tmpText = null; // asegurar fallback
#endif
                }
            }
            if (string.IsNullOrEmpty(blockText)) blockText = "";
            StartTypewriter(blockText, true);
            while (!_finished)
                yield return null;
            if (i < paragraphPrefabs.Count - 1 && delay > 0f)
                yield return new WaitForSeconds(delay);
        }
        if (clearPrefabInstanceOnEnd)
        {
            _currentPrefabInstance = null;
        }
        _prefabSequenceRoutine = null;
    }

    private IEnumerator TypeRoutine()
    {
        if (string.IsNullOrEmpty(_fullText)) yield break;
        float delayPerChar = 1f / Mathf.Max(1f, charsPerSecond);

#if TMP_PRESENT || UNITY_EDITOR
        if (tmpText != null)
        {
            while (_visibleChars < tmpText.textInfo.characterCount)
            {
                _visibleChars++;
                tmpText.maxVisibleCharacters = _visibleChars;

                char justAdded = GetRawChar(_visibleChars - 1);
                float extraPause = (justAdded == '.' || justAdded == ',' || justAdded == '!' || justAdded == '?') ? punctuationPause : 0f;
                yield return new WaitForSeconds(delayPerChar + extraPause);
            }
        }
        else
#endif
        if (uiTextFallback != null)
        {
            for (int i = 0; i < _fullText.Length; i++)
            {
                uiTextFallback.text = _fullText.Substring(0, i + 1);
                char justAdded = _fullText[i];
                float extraPause = (justAdded == '.' || justAdded == ',' || justAdded == '!' || justAdded == '?') ? punctuationPause : 0f;
                yield return new WaitForSeconds(delayPerChar + extraPause);
            }
        }

        _finished = true;
        onTypewriterCompleted?.Invoke();
    }

#if TMP_PRESENT || UNITY_EDITOR
    void LateUpdate()
    {
        if (tmpText == null) return;
        if (shakeOnlyWhileAppearing && _finished) return; // no temblor tras terminar (opcional)
        if (_visibleChars == 0) return;

        tmpText.ForceMeshUpdate();
        var textInfo = tmpText.textInfo;
        int charCount = textInfo.characterCount;
        float t = Time.unscaledTime; // independiente de pausa

        for (int i = 0; i < charCount; i++)
        {
            if (i >= _visibleChars) break; // no modificar los ocultos
            var charInfo = textInfo.characterInfo[i];
            if (!charInfo.isVisible) continue;
            int matIndex = charInfo.materialReferenceIndex;
            int vertIndex = charInfo.vertexIndex;
            var verts = textInfo.meshInfo[matIndex].vertices;

            float seed = (i < _charSeeds.Length) ? _charSeeds[i] : 0f;
            float offsetX = (Mathf.PerlinNoise(seed, t * shakeSpeed) - 0.5f) * 2f * shakeAmplitude.x;
            float offsetY = (Mathf.PerlinNoise(seed + 77.7f, t * shakeSpeed) - 0.5f) * 2f * shakeAmplitude.y;
            Vector3 jitter = new Vector3(offsetX, offsetY, 0f);

            verts[vertIndex + 0] += jitter;
            verts[vertIndex + 1] += jitter;
            verts[vertIndex + 2] += jitter;
            verts[vertIndex + 3] += jitter;
        }

        // Aplicar cambios a los meshes
        for (int m = 0; m < textInfo.meshInfo.Length; m++)
        {
            var meshInfo = textInfo.meshInfo[m];
            meshInfo.mesh.vertices = meshInfo.vertices;
            tmpText.UpdateGeometry(meshInfo.mesh, m);
        }
    }

    private char GetRawChar(int visibleIndex)
    {
        // Busca el N-ésimo carácter real (ignorando etiquetas rich text)
        int rawCount = 0;
        for (int i = 0; i < _fullText.Length; i++)
        {
            char c = _fullText[i];
            if (c == '<')
            {
                // Saltar etiqueta
                int close = _fullText.IndexOf('>', i);
                if (close != -1) { i = close; continue; }
            }
            rawCount++;
            if (rawCount - 1 == visibleIndex) return c;
        }
        return '\0';
    }
#endif
}
