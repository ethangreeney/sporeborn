using System;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInventory : MonoBehaviour
{
    [SerializeField] private List<Item> items = new List<Item>();
    public IReadOnlyList<Item> Items => items;

    public event Action<IReadOnlyList<Item>> InventoryChanged;

    public void AddItem(Item item)
    {
        if (item == null) return;
        items.Add(item);
        InventoryChanged?.Invoke(Items);
    }

    public bool RemoveItem(Item item)
    {
        if (item == null) return false;
        var removed = items.Remove(item);
        if (removed) InventoryChanged?.Invoke(Items);
        return removed;
    }

    public void Clear()
    {
        items.Clear();
        InventoryChanged?.Invoke(Items);
    }
}