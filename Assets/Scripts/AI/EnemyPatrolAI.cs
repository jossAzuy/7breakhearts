using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// IA sencilla de patrullaje con NavMesh + gizmos.
/// Estados: Idle (sin waypoints), Patrol, Wait, Chase (opcional si jugador entra en radio).
/// - Asigna un array de waypoints (Transform) en el Inspector.
/// - Usa un NavMeshAgent (se añade con RequireComponent).
/// - Dibuja gizmos: líneas entre waypoints, esfera de detección y objetivo actual.
/// </summary>
[RequireComponent(typeof(NavMeshAgent))]
public class EnemyPatrolAI : MonoBehaviour
{
    public enum AIState { Idle, Patrol, Wait, Chase }

    [Header("Waypoints")] public Transform[] waypoints;
    [Tooltip("Volver al primer waypoint tras el último.")] public bool loop = true;
    [Tooltip("Distancia para considerar que llegó al waypoint.")] public float waypointTolerance = 0.3f;
    [Tooltip("Tiempo de espera en cada waypoint.")] public float waitAtWaypoint = 1.2f;

    [Header("Velocidades")] public float patrolSpeed = 2.0f;
    public float chaseSpeed = 4.0f;

    [Header("Detección Jugador (opcional)")] public bool enableChase = true;
    public string playerTag = "Player";
    public float detectionRadius = 8f;
    [Tooltip("Factor de histéresis al salir del radio (se necesita superar radius * outFactor para soltar chase)." )]
    public float chaseExitFactor = 1.4f;

    [Header("Debug / Gizmos")] public bool drawDetectionSphere = true;
    public bool drawWaypointLines = true;
    public Color gizmoWaypointColor = new Color(0.2f, 0.8f, 1f, 0.8f);
    public Color gizmoCurrentTargetColor = new Color(1f, 0.5f, 0.1f, 0.9f);
    public Color gizmoDetectionColor = new Color(1f, 0.9f, 0f, 0.25f);

    [Header("Animación / Ataque")]
    [Tooltip("Animator del enemigo.")] public Animator animator;
    [Tooltip("Nombre del parámetro bool que indica movimiento/caminar.")] public string walkBool = "IsWalking";
    [Tooltip("Nombre del trigger para ataque.")] public string attackTrigger = "Attack";
    [Tooltip("Rango para disparar ataque.")] public float attackRange = 1.6f;
    [Tooltip("Tiempo entre ataques.")] public float attackCooldown = 1.2f;
    [Tooltip("Detener al agente mientras dura la animación de ataque (recomendado con evento de final).")]
    public bool stopAgentDuringAttack = true;
    [Tooltip("Velocidad mínima (sqrMagnitude) para considerar que está moviéndose.")] public float movingThresholdSqr = 0.05f;
    [Tooltip("Daño infligido al jugador en cada golpe.")] public float attackDamage = 15f;
    [Tooltip("Evitar que un mismo ataque aplique daño múltiples veces (Animation Event llamado varias veces / frames).")] public bool singleHitPerAttack = true;

    [Header("Audio Golpe Jugador (AudioManager)")]
    [Tooltip("Clip reproducido cuando el ataque conecta (Animation Event -> OnAttackAnimationHit).")]
    public AudioClip hitClip;
    [Range(0f,1f)] [Tooltip("Volumen relativo para AudioManager.Play2D.")] public float hitVolume = 1f;
    [Tooltip("Si hay un AudioManager se usará Play2D; si no, se ignora.")] public bool useAudioManager = true;

    [Header("Vocalizaciones (Componente externo)")]
    [Tooltip("EnemyVocalizer que gestiona gritos. Si es null se buscará en Start.")]
    public EnemyVocalizer vocalizer;

    private float _attackTimer;
    private bool _isAttacking;
    [Header("Debug")] public bool debugCombat = false;

    private NavMeshAgent _agent;
    private int _currentIndex = -1;
    private float _waitTimer;
    private Transform _player;

    public AIState State { get; private set; } = AIState.Idle;

