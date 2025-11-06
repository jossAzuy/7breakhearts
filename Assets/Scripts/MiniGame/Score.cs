using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Score : MonoBehaviour
{
    [Header("Configuración de Puntuación")]
    public int score = 0;                 // Puntaje actual del jugador
    public int puntosPorColision = 10;    // Cuántos puntos suma cada colisión
    public string prefabTag = "Prefab";   // Tag de los objetos que suman puntos

    [Header("Zona de detección")]
    public Collider2D zonaColision;       // Colisionador de la zona que detecta los prefabs

    [Header("UI (Opcional)")]
    public TextMeshProUGUI scoreText;                // Texto en pantalla para mostrar el puntaje

    void Start()
    {
        // Si no se asigna la zona, usa el propio collider del objeto
        if (zonaColision == null)
            zonaColision = GetComponent<Collider2D>();

        // Verifica que el collider esté configurado como Trigger
        if (zonaColision != null && !zonaColision.isTrigger)
        {
            Debug.LogWarning("El collider de la zona debe tener 'Is Trigger' activado.");
        }

        ActualizarUI();
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        // Detectar si el objeto que entra tiene el tag correcto
        if (other.CompareTag(prefabTag))
        {
            score += puntosPorColision;
            //Debug.Log($"Puntos +{puntosPorColision}! Puntaje total: {score}");
            ActualizarUI();

            // Opcional: destruir el prefab tras sumar puntos
            //Destroy(other.gameObject);
        }
    }

    void ActualizarUI()
    {
        if (GameManager.IsDead) return;

        if (scoreText != null)
            scoreText.text = score.ToString();
    }
}
