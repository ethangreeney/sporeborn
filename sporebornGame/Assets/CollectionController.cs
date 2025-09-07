using UnityEngine;

[System.Serializable]
public class Item
{
    public string itemName;
    public string itemDescription;
    public Sprite itemIcon;

}

public class CollectionController : MonoBehaviour
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


    private void Start()
    {
        GetComponent<SpriteRenderer>().sprite = item.itemIcon;
        var oldCollider = GetComponent<PolygonCollider2D>();
        if (oldCollider != null) Destroy(oldCollider);

        var col = gameObject.AddComponent<PolygonCollider2D>();
        col.isTrigger = true; 
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.CompareTag("Player")) return;

        bool consumed = false;

    

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

            if (projectileSizeChange != 0)
            {
                shooting.projectileSize += projectileSizeChange;
                consumed = true;
                // Optional clamp
                if (shooting.projectileSize < 0.1f)
                    shooting.projectileSize = 0.1f;
            }
        }

        // If item was actually applied
        if (consumed)
        {
            Debug.Log($"Collected item: {item.itemName}");


            Destroy(gameObject);
        }
        else
        {
            Debug.LogWarning("Item could not be consumed by player.");
        }
    }
}
