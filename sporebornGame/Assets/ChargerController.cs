using System.Collections;
using UnityEngine;
using Pathfinding;

[RequireComponent(typeof(AIPath))]
[RequireComponent(typeof(AIDestinationSetter))]
[RequireComponent(typeof(Rigidbody2D))]
public class ChargerController : MonoBehaviour
{
    public enum State { Idle, Chase, Windup, Charge, Recover }

    [Header("Aggro")]
    public float aggroRadius = 6f;

    [Header("Charge Settings")]
    [Tooltip("ON = A* pathfinds while charging. OFF = straight physics dash to last known player position.")]
    public bool pathfindWhileCharging = true;
    public float chargeSpeed = 11f;
    public float acceleration = 20f;   // only used to smooth A* spikes if instantSpeedSpike=false

    [Header("Timing")]
    [Min(0.3f)] public float windupTime = 0.35f;
    public float chargeDuration = 2.5f;   // <-- set to your full desired time
    public float recoverTime = 2.0f;

    [Header("Visual Tell (optional)")]
    public SpriteRenderer spriteRenderer;
    public Color windupTint = new Color(1f, 0.85f, 0.35f);
    public Color chargeTint = Color.red;

    [Header("Charge Behaviour (A* mode only)")]
    public bool instantSpeedSpike = true;
    public bool tighterTurningWhileCharging = true;

    [Header("Physics Dash (OFF mode)")]
    public LayerMask obstacleMask;   // walls/obstacles that should stop the dash early

    // internals
    private AIPath _ai;
    private AIDestinationSetter _dest;
    private Transform _player;
    private Rigidbody2D _rb;
    private State _state = State.Idle;

    private float _timer;
    private Color _origTint;
    private float _zLock;

    // A* baseline we restore after charge
    private float _origMaxSpeed;
    private bool _origSlowWhenNotFacingTarget;
    private float _origPickNextWaypointDist;

    // physics dash data (OFF mode)
    private Vector2 _lockedTargetPos;
    private Vector2 _dashDir;

    void Awake()
    {
        _ai = GetComponent<AIPath>();
        _dest = GetComponent<AIDestinationSetter>();
        _rb = GetComponent<Rigidbody2D>();

        if (_dest.target == null)
        {
            var p = GameObject.FindGameObjectWithTag("Player");
            if (p) _dest.target = p.transform;
        }
        _player = _dest.target;

        _ai.updateRotation = false;
        _ai.canMove = true;

        _origMaxSpeed = _ai.maxSpeed;
        _origSlowWhenNotFacingTarget = _ai.slowWhenNotFacingTarget;
        _origPickNextWaypointDist    = _ai.pickNextWaypointDist;

        _rb.gravityScale = 0f;
        _rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        _rb.interpolation = RigidbodyInterpolation2D.Interpolate;
        _rb.freezeRotation = true;

        if (!spriteRenderer) spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        if (spriteRenderer) _origTint = spriteRenderer.color;

        _zLock = transform.position.z;
    }

    void Update()
    {
        switch (_state)
        {
            case State.Idle:
            case State.Chase:
                RunChase();
                break;

            case State.Windup:
                _timer -= Time.deltaTime;
                if (_timer <= 0f) BeginCharge();
                break;

            case State.Charge:
                _timer -= Time.deltaTime;
                if (_timer <= 0f) EndCharge();   // full duration unless collision
                break;

            case State.Recover:
                _timer -= Time.deltaTime;
                if (_timer <= 0f) Transition(State.Chase);
                break;
        }
    }

    void FixedUpdate()
    {
        // Only drive physics when we’re in physics-dash mode (OFF)
        if (_state == State.Charge && !pathfindWhileCharging)
        {
            _rb.linearVelocity = _dashDir * chargeSpeed;
            // NO distance/overshoot check here → ensures full duration run
        }
    }

    void LateUpdate()
    {
        var p = transform.position;
        if (Mathf.Abs(p.z - _zLock) > 0.0001f)
            transform.position = new Vector3(p.x, p.y, _zLock);
    }

    void RunChase()
    {
        if (_player == null) { Transition(State.Idle); return; }

        // A* should be on in Chase/Idle
        _ai.canMove = true;
        _ai.canSearch = true;
        _ai.maxSpeed = _origMaxSpeed;

        float dist = Vector2.Distance(transform.position, _player.position);
        if (dist <= aggroRadius && _state != State.Windup)
            StartCoroutine(WindupRoutine());
        else
            Transition(State.Chase);
    }

    IEnumerator WindupRoutine()
    {
        if (_state == State.Windup || _state == State.Charge || _state == State.Recover)
            yield break;

        Transition(State.Windup);

        // keep moving during wind-up
        if (spriteRenderer) spriteRenderer.color = windupTint;

        _timer = Mathf.Max(0.3f, windupTime);
        float t = _timer;
        while (t > 0f && _state == State.Windup)
        {
            t -= Time.deltaTime;
            yield return null;
        }
        // Update() → BeginCharge next frame
    }

    void BeginCharge()
    {
        if (spriteRenderer) spriteRenderer.color = chargeTint;
        _timer = chargeDuration;

        if (pathfindWhileCharging)
        {
            // A* mode: keep pathfinding and spike speed
            _ai.canMove = true;
            _ai.canSearch = true;

            if (instantSpeedSpike)
                _ai.maxSpeed = chargeSpeed;
            else
                _ai.maxSpeed = Mathf.MoveTowards(_ai.maxSpeed, chargeSpeed, acceleration * Time.deltaTime);

            if (tighterTurningWhileCharging)
            {
                _ai.slowWhenNotFacingTarget = false;
                _ai.pickNextWaypointDist    = Mathf.Max(0.2f, _origPickNextWaypointDist * 0.5f);
            }
        }
        else
        {
            // Physics-dash mode: lock player's position NOW
            _lockedTargetPos = _player ? (Vector2)_player.position : (Vector2)transform.position;
            _dashDir = (_lockedTargetPos - (Vector2)transform.position).normalized;
            if (_dashDir.sqrMagnitude < 0.0001f) _dashDir = Vector2.right;

            // FULLY disable A* so it can't interrupt
            _ai.canMove = false;
            _ai.canSearch = false;

            // start dash; FixedUpdate will keep velocity for the full duration
            _rb.linearVelocity = _dashDir * chargeSpeed;
        }

        Transition(State.Charge);
    }

    void EndCharge()
    {
        // stop physics if we were dashing
        _rb.linearVelocity = Vector2.zero;

        // restore A* baseline
        _ai.maxSpeed = _origMaxSpeed;
        _ai.slowWhenNotFacingTarget = _origSlowWhenNotFacingTarget;
        _ai.pickNextWaypointDist    = _origPickNextWaypointDist;
        _ai.canSearch = true;
        _ai.canMove = true;
        _ai.SearchPath();

        if (spriteRenderer) spriteRenderer.color = _origTint;

        _timer = recoverTime;
        Transition(State.Recover);
    }

    void Transition(State s) => _state = s;

    void OnCollisionEnter2D(Collision2D col)
    {
        if (_state == State.Charge && !pathfindWhileCharging)
        {
            // physics mode: hitting an obstacle ends the dash early (by design)
            if (((1 << col.collider.gameObject.layer) & obstacleMask) != 0)
                EndCharge();
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1f, 0.3f, 0.3f, 0.25f);
        Gizmos.DrawWireSphere(transform.position, aggroRadius);
    }
}
