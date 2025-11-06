using UnityEngine;
using UnityEngine.Playables;   // Necesario para controlar la Timeline
using UnityEngine.SceneManagement; // Necesario para cambiar de escena

public class LoadSceneOnTimelineEnd : MonoBehaviour
{
    [Header("Asigna el PlayableDirector que controla la Timeline")]
    public PlayableDirector timelineDirector;

    [Header("Nombre de la escena a cargar al finalizar la Timeline")]
    public string sceneToLoad;

    private void Start()
    {
        // Suscribirse al evento que se dispara cuando termina la Timeline
        if (timelineDirector != null)
            timelineDirector.stopped += OnTimelineStopped;
    }

    private void OnTimelineStopped(PlayableDirector director)
    {
        // Cargar la nueva escena cuando la Timeline termina
        if (!string.IsNullOrEmpty(sceneToLoad))
        {
            SceneManager.LoadScene(sceneToLoad);
        }
        else
        {
            Debug.LogWarning("No se ha asignado el nombre de la escena en LoadSceneOnTimelineEnd.");
        }
    }

    private void OnDestroy()
    {
        // Evitar errores si el objeto se destruye
        if (timelineDirector != null)
            timelineDirector.stopped -= OnTimelineStopped;
    }
}
