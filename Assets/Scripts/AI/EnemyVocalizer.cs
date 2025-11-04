using UnityEngine;

/// <summary>
/// Componente independiente que gestiona los gritos / vocalizaciones del enemigo.
/// Desacopla la lógica de temporizado y selección de clips respecto al AI principal.
/// Llamadas necesarias desde el AI:
///  - OnEnterChase() cuando entra en persecución.
///  - OnEnterPatrol() cuando vuelve / entra a patrulla.
///  - Tick(state) cada frame para actualizar timers.
/// Requiere AudioManager con método Play3DFollow.
/// </summary>
[DisallowMultipleComponent]
public class EnemyVocalizer : MonoBehaviour
{
    [Header("Chase Screams")] public bool enableChaseScreams = true;
    [Tooltip("Grito inmediato al entrar en Chase.")] public bool screamOnChaseEnter = true;
    [Tooltip("Seguir gritando aleatoriamente mientras dura el Chase.")] public bool loopChaseScreams = true;
    [Tooltip("Clips de gritos (elige aleatorio evitando repetir el último).")] public AudioClip[] screamClips;
    [Range(0f,1f)] public float chaseScreamVolume = 1f;
    public float chaseIntervalMin = 4f;
    public float chaseIntervalMax = 9f;

    [Header("Patrol Screams")] public bool enablePatrolScreams = false;
    public bool patrolScreamOnEnter = false;
    public float patrolIntervalMin = 10f;
    public float patrolIntervalMax = 18f;
    [Tooltip("Si < 0 reutiliza chaseScreamVolume.")] public float patrolScreamVolume = -1f;

    [Header("Debug")] public bool debugLogs = false;

    private Transform _followTransform;
    private float _nextChaseTime = -1f;
    private float _nextPatrolTime = -1f;
    private int _lastIndex = -1;

    void Awake()
    {
        _followTransform = transform;
    }

    /// <summary>Permite redefinir el transform que seguirá el audio (por defecto el propio).</summary>
    public void Initialize(Transform t) => _followTransform = t != null ? t : transform;

    public void OnEnterChase()
    {
        if (!enableChaseScreams) return;
        if (screamOnChaseEnter) PlayScream(false);
        ScheduleNextChase();
    }

    public void OnEnterPatrol()
    {
        if (!enablePatrolScreams) return;
        if (patrolScreamOnEnter) PlayScream(true);
        if (_nextPatrolTime < 0f) ScheduleNextPatrol();
    }

    public void Tick(EnemyPatrolAI.AIState state)
    {
        float now = Time.time;
        // CHASE
        if (enableChaseScreams && loopChaseScreams && state == EnemyPatrolAI.AIState.Chase)
        {
            if (_nextChaseTime >= 0f && now >= _nextChaseTime)
            {
                PlayScream(false);
                ScheduleNextChase();
            }
        }
        else if (state != EnemyPatrolAI.AIState.Chase)
        {
            _nextChaseTime = -1f; // invalidar al salir
        }

        // PATROL
        if (enablePatrolScreams && (state == EnemyPatrolAI.AIState.Patrol || state == EnemyPatrolAI.AIState.Wait))
        {
            if (_nextPatrolTime < 0f) ScheduleNextPatrol();
            else if (now >= _nextPatrolTime)
            {
                PlayScream(true);
                ScheduleNextPatrol();
            }
        }
        else if (state != EnemyPatrolAI.AIState.Patrol && state != EnemyPatrolAI.AIState.Wait)
        {
            _nextPatrolTime = -1f;
        }
    }

    private void ScheduleNextChase()
    {
        if (!loopChaseScreams || !enableChaseScreams) { _nextChaseTime = -1f; return; }
        if (chaseIntervalMax < chaseIntervalMin) chaseIntervalMax = chaseIntervalMin;
        _nextChaseTime = Time.time + Random.Range(chaseIntervalMin, chaseIntervalMax);
    }

    private void ScheduleNextPatrol()
    {
        if (!enablePatrolScreams) { _nextPatrolTime = -1f; return; }
        if (patrolIntervalMax < patrolIntervalMin) patrolIntervalMax = patrolIntervalMin;
        _nextPatrolTime = Time.time + Random.Range(patrolIntervalMin, patrolIntervalMax);
    }

    private void PlayScream(bool isPatrol)
    {
        if (screamClips == null || screamClips.Length == 0) return;
        if (AudioManager.Instance == null) return;
        int idx;
        if (screamClips.Length == 1) idx = 0; else { do { idx = Random.Range(0, screamClips.Length); } while (idx == _lastIndex); }
        _lastIndex = idx;
        var clip = screamClips[idx];
        if (clip == null) return;
        float vol = (isPatrol && patrolScreamVolume >= 0f) ? patrolScreamVolume : chaseScreamVolume;
        AudioManager.Instance.Play3DFollow(clip, _followTransform, vol);
        if (debugLogs)
        {
            string mode = isPatrol ? "PATROL" : "CHASE";
            Debug.Log($"[EnemyVocalizer] Scream {mode} '{clip.name}' t={Time.time:F2}", this);
        }
    }
}
