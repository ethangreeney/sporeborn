using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class EnemyModel : MonoBehaviour
{
    public HealthModel health;

    // Used ONLY for movement animation params in this class
    public Animator animator;
    public SpriteRenderer spriteRenderer;

    private EnemyPresenter enemyPresenter;
    private bool isDead;

    // -------- Movement / Animation glue --------
    private Pathfinding.AIPath aiPath;   // preferred velocity source (A* Pathfinding)
    private Rigidbody2D rb;               // fallback if no AIPath
    private Vector2 lastFacing = Vector2.right;

    [SerializeField, Tooltip("Speed threshold to consider 'moving' (world units/s).")]
    private float moveThreshold = 0.01f;  // tiny; we'll compare squared

    // Optional: avoid 1-frame idle flash
    private bool prewarmedAnimator = false;

    // --- SLOW EFFECT FIELDS ---
    private bool isSlowed = false;
    private float slowTimer = 0f;
    private float originalSpeed = -1f;

    // --- VISUAL HIT FLASH (unchanged) ---
    private Color cachedOriginalColor;
    private Coroutine hitFlashCoroutine = null;

    void Awake()
    {
        enemyPresenter = FindFirstObjectByType<EnemyPresenter>();
        if (health == null) health = GetComponent<HealthModel>();

        // Auto-find animator/sprite on the child (Enemy GFX)
        if (animator == null)       animator = GetComponentInChildren<Animator>(true);
        if (spriteRenderer == null) spriteRenderer = GetComponentInChildren<SpriteRenderer>(true);

        aiPath = GetComponent<Pathfinding.AIPath>();
        rb     = GetComponent<Rigidbody2D>();

        if (health != null && health.currHealth <= 0)
            health.currHealth = health.maxHealth > 0 ? health.maxHealth : 1;

        if (spriteRenderer != null)
            cachedOriginalColor = spriteRenderer.color;
    }

    void Update()
    {
        // Handle slow timer (unchanged)
        if (isSlowed)
        {
            slowTimer -= Time.deltaTime;
            if (slowTimer <= 0f) RemoveSlow();
        }
    }

    // --- Movement animation wiring (only place we touch Animator in this class) ---
void LateUpdate()
{
    if (animator == null) return;

    // 1) Read desired movement (A* desiredVelocity is non-zero while pathing)
    Vector2 v = Vector2.zero;
    if (aiPath != null)      v = (Vector2)aiPath.desiredVelocity;
    else if (rb != null)     v = rb.linearVelocity;

    // 2) Decide moving/idle (tiny threshold; compare squared to skip sqrt)
    float threshSq    = moveThreshold * moveThreshold;   // e.g., 0.01f^2
    bool isMovingNow  = v.sqrMagnitude > threshSq;
    animator.SetBool("IsMoving", isMovingNow);           // must match Animator parameter name

    // 3) Direction for blend trees — do NOT change facing while in Attack
    bool inAttack = animator.GetCurrentAnimatorStateInfo(0).IsName("Attack"); // state name must match yours
    if (isMovingNow && !inAttack)
        lastFacing = v.normalized;

    animator.SetFloat("MoveX", lastFacing.x);
    animator.SetFloat("MoveY", lastFacing.y);

    // 4) Mirror side sprites for left/right
    if (spriteRenderer != null)
    {
        if      (lastFacing.x >  0.05f) spriteRenderer.flipX = false; // face right
        else if (lastFacing.x < -0.05f) spriteRenderer.flipX = true;  // face left
        // near-zero: keep current flip to avoid popping while idle
    }

    // 5) Optional: skip the 1-frame idle blink when starting to move
    if (!prewarmedAnimator && isMovingNow)
    {
        animator.Play("Move", 0, 0f);   // state name must be exactly "Move"
        prewarmedAnimator = true;
    }
}


    // ----------------- existing damage / death flow (unchanged) -----------------

    public void TakeDamage(float amount)
    {
        if (isDead || health == null) return;

        health.Damage(amount);
        if (spriteRenderer != null)
        {
            if (hitFlashCoroutine == null)
                hitFlashCoroutine = StartCoroutine(HitFlash());
        }

        if (health.currHealth <= 0)
        {
            Die();
        }
    }

    IEnumerator HitFlash()
    {
        spriteRenderer.color = Color.black;
        yield return new WaitForSeconds(0.1f);
        spriteRenderer.color = cachedOriginalColor;
        hitFlashCoroutine = null;
    }

    void Die()
    {
        if (isDead) return;
        isDead = true;

        // stop interaction/motion
        foreach (var c in GetComponentsInChildren<Collider2D>()) c.enabled = false;
        var rb2 = GetComponent<Rigidbody2D>(); if (rb2) rb2.simulated = false;
        if (aiPath) aiPath.canMove = false;

        // >>> Play the death animation
        if (animator != null) animator.SetBool("Dead", true);

        // notify presenter (score, counters, etc.)
        if (enemyPresenter != null) enemyPresenter.EnemyDies(transform.position);

        // Fallback safety (only if you forget to add the animation event):
        Destroy(gameObject, 1.0f);
    }

    public void OnDeathAnimationComplete()
    {
        Destroy(gameObject);
    }

    public void ApplySlow(float slowMultiplier, float duration)
    {
        if (isSlowed || aiPath == null) return; // Do not stack

        isSlowed = true;
        slowTimer = duration;
        originalSpeed = aiPath.maxSpeed;
        aiPath.maxSpeed *= slowMultiplier; // e.g., 0.5f for 50% slow
    }

    private void RemoveSlow()
    {
        if (aiPath != null && originalSpeed > 0f)
        {
            aiPath.maxSpeed = originalSpeed;
        }
        isSlowed = false;
        slowTimer = 0f;
        originalSpeed = -1f;
    }
}