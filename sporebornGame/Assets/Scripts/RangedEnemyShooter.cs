using UnityEngine;

public class RangedEnemyShooter : MonoBehaviour
{
    [Header("Target")]
    [SerializeField] string playerTag = "Player";
    Transform player;

    [Header("Shooting Settings")]
    [SerializeField] GameObject projectilePrefab;   // EnemyProjectile prefab
    [SerializeField] float attackRange = 7f;
    [SerializeField] float fireCooldown = 1f;

    [Header("Fire Points (place under Enemy GFX)")]
    [SerializeField] Transform fireRight;
    [SerializeField] Transform fireLeft;
    [SerializeField] Transform fireUp;
    [SerializeField] Transform fireDown;

    [Header("Animation/FX")]
    [SerializeField] Animator animator;             // Animator that uses AOC_Enemy_Ranged

    float cooldownTimer;
    Vector2 pendingShotDir = Vector2.right;         // cached until the anim event fires

    void Awake()
    {
        if (!animator) animator = GetComponent<Animator>();                  // prefer same GO
        if (!animator) animator = GetComponentInChildren<Animator>(true);    // fallback
    }

    void Start()
    {
        var p = GameObject.FindWithTag(playerTag);
        if (p) player = p.transform;
    }

    void Update()
    {
        if (!player) return;

        cooldownTimer -= Time.deltaTime;

        // In range and off cooldown?
        if (cooldownTimer <= 0f &&
            Vector2.Distance(transform.position, player.position) <= attackRange)
        {
            // Aim at player and feed directional params for the attack blend
            pendingShotDir = (player.position - transform.position).normalized;
            animator.SetFloat("AttackX", pendingShotDir.x);
            animator.SetFloat("AttackY", pendingShotDir.y);

            // Trigger the shoot animation (projectile spawns via Animation Event)
            animator.SetTrigger("Attack");

            // Reset cooldown (respect your DifficultyManager if you have one)
            float mult = 1f;
            if (DifficultyManager.Instance)
                mult = DifficultyManager.Instance.EnemyFireIntervalMult;

            cooldownTimer = fireCooldown * mult;
        }
    }

    // Animation Event on Archer_Attack_* clips calls this
    public void SpawnProjectile()
    {
        if (!projectilePrefab) { Debug.LogError("Assign projectilePrefab."); return; }

        Transform fp = ActiveFirePointFrom(pendingShotDir);
        Vector3 spawnPos = fp ? fp.position : transform.position;

        var go = Instantiate(projectilePrefab, spawnPos, Quaternion.identity);

        // Rotate to face direction (optional)
        float angle = Mathf.Atan2(pendingShotDir.y, pendingShotDir.x) * Mathf.Rad2Deg;
        go.transform.rotation = Quaternion.Euler(0f, 0f, angle);

        var proj = go.GetComponent<EnemyProjectileScript>();
        if (proj) proj.Initialize(pendingShotDir);
    }

    Transform ActiveFirePointFrom(Vector2 dir)
    {
        // Pick axis-dominant facing
        if (Mathf.Abs(dir.x) > Mathf.Abs(dir.y))
            return dir.x >= 0f ? (fireRight ? fireRight : fireLeft)
                               : (fireLeft  ? fireLeft  : fireRight);
        else
            return dir.y >= 0f ? (fireUp    ? fireUp    : fireDown)
                               : (fireDown  ? fireDown  : fireUp);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}
