using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UI;

public class ShopModel : MonoBehaviour
{

    [SerializeField]
    private List<GameObject> ShopItemPool;

    private GameObject[] ShopStock;

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

    public GameObject[] SetupNewShop(List<GameObject> ShopItemPool)
    {
        GameObject[] shopItems = new GameObject[4];

        for (int i = 0; i < shopItems.Length; i++)
        {
            GameObject RandomShopItem = ShopItemPool[rng.Next(0, ShopItemPool.Count - 1)];
            shopItems[i] = RandomShopItem;
            ShopItemPool.Remove(RandomShopItem);
        }

        return shopItems;
    }

    
    public void TryPurchaseItem(GameObject PurchaseItem)
    {
        for(int i=0; i<ShopStock.Length; i++)
        {
            CurrencyModel playerWallet = map.Player.GetComponent<CurrencyModel>();

            // Checks if correct item & player has enough money to purchase item
            if (ShopStock[i] == PurchaseItem && playerWallet.GetCurrentNectar() >= tempItemCost)
            {
                // Set to null - so can't be purchased again
                ShopStock[i] = null;
                // Deduct currency (Nectar) from player
                playerWallet.RemoveCurrency(tempItemCost);
            }
        }

    }
    

}
