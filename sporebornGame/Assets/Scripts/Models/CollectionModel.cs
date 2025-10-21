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
    public Item item;

    [Header("Player Stat Changes")]
    public float healthChange;
    public float maxHealthFlatIncrease = 2f;
    public float moveSpeedChange;
    public float fireDelayChange;
    public float bulletSpeedChange;
    public float bulletDamageChange;
    public float projectileSizeChange;
    public float damageBonus = 0f;

    [Header("Projectile Type Settings")]
    public bool homingEnabled;
    public float homingStrength;
    public int extraProjectiles;
    public float extraSpreadAngle;
    public bool rubberEnabled;
    public int rubberBounces;

    [Header("Slow Effect Settings")]
    public bool slowOnHitEnabled;
    public float slowMultiplier = 0.5f;
    public float slowDuration = 1f;

    [Header("Projectile Visuals")]
    public Color projectileColor = Color.clear;

    [Header("Pet Companion Settings")]
    public GameObject petPrefab;

    private ItemPresenter itemPresenter;

    void Start()
    {
        if (GetComponent<PolygonCollider2D>() is var oldCollider && oldCollider)
            Destroy(oldCollider);

        gameObject.AddComponent<PolygonCollider2D>().isTrigger = true;
        itemPresenter = FindFirstObjectByType<ItemPresenter>();
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.CompareTag("Player")) return;

        if (GetComponent<HeartData>())
        {
            var playerHealth = collision.GetComponent<HealthModel>();
            if (playerHealth && healthChange != 0 && playerHealth.currHealth < playerHealth.maxHealth)
                playerHealth.Heal(healthChange);
            return;
        }

        var movement = collision.GetComponent<PlayerMovement>();
        var shooting = collision.GetComponent<PlayerShootingPresenter>();
        var stats = collision.GetComponent<PlayerStats>();
        var health = collision.GetComponent<HealthModel>();

        bool consumed = false;

        if (health && maxHealthFlatIncrease != 0)
        {
            health.maxHealth += maxHealthFlatIncrease;
            health.Heal(maxHealthFlatIncrease);
            consumed = true;
        }

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
                shooting.projectileColor = projectileColor;
            }

            // Pet Companion logic
            if (petPrefab != null)
            {
                // Just spawn the pet at the player's position
                Instantiate(petPrefab, collision.transform.position, Quaternion.identity);
                consumed = true;
            }




        }

        stats?.NotifyStatsChanged();

        if (consumed)
        {
            collision.GetComponent<PlayerInventory>()?.AddItem(item);
            if (room != null) itemPresenter?.NotifyItemCollected(room);
            Destroy(gameObject);
        }
        else
        {
            Debug.LogWarning("Item could not be consumed by player.");
        }

    }
}