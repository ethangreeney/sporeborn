using System.Collections.Generic;
using UnityEngine;

public class ShopModel : MonoBehaviour
{

    [SerializeField]
    private List<ShopItem> ShopItemPool;


    [SerializeField]
    private GameObject ShopItem;
    private List<GameObject> ShopInventory;

    [SerializeField]
    private int NumberOfShopItems = 3;

    // Generate random numbers
    System.Random rng;
    void Start()
    {
        rng = new System.Random();
    }

    public void SetupNewShop()
    {

        for (int i = 0; i < NumberOfShopItems; i++)
        {
            int randItem = rng.Next(0, ShopItemPool.Count);
            ShopItem RandomShopItem = ShopItemPool[randItem];
            
            // Creates a new ShopItem and instantiates under the UI layer that the ShopModel is attached to
            GameObject ShopButton = Instantiate(ShopItem, transform, false);
            ShopItemUI ItemSetup = ShopButton.GetComponent<ShopItemUI>();
            ItemSetup.SetupItem(RandomShopItem, this);

            // Add to list of ShopItems
            ShopInventory.Add(ShopButton);
            ShopItemPool.RemoveAt(randItem);
        }
        
    }

    
    public void TryPurchaseItem(ShopItem PurchaseItem, PlayerPresenter player, ShopItemUI ItemUI)
    {
        foreach(GameObject Item in ShopInventory)
        {
            ShopItem InventoryItem = Item.GetComponent<ShopItem>();
            CurrencyModel playerWallet = player.GetComponent<CurrencyModel>();

            // Checks if correct item & player has enough money to purchase item
            if (InventoryItem == PurchaseItem && playerWallet.GetCurrentNectar() >= PurchaseItem.Cost)
            {
                // Only can buy if item hasn't been bought
                if (Item != null)
                {
                    // Deduct currency (Nectar) from player
                    playerWallet.RemoveCurrency(PurchaseItem.Cost);
                    
                     // Set to null - so can't be purchased again
                    // Set item to be visually inactive in the ShopUI
                    ItemUI.gameObject.SetActive(false);
                }
            }
        }

    }
    

}
