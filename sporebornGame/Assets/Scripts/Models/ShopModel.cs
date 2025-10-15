using System.Collections.Generic;
using UnityEngine;

public class ShopModel : MonoBehaviour
{

    [SerializeField]
    private List<ShopItem> ShopItemPool;

    private ShopItem[] ShopStock;
    private ShopItemUI Item;

    // Generate random numbers
    System.Random rng;
    void Start()
    {
        rng = new System.Random();
    }

    public void SetupNewShop()
    {
        ShopStock = new ShopItem[4];

        for (int i = 0; i < ShopStock.Length; i++)
        {
            int randItem = rng.Next(0, ShopItemPool.Count);
            ShopStock[i] = ShopItemPool[randItem];
            ShopItemPool.RemoveAt(randItem);

            // Pass through item information and the ShopModel
            Item.SetupItem(ShopStock[i], this);
        }
        
    }

    
    public void TryPurchaseItem(ShopItem PurchaseItem, PlayerPresenter player)
    {
        for(int i=0; i<ShopStock.Length; i++)
        {
            CurrencyModel playerWallet = player.GetComponent<CurrencyModel>();

            // Checks if correct item & player has enough money to purchase item
            if (ShopStock[i] == PurchaseItem && playerWallet.GetCurrentNectar() >= PurchaseItem.Cost)
            {
                // Only can buy if item hasn't been bought
                if (ShopStock[i] != null)
                {
                    // Set to null - so can't be purchased again
                    ShopStock[i] = null;
                    // Deduct currency (Nectar) from player
                    playerWallet.RemoveCurrency(PurchaseItem.Cost);

                    // Set item to be visually inactive in the ShopUI
                }
            }
        }

    }
    

}
