using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// Versión simplificada: ya NO cambia imágenes. Ahora sólo dispara animaciones
/// en un Animator cuando el mouse entra y sale del área del botón.
/// 
/// Uso:
/// 1. Añade este script al GameObject del botón (debe tener componente que reciba eventos: Button/Image + Raycaster + EventSystem en la escena).
/// 2. Asigna el Animator (puede estar en el mismo objeto o un hijo).
/// 3. Define los nombres de los triggers o, si prefieres, usa un parámetro bool.
/// 4. (Opcional) Usa los UnityEvents para reproducir sonido / efectos.
/// 
/// Animator sugerido:
///  - Estado Base (Idle)
///  - Estado Hover (activado por Trigger o Bool)
///  - Transiciones limpias de ida y vuelta.
/// </summary>
[DisallowMultipleComponent]
public class UIButtonHoverSwap : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Animator Principal")]
    [Tooltip("Animator que controla la animación del botón.")]
    public Animator animator;

    [Header("Modo Triggers")] 
    [Tooltip("Usar triggers (Hover / Exit) en lugar de un bool.")]
    public bool useTriggers = true;
    [Tooltip("Nombre del trigger cuando el mouse entra.")]
    public string hoverTrigger = "Hover";
    [Tooltip("Nombre del trigger cuando el mouse sale.")]
    public string exitTrigger = "Exit";

    [Header("Modo Bool (si no usas triggers)")]
    [Tooltip("Nombre del parámetro bool que se pondrá en true al entrar y false al salir.")]
    public string hoverBoolName = "IsHover";

    [Header("Eventos (Opcional)")]
    public UnityEvent onHoverEnter;
    public UnityEvent onHoverExit;

    private bool _isHovering;
    private int _hoverBoolHash;

    private void Awake()
    {
        if (!useTriggers && !string.IsNullOrWhiteSpace(hoverBoolName))
        {
            _hoverBoolHash = Animator.StringToHash(hoverBoolName);
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (_isHovering) return;
        _isHovering = true;
        ApplyHoverState(true);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (!_isHovering) return;
        _isHovering = false;
        ApplyHoverState(false);
    }

    private void ApplyHoverState(bool hover)
    {
        if (animator != null)
        {
            if (useTriggers)
            {
                if (hover)
                {
                    if (!string.IsNullOrEmpty(exitTrigger)) animator.ResetTrigger(exitTrigger);
                    if (!string.IsNullOrEmpty(hoverTrigger)) animator.SetTrigger(hoverTrigger);
                }
                else
                {
                    if (!string.IsNullOrEmpty(hoverTrigger)) animator.ResetTrigger(hoverTrigger);
                    if (!string.IsNullOrEmpty(exitTrigger)) animator.SetTrigger(exitTrigger);
                }
            }
            else
            {
                if (_hoverBoolHash != 0)
                {
                    animator.SetBool(_hoverBoolHash, hover);
                }
            }
        }

        if (hover) onHoverEnter?.Invoke(); else onHoverExit?.Invoke();
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (animator == null)
        {
            animator = GetComponent<Animator>();
        }
    }
#endif
}
