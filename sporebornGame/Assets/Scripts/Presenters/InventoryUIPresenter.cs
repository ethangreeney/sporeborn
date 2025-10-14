using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InventoryUIPresenter : MonoBehaviour
{
    [Header("References")]
    public PlayerInventory inventory; // Assign in Inspector (Player)
    public Transform contentRoot; // The Content object under InventoryPanel
    public GameObject slotPrefab; // InventoryItemSlot prefab

    [Header("Options")]
    public bool groupDuplicates = true;

    private void OnEnable()
    {
        TryWireInventory();
        Refresh();
    }

    private void OnDisable()
    {
        if (inventory != null)
        {
            inventory.InventoryChanged -= OnInventoryChanged;
        }
    }

    private void TryWireInventory()
    {
        if (inventory == null)
        {
            inventory = FindFirstObjectByType<PlayerInventory>();
        }

        if (inventory != null)
        {
            inventory.InventoryChanged -= OnInventoryChanged;
            inventory.InventoryChanged += OnInventoryChanged;
        }
    }

    private void OnInventoryChanged(IReadOnlyList<Item> _)
    {
        Refresh();
    }

    public void Refresh()
    {
        if (contentRoot == null || slotPrefab == null) return;

        for (int i = contentRoot.childCount - 1; i >= 0; i--)
        {
            Destroy(contentRoot.GetChild(i).gameObject);
        }

        if (inventory == null) return;

        if (!groupDuplicates)
        {
            foreach (var it in inventory.Items)
            {
                CreateSlot(it, 1);
            }
            return;
        }

        // Group duplicates by name (alternatively by icon)
        var counts = new Dictionary<string, (Item item, int count)>();
        foreach (var it in inventory.Items)
        {
            var key = string.IsNullOrEmpty(it.itemName) ? it.GetHashCode().ToString() : it.itemName;
            if (!counts.TryGetValue(key, out var tuple))
            {
                counts[key] = (it, 1);
            }
            else
            {
                counts[key] = (tuple.item, tuple.count + 1);
            }
        }

        foreach (var kvp in counts.Values)
        {
            CreateSlot(kvp.item, kvp.count);
        }
    }

    private void CreateSlot(Item item, int count)
    {
        var slot = Instantiate(slotPrefab, contentRoot);
        var img = slot.GetComponentInChildren<Image>();
        if (img != null && item != null)
        {
            img.sprite = item.itemIcon;
        }
    }
}