using UnityEngine;
using System.Collections;
using System;

public class PlayerPresenter : MonoBehaviour
{
    public HealthModel health;
    public SpriteRenderer spriteRenderer;
    public SpriteRenderer forcefieldRenderer;
    [SerializeField] private float invulnDuration = 0.3f;
    [SerializeField] private float hitFlashDuration = 0.1f;

    private bool isDead;
    private bool invulnFromDamage;
    private bool invulnFromForcefield;
    private Rigidbody2D rb;

    public static event Action OnPlayerDied;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        if (forcefieldRenderer != null)
            forcefieldRenderer.enabled = false;

        if (DifficultyManager.Instance)
        {
            health.maxHealth = DifficultyManager.Instance.PlayerMaxHealth;
            health.currHealth = health.maxHealth;
        }
    }

    public void SetInvulnerable(bool value)
    {
        invulnFromForcefield = value;
        if (forcefieldRenderer != null)
            forcefieldRenderer.enabled = value;
    }

    public void TakeDamage(int amount)
    {
        if (isDead || invulnFromDamage || invulnFromForcefield)
            return;

        SoundManager.instance.PlayDamageSound();
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
        invulnFromDamage = true;
        yield return new WaitForSeconds(invulnDuration);
        invulnFromDamage = false;
    }

    public void GrantRoomEntryInvuln()
    {
        StartCoroutine(RoomEntryInvuln());
    }

    IEnumerator RoomEntryInvuln()
    {
        invulnFromDamage = true;
        yield return new WaitForSeconds(1f);
        invulnFromDamage = false;
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
        if (isDead) return;
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

        OnPlayerDied?.Invoke();
    }
}