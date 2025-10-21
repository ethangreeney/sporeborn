using System;
using UnityEngine;
using UnityEngine.Events;
using Pathfinding;

[RequireComponent(typeof(AIPath))]
[RequireComponent(typeof(AIDestinationSetter))]
[RequireComponent(typeof(Rigidbody2D))]
public class FinalBossController : MonoBehaviour
{
    public enum Phase { Phase1, Phase2_ShieldRanged, Phase3_Charge, Dead }

    [Header("Health Source")]
    public HealthModel healthModel;

    [Header("Phase Thresholds")]
    [Tooltip("Enter Phase 2 when currentHealth <= maxHealth * threshold (e.g., 0.66 for 66%).")]
    [Range(0.1f, 0.99f)] public float phase2Threshold = 0.66f;

    [Header("Ranged (Phase 2)")]
    [SerializeField] string playerTag = "Player";
    [SerializeField] GameObject projectilePrefab;   // EnemyProjectile prefab implementing EnemyProjectileScript
    [SerializeField] float attackRange = 7f;        // Start shooting within this distance
    [SerializeField] float fireCooldown = 1f;       // Seconds between shots
    private float _rangedTimer;

    [Header("Shield Visual (Phase 2)")]
    [Tooltip("forcefield.png. A child SpriteRenderer will be created and only enabled during Phase 2.")]
    public Sprite shieldSprite;
    [Tooltip("Sorting order difference above the boss body.")]
    public int shieldOrderOffset = 5;
    [Tooltip("Initial scale of the shield child.")]
    public float shieldInitialScale = 1f;
    [Tooltip("Initial local offset of the shield child.")]
    public Vector2 shieldInitialOffset = Vector2.zero;

    [Header("Spawner (Phase 2)")]
    [Tooltip("Assign BossMinionSpawner (expects: void SpawnWave(); bool HasActiveMinions {get;})")]
    public MonoBehaviour minionSpawner;

    [Header("Phase 2 Behaviour")]
    [Tooltip("If true AND no spawner/flag, auto-advance to Phase 3 after fallbackSeconds.")]
    public bool phase2AutoAdvanceWithoutSpawner = false;
    [Tooltip("Delay before we first read HasActiveMinions so minions can initialize.")]
    public float phase2MinionInitGrace = 0.35f;
    public float phase2FallbackSeconds = 6f;

    [Header("Phase 3 Charging (A* pathfinds while charging)")]
    [Min(0.3f)] public float p3WindupTime = 0.35f;
    public float p3ChargeDuration = 2.5f;
    public float p3RecoverTime = 2.0f;
    [Tooltip("Temporary A* maxSpeed while charging.")]
    public float p3ChargeSpeed = 11f;
    [Tooltip("If false, lerp up to charge speed (still only touches maxSpeed).")]
    public bool p3InstantSpeedSpike = true;

    [Header("Events")]
    public UnityEvent onBossDefeated;
    public UnityEvent<Phase> onPhaseChanged;
    public UnityEvent<int,int> onHealthChanged;

    // ---- Internals ----
    private Phase _phase = Phase.Phase1;
    private bool _invulnerable;
    private float _phase2LockedHP;
    private float _phase2CheckDelay;
    private bool  _phase2FallbackArmed;

    // Movement/target
    private Transform _player;
    private AIPath _ai;
    private AIDestinationSetter _dest;
    private Rigidbody2D _rb;

    // Cache original settings (for clean restore)
    private float _origMaxSpeed;
    private float _origPickNextWaypointDist;
    private bool  _origSlowWhenNotFacingTarget;
    private float _origRepathRate;
    private float _origSlowdownDistance;
    private float _origEndReachedDistance;
    private RigidbodyConstraints2D _origConstraints;
    private float _zLock;

    // Phase 3 local state (Charger-style)
    private enum P3State { Idle, Windup, Charge, Recover }
    private P3State _p3State = P3State.Idle;
    private float _p3Timer;

    // Shield (child so we can scale independently)
    private SpriteRenderer _shieldRenderer;
    private Transform _shieldTransform;

    // ---- Public read-only ----
    public int CurrentHP => healthModel ? Mathf.RoundToInt(healthModel.currHealth) : 0;
    public int MaxHP => healthModel ? Mathf.RoundToInt(healthModel.maxHealth) : 0;
    public Phase CurrentPhase => _phase;
    public bool IsInvulnerable => _invulnerable;

