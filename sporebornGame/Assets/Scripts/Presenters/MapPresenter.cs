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

    // Pixel scaling of scene
    private int PixelsPerUnit = 16;

    // Reference to players starting room on new level
    private Room StarterRoom;
    public Room GetStarterRoom => StarterRoom;

    // References to the Current Room
    public Room CurrentPlayerRoom;
    private GameObject ActiveRoomInstance;
    private GameObject CurrentRoomPrefab;
    private Door[] DoorsInRoom;

    // Reference to spawnEnemies after room is setup
    private EnemyPresenter enemyPresenter;

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

        // Generates the first room
        StarterRoom = FindRoom(model.GetStartingRoomIndex);

        // Location for centre of the OneByOne Room
        Player.transform.SetParent(null); // temp

        // Gets the current Enemy Presenter
        enemyPresenter = FindFirstObjectByType<EnemyPresenter>();

        // Build the starter room
        BuildRoom(StarterRoom, null);
    }

    void Update()
    {
        if (Input.GetKey(KeyCode.R))
        {
            Start();
        }
    }

    public int RelIndexToRoomIndex(Room CurrentRoom, int dx, int dy)
    {
        // Current Room to Coordinates 
        int index = CurrentRoom.OriginIndex;
        int x = index % GRID_SIDE;
        int y = index / GRID_SIDE;

        // Coordiantes plus the relative x and y
        int newX = x + dx;
        int newY = y - dy;

        // Index is out of bounds
        if (newX < 0 || newX >= GRID_SIDE) return -1;
        if (newY < 0 || newY >= GRID_SIDE) return -1;

        int newIndex = newY * GRID_SIDE + newX; // Coordinates to index

        // Not a valid room cell if outside the room
        if (!CurrentRoom.OccupiedIndexes.Contains(newIndex))
        {
            return -1;
        }
        // No room on floor plan
        if (model.GetFloorPlan[newIndex] == 0)
        {
            return -1;
        }

        // Returns the room index if meets all conditions 
        return newIndex;
    }

    // Creates a new room and correctly positions the player
    public void BuildRoom(Room RoomToSpawn, Door EnterDoor)
    {
        // Reset the room prefab
        CurrentRoomPrefab = null;
        // Destroy the previous Room
        if (ActiveRoomInstance != null)
        {
            Destroy(ActiveRoomInstance);
        }

        // Place room
        string RoomName = RoomToSpawn.RoomShape + "_" + RoomToSpawn.RoomType;

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
            Debug.Log($"BuildRoom(): Current Room Name {RoomName} Cannot be found");
            return;
        }

        // Update the current room variable
        CurrentPlayerRoom = RoomToSpawn;
        // Instantiates the room and Aligns the room to the bottom left
        ActiveRoomInstance = Instantiate(CurrentRoomPrefab, Vector3.zero, Quaternion.identity);

        // Gets all Doors in scene
        DoorsInRoom = ActiveRoomInstance.GetComponentsInChildren<Door>();
        // Assigns the MapPresenter to each door in the scene
        foreach (Door door in DoorsInRoom)
        {
            door.map = this;
        }

        Vector3 PlayerSpawnPosition = Vector3.zero;
        // If we are in starter room
        if (EnterDoor != null)
        {
            PlayerSpawnPosition = CalculateSpawnOffset(EnterDoor);
        }

        // Player location will be based on the door they enter from
        Player.transform.position = PlayerSpawnPosition;

        // Locks the doors of the room if its an enemy room
        ToggleLockDoors(CurrentPlayerRoom, true);

        // After Doors have been locked start spawning enemies
        if (ValidEnemyRoom(CurrentPlayerRoom))
        {
            if (CurrentPlayerRoom.RoomType == RoomType.Regular)
            {
                enemyPresenter.SpawnEnemies(ActiveRoomInstance, CurrentPlayerRoom);
            }
        }
    }

    public Vector3 CalculateSpawnOffset(Door EnterDoor)
    {
        // Get the relative doors position within the room and convert it to tile sizes
        int CellToEnter = EnterDoor.AdjacentCellIndex;
        Vector2 RelativePosition = IndexToRelativeCoordinate(CellToEnter, EnterDoor.ConnectingRoom.OriginIndex);

        // Convert relative room position into full cell lengths (i.e., 1 on the x is a transform of 16)
        RelativePosition.x *= PixelsPerUnit;
        RelativePosition.y *= PixelsPerUnit;

        // Find the position within that cell to be spawned
        int[] EnterDoorOffset = EnterDoor.RelPosFromOrigin[(int)EnterDoor.CurrentDoorType];

        RelativePosition.x += EnterDoorOffset[0];
        RelativePosition.y += EnterDoorOffset[1];

        return RelativePosition;

    }

    public Vector2 IndexToRelativeCoordinate(int CellIndex, int OriginIndex)
    {
        // 2D coords for each flat index
        int cellX = CellIndex % GRID_SIDE;
        int cellY = CellIndex / GRID_SIDE;
        int originX = OriginIndex % GRID_SIDE;
        int originY = OriginIndex / GRID_SIDE;

        // flip the X-subtraction so moving right is +X
        float x = originX - cellX;
        float y = cellY - originY;

        return new Vector2(-x, -y);
    }



    // Finds room based an index value that makes up that room
    public Room FindRoom(int index)
    {
        Room room = SpawnedRooms.FirstOrDefault(room => room.OccupiedIndexes.Contains(index));
        return room;
    }

    public void PrintRoomList()
    {
        foreach (Room room in SpawnedRooms)
        {
            Debug.Log(room.ToString());
        }
    }

    public bool ValidEnemyRoom(Room CurrentRoom)
    {
        // If room is regular/boss types and not start room then it can be enemy room
        return (CurrentRoom.RoomType == RoomType.Regular
        || CurrentRoom.RoomType == RoomType.Boss)
        && CurrentRoom.OriginIndex != StarterRoom.OriginIndex;
    }

    public void ToggleLockDoors(Room CurrentRoom, bool Locked)
    {
        // If its not a valid enemy room no need to lock/unlock doors
        if (!ValidEnemyRoom(CurrentRoom))
        {
            return;
        }

        // Locks each active door with boolean
        foreach (Door door in DoorsInRoom)
        {
            // if (door.ConnectingRoom != null)
            // {
            door.DoorIsLocked = Locked;
            // }
        }
       
    }
}