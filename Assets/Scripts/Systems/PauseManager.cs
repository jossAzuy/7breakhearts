using UnityEngine;
using System.Collections.Generic;
using System;

/*
 * PauseManager (Uso práctico)
 * ---------------------------------------
 * Propósito: Gestionar apertura/cierre de menús (Pausa, Inventario, Opciones, etc.)
 *            y controlar el estado de pausa del juego (Time.timeScale), cursor y flujo
 *            entre menú de pausa y menú de opciones.
 *
 * Cómo configurarlo:
 * 1. Añade este script a un GameObject único en tu escena (o uno persistente si necesitas en varias escenas de juego).
 * 2. En la lista 'menus' crea una entrada por cada menú: asigna su GameObject y (opcional) la tecla que lo abre.
 *    - pausesGame: si está marcado, al abrir ese menú se pone Time.timeScale = 0.
 *    - toggleKey: tecla para abrir/cerrar ese menú (si None, sólo se abre por botón/UI).
 * 3. Asigna 'uICursorBounds' si usas el sistema de crosshair/cursor UI para que se muestre al abrir un menú.
 *
 * Reglas de apertura:
 * - No se puede abrir un segundo menú por tecla mientras otro esté abierto (se ignora la pulsación).
 * - Flujo especial Pausa -> Opciones: usa OpenOptionsFromPause() y CloseOptionsBackToPause().
 *
 * Métodos UI típicos (asignar a botones OnClick):
 * - OpenOptionsFromPause()  : Abre opciones desde pausa (mantiene el juego pausado).
 * - CloseOptionsBackToPause(): Vuelve de opciones a pausa sin reanudar.
 * - ResumeGame()            : Cierra el menú de pausa y reanuda si no hay otros menús que pausen.
 * - ReturnToMainMenu("EscenaMainMenu"): Cierra todo y carga escena de menú principal.
 * - CloseAllMenus()         : Cierra todos los menús y reanuda.
 *
 * Extensiones sugeridas (futuro):
 * - Eventos OnMenuOpened / OnMenuClosed.
 * - Lista blanca de menús que sí pueden superponerse.
 * - Integración con sistema Input rebindeable.
 * - Persistencia de configuración (audio/video) al cerrar opciones.
 *
 * Notas:
 * - IsPaused se basa directamente en Time.timeScale == 0.
 * - Asegúrate de que tus animators en menús usan UnscaledTime (se fuerza en Awake).
 * - Si un menú no debe pausar (ej: Inventario rápido), desmarca 'pausesGame'.
 */

public class PauseManager : MonoBehaviour
{
    public enum MenuType
    {
        Pause,
        Inventory,
        Options,
        Map
    }

    [System.Serializable]
    public class UIMenu
    {
        public MenuType type;
        public GameObject menuUI;
        public bool pausesGame = true;
        public KeyCode toggleKey = KeyCode.None;
        [HideInInspector] public bool isOpen;
    }

    public List<UIMenu> menus = new List<UIMenu>();

    public static PauseManager Instance { get; private set; }

    public bool IsPaused => Time.timeScale == 0f;
    public UICursorBounds uICursorBounds;
    [Header("Debug")] public bool debugEvents = false; // (Eventos eliminados; mantiene logs básicos si se amplía en futuro)

