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

    private ItemPresenter itemPresenter;

    private void Start()
    {
        GetComponent<SpriteRenderer>().sprite = item.itemIcon;
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
        var playerHealth = collision.GetComponent<HealthModel>();

        if(playerHealth.currHealth == playerHealth.maxHealth) return;
        
        if (playerHealth != null && healthChange != 0)
        {
            playerHealth.Health(healthChange);
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

        }

        // If item was actually applied
        if (consumed)
        {
            Debug.Log($"Collected item: {item.itemName}");
            itemPresenter.NotifyItemCollected();

            Destroy(gameObject);
        }
        else
        {
            Debug.LogWarning("Item could not be consumed by player.");
        }
    }
}
