using System;
using System.Collections;
using UnityEngine;
using Pathfinding;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
[RequireComponent(typeof(AIPath))]
public class ChargerController : MonoBehaviour
{
    public enum State { Idle, Chase, Windup, Charge, Recover }

    [Header("Aggro")]
    [Tooltip("If the player enters this radius, start the charge sequence.")]
    public float aggroRadius = 6f;

    [Header("Movement (A* handles chase)")]
    [Tooltip("Chase speed used by AIPath (A*).")]
    public float baseSpeed = 3.5f;
    [Tooltip("Dash speed during the straight-line charge (Rigidbody-based).")]
    public float chargeSpeed = 12f;
    [Tooltip("Hard cap distance so we never leave the arena even without walls.")]
    public float maxChargeDistance = 8f;

    [Header("Timing")]
    [Min(0.3f)] public float windupTime = 0.35f; // ≥ 300 ms
    public float maxChargeDuration = 0.6f;       // failsafe timer
    public float recoverTime = 0.6f;             // cooldown before resuming chase

    [Header("Collision / Blocking")]
    [Tooltip("Layers that block the charge (Walls/Obstacles).")]
    public LayerMask obstacleMask;
    [Tooltip("Stop slightly before the hit point to avoid clipping.")]
    public float stopBuffer = 0.1f;

    [Header("Temp Visual Tell (replace with Animator later)")]
    public SpriteRenderer spriteRenderer;
    public Color windupTint = new Color(1f, 0.85f, 0.35f);
    public Color chargeTint = Color.red;

    public event Action OnWindupStart;
    public event Action OnWindupEnd;
    public event Action OnChargeStart;
    public event Action OnChargeEnd;

    // internals
    private Rigidbody2D _rb;
    private AIPath _ai;
    private AIDestinationSetter _dest;
    private Transform _player;
    private State _state = State.Idle;
    private Vector2 _chargeDir;
    private Vector2 _chargeTarget;
    private float _timer;
    private Color _originalTint;
    private float _zLock; // keep Z fixed for top-down aesthetic

    void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _rb.gravityScale = 0f;
        _rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        _rb.interpolation = RigidbodyInterpolation2D.Interpolate;
        _rb.constraints = RigidbodyConstraints2D.FreezeRotation; // don't rotate in 2D

        _ai = GetComponent<AIPath>();
        _ai.maxSpeed = baseSpeed;  // A* owns chase speed
        _ai.canMove = true;
        _ai.isStopped = false;
        _ai.updateRotation = false; // IMPORTANT: do not rotate enemy

        _dest = GetComponent<AIDestinationSetter>() ?? gameObject.AddComponent<AIDestinationSetter>();
        if (_dest.target == null)
        {
            var p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) _dest.target = p.transform;
        }
        _player = _dest.target;

        if (spriteRenderer == null)
            spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        if (spriteRenderer != null)
            _originalTint = spriteRenderer.color;

        _zLock = transform.position.z; // lock Z at startup
    }

    void Update()
    {
        switch (_state)
        {
            case State.Idle:
            case State.Chase:
                ChaseLogic();
                break;

            case State.Windup:
                _timer -= Time.deltaTime;
                if (_timer <= 0f) BeginCharge();
                break;

            case State.Charge:
                _timer -= Time.deltaTime;
                if (Vector2.Distance(transform.position, _chargeTarget) <= 0.05f || _timer <= 0f)
                    EndCharge();
                break;

            case State.Recover:
                _timer -= Time.deltaTime;
                if (_timer <= 0f) ResumeChase();
                break;
        }
    }

    void LateUpdate()
    {
        // hard-lock Z so enemies never drift in Z
        var p = transform.position;
        if (Mathf.Abs(p.z - _zLock) > 0.0001f)
            transform.position = new Vector3(p.x, p.y, _zLock);
    }

    void ChaseLogic()
    {
        if (_player == null) { SetState(State.Idle); return; }

        // ensure A* is active during chase
        if (_state != State.Charge && _state != State.Windup)
        {
            _ai.isStopped = false;
            _ai.canMove = true;
        }

        float dist = Vector2.Distance(transform.position, _player.position);
        if (dist <= aggroRadius && _state != State.Windup)
            StartCoroutine(WindupRoutine());
        else
            SetState(State.Chase);
    }

    IEnumerator WindupRoutine()
    {
        if (_state == State.Windup || _state == State.Charge || _state == State.Recover)
            yield break;

        SetState(State.Windup);

        // full stop: no A* motion and no physics drift
        _ai.isStopped = true;
        _ai.canMove = false;
        _rb.linearVelocity = Vector2.zero;

        // temp visual tell
        if (spriteRenderer) spriteRenderer.color = windupTint;
        OnWindupStart?.Invoke();

        _timer = Mathf.Max(0.3f, windupTime);
        float t = _timer;
        while (t > 0f && _state == State.Windup)
        {
            t -= Time.deltaTime;
            yield return null;
        }

        OnWindupEnd?.Invoke();
        _timer = 0f; // Update triggers BeginCharge next frame
    }

    void BeginCharge()
    {
        if (_player == null) { SetRecover(); return; }

        Vector2 origin = transform.position;
        Vector2 dir = (Vector2)_player.position - origin;
        if (dir.sqrMagnitude < 0.001f) dir = Vector2.right;
        dir.Normalize();
        _chargeDir = dir;

        // compute stop target: wall (raycast) or max distance
        RaycastHit2D hit = Physics2D.Raycast(origin, dir, maxChargeDistance, obstacleMask);
        _chargeTarget = hit.collider ? hit.point - dir * stopBuffer
                                     : origin + dir * maxChargeDistance;

        // disable A* control during dash
        _ai.isStopped = true;
        _ai.canMove = false;

        // straight-line physics dash
        _rb.linearVelocity = _chargeDir * chargeSpeed;

        if (spriteRenderer) spriteRenderer.color = chargeTint;
        OnChargeStart?.Invoke();

        _timer = maxChargeDuration;
        SetState(State.Charge);
    }

    void EndCharge()
    {
        _rb.linearVelocity = Vector2.zero;

        if (spriteRenderer) spriteRenderer.color = _originalTint;
        OnChargeEnd?.Invoke();

        SetRecover();
    }

    void SetRecover()
    {
        _timer = recoverTime;
        SetState(State.Recover);
    }

    void ResumeChase()
    {
        // hand control back to A*
        _ai.isStopped = false;
        _ai.canMove = true;
        _ai.SearchPath(); // refresh
        SetState(State.Chase);
    }

    void SetState(State s) => _state = s;

    void OnCollisionEnter2D(Collision2D col)
    {
        if (_state != State.Charge) return;

        // stop immediately if we hit a blocking layer
        if (((1 << col.collider.gameObject.layer) & obstacleMask) != 0)
            EndCharge();
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1f, 0.3f, 0.3f, 0.25f);
        Gizmos.DrawWireSphere(transform.position, aggroRadius);

        if (Application.isPlaying && _state == State.Charge)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(transform.position, _chargeTarget);
            Gizmos.DrawSphere(_chargeTarget, 0.07f);
        }
    }
}
