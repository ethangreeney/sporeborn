using UnityEngine;

[System.Serializable]
public class ShopItem
{
    public DropModel ItemType;
    public string Name;
    public int Cost;
    public Sprite Icon;

    public ShopItem(DropModel ItemType, string Name, int Cost, Sprite Icon)
    {
        this.ItemType = ItemType;
        this.Name = Name;
        this.Cost = Cost;
        this.Icon = Icon;
    }
}