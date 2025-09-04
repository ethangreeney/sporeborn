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
