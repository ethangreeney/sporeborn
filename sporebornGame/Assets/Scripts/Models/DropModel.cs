using UnityEngine;

public class DropModel : MonoBehaviour
{
    // Assign these variables in Inspector - Set on each prefab
    public float DropChance;  // Chance of item dropping
    public ItemType CurrentItemType; // E.g. Heart, Nectar

    public int HeartHealthAmount = 10;
    public int NectarValue = 1;

    // For Player reference
    private MapPresenter map;
    private EnemyDropPresenter DropPresenter;

    public enum ItemType
    {
        Heart,
        Nectar
    }

    // Called when dropped item is instantiated 
    void Awake() {
        DropPresenter = FindFirstObjectByType<EnemyDropPresenter>();
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

        // Gets item information
        DropModel CurrentItem = CollidedItem.GetComponent<DropModel>();

        // Gets the CurrentInstance of the Player
        PlayerPresenter PlayerActiveInstance = map.Player.GetComponent<PlayerPresenter>();

        // Applies appropriate effect to player based on item
        switch (CurrentItem.CurrentItemType)
        {
            case ItemType.Heart:
                // Stops health from increasing more than Max Health
                if (PlayerActiveInstance.health.currHealth + HeartHealthAmount <= PlayerActiveInstance.health.maxHealth)
                {
                    PlayerActiveInstance.health.Heal(HeartHealthAmount);
                }
                break;

            case ItemType.Nectar:
                PlayerActiveInstance.playerMoney.AddCurrency(NectarValue);
                break;

         }
        
        // Tells the presenter to destroy this item
        DropPresenter.DestroyItem(CollidedItem);
   

    }

}