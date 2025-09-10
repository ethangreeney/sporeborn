using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MapPresenter : MonoBehaviour
{
    private MapModel model;

    private List<Room> SpawnedRooms;

    [Header("Room Prefabs")]
    public List<GameObject> RoomPrefabs;

    [Header("Player")]
    public GameObject Player;

    private Vector3 SpawnLocation;

    public Room CurrentPlayerRoom;
    private GameObject ActiveRoomInstance;
    private GameObject CurrentRoomPrefab;


    void Start()
    {

        // Generates first level map
        model = new MapModel(10, 20);

        // Gets the list of rooms
        SpawnedRooms = model.GetRoomList;
        if (SpawnedRooms.Count == 0)
        {
            Debug.LogWarning("Room List is empty");
        }
        // Debug Check of Rooms
        // PrintRoomList();

        // Generates the first room
        Room StarterRoom = FindRoom(model.GetStartingRoomIndex);

        // Location for centre of the OneByOne Room
        Player.transform.SetParent(null); // temp
        SpawnLocation = new Vector3(8, 8, 0);

        // Build the starter room
        BuildRoom(StarterRoom, SpawnLocation, null);
    }

    // A cell is a like several 1x1 areas that make up large rooms
    // E.g. OneByOne has 1 cell but L shapes have 3
    public bool ValidCellNeighbour(Room CurrentRoom, int dx, int dy)
    {
        int index = CurrentRoom.Index;

        int x = index % 10;
        int y = index / 10;

        int newX = x + dx;
        int newY = y + dy;
        
        // Index is out of bounds
        if (newX < 0 || newX >= 10) return false;
        if (newY < 0 || newY >= 10) return false;

        int newIndex = newY * 10 + newX;

        // Can't be an adjacent neighbour if within the same room
        if (CurrentRoom.OccupiedIndexes.Contains(newIndex))
        {
            return false;
        }
        // Returns true if there is a room there
        return model.GetFloorPlan[newIndex] == 1;
    }

    // Creates a new room and correctly positions the player
    public void BuildRoom(Room CurrentRoom, Vector3 PlayerSpawnPosition, Door EnterDoor)
    {
        // Player location will be based on the door they enter from
        Player.transform.position = PlayerSpawnPosition;

        // Destroy the previous Room
        if (ActiveRoomInstance != null)
        {
            Destroy(ActiveRoomInstance);
        }
        
        // Place room
        String RoomName = CurrentRoom.RoomShape + "_" + CurrentRoom.RoomType;
        // Find the correct room Prefab
        foreach (GameObject prefab in RoomPrefabs)
        {
            if (prefab.name == RoomName)
            {
                CurrentRoomPrefab = prefab;
                break;
            }
        }
        if (CurrentRoomPrefab == null)
        {
            Debug.Log("BuildRoom(): Current Room Name Cannot be found");
            return;
        }

        // Instantiates the room and Aligns the room to the bottom left
        ActiveRoomInstance = SpawnRoomBottomLeft(CurrentRoomPrefab, Vector3.zero);

        Door[] theDoorScript = ActiveRoomInstance.GetComponentsInChildren<Door>();
        foreach (Door door in theDoorScript)
        {
            door.map = this;
        }

        // Doesn't need offset as if it is the first room
        // if (PlayerSpawnPosition != Vector3.zero)
        // {
        //     // TODO: Calculate offset baseed on EnterDoor
        // }

        
        

        // Update the current room 
        CurrentPlayerRoom = CurrentRoom;
    }

    public GameObject SpawnRoomBottomLeft(GameObject RoomPrefab, Vector3 RoomPosition)
    {
        // Instantiate the room first
        GameObject instance = Instantiate(RoomPrefab, RoomPosition, Quaternion.identity);

        // Get the bounds from the Renderers
        Renderer[] RoomRenderers = instance.GetComponentsInChildren<Renderer>();

        if (RoomRenderers.Length == 0)
        {
            Debug.LogWarning("No renderers found on prefab: " + RoomPrefab.name);
        }

        // Encapsulate the room into one bounding box
        Bounds bounds = RoomRenderers[0].bounds;
        foreach (Renderer r in RoomRenderers)
        {
            bounds.Encapsulate(r.bounds);
        }

        // Work out the bottom-left corner from rooms pivot
        Vector3 RoomOffset = bounds.min - instance.transform.position;

        // Move the room so that bottom-left is at 0,0,0
        instance.transform.position -= RoomOffset;

        return instance;
    }

    public void SpawnEnemies()
    {

    }

    public void SpawnBoss()
    {

    }

    public Vector3 IndexToCoordinate(int index)
    {
        int x = index % 10;
        int y = index / 10;
        Vector3 cooordinate = new(x, y, 0);

        return cooordinate;
    }

    public Room FindRoom(int index)
    {        
        return SpawnedRooms.First(room => room.Index == index);
    }

    public void PrintRoomList()
    {
        foreach (Room room in SpawnedRooms)
        {
            Debug.Log(room.ToString());

        }
    }
}