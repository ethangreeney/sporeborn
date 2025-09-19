using System;
using System.Collections.Generic;
using UnityEngine;

public class ItemPresenter : MonoBehaviour
{
    // Keeps track of item on this level
    private static GameObject CurrentItem;
    private static GameObject ActiveItemInScene;
    // All Items in Game
    public List<GameObject> ItemList;

    private static bool ItemHasBeenCollected;
    public static bool GetItemHasBeenCollected => ItemHasBeenCollected;

    System.Random rng;
    void Start()
    {
        rng = new System.Random();
        ItemHasBeenCollected = false;
    }

    // Called when the player enters the item room
    public void PlaceItemInItemRoom()
    {
        // Don't spawn item if its already been collected
        if (ItemHasBeenCollected)
        {
            return;
        }
        // Only pick a new item if it hasn't already been generated
        if (CurrentItem == null)
        {
            CurrentItem = PickRandomItem();
        }
        
        // Spawn Item in the Centre of the room
        ActiveItemInScene = Instantiate(CurrentItem, Vector3.zero, Quaternion.identity);
    }

    // If player chooses not to collection item then 
    // when they leave the room it shouldn't be there
    public void RemoveItemFromRoom()
    {
        if (ActiveItemInScene != null)
        {
            Destroy(ActiveItemInScene);
            ActiveItemInScene = null;
        }
    }

    // Gets random item GameObject
    public GameObject PickRandomItem()
    {
        return ItemList[rng.Next(0, ItemList.Count)];
    }

    // Model notifys presenter
    public void NotifyItemCollected()
    {
        ItemHasBeenCollected = true;
    }
}
