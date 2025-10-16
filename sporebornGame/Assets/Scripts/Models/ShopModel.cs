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

    private ActivatableItemUI ActiveItem;
    private ActivatableItem ActiveItemModel;

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
        for (int i = 0; i < NumberOfShopItems; i++)
        {
            int randItem = rng.Next(0, ShopItemPool.Count);
            ShopItem RandomShopItem = ShopItemPool[randItem];

            // Creates a new ShopItem under the UI layer that the ShopModel is on
            GameObject ShopButton = Instantiate(ShopItemPrefab, transform, false);
            
            ShopItemUI ItemSetup = ShopButton.GetComponent<ShopItemUI>();
            ItemSetup.SetupItem(RandomShopItem, this);

            // Add the ShopItem Data type to the Shop's inventory
            ShopInventory.Add(RandomShopItem);

        }

    }
    
    public void TryPurchaseItem(ShopItem PurchaseItem, PlayerPresenter player, ShopItemUI ItemUI)
    {
        foreach(ShopItem Item in ShopInventory)
        {
            CurrencyModel playerWallet = player.GetComponent<CurrencyModel>();

            // Checks if correct item & player has enough money to purchase item
            if (Item != PurchaseItem || playerWallet.GetCurrentNectar() < PurchaseItem.Cost)
            {
                continue;
            }
            // Only can buy if item hasn't been bought
            if (Item != null)
            {
                // Deduct currency (Nectar) from player
                playerWallet.RemoveCurrency(PurchaseItem.Cost);

                // Set to null - so can't be purchased again
                

                // Set item to be visually inactive in the ShopUI
                ItemUI.gameObject.SetActive(false);

                // Give item to player
                //ActiveItem.Activate(Item);

                // Remove item from pool so it can't be chosen again
                ShopItemPool.Remove(Item);
            }
            
        }

    }
    

}
