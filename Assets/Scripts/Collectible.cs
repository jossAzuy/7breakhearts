using UnityEngine;

public class Collectible : MonoBehaviour
{
    [Header("Data")] public CollectibleSO collectibleData;
    public ParticleSystem pickupEffect;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        // Añade al inventario
        if (collectibleData != null)
            InventorySystem.Instance.AddItem(collectibleData);

        // Sonido centralizado en AudioManager (si está configurado)
        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayCollectible(collectibleData, transform.position);

        // Efecto visual de recogida
        if (pickupEffect != null)
        {
            var effect = Instantiate(pickupEffect, transform.position - Vector3.up, pickupEffect.transform.rotation);
            //Destroy(effect.gameObject, effect.main.duration);
            Destroy(effect.gameObject, 2f);

        }

        Destroy(gameObject);
    }
}