    // Header("Sistema Ventana de Impacto (Opcional)")
    [Tooltip("Usar ventana de tiempo en la animación en lugar / además del Animation Event para aplicar el daño.")] public bool useHitWindow = true;
    [Tooltip("Fragmento que debe contener el nombre del estado de ataque en Animator (case sensitive).")] public string attackStateNameFragment = "Attack";
    [Range(0f,1f)] [Tooltip("Inicio de la ventana (normalized time 0-1).")] public float hitWindowStart = 0.3f;
    [Range(0f,1f)] [Tooltip("Fin de la ventana (normalized time 0-1).")] public float hitWindowEnd = 0.55f;
    [Tooltip("Resetear bandera de daño al iniciar cada ataque (si la animación se reinicia).")] public bool resetHitWindowOnNewAttack = true;
    private bool _hitWindowDamageApplied;

    private bool _attackDamageApplied; // usado por singleHitPerAttack y sincronizado con ventana

    void Awake()
    {
        _agent = GetComponent<NavMeshAgent>();
    }

    void Start()
    {
        if (enableChase)
        {
            var playerGO = GameObject.FindGameObjectWithTag(playerTag);
            if (playerGO != null) _player = playerGO.transform;
        }
        if (vocalizer == null) vocalizer = GetComponent<EnemyVocalizer>();
        InitializePatrol();
    }

    void Update()
    {
        switch (State)
        {
            case AIState.Idle:
                // Si en runtime asignan waypoints, iniciar.
                if (waypoints != null && waypoints.Length > 0)
                    InitializePatrol();
                break;
            case AIState.Patrol:
                TickPatrol();
                break;
            case AIState.Wait:
                TickWait();
                break;
            case AIState.Chase:
                TickChase();
                break;
        }

        if (enableChase && _player != null)
        {
            EvaluateChaseTransition();
        }

    TickAnimation();
    if (useHitWindow) TickHitWindowDamage();
    if (vocalizer != null) vocalizer.Tick(State);
    }

    private void InitializePatrol()
    {
        if (waypoints == null || waypoints.Length == 0)
        {
            State = AIState.Idle;
            _agent.ResetPath();
            return;
        }
        _agent.speed = patrolSpeed;
        _currentIndex = 0;
        SetDestinationCurrentIndex();
        State = AIState.Patrol;
        OnEnterPatrol();
    }

    private void SetDestinationCurrentIndex()
    {
        if (_currentIndex < 0 || _currentIndex >= waypoints.Length) return;
        Transform t = waypoints[_currentIndex];
        if (t != null)
        {
            _agent.SetDestination(t.position);
        }
    }

    private void AdvanceWaypoint()
    {
        if (waypoints.Length == 0) return;
        _currentIndex++;
        if (_currentIndex >= waypoints.Length)
        {
            if (loop) _currentIndex = 0; else { State = AIState.Idle; _agent.ResetPath(); return; }
        }
        SetDestinationCurrentIndex();
    }

    private void TickPatrol()
    {
        if (_agent.pathPending) return;
        if (_agent.remainingDistance <= _agent.stoppingDistance + waypointTolerance)
        {
            if (waitAtWaypoint > 0f)
            {
                _waitTimer = 0f;
                State = AIState.Wait;
            }
            else
            {
                AdvanceWaypoint();
            }
        }
    }

    private void TickWait()
    {
        _waitTimer += Time.deltaTime;
        if (_waitTimer >= waitAtWaypoint)
        {
            AdvanceWaypoint();
            if (State != AIState.Idle) State = AIState.Patrol;
        }
    }

    private void TickChase()
    {
        if (_player == null)
        {
            ReturnToPatrol();
            return;
        }
        _agent.speed = chaseSpeed;
        _agent.SetDestination(_player.position);
        TryAttack();
    }

    private void TickAnimation()
    {
        if (animator == null) return;
        _attackTimer -= Time.deltaTime;

        // Actualizar bool de caminar (movimiento general excepto cuando lo detenemos por ataque)
        bool moving = !_agent.isStopped && _agent.velocity.sqrMagnitude > movingThresholdSqr && State != AIState.Wait;
        animator.SetBool(walkBool, moving);
    }

