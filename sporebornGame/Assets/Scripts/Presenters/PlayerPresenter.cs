using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class PlayerPresenter : MonoBehaviour
{
    [SerializeField] private HealthModel health;
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private float invulnDuration = 0.3f;
    [SerializeField] private float hitFlashDuration = 0.1f;

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
        yield return new WaitForSeconds(hitFlashDuration);
        spriteRenderer.color = original;
    }

    void Die()
    {
        isDead = true;
        SceneManager.LoadScene("Start Menu");
    }
}