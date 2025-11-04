/* using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

[System.Serializable]
public class ButtonAnimationMapping
{
    public Button button;         // El botón que dispara la animación
    public Animator targetAnimator; // La imagen que quieres animar
    public string hoverTrigger;     // Trigger al posar el mouse
    public string exitTrigger = "Normal"; // Trigger al salir el mouse
}

public class UIAnimationController : MonoBehaviour
{
    public ButtonAnimationMapping[] mappings;

    void Start()
    {
        foreach (var mapping in mappings)
        {
            // Agrega listeners de hover usando EventTrigger
            EventTrigger trigger = mapping.button.gameObject.GetComponent<EventTrigger>();
            if (trigger == null)
                trigger = mapping.button.gameObject.AddComponent<EventTrigger>();

            // Pointer Enter
            var entryEnter = new EventTrigger.Entry { eventID = EventTriggerType.PointerEnter };
            entryEnter.callback.AddListener((data) => OnHover(mapping));
            trigger.triggers.Add(entryEnter);

            // Pointer Exit
            var entryExit = new EventTrigger.Entry { eventID = EventTriggerType.PointerExit };
            entryExit.callback.AddListener((data) => OnExit(mapping));
            trigger.triggers.Add(entryExit);
        }
    }

    private void OnHover(ButtonAnimationMapping mapping)
    {
        if (mapping.targetAnimator != null)
            mapping.targetAnimator.SetTrigger(mapping.hoverTrigger);
    }

    private void OnExit(ButtonAnimationMapping mapping)
    {
        if (mapping.targetAnimator != null)
            mapping.targetAnimator.SetTrigger(mapping.exitTrigger);
    }
}
 */