    private void TryAttack()
    {
        if (animator == null || _player == null) return;
        if (State != AIState.Chase) return;
        if (_isAttacking) return; // esperando fin de animación
        if (_attackTimer > 0f) return;

        float dist = Vector3.Distance(transform.position, _player.position);
        if (dist <= attackRange)
        {
            animator.ResetTrigger(attackTrigger); // limpiar por seguridad
            animator.SetTrigger(attackTrigger);
            _attackTimer = attackCooldown;
            if (singleHitPerAttack) _attackDamageApplied = false;
            if (useHitWindow && resetHitWindowOnNewAttack) _hitWindowDamageApplied = false;
            if (stopAgentDuringAttack)
            {
                StartCoroutine(AttackStopCoroutine());
            }
        }
    }

    private System.Collections.IEnumerator AttackStopCoroutine()
    {
        _isAttacking = true;
        bool prevStopped = _agent.isStopped;
        _agent.isStopped = true;
        // Espera un frame para iniciar animación y evitar snap
        yield return null;
        // Espera hasta que el cooldown llegue a (attackCooldown - 0.2) o se dispare evento manual de OnAttackAnimationEnd
        float expected = attackCooldown * 0.7f; // heurística: la mayor parte de la animación
        float t = 0f;
        while (t < expected)
        {
            t += Time.deltaTime;
            yield return null;
        }
        if (!prevStopped) _agent.isStopped = false; else _agent.isStopped = prevStopped;
        _isAttacking = false;
    }

    // Opcional: Llamar este método desde un Animation Event al final del clip de ataque para mayor precisión.
    public void OnAttackAnimationEnd()
    {
        if (stopAgentDuringAttack && _isAttacking)
        {
            _agent.isStopped = false;
            _isAttacking = false;
        }
        // En caso de necesitar permitir siguiente ataque aunque la animación tenga extended frames
        if (singleHitPerAttack) _attackDamageApplied = true; // asegurarnos de no aplicar daño fuera de ventana
    }

    // Llamar desde un Animation Event en el momento del impacto (no al final) para aplicar daño.
    public void OnAttackAnimationHit()
    {
        if (useHitWindow) return; // ignorar si usamos ventana
        if (_player == null) return;
        if (singleHitPerAttack && _attackDamageApplied) return; // ya aplicamos daño este ataque
        var health = _player.GetComponent<HealthComponent>();
        if (health != null && Vector3.Distance(transform.position, _player.position) <= attackRange + 0.3f)
        {
            health.ApplyDamage(attackDamage);
            if (singleHitPerAttack) _attackDamageApplied = true;
            if (debugCombat)
            {
                Debug.Log($"[EnemyPatrolAI] Hit player for {attackDamage} at {Time.time:F2}s (player hp: {health.Current}/{health.maxHealth})", this);
            }
            if (useAudioManager && hitClip != null && AudioManager.Instance != null)
            {
                AudioManager.Instance.Play2D(hitClip, hitVolume);
            }
        }
    }

    private void EvaluateChaseTransition()
    {
        float dist = Vector3.Distance(transform.position, _player.position);
        if (State != AIState.Chase)
        {
            if (dist <= detectionRadius)
            {
                // Entrar en persecución
                State = AIState.Chase;
                OnEnterChase();
            }
        }
        else // ya en Chase
        {
            if (dist > detectionRadius * chaseExitFactor)
            {
                ReturnToPatrol();
            }
        }
    }

    private void OnEnterChase()
    {
        if (vocalizer != null) vocalizer.OnEnterChase();
    }

    // (Lógica de temporizado de gritos movida a EnemyVocalizer)

    // (Reproducción de clips movida a EnemyVocalizer)

    private void ReturnToPatrol()
    {
        if (waypoints == null || waypoints.Length == 0)
        {
            State = AIState.Idle;
            _agent.ResetPath();
            return;
        }
        _agent.speed = patrolSpeed;
        // Buscar el waypoint más cercano para reenganchar
        float bestDist = float.MaxValue;
        int bestIndex = 0;
        for (int i = 0; i < waypoints.Length; i++)
        {
            if (waypoints[i] == null) continue;
            float d = Vector3.SqrMagnitude(transform.position - waypoints[i].position);
            if (d < bestDist)
            {
                bestDist = d;
                bestIndex = i;
            }
        }
        _currentIndex = bestIndex;
        SetDestinationCurrentIndex();
        State = AIState.Patrol;
        OnEnterPatrol();
    }

