using UnityEngine;
using UnityEngine.EventSystems;

public class ButtonHoverAnimation : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Animación a controlar")]
    public Animator targetAnimator;       // Animator de la imagen que quieres animar
    public string hoverTriggerName;       // Nombre del trigger que activa la animación
    public string exitTriggerName = "Normal"; // Trigger que vuelve al estado base

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (targetAnimator != null && !string.IsNullOrEmpty(hoverTriggerName))
        {
            targetAnimator.SetTrigger(hoverTriggerName);
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (targetAnimator != null && !string.IsNullOrEmpty(exitTriggerName))
        {
            targetAnimator.SetTrigger(exitTriggerName);
        }
    }
}
