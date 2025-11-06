using UnityEngine;
using System.Collections;

public class PrefabSpawner : MonoBehaviour
{
    [Header("Configuración del spawn")]
    public GameObject prefab;              // Prefab a instanciar
    public Vector2 areaSize = new Vector2(5, 5);  // Tamaño del área
    public Vector2 areaOffset = Vector2.zero;     // Desplazamiento del área
    public float spawnInterval = 1f;       // Tiempo entre spawns (en segundos)

    [Header("Visualización")]
    public bool mostrarArea = true;
    public Color colorArea = new Color(0, 1, 0, 0.3f);

    private bool spawning = true;

    void Start()
    {
        if (prefab == null)
        {
            Debug.LogWarning("No hay prefab asignado al spawner.");
            return;
        }

        // Inicia el spawn infinito
        StartCoroutine(SpawnLoop());
    }

    IEnumerator SpawnLoop()
    {
        if (GameManager.IsDead) yield break;

        while (spawning)
        {
            SpawnPrefab();
            yield return new WaitForSeconds(spawnInterval);
        }
    }

    void SpawnPrefab()
    {

        if (GameManager.IsDead) return;

        // Generar posición aleatoria dentro del área
        Vector2 randomLocalPos = new Vector2(
            Random.Range(-areaSize.x / 2, areaSize.x / 2),
            Random.Range(-areaSize.y / 2, areaSize.y / 2)
        );

        // Calcular posición global usando el offset
        Vector2 spawnPos = (Vector2)transform.position + areaOffset + randomLocalPos;

        // Instanciar el prefab
        Instantiate(prefab, spawnPos, Quaternion.identity);
    }

    void OnDrawGizmos()
    {
        if (!mostrarArea) return;

        Gizmos.color = colorArea;
        Vector3 areaCenter = transform.position + (Vector3)areaOffset;
        Gizmos.DrawWireCube(areaCenter, (Vector3)areaSize);
        Gizmos.DrawCube(areaCenter, (Vector3)areaSize * 0.98f);
    }
}
