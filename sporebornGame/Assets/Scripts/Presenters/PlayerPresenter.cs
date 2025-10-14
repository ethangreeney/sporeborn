using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class PlayerPresenter : MonoBehaviour
{
    public HealthModel health;
    public SpriteRenderer spriteRenderer;
    public float invulnDuration = 0.5f;

    private bool invuln;
    private bool isDead;
    private PlayerShootingPresenter shooting;

    void Awake()
    {
        shooting = GetComponent<PlayerShootingPresenter>();
        health.maxHealth = DifficultyManager.Instance.PlayerMaxHealth;
        health.currHealth = health.maxHealth;
    }

    public void TakeDamage(int amount)
    {
        if (isDead || invuln)
            return;

        health.Damage(amount);
        StartCoroutine(HitFlash());
        if (health.currHealth <= 0)
        {
            Die();
            return;
        }

        StartCoroutine(Invuln());
    }

    IEnumerator Invuln()
    {
        invuln = true;
        yield return new WaitForSeconds(invulnDuration);
        invuln = false;
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
        foreach (var c in GetComponentsInChildren<Collider2D>()) c.enabled = false;
        shooting.enabled = false;
        SceneManager.LoadScene(0);
    }
}