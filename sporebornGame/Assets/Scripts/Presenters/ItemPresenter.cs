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
            return;
        }
        // Only pick a new item if it hasn't already been generated
        if (CurrentItem == null)
        {
            CurrentItem = PickRandomItem();
        }
        
        // Spawn Item in the Centre of the room
        Instantiate(CurrentItem, Vector3.zero, Quaternion.identity);
    }

    // Gets random item GameObject
    public GameObject PickRandomItem()
    {
        return ItemList[rng.Next(0, ItemList.Count)];
    }

    public void NotifyItemCollected()
    {
        itemHasBeenCollected = true;
    }
}
