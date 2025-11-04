using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenuInteractionButtons : MonoBehaviour
{
    [Header("Menús")]
    public GameObject pauseMenuUI;
    public GameObject optionsMenuUI;

    //private bool isPaused = false;

    void Update()
    {
       /*  // Detectar tecla ESC para pausar
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (isPaused)
                Resume();
            else
                Pause();
        } */
    }
/* 
    public void Resume()
    {
        pauseMenuUI.SetActive(false);
        Time.timeScale = 1f; // Reanudar el tiempo
        isPaused = false;
    } */

/*     void Pause()
    {
        pauseMenuUI.SetActive(true);
        Time.timeScale = 0f; // Congelar el tiempo
        isPaused = true;
    } */

    public void OpenOptions()
    {
        pauseMenuUI.SetActive(false);
        optionsMenuUI.SetActive(true);
    }

    public void CloseOptions()
    {
        optionsMenuUI.SetActive(false);
        pauseMenuUI.SetActive(true);
    }

    public void LoadMainMenu()
    {
        //Time.timeScale = 1f; 
        SceneManager.LoadScene("MainMenu"); 
    }
}
