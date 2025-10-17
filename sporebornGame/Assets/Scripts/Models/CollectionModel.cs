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

    [Header("Pet Companion Settings")]
    public GameObject petPrefab;

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

        bool consumed = false;

        // Health
        var heartData = gameObject.GetComponent<HeartData>();
        if (heartData != null)
        {
            Debug.Log("Heart collected");
            var playerHealthHeart = collision.GetComponent<HealthModel>();
            if (playerHealthHeart != null && healthChange != 0)
            {
                // Only apply health if not at max health
                if (playerHealthHeart.currHealth < playerHealthHeart.maxHealth)
                {
                    playerHealthHeart.Heal(healthChange);
                }
                // If at full health, do nothing (heart stays in room and respawn list)
            }
            return; // Don't apply other item logic for hearts
        }

        // Max Health increase (non-heart items)
        var playerHealth = collision.GetComponent<HealthModel>();
        if (playerHealth != null && maxHealthFlatIncrease != 0f && heartData == null)
        {
            // Increase max health by fixed amount
            playerHealth.maxHealth += maxHealthFlatIncrease;

            // also raise current health by same absolute amount
            playerHealth.Heal(maxHealthFlatIncrease);

            consumed = true;
        }

        // Movement speed
        PlayerMovement playerMovement = collision.GetComponent<PlayerMovement>();
        if (playerMovement != null && moveSpeedChange != 0)
        {
            playerMovement.moveSpeed += moveSpeedChange;
            consumed = true;

            // Optional clamp
            if (playerMovement.moveSpeed < 0.1f)
                playerMovement.moveSpeed = 0.1f;
        }

        PlayerShootingPresenter shooting = collision.GetComponent<PlayerShootingPresenter>();
        if (shooting != null)
        {
            // Fire delay
            if (fireDelayChange != 0)
            {
                shooting.fireRate += fireDelayChange;
                consumed = true;
                // Optional clamp
                if (shooting.fireRate < 0.05f)
                    shooting.fireRate = 0.05f;
            }
            // Bullet speed
            if (bulletSpeedChange != 0)
            {
                shooting.projectileSpeed += bulletSpeedChange;
                consumed = true;
                // Optional clamp
                if (shooting.projectileSpeed < 1f)
                    shooting.projectileSpeed = 1f;
            }

            // Bullet Damage
            if (bulletDamageChange != 0)
            {
                shooting.projectileDamage += bulletDamageChange;
                consumed = true;
                // Optional clamp
                if (shooting.projectileDamage < 1f)
                    shooting.projectileDamage = 1f;
            }

            // Bullet Size
            if (projectileSizeChange != 0)
            {
                shooting.projectileSize += projectileSizeChange;
                consumed = true;
                // Optional clamp
                if (shooting.projectileSize < 0.1f)
                    shooting.projectileSize = 0.1f;
            }

            // Homing
            if (homingEnabled)
            {
                shooting.homingEnabled = true;
                shooting.homingStrength = homingStrength;
                consumed = true;
            }

            // Extra Projectiles
            if (extraProjectiles != 0)
            {
                shooting.projectileCount += extraProjectiles;
                if (shooting.projectileCount < 1)
                    shooting.projectileCount = 1; // safety
                consumed = true;
            }

            // Extra Spread Angle (per bullet)
            if (extraSpreadAngle != 0)
            {
                shooting.spreadAngle += extraSpreadAngle;
                consumed = true;
            }

            // Damage Bonus (e.g., 0.5 = +50%)
            if (damageBonus != 0)
            {
                shooting.damageBonus += damageBonus;
                consumed = true;
            }

            // Rubber Bullets
            if (rubberEnabled)
            {
                shooting.rubberEnabled = true;
                shooting.bounceCount = rubberBounces;
                consumed = true;
            }
            // Slow effect
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

            // Pet Companion logic
            if (petPrefab != null)
            {
                // Just spawn the pet at the player's position
                Instantiate(petPrefab, collision.transform.position, Quaternion.identity);
                consumed = true;
            }




        }

        // If item was actually applied
        if (consumed)
        {
            var inv = collision.GetComponent<PlayerInventory>();
            if (inv != null)
            {
                inv.AddItem(item);
            }
            if (room != null)
            {
                itemPresenter.NotifyItemCollected(room);
            }
            else
            {
                Debug.LogWarning("Room reference not set for item collection.");
            }
            Destroy(gameObject);
        }
        else
        {
            Debug.LogWarning("Item could not be consumed by player.");
        }

    }
}