    private void Awake()
    {
        if (!healthModel) healthModel = GetComponent<HealthModel>();
        _ai   = GetComponent<AIPath>();
        _dest = GetComponent<AIDestinationSetter>();
        _rb   = GetComponent<Rigidbody2D>();

        if (_dest && _dest.target == null)
        {
            var p = GameObject.FindWithTag(playerTag);
            if (p) _dest.target = p.transform;
        }
        _player = _dest ? _dest.target : null;

        if (_ai)
        {
            _ai.updateRotation = false;
            _origMaxSpeed                = _ai.maxSpeed;
            _origSlowWhenNotFacingTarget = _ai.slowWhenNotFacingTarget;
            _origPickNextWaypointDist    = _ai.pickNextWaypointDist;
            _origRepathRate              = _ai.repathRate;
            _origSlowdownDistance        = _ai.slowdownDistance;
            _origEndReachedDistance      = _ai.endReachedDistance;
        }

        // RB2D parity with Charger for smooth high-speed motion
        _rb.gravityScale = 0f;
        _rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        _rb.interpolation = RigidbodyInterpolation2D.Interpolate;
        _rb.freezeRotation = true;

        _origConstraints = _rb.constraints;
        _zLock = transform.position.z;

        if (!healthModel)
            Debug.LogError("[Boss] FinalBossController requires a HealthModel on the same GameObject.", this);

        SetupShieldChild();
    }

    private void OnEnable()
    {
        EnterPhase1();
        if (healthModel) healthModel.OnHealthChanged += HandleHealthChanged;
        if (healthModel) onHealthChanged?.Invoke(CurrentHP, MaxHP);
        EvaluatePhaseTransitions();
    }

    private void OnDisable()
    {
        if (healthModel) healthModel.OnHealthChanged -= HandleHealthChanged;
    }

    private void Update()
    {
        if (_phase == Phase.Dead) return;

        // Clamp HP while invulnerable (Phase 2 cannot die)
        if (_phase == Phase.Phase2_ShieldRanged && _invulnerable && healthModel && healthModel.currHealth < _phase2LockedHP)
        {
            healthModel.currHealth = _phase2LockedHP;
            onHealthChanged?.Invoke(CurrentHP, MaxHP);
        }

        EvaluatePhaseTransitions();

        switch (_phase)
        {
            case Phase.Phase2_ShieldRanged:
                TickPhase2_Ranged();
                break;
            case Phase.Phase3_Charge:
                TickPhase3_Charge();
                break;
        }
    }

    private void FixedUpdate()
    {
        if (_phase == Phase.Phase3_Charge && (_p3State == P3State.Windup || _p3State == P3State.Recover))
        {
            _rb.linearVelocity = Vector2.zero;
        }
    }

    private void LateUpdate()
    {
        var p = transform.position;
        if (Mathf.Abs(p.z - _zLock) > 0.0001f)
            transform.position = new Vector3(p.x, p.y, _zLock);
    }

    // ---------------- HEALTH / PHASE TRANSITIONS ----------------
    private void HandleHealthChanged()
    {
        onHealthChanged?.Invoke(CurrentHP, MaxHP);
        EvaluatePhaseTransitions();
    }

    private void EvaluatePhaseTransitions()
    {
        if (!healthModel) return;

        if (healthModel.currHealth <= 0f)
        {
            if (_phase != Phase.Dead) Die();
            return;
        }

        float thresholdValue = healthModel.maxHealth * phase2Threshold;
        if (_phase == Phase.Phase1 && healthModel.currHealth <= thresholdValue)
        {
            EnterPhase2_ShieldRanged();
        }
    }

    // ---------------- PHASE 1 ----------------
    private void EnterPhase1()
    {
        _phase = Phase.Phase1; onPhaseChanged?.Invoke(_phase);
        _invulnerable = false;
        if (_shieldRenderer) _shieldRenderer.enabled = false; // hide shield
        _rangedTimer = 0f;

        _rb.constraints = _origConstraints;

        Debug.Log("[Boss] Phase 1: Chase (contact damage handled by Player)");
    }

    // ---------------- PHASE 2 (Shield + Ranged + Minions) ----------------
    private void EnterPhase2_ShieldRanged()
    {
        _phase = Phase.Phase2_ShieldRanged; onPhaseChanged?.Invoke(_phase);
        _invulnerable = true;
        _phase2LockedHP = healthModel ? healthModel.currHealth : 0f;

        if (_shieldRenderer) _shieldRenderer.enabled = true;  // show shield
        _rangedTimer = 0f;

        // Freeze position (X & Y) but leave A* untouched
        _rb.constraints = (_origConstraints | RigidbodyConstraints2D.FreezePositionX | RigidbodyConstraints2D.FreezePositionY);
        _rb.linearVelocity = Vector2.zero;

        // Arm grace delay before reading HasActiveMinions
        _phase2CheckDelay = Mathf.Max(0f, phase2MinionInitGrace);
        _phase2FallbackArmed = false;

        if (minionSpawner)
        {
            var m = minionSpawner.GetType().GetMethod("SpawnWave");
            if (m != null) m.Invoke(minionSpawner, null);

            var p = minionSpawner.GetType().GetProperty("HasActiveMinions");
            if (p == null && phase2AutoAdvanceWithoutSpawner && !_phase2FallbackArmed)
            {
                _phase2FallbackArmed = true;
                Invoke(nameof(EnterPhase3_Charge), Mathf.Max(0.1f, phase2FallbackSeconds));
            }
        }
        else if (phase2AutoAdvanceWithoutSpawner && !_phase2FallbackArmed)
        {
            _phase2FallbackArmed = true;
            Invoke(nameof(EnterPhase3_Charge), Mathf.Max(0.1f, phase2FallbackSeconds));
        }

        Debug.Log("[Boss] Phase 2: Shield + Ranged + Minions" + (_phase2FallbackArmed ? " (fallback armed)" : ""));
    }

