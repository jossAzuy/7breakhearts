using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Events;

/// <summary>
/// Escucha la muerte del jugador (HealthComponent) y dispara una pantalla / escena de Game Over.
/// Modos:
///  - Mostrar un Canvas (activar GameObject) y pausar tiempo.
///  - Cargar una escena de GameOver por nombre (opcional delay).
/// 
/// Uso rápido:
///  1. Crear un GameObject en la escena (GameOverHandler) y asignar este script.
///  2. Asignar campo player (o dejar vacío si el Player tiene tag "Player").
///  3. Arrastrar el HealthComponent del jugador al campo playerHealth si quieres atajarlo manualmente (si no, lo busca en Start).
///  4. Para UI local: crear un Canvas desactivado con tu panel Game Over y asignarlo a gameOverCanvasRoot.
///  5. Configurar showCanvasOnDeath = true. Ajustar pauseOnGameOver si quieres congelar.
///  6. Si prefieres una escena separada, asigna gameOverSceneName y habilita loadSceneOnGameOver.
///  7. (Opcional) Usar eventos onGameOverShown y onBeforeSceneLoad para animaciones extra.
/// </summary>
public class GameOverHandler : MonoBehaviour
{
    [Header("Player / Salud")] public Transform player; // opcional (se auto-busca por tag)
    public HealthComponent playerHealth; // si no se asigna, se busca en player
    public string playerTag = "Player";

    [Header("Canvas GameOver (opcional)")] public GameObject gameOverCanvasRoot; // raíz a activar
    [Tooltip("Mostrar un Canvas local al morir.")] public bool showCanvasOnDeath = true;

    [Header("Escena GameOver (opcional)")] public bool loadSceneOnGameOver = false;
    public string gameOverSceneName = "GameOver";
    [Tooltip("Tiempo (segundos) antes de cargar la escena de GameOver.")] public float sceneLoadDelay = 1.5f;

    [Header("Control de Tiempo")] [Tooltip("Poner Time.timeScale = 0 cuando se muestre la pantalla.")] public bool pauseOnGameOver = true;
    [Tooltip("Restaurar el timeScale previo al reiniciar/cambiar escena.")] public bool restoreTimeOnSceneLoad = true;

    [Header("Audio")] [Tooltip("Reproducir este clip 2D al activarse el GameOver (requiere AudioManager).")] public AudioClip gameOverClip;
    [Range(0f,1f)] public float gameOverClipVolume = 1f;

    [Header("Eventos")] public UnityEvent onGameOver; // se dispara al detectar muerte (antes de UI/scene)
    public UnityEvent onGameOverShown; // después de activar UI
    public UnityEvent onBeforeSceneLoad; // justo antes de cargar escena

    [Header("Debug")] public bool debugLogs = false;

    private bool _handled;
    private float _previousTimeScale = 1f;

    void Start()
    {
        if (player == null)
        {
            var playerGO = GameObject.FindGameObjectWithTag(playerTag);
            if (playerGO != null) player = playerGO.transform;
        }
        if (playerHealth == null && player != null)
        {
            playerHealth = player.GetComponent<HealthComponent>();
        }
        if (playerHealth != null)
        {
            playerHealth.onDeath.AddListener(HandlePlayerDeath);
        }
        else if (debugLogs)
        {
            Debug.LogWarning("[GameOverHandler] No se encontró HealthComponent del jugador.", this);
        }
    }

    private void OnDestroy()
    {
        if (playerHealth != null)
        {
            playerHealth.onDeath.RemoveListener(HandlePlayerDeath);
        }
    }

    private void HandlePlayerDeath()
    {
        if (_handled) return;
        // Validar realmente que la salud llegó a 0 (protección por si onDeath fue invocado manualmente erróneo)
        if (playerHealth != null)
        {
            if (!playerHealth.IsDead && playerHealth.Current > 0f)
            {
                if (debugLogs) Debug.LogWarning($"[GameOverHandler] onDeath recibido pero Current={playerHealth.Current} > 0. Ignorando.", this);
                return;
            }
        }
        _handled = true;
        if (debugLogs) Debug.Log("[GameOverHandler] Player muerto -> GameOver", this);

        onGameOver?.Invoke();

        // Audio opcional
        if (gameOverClip != null && AudioManager.Instance != null)
        {
            AudioManager.Instance.Play2D(gameOverClip, gameOverClipVolume);
        }

        if (showCanvasOnDeath && gameOverCanvasRoot != null)
        {

            // Asegurar que el cursor y crosshair (si hay) se muestren
            var uICursorBounds = FindObjectOfType<UICursorBounds>();
            if (uICursorBounds && uICursorBounds.crosshair)
            uICursorBounds.crosshair.gameObject.SetActive(true);

            gameOverCanvasRoot.SetActive(true);
            if (pauseOnGameOver)
            {
                _previousTimeScale = Time.timeScale;
                Time.timeScale = 0f;
            }
            onGameOverShown?.Invoke();
        }

        if (loadSceneOnGameOver && !string.IsNullOrEmpty(gameOverSceneName))
        {
            if (pauseOnGameOver && Time.timeScale != 0f)
            {
                _previousTimeScale = Time.timeScale;
                Time.timeScale = 0f; // congelar mientras esperamos
            }
            if (sceneLoadDelay > 0f) Invoke(nameof(LoadSceneNow), sceneLoadDelay); else LoadSceneNow();
        }
    }

    public void LoadSceneNow()
    {
        if (restoreTimeOnSceneLoad) Time.timeScale = _previousTimeScale;
        onBeforeSceneLoad?.Invoke();
        if (debugLogs) Debug.Log($"[GameOverHandler] Cargando escena '{gameOverSceneName}'", this);
        if (!string.IsNullOrEmpty(gameOverSceneName))
        {
            SceneManager.LoadScene(gameOverSceneName);
        }
    }

    // Métodos helper para UI Buttons (asignar en onClick)
    public void RestartCurrentScene()
    {
        if (restoreTimeOnSceneLoad) Time.timeScale = 1f;
        var scene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(scene.name);
    }

    public void QuitApplication()
    {
        if (restoreTimeOnSceneLoad) Time.timeScale = 1f;
    #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
    #else
        Application.Quit();
    #endif
    }
}
