using UnityEngine;

public class DropModel : MonoBehaviour
{
    // Public variables assigned in Inspector - Set on each prefab
    public ItemType CurrentItemType; // E.g. Heart, Nectar
    // How much the item affects the player 
    public int ItemModifierValue; // - E.g. Currency = 1 -> + $1, Heart = 2 -> 2+ Health
                                
    // For Player reference
    private MapPresenter map;
    private EnemyPresenter Enemy;

    public enum ItemType
    {
        Heart,
        Nectar
    }

    // Called when dropped item is instantiated 
    void Awake() {
        Enemy = FindFirstObjectByType<EnemyPresenter>();
        map = FindFirstObjectByType<MapPresenter>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Only Player can collide with Dropped Items
        if (collision.gameObject != map.Player)
        {   
            return;
        }

        // Gets current item object
        GameObject CollidedItem = this.gameObject;

        // Gets item information - What type of item
        DropModel CurrentItem = CollidedItem.GetComponent<DropModel>();

        // Gets the Current Instance of the Player in game
        PlayerPresenter PlayerActiveInstance = map.Player.GetComponent<PlayerPresenter>();

        // Applies appropriate effect to player based on item
        switch (CurrentItem.CurrentItemType)
        {
            case ItemType.Heart:
                // Stops health from increasing more than Max Health
                if (PlayerActiveInstance.health.currHealth + ItemModifierValue <= PlayerActiveInstance.health.maxHealth)
                {
                    PlayerActiveInstance.health.Heal(ItemModifierValue);
                }
                break;

            case ItemType.Nectar:
                PlayerActiveInstance.playerMoney.AddCurrency(ItemModifierValue);
                break;

         }
        
        // Tells the presenter to destroy this item
        Enemy.DestroyItem(CollidedItem);
   

    }

}