    private void TickPhase2_Ranged()
    {
        if (_phase2CheckDelay > 0f)
        {
            _phase2CheckDelay -= Time.deltaTime;
            HandleRangedFire();
            return;
        }

        if (minionSpawner)
        {
            var p = minionSpawner.GetType().GetProperty("HasActiveMinions");
            if (p != null && p.PropertyType == typeof(bool))
            {
                bool hasActiveMinions = (bool)p.GetValue(minionSpawner);
                if (!hasActiveMinions)
                {
                    EnterPhase3_Charge();
                }
            }
        }

        HandleRangedFire();
    }

    private void HandleRangedFire()
    {
        if (!_player || !projectilePrefab) return;

        _rangedTimer -= Time.deltaTime;
        float mult = (DifficultyManager.Instance ? DifficultyManager.Instance.EnemyFireIntervalMult : 1f);
        if (Vector2.Distance(transform.position, _player.position) <= attackRange && _rangedTimer <= 0f)
        {
            ShootAtPlayer();
            _rangedTimer = fireCooldown * mult;
        }
    }

    private void ShootAtPlayer()
    {
        if (!projectilePrefab) { Debug.LogError("[Boss] Assign projectilePrefab for Phase 2 ranged.", this); return; }
        if (!_player) return;

        Vector2 dir = (_player.position - transform.position).normalized;
        var go = Instantiate(projectilePrefab, transform.position, Quaternion.identity);
        var proj = go.GetComponent<EnemyProjectileScript>();
        if (!proj) { Debug.LogError("[Boss] Projectile prefab missing EnemyProjectileScript.", this); return; }
        proj.Initialize(dir);
    }

    // ---------------- PHASE 3 (Charger-style: A* pathfinds while charging) ----------------
    private void EnterPhase3_Charge()
    {
        if (_phase == Phase.Phase3_Charge) return;
        CancelInvoke(nameof(EnterPhase3_Charge)); // cancel any fallback

        _phase = Phase.Phase3_Charge; onPhaseChanged?.Invoke(_phase);
        _invulnerable = false;
        if (_shieldRenderer) _shieldRenderer.enabled = false; // hide shield

        _rb.constraints = _origConstraints;
        _rb.linearVelocity = Vector2.zero;

        _p3State = P3State.Windup;
        _p3Timer = Mathf.Max(0.3f, p3WindupTime);

        Debug.Log("[Boss] Phase 3: Charging");
    }

    private void TickPhase3_Charge()
    {
        switch (_p3State)
        {
            case P3State.Windup:
                _p3Timer -= Time.deltaTime;
                if (_p3Timer <= 0f) BeginP3Charge();
                break;

            case P3State.Charge:
                _p3Timer -= Time.deltaTime;
                if (_p3Timer <= 0f) EndP3Charge();
                break;

            case P3State.Recover:
                _p3Timer -= Time.deltaTime;
                if (_p3Timer <= 0f) BeginP3Windup();
                break;
        }
    }

    private void BeginP3Windup()
    {
        _p3State = P3State.Windup;
        _p3Timer = Mathf.Max(0.3f, p3WindupTime);

        if (_ai)
        {
            _ai.maxSpeed                = _origMaxSpeed;
            _ai.slowWhenNotFacingTarget = _origSlowWhenNotFacingTarget;
            _ai.pickNextWaypointDist    = _origPickNextWaypointDist;
            _ai.repathRate              = _origRepathRate;
            _ai.slowdownDistance        = _origSlowdownDistance;
            _ai.endReachedDistance      = _origEndReachedDistance;
        }
    }

