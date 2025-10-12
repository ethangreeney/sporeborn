using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class PlayerPresenter : MonoBehaviour
{
    public HealthModel health;
    public SpriteRenderer spriteRenderer;
    public SpriteRenderer forcefieldRenderer; 
    public Animator animator;
    public float invulnDuration = 0.5f;
    public float knockbackForce = 8f;

    bool invuln;
    bool isDead;
    bool invulnFromDamage;
    bool invulnFromForcefield;
    Rigidbody2D rb;

    void Awake()
    {
        if (health == null)
            health = GetComponent<HealthModel>();
        if (spriteRenderer == null)
            spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        rb = GetComponent<Rigidbody2D>();

        if (forcefieldRenderer != null)
            forcefieldRenderer.enabled = false; // Hide forcefield at start
        if (health != null && DifficultyManager.Instance)
        {
            health.maxHealth = DifficultyManager.Instance.PlayerMaxHealth;
            health.currHealth = health.maxHealth;
            if (health.currHealth <= 0)
                health.currHealth = health.maxHealth > 0 ? health.maxHealth : 1;
            else if (health.currHealth > health.maxHealth)
                health.currHealth = health.maxHealth;
        }
        if (health != null && health.currHealth <= 0)
            health.currHealth = health.maxHealth > 0 ? health.maxHealth : 1;
    }

    public void SetInvulnerable(bool value)
    {
        invulnFromForcefield = value;
        if (forcefieldRenderer != null)
            forcefieldRenderer.enabled = value;
    }

    public void TakeDamage(int amount, Vector2 hitFrom)
    {
        if (isDead || invulnFromDamage || invulnFromForcefield || health == null)
            return;

        health.Damage(amount);
        if (spriteRenderer)
            StartCoroutine(HitFlash());
        if (health.currHealth <= 0)
        {
            Die();
            return;
        }

        StartCoroutine(Invuln());
        if (rb != null)
        {
            var dir = ((Vector2)transform.position - hitFrom).normalized;
            rb.AddForce(dir * knockbackForce, ForceMode2D.Impulse);
        }
    }

    IEnumerator Invuln()
    {
        invulnFromDamage = true;
        yield return new WaitForSeconds(invulnDuration);
        invulnFromDamage = false;
    }

    IEnumerator HitFlash()
    {
        var original = spriteRenderer.color;
        spriteRenderer.color = Color.red;
        yield return new WaitForSeconds(0.1f);
        spriteRenderer.color = original;
    }

    void Die()
    {
        isDead = true;
        foreach (var c in GetComponentsInChildren<Collider2D>())
            c.enabled = false;
        if (rb)
            rb.simulated = false;

        var move = GetComponent<Player>();
        if (move)
            move.enabled = false;

        var shoot = GetComponent<PlayerShootingPresenter>();
        if (shoot)
            shoot.enabled = false;

        if (animator)
            animator.SetTrigger("Death");
        else
           Time.timeScale = 1f;
           SceneManager.LoadScene(0);
    }
}