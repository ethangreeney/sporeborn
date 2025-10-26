using System.Collections;
using UnityEngine;
using Pathfinding;

[RequireComponent(typeof(AIPath))]
[RequireComponent(typeof(AIDestinationSetter))]
[RequireComponent(typeof(Rigidbody2D))]
public class ChargerController : MonoBehaviour
{
    public enum State { Idle, Chase, Windup, Charge, Stunned, Recover }

    [Header("Aggro")]
    public float aggroRadius = 6f;

    [Header("Charge Settings")]
    [Tooltip("ON = A* pathfinds while charging. OFF = straight physics dash to the last known player position.")]
    public bool pathfindWhileCharging = true;
    public float chargeSpeed = 11f;
    public float acceleration = 20f; // used only if instantSpeedSpike=false (A* mode)

    [Header("Timing")]
    [Min(0.3f)] public float windupTime = 0.35f;
    public float chargeDuration = 2.5f;
    public float recoverTime = 2.0f;

    [Header("Stun on Wall Hit (physics mode only)")]
    [Tooltip("Name of the Layer that should stun the charger when hit during a physics dash.")]
    public string environmentLayerName = "enviroment"; // layer name
    public float stunTime = 0.7f;

    [Header("Visual Tell (optional)")]
    public SpriteRenderer spriteRenderer;
    public Color windupTint = new Color(1f, 0.85f, 0.35f);
    [Header("Charge Behaviour (A* mode only)")]
    public bool instantSpeedSpike = true;
    public bool tighterTurningWhileCharging = true;

    [Header("Physics Dash (OFF mode)")]
    public LayerMask obstacleMask; // optional fallback end condition

    // internals
    private AIPath _ai;
    private AIDestinationSetter _dest;
    private Transform _player;
    private Rigidbody2D _rb;
    private State _state = State.Idle;

    private float _timer;
    private Color _origTint;
    private float _zLock;

    private float _origMaxSpeed;
    private bool _origSlowWhenNotFacingTarget;
    private float _origPickNextWaypointDist;

    private Vector2 _lockedTargetPos;
    private Vector2 _dashDir;

    // cached environment layer id
    private int _environmentLayer = -1;

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

        // cache the layer index from the name
        _environmentLayer = LayerMask.NameToLayer(environmentLayerName);
        if (_environmentLayer == -1)
        {
            Debug.LogWarning($"[ChargerController] Layer '{environmentLayerName}' not found. " +
                             $"Create it in Project Settings > Tags & Layers or change 'environmentLayerName'.");
        }
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
                if (_timer <= 0f) EndCharge();  // full duration unless stunned/collision end
                break;

            case State.Stunned:
                _timer -= Time.deltaTime;
                if (_timer <= 0f) BeginRecover();
                break;

            case State.Recover:
                _timer -= Time.deltaTime;
                if (_timer <= 0f) Transition(State.Chase);
                break;
        }
    }

    void FixedUpdate()
    {
        if (_state == State.Charge && !pathfindWhileCharging)
        {
            _rb.linearVelocity = _dashDir * chargeSpeed; // keep dashing full duration
        }
        else if (_state == State.Stunned || _state == State.Recover || _state == State.Windup)
        {
            _rb.linearVelocity = Vector2.zero; // no drift
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
        if (_state == State.Windup || _state == State.Charge || _state == State.Stunned || _state == State.Recover)
            yield break;

        Transition(State.Windup);

        if (spriteRenderer) spriteRenderer.color = windupTint;

        _timer = Mathf.Max(0.3f, windupTime);
        float t = _timer;
        while (t > 0f && _state == State.Windup)
        {
            t -= Time.deltaTime;
            yield return null;
        }
    }

    void BeginCharge()
    {
        if (spriteRenderer) 
        _timer = chargeDuration;

        if (pathfindWhileCharging)
        {
            _ai.canMove = true;
            _ai.canSearch = true;

            _ai.maxSpeed = instantSpeedSpike
                ? chargeSpeed
                : Mathf.MoveTowards(_ai.maxSpeed, chargeSpeed, acceleration * Time.deltaTime);

            if (tighterTurningWhileCharging)
            {
                _ai.slowWhenNotFacingTarget = false;
                _ai.pickNextWaypointDist    = Mathf.Max(0.2f, _origPickNextWaypointDist * 0.5f);
            }
        }
        else
        {
            // lock target and dash straight
            _lockedTargetPos = _player ? (Vector2)_player.position : (Vector2)transform.position;
            _dashDir = (_lockedTargetPos - (Vector2)transform.position).normalized;
            if (_dashDir.sqrMagnitude < 0.0001f) _dashDir = Vector2.right;

            // fully disable A* so it can't interrupt
            _ai.canMove = false;
            _ai.canSearch = false;

            _rb.linearVelocity = _dashDir * chargeSpeed;
        }

        Transition(State.Charge);
    }

    void BeginRecover()
    {
        _timer = recoverTime;
        Transition(State.Recover);

        // re-enable A*
        _ai.canSearch = true;
        _ai.canMove   = true;
        _ai.maxSpeed  = _origMaxSpeed;
        _ai.SearchPath();

        if (spriteRenderer) spriteRenderer.color = _origTint;
    }

    void EndCharge()
    {
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

    // ---- Wall Stun (by LAYER) ----
    void OnCollisionEnter2D(Collision2D col)
    {
        if (_state == State.Charge && !pathfindWhileCharging)
            TryStunOnEnvironment(col);
    }

    void OnCollisionStay2D(Collision2D col)
    {
        if (_state == State.Charge && !pathfindWhileCharging)
            TryStunOnEnvironment(col);
    }

    private void TryStunOnEnvironment(Collision2D col)
    {
        // Must be charging in physics mode and the other collider on the environment layer
        if (_environmentLayer != -1 && col.collider.gameObject.layer == _environmentLayer)
        {
            // Immediate stop
            _rb.linearVelocity = Vector2.zero;

            // optional tiny back-off along the average contact normal to prevent scraping
            if (col.contactCount > 0)
            {
                Vector2 n = Vector2.zero;
                for (int i = 0; i < col.contactCount; i++) n += col.GetContact(i).normal;
                n.Normalize();
                _rb.position += n * 0.015f; // ~1.5cm nudge away from wall
            }

            // keep A* off during stun (physics mode)
            _ai.canMove = false;
            _ai.canSearch = false;

            _timer = stunTime;
            Transition(State.Stunned);
        }
        else if (((1 << col.collider.gameObject.layer) & obstacleMask) != 0)
        {
            // Optional: end the dash (no stun) on other obstacle layers
            EndCharge();
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1f, 0.3f, 0.3f, 0.25f);
        Gizmos.DrawWireSphere(transform.position, aggroRadius);
    }
}
