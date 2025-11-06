using UnityEngine;
using UnityEngine.Playables; // Necesario para controlar la Timeline

public class ActivateTimelineOnTrigger : MonoBehaviour
{
    [Header("Asignar el PlayableDirector con la Timeline")]
    public PlayableDirector timelineDirector;

    [Header("Tag del jugador que activa el trigger")]
    public string playerTag = "Player";

    private bool hasPlayed = false; // Evita que se repita (opcional)

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(playerTag) && !hasPlayed)
        {
            timelineDirector.Play();
            hasPlayed = true;
        }
    }
}
