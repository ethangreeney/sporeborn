using System.Collections.Generic;
using UnityEngine;

public class ShopPresenter : MonoBehaviour
{
    [SerializeField]
    private List<GameObject> ShopItemPool;

    [SerializeField]
    private GameObject ShopTriggerZone;
    [SerializeField]
    private GameObject ShopUI;

    private ShopModel ShopLogic;

    private Vector3 ShopZonePosition;




    void Start()
    {

        Debug.LogWarning("Shop Presenter is Called");

        // Gets the Model from the ShopUI
        ShopLogic = ShopUI.GetComponentInChildren<ShopModel>(false);
        
        // Already instantiated in scene but set to inactive for faster load time
        ShopTriggerZone.SetActive(false);
        ShopUI.SetActive(false);

        // Used to detect whether playre is in range of shop
        ShopZonePosition = new Vector3(0, -2, 0);

    }

    // Only Activates the Shop Trigger Zone when in Shop Room
    public void ActivateShopTrigger()
    {
        ShopTriggerZone.SetActive(true);

        // If detection Zone not in the right position
        if(ShopTriggerZone.transform.position != ShopZonePosition)
        {
            ShopTriggerZone.transform.position = ShopZonePosition;
        }
        
    }
    
    public void OpenShop()
    {
        ShopUI.SetActive(true);
    }

    public void CloseShop()
    {
        ShopUI.SetActive(false);
    }

   

    public void PlayerClicksItem()
    {

    }

    public void PlayerLeavesShopRoom()
    {
        ShopTriggerZone.SetActive(false);
        ShopUI.SetActive(false);
    }
    

}
