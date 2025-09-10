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

    [Header("GridSize")]
    public int GRID_SIDE = 10;

    [Header("Number of Rooms")]
    public int MINROOMS = 10;
    public int MAXROOMS = 20;

    private int PixelsPerUnit = 16;

    private Vector3 SpawnLocation;

    public Room CurrentPlayerRoom;
    private GameObject ActiveRoomInstance;
    private GameObject CurrentRoomPrefab;


    void Start()
    {
        // Generates first level map
        model = new MapModel(MINROOMS, MAXROOMS);

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

        int x = index % GRID_SIDE;
        int y = index / GRID_SIDE;

        int newX = x + dx;
        int newY = y + dy;
        
        // Index is out of bounds
        if (newX < 0 || newX >= GRID_SIDE) return false;
        if (newY < 0 || newY >= GRID_SIDE) return false;

        int newIndex = newY * GRID_SIDE + newX;

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
        if (PlayerSpawnPosition != SpawnLocation)
        {
            // TODO: Calculate offset baseed on EnterDoor
            PlayerSpawnPosition = CalculateSpawnOffset(EnterDoor);
        }

        // Player location will be based on the door they enter from
        Player.transform.position = PlayerSpawnPosition;

        // Update the current room 
        CurrentPlayerRoom = CurrentRoom;
    }

    public Vector3 CalculateSpawnOffset(Door EnterDoor)
    {
        Vector3 PlayerOffsetFromDoor = Player.transform.position - EnterDoor.GetPositionDoor();

        int CurrentRelativeDoorX = EnterDoor.RelativeDoorPosition[0];
        int CurrentRelativeDoorY = EnterDoor.RelativeDoorPosition[1];

        // Relative position of the player
        Vector2 DoorCellPosition = new(CurrentRelativeDoorX, CurrentRelativeDoorY);

        // Flips the relative position
        switch (EnterDoor.CurrentDoorType)
        {
            case Door.DoorType.North:
                // Flip to South relative cell
                DoorCellPosition.y -= 1;
                break;
            case Door.DoorType.South:
                // Flip to North relative cell
                DoorCellPosition.y += 1;
                break;
            case Door.DoorType.East:
                // Flip to West relative cell
                DoorCellPosition.x += 1;
                break;
            case Door.DoorType.West:
                // Flip to East relative cell
                DoorCellPosition.x -= 1;
                break;
        }

        Vector3 NewPlayerPosition = new Vector3
        (
            DoorCellPosition.x * PixelsPerUnit,
            DoorCellPosition.y * PixelsPerUnit,
            0
        );

        NewPlayerPosition += PlayerOffsetFromDoor;

        return NewPlayerPosition;
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
        Room room = SpawnedRooms.First(room => room.Index == index);
        if (room == null)
        {
            Debug.LogWarning("Can't find room in MapModel");
        }
        return room; 
    }

    public void PrintRoomList()
    {
        foreach (Room room in SpawnedRooms)
        {
            Debug.Log(room.ToString());

        }
    }
}