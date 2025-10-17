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

    private int CurrentCost;

    public void SetupItem(ShopItem data, ShopModel model)
    {
        // Initalise instance vairables
        shopModel = model;
        CurrentItemData = data;

        // Set the Shop Item visuals    
        ImageIcon.sprite = data.Icon;
        ItemName.text = data.Name;
        ItemCost.text = "Cost: $" + data.Cost;

        this.CurrentCost = data.Cost;
    }

    // Called when player clicks on a ShopItem button
    public void OnPurchaseClick()
    {
        Debug.LogWarning("Purchase Clicked");

        // Find the Player Presenter
        PlayerPresenter p = FindAnyObjectByType<PlayerPresenter>();

        if (p == null)
        {
            Debug.LogWarning("Can't find player presenter");
        }

        shopModel.TryPurchaseItem(CurrentItemData, p, this);
    }

    public void OnHover()
    {
        PlayerPresenter p = FindAnyObjectByType<PlayerPresenter>();
        CurrencyModel PlayerWallet = p.GetComponent<CurrencyModel>();

        // On hover if player doesn't have enough funds change visuals
        if (PlayerWallet.GetCurrentNectar() < this.CurrentCost)
        {
            ImageIcon.color = new Color(1f, 0.3f, 0.3f);
            ItemCost.text = "Insuffcient Nectar";
        }

    }
    
    public void ItemHasBeenPurchased(GameObject PurchasedItem)
    {
        ImageIcon.color = Color.gray;
        ItemName.color = Color.gray;

        ItemCost.text = "Purchased";

        // Makes button not interactable after purchase
        PurchasedItem.GetComponent<Button>().interactable = false;
    }
}