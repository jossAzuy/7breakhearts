using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Muestra hasta 3 imágenes de daño progresivo según los golpes recibidos.
/// Diseñado para un jugador que muere al 4º golpe (ya gestionado por GameOverHandler / HealthComponent).
/// Estrategia:
///  - Se suscribe a onDamageTaken (o alternativamente a onHealthChanged) del HealthComponent del jugador.
///  - Cada golpe incrementa un contador interno (hits).
///  - Al primer golpe activa imagen1, al segundo activa también imagen2, al tercero activa también imagen3.
///  - El cuarto golpe no se gestiona aquí (se asume que dispara Game Over y/o muerte) pero no causa error.
/// Configuración:
///  - Arrastra este script a un GameObject del Canvas (p.ej. un Panel vacío).
///  - Asigna las referencias a imageHit1 / imageHit2 / imageHit3 (pueden tener diferentes transparencias / efectos).
///  - Campo autoFindPlayer: si está activo buscará por tag Player el HealthComponent; si no, asignar manualmente.
///  - Reset en Start para ocultar todas las imágenes.
/// Opcional: Puedes llamar ResetHUD() manualmente cuando reinicies la escena.
/// </summary>
public class DamageOverlayHUD : MonoBehaviour
{
    [Header("Referencias Imágenes en orden de aparición")] public Image imageHit1;
    public Image imageHit2;
    public Image imageHit3;

    [Header("Detección Player")] public bool autoFindPlayer = true;
    public string playerTag = "Player";
    public HealthComponent playerHealth;

    [Header("Debug")] public bool debugLogs = false;

    private int _hits; // golpes acumulados desde inicio / último reset

    void Start()
    {
        if (autoFindPlayer && playerHealth == null)
        {
            var player = GameObject.FindGameObjectWithTag(playerTag);
            if (player != null) playerHealth = player.GetComponent<HealthComponent>();
        }
        if (playerHealth != null)
        {
            // Suscribir a evento de daño puntual (más fiable para contar golpes que healthChanged si el daño >1 golpe)
            playerHealth.onDamageTaken.AddListener(OnPlayerDamaged);
        }
        ResetHUD();
    }

    private void OnDestroy()
    {
        if (playerHealth != null)
        {
            playerHealth.onDamageTaken.RemoveListener(OnPlayerDamaged);
        }
    }

    private void OnPlayerDamaged(float dmg)
    {
        _hits++;
        if (debugLogs) Debug.Log($"[DamageOverlayHUD] Golpe #{_hits} (dmg={dmg})", this);
        UpdateVisual();
    }

    private void UpdateVisual()
    {
        if (imageHit1 != null) imageHit1.enabled = _hits >= 1;
        if (imageHit2 != null) imageHit2.enabled = _hits >= 2;
        if (imageHit3 != null) imageHit3.enabled = _hits >= 3;
    }

    public void ResetHUD()
    {
        _hits = 0;
        if (imageHit1 != null) imageHit1.enabled = false;
        if (imageHit2 != null) imageHit2.enabled = false;
        if (imageHit3 != null) imageHit3.enabled = false;
    }

    /// <summary>
    /// Permite forzar el contador (por ejemplo al cargar partida) y refrescar.
    /// </summary>
    public void SetHits(int hits)
    {
        _hits = Mathf.Max(0, hits);
        UpdateVisual();
    }
}
