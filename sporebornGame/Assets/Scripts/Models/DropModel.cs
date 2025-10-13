using UnityEngine;

public class DropModel : MonoBehaviour
{
    // Public variables assigned in Inspector - Set on each prefab
    public ItemType CurrentItemType; // E.g. Heart, Nectar
    // How much the item affects the player 
    public int ItemModifierValue; // - E.g. Currency = 1 -> + $1, Heart = 2 -> 2+ Health

    // For Player reference
    private PlayerPresenter PlayerInstance;
    private EnemyPresenter Enemy;

    public enum ItemType
    {
        Heart,
        Nectar
    }

    // Called when dropped item is instantiated 
    void Awake() {
        Enemy = FindFirstObjectByType<EnemyPresenter>();
        PlayerInstance = FindFirstObjectByType<PlayerPresenter>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Only Player can collide with Dropped Items
        if (collision.gameObject != PlayerInstance.gameObject)
        {
            return;
        }

        // Gets current item object
        GameObject CollidedItem = this.gameObject;

        // Gets item information - What type of item
        DropModel CurrentItem = CollidedItem.GetComponent<DropModel>();

        // Applies appropriate effect to player based on item
        switch (CurrentItem.CurrentItemType)
        {
            
            case ItemType.Heart:
                // Stops health from increasing more than Max Health
                if (PlayerInstance.health.currHealth + ItemModifierValue <= PlayerInstance.health.maxHealth)
                {
                    PlayerInstance.health.Heal(ItemModifierValue);
                }
                break;

            case ItemType.Nectar:
                CurrencyModel playerwallet = PlayerInstance.GetComponent<CurrencyModel>();
                playerwallet.AddCurrency(ItemModifierValue);
                break;

         }
        
        // Tells the presenter to destroy this item
        Enemy.DestroyItem(CollidedItem);
   

    }

}