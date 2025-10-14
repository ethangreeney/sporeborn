using UnityEngine;

[System.Serializable]
public class Item
{
    public string itemName;
    public string itemDescription;
    public Sprite itemIcon;

}

public class CollectionModel : MonoBehaviour
{
    [Header("Item Settings")]
    public Item item; //- setting the sprite to the prefab works better - Benjamin
    // Reintroduced this item field for the inventory - Ethan   

    [Header("Player Stat Changes")]
    public float healthChange;
    public float maxHealthFlatIncrease = 2f; // Amount to increase max health by
    public float moveSpeedChange;
    public float fireDelayChange;
    public float bulletSpeedChange;
    public float bulletDamageChange;
    public float projectileSizeChange;
    public float damageBonus = 0f; // e.g., 0.5 = +50% damage



    [Header("Projectile Type Settings")]
    public bool homingEnabled = false;
    public float homingStrength; // how fast bullets turn toward targets
    public int extraProjectiles = 0;      // e.g., Triple Shot = 2 extra
    public float extraSpreadAngle = 0f;   // spread per bullet
    public bool rubberEnabled = false;
    public int rubberBounces; // how many times bullets bounce off walls

    [Header("Slow Effect Settings")]
    public bool slowOnHitEnabled = false; // Does this item enable slow on hit?
    public float slowMultiplier = 0.5f;   // e.g., 0.5 = 50% speed
    public float slowDuration = 1f;       // Duration in seconds

    [Header("Projectile Visuals")]
    public Color projectileColor = Color.clear; // Default is null (no change)

    private ItemPresenter itemPresenter;

    [HideInInspector]
    public Room room; // Set this when spawning the item

    private void Start()
    {
        var oldCollider = GetComponent<PolygonCollider2D>();
        if (oldCollider != null) Destroy(oldCollider);

        var col = gameObject.AddComponent<PolygonCollider2D>();
        col.isTrigger = true;

        // Reference to Presenter
        itemPresenter = FindFirstObjectByType<ItemPresenter>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.CompareTag("Player")) return;

        // Handle hearts separately
        var heartData = GetComponent<HeartData>();
        if (heartData != null)
        {
            var playerHealth = collision.GetComponent<HealthModel>();
            if (playerHealth != null && healthChange != 0 && playerHealth.currHealth < playerHealth.maxHealth)
            {
                playerHealth.Health(healthChange);
                FindFirstObjectByType<EnemyPresenter>()?.OnHeartCollected(gameObject);
                Destroy(gameObject);
            }
            return;
        }

        // Get all components once
        var health = collision.GetComponent<HealthModel>();
        var movement = collision.GetComponent<PlayerMovement>();
        var shooting = collision.GetComponent<PlayerShootingPresenter>();
        var stats = collision.GetComponent<PlayerStats>();
        bool consumed = false;

        // Max health increase
        if (health && maxHealthFlatIncrease != 0f)
        {
            health.maxHealth += maxHealthFlatIncrease;
            health.Health(maxHealthFlatIncrease);
            consumed = true;
        }

        // Movement speed
        if (movement && moveSpeedChange != 0)
        {
            movement.moveSpeed = Mathf.Max(0.1f, movement.moveSpeed + moveSpeedChange);
            if (stats) stats.currentMoveSpeed = movement.moveSpeed;
            consumed = true;
        }

        if (shooting)
        {
            if (fireDelayChange != 0)
            {
                shooting.fireRate = Mathf.Max(0.05f, shooting.fireRate + fireDelayChange);
                if (stats) stats.currentFireRate = shooting.fireRate;
                consumed = true;
            }

            if (bulletSpeedChange != 0)
            {
                shooting.projectileSpeed = Mathf.Max(1f, shooting.projectileSpeed + bulletSpeedChange);
                if (stats) stats.currentProjectileSpeed = shooting.projectileSpeed;
                consumed = true;
            }

            if (bulletDamageChange != 0)
            {
                shooting.projectileDamage = Mathf.Max(1f, shooting.projectileDamage + bulletDamageChange);
                if (stats) stats.currentDamage = shooting.projectileDamage;
                consumed = true;
            }

            if (projectileSizeChange != 0)
            {
                shooting.projectileSize = Mathf.Max(0.1f, shooting.projectileSize + projectileSizeChange);
                consumed = true;
            }

            if (homingEnabled)
            {
                shooting.homingEnabled = true;
                shooting.homingStrength = homingStrength;
                consumed = true;
            }

            if (extraProjectiles != 0)
            {
                shooting.projectileCount = Mathf.Max(1, shooting.projectileCount + extraProjectiles);
                consumed = true;
            }

            if (extraSpreadAngle != 0)
            {
                shooting.spreadAngle += extraSpreadAngle;
                consumed = true;
            }

            if (damageBonus != 0)
            {
                shooting.damageBonus += damageBonus;
                consumed = true;
            }

            if (rubberEnabled)
            {
                shooting.rubberEnabled = true;
                shooting.bounceCount = rubberBounces;
                consumed = true;
            }

            if (slowOnHitEnabled)
            {
                shooting.slowOnHitEnabled = true;
                shooting.slowMultiplier = slowMultiplier;
                shooting.slowDuration = slowDuration;
                consumed = true;
            }

            if (projectileColor != Color.clear)
            {
                shooting.projectileColor = projectileColor;
            }
        }

        // Notify stats changed once
        stats?.NotifyStatsChanged();

        // Cleanup
        if (consumed)
        {
            collision.GetComponent<PlayerInventory>()?.AddItem(item);
            if (room != null) itemPresenter.NotifyItemCollected(room);
            Destroy(gameObject);
        }
    }
}