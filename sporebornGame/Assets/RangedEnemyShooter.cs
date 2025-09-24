using UnityEngine;

public class RangedEnemyShooter : MonoBehaviour
{
    [Header("Target")]
    [SerializeField] string playerTag = "Player";
    Transform player;

    [Header("Shooting Settings")]
    [SerializeField] GameObject projectilePrefab; // assign EnemyProjectile prefab
    [SerializeField] float attackRange = 7f;      // start shooting within this distance
    [SerializeField] float fireCooldown = 1f;     // seconds between shots

    float cooldownTimer;

    void Start()
    {
        var p = GameObject.FindWithTag(playerTag);
        if (p) player = p.transform;
    }

    void Update()
    {
        if (!player) return;

        cooldownTimer -= Time.deltaTime;

        if (Vector2.Distance(transform.position, player.position) <= attackRange
            && cooldownTimer <= 0f)
        {
            ShootAtPlayer();
            cooldownTimer = fireCooldown * (DifficultyManager.Instance ? DifficultyManager.Instance.EnemyFireIntervalMult : 1f);
        }
    }

    void ShootAtPlayer()
    {
        if (!projectilePrefab) { Debug.LogError("Assign projectilePrefab on RangedEnemyShooter."); return; }

        Vector2 dir = (player.position - transform.position).normalized;

        var go = Instantiate(projectilePrefab, transform.position, Quaternion.identity);
        var proj = go.GetComponent<EnemyProjectileScript>();
        if (!proj) { Debug.LogError("Projectile prefab missing EnemyProjectile component."); return; }

        proj.Initialize(dir); // give it its travel direction
    }

    // Visualize attack range while selected
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}
