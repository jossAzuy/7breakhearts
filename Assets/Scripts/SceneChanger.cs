using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneChanger : MonoBehaviour
{
    /// <summary>
    /// Cambia a la escena indicada por nombre.
    /// </summary>
    /// <param name="sceneName">Nombre exacto de la escena (debe estar en Build Settings)</param>
    public void ChangeScene(string sceneName)
    {
        if (!string.IsNullOrEmpty(sceneName))
        {
            SceneManager.LoadScene(sceneName);
        }
        else
        {
            Debug.LogWarning("No se ha especificado el nombre de la escena para cambiar.");
        }
    }

    /// <summary>
    /// Carga la siguiente escena en el orden del Build Settings.
    /// </summary>
    public void LoadNextScene()
    {
        int currentIndex = SceneManager.GetActiveScene().buildIndex;
        int nextIndex = currentIndex + 1;

        if (nextIndex < SceneManager.sceneCountInBuildSettings)
        {
            SceneManager.LoadScene(nextIndex);
        }
        else
        {
            Debug.LogWarning("No hay más escenas en el Build Settings.");
        }
    }

    /// <summary>
    /// Recarga la escena actual.
    /// </summary>
    public void ReloadCurrentScene()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    /// <summary>
    /// Cierra la aplicación (funciona solo en build, no en el editor).
    /// </summary>
    public void QuitGame()
    {
        Debug.Log("Saliendo del juego...");
        Application.Quit();
    }
}
