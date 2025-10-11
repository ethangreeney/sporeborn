using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class ShopPresenter : MonoBehaviour
{
    [SerializeField]
    private List<GameObject> ShopItemPool;

    private GameObject ShopTriggerZone;
    private GameObject ShopUI;

    private ShopModel ShopLogic;



    void Start()
    {

        Debug.LogWarning("Shop Presenter is Called");
        // Find Objects
        ShopTriggerZone = GameObject.Find("ShopTriggerZone");
        ShopUI = GameObject.Find("ShopUI");

        Debug.Log($"ShopTriggerZone: {ShopTriggerZone}");
        Debug.Log($"ShopUI: {ShopUI}");

        if(ShopTriggerZone == null || ShopUI == null)
        {
            Debug.LogWarning("Objects Not Found");
        }

        // Gets the Model from the ShopUI
        ShopLogic = ShopUI.GetComponentInChildren<ShopModel>();
        
        // Already instantiated in scene but set to inactive for faster load time
        ShopTriggerZone.SetActive(false);
        ShopUI.SetActive(false);

    }

    // Only Activates the Shop Trigger Zone when in Shop Room
    public void ActivateShopTrigger()
    {
        ShopTriggerZone.SetActive(true);
        ShopTriggerZone.transform.position = new Vector3(0,-2,0);
    }
    
    public void OpenShop()
    {
        // Need to Pause game while shop is open 
    }

    public void CloseShop()
    {

    }

    // Detects if the player is in range of the shop 
    private void OnTriggerEnter2D(Collider2D collision)
    {
        // If Player presses 'E' while in shop trigger zone open UI
        if (Input.GetKey(KeyCode.E))
        {
            OpenShop();
        }
    }

    public void PlayerClicksItem()
    {

    }

}
