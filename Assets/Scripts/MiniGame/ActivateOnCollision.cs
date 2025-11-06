using UnityEngine;

public class ActivateOnCollision : MonoBehaviour
{
    [Header("Objeto a activar")]
    public GameObject objectToActivate;

    [Header("Configuración del jugador")]
    public string playerTag = "Player";

    //public static bool IsDead { get; private set; }

    // Quita el reinicio de Start()
    // void Start()
    // {
    //     IsDead = false;
    // }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag(playerTag))
        {
            if (!GameManager.IsDead)
            {
                GameManager.IsDead = true;

                if (objectToActivate != null)
                {
                    objectToActivate.SetActive(true);
                    Debug.Log($"{objectToActivate.name} activado al colisionar con el jugador.");
                }
                else
                {
                    Debug.LogWarning("No hay ningún objeto asignado para activar.");
                }
            }
        }
    }
}