    private const string DEFAULT_MAIN_MENU_SCENE = "MainMenuScene";


    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        // Configuración inicial
        foreach (var menu in menus)
        {
            // Encuentra todos los animators en el menú y sus hijos
            Animator[] animators = menu.menuUI.GetComponentsInChildren<Animator>();
            foreach (var anim in animators)
                anim.updateMode = AnimatorUpdateMode.UnscaledTime;

            menu.menuUI.SetActive(false);
            menu.isOpen = false;
        }

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        // Revisa todas las teclas configuradas
        foreach (var menu in menus)
        {
            if (menu.toggleKey != KeyCode.None && Input.GetKeyDown(menu.toggleKey))
            {
                ToggleMenu(menu.type);
            }
        }
    }

    public void ToggleMenu(MenuType type)
    {
        UIMenu menu = menus.Find(m => m.type == type);
        if (menu == null) return;
        // Restricción: si ya hay otro menú abierto, no permitir abrir uno nuevo con tecla.
        if (!menu.isOpen)
        {
            bool anotherOpen = menus.Exists(m => m.isOpen && m != menu);
            if (anotherOpen)
                return; // ignoramos la solicitud
        }

        if (menu.isOpen) CloseMenu(menu);
        else OpenMenu(menu);
    }

    // --- Opciones específicas ---
    public void OpenOptionsFromPause()
    {
        var pause = menus.Find(m => m.type == MenuType.Pause);
        var options = menus.Find(m => m.type == MenuType.Options);
        if (options == null) return;

        // Cierra pausa (si estaba) y abre Options
        if (pause != null && pause.isOpen)
            CloseMenu(pause, affectTimeScale: false); // mantenemos pausa

        OpenMenu(options, forceKeepPaused: true);
    }

    public void CloseOptionsBackToPause()
    {
        var pause = menus.Find(m => m.type == MenuType.Pause);
        var options = menus.Find(m => m.type == MenuType.Options);
        if (options != null && options.isOpen)
            CloseMenu(options, affectTimeScale: false); // no reanudar todavía

        if (pause != null && !pause.isOpen)
            OpenMenu(pause, forceKeepPaused: true);
    }

    public void CloseAllMenus()
    {
        foreach (var m in menus)
        {
            if (m.isOpen)
                CloseMenu(m, affectTimeScale: false);
        }
        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        if (uICursorBounds && uICursorBounds.crosshair)
            uICursorBounds.crosshair.gameObject.SetActive(false);
    }

    // Botón: Reanudar juego (desde menú de pausa)
    public void ResumeGame()
    {
        // cierra sólo el menú de pausa si está abierto
        var pause = menus.Find(m => m.type == MenuType.Pause);
        if (pause != null && pause.isOpen)
            CloseMenu(pause, affectTimeScale: true);
    }

    // Botón: Ir al menú principal
    public void ReturnToMainMenu(string sceneName = DEFAULT_MAIN_MENU_SCENE)
    {
        // Asegura estado limpio
        CloseAllMenus();
        // Cargar escena principal (requiere using UnityEngine.SceneManagement)
        UnityEngine.SceneManagement.SceneManager.LoadScene(sceneName);
    }

    private void OpenMenu(UIMenu menu, bool forceKeepPaused = false)
    {
        // Ya no cerramos otros automáticamente: la restricción evita multi-apertura.

        menu.menuUI.SetActive(true);
        menu.isOpen = true;

        bool willPause = menu.pausesGame || forceKeepPaused;
        bool wasAlreadyPaused = IsPaused; // estado antes de abrir
        if (willPause)
            Time.timeScale = 0f;

        Cursor.lockState = CursorLockMode.Confined;

        if (uICursorBounds && uICursorBounds.crosshair)
            uICursorBounds.crosshair.gameObject.SetActive(true);

        // Antes aquí se disparaba OnGamePaused(true). Eliminado por simplificación.
        if (debugEvents && willPause && !wasAlreadyPaused)
            Debug.Log("[PauseManager] -> Paused (sin evento)");
        else if (debugEvents && willPause && wasAlreadyPaused)
            Debug.Log("[PauseManager] Pausa redundante (sin evento)");


    }

    private void CloseMenu(UIMenu menu, bool affectTimeScale = true)
    {
        menu.menuUI.SetActive(false);
        menu.isOpen = false;

        // Si no queda ningún menú que pause → reanudar
        if (affectTimeScale)
        {
            bool anyPauseMenuOpen = menus.Exists(m => m.isOpen && m.pausesGame);
            if (!anyPauseMenuOpen)
                Time.timeScale = 1f;
        }

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        if (uICursorBounds && uICursorBounds.crosshair)
            uICursorBounds.crosshair.gameObject.SetActive(false);

        // Antes aquí se disparaba OnGamePaused(false). Eliminado.
        if (debugEvents && !IsPaused)
            Debug.Log("[PauseManager] -> Resumed (sin evento)");
        else if (debugEvents && IsPaused)
            Debug.Log("[PauseManager] Otro menú que pausa sigue abierto (sin evento)");

    }
}
