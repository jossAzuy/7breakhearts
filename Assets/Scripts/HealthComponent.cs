using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Componente genérico de salud. Se puede usar para jugador o enemigos.
/// Para el jugador: adjuntar al GameObject con tag Player.
/// </summary>
public class HealthComponent : MonoBehaviour
{
    [Header("Configuración")] public float maxHealth = 100f;
    public bool destroyOnDeath = false;

    [Header("Eventos")] public UnityEvent<float, float> onHealthChanged; // current, max
    public UnityEvent onDeath;
    public UnityEvent<float> onDamageTaken; // amount

    private float _current;
    public float Current => _current;
    public bool IsDead => _current <= 0f;

    void Awake()
    {
        _current = maxHealth;
        onHealthChanged?.Invoke(_current, maxHealth);
    }

    public void ApplyDamage(float amount)
    {
        if (IsDead) return;
        if (amount <= 0f) return;
        _current = Mathf.Max(0f, _current - amount);
        onDamageTaken?.Invoke(amount);
        onHealthChanged?.Invoke(_current, maxHealth);
        if (_current <= 0f)
        {
            onDeath?.Invoke();
            if (destroyOnDeath) Destroy(gameObject);
        }
    }

    public void Heal(float amount)
    {
        if (amount <= 0f || IsDead) return;
        _current = Mathf.Min(maxHealth, _current + amount);
        onHealthChanged?.Invoke(_current, maxHealth);
    }

    public void Kill()
    {
        if (IsDead) return;
        _current = 0f;
        onHealthChanged?.Invoke(_current, maxHealth);
        onDeath?.Invoke();
        if (destroyOnDeath) Destroy(gameObject);
    }
}
