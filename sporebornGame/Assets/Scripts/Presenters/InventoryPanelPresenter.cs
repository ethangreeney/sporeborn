using System.Collections.Generic;

using TMPro;

using Unity.VisualScripting.Antlr3.Runtime.Misc;

using UnityEngine;
using UnityEngine.UI;

public class InventoryPanelPresenter : MonoBehaviour
{
    public PlayerInventory inventory;
    public StatsDisplayPresenter stats;
    public Transform content;
    public GameObject slotPrefab;
    public GameObject tooltip;
    public ItemPool itemPool;
    public TextMeshProUGUI itemName;
    public TextMeshProUGUI itemDesc;
    public TextMeshProUGUI inventoryProgess;

    void OnEnable() { inventory.OnInventoryChanged += Refresh; Refresh(); }
    void OnDisable() => inventory.OnInventoryChanged -= Refresh;
    void Refresh()
    {
        stats.UpdateDisplay();
        inventoryProgess.text = "Items found so far: " + inventory.FoundSoFar() + "/" + itemPool.itemPrefabs.Count;
        foreach (Transform child in content) Destroy(child.gameObject);

        var sortedItems = new List<Item>(inventory.itemPool.GetItems());
        sortedItems.Sort((a, b) =>
        {
            bool aColl = inventory.HasCollected(a.itemName);
            bool bColl = inventory.HasCollected(b.itemName);
            if (aColl != bColl) return bColl.CompareTo(aColl);
            return a.itemName.CompareTo(b.itemName);
        });


        foreach (var item in sortedItems)
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