    private void BeginP3Charge()
    {
        _p3State = P3State.Charge;
        _p3Timer = Mathf.Max(0.1f, p3ChargeDuration);

        if (_ai)
        {
            _ai.canMove = true;
            _ai.canSearch = true;

            _ai.maxSpeed = p3InstantSpeedSpike
                ? p3ChargeSpeed
                : Mathf.MoveTowards(_ai.maxSpeed, p3ChargeSpeed, 20f * Time.deltaTime);

            _ai.slowWhenNotFacingTarget = false;
            _ai.pickNextWaypointDist    = Mathf.Max(0.2f, _origPickNextWaypointDist * 0.5f);
            _ai.repathRate              = 0.05f;
            _ai.slowdownDistance        = 0.05f;
            _ai.endReachedDistance      = 0.01f;

            _ai.SearchPath();
        }
    }

    private void EndP3Charge()
    {
        if (_ai)
        {
            _ai.maxSpeed                = _origMaxSpeed;
            _ai.slowWhenNotFacingTarget = _origSlowWhenNotFacingTarget;
            _ai.pickNextWaypointDist    = _origPickNextWaypointDist;
            _ai.repathRate              = _origRepathRate;
            _ai.slowdownDistance        = _origSlowdownDistance;
            _ai.endReachedDistance      = _origEndReachedDistance;
            _ai.canSearch               = true;
            _ai.canMove                 = true;
            _ai.SearchPath();
        }

        _p3State = P3State.Recover;
        _p3Timer = Mathf.Max(0.1f, p3RecoverTime);
    }

    // ---------------- DEATH ----------------
    private void Die()
    {
        _phase = Phase.Dead; onPhaseChanged?.Invoke(_phase);
        _invulnerable = false;
        if (_shieldRenderer) _shieldRenderer.enabled = false;

        _rb.constraints = _origConstraints;
        _rb.linearVelocity = Vector2.zero;

        if (_ai)
        {
            _ai.maxSpeed                = _origMaxSpeed;
            _ai.slowWhenNotFacingTarget = _origSlowWhenNotFacingTarget;
            _ai.pickNextWaypointDist    = _origPickNextWaypointDist;
            _ai.repathRate              = _origRepathRate;
            _ai.slowdownDistance        = _origSlowdownDistance;
            _ai.endReachedDistance      = _origEndReachedDistance;
            _ai.SearchPath();
        }

        Debug.Log("[Boss] Defeated");
        onBossDefeated?.Invoke();
    }

    // ---------------- DEBUG (optional) ----------------
    [ContextMenu("Debug → Force Phase 2 Now")] private void DebugForcePhase2() => EnterPhase2_ShieldRanged();
    [ContextMenu("Debug → Force Phase 3 Now")] private void DebugForcePhase3() => EnterPhase3_Charge();

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan; Gizmos.DrawWireSphere(transform.position, attackRange);
    }

    // ---------------- Shield child + scale methods ----------------
    private void SetupShieldChild()
    {
        // Find the body SpriteRenderer to copy sorting (before we create the shield)
        var body = GetComponent<SpriteRenderer>();
        if (!body) body = GetComponentInChildren<SpriteRenderer>();

        // Create shield child so we can scale independently
        var go = new GameObject("_ShieldOverlay");
        go.transform.SetParent(transform, false);
        _shieldTransform = go.transform;
        _shieldTransform.localPosition = new Vector3(shieldInitialOffset.x, shieldInitialOffset.y, 0f);
        _shieldTransform.localScale = Vector3.one * Mathf.Max(0.01f, shieldInitialScale);

        _shieldRenderer = go.AddComponent<SpriteRenderer>();
        _shieldRenderer.sprite = shieldSprite;
        _shieldRenderer.enabled = false; // only on in Phase 2

        if (body)
        {
            _shieldRenderer.sortingLayerID = body.sortingLayerID;
            _shieldRenderer.sortingOrder   = body.sortingOrder + shieldOrderOffset;
            _shieldRenderer.material       = body.sharedMaterial;
        }
    }

    /// <summary>Uniform scale for the shield overlay (e.g., 1.25f).</summary>
    public void SetShieldScale(float uniform)
    {
        if (_shieldTransform) _shieldTransform.localScale = new Vector3(Mathf.Max(0.01f, uniform), Mathf.Max(0.01f, uniform), 1f);
    }

    /// <summary>Non-uniform scale (x,y) for the shield overlay.</summary>
    public void SetShieldScale(Vector2 xy)
    {
        if (_shieldTransform) _shieldTransform.localScale = new Vector3(Mathf.Max(0.01f, xy.x), Mathf.Max(0.01f, xy.y), 1f);
    }

    /// <summary>Move the shield overlay relative to the boss.</summary>
    public void SetShieldOffset(Vector2 offset)
    {
        if (_shieldTransform) _shieldTransform.localPosition = new Vector3(offset.x, offset.y, 0f);
    }
}
