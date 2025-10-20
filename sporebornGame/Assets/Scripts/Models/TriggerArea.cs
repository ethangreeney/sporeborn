using UnityEngine;

public class TriggerArea : MonoBehaviour
{
    
    
    private bool PlayerInZone; // Player in range of shop
    private bool ShopOpen;     // Shop UI is open
    private ShopPresenter shop;

    void Start()
    {
        shop = FindFirstObjectByType<ShopPresenter>();
        PlayerInZone = false;
        ShopOpen = false;
    }

    // Detects if the player is in range of the shop 
    private void OnTriggerEnter2D(Collider2D collision)
    {
        PlayerInZone = true;
    }
    private void OnTriggerExit2D(Collider2D collision)
    {
        PlayerInZone = false;
    }
    
    // Opens and Closes the ShopUI
    void Update()
    {
        // If Player presses 'E' while in shop trigger zone open UI
        if (PlayerInZone && Input.GetKeyDown(KeyCode.E))
        {
            // Toggles Shop being open
            ShopOpen = !ShopOpen;

            if (ShopOpen)
            {
                shop.OpenShop();
            }
            else
            {
                shop.CloseShop();
            }
            
        }

    }
}
