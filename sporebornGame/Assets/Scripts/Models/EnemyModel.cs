using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class EnemyModel : MonoBehaviour
{
    public HealthModel health;
    public Animator animator;
    public SpriteRenderer spriteRenderer;

    private EnemyPresenter enemyPresenter;

    bool isDead;

    // --- SLOW EFFECT FIELDS ---
    private bool isSlowed = false;
    private float slowTimer = 0f;
    private float originalSpeed = -1f;
    private Pathfinding.AIPath aiPath;

    private Color cachedOriginalColor;
    private Coroutine hitFlashCoroutine = null;

    void Awake()
    {
        enemyPresenter = FindFirstObjectByType<EnemyPresenter>();
        if (health == null) health = GetComponent<HealthModel>();
        if (spriteRenderer == null) spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        if (health != null && health.currHealth <= 0)
        {
            health.currHealth = health.maxHealth > 0 ? health.maxHealth : 1;
        }
        aiPath = GetComponent<Pathfinding.AIPath>();

        // Cache the original color at startup
        if (spriteRenderer != null)
            cachedOriginalColor = spriteRenderer.color;
    }

    void Update()
    {
        // Handle slow timer
        if (isSlowed)
        {
            slowTimer -= Time.deltaTime;
            if (slowTimer <= 0f)
            {
                RemoveSlow();
            }
        }
    }

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
        isDead = true;

        foreach (var c in GetComponentsInChildren<Collider2D>()) c.enabled = false;
        var rb = GetComponent<Rigidbody2D>();
        if (rb) rb.simulated = false;

        // fall back as there is currently no death animatior

        Destroy(gameObject, 0.2f);

        // Decreases enemy count
        enemyPresenter.EnemyDies(transform.position);

    }
    // currently unreachable code
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
        aiPath.maxSpeed *= slowMultiplier; // e.g. 0.5f for 50% slow
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