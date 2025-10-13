using System;
using System.Collections.Generic;

using UnityEngine;

public class PlayerInventory : MonoBehaviour
{
    public ItemPool itemPool;
    private HashSet<string> collected = new HashSet<string>();
    public event Action OnInventoryChanged;

    public void AddItem(Item item)
    {
        collected.Add(item.itemName);
        OnInventoryChanged?.Invoke();
    }

    public bool HasCollected(string itemName) => collected.Contains(itemName);
}