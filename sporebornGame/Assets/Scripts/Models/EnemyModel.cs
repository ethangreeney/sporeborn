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

    void Awake()
    {
        enemyPresenter = FindFirstObjectByType<EnemyPresenter>();
        
        if (health == null) health = GetComponent<HealthModel>();
        if (spriteRenderer == null) spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        if (health != null && health.currHealth <= 0)
        {
            health.currHealth = health.maxHealth > 0 ? health.maxHealth : 1;
        }
    }

    public void TakeDamage(float amount)
    {
        if (isDead || health == null) return;

        health.Damage(amount);
        if (spriteRenderer != null) StartCoroutine(HitFlash());

        if (health.currHealth <= 0)
        {
            Die();
        }
    }

    IEnumerator HitFlash()
    {
        // conflicts with boss charging animation (to be reconsidered)
        var original = spriteRenderer.color;
        spriteRenderer.color = Color.black;
        yield return new WaitForSeconds(0.1f);
        spriteRenderer.color = original;
    }

    void Die()
    {
        isDead = true;

        foreach (var c in GetComponentsInChildren<Collider2D>()) c.enabled = false;
        var rb = GetComponent<Rigidbody2D>();
        if (rb) rb.simulated = false;

        if (animator != null)
        {
            animator.SetTrigger("Death");
        }
        // fall back as there is currently no death animatior
        else
        {
            Destroy(gameObject, 0.2f);
        }
        // Decreases enemy count
        enemyPresenter.EnemyDies();

    }
    // currently unreachable code
    public void OnDeathAnimationComplete()
    {
        Destroy(gameObject);
    }
}