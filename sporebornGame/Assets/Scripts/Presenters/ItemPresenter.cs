using System.Collections.Generic;
using UnityEngine;

public class ItemPresenter : MonoBehaviour
{
    // All Items in Game
    public List<GameObject> ItemList;

    // Track items per room
    private Dictionary<int, GameObject> roomItems = new Dictionary<int, GameObject>();
    private Dictionary<int, GameObject> activeRoomItems = new Dictionary<int, GameObject>();
    private Dictionary<int, bool> roomItemCollected = new Dictionary<int, bool>();

    // Track the current room type
    private RoomType lastRoomType;

    private MapPresenter mapPresenter;
    System.Random rng;

    void Start()
    {
        rng = new System.Random();
        mapPresenter = FindFirstObjectByType<MapPresenter>();
    }

    // Called when the player enters the item room
    public void PlaceItemInItemRoom()
    {
        if (mapPresenter == null || mapPresenter.CurrentPlayerRoom == null)
        {
            Debug.LogWarning("MapPresenter or CurrentPlayerRoom not found.");
            return;
        }

        int roomId = mapPresenter.CurrentPlayerRoom.OriginIndex;
        lastRoomType = mapPresenter.CurrentPlayerRoom.RoomType;

        // Only spawn items in Item rooms or Boss rooms
        if (lastRoomType != RoomType.Item && lastRoomType != RoomType.Boss)
        {
            return;
        }

        // Don't spawn item if it's already been collected in this room
        if (roomItemCollected.ContainsKey(roomId) && roomItemCollected[roomId])
        {
            return;
        }

        // Only pick a new item if it hasn't already been generated for this room
        if (!roomItems.ContainsKey(roomId))
        {
            roomItems[roomId] = PickRandomItem();
        }
        
        // Spawn Item in the Centre of the room
        GameObject item = Instantiate(roomItems[roomId], Vector3.zero, Quaternion.identity);
        activeRoomItems[roomId] = item;
    }

    // If player chooses not to collect item then 
    // when they leave the room it shouldn't be there
    public void RemoveItemFromRoom()
    {
        if (mapPresenter == null || mapPresenter.CurrentPlayerRoom == null)
        {
            return;
        }

        // Get the previous room ID from the active items
        foreach (var kvp in activeRoomItems)
        {
            if (kvp.Value != null)
            {
                Destroy(kvp.Value);
            }
        }
        
        // Clear the active items (but keep the generated items for persistence)
        activeRoomItems.Clear();
    }

    // Gets random item GameObject
    public GameObject PickRandomItem()
    {
        return ItemList[rng.Next(0, ItemList.Count)];
    }

    // Model notifies presenter
    public void NotifyItemCollected()
    {
        if (mapPresenter == null || mapPresenter.CurrentPlayerRoom == null)
        {
            Debug.LogWarning("MapPresenter or CurrentPlayerRoom not found.");
            return;
        }

        int roomId = mapPresenter.CurrentPlayerRoom.OriginIndex;
        roomItemCollected[roomId] = true;
        
        // Also remove the active item for this room
        if (activeRoomItems.ContainsKey(roomId))
        {
            // Item is already being destroyed by CollectionModel
            activeRoomItems.Remove(roomId);
        }
    }

    
}
