using System;
using UnityEngine;

[Serializable]
public class ShopItem
{
    public ActivatableItem ItemType;
    public string Name;
    public int Cost;
    public Sprite Icon;
    public bool Purchased;
    public ShopItemUI UIReference;
    
    public ShopItem(ActivatableItem ItemType, string Name, int Cost, Sprite Icon)
    {
        this.ItemType = ItemType;
        this.Name = Name;
        this.Cost = Cost;
        this.Icon = Icon;

        Purchased = false;
    }
}