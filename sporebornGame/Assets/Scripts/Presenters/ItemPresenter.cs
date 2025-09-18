using System;
using System.Collections.Generic;
using UnityEngine;

public class ItemPresenter : MonoBehaviour
{
    // Keeps track of item on this level
    private static GameObject CurrentItem;
    // All Items in Game
    public List<GameObject> ItemList;

    private static bool itemHasBeenCollected;

    System.Random rng;
    void Start()
    {
        rng = new System.Random();
        itemHasBeenCollected = false;
    }

    // Called when the player enters the item room
    public void PlaceItemInItemRoom()
    {
        // Don't spawn item if its already been collected
        if (itemHasBeenCollected)
        {
            Debug.Log("Item has been collected already");
            return;
        }
        // Only pick a new item if it hasn't already been generated
        if (CurrentItem == null)
        {
            Debug.Log("Item is null");
            CurrentItem = PickRandomItem();
        }
        // Spawn Item in the Centre of the room
        Vector2 RoomCentre = new Vector2(16, 16);
        Instantiate(CurrentItem, RoomCentre, Quaternion.identity);
    }

    // Gets random item GameObject
    public GameObject PickRandomItem()
    {
        Debug.Log("Random item is chosen");

        if (ItemList == null)
        {
            Debug.Log("Item List is empty");
        }
        return ItemList[rng.Next(0, ItemList.Count)];
    }

    public void NotifyItemCollected()
    {
        itemHasBeenCollected = true;
    }
}
