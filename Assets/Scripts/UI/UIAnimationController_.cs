using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

[System.Serializable]
public class ButtonAnimationMapping_
{
    public Button button;               // El botón que dispara la animación
    public Animator targetAnimator;     // Animator de la imagen que quieres animar
    public string hoverAnimationName;   // Nombre exacto de la animación para hover
    public string exitAnimationName = "Normal"; // Nombre de la animación base al salir
}

public class UIAnimationController_ : MonoBehaviour
{
    public ButtonAnimationMapping_[] mappings;

    void Start()
    {
        foreach (var mapping in mappings)
        {
            if (mapping.button == null || mapping.targetAnimator == null) continue;

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

    private void OnHover(ButtonAnimationMapping_ mapping)
    {
        if (mapping.targetAnimator != null && !string.IsNullOrEmpty(mapping.hoverAnimationName))
        {
            mapping.targetAnimator.Play(mapping.hoverAnimationName, 0, 0f);
        }
    }

    private void OnExit(ButtonAnimationMapping_ mapping)
    {
        if (mapping.targetAnimator != null && !string.IsNullOrEmpty(mapping.exitAnimationName))
        {
            mapping.targetAnimator.Play(mapping.exitAnimationName, 0, 0f);
        }
    }
}
