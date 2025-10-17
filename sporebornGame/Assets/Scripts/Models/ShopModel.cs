using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ShopModel : MonoBehaviour
{

    [SerializeField]
    private List<ShopItem> ShopItemPool;

    [SerializeField]
    private GameObject ShopItemPrefab;
    
    // All Items currently in the Shop
    private List<ShopItem> ShopInventory;

    [SerializeField]
    private int NumberOfShopItems = 3;

    // Generate random numbers
    System.Random rng;
    void Awake()
    {
        rng = new System.Random();
        // A new Shop Inventory per level
        ShopInventory = new List<ShopItem>();
    }

    public void SetupNewShop()
    {
        // Temp list so item can be used again for future levels if not purchased
        List<ShopItem> TempPool = ShopItemPool;

        for (int i = 0; i < NumberOfShopItems; i++)
        {
            int randItem = rng.Next(0, TempPool.Count);
            ShopItem RandomShopItem = TempPool[randItem];

            // Creates a new ShopItem under the UI layer that the ShopModel is on
            GameObject ShopButton = Instantiate(ShopItemPrefab, transform, false);

            // Setups up UI based on ShopItem data
            ShopItemUI ItemSetup = ShopButton.GetComponent<ShopItemUI>();
            ItemSetup.SetupItem(RandomShopItem, this);

            // Used to visually updated item
            RandomShopItem.UIReference = ItemSetup;

            // Add the ShopItem Data type to the Shop's inventory
            ShopInventory.Add(RandomShopItem);

            // Remove Item from this levels item pool
            TempPool.Remove(RandomShopItem);

        }

    }

    public void TryPurchaseItem(ShopItem PurchaseItem, PlayerPresenter player, ShopItemUI ItemUI)
    {
        
        foreach (ShopItem CurrentItem in ShopInventory)
        {
            // Return if Item has already been purchased
            if(CurrentItem == PurchaseItem && CurrentItem.Purchased)
            {
                return;
            }
            
            CurrencyModel playerWallet = player.GetComponent<CurrencyModel>();
            
            // Checks if correct item & player has enough money to purchase item
            if (CurrentItem != PurchaseItem || playerWallet.GetCurrentNectar() < PurchaseItem.Cost)
            {
                continue;
            }
            // Only can buy if item hasn't been bought
            if (!CurrentItem.Purchased)
            {
                // Deduct currency (Nectar) from player
                playerWallet.RemoveCurrency(PurchaseItem.Cost);
                
                // Mark Item as purchased
                CurrentItem.Purchased = true;

                // Get Player Active Item Slot & and set it to the purchased item
                PlayerActivatableItem PlayerItem = player.GetComponent<PlayerActivatableItem>();
                PlayerItem.equippedItem = PurchaseItem.ItemType;

                // Set item to be visually inactive in the ShopUI
                SetItemPurchasedUI(CurrentItem.UIReference);

                // Remove item from pool so it can't be chosen in future
                ShopItemPool.Remove(CurrentItem);
            }

        }

    }
    
    // Updates visuals to show player that item has been purchased
    public void SetItemPurchasedUI(ShopItemUI itemreference)
    {
        itemreference.ItemHasBeenPurchased();
    }
    

}
