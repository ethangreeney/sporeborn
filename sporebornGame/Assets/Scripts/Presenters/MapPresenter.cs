using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.Tilemaps;

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

    // Tilemap References
    private Tilemap FloorTilemap;
    private Grid RoomGrid;

    // Reference to spawn in GameObjects when entering a new room
    private EnemyPresenter enemyPresenter;
    private ItemPresenter itemPresenter;

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
        itemPresenter = FindFirstObjectByType<ItemPresenter>();

        // Build the starter room
        BuildRoom(StarterRoom, null);

        enemyPresenter.RemovePortal();
        enemyPresenter.ClearItems();
    }
    
    public void ResetMap()
    {
        Start();
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
        // Remove lingering item from previous room
        itemPresenter.RemoveItemFromRoom();

        enemyPresenter.RemovePortal();
        enemyPresenter.ClearItems();
        
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

        // Determinds what should spawn based on room type
        PlaceEntities(CurrentPlayerRoom);
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

    public void ToggleLockDoors(bool Locked)
    {
        // Locks each active door with boolean
        foreach (Door door in DoorsInRoom)
        {
            // if (door.ConnectingRoom != null)
            // {
            door.DoorIsLocked = Locked;
            // }
        }

    }

    public List<Vector3> GetSpawnLocations()
    {
        FloorTilemap = GetFloorTileLayer(ActiveRoomInstance);
        List<Vector3> SpawnableTiles = new List<Vector3>();


        if (FloorTilemap == null)
        {
            Debug.LogWarning("Can't find Floor Layer");
            return default;
        }

        BoundsInt roomBounds = FloorTilemap.cellBounds;

        foreach (Vector3Int pos in roomBounds.allPositionsWithin)
        {
            if (FloorTilemap.HasTile(pos))
            {
                // Converts from relative pos in grid to world position in scene
                Vector3 WorldlPos = FloorTilemap.CellToWorld(pos);

                // Spawns enemies in the centre of tiles
                WorldlPos += FloorTilemap.cellSize / 2f;

                // Adds to Vector3 list to tell enemies where they can spawn
                SpawnableTiles.Add(WorldlPos);
            }
        }

        return SpawnableTiles;
    }


    private Tilemap GetFloorTileLayer(GameObject CurrentRoomInstance)
    {
        if (CurrentRoomInstance == null)
        {
            Debug.Log("Current Room Instance is null");
        }

        // Find the Grid Object which stores all layers in RoomPrefabs
        RoomGrid = CurrentRoomInstance.GetComponentInChildren<Grid>();

        if (RoomGrid != null)
        {
            // Get transform of the FloorLayer within the Grid
            Transform FloorTileMapTransform = RoomGrid.transform.Find("FloorLayer");

            if (FloorTileMapTransform != null)
            {
                // Get the floor tilemap using its transform 
                Tilemap FloorTileMap = FloorTileMapTransform.GetComponent<Tilemap>();

                if (FloorTileMap == null)
                {
                    return default;
                }

                return FloorTileMap;

            }
        }
        // Can't find grid or floorplan transform
        return default;
    }

    public void RoomCompleted()
    {
        CurrentPlayerRoom.RoomCompleted = true;
        
        // if the room is a boss room, play normal music
        if (CurrentPlayerRoom.RoomType == RoomType.Boss && SoundManager.instance != null)
        {
            SoundManager.instance.BossDefeated();
        }
    }


    public void PlaceEntities(Room CurrentRoom)
    {
        
        // If Room is a valid enemy room and hasn't been completed
        if (ValidEnemyRoom(CurrentRoom) && CurrentRoom.RoomCompleted == false)
        {
            // Locks the doors of the room if room is a valid enemy room
            ToggleLockDoors(true);
            if (CurrentRoom.RoomType == RoomType.Regular)
            {
                // Spawns Enemies
                enemyPresenter.SpawnEnemies(ActiveRoomInstance, CurrentRoom);
            }
            else if (CurrentRoom.RoomType == RoomType.Boss)
            {
                // Spawn Boss
                enemyPresenter.SpawnBoss(ActiveRoomInstance, CurrentRoom);
                // play boss music
                SoundManager.instance.EnterBossRoom();             
            }
        }

        // Item room: spawn item if not collected
        if (CurrentRoom.RoomType == RoomType.Item && !CurrentRoom.itemCollected)
        {
            itemPresenter.PlaceItemInItemRoom(CurrentRoom);
        }

        // Boss room: spawn item only if boss defeated and not collected
        if (CurrentRoom.RoomType == RoomType.Boss && CurrentRoom.RoomCompleted && !CurrentRoom.itemCollected)
        {
            itemPresenter.PlaceItemInItemRoom(CurrentRoom);

        }

        // Spawn portal if boss defeated
        if (CurrentRoom.RoomType == RoomType.Boss && CurrentRoom.RoomCompleted)
        {
            enemyPresenter.SpawnPortal();
        }

    }
    


}