using UnityEngine;

public class TriggerArea : MonoBehaviour
{
    
    
    private bool PlayerInZone; // Player in range of shop
    private bool ShopOpen;     // Shop UI is open
    private ShopPresenter shop;

    // Controls how fast the player can open the shop
    public float ShopOpenCooldown = 0.5f;
    private float NextShopOpenTime = 0f;

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
        if (PlayerInZone && Input.GetKey(KeyCode.E) && Time.time >= NextShopOpenTime)
        {
            // Adds delay for key inputs when opening and closing shop
            NextShopOpenTime = Time.time + ShopOpenCooldown;
            
            if (!ShopOpen)
            {
                ShopOpen = true;
                shop.OpenShop();
            }
        
            else if(ShopOpen)
            {
                ShopOpen = false;
                shop.CloseShop();
            }
        }

    }
}
