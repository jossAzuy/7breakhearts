using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Events;

/// <summary>
/// Al detectar contacto del jugador (trigger o colisión) realiza 3 acciones en orden:
/// 1. Activa un GameObject (por ejemplo, UI, puerta, evento, etc.).
/// 2. Teletransporta al jugador a un punto designado (posición y rotación opcional).
/// 3. Desactiva otro GameObject (por ejemplo, un área previa, un bloqueo, etc.).
///
/// Coloca este script en el objeto del escenario que tendrá el Collider.
/// - Si usas OnTriggerEnter: marca el collider como IsTrigger.
/// - Si prefieres colisión física: deja IsTrigger desmarcado y el jugador debe tener Rigidbody/CharacterController.
///
/// Para evitar múltiples disparos se bloquea automáticamente después del primer contacto (opción reiniciable).
/// </summary>
public class ActivateTeleportAndDeactivate : MonoBehaviour
{
    [Header("Detección")]
    [Tooltip("Tag que debe tener el jugador.")] public string playerTag = "Player";
    [Tooltip("Usar OnTriggerEnter (true) o OnCollisionEnter (false). Requiere collider apropiado.")] public bool useTrigger = true;
    [Tooltip("Permitir disparar más de una vez.")] public bool allowMultipleActivations = false;

    [Header("Acciones - Activar")]
    [Tooltip("GameObject que se activará al contacto. (Opcional)")] public GameObject activateOnContact;

    [Header("Acciones - Teletransporte")]
    [Tooltip("Destino al que se moverá el jugador.")] public Transform teleportTarget;
    [Tooltip("Aplicar también la rotación del destino al jugador.")] public bool applyRotation = true;

    [Header("Acciones - Desactivar")]
    [Tooltip("Lista de GameObjects que se desactivarán tras el teletransporte. (Opcional)")] public List<GameObject> deactivateAfterTeleport = new List<GameObject>();

    [Header("Eventos")] public UnityEvent onActivated; // Por si quieres encadenar más lógica

    private bool _used;

    private void OnTriggerEnter(Collider other)
    {
        if (!useTrigger) return; // Estamos configurados para colisión normal
        TryExecute(other.gameObject);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (useTrigger) return; // Estamos configurados para trigger
        TryExecute(collision.gameObject);
    }

    private void TryExecute(GameObject other)
    {
        if (_used && !allowMultipleActivations) return;
        if (!other.CompareTag(playerTag)) return;

        // 1. Activar
        if (activateOnContact != null && !activateOnContact.activeSelf)
            activateOnContact.SetActive(true);

        // 2. Teletransportar
        if (teleportTarget != null)
        {
            Transform playerT = other.transform;

            // Si tiene CharacterController lo deshabilitamos momentáneamente para evitar fricción al mover.
            CharacterController cc = playerT.GetComponent<CharacterController>();
            bool reenableCC = false;
            if (cc != null && cc.enabled)
            {
                cc.enabled = false;
                reenableCC = true;
            }

            // Rigidbody: limpiamos velocidad para evitar que conserve inercia rara.
            Rigidbody rb = playerT.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
            }

            playerT.position = teleportTarget.position;
            if (applyRotation)
            {
                playerT.rotation = teleportTarget.rotation;
            }

            if (reenableCC)
                cc.enabled = true;
        }

        // 3. Desactivar (múltiples)
        if (deactivateAfterTeleport != null)
        {
            for (int i = 0; i < deactivateAfterTeleport.Count; i++)
            {
                var go = deactivateAfterTeleport[i];
                if (go == null) continue;
                if (go.activeSelf) go.SetActive(false);
            }
        }

        onActivated?.Invoke();

        _used = true;
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        if (teleportTarget != null)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(teleportTarget.position, 0.25f);
            Gizmos.DrawLine(transform.position, teleportTarget.position);
        }
    }
#endif
}
