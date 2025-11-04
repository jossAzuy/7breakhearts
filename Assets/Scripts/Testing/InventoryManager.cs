using UnityEngine;

public class InventoryManager : MonoBehaviour
{
    [Header("Referencias")]
    public GameObject inventoryCanvas; // asigna tu Canvas B en el Inspector
    public GameObject inventoryMenuIU; // Asigna tu menú de pausa en el inspector
    public RigidbodyFPSController playerController; // referencia al controlador del jugador

    [Header("Configuración")]
    public KeyCode toggleKey = KeyCode.I; // tecla para abrir/cerrar inventario

    private bool isOpen = false;

    void Start()
    {
      /*   if (inventoryCanvas != null)
            inventoryCanvas.SetActive(false); // empieza cerrado */

        /* Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false; */
    }

    void Update()
    {
        if (Input.GetKeyDown(toggleKey))
        {
            ToggleInventory();
        }
    }

    public void ToggleInventory()
    {
        isOpen = !isOpen;

      /*   if (inventoryCanvas != null)
            inventoryCanvas.SetActive(isOpen); */

        if (isOpen)
        {
            inventoryMenuIU.SetActive(true);


            // Pausar el juego
            Time.timeScale = 0f;
            Cursor.lockState = CursorLockMode.None; 
            Cursor.visible = true;

            //PauseManager.Instance.PauseGame(inventoryMenuIU);

            playerController.enabled = false; // desactivar el controlador del jugador
        }
        else
        {
            //PauseManager.Instance.ResumeGame(inventoryMenuIU);
            inventoryMenuIU.SetActive(false);


            // Reanudar el juego
            playerController.enabled = true; // activar el controlador del jugador

            Time.timeScale = 1f;
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }
}
