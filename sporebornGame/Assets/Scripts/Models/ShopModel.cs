using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ShopModel : MonoBehaviour
{

    [SerializeField]
    private List<ShopItem> ShopItemPool;

    private ShopItem[] ShopStock;

    private MapPresenter map;


    // Temp for testing
    int tempItemCost = 10;

    // Generate random numbers
    System.Random rng;
    void Start()
    {
        rng = new System.Random();
        map = FindFirstObjectByType<MapPresenter>();
    }

    public ShopItem[] SetupNewShop()
    {
        ShopStock = new ShopItem[4];

        for (int i = 0; i < ShopStock.Length; i++)
        {
            int randItem = rng.Next(0, ShopItemPool.Count);
            ShopStock[i] = ShopItemPool[randItem];
            ShopItemPool.RemoveAt(randItem);
        }

        return ShopStock;
    }

    
    public void TryPurchaseItem(ShopItem PurchaseItem)
    {
        for(int i=0; i<ShopStock.Length; i++)
        {
            CurrencyModel playerWallet = map.Player.GetComponent<CurrencyModel>();

            // Checks if correct item & player has enough money to purchase item
            if (ShopStock[i] == PurchaseItem && playerWallet.GetCurrentNectar() >= tempItemCost)
            {
                // Only can buy if item hasn't been bought
                if (ShopStock[i] != null)
                {
                    // Set to null - so can't be purchased again
                    ShopStock[i] = null;
                    // Deduct currency (Nectar) from player
                    playerWallet.RemoveCurrency(tempItemCost);

                    // Set item to be visually inactive in the ShopUI
                }
            }
        }

    }
    

}
