using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Main Menu controller: hook its public methods to UI Buttons (OnClick) in the Inspector.
/// - Play: loads the target scene by name or build index.
/// - Credits: shows/hides a credits panel GameObject.
/// - Quit: exits the application (and stops play mode in the Editor).
/// </summary>
public class MainMenuController : MonoBehaviour
{
    [Header("Play Settings")]
    [Tooltip("Name of the scene to load when pressing Play. Takes priority over Build Index.")]
    public string playSceneName;
    [Tooltip("If no Play Scene Name is set, this build index will be used. Set to -1 to ignore.")]
    public int playSceneBuildIndex = -1;

    [Header("Credits Panel")]
    [Tooltip("Assign the Credits panel GameObject to show/hide.")]
    public GameObject creditsPanel;

   /*  [Header("Crosshair (optional)")]
    [Tooltip("Assign the crosshair GameObject to hide while Credits are open.")]
    public GameObject crosshair; */

    [Header("Optional")] 
    [Tooltip("Force Time.timeScale = 1 before loading Play scene.")]
    public bool resetTimeScaleOnPlay = true;

    // --- Button Hooks ---

    // Hook this to the Play button OnClick
    public void OnPlayPressed()
    {
        if (resetTimeScaleOnPlay) Time.timeScale = 1f;

        if (!string.IsNullOrWhiteSpace(playSceneName))
        {
            SceneManager.LoadScene(playSceneName);
        }
        else if (playSceneBuildIndex >= 0)
        {
            SceneManager.LoadScene(playSceneBuildIndex);
        }
        else
        {
            Debug.LogWarning("[MainMenuController] No Play scene configured. Set playSceneName or playSceneBuildIndex.");
        }
    }

    // Hook this to the Credits button OnClick
    public void OnCreditsPressed()
    {
        SetCreditsVisible(true);
    }

    // Hook this to a Back/Close button inside the Credits panel
    public void OnCloseCreditsPressed()
    {
        SetCreditsVisible(false);
    }

    // Optional: Hook to a single button if you want a toggle behavior
    public void OnToggleCreditsPressed()
    {
        if (creditsPanel == null) return;
        SetCreditsVisible(!creditsPanel.activeSelf);
    }

    // Hook this to the Exit/Quit button OnClick
    public void OnQuitPressed()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif

        Debug.Log("[MainMenuController] Quit requested.");
    }

    private void SetCreditsVisible(bool visible)
    {
        if (creditsPanel != null)
        {
            creditsPanel.SetActive(visible);
        }
        else
        {
            Debug.LogWarning("[MainMenuController] Credits Panel is not assigned in the inspector.");
        }

       /*  // Ensure crosshair remains visible even when credits are open
        if (crosshair != null)
        {
            crosshair.SetActive(true);
        } */
    }
}
