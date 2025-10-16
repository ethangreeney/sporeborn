using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ShopItemUI : MonoBehaviour
{
    [SerializeField] private Image ImageIcon;
    [SerializeField] private TextMeshProUGUI ItemName;
    [SerializeField] private TextMeshProUGUI ItemCost;

    private ShopModel shopModel;
    private ShopItem CurrentItemData;

    public void SetupItem(ShopItem data, ShopModel model)
    {
        // Initalise instance vairables
        shopModel = model;
        CurrentItemData = data;

        // Set the Shop Item visuals    
        ImageIcon.sprite = data.Icon;
        ItemName.text = data.Name;
        ItemCost.text = "Cost: $" + data.Cost;
    }

    // Called when player clicks on a ShopItem button
    public void OnPurchaseClick()
    {
        Debug.LogWarning("Purchase Clicked");

        // Find the Player Presenter
        PlayerPresenter p = FindAnyObjectByType<PlayerPresenter>();
        
        if(p == null)
        {
            Debug.LogWarning("Can't find player presenter");
        }
        
        shopModel.TryPurchaseItem(CurrentItemData, p, this);
    }
}