    private void OnEnterPatrol()
    {
        if (vocalizer != null) vocalizer.OnEnterPatrol();
    }

    private void TickHitWindowDamage()
    {
        if (!useHitWindow) return;
        if (animator == null) return;
        if (State != AIState.Chase) return;
        if (_player == null) return;

        var st = animator.GetCurrentAnimatorStateInfo(0);
        bool matches = st.IsTag(attackStateNameFragment) || st.IsName(attackStateNameFragment);
        if (!matches)
        {
            // Inspeccionar clips actuales para coincidencia parcial
            var clips = animator.GetCurrentAnimatorClipInfo(0);
            matches = false;
            string clipFound = null;
            for (int i = 0; i < clips.Length; i++)
            {
                var clip = clips[i].clip;
                if (clip != null && clip.name.IndexOf(attackStateNameFragment, System.StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    matches = true;
                    clipFound = clip.name;
                    break;
                }
            }
            if (!matches)
            {
                if (debugCombat)
                {
                    Debug.Log($"[EnemyPatrolAI][HitWindow] No match state/tag/clip para fragment '{attackStateNameFragment}'. Fallback Animation Event.", this);
                }
                return; // no aplicamos daño por ventana esta frame (Animation Event puede seguir activo si no lo quitaste)
            }
            else if (debugCombat)
            {
                Debug.Log($"[EnemyPatrolAI][HitWindow] Detectado clip '{clipFound}'", this);
            }
        }

        float nt = st.normalizedTime; // >= 0
        float frac = nt - Mathf.Floor(nt);
        if (hitWindowEnd < hitWindowStart) hitWindowEnd = hitWindowStart;

        if (frac >= hitWindowStart && frac <= hitWindowEnd)
        {
            if (!_hitWindowDamageApplied)
            {
                float dist = Vector3.Distance(transform.position, _player.position);
                if (dist <= attackRange + 0.3f)
                {
                    var health = _player.GetComponent<HealthComponent>();
                    if (health != null)
                    {
                        health.ApplyDamage(attackDamage);
                        _hitWindowDamageApplied = true;
                        if (singleHitPerAttack) _attackDamageApplied = true;
                        if (debugCombat)
                        {
                            Debug.Log($"[EnemyPatrolAI][HitWindow] Damage {attackDamage} nt={frac:F2} hp:{health.Current}/{health.maxHealth}", this);
                        }
                        if (useAudioManager && hitClip != null && AudioManager.Instance != null)
                        {
                            AudioManager.Instance.Play2D(hitClip, hitVolume);
                        }
                    }
                }
                else if (debugCombat)
                {
                    Debug.Log($"[EnemyPatrolAI][HitWindow] Jugador fuera de rango dist={dist:F2} (<= {attackRange + 0.3f})", this);
                }
            }
        }
        else if (frac > hitWindowEnd && resetHitWindowOnNewAttack == false)
        {
            if (!_hitWindowDamageApplied && singleHitPerAttack)
            {
                _hitWindowDamageApplied = true;
            }
        }
    }

    void OnDrawGizmosSelected()
    {
        // Detección
        if (drawDetectionSphere && enableChase)
        {
            Gizmos.color = gizmoDetectionColor;
            Gizmos.DrawSphere(transform.position, detectionRadius);
        }

        // Waypoints
        if (drawWaypointLines && waypoints != null)
        {
            Gizmos.color = gizmoWaypointColor;
            Transform prev = null;
            for (int i = 0; i < waypoints.Length; i++)
            {
                var w = waypoints[i];
                if (w == null) continue;
                Gizmos.DrawWireSphere(w.position, 0.25f);
                if (prev != null)
                {
                    Gizmos.DrawLine(prev.position, w.position);
                }
                prev = w;
            }
            if (loop && waypoints.Length > 1 && waypoints[0] != null && prev != null)
            {
                Gizmos.DrawLine(prev.position, waypoints[0].position);
            }
        }

        // Destino actual
        if (Application.isPlaying && State != AIState.Idle && _agent != null && _agent.hasPath)
        {
            Gizmos.color = gizmoCurrentTargetColor;
            Gizmos.DrawWireSphere(_agent.destination, 0.35f);
            Gizmos.DrawLine(transform.position, _agent.destination);
        }
    }
}
