using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ItemPresenter : MonoBehaviour
{
    // Keeps track of item on this level
    
    private static GameObject ActiveItemInScene;
    // All Items in Game
    public List<GameObject> ItemList;

    private HashSet<GameObject> spawnedItems = new HashSet<GameObject>();

    System.Random rng;
    void Start()
    {
        rng = new System.Random();
        
    }

    // Called when the player enters the item room
    // Add a parameter for the current room
    public void PlaceItemInItemRoom(Room room)
    {
        // Don't spawn item if it's already been collected in this room
        if (room.itemCollected)
        {
            return;
        }

        // Assign item only once
        if (room.assignedItemPrefab == null)
        {
            room.assignedItemPrefab = PickRandomItem();
            if (room.assignedItemPrefab != null)
            {
                spawnedItems.Add(room.assignedItemPrefab);
            }
        }

        ActiveItemInScene = Instantiate(room.assignedItemPrefab, Vector3.zero, Quaternion.identity);

        spawnedItems.Add(ActiveItemInScene);

        // Set the room reference on the CollectionModel
        var collectionModel = ActiveItemInScene.GetComponent<CollectionModel>();
        if (collectionModel != null)
        {
            collectionModel.room = room;
        }
    }

    // If player chooses not to collect item then 
    // when they leave the room it shouldn't be there
    public void RemoveItemFromRoom()
    {
        if (ActiveItemInScene != null)
        {
            Destroy(ActiveItemInScene);
            spawnedItems.Remove(ActiveItemInScene);
            ActiveItemInScene = null;
        }
    }

    // Gets random item GameObject
    public GameObject PickRandomItem()
    {
        var availableItems = ItemList.Where(item => !spawnedItems.Contains(item)).ToList();
        if (availableItems.Count == 0)
        {
            // All items have been spawned, return null (at least for now)
            return null;
        }
        return availableItems[rng.Next(0, availableItems.Count)];
    }

    // Model notifys presenter
    // Add a parameter for the current room
    public void NotifyItemCollected(Room room)
    {
        room.itemCollected = true;
        
    }
}
