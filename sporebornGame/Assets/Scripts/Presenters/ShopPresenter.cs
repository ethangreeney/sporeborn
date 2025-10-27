using System.Collections.Generic;

using UnityEngine;

public class ShopPresenter : MonoBehaviour
{
    [SerializeField]
    private GameObject ShopTriggerZone;

    [SerializeField]
    private GameObject ShopUI;

    private ShopModel shopModel;

    private Vector3 ShopZonePosition;

    void Start()
    {
        // Gets the Model from within the ShopUI prefab
        shopModel = ShopUI.GetComponentInChildren<ShopModel>();

        // Already instantiated in scene but set to inactive for faster load time
        ShopTriggerZone.SetActive(false);
        ShopUI.SetActive(false);

        // Default position of the Shop zone - where the player is able to open shop
        ShopZonePosition = new Vector3(0, -2, 0);

        // Randomly choosing items for shop
        if (shopModel == null)
        {
            Debug.Log("Shop Model is null");
        }
        // Create a new shop for each level
        shopModel.SetupNewShop();

    }

    // Activates the UI & trigger zone when player enters the shop
    public void PlayerEntersShop()
    {
        ShopTriggerZone.SetActive(true);

        // If detection Zone not in the right position
        if (ShopTriggerZone.transform.position != ShopZonePosition)
        {
            ShopTriggerZone.transform.position = ShopZonePosition;
        }

    }

    public void OpenShop()
    {
        if (!MenuManager.TryOpenMenu()) return;
        ShopUI.SetActive(true);
    }

    public void CloseShop()
    {
        ShopUI.SetActive(false);
        MenuManager.CloseMenu();
    }

    public void PlayerLeavesShopRoom()
    {
        ShopTriggerZone.SetActive(false);
        ShopUI.SetActive(false);
        MenuManager.CloseMenu();
    }


}
