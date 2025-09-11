using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MapPresenter : MonoBehaviour
{
    private MapModel model;

    private List<Room> SpawnedRooms;
    public List<Room> GetSpawnedRooms => SpawnedRooms;

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

    private Room StarterRoom;
    public Room GetStarterRoom => StarterRoom;

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
        
        //

        // Generates the first room
        StarterRoom = FindRoom(model.GetStartingRoomIndex);

        // Location for centre of the OneByOne Room
        Player.transform.SetParent(null); // temp

        // Build the starter room
        BuildRoom(StarterRoom, null);
    }

    void Update()
    {
        if (Input.GetKey(KeyCode.R)) {
            Start();
        }
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

        int newIndex = newY * GRID_SIDE + newX; // Coordinates to index

        // Can't be an adjacent neighbour if within the same room
        if (CurrentRoom.OccupiedIndexes.Contains(newIndex))
        {
            return false;
        }
        // Returns true if there is a room there
        return model.GetFloorPlan[newIndex] == 1;
    }

    // Creates a new room and correctly positions the player
    public void BuildRoom(Room RoomToSpawn, Door EnterDoor)
    {   
        // Destroy the previous Room
        if (ActiveRoomInstance != null)
        {
            Destroy(ActiveRoomInstance);
        }
        
        // Place room
        String RoomName = RoomToSpawn.RoomShape + "_" + RoomToSpawn.RoomType;

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

        // Update the current room variable
        CurrentPlayerRoom = RoomToSpawn;

        Debug.Log(CurrentRoomPrefab.name);
        // Instantiates the room and Aligns the room to the bottom left
        ActiveRoomInstance = Instantiate(CurrentRoomPrefab, Vector3.zero, Quaternion.identity);

        Door[] theDoorScript = ActiveRoomInstance.GetComponentsInChildren<Door>();
        foreach (Door door in theDoorScript)
        {
            door.map = this;
        }

        Vector3 PlayerSpawnPosition = Vector3.zero;
        // If we are in starter room
        if (EnterDoor != null) {
            PlayerSpawnPosition = CalculateSpawnOffset(EnterDoor);
        }

        // Player location will be based on the door they enter from
        Player.transform.position = PlayerSpawnPosition;

        
    }

    public Vector3 CalculateSpawnOffset(Door EnterDoor)
    {
        Vector3 NewPlayerPosition = Vector3.zero;

        // Get the relative doors position within the room and convert it to tile sizes
        int CellToEnter = EnterDoor.AdjacentCellIndex;
        int CellWeAreIn = EnterDoor.ConnectingRoom.Index;

        Vector3 RelativeCoords = IndexToRelativeCoordinate(CellToEnter - CellWeAreIn);

        // Gets the position within that tile
        int[] CellOffset = EnterDoor.RelPosFromOrigin[(int)EnterDoor.CurrentDoorType];

        NewPlayerPosition.x = (RelativeCoords.x * PixelsPerUnit);// + CellOffset[0];
        NewPlayerPosition.y = (RelativeCoords.y * PixelsPerUnit);// + CellOffset[1];

        return NewPlayerPosition;
    }

    public void SpawnEnemies()
    {

    }

    public void SpawnBoss()
    {

    }

    public Vector2 IndexToRelativeCoordinate(int index)
    {
        int x = index % GRID_SIDE;
        int y = index / GRID_SIDE;

        float relativeX = (float)x / (GRID_SIDE - 1);
        float relativeY = (float)y / (GRID_SIDE - 1);
        
        return new Vector2(relativeX, relativeY);
    }

    public Vector3 IndexToCoordinate(int index)
    {
        int x = index % GRID_SIDE;
        int y = index / GRID_SIDE;

        return new Vector3 (x, y, 0);
    }

    // Finds room based an index value that makes up that room
    public Room FindRoom(int index)
    {
        Room room = SpawnedRooms.FirstOrDefault(room => room.OccupiedIndexes.Contains(index));
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