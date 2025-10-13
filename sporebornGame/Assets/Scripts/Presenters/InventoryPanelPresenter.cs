using TMPro;

using UnityEngine;
using UnityEngine.UI;

public class InventoryPanelPresenter : MonoBehaviour
{
    public PlayerInventory inventory;
    public Transform content;
    public GameObject slotPrefab;
    public GameObject tooltip;
    public TextMeshProUGUI itemName;
    public TextMeshProUGUI itemDesc;

    void OnEnable() => Refresh();

    void Refresh()
    {
        foreach (Transform child in content) Destroy(child.gameObject);

        foreach (var item in inventory.itemPool.GetItems())
        {
            bool collected = inventory.HasCollected(item.itemName);
            var slot = Instantiate(slotPrefab, content).GetComponent<InventoryItemSlot>();
            slot.Setup(item, collected, this);
        }

        HideTooltip();
    }

    public void ShowTooltip(Item item, bool collected)
    {
        tooltip.SetActive(true);
        itemName.text = item.itemName;
        itemDesc.text = collected ? item.itemDescription : "Not collected yet";
    }

    public void HideTooltip() => tooltip.SetActive(